#include "pch.h"
#include "TemplateMatcherTests.h"


namespace ArrayFireNCCTests {

    void BitmapToZeroMeanGrayscaleConverterTest::_bitmap_fixture(UINT32* test_bitmap) {
        // Create a 3x5 bitmap with different colors for testing
        // Row 0: Black to white gradient
        test_bitmap[0] = 0xFF000000;
        test_bitmap[1] = 0xFF808080;
        test_bitmap[2] = 0xFFFFFFFF;
        // Row 1: Primary colors
        test_bitmap[3] = 0xFFFF0000;
        test_bitmap[4] = 0xFF00FF00;
        test_bitmap[5] = 0xFF0000FF;
        // Row 2: Mixed colors
        test_bitmap[6] = 0xFFFFFF00;
        test_bitmap[7] = 0xFF00FFFF;
        test_bitmap[8] = 0xFFFF00FF;
        // Row 3: Different shades
        test_bitmap[9] = 0xFF643219;
        test_bitmap[10] = 0xFF326419;
        test_bitmap[11] = 0xFF193264;
        // Row 4: More variations
        test_bitmap[12] = 0xFFC86432;
        test_bitmap[13] = 0xFF969696;
        test_bitmap[14] = 0xFF4B7DAF;
    }

    af::array BitmapToZeroMeanGrayscaleConverterTest::_image_mask_fixture(void) {
        float mask_data[] = {
            1, 0, 1,
            0, 1, 0,
            1, 0, 1,
            0, 1, 0,
            1, 0, 1
        };
        return af::array(3, 5, mask_data);
    }

    af::array BitmapToZeroMeanGrayscaleConverterTest::_image_maskless_fixture(void) {
        float mask_data[] = {
            1, 1, 1,
            1, 1, 1,
            1, 1, 1,
            1, 1, 1,
            1, 1, 1
        };
        return af::array(3, 5, mask_data);
    }

    float BitmapToZeroMeanGrayscaleConverterTest::_masked_sum(float* bitmap, float* mask, int count) {
        float sum = 0.0f;
        for (int i = 0; i < count; i++)
            if (mask[i] > 0.0f)
                sum += bitmap[i];
        return sum;
    }

    void BitmapToZeroMeanGrayscaleConverterTest::run(void) {
        test_convert_with_no_mask();
        test_convert_with_mask();
    }

    void BitmapToZeroMeanGrayscaleConverterTest::test_convert_with_no_mask(void) {
        float sum = 0.0f;
        auto mask = _image_maskless_fixture();
        UINT32 bitmap_space[15] = { 0 };
        _bitmap_fixture(bitmap_space);
        auto converter = gcnew BitmapToZeroMeanGrayscaleConverter();
        auto converted = converter->convert(bitmap_space, 3, 5, 3, mask);
        float converted_buffer[15] = { 0 };
        float converted_mask[15] = { 0 };
        mask.host(converted_mask);
        converted.host(converted_buffer);
        // Row 0: Black to white gradient
        _assertions.Assert(std::abs(converted_buffer[0] + 0.450921565) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[1] - 0.051039248) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[2] - 0.549078465) < 0.00001f);
        // Row 1: Primary colors
        _assertions.Assert(std::abs(converted_buffer[3] + 0.151921570) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[4] - 0.136078447) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[5] + 0.336921573) < 0.00001f);
        // Row 2: Mixed colors
        _assertions.Assert(std::abs(converted_buffer[6] - 0.435078472) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[7] - 0.250078470) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[8] + 0.037921577) < 0.00001f);
        // Row 3: Different shades
        _assertions.Assert(std::abs(converted_buffer[9] + 0.207392156) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[10] + 0.150921553) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[11] + 0.261803925) < 0.00001f);
        // Row 4: More variations
        _assertions.Assert(std::abs(converted_buffer[12] - 0.036137253) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[13] - 0.137313753) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[14] - 0.003000020) < 0.00001f);
        _assertions.Assert(_masked_sum(converted_buffer, converted_mask, 15) < 0.00001f);
    }

    void BitmapToZeroMeanGrayscaleConverterTest::test_convert_with_mask(void) {
        float sum = 0.0f;
        auto mask = _image_mask_fixture();
        UINT32 bitmap_space[15] = { 0 };
        _bitmap_fixture(bitmap_space);
        auto converter = gcnew BitmapToZeroMeanGrayscaleConverter();
        auto converted = converter->convert(bitmap_space, 3, 5, 3, mask);
        float converted_buffer[15] = { 0 };
        float converted_mask[15] = { 0 };
        mask.host(converted_mask);
        converted.host(converted_buffer);
        // Row 0: Black to white gradient
        _assertions.Assert(std::abs(converted_buffer[0] + 0.515872538) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[1] + 0.013911724) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[2] - 0.484127462) < 0.00001f);
        // Row 1: Primary colors
        _assertions.Assert(std::abs(converted_buffer[3] + 0.216872543) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[4] - 0.071127474) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[5] + 0.401872545) < 0.00001f);
        // Row 2: Mixed colors
        _assertions.Assert(std::abs(converted_buffer[6] - 0.370127499) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[7] - 0.185127497) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[8] + 0.102872550) < 0.00001f);
        // Row 3: Different shades
        _assertions.Assert(std::abs(converted_buffer[9] + 0.272343129) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[10] + 0.215872526) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[11] + 0.326754868) < 0.00001f);
        // Row 4: More variations
        _assertions.Assert(std::abs(converted_buffer[12] + 0.028813719) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[13] - 0.072362780) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[14] + 0.061950951) < 0.00001f);
        _assertions.Assert(_masked_sum(converted_buffer, converted_mask, 15) < 0.00001f);
    }

}


