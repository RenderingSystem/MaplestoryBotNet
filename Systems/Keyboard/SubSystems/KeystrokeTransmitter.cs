using System.Globalization;
using Interception;
using MaplestoryBotNet.LibraryWrappers;
using MaplestoryBotNet.Systems.Configuration;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems
{
    public abstract class AbstractKeystrokeTransmitter
    {
        public abstract void InjectKeyboardDevice(KeyboardDeviceContext keyboardDevice);

        public abstract void Keydown(string keystroke);

        public abstract void Keyup(string keystroke);
    }


    public abstract class AbstractKeystrokeConverter
    {

        public abstract InterceptionInterop.KeyStroke ConvertToKeydown(string stroke);

        public abstract InterceptionInterop.KeyStroke ConvertToKeyup(string stroke);

    }


    public abstract class AbstractKeystrokeTransmitterBuilder
    {
        public abstract AbstractKeystrokeTransmitterBuilder WithKeyboardMapping(KeyboardMapping keyboardMapping);

        public abstract AbstractKeystrokeTransmitter Build();
    }


    public class KeystrokeConverter : AbstractKeystrokeConverter
    {
        private InterceptionInterop.KeyStroke _parseStroke(string stroke)
        {
            var split = stroke.Split(' ');
            var keystroke = new InterceptionInterop.KeyStroke();
            for (int i = 0; i < split.Length; i++)
            {
                var hex = split[i].ToUpper();
                if (hex.StartsWith("0X"))
                    hex = hex.Substring(2);
                if (hex == "E0")
                    keystroke.State |= InterceptionInterop.KeyState.E0;
                else if (hex == "E1")
                    keystroke.State |= InterceptionInterop.KeyState.E1;
                else
                    keystroke.Code = ushort.Parse(hex, NumberStyles.HexNumber);
            }
            return keystroke;
        }

        public override InterceptionInterop.KeyStroke ConvertToKeydown(string keystrokeString)
        {
            var keystroke = _parseStroke(keystrokeString);
            keystroke.State |= InterceptionInterop.KeyState.Down;
            return keystroke;
        }

        public override InterceptionInterop.KeyStroke ConvertToKeyup(string keystrokeString)
        {
            var keystroke = _parseStroke(keystrokeString);
            keystroke.State |= InterceptionInterop.KeyState.Up;
            return keystroke;
        }
    }


    public class KeystrokeTransmitter : AbstractKeystrokeTransmitter
    {
        private AbstractInterceptionLibrary _interceptionLibrary;

        private KeyboardMapping _keyboardMapping;

        private AbstractKeystrokeConverter _keystrokeConverter;

        private KeyboardDeviceContext? _keyboardDeviceValue;

        private ReaderWriterLockSlim _keyboardDeviceLock;

        private object _sendLock;

        private KeyboardDeviceContext? _keyboardDevice
        {
            get
            {
                try
                {
                    _keyboardDeviceLock.EnterReadLock();
                    return _keyboardDeviceValue;
                }
                finally
                {
                    _keyboardDeviceLock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _keyboardDeviceLock.EnterWriteLock();
                    _keyboardDeviceValue = value;
                }
                finally
                {
                    _keyboardDeviceLock.ExitWriteLock();
                }
            }
        }

        public KeystrokeTransmitter(
            AbstractInterceptionLibrary interceptionLibrary,
            AbstractKeystrokeConverter keystrokeConverter,
            KeyboardMapping KeyboardMapping
        )
        {
            _interceptionLibrary = interceptionLibrary;
            _keyboardMapping = KeyboardMapping;
            _keystrokeConverter = keystrokeConverter;
            _keyboardDeviceValue = null;
            _keyboardDeviceLock = new ReaderWriterLockSlim();
            _sendLock = new object();
        }


        private void _sendKeyStroke(InterceptionInterop.KeyStroke keystroke)
        {
            var keyboardDevice = _keyboardDevice;
            if (keyboardDevice == null)
                return;
            var context = keyboardDevice.Context;
            var device = keyboardDevice.Device;
            unsafe
            {
                var stroke = (InterceptionInterop.Stroke*)&keystroke;
                lock (_sendLock) { _interceptionLibrary.Send(context, device, stroke, 1); }
            }
        }

        public override void InjectKeyboardDevice(KeyboardDeviceContext keyboardDevice)
        {
            _keyboardDevice = keyboardDevice;
        }

        public override void Keydown(string keystroke)
        {
            var byteString = _keyboardMapping.GetMapping(keystroke);
            if (byteString.Length > 0)
            {
                var keydown = _keystrokeConverter.ConvertToKeydown(byteString);
                _sendKeyStroke(keydown);
            }
        }

        public override void Keyup(string keystroke)
        {
            var byteString = _keyboardMapping.GetMapping(keystroke);
            if (byteString.Length > 0)
            {
                var keyup = _keystrokeConverter.ConvertToKeyup(byteString);
                _sendKeyStroke(keyup);
            }
        }
    }


    public class KeystrokeTransmitterBuilder : AbstractKeystrokeTransmitterBuilder
    {
        private KeyboardMapping? _keyboardMapping;

        public override AbstractKeystrokeTransmitterBuilder WithKeyboardMapping(
            KeyboardMapping keyboardMapping
        )
        {
            _keyboardMapping = keyboardMapping;
            return this;
        }

        public override AbstractKeystrokeTransmitter Build()
        {
            if (_keyboardMapping == null)
                _keyboardMapping = new KeyboardMapping();
            return new KeystrokeTransmitter(
                new InterceptionLibrary(),
                new KeystrokeConverter(),
                _keyboardMapping
            );
        }
    }
}
