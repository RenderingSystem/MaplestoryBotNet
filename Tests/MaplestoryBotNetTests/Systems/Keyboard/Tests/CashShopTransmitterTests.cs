using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
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
    public class CashShopExecutorThreadHelperTests
    {
        private MockTimestamp _cashShopStopwatch = new MockTimestamp();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)CashShopExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.CashShop
            )
        );

        private MockMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder = (
            new MockMacroCommandsExecutorBuilder()
        );

        private MockMacroCommandsExecutor _macroCommandsExecutor = new MockMacroCommandsExecutor();

        private AbstractConfiguration _maplestoryBotConfiguration = new MaplestoryBotConfiguration();

        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private AbstractKeystrokeTransmitterThreadHelper _fixture(double delay, int timeout)
        {
            _cashShopStopwatch = new MockTimestamp();
            _threadState = new KeystrokeTransmitterThreadState(
                (int)CashShopExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.CashShop
            );
            _macroCommandsExecutor = new MockMacroCommandsExecutor();
            _macroCommandsExecutorBuilder = new MockMacroCommandsExecutorBuilder();
            _macroCommandsExecutorBuilder.BuildReturn.Add(_macroCommandsExecutor);
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                MacroSettings = new MacroSettings { CashShopTimeout = timeout },
                MacroKeySettings = new MacroKeySettings { CashShopKey = "meow" }
            };
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            var helper = new CashShopExecutorThreadHelper(
                _cashShopStopwatch,
                _threadState,
                _macroCommandsExecutorBuilder,
                delay
            );
            helper.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            helper.Inject(SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration);
            return helper;
        }

        /**
         * @brief Verifies that resetting the cash shop executor updates the timestamp
         * used to track when the character should enter the cash shop
         * 
         * When the cash shop executor begins a new map reset cycle, it must reset the
         * stopwatch that tracks the elapsed time since the reset was triggered. This
         * timestamp is used to determine when to send keystrokes to enter the cash shop.
         */
        private void _testExecutorResetSetsTimestamp()
        {
            var helper = _fixture(10, 10);
            helper.Reset();
            Debug.Assert(_cashShopStopwatch.SetTimestampCalls == 1);
        }

        /**
         * @brief Verifies that the executor sends keystrokes to enter the cash shop when
         * the elapsed time is less than or equal to the configured delay
         * 
         * When the macro system decides to perform a map reset via the cash shop, the
         * executor should send cash shop entrance keystrokes as long as the elapsed time
         * has not exceeded the configured delay. The test checks three scenarios:
         * - Elapsed time less than delay (delta negative) → send keystrokes
         * - Elapsed time exactly equal to delay (delta zero) → send keystrokes
         * - Elapsed time greater than delay (delta positive) → do NOT send keystrokes
         */
        private void _testExecutorAttemptsToEnterCashShop()
        {
            for (int delay = 5; delay < 10; delay++)
            foreach (var delta in new[] { -0.01, 0.0, 0.01 })
            {
                var helper = _fixture(delay, 10);
                _cashShopStopwatch.GetTimestampReturn.Add(delay + delta);
                Debug.Assert(helper.Transmit());
                if (delta < 0)
                {
                    var commands = _macroCommandsExecutor.ExecuteCallArg_macroCommands;
                    Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 2);
                    Debug.Assert(commands[0].Count == 1);
                    Debug.Assert(commands[0][0] == "key press {meow} {100} {150}");
                    Debug.Assert(commands[1].Count == 1);
                    Debug.Assert(commands[1][0] == "wait {100} {150}");
                }
                else
                {
                    Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 0);
                }
            }
        }

        /**
         * @brief Verifies that the executor sends keystrokes to exit the cash shop and
         * updates the thread state to TimedOut when the elapsed time exceeds the timeout
         * 
         * Once the character has entered the cash shop, the executor must wait for the
         * full timeout period to elapse before attempting to exit. After sending the exit
         * keystrokes (Escape, wait, Enter), the thread state is updated to TimedOut to
         * signal that the map reset operation has completed and the character is returning
         * to the normal map.
         */
        private void _testExecutorAttemptsToExitCashShop()
        {
            for (int delay = 5; delay < 10; delay++)
            for (int timeout = 5; timeout < 10; timeout++)
            foreach (var delta in new[] {-0.01, 0.0, 0.01})
            {
                var helper = _fixture(delay, timeout);
                var timedOutState = (int)CashShopExecutorThreadedUpdate.TimedOut;
                var currState = 123;
                _cashShopStopwatch.GetTimestampReturn.Add(delay + timeout + delta);
                _threadState.SetState(currState);
                Debug.Assert(helper.Transmit());
                if (delta > 0)
                {
                    var commands = _macroCommandsExecutor.ExecuteCallArg_macroCommands;
                    Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 3);
                    Debug.Assert(commands[0].Count == 1);
                    Debug.Assert(commands[0][0] == "key press {ESCAPE} {100} {150}");
                    Debug.Assert(commands[1].Count == 1);
                    Debug.Assert(commands[1][0] == "wait {900} {1100}");
                    Debug.Assert(commands[2].Count == 1);
                    Debug.Assert(commands[2][0] == "key press {ENTER} {100} {150}");
                    Debug.Assert(_threadState.GetState() == timedOutState);
                }
                else
                {
                    Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 0);
                    Debug.Assert(_threadState.GetState() == currState);
                }
            }
        }

        public void Run()
        {
            _testExecutorResetSetsTimestamp();
            _testExecutorAttemptsToEnterCashShop();
            _testExecutorAttemptsToExitCashShop();
        }
    }


    public class CashShopExecutorThreadTests
    {
        private MockKeystrokeTransmitterThreadHelper _executorThreadHelper = new();

        private MockResetEvent _executionEvent = new MockResetEvent();

        private MockRunningState _transmittingState = new MockRunningState();

        private MockRunningState _runningState = new MockRunningState();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)CashShopExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.CashShop
            )
        );

        private MockInjectAction _injectAction = new MockInjectAction();

        private List<string> _callOrder = [];

        private string _threadStateRef = "";

        private string _transmittingStateRef = "";

        private string _executionEventRef = "";

        private string _executorThreadHelperRef = "";

        private void _setupNewFixture(
            AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _executorThreadHelper = new MockKeystrokeTransmitterThreadHelper();
            _executionEvent = new MockResetEvent();
            _transmittingState = new MockRunningState();
            _runningState = new MockRunningState();
            _threadState = threadState;
            _injectAction = new MockInjectAction();
            _callOrder = [];
        }

        private void _setupCallOrder()
        {
            if (_threadState is MockKeystrokeTransmitterThreadState mockThreadState)
            {
                mockThreadState.CallOrder = _callOrder;
            }
            _executionEvent.CallOrder = _callOrder;
            _injectAction.CallOrder = _callOrder;
            _transmittingState.CallOrder = _callOrder;
        }

        private void _setupRunningState()
        {
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
        }

        private void _setupTransmit(int transmitCount)
        {
            for (int i = 0; i < transmitCount - 1; i++)
            {
                _executorThreadHelper.TransmitReturn.Add(true);
            }
            _executorThreadHelper.TransmitReturn.Add(false);
        }

        private List<Action> _stopLambdas(AbstractThread abstractThread)
        {
            return [
                () =>
                {
                    abstractThread.Inject(
                        CashShopOrchestratorThreadInjectType.Stop, null
                    );
                },
                () =>
                {
                    abstractThread.Stop();
                }
            ];
        }

        private void _setupReferences()
        {
            _threadStateRef = new TestUtilities().Reference(_threadState);
            _transmittingStateRef = new TestUtilities().Reference(_transmittingState); ;
            _executionEventRef = new TestUtilities().Reference(_executionEvent);
            _executorThreadHelperRef = new TestUtilities().Reference(_executorThreadHelper);
        }

        private AbstractThread _fixture(
            int transmitCount, AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _setupNewFixture(threadState);
            _setupCallOrder();
            _setupRunningState();
            _setupTransmit(transmitCount);
            _setupReferences();
            return new CashShopExecutorThread(
                _executionEvent,
                _executorThreadHelper,
                _threadState,
                _transmittingState,
                _runningState
            );
        }

        /**
         * @brief Verifies the handshake sequence when the cash shop executor starts its
         * map reset transmission routine
         * 
         * When the macro system determines that the character needs to enter the cash shop
         * to reset the map (due to repeated missed runes or stuck puzzles), the orchestrator
         * signals the executor to start its transmission routine. The executor performs a
         * coordinated startup handshake with the keystroke transmitter to ensure the transmitter
         * is ready before any cash shop navigation keystrokes are sent to MapleStory.
         */
        private void _testExecutorStartingHandshake()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                for (int j = 0; j < i; j++)
                {
                    _transmittingState.IsRunningReturn.Add(true);
                }
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(CashShopOrchestratorThreadInjectType.Start, 0);
                Debug.Assert(_callOrder.Count == (i + 4));
                Debug.Assert(_callOrder[0] == _threadStateRef + "SetState");
                for (int j = 1; j <= i + 1; j++)
                {
                    Debug.Assert(_callOrder[j] == _transmittingStateRef + "IsRunning");
                }
                Debug.Assert(_callOrder[i + 2] == _threadStateRef + "SetState");
                Debug.Assert(_callOrder[i + 3] == _executionEventRef + "Set");
            }
        }

        /**
         * @brief Verifies thread state changes correctly when the cash shop executor begins
         * map reset operations
         * 
         * When the macro system initiates a cash shop map reset, the executor thread transitions
         * through proper states: Starting → Started. This ensures the rest of the system knows
         * the executor is actively processing cash shop navigation and can coordinate other
         * activities accordingly.
         */
        private void _testExecutorStartingHandshakeSetsThreadStates()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            _transmittingState.IsRunningReturn.Add(false);
            keystrokeTransmitterExecutorThread.Inject(
                CashShopOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(threadState.SetStateCallArg_state[0] == (int)CashShopExecutorThreadedUpdate.Starting);
            Debug.Assert(threadState.SetStateCallArg_state[1] == (int)CashShopExecutorThreadedUpdate.Started);
        }

        /**
         * @brief Verifies the handshake sequence when the cash shop executor stops its
         * map reset transmission routine
         * 
         * When the macro system needs to stop the cash shop map reset operation (e.g., the
         * reset is complete or timed out), the orchestrator signals the executor to stop
         * sending keystrokes. The executor performs a coordinated shutdown handshake to ensure
         * keystrokes stop cleanly before the routine exits, preventing stray inputs during
         * the transition.
         */
        private void _testExecutorStoppingHandshake()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                for (int j = 0; j < i; j++)
                {
                    _transmittingState.IsRunningReturn.Add(true);
                }
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(CashShopOrchestratorThreadInjectType.Stop, 0);
                Debug.Assert(_callOrder.Count == (i + 3));
                Debug.Assert(_callOrder[0] == _threadStateRef + "SetState");
                for (int j = 1; j <= i + 1; j++)
                {
                    Debug.Assert(_callOrder[j] == _transmittingStateRef + "IsRunning");
                }
                Debug.Assert(_callOrder[i + 2] == _threadStateRef + "SetState");
            }
        }

        /**
         * @brief Verifies thread state changes correctly when the cash shop executor stops
         * 
         * When cash shop map reset operations complete or time out, the executor thread
         * transitions through proper shutdown states: Started → Stopping → Stopped. This ensures
         * the system accurately reflects that map reset is no longer active and botting can
         * safely resume.
         */
        private void _testExecutorStoppingHandshakeSetsThreadStates()
        {
            for (int i = 0; i < 2; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                _transmittingState.IsRunningReturn.Add(false);
                var stopLambdas = _stopLambdas(keystrokeTransmitterExecutorThread);
                stopLambdas[i]();
                Debug.Assert(threadState.SetStateCallArg_state[0] == (int)CashShopExecutorThreadedUpdate.Stopping);
                Debug.Assert(threadState.SetStateCallArg_state[1] == (int)CashShopExecutorThreadedUpdate.Stopped);
            }
        }

        /**
         * @brief Verifies the executor continuously transmits cash shop navigation commands
         * while the map reset operation is active
         * 
         * When the macro system is actively performing a cash shop map reset (entering the
         * cash shop, waiting, exiting), the executor thread must continuously send the
         * appropriate keystrokes to navigate the character through the cash shop interface
         * and trigger the map reload. This test ensures that once started, the thread
         * repeatedly calls the transmit method to send navigation commands without stopping
         * until the reset is complete.
         */
        private void _testExecutorThreadLoopTransmitsWhenStarted()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)CashShopExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.CashShop
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var start = CashShopOrchestratorThreadInjectType.Start;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(start, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == i);
            }
        }

        /**
         * @brief Verifies keystroke transmission stops immediately when cash shop map reset
         * is stopped or times out
         * 
         * When the cash shop map reset operation completes successfully or times out, the
         * executor thread must stop sending navigation keystrokes immediately. This prevents
         * the character from continuing to navigate the cash shop interface after the reset
         * is already complete.
         */
        private void _testExecutorThreadLoopDoesntTransmitWhenStopped()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)CashShopExecutorThreadedUpdate.Started,
                    KeystrokeTransmitterThreadType.CashShop
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var stop = CashShopOrchestratorThreadInjectType.Stop;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(stop, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == 0);
            }
        }

        /**
         * @brief Verifies that the executor thread helper is reset before and after each
         * cash shop operation cycle
         * 
         * When the cash shop executor processes map reset commands, the thread helper must
         * be reset to a clean state before sending keystrokes for the current operation.
         * This prevents stale command data from previous operations from affecting the
         * current navigation. After the keystroke transmission completes, the helper is reset
         * again to clear any pending state, ensuring the next operation starts fresh.
         */
        private void _testExecutorThreadLoopResetsBeforeAndAfterTransmit()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)CashShopExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.CashShop
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var callOrder = _executorThreadHelper.CallOrder;
                _transmittingState.IsRunningReturn.Add(false);
                var start = CashShopOrchestratorThreadInjectType.Start;
                keystrokeTransmitterExecutorThread.Inject(start, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(callOrder.Count == i + 2);
                Debug.Assert(callOrder[0] == _executorThreadHelperRef + "Reset");
                for (int j = 1; j <= i; j++)
                {
                    Debug.Assert(callOrder[j] == _executorThreadHelperRef + "Transmit");
                }
                Debug.Assert(callOrder[i + 1] == _executorThreadHelperRef + "Reset");
            }
        }

        public void Run()
        {
            _testExecutorStartingHandshake();
            _testExecutorStartingHandshakeSetsThreadStates();
            _testExecutorStoppingHandshake();
            _testExecutorStoppingHandshakeSetsThreadStates();
            _testExecutorThreadLoopTransmitsWhenStarted();
            _testExecutorThreadLoopDoesntTransmitWhenStopped();
            _testExecutorThreadLoopResetsBeforeAndAfterTransmit();
        }
    }


    public class CashShopOrchestratorThreadTests
    {
        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.CashShop
            )
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
            return new CashShopOrchestratorThread(
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
         * @brief Verifies that starting the cash shop orchestrator also starts the
         * underlying executor thread that processes cash shop operations
         * 
         * The cash shop orchestrator manages entering and exiting the cash shop to
         * reset the game map. When the orchestrator starts, it must also start the
         * executor thread that actually performs the keystroke transmissions for
         * navigating the cash shop interface and map reset sequence.
         */
        private void _testStartingOrchestratorStartsExecutorThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.CashShop
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(false);
            transmitterOrchestratorThread.Start();
            Debug.Assert(_thread.ThreadStartCalls == 1);
        }

        /**
         * @brief Verifies that stopping the cash shop orchestrator stops the underlying
         * executor thread and adds a stop signal to the thread state collection
         * 
         * When the cash shop orchestrator is stopped, it must shut down its executor
         * thread to prevent any ongoing cash shop operations from continuing.
         */
        private void _testStoppingOrchestratorStopsExecutorThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.CashShop
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
         * @brief Verifies that injecting an orchestrator command adds the corresponding
         * thread state to the collection for processing
         * 
         * When the macro system injects a command into the cash shop orchestrator
         * the orchestrator adds the integer representation of that command to the
         * thread state collection. The executor thread consumes these states and executes
         * the appropriate cash shop operations.
         */
        private void _testInjectingOrchestratorCommandAssignsThreadState()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                123, KeystrokeTransmitterThreadType.CashShop
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            var max = CashShopOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                transmitterOrchestratorThread.Inject(
                    (CashShopOrchestratorThreadInjectType)i, 0
                );
                Debug.Assert(_threadStates.Count == 1);
                Debug.Assert(_threadStates.Take() == i);
            }
        }

        /**
         * @brief Verifies that injecting an orchestrator command correctly adds the
         * command's state to the thread state collection
         * 
         * This test validates that all possible orchestrator command types (from 0 to
         * MaxNum) are properly converted to integers and added to the thread state
         * collection when injected. The executor thread uses these integer states to
         * determine which cash shop operation to execute.
         */
        private void _testInjectOrchestratorCommand()
        {
            var max = CashShopOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var transmitterOrchestratorThread = _fixture(threadState);
                transmitterOrchestratorThread.Inject((CashShopOrchestratorThreadInjectType)i, 0);
                Debug.Assert(_threadStates.Count == 1);
                Debug.Assert(_threadStates.Take() == i);
            }
        }

        /**
         * @brief Verifies that non-orchestrator injections (e.g., configuration updates,
         * dependency injections) are passed through to the executor thread
         * 
         * When the macro system injects data types that are not cash shop orchestrator
         * commands (such as configuration settings or keystroke transmitter dependencies),
         * the orchestrator must forward these injections to the underlying executor
         * thread without modification.
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
         * @brief Verifies that inject actions (callbacks) are properly set up to return
         * the orchestrator thread as a dependency
         * 
         * When an inject action is registered with the orchestrator, the action should
         * be invoked with the ThreadDependency type and the orchestrator itself as the
         * data. This allows other components to receive a reference to the orchestrator
         * thread for coordination purposes.
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
         * @brief Verifies that the thread loop consumes injected commands in order and
         * forwards them to the executor thread for processing
         * 
         * When commands are added to the thread state collection, the orchestrator's
         * main loop consumes them sequentially and injects each command into the
         * executor thread. This ensures that cash shop operations (starting the map
         * reset, timing out, stopping) are processed in the order they were requested.
         */
        private void _testThreadLoopInjectsCommands()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    123, KeystrokeTransmitterThreadType.CashShop
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


    public class CashShopTransmitterTestSuite
    {
        public void Run()
        {
            new CashShopExecutorThreadHelperTests().Run();
            new CashShopExecutorThreadTests().Run();
            new CashShopOrchestratorThreadTests().Run();
        }
    }
}
