using MaplestoryBotNet.Systems.Device.SubSystems;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Device.Tests.SubSystems.Mocks
{
    internal class MockDeviceDetector : AbstractDeviceDetector
    {
        public List<string> CallOrder = [];

        public int DetectCalls = 0;
        public int DetectIndex = 0;
        public List<DeviceContext> DetectReturn = [];
        public override DeviceContext Detect()
        {
            var callReference = new TestUtilities().Reference(this) + "Detect";
            CallOrder.Add(callReference);
            DetectCalls++;
            if (DetectIndex < DetectReturn.Count)
                return DetectReturn[DetectIndex++];
            throw new IndexOutOfRangeException();
        }
    }
}
