using MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks
{
    public class MockAilmentExecutorThreadHandler : AbstractAilmentExecutorThreadHandler
    {
        public List<string> CallOrder = [];

        public int HandleCalls = 0;
        public List<int> HandleCallArg_activeDelay = [];
        public override void Handle(int activeDelay)
        {
            var callReference = new TestUtilities().Reference(this) + "Handle";
            CallOrder.Add(callReference);
            HandleCalls++;
            HandleCallArg_activeDelay.Add(activeDelay);
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
}
