using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.Systems.ScreenCapture.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using System.Diagnostics;
using System.Drawing;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests
{
    public class RandomMacroCommandsSelectorTests
    {
        private List<MinimapPointMacros> _minimapPointMacros = [];

        private MockMacroRandom _macroRandom = new MockMacroRandom();

        private AbstractMacroCommandsSelector _fixture(int macroCount)
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
            return new RandomMacroCommandsSelector(_macroRandom);
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


    public class KeystrokeTransmitterPointDataSelectorTests
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

        private AbstractPointDataSelector _fixture()
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
            return new KeystrokeTransmitterPointDataSelector("some key");
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


    public class KeystrokeTransmitterExecutorThreadHelperTests
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

        private AbstractKeystrokeTransmitterExecutorThreadHelper _fixture()
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
            return new KeystrokeTransmitterExecutorThreadHelper(
                new KeystrokeTransmitterPointDataSelector("some key"),
                new RandomMacroCommandsSelector(new MacroRandom()),
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
        private void _testInjectingKeystrokeTransmitterBuildsExecutor()
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
            _testInjectingKeystrokeTransmitterBuildsExecutor();
            _testTransmitExecutesSelectedMacroCommands();
        }
    }
    

    public class KeystrokeTransmitterExecutorThreadTests
    {
        private MockCountDown _receiveCountDown = new MockCountDown();

        private MockCountDown _transmitCountDown = new MockCountDown();

        private MockKeystrokeTransmitterExecutorThreadHelper _executorThreadHelper = (
            new MockKeystrokeTransmitterExecutorThreadHelper()
        );

        private MockRunningState _runningState = new MockRunningState();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int) KeystrokeTransmitterExecutorThreadedUpdate.Stopped
            )
        );

        private MockInjectAction _injectAction = new MockInjectAction();

        private List<string> _callOrder = [];

        private string _executorThreadHelperRef = "";

        private string _transmitCountDownRef = "";

        private string _receiveCountDownRef = "";

        private string _threadStateRef = "";

        private string _injectActionRef = "";

        private void _setupNewFixture(
            AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _receiveCountDown = new MockCountDown();
            _transmitCountDown = new MockCountDown();
            _executorThreadHelper = new MockKeystrokeTransmitterExecutorThreadHelper();
            _runningState = new MockRunningState();
            _callOrder = [];
            _injectAction = new MockInjectAction();
            _threadState = threadState;
        }

        private void _setupReferences()
        {
            _transmitCountDownRef = new TestUtilities().Reference(_transmitCountDown);
            _receiveCountDownRef = new TestUtilities().Reference(_receiveCountDown);
            _threadStateRef = new TestUtilities().Reference(_threadState);
            _injectActionRef = new TestUtilities().Reference(_injectAction);
            _executorThreadHelperRef = new TestUtilities().Reference(_executorThreadHelper);
        }

        private void _setupCallOrder()
        {
            _transmitCountDown.CallOrder = _callOrder;
            _receiveCountDown.CallOrder = _callOrder;
            if (_threadState is MockKeystrokeTransmitterThreadState mockThreadState)
            {
                mockThreadState.CallOrder = _callOrder;
            }
            _injectAction.CallOrder = _callOrder;
        }

        private void _setupRunningState()
        {
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(true);
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
                        KeystrokeTransmitterOrchestratorThreadInjectType.Stop, null
                    );
                },
                () =>
                {
                    abstractThread.Stop();
                }
            ];
        }

        private AbstractThread _fixture(
            int transmitCount, AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _setupNewFixture(threadState);
            _setupReferences();
            _setupCallOrder();
            _setupRunningState();
            _setupTransmit(transmitCount);
            return new KeystrokeTransmitterExecutorThread(
                _receiveCountDown,
                _transmitCountDown,
                _executorThreadHelper,
                _threadState,
                _runningState
            );
        }

        private void _setupInjectAction(
            AbstractThread thread, Action<object, object> injectAction
        )
        {
            _injectAction.GetActionReturn.Add(injectAction);
            _injectAction.GetActionReturn.Add(injectAction);
            thread.Inject(
                SystemInjectType.InjectAction, _injectAction
            );
        }

        /**
         * @brief Verifies the startup sequence when automation begins
         * 
         * When users start their automation, the system needs to properly initialize
         * the macro execution thread. This test ensures the thread performs the
         * correct startup handshake, including setting up counters, and updating state.
         */
        private void _testExecutorStartingHandshake()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            var injectAction = (object dataType, object data) =>
            {
                var callReference = new TestUtilities().Reference(_injectAction) + "ActionCall";
                _callOrder.Add(callReference);
            };
            _setupInjectAction(keystrokeTransmitterExecutorThread, injectAction);
            Debug.Assert(_callOrder.Count == 0);
            keystrokeTransmitterExecutorThread.Inject(
                KeystrokeTransmitterOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(_callOrder.Count == 10);
            Debug.Assert(_callOrder[0] == _transmitCountDownRef + "SetCountDown");
            Debug.Assert(_callOrder[1] == _threadStateRef + "SetState");
            Debug.Assert(_callOrder[2] == _injectActionRef + "GetAction");
            Debug.Assert(_callOrder[3] == _injectActionRef + "ActionCall");
            Debug.Assert(_callOrder[4] == _receiveCountDownRef + "CountDown");
            Debug.Assert(_callOrder[5] == _transmitCountDownRef + "WaitCountDown");
            Debug.Assert(_callOrder[6] == _threadStateRef + "SetState");
            Debug.Assert(_callOrder[7] == _injectActionRef + "GetAction");
            Debug.Assert(_callOrder[8] == _injectActionRef + "ActionCall");
            Debug.Assert(_callOrder[9] == _receiveCountDownRef + "CountDown");
        }

        /**
         * @brief Verifies the shutdown sequence when automation stops
         * 
         * When users stop their automation, the macro execution thread needs to
         * shut down cleanly. This test ensures the thread properly stops its loop,
         * updates its state, and releases resources.
         */
        private void _testExecutorStoppingHandshake()
        {
            for (int i = 0; i < 2; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                var stopLambdas = _stopLambdas(keystrokeTransmitterExecutorThread);
                var injectAction = (object dataType, object data) =>
                {
                    var callReference = new TestUtilities().Reference(_injectAction) + "ActionCall";
                    _callOrder.Add(callReference);
                };
                _setupInjectAction(keystrokeTransmitterExecutorThread, injectAction);
                Debug.Assert(_callOrder.Count == 0);
                stopLambdas[i]();
                Debug.Assert(_callOrder.Count == 9);
                Debug.Assert(_callOrder[0] == _transmitCountDownRef + "SetCountDown");
                Debug.Assert(_callOrder[1] == _threadStateRef + "SetState");
                Debug.Assert(_callOrder[2] == _injectActionRef + "GetAction");
                Debug.Assert(_callOrder[3] == _injectActionRef + "ActionCall");
                Debug.Assert(_callOrder[4] == _receiveCountDownRef + "CountDown");
                Debug.Assert(_callOrder[5] == _transmitCountDownRef + "WaitCountDown");
                Debug.Assert(_callOrder[6] == _threadStateRef + "SetState");
                Debug.Assert(_callOrder[7] == _injectActionRef + "GetAction");
                Debug.Assert(_callOrder[8] == _injectActionRef + "ActionCall");
            }
        }

        /**
         * @brief Confirms the startup handshake uses a single countdown
         * 
         * When starting automation, the system uses a countdown mechanism to
         * coordinate between threads. This test ensures the countdown is correctly
         * set to 1, meaning the thread will wait for exactly one signal before
         * beginning macro execution.
         */
        private void _testExecutorStartingHandshakeSetsCountDownToOne()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            keystrokeTransmitterExecutorThread.Inject(
                KeystrokeTransmitterOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(_transmitCountDown.SetCountDownCallArg_countDown[0] == 1);
        }

        /**
         * @brief Confirms the shutdown handshake uses a single countdown
         * 
         * When stopping automation, the system uses a countdown to coordinate the
         * thread shutdown. This test ensures the countdown is correctly set to 1,
         * allowing the thread to complete the final operation before stopping.
         */
        private void _testExecutorStoppingHandshakeSetsCountDownToOne()
        {
            for (int i = 0; i < 2; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                var stopLambdas = _stopLambdas(keystrokeTransmitterExecutorThread);
                stopLambdas[i]();
                Debug.Assert(_transmitCountDown.SetCountDownCallArg_countDown[0] == 1);
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
            keystrokeTransmitterExecutorThread.Inject(
                KeystrokeTransmitterOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(threadState.SetStateCallArg_state[0] == (int)KeystrokeTransmitterExecutorThreadedUpdate.Starting);
            Debug.Assert(threadState.SetStateCallArg_state[1] == (int)KeystrokeTransmitterExecutorThreadedUpdate.Started);
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
                var stopLambdas = _stopLambdas(keystrokeTransmitterExecutorThread);
                stopLambdas[i]();
                Debug.Assert(threadState.SetStateCallArg_state[0] == (int)KeystrokeTransmitterExecutorThreadedUpdate.Stopping);
                Debug.Assert(threadState.SetStateCallArg_state[1] == (int)KeystrokeTransmitterExecutorThreadedUpdate.Stopped);
            }
        }

        /**
         * @brief Confirms status updates are broadcast during startup
         * 
         * When starting automation, the system broadcasts its status so other
         * components know what's happening. This test ensures the thread sends
         * the proper notifications when it's starting and after it's started.
         */
        private void _testExecutorStartingHandshakeInjectsThreadStates()
        {
            var dataTypeList = new List<object>();
            var injectAction = (object dataType, object data) =>
            {
                dataTypeList.Add(dataType);
            };
            var threadState = new MockKeystrokeTransmitterThreadState();
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            _setupInjectAction(keystrokeTransmitterExecutorThread, injectAction);
            keystrokeTransmitterExecutorThread.Inject(
                KeystrokeTransmitterOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(dataTypeList.Count == 2);
            Debug.Assert(dataTypeList[0] is KeystrokeTransmitterExecutorThreadedUpdate.Starting);
            Debug.Assert(dataTypeList[1] is KeystrokeTransmitterExecutorThreadedUpdate.Started);
        }

        /**
         * @brief Confirms status updates are broadcast during shutdown
         * 
         * When stopping automation, the system broadcasts its status so other
         * components know it's shutting down. This test ensures the thread sends
         * proper notifications during the stopping process.
         */
        private void _testExecutorStoppingHandshakeInjectsThreadStates()
        {
            for (int i = 0; i < 2; i++)
            {
                var dataTypeList = new List<object>();
                var injectAction = (object dataType, object data) =>
                {
                    dataTypeList.Add(dataType);
                };
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                var stopLambdas = _stopLambdas(keystrokeTransmitterExecutorThread);
                _setupInjectAction(keystrokeTransmitterExecutorThread, injectAction);
                stopLambdas[i]();
                Debug.Assert(dataTypeList.Count == 2);
                Debug.Assert(dataTypeList[0] is KeystrokeTransmitterExecutorThreadedUpdate.Stopping);
                Debug.Assert(dataTypeList[1] is KeystrokeTransmitterExecutorThreadedUpdate.Stopped);
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
                    (int) KeystrokeTransmitterExecutorThreadedUpdate.Stopped
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                keystrokeTransmitterExecutorThread.Inject(
                    KeystrokeTransmitterOrchestratorThreadInjectType.Start, 0
                );
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
                    (int) KeystrokeTransmitterExecutorThreadedUpdate.Started
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                keystrokeTransmitterExecutorThread.Inject(
                    KeystrokeTransmitterOrchestratorThreadInjectType.Stop, 0
                );
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == 0);
            }
        }

        /**
         * @brief Verifies the complete thread loop coordination
         * 
         * This test ensures the thread's main loop properly coordinates all its
         * activities: waiting for commands, executing macros, and handling the
         * coordination between the orchestrator thread and the macro execution thread.
         */
        private void _testExecutorThreadLoopHandshake()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int) KeystrokeTransmitterExecutorThreadedUpdate.Started
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                keystrokeTransmitterExecutorThread.Inject(
                    KeystrokeTransmitterOrchestratorThreadInjectType.Start, 0
                );
                _callOrder.Clear();
                _executorThreadHelper.CallOrder = _callOrder;
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_callOrder.Count == 3 + i);
                Debug.Assert(_callOrder[0] == _receiveCountDownRef + "WaitCountDown");
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(_callOrder[j + 1] == _executorThreadHelperRef + "Transmit");
                }
                Debug.Assert(_callOrder[i + 1] == _receiveCountDownRef + "SetCountDown");
                Debug.Assert(_callOrder[i + 2] == _transmitCountDownRef + "CountDown");
            }
        }

        /**
         * @brief Confirms the thread loop uses a single countdown for coordination
         * 
         * When the thread loop is running, it uses a countdown to know when to
         * check for new commands. This test ensures the countdown is correctly
         * set to 1 for proper thread coordination.
         */
        private void _testExecutorThreadLoopHandshakeSetsCountDownToOne()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                (int)KeystrokeTransmitterExecutorThreadedUpdate.Started
            );
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            keystrokeTransmitterExecutorThread.Inject(
                KeystrokeTransmitterOrchestratorThreadInjectType.Start, 0
            );
            _callOrder.Clear();
            keystrokeTransmitterExecutorThread.Start();
            keystrokeTransmitterExecutorThread.Join(10000);
            Debug.Assert(_receiveCountDown.SetCountDownCallArg_countDown[0] == 1);
        }

        public void Run()
        {
            _testExecutorStartingHandshake();
            _testExecutorStoppingHandshake();
            _testExecutorStartingHandshakeSetsCountDownToOne();
            _testExecutorStoppingHandshakeSetsCountDownToOne();
            _testExecutorStartingHandshakeSetsThreadStates();
            _testExecutorStoppingHandshakeSetsThreadStates();
            _testExecutorStartingHandshakeInjectsThreadStates();
            _testExecutorStoppingHandshakeInjectsThreadStates();
            _testExecutorThreadLoopHandshake();
            _testExecutorThreadLoopHandshakeSetsCountDownToOne();
            _testExecutorThreadLoopTransmitsWhenStarted();
            _testExecutorThreadLoopDoesntTransmitWhenStopped();
        }
    }


    public class KeystrokeTransmitterOrchestratorThreadTests
    {
        private AbstractKeystrokeTransmitterThreadState _threadState = new KeystrokeTransmitterThreadState(0);

        private MockCountDown _receiveCountDown = new MockCountDown();

        private MockThread _thread = new MockThread(new ThreadRunningState());

        private MockRunningState _runningState = new MockRunningState();

        private string _threadStateRef = "";

        private string _receiveCountDownRef = "";

        private string _threadRef = "";

        private List<string> _callOrder = [];

        private AbstractThread _fixture(AbstractKeystrokeTransmitterThreadState threadState)
        {
            _threadState = threadState;
            _receiveCountDown = new MockCountDown();
            _thread = new MockThread(new ThreadRunningState());
            _runningState = new MockRunningState();
            _callOrder = [];
            _receiveCountDown.CallOrder = _callOrder;
            _thread.CallOrder = _callOrder;
            if (_threadState is MockKeystrokeTransmitterThreadState mockThreadState)
            {
                mockThreadState.CallOrder = _callOrder;
            }
            _threadStateRef = new TestUtilities().Reference(_threadState);
            _receiveCountDownRef = new TestUtilities().Reference(_receiveCountDown);
            _threadRef = new TestUtilities().Reference(_thread);
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
            return new KeystrokeTransmitterOrchestratorThread(
                _threadState,
                _receiveCountDown,
                _thread,
                _runningState
            );
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
            var threadState = new KeystrokeTransmitterThreadState(0);
            var transmitterOrchestratorThread = _fixture(threadState);
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
            var threadState = new KeystrokeTransmitterThreadState(0);
            var transmitterOrchestratorThread = _fixture(threadState);
            _thread.CallOrder = _callOrder;
            _receiveCountDown.CallOrder = _callOrder;
            transmitterOrchestratorThread.Stop();
            Debug.Assert(_callOrder.Count == 2);
            Debug.Assert(_callOrder[0] == _threadRef + "ThreadStop");
            Debug.Assert(_callOrder[1] == _receiveCountDownRef + "CountDown");
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
            var threadState = new KeystrokeTransmitterThreadState(123);
            var transmitterOrchestratorThread = _fixture(threadState);
            var max = KeystrokeTransmitterOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                transmitterOrchestratorThread.Inject(
                    (KeystrokeTransmitterOrchestratorThreadInjectType)i, 0
                );
                Debug.Assert(_threadState.GetState() == i);
            }
        }

        /**
         * @brief Confirms the orchestrator properly acknowledges commands
         * 
         * When commands are sent to the orchestrator, it should acknowledge them
         * by updating its state and signaling that the command was received.
         * This test ensures the orchestrator properly handles the command handshake.
         */
        private void _testInjectOrchestratorCommandHandshake()
        {
            var max = KeystrokeTransmitterOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var transmitterOrchestratorThread = _fixture(threadState);
                transmitterOrchestratorThread.Inject(
                    (KeystrokeTransmitterOrchestratorThreadInjectType)i, 0
                );
                Debug.Assert(_thread.InjectCalls == 0);
                Debug.Assert(_callOrder.Count == 2);
                Debug.Assert(_callOrder[0] == _threadStateRef + "SetState");
                Debug.Assert(_callOrder[1] == _receiveCountDownRef + "CountDown");
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
        private void _testThreadLoopHandshake()
        {
            var threadState = new KeystrokeTransmitterThreadState(123);
            var transmitterOrchestratorThread = _fixture(threadState);
            transmitterOrchestratorThread.Start();
            transmitterOrchestratorThread.Join(10000);
            Debug.Assert(_callOrder.Count == 4);
            Debug.Assert(_callOrder[0] == _threadRef + "ThreadStart");
            Debug.Assert(_callOrder[1] == _receiveCountDownRef + "WaitCountDown");
            Debug.Assert(_callOrder[2] == _threadRef + "ThreadInject");
            Debug.Assert(_callOrder[3] == _receiveCountDownRef + "SetCountDown");
        }

        /**
         * @brief Confirms the thread loop uses proper countdown coordination
         * 
         * The orchestrator uses a countdown mechanism to coordinate with th executor
         * thread. This test ensures the countdown is correctly configured to
         * manage the thread coordination properly.
         */
        private void _testThreadLoopSetsCountdownToOne()
        {
            var threadState = new KeystrokeTransmitterThreadState(123);
            var transmitterOrchestratorThread = _fixture(threadState);
            transmitterOrchestratorThread.Start();
            transmitterOrchestratorThread.Join(10000);
            Debug.Assert(_receiveCountDown.SetCountDownCallArg_countDown[0] == 1);
        }

        /**
         * @brief Verifies the orchestrator sends its current state to the executor
         * 
         * When the orchestrator is running, it should inform the executor
         * thread what state it should be in. This test ensures the current thread
         * state is properly injected into the executor so it knows what to do.
         */
        private void _testThreadLoopInjectsCurrentThreadState()
        {
            var threadState = new KeystrokeTransmitterThreadState(123);
            var transmitterOrchestratorThread = _fixture(threadState);
            transmitterOrchestratorThread.Start();
            transmitterOrchestratorThread.Join(10000);
            Debug.Assert((int)_thread.InjectCallArg_dataType[0] == 123);
            Debug.Assert(_thread.InjectCallArg_data[0] == null);
        }

        public void Run()
        {
            _testStartingOrchestratorStartsExecutorThread();
            _testStoppingOrchestratorStopsExecutorThread();
            _testInjectingOrchestratorCommandAssignsThreadState();
            _testInjectOrchestratorCommandHandshake();
            _testInjectToExecutorThread();
            _testInjectActionToExecutorThread();
            _testThreadLoopHandshake();
            _testThreadLoopSetsCountdownToOne();
            _testThreadLoopInjectsCurrentThreadState();
        }
    }


    public class KeystrokeTransmitterTestSuite
    {
        public void Run()
        {
            new RandomMacroCommandsSelectorTests().Run();
            new KeystrokeTransmitterPointDataSelectorTests().Run();
            new KeystrokeTransmitterExecutorThreadHelperTests().Run();
            new KeystrokeTransmitterExecutorThreadTests().Run();
            new KeystrokeTransmitterOrchestratorThreadTests().Run();
        }
    }
}
