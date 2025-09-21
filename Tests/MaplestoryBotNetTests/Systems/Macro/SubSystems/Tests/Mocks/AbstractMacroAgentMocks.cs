using MaplestoryBotNet.Systems.Macro.SubSystems;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Macro.SubSystems.Tests.Mocks
{
    public class MockMacroAgent : AbstractMacroAgent
    {
        public List<string> CallOrder = [];

        public int ExecuteCalls = 0;
        public override void Execute()
        {
            var callReference = new TestUtilities().Reference(this) + "Execute";
            CallOrder.Add(callReference);
            ExecuteCalls++;
        }

        public int UpdateCalls = 0;
        public List<object> UpdateCallArg_data = [];
        public override void Update(object data)
        {
            var callReference = new TestUtilities().Reference(this) + "Update";
            CallOrder.Add(callReference);
            UpdateCalls++;
            UpdateCallArg_data.Add(data);
        }
    }
}
