using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.TestHelpers;
using System.Windows;
using System.Windows.Input;


namespace MaplestoryBotNetTests.Systems.UIHandler.Utilities.Mocks
{
    public class MockMouseEventPositionExtractor : AbstractMouseEventPositionExtractor
    {
        public List<string> CallOrder = [];

        public int GetPositionCalls = 0;
        public int GetPositionIndex = 0;
        public List<MouseButtonEventArgs> GetPositionCallArg_mouseButtonEvent = [];
        public List<IInputElement> GetPositionCallArg_relativeTo = [];
        public List<Point> GetPositionReturn = [];
        public override Point GetPosition(
            MouseButtonEventArgs mouseButtonEvent,
            IInputElement relativeTo
        )
        {
            var callReference = new TestUtilities().Reference(this) + "GetPosition";
            CallOrder.Add(callReference);
            GetPositionCalls++;
            GetPositionCallArg_mouseButtonEvent.Add(mouseButtonEvent);
            GetPositionCallArg_relativeTo.Add(relativeTo);
            if (GetPositionIndex < GetPositionReturn.Count)
            {
                return GetPositionReturn[GetPositionIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }
}
