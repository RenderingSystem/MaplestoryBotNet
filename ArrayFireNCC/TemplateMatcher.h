#pragma once
#include "ArrayFireWrapper.h"
#using <System.Drawing.dll>


namespace ArrayFireNCC {

    using namespace System;
    using namespace System::Collections::Generic;
    using namespace System::Drawing;
    using namespace System::Drawing::Imaging;


    public ref class AbstractNormalizedCrossCorrelation abstract {

    public:

        virtual af::array calculate(
            const af::array& image,
            const af::array& image_mask,
            const af::array& templ,
            const af::array& templ_mask
        ) abstract;

    };


    public ref class AbstractBitmapTemplateMatcher abstract {

    public:

        virtual List<Tuple<int, int, int, int, float>^>^ calculate(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride,
            float threshold
        ) abstract;

        virtual List<Tuple<int, int, int, int, float>^>^ calculate(
            Bitmap^ bitmap,
            float threshold
        ) abstract;

        virtual List<Bitmap^>^ get_templates() abstract;

    };


    public ref class AbstractBitmapTemplateMatcherBuilder abstract {

    public:

        virtual AbstractBitmapTemplateMatcherBuilder^ with_templates(
            List<Bitmap^>^ templates
        ) abstract;

        virtual AbstractBitmapTemplateMatcher^ build(void) abstract;

    };


    public ref class AbstractArrayFireSaver abstract {

    public:

        virtual void save(
            const af::array& arr,
            const std::string& filename,
            bool normalize
        ) abstract;

    };


    public ref class AbstractBitmapToGrayscaleConverter abstract {

    public:

        virtual std::vector<af::array> convert(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride
        ) abstract;

        virtual std::vector<af::array> convert(Bitmap^ bitmap) abstract;
    };


    public ref class AbstractBitmapToMaskConverter abstract {

    public:

        virtual af::array convert(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride
        ) abstract;

        virtual af::array convert(
            Bitmap^ bitmap
        ) abstract;

    };


    public ref class AbstractArrayFireShifter abstract {

    public:

        virtual af::dim4 target_size(
            const af::array& image,
            const af::array& templ
        ) abstract;

        virtual af::array pad(
            af::array image,
            af::dim4 target_dims
        ) abstract;

        virtual af::array shift(
            const af::array& product,
            af::dim4 target_dims,
            af::dim4 image_dims,
            af::dim4 templ_dims
        ) abstract;

    };


    public ref class AbstractLocationDetector abstract {

    public:

        virtual List<Tuple<int, int, float>^>^ detect(
            af::array& ncc_map,
            float threshold
        ) abstract;

    };


    public ref class AbstractBitmapTemplateMatcherFactory abstract {

    public:

        virtual AbstractBitmapTemplateMatcher^ create(void) abstract;

    };


    public ref class AbstractRectangleMerger abstract {

    public:

        virtual List<Tuple<int, int, int, int, float>^>^ merge(
            List<Tuple<int, int, int, int, float>^>^ rectangles,
            float merge_threshold
        ) abstract;

    };


    public ref class ArrayFireSaver : public AbstractArrayFireSaver {

    public:

        virtual void save(
            const af::array& arr,
            const std::string& filename,
            bool normalize
        ) override;

    };


    public ref class BitmapToGrayscaleConverter : public AbstractBitmapToGrayscaleConverter {

    public:

        virtual std::vector<af::array> convert(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride
        ) override;

        virtual std::vector<af::array> convert(Bitmap^ bitmap) override;
    };


    public ref class BitmapToMaskConverter : public AbstractBitmapToMaskConverter {

    public:

        virtual af::array convert(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride
        ) override;

        virtual af::array convert(
            Bitmap^ bitmap
        ) override;

    };


    public ref class ArrayFireCenterShifter : public AbstractArrayFireShifter {

    public:

        virtual af::dim4 target_size(
            const af::array& image,
            const af::array& templ
        ) override;

        virtual af::array pad(
            af::array image,
            af::dim4 target_dims
        ) override;

        virtual af::array shift(
            const af::array& product,
            af::dim4 target_dims,
            af::dim4 image_dims,
            af::dim4 templ_dims
        ) override;

    };


    public ref class LocationDetector : public AbstractLocationDetector {

    public:

        virtual List<Tuple<int, int, float>^>^ detect(af::array& ncc_map, float threshold) override;

    };


