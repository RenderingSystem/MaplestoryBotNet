using System.Diagnostics;
using System.Drawing;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNetTests.LibraryWrappers.Tests;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;

namespace MaplestoryBotNetTests.Systems.Keyboard.Tests
{
    /**
     * @class KeyboardDeviceDetectorTests
     * 
     * @brief Unit tests for verifying keyboard device detection functionality
     * 
     * This test class validates that the bot correctly identifies and configures
     * keyboard input devices, ensuring reliable key press detection and accurate
     * input monitoring during gameplay automation.
     */
    public class KeyboardDeviceDetectorTests
    {
        MockInterceptionLibrary _interceptionLibrary = new MockInterceptionLibrary();

        /**
         * @brief Creates a test environment for keyboard detection testing
         * 
         * @return Configured KeyboardDeviceDetector instance
         * 
         * Prepares a test environment with mock interception capabilities to
         * verify keyboard device detection without requiring physical hardware
         * or actual system interception.
         */
        private KeyboardDeviceDetector _fixture()
        {
            _interceptionLibrary = new MockInterceptionLibrary();
            return new KeyboardDeviceDetector(_interceptionLibrary);
        }

        /**
         * @brief Tests proper filter configuration before device detection
         * 
         * Validates that the bot correctly configures keyboard filters before
         * attempting to detect devices, ensuring that only relevant keyboard
         * input is captured during gameplay automation.
         */
        private void _testDetectSetsKeyboardFilterBeforeObtainingDevice()
        {
            var keyboardDeviceDetector = _fixture();
            _interceptionLibrary.CreateContextReturn.Add(0x1234);
            _interceptionLibrary.WaitWithTimeoutReturn.Add(0x2345);
            var keyboardDeviceContext = keyboardDeviceDetector.Detect();
            var interceptionRef = new TestUtilities().Reference(_interceptionLibrary);
            var setFilterIndex = _interceptionLibrary.CallOrder.IndexOf(interceptionRef + "SetFilter");
            var waitIndex = _interceptionLibrary.CallOrder.IndexOf(interceptionRef + "WaitWithTimeout");
            Debug.Assert(setFilterIndex != -1);
            Debug.Assert(waitIndex != -1);
            Debug.Assert(setFilterIndex < waitIndex);
        }