namespace ArrayFireNCCTests {

    void BitmapToMaskConverterTest::_create_test_bitmap(
        UINT32* test_bitmap,
        int width,
        int height,
        const std::vector<int>& alpha_values
    ) {
        for (int y = 0; y < height; ++y)
            for (int x = 0; x < width; ++x) {
                UINT32 alpha = alpha_values[y * width + x];
                test_bitmap[y * width + x] = alpha << 0x18;
            }
    }

    void BitmapToMaskConverterTest::run(void) {
        test_mask_2px_left();
        test_mask_2px_top();
        test_mask_2px_right();
        test_mask_2px_bottom();
        test_mask_2px_all();
    }

    void BitmapToMaskConverterTest::test_mask_2px_left(void) {
        std::vector<int> alpha_values = {
            255, 255,   0,   0,   0,
            255, 255,   0,   0,   0,
            255, 255,   0,   0,   0,
            255, 255,   0,   0,   0,
        };
        float expected[] = {
              1,   1,   0,   0,   0,
              1,   1,   0,   0,   0,
              1,   1,   0,   0,   0,
              1,   1,   0,   0,   0,
        };
        UINT32 test_bitmap[5 * 4] = { 0 };
        _create_test_bitmap(test_bitmap, 5, 4, alpha_values);
        af::array mask = BitmapToMaskConverter().convert(test_bitmap, 5, 4, 5);
        af::array expected_mask(5, 4, expected);
        _assertions.Assert(af::allTrue<bool>(mask == expected_mask));
    }

    void BitmapToMaskConverterTest::test_mask_2px_top(void) {
        std::vector<int> alpha_values = {
            255, 255, 255, 255, 255,
            255, 255, 255, 255, 255,
              0,   0,   0,   0,   0,
              0,   0,   0,   0,   0,
        };
        float expected[] = {
              1,   1,   1,   1,   1,
              1,   1,   1,   1,   1,
              0,   0,   0,   0,   0,
              0,   0,   0,   0,   0
        };
        UINT32 test_bitmap[5 * 4] = { 0 };
        _create_test_bitmap(test_bitmap, 5, 4, alpha_values);
        af::array mask = BitmapToMaskConverter().convert(test_bitmap, 5, 4, 5);
        af::array expected_mask(5, 4, expected);
        _assertions.Assert(af::allTrue<bool>(mask == expected_mask));
    }

    void BitmapToMaskConverterTest::test_mask_2px_right(void) {
        std::vector<int> alpha_values = {
              0,   0,   0, 255, 255,
              0,   0,   0, 255, 255,
              0,   0,   0, 255, 255,
              0,   0,   0, 255, 255
        };
        float expected[] = {
              0,   0,   0,   1,   1,
              0,   0,   0,   1,   1,
              0,   0,   0,   1,   1,
              0,   0,   0,   1,   1
        };
        UINT32 test_bitmap[5 * 4] = { 0 };
        _create_test_bitmap(test_bitmap, 5, 4, alpha_values);
        af::array mask = BitmapToMaskConverter().convert(test_bitmap, 5, 4, 5);
        af::array expected_mask(5, 4, expected);
        _assertions.Assert(af::allTrue<bool>(mask == expected_mask));
    }

