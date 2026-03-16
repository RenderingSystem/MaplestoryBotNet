#include "pch.h"
#include "TemplateMatcher.h"
#include <algorithm>


namespace ArrayFireNCC {

    void ArrayFireSaver::save(const af::array& arr, const std::string& filename, bool normalize)
    {
        auto width = (int)arr.dims(0);
        auto height = (int)arr.dims(1);
        auto bitmap = gcnew Drawing::Bitmap(width, height, PixelFormat::Format8bppIndexed);
        Drawing::Imaging::BitmapData^ bitmap_data = bitmap->LockBits(
            Drawing::Rectangle(0, 0, width, height),
            Imaging::ImageLockMode::WriteOnly,
            PixelFormat::Format8bppIndexed
        );
        auto* af_data = arr.host<float>();
        uint8_t* bitmap_ptr = static_cast<uint8_t*>(bitmap_data->Scan0.ToPointer());
        const int stride = bitmap_data->Stride / sizeof(uint8_t);
        float min_val = 0.0f, max_val = 1.0f;
        if (normalize)
        {
            min_val = af::min<float>(arr);
            max_val = af::max<float>(arr);
            float range = max_val - min_val;
            if (range < 1e-12f) range = 1.0f;
        }
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float val = af_data[y * width + x];
                if (normalize)
                {
                    val = (val - min_val) / (max_val - min_val);
                }
                else
                {
                    val = std::max(0.0f, std::min(1.0f, val));
                }
                bitmap_ptr[y * stride + x] = static_cast<uint8_t>(val * 255.0f + 0.5f);
            }
        }
        bitmap->UnlockBits(bitmap_data);
        Imaging::ColorPalette^ palette = bitmap->Palette;
        for (int i = 0; i < 256; i++)
        {
            palette->Entries[i] = Color::FromArgb(i, i, i);
        }
        bitmap->Palette = palette;
        bitmap->Save(gcnew String(filename.c_str()), Imaging::ImageFormat::Png);
    }

}


namespace ArrayFireNCC {

    af::array BitmapToZeroMeanGrayscaleConverter::convert(
        UINT32* image,
        int image_width,
        int image_height,
        int image_stride
    )
    {
        // Create buffer for original image data
        std::vector<float> host_buffer(image_width * image_height);
        // Convert to grayscale
        for (auto y = 0; y < image_height; ++y)
        {
            for (auto x = 0; x < image_width; ++x)
            {
                auto argb = image[y * image_stride + x];
                auto r = float((argb & 0x00FF0000) >> 0x10);
                auto g = float((argb & 0x0000FF00) >> 0x08);
                auto b = float((argb & 0x000000FF) >> 0x00);
                auto grayscale = (0.299f * r + 0.587f * g + 0.114f * b) / 255.0f;
                host_buffer[y * image_width + x] = grayscale;
            }
        }
        // Create ArrayFire array
        af::array grayscale_image(image_width, image_height, host_buffer.data());
        return grayscale_image.as(f32);
    }

    af::array BitmapToZeroMeanGrayscaleConverter::convert(Bitmap^ bitmap)
    {
        auto rect = System::Drawing::Rectangle(
            0,
            0,
            bitmap->Width,
            bitmap->Height
        );
        auto bitmap_data = bitmap->LockBits(
            rect,
            ImageLockMode::ReadOnly,
            PixelFormat::Format32bppArgb
        );
        try
        {
            return convert(
                static_cast<UINT32*>(bitmap_data->Scan0.ToPointer()),
                static_cast<int>(bitmap_data->Width),
                static_cast<int>(bitmap_data->Height),
                static_cast<int>(bitmap_data->Stride / sizeof(UINT32))
            );
        }
        finally
        {
            bitmap->UnlockBits(bitmap_data);
        }
    }

}


namespace ArrayFireNCC {

