using System.Diagnostics;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Device.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.LibraryWrappers.Tests;
using MaplestoryBotNetTests.Systems.Device.Tests.Mocks;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;


namespace MaplestoryBotNetTests.Systems.Device.Tests
{
    public class KeyboardDeviceDetectorTests
    {
        MockInterceptionLibrary _interceptionLibrary = new MockInterceptionLibrary();

        private AbstractDeviceDetector _fixture()
        {
            _interceptionLibrary = new MockInterceptionLibrary();
            return new KeyboardDeviceDetector(_interceptionLibrary);
        }

        /**
         * @brief Tests that the keyboard detector properly configures hardware filters before
         * waiting for device input
         * 
         * When detecting a keyboard device, the bot must first set up an interception filter
         * that captures keyboard events from the created hardware context. It must then wait
         * for keyboard activity, and finally clear the filter to return to normal operation.
         */
        private void _testDetectSetsKeyboardFilterBeforeObtainingDevice()
        {
            var keyboardDeviceDetector = _fixture();
            _interceptionLibrary.CreateContextReturn.Add(0x1234);
            _interceptionLibrary.WaitWithTimeoutReturn.Add(0x2345);
            var keyboardDeviceContext = keyboardDeviceDetector.Detect();
            var interceptionRef = new TestUtilities().Reference(_interceptionLibrary);
            Debug.Assert(_interceptionLibrary.CallOrder.Count == 4);
            Debug.Assert(_interceptionLibrary.CallOrder[0] == interceptionRef + "CreateContext");
            Debug.Assert(_interceptionLibrary.CallOrder[1] == interceptionRef + "SetFilter");
            Debug.Assert(_interceptionLibrary.CallOrder[2] == interceptionRef + "WaitWithTimeout");
            Debug.Assert(_interceptionLibrary.CallOrder[3] == interceptionRef + "SetFilter");
        }

        /**
         * @brief Tests that the keyboard filter is applied to the created context while searching
         * for a device ID
         * 
         * When searching for a keyboard device, the bot must obtain a unique device ID that
         * represents a specific physical keyboard. To find this ID, the bot creates a hardware
         * context, applies a keyboard filter to capture only keyboard events, then waits for
         * input. When the user presses any key, the interception library returns the device ID
         * that generated that event.
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
         * @brief Tests that the created hardware context is used when waiting for keyboard input
         * 
         * After setting up the keyboard filter, the bot must call WaitWithTimeout on the exact
         * same hardware context that was created. This ensures the detection listens on the
         * correct device context rather than a default or unrelated context.
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
         * @brief Tests that the detector returns both the hardware context and the detected
         * device
         * 
         * When keyboard detection completes successfully, the bot needs both the hardware
         * context (for future input interception) and the specific device ID (for identifying
         * which keyboard was detected). This information allows the bot to later filter
         * input specifically from this device.
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

        public void Run()
        {
            _testDetectSetsKeyboardFilterBeforeObtainingDevice();
            _testDetectSetsFilterOfTheCreatedContext();
            _testDetectUsesCreatedContextToFindDevice();
            _testDetectObtainsKeyboardContextAndDeviceFromInterception();
        }
    }


    public class KeyboardDeviceDetectorThreadTests
    {
        MockRunningState _runningState = new MockRunningState();

        MockDeviceDetector _keyboardDeviceDetector = new MockDeviceDetector();

        AbstractWindowActionHandler _splashScreenActionHandler = new MockWindowActionHandler();

        MockWindowStateModifier _splashScreenModifier = new MockWindowStateModifier();

        private KeyboardDeviceDetectorThread _fixture()
        {
            _runningState = new MockRunningState();
            _runningState.IsRunningReturn.Add(false);
            _keyboardDeviceDetector = new MockDeviceDetector();
            _keyboardDeviceDetector.DetectReturn.Add(new DeviceContext(0x1234, 0x2345));
            _splashScreenModifier = new MockWindowStateModifier();
            _splashScreenActionHandler = new WindowSplashScreenCompleteActionHandler(_splashScreenModifier);
            var detectorThread = new KeyboardDeviceDetectorThread(_keyboardDeviceDetector, _runningState);
            _splashScreenModifier.StateReturn.Add(SplashScreenTypes.StartSplash);
            detectorThread.Inject(SystemInjectType.ActionHandler, _splashScreenActionHandler);
            return detectorThread;
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
            var result = (DeviceContext?) keyboardDeviceDetectorThread.Result();
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


    public class MouseDeviceDetectorTests
    {
        MockInterceptionLibrary _interceptionLibrary = new MockInterceptionLibrary();

        private AbstractDeviceDetector _fixture()
        {
            _interceptionLibrary = new MockInterceptionLibrary();
            return new MouseDeviceDetector(_interceptionLibrary);
        }

        /**
         * @brief Tests that the mouse detector properly configures hardware filters before
         * waiting for device input
         * 
         * When detecting a mouse device, the bot must first set up an interception filter
         * that captures mouse events from the created hardware context. It must then wait
         * for mouse activity (movement or clicks), and finally clear the filter to return
         * to normal operation.
         */
        private void _testDetectSetsKeyboardFilterBeforeObtainingDevice()
        {
            var mouseDeviceDetector = _fixture();
            _interceptionLibrary.CreateContextReturn.Add(0x1234);
            _interceptionLibrary.WaitWithTimeoutReturn.Add(0x2345);
            var keyboardDeviceContext = mouseDeviceDetector.Detect();
            var interceptionRef = new TestUtilities().Reference(_interceptionLibrary);
            Debug.Assert(_interceptionLibrary.CallOrder.Count == 4);
            Debug.Assert(_interceptionLibrary.CallOrder[0] == interceptionRef + "CreateContext");
            Debug.Assert(_interceptionLibrary.CallOrder[1] == interceptionRef + "SetFilter");
            Debug.Assert(_interceptionLibrary.CallOrder[2] == interceptionRef + "WaitWithTimeout");
            Debug.Assert(_interceptionLibrary.CallOrder[3] == interceptionRef + "SetFilter");
        }

