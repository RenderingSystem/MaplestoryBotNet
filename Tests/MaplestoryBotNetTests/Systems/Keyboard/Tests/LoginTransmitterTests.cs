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
    public class LoginExecutorThreadHelperTests
    {
        private MockTimestamp _loginStopwatch = new MockTimestamp();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)LoginExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Login
            )
        );

        private MockMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder = (
            new MockMacroCommandsExecutorBuilder()
        );

        private MockMacroCommandsExecutor _macroCommandsExecutor = new MockMacroCommandsExecutor();

        private AbstractConfiguration _maplestoryBotConfiguration = new MaplestoryBotConfiguration();

        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private AbstractKeystrokeTransmitterThreadHelper _fixture(int timeout)
        {
            _loginStopwatch = new MockTimestamp();
            _threadState = new KeystrokeTransmitterThreadState(
                (int)LoginExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Login
            );
            _macroCommandsExecutor = new MockMacroCommandsExecutor();
            _macroCommandsExecutorBuilder = new MockMacroCommandsExecutorBuilder();
            _macroCommandsExecutorBuilder.BuildReturn.Add(_macroCommandsExecutor);
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                MacroSettings = new MacroSettings { LoginTimeout = timeout },
                MacroKeySettings = new MacroKeySettings()
            };
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            var helper = new LoginScreenExecutorThreadHelper(
                _loginStopwatch,
                _threadState,
                _macroCommandsExecutorBuilder
            );
            helper.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            helper.Inject(SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration);
            return helper;
        }

        /**
         * @brief Verifies that resetting the login executor updates the timestamp
         * used to track when the character should enter the login
         * 
         * When the login executor begins a new map reset cycle, it must reset the
         * stopwatch that tracks the elapsed time since the reset was triggered. This
         * timestamp is used to determine when to send keystrokes to enter the login.
         */
        private void _testExecutorResetSetsTimestamp()
        {
            var helper = _fixture(10);
            helper.Reset();
            Debug.Assert(_loginStopwatch.SetTimestampCalls == 1);
        }


        /**
         * @brief Verifies that the executor sends the complete keystroke sequence to
         * enter the login screen, and that the sequence is sent only once regardless
         * of how many times Transmit is called while still within the timeout period
         * 
         * When the bot needs to log into MapleStory, the executor sends a specific
         * sequence of keystrokes: Escape (open options menu), wait, Arrow Up (select
         * exit menu), wait, Enter (select), wait, Enter (confirm), wait. This sequence
         * navigates through the login interface and confirms the character selection.
         * The executor should send the full sequence only once per login attempt,
         * even if Transmit is called multiple times while still waiting for the login
         * to complete. Subsequent Transmit calls should not resend the sequence until
         * the timeout expires.
         */
        private void _testExecutorAttemptsToEnterLogin()
        {
            var helper = _fixture(10);
            var commands = _macroCommandsExecutor.ExecuteCallArg_macroCommands;
            _loginStopwatch.GetTimestampReturn.Add(0);
            Debug.Assert(helper.Transmit());
            Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 1);
            Debug.Assert(commands[0].Count == 8);
            Debug.Assert(commands[0][0] == "key press {ESCAPE} {100} {200}");
            Debug.Assert(commands[0][1] == "wait {350} {350}");
            Debug.Assert(commands[0][2] == "key press {ARROW_UP} {100} {200}");
            Debug.Assert(commands[0][3] == "wait {350} {350}");
            Debug.Assert(commands[0][4] == "key press {ENTER} {100} {200}");
            Debug.Assert(commands[0][5] == "wait {350} {350}");
            Debug.Assert(commands[0][6] == "key press {ENTER} {100} {200}");
            Debug.Assert(commands[0][7] == "wait {350} {350}");
            _loginStopwatch.GetTimestampReturn.Add(0);
            Debug.Assert(helper.Transmit());
            Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 1);
        }

        /**
         * @brief Verifies that the executor sends the full exit sequence to return to
         * the character selection screen after timeout, then presses Enter to reconnect
         * the previously played character back into the game
         * 
         * When the bot decides that the character should be logged out for a while
         * (e.g., during maintenance breaks, after completing a session, or when switching
         * accounts), the executor must first send the complete exit sequence to navigate
         * from the game world back to the character selection screen. After the timeout
         * period, the executor presses Enter to reconnect the previously played character
         * back into the game, allowing the bot to resume automation without needing to
         * manually select the character again.
         */
        private void _testExecutorAttemptsToEnterAndExitLogin()
        {
            for (int timeout = 5; timeout < 10; timeout++)
            foreach (var delta in new[] {-0.01, 0.0, 0.01})
            {
                var helper = _fixture(timeout);
                var commands = _macroCommandsExecutor.ExecuteCallArg_macroCommands;
                var timedOutState = (int)LoginExecutorThreadedUpdate.TimedOut;
                var currState = 123;
                _loginStopwatch.GetTimestampReturn.Add(0);
                _threadState.SetState(currState);
                Debug.Assert(helper.Transmit());
                Debug.Assert(commands[0].Count == 8);
                Debug.Assert(commands[0][0] == "key press {ESCAPE} {100} {200}");
                Debug.Assert(commands[0][1] == "wait {350} {350}");
                Debug.Assert(commands[0][2] == "key press {ARROW_UP} {100} {200}");
                Debug.Assert(commands[0][3] == "wait {350} {350}");
                Debug.Assert(commands[0][4] == "key press {ENTER} {100} {200}");
                Debug.Assert(commands[0][5] == "wait {350} {350}");
                Debug.Assert(commands[0][6] == "key press {ENTER} {100} {200}");
                Debug.Assert(commands[0][7] == "wait {350} {350}");
                Debug.Assert(_threadState.GetState() == currState);
                _loginStopwatch.GetTimestampReturn.Add(timeout + delta);
                Debug.Assert(helper.Transmit());
                if (delta > 0)
                {
                    Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 2);
                    Debug.Assert(commands[1].Count == 1);
                    Debug.Assert(commands[1][0] == "key press {ENTER} {100} {200}");
                    Debug.Assert(_threadState.GetState() == timedOutState);
                }
                else
                {
                    Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 1);
                    Debug.Assert(_threadState.GetState() == currState);
                }
            }
        }

        public void Run()
        {
            _testExecutorResetSetsTimestamp();
            _testExecutorAttemptsToEnterLogin();
            _testExecutorAttemptsToEnterAndExitLogin();
        }
    }


    public class LoginExecutorThreadTests
    {
        private MockKeystrokeTransmitterThreadHelper _executorThreadHelper = new();

        private MockResetEvent _executionEvent = new MockResetEvent();

        private MockRunningState _transmittingState = new MockRunningState();

        private MockRunningState _runningState = new MockRunningState();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)LoginExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Login
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
                        LoginOrchestratorThreadInjectType.Stop, null
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
            return new LoginExecutorThread(
                _executionEvent,
                _executorThreadHelper,
                _threadState,
                _transmittingState,
                _runningState
            );
        }

        /**
         * @brief Verifies the handshake sequence when the login executor starts its
         * map reset transmission routine
         * 
         * When the macro system determines that the character needs to enter the login
         * to reset the map (due to repeated missed runes or stuck puzzles), the orchestrator
         * signals the executor to start its transmission routine. The executor performs a
         * coordinated startup handshake with the keystroke transmitter to ensure the transmitter
         * is ready before any login navigation keystrokes are sent to MapleStory.
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
                keystrokeTransmitterExecutorThread.Inject(LoginOrchestratorThreadInjectType.Start, 0);
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
         * @brief Verifies thread state changes correctly when the login executor begins
         * map reset operations
         * 
         * When the macro system initiates a login map reset, the executor thread transitions
         * through proper states: Starting → Started. This ensures the rest of the system knows
         * the executor is actively processing login navigation and can coordinate other
         * activities accordingly.
         */
        private void _testExecutorStartingHandshakeSetsThreadStates()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            _transmittingState.IsRunningReturn.Add(false);
            keystrokeTransmitterExecutorThread.Inject(
                LoginOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(threadState.SetStateCallArg_state[0] == (int)LoginExecutorThreadedUpdate.Starting);
            Debug.Assert(threadState.SetStateCallArg_state[1] == (int)LoginExecutorThreadedUpdate.Started);
        }

        /**
         * @brief Verifies the handshake sequence when the login executor stops its
         * map reset transmission routine
         * 
         * When the macro system needs to stop the login map reset operation (e.g., the
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
                keystrokeTransmitterExecutorThread.Inject(LoginOrchestratorThreadInjectType.Stop, 0);
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
         * @brief Verifies thread state changes correctly when the login executor stops
         * 
         * When login map reset operations complete or time out, the executor thread
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
                Debug.Assert(threadState.SetStateCallArg_state[0] == (int)LoginExecutorThreadedUpdate.Stopping);
                Debug.Assert(threadState.SetStateCallArg_state[1] == (int)LoginExecutorThreadedUpdate.Stopped);
            }
        }

        /**
         * @brief Verifies the executor continuously transmits login navigation commands
         * while the map reset operation is active
         * 
         * When the macro system is actively performing a login map reset (entering the
         * login, waiting, exiting), the executor thread must continuously send the
         * appropriate keystrokes to navigate the character through the login interface
         * and trigger the map reload. This test ensures that once started, the thread
         * repeatedly calls the transmit method to send navigation commands without stopping
         * until the reset is complete.
         */
        private void _testExecutorThreadLoopTransmitsWhenStarted()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)LoginExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Login
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var start = LoginOrchestratorThreadInjectType.Start;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(start, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == i);
            }
        }

        /**
         * @brief Verifies keystroke transmission stops immediately when login map reset
         * is stopped or times out
         * 
         * When the login map reset operation completes successfully or times out, the
         * executor thread must stop sending navigation keystrokes immediately. This prevents
         * the character from continuing to navigate the login interface after the reset
         * is already complete.
         */
        private void _testExecutorThreadLoopDoesntTransmitWhenStopped()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)LoginExecutorThreadedUpdate.Started,
                    KeystrokeTransmitterThreadType.Login
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var stop = LoginOrchestratorThreadInjectType.Stop;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(stop, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == 0);
            }
        }

        /**
         * @brief Verifies that the executor thread helper is reset before and after each
         * login operation cycle
         * 
         * When the login executor processes map reset commands, the thread helper must
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
                    (int)LoginExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Login
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var callOrder = _executorThreadHelper.CallOrder;
                _transmittingState.IsRunningReturn.Add(false);
                var start = LoginOrchestratorThreadInjectType.Start;
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


    public class LoginOrchestratorThreadTests
    {
        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.Login
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
            return new LoginOrchestratorThread(
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
         * @brief Verifies that starting the login orchestrator also starts the
         * underlying executor thread that processes login operations
         * 
         * The login orchestrator manages entering and exiting the login to
         * reset the game map. When the orchestrator starts, it must also start the
         * executor thread that actually performs the keystroke transmissions for
         * navigating the login interface and map reset sequence.
         */
        private void _testStartingOrchestratorStartsExecutorThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.Login
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(false);
            transmitterOrchestratorThread.Start();
            Debug.Assert(_thread.ThreadStartCalls == 1);
        }

        /**
         * @brief Verifies that stopping the login orchestrator stops the underlying
         * executor thread and adds a stop signal to the thread state collection
         * 
         * When the login orchestrator is stopped, it must shut down its executor
         * thread to prevent any ongoing login operations from continuing.
         */
        private void _testStoppingOrchestratorStopsExecutorThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.Login
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
         * When the macro system injects a command into the login orchestrator
         * the orchestrator adds the integer representation of that command to the
         * thread state collection. The executor thread consumes these states and executes
         * the appropriate login operations.
         */
        private void _testInjectingOrchestratorCommandAssignsThreadState()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                123, KeystrokeTransmitterThreadType.Login
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            var max = LoginOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                transmitterOrchestratorThread.Inject(
                    (LoginOrchestratorThreadInjectType)i, 0
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
         * determine which login operation to execute.
         */
        private void _testInjectOrchestratorCommand()
        {
            var max = LoginOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var transmitterOrchestratorThread = _fixture(threadState);
                transmitterOrchestratorThread.Inject((LoginOrchestratorThreadInjectType)i, 0);
                Debug.Assert(_threadStates.Count == 1);
                Debug.Assert(_threadStates.Take() == i);
            }
        }

        /**
         * @brief Verifies that non-orchestrator injections (e.g., configuration updates,
         * dependency injections) are passed through to the executor thread
         * 
         * When the macro system injects data types that are not login orchestrator
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
         * executor thread. This ensures that login operations (starting the map
         * reset, timing out, stopping) are processed in the order they were requested.
         */
        private void _testThreadLoopInjectsCommands()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    123, KeystrokeTransmitterThreadType.Login
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


    public class LoginTransmitterTestSuite
    {
        public void Run()
        {
            new LoginExecutorThreadHelperTests().Run();
            new LoginExecutorThreadTests().Run();
            new LoginOrchestratorThreadTests().Run();
        }
    }
}
