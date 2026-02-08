#include "pch.h"
#include "TemplateMatcher.h"


namespace ArrayFireNCC {

    void ArrayFireSaver::save(const af::array& arr, const std::string& filename)
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
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float val = af_data[y * width + x];
                val = std::max(std::min(val, 0.0f), 1.0f);
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
        delete[] af_data;
    }

}


namespace ArrayFireNCC {

    af::array BitmapToZeroMeanGrayscaleConverter::convert(
        UINT32* image,
        int image_width,
        int image_height,
        int image_stride,
        af::array& image_mask
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
        // Zero-mean the masked region
        auto masked_grayscale = grayscale_image * image_mask;
        auto masked_count = af::sum<double>(image_mask);
        auto masked_sum = af::sum<double>(masked_grayscale);
        if (masked_count > 1e-12)
        {
            auto mean_value = masked_sum / masked_count;
            grayscale_image = grayscale_image - mean_value;
        }
        return grayscale_image;
    }

    af::array BitmapToZeroMeanGrayscaleConverter::convert(
        Bitmap^ bitmap,
        af::array& image_mask
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
            return convert(
                static_cast<UINT32*>(bitmap_data->Scan0.ToPointer()),
                static_cast<int>(bitmap_data->Width),
                static_cast<int>(bitmap_data->Height),
                static_cast<int>(bitmap_data->Stride / sizeof(UINT32)),
                image_mask
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
        af::array spatial_map = af::shift(product, -x_shift, -y_shift);
        const int start_x = static_cast<int>(Math::Floor((templ_dims[0] / 2.0f) - 0.5f));
        const int start_y = static_cast<int>(Math::Floor((templ_dims[1] / 2.0f) - 0.5f));
        return spatial_map(
            af::seq(start_x, static_cast<double>(start_x + image_dims[0] - 1)),
            af::seq(start_y, static_cast<double>(start_y + image_dims[1] - 1)),
            af::span,
            af::span
        );
    }

}


namespace ArrayFireNCC {

    List<Tuple<int, int>^>^ LocationDetector::detect(af::array& ncc_map, float threshold)
    {
        // Find peaks on GPU
        af::array locations = af::where(ncc_map >= threshold);
        int count = static_cast<int>(locations.elements());
        if (count == 0)
        {
            return gcnew List<Tuple<int, int>^>();
        }
        // Convert to coordinates on GPU
        af::array coords = af::moddims(locations, count);
        af::array x = coords % ncc_map.dims(0);
        af::array y = coords / ncc_map.dims(0);
        // Single transfer to host
        array<long long>^ h_x = gcnew array<long long>(count);
        array<long long>^ h_y = gcnew array<long long>(count);
        // Pin the arrays to get native pointers
        pin_ptr<long long> pin_x = &h_x[0];
        pin_ptr<long long> pin_y = &h_y[0];
        // Copy data from GPU to pinned arrays
        x.host(pin_x);
        y.host(pin_y);
        // Create managed list
        auto matches = gcnew List<Tuple<int, int>^>();
        for (int i = 0; i < count; ++i)
        {
            matches->Add(
                gcnew Tuple<int, int>(
                    static_cast<int>(h_x[i]),
                    static_cast<int>(h_y[i])
                )
            );
        }
        return matches;
    }

}


namespace ArrayFireNCC {

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
        af::array image_ft = af::fft2(padded_image);
        af::array templ_ft = af::fft2(padded_templ);
        // Multiply image_ft by conjugate of templ_ft
        auto product_ft = image_ft * af::conjg(templ_ft);
        // Inverse Fourier transform
        auto out = af::real(af::ifft2(product_ft));
        // Roll and cut to the spatial domain
        return _shifter->shift(out, target_dims, image_dims, templ_dims);
    }

    NormalizedCrossCorrelation::NormalizedCrossCorrelation(
        AbstractArrayFireShifter^ shifter
    )
    {
        _shifter = shifter;
    }