        /**
         * @brief Tests correct filter application to device context
         * 
         * Validates that the bot correctly applies keyboard-specific filters
         * to the interception context, ensuring accurate input detection
         * and minimizing false positives from non-keyboard devices.
         */
        private void _testDetectSetsFilterOfTheCreatedContext()
        {
            var keyboardDeviceDetector = _fixture();
            _interceptionLibrary.CreateContextReturn.Add(0x1234);
            _interceptionLibrary.WaitWithTimeoutReturn.Add(0x2345);
            var keyboardDeviceContext = keyboardDeviceDetector.Detect();
            Debug.Assert(_interceptionLibrary.SetFilterCalls == 2);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_context[0] == 0x1234);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_interception_predicate[0] == _interceptionLibrary.IsKeyboard);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_filter[0] == Interception.InterceptionInterop.Filter.All);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_context[1] == 0x1234);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_interception_predicate[1] == _interceptionLibrary.IsKeyboard);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_filter[1] == Interception.InterceptionInterop.Filter.None);
        }

        /**
         * @brief Tests proper context usage during device detection
         * 
         * Validates that the bot correctly utilizes the created interception
         * context when waiting for keyboard devices.
         */
        private void _testDetectUsesCreatedContextToFindDevice()
        {
            var keyboardDeviceDetector = _fixture();
            _interceptionLibrary.CreateContextReturn.Add(0x1234);
            _interceptionLibrary.WaitWithTimeoutReturn.Add(0x2345);
            var keyboardDeviceContext = keyboardDeviceDetector.Detect();
            Debug.Assert(_interceptionLibrary.WaitWithTimeoutCalls == 1);
            Debug.Assert(_interceptionLibrary.WaitWithTimeoutCallArg_context[0] == 0x1234);
        }

        /**
         * @brief Tests complete device context retrieval
         * 
         * Validates that the bot correctly retrieves both the interception context
         * and specific device identifier, ensuring all necessary information is
         * available for accurate keyboard IO.
         */
        private void _testDetectObtainsKeyboardContextAndDeviceFromInterception()
        {
            var keyboardDeviceDetector = _fixture();
            _interceptionLibrary.CreateContextReturn.Add(0x1234);
            _interceptionLibrary.WaitWithTimeoutReturn.Add(0x2345);
            var keyboardDeviceContext = keyboardDeviceDetector.Detect();
            Debug.Assert(keyboardDeviceContext.Context == 0x1234);
            Debug.Assert(keyboardDeviceContext.Device == 0x2345);
        }

        /**
         * @brief Executes all keyboard device detection tests
         * 
         * Runs the complete test suite to ensure the bot will correctly detect
         * and configure keyboard devices, providing confidence in the reliability
         * of keyboard input monitoring during gameplay automation.
         */
        public void Run()
        {
            _testDetectSetsKeyboardFilterBeforeObtainingDevice();
            _testDetectSetsFilterOfTheCreatedContext();
            _testDetectUsesCreatedContextToFindDevice();
            _testDetectObtainsKeyboardContextAndDeviceFromInterception();
        }
    }


    /**
     * @class KeyboardDeviceDetectorThreadTest
     * 
     * @brief Unit tests for verifying threaded keyboard device detection functionality
     * 
     * This test class validates that the bot correctly implements threaded keyboard device
     * detection, ensuring reliable and non-blocking identification of keyboard input devices
     * during gameplay automation initialization.
     */
    public class KeyboardDeviceDetectorThreadTest
    {
        MockRunningState _runningState = new MockRunningState();

        MockKeyboardDeviceDetector _keyboardDeviceDetector = new MockKeyboardDeviceDetector();

        /**
         * @brief Creates a test environment for threaded keyboard detection testing
         * 
         * @return Configured KeyboardDeviceDetectorThread instance
         * 
         * Prepares a test environment with mock running state and device detection
         * capabilities to verify threaded keyboard detection without requiring
         * actual hardware or blocking system operations.
         */
        private KeyboardDeviceDetectorThread _fixture()
        {
            _runningState = new MockRunningState();
            _runningState.IsRunningReturn.Add(false);
            _keyboardDeviceDetector = new MockKeyboardDeviceDetector();
            _keyboardDeviceDetector.DetectReturn.Add(new KeyboardDeviceContext(0x1234, 0x2345));
            return new KeyboardDeviceDetectorThread(_keyboardDeviceDetector, _runningState);
        }

        /**
         * @brief Tests successful keyboard device detection in threaded environment
         * 
         * Validates that the threaded detector correctly identifies keyboard devices
         * and returns the appropriate device context, ensuring that keyboard input
         * monitoring can be properly initialized for gameplay automation.
         */
        private void _testDetectorThreadDetectsAndReturnsContext()
        {
            var keyboardDeviceDetectorThread = _fixture();
            keyboardDeviceDetectorThread.Start();
            keyboardDeviceDetectorThread.Join(10000);
            var result = (KeyboardDeviceContext?) keyboardDeviceDetectorThread.Result();
            Debug.Assert(result != null);
            Debug.Assert(result.Context == 0x1234);
            Debug.Assert(result.Device == 0x2345);
        }

        /**
         * @brief Executes the threaded keyboard detection test
         * 
         * Runs the test to ensure the bot will correctly implement threaded
         * keyboard device detection, providing confidence in the reliability
         * of keyboard input monitoring initialization during automation startup.
         */
        public void Run()
        {
            _testDetectorThreadDetectsAndReturnsContext();
        }
    }


    /**
     * @class KeyboardDeviceDetectorSystemTest
     * 
     * @brief Unit tests for verifying keyboard device detection system functionality
     * 
     * This test class validates the complete keyboard device detection system, ensuring
     * proper thread management, initialization sequencing, and device context injection
     * for reliable keyboard input handling during gameplay automation.
     */
    public class KeyboardDeviceDetectorSystemTest
    {
        private MockThreadFactory _threadFactory = new MockThreadFactory();

        private MockThread _thread = new MockThread(new MockRunningState());

        private MockInjector _injector = new MockInjector();

        /**
         * @brief Creates a test environment for keyboard detection system testing
         * 
         * @return Configured KeyboardDeviceDetectorSystem instance
         * 
         * Prepares a test environment with mock thread factory and injector components
         * to verify the keyboard detection system's thread management and device
         * context injection capabilities without requiring actual hardware access.
         */
        public KeyboardDeviceDetectorSystem _fixture()
        {
            _threadFactory = new MockThreadFactory();
            _thread = new MockThread(new MockRunningState());
            _injector = new MockInjector();
            _threadFactory.CreateThreadReturn.Add(_thread);
            return new KeyboardDeviceDetectorSystem(_threadFactory, _injector);
        }

        /**
         * @brief Tests proper thread creation during system initialization
         * 
         * Validates that the keyboard detection system correctly creates its
         * detector thread during initialization, ensuring the necessary
         * infrastructure is in place for keyboard device identification.
         */
        private void _testInitializationCreatesDetectorThread()
        {
            var keyboardDeviceDetectorSystem = _fixture();
            keyboardDeviceDetectorSystem.Initialize();
            Debug.Assert(_threadFactory.CreateThreadCalls == 1);
        }

        /**
         * @brief Tests idempotent behavior of system initialization
         * 
         * Validates that repeated initialization calls don't create additional
         * threads, ensuring resource efficiency and preventing thread proliferation
         * during system startup.
         */
        private void _testInitializationOnlyCreatesOneThread()
        {
            var keyboardDeviceDetectorSystem = _fixture();
            keyboardDeviceDetectorSystem.Initialize();
            keyboardDeviceDetectorSystem.Initialize();
            Debug.Assert(_threadFactory.CreateThreadCalls == 1);
        }

        /**
         * @brief Tests proper thread startup during system activation
         * 
         * Validates that the keyboard detection system correctly starts its
         * detector thread when the system is activated, ensuring keyboard
         * device identification begins at the appropriate time.
         */
        private void _testStartSystemStartsDetectorThread()
        {
            var keyboardDeviceDetectorSystem = _fixture();
            keyboardDeviceDetectorSystem.Initialize();
            keyboardDeviceDetectorSystem.Start();
            Debug.Assert(_thread.ThreadStartCalls == 1);
        }

        /**
         * @brief Tests proper initialization requirement enforcement
         * 
         * Validates that the keyboard detection system requires initialization
         * before activation, preventing premature thread startup and ensuring
         * proper system sequencing.
         */
        private void _testStartSystemCannotStartDetectorThreadWithoutInitialization()
        {
            var keyboardDeviceDetectorSystem = _fixture();
            keyboardDeviceDetectorSystem.Start();
            Debug.Assert(_thread.ThreadStartCalls == 0);
        }

        /**
         * @brief Tests proper handling of incomplete device detection
         * 
         * Validates that the keyboard detection system correctly handles
         * situations where device detection is not yet complete, preventing
         * premature injection of incomplete device context information.
         */
        private void _testUpdateSystemDoesNotInjectWithoutThreadResult()
        {
            var keyboardDeviceDetectorSystem = _fixture();
            _thread.ThreadResultReturn.Add(null);
            keyboardDeviceDetectorSystem.Initialize();
            keyboardDeviceDetectorSystem.Update();
            Debug.Assert(_injector.InjectCalls == 0);
        }

        /**
         * @brief Tests successful device context injection
         * 
         * Validates that the keyboard detection system correctly injects
         * detected keyboard device context into the system, ensuring
         * keyboard input monitoring can be properly established.
         */
        private void _testUpdateSystemInjectsWithThreadResult()
        {
            var keyboardDeviceDetectorSystem = _fixture();
            var deviceContext = new KeyboardDeviceContext(0x1234, 0x2345);
            _thread.ThreadResultReturn.Add(deviceContext);
            keyboardDeviceDetectorSystem.Initialize();
            keyboardDeviceDetectorSystem.Update();
            Debug.Assert(_injector.InjectCalls == 1);
            Debug.Assert(_injector.InjectCallArg_dataType[0] == SystemInjectType.KeyboardDevice);
            Debug.Assert(_injector.InjectCallArg_data[0] == deviceContext);
        }

        /**
         * @brief Tests idempotent behavior of device context injection
         * 
         * Validates that the keyboard detection system injects device context
         * only once, preventing duplicate injections and ensuring system
         * stability after successful device identification.
         */
        private void _testUpdateSystemInjectsWithThreadResultOnlyOnce()
        {
            var keyboardDeviceDetectorSystem = _fixture();
            var deviceContext = new KeyboardDeviceContext(0x1234, 0x2345);
            _thread.ThreadResultReturn.Add(deviceContext);
            keyboardDeviceDetectorSystem.Initialize();
            keyboardDeviceDetectorSystem.Update();
            keyboardDeviceDetectorSystem.Update();
            Debug.Assert(_injector.InjectCalls == 1);
            Debug.Assert(_injector.InjectCallArg_dataType[0] == SystemInjectType.KeyboardDevice);
            Debug.Assert(_injector.InjectCallArg_data[0] == deviceContext);
        }

        /**
         * @brief Tests proper handling of missing thread results
         * 
         * Validates that the keyboard detection system correctly handles
         * situations where no thread result is available, preventing
         * injection of invalid device context information.
         */
        private void _testUpdateSystemDoesNotInjectWithNoThreadResult()
        {
            var keyboardDeviceDetectorSystem = _fixture();
            _thread.ThreadResultReturn.Add(null);
            keyboardDeviceDetectorSystem.Update();
            Debug.Assert(_injector.InjectCalls == 0);
        }

        /**
         * @brief Executes all keyboard detection system tests
         * 
         * Runs the complete test suite to ensure the keyboard detection system
         * correctly manages threads, handles device identification, and injects
         * device context, providing confidence in the reliability of keyboard
         * input monitoring during automation operations.
         */
        public void Run()
        {
            _testInitializationCreatesDetectorThread();
            _testInitializationOnlyCreatesOneThread();
            _testStartSystemStartsDetectorThread();
            _testStartSystemCannotStartDetectorThreadWithoutInitialization();
            _testUpdateSystemDoesNotInjectWithoutThreadResult();
            _testUpdateSystemInjectsWithThreadResult();
            _testUpdateSystemInjectsWithThreadResultOnlyOnce();
            _testUpdateSystemDoesNotInjectWithNoThreadResult();
        }
    }


    /**
     * @class KeyboardDeviceDetectorTestSuite
     * 
     * @brief Comprehensive test suite for keyboard device detection functionality
     * 
     * This test suite executes all keyboard device detection related tests to ensure
     * complete verification of keyboard input handling capabilities. It validates both
     * individual device detection components and their threaded implementation.
     */
    public class KeyboardDeviceDetectorTestSuite
    {
        /**
         * @brief Executes the complete keyboard device detection test suite
         * 
         * Runs all keyboard device detection tests to ensure comprehensive coverage
         * of keyboard input handling functionality, validating both direct detection
         * and threaded implementation for reliable keyboard monitoring during
         * gameplay automation sessions.
         */
        public void Run()
        {
            new KeyboardDeviceDetectorTests().Run();
            new KeyboardDeviceDetectorThreadTest().Run();
            new KeyboardDeviceDetectorSystemTest().Run();
        }
    }
}