    af::array BitmapToMaskConverter::convert(
        UINT32* image,
        int image_width,
        int image_height,
        int image_stride
    )
    {
        // Create buffer for original image data
        std::vector<float> host_buffer(image_width * image_height);
        // Convert to masked data
        for (auto y = 0; y < image_height; ++y)
        {
            for (auto x = 0; x < image_width; ++x)
            {
                auto argb = image[y * image_stride + x];
                auto alpha = float((argb & 0xFF000000) >> 0x18);
                host_buffer[y * image_width + x] = alpha;
            }
        }
        // Create ArrayFire array
        af::array mask_array(image_width, image_height, host_buffer.data());
        auto mask = (mask_array == 255.0f).as(af::dtype::f32);
        return mask;
    }

    af::array BitmapToMaskConverter::convert(Bitmap^ bitmap)
    {
        auto rect = System::Drawing::Rectangle(0, 0, bitmap->Width, bitmap->Height);
        auto bitmap_data = bitmap->LockBits(
            rect,
            ImageLockMode::ReadOnly,
            PixelFormat::Format32bppArgb
        );
        try
        {
            return convert(
                static_cast<UINT32*>(bitmap_data->Scan0.ToPointer()),
                static_cast<int>(bitmap_data->Width),
                static_cast<int>(bitmap_data->Height),
                static_cast<int>(bitmap_data->Stride / sizeof(UINT32))
            );
        }
        finally {
            bitmap->UnlockBits(bitmap_data);
        }
    }

}


namespace ArrayFireNCC {

    af::dim4 ArrayFireCenterShifter::target_size(
        const af::array& image, const af::array& templ
    )
    {
        af::dim4 target_dims = image.dims();
        for (int i = 0; i < 2; ++i)
        {
            target_dims[i] = image.dims(i) + templ.dims(i) - 1;
        }
        return target_dims;
    }

    af::array ArrayFireCenterShifter::pad(af::array image, af::dim4 target_dims)
    {
        auto before_pad = af::dim4(0, 0, 0, 0);
        auto after_pad = af::dim4(0, 0, 0, 0);
        for (int i = 0; i < 2; i++)
        {
            auto total_pad = target_dims[i] - image.dims(i);
            if (total_pad < 0) total_pad = 0;
            auto before = total_pad / 2;
            auto after = total_pad - before;
            before_pad[i] = before;
            after_pad[i] = after;
        }
        return af::pad(image, before_pad, after_pad, AF_PAD_ZERO).as(f32);
    }

    af::array ArrayFireCenterShifter::shift(
        const af::array& product,
        af::dim4 target_dims,
        af::dim4 image_dims,
        af::dim4 templ_dims
    )
    {
        const int x_shift = static_cast<int>(Math::Ceiling((target_dims[0] / 2.0f) + 0.5f));
        const int y_shift = static_cast<int>(Math::Ceiling((target_dims[1] / 2.0f) + 0.5f));
        const int start_x = static_cast<int>(Math::Floor((templ_dims[0] / 2.0f) - 0.5f));
        const int start_y = static_cast<int>(Math::Floor((templ_dims[1] / 2.0f) - 0.5f));
        af::array spatial_map = af::shift(product, -x_shift, -y_shift);
        return spatial_map(
            af::seq(start_x, static_cast<double>(start_x + image_dims[0] - 1)),
            af::seq(start_y, static_cast<double>(start_y + image_dims[1] - 1)),
            af::span,
            af::span
        );
    }

}


