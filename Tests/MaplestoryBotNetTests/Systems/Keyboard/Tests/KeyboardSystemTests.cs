using System.Diagnostics;
using System.Threading;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.ThreadingUtils;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests
{
    public class KeyboardSystemTests
    {
        private KeyboardDeviceDetectorSystem? _keyboardDeviceDetectorSystem;

        private MockThreadFactory _keyboardDeviceDetectorThreadFactory = new MockThreadFactory();

        private MockThread _keyboardDeviceDetectorThread = new MockThread(new ThreadRunningState());

        private MockKeystrokeTransmitterBuilder _keystrokeTransmitterBuilder = new MockKeystrokeTransmitterBuilder();

        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        /**
         * @brief Creates a complete test environment for keyboard system testing
         * 
         * @return Configured KeyboardSystem instance
         * 
         * Prepares a comprehensive test environment with all keyboard system components,
         * including device detection, thread management, and keystroke transmission,
         * ensuring thorough testing of keyboard input functionality without requiring
         * actual hardware interaction.
         */
        private KeyboardSystem _fixture()
        {
            _keyboardDeviceDetectorThreadFactory = new MockThreadFactory();
            _keyboardDeviceDetectorThread = new MockThread(new ThreadRunningState());
            _keyboardDeviceDetectorThreadFactory.CreateThreadReturn.Add(_keyboardDeviceDetectorThread);
            _keyboardDeviceDetectorSystem = new KeyboardDeviceDetectorSystem(
                _keyboardDeviceDetectorThreadFactory
            );
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _keystrokeTransmitterBuilder = new MockKeystrokeTransmitterBuilder();
            _keystrokeTransmitterBuilder.BuildReturn.Add(_keystrokeTransmitter);
            return new KeyboardSystem(
                _keyboardDeviceDetectorSystem,
                _keystrokeTransmitterBuilder
            );
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
            var keyboardSystem = _fixture();
            keyboardSystem.Initialize();
            Debug.Assert(_keyboardDeviceDetectorThreadFactory.CreateThreadCalls == 1);
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
            var keyboardSystem = _fixture();
            keyboardSystem.Initialize();
            keyboardSystem.Initialize();
            Debug.Assert(_keyboardDeviceDetectorThreadFactory.CreateThreadCalls == 1);
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
            var keyboardSystem = _fixture();
            keyboardSystem.Initialize();
            keyboardSystem.Start();
            Debug.Assert(_keyboardDeviceDetectorThread.ThreadStartCalls == 1);
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
            var keyboardSystem = _fixture();
            keyboardSystem.Start();
            Debug.Assert(_keyboardDeviceDetectorThread.ThreadStartCalls == 0);
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
            var keyboardSystem = _fixture();
            var deviceContext = new KeyboardDeviceContext(0x1234, 0x2345);
            keyboardSystem.Initialize();
            keyboardSystem.Inject(SystemInjectType.KeyboardDevice, deviceContext);
            Debug.Assert(_keyboardDeviceDetectorThread.InjectCalls == 1);
            Debug.Assert(_keyboardDeviceDetectorThread.InjectCallArg_dataType[0] == SystemInjectType.KeyboardDevice);
            Debug.Assert(_keyboardDeviceDetectorThread.InjectCallArg_data[0] == deviceContext);
        }

        /**
         * @brief Tests automatic keystroke transmitter creation on keyboard mapping injection
         * 
         * Validates that the keyboard system automatically creates and injects a keystroke
         * transmitter when a keyboard mapping configuration is provided, ensuring that
         * keyboard input simulation capabilities are properly initialized when configuration
         * data becomes available.
         */
        private void _testInjectKeyboardTransmitterOnKeyboardMappingInjection()
        {
            var keyboardSystem = _fixture();
            _keyboardDeviceDetectorThread.ThreadResultReturn.Add(null);
            keyboardSystem.Initialize();
            keyboardSystem.Inject(SystemInjectType.Configuration, new KeyboardMapping());
            Debug.Assert(_keyboardDeviceDetectorThread.InjectCalls == 1);
            Debug.Assert(_keyboardDeviceDetectorThread.InjectCallArg_dataType[0] == SystemInjectType.KeystrokeTransmitter);
            Debug.Assert(_keyboardDeviceDetectorThread.InjectCallArg_data[0] == _keystrokeTransmitter);
        }

        /**
         * @brief Executes all keyboard system functionality tests
         * 
         * Runs the complete test suite to ensure the keyboard system correctly handles
         * device detection, thread management, and keystroke transmission, providing
         * confidence in the reliability of keyboard input handling during automation
         * operations across various scenarios and conditions.
         */
        public void Run()
        {
            _testInitializationCreatesDetectorThread();
            _testInitializationOnlyCreatesOneThread();
            _testStartSystemStartsDetectorThread();
            _testStartSystemCannotStartDetectorThreadWithoutInitialization();
            _testUpdateSystemInjectsKeyboardDevice();
            _testInjectKeyboardTransmitterOnKeyboardMappingInjection();
        }
    }


    public class KeyboardSystemTestSuite
    {
        public void Run()
        {
            new KeyboardSystemTests().Run();
        }
    }
}