        /**
         * @brief Tests that the mouse filter is applied to the created context while searching
         * for a device ID
         * 
         * When searching for a mouse device, the bot must obtain a unique device ID that
         * represents a specific physical mouse. To find this ID, the bot creates a hardware
         * context, applies a mouse filter to capture only mouse events, then waits for
         * input. When the user moves the mouse or clicks a button, the interception library
         * returns the device ID that generated that event.
         */
        private void _testDetectSetsFilterOfTheCreatedContext()
        {
            var mouseDeviceDetector = _fixture();
            _interceptionLibrary.CreateContextReturn.Add(0x1234);
            _interceptionLibrary.WaitWithTimeoutReturn.Add(0x2345);
            var mouseDeviceContext = mouseDeviceDetector.Detect();
            Debug.Assert(_interceptionLibrary.SetFilterCalls == 2);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_context[0] == 0x1234);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_interception_predicate[0] == _interceptionLibrary.IsMouse);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_filter[0] == Interception.InterceptionInterop.Filter.All);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_context[1] == 0x1234);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_interception_predicate[1] == _interceptionLibrary.IsMouse);
            Debug.Assert(_interceptionLibrary.SetFilterCallArg_filter[1] == Interception.InterceptionInterop.Filter.None);
        }

        /**
         * @brief Tests that the created hardware context is used when waiting for mouse input
         * 
         * After setting up the mouse filter, the bot must call WaitWithTimeout on the exact
         * same hardware context that was created. This ensures the detection listens on the
         * correct device context rather than a default or unrelated context.
         */
        private void _testDetectUsesCreatedContextToFindDevice()
        {
            var mouseDeviceDetector = _fixture();
            _interceptionLibrary.CreateContextReturn.Add(0x1234);
            _interceptionLibrary.WaitWithTimeoutReturn.Add(0x2345);
            var mouseDeviceContext = mouseDeviceDetector.Detect();
            Debug.Assert(_interceptionLibrary.WaitWithTimeoutCalls == 1);
            Debug.Assert(_interceptionLibrary.WaitWithTimeoutCallArg_context[0] == 0x1234);
        }

        /**
         * @brief Tests that the detector returns both the hardware context and the detected
         * mouse device
         * 
         * When mouse detection completes successfully, the bot needs both the hardware
         * context (for future input interception) and the specific device ID (for identifying
         * which mouse was detected). This information allows the bot to later filter input
         * specifically from this device.
         */
        private void _testDetectObtainsKeyboardContextAndDeviceFromInterception()
        {
            var mouseDeviceDetector = _fixture();
            _interceptionLibrary.CreateContextReturn.Add(0x1234);
            _interceptionLibrary.WaitWithTimeoutReturn.Add(0x2345);
            var mouseDeviceContext = mouseDeviceDetector.Detect();
            Debug.Assert(mouseDeviceContext.Context == 0x1234);
            Debug.Assert(mouseDeviceContext.Device == 0x2345);
        }

        public void Run()
        {
            _testDetectSetsKeyboardFilterBeforeObtainingDevice();
            _testDetectSetsFilterOfTheCreatedContext();
            _testDetectUsesCreatedContextToFindDevice();
            _testDetectObtainsKeyboardContextAndDeviceFromInterception();
        }
    }


    public class MouseDeviceDetectorThreadTests
    {
        MockRunningState _runningState = new MockRunningState();

        MockDeviceDetector _mouseDeviceDetector = new MockDeviceDetector();

        AbstractWindowActionHandler _splashScreenActionHandler = new MockWindowActionHandler();

        MockWindowStateModifier _splashScreenModifier = new MockWindowStateModifier();

        private MouseDeviceDetectorThread _fixture()
        {
            _runningState = new MockRunningState();
            _runningState.IsRunningReturn.Add(false);
            _mouseDeviceDetector = new MockDeviceDetector();
            _mouseDeviceDetector.DetectReturn.Add(new DeviceContext(0x1234, 0x2345));
            _splashScreenModifier = new MockWindowStateModifier();
            _splashScreenActionHandler = new WindowSplashScreenCompleteActionHandler(_splashScreenModifier);
            var detectorThread = new MouseDeviceDetectorThread(_mouseDeviceDetector, _runningState);
            _splashScreenModifier.StateReturn.Add(SplashScreenTypes.StartSplash);
            detectorThread.Inject(SystemInjectType.ActionHandler, _splashScreenActionHandler);
            return detectorThread;
        }

        /**
         * @brief Tests successful mouse device detection in threaded environment
         * 
         * Validates that the threaded detector correctly identifies mouse devices
         * and returns the appropriate device context, ensuring that mouse input
         * monitoring can be properly initialized for gameplay automation.
         */
        private void _testDetectorThreadDetectsAndReturnsContext()
        {
            var mouseDeviceDetectorThread = _fixture();
            mouseDeviceDetectorThread.Start();
            mouseDeviceDetectorThread.Join(10000);
            var result = (DeviceContext?)mouseDeviceDetectorThread.Result();
            Debug.Assert(result != null);
            Debug.Assert(result.Context == 0x1234);
            Debug.Assert(result.Device == 0x2345);
        }

        /**
         * @brief Executes the threaded mouse detection test
         * 
         * Runs the test to ensure the bot will correctly implement threaded
         * mouse device detection, providing confidence in the reliability
         * of mouse input monitoring initialization during automation startup.
         */
        public void Run()
        {
            _testDetectorThreadDetectsAndReturnsContext();
        }
    }


    public class DeviceDetectorSystemTests
    {
        private List<AbstractThreadFactory> _threadFactories = [];

        private List<MockThread> _threads = [];

        public DeviceDetectorSystem _fixture()
        {
            _threadFactories = [];
            _threads = [];
            for (int i = 0; i < 5; i++)
            {
                var threadFactory = new MockThreadFactory();
                var thread = new MockThread(new MockRunningState());
                threadFactory.CreateThreadReturn.Add(thread);
                _threadFactories.Add(threadFactory);
                _threads.Add(thread);
            }
            return new DeviceDetectorSystem(_threadFactories);
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
            foreach (MockThreadFactory threadFactory in _threadFactories)
            {
                Debug.Assert(threadFactory.CreateThreadCalls == 1);
            }
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
            foreach (var thread in _threads)
            {
                Debug.Assert(thread.ThreadStartCalls == 1);
            }
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
            foreach (var thread in _threads)
            {
                Debug.Assert(thread.ThreadStartCalls == 0);
            }
        }


        /**
         * @brief Tests successful device context injection
         * 
         * Validates that the keyboard detection system correctly injects
         * detected keyboard device context into the system, ensuring
         * keyboard input monitoring can be properly established.
         */
        private void _testUpdateSystemInjectsKeyboardDevice()
        {
            var keyboardDeviceDetectorSystem = _fixture();
            var deviceContext = new DeviceContext(0x1234, 0x2345);
            keyboardDeviceDetectorSystem.Initialize();
            keyboardDeviceDetectorSystem.Inject(SystemInjectType.KeyboardDevice, deviceContext);
            foreach (var thread in _threads)
            {
                Debug.Assert(thread.InjectCalls == 1);
                Debug.Assert(thread.InjectCallArg_dataType[0] is SystemInjectType.KeyboardDevice);
                Debug.Assert(thread.InjectCallArg_data[0] == deviceContext);
            }
        }

        public void Run()
        {
            _testInitializationCreatesDetectorThread();
            _testStartSystemStartsDetectorThread();
            _testStartSystemCannotStartDetectorThreadWithoutInitialization();
            _testUpdateSystemInjectsKeyboardDevice();
        }
    }


    public class DeviceDetectorTestSuite
    {
        public void Run()
        {
            new KeyboardDeviceDetectorTests().Run();
            new KeyboardDeviceDetectorThreadTests().Run();
            new MouseDeviceDetectorTests().Run();
            new MouseDeviceDetectorThreadTests().Run();
            new DeviceDetectorSystemTests().Run();
        }
    }
}
