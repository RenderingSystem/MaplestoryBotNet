using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters;


namespace MaplestoryBotNet.Systems.Keyboard
{

    public class KeyboardSystem : AbstractSystem
    {
        private List<AbstractSystem> _keyboardSubSystems;

        private AbstractKeystrokeTransmitter? _keystrokeTransmitter;

        private AbstractKeystrokeTransmitterBuilder _keystrokeTransmitterBuilder;

        private bool _initialized;

        public KeyboardSystem(
            List<AbstractSystem> keyboardSubSystems,
            AbstractKeystrokeTransmitterBuilder keystrokeTransmitterBuilder
        )
        {
            _keyboardSubSystems = keyboardSubSystems;
            _keystrokeTransmitterBuilder = keystrokeTransmitterBuilder;
            _keystrokeTransmitter = null;
            _initialized = false;
        }

        public override void Initialize()
        {
            if (!_initialized)
            {
                for (int i = 0; i < _keyboardSubSystems.Count; i++)
                {
                    _keyboardSubSystems[i].Initialize();
                }
                _initialized = true;
            }
        }

        public override void Start()
        {
            if (_initialized)
            {
                for (int i = 0; i < _keyboardSubSystems.Count; i++)
                {
                    _keyboardSubSystems[i].Start();
                }
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (!_initialized)
            {
                return;
            }
            if (
                dataType is SystemInjectType.Configuration
                && data is KeyboardMapping keyboardMapping
            )
            {
                keyboardMapping = (KeyboardMapping)(keyboardMapping.Copy());
                _keystrokeTransmitterBuilder.WithKeyboardMapping(keyboardMapping);
                _keystrokeTransmitter = _keystrokeTransmitterBuilder.Build();
            }
            else if (
                dataType is SystemInjectType.KeyboardDevice
                && data is KeyboardDeviceContext keyboardDevice
            )
            {
                if (_keystrokeTransmitter != null)
                {
                    _keystrokeTransmitter.InjectKeyboardDevice(keyboardDevice);
                    for (int i = 0; i < _keyboardSubSystems.Count; i++)
                    {
                        _keyboardSubSystems[i].Inject(
                            SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter
                        );
                    }
                }
            }
            else
            {
                for (int i = 0; i < _keyboardSubSystems.Count; i++)
                {
                    _keyboardSubSystems[i].Inject(dataType, data);
                }
            }
        }
    }


    public class KeyboardSystemBuilder : AbstractSystemBuilder
    {
        private AbstractSystemBuilder _keyboardDeviceDetectorSystemBuilder;

        private AbstractSystemBuilder _bottingOrchestratorSystemBuilder;

        private AbstractSystemBuilder _runeingOrchestratorSystemBuilder;

        private AbstractSystemBuilder _solvingOrchestratorSystemBuilder;

        private AbstractSystemBuilder _cashShopOrchestratorSystemBuilder;

        public KeyboardSystemBuilder()
        {
            _keyboardDeviceDetectorSystemBuilder = new KeyboardDeviceDetectorSystemBuilder();
            _bottingOrchestratorSystemBuilder = new BottingOrchestratorSystemBuilder();
            _runeingOrchestratorSystemBuilder = new RuneingOrchestratorSystemBuilder();
            _solvingOrchestratorSystemBuilder = new SolvingOrchestratorSystemBuilder();
            _cashShopOrchestratorSystemBuilder = new CashShopOrchestratorSystemBuilder();
        }

        public override AbstractSystem Build()
        {
            return new KeyboardSystem(
                [
                    _keyboardDeviceDetectorSystemBuilder.Build(),
                    _bottingOrchestratorSystemBuilder.Build(),
                    _runeingOrchestratorSystemBuilder.Build(),
                    _solvingOrchestratorSystemBuilder.Build(),
                    _cashShopOrchestratorSystemBuilder.Build()
                ],
                new KeystrokeTransmitterBuilder()
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            _keyboardDeviceDetectorSystemBuilder.WithArg(arg);
            _bottingOrchestratorSystemBuilder.WithArg(arg);
            return this;
        }
    }
}
