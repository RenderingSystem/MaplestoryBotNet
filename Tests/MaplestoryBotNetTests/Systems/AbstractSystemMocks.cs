using MaplestoryBotNet.Systems;


namespace MaplestoryBotNetTests.Systems
{
    internal class MockSystem : AbstractSystem
    {
        public List<string> CallOrder = [];

        public int InitializeSystemCalls = 0;
        public override void InitializeSystem()
        {
            var callReference = new TestUtils().Reference(this) + "InitializeSystem";
            CallOrder.Add(callReference);
            InitializeSystemCalls++;
        }

        public int StartSystemCalls = 0;
        public override void StartSystem()
        {
            var callReference = new TestUtils().Reference(this) + "StartSystem";
            CallOrder.Add(callReference);
            StartSystemCalls++;
        }

        public int InjectCalls = 0;
        public List<object> InjectCallArg_data = [];
        public override void Inject(List<object> data)
        {
            var callReference = new TestUtils().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_data.Add(data);
        }
    }
}
