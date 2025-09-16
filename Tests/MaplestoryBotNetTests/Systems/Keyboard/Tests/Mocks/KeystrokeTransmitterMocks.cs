using Interception;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks
{
    public class MockKeystrokeTransmitter : AbstractKeystrokeTransmitter
    {
        public List<string> CallOrder = [];

        public int InjectKeyboardDeviceCalls = 0;
        public List<KeyboardDeviceContext> InjectKeyboardDeviceCallArg_keyboardDevice = [];
        public override void InjectKeyboardDevice(KeyboardDeviceContext keyboardDevice)
        {
            var callReference = new TestUtilities().Reference(this) + "InjectKeyboardDevice";
            CallOrder.Add(callReference);
            InjectKeyboardDeviceCalls++;
            InjectKeyboardDeviceCallArg_keyboardDevice.Add(keyboardDevice);
        }

        public int KeydownCalls = 0;
        public List<string> KeydownCallArg_keystroke = [];
        public override void Keydown(string keystroke)
        {
            var callReference = new TestUtilities().Reference(this) + "Keydown";
            CallOrder.Add(callReference);
            KeydownCalls++;
            KeydownCallArg_keystroke.Add(keystroke);
        }

        public int KeyupCalls = 0;
        public List<string> KeyupCallArg_keystroke = [];
        public override void Keyup(string keystroke)
        {
            var callReference = new TestUtilities().Reference(this) + "Keyup";
            CallOrder.Add(callReference);
            KeyupCalls++;
            KeyupCallArg_keystroke.Add(keystroke);
        }
    }


    public class MockKeystrokeConverter : AbstractKeystrokeConverter
    {
        public List<string> CallOrder = [];

        public int ConvertToKeydownCalls = 0;
        public int ConvertToKeydownIndex = 0;
        public List<string> ConvertToKeydownCallArg_stroke = [];
        public List<InterceptionInterop.KeyStroke> ConvertToKeydownReturn = [];
        public override InterceptionInterop.KeyStroke ConvertToKeydown(string stroke)
        {
            var callReference = new TestUtilities().Reference(this) + "ConvertToKeydown";
            CallOrder.Add(callReference);
            ConvertToKeydownCalls++;
            ConvertToKeydownCallArg_stroke.Add(stroke);
            if (ConvertToKeydownIndex < ConvertToKeydownReturn.Count)
                return ConvertToKeydownReturn[ConvertToKeydownIndex++];
            throw new IndexOutOfRangeException();
        }

        public int ConvertToKeyupCalls = 0;
        public int ConvertToKeyupIndex = 0;
        public List<string> ConvertToKeyupCallArg_stroke = [];
        public List<InterceptionInterop.KeyStroke> ConvertToKeyupReturn = [];
        public override InterceptionInterop.KeyStroke ConvertToKeyup(string stroke)
        {
            var callReference = new TestUtilities().Reference(this) + "ConvertToKeyup";
            CallOrder.Add(callReference);
            ConvertToKeyupCalls++;
            ConvertToKeyupCallArg_stroke.Add(stroke);
            if (ConvertToKeyupIndex < ConvertToKeyupReturn.Count)
                return ConvertToKeyupReturn[ConvertToKeyupIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockKeystrokeTransmitterBuilder : AbstractKeystrokeTransmitterBuilder
    {
        public List<string> CallOrder = [];

        public int BuildCalls = 0;
        public int BuildIndex = 0;
        public List<AbstractKeystrokeTransmitter> BuildReturn = [];
        public override AbstractKeystrokeTransmitter Build()
        {
            var callReference = new TestUtilities().Reference(this) + "Build";
            CallOrder.Add(callReference);
            BuildCalls++;
            if (BuildIndex < BuildReturn.Count)
                return BuildReturn[BuildIndex++];
            throw new IndexOutOfRangeException();
        }

        public int WithKeyboardMappingCalls = 0;
        public int WithKeyboardMappingIndex = 0;
        public List<KeyboardMapping> WithKeyboardMappingCallArg_keyboardMapping = [];
        public override AbstractKeystrokeTransmitterBuilder WithKeyboardMapping(KeyboardMapping keyboardMapping)
        {
            var callReference = new TestUtilities().Reference(this) + "WithKeyboardMapping";
            CallOrder.Add(callReference);
            WithKeyboardMappingCalls++;
            WithKeyboardMappingCallArg_keyboardMapping.Add(keyboardMapping);
            return this;
        }
    }
}