namespace ArrayFireNCC {
    List<Tuple<int, int, float>^>^ LocationDetector::detect(af::array& ncc_map, float threshold)
    {
        // Find peaks on GPU
        af::array locations = af::where(ncc_map >= threshold);
        int count = static_cast<int>(locations.elements());
        if (count == 0) return gcnew List<Tuple<int, int, float>^>();
        // Get the NCC values at those locations (unsorted)
        af::array values = af::lookup(af::flat(ncc_map), locations);
        // Convert to coordinates on GPU
        af::array coords = af::moddims(locations, count);
        af::array x = coords % ncc_map.dims(0);
        af::array y = coords / ncc_map.dims(0);
        // Transfer to host - include values array
        array<long long>^ h_x = gcnew array<long long>(count);
        array<long long>^ h_y = gcnew array<long long>(count);
        array<float>^ h_values = gcnew array<float>(count);
        // Pin the arrays to get native pointers
        pin_ptr<long long> pin_x = &h_x[0];
        pin_ptr<long long> pin_y = &h_y[0];
        pin_ptr<float> pin_values = &h_values[0];
        // Copy data from GPU to pinned arrays
        x.host(pin_x);
        y.host(pin_y);
        values.host(pin_values);
        // Create managed list with values
        auto matches = gcnew List<Tuple<int, int, float>^>();
        for (int i = 0; i < count; ++i)
        {
            auto h_x_int = static_cast<int>(h_x[i]);
            auto h_y_int = static_cast<int>(h_y[i]);
            auto h_value = h_values[i];
            auto new_match = gcnew Tuple<int, int, float>(h_x_int, h_y_int, h_value);
            matches->Add(new_match);
        }
        return matches;
    }
}


namespace ArrayFireNCC {

    NormalizedCrossCorrelation::NormalizedCrossCorrelation(
        AbstractArrayFireShifter^ shifter
    )
    {
        _shifter = shifter;
    }

    af::array NormalizedCrossCorrelation::_compute_cross_correlation(
        const af::array& image, const af::array& templ
    )
    {
        // Read in the image and template dimensions
        auto image_dims = image.dims();
        auto templ_dims = templ.dims();
        // Pad to target size
        auto target_dims = _shifter->target_size(image, templ);
        auto padded_image = _shifter->pad(image, target_dims);
        auto padded_templ = _shifter->pad(templ, target_dims);
        auto image_ft = af::fft2(padded_image);
        auto templ_ft = af::fft2(padded_templ);
        // Multiply image_ft by conjugate of templ_ft
        auto product_ft = image_ft * af::conjg(templ_ft);
        // Inverse Fourier transform
        auto out = af::real(af::ifft2(product_ft));
        // Roll and cut to the spatial domain
        return _shifter->shift(out, target_dims, image_dims, templ_dims);
    }

    af::array NormalizedCrossCorrelation::_compute_image_sq_dev(
        const af::array& masked_image,
        const af::array& templ_mask,
        const af::array& valid_count
    )
    {
        auto local_sq_image = masked_image * masked_image;
        auto local_sq_sum = _compute_cross_correlation(local_sq_image, templ_mask);
        auto local_sum = _compute_cross_correlation(masked_image, templ_mask);
        auto sum_sq_dev = local_sq_sum - af::pow(local_sum, 2) / valid_count;
        return af::max(sum_sq_dev, 0.0f);
    }

    af::array NormalizedCrossCorrelation::calculate(
        const af::array& image,
        const af::array& image_mask,
        const af::array& templ,
        const af::array& templ_mask
    )
    {
        // Compute normalization
        auto masked_image = image * image_mask;
        auto masked_templ = templ * templ_mask;
        // Compute zero-mean to image and template
        auto mean_image = af::sum<float>(masked_image) / af::sum<float>(image_mask);
        auto mean_templ = af::sum<float>(masked_templ) / af::sum<float>(templ_mask);
        auto zero_image = (masked_image - mean_image) * image_mask;
        auto zero_templ = (masked_templ - mean_templ) * templ_mask;
        // Compute valid regions
        auto valid_count = _compute_cross_correlation(image_mask, templ_mask);
        auto valid_confidence = (valid_count >= (af::sum<float>(templ_mask) - 0.1f)).as(f32);
        // Compute components
        auto cross_correlation = _compute_cross_correlation(zero_image, zero_templ);
        auto image_sq_dev = _compute_image_sq_dev(zero_image, templ_mask, valid_count);
        auto templ_sq_dev = af::sum<float>(af::pow(zero_templ, 2));
        // Compute normalization term
        auto normalization = af::sqrt(templ_sq_dev * image_sq_dev);
        // Compute zero-mean normalized cross correlation
        auto ncc_map = cross_correlation / normalization;
        ncc_map(ncc_map == af::NaN) = 0.0f;
        ncc_map(ncc_map >= +1.0f + 1e-6) = 0.0f;
        ncc_map(ncc_map <= -1.0f - 1e-6) = 0.0f;
        return ncc_map * valid_confidence;
    }   

}


