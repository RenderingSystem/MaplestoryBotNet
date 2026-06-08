using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Consumables;
using MaplestoryBotNet.Systems.Device;
using MaplestoryBotNet.Systems.Device.SubSystems;
using MaplestoryBotNet.Systems.Device.SubSystems.Transmitters;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Device.Tests.SubSystems.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.Device.Tests
{
    public class DeviceSystemTests
    {
        private MockKeystrokeTransmitterBuilder _keystrokeTransmitterBuilder = new MockKeystrokeTransmitterBuilder();

        private MockMouseTransmitterBuilder _mouseTransmitterBuilder = new MockMouseTransmitterBuilder();

        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private MockMouseTransmitter _mouseTransmitter = new MockMouseTransmitter();

        private List<List<AbstractThreadFactory>> _deviceSubSystemsThreadFactories = [];

        private List<List<AbstractThread>> _deviceSubSystemsThreads = [];

        private List<AbstractSystem> _deviceSubSystems = [];

        private KeyboardMapping _keyboardMapping = new KeyboardMapping();

        private LockObject _lockObject = new LockObject();

        private DeviceSystem _fixture()
        {
            _deviceSubSystemsThreadFactories = [];
            _deviceSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _deviceSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _deviceSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _deviceSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _deviceSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _deviceSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _deviceSubSystemsThreadFactories.Add([new MockThreadFactory()]);
            _deviceSubSystems = [];
            _deviceSubSystems.Add(new DeviceDetectorSystem([_deviceSubSystemsThreadFactories[0][0]]));
            _deviceSubSystems.Add(new BottingOrchestratorSystem(_deviceSubSystemsThreadFactories[1]));
            _deviceSubSystems.Add(new RuneingOrchestratorSystem(_deviceSubSystemsThreadFactories[2]));
            _deviceSubSystems.Add(new SolvingOrchestratorSystem(_deviceSubSystemsThreadFactories[3]));
            _deviceSubSystems.Add(new LoginOrchestratorSystem(_deviceSubSystemsThreadFactories[4]));
            _deviceSubSystems.Add(new AilmentOrchestratorSystem(_deviceSubSystemsThreadFactories[5]));
            _deviceSubSystems.Add(new ConsumptionSystem(_deviceSubSystemsThreadFactories[6]));
            _deviceSubSystemsThreads = [];
            _keyboardMapping = new KeyboardMapping();
            for (int i = 0; i < _deviceSubSystems.Count; i++)
            for (int j = 0; j < _deviceSubSystemsThreadFactories[i].Count; j++)
            {
                var factory = (MockThreadFactory)_deviceSubSystemsThreadFactories[i][j];
                var currThread = new MockThread(new ThreadRunningState());
                _deviceSubSystemsThreads.Add([currThread]);
                factory.CreateThreadReturn.Add(currThread);
            }
            _keystrokeTransmitterBuilder = new MockKeystrokeTransmitterBuilder();
            _mouseTransmitterBuilder = new MockMouseTransmitterBuilder();
            return new DeviceSystem(
                _deviceSubSystems,
                _keystrokeTransmitterBuilder,
                _mouseTransmitterBuilder,
                _lockObject
            );
        }

        private DeviceSystem _transmitterFixture()
        {
            _deviceSubSystems = [
                new MockSystem(),
                new MockSystem(),
                new MockSystem(),
                new MockSystem(),
                new MockSystem()
            ];
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _keystrokeTransmitterBuilder = new MockKeystrokeTransmitterBuilder();
            _keystrokeTransmitterBuilder.BuildReturn.Add(_keystrokeTransmitter);
            _mouseTransmitter = new MockMouseTransmitter();
            _mouseTransmitterBuilder = new MockMouseTransmitterBuilder();
            _mouseTransmitterBuilder.BuildReturn.Add(_mouseTransmitter);
            _keyboardMapping = new KeyboardMapping();
            return new DeviceSystem(
                _deviceSubSystems,
                _keystrokeTransmitterBuilder,
                _mouseTransmitterBuilder,
                _lockObject
            );
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
            var deviceSystem = _fixture();
            deviceSystem.Initialize();
            for (int i = 0; i < _deviceSubSystemsThreadFactories.Count; i++)
            for (int j = 0; j < _deviceSubSystemsThreadFactories[i].Count; j++)
            {
                var factory = (MockThreadFactory) _deviceSubSystemsThreadFactories[i][j];
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
            var deviceSystem = _fixture();
            deviceSystem.Initialize();
            deviceSystem.Start();
            for (int i = 0; i < _deviceSubSystemsThreads.Count; i++)
            for (int j = 0; j < _deviceSubSystemsThreads[i].Count; j++)
            {
                var currThread = (MockThread) _deviceSubSystemsThreads[i][j];
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
            for (int i = 0; i < _deviceSubSystemsThreads.Count; i++)
            for (int j = 0; j < _deviceSubSystemsThreads[i].Count; j++)
            {
                var currThread = (MockThread)_deviceSubSystemsThreads[i][j];
                Debug.Assert(currThread.ThreadStartCalls == 0);
            }
        }

        /**
         * @brief Tests that the mouse device context is properly injected into the mouse
         * transmitter
         * 
         * When a mouse device is detected by the system, the device context (containing the
         * hardware context ID and device ID) must be passed to the mouse transmitter. The
         * transmitter uses this information to send mouse commands at the hardware level.
         */
        private void _testInjectingMouseDeviceToTransmitter()
        {
            var deviceSystem = _transmitterFixture();
            var mouseContext = new DeviceContext(0x2345, 0x3456);
            deviceSystem.Inject(SystemInjectType.MouseDevice, mouseContext);
            Debug.Assert(_mouseTransmitter.InjectMouseDeviceCalls == 1);
            Debug.Assert(_mouseTransmitter.InjectMouseDeviceCallArg_mouseDevice[0] == mouseContext);
        }

        /**
         * @brief Tests that the keyboard device context is properly injected into the keyboard
         * transmitter
         * 
         * When a keyboard device is detected by the system, the device context (containing the
         * hardware context ID and device ID) must be passed to the keyboard transmitter. The
         * transmitter uses this information to send keystroke commands at the hardware level.
         */
        private void _testInjectingKeyboardDeviceToTransmitter()
        {
            var deviceSystem = _transmitterFixture();
            var keyboardContext = new DeviceContext(0x1234, 0x2345);
            deviceSystem.Inject(SystemInjectType.Configuration, _keyboardMapping);
            deviceSystem.Inject(SystemInjectType.KeyboardDevice, keyboardContext);
            Debug.Assert(_keystrokeTransmitter.InjectKeyboardDeviceCalls == 1);
            Debug.Assert(_keystrokeTransmitter.InjectKeyboardDeviceCallArg_keyboardDevice[0] == keyboardContext);
        }

        /**
         * @brief Tests that mouse device injection triggers the mouse transmitter builder
         * 
         * Before a mouse transmitter can be used, it must be constructed with the appropriate
         * dependencies. The DeviceSystem uses a builder pattern to construct the mouse
         * transmitter, passing in the shared lock object that ensures thread-safe access to
         * the shared libraries.
         */
        private void _testInjectingMouseDeviceBuildsMouseTransmitter()
        {
            var deviceSystem = _transmitterFixture();
            var mouseContext = new DeviceContext(0x2345, 0x3456);
            var builderRef = new TestUtilities().Reference(_mouseTransmitterBuilder);
            deviceSystem.Inject(SystemInjectType.MouseDevice, mouseContext);
            Debug.Assert(_mouseTransmitterBuilder.CallOrder.Count == 2);
            Debug.Assert(_mouseTransmitterBuilder.CallOrder[0] == builderRef + "WithArg");
            Debug.Assert(_mouseTransmitterBuilder.CallOrder[1] == builderRef + "Build");
            Debug.Assert(_mouseTransmitterBuilder.WithArgCallArg_arg[0] == _lockObject);
        }

        /**
         * @brief Tests that keyboard device injection triggers the keyboard transmitter builder
         * 
         * Before a keyboard transmitter can be used, it must be constructed with the appropriate
         * dependencies. The DeviceSystem uses a builder pattern to construct the keyboard
         * transmitter, passing in the keyboard mapping (for key name to byte conversion) and
         * the shared lock object (for thread safety).
         */
        private void _testInjectingKeyboardDeviceBuildsKeyboardTransmitter()
        {
            var deviceSystem = _transmitterFixture();
            var keyboardContext = new DeviceContext(0x2345, 0x3456);
            var builderRef = new TestUtilities().Reference(_keystrokeTransmitterBuilder);
            deviceSystem.Inject(SystemInjectType.Configuration, _keyboardMapping);
            deviceSystem.Inject(SystemInjectType.KeyboardDevice, keyboardContext);
            Debug.Assert(_keystrokeTransmitterBuilder.CallOrder.Count == 3);
            Debug.Assert(_keystrokeTransmitterBuilder.CallOrder[0] == builderRef + "WithArg");
            Debug.Assert(_keystrokeTransmitterBuilder.CallOrder[1] == builderRef + "WithArg");
            Debug.Assert(_keystrokeTransmitterBuilder.CallOrder[2] == builderRef + "Build");
            Debug.Assert(_keystrokeTransmitterBuilder.WithArgCallArg_arg[0] == _keyboardMapping);
            Debug.Assert(_keystrokeTransmitterBuilder.WithArgCallArg_arg[1] == _lockObject);
        }

        /**
         * @brief Tests that the completed mouse transmitter is injected into all subsystems
         * 
         * After the mouse transmitter is built and configured, it must be distributed to all
         * subsystems that need to send mouse commands. This includes botting orchestrators,
         * rune solvers, and other systems that perform automated mouse actions.
         */
        private void _testInjectingMouseDeviceInjectsTransmitter()
        {
            var deviceSystem = _transmitterFixture();
            var keyboardContext = new DeviceContext(0x1234, 0x2345);
            var mouseContext = new DeviceContext(0x2345, 0x3456);
            deviceSystem.Inject(SystemInjectType.Configuration, _keyboardMapping);
            deviceSystem.Inject(SystemInjectType.KeyboardDevice, keyboardContext);
            foreach(MockSystem system in _deviceSubSystems)
            {
                Debug.Assert(system.InjectCalls == 0);
            }
            deviceSystem.Inject(SystemInjectType.MouseDevice, mouseContext);
            foreach (MockSystem system in _deviceSubSystems)
            {
                Debug.Assert(system.InjectCalls == 1);
                Debug.Assert(system.InjectCallArg_dataType[0] is SystemInjectType.Transmitters);
                Debug.Assert(system.InjectCallArg_data[0] is TransmitterInfo);
                Debug.Assert(
                    ((TransmitterInfo)system.InjectCallArg_data[0]!).MouseTransmitter ==
                    _mouseTransmitter
                );
            }
        }

        /**
         * @brief Tests that the completed keyboard transmitter is injected into all subsystems
         * 
         * After the keyboard transmitter is built and configured, it must be distributed to
         * all subsystems that need to send keystroke commands. This includes botting orchestrators,
         * macro executors, and other systems that perform automated keyboard actions.
         */
        private void _testInjectingKeyboardDeviceInjectsTransmitter()
        {
            var deviceSystem = _transmitterFixture();
            var keyboardContext = new DeviceContext(0x1234, 0x2345);
            var mouseContext = new DeviceContext(0x2345, 0x3456);
            deviceSystem.Inject(SystemInjectType.MouseDevice, mouseContext);
            foreach (MockSystem system in _deviceSubSystems)
            {
                Debug.Assert(system.InjectCalls == 0);
            }
            deviceSystem.Inject(SystemInjectType.Configuration, _keyboardMapping);
            deviceSystem.Inject(SystemInjectType.KeyboardDevice, keyboardContext);
            foreach (MockSystem system in _deviceSubSystems)
            {
                Debug.Assert(system.InjectCalls == 1);
                Debug.Assert(system.InjectCallArg_dataType[0] is SystemInjectType.Transmitters);
                Debug.Assert(system.InjectCallArg_data[0] is TransmitterInfo);
                Debug.Assert(
                    ((TransmitterInfo)system.InjectCallArg_data[0]!).KeystrokeTransmitter ==
                    _keystrokeTransmitter
                );
            }
        }

        public void Run()
        {
            _testInitializationCreatesThreads();
            _testStartSystemStartsThreads();
            _testStartSystemCannotStartThreadsWithoutInitialization();
            _testInjectingMouseDeviceToTransmitter();
            _testInjectingKeyboardDeviceToTransmitter();
            _testInjectingMouseDeviceBuildsMouseTransmitter();
            _testInjectingKeyboardDeviceBuildsKeyboardTransmitter();
            _testInjectingMouseDeviceInjectsTransmitter();
            _testInjectingKeyboardDeviceInjectsTransmitter();
        }
    }


    public class KeyboardSystemTestSuite
    {
        public void Run()
        {
            new DeviceSystemTests().Run();
        }
    }
}
