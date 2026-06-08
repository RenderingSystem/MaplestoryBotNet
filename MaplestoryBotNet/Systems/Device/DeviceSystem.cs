using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Consumables;
using MaplestoryBotNet.Systems.Device.SubSystems;
using MaplestoryBotNet.Systems.Device.SubSystems.Transmitters;
using MaplestoryBotNet.ThreadingUtils;


namespace MaplestoryBotNet.Systems.Device
{
    public class TransmitterInfo
    {
        public AbstractKeystrokeTransmitter? KeystrokeTransmitter;

        public AbstractMouseTransmitter? MouseTransmitter;
    }


    public class DeviceSystem : AbstractSystem
    {
        private List<AbstractSystem> _deviceSubSystems;

        private AbstractKeystrokeTransmitter? _keystrokeTransmitter;

        private AbstractKeystrokeTransmitterBuilder _keystrokeTransmitterBuilder;

        private AbstractMouseTransmitter? _mouseTransmitter;

        private AbstractMouseTransmitterBuilder _mouseTransmitterBuilder;

        private LockObject _lockObject;

        private KeyboardMapping _keyboardMapping;

        public DeviceSystem(
            List<AbstractSystem> deviceSubSystems,
            AbstractKeystrokeTransmitterBuilder keystrokeTransmitterBuilder,
            AbstractMouseTransmitterBuilder mouseTransmitterBuilder,
            LockObject lockObject
        )
        {
            _deviceSubSystems = deviceSubSystems;
            _keystrokeTransmitter = null;
            _keystrokeTransmitterBuilder = keystrokeTransmitterBuilder;
            _mouseTransmitter = null;
            _mouseTransmitterBuilder = mouseTransmitterBuilder;
            _lockObject = lockObject;
            _keyboardMapping = new KeyboardMapping();
        }

        public override void Initialize()
        {
            for (int i = 0; i < _deviceSubSystems.Count; i++)
            {
                _deviceSubSystems[i].Initialize();
            }
        }

        public override void Start()
        {
            for (int i = 0; i < _deviceSubSystems.Count; i++)
            {
                _deviceSubSystems[i].Start();
            }
        }

        private void _tryInjectTransmitters()
        {
            if (_mouseTransmitter != null && _keystrokeTransmitter != null)
            {
                for (int i = 0; i < _deviceSubSystems.Count; i++)
                {
                    _deviceSubSystems[i].Inject(
                        SystemInjectType.Transmitters,
                        new TransmitterInfo
                        {
                            KeystrokeTransmitter = _keystrokeTransmitter,
                            MouseTransmitter = _mouseTransmitter
                        }
                    );
                }
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.Configuration
                && data is KeyboardMapping keyboardMapping
            )
            {
                _keyboardMapping = keyboardMapping;
            }
            else if (
                dataType is SystemInjectType.KeyboardDevice
                && data is DeviceContext keyboardDevice
            )
            {
                if (_keystrokeTransmitter == null)
                {
                    _keystrokeTransmitter = _keystrokeTransmitterBuilder
                        .WithArg(_keyboardMapping)
                        .WithArg(_lockObject)
                        .Build();
                }
                _keystrokeTransmitter.InjectKeyboardDevice(keyboardDevice);
                _tryInjectTransmitters();
            }
            else if (
                dataType is SystemInjectType.MouseDevice
                && data is DeviceContext mouseDevice
            )
            {
                if (_mouseTransmitter == null)
                {
                    _mouseTransmitter = _mouseTransmitterBuilder
                        .WithArg(_lockObject)
                        .Build();
                }
                _mouseTransmitter.InjectMouseDevice(mouseDevice);
                _tryInjectTransmitters();
            }
            else
            {
                for (int i = 0; i < _deviceSubSystems.Count; i++)
                {
                    _deviceSubSystems[i].Inject(dataType, data);
                }
            }
        }
    }


    public class DeviceSystemBuilder : AbstractSystemBuilder
    {
        private AbstractSystemBuilder _keyboardDeviceDetectorSystemBuilder;

        private AbstractSystemBuilder _bottingOrchestratorSystemBuilder;

        private AbstractSystemBuilder _runeingOrchestratorSystemBuilder;

        private AbstractSystemBuilder _solvingOrchestratorSystemBuilder;

        private AbstractSystemBuilder _loginOrchestratorSystemBuilder;

        private AbstractSystemBuilder _ailmentOrchestratorSystemBuilder;

        private AbstractSystemBuilder _consumptionSystemBuilder;

        public DeviceSystemBuilder()
        {
            _keyboardDeviceDetectorSystemBuilder = new DeviceDetectorSystemBuilder();
            _bottingOrchestratorSystemBuilder = new BottingOrchestratorSystemBuilder();
            _runeingOrchestratorSystemBuilder = new RuneingOrchestratorSystemBuilder();
            _solvingOrchestratorSystemBuilder = new SolvingOrchestratorSystemBuilder();
            _loginOrchestratorSystemBuilder = new LoginOrchestratorSystemBuilder();
            _ailmentOrchestratorSystemBuilder = new AilmentOrchestratorSystemBuilder();
            _consumptionSystemBuilder = new ConsumptionSystemBuilder();
        }

        public override AbstractSystem Build()
        {
            return new DeviceSystem(
                [
                    _keyboardDeviceDetectorSystemBuilder.Build(),
                    _bottingOrchestratorSystemBuilder.Build(),
                    _runeingOrchestratorSystemBuilder.Build(),
                    _solvingOrchestratorSystemBuilder.Build(),
                    _loginOrchestratorSystemBuilder.Build(),
                    _ailmentOrchestratorSystemBuilder.Build(),
                    _consumptionSystemBuilder.Build()
                ],
                new KeystrokeTransmitterBuilder(),
                new MouseTransmitterBuilder(),
                new LockObject()
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
