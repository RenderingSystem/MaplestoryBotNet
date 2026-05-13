using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using System.Collections.Concurrent;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests
{
    public class AllCureExecutorThreadHandlerTests
    {
        private MockTimestamp _activeStopwatch = new MockTimestamp();

        private MockMacroSleeper _macroSleeper = new MockMacroSleeper();

        private MockMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder = (
            new MockMacroCommandsExecutorBuilder()
        );

        private MockMacroCommandsExecutor _macroCommandsExecutor = (
            new MockMacroCommandsExecutor()
        );

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private MockKeystrokeTransmitter _keystrokeTransmitter = (
            new MockKeystrokeTransmitter()
        );

        private List<string> _callOrder = [];

        private AbstractAilmentExecutorThreadHandler _fixture()
        {
            _activeStopwatch = new MockTimestamp();
            _macroSleeper = new MockMacroSleeper();
            _macroCommandsExecutorBuilder = new MockMacroCommandsExecutorBuilder();
            _macroCommandsExecutor = new MockMacroCommandsExecutor();
            _macroCommandsExecutorBuilder.BuildReturn.Add(_macroCommandsExecutor);
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                MacroKeySettings = new MacroKeySettings
                {
                    AilmentsAllcureKey = "meow"
                }
            };
            _callOrder = [];
            _activeStopwatch.CallOrder = _callOrder;
            _macroCommandsExecutor.CallOrder = _callOrder;
            _macroSleeper.CallOrder = _callOrder;
            var handler = new AllCureExecutorThreadHandler(
                _activeStopwatch,
                _macroSleeper,
                _macroCommandsExecutorBuilder
            );
            handler.Inject(
                SystemInjectType.KeystrokeTransmitter,
                _keystrokeTransmitter

            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Verifies that the AllCure handler executes the complete sequence of
         * operations (set stopwatch, send key press, calculate delay, and sleep) when
         * handling an ailment that requires the all-cure key
         * 
         * When a status ailment is detected that requires the all-cure key (e.g., a
         * single key that removes all ailments), the handler must reset the stopwatch,
         * send the all-cure key press, read the elapsed time, and sleep for the remaining
         * cooldown period. This test ensures all four operations occur in the correct
         * order when the handler is invoked.
         */
        private void _testHandlingAllCureExecutesKeyPressAndSleeps()
        {
            var handler = _fixture();
            _activeStopwatch.GetTimestampReturn.Add(.729);
            var stopwatchRef = new TestUtilities().Reference(_activeStopwatch);
            var executorRef = new TestUtilities().Reference(_macroCommandsExecutor);
            var sleeperRef = new TestUtilities().Reference(_macroSleeper);
            handler.Handle(1234);
            Debug.Assert(_callOrder.Count == 4);
            Debug.Assert(_callOrder[0] == stopwatchRef + "SetTimestamp");
            Debug.Assert(_callOrder[1] == executorRef + "Execute");
            Debug.Assert(_callOrder[2] == stopwatchRef + "GetTimestamp");
            Debug.Assert(_callOrder[3] == sleeperRef + "Sleep");
        }

        /**
         * @brief Verifies that the AllCure handler sends the correct key press command
         * using the configured all-cure key from the bot's macro settings
         * 
         * When the handler processes an ailment requiring the all-cure key, it must
         * send a key press command with the specific key configured in
         * MacroKeySettings.AilmentsAllcureKey. This test ensures the correct key
         * (configured as "meow" in the fixture) is used.
         */
        private void _testHandlingAllCureKeyPress()
        {
            var handler = _fixture();
            _activeStopwatch.GetTimestampReturn.Add(.729);
            handler.Handle(1234);
            var macroCommands = _macroCommandsExecutor.ExecuteCallArg_macroCommands[0];
            Debug.Assert(macroCommands.Count == 1);
            Debug.Assert(macroCommands[0] == "key press {meow} {100} {150}");
        }

        /**
         * @brief Verifies that the AllCure handler calculates the correct sleep duration
         * based on the active delay and elapsed time since the key press
         * 
         * After pressing the all-cure key, the handler must wait for the remaining
         * portion of the active delay period. With an active delay of 1234ms and elapsed
         * time of 0.729 seconds (729ms), the remaining time is 1234 - 729 = 505ms.
         */
        private void _testHandlingAllCureSleepTimer()
        {
            var handler = _fixture();
            _activeStopwatch.GetTimestampReturn.Add(.729);
            handler.Handle(1234);
            Debug.Assert(_macroSleeper.SleepCallArg_milliseconds[0] == 505);
        }

        public void Run()
        {
            _testHandlingAllCureExecutesKeyPressAndSleeps();
            _testHandlingAllCureKeyPress();
            _testHandlingAllCureSleepTimer();
        }
    }


    public class ArrowKeysExecutorThreadHandlerTests
    {
        private MockTimestamp _activeStopwatch = new MockTimestamp();

        private MockMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder = (
            new MockMacroCommandsExecutorBuilder()
        );

        private MockMacroCommandsExecutor _macroCommandsExecutor = (
            new MockMacroCommandsExecutor()
        );

        private MockKeystrokeTransmitter _keystrokeTransmitter = (
            new MockKeystrokeTransmitter()
        );

        private List<string> _callOrder = [];

        private AbstractAilmentExecutorThreadHandler _fixture()
        {
            _activeStopwatch = new MockTimestamp();
            _macroCommandsExecutorBuilder = new MockMacroCommandsExecutorBuilder();
            _macroCommandsExecutor = new MockMacroCommandsExecutor();
            _macroCommandsExecutorBuilder.BuildReturn.Add(_macroCommandsExecutor);
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            var handler = new ArrowKeysExecutorThreadHandler(
                _activeStopwatch,
                _macroCommandsExecutorBuilder
            );
            handler.Inject(
                SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter
            );
            _callOrder = [];
            _activeStopwatch.CallOrder = _callOrder;
            _macroCommandsExecutor.CallOrder = _callOrder;
            return handler;
        }

        /**
         * @brief Verifies that the ArrowKeys handler repeatedly sends left and right arrow
         * key presses for the entire duration of the active delay period to cure petrification
         * 
         * When the character is afflicted with petrification (a status ailment that freezes
         * the character in place), the cure requires rapidly alternating left and right
         * arrow key presses to break free. The handler must continuously send these
         * alternating key presses until the active delay period expires, simulating the
         * player mashing left and right arrows to escape the petrified state.
         */
        private void _testHandlingArrowkeysExecutesUntilDelayThreshold()
        {
            for (int i = 1; i < 10; i++)
            {
                var handler = _fixture();
                var stopwatchRef = new TestUtilities().Reference(_activeStopwatch);
                var executorRef = new TestUtilities().Reference(_macroCommandsExecutor);
                for (int j = i - 1; j >= 0; j--)
                {
                    _activeStopwatch.GetTimestampReturn.Add(1.234 - (j / 1000.0));
                }
                handler.Handle(1234);
                Debug.Assert(_callOrder.Count == (3 * (i - 1)) + 2);
                Debug.Assert(_callOrder[0] == stopwatchRef + "SetTimestamp");
                Debug.Assert(_callOrder[1] == stopwatchRef + "GetTimestamp");
                for (int j = 0; j < i - 1; j++)
                {
                    Debug.Assert(_callOrder[(3 * j) + 2] == executorRef + "Execute");
                    Debug.Assert(_callOrder[(3 * j) + 3] == executorRef + "Execute");
                    Debug.Assert(_callOrder[(3 * j) + 4] == stopwatchRef + "GetTimestamp");
                }
            }
        }

        /**
         * @brief Verifies that the ArrowKeys handler sends the correct alternating arrow
         * key commands (LEFT then RIGHT) to cure petrification
         * 
         * When curing petrification, the handler must send alternating left and right
         * arrow key presses. Left is pressed first, followed immediately by right, and
         * this pattern repeats until the character breaks free from the petrified state.
         */
        private void _testHandlingArrowKeysExecutesArrowKeyPresses()
        {
            var handler = _fixture();
            _activeStopwatch.GetTimestampReturn.Add(1.233);
            _activeStopwatch.GetTimestampReturn.Add(1.234);
            handler.Handle(1234);
            var macroCommands = _macroCommandsExecutor.ExecuteCallArg_macroCommands;
            Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 2);
            Debug.Assert(macroCommands[0].Count == 1);
            Debug.Assert(macroCommands[1].Count == 1);
            Debug.Assert(macroCommands[0][0] == "key press {ARROW_LEFT} {50} {100}");
            Debug.Assert(macroCommands[1][0] == "key press {ARROW_RIGHT} {50} {100}");
        }

        public void Run()
        {
            _testHandlingArrowkeysExecutesUntilDelayThreshold();
            _testHandlingArrowKeysExecutesArrowKeyPresses();
        }
    }


    public class AilmentExecutorThreadHelperTests
    {
        private Dictionary<string, Ailment> _ailments = [];

        private AbstractBottingModel _bottingModel = new BottingModel();

        private MockAilmentExecutorThreadHandler _allCureHandler = (
            new MockAilmentExecutorThreadHandler()
        );

        private MockAilmentExecutorThreadHandler _arrowKeysHandler = (
            new MockAilmentExecutorThreadHandler()
        );

        private MockKeystrokeTransmitterThreadState _threadState = (
            new MockKeystrokeTransmitterThreadState()
        );

        private AbstractKeystrokeTransmitterThreadHelper _fixture()
        {
            _allCureHandler = new MockAilmentExecutorThreadHandler();
            _arrowKeysHandler = new MockAilmentExecutorThreadHandler();
            _threadState = new MockKeystrokeTransmitterThreadState();
            _bottingModel = new BottingModel();
            var helper = new AilmentExecutorThreadHelper(
                _allCureHandler,
                _arrowKeysHandler,
                _threadState
            );
            return helper;
        }

        /**
         * @brief Verifies that the transmit method always returns true regardless of
         * whether ailments are detected or cured, ensuring the thread continues running
         * 
         * When the ailment executor processes status ailments, the transmit method must
         * always return true to indicate successful completion, even when no ailments
         * are found or when errors occur. This prevents the thread from stopping
         * unexpectedly.
         */
        private void _testTransmitReturningTrue()
        {
            var helper = _fixture();
            Debug.Assert(helper.Transmit());
        }

        /**
         * @brief Verifies that configuration updates and dependency injections are
         * forwarded to both the AllCure and ArrowKeys handlers
         * 
         * When the ailment executor receives configuration updates (e.g., new ailment
         * settings, keystroke transmitter) or dependency injections, it must forward
         * these to all registered handlers so they can update their internal state
         * (e.g., all-cure key binding, arrow key timing). This ensures handlers stay
         * synchronized with the current bot configuration.
         */
        private void _testInjectingToHandlers()
        {
            var helper = _fixture();
            helper.Inject(123, 234);
            Debug.Assert(_allCureHandler.InjectCalls == 1);
            Debug.Assert(_arrowKeysHandler.InjectCalls == 1);
            Debug.Assert((int)_allCureHandler.InjectCallArg_dataType[0] == 123);
            Debug.Assert((int)_allCureHandler.InjectCallArg_data[0]! == 234);
            Debug.Assert((int)_arrowKeysHandler.InjectCallArg_dataType[0] == 123);
            Debug.Assert((int)_arrowKeysHandler.InjectCallArg_data[0]! == 234);
        }

        /**
         * @brief Verifies that when an AllCure-type ailment is detected, the AllCure
         * handler is invoked with the correct active delay, but only when the detection
         * count is non-zero
         * 
         * When the ailments model reports a positive detection count for an ailment
         * configured with AllCure, the executor must trigger the AllCure handler to
         * press the all-cure key and wait for the cooldown. If the detection count is
         * zero (ailment not present), the handler should not be invoked.
         */
        private void _testTransmittingAllCure()
        {
            foreach (var count in new[] { 234, 0 })
            {
                var helper = _fixture();
                helper.Inject(
                    SystemInjectType.ConfigurationUpdate,
                    new MaplestoryBotConfiguration
                    {
                        Ailments = new Dictionary<string, Ailment>
                        {
                            ["meow1"] = new Ailment { AllCure = 123, ActiveDelay = 456 },
                            ["meow2"] = new Ailment { StopBot = 123 },
                            ["meow3"] = new Ailment { ArrowKeys = 123 }
                        }
                    }
                );
                _bottingModel.GetAilmentsModel().SetAilment("meow1", count);
                _bottingModel.GetAilmentsModel().SetAilment("meow2", 0);
                _bottingModel.GetAilmentsModel().SetAilment("meow3", 0);
                helper.Inject(SystemInjectType.BottingModel, _bottingModel);
                helper.Transmit();
                if (count != 0)
                {
                    Debug.Assert(_allCureHandler.HandleCalls == 1);
                    Debug.Assert(_allCureHandler.HandleCallArg_activeDelay[0] == 456);
                }
                else
                {
                    Debug.Assert(_allCureHandler.HandleCalls == 0);
                }
            }
        }

        /**
         * @brief Verifies that when an ArrowKeys-type ailment is detected, the ArrowKeys
         * handler is invoked with the correct active delay, but only when the detection
         * count is non-zero
         * 
         * When the ailments model reports a positive detection count for an ailment
         * configured with ArrowKeys (e.g., petrification requiring alternating arrow
         * presses), the executor must trigger the ArrowKeys handler to send the
         * alternating left/right arrow key sequence for the cure duration.
         */
        private void _testTransmittingArrowKeys()
        {
            foreach (var count in new[] { 234, 0 })
            {
                var helper = _fixture();
                helper.Inject(
                    SystemInjectType.ConfigurationUpdate,
                    new MaplestoryBotConfiguration
                    {
                        Ailments = new Dictionary<string, Ailment>
                        {
                            ["meow1"] = new Ailment { ArrowKeys = 123, ActiveDelay = 456 },
                            ["meow2"] = new Ailment { StopBot = 123 },
                            ["meow3"] = new Ailment { AllCure = 123 }
                        }
                    }
                );
                _bottingModel.GetAilmentsModel().SetAilment("meow1", count);
                _bottingModel.GetAilmentsModel().SetAilment("meow2", 0);
                _bottingModel.GetAilmentsModel().SetAilment("meow3", 0);
                helper.Inject(SystemInjectType.BottingModel, _bottingModel);
                helper.Transmit();
                if (count != 0)
                {
                    Debug.Assert(_arrowKeysHandler.HandleCalls == 1);
                    Debug.Assert(_arrowKeysHandler.HandleCallArg_activeDelay[0] == 456);
                }
                else
                {
                    Debug.Assert(_arrowKeysHandler.HandleCalls == 0);
                }
            }
        }

        /**
         * @brief Verifies that when a StopBot-type ailment is detected, the executor
         * sets the thread state to StopBot without invoking any handlers
         * 
         * When the ailments model reports a positive detection count for an ailment
         * configured with StopBot (a critical ailment requiring immediate bot shutdown),
         * the executor must set the thread state to StopBot. This signals the higher-level
         * orchestrator to pause or stop automation. No cure handlers should be invoked
         * since the situation requires manual intervention or a full bot reset.
         */
        private void _testSettingStopBotState()
        {
            foreach (var count in new[] { 234, 0 })
            {
                var helper = _fixture();
                helper.Inject(
                    SystemInjectType.ConfigurationUpdate,
                    new MaplestoryBotConfiguration
                    {
                        Ailments = new Dictionary<string, Ailment>
                        {
                            ["meow1"] = new Ailment { StopBot = 123, ActiveDelay = 456 },
                            ["meow2"] = new Ailment { AllCure = 123 },
                            ["meow3"] = new Ailment { ArrowKeys = 123 }
                        }
                    }
                );
                _bottingModel.GetAilmentsModel().SetAilment("meow1", count);
                _bottingModel.GetAilmentsModel().SetAilment("meow2", 0);
                _bottingModel.GetAilmentsModel().SetAilment("meow3", 0);
                helper.Inject(SystemInjectType.BottingModel, _bottingModel);
                helper.Transmit();
                if (count != 0)
                {
                    Debug.Assert(_threadState.SetStateCalls == 1);
                    Debug.Assert(
                        _threadState.SetStateCallArg_state[0] ==
                        (int)AilmentExecutorThreadedUpdate.StopBot
                    );
                }
                else
                {
                    Debug.Assert(_arrowKeysHandler.HandleCalls == 0);
                }
            }
        }

        /**
         * @brief Verifies that when no ailments are detected with non-zero counts, the
         * executor sets the thread state to Cured
         * 
         * When the ailments model reports zero detection counts for all configured
         * ailments (meaning the character is free of any status effects), the executor
         * must set the thread state to Cured. This informs the orchestrator that no
         * ailments are active and the bot can continue normal operation.
         */
        private void _testSettingCuredState()
        {
            var helper = _fixture();
            helper.Inject(
                SystemInjectType.ConfigurationUpdate,
                new MaplestoryBotConfiguration
                {
                    Ailments = new Dictionary<string, Ailment>
                    {
                        ["meow1"] = new Ailment { StopBot = 123, ActiveDelay = 456 },
                        ["meow2"] = new Ailment { ArrowKeys = 123, ActiveDelay = 456 },
                        ["meow3"] = new Ailment { AllCure = 123, ActiveDelay = 456 }
                    }
                }
            );
            _bottingModel.GetAilmentsModel().SetAilment("meow1", 0);
            _bottingModel.GetAilmentsModel().SetAilment("meow2", 0);
            _bottingModel.GetAilmentsModel().SetAilment("meow3", 0);
            helper.Inject(SystemInjectType.BottingModel, _bottingModel);
            helper.Transmit();
            Debug.Assert(_allCureHandler.HandleCalls == 0);
            Debug.Assert(_arrowKeysHandler.HandleCalls == 0);
            Debug.Assert(_threadState.SetStateCalls == 1);
            Debug.Assert(
                _threadState.SetStateCallArg_state[0] ==
                (int)AilmentExecutorThreadedUpdate.Cured
            );
        }

        /**
         * @brief Verifies that StopBot-type ailments take priority over ArrowKeys and
         * AllCure when multiple ailments are detected simultaneously
         * 
         * When the character suffers from multiple status ailments at once, the most
         * dangerous ailment (StopBot) must be handled first. This test ensures that
         * even when ArrowKeys and AllCure ailments are also detected, the executor
         * prioritizes StopBot and sets the thread state to StopBot without invoking
         * the other handlers.
         */
        private void _testStopBotPriority()
        {
            var helper = _fixture();
            helper.Inject(
                SystemInjectType.ConfigurationUpdate,
                new MaplestoryBotConfiguration
                {
                    Ailments = new Dictionary<string, Ailment>
                    {
                        ["meow1"] = new Ailment { StopBot = 123 },
                        ["meow2"] = new Ailment { ArrowKeys = 123 },
                        ["meow3"] = new Ailment { AllCure = 123 }
                    }
                }
            );
            _bottingModel.GetAilmentsModel().SetAilment("meow1", 123);
            _bottingModel.GetAilmentsModel().SetAilment("meow2", 123);
            _bottingModel.GetAilmentsModel().SetAilment("meow3", 123);
            helper.Inject(SystemInjectType.BottingModel, _bottingModel);
            helper.Transmit();
            Debug.Assert(_arrowKeysHandler.HandleCalls == 0);
            Debug.Assert(_threadState.SetStateCalls == 1);
            Debug.Assert(
                _threadState.SetStateCallArg_state[0] ==
                (int)AilmentExecutorThreadedUpdate.StopBot
            );
        }

        /**
         * @brief Verifies that ArrowKeys-type ailments take priority over AllCure-type
         * ailments when StopBot is not detected
         * 
         * When multiple non-critical ailments are detected simultaneously, the executor
         * must prioritize ArrowKeys ailments (which require active user input to cure,
         * such as petrification) over AllCure ailments (which can be cured with a single
         * key press). This ensures the most urgent interaction-based cures are handled
         * first.
         */
        private void _testArrowKeysPriority()
        {
            var helper = _fixture();
            helper.Inject(
                SystemInjectType.ConfigurationUpdate,
                new MaplestoryBotConfiguration
                {
                    Ailments = new Dictionary<string, Ailment>
                    {
                        ["meow1"] = new Ailment { ArrowKeys = 123 },
                        ["meow2"] = new Ailment { AllCure = 123 },
                        ["meow3"] = new Ailment { StopBot = 123 }
                    }
                }
            );
            _bottingModel.GetAilmentsModel().SetAilment("meow1", 123);
            _bottingModel.GetAilmentsModel().SetAilment("meow2", 123);
            _bottingModel.GetAilmentsModel().SetAilment("meow3", 0);
            helper.Inject(SystemInjectType.BottingModel, _bottingModel);
            helper.Transmit();
            Debug.Assert(_arrowKeysHandler.HandleCalls == 1);
        }

        public void Run()
        {
            _testTransmitReturningTrue();
            _testInjectingToHandlers();
            _testTransmittingAllCure();
            _testTransmittingArrowKeys();
            _testSettingStopBotState();
            _testSettingCuredState();
            _testStopBotPriority();
            _testArrowKeysPriority();
        }
    }


    public class AilmentExecutorThreadTests
    {
        private MockKeystrokeTransmitterThreadHelper _executorThreadHelper = (
            new MockKeystrokeTransmitterThreadHelper()
        );

        private MockResetEvent _executionEvent = new MockResetEvent();

        private MockRunningState _transmittingState = new MockRunningState();

        private MockRunningState _runningState = new MockRunningState();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)AilmentExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Ailment
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
                        AilmentOrchestratorThreadInjectType.Stop, null
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
            return new AilmentExecutorThread(
                _executionEvent,
                _executorThreadHelper,
                _threadState,
                _transmittingState,
                _runningState
            );
        }

        /**
         * @brief Verifies the handshake sequence when the ailment executor thread starts
         * its status ailment detection and curing routine
         * 
         * When the macro system determines that the bot needs to check for and cure
         * status ailments (e.g., seal, weakness, curse), the orchestrator signals the
         * executor to start its transmission routine. The executor performs a coordinated
         * startup handshake with the keystroke transmitter to ensure the transmitter is
         * ready before any cure keystrokes are sent to MapleStory.
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
                keystrokeTransmitterExecutorThread.Inject(AilmentOrchestratorThreadInjectType.Start, 0);
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
         * @brief Verifies that the ailment executor thread state changes correctly
         * during the startup sequence
         * 
         * When the macro system initializes ailment detection, the executor thread must
         * transition through proper states: Stopped → Starting → Started. This ensures
         * the rest of the system knows the executor is actively monitoring for status
         * ailments and ready to send cure keystrokes when needed.
         */
        private void _testExecutorStartingHandshakeSetsThreadStates()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            _transmittingState.IsRunningReturn.Add(false);
            keystrokeTransmitterExecutorThread.Inject(
                AilmentOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(threadState.SetStateCallArg_state[0] == (int)AilmentExecutorThreadedUpdate.Starting);
            Debug.Assert(threadState.SetStateCallArg_state[1] == (int)AilmentExecutorThreadedUpdate.Started);
        }

        /**
         * @brief Verifies the handshake sequence when the ailment executor thread stops
         * its status ailment detection and curing routine
         * 
         * When the macro system needs to stop ailment monitoring (e.g., during bot
         * shutdown or when switching to a critical task), the orchestrator signals the
         * executor to stop sending cure keystrokes. The executor performs a coordinated
         * shutdown handshake to ensure keystrokes stop cleanly before the routine exits.
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
                keystrokeTransmitterExecutorThread.Inject(AilmentOrchestratorThreadInjectType.Stop, 0);
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
         * @brief Verifies that the ailment executor thread state changes correctly
         * during the shutdown sequence
         * 
         * When ailment monitoring is stopped, the executor thread must transition
         * through proper shutdown states: Started → Stopping → Stopped. This ensures
         * the system accurately reflects that ailment scanning has ceased and cure
         * keystrokes are no longer being sent.
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
                Debug.Assert(threadState.SetStateCallArg_state[0] == (int)AilmentExecutorThreadedUpdate.Stopping);
                Debug.Assert(threadState.SetStateCallArg_state[1] == (int)AilmentExecutorThreadedUpdate.Stopped);
            }
        }

        /**
         * @brief Verifies that the ailment executor continuously transmits cure commands
         * while the monitoring routine is active
         * 
         * When the macro system is actively monitoring for status ailments, the executor
         * thread must continuously process detections and send the appropriate cure
         * commands (all-cure key presses, alternating arrow keys, or stop signals) to
         * the game. This test ensures that once started, the thread repeatedly calls
         * the transmit method to process ailments without stopping.
         */
        private void _testExecutorThreadLoopTransmitsWhenStarted()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)AilmentExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Ailment
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var start = AilmentOrchestratorThreadInjectType.Start;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(start, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == i);
            }
        }

        /**
         * @brief Verifies that cure keystrokes stop immediately when ailment monitoring
         * is stopped or interrupted
         * 
         * When the ailment monitoring routine is stopped (e.g., bot shutdown, critical
         * error, or user intervention), the executor thread must stop sending cure
         * keystrokes immediately. This prevents the bot from continuing to press cure
         * keys after monitoring has ended, which could interfere with manual control.
         */
        private void _testExecutorThreadLoopDoesntTransmitWhenStopped()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)AilmentExecutorThreadedUpdate.Started,
                    KeystrokeTransmitterThreadType.Ailment
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var stop = AilmentOrchestratorThreadInjectType.Stop;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(stop, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == 0);
            }
        }

        /**
         * @brief Verifies that the executor thread helper is reset before and after each
         * ailment detection cycle to ensure clean state for status processing
         * 
         * When the ailment executor processes status detection, the thread helper must
         * be reset to a clean state before checking for ailments and sending cure
         * commands. This prevents stale detection data from previous cycles from
         * affecting the current cure decision. After the transmission completes, the
         * helper is reset again to clear any pending state, ensuring the next detection
         * cycle starts fresh without leftover results.
         */
        private void _testExecutorThreadLoopResetsBeforeAndAfterTransmit()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)AilmentExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Ailment
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var callOrder = _executorThreadHelper.CallOrder;
                _transmittingState.IsRunningReturn.Add(false);
                var start = AilmentOrchestratorThreadInjectType.Start;
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


    public class AilmentOrchestratorThreadTests
    {
        private AbstractKeystrokeTransmitterThreadState _threadState = new KeystrokeTransmitterThreadState(
            0, KeystrokeTransmitterThreadType.Ailment
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
            return new AilmentOrchestratorThread(
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
                0, KeystrokeTransmitterThreadType.Ailment
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
                0, KeystrokeTransmitterThreadType.Ailment
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
                123, KeystrokeTransmitterThreadType.Ailment
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            var max = AilmentOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                transmitterOrchestratorThread.Inject(
                    (AilmentOrchestratorThreadInjectType)i, 0
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
            var max = AilmentOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var transmitterOrchestratorThread = _fixture(threadState);
                transmitterOrchestratorThread.Inject((AilmentOrchestratorThreadInjectType)i, 0);
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
                    123, KeystrokeTransmitterThreadType.Ailment
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


    public class AilmentTransmitterTestSuite
    {
        public void Run()
        {
            new AllCureExecutorThreadHandlerTests().Run();
            new ArrowKeysExecutorThreadHandlerTests().Run();
            new AilmentExecutorThreadHelperTests().Run();
            new AilmentExecutorThreadTests().Run();
            new AilmentOrchestratorThreadTests().Run();
        }
    }
}
