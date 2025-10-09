using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;


namespace MaplestoryBotNet.Systems.Keyboard
{

    public class KeyboardSystem : AbstractSystem
    {
        private AbstractInjector _keystrokeTransmitterInjector;

        private AbstractSystem _keyboardDeviceDetectorSystem;

        private AbstractKeystrokeTransmitter? _keystrokeTransmitter;

        private AbstractKeystrokeTransmitterBuilder _keystrokeTransmitterBuilder;

        public KeyboardSystem(
            AbstractSystem keyboardDeviceDetectorSystem,
            AbstractKeystrokeTransmitterBuilder keystrokeTransmitterBuilder,
            AbstractInjector keystrokeTransmitterInjector
        )
        {
            _keyboardDeviceDetectorSystem = keyboardDeviceDetectorSystem;
            _keystrokeTransmitterBuilder = keystrokeTransmitterBuilder;
            _keystrokeTransmitterInjector = keystrokeTransmitterInjector;
            _keystrokeTransmitter = null;
        }

        public override void Initialize()
        {
            _keyboardDeviceDetectorSystem.Initialize();
        }

        public override void Start()
        {
            _keyboardDeviceDetectorSystem.Start();
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (
                dataType == SystemInjectType.Configuration
                && data is KeyboardMapping
            )
            {
                var keyboardMapping = (KeyboardMapping)((KeyboardMapping)data).Copy();
                _keystrokeTransmitterBuilder.WithKeyboardMapping(keyboardMapping);
                _keystrokeTransmitter = _keystrokeTransmitterBuilder.Build();
                _keystrokeTransmitterInjector.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            }
        }

        public override void Update()
        {
            _keyboardDeviceDetectorSystem.Update();
        }
    }


    public class KeyboardSystemBuilder : AbstractSystemBuilder
    {
        private AbstractSystemBuilder _keyboardDeviceDetectorSystemBuilder;

        private List<AbstractSystem> _systems;

        public KeyboardSystemBuilder()
        {
            _keyboardDeviceDetectorSystemBuilder = new KeyboardDeviceDetectorSystemBuilder();
            _systems = [];
        }

        public override AbstractSystem Build()
        {
            var keyboardDeviceDetectorSystem = _keyboardDeviceDetectorSystemBuilder.Build();
            return new KeyboardSystem(
                keyboardDeviceDetectorSystem,
                new KeystrokeTransmitterBuilder(),
                new SystemInjector(_systems)
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            if (arg is AbstractSystem)
                _systems.Add((AbstractSystem)arg);
            _keyboardDeviceDetectorSystemBuilder.WithArg(arg);
            return this;
        }
    }
}
