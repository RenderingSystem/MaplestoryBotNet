using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNetTests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks
{
    public class MockPotionsMenuState : AbstractPotionsMenuState
    {
        public List<string> CallOrder = [];

        public int GetEditingStateCalls = 0;
        public int GetEditingStateIndex = 0;
        public List<int> GetEditingStateReturn = [];
        public override int GetEditingState()
        {
            CallOrder.Add(new TestUtilities().Reference(this) + "GetEditingState");
            GetEditingStateCalls++;
            if (GetEditingStateIndex < GetEditingStateReturn.Count)
            {
                return GetEditingStateReturn[GetEditingStateIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int SetEditingStateCalls = 0;
        public List<int> SetEditingStateCallArg_state = [];

        public override void SetEditingState(int state)
        {
            CallOrder.Add(new TestUtilities().Reference(this) + "SetEditingState");
            SetEditingStateCallArg_state.Add(state);
            GetEditingStateCalls++;
        }
    }
}
