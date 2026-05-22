#pragma once
#include "TemplateMatcher.h"
#include "Assertions.h"
#include "AcceleratedDeviceSelectionSystem.h"
#include "af/device.h"


namespace ArrayFireNCCTests {
    using namespace System;
    using namespace System::Collections::Generic;
    using namespace System::Drawing;
    using namespace ArrayFireNCC;


    public ref class BitmapToZeroMeanGrayscaleConverterTest {

    private:

        UnitTestAssertions _assertions;


    private:

        void _bitmap_fixture(UINT32* test_bitmap);

        float _masked_sum(float* bitmap, float* mask, int count);

    public:

        void run(void);

        /**
         * @brief Tests bitmap conversion to zero-mean grayscale without masking.
         *
         * Verifies that the converter correctly transforms a color bitmap into a zero-mean grayscale
         * representation.
         */
        void test_convert(void);


    };


    public ref class BitmapToMaskConverterTest {

    private:

        UnitTestAssertions _assertions;

        void _create_test_bitmap(
            UINT32* test_bitmap,
            int width,
            int height,
            const std::vector<int>& alpha_values
        );

    public:

        void run(void);

        /**
         * @brief Tests left-side opaque pixel detection
         *
         * Verifies the mask correctly identifies opaque pixels on the left side
         * of the test image.
         */
        void test_mask_2px_left(void);


        /**
         * @brief Tests top-edge opaque pixel detection
         *
         * Validates proper mask generation for opaque pixels along the top
         * of the test image.
         */
        void test_mask_2px_top(void);

        /**
         * @brief Tests right-side opaque pixel detection
         *
         * Confirms correct mask generation for opaque pixels on the right side
         * of the test image.
         */
        void test_mask_2px_right(void);

        /**
         * @brief Tests bottom-edge opaque pixel detection
         *
         * Ensures proper mask generation for opaque pixels along the bottom
         * of the test image.
         */
        void test_mask_2px_bottom(void);

        /**
         * @brief Tests border-only opaque pixel detection
         *
         * Verifies correct mask generation when only the outer border pixels
         * are opaque in the test image.
         */
        void test_mask_2px_all(void);

    };


    public ref class ArrayFireCenterShifterTest {

    private:

        UnitTestAssertions _assertions;

    public:

        void run(void);

        /**
         * @brief Tests vertical (top and bottom) padding
         *
         * Verifies that:
         * - Original image content remains centered vertically
         * - Correct amount of zeros are added to top and bottom
         * - Input values are preserved exactly in output
         */
        void test_top_and_bottom_padding(void);

        /**
         * @brief Tests horizontal (left and right) padding
         *
         * Verifies that:
         * - Original image content remains centered horizontally
         * - Correct amount of zeros are added to left and right
         * - Input values are preserved exactly in output
         */
        void test_left_and_right_padding(void);

        /**
         * @brief Tests combined padding on all sides
         *
         * Verifies that:
         * - Original image content remains centered both vertically and horizontally
         * - Correct amount of zeros are added to all sides
         * - Handles non-square padding cases correctly
         */
        void test_all_padding(void);

        /**
         * @brief Tests target size calculation
         *
         * Verifies that:
         * - Size calculation follows the expected mathematical formula
         * - Handles non-square input dimensions correctly
         * - Returns proper 4D dimensions
         */
        void test_target_size(void);

        /**
         * @brief Tests the circular shift and cropping functionality
         *
         * Verifies that:
         * - The input product data is properly circularly shifted to center the correlation peak
         * - The result is correctly cropped to match the dimensions of the input image
         * - The output maintains the correct spatial relationship of values after shifting
         * - Zero padding is applied appropriately when needed
         */
        void test_shift(void);

    };


    public ref class LocationDetectorTest {

    private:

        UnitTestAssertions _assertions;

    private:

        bool _in_matches(
            int x,
            int y,
            float confidence,
            List<Tuple<int, int, float>^>^ matches
        );

    public:

        void run(void);

        
        /**
         * @brief Tests the detection of high-correlation locations in an NCC
         * (Normalized Cross-Correlation) result map
         *
         * Verifies that:
         * - Locations with NCC values above the threshold are correctly identified
         * - The detector returns all valid matches (no false negatives)
         * - The returned coordinates correspond to peak correlation positions
         */
        void test_detect(void);

    }; 