    void BitmapToMaskConverterTest::test_mask_2px_bottom(void) {
        std::vector<int> alpha_values = {
              0,   0,   0,   0,   0,
              0,   0,   0,   0,   0,
            255, 255, 255, 255, 255,
            255, 255, 255, 255, 255
        };
        float expected[] = {
              0,   0,   0,   0,   0,
              0,   0,   0,   0,   0,
              1,   1,   1,   1,   1,
              1,   1,   1,   1,   1
        };
        UINT32 test_bitmap[5 * 4] = { 0 };
        _create_test_bitmap(test_bitmap, 5, 4, alpha_values);
        af::array mask = BitmapToMaskConverter().convert(test_bitmap, 5, 4, 5);
        af::array expected_mask(5, 4, expected);
        _assertions.Assert(af::allTrue<bool>(mask == expected_mask));
    }

    void BitmapToMaskConverterTest::test_mask_2px_all(void) {
        std::vector<int> alpha_values = {
            255, 255, 255, 255, 255,
            255,   0,   0,   0, 255,
            255,   0,   0,   0, 255,
            255, 255, 255, 255, 255
        };
        float expected[] = {
              1,   1,   1,   1,   1,
              1,   0,   0,   0,   1,
              1,   0,   0,   0,   1,
              1,   1,   1,   1,   1
        };
        UINT32 test_bitmap[5 * 4] = { 0 };
        _create_test_bitmap(test_bitmap, 5, 4, alpha_values);
        af::array mask = BitmapToMaskConverter().convert(test_bitmap, 5, 4, 5);
        af::array expected_mask(5, 4, expected);
        _assertions.Assert(af::allTrue<bool>(mask == expected_mask));
    }

}


namespace ArrayFireNCCTests {


    void ArrayFireCenterShifterTest::run(void) {
        test_top_and_bottom_padding();
        test_left_and_right_padding();
        test_all_padding();
        test_target_size();
        test_shift();
    }

    void ArrayFireCenterShifterTest::test_top_and_bottom_padding(void) {
        std::vector<float> shift_values = {
             1.0f,  2.0f,  3.0f,  4.0f,
             5.0f,  6.0f,  7.0f,  8.0f,
             9.0f, 10.0f, 11.0f, 12.0f,
            13.0f, 14.0f, 15.0f, 16.0f,
            17.0f, 18.0f, 19.0f, 20.0f
        };
        std::vector<float> expected = {
             0.0f,  0.0f,  0.0f,  0.0f,
             0.0f,  0.0f,  0.0f,  0.0f,
             1.0f,  2.0f,  3.0f,  4.0f,
             5.0f,  6.0f,  7.0f,  8.0f,
             9.0f, 10.0f, 11.0f, 12.0f,
            13.0f, 14.0f, 15.0f, 16.0f,
            17.0f, 18.0f, 19.0f, 20.0f,
             0.0f,  0.0f,  0.0f,  0.0f,
             0.0f,  0.0f,  0.0f,  0.0f,
             0.0f,  0.0f,  0.0f,  0.0f
        };
        af::array shift_image(4, 5, shift_values.data());
        af::array expected_array(4, 10, expected.data());
        std::vector<float> expected_data(40);
        auto padded_image = ArrayFireCenterShifter().pad(shift_image, af::dim4(4, 10, 0, 0));
        expected_array.host(expected_data.data());
        for (int i = 0; i < expected.size(); i++)
            _assertions.Assert(expected_data[i] == expected[i]);
    }

    void ArrayFireCenterShifterTest::test_left_and_right_padding(void) {
        std::vector<float> shift_values = {
             1.0f,  2.0f,  3.0f,  4.0f,
             5.0f,  6.0f,  7.0f,  8.0f,
             9.0f, 10.0f, 11.0f, 12.0f,
            13.0f, 14.0f, 15.0f, 16.0f,
            17.0f, 18.0f, 19.0f, 20.0f
        };
        std::vector<float> expected = {
             0.0f,  0.0f, 1.0f,   2.0f,  3.0f,  4.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f, 5.0f,   6.0f,  7.0f,  8.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f, 9.0f,  10.0f, 11.0f, 12.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f, 13.0f, 14.0f, 15.0f, 16.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f, 17.0f, 18.0f, 19.0f, 20.0f, 0.0f,  0.0f,  0.0f
        };
        af::array shift_image(4, 5, shift_values.data());
        af::array expected_array(9, 5, expected.data());
        std::vector<float> expected_data(45);
        auto padded_image = ArrayFireCenterShifter().pad(shift_image, af::dim4(9, 5, 0, 0));
        expected_array.host(expected_data.data());
        for (int i = 0; i < expected.size(); i++)
            _assertions.Assert(expected_data[i] == expected[i]);
    }

