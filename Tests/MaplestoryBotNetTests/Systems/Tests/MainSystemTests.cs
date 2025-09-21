using System.Diagnostics;
using MaplestoryBotNet.Systems;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;

namespace MaplestoryBotNetTests.Systems.Tests
{
    /**
     * @class MainSubSystemsTests
     * 
     * @brief Unit tests for verifying main subsystem coordination and prioritization
     * 
     * This test class validates that the main system correctly coordinates multiple subsystems,
     * ensuring proper build order based on dependencies and correct execution order based on
     * initialization, start, and update priorities during system operation.
     */
    public class MainSubSystemsTests
    {
        private List<AbstractSystemBuilder> _systemBuilders = [];

        private List<AbstractSystem> _systems = [];

        private List<SubSystemInformation> _subSystemInfo = [];

        private List<string> _callOrder = [];

        /**
         * @brief Creates a test environment with subsystem dependencies
         * 
         * @return List of configured SubSystemInformation instances
         * 
         * Prepares a test environment with multiple subsystems having complex dependencies
         * to verify proper build order and execution sequencing during system initialization
         * and operation.
         */
        private List<SubSystemInformation> _subSystemInfoFixture()
        {
            _callOrder = [];
            _systemBuilders = [
                new MockSystemBuilder(),
                new MockSystemBuilder(),
                new MockSystemBuilder()
            ];
            _systems = [
                new MockSystem(),
                new MockSystem(),
                new MockSystem()
            ];
            ((MockSystemBuilder)_systemBuilders[0]).BuildReturn.Add(_systems[0]);
            ((MockSystemBuilder)_systemBuilders[1]).BuildReturn.Add(_systems[1]);
            ((MockSystemBuilder)_systemBuilders[2]).BuildReturn.Add(_systems[2]);
            for (int i = 0; i < _systemBuilders.Count; i++)
            {
                ((MockSystemBuilder)_systemBuilders[i]).CallOrder = _callOrder;
            }
            for (int i = 0; i < _systemBuilders.Count; i++)
            {
                ((MockSystem)_systems[i]).CallOrder = _callOrder;
            }
            _subSystemInfo = [
                new SubSystemInformation(_systemBuilders[0], [], 3, 2, 3),
                new SubSystemInformation(_systemBuilders[1], [], 2, 3, 1),
                new SubSystemInformation(_systemBuilders[2], [], 1, 1, 2)
            ];
            _subSystemInfo[0].BuildDependencies.Add(_subSystemInfo[1]);
            _subSystemInfo[0].BuildDependencies.Add(_subSystemInfo[2]);
            _subSystemInfo[1].BuildDependencies.Add(_subSystemInfo[2]);
            return _subSystemInfo;
        }

        /**
         * @brief Tests proper subsystem building order based on dependencies
         * 
         * Validates that the main system correctly builds subsystems in the proper
         * order based on their dependencies, ensuring that dependent systems are
         * built before the systems that require them.
         */
        private void _testInstantiationOfMainSubSystemBuildsSubSystems()
        {
            new MainSubSystem(_subSystemInfoFixture());
            var buildRef0 = new TestUtilities().Reference(_systemBuilders[0]) + "Build";
            var buildRef1 = new TestUtilities().Reference(_systemBuilders[1]) + "Build";
            var buildRef2 = new TestUtilities().Reference(_systemBuilders[2]) + "Build";
            var buildIndex0 = _callOrder.IndexOf(buildRef0);
            var buildIndex1 = _callOrder.IndexOf(buildRef1);
            var buildIndex2 = _callOrder.IndexOf(buildRef2);
            Debug.Assert(buildIndex0 != -1);
            Debug.Assert(buildIndex1 != -1);
            Debug.Assert(buildIndex2 != -1);
            Debug.Assert(buildIndex2 < buildIndex1);
            Debug.Assert(buildIndex1 < buildIndex0);
        }

