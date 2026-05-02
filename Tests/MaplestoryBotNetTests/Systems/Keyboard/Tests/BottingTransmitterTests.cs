using MaplestoryBotNet.Systems;
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
using System.Drawing;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests
{
    public class RandomBottingMacroCommandsSelectorTests
    {
        private List<MinimapPointMacros> _minimapPointMacros = [];

        private MockMacroRandom _macroRandom = new MockMacroRandom();

        private AbstractBottingMacroCommandsSelector _fixture(int macroCount)
        {
            _macroRandom = new MockMacroRandom();
            _minimapPointMacros = [];
            for (int i = 0; i < macroCount; i++)
            {
                _macroRandom.NextReturn.Add(10 + (20 * i));
                _minimapPointMacros.Add(
                    new MinimapPointMacros
                    {
                        MacroChance = 20,
                        MacroCommands = [i.ToString()]
                    }
                );
            }
            return new BottingRandomMacroCommandsSelector(_macroRandom);
        }

        /**
         * @brief Verifies that macro commands are selected based on random chance
         * 
         * When users configure multiple macro points with trigger probabilities,
         * the system should evaluate each point in order and randomly determine
         * whether to execute its commands. This test ensures the selection logic
         * correctly uses the random values to determine which macro run.
         */
        private void _testSelectMacroCommand()
        {
            var macroCount = 5;
            var macroCommandsSelector = _fixture(macroCount);
            for (int i = 0; i < macroCount; i++)
            {
                var commands = macroCommandsSelector.SelectMacroCommands(_minimapPointMacros);
                Debug.Assert(_minimapPointMacros[i].MacroCommands == commands);
                Debug.Assert(commands.Count == 1);
                Debug.Assert(commands[0] == i.ToString());
            }
        }

        public void Run()
        {
            _testSelectMacroCommand();
        }
    }


    public class BottingPointDataSelectorTests
    {
        private BottingModel _bottingModel = new BottingModel();

        private List<RectangleF> _minimapRects()
        {
            return [
                new RectangleF { X = -5.0f, Y= -5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  5.0f, Y= -5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X = -5.0f, Y=  5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  5.0f, Y=  5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  0.0f, Y=  0.0f, Width = 10.0f, Height = 10.0f}
            ];
        }

        private List<PointF> _closestPoints()
        {
            return [
                new PointF(1.0f, 1.0f),
                new PointF(9.0f, 1.0f),
                new PointF(1.0f, 9.0f),
                new PointF(9.0f, 9.0f),
                new PointF(4.0f, 5.0f)
            ];
        }

        private AbstractBottingPointDataSelector _fixture()
        {
            _bottingModel = new BottingModel();
            var minimapRects = _minimapRects();
            for (int i = 0; i < minimapRects.Count(); i++)
            {
                _bottingModel.GetMacroModel().AddMacroPoint(
                    new MinimapPoint
                    {
                        X = minimapRects[i].X,
                        Y = minimapRects[i].Y,
                        XRange = minimapRects[i].Width,
                        YRange = minimapRects[i].Height,
                        PointData = new MinimapPointData{ElementName = i.ToString()},
                    }
                );
            }
            return new BottingPointDataSelector("some key");
        }

        /**
         * @brief Verifies that the closest minimap point is correctly identified
         * 
         * When users move around the game world, the system should always select
         * the nearest predefined point based on the player's current position.
         * This test ensures the distance calculation works correctly across
         * different regions of the minimap.
         */
        private void _testSelectPointSelectsClosestPoint()
        {
            var closestPoints = _closestPoints();
            var pointDataSelector = _fixture();
            for (int i = 0; i < closestPoints.Count(); i++)
            {
                var (pX, pY) = ((int)closestPoints[i].X, (int)closestPoints[i].Y);
                _bottingModel.GetMapModel().SetTemplatePosition("some key", pX, pY);
                var result = pointDataSelector.SelectPoint(_bottingModel);
                Debug.Assert(result!.ElementName == i.ToString());
            }
        }

        public void Run()
        {
            _testSelectPointSelectsClosestPoint();
        }
    }


    public class BottingExecutorThreadHelperTests
    {
        private MockMacroCommandsExecutorBuilder _executorBuilder = new MockMacroCommandsExecutorBuilder();

        private MockMacroCommandsExecutor _executor = new MockMacroCommandsExecutor();

        private AbstractKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private BottingModel _bottingModel = new BottingModel();

        private List<RectangleF> _minimapRects()
        {
            return [
                new RectangleF { X = -5.0f, Y= -5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  5.0f, Y= -5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X = -5.0f, Y=  5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  5.0f, Y=  5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  0.0f, Y=  0.0f, Width = 10.0f, Height = 10.0f}
            ];
        }

        private List<PointF> _closestPoints()
        {
            return [
                new PointF(1.0f, 1.0f),
                new PointF(9.0f, 1.0f),
                new PointF(1.0f, 9.0f),
                new PointF(9.0f, 9.0f),
                new PointF(4.0f, 5.0f)
            ];
        }

        private AbstractKeystrokeTransmitterThreadHelper _fixture()
        {
            _executorBuilder = new MockMacroCommandsExecutorBuilder();
            _executor = new MockMacroCommandsExecutor();
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _bottingModel = new BottingModel();
            var minimapRects = _minimapRects();
            for (int i = 0; i < minimapRects.Count(); i++)
            {
                _bottingModel.GetMacroModel().AddMacroPoint(
                    new MinimapPoint
                    {
                        X = minimapRects[i].X,
                        Y = minimapRects[i].Y,
                        XRange = minimapRects[i].Width,
                        YRange = minimapRects[i].Height,
                        PointData = new MinimapPointData
                        {
                            ElementName = i.ToString(),
                            Commands = [
                                new MinimapPointMacros
                                {
                                    MacroChance = 20,
                                    MacroCommands = [i.ToString()]
                                }
                            ]
                        }
                    }
                );
            }
            _executorBuilder.BuildReturn.Add(_executor);
            return new BottingExecutorThreadHelper(
                new BottingPointDataSelector("some key"),
                new BottingRandomMacroCommandsSelector(new MacroRandom()),
                _executorBuilder
            );
        }

        /**
         * @brief Verifies that the executor is built when a keystroke transmitter is injected
         * 
         * When users start using keyboard automation, the system needs to create
         * an executor that can run macro commands. This test ensures that the
         * executor is properly built once the keystroke transmitter is available.
         */
        private void _testInjectingBottingBuildsExecutor()
        {
            var executorThreadHelper = _fixture();
            executorThreadHelper.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            Debug.Assert(_executorBuilder.WithArgCalls == 1);
            Debug.Assert(_executorBuilder.WithArgCallArg_arg[0] == _keystrokeTransmitter);
            Debug.Assert(_executorBuilder.BuildCalls == 1);
        }

        /**
         * @brief Verifies that the correct macro commands execute based on player position
         * 
         * When users move to different locations in the game, the system should
         * automatically select and execute the appropriate macro commands for
         * that area. This test ensures that the current position correctly
         * determines which macros run.
         */
        private void _testTransmitExecutesSelectedMacroCommands()
        {
            var closestPoints = _closestPoints();
            for (int i = 0; i < closestPoints.Count(); i++)
            {
                var executorThreadHelper = _fixture();
                var (pX, pY) = ((int) closestPoints[i].X, (int) closestPoints[i].Y);
                _bottingModel.GetMapModel().SetTemplatePosition("some key", pX, pY);
                executorThreadHelper.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
                executorThreadHelper.Inject(SystemInjectType.BottingModel, _bottingModel);
                executorThreadHelper.Transmit();
                Debug.Assert(_executor.ExecuteCalls == 1);
                Debug.Assert(_executor.ExecuteCallArg_macroCommands.Count == 1);
                Debug.Assert(_executor.ExecuteCallArg_macroCommands[0].Count == 1);
                Debug.Assert(_executor.ExecuteCallArg_macroCommands[0][0] == i.ToString());
            }
        }

        public void Run()
        {
            _testInjectingBottingBuildsExecutor();
            _testTransmitExecutesSelectedMacroCommands();
        }
    }
    

    public class BottingExecutorThreadTests
    {
        private MockKeystrokeTransmitterThreadHelper _executorThreadHelper = (
            new MockKeystrokeTransmitterThreadHelper()
        );

        private MockResetEvent _executionEvent = new MockResetEvent();

        private MockRunningState _transmittingState = new MockRunningState();

        private MockRunningState _runningState = new MockRunningState();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)BottingExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Botting
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
                        BottingOrchestratorThreadInjectType.Stop, null
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
            return new BottingExecutorThread(
                _executionEvent,
                _executorThreadHelper,
                _threadState,
                _transmittingState,
                _runningState
            );
        }

        /**
         * @brief Verifies the handshake sequence when the botting executor starts its
         * monster-killing transmission routine
         * 
         * When the macro system determines that the character should begin killing
         * monsters on the map, the botting orchestrator signals the executor to start
         * its transmission routine. The executor performs a coordinated startup handshake
         * with the botting executor to ensure that transmission is ready.
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
                keystrokeTransmitterExecutorThread.Inject(BottingOrchestratorThreadInjectType.Start, 0);
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
         * @brief Verifies thread state changes correctly during startup
         * 
         * When users start automation, the thread transitions through proper states:
         * Starting -> Started. This test ensures the thread correctly updates its
         * state so the rest of the system knows what it's doing.
         */
        private void _testExecutorStartingHandshakeSetsThreadStates()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            _transmittingState.IsRunningReturn.Add(false);
            keystrokeTransmitterExecutorThread.Inject(
                BottingOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(threadState.SetStateCallArg_state[0] == (int)BottingExecutorThreadedUpdate.Starting);
            Debug.Assert(threadState.SetStateCallArg_state[1] == (int)BottingExecutorThreadedUpdate.Started);
        }

        /**
         * @brief Verifies the handshake sequence when the botting executor stops its
         * monster-killing transmission routine
         * 
         * When the macro system needs to switch to a different transmission routine
         * (such as navigating to a rune or solving the rune puzzle), the orchestrator
         * signals the executor to stop its current routine. The executor performs a
         * coordinated shutdown handshake to ensure keystrokes stop cleanly before
         * the routine exits.
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
                keystrokeTransmitterExecutorThread.Inject(BottingOrchestratorThreadInjectType.Stop, 0);
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
         * @brief Verifies thread state changes correctly during shutdown
         * 
         * When users stop automation, the thread transitions from Started → Stopping
         * -> Stopped. This test ensures the thread correctly reports its state during
         * shutdown for proper system coordination.
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
                Debug.Assert(threadState.SetStateCallArg_state[0] == (int)BottingExecutorThreadedUpdate.Stopping);
                Debug.Assert(threadState.SetStateCallArg_state[1] == (int)BottingExecutorThreadedUpdate.Stopped);
            }
        }

        /**
         * @brief Verifies macros execute continuously while automation runs
         * 
         * When users have automation running, the thread should continuously process
         * macro commands based on their location. This test ensures that once started,
         * the thread repeatedly checks the player's position and executes the
         * appropriate macros without stopping.
         */
        private void _testExecutorThreadLoopTransmitsWhenStarted()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)BottingExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Botting
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var start = BottingOrchestratorThreadInjectType.Start;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(start, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == i);
            }
        }

        /**
         * @brief Verifies macros stop executing when automation is stopped
         * 
         * When users stop automation, the thread should immediately stop executing
         * macros. This test ensures that after a stop command, no further macros
         * are executed even if the player continues moving.
         */
        private void _testExecutorThreadLoopDoesntTransmitWhenStopped()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)BottingExecutorThreadedUpdate.Started,
                    KeystrokeTransmitterThreadType.Botting
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var stop = BottingOrchestratorThreadInjectType.Stop;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(stop, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == 0);
            }
        }

        /**
         * @brief Verifies that the executor thread helper is reset before and after each
         * transmission cycle to ensure clean state for the next macro execution
         * 
         * When the botting executor processes macros for killing monsters, the thread
         * helper must be reset to a clean state before executing keystroke transmissions
         * for the current character position. This prevents stale data from previous
         * macro executions from affecting the current transmission.
         */
        private void _testExecutorThreadLoopResetsBeforeAndAfterTransmit()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)BottingExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Botting
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var callOrder = _executorThreadHelper.CallOrder;
                _transmittingState.IsRunningReturn.Add(false);
                var start = BottingOrchestratorThreadInjectType.Start;
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


    public class BottingOrchestratorThreadTests
    {
        private AbstractKeystrokeTransmitterThreadState _threadState = new KeystrokeTransmitterThreadState(
            0, KeystrokeTransmitterThreadType.Botting
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
            return new BottingOrchestratorThread(
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
                0, KeystrokeTransmitterThreadType.Botting
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
                0, KeystrokeTransmitterThreadType.Botting
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
                123, KeystrokeTransmitterThreadType.Botting
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            var max = BottingOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                transmitterOrchestratorThread.Inject(
                    (BottingOrchestratorThreadInjectType)i, 0
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
            var max = BottingOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var transmitterOrchestratorThread = _fixture(threadState);
                transmitterOrchestratorThread.Inject((BottingOrchestratorThreadInjectType)i, 0);
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
                    123, KeystrokeTransmitterThreadType.Botting
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



    public class BottingTransmitterTestSuite
    {
        public void Run()
        {
            new RandomBottingMacroCommandsSelectorTests().Run();
            new BottingPointDataSelectorTests().Run();
            new BottingExecutorThreadHelperTests().Run();
            new BottingExecutorThreadTests().Run();
            new BottingOrchestratorThreadTests().Run();
        }
    }
}