    void ArrayFireCenterShifterTest::test_all_padding(void) {
        std::vector<float> shift_values = {
             1.0f,  2.0f,  3.0f,  4.0f,
             5.0f,  6.0f,  7.0f,  8.0f,
             9.0f, 10.0f, 11.0f, 12.0f,
            13.0f, 14.0f, 15.0f, 16.0f,
            17.0f, 18.0f, 19.0f, 20.0f
        };
        std::vector<float> expected = {
             0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f,  1.0f,  2.0f,  3.0f,  4.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f,  5.0f,  6.0f,  7.0f,  8.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f,  9.0f, 10.0f, 11.0f, 12.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f, 13.0f, 14.0f, 15.0f, 16.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f, 17.0f, 18.0f, 19.0f, 20.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f, 0.0f,  0.0f,  0.0f,
             0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f, 0.0f,  0.0f,  0.0f,
        };
        af::array shift_image(4, 5, shift_values.data());
        af::array expected_array(9, 8, expected.data());
        std::vector<float> expected_data(72);
        auto padded_image = ArrayFireCenterShifter().pad(shift_image, af::dim4(9, 8, 0, 0));
        expected_array.host(expected_data.data());
        for (int i = 0; i < expected.size(); i++)
            _assertions.Assert(expected_data[i] == expected[i]);
    }

    void ArrayFireCenterShifterTest::test_target_size(void) {
        af::array _123x234(123, 234, f32);
        af::array _234x345(234, 345, f32);
        auto target_size = ArrayFireCenterShifter().target_size(_123x234, _234x345);
        _assertions.Assert(target_size == af::dim4(356, 578, 1, 1));
    }

    void ArrayFireCenterShifterTest::test_shift(void) {
        std::vector<float> product_data = {
             1.0f,  2.0f,  3.0f,  4.0f,  5.0f,  6.0f,  7.0f,  8.0f,
             9.0f, 10.0f, 11.0f, 12.0f, 13.0f, 14.0f, 15.0f, 16.0f,
            17.0f, 18.0f, 19.0f, 20.0f, 21.0f, 22.0f, 23.0f, 24.0f,
            25.0f, 26.0f, 27.0f, 28.0f, 29.0f, 30.0f, 31.0f, 32.0f,
            33.0f, 34.0f, 35.0f, 36.0f, 37.0f, 38.0f, 39.0f, 40.0f,
            41.0f, 42.0f, 43.0f, 44.0f, 45.0f, 46.0f, 47.0f, 48.0f,
        };
        std::vector<float> expected = {
            39.0f, 40.0f, 33.0f, 34.0f, 35.0f, 36.0f,
            47.0f, 48.0f, 41.0f, 42.0f, 43.0f, 44.0f,
             7.0f,  8.0f,  1.0f,  2.0f,  3.0f,  4.0f,
            15.0f, 16.0f,  9.0f, 10.0f, 11.0f, 12.0f,
            23.0f, 24.0f, 17.0f, 18.0f, 19.0f, 20.0f

        };
        std::vector<float> shifted_data(30);
        af::array product(8, 6, product_data.data());
        af::array image(6, 5, 1, 1, f32);
        af::array templ(3, 2, 1, 1, f32);
        af::array expected_array(6, 5, 1, 1, f32);
        auto shifted = ArrayFireCenterShifter().shift(
            product, product.dims(), image.dims(), templ.dims()
        );
        shifted.host(shifted_data.data());
        for (int i = 0; i < shifted_data.size(); i++)
            _assertions.Assert(Math::Abs(expected[i] - shifted_data[i]) < 1e-4f);
    }

}


namespace ArrayFireNCCTests {

    bool LocationDetectorTest::_in_matches(int x, int y, List<Tuple<int, int>^>^ matches) {
        for (int i = 0; i < matches->Count; i++) {
            if (
                matches[i]->Item1 == x
                && matches[i]->Item2 == y
                ) {
                return true;
            }
        }
        return false;
    }

    void LocationDetectorTest::run(void) {
        test_detect();
    }