        /**
         * @brief Tests proper dependency injection during subsystem building
         * 
         * Validates that the main system correctly injects dependency references
         * during the building process, ensuring that each subsystem receives the
         * necessary dependencies for proper operation.
         */
        public void _testInstantiationOfMainSubSystemBuildsSubSystemsWithDependencies()
        {
            new MainSubSystem(_subSystemInfoFixture());
            var ref0 = new TestUtilities().Reference(_systemBuilders[0]);
            var ref1 = new TestUtilities().Reference(_systemBuilders[1]);
            var ref2 = new TestUtilities().Reference(_systemBuilders[2]);
            var buildIndex0 = _callOrder.IndexOf(ref0 + "Build");
            var buildIndex1 = _callOrder.IndexOf(ref1 + "Build");
            var buildIndex2 = _callOrder.IndexOf(ref2 + "Build");
            Debug.Assert(buildIndex0 != -1);
            Debug.Assert(buildIndex1 != -1);
            Debug.Assert(buildIndex2 != -1);
            Debug.Assert(_callOrder[buildIndex1 - 1] == ref1 + "WithArg");
            Debug.Assert(_callOrder[buildIndex0 - 1] == ref0 + "WithArg");
            Debug.Assert(_callOrder[buildIndex0 - 2] == ref0 + "WithArg");
            Debug.Assert(((MockSystemBuilder)_systemBuilders[0]).WithArgCalls == 2);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[0]).WithArgCallArg_arg[0] == _systems[1]);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[0]).WithArgCallArg_arg[1] == _systems[2]);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[1]).WithArgCalls == 1);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[1]).WithArgCallArg_arg[0] == _systems[2]);
        }

        /**
         * @brief Tests proper initialization order based on priority
         * 
         * Validates that the main system initializes subsystems in the correct
         * order based on their initialization priorities, ensuring that higher
         * priority systems are initialized before lower priority ones.
         */
        private void _testInitializeCallsSubSystemInitializationInOrder()
        {
            var mainSubSystem = new MainSubSystem(_subSystemInfoFixture());
            _callOrder.Clear();
            mainSubSystem.Initialize();
            var ref0 = new TestUtilities().Reference(_systems[0]);
            var ref1 = new TestUtilities().Reference(_systems[1]);
            var ref2 = new TestUtilities().Reference(_systems[2]);
            Debug.Assert(_callOrder.Count == 3);
            Debug.Assert(_callOrder[0] == ref2 + "InitializeSystem");
            Debug.Assert(_callOrder[1] == ref1 + "InitializeSystem");
            Debug.Assert(_callOrder[2] == ref0 + "InitializeSystem");
        }

        /**
         * @brief Tests proper start order based on priority
         * 
         * Validates that the main system starts subsystems in the correct
         * order based on their start priorities, ensuring that higher
         * priority systems are started before lower priority ones.
         */
        private void _testStartCallsSubSystemStartInOrder()
        {
            var mainSubSystem = new MainSubSystem(_subSystemInfoFixture());
            _callOrder.Clear();
            mainSubSystem.Start();
            var ref0 = new TestUtilities().Reference(_systems[0]);
            var ref1 = new TestUtilities().Reference(_systems[1]);
            var ref2 = new TestUtilities().Reference(_systems[2]);
            Debug.Assert(_callOrder.Count == 3);
            Debug.Assert(_callOrder[0] == ref2 + "StartSystem");
            Debug.Assert(_callOrder[1] == ref0 + "StartSystem");
            Debug.Assert(_callOrder[2] == ref1 + "StartSystem");
        }

        /**
         * @brief Tests proper update order based on priority
         * 
         * Validates that the main system updates subsystems in the correct
         * order based on their update priorities, ensuring that higher
         * priority systems are updated before lower priority ones.
         */
        private void _testUpdateCallsSubSystemUpdateInOrder()
        {
            var mainSubSystem = new MainSubSystem(_subSystemInfoFixture());
            _callOrder.Clear();
            mainSubSystem.Update();
            var ref0 = new TestUtilities().Reference(_systems[0]);
            var ref1 = new TestUtilities().Reference(_systems[1]);
            var ref2 = new TestUtilities().Reference(_systems[2]);
            Debug.Assert(_callOrder.Count == 3);
            Debug.Assert(_callOrder[0] == ref1 + "UpdateSystem");
            Debug.Assert(_callOrder[1] == ref2 + "UpdateSystem");
            Debug.Assert(_callOrder[2] == ref0 + "UpdateSystem");
        }

        /**
         * @brief Executes all main subsystem coordination tests
         * 
         * Runs the complete test suite to ensure the main system correctly
         * coordinates all subsystems, providing confidence in the reliability
         * of system initialization, startup, and operation.
         */
        public void Run()
        {
            _testInstantiationOfMainSubSystemBuildsSubSystems();
            _testInstantiationOfMainSubSystemBuildsSubSystemsWithDependencies();
            _testInitializeCallsSubSystemInitializationInOrder();
            _testStartCallsSubSystemStartInOrder();
            _testUpdateCallsSubSystemUpdateInOrder();
        }
    }


    /**
     * @class MainSubSystemThreadTests
     * 
     * @brief Unit tests for verifying threaded execution of the main subsystem
     * 
     * This test class validates that the main subsystem correctly operates within a threaded
     * environment, ensuring proper initialization, startup, and update sequencing while
     * maintaining the correct execution order and frequency during automated operation.
     */
    public class MainSubSystemThreadTests
    {
        private MockRunningState _runningState = new MockRunningState();

        private MockSystem _mainSubSystem = new MockSystem();

        /**
         * @brief Creates a test environment for threaded subsystem testing
         * 
         * @return Configured MainSubSystemThread instance
         * 
         * Prepares a test environment with mock running state and subsystem components
         * to verify threaded operation.
         */
        private MainSubSystemThread _fixture()
        {
            _runningState = new MockRunningState();
            _mainSubSystem = new MockSystem();
            return new MainSubSystemThread(_mainSubSystem, _runningState);
        }

        /**
         * @brief Configures the running state for a specific number of execution cycles
         * 
         * @param count Number of active update cycles to simulate
         * 
         * Sets up a controlled test scenario that simulates the thread's running state
         * transitions, allowing precise testing of update behavior during different
         * operational phases without relying on timing or external signals.
         */
        private void _setLoopCount(int count)
        {
            _runningState.IsRunningReturn.Add(false);
            for (int i = 0; i < count; i++)
                _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
        }

        /**
         * @brief Tests single initialization and startup sequence
         * 
         * Validates that the threaded subsystem correctly performs initialization
         * and startup sequences exactly once, ensuring proper system preparation
         * without redundant or missing initialization calls.
         */
        private void _testStartThreadCallsInitializeAndStartOnce()
        {
            var mainSubSystemThread = _fixture();
            _setLoopCount(0);
            mainSubSystemThread.ThreadStart();
            mainSubSystemThread.ThreadJoin(10000);
            var initializeRef = new TestUtilities().Reference(_mainSubSystem) + "InitializeSystem";
            var initializeIndex = _mainSubSystem.CallOrder.IndexOf(initializeRef);
            var startRef = new TestUtilities().Reference(_mainSubSystem) + "StartSystem";
            var startIndex = _mainSubSystem.CallOrder.IndexOf(startRef);
            Debug.Assert(initializeIndex != -1);
            Debug.Assert(startIndex != -1);
            Debug.Assert(initializeIndex < startIndex);
            Debug.Assert(_mainSubSystem.InitializeSystemCalls == 1);
            Debug.Assert(_mainSubSystem.StartSystemCalls == 1);
            Debug.Assert(_mainSubSystem.UpdateSystemCalls == 0);
        }

        /**
         * @brief Tests proper update sequencing after initialization
         * 
         * Validates that the threaded subsystem correctly performs update operations
         * after initialization and startup, ensuring the proper sequence of operations
         * and the correct number of update cycles during threaded execution.
         */
        private void _testStartThreadCallsUpdateSystemAfterInitializationAndStart()
        {
            for (int i = 0; i < 10; i++)
            {
                var mainSubSystemThread = _fixture();
                _setLoopCount(i);
                mainSubSystemThread.ThreadStart();
                mainSubSystemThread.ThreadJoin(10000);
                var initializeRef = new TestUtilities().Reference(_mainSubSystem) + "InitializeSystem";
                var initializeIndex = _mainSubSystem.CallOrder.IndexOf(initializeRef);
                var startRef = new TestUtilities().Reference(_mainSubSystem) + "StartSystem";
                var startIndex = _mainSubSystem.CallOrder.IndexOf(startRef);
                var updateRef = new TestUtilities().Reference(_mainSubSystem) + "UpdateSystem";
                var updateIndices = new List<int>();
                for (int j = 0; j < _mainSubSystem.CallOrder.Count; j++)
                {
                    if (_mainSubSystem.CallOrder[j] == updateRef)
                    {
                        updateIndices.Add(j);
                    }
                }
                Debug.Assert(updateIndices.Count == i);
                for (int j = 0; j < updateIndices.Count; j++)
                {
                    var index = updateIndices[j];
                    Debug.Assert(index > initializeIndex);
                    Debug.Assert(index > startIndex);
                }
            }
        }

        /**
         * @brief Executes all threaded subsystem tests
         * 
         * Runs the complete test suite to ensure the main subsystem correctly operates
         * in a threaded environment, providing confidence in the reliability of
         * initialization, startup, and update sequencing during automated operation.
         */
        public void Run()
        {
            _testStartThreadCallsInitializeAndStartOnce();
            _testStartThreadCallsUpdateSystemAfterInitializationAndStart();
        }
    }

    /**
     * @class MainSystemTestSuite
     * 
     * @brief Comprehensive test suite for main subsystem functionality
     * 
     * This class aggregates all tests related to the main subsystem, providing
     * a single entry point to execute the full suite of unit tests that validate
     * the correct coordination, prioritization, and threaded operation of subsystems.
     */
    public class MainSystemTestSuite
    {
        public void Run()
        {
            new MainSubSystemsTests().Run();
            new MainSubSystemThreadTests().Run();
        }
    }
}
