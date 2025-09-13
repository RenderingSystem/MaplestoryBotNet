using System.Windows.Input;
using Interception;
using MaplestoryBotNet.LibraryWrappers;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard;

namespace MaplestoryBotNet.Systems.Keyboard
{
    public abstract class AbstractKeystrokeTransmitter
    {
        public abstract void InjectKeyboardDevice(KeyboardDeviceContext keyboardDevice);

        public abstract void Keydown(string keystroke);

        public abstract void KeyUp(string keystroke);
    }


    public class KeystrokeTransmitter : AbstractKeystrokeTransmitter
    {
        private AbstractInterceptionLibrary _interceptionLibrary;

        private KeyboardDeviceContext? _keyboardDevice;

        private KeyboardMapping _keyboardMapping;

        public KeystrokeTransmitter(
            AbstractInterceptionLibrary interceptionLibrary,
            KeyboardMapping KeyboardMapping
        )
        {
            _interceptionLibrary = interceptionLibrary;
            _keyboardMapping = KeyboardMapping;
            _keyboardDevice = null;
        }

        public override void InjectKeyboardDevice(KeyboardDeviceContext keyboardDevice)
        {
            _keyboardDevice = keyboardDevice;
        }

        public override void Keydown(string keystroke)
        {
            if (_keyboardDevice == null)
                return;
            var context = _keyboardDevice.Context;
            var device = _keyboardDevice.Device;
            var stroke = new InterceptionInterop.Stroke();
            // _interceptionLibrary.Send(context, device,)
        }

        public override void KeyUp(string keystroke)
        {
            if (_keyboardDevice == null)
                return;
        }
    }
}