    void LocationDetectorTest::test_detect(void) {
        std::vector<float> ncc_vector = {
            0.0f,  0.0f,  0.0f,  0.0f,  0.0f, 0.0f,
            0.0f,  0.6f, 0.61f, 0.62f, 0.63f, 0.0f,
            0.0f, 0.64f, 0.65f, 0.66f, 0.67f, 0.0f,
            0.0f,  0.6f, 0.61f, 0.62f, 0.63f, 0.0f,
            0.0f, 0.54f, 0.65f, 0.66f, 0.67f, 0.0f,
            0.0f,  0.0f,  0.0f,  0.0f,  0.0f, 0.0f,
        };
        af::array ncc_array(6, 6, ncc_vector.data());
        auto matches = LocationDetector().detect(ncc_array, 0.65f);
        _assertions.Assert(matches->Count == 6);
        _assertions.Assert(_in_matches(2, 2, matches));
        _assertions.Assert(_in_matches(3, 2, matches));
        _assertions.Assert(_in_matches(4, 2, matches));
        _assertions.Assert(_in_matches(2, 4, matches));
        _assertions.Assert(_in_matches(3, 4, matches));
        _assertions.Assert(_in_matches(4, 4, matches));
    }

}


namespace ArrayFireNCCTests {


    void NormalizedCrossCorrelationTest::run(void) {
        test_perfect_match();
        test_masked_perfect_match();
        test_masked_perfect_even_match();
    }

    void NormalizedCrossCorrelationTest::test_perfect_match() {
        std::vector<float> image_data = {
             0.0f,   1.0f,     2.0f,  3.0f,  4.0f,
             4.0f,  50.0f,   400.0f,  7.0f,  8.0f,
             8.0f, 200.0f,   100.0f, 11.0f, 12.0f,
            12.0f,  13.0f,    14.0f, 15.0f, 16.0f,
            16.0f,  17.0f,    18.0f, 19.0f, 20.0f,
        };
        std::vector<float> image_mask_data = {
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
        };
        std::vector<float> templ_data = {
             50.0f, 400.0f,  7.0f,
            200.0f, 100.0f, 11.0f,
             13.0f,  14.0f, 15.0f
        };
        std::vector<float> templ_mask_data = {
            1, 1, 1,
            1, 1, 1,
            1, 1, 1
        };
        std::vector<float> expected_data = {
            0.158606231f, 0.0757188424f, 0.074761852f, 0.068378239f, 0.456303537f,
            0.047742150f, 0.0705327317f, 0.258780807f, 0.439325482f, 0.460049719f,
            0.062111016f, 0.2169490310f, 1.000000120f, 0.234141961f, 0.544952452f,
            0.076862394f, 0.8179282550f, 0.523872793f, 0.291442364f, 0.584963143f,
            0.552663565f, 0.6423293950f, 0.645036519f, 0.647389412f, 0.797133029f
        };
        std::vector<float> result_data(25);
        af::array image = af::array(5, 5, image_data.data());
        af::array image_mask = af::array(5, 5, image_mask_data.data());
        af::array templ = af::array(3, 3, templ_data.data());
        af::array templ_mask = af::array(3, 3, templ_mask_data.data());
        auto ncc = gcnew NormalizedCrossCorrelationFacade();
        auto result = ncc->calculate(image, image_mask, templ, templ_mask);
        result.host(result_data.data());
        for (int i = 0; i < result_data.size(); i++)
            _assertions.Assert(Math::Abs(expected_data[i] - result_data[i]) < 1e-4f);
    }

    void NormalizedCrossCorrelationTest::test_masked_perfect_match(void) {
        std::vector<float> image_data = {
             0.0f,   1.0f,   2.0f,  3.0f,  4.0f,
             4.0f,  50.0f, 400.0f,  7.0f,  8.0f,
             8.0f, 200.0f, 100.0f, 11.0f, 12.0f,
            12.0f,  13.0f,  14.0f, 15.0f, 16.0f,
            16.0f,  17.0f,  18.0f, 19.0f, 20.0f,
            20.0f,  21.0f,  22.0f, 23.0f, 24.0f,
        };
        std::vector<float> image_mask_data = {
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1
        };
        std::vector<float> templ_data = {
           9099.0f,  400.0f, 6666.0f,
            200.0f, 8888.0f,   11.0f,
           5555.0f,   14.0f, 7777.0f
        };
        std::vector<float> templ_mask_data = {
            0, 1, 0,
            1, 0, 1,
            0, 1, 0
        };
        std::vector<float> expected_data = {
            0.912729502f, 0.071857869f, 0.072622999f, 0.324954569f, 0.415650606f,
            0.032651610f, 0.041964948f, 0.244826987f, 0.454638690f, 0.489754111f,
            0.049452081f, 0.453074425f, 0.999999940f, 0.506582141f, 0.598544180f,
            0.402863115f, 0.917722523f, 0.928376734f, 0.558695912f, 0.651209652f,
            0.455774635f, 0.575925827f, 0.583062768f, 0.589423001f, 0.681755424f,
            0.627680480f, 0.720693529f, 0.724143088f, 0.727257788f, 0.924373686f
        };
        std::vector<float> result_data(30);
        af::array image = af::array(5, 6, image_data.data());
        af::array image_mask = af::array(5, 6, image_mask_data.data());
        af::array templ = af::array(3, 3, templ_data.data());
        af::array templ_mask = af::array(3, 3, templ_mask_data.data());
        auto ncc = gcnew NormalizedCrossCorrelationFacade();
        auto result = ncc->calculate(image, image_mask, templ, templ_mask);
        result.host(result_data.data());
        for (int i = 0; i < result_data.size(); i++)
            _assertions.Assert(Math::Abs(expected_data[i] - result_data[i]) < 1e-4f);
    }

