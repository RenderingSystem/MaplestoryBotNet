using System.Diagnostics;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using MaplestoryBotNetTests.UserInterface.Tests.Mocks;


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

        private List<SystemInformation> _subSystemInfo = [];

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
        private List<SystemInformation> _subSystemInfoFixture()
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
                new SystemInformation(_systemBuilders[0], [], [123, 234], 3, 2, 3),
                new SystemInformation(_systemBuilders[1], [], [234, 345], 2, 3, 1),
                new SystemInformation(_systemBuilders[2], [], [345, 456], 1, 1, 2)
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
            Debug.Assert(_callOrder[buildIndex1 - 3] == ref1 + "WithArg");
            Debug.Assert(_callOrder[buildIndex0 - 4] == ref0 + "WithArg");
            Debug.Assert(_callOrder[buildIndex0 - 3] == ref0 + "WithArg");
            Debug.Assert(((MockSystemBuilder)_systemBuilders[0]).WithArgCalls == 4);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[1]).WithArgCalls == 3);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[2]).WithArgCalls == 2);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[0]).WithArgCallArg_arg[0] == _systems[1]);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[0]).WithArgCallArg_arg[1] == _systems[2]);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[1]).WithArgCallArg_arg[0] == _systems[2]);
        }

        /**
         * @brief Tests proper build object injection during subsystem construction
         * 
         * Validates that the main system correctly injects build objects (configuration
         * and data objects) into each subsystem during the building process. Ensures
         * that build objects are provided in the specified order and that each subsystem
         * receives exactly the build objects configured for it, in addition to any
         * required dependency references.
         */
        public void _testInstantiationOfMainSubSystemBuildsSubSystemsWithBuildObjects()
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
            Debug.Assert(_callOrder[buildIndex2 - 2] == ref2 + "WithArg");
            Debug.Assert(_callOrder[buildIndex2 - 1] == ref2 + "WithArg");
            Debug.Assert(_callOrder[buildIndex1 - 2] == ref1 + "WithArg");
            Debug.Assert(_callOrder[buildIndex1 - 1] == ref1 + "WithArg");
            Debug.Assert(_callOrder[buildIndex0 - 2] == ref0 + "WithArg");
            Debug.Assert(_callOrder[buildIndex0 - 1] == ref0 + "WithArg");
            Debug.Assert(((MockSystemBuilder)_systemBuilders[0]).WithArgCalls == 4);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[1]).WithArgCalls == 3);
            Debug.Assert(((MockSystemBuilder)_systemBuilders[2]).WithArgCalls == 2);
            Debug.Assert((int)((MockSystemBuilder)_systemBuilders[0]).WithArgCallArg_arg[2] == 123);
            Debug.Assert((int)((MockSystemBuilder)_systemBuilders[0]).WithArgCallArg_arg[3] == 234);
            Debug.Assert((int)((MockSystemBuilder)_systemBuilders[1]).WithArgCallArg_arg[1] == 234);
            Debug.Assert((int)((MockSystemBuilder)_systemBuilders[1]).WithArgCallArg_arg[2] == 345);
            Debug.Assert((int)((MockSystemBuilder)_systemBuilders[2]).WithArgCallArg_arg[0] == 345);
            Debug.Assert((int)((MockSystemBuilder)_systemBuilders[2]).WithArgCallArg_arg[1] == 456);
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
         * @brief Tests runtime dependency injection ordering and data consistency
         * 
         * Validates that the main system performs runtime dependency injection
         * to all subsystems in the correct sequence based on their update priorities.
         * Ensures that injection data is consistently delivered with proper type
         * and value to every subsystem, maintaining data integrity across the system.
         */
        private void _testUpdateCallsSubSystemInjectInUpdateOrder()
        {
            var mainSubSystem = new MainSubSystem(_subSystemInfoFixture());
            _callOrder.Clear();
            mainSubSystem.Inject((SystemInjectType)0x1234, 0x2345);
            var ref0 = new TestUtilities().Reference(_systems[0]);
            var ref1 = new TestUtilities().Reference(_systems[1]);
            var ref2 = new TestUtilities().Reference(_systems[2]);
            Debug.Assert(_callOrder.Count == 3);
            Debug.Assert(_callOrder[0] == ref1 + "Inject");
            Debug.Assert(_callOrder[1] == ref2 + "Inject");
            Debug.Assert(_callOrder[2] == ref0 + "Inject");
            for (int i = 0; i < _systems.Count; i++)
            {
                Debug.Assert(((MockSystem)_systems[i]).InjectCallArg_dataType[0] == (SystemInjectType)0x1234);
                Debug.Assert((int?)((MockSystem)_systems[i]).InjectCallArg_data[0] == 0x2345);
            }
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
            _testInstantiationOfMainSubSystemBuildsSubSystemsWithBuildObjects();
            _testInitializeCallsSubSystemInitializationInOrder();
            _testStartCallsSubSystemStartInOrder();
            _testUpdateCallsSubSystemUpdateInOrder();
            _testUpdateCallsSubSystemInjectInUpdateOrder();
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
            mainSubSystemThread.Start();
            mainSubSystemThread.Join(10000);
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
                mainSubSystemThread.Start();
                mainSubSystemThread.Join(10000);
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
         * @brief Tests thread state transition from stopped to running
         * 
         * Validates that starting the main subsystem thread properly transitions
         * the thread state from inactive to active. Ensures the thread state
         * accurately reflects the operational status after thread initialization
         * and startup sequence completion.
         */
        private void _testStartThreadSetsTheStateOfTheThreadToTrue()
        {
            var mainSubSystemThread = _fixture();
            _setLoopCount(1);
            Debug.Assert((bool?)mainSubSystemThread.State() == false);
            mainSubSystemThread.Start();
            mainSubSystemThread.Join(10000);
            Debug.Assert((bool?)mainSubSystemThread.State() == true);
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
            _testStartThreadSetsTheStateOfTheThreadToTrue();
        }
    }


    /**
     * @class MainSubSystemInfoListTests
     * 
     * @brief Unit tests for subsystem configuration and dependency management
     * 
     * This test class validates that the main subsystem information list correctly
     * configures all required subsystems with proper dependencies and execution priorities.
     * Ensures the system maintains correct dependency relationships and priority ordering
     * between keyboard input, screen capture, and configuration management subsystems.
     */
    public class MainSubSystemInfoListTests
    {
        /**
         * @brief Tests complete subsystem enumeration
         * 
         * Validates that the main system information list contains all required
         * subsystems for proper system operation, ensuring no essential components
         * are missing from the configuration.
         */
        private void _testGetSubSystemInfoObtainsAllSubSystems()
        {
            var subSystemInfoList = new MainSubSystemInfoList();
            var subSystemInfo = subSystemInfoList.GetSubSystemInfo();
            Debug.Assert(subSystemInfo.Count == 3);
        }

        /**
         * @brief Tests keyboard subsystem configuration
         * 
         * Validates that the keyboard input subsystem is properly configured
         * with correct execution priorities and no external dependencies,
         * ensuring responsive user input handling throughout system operation.
         */
        private void _testGetSubSystemInfoObtainsCorrectKeyboardInfo()
        {
            var subSystemInfoList = new MainSubSystemInfoList();
            var subSystemInfo = subSystemInfoList.GetSubSystemInfo();
            var keyboardInfo = subSystemInfo.FirstOrDefault(
                info => info.SystemBuilder is KeyboardSystemBuilder
            );
            Debug.Assert(keyboardInfo != null);
            Debug.Assert(keyboardInfo.BuildDependencies.Count == 0);
            Debug.Assert(keyboardInfo.InitializationPriority == 2);
            Debug.Assert(keyboardInfo.StartPriority == 2);
            Debug.Assert(keyboardInfo.UpdatePriority == 2);
        }

        /**
         * @brief Tests screen capture subsystem configuration
         * 
         * Validates that the screen capture subsystem is properly configured
         * with appropriate execution priorities and no external dependencies,
         * ensuring reliable screen capture functionality during operation.
         */
        private void _testGetSubSystemInfoObtainsCorrectScreenCaptureInfo()
        {
            var subSystemInfoList = new MainSubSystemInfoList();
            var subSystemInfo = subSystemInfoList.GetSubSystemInfo();
            var screenCaptureInfo = subSystemInfo.FirstOrDefault(
                info => info.SystemBuilder is GameScreenCaptureSystemBuilder
            );
            Debug.Assert(screenCaptureInfo != null);
            Debug.Assert(screenCaptureInfo.BuildDependencies.Count == 0);
            Debug.Assert(screenCaptureInfo.InitializationPriority == 1);
            Debug.Assert(screenCaptureInfo.StartPriority == 1);
            Debug.Assert(screenCaptureInfo.UpdatePriority == 1);
        }

        /**
         * @brief Tests configuration subsystem dependencies
         * 
         * Validates that the configuration management subsystem properly depends
         * on both keyboard and screen capture subsystems, ensuring configuration
         * services are initialized after all dependent systems are available.
         */
        private void _testGetSubSystemInfoObtainsCorrectConfigurationSystemInfo()
        {
            var subSystemInfoList = new MainSubSystemInfoList();
            var subSystemInfo = subSystemInfoList.GetSubSystemInfo();
            var configInfo = subSystemInfo.FirstOrDefault(
                info => info.SystemBuilder is ConfigurationSystemBuilder
            );
            Debug.Assert(configInfo != null);
            Debug.Assert(configInfo.BuildDependencies.Count == 2);
            Debug.Assert(
                configInfo.BuildDependencies.Any(
                    dep => dep.SystemBuilder is GameScreenCaptureSystemBuilder
                )
            );
            Debug.Assert(
                configInfo.BuildDependencies.Any(
                    dep => dep.SystemBuilder is KeyboardSystemBuilder
                )
            );
            Debug.Assert(configInfo.InitializationPriority == 0);
            Debug.Assert(configInfo.StartPriority == 0);
            Debug.Assert(configInfo.UpdatePriority == 0);
        }

        /**
         * @brief Executes all subsystem configuration validation tests
         * 
         * Runs the complete test suite to ensure the main subsystem information
         * list correctly configures all subsystems with proper dependencies
         * and execution priorities, providing confidence in system reliability.
         */
        public void Run()
        {
            _testGetSubSystemInfoObtainsAllSubSystems();
            _testGetSubSystemInfoObtainsCorrectKeyboardInfo();
            _testGetSubSystemInfoObtainsCorrectScreenCaptureInfo();
            _testGetSubSystemInfoObtainsCorrectConfigurationSystemInfo();
        }
    }


    /**
     * @class MainSystemTests
     * 
     * @brief Unit tests for main system coordination and thread management
     * 
     * This test class validates that the main system properly coordinates subsystem
     * execution through thread management, ensuring correct initialization sequencing,
     * thread lifecycle control, and data propagation between system components.
     */
    class MainSystemTests
    {
        MockThreadFactory _mainSubSystemThreadFactory = new MockThreadFactory();

        MockThread _mainSubSystemThread = new MockThread(new ThreadRunningState());

        /**
         * @brief Creates test environment with mock thread dependencies
         * 
         * @return Configured MainSystem instance for testing
         * 
         * Prepares a test environment with mock thread factory and thread
         * to isolate main system behavior from actual thread execution.
         */
        public AbstractSystem _fixture()    
        {
            _mainSubSystemThreadFactory = new MockThreadFactory();
            _mainSubSystemThread = new MockThread(new ThreadRunningState());
            _mainSubSystemThreadFactory.CreateThreadReturn.Add(_mainSubSystemThread);
            return new MainSystem(_mainSubSystemThreadFactory);
        }

        /**
         * @brief Tests subsystem thread creation during initialization
         * 
         * Validates that the main system creates the subsystem execution thread
         * during initialization, establishing the foundation for subsystem coordination
         * and parallel execution.
         */
        public void _testInitializeCreatesSubSystemThread()
        {
            var mainSystem = _fixture();
            mainSystem.Initialize();
            Debug.Assert(_mainSubSystemThreadFactory.CreateThreadCalls == 1);
        }

        /**
         * @brief Tests thread activation during system startup
         * 
         * Validates that the main system activates the subsystem execution thread
         * during startup, ensuring all subsystems begin coordinated operation
         * when the main system starts.
         */
        public void _testStartThreadStartsSubSystemThread()
        {
            var mainSystem = _fixture();
            mainSystem.Initialize();
            mainSystem.Start();
            Debug.Assert(_mainSubSystemThread.ThreadStartCalls == 1);
        }

        /**
         * @brief Tests data propagation to subsystem thread
         * 
         * Validates that the main system correctly propagates injection data
         * to the subsystem thread, ensuring runtime data and configuration changes
         * are properly distributed to all subsystem components.
         */
        public void _testInjectThreadInjectsToSubSystemThread()
        {
            var mainSystem = _fixture();
            mainSystem.Initialize();
            mainSystem.Inject((SystemInjectType)0x1234, 0x2345);
            Debug.Assert(_mainSubSystemThread.InjectCalls == 1);
            Debug.Assert((int)_mainSubSystemThread.InjectCallArg_dataType[0] == 0x1234);
            Debug.Assert((int?)_mainSubSystemThread.InjectCallArg_data[0] == 0x2345);
        }

        /**
         * @brief Tests system state delegation to subsystem thread
         * 
         * Validates that the main system correctly delegates state queries
         * to the subsystem thread, ensuring consistent state reporting
         * across the entire system hierarchy.
         */
        public void _testStateObtainsSubSystemState()
        {
            var mainSystem = _fixture();
            mainSystem.Initialize();
            _mainSubSystemThread.ThreadStateReturn.Add(0x1234);
            Debug.Assert((int?)mainSystem.State() == 0x1234);
        }

        /**
         * @brief Executes all main system coordination tests
         * 
         * Runs the complete test suite to ensure the main system properly
         * manages subsystem thread lifecycle, data propagation, and state
         * coordination, providing confidence in system reliability.
         */
        public void Run()
        {
            _testInitializeCreatesSubSystemThread();
            _testStartThreadStartsSubSystemThread();
            _testInjectThreadInjectsToSubSystemThread();
            _testStateObtainsSubSystemState();
        }
    }


    /**
     * @class MainApplicationTests
     * 
     * @brief Unit tests for application lifecycle management and system coordination
     * 
     * This test class validates that the main application properly manages system
     * lifecycle events including launch sequencing, shutdown procedures, and system
     * access. Ensures the application correctly coordinates system initialization
     * and provides proper shutdown signaling to all components.
     */
    public class MainApplicationTests
    {
        MockSystem _mainSystem = new MockSystem();

        /**
         * @brief Creates test environment with mock system dependencies
         * 
         * @return Configured MainApplication instance for testing
         * 
         * Prepares a test environment with mock system components to isolate
         * application behavior from actual system implementation details.
         */
        public AbstractApplication _fixture()
        {
            _mainSystem = new MockSystem();
            return new MainApplication(_mainSystem);
        }

        /**
         * @brief Tests application launch sequence
         * 
         * Validates that launching the application properly initializes and starts
         * the main system in the correct sequence, ensuring all system components
         * are ready for operation when the application becomes active.
         */
        public void _testLaunchInitializesAndStartsMainSystem()
        {
            var mainApplication = _fixture();
            var reference = new TestUtilities().Reference(_mainSystem);
            mainApplication.Launch([]);
            Debug.Assert(_mainSystem.CallOrder.Count == 2);
            Debug.Assert(_mainSystem.CallOrder[0] == reference + "InitializeSystem");
            Debug.Assert(_mainSystem.CallOrder[1] == reference + "StartSystem");
        }

        /**
         * @brief Tests application shutdown procedure
         * 
         * Validates that application shutdown correctly signals the main system
         * to begin termination procedures, ensuring graceful system decomposition
         * and resource cleanup when the application exits.
         */
        public void _testShutdownInjectShutdownStateToMainSystem()
        {
            var mainApplication = _fixture();
            mainApplication.ShutDown();
            Debug.Assert(_mainSystem.InjectCalls == 1);
            Debug.Assert(_mainSystem.InjectCallArg_dataType[0] == SystemInjectType.ShutDown);
            Debug.Assert((bool?)_mainSystem.InjectCallArg_data[0] == true);
        }

        /**
         * @brief Tests system access from application
         * 
         * Validates that the application provides proper access to the underlying
         * system instance, enabling external components to interact with system
         * services and state through the application interface.
         */
        public void _testSystemRetrievableFromMainApplication()
        {
            var mainApplication = _fixture();
            var system = mainApplication.System();
            Debug.Assert(system == _mainSystem);
        }

        /**
         * @brief Executes all application lifecycle validation tests
         * 
         * Runs the complete test suite to ensure the main application properly
         * manages system lifecycle events, shutdown procedures, and system
         * accessibility, providing confidence in application reliability.
         */
        public void Run()
        {
            _testLaunchInitializesAndStartsMainSystem();
            _testShutdownInjectShutdownStateToMainSystem();
            _testSystemRetrievableFromMainApplication();
        }
    }


    /**
     * @class MainApplicationInitializerTests
     * 
     * @brief Unit tests for application initialization coordination and dependency injection
     * 
     * This test class validates that the application initializer properly coordinates
     * system startup synchronization and correctly injects external dependencies into 
     * the main system. Ensures the application waits for full system readiness before 
     * proceeding and properly configures all required dependencies for system operation.
     */
    public class MainApplicationInitializerTests
    {
        MockApplication _mainApplication = new MockApplication();

        MockSystem _mainSystem = new MockSystem();

        MockWindowActionHandler _windowViewUpdaterActionHandler = new MockWindowActionHandler();

        MockWindowActionHandler _windowViewCheckboxActionHandler = new MockWindowActionHandler();

        MockWindowActionHandler _splashScreenCompleteActionHandler = new MockWindowActionHandler();

        /**
         * @brief Creates test environment with mock application dependencies
         * 
         * @return Configured MainApplicationInitializer instance for testing
         * 
         * Prepares a test environment with mock application, system, and dependency
         * providers to isolate initialization behavior from actual component implementations.
         */
        private AbstractApplicationInitializer _fixture()
        {
            _mainApplication = new MockApplication();
            _mainSystem = new MockSystem();
            _mainApplication.SystemReturn.Add(_mainSystem);
            _windowViewUpdaterActionHandler = new MockWindowActionHandler();
            _windowViewCheckboxActionHandler = new MockWindowActionHandler();
            _splashScreenCompleteActionHandler = new MockWindowActionHandler();
            return new MainApplicationInitializer(
                _mainApplication,
                _windowViewUpdaterActionHandler,
                _windowViewCheckboxActionHandler,
                _splashScreenCompleteActionHandler
            );
        }

        /**
         * @brief Tests system readiness synchronization
         * 
         * Validates that the application initializer continuously monitors system
         * state until all components report ready status, ensuring the application
         * only proceeds when the entire system is fully operational and stable.
         */
        private void _testSynchronizeChecksMainSystemStateUntilReady()
        {
            for (int i = 0; i < 10; i++) {
                var mainApplicationInitializer = _fixture();
                for (int j = 0; j < i; j++)
                    _mainSystem.StateReturn.Add(false);
                _mainSystem.StateReturn.Add(true);
                mainApplicationInitializer.Synchronize();
                Debug.Assert(_mainSystem.StateCalls == i + 1);
            }
        }


        /**
         * @brief Tests dependency injection during initialization
         * 
         * Validates that the application initializer properly extracts dependencies
         * from external providers and injects them into the main system, ensuring
         * all required system components are correctly connected and configured.
         */
        private void _testInitializeInjectsModifiersToMainSystem()
        {
            var mainApplicationInitializer = _fixture();
            var windowViewUpdateModifier = new MockWindowStateModifier();
            var windowViewCheckboxModifier = new MockWindowStateModifier();
            var splashScreenModifier = new MockWindowStateModifier();
            _windowViewUpdaterActionHandler.ModifierReturn.Add(windowViewUpdateModifier);
            _windowViewCheckboxActionHandler.ModifierReturn.Add(windowViewCheckboxModifier);
            _splashScreenCompleteActionHandler.ModifierReturn.Add(splashScreenModifier);
            mainApplicationInitializer.Initialize();
            var viewModifierIndex = _mainSystem.InjectCallArg_dataType.IndexOf(SystemInjectType.ViewModifier);
            var viewCheckboxIndex = _mainSystem.InjectCallArg_dataType.IndexOf(SystemInjectType.ViewCheckbox);
            var splashScreenIndex = _mainSystem.InjectCallArg_dataType.IndexOf(SystemInjectType.SplashScreen);
            Debug.Assert(_mainSystem.InjectCalls == 3);
            Debug.Assert(viewModifierIndex != -1);
            Debug.Assert(viewCheckboxIndex != -1);
            Debug.Assert(splashScreenIndex != -1);
            Debug.Assert(_mainSystem.InjectCallArg_data[viewModifierIndex] == windowViewUpdateModifier);
            Debug.Assert(_mainSystem.InjectCallArg_data[viewCheckboxIndex] == windowViewCheckboxModifier);
            Debug.Assert(_mainSystem.InjectCallArg_data[splashScreenIndex] == splashScreenModifier);
        }

        /**
         * @brief Executes all application initialization validation tests
         * 
         * Runs the complete test suite to ensure the application initializer properly
         * synchronizes system readiness and configures all dependencies, providing
         * confidence in application startup reliability.
         */
        public void Run()
        {
            _testSynchronizeChecksMainSystemStateUntilReady();
            _testInitializeInjectsModifiersToMainSystem();
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
            new MainSubSystemInfoListTests().Run();
            new MainSystemTests().Run();
            new MainApplicationTests().Run();
            new MainApplicationInitializerTests().Run();
        }
    }
}
