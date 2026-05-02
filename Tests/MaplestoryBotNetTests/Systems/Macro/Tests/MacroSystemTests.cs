using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters;
using MaplestoryBotNet.Systems.Macro;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Macro.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;


namespace MaplestoryBotNetTests.Systems.Macro.Tests
{
    public class OrchestratorControllerTests
    {
        private MockThread _mockOrchestrator = new MockThread(
            new ThreadRunningState()
        );

        private MockKeystrokeTransmitterThreadState _mockState = (
            new MockKeystrokeTransmitterThreadState()
        );

        private List<string> _callOrder = [];

        private AbstractOrchestratorController _fixture()
        {
            _mockOrchestrator = new MockThread(new ThreadRunningState());
            _mockState = new MockKeystrokeTransmitterThreadState();
            _callOrder = [];
            var controller = new OrchestratorController<
                MockOrchestratorInjectType,
                MockThreadedUpdateType
            >(
                MockOrchestratorInjectType.Start,
                MockThreadedUpdateType.Started,
                MockOrchestratorInjectType.Stop,
                MockThreadedUpdateType.Stopped
            );
            controller.SetOrchestrator(_mockOrchestrator);
            controller.SetOrchestratorThreadState(_mockState);
            return controller;
        }

        /**
         * @brief Verifies that when starting an orchestrator, the controller synchronously
         * waits for it to reach the Started state before returning, ensuring proper
         * handoff between different macro subroutines
         * 
         * When the executor needs to activate a specific macro subroutine (such as
         * switching from monster killing to rune navigation), the controller must ensure
         * the orchestrator is fully running before the executor proceeds. This
         * synchronous handshake prevents the previous subroutine from still sending
         * keystrokes while a new orchestrator tries to take over.
         */
        private void _testControllerStartsOrchestratorSynchronously()
        {
            for (int i = 0; i < (int)MockThreadedUpdateType.MaxNum; i++)
            {
                if (i == (int)MockThreadedUpdateType.Started)
                {
                    continue;
                }
                for (var j = 1; j < 10; j++)
                {
                    var orchestratorController = _fixture();
                    var orchestratorRef = new TestUtilities().Reference(_mockOrchestrator);
                    var stateRef = new TestUtilities().Reference(_mockState);
                    _mockOrchestrator.CallOrder = _callOrder;
                    _mockState.CallOrder = _callOrder;
                    for (var k = 0; k < j; k++)
                    {
                        _mockState.GetStateReturn.Add(i);
                    }
                    _mockState.GetStateReturn.Add((int)MockThreadedUpdateType.Started);
                    orchestratorController.StartOrchestrator();
                    Debug.Assert(_callOrder.Count == j + 2);
                    Debug.Assert(_callOrder[0] == orchestratorRef + "ThreadInject");
                    for (int k = 0; k < j; k++)
                    {
                        Debug.Assert(_callOrder[k + 1] == stateRef + "GetState");
                    }
                    Debug.Assert(_callOrder[j + 1] == stateRef + "GetState");
                }
            }
        }

        /**
         * @brief Verifies that the controller sends the correct start signal to activate
         * an orchestrator subroutine
         * 
         * When the executor needs to begin a specific macro subroutine (e.g., start
         * navigating to a detected rune), the controller must send the appropriate start
         * command to activate that orchestrator.
         */
        private void _testControllerStartSendsSignal()
        {
            var orchestratorController = _fixture();
            _mockState.GetStateReturn.Add((int)MockThreadedUpdateType.Started);
            orchestratorController.StartOrchestrator();
            Debug.Assert(
                (int)_mockOrchestrator.InjectCallArg_dataType[0] ==
                (int)MockOrchestratorInjectType.Start
            );
        }

        /**
         * @brief Verifies that when stopping an orchestrator, the controller synchronously
         * waits for it to reach the Stopped state before returning, ensuring clean
         * deactivation before the next orchestrator starts
         * 
         * When the executor needs to deactivate a macro subroutine (such as stopping
         * monster killing because a rune was detected), the controller must ensure the
         * orchestrator is fully stopped before allowing the next orchestrator to start.
         * This synchronous wait prevents two orchestrators from being active at the same.
         */
        private void _testControllerStopsOrchestratorSynchronously()
        {
            for (int i = 0; i < (int)MockThreadedUpdateType.MaxNum; i++)
            {
                if (i == (int)MockThreadedUpdateType.Stopped)
                {
                    continue;
                }
                for (var j = 1; j < 10; j++)
                {
                    var orchestratorController = _fixture();
                    var orchestratorRef = new TestUtilities().Reference(_mockOrchestrator);
                    var stateRef = new TestUtilities().Reference(_mockState);
                    _mockOrchestrator.CallOrder = _callOrder;
                    _mockState.CallOrder = _callOrder;
                    for (var k = 0; k < j; k++)
                    {
                        _mockState.GetStateReturn.Add(i);
                    }
                    _mockState.GetStateReturn.Add((int)MockThreadedUpdateType.Stopped);
                    orchestratorController.StopOrchestrator();
                    Debug.Assert(_callOrder.Count == j + 2);
                    Debug.Assert(_callOrder[0] == orchestratorRef + "ThreadInject");
                    for (int k = 0; k < j; k++)
                    {
                        Debug.Assert(_callOrder[k + 1] == stateRef + "GetState");
                    }
                    Debug.Assert(_callOrder[j + 1] == stateRef + "GetState");
                }
            }
        }

        /**
         * @brief Verifies that the controller sends the correct stop signal to deactivate
         * an orchestrator subroutine
         * 
         * When the MacroExecutor needs to end a macro subroutine (e.g., stop rune
         * navigation because the rune was found and solving needs to begin), the controller
         * must send the appropriate stop command to deactivate the current orchestrator.
         */
        private void _testControllerStopSendsSignal()
        {
            var orchestratorController = _fixture();
            _mockState.GetStateReturn.Add((int)MockThreadedUpdateType.Stopped);
            orchestratorController.StopOrchestrator();
            Debug.Assert(
                (int)_mockOrchestrator.InjectCallArg_dataType[0] ==
                (int)MockOrchestratorInjectType.Stop
            );
        }

        public void Run()
        {
            _testControllerStartsOrchestratorSynchronously();
            _testControllerStartSendsSignal();
            _testControllerStopsOrchestratorSynchronously();
            _testControllerStopSendsSignal();
        }
    }


    public class MacroExecutorStateResetTests
    {
        private MockOrchestratorController _bottingController = new MockOrchestratorController();

        private MockOrchestratorController _runeingController = new MockOrchestratorController();

        private MockOrchestratorController _solvingController = new MockOrchestratorController();

        private MacroExecutorThreadContext _context = new MacroExecutorThreadContext(
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new StopwatchTimestamp(),
            new StopwatchTimestamp(),
            MapIconInfo.Rune
        );

        private AbstractBottingModel _bottingModel = new BottingModel();

        private AbstractExecutorState _fixture()
        {
            _bottingController = new MockOrchestratorController();
            _runeingController = new MockOrchestratorController();
            _solvingController = new MockOrchestratorController();
            _bottingModel = new BottingModel();
            _context = new MacroExecutorThreadContext(
                _bottingController,
                _runeingController,
                _solvingController,
                new StopwatchTimestamp(),
                new StopwatchTimestamp(),
                MapIconInfo.Rune
            );
            _context.BottingModel = _bottingModel;
            return new MacroExecutorStateReset(_context);
        }

        private void _stopFixture()
        {
            _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
            _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
            _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
        }