    void NormalizedCrossCorrelationTest::test_masked_perfect_even_match(void) {
        std::vector<float> image_data = {
             0.0f,   1.0f,   2.0f,   3.0f,  4.0f,
             4.0f,   5.0f,   6.0f,   7.0f,  8.0f,
             8.0f, 123.0f,  10.0f, 567.0f, 12.0f,
           234.0f,  13.0f, 456.0f,  15.0f, 16.0f,
            16.0f, 345.0f,  18.0f, 678.0f, 20.0f,
            20.0f,  21.0f,  22.0f,  23.0f, 24.0f,
        };
        std::vector<float> image_mask_data = {
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1
        };
        std::vector<float> templ_data = {
              8.0f,  123.0f,   10.0f,  567.0f,
            234.0f, 9999.0f,  456.0f,   15.0f,
             16.0f,  345.0f,   18.0f,  678.0f
        };
        std::vector<float> templ_mask_data = {
            0, 1, 0, 1,
            1, 0, 1, 0,
            0, 1, 0, 1
        };
        std::vector<float> expected_data = {
            0.914229095f, 0.911527753f, 0.945480525f, 0.917136371f, 0.972206533f,
            0.879813373f, 0.686527133f, 0.925224662f, 0.562737942f, 0.997854769f,
            0.805253804f, 0.940847754f, 0.575110495f, 0.965857327f, 0.564490438f,
            0.946364403f, 1.000000120f, 0.940846860f, 0.666537046f, 0.986157537f,
            0.711557388f, 0.938153148f, 0.497089326f, 0.954308391f, 0.572637498f,
            0.922412455f, 0.749230087f, 0.908773482f, 0.278067172f, 0.973253846f
        };
        std::vector<float> result_data(30);
        af::array image = af::array(5, 6, image_data.data());
        af::array image_mask = af::array(5, 6, image_mask_data.data());
        af::array templ = af::array(4, 3, templ_data.data());
        af::array templ_mask = af::array(4, 3, templ_mask_data.data());
        auto ncc = gcnew NormalizedCrossCorrelationFacade();
        auto result = ncc->calculate(image, image_mask, templ, templ_mask);
        result.host(result_data.data());
        for (int i = 0; i < result_data.size(); i++)
            _assertions.Assert(Math::Abs(expected_data[i] - result_data[i]) < 1e-4f);
    }

}


namespace ArrayFireNCCTests {

    void RectangleMergerTest::run(void) {
        test_merge_all_rectangles_in_list();
        test_merge_with_outlier_rectangle();
        test_merge_with_two_rectangle_groups();
        test_merge_fails_when_threshold_not_met();
    }

    void RectangleMergerTest::test_merge_all_rectangles_in_list(void) {
        auto merger = gcnew RectangleMerger();
        auto rectangles = gcnew List<Tuple<int, int, int, int>^>();
        rectangles->Add(gcnew Tuple<int, int, int, int>(0, 0, 60, 60));
        rectangles->Add(gcnew Tuple<int, int, int, int>(40, 0, 60, 60));
        rectangles->Add(gcnew Tuple<int, int, int, int>(0, 40, 60, 60));
        rectangles->Add(gcnew Tuple<int, int, int, int>(40, 40, 60, 60));
        float merge_threshold = 0.1f;
        auto merged_rects = merger->merge(rectangles, merge_threshold);
        _assertions.Assert(merged_rects->Count == 1);
        auto final_rect = merged_rects[0];
        _assertions.Assert(final_rect->Item1 == 0);
        _assertions.Assert(final_rect->Item2 == 0);
        _assertions.Assert(final_rect->Item3 == 100);
        _assertions.Assert(final_rect->Item4 == 100);
    }

