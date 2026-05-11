using ArrayFireNCC;
using MaplestoryBotNet.Systems.ScreenProcessing.SubSystems;
using MaplestoryBotNetTests.TestHelpers;
using System.Drawing;


namespace MaplestoryBotNetTests.Systems.ScreenProcessing.Tests.Mocks
{
    public class MockGameMinimapProcessHandler : AbstractGameMinimapProcessHandler
    {
        public List<string> CallOrder = [];

        public int HandleCalls = 0;
        public List<GameMinimapProcessorThreadState> HandleCallArg_threadState = [];
        public override void Handle(GameMinimapProcessorThreadState threadState)
        {
            var callReference = new TestUtilities().Reference(this) + "Handle";
            CallOrder.Add(callReference);
            HandleCalls++;
            HandleCallArg_threadState.Add(threadState);
        }
    }


    public class MockGameMinimapProcessorThreadStateUpdater : 
        AbstractGameMinimapProcessorThreadStateUpdater<
            GameMinimapProcessorThreadState
        >
    {
        public List<string> CallOrder = [];

        public int AtomicUpdateCalls = 0;
        public List<GameMinimapProcessorThreadState?> AtomicUpdateCallArg_atomicField = [];
        public List<GameMinimapProcessorThreadState?> AtomicUpdateCallArg_updateObject = [];
        public override void AtomicUpdate(
            ref GameMinimapProcessorThreadState? atomicField,
            GameMinimapProcessorThreadState? updateObject
        )
        {
            var callReference = new TestUtilities().Reference(this) + "AtomicUpdate";
            CallOrder.Add(callReference);
            AtomicUpdateCalls++;
            AtomicUpdateCallArg_atomicField.Add(atomicField);
            AtomicUpdateCallArg_updateObject.Add(updateObject);
        }
    }


    public class MockBitmapTemplateMatcher : AbstractBitmapTemplateMatcher
    {
        public List<string> CallOrder = [];


        public int calculateBitmapCalls = 0;
        public int calculateBitmapIndex = 0;
        public List<Bitmap> calculateBitmapCallArg_bitmap = [];
        public List<float> calculateBitmapCallArg_threshold = [];
        public List<List<Tuple<int, int, int, int, float>>> calculateBitmapReturn = [];
        public override List<Tuple<int, int, int, int, float>> calculate(
            Bitmap bitmap, float threshold
        )
        {
            var callReference = new TestUtilities().Reference(this) + "calculateBitmap";
            CallOrder.Add(callReference);
            calculateBitmapCalls++;
            calculateBitmapCallArg_bitmap.Add(bitmap);
            calculateBitmapCallArg_threshold.Add(threshold);
            if (calculateBitmapIndex < calculateBitmapReturn.Count)
            {
                return calculateBitmapReturn[calculateBitmapIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int calculatePointerCalls = 0;
        public int calculatePointerIndex = 0;
        public List<UIntPtr> calculatePointerCallArg_image = [];
        public List<int> calculatePointerCallArg_image_width = [];
        public List<int> calculatePointerCallArg_image_height = [];
        public List<int> calculatePointerCallArg_image_stride = [];
        public List<float> calculatePointerCallArg_threshold = [];
        public List<List<Tuple<int, int, int, int, float>>> calculatePointerReturn = [];
        public override unsafe List<Tuple<int, int, int, int, float>> calculate(
            uint* image,
            int image_width,
            int image_height,
            int image_stride,
            float threshold
        )
        {
            var callReference = new TestUtilities().Reference(this) + "calculatePointer";
            CallOrder.Add(callReference);
            calculatePointerCalls++;
            calculatePointerCallArg_image.Add(new UIntPtr(image));
            calculatePointerCallArg_image_width.Add(image_width);
            calculatePointerCallArg_image_height.Add(image_height);
            calculatePointerCallArg_image_stride.Add(image_stride);
            calculatePointerCallArg_threshold.Add(threshold);
            if (calculatePointerIndex < calculatePointerReturn.Count)
            {
                return calculatePointerReturn[calculatePointerIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int get_templateCalls = 0;
        public int get_templateIndex = 0;
        public List<List<Bitmap>> get_templateReturn = [];
        public override List<Bitmap> get_templates()
        {
            var callReference = new TestUtilities().Reference(this) + "get_templates";
            CallOrder.Add(callReference);
            get_templateCalls++;
            if (get_templateIndex < get_templateReturn.Count)
            {
                return get_templateReturn[get_templateIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockGameMinimapPositionProcessor : AbstractScreenPositionProcessor
    {
        public List<string> CallOrder = [];

        public int ProcessCalls = 0;
        public int ProcessIndex = 0;
        public List<AbstractBitmapTemplateMatcher> ProcessCallArg_templateMatcher = [];
        public List<AbstractRectangleMerger> ProcessCallArg_merger = [];
        public List<float> ProcessCallArg_threshold = [];
        public List<float> ProcessCallArg_overlap = [];
        public List<Bitmap> ProcessCallArg_inputSource = [];
        public List<Tuple<int, int>> ProcessReturn = [];
        public override Tuple<int, int>? Process(
            AbstractBitmapTemplateMatcher templateMatcher,
            AbstractRectangleMerger merger,
            float threshold,
            float overlap,
            Bitmap inputSource
        )
        {
            var callReference = new TestUtilities().Reference(this) + "Process";
            CallOrder.Add(callReference);
            ProcessCalls++;
            ProcessCallArg_templateMatcher.Add(templateMatcher);
            ProcessCallArg_merger.Add(merger);
            ProcessCallArg_threshold.Add(threshold);
            ProcessCallArg_overlap.Add(overlap);
            ProcessCallArg_inputSource.Add(inputSource);
            if (ProcessIndex < ProcessReturn.Count)
            {
                return ProcessReturn[ProcessIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }
}
