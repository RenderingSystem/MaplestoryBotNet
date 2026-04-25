using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using System.Collections.Concurrent;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests
{
    public class SolvingOrchestratorThreadTests
    {
        private AbstractKeystrokeTransmitterThreadState _threadState = new KeystrokeTransmitterThreadState(
            0, KeystrokeTransmitterThreadType.Solving
        );

        private MockThread _thread = new MockThread(new ThreadRunningState());

        private MockRunningState _runningState = new MockRunningState();

        private BlockingCollection<int> _threadStates = new BlockingCollection<int>();

        private string _threadRef = "";

        private List<string> _callOrder = [];

        private AbstractThread _fixture(AbstractKeystrokeTransmitterThreadState threadState)
        {
            _threadState = threadState;
            _thread = new MockThread(new ThreadRunningState());
            _runningState = new MockRunningState();
            _callOrder = [];
            _thread.CallOrder = _callOrder;
            _threadStates = new BlockingCollection<int>();
            if (_threadState is MockKeystrokeTransmitterThreadState mockThreadState)
            {
                mockThreadState.CallOrder = _callOrder;
            }
            _threadRef = new TestUtilities().Reference(_thread);
            return new SolvingOrchestratorThread(
                _thread,
                _runningState,
                _threadStates
            );
        }

        private void _setTransmitFixture(int transmitCount)
        {
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(true);
            for (int j = 0; j < transmitCount; j++)
            {
                _runningState.IsRunningReturn.Add(true);
            }
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(false);
            for (int j = 0; j < transmitCount + 1; j++)
            {
                _threadStates.Add(j);
            }
        }

        /**
         * @brief Verifies that starting the orchestrator launches the executor thread
         * 
         * When users start their automation, the orchestrator should launch the
         * executor thread that actually runs the macros. This test ensures that
         * starting the orchestrator properly kicks off the executor.
         */
        private void _testStartingOrchestratorStartsExecutorThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.Solving
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(false);
            transmitterOrchestratorThread.Start();
            Debug.Assert(_thread.ThreadStartCalls == 1);
        }

        /**
         * @brief Verifies that stopping the orchestrator shuts down the executor thread
         * 
         * When users stop their automation, the orchestrator should cleanly shut
         * down the executor thread. This test ensures the shutdown sequence works
         * properly, including the handshake that confirms the thread has stopped.
         */
        private void _testStoppingOrchestratorStopsExecutorThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.Solving
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            _runningState.IsRunningReturn.Add(true);
            _thread.CallOrder = _callOrder;
            Debug.Assert(_threadStates.Count == 0);
            transmitterOrchestratorThread.Stop();
            Debug.Assert(_threadStates.Count == 1);
            Debug.Assert(_callOrder.Count == 1);
            Debug.Assert(_callOrder[0] == _threadRef + "ThreadStop");

        }

        /**
         * @brief Verifies that injected commands update the thread state
         * 
         * When the system sends commands to the orchestrator, the thread state
         * should update to reflect what it should be doing (starting, stopping,
         * running, etc.). This test ensures the orchestrator correctly tracks
         * its current operational state.
         */
        private void _testInjectingOrchestratorCommandAssignsThreadState()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                123, KeystrokeTransmitterThreadType.Solving
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            var max = SolvingOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                transmitterOrchestratorThread.Inject(
                    (SolvingOrchestratorThreadInjectType)i, 0
                );
                Debug.Assert(_threadStates.Count == 1);
                Debug.Assert(_threadStates.Take() == i);
            }
        }

        /**
         * @brief Confirms the orchestrator properly acknowledges commands
         * 
         * When commands are sent to the orchestrator, it should acknowledge them
         * by updating its state and signaling that the command was received.
         * This test ensures the orchestrator properly handles the command.
         */
        private void _testInjectOrchestratorCommand()
        {
            var max = SolvingOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var transmitterOrchestratorThread = _fixture(threadState);
                transmitterOrchestratorThread.Inject((SolvingOrchestratorThreadInjectType)i, 0);
                Debug.Assert(_threadStates.Count == 1);
                Debug.Assert(_threadStates.Take() == i);
            }
        }

        /**
         * @brief Verifies that data is forwarded to the executor thread
         * 
         * When the orchestrator receives data (like macro commands or configuration),
         * it should forward that data to the executor thread that will process it.
         * This test ensures the orchestrator correctly passes data along.
         */
        private void _testInjectToExecutorThread()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var transmitterOrchestratorThread = _fixture(threadState);
            transmitterOrchestratorThread.Inject(123, 456);
            Debug.Assert(_thread.InjectCalls == 1);
            Debug.Assert((int)_thread.InjectCallArg_dataType[0] == 123);
            Debug.Assert((int)_thread.InjectCallArg_data[0]! == 456);
        }

        /**
         * @brief Verifies that the orchestrator makes itself available as a thread dependency
         * 
         * When other systems in the application need to communicate with or control
         * the orchestrator thread, they need a reference to it. This test ensures
         * that when an InjectAction is received, the orchestrator properly registers
         * itself as a thread dependency that other components can discover and use.
         */
        private void _testInjectActionToExecutorThread()
        {
            var getActionDataType = new List<object>();
            var getActionData = new List<object>();
            var injectAction = new MockInjectAction();
            injectAction.GetActionReturn.Add(
                (object dataType, object data) =>
                {
                    getActionDataType.Add(dataType);
                    getActionData.Add(data);
                }
            );
            var threadState = new MockKeystrokeTransmitterThreadState();
            var transmitterOrchestratorThread = _fixture(threadState);
            transmitterOrchestratorThread.Inject(SystemInjectType.InjectAction, injectAction);
            Debug.Assert(_thread.InjectCalls == 1);
            Debug.Assert((int)_thread.InjectCallArg_dataType[0] == (int)SystemInjectType.InjectAction);
            Debug.Assert(_thread.InjectCallArg_data[0] == injectAction);
            Debug.Assert(injectAction.GetActionCalls == 1);
            Debug.Assert(getActionDataType.Count == 1);
            Debug.Assert((int)getActionDataType[0] == (int)SystemInjectType.ThreadDependency);
            Debug.Assert(getActionData[0] == transmitterOrchestratorThread);
        }

        /**
         * @brief Verifies the orchestrator's main processing loop
         * 
         * The orchestrator runs a main loop that coordinates all activities:
         * waiting for commands, updating state, and managing the executor.
         * This test ensures the loop properly sequences all these activities.
         */
        private void _testThreadLoopInjectsCommands()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    123, KeystrokeTransmitterThreadType.Solving
                );
                var transmitterOrchestratorThread = _fixture(threadState);
                _setTransmitFixture(i);
                transmitterOrchestratorThread.Start();
                transmitterOrchestratorThread.Join(10000);
                Debug.Assert(_callOrder.Count == i + 1);
                Debug.Assert(_callOrder[0] == _threadRef + "ThreadStart");
                for (int j = 1; j <= i; j++)
                {
                    Debug.Assert(_callOrder[j] == _threadRef + "ThreadInject");
                    Debug.Assert((int)_thread.InjectCallArg_dataType[j - 1]! == j - 1);
                }
            }
        }

        public void Run()
        {
            _testStartingOrchestratorStartsExecutorThread();
            _testStoppingOrchestratorStopsExecutorThread();
            _testInjectingOrchestratorCommandAssignsThreadState();
            _testInjectOrchestratorCommand();
            _testInjectToExecutorThread();
            _testInjectActionToExecutorThread();
            _testThreadLoopInjectsCommands();
        }
    }


    public class SolvingTransmitterTestSuite
    {
        public void Run()
        {
            new SolvingOrchestratorThreadTests().Run();
        }
    }
}
