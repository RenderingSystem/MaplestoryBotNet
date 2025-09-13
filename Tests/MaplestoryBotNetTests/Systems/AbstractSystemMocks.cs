using MaplestoryBotNet.Systems;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems
{
    public class MockSystem : AbstractSystem
    {
        public List<string> CallOrder = [];

        public int InitializeSystemCalls = 0;
        public override void InitializeSystem()
        {
            var callReference = new TestUtilities().Reference(this) + "InitializeSystem";
            CallOrder.Add(callReference);
            InitializeSystemCalls++;
        }

        public int StartSystemCalls = 0;
        public override void StartSystem()
        {
            var callReference = new TestUtilities().Reference(this) + "StartSystem";
            CallOrder.Add(callReference);
            StartSystemCalls++;
        }

        public int InjectCalls = 0;
        public List<SystemInjectType> InjectCallArg_dataType = [];
        public List<object> InjectCallArg_data = [];
        public override void Inject(SystemInjectType dataType, object data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }
    }


    public class MockInjector : AbstractInjector
    {
        public List<string> CallOrder = [];

        public int InjectCalls = 0;
        public List<SystemInjectType> InjectCallArg_dataType = [];
        public List<object> InjectCallArg_data = [];
        public override void Inject(SystemInjectType dataType, object data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }
    }
}
