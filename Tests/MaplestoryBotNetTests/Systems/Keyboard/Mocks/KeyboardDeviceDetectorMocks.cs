using MaplestoryBotNet.Systems.Keyboard;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Keyboard.Mocks
{
    internal class MockKeyboardDeviceDetector : AbstractKeyboardDeviceDetector
    {
        public List<string> CallOrder = [];

        public int DetectCalls = 0;
        public int DetectIndex = 0;
        public List<KeyboardDeviceContext> DetectReturn = [];
        public override KeyboardDeviceContext Detect()
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
