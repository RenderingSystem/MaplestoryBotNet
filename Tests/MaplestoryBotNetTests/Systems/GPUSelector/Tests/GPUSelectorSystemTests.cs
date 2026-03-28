using ArrayFireNCCTests;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.GPUSelector;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using System.Diagnostics;
using MaplestoryBotNetTests.ThreadingUtils;


namespace MaplestoryBotNetTests.Systems.GPUSelector.Tests
{
    /**
     * @class GPUSelectorThreadTests
     * 
     * @brief Unit tests for GPU selector thread functionality and coordination.
     * 
     * @details
     * This test class validates the behavior of the GPUSelectorThread, which manages
     * asynchronous GPU device selection during application startup. Tests ensure that
     * GPU context selection occurs in the correct sequence and properly notifies the
     * splash screen completion system when GPU selection is complete.
     */
    public class GPUSelectorThreadTests
    {
        private AcceleratedDeviceSelectionSystemMock _deviceSelectionSystemMock;

        private GPUSelection _gpuSelection;

        private MockWindowStateModifier _modifierMock;

        private AbstractWindowActionHandler _actionHandler;

        /**
         * @brief Constructor initializing test components with clean state.
         * 
         * @details
         * Each test method starts from this clean state to ensure consistent
         * behavior and isolation between test executions.
         */
        public GPUSelectorThreadTests()
        {
            _deviceSelectionSystemMock = new AcceleratedDeviceSelectionSystemMock();
            _gpuSelection = new GPUSelection();
            _modifierMock = new MockWindowStateModifier();
            _actionHandler = new WindowSplashScreenCompleteActionHandler(_modifierMock);
        }

        /**
         * @brief Creates and configures a test GPUSelectorThread instance with all
         * dependencies.
         * 
         * @details
         * Sets up a complete test environment with:
         * - Fresh mock GPU device selection system
         * - GPU selection data structure
         * - Mock window state modifier
         * - Action handler for splash screen completion
         * - GPUSelectorThread instance configured with all dependencies
         * 
         * @return GPUSelectorThread Fully configured test thread instance ready for testing
         */
        private GPUSelectorThread _fixture()
        {
            _deviceSelectionSystemMock = new AcceleratedDeviceSelectionSystemMock();
            _gpuSelection = new GPUSelection();
            _modifierMock = new MockWindowStateModifier();
            _actionHandler = new WindowSplashScreenCompleteActionHandler(_modifierMock);
            return new GPUSelectorThread(
                new ThreadRunningState(),
                _deviceSelectionSystemMock,
                _gpuSelection
            );
        }


        /**
         * @brief Verifies that GPU context selection occurs in the correct operational
         * sequence.
         * 
         * @test Validates that context selection is called before context retrieval in
         * the correct order.
         * 
         * This sequence is critical for proper GPU initialization and ensures that the
         * system doesn't attempt to retrieve a context before one has been selected.
         */
        private void _testGPUSelectorSelectsContextBeforeGettingContext()
        {
            var thread = _fixture();
            _deviceSelectionSystemMock.context_selected_return.Add(123);
            thread.Inject(SystemInjectType.ActionHandler, _actionHandler);
            thread.Start();
            thread.Join(10000);
            Debug.Assert(_deviceSelectionSystemMock.call_order.Count == 2);
            Debug.Assert(
                _deviceSelectionSystemMock.call_order[0] ==
                "AcceleratedDeviceSelectionSystemMock::context_select"
            );
            Debug.Assert(
                _deviceSelectionSystemMock.call_order[1] ==
                "AcceleratedDeviceSelectionSystemMock::context_selected"
            );
        }

        /**
         * @brief Verifies that GPU selector properly notifies splash screen completion.
         * 
         * @test Validates that the splash screen completion system is notified with
         * correct GPU selection.
         * 
         * @details
         * This test ensures that once GPU selection is complete, the thread properly
         * notifies the splash screen completion system with the selected GPU context.
         * This coordination allows the application to transition from splash screen
         * to main window only after GPU acceleration is configured.
         */
        private void _tesGPUSelectorNotifiesSplashScreenComplete()
        {
            var thread = _fixture();
            _deviceSelectionSystemMock.context_selected_return.Add(123);
            thread.Inject(SystemInjectType.ActionHandler, _actionHandler);
            thread.Start();
            thread.Join(10000);
            Debug.Assert(_modifierMock.ModifyCalls == 1);
            Debug.Assert((GPUSelection)_modifierMock.ModifyCallArg_value[0]! == _gpuSelection);
            Debug.Assert(_gpuSelection.GetSelection() == 123);
        }