    public ref class NormalizedCrossCorrelationTest {

    private:

        UnitTestAssertions _assertions;

    public:

        void run(void);

        /**
         * @brief Verifies that NCC detects an exact template match within an image.
         *
         * @details
         * - A 5×5 test image contains a 3×3 template (a distinct bright region).
         * - No masking is applied (all pixels are considered).
         * - Expects a confidence score of **1.0** at the template's true location.
         * - Surrounding regions should have near-zero scores.
         *
         * @note This test ensures basic NCC functionality works without masks.
         */
        void test_perfect_match(void);

        /**
         * @brief Validates NCC with a partially masked template (ignoring irrelevant pixels).
         *
         * @details
         * - A 5×6 image contains a 3×3 template, but only pixels marked `1` in the mask are compared.
         * - The mask ignores alternating pixels (checkerboard pattern).
         * - Despite noisy/unmatched template values, NCC must still return **1.0** at the correct position.
         */
        void test_masked_perfect_match(void);

        /**
         * @brief Tests NCC with a non-square (4×3) template and selective masking.
         *
         * @details
         * - Uses a 5×6 image and a rectangular 4×3 template.
         * - Mask excludes alternating pixels, simulating real-world partial matches.
         * - Confirms NCC handles even-sized templates correctly.
         */
        void test_masked_perfect_even_match(void);
    };


    public ref class RectangleMergerTest {

    private:

        UnitTestAssertions _assertions;

    public:

        void run(void);

        /**
         * @brief Tests complete merging of overlapping detection rectangles
         *
         * Verifies that when multiple detection rectangles significantly overlap
         * (representing parts of the same sprite), they are correctly merged into
         * a single bounding box encompassing the entire detected area.
         */
        void test_merge_all_rectangles_in_list(void);

        /**
         * @brief Tests handling of unrelated detections
         *
         * Validates that when presented with a group of overlapping rectangles
         * (representing one sprite) and an isolated rectangle (representing either
         * noise or a separate sprite), the system:
         * 
         * 1. Merges the overlapping group into one bounding box
         * 2. Preserves the isolated detection unchanged
         */
        void test_merge_with_outlier_rectangle(void);

        /**
         * @brief Tests multiple sprite detection scenarios
         *
         * Ensures the system can handle multiple distinct groups of overlapping
         * rectangles (representing separate sprites) by:
         * 
         * 1. Merging rectangles within each group appropriately
         * 2. Maintaining separation between non-overlapping groups
         * 3. Producing correct bounding boxes for each sprite
         */
        void test_merge_with_two_rectangle_groups(void);

        /**
         * @brief Tests threshold sensitivity for merging
         *
         * Verifies that when rectangles overlap but don't meet the minimum
         * threshold requirement, the system:
         * 
         * 1. Keeps the rectangles separate
         * 2. Preserves their original dimensions and positions
         * 3. Does not produce false merges
         */
        void test_merge_fails_when_threshold_not_met(void);

    };


    public ref class BitmapTemplateMatcherTest {

    private:

        UnitTestAssertions _assertions;

        Bitmap^ _image_fixture(void);

        List<Bitmap^>^ _template_fixture(void);

        AbstractBitmapTemplateMatcher^ _matcher_fixture(void);

        AbstractBitmapTemplateMatcher^ _rgb_matcher_fixture(void);

        bool _point_in_matches(
            List<Tuple<int, int, int, int, float>^>^ matches, int x, int y
        );

        bool _matches_lt_threshold_area(
            List<Tuple<int, int, int, int, float>^>^ matches, int area
        );

        bool _matches_gt_threshold_area(
            List<Tuple<int, int, int, int, float>^>^ matches, int area
        );

    public:

        void run(void);

        /**
         * @brief Verifies that the template matcher correctly detects and locates template
         * instances in an image.
         *
         * This test ensures that:
         * 
         * - The matcher identifies all expected occurrences of the template in the test image.
         * - Matches meet the minimum similarity threshold (0.6 in this test).
         * - Overlapping or adjacent matches are merged into coherent regions.
         * - The output matches cover specific known locations of the template in the image.
         */
        void test_calculate(void);

    };

    public ref class TestTemplateMatcher {

    public:

        void run(void);

    };
}
