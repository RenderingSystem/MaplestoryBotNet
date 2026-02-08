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

        virtual List<Tuple<int, int, int, int>^>^ calculate(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride,
            float threshold
        ) abstract;

        virtual List<Tuple<int, int, int, int>^>^ calculate(
            Bitmap^ bitmap,
            float threshold
        ) abstract;

    };


    public ref class AbstractBitmapTemplateMatcherBuilder abstract {

    public:

        virtual AbstractBitmapTemplateMatcherBuilder^ with_template(
            Bitmap^ image
        ) abstract;

        virtual AbstractBitmapTemplateMatcher^ build(void) abstract;

    };


    public ref class AbstractArrayFireSaver abstract {

    public:

        virtual void save(
            const af::array& arr,
            const std::string& filename
        ) abstract;

    };


    public ref class AbstractBitmapToGrayscaleConverter abstract {

    public:

        virtual af::array convert(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride,
            af::array& image_mask
        ) abstract;

        virtual af::array convert(
            Bitmap^ bitmap,
            af::array& image_mask
        ) abstract;
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

        virtual List<Tuple<int, int>^>^ detect(
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

        virtual List<Tuple<int, int, int, int>^>^ merge(
            List<Tuple<int, int, int, int>^>^ rectangles,
            float merge_threshold
        ) abstract;

    };


    public ref class ArrayFireSaver : public AbstractArrayFireSaver {

    public:

        virtual void save(
            const af::array& arr,
            const std::string& filename
        ) override;

    };


    public ref class BitmapToZeroMeanGrayscaleConverter : public AbstractBitmapToGrayscaleConverter {

    public:

        virtual af::array convert(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride,
            af::array& image_mask
        ) override;

        virtual af::array convert(
            Bitmap^ bitmap,
            af::array& image_mask
        ) override;
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

        virtual List<Tuple<int, int>^>^ detect(af::array& ncc_map, float threshold) override;

    };


    public ref class NormalizedCrossCorrelation : public AbstractNormalizedCrossCorrelation {

    private:

        AbstractArrayFireShifter^ _shifter;

    private:

        af::array _compute_cross_correlation(
            const af::array& image,
            const af::array& templ
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

        Tuple<int, int, int, int>^ _compute_merge_area(
            Tuple<int, int, int, int>^ rect_1,
            Tuple<int, int, int, int>^ rect_2,
            float merge_threshold
        );

    public:

        virtual List<Tuple<int, int, int, int>^>^ merge(
            List<Tuple<int, int, int, int>^>^ rectangles,
            float merge_threshold
        ) override;

    };


    public ref class BitmapTemplateMatcher : public AbstractBitmapTemplateMatcher {

    private:

        Bitmap^ _templates;

        AbstractNormalizedCrossCorrelation^ _matcher;

        AbstractBitmapToGrayscaleConverter^ _grayscale;

        AbstractBitmapToMaskConverter^ _mask;

        AbstractLocationDetector^ _detector;

    private:

        array<af::array*>^ _calculate_ncc_maps(
            af::array& af_image,
            af::array& af_image_mask
        );

        List<Tuple<int, int, int, int>^>^ _calculate_matches(
            array<af::array*>^ ncc_maps,
            float threshold
        );

        void _delete_ncc_maps(
            array<af::array*>^ ncc_maps
        );

    public:

        BitmapTemplateMatcher(
            Bitmap^ templates,
            AbstractNormalizedCrossCorrelation^ matcher,
            AbstractBitmapToGrayscaleConverter^ grayscale,
            AbstractBitmapToMaskConverter^ mask,
            AbstractLocationDetector^ detector
        );

        ~BitmapTemplateMatcher(void);

        virtual List<Tuple<int, int, int, int>^>^ calculate(
            UINT32* image,
            int image_width,
            int image_height,
            int image_stride,
            float threshold
        ) override;

        virtual List<Tuple<int, int, int, int>^>^ calculate(
            Bitmap^ bitmap,
            float threshold
        ) override;

    };


    public ref class BitmapTemplateMatcherBuilder : public AbstractBitmapTemplateMatcherBuilder {

    private:

        Bitmap^ _image;

    private:

        Bitmap^ _deep_clone_gif(void);

    public:

        BitmapTemplateMatcherBuilder();

        virtual AbstractBitmapTemplateMatcherBuilder^ with_template(
            Bitmap^ image
        ) override;

        virtual AbstractBitmapTemplateMatcher^ build(void) override;

    };

}