namespace ArrayFireNCC {

    NormalizedCrossCorrelationFacade::NormalizedCrossCorrelationFacade(void)
    {
        _ncc = gcnew NormalizedCrossCorrelation(
            gcnew ArrayFireCenterShifter()
        );
    }

    af::array NormalizedCrossCorrelationFacade::calculate(
        const af::array& image,
        const af::array& image_mask,
        const af::array& templ,
        const af::array& templ_mask
    )
    {
        return _ncc->calculate(image, image_mask, templ, templ_mask);
    }

}


namespace ArrayFireNCC {

    Tuple<int, int, int, int, float>^ RectangleMerger::_compute_merge_area(
        Tuple<int, int, int, int, float>^ rect_1,
        Tuple<int, int, int, int, float>^ rect_2,
        float merge_threshold
    )
    {
        // Extract rectangle coordinates
        int r1_x1 = rect_1->Item1;
        int r1_y1 = rect_1->Item2;
        int r1_x2 = r1_x1 + rect_1->Item3;
        int r1_y2 = r1_y1 + rect_1->Item4;
        int r2_x1 = rect_2->Item1;
        int r2_y1 = rect_2->Item2;
        int r2_x2 = r2_x1 + rect_2->Item3;
        int r2_y2 = r2_y1 + rect_2->Item4;
        // Calculate intersection area (overlap)
        int intersect_x1 = Math::Max(r1_x1, r2_x1);
        int intersect_y1 = Math::Max(r1_y1, r2_y1);
        int intersect_x2 = Math::Min(r1_x2, r2_x2);
        int intersect_y2 = Math::Min(r1_y2, r2_y2);
        float overlap_width = static_cast<float>(Math::Max(0, intersect_x2 - intersect_x1));
        float overlap_height = static_cast<float>(Math::Max(0, intersect_y2 - intersect_y1));
        float overlap_area = overlap_width * overlap_height;
        // Early exit if no overlap
        if (overlap_area <= 1e-6f)
        {
            return nullptr;
        }
        // Compute areas
        float r1_area = static_cast<float>(rect_1->Item3) * static_cast<float>(rect_1->Item4);
        float r2_area = static_cast<float>(rect_2->Item3) * static_cast<float>(rect_2->Item4);
        // Avoid division by zero (unlikely but possible)
        float total_area = r1_area + r2_area;
        if (total_area <= 1e-6f)
        {
            return nullptr;
        }
        // Check if overlap ratio meets threshold
        float overlap_ratio = (2 * overlap_area) / total_area;
        if (overlap_ratio < merge_threshold)
        {
            return nullptr;
        }
        // Compute merged bounding box
        int merged_x1 = Math::Min(r1_x1, r2_x1);
        int merged_y1 = Math::Min(r1_y1, r2_y1);
        int merged_x2 = Math::Max(r1_x2, r2_x2);
        int merged_y2 = Math::Max(r1_y2, r2_y2);
        // Take the maximum confidence of the two merged rectangles
        float max_confidence = Math::Max(rect_1->Item5, rect_2->Item5);
        // Return as (x, y, width, height, confidence)
        return gcnew Tuple<int, int, int, int, float>(
            merged_x1,
            merged_y1,
            merged_x2 - merged_x1,
            merged_y2 - merged_y1,
            max_confidence
        );
    }