    void RectangleMergerTest::test_merge_with_outlier_rectangle(void) {
        auto merger = gcnew RectangleMerger();
        auto rectangles = gcnew List<Tuple<int, int, int, int>^>();
        rectangles->Add(gcnew Tuple<int, int, int, int>(0, 0, 60, 60));
        rectangles->Add(gcnew Tuple<int, int, int, int>(40, 0, 60, 60));
        rectangles->Add(gcnew Tuple<int, int, int, int>(0, 40, 60, 60));
        rectangles->Add(gcnew Tuple<int, int, int, int>(200, 200, 30, 30));
        float merge_threshold = 0.1f;
        auto merged_rects = merger->merge(rectangles, merge_threshold);
        _assertions.Assert(merged_rects->Count == 2);
        Tuple<int, int, int, int>^ merged_rect = nullptr;
        Tuple<int, int, int, int>^ outlier_rect = nullptr;
        for each (auto rect in merged_rects) {
            if (rect->Item3 == 100 && rect->Item4 == 100)
                merged_rect = rect;
            else if (rect->Item1 == 200 && rect->Item2 == 200)
                outlier_rect = rect;
        }
        _assertions.Assert(merged_rect != nullptr);
        _assertions.Assert(merged_rect->Item1 == 0);
        _assertions.Assert(merged_rect->Item2 == 0);
        _assertions.Assert(merged_rect->Item3 == 100);
        _assertions.Assert(merged_rect->Item4 == 100);
        _assertions.Assert(outlier_rect != nullptr);
        _assertions.Assert(outlier_rect->Item1 == 200);
        _assertions.Assert(outlier_rect->Item2 == 200);
        _assertions.Assert(outlier_rect->Item3 == 30);
        _assertions.Assert(outlier_rect->Item4 == 30);
    }

    void RectangleMergerTest::test_merge_with_two_rectangle_groups(void) {
        auto merger = gcnew RectangleMerger();
        auto rectangles = gcnew List<Tuple<int, int, int, int>^>();
        rectangles->Add(gcnew Tuple<int, int, int, int>(0, 0, 60, 60));
        rectangles->Add(gcnew Tuple<int, int, int, int>(40, 0, 60, 60));
        rectangles->Add(gcnew Tuple<int, int, int, int>(0, 40, 60, 60));
        rectangles->Add(gcnew Tuple<int, int, int, int>(40, 40, 60, 60));
        rectangles->Add(gcnew Tuple<int, int, int, int>(200, 200, 50, 50));
        rectangles->Add(gcnew Tuple<int, int, int, int>(240, 200, 50, 50));
        rectangles->Add(gcnew Tuple<int, int, int, int>(200, 240, 50, 50));
        rectangles->Add(gcnew Tuple<int, int, int, int>(240, 240, 50, 50));
        float merge_threshold = 0.1f;
        auto merged_rects = merger->merge(rectangles, merge_threshold);
        _assertions.Assert(merged_rects->Count == 2);
        Tuple<int, int, int, int>^ group1_rect = nullptr;
        Tuple<int, int, int, int>^ group2_rect = nullptr;
        for each (auto rect in merged_rects) {
            if (rect->Item1 == 0 && rect->Item2 == 0)
                group1_rect = rect;
            else if (rect->Item1 == 200 && rect->Item2 == 200)
                group2_rect = rect;
        }
        _assertions.Assert(group1_rect != nullptr);
        _assertions.Assert(group1_rect->Item1 == 0);
        _assertions.Assert(group1_rect->Item2 == 0);
        _assertions.Assert(group1_rect->Item3 == 100);
        _assertions.Assert(group1_rect->Item4 == 100);
        _assertions.Assert(group2_rect != nullptr);
        _assertions.Assert(group2_rect->Item1 == 200);
        _assertions.Assert(group2_rect->Item2 == 200);
        _assertions.Assert(group2_rect->Item3 == 90);
        _assertions.Assert(group2_rect->Item4 == 90);
    }

