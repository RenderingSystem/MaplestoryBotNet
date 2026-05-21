using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Consumables;
using MaplestoryBotNetTests.TestHelpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace MaplestoryBotNetTests.Systems.Consumption.Mocks
{
    public class MockResourceDetectionThreshold : AbstractResourceDetectionThreshold
    {
        public List<string> CallOrder = [];

        public int ThresholdCalls = 0;
        public int ThresholdIndex = 0;
        public List<Resource> ThresholdCallArg_resource = [];
        public List<Image<Bgra32>> ThresholdCallArg_image = [];
        public List<int> ThresholdReturn = [];
        public override int Threshold(
            Resource resource, Image<Bgra32> image
        )
        {
            var callReference = new TestUtilities().Reference(this) + "Threshold";
            CallOrder.Add(callReference);
            ThresholdCalls++;
            ThresholdCallArg_resource.Add(resource);
            ThresholdCallArg_image.Add(image);
            if (ThresholdIndex < ThresholdReturn.Count)
            {
                return ThresholdReturn[ThresholdIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockConsumptionThreadRefresher : AbstractConsumptionThreadRefresher
    {
        public List<string> CallOrder = [];

        public int InjectCalls = 0;
        public List<object> InjectCallArg_dataType = [];
        public List<object?> InjectCallArg_data = [];
        public override void Inject(object dataType, object? data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }

        public int RefreshCalls = 0;
        public override void Refresh()
        {
            var callReference = new TestUtilities().Reference(this) + "Refresh";
            CallOrder.Add(callReference);
            RefreshCalls++;
        }
    }


    public class MockConsumptionQueueUpdater : AbstractConsumptionQueueUpdater
    {
        public List<string> CallOrder = [];

        public int InjectCalls = 0;
        public List<object> InjectCallArg_dataType = [];
        public List<object?> InjectCallArg_data = [];
        public override void Inject(object dataType, object? data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }

        public int UpdateCalls = 0;
        public override void Update()
        {
            var callReference = new TestUtilities().Reference(this) + "Update";
            CallOrder.Add(callReference);
            UpdateCalls++;
        }
    }


    public class MockConsumptionExecutor : AbstractConsumptionExecutor
    {
        public List<string> CallOrder = [];

        public int ExecuteCalls = 0;
        public int ExecuteIndex = 0;
        public List<bool> ExecuteReturn = [];
        public override bool Execute()
        {
            var callReference = new TestUtilities().Reference(this) + "Execute";
            CallOrder.Add(callReference);
            ExecuteCalls++;
            if (ExecuteIndex < ExecuteReturn.Count)
            {
                return ExecuteReturn[ExecuteIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int InjectCalls = 0;
        public List<object> InjectCallArg_dataType = [];
        public List<object?> InjectCallArg_data = [];
        public override void Inject(object dataType, object? data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }
    }


    public class MockConsumptionThreadHelper : AbstractConsumptionThreadHelper
    {
        public List<string> CallOrder = [];

        public int InjectCalls = 0;
        public List<object> InjectCallArg_dataType = [];
        public List<object?> InjectCallArg_data = [];
        public override void Inject(object dataType, object? data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }

        public int RunCalls = 0;
        public override void Run()
        {
            var callReference = new TestUtilities().Reference(this) + "Run";
            CallOrder.Add(callReference);
            RunCalls++;
        }
    }
}