        /**
         * @brief Executes the complete test suite for GPU selector thread functionality.
         * 
         * @details
         * Runs all test methods to validate the comprehensive behavior of the
         * GPUSelectorThread, ensuring that GPU device selection occurs in the
         * correct sequence and properly coordinates with the splash screen
         * completion system for smooth application startup.
         */
        public void Run()
        {
            _testGPUSelectorSelectsContextBeforeGettingContext();
            _tesGPUSelectorNotifiesSplashScreenComplete();
        }
    }


    /**
     * @class GPUSelectorSystemTests
     * 
     * @brief Unit tests for the GPU selector system's thread management and
     * dependency injection.
     * 
     * @details
     * This test class validates the behavior of the GPUSelectorSystem, which acts as a
     * facade and manager for the GPU selector thread. Tests ensure that the system
     * properly initializes, starts the background thread, and forwards dependency
     * injections to the underlying thread component.
     */
    public class GPUSelectorSystemTests
    {
        MockThread _mockThread;

        MockThreadFactory _mockThreadFactory;

        /**
         * @brief Constructor initializing test components with clean state.
         * 
         * @details
         * Each test method starts from this clean state to ensure consistent
         * behavior and isolation between test executions.
         */
        public GPUSelectorSystemTests()
        {
            _mockThread = new MockThread(new ThreadRunningState());
            _mockThreadFactory = new MockThreadFactory();
        }

        /**
         * @brief Creates and configures a test GPUSelectorSystem instance with all
         * dependencies.
         * 
         * @details
         * Sets up a complete test environment with:
         * - Fresh mock thread instance
         * - Mock thread factory configured to return the mock thread
         * - GPUSelectorSystem instance using the mock factory
         * 
         * @return GPUSelectorSystem Fully configured test system instance ready for testing
         */
        public GPUSelectorSystem _fixture()
        {
            _mockThread = new MockThread(new ThreadRunningState());
            _mockThreadFactory = new MockThreadFactory();
            _mockThreadFactory.CreateThreadReturn.Add(_mockThread);
            return new GPUSelectorSystem(_mockThreadFactory);
        }

        /**
         * @brief Verifies that the system properly starts the GPU selector thread.
         * 
         * @test Validates that thread startup is initiated when the system starts.
         * 
         * @details
         * This test ensures that when the GPUSelectorSystem is started, it properly
         * initializes and begins execution of the GPU selector thread. This is
         * critical for ensuring that GPU selection begins automatically as part of
         * the application startup sequence.
         */
        public void _testStartingSystemStartsSelectorThread()
        {
            var selectorSystem = _fixture();
            selectorSystem.Initialize();
            selectorSystem.Start();
            Debug.Assert(_mockThread.ThreadStartCalls == 1);
        }

        /**
         * @brief Verifies that dependency injections are properly forwarded to the thread.
         * 
         * @test Validates that system-level injections are propagated to the managed thread.
         * 
         * @details
         * This test ensures that when dependencies are injected into the GPUSelectorSystem,
         * they are correctly forwarded to the underlying GPU selector thread. This is
         * essential for proper dependency management and configuration propagation
         * through the system architecture.
         */
        public void _testInjectingToSystemInjectsToThread()
        {
            var selectorSystem = _fixture();
            selectorSystem.Initialize();
            selectorSystem.Inject(SystemInjectType.Configuration, 1234);
            Debug.Assert(_mockThread.InjectCalls == 1);
            Debug.Assert(_mockThread.InjectCallArg_dataType[0] is SystemInjectType.Configuration);
            Debug.Assert((int)_mockThread.InjectCallArg_data[0]! == 1234);
        }


        /**
         * @brief Executes the complete test suite for GPU selector system functionality.
         * 
         * @details
         * Runs all test methods to validate the comprehensive behavior of the
         * GPUSelectorSystem, ensuring proper thread management and dependency
         * injection forwarding in the GPU selection subsystem.
         */
        public void Run()
        {
            _testStartingSystemStartsSelectorThread();
            _testInjectingToSystemInjectsToThread();
        }
    }


    public class GPUSelectorSystemTestSuite
    {
        public void Run()
        {
            new GPUSelectorThreadTests().Run();
            new GPUSelectorSystemTests().Run();
        }
    }
}
