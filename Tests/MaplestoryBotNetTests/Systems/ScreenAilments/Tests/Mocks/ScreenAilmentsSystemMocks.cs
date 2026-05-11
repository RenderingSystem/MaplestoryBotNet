using ArrayFireNCC;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.ScreenAilmentsProcessing;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.TestHelpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;


namespace MaplestoryBotNetTests.Systems.ScreenAilments.Tests.Mocks
{
    public class MockScreenAilmentDetectionHelper : AbstractScreenAilmentDetectionHelper
    {
        public List<string> CallOrder = [];

        public int AilmentDetectedCalls = 0;
        public int AilmentDetectedIndex = 0;
        public List<Image<Bgra32>> AilmentDetectedCallArg_image = [];
        public List<float> AilmentDetectedCallArg_threshold = [];
        public List<List<Tuple<int, int, int, int, float>>> AilmentDetectedReturn = [];
        public override List<Tuple<int, int, int, int, float>> AilmentDetected(
            Image<Bgra32> image, float threshold
        )
        {
            var callReference = new TestUtilities().Reference(this) + "AilmentDetected";
            CallOrder.Add(callReference);
            AilmentDetectedCalls++;
            AilmentDetectedCallArg_image.Add(image);
            AilmentDetectedCallArg_threshold.Add(threshold);
            if (AilmentDetectedIndex < AilmentDetectedReturn.Count)
            {
                return AilmentDetectedReturn[AilmentDetectedIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int ShouldCheckCalls = 0;
        public int ShouldCheckIndex = 0;
        public List<float> ShouldCheckCallArg_checkDelay = [];
        public List<bool> ShouldCheckReturn = [];
        public override bool ShouldCheck(float checkDelay)
        {
            var callReference = new TestUtilities().Reference(this) + "ShouldCheck";
            CallOrder.Add(callReference);
            ShouldCheckCalls++;
            ShouldCheckCallArg_checkDelay.Add(checkDelay);
            if (ShouldCheckIndex < ShouldCheckReturn.Count)
            {
                return ShouldCheckReturn[ShouldCheckIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockTemplateMatcherBuilder : AbstractTemplateMatcherBuilder
    {
        public List<string> CallOrder = [];

        public int BuildCalls = 0;
        public int BuildIndex = 0;
        public List<AbstractBitmapTemplateMatcher> BuildReturn = [];
        public override AbstractBitmapTemplateMatcher Build()
        {
            var callReference = new TestUtilities().Reference(this) + "Build";
            CallOrder.Add(callReference);
            BuildCalls++;
            if (BuildIndex < BuildReturn.Count)
            {
                return BuildReturn[BuildIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int WithArgCalls = 0;
        public int WithArgIndex = 0;
        public List<object> WithArgCallArg_arg = [];
        public override AbstractTemplateMatcherBuilder WithArg(object arg)
        {
            var callReference = new TestUtilities().Reference(this) + "WithArg";
            CallOrder.Add(callReference);
            WithArgCalls++;
            WithArgCallArg_arg.Add(arg);
            return this;
        }
    }


    public class MockScreenAilmentDetectionThreadStarter : AbstractScreenAilmentDetectionThreadStarter
    {
        public List<string> CallOrder = [];

        public int StartAttemptCalls = 0;
        public List<ConcurrentDictionary<string, AbstractThread>?> StartAttemptCallArg_ailmentThreads = [];
        public List<AbstractInjectAction?> StartAttemptCallArg_injectAction = [];
        public override void StartAttempt(
            ConcurrentDictionary<string, AbstractThread>? ailmentThreads,
            AbstractInjectAction? injectAction
        )
        {
            var callReference = new TestUtilities().Reference(this) + "StartAttempt";
            CallOrder.Add(callReference);
            StartAttemptCalls++;
            StartAttemptCallArg_ailmentThreads.Add(ailmentThreads);
            StartAttemptCallArg_injectAction.Add(injectAction);
        }
    }
}
