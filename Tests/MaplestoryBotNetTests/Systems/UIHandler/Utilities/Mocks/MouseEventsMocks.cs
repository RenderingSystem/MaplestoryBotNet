using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.TestHelpers;
using System.Windows;
using System.Windows.Input;


namespace MaplestoryBotNetTests.Systems.UIHandler.Utilities.Mocks
{
    public class MockMouseEventDataExtractor : AbstractMouseEventDataExtractor
    {
        public List<string> CallOrder = [];

        public int GetPositionCalls = 0;
        public int GetPositionIndex = 0;
        public List<MouseEventArgs> GetPositionCallArg_mouseButtonEvent = [];
        public List<IInputElement> GetPositionCallArg_relativeTo = [];
        public List<Point> GetPositionReturn = [];
        public override Point GetPosition(
            MouseEventArgs mouseButtonEvent,
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

        public int GetButtonStateCalls = 0;
        public int GetButtonStateIndex = 0;
        public List<MouseButtonState> GetButtonStateCallArg_buttonState = [];
        public List<MouseButtonState> GetButtonStateReturn = [];
        public override MouseButtonState GetButtonState(MouseButtonState buttonState)
        {
            var callReference = new TestUtilities().Reference(this) + "GetButtonState";
            CallOrder.Add(callReference);
            GetButtonStateCalls++;
            GetButtonStateCallArg_buttonState.Add(buttonState);
            if (GetButtonStateIndex < GetButtonStateReturn.Count)
            {
                return GetButtonStateReturn[GetButtonStateIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }
}