    int RectangleMerger::_compareByConfidenceDescending(
        Tuple<int, int, int, int, float>^ a,
        Tuple<int, int, int, int, float>^ b)
    {
        return b->Item5.CompareTo(a->Item5);
    }

    List<Tuple<int, int, int, int, float>^>^ RectangleMerger::merge(
        List<Tuple<int, int, int, int, float>^>^ rectangles, float merge_threshold
    )
    {
        if (rectangles == nullptr || rectangles->Count == 0)
        {
            return gcnew List<Tuple<int, int, int, int, float>^>();
        }
        auto working_list = gcnew List<Tuple<int, int, int, int, float>^>(rectangles);
        bool changes_made;
        do
        {
            changes_made = false;
            auto next_pass_list = gcnew List<Tuple<int, int, int, int, float>^>();
            auto merged_flags = gcnew array<bool>(working_list->Count);
            for (int i = 0; i < working_list->Count; i++)
            {
                if (merged_flags[i])
                {
                    continue;
                }
                auto current_rect = working_list[i];
                bool current_was_merged = false;
                for (int j = i + 1; j < working_list->Count; j++)
                {
                    if (merged_flags[j])
                    {
                        continue;
                    }
                    auto other_rect = working_list[j];
                    auto merged_rect = _compute_merge_area(
                        current_rect, other_rect, merge_threshold
                    );
                    if (merged_rect != nullptr)
                    {
                        current_rect = merged_rect;
                        merged_flags[j] = true;
                        current_was_merged = true;
                        changes_made = true;
                    }
                }
                next_pass_list->Add(current_rect);
                if (current_was_merged)
                    merged_flags[i] = true;
            }
            working_list = next_pass_list;
        } while (changes_made && working_list->Count > 1);
        auto arr = working_list->ToArray();
        Array::Sort(
            arr,
            gcnew Comparison<Tuple<int, int, int, int, float>^>(
                _compareByConfidenceDescending
            )
        );
        return gcnew List<Tuple<int, int, int, int, float>^>(arr);
    }

}


namespace ArrayFireNCC {

    array<af::array*>^ BitmapTemplateMatcher::_calculate_ncc_maps(
        af::array& af_image, af::array& af_image_mask
    )
    {
        auto ncc_maps = gcnew array<af::array*>(_templates->Count);
        for (int active_frame = 0; active_frame < _templates->Count; active_frame++)
        {
            auto bmp_templ = _templates[active_frame];
            auto af_templ_mask = _mask->convert(bmp_templ);
            auto af_templ = _grayscale->convert(bmp_templ);
            auto ncc_map = _matcher->calculate(
                af_image, af_image_mask, af_templ, af_templ_mask
            );
            auto new_ncc_map = new af::array(ncc_map);
            ncc_maps[active_frame] = new_ncc_map;
        }
        return ncc_maps;
    }

    List<Tuple<int, int, int, int, float>^>^ BitmapTemplateMatcher::_calculate_matches(
        array<af::array*>^ ncc_maps, float threshold
    )
    {
        auto detected = gcnew List<Tuple<int, int, int, int, float>^>();
        for (int active_frame = 0; active_frame < _templates->Count; active_frame++)
        {
            auto bmp_templ = _templates[active_frame];
            auto& ncc_map = *ncc_maps[active_frame];
            auto current_detected = _detector->detect(ncc_map, threshold);
            for (int j = 0; j < current_detected->Count; j++)
            {
                auto current = current_detected[j];
                auto templ_width = bmp_templ->Width;
                auto templ_height = bmp_templ->Height;
                auto tuple = gcnew Tuple<int, int, int, int, float>(
                    (current->Item1 - (templ_width / 2)),
                    (current->Item2 - (templ_height / 2)),
                    templ_width,
                    templ_height,
                    current->Item3
                );
                detected->Add(tuple);
            }
        }
        return detected;
    }