    void RectangleMergerTest::test_merge_fails_when_threshold_not_met(void) {
        auto merger = gcnew RectangleMerger();
        auto rectangles = gcnew List<Tuple<int, int, int, int>^>();
        rectangles->Add(gcnew Tuple<int, int, int, int>(0, 0, 100, 100));
        rectangles->Add(gcnew Tuple<int, int, int, int>(95, 95, 100, 100));
        float merge_threshold = 0.5f;
        auto merged_rects = merger->merge(rectangles, merge_threshold);
        _assertions.Assert(merged_rects->Count == 2);
        bool found_first = false;
        bool found_second = false;
        for each (auto rect in merged_rects) {
            if (
                rect->Item1 == 0
                && rect->Item2 == 0
                && rect->Item3 == 100
                && rect->Item4 == 100
            )
                found_first = true;
            if (
                rect->Item1 == 95
                && rect->Item2 == 95
                && rect->Item3 == 100
                && rect->Item4 == 100
            )
                found_second = true;
        }
        _assertions.Assert(found_first);
        _assertions.Assert(found_second);
    }

}


namespace ArrayFireNCCTests {

    Bitmap^ BitmapTemplateMatcherTest::_image_fixture(void) {
        return dynamic_cast<Bitmap^>(
            Bitmap::FromFile("TemplateMatcherTestImage.png")
        );
    }

    Bitmap^ BitmapTemplateMatcherTest::_template_fixture() {
        return dynamic_cast<Bitmap^>(
            Bitmap::FromFile("TemplateMatcherTestTemplate.gif")
        );
    }

    AbstractBitmapTemplateMatcher^ BitmapTemplateMatcherTest::_matcher_fixture(void) {
        auto templ = _template_fixture();
        auto builder = gcnew BitmapTemplateMatcherBuilder();
        return builder->with_template(templ)->build();

    }

    bool BitmapTemplateMatcherTest::_point_in_matches(
        List<Tuple<int, int, int, int>^>^ matches, int x, int y
    ) {
        for each (auto match in matches) {
            auto left = match->Item1;
            auto top = match->Item2;
            auto right = match->Item1 + match->Item3;
            auto bottom = match->Item2 + match->Item4;
            if (x >= left && x <= right && y >= top && y <= bottom)
                return true;
        }
        return false;
    }

    bool BitmapTemplateMatcherTest::_matches_lt_threshold_area(
        List<Tuple<int, int, int, int>^>^ matches, int area
    ) {
        for each (auto match in matches) {
            auto left = match->Item1;
            auto top = match->Item2;
            auto right = match->Item1 + match->Item3;
            auto bottom = match->Item2 + match->Item4;
            if (match->Item3 * match->Item4 > area)
                return false;
        }
        return true;
    }

    bool BitmapTemplateMatcherTest::_matches_gt_threshold_area(
        List<Tuple<int, int, int, int>^>^ matches, int area
    ) {
        for each (auto match in matches) {
            auto left = match->Item1;
            auto top = match->Item2;
            auto right = match->Item1 + match->Item3;
            auto bottom = match->Item2 + match->Item4;
            if (match->Item3 * match->Item4 < area)
                return false;
        }
        return true;
    }

    void BitmapTemplateMatcherTest::run(void) {
        test_calculate();
    }

    void BitmapTemplateMatcherTest::test_calculate(void) {
        auto image = _image_fixture();
        auto matcher = _matcher_fixture();
        auto matches = matcher->calculate(image, 0.6f);
        matches = RectangleMerger().merge(matches, 0.3f);
        _assertions.Assert(matches->Count == 8);
        _assertions.Assert(_matches_gt_threshold_area(matches, 900));
        _assertions.Assert(_matches_lt_threshold_area(matches, 1200));
        _assertions.Assert(_point_in_matches(matches, 150, 110));
        _assertions.Assert(_point_in_matches(matches, 795, 125));
        _assertions.Assert(_point_in_matches(matches, 1125, 155));
        _assertions.Assert(_point_in_matches(matches, 640, 360));
        _assertions.Assert(_point_in_matches(matches, 1165, 395));
        _assertions.Assert(_point_in_matches(matches, 75, 390));
        _assertions.Assert(_point_in_matches(matches, 290, 630));
        _assertions.Assert(_point_in_matches(matches, 775, 700));
    }

}


namespace ArrayFireNCCTests {

    void TestTemplateMatcher::run(void) {
        _CrtSetDbgFlag(0);
        {
            af::setBackend(AF_BACKEND_CPU);
            af::setDevice(0);
            BitmapToZeroMeanGrayscaleConverterTest().run();
            BitmapToMaskConverterTest().run();
            ArrayFireCenterShifterTest().run();
            LocationDetectorTest().run();
            NormalizedCrossCorrelationTest().run();
            RectangleMergerTest().run();
            BitmapTemplateMatcherTest().run();
        }
        _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
    }

}