        /**
         * @brief Verifies that after resetting, the macro executor transitions to the Idle state
         * 
         * When users stop automation or the system needs to reset, the macro executor should
         * return to an idle state where no automation activity is running. This idle state
         * allows the system to be restarted cleanly without any leftover state from previous
         * operations.
         */
        private void _testExecutorTransitionsToIdleState()
        {
            var macroExecutorStateReset = _fixture();
            _stopFixture();
            var result = macroExecutorStateReset.Execute();
            Debug.Assert(result == (int)MacroExecutorStateTypes.Idle);
        }

        /**
         * @brief Verifies that the reset process sets the current rune activation period
         * from the botting model's cooldown value
         * 
         * When the macro system resets, the current rune activation period in the execution
         * context should be updated to match the cooldown value stored in the botting
         * model. This cooldown determines how long the system must wait before attempting
         * to activate another rune after successfully solving one.
         */
        private void _testExecutorSetsActivationPeriod()
        {
            var macroExecutorStateReset = _fixture();
            _stopFixture();
            _bottingModel.GetRuneModel().SetCooldown(123);
            macroExecutorStateReset.Execute();
            Debug.Assert(_context.RuneActivationPeriodCurrent == 123);
        }

        /**
         * @brief Verifies that when a macro system reset occurs, the botting orchestrator
         * is stopped if it is currently in any state other than Stopped
         * 
         * When the button system needs to reset, the monster-killing orchestrator must be
         * deactivated to prevent it from continuing to send keystrokes while the system
         * transitions to idle or starts a different subroutine. If the orchestrator is
         * already Stopped, no action is needed. If it is in any other state,
         * the reset process calls to terminate it cleanly.
         */
        private void _testExecutorStopsBottingOrchestrator()
        {
            for (int i = 0; i < (int)BottingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateReset = _fixture();
                _bottingController.GetStateReturn.Add(i);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                macroExecutorStateReset.Execute();
                Debug.Assert(
                    _bottingController.StopOrchestratorCalls ==
                    (i != (int)BottingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when a macro system reset occurs, the rune navigation
         * orchestrator is stopped if it is currently in any state other than Stopped
         * 
         * When the bot system needs to reset, any active rune navigation subroutine
         * must be deactivated. This ensures the character does not continue walking
         * toward a rune after the rune. If the rune navigation orchestrator is already
         * Stopped, no action is taken. If it is in any other state, the
         * reset process calls to terminate it cleanly.
         */
        private void _testExecutorStopsRuneingOrchestrator()
        {
            for (int i = 0; i < (int)RuneingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateReset = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add(i);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                macroExecutorStateReset.Execute();
                Debug.Assert(
                    _runeingController.StopOrchestratorCalls ==
                    (i != (int)RuneingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when a macro system reset occurs, the puzzle solving
         * orchestrator is stopped if it is currently in any state other than Stopped
         * 
         * When the button system needs to reset, the puzzle solving orchestrator must
         * be deactivated. This prevents the system from continuing to process puzzle
         * inputs after the rune has already been completed. If the solving orchestrator
         * is already Stopped, no action is needed. If it is in any other state, the
         * reset process calls to terminate it cleanly.
         */
        private void _testExecutorStopsSolvingOrchestrator()
        {
            for (int i = 0; i < (int)SolvingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateReset = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add(i);
                macroExecutorStateReset.Execute();
                Debug.Assert(
                    _solvingController.StopOrchestratorCalls ==
                    (i != (int)SolvingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        public void Run()
        {
            _testExecutorTransitionsToIdleState();
            _testExecutorSetsActivationPeriod();
            _testExecutorStopsBottingOrchestrator();
            _testExecutorStopsRuneingOrchestrator();
            _testExecutorStopsSolvingOrchestrator();
        }
    }


    public class MacroExecuteStateIdleTests
    {
        private MockOrchestratorController _bottingController = new MockOrchestratorController();

        private MockOrchestratorController _runeingController = new MockOrchestratorController();

        private MockOrchestratorController _solvingController = new MockOrchestratorController();

        private MacroExecutorThreadContext _context = new MacroExecutorThreadContext(
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new StopwatchTimestamp(),
            new StopwatchTimestamp(),
            MapIconInfo.Rune
        );

        private MockTimestamp _runeingStopwatch = new MockTimestamp();

        private AbstractExecutorState _fixture()
        {
            _bottingController = new MockOrchestratorController();
            _runeingController = new MockOrchestratorController();
            _solvingController = new MockOrchestratorController();
            _runeingStopwatch = new MockTimestamp();
            _context = new MacroExecutorThreadContext(
                _bottingController,
                _runeingController,
                _solvingController,
                _runeingStopwatch,
                new StopwatchTimestamp(),
                MapIconInfo.Rune
            );
            return new MacroExecutorStateIdle(_context);
        }

        private void _stopFixture()
        {
            _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
            _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
            _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
        }

        /**
         * @brief Verifies that when the macro executor is in the Idle state, executing it
         * transitions to the Botting state to begin monster-killing automation
         * 
         * When the bot system is idle (no automation running) and the macro executor runs,
         * it should automatically transition to the Botting state to start the monster-killing
         * subroutine. This is the normal startup flow for the bot - from idle into active
         * combat automation.
         */
        private void _testExecutorTransitionsToBottingState()
        {
            var macroExecutorStateIdle = _fixture();
            _stopFixture();
            var result = macroExecutorStateIdle.Execute();
            Debug.Assert(result == (int)MacroExecutorStateTypes.Botting);
        }

        /**
         * @brief Verifies that the rune detection stopwatch is reset when the executor is idle.
         * 
         * When the macro system is idle, the timer used to track rune detection cooldowns
         * should be reset. This ensures that rune detection timing starts fresh when
         * automation resumes, preventing stale cooldown times from affecting the next
         * automation session.
         */
        private void _testExecutorSetsRuneingStopwatch()
        {
            var macroExecutorStateIdle = _fixture();
            _stopFixture();
            macroExecutorStateIdle.Execute();
            Debug.Assert(_runeingStopwatch.SetTimestampCalls == 1);
        }

        /**
         * @brief Verifies that when entering the Botting state from Idle, the botting
         * orchestrator is stopped if it is currently in any state other than Stopped
         * 
         * When the macro system transitions from Idle to Botting, it ensures the botting
         * orchestrator is fully stopped before starting a new botting session. If the
         * botting orchestrator is already Stopped, no action is needed.
         */
        private void _testExecutorStopsBottingOrchestrator()
        {
            for (int i = 0; i < (int)BottingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateIdle = _fixture();
                _bottingController.GetStateReturn.Add(i);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                macroExecutorStateIdle.Execute();
                Debug.Assert(
                    _bottingController.StopOrchestratorCalls ==
                    (i != (int)BottingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when entering the Botting state from Idle, the rune navigation
         * orchestrator is stopped if it is currently in any state other than Stopped
         * 
         * When the macro system transitions from Idle to Botting, it ensures the rune
         * navigation orchestrator is fully stopped. This prevents any active rune approach
         * behavior from continuing while the bot is trying to kill monsters. If the rune
         * navigation orchestrator is already Stopped, no action is taken.
         */
        private void _testExecutorStopsRuneingOrchestrator()
        {
            for (int i = 0; i < (int)RuneingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateIdle = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add(i);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                macroExecutorStateIdle.Execute();
                Debug.Assert(
                    _runeingController.StopOrchestratorCalls ==
                    (i != (int)RuneingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when entering the Botting state from Idle, the puzzle solving
         * orchestrator is stopped if it is currently in any state other than Stopped
         * 
         * When the macro system transitions from Idle to Botting, it ensures the puzzle
         * solving orchestrator is fully stopped. This prevents any leftover rune puzzle
         * solving behavior from continuing while the bot is trying to kill monsters.
         * If the solving orchestrator is already Stopped, no action is needed.
         */
        private void _testExecutorStopsSolvingOrchestrator()
        {
            for (int i = 0; i < (int)SolvingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateIdle = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add(i);
                macroExecutorStateIdle.Execute();
                Debug.Assert(
                    _solvingController.StopOrchestratorCalls ==
                    (i != (int)SolvingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        public void Run()
        {
            _testExecutorTransitionsToBottingState();
            _testExecutorSetsRuneingStopwatch();
            _testExecutorStopsBottingOrchestrator();
            _testExecutorStopsRuneingOrchestrator();
            _testExecutorStopsSolvingOrchestrator();
        }

    }


    public class MacroExecutorStateBottingTests
    {
        private MockOrchestratorController _bottingController = new MockOrchestratorController();

        private MockOrchestratorController _runeingController = new MockOrchestratorController();

        private MockOrchestratorController _solvingController = new MockOrchestratorController();

        private MacroExecutorThreadContext _context = new MacroExecutorThreadContext(
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new StopwatchTimestamp(),
            new StopwatchTimestamp(),
            MapIconInfo.Rune
        );

        private MockTimestamp _runeingStopwatch = new MockTimestamp();

        private AbstractExecutorState _fixture()
        {
            _bottingController = new MockOrchestratorController();
            _runeingController = new MockOrchestratorController();
            _solvingController = new MockOrchestratorController();
            _runeingStopwatch = new MockTimestamp();
            _context = new MacroExecutorThreadContext(
                _bottingController,
                _runeingController,
                _solvingController,
                _runeingStopwatch,
                new MockTimestamp(),
                MapIconInfo.Rune
            );
            _context.BottingModel = new BottingModel();
            return new MacroExecutorStateBotting(_context);
        }

        private void _stopFixture()
        {
            _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
            _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
            _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
        }

        /**
         * @brief Verifies that when the rune activation cooldown has not yet expired, the
         * macro executor remains in the Botting state to continue monster killing
         * 
         * When the bot is actively killing monsters and the cooldown period since the
         * last rune activation has not elapsed, the system should stay in Botting state
         * and continue normal monster-killing operations.
         */
        private void _testExecutorTransitionsToBottingState()
        {
            var macroExecutorStateBotting = _fixture();
            _stopFixture();
            _runeingStopwatch.GetTimestampReturn.Add(123);
            _context.RuneActivationPeriodCurrent = 1234;
            var result = macroExecutorStateBotting.Execute();
            Debug.Assert(result == (int)MacroExecutorStateTypes.Botting);
        }

        /**
         * @brief Verifies that when the rune activation cooldown has expired, the macro
         * executor transitions from Botting to Runeing state to begin rune approach
         * 
         * When the bot has been killing monsters for longer than the configured rune
         * activation cooldown period, the system should transition to the Runeing state
         * to start looking for and approaching a rune. This ensures the bot periodically
         * attempts to solve runes.
         */
        private void _testExecutorStaysBottingUntilRuneSpawns()
        {
            var positions = new[] { new Point(1, 1), new Point(-1, -1) };
            var expecteds = new[] { MacroExecutorStateTypes.Runeing, MacroExecutorStateTypes.Botting };
            for (int i = 0; i < positions.Count(); i++)
            {
                var position = positions[i];
                var expected = expecteds[i];
                var macroExecutorStateBotting = _fixture();
                var mapModel = _context.BottingModel!.GetMapModel();
                _stopFixture();
                _runeingStopwatch.GetTimestampReturn.Add(1234);
                _context.RuneActivationPeriodCurrent = 123;
                mapModel.SetTemplatePosition(_context.RuneKey, position.X, position.Y);
                var result = macroExecutorStateBotting.Execute();
                Debug.Assert(result == (int)expected);
            }
        }

        /**
         * @brief Verifies that when entering the Botting state, the botting orchestrator
         * is started if it is not already in the Started state
         * 
         * When the macro system needs to begin or remain in monster-killing mode, it
         * ensures the botting orchestrator is actively running. If the orchestrator is
         * already Started, no action is taken. If it is in any other state, the Botting
         * state starts the orchestrator to launch the monster-killing subroutine.
         */
        private void _testExecutorStartsBottingOrchestrator()
        {
            for (int i = 0; i < (int)BottingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateBotting = _fixture();
                _bottingController.GetStateReturn.Add(i);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                _runeingStopwatch.GetTimestampReturn.Add(123);
                macroExecutorStateBotting.Execute();
                Debug.Assert(
                    _bottingController.StartOrchestratorCalls ==
                    (i != (int)BottingExecutorThreadedUpdate.Started ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when entering the Botting state, the rune navigation
         * orchestrator is stopped to prevent conflicting behaviors
         * 
         * When the macro system enters or remains in Botting state for monster killing,
         * it ensures the rune navigation orchestrator is fully stopped. This prevents
         * the bot from simultaneously trying to approach a rune while killing monsters.
         */
        private void _testExecutorStopsRuneingOrchestrator()
        {
            for (int i = 0; i < (int)RuneingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateBotting = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add(i);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                _runeingStopwatch.GetTimestampReturn.Add(123);
                macroExecutorStateBotting.Execute();
                Debug.Assert(
                    _runeingController.StopOrchestratorCalls ==
                    (i != (int)RuneingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when entering the Botting state, the puzzle solving
         * orchestrator is stopped to prevent conflicting behaviors
         * 
         * When the macro system enters or remains in Botting state for monster killing,
         * it ensures the puzzle solving orchestrator is fully stopped. This prevents any
         * leftover rune puzzle solving behavior from interfering with monster killing
         * keystrokes.
         */
        private void _testExecutorStopsSolvingOrchestrator()
        {
            for (int i = 0; i < (int)SolvingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateBotting = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add(i);
                _runeingStopwatch.GetTimestampReturn.Add(123);
                macroExecutorStateBotting.Execute();
                Debug.Assert(
                    _solvingController.StopOrchestratorCalls ==
                    (i != (int)SolvingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        public void Run()
        {
            _testExecutorTransitionsToBottingState();
            _testExecutorStaysBottingUntilRuneSpawns();
            _testExecutorStartsBottingOrchestrator();
            _testExecutorStopsRuneingOrchestrator();
            _testExecutorStopsSolvingOrchestrator();
        }
    }


    public class MacroExecutorStateRuneingTests
    {
        private MockOrchestratorController _bottingController = new MockOrchestratorController();

        private MockOrchestratorController _runeingController = new MockOrchestratorController();

        private MockOrchestratorController _solvingController = new MockOrchestratorController();

        private MacroExecutorThreadContext _context = new MacroExecutorThreadContext(
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new StopwatchTimestamp(),
            new StopwatchTimestamp(),
            MapIconInfo.Rune
        );

        private AbstractExecutorState _fixture()
        {
            _bottingController = new MockOrchestratorController();
            _runeingController = new MockOrchestratorController();
            _solvingController = new MockOrchestratorController();
            _context = new MacroExecutorThreadContext(
                _bottingController,
                _runeingController,
                _solvingController,
                new MockTimestamp(),
                new MockTimestamp(),
                MapIconInfo.Rune
            );
            return new MacroExecutorStateRuneing(_context);
        }

        /**
         * @brief Verifies that the macro executor transitions from Runeing state to either
         * continue Runeing or move to Solving state based on the rune navigation thread's status
         * 
         * When the bot is navigating toward a detected rune, the system monitors the rune
         * navigation orchestrator's state. If the orchestrator reports any state other than
         * Arrived, the system stays in Runeing state to continue approaching the rune. When
         * the orchestrator reports Arrived the system transitions to Solving state to begin
         * the rune puzzle solving subroutine.
         */
        private void _testExecutorTransitions()
        {
            for (int i = 0; i < (int)RuneingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateRuneing = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add(i);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                var result = macroExecutorStateRuneing.Execute();
                Debug.Assert(
                    result ==
                    (
                        i != (int)RuneingExecutorThreadedUpdate.Arrived ?
                        (int)MacroExecutorStateTypes.Runeing :
                        (int)MacroExecutorStateTypes.Solving
                    )
                );
            }
        }

        /**
         * @brief Verifies that when entering the Runeing state, the rune navigation
         * orchestrator is started if it is not already Started or Arrived
         * 
         * When the macro system needs to begin or continue navigating toward a rune,
         * it ensures the rune navigation orchestrator is actively running. If the
         * orchestrator is already Started or Arrived (meaning navigation is already in
         * progress or complete), no action is taken. If it is in any other state
         * the Runeing state calls StartOrchestrator to launch the rune approach
         * subroutine.
         */
        private void _testExecutorStartsRuneingOrchestrator()
        {
            for (int i = 0; i < (int)RuneingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateRuneing = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add(i);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                macroExecutorStateRuneing.Execute();
                Debug.Assert(
                    _runeingController.StartOrchestratorCalls ==
                    (
                        (
                            i != (int)RuneingExecutorThreadedUpdate.Started &&
                            i != (int)RuneingExecutorThreadedUpdate.Arrived
                        ) ? 1 : 0
                    )
                );
            }
        }

        /**
         * @brief Verifies that when entering the Runeing state, the botting orchestrator
         * is stopped to prevent conflicting keystrokes during rune approach
         * 
         * When the macro system switches from monster killing to rune navigation, it
         * ensures the botting orchestrator is fully stopped. This prevents the bot from
         * simultaneously trying to kill monsters while approaching a rune, which could
         * confuse the character and produce erratic behavior.
         */
        private void _testExecutorStopsBottingOrchestrator()
        {
            for (int i = 0; i < (int)BottingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateRuneing = _fixture();
                _bottingController.GetStateReturn.Add(i);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                macroExecutorStateRuneing.Execute();
                Debug.Assert(
                    _bottingController.StopOrchestratorCalls ==
                    (i != (int)BottingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when entering the Runeing state, the puzzle solving
         * orchestrator is stopped to prevent duplicate rune interaction attempts
         * 
         * When the macro system begins rune navigation, it ensures the puzzle solving
         * orchestrator is fully stopped. This prevents any previous solving attempts
         * from interfering with the navigation to a new rune. If the solving orchestrator
         * is already Stopped, no action is needed. If it is in any other state, the Runeing
         * state calls the orchestrator to shut it down.
         */
        private void _testExecutorStopsSolvingOrchestrator()
        {
            for (int i = 0; i < (int)SolvingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateRuneing = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add(i);
                macroExecutorStateRuneing.Execute();
                Debug.Assert(
                    _solvingController.StopOrchestratorCalls ==
                    (i != (int)SolvingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        public void Run()
        {
            _testExecutorTransitions();
            _testExecutorStartsRuneingOrchestrator();
            _testExecutorStopsBottingOrchestrator();
            _testExecutorStopsSolvingOrchestrator();
        }
    }


    public class MacroExecutorStateSolvingTests
    {
        private MockOrchestratorController _bottingController = new MockOrchestratorController();

        private MockOrchestratorController _runeingController = new MockOrchestratorController();

        private MockOrchestratorController _solvingController = new MockOrchestratorController();

        private MacroExecutorThreadContext _context = new MacroExecutorThreadContext(
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new StopwatchTimestamp(),
            new StopwatchTimestamp(),
            MapIconInfo.Rune
        );

        private MockTimestamp _solvingStopwatch = new MockTimestamp();

        private AbstractExecutorState _fixture()
        {
            _bottingController = new MockOrchestratorController();
            _runeingController = new MockOrchestratorController();
            _solvingController = new MockOrchestratorController();
            _solvingStopwatch = new MockTimestamp();
            _context = new MacroExecutorThreadContext(
                _bottingController,
                _runeingController,
                _solvingController,
                new MockTimestamp(),
                _solvingStopwatch,
                MapIconInfo.Rune
            );
            return new MacroExecutorStateSolving(_context);
        }

        /**
         * @brief Verifies that the macro executor transitions from Solving state based on
         * whether the rune puzzle has been successfully solved
         * 
         * When the bot is attempting to solve a rune puzzle, the system continues solving
         * until the puzzle is completed. Once the solving orchestrator reports that the
         * puzzle is Solved, the system moves to SolvedCheck state to verify success and
         * reset timers.
         */
        private void _testExecutorTransitions()
        {
            for (int i = 0; i < (int)SolvingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateSolving = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add(i);
                var result = macroExecutorStateSolving.Execute();
                Debug.Assert(
                    result ==
                    (
                        i != (int)SolvingExecutorThreadedUpdate.Solved &&
                        i != (int)SolvingExecutorThreadedUpdate.Failed ?
                        (int)MacroExecutorStateTypes.Solving :
                        (int)MacroExecutorStateTypes.SolvedCheck
                    )
                );
            }
        }

        /**
         * @brief Verifies that when a rune puzzle is successfully solved, the solving
         * stopwatch records the completion time for cooldown tracking
         * 
         * When the bot successfully solves a rune puzzle, the system records the current
         * timestamp. This timestamp determines when the next rune can be attempted,
         * enforcing a cooldown period between successful rune solves to prevent rapid
         * repeated rune attempts.
         */
        private void _testExecutorSetsSolvingStopwatch()
        {
            for (int i = 0; i < (int)SolvingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateSolving = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add(i);
                var result = macroExecutorStateSolving.Execute();
                Debug.Assert(
                    _solvingStopwatch.SetTimestampCalls ==
                    (i == (int)SolvingExecutorThreadedUpdate.Solved ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when entering the Solving state, the puzzle solving
         * orchestrator is started if it is not already active or complete
         * 
         * When the bot arrives at a rune and needs to solve it, the system ensures the
         * solving orchestrator is running. If the orchestrator is already Started or Solved,
         * no action is taken. If it is in any other state, the Solving state starts the
         * orchestrator to begin the puzzle-solving sequence.
         */
        private void _testExecutorStartsSolvingOrchestrator()
        {
            for (int i = 0; i < (int)SolvingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateSolving = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add(i);
                macroExecutorStateSolving.Execute();
                Debug.Assert(
                    _solvingController.StartOrchestratorCalls ==
                    (
                        (
                            i != (int)SolvingExecutorThreadedUpdate.Started &&
                            i != (int)SolvingExecutorThreadedUpdate.Failed &&
                            i != (int)SolvingExecutorThreadedUpdate.Solved
                        ) ? 1 : 0
                    )
                );
            }
        }

        /**
         * @brief Verifies that when entering the Solving state, the botting orchestrator
         * is stopped to prevent monster-killing keystrokes during puzzle solving
         * 
         * When the bot begins solving a rune puzzle, the system stops the monster-killing
         * orchestrator. This prevents the bot from sending combat keystrokes while trying
         * to solve the puzzle, which would interfere with solving and likely cause failure.
         */
        private void _testExecutorStopsBottingOrchestrator()
        {
            for (int i = 0; i < (int)BottingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateSolving = _fixture();
                _bottingController.GetStateReturn.Add(i);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                macroExecutorStateSolving.Execute();
                Debug.Assert(
                    _bottingController.StopOrchestratorCalls ==
                    (i != (int)BottingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when entering the Solving state, the rune navigation
         * orchestrator is stopped to prevent continued movement during puzzle solving
         * 
         * When the bot reaches a rune and begins solving the puzzle, the system stops the
         * rune navigation orchestrator. This prevents the bot from continuing to move
         * toward the rune while already at it, which could disrupt the solving process.
         */
        private void _testExecutorStopsRuneingOrchestrator()
        {
            for (int i = 0; i < (int)RuneingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateSolving = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add(i);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                macroExecutorStateSolving.Execute();
                Debug.Assert(
                    _runeingController.StopOrchestratorCalls ==
                    (i != (int)RuneingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        public void Run()
        {
            _testExecutorTransitions();
            _testExecutorSetsSolvingStopwatch();
            _testExecutorStartsSolvingOrchestrator();
            _testExecutorStopsBottingOrchestrator();
            _testExecutorStopsRuneingOrchestrator();
        }
    }


    public class MacroExecutorStateSolvedCheckTests
    {
        private MockOrchestratorController _bottingController = new MockOrchestratorController();

        private MockOrchestratorController _runeingController = new MockOrchestratorController();

        private MockOrchestratorController _solvingController = new MockOrchestratorController();

        private MacroExecutorThreadContext _context = new MacroExecutorThreadContext(
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new StopwatchTimestamp(),
            new StopwatchTimestamp(),
            MapIconInfo.Rune
        );

        private AbstractBottingModel _bottingModel = new BottingModel();

        private MockTimestamp _runeingStopwatch = new MockTimestamp();

        private MockTimestamp _solvingStopwatch = new MockTimestamp();

        private AbstractExecutorState _fixture()
        {
            _bottingController = new MockOrchestratorController();
            _runeingController = new MockOrchestratorController();
            _solvingController = new MockOrchestratorController();
            _runeingStopwatch = new MockTimestamp();
            _solvingStopwatch = new MockTimestamp();
            _bottingModel = new BottingModel();
            _context = new MacroExecutorThreadContext(
                _bottingController,
                _runeingController,
                _solvingController,
                _runeingStopwatch,
                _solvingStopwatch,
                MapIconInfo.Rune
            );
            _context.BottingModel = _bottingModel;
            return new MacroExecutorStateSolvedCheck(_context);
        }

        /**
         * @brief Verifies that when the solved check timeout expires without a rune
         * appearing, the system returns to Botting state to resume monster killing
         * 
         * After successfully solving a rune puzzle, the bot waits for the rune to be
         * detected. If the configured timeout period elapses and no new rune has
         * appeared, the system transitions back to Botting state. The bot resumes
         * killing monsters until the rune cooldown period expires and a new rune can be
         * detected.
         */
        private void _testExecutorTransitionsToBottingOnTimeout()
        {
            var macroExecutorStateSolvedCheck = _fixture();
            _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Started);
            _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
            _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
            _solvingStopwatch.GetTimestampReturn.Add(123);
            _context.SolvedCheckTimeout = 12;
            var result = macroExecutorStateSolvedCheck.Execute();
            Debug.Assert(result == (int)MacroExecutorStateTypes.Botting);
        }

        /**
         * @brief Verifies that when a rune is detected during the solved check period,
         * the system transitions to Runeing state to approach it
         * 
         * After solving a rune puzzle, the bot continues monitoring for the rune.
         * If the rune appears on the minimap during the solved check window, the system
         * immediately transitions to Runeing state to begin approaching the rune.
         * This allows the bot to handle failed rune solves.
         */
        private void _testExecutorTransitionsToRuneingOnDetectedRune()
        {
            var macroExecutorStateSolvedCheck = _fixture();
            _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Started);
            _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
            _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
            _solvingStopwatch.GetTimestampReturn.Add(12);
            _context.SolvedCheckTimeout = 123;
            _bottingModel.GetMapModel().SetTemplatePosition(MapIconInfo.Rune, 12, 23);
            var result = macroExecutorStateSolvedCheck.Execute();
            Debug.Assert(result == (int)MacroExecutorStateTypes.Runeing);
        }

        /**
         * @brief Verifies that when no rune is detected and the timeout has not yet
         * expired, the system stays in SolvedCheck state to continue waiting
         * 
         * After solving a rune puzzle, the bot waits for either a rune to spawn or
         * the timeout to expire. If neither condition has been met yet, the system remains
         * in SolvedCheck state to continue monitoring. This prevents the bot from
         * prematurely returning to botting after a failed rune solve.
         */
        private void _testExecutorTransitionsToSolvedCheckOnNoRune()
        {
            var macroExecutorStateSolvedCheck = _fixture();
            _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Started);
            _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
            _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
            _solvingStopwatch.GetTimestampReturn.Add(12);
            _context.SolvedCheckTimeout = 123;
            _bottingModel.GetMapModel().SetTemplatePosition(MapIconInfo.Rune, -1, -1);
            var result = macroExecutorStateSolvedCheck.Execute();
            Debug.Assert(result == (int)MacroExecutorStateTypes.SolvedCheck);
        }

        /**
         * @brief Verifies that the rune detection stopwatch is reset when the solved check
         * times out and the system returns to botting
         * 
         * When the bot times out waiting for another rune and returns to Botting state,
         * the rune detection stopwatch is reset. This ensures that rune detection timing
         * starts fresh for the next automation cycle, preventing stale cooldown values
         * from affecting the new session.
         */
        private void _testExecutorSetsRuneingTimestampOnTimeout()
        {
            for (int i = 0; i < 2; i++)
            {
                var macroExecutorStateSolvedCheck = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Started);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                _solvingStopwatch.GetTimestampReturn.Add(i == 1 ? 123 : 12);
                _context.SolvedCheckTimeout = i == 1 ? 12 : 123;
                macroExecutorStateSolvedCheck.Execute();
                Debug.Assert(_runeingStopwatch.SetTimestampCalls == (i == 1 ? 1 : 0));
            }
        }

        /**
         * @brief Verifies that the rune activation period is updated from the botting
         * model when the solved check times out
         * 
         * When the bot times out waiting for another rune and returns to Botting state,
         * the current rune activation period in the context is updated to match the
         * configured value from the botting model.
         */
        private void _testExecutorSetsRuneActivationPeriodOnTimeout()
        {
            for (int i = 0; i < 2; i++)
            {
                var macroExecutorStateSolvedCheck = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Started);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                _solvingStopwatch.GetTimestampReturn.Add(i == 1 ? 123 : 12);
                _context.SolvedCheckTimeout = i == 1 ? 12 : 123;
                _context.RuneActivationPeriodCurrent = 1234;
                _context.RuneActivationPeriod = 12345;
                macroExecutorStateSolvedCheck.Execute();
                Debug.Assert(_context.RuneActivationPeriodCurrent == (i == 1 ? 12345 : 1234));
            }
        }

        /**
         * @brief Verifies that when entering SolvedCheck state, the puzzle solving
         * orchestrator is stopped to prevent continued solving after completion
         * 
         * After a rune puzzle has been solved, the system stops the solving orchestrator
         * to prevent any leftover puzzle-solving keystrokes from being sent. This ensures
         * the bot does not continue attempting to solve a puzzle that has already been
         * completed.
         */
        private void _testExecutorStopsSolvingOrchestrator()
        {
            for (int i = 0; i < (int)SolvingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateSolvedCheck = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add(i);
                _solvingStopwatch.GetTimestampReturn.Add(123);
                macroExecutorStateSolvedCheck.Execute();
                Debug.Assert(
                    _solvingController.StopOrchestratorCalls ==
                    (i != (int)SolvingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when entering SolvedCheck state, the botting orchestrator
         * is started if it is not already active
         * 
         * When the bot is waiting for a rune to be detected spawn (or timing out), the
         * system ensures the botting orchestrator is operating to resume monster killing
         * If the orchestrator is already Started, no action is taken. If it is in any other
         * state, StartOrchestrator is called to start botting.
         */
        private void _testExecutorStartsBottingOrchestrator()
        {
            for (int i = 0; i < (int)BottingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateSolvedCheck = _fixture();
                _bottingController.GetStateReturn.Add(i);
                _runeingController.GetStateReturn.Add((int)RuneingExecutorThreadedUpdate.Stopped);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                _solvingStopwatch.GetTimestampReturn.Add(123);
                macroExecutorStateSolvedCheck.Execute();
                Debug.Assert(
                    _bottingController.StartOrchestratorCalls ==
                    (i != (int)BottingExecutorThreadedUpdate.Started ? 1 : 0)
                );
            }
        }

        /**
         * @brief Verifies that when entering SolvedCheck state, the rune navigation
         * orchestrator is stopped to prevent unwanted movement
         * 
         * After solving a rune, the system stops the rune navigation orchestrator to
         * prevent the bot from continuing to approach a rune that has already been solved.
         */
        private void _testExecutorStopsRuneingOrchestrator()
        {
            for (int i = 0; i < (int)RuneingExecutorThreadedUpdate.MaxNum; i++)
            {
                var macroExecutorStateSolvedCheck = _fixture();
                _bottingController.GetStateReturn.Add((int)BottingExecutorThreadedUpdate.Stopped);
                _runeingController.GetStateReturn.Add(i);
                _solvingController.GetStateReturn.Add((int)SolvingExecutorThreadedUpdate.Stopped);
                _solvingStopwatch.GetTimestampReturn.Add(123);
                macroExecutorStateSolvedCheck.Execute();
                Debug.Assert(
                    _runeingController.StopOrchestratorCalls ==
                    (i != (int)RuneingExecutorThreadedUpdate.Stopped ? 1 : 0)
                );
            }
        }

        public void Run()
        {
            _testExecutorTransitionsToBottingOnTimeout();
            _testExecutorTransitionsToRuneingOnDetectedRune();
            _testExecutorTransitionsToSolvedCheckOnNoRune();
            _testExecutorSetsRuneingTimestampOnTimeout();
            _testExecutorSetsRuneActivationPeriodOnTimeout();
            _testExecutorStopsSolvingOrchestrator();
            _testExecutorStartsBottingOrchestrator();
            _testExecutorStopsRuneingOrchestrator();
        }
    }


    public class MacroExecutorThreadStateMachineTests
    {
        private List<AbstractExecutorState> _executorStates = [];

        private MockOrchestratorController _bottingController = new MockOrchestratorController();

        private MockOrchestratorController _runeingController = new MockOrchestratorController();

        private MockOrchestratorController _solvingController = new MockOrchestratorController();

        private MacroExecutorThreadContext _context = new MacroExecutorThreadContext(
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new MockOrchestratorController(),
            new StopwatchTimestamp(),
            new StopwatchTimestamp(),
            MapIconInfo.Rune
        );

        private MockMacroSleeper _sleeper = new MockMacroSleeper();

        private MockTimestamp _executeTimestamp = new MockTimestamp();

        private List<string> _callOrder = [];

        private AbstractKeystrokeTransmitterThreadHelper _fixture()
        {
            _executorStates = [];
            _bottingController = new MockOrchestratorController();
            _runeingController = new MockOrchestratorController();
            _solvingController = new MockOrchestratorController();
            _sleeper = new MockMacroSleeper();
            _executeTimestamp = new MockTimestamp();
            _context = new MacroExecutorThreadContext(
                _bottingController,
                _runeingController,
                _solvingController,
                new StopwatchTimestamp(),
                new StopwatchTimestamp(),
                MapIconInfo.Rune
            );
            _callOrder = [];
            return new MacroExecutorThreadStateMachine(
                _executorStates,
                _context,
                _sleeper,
                _executeTimestamp,
                (int)MacroExecutorStateTypes.Idle
            );
        }

        private void _setupExecutorStates()
        {
            _executorStates.Add(new MockExecutorState());
            _executorStates.Add(new MockExecutorState());
            _executorStates.Add(new MockExecutorState());
            _executorStates.Add(new MockExecutorState());
            _executorStates.Add(new MockExecutorState());
            ((MockExecutorState)_executorStates[0]).ExecuteReturn.Add(2);
            ((MockExecutorState)_executorStates[2]).ExecuteReturn.Add(4);
            ((MockExecutorState)_executorStates[4]).ExecuteReturn.Add(1);
            ((MockExecutorState)_executorStates[1]).ExecuteReturn.Add(3);
            ((MockExecutorState)_executorStates[3]).ExecuteReturn.Add(0);
            _executeTimestamp.GetTimestampReturn.Add(0);
            _executeTimestamp.GetTimestampReturn.Add(0);
            _executeTimestamp.GetTimestampReturn.Add(0);
            _executeTimestamp.GetTimestampReturn.Add(0);
            _executeTimestamp.GetTimestampReturn.Add(0);
            foreach (var executorState in _executorStates)
            {
                ((MockExecutorState)executorState).CallOrder = _callOrder;
            }
        }

        /**
         * @brief Verifies that resetting the state machine triggers execution of the
         * Reset state, preparing the system for a fresh automation cycle
         * 
         * When the bot needs to start a new automation session, the state machine resets
         * to its initial state. This executes any cleanup or initialization logic in the
         * Reset state, ensuring the bot starts fresh without leftover state from
         * previous operations.
         */
        private void _testResettingStateMachineExecutesAtReset()
        {
            var stateMachine = _fixture();
            _setupExecutorStates();
            stateMachine.Reset();
            var executorState = (MockExecutorState)
                _executorStates[(int)MacroExecutorStateTypes.Reset];
            Debug.Assert(executorState.ExecuteCalls == 1);
        }

        /**
         * @brief Verifies that transmitting the state machine correctly executes the
         * sequence of states defined by their transition rules
         * 
         * When the bot's main automation loop runs, it transmits the state machine,
         * which executes the current state and transitions to the next state based on
         * what each state returns. This test creates a custom transition chain
         * and verifies the states execute in the expected order, demonstrating that
         * the state machine properly follows non-sequential transitions.
         */
        private void _testTransmittingStateMachineRunsNextStates()
        {
            var stateMachine = _fixture();
            _setupExecutorStates();
            for (int i = 0; i < _executorStates.Count; i++)
            {
                stateMachine.Transmit();
            }
            var ref0 = new TestUtilities().Reference(_executorStates[0]);
            var ref1 = new TestUtilities().Reference(_executorStates[1]);
            var ref2 = new TestUtilities().Reference(_executorStates[2]);
            var ref3 = new TestUtilities().Reference(_executorStates[3]);
            var ref4 = new TestUtilities().Reference(_executorStates[4]);
            Debug.Assert(_callOrder.Count == 5);
            Debug.Assert(_callOrder[0] == ref1 + "Execute");
            Debug.Assert(_callOrder[1] == ref3 + "Execute");
            Debug.Assert(_callOrder[2] == ref0 + "Execute");
            Debug.Assert(_callOrder[3] == ref2 + "Execute");
            Debug.Assert(_callOrder[4] == ref4 + "Execute");
        }

        /**
         * @brief Verifies that the state machine sleeps for the remaining time needed
         * to maintain the configured execution frequency
         * 
         * To prevent the bot from overwhelming the system, the state machine limits
         * how often it executes states. This test sets an execution frequency
         * and verifies that if the last execution was too recent, the state machine
         * sleeps for the appropriate amount of time to maintain the desired frequency.
         */
        private void _testTransmittingStateMachineSleepsRemaining()
        {
            var stateMachine = _fixture();
            _executorStates.Add(new MockExecutorState());
            _executorStates.Add(new MockExecutorState());
            ((MockExecutorState)_executorStates[0]).ExecuteReturn.Add(0);
            ((MockExecutorState)_executorStates[1]).ExecuteReturn.Add(1);
            _executeTimestamp.GetTimestampReturn.Add(87);
            _context.ExecutionFrequency = 0.01;
            stateMachine.Transmit();
            Debug.Assert(_sleeper.SleepCalls == 1);
            Debug.Assert(_sleeper.SleepCallArg_milliseconds[0] == 13000);
        }

        /**
         * @brief Verifies that the state machine does not sleep when the execution
         * time already exceeds the configured frequency period
         * 
         * If the bot's state execution takes longer than the configured frequency
         * period (meaning it's already "behind schedule"), the state machine should
         * not add any additional delay. This prevents the bot from falling further
         * behind and ensures maximum responsiveness when processing is slow.
         */
        private void _testTransmittingStateMachineDoesntSleepWhenDelayed()
        {
            var stateMachine = _fixture();
            _executorStates.Add(new MockExecutorState());
            _executorStates.Add(new MockExecutorState());
            ((MockExecutorState)_executorStates[0]).ExecuteReturn.Add(0);
            ((MockExecutorState)_executorStates[1]).ExecuteReturn.Add(1);
            _executeTimestamp.GetTimestampReturn.Add(101);
            _context.ExecutionFrequency = 0.01;
            stateMachine.Transmit();
            Debug.Assert(_sleeper.SleepCalls == 0);
        }

        /**
         * @brief Verifies that the state machine does not attempt to sleep when the
         * execution frequency is set to zero (disabled)
         * 
         * If the bot's configuration has execution frequency set to zero (meaning no
         * rate limiting), the state machine should not add any sleep delays.
         */
        private void _testTransmittingStateMachineDoesntSleepWhenInvalidFrequency()
        {
            var stateMachine = _fixture();
            _executorStates.Add(new MockExecutorState());
            _executorStates.Add(new MockExecutorState());
            ((MockExecutorState)_executorStates[0]).ExecuteReturn.Add(0);
            ((MockExecutorState)_executorStates[1]).ExecuteReturn.Add(1);
            _executeTimestamp.GetTimestampReturn.Add(101);
            _context.ExecutionFrequency = 0;
            stateMachine.Transmit();
            Debug.Assert(_sleeper.SleepCalls == 0);
        }

        /**
         * @brief Verifies that the botting orchestrator thread and state are properly
         * injected into the state machine at startup
         * 
         * When the bot initializes, it needs to provide the state machine with the
         * thread that controls macro killing operations. This test ensures that
         * the botting thread and its state object are correctly wired to the state
         * machine's botting controller, allowing the state machine to start and stop
         * monster killing when the state transitions.
         */
        private void _testInjectingBottingControllerThreadDependency()
        {
            var stateMachine = _fixture();
            var thread = new MockThread(new ThreadRunningState());
            var state = new KeystrokeTransmitterThreadState(0, KeystrokeTransmitterThreadType.Botting);
            thread.ThreadStateReturn.Add(state);
            stateMachine.Inject(SystemInjectType.ThreadDependency, thread);
            Debug.Assert(_bottingController.SetOrchestratorCalls == 1);
            Debug.Assert(_bottingController.SetOrchestratorCallArg_orchestrator[0] == thread);
            Debug.Assert(_bottingController.SetOrchestratorThreadStateCalls == 1);
            Debug.Assert(_bottingController.SetOrchestratorThreadStateCallArg_threadState[0] == state);
        }

        /**
         * @brief Verifies that the rune navigation orchestrator thread and state are
         * properly injected into the state machine
         * 
         * When the bot initializes, it needs to provide the state machine with the
         * thread that controls navigation to runes. This test ensures that the rune
         * navigation thread and its state object are correctly wired to the state
         * machine's runeing controller, allowing the state machine to start and stop
         * rune approach when needed.
         */
        private void _testInjectingRuneingControllerThreadDependency()
        {
            var stateMachine = _fixture();
            var thread = new MockThread(new ThreadRunningState());
            var state = new KeystrokeTransmitterThreadState(0, KeystrokeTransmitterThreadType.Runeing);
            thread.ThreadStateReturn.Add(state);
            stateMachine.Inject(SystemInjectType.ThreadDependency, thread);
            Debug.Assert(_runeingController.SetOrchestratorCalls == 1);
            Debug.Assert(_runeingController.SetOrchestratorCallArg_orchestrator[0] == thread);
            Debug.Assert(_runeingController.SetOrchestratorThreadStateCalls == 1);
            Debug.Assert(_runeingController.SetOrchestratorThreadStateCallArg_threadState[0] == state);
        }

        /**
         * @brief Verifies that the puzzle solving orchestrator thread and state are
         * properly injected into the state machine
         * 
         * When the bot initializes, it needs to provide the state machine with the
         * thread that controls rune puzzle solving. This test ensures that the solving
         * thread and its state object are correctly wired to the state machine's
         * solving controller, allowing the state machine to start and stop puzzle
         * solving when the bot reaches a rune.
         */
        private void _testInjectingSolvingControllerThreadDependency()
        {
            var stateMachine = _fixture();
            var thread = new MockThread(new ThreadRunningState());
            var state = new KeystrokeTransmitterThreadState(0, KeystrokeTransmitterThreadType.Solving);
            thread.ThreadStateReturn.Add(state);
            stateMachine.Inject(SystemInjectType.ThreadDependency, thread);
            Debug.Assert(_solvingController.SetOrchestratorCalls == 1);
            Debug.Assert(_solvingController.SetOrchestratorCallArg_orchestrator[0] == thread);
            Debug.Assert(_solvingController.SetOrchestratorThreadStateCalls == 1);
            Debug.Assert(_solvingController.SetOrchestratorThreadStateCallArg_threadState[0] == state);
        }

        /**
         * @brief Verifies that bot configuration settings are properly injected into
         * the state machine's context
         * 
         * When the bot loads a configuration file, settings like macro execution
         * frequency, rune activation cooldown period, and solve check timeout must
         * be passed to the state machine. These settings control how often the bot
         * checks for macros, how long it waits between rune attempts, and how long
         * it waits after solving before giving up on another rune.
         */
        private void _testInjectingConfiguration()
        {
            var stateMachine = _fixture();
            var data = new MaplestoryBotConfiguration
            {
                MacroSettings = new MacroSettings
                {
                    CheckFrequency = 12,
                    RuneActivationPeriod = 23,
                    SolveCheckTimeout = 34
                }
            };
            stateMachine.Inject(SystemInjectType.Configuration, data);
            Debug.Assert(_context.ExecutionFrequency == 12);
            Debug.Assert(_context.RuneActivationPeriod == 23);
            Debug.Assert(_context.SolveCheckTimeout == 34);
        }

        /**
         * @brief Verifies that the botting data model is properly injected into the
         * state machine's context
         * 
         * The botting model contains all runtime data about rune frames, waypoints,
         * and automation state. This test ensures that the model is correctly passed
         * to the state machine, allowing the various states to access automation data
         * as they execute.
         */
        private void _testInjectingBottingModel()
        {
            var stateMachine = _fixture();
            var data = new BottingModel();
            stateMachine.Inject(SystemInjectType.BottingModel, data);
            Debug.Assert(_context.BottingModel == data);
        }

        public void Run()
        {
            _testResettingStateMachineExecutesAtReset();
            _testTransmittingStateMachineRunsNextStates();
            _testTransmittingStateMachineSleepsRemaining();
            _testTransmittingStateMachineDoesntSleepWhenDelayed();
            _testTransmittingStateMachineDoesntSleepWhenInvalidFrequency();
            _testInjectingBottingControllerThreadDependency();
            _testInjectingRuneingControllerThreadDependency();
            _testInjectingSolvingControllerThreadDependency();
            _testInjectingConfiguration();
            _testInjectingBottingModel();
        }
    }


    public class MacroExecutorThreadTests
    {
        private MockKeystrokeTransmitterThreadHelper _executorThreadHelper = new();

        private MockResetEvent _executionEvent = new MockResetEvent();

        private MockRunningState _transmittingState = new MockRunningState();

        private MockRunningState _runningState = new MockRunningState();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)MacroExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Macro
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
                        MacroOrchestratorThreadInjectType.Stop, null
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
            return new MacroExecutorThread(
                _executionEvent,
                _executorThreadHelper,
                _threadState,
                _transmittingState,
                _runningState
            );
        }

        /**
         * @brief Verifies the handshake sequence when the botting executor starts its
         * transmission routine
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
                keystrokeTransmitterExecutorThread.Inject(MacroOrchestratorThreadInjectType.Start, 0);
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
                MacroOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(threadState.SetStateCallArg_state[0] == (int)MacroExecutorThreadedUpdate.Starting);
            Debug.Assert(threadState.SetStateCallArg_state[1] == (int)MacroExecutorThreadedUpdate.Started);
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
                keystrokeTransmitterExecutorThread.Inject(MacroOrchestratorThreadInjectType.Stop, 0);
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
                Debug.Assert(threadState.SetStateCallArg_state[0] == (int)MacroExecutorThreadedUpdate.Stopping);
                Debug.Assert(threadState.SetStateCallArg_state[1] == (int)MacroExecutorThreadedUpdate.Stopped);
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
                    (int)MacroExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Macro
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var start = MacroOrchestratorThreadInjectType.Start;
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
                    (int)MacroExecutorThreadedUpdate.Started,
                    KeystrokeTransmitterThreadType.Macro
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var stop = MacroOrchestratorThreadInjectType.Stop;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(stop, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == 0);
            }
        }

        /**
         * @brief Verifies that the executor thread sends proper state update notifications
         * when starting its macro transmission routine
         * 
         * When the macro system begins executing a subroutine (such as monster killing
         * or rune navigation), the orchestrator needs to know what state the executor
         * is in. This test ensures that the executor sends a Starting notification
         * immediately when the start command is received, and a Started notification
         * once the routine is fully operational. These notifications allow the
         * orchestrator to coordinate activities, such as waiting for the executor to
         * be ready before sending commands.
         */
        private void _testExecutorThreadSendsStartStateUpdates()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                (int)MacroExecutorThreadedUpdate.Started,
                KeystrokeTransmitterThreadType.Macro
            );
            List<object> dataTypes = [];
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            keystrokeTransmitterExecutorThread.Inject(
                SystemInjectType.InjectAction, new InjectAction(
                    (dataType, data) => { dataTypes.Add(dataType); }
                )
            );
            var start = MacroOrchestratorThreadInjectType.Start;
            _transmittingState.IsRunningReturn.Add(false);
            keystrokeTransmitterExecutorThread.Inject(start, 0);
            Debug.Assert(dataTypes.Count == 2);
            Debug.Assert(dataTypes[0] is MacroExecutorThreadedUpdate.Starting);
            Debug.Assert(dataTypes[1] is MacroExecutorThreadedUpdate.Started);
        }

        /**
         * @brief Verifies that the executor thread broadcasts state change notifications
         * when starting its macro transmission routine
         * 
         * When the macro system begins executing a subroutine (such as monster killing,
         * rune navigation, or puzzle solving), the executor thread sends out state
         * updates to notify any interested listeners. These notifications could be used
         * by the UI to update a status display or by other components that
         * need to react to state changes.
         */
        private void _testExecutorThreadSendsStopStateUpdates()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                (int)MacroExecutorThreadedUpdate.Started,
                KeystrokeTransmitterThreadType.Macro
            );
            List<object> dataTypes = [];
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            keystrokeTransmitterExecutorThread.Inject(
                SystemInjectType.InjectAction, new InjectAction(
                    (dataType, data) => { dataTypes.Add(dataType); }
                )
            );
            var stop = MacroOrchestratorThreadInjectType.Stop;
            _transmittingState.IsRunningReturn.Add(false);
            keystrokeTransmitterExecutorThread.Inject(stop, 0);
            Debug.Assert(dataTypes.Count == 2);
            Debug.Assert(dataTypes[0] is MacroExecutorThreadedUpdate.Stopping);
            Debug.Assert(dataTypes[1] is MacroExecutorThreadedUpdate.Stopped);
        }

        /**
         * @brief Verifies that the executor thread broadcasts state change notifications
         * when stopping its macro transmission routine
         * 
         * When the macro system needs to stop a subroutine, the executor thread sends
         * out state updates to notify any interested listeners. These notifications
         * could be used by the UI to update a status display or by other components that
         * need to react to state changes.
         */
        public void Run()
        {
            _testExecutorStartingHandshake();
            _testExecutorStartingHandshakeSetsThreadStates();
            _testExecutorStoppingHandshake();
            _testExecutorStoppingHandshakeSetsThreadStates();
            _testExecutorThreadLoopTransmitsWhenStarted();
            _testExecutorThreadLoopDoesntTransmitWhenStopped();
            _testExecutorThreadSendsStartStateUpdates();
            _testExecutorThreadSendsStopStateUpdates();
        }
    }


    public class MacroOrchestratorThreadTests
    {
        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.Macro
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
            return new MacroOrchestratorThread(
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
                0, KeystrokeTransmitterThreadType.Macro
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
                0, KeystrokeTransmitterThreadType.Macro
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
                123, KeystrokeTransmitterThreadType.Macro
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            var max = MacroOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                transmitterOrchestratorThread.Inject(
                    (MacroOrchestratorThreadInjectType)i, 0
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
            var max = MacroOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var transmitterOrchestratorThread = _fixture(threadState);
                transmitterOrchestratorThread.Inject((MacroOrchestratorThreadInjectType)i, 0);
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
         * @brief Verifies that the orchestrator makes itself available as a thread
         * dependency
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
                    123, KeystrokeTransmitterThreadType.Macro
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


    public class MacroSystemTestSuite
    {
        public void Run()
        {
            new OrchestratorControllerTests().Run();
            new MacroExecutorStateResetTests().Run();
            new MacroExecuteStateIdleTests().Run();
            new MacroExecutorStateBottingTests().Run();
            new MacroExecutorStateRuneingTests().Run();
            new MacroExecutorStateSolvingTests().Run();
            new MacroExecutorStateSolvedCheckTests().Run();
            new MacroExecutorThreadStateMachineTests().Run();
            new MacroExecutorThreadTests().Run();
            new MacroOrchestratorThreadTests().Run();
        }
    }
}