    void BitmapTemplateMatcher::_delete_ncc_maps(array<af::array*>^ ncc_maps)
    {
        for (int map_index = 0; map_index < ncc_maps->Length; map_index++)
        {
            delete ncc_maps[map_index];
            ncc_maps[map_index] = nullptr;
        }
    }

    BitmapTemplateMatcher::BitmapTemplateMatcher(
        List<Bitmap^>^ templates,
        AbstractNormalizedCrossCorrelation^ matcher,
        AbstractBitmapToGrayscaleConverter^ grayscale,
        AbstractBitmapToMaskConverter^ mask,
        AbstractLocationDetector^ detector
    )
    {
        _templates = templates;
        _matcher = matcher;
        _grayscale = grayscale;
        _mask = mask;
        _detector = detector;
    }

    BitmapTemplateMatcher::~BitmapTemplateMatcher(void) {
        delete _templates;
    }

    List<Tuple<int, int, int, int, float>^>^ BitmapTemplateMatcher::calculate(
        UINT32* image,
        int image_width,
        int image_height,
        int image_stride,
        float threshold
    )
    {
        auto af_image_mask = _mask->convert(
            image, image_width, image_height, image_stride
        );
        auto af_image = _grayscale->convert(
            image, image_width, image_height, image_stride
        );
        auto ncc_maps = _calculate_ncc_maps(af_image, af_image_mask);
        auto matches = _calculate_matches(ncc_maps, threshold);
        _delete_ncc_maps(ncc_maps);
        return matches;
    }

    List<Tuple<int, int, int, int, float>^>^ BitmapTemplateMatcher::calculate(
        Bitmap^ bitmap, float threshold
    )
    {
        auto rect = System::Drawing::Rectangle(0, 0, bitmap->Width, bitmap->Height);
        auto bitmap_data = bitmap->LockBits(
            rect,
            ImageLockMode::ReadOnly,
            PixelFormat::Format32bppArgb
        );
        try
        {
            return calculate(
                static_cast<UINT32*>(bitmap_data->Scan0.ToPointer()),
                static_cast<int>(bitmap_data->Width),
                static_cast<int>(bitmap_data->Height),
                static_cast<int>(bitmap_data->Stride / sizeof(UINT32)),
                threshold
            );
        }
        finally
        {
            bitmap->UnlockBits(bitmap_data);
        }
    }

    List<Bitmap^>^ BitmapTemplateMatcher::get_templates()
    {
        auto templates = gcnew List<Bitmap^>();
        for (int i = 0; i < _templates->Count; i++)
        {
            templates->Add(_templates[i]);
        }
        return templates;
    }
}


namespace ArrayFireNCC {

    List<Bitmap^>^ BitmapTemplateMatcherBuilder::_deep_clone_templates(void)
    {
        auto templates = gcnew List<Bitmap^>();
        for (int active_frame = 0; active_frame < _templates->Count; active_frame++)
        {
            auto stream = gcnew System::IO::MemoryStream();
            _templates[active_frame]->Save(stream, ImageFormat::Png);
            stream->Position = 0;
            templates->Add(gcnew Bitmap(stream));
        }
        return templates;
    }

    BitmapTemplateMatcherBuilder::BitmapTemplateMatcherBuilder() {
        _templates = nullptr;
    }

    AbstractBitmapTemplateMatcherBuilder^ BitmapTemplateMatcherBuilder::with_templates(
        List<Bitmap^>^ templates
    ) {
        _templates = templates;
        return this;
    }

    AbstractBitmapTemplateMatcher^ BitmapTemplateMatcherBuilder::build(void) {
        return gcnew BitmapTemplateMatcher(
            _deep_clone_templates(),
            gcnew NormalizedCrossCorrelationFacade(),
            gcnew BitmapToZeroMeanGrayscaleConverter(),
            gcnew BitmapToMaskConverter(),
            gcnew LocationDetector()
        );
    }

}