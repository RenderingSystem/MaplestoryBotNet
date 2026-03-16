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

    float BitmapToZeroMeanGrayscaleConverterTest::_masked_sum(float* bitmap, float* mask, int count) {
        float sum = 0.0f;
        for (int i = 0; i < count; i++)
            if (mask[i] > 0.0f)
                sum += bitmap[i];
        return sum;
    }

    void BitmapToZeroMeanGrayscaleConverterTest::run(void) {
        test_convert();
    }

    void BitmapToZeroMeanGrayscaleConverterTest::test_convert(void) {
        float sum = 0.0f;
        UINT32 bitmap_space[15] = { 0 };
        _bitmap_fixture(bitmap_space);
        auto converter = gcnew BitmapToZeroMeanGrayscaleConverter();
        auto converted = converter->convert(bitmap_space, 3, 5, 3);
        float converted_buffer[15] = { 0 };
        float converted_mask[15] = { 0 };
        converted.host(converted_buffer);
        // Row 0: Black to white gradient
        _assertions.Assert(std::abs(converted_buffer[0] - 0.000000000) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[1] - 0.501960814) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[2] - 1.000000000) < 0.00001f);
        // Row 1: Primary colors
        _assertions.Assert(std::abs(converted_buffer[3] - 0.298999995) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[4] - 0.587000012) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[5] - 0.114000000) < 0.00001f);
        // Row 2: Mixed colors
        _assertions.Assert(std::abs(converted_buffer[6] - 0.886000037) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[7] - 0.701000035) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[8] - 0.412999988) < 0.00001f);
        // Row 3: Different shades
        _assertions.Assert(std::abs(converted_buffer[ 9] - 0.243529409) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[10] - 0.300000012) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[11] - 0.189117655) < 0.00001f);
        // Row 4: More variations
        _assertions.Assert(std::abs(converted_buffer[12] - 0.487058818) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[13] - 0.588235319) < 0.00001f);
        _assertions.Assert(std::abs(converted_buffer[14] - 0.453921586) < 0.00001f);
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

    bool LocationDetectorTest::_in_matches(
        int x,
        int y,
        float confidence,
        List<Tuple<int, int, float>^>^ matches
    ) {
        for (int i = 0; i < matches->Count; i++) {
            if (
                matches[i]->Item1 == x
                && matches[i]->Item2 == y
                && matches[i]->Item3 == confidence
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
        _assertions.Assert(_in_matches(2, 2, 0.65f, matches));
        _assertions.Assert(_in_matches(3, 2, 0.66f, matches));
        _assertions.Assert(_in_matches(4, 2, 0.67f, matches));
        _assertions.Assert(_in_matches(2, 4, 0.65f, matches));
        _assertions.Assert(_in_matches(3, 4, 0.66f, matches));
        _assertions.Assert(_in_matches(4, 4, 0.67f, matches));
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
            0.000000000f,  0.0000000000f,  0.000000000f,  0.0000000000f, 0.000000000f,
            0.000000000f, -0.3740542830f, -0.101577379f,  0.2487899960f, 0.000000000f,
            0.000000000f, -0.1831749830f,  1.000000120f, -0.0566605851f, 0.000000000f,
            0.000000000f,  0.7232999210f,  0.268781185f, -0.1831442120f, 0.000000000f,
            0.000000000f,  0.0000000000f,  0.000000000f,  0.0000000000f, 0.000000000f
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
        {
            _assertions.Assert(Math::Abs(expected_data[i] - result_data[i]) < 1e-4f);
        }
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
            0.000000000f,  0.000000000f,  0.000000000f,  0.0000000000f, 0.00000000f,
            0.000000000f, -0.816060185f, -0.496799260f,  0.1422786270f, 0.00000000f,
            0.000000000f, -0.162611291f,  1.000000240f,  0.0893548355f, 0.00000000f,
            0.000000000f,  0.869821668f,  0.858997345f, -0.9276137950f, 0.00000000f,
            0.000000000f, -0.927625299f, -0.927591860f, -0.9275859000f, 0.00000000f,
            0.000000000f,  0.000000000f,  0.000000000f,  0.0000000000f, 0.00000000f
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
            0.000000000f, 0.000000000f,  0.0000000000f, 0.000000000f, 0.000000000f,
            0.000000000f, 0.642594993f,  0.6227093340f, 0.000000000f, 0.000000000f,
            0.000000000f, 0.622741878f, -0.0392209105f, 0.000000000f, 0.000000000f,
            0.000000000f, 0.999999881f,  0.6228618620f, 0.000000000f, 0.000000000f,
            0.000000000f, 0.622715771f, -0.4754244980f, 0.000000000f, 0.000000000f,
            0.000000000f, 0.000000000f,  0.0000000000f, 0.000000000f, 0.000000000f,
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
        {
            _assertions.Assert(Math::Abs(expected_data[i] - result_data[i]) < 1e-4f);
        }
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
        auto rectangles = gcnew List<Tuple<int, int, int, int, float>^>();
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(0, 0, 60, 60, 0.0));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(40, 0, 60, 60, 0.0));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(0, 40, 60, 60, 0.0));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(40, 40, 60, 60, 0.0));
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
        auto rectangles = gcnew List<Tuple<int, int, int, int, float>^>();
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(0, 0, 60, 60, 0.0));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(40, 0, 60, 60, 0.0));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(0, 40, 60, 60, 0.0));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(200, 200, 30, 30, 0.0));
        float merge_threshold = 0.1f;
        auto merged_rects = merger->merge(rectangles, merge_threshold);
        _assertions.Assert(merged_rects->Count == 2);
        Tuple<int, int, int, int, float>^ merged_rect = nullptr;
        Tuple<int, int, int, int, float>^ outlier_rect = nullptr;
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
        auto rectangles = gcnew List<Tuple<int, int, int, int, float>^>();
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(0, 0, 60, 60, 1.0f));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(40, 0, 60, 60, 4.0f));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(0, 40, 60, 60, 3.0f));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(40, 40, 60, 60, 2.0f));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(200, 200, 50, 50, 5.0f));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(240, 200, 50, 50, 8.0f));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(200, 240, 50, 50, 6.0f));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(240, 240, 50, 50, 7.0f));
        float merge_threshold = 0.1f;
        auto merged_rects = merger->merge(rectangles, merge_threshold);
        _assertions.Assert(merged_rects->Count == 2);
        _assertions.Assert(merged_rects[0] != nullptr);
        _assertions.Assert(merged_rects[0]->Item1 == 200);
        _assertions.Assert(merged_rects[0]->Item2 == 200);
        _assertions.Assert(merged_rects[0]->Item3 == 90);
        _assertions.Assert(merged_rects[0]->Item4 == 90);
        _assertions.Assert(merged_rects[0]->Item5 == 8.0f);
        _assertions.Assert(merged_rects[1] != nullptr);
        _assertions.Assert(merged_rects[1]->Item1 == 0);
        _assertions.Assert(merged_rects[1]->Item2 == 0);
        _assertions.Assert(merged_rects[1]->Item3 == 100);
        _assertions.Assert(merged_rects[1]->Item4 == 100);
        _assertions.Assert(merged_rects[1]->Item5 == 4.0f);
    }

    void RectangleMergerTest::test_merge_fails_when_threshold_not_met(void) {
        auto merger = gcnew RectangleMerger();
        auto rectangles = gcnew List<Tuple<int, int, int, int, float>^>();
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(0, 0, 100, 100, 0.0));
        rectangles->Add(gcnew Tuple<int, int, int, int, float>(95, 95, 100, 100, 0.0));
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

    List<Bitmap^>^ BitmapTemplateMatcherTest::_template_fixture() {
        auto fixture = gcnew List<Bitmap^>();
        fixture->Add(
            dynamic_cast<Bitmap^>(
                Bitmap::FromFile("TemplateMatcherTestTemplate1.png")
                )
        );
        fixture->Add(
            dynamic_cast<Bitmap^>(
                Bitmap::FromFile("TemplateMatcherTestTemplate2.png")
                )
        );
        fixture->Add(
            dynamic_cast<Bitmap^>(
                Bitmap::FromFile("TemplateMatcherTestTemplate3.png")
                )
        );
        return fixture;
    }

    AbstractBitmapTemplateMatcher^ BitmapTemplateMatcherTest::_matcher_fixture(void) {
        auto templates = _template_fixture();
        auto builder = gcnew BitmapTemplateMatcherBuilder();
        return builder->with_templates(templates)->build();
    }

    bool BitmapTemplateMatcherTest::_point_in_matches(
        List<Tuple<int, int, int, int, float>^>^ matches, int x, int y
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
        List<Tuple<int, int, int, int, float>^>^ matches, int area
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
        List<Tuple<int, int, int, int, float>^>^ matches, int area
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