    af::array NormalizedCrossCorrelation::calculate(
        const af::array& image,
        const af::array& image_mask,
        const af::array& templ,
        const af::array& templ_mask
    )
    {
        // Compute masked images.
        auto masked_image = image * image_mask;
        auto masked_templ = templ * templ_mask;
        // Compute components
        auto cross_correlation = _compute_cross_correlation(masked_image, masked_templ);
        auto image_sq_sum = _compute_cross_correlation(af::pow(masked_image, 2), templ_mask);
        auto templ_sq_sum = _compute_cross_correlation(image_mask, af::pow(masked_templ, 2));
        // Compute normalization term with epsilon for stability
        auto normalization = (af::sqrt(templ_sq_sum) * af::sqrt(image_sq_sum)) + 1e-6f;
        auto ncc_map = cross_correlation / normalization;
        return ncc_map;
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

    Tuple<int, int, int, int>^ RectangleMerger::_compute_merge_area(
        Tuple<int, int, int, int>^ rect_1,
        Tuple<int, int, int, int>^ rect_2,
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
        // Return as (x, y, width, height)
        return gcnew Tuple<int, int, int, int>(
            merged_x1,
            merged_y1,
            merged_x2 - merged_x1,
            merged_y2 - merged_y1
        );
    }

    List<Tuple<int, int, int, int>^>^ RectangleMerger::merge(
        List<Tuple<int, int, int, int>^>^ rectangles, float merge_threshold
    )
    {
        if (rectangles == nullptr || rectangles->Count == 0)
        {
            return gcnew List<Tuple<int, int, int, int>^>();
        }
        auto working_list = gcnew List<Tuple<int, int, int, int>^>(rectangles);
        bool changes_made;
        do
        {
            changes_made = false;
            auto next_pass_list = gcnew List<Tuple<int, int, int, int>^>();
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
        }
        while (changes_made && working_list->Count > 1);
        return working_list;
    }

}


namespace ArrayFireNCC {

    array<af::array*>^ BitmapTemplateMatcher::_calculate_ncc_maps(
        af::array& af_image, af::array& af_image_mask
    )
    {
        auto frame_count = _templates->GetFrameCount(FrameDimension::Time);
        auto ncc_maps = gcnew array<af::array*>(frame_count);
        for (int active_frame = 0; active_frame < frame_count; active_frame++)
        {
            _templates->SelectActiveFrame(FrameDimension::Time, active_frame);
            auto af_templ_mask = _mask->convert(_templates);
            auto af_templ = _grayscale->convert(_templates, af_templ_mask);
            auto ncc_map = _matcher->calculate(
                af_image, af_image_mask, af_templ, af_templ_mask
            );
            auto new_ncc_map = new af::array(ncc_map);
            ncc_maps[active_frame] = new_ncc_map;
        }
        return ncc_maps;
    }

    List<Tuple<int, int, int, int>^>^ BitmapTemplateMatcher::_calculate_matches(
        array<af::array*>^ ncc_maps, float threshold
    )
    {
        auto frame_count = _templates->GetFrameCount(FrameDimension::Time);
        auto detected = gcnew List<Tuple<int, int, int, int>^>();
        for (int active_frame = 0; active_frame < frame_count; active_frame++)
        {
            _templates->SelectActiveFrame(FrameDimension::Time, active_frame);
            auto& ncc_map = *ncc_maps[active_frame];
            auto current_detected = _detector->detect(ncc_map, threshold);
            for (int j = 0; j < current_detected->Count; j++)
            {
                auto current = current_detected[j];
                auto templ_width = _templates->Width;
                auto templ_height = _templates->Height;
                auto tuple = gcnew Tuple<int, int, int, int>(
                    (current->Item1 - (templ_width / 2)),
                    (current->Item2 - (templ_height / 2)),
                    templ_width,
                    templ_height
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
        Bitmap^ templates,
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

    List<Tuple<int, int, int, int>^>^ BitmapTemplateMatcher::calculate(
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
            image, image_width, image_height, image_stride, af_image_mask
        );
        auto ncc_maps = _calculate_ncc_maps(af_image, af_image_mask);
        auto matches = _calculate_matches(ncc_maps, threshold);
        _delete_ncc_maps(ncc_maps);
        return matches;
    }

    List<Tuple<int, int, int, int>^>^ BitmapTemplateMatcher::calculate(
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

}


namespace ArrayFireNCC {

    Bitmap^ BitmapTemplateMatcherBuilder::_deep_clone_gif(void)
    {
        System::IO::MemoryStream^ stream = gcnew System::IO::MemoryStream();
        _image->Save(stream, ImageFormat::Gif);
        stream->Position = 0;
        return gcnew Bitmap(stream);
    }

    BitmapTemplateMatcherBuilder::BitmapTemplateMatcherBuilder() {
        _image = nullptr;
    }

    AbstractBitmapTemplateMatcherBuilder^ BitmapTemplateMatcherBuilder::with_template(
        Bitmap^ image
    ) {
        _image = image;
        return this;
    }

    AbstractBitmapTemplateMatcher^ BitmapTemplateMatcherBuilder::build(void) {
        return gcnew BitmapTemplateMatcher(
            (Bitmap^)_deep_clone_gif(),
            gcnew NormalizedCrossCorrelationFacade(),
            gcnew BitmapToZeroMeanGrayscaleConverter(),
            gcnew BitmapToMaskConverter(),
            gcnew LocationDetector()
        );
    }

}