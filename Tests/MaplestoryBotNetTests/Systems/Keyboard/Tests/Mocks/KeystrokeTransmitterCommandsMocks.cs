using Interception;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks
{
    public class MockMacroSleeper : AbstractMacroSleeper
    {
        public List<string> CallOrder = [];

        public int SleepCalls = 0;
        public List<int> SleepCallArg_milliseconds = [];
        public override void Sleep(int milliseconds)
        {
            var callReference = new TestUtilities().Reference(this) + "Sleep";
            CallOrder.Add(callReference);
            SleepCalls++;
            SleepCallArg_milliseconds.Add(milliseconds);
        }
    }


    public class MockMacroRandom : AbstractMacroRandom
    {
        public List<string> CallOrder = [];

        public int NextCalls = 0;
        public int NextIndex = 0;
        public List<int> NextCallArg_minValue = [];
        public List<int> NextCallArg_maxValue = [];
        public List<int> NextReturn = [];
        public override int Next(int minValue, int maxValue)
        {
            var callReference = new TestUtilities().Reference(this) + "Next";
            CallOrder.Add(callReference);
            NextCalls++;
            NextCallArg_minValue.Add(minValue);
            NextCallArg_maxValue.Add(maxValue);
            if (NextIndex < NextReturn.Count)
                return NextReturn[NextIndex++];
            throw new IndexOutOfRangeException();
        }
    }


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


    public class MockParsedMacroCommandBuilder : AbstractParsedMacroCommandBuilder
    {
        public List<string> CallOrder = [];

        public int BuildCalls = 0;
        public int BuildIndex = 0;
        public List<AbstractParsedMacroCommand> BuildReturn = [];
        public override AbstractParsedMacroCommand Build()
        {
            var callReference = new TestUtilities().Reference(this) + "Build";
            CallOrder.Add(callReference);
            BuildCalls++;
            if (BuildIndex < BuildReturn.Count)
                return BuildReturn[BuildIndex++];
            throw new IndexOutOfRangeException();
        }

        public int WithArgCalls = 0;
        public List<object> WithArgCallArg_args = [];
        public override AbstractParsedMacroCommandBuilder WithArg(object args)
        {
            var callReference = new TestUtilities().Reference(this) + "WithArg";
            CallOrder.Add(callReference);
            WithArgCalls++;
            WithArgCallArg_args.Add(args);
            return this;
        }
    }


    public class MockParsedMacroCommand : AbstractParsedMacroCommand
    {
        public List<string> CallOrder = [];

        public int RunCalls = 0;
        public override void Run()
        {
            var callReference = new TestUtilities().Reference(this) + "Run";
            CallOrder.Add(callReference);
            RunCalls++;
        }
    }


    public class MockMacroCommandsParser : AbstractMacroCommandParser
    {
        public List<string> CallOrder = [];

        public int ParseCalls = 0;
        public int ParseIndex = 0;
        public List<string> ParseCallArg_macroCommand = [];
        public List<AbstractParsedMacroCommand?> ParseReturn = [];
        public override AbstractParsedMacroCommand? Parse(string macroCommand)
        {
            var callReference = new TestUtilities().Reference(this) + "Parse";
            CallOrder.Add(callReference);
            ParseCallArg_macroCommand.Add(macroCommand);
            ParseCalls++;
            if (ParseIndex < ParseReturn.Count)
                return ParseReturn[ParseIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockMacroCommandsExecutor : AbstractMacroCommandsExecutor
    {
        public List<string> CallOrder = [];

        public int ExecuteCalls = 0;
        public List<List<string>> ExecuteCallArg_macroCommands = [];
        public override void Execute(List<string> macroCommands)
        {
            var callReference = new TestUtilities().Reference(this) + "Execute";
            CallOrder.Add(callReference);
            ExecuteCalls++;
            ExecuteCallArg_macroCommands.Add(macroCommands);
        }
    }

}
