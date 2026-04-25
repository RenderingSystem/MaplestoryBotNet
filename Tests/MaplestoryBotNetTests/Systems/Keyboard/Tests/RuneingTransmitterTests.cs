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
using System.Windows;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests
{
    public class RuneingExecutorThreadHelperTests
    {
        private MockMacroCommandsExecutorBuilder _executorBuilder = new MockMacroCommandsExecutorBuilder();

        private MockMacroCommandsExecutor _executor = new MockMacroCommandsExecutor();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private AbstractKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private AbstractKeystrokeTransmitterThreadHelper _fixture()
        {
            _executorBuilder = new MockMacroCommandsExecutorBuilder();
            _executor = new MockMacroCommandsExecutor();
            _executorBuilder.BuildReturn.Add(_executor);
            _bottingModel = new BottingModel();
            var helper = new RuneingExecutorThreadHelper(
                MapIconInfo.Character,
                MapIconInfo.Rune,
                _executorBuilder
            );
            helper.Inject(SystemInjectType.BottingModel, _bottingModel);
            helper.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            return helper;
        }

        private List<RuneFrameMacro> _runeFrameMacrosFixture(int i, int j)
        {
            var runeFrameMacros = new List<RuneFrameMacro>();
            var prefix = ((j * 3) + i).ToString();
            if (i - 1 > -1)
            {
                runeFrameMacros.Add(
                    new RuneFrameMacro
                    {
                        X = 10,
                        Y = 50,
                        ScaleX = 100,
                        ScaleY = 100,
                        PointCommands = [
                            "M" + prefix + "<1",
                            "M" + prefix + "<2",
                            "M" + prefix + "<3",
                        ],
                        Radius = 2
                    }
                );
            }
            if (j - 1 > -1)
            {
                runeFrameMacros.Add(
                    new RuneFrameMacro 
                    {
                        X = 50,
                        Y = 10,
                        ScaleX = 100,
                        ScaleY = 100,
                        PointCommands = [
                            "M" + prefix + "^1",
                            "M" + prefix + "^2",
                            "M" + prefix + "^3",
                        ],
                        Radius = 2
                    }
                );
            }
            if (i + 1 < 3)
            {
                runeFrameMacros.Add(
                    new RuneFrameMacro
                    {
                        X = 90,
                        Y = 50,
                        ScaleX = 100,
                        ScaleY = 100,
                        PointCommands = [
                            "M" + prefix + ">1",
                            "M" + prefix + ">2",
                            "M" + prefix + ">3",
                        ],
                        Radius = 2
                    }
                );
            }
            if (j + 1 < 3)
            {
                runeFrameMacros.Add(
                    new RuneFrameMacro
                    {
                        X = 50,
                        Y = 90,
                        ScaleX = 100,
                        ScaleY = 100,
                        PointCommands = [
                            "M" + prefix + "V1",
                            "M" + prefix + "V2",
                            "M" + prefix + "V3",
                        ],
                        Radius = 2
                    }
                );
            }
            return runeFrameMacros;
        }

        private List<RuneFrameDirection> _runeFrameDirectionsFixture(int i, int j)
        {
            var directions = new List<RuneFrameDirection>();
            var directionTypes = new[] { RuneFrameDirectionTypes.Left, RuneFrameDirectionTypes.Right };
            foreach (var direction in directionTypes)
            {
                var prefix = direction == RuneFrameDirectionTypes.Left ? "L" : "R";
                directions.Add(
                    new RuneFrameDirection
                    {
                        Direction = direction,
                        DirectionCommands = [
                            prefix + ((j * 3) + i).ToString() + "|1-0",
                            prefix + ((j * 3) + i).ToString() + "|1-1",
                            prefix + ((j * 3) + i).ToString() + "|1-2"
                        ],
                        Distance = 1
                    }
                );
                directions.Add(
                    new RuneFrameDirection
                    {
                        Direction = direction,
                        DirectionCommands = [
                            prefix + ((j * 3) + i).ToString() + "|10-0",
                            prefix + ((j * 3) + i).ToString() + "|10-1",
                            prefix + ((j * 3) + i).ToString() + "|10-2"
                        ],
                        Distance = 10
                    }
                );
                directions.Add(
                    new RuneFrameDirection
                    {
                        Direction = direction,
                        DirectionCommands = [
                            prefix + ((j * 3) + i).ToString() + "|20-0",
                            prefix + ((j * 3) + i).ToString() + "|20-1",
                            prefix + ((j * 3) + i).ToString() + "|20-2"
                        ],
                        Distance = 20
                    }
                );
            }
            return directions;
        }

        private void _runeFrameFixture()
        {
            var runeFrames = new List<RuneFrame>();
            for (int j = 0; j < 3; j++)
            for (int i = 0; i < 3; i++)
            {
                runeFrames.Add(
                    new RuneFrame
                    {
                        X = i * 100,
                        Y = j * 100,
                        Width = 100,
                        Height = 100,
                        FrameData = new RuneFrameData
                        {
                            FrameName = "F" + ((j * 3) + i).ToString(),
                            ElementLabel = "FT" + ((j * 3) + i).ToString(),
                            RuneFrameMacros = _runeFrameMacrosFixture(i, j),
                            RuneFrameDirections = _runeFrameDirectionsFixture(i, j),
                        },
                    }
                );
            }
            for (int j = 0; j < 3; j++)
            for (int i = 0; i < 3; i++)
            {
                var runeFrame = runeFrames[(j * 3) + i];
                var index = 0;
                if (i - 1 > -1)
                {
                    runeFrame.FrameData.RuneFrameMacros[index++].NextRuneFrame = (
                        runeFrames[(j * 3) + (i - 1)]
                    );
                }
                if (j - 1 > -1)
                {
                    runeFrame.FrameData.RuneFrameMacros[index++].NextRuneFrame = (
                        runeFrames[((j - 1) * 3) + i]
                    );
                }
                if (i + 1 < 3)
                {
                    runeFrame.FrameData.RuneFrameMacros[index++].NextRuneFrame = (
                        runeFrames[(j * 3) + (i + 1)]
                    );
                }
                if (j + 1 < 3)
                {
                    runeFrame.FrameData.RuneFrameMacros[index++].NextRuneFrame = (
                        runeFrames[((j + 1) * 3) + i]
                    );
                }
            }
            for (int i = 0; i < runeFrames.Count; i++)
            {
                _bottingModel.GetRuneModel().AddRuneFrame(runeFrames[i]);
            }
            _bottingModel.GetRuneModel().SetRadius(3);
        }

        private List<(Point, Point, List<string>)> _testCases()
        {
            return new List<(Point, Point, List<string>)>
            {
                (new Point(70, 50), new Point(250, 50), ["R0|20-0", "R0|20-1", "R0|20-2"]),
                (new Point(80, 50), new Point(250, 50), ["R0|10-0", "R0|10-1", "R0|10-2"]),
                (new Point(87, 50), new Point(250, 50), ["R0|1-0", "R0|1-1", "R0|1-2"]),
                (new Point(88, 50), new Point(250, 50), ["M0>1", "M0>2", "M0>3"]),
                (new Point(170, 50), new Point(250, 50), ["R1|20-0", "R1|20-1", "R1|20-2"]),
                (new Point(180, 50), new Point(250, 50), ["R1|10-0", "R1|10-1", "R1|10-2"]),
                (new Point(187, 50), new Point(250, 50), ["R1|1-0", "R1|1-1", "R1|1-2"]),
                (new Point(188, 50), new Point(250, 50), ["M1>1", "M1>2", "M1>3"]),
                (new Point(230, 50), new Point(250, 50), ["R2|20-0", "R2|20-1", "R2|20-2"]),
                (new Point(240, 50), new Point(250, 50), ["R2|10-0", "R2|10-1", "R2|10-2"]),
                (new Point(246, 50), new Point(250, 50), ["R2|1-0", "R2|1-1", "R2|1-2"]),
                (new Point(247, 50), new Point(250, 50), []),

                (new Point(70, 150), new Point(250, 150), ["R3|20-0", "R3|20-1", "R3|20-2"]),
                (new Point(80, 150), new Point(250, 150), ["R3|10-0", "R3|10-1", "R3|10-2"]),
                (new Point(87, 150), new Point(250, 150), ["R3|1-0", "R3|1-1", "R3|1-2"]),
                (new Point(88, 150), new Point(250, 150), ["M3>1", "M3>2", "M3>3"]),
                (new Point(170, 150), new Point(250, 150), ["R4|20-0", "R4|20-1", "R4|20-2"]),
                (new Point(180, 150), new Point(250, 150), ["R4|10-0", "R4|10-1", "R4|10-2"]),
                (new Point(187, 150), new Point(250, 150), ["R4|1-0", "R4|1-1", "R4|1-2"]),
                (new Point(188, 150), new Point(250, 150), ["M4>1", "M4>2", "M4>3"]),
                (new Point(230, 150), new Point(250, 150), ["R5|20-0", "R5|20-1", "R5|20-2"]),
                (new Point(240, 150), new Point(250, 150), ["R5|10-0", "R5|10-1", "R5|10-2"]),
                (new Point(246, 150), new Point(250, 150), ["R5|1-0", "R5|1-1", "R5|1-2"]),
                (new Point(247, 150), new Point(250, 150), []),

                (new Point(70, 250), new Point(250, 250), ["R6|20-0", "R6|20-1", "R6|20-2"]),
                (new Point(80, 250), new Point(250, 250), ["R6|10-0", "R6|10-1", "R6|10-2"]),
                (new Point(87, 250), new Point(250, 250), ["R6|1-0", "R6|1-1", "R6|1-2"]),
                (new Point(88, 250), new Point(250, 250), ["M6>1", "M6>2", "M6>3"]),
                (new Point(170, 250), new Point(250, 250), ["R7|20-0", "R7|20-1", "R7|20-2"]),
                (new Point(180, 250), new Point(250, 250), ["R7|10-0", "R7|10-1", "R7|10-2"]),
                (new Point(187, 250), new Point(250, 250), ["R7|1-0", "R7|1-1", "R7|1-2"]),
                (new Point(188, 250), new Point(250, 250), ["M7>1", "M7>2", "M7>3"]),
                (new Point(230, 250), new Point(250, 250), ["R8|20-0", "R8|20-1", "R8|20-2"]),
                (new Point(240, 250), new Point(250, 250), ["R8|10-0", "R8|10-1", "R8|10-2"]),
                (new Point(246, 250), new Point(250, 250), ["R8|1-0", "R8|1-1", "R8|1-2"]),
                (new Point(247, 250), new Point(250, 250), []),

                (new Point(230, 50), new Point(50, 50), ["L2|20-0", "L2|20-1", "L2|20-2"]),
                (new Point(220, 50), new Point(50, 50), ["L2|10-0", "L2|10-1", "L2|10-2"]),
                (new Point(213, 50), new Point(50, 50), ["L2|1-0", "L2|1-1", "L2|1-2"]),
                (new Point(212, 50), new Point(50, 50), ["M2<1", "M2<2", "M2<3"]),
                (new Point(130, 50), new Point(50, 50), ["L1|20-0", "L1|20-1", "L1|20-2"]),
                (new Point(120, 50), new Point(50, 50), ["L1|10-0", "L1|10-1", "L1|10-2"]),
                (new Point(113, 50), new Point(50, 50), ["L1|1-0", "L1|1-1", "L1|1-2"]),
                (new Point(112, 50), new Point(50, 50), ["M1<1", "M1<2", "M1<3"]),
                (new Point(70, 50), new Point(50, 50), ["L0|20-0", "L0|20-1", "L0|20-2"]),
                (new Point(60, 50), new Point(50, 50), ["L0|10-0", "L0|10-1", "L0|10-2"]),
                (new Point(54, 50), new Point(50, 50), ["L0|1-0", "L0|1-1", "L0|1-2"]),
                (new Point(53, 50), new Point(50, 50), []),

                (new Point(230, 150), new Point(50, 150), ["L5|20-0", "L5|20-1", "L5|20-2"]),
                (new Point(220, 150), new Point(50, 150), ["L5|10-0", "L5|10-1", "L5|10-2"]),
                (new Point(213, 150), new Point(50, 150), ["L5|1-0", "L5|1-1", "L5|1-2"]),
                (new Point(212, 150), new Point(50, 150), ["M5<1", "M5<2", "M5<3"]),
                (new Point(130, 150), new Point(50, 150), ["L4|20-0", "L4|20-1", "L4|20-2"]),
                (new Point(120, 150), new Point(50, 150), ["L4|10-0", "L4|10-1", "L4|10-2"]),
                (new Point(113, 150), new Point(50, 150), ["L4|1-0", "L4|1-1", "L4|1-2"]),
                (new Point(112, 150), new Point(50, 150), ["M4<1", "M4<2", "M4<3"]),
                (new Point(70, 150), new Point(50, 150), ["L3|20-0", "L3|20-1", "L3|20-2"]),
                (new Point(60, 150), new Point(50, 150), ["L3|10-0", "L3|10-1", "L3|10-2"]),
                (new Point(54, 150), new Point(50, 150), ["L3|1-0", "L3|1-1", "L3|1-2"]),
                (new Point(53, 150), new Point(50, 150), []),

                (new Point(230, 250), new Point(50, 250), ["L8|20-0", "L8|20-1", "L8|20-2"]),
                (new Point(220, 250), new Point(50, 250), ["L8|10-0", "L8|10-1", "L8|10-2"]),
                (new Point(213, 250), new Point(50, 250), ["L8|1-0", "L8|1-1", "L8|1-2"]),
                (new Point(212, 250), new Point(50, 250), ["M8<1", "M8<2", "M8<3"]),
                (new Point(130, 250), new Point(50, 250), ["L7|20-0", "L7|20-1", "L7|20-2"]),
                (new Point(120, 250), new Point(50, 250), ["L7|10-0", "L7|10-1", "L7|10-2"]),
                (new Point(113, 250), new Point(50, 250), ["L7|1-0", "L7|1-1", "L7|1-2"]),
                (new Point(112, 250), new Point(50, 250), ["M7<1", "M7<2", "M7<3"]),
                (new Point(70, 250), new Point(50, 250), ["L6|20-0", "L6|20-1", "L6|20-2"]),
                (new Point(60, 250), new Point(50, 250), ["L6|10-0", "L6|10-1", "L6|10-2"]),
                (new Point(54, 250), new Point(50, 250), ["L6|1-0", "L6|1-1", "L6|1-2"]),
                (new Point(53, 250), new Point(50, 250), []),
            };
        }

        /**
         * @brief Verifies that the rune navigation correctly guides the character from any
         * position in the 3x3 grid to a rune location in any other position
         * 
         * When users are botting in MapleStory and a rune (anti-botting mechanism) spawns
         * on the map, the system must navigate the character through the grid of frames
         * to reach the rune. This test simulates the character at various starting positions
         * across all 9 frames (top row Y=50, middle row Y=150, bottom row Y=250) navigating
         * to rune positions on both the right side (X=250) and left side (X=50) of the map.
         */
        private void _testNavigationToRune()
        {
            foreach (var t in _testCases())
            {
                var keystrokeTransmitterThreadHelper = _fixture();
                _runeFrameFixture();
                _bottingModel.GetMapModel().SetTemplatePosition(MapIconInfo.Character, (int)t.Item1.X, (int)t.Item1.Y);
                _bottingModel.GetMapModel().SetTemplatePosition(MapIconInfo.Rune, (int)t.Item2.X, (int)t.Item2.Y);
                var transmitResult = keystrokeTransmitterThreadHelper.Transmit();
                Debug.Assert(transmitResult == (t.Item3.Count > 0));
                Debug.Assert(_executor.ExecuteCalls == 1);
                Debug.Assert(_executor.ExecuteCallArg_macroCommands[0].Count == t.Item3.Count);
                for (int i = 0; i < t.Item3.Count; i++)
                {
                    Debug.Assert(_executor.ExecuteCallArg_macroCommands[0][i] == t.Item3[i]);
                }
            }
        }

        /**
         * @brief Verifies that the rune navigation system gracefully handles scenarios
         * when no frame data is loaded into the botting model
         * 
         * When users are botting in MapleStory but have not yet configured any rune frames
         * for the current map, the navigation system should not attempt to guide the
         * character toward runes. Instead, it should return no movement commands, allowing
         * the botting system to continue with whatever default behavior is appropriate.
         */
        private void _testNavigationWithNoFrameData()
        {
            foreach (var t in _testCases())
            {
                var keystrokeTransmitterThreadHelper = _fixture();
                _bottingModel.GetMapModel().SetTemplatePosition(MapIconInfo.Character, (int)t.Item1.X, (int)t.Item1.Y);
                _bottingModel.GetMapModel().SetTemplatePosition(MapIconInfo.Rune, (int)t.Item2.X, (int)t.Item2.Y);
                var transmitResult = keystrokeTransmitterThreadHelper.Transmit();
                Debug.Assert(transmitResult == false);
                Debug.Assert(_executor.ExecuteCalls == 1);
                Debug.Assert(_executor.ExecuteCallArg_macroCommands[0].Count == 0);
            }
        }

        /**
         * @brief Verifies that the rune navigation system fails gracefully when the character's
         * position cannot be detected on the minimap
         * 
         * When users are botting in MapleStory, the system relies on image recognition to
         * detect the character's position marker on the minimap. If this detection fails
         * (e.g., due to map overlay, character being off-screen, or poor image quality),
         * the navigation system should abort the rune approach attempt and return no
         * commands. This prevents the bot from attempting to navigate without knowing
         * where the character currently is.
         */
        private void _testNavigationFailsWithNoCharacterDetected()
        {
            foreach (var t in _testCases())
            {
                var keystrokeTransmitterThreadHelper = _fixture();
                _runeFrameFixture();
                _bottingModel.GetMapModel().SetTemplatePosition(MapIconInfo.Character, -1, -1);
                _bottingModel.GetMapModel().SetTemplatePosition(MapIconInfo.Rune, (int)t.Item2.X, (int)t.Item2.Y);
                var transmitResult = keystrokeTransmitterThreadHelper.Transmit();
                Debug.Assert(transmitResult == false);
                Debug.Assert(_executor.ExecuteCalls == 0);
            }
        }

        /**
         * @brief Verifies that the rune navigation system fails gracefully when no rune is
         * detected on the minimap
         * 
         * When users are botting in MapleStory, the system relies on image recognition to
         * detect rune spawn locations on the minimap. If no rune is currently present or
         * detection fails (e.g., the rune hasn't spawned yet, is obscured, or the detection
         * threshold is too high), the navigation system should abort the approach attempt
         * and continue normal botting operations.
         */
        private void _testNaviagtionFailsWithNoRuneDetected()
        {
            foreach (var t in _testCases())
            {
                var keystrokeTransmitterThreadHelper = _fixture();
                _runeFrameFixture();
                _bottingModel.GetMapModel().SetTemplatePosition(MapIconInfo.Character, (int)t.Item1.X, (int)t.Item1.Y);
                _bottingModel.GetMapModel().SetTemplatePosition(MapIconInfo.Rune, -1, -1);
                var transmitResult = keystrokeTransmitterThreadHelper.Transmit();
                Debug.Assert(transmitResult == false);
                Debug.Assert(_executor.ExecuteCalls == 0);
            }
        }

        public void Run()
        {
            _testNavigationToRune();
            _testNavigationWithNoFrameData();
            _testNavigationFailsWithNoCharacterDetected();
            _testNaviagtionFailsWithNoRuneDetected();
        }
    }



    public class RuneingExecutorThreadTests
    {
        private MockKeystrokeTransmitterThreadHelper _executorThreadHelper = new();

        private MockResetEvent _executionEvent = new MockResetEvent();

        private MockRunningState _transmittingState = new MockRunningState();

        private MockRunningState _runningState = new MockRunningState();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)RuneingExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Runeing
            )
        );

        private MockInjectAction _injectAction = new MockInjectAction();

        private List<string> _callOrder = [];

        private string _threadStateRef = "";

        private string _transmittingStateRef = "";

        private string _executionEventRef = "";

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
                        RuneingOrchestratorThreadInjectType.Stop, null
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
            return new RuneingExecutorThread(
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
                keystrokeTransmitterExecutorThread.Inject(RuneingOrchestratorThreadInjectType.Start, 0);
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
                RuneingOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(threadState.SetStateCallArg_state[0] == (int)RuneingExecutorThreadedUpdate.Starting);
            Debug.Assert(threadState.SetStateCallArg_state[1] == (int)RuneingExecutorThreadedUpdate.Started);
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
                keystrokeTransmitterExecutorThread.Inject(RuneingOrchestratorThreadInjectType.Stop, 0);
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
                Debug.Assert(threadState.SetStateCallArg_state[0] == (int)RuneingExecutorThreadedUpdate.Stopping);
                Debug.Assert(threadState.SetStateCallArg_state[1] == (int)RuneingExecutorThreadedUpdate.Stopped);
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
                    (int)RuneingExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Runeing
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var start = RuneingOrchestratorThreadInjectType.Start;
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
                    (int)RuneingExecutorThreadedUpdate.Started,
                    KeystrokeTransmitterThreadType.Runeing
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var stop = RuneingOrchestratorThreadInjectType.Stop;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(stop, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == 0);
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
        }
    }


    public class RuneingOrchestratorThreadTests
    {
        private AbstractKeystrokeTransmitterThreadState _threadState = new KeystrokeTransmitterThreadState(
            0, KeystrokeTransmitterThreadType.Runeing
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
            return new RuneingOrchestratorThread(
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
                0, KeystrokeTransmitterThreadType.Runeing
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
                0, KeystrokeTransmitterThreadType.Runeing
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
                123, KeystrokeTransmitterThreadType.Runeing
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            var max = RuneingOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                transmitterOrchestratorThread.Inject(
                    (RuneingOrchestratorThreadInjectType)i, 0
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
            var max = RuneingOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var transmitterOrchestratorThread = _fixture(threadState);
                transmitterOrchestratorThread.Inject((RuneingOrchestratorThreadInjectType)i, 0);
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
                    123, KeystrokeTransmitterThreadType.Runeing
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


    public class RuneingTransmitterTestSuite
    {
        public void Run()
        {
            new RuneingExecutorThreadHelperTests().Run();
            new RuneingExecutorThreadTests().Run();
            new RuneingOrchestratorThreadTests().Run();
        }
    }
}
