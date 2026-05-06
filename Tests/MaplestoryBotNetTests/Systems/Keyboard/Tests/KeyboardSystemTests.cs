using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.ThreadingUtils;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests
{
    public class KeyboardSystemTests
    {
        private MockKeystrokeTransmitterBuilder _keystrokeTransmitterBuilder = new MockKeystrokeTransmitterBuilder();

        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private List<List<AbstractThreadFactory>> _keyboardSubSystemsThreadFactories = [];

        private List<List<AbstractThread>> _keyboardSubSystemsThreads = [];

        private List<AbstractSystem> _keyboardSubSystems = [];

        /**
         * @brief Creates a complete test environment for keyboard system testing
         * 
         * @return Configured KeyboardSystem instance with multiple subsystems
         * 
         * Prepares a comprehensive test environment containing both the keyboard device
         * detector and keystroke transmitter orchestrator subsystems. Each subsystem
         * may have multiple thread factories and threads, allowing thorough testing
         * of complex thread management scenarios without requiring actual hardware.
         */
        private KeyboardSystem _fixture()
        {
            _keyboardSubSystemsThreadFactories = [];
            _keyboardSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _keyboardSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _keyboardSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _keyboardSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _keyboardSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _keyboardSubSystems = [];
            _keyboardSubSystems.Add(new KeyboardDeviceDetectorSystem(_keyboardSubSystemsThreadFactories[0][0]));
            _keyboardSubSystems.Add(new BottingOrchestratorSystem(_keyboardSubSystemsThreadFactories[1]));
            _keyboardSubSystems.Add(new RuneingOrchestratorSystem(_keyboardSubSystemsThreadFactories[2]));
            _keyboardSubSystems.Add(new SolvingOrchestratorSystem(_keyboardSubSystemsThreadFactories[3]));
            _keyboardSubSystems.Add(new CashShopOrchestratorSystem(_keyboardSubSystemsThreadFactories[4]));
            _keyboardSubSystemsThreads = [];
            for (int i = 0; i < _keyboardSubSystems.Count; i++)
            for (int j = 0; j < _keyboardSubSystemsThreadFactories[i].Count; j++)
            {
                var factory = (MockThreadFactory)_keyboardSubSystemsThreadFactories[i][j];
                var currThread = new MockThread(new ThreadRunningState());
                _keyboardSubSystemsThreads.Add([currThread]);
                factory.CreateThreadReturn.Add(currThread);
            }
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _keystrokeTransmitterBuilder = new MockKeystrokeTransmitterBuilder();
            _keystrokeTransmitterBuilder.BuildReturn.Add(_keystrokeTransmitter);
            return new KeyboardSystem(_keyboardSubSystems, _keystrokeTransmitterBuilder);
        }

        /**
         * @brief Tests thread creation for all subsystems during initialization
         * 
         * Validates that each subsystem correctly creates its threads when the
         * keyboard system is initialized. This ensures all necessary thread
         * infrastructure is properly established for both keyboard device
         * detection and keystroke transmission orchestration.
         * 
         * The test verifies that every thread factory across all subsystems
         * receives exactly one create thread call during initialization.
         */
        private void _testInitializationCreatesThreads()
        {
            var keyboardSystem = _fixture();
            keyboardSystem.Initialize();
            for (int i = 0; i < _keyboardSubSystemsThreadFactories.Count; i++)
            for (int j = 0; j < _keyboardSubSystemsThreadFactories[i].Count; j++)
            {
                var factory = (MockThreadFactory) _keyboardSubSystemsThreadFactories[i][j];
                Debug.Assert(factory.CreateThreadCalls == 1);
            }
        }

        /**
         * @brief Tests idempotent initialization behavior across multiple subsystems
         * 
         * Validates that repeated initialization calls do not create additional
         * threads for any subsystem, ensuring resource efficiency and preventing
         * thread proliferation even when the system is initialized multiple times.
         * 
         * This test ensures each subsystem properly handles re-initialization
         * attempts without duplicating thread resources.
         */
        private void _testInitializationOnlyCreatesOneThread()
        {
            var keyboardSystem = _fixture();
            keyboardSystem.Initialize();
            keyboardSystem.Initialize();
            for (int i = 0; i < _keyboardSubSystemsThreadFactories.Count; i++)
            for (int j = 0; j < _keyboardSubSystemsThreadFactories[i].Count; j++)
            {
                var factory = (MockThreadFactory) _keyboardSubSystemsThreadFactories[i][j];
                Debug.Assert(factory.CreateThreadCalls == 1);
            }
        }

        /**
         * @brief Tests proper thread startup for all subsystems during system activation
         * 
         * Validates that when the keyboard system is activated, all subsystems
         * correctly start their respective threads. This ensures both keyboard
         * device detection and keystroke transmission orchestration begin
         * operating at the appropriate time.
         * 
         * The test verifies that every thread across all subsystems receives
         * exactly one start call after the system is activated.
         */
        private void _testStartSystemStartsThreads()
        {
            var keyboardSystem = _fixture();
            keyboardSystem.Initialize();
            keyboardSystem.Start();
            for (int i = 0; i < _keyboardSubSystemsThreads.Count; i++)
            for (int j = 0; j < _keyboardSubSystemsThreads[i].Count; j++)
            {
                var currThread = (MockThread) _keyboardSubSystemsThreads[i][j];
                Debug.Assert(currThread.ThreadStartCalls == 1);
            }
        }

        /**
         * @brief Tests initialization requirement enforcement across all subsystems
         * 
         * Validates that all subsystems require proper initialization before
         * activation, preventing premature thread startup across the entire
         * keyboard system. This ensures proper sequencing of system startup
         * and prevents threads from starting in an uninitialized state.
         * 
         * The test verifies that no threads from any subsystem are started
         * when activation is attempted without prior initialization.
         */
        private void _testStartSystemCannotStartThreadsWithoutInitialization()
        {
            var keyboardSystem = _fixture();
            keyboardSystem.Start();
            for (int i = 0; i < _keyboardSubSystemsThreads.Count; i++)
            for (int j = 0; j < _keyboardSubSystemsThreads[i].Count; j++)
            {
                var currThread = (MockThread)_keyboardSubSystemsThreads[i][j];
                Debug.Assert(currThread.ThreadStartCalls == 0);
            }
        }

        /**
         * @brief Tests successful keystroke transmitter injection to all subsystems
         * 
         * Validates that when a keyboard device is injected after proper configuration,
         * the system correctly creates and injects a keystroke transmitter into all
         * subsystem threads. This ensures that once both keyboard mapping and device
         * context are provided, all components receive the transmitter for handling
         * keystrokes from the detected device.
         */
        private void _testInjectKeyboardTransmitterOnKeyboardDeviceInjection()
        {
            var keyboardSystem = _fixture();
            var deviceContext = new KeyboardDeviceContext(0x1234, 0x2345);
            keyboardSystem.Initialize();
            keyboardSystem.Inject(SystemInjectType.Configuration, new KeyboardMapping());
            keyboardSystem.Inject(SystemInjectType.KeyboardDevice, deviceContext);
            for (int i = 0; i < _keyboardSubSystemsThreads.Count; i++)
            for (int j = 0; j < _keyboardSubSystemsThreads[i].Count; j++)
            {
                var currThread = (MockThread)_keyboardSubSystemsThreads[i][j];
                Debug.Assert(currThread.InjectCalls == 1);
                Debug.Assert(currThread.InjectCallArg_dataType[0] is SystemInjectType.KeystrokeTransmitter);
                Debug.Assert(currThread.InjectCallArg_data[0] == _keystrokeTransmitter);
            }
        }


        /**
         * @brief Tests keystroke transmitter injection prevention when configuration is missing
         * 
         * Validates that the system correctly prevents keystroke transmitter injection
         * across all subsystems when a keyboard device is detected but no keyboard
         * mapping has been configured. This ensures the system maintains proper
         * configuration sequencing and prevents operation with incomplete setup,
         * which could lead to undefined behavior or improper keystroke handling.
         */
        private void _testInjectKeyboardTransmitterFailsIfNoKeyboardMapping()
        {
            var keyboardSystem = _fixture();
            var deviceContext = new KeyboardDeviceContext(0x1234, 0x2345);
            keyboardSystem.Initialize();
            keyboardSystem.Inject(SystemInjectType.KeyboardDevice, deviceContext);
            for (int i = 0; i < _keyboardSubSystemsThreads.Count; i++)
            for (int j = 0; j < _keyboardSubSystemsThreads[i].Count; j++)
            {
                var currThread = (MockThread)_keyboardSubSystemsThreads[i][j];
                Debug.Assert(currThread.InjectCalls == 0);
            }
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
            _testInitializationCreatesThreads();
            _testInitializationOnlyCreatesOneThread();
            _testStartSystemStartsThreads();
            _testStartSystemCannotStartThreadsWithoutInitialization();
            _testInjectKeyboardTransmitterOnKeyboardDeviceInjection();
            _testInjectKeyboardTransmitterFailsIfNoKeyboardMapping();
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
