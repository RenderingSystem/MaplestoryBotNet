using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Macro;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Macro.Tests.Mocks
{
    public enum MockOrchestratorInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }

    public enum MockThreadedUpdateType
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        MaxNum
    }


    public class MockOrchestratorController : AbstractOrchestratorController
    {
        public List<string> CallOrder = [];

        public int GetStateCalls = 0;
        public int GetStateIndex = 0;
        public List<int> GetStateReturn = [];
        public override int? GetState()
        {
            var callReference = new TestUtilities().Reference(this) + "GetState";
            CallOrder.Add(callReference);
            GetStateCalls++;
            if (GetStateIndex < GetStateReturn.Count)
            {
                return GetStateReturn[GetStateIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int SetOrchestratorCalls = 0;
        public List<AbstractThread> SetOrchestratorCallArg_orchestrator = [];
        public override void SetOrchestrator(AbstractThread orchestrator)
        {
            var callReference = new TestUtilities().Reference(this) + "SetOrchestrator";
            CallOrder.Add(callReference);
            SetOrchestratorCalls++;
            SetOrchestratorCallArg_orchestrator.Add(orchestrator);
        }

        public int SetOrchestratorThreadStateCalls = 0;
        public List<AbstractKeystrokeTransmitterThreadState> SetOrchestratorThreadStateCallArg_threadState = [];
        public override void SetOrchestratorThreadState(AbstractKeystrokeTransmitterThreadState threadState)
        {
            var callReference = new TestUtilities().Reference(this) + "SetOrchestratorThreadState";
            CallOrder.Add(callReference);
            SetOrchestratorThreadStateCalls++;
            SetOrchestratorThreadStateCallArg_threadState.Add(threadState);
        }

        public int StartOrchestratorCalls = 0;
        public override void StartOrchestrator()
        {
            var callReference = new TestUtilities().Reference(this) + "StartOrchestrator";
            CallOrder.Add(callReference);
            StartOrchestratorCalls++;
        }

        public int StopOrchestratorCalls = 0;
        public override void StopOrchestrator()
        {
            var callReference = new TestUtilities().Reference(this) + "StopOrchestrator";
            CallOrder.Add(callReference);
            StopOrchestratorCalls++;
        }
    }


    public class MockExecutorState : AbstractExecutorState
    {
        public List<string> CallOrder = [];

        public int ExecuteCalls = 0;
        public int ExecuteIndex = 0;
        public List<int> ExecuteReturn = [];
        public override int Execute()
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
    }


    public class MockExecutorStateActivator : AbstractExecutorStateActivator
    {
        public List<string> CallOrder = [];

        public int ActivateCalls = 0;
        public List<MacroExecutorStateTypes> ActivateCallArg_stateType = [];
        public override void Activate(MacroExecutorStateTypes stateType)
        {
            var callReference = new TestUtilities().Reference(this) + "Activate";
            CallOrder.Add(callReference);
            ActivateCalls++;
            ActivateCallArg_stateType.Add(stateType);
        }
    }
}