    public ref class NormalizedCrossCorrelation : public AbstractNormalizedCrossCorrelation {

    private:

        AbstractArrayFireShifter^ _shifter;

    private:

        af::array _compute_cross_correlation(
            const af::array& image,
            const af::array& templ
        );

        af::array NormalizedCrossCorrelation::_compute_image_sq_dev(
            const af::array& masked_image,
            const af::array& templ_mask,
            const af::array& valid_count
        );

    public:

        NormalizedCrossCorrelation(
            AbstractArrayFireShifter^ shifter
        );

        virtual af::array calculate(
            const af::array& image,
            const af::array& image_mask,
            const af::array& templ,
            const af::array& templ_mask
        ) override;

    };


    public ref class NormalizedCrossCorrelationFacade : public AbstractNormalizedCrossCorrelation {

    private:

        AbstractNormalizedCrossCorrelation^ _ncc;

    public:

        NormalizedCrossCorrelationFacade(void);

        virtual af::array calculate(
            const af::array& image,
            const af::array& image_mask,
            const af::array& templ,
            const af::array& templ_mask
        ) override;
    };


    public ref class RectangleMerger : public AbstractRectangleMerger {


    private:

        Tuple<int, int, int, int, float>^ _compute_merge_area(
            Tuple<int, int, int, int, float>^ rect_1,
            Tuple<int, int, int, int, float>^ rect_2,
            float merge_threshold
        );

        static int _compareByConfidenceDescending(
            Tuple<int, int, int, int, float>^ a,
            Tuple<int, int, int, int, float>^ b
        );

    public:

        virtual List<Tuple<int, int, int, int, float>^>^ merge(
            List<Tuple<int, int, int, int, float>^>^ rectangles,
            float merge_threshold
        ) override;

    };


    public ref class BitmapTemplateMatcher : public AbstractBitmapTemplateMatcher {

    private:

        List<Bitmap^>^ _templates;

        AbstractNormalizedCrossCorrelation^ _matcher;

        AbstractBitmapToGrayscaleConverter^ _grayscale;

        AbstractBitmapToMaskConverter^ _mask;

        AbstractLocationDetector^ _detector;

    private:

        std::vector<af::array> BitmapTemplateMatcher::_calculate_ncc_maps(
            std::vector<af::array> af_image_channels,
            af::array& af_image_mask,
            std::vector<af::array> af_templ_channels,
            af::array& af_templ_mask
        );

        List<Tuple<int, int, int, int, float>^>^ _calculate_ncc_matches(
            std::vector<af::array> ncc_maps,
            int templ_width,
            int templ_height,
            float threshold
        );

    public:

        BitmapTemplateMatcher(
            List<Bitmap^>^ templates,
            AbstractNormalizedCrossCorrelation^ matcher,
            AbstractBitmapToGrayscaleConverter^ grayscale,
            AbstractBitmapToMaskConverter^ mask,
            AbstractLocationDetector^ detector
        );

        ~BitmapTemplateMatcher(void);

        virtual List<Tuple<int, int, int, int, float>^>^ calculate(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride,
            float threshold
        ) override;

        virtual List<Tuple<int, int, int, int, float>^>^ calculate(
            Bitmap^ bitmap,
            float threshold
        ) override;

        virtual List<Bitmap^>^ get_templates() override;

    };


    public ref class BitmapTemplateMatcherBuilder : public AbstractBitmapTemplateMatcherBuilder {

    private:

        List<Bitmap^>^ _templates;

    private:

        List<Bitmap^>^ _deep_clone_templates(void);

    public:

        BitmapTemplateMatcherBuilder();

        virtual AbstractBitmapTemplateMatcherBuilder^ with_templates(
            List<Bitmap^>^ templates
        ) override;

        virtual AbstractBitmapTemplateMatcher^ build(void) override;

    };


    public ref class BitmapTemplateRGBMatcherBuilder : AbstractBitmapTemplateMatcherBuilder {

    private:

        List<Bitmap^>^ _templates;

    private:

        List<Bitmap^>^ _deep_clone_templates(void);

    public:

        BitmapTemplateRGBMatcherBuilder();

        virtual AbstractBitmapTemplateMatcherBuilder^ with_templates(
            List<Bitmap^>^ templates
        ) override;

        virtual AbstractBitmapTemplateMatcher^ build(void) override;

    };


    public ref class BitmapToRGBGrayscaleConverter : AbstractBitmapToGrayscaleConverter
    {
        public:

        virtual std::vector<af::array> convert(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride
        ) override;

        virtual std::vector<af::array> convert(Bitmap^ bitmap) override;
    };
}
