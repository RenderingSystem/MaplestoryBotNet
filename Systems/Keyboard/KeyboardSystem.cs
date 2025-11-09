using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;


namespace MaplestoryBotNet.Systems.Keyboard
{

    public class KeyboardSystem : AbstractSystem
    {
        private AbstractSystem _keyboardDeviceDetectorSystem;

        private AbstractKeystrokeTransmitter? _keystrokeTransmitter;

        private AbstractKeystrokeTransmitterBuilder _keystrokeTransmitterBuilder;

        public KeyboardSystem(
            AbstractSystem keyboardDeviceDetectorSystem,
            AbstractKeystrokeTransmitterBuilder keystrokeTransmitterBuilder
        )
        {
            _keyboardDeviceDetectorSystem = keyboardDeviceDetectorSystem;
            _keystrokeTransmitterBuilder = keystrokeTransmitterBuilder;
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
                && data is KeyboardMapping keyboardMapping
            )
            {
                keyboardMapping = (KeyboardMapping)(keyboardMapping.Copy());
                _keystrokeTransmitterBuilder.WithKeyboardMapping(keyboardMapping);
                _keystrokeTransmitter = _keystrokeTransmitterBuilder.Build();
                _keyboardDeviceDetectorSystem.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            }
            else
            {
                _keyboardDeviceDetectorSystem.Inject(dataType, data);
            }
        }
    }


    public class KeyboardSystemBuilder : AbstractSystemBuilder
    {
        private AbstractSystemBuilder _keyboardDeviceDetectorSystemBuilder;

        public KeyboardSystemBuilder()
        {
            _keyboardDeviceDetectorSystemBuilder = new KeyboardDeviceDetectorSystemBuilder();
        }

        public override AbstractSystem Build()
        {
            var keyboardDeviceDetectorSystem = _keyboardDeviceDetectorSystemBuilder.Build();
            return new KeyboardSystem(
                keyboardDeviceDetectorSystem,
                new KeystrokeTransmitterBuilder()
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            _keyboardDeviceDetectorSystemBuilder.WithArg(arg);
            return this;
        }
    }
}
