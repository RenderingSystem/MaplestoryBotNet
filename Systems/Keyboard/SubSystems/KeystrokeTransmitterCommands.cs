using Interception;
using MaplestoryBotNet.LibraryWrappers;
using MaplestoryBotNet.Systems.Configuration;
using System.Globalization;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems
{
    public abstract class AbstractMacroSleeper
    {
        public abstract void Sleep(int milliseconds);
    }


    public abstract class AbstractMacroRandom
    {
        public abstract int Next(int minValue, int maxValue);
    }


    public abstract class AbstractMacroCommandsExecutor
    {
        public abstract void Execute(List<string> macroCommands);
    }


    public abstract class AbstractMacroCommandsExecutorBuilder
    {
        public abstract AbstractMacroCommandsExecutorBuilder WithArg(object arg);

        public abstract AbstractMacroCommandsExecutor Build();
    }


    public abstract class AbstractParsedMacroCommand
    {
        public abstract void Run();
    }


    public abstract class AbstractParsedMacroCommandBuilder
    {
        public abstract AbstractParsedMacroCommand Build();

        public abstract AbstractParsedMacroCommandBuilder WithArg(object args);
    }


    public abstract class AbstractMacroCommandParser
    {
        public abstract AbstractParsedMacroCommand? Parse(string macroCommand);
    }


    public abstract class AbstractBracketContentsParser
    {
        public abstract List<string> Parse(string macroCommand);
    }


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
        public abstract AbstractKeystrokeTransmitterBuilder WithKeyboardMapping(
            KeyboardMapping keyboardMapping
        );

        public abstract AbstractKeystrokeTransmitter Build();
    }


    public class MacroSleeper : AbstractMacroSleeper
    {
        public override void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }
    }


    public class MacroRandom : AbstractMacroRandom
    {
        public override int Next(int minValue, int maxValue)
        {
            return Random.Shared.Next(minValue, maxValue + 1);
        }
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

        private volatile KeyboardDeviceContext? _keyboardDeviceValue;

        private object _sendLock;

        private KeyboardDeviceContext? _keyboardDevice
        {
            get => _keyboardDeviceValue;

            set => _keyboardDeviceValue = value;
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
            _sendLock = new object();
        }


        private void _sendKeyStroke(InterceptionInterop.KeyStroke keystroke)
        {
            var keyboardDevice = _keyboardDevice;
            if (keyboardDevice == null)
            {
                return;
            }
            var context = keyboardDevice.Context;
            var device = keyboardDevice.Device;
            unsafe
            {
                var stroke = (InterceptionInterop.Stroke*)&keystroke;
                lock (_sendLock)
                {
                    _interceptionLibrary.Send(context, device, stroke, 1);
                }
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
            return new KeystrokeTransmitter(
                new InterceptionLibrary(),
                new KeystrokeConverter(),
                _keyboardMapping ?? new KeyboardMapping()
            );
        }
    }


    public class BracketContentsParser : AbstractBracketContentsParser
    {
        private string _input = "";

        private List<string> _contents = [];

        private int _recursiveParse(int startIndex)
        {
            var currIndex = startIndex;
            while (currIndex < _input.Length)
            {
                if (_input[currIndex] == '{')
                {
                    var endIndex = _recursiveParse(currIndex + 1);
                    if (endIndex < _input.Length)
                    {
                        var content = _input.Substring(currIndex + 1, endIndex - currIndex - 1);
                        if (!content.Contains('{') && !content.Contains('}'))
                        {
                            _contents.Add(content);
                        }
                    }
                    currIndex = endIndex + 1;
                }
                else if (_input[currIndex] == '}')
                {
                    return currIndex;
                }
                else
                {
                    currIndex++;
                }
            }
            return currIndex;
        }

        public override List<string> Parse(string macroCommand)
        {
            _input = macroCommand;
            _contents = [];
            _recursiveParse(0);
            return _contents;
        }
    }


    public class WaitMacroCommand : AbstractParsedMacroCommand
    {
        private int _waitMilliseconds;

        private AbstractMacroSleeper _macroSleeper;

        public WaitMacroCommand(
            int waitMilliseconds,
            AbstractMacroSleeper macroSleeper
        )
        {
            _waitMilliseconds = waitMilliseconds;
            _macroSleeper = macroSleeper;
        }

        public override void Run()
        {
            _macroSleeper.Sleep(_waitMilliseconds);
        }
    }


    public class WaitMacroCommandBuilder : AbstractParsedMacroCommandBuilder
    {
        private int _waitMilliseconds;

        private AbstractMacroSleeper _macroSleeper;

        public WaitMacroCommandBuilder(AbstractMacroSleeper macroSleeper)
        {
            _waitMilliseconds = 0;
            _macroSleeper = macroSleeper;
        }

        public override AbstractParsedMacroCommand Build()
        {
            return new WaitMacroCommand(_waitMilliseconds, _macroSleeper);
        }

        public override AbstractParsedMacroCommandBuilder WithArg(object args)
        {
            if (args is int waitMilliseconds)
            {
                _waitMilliseconds = waitMilliseconds;
            }
            return this;
        }
    }


    public class KeyPressMacroCommand : AbstractParsedMacroCommand
    {
        private string _key;

        private int _waitMilliseconds;

        private AbstractMacroSleeper _macroSleeper;

        private AbstractKeystrokeTransmitter _keystrokeTransmitter;

        public KeyPressMacroCommand(
            string key,
            int waitMilliseconds,
            AbstractMacroSleeper macroSleeper,
            AbstractKeystrokeTransmitter keystrokeTransmitter
        )
        {
            _key = key;
            _waitMilliseconds = waitMilliseconds;
            _macroSleeper = macroSleeper;
            _keystrokeTransmitter = keystrokeTransmitter;
        }

        public override void Run()
        {
            _keystrokeTransmitter.Keydown(_key);
            _macroSleeper.Sleep(_waitMilliseconds);
            _keystrokeTransmitter.Keyup(_key);
        }
    }


    public class KeyPressMacroCommandBuilder : AbstractParsedMacroCommandBuilder
    {
        private string _key;

        private int _waitMilliseconds;

        private AbstractMacroSleeper _macroSleeper;

        private AbstractKeystrokeTransmitter _keystrokeTransmitter;

        public KeyPressMacroCommandBuilder(
            AbstractMacroSleeper macroSleeper,
            AbstractKeystrokeTransmitter keystrokeTransmitter
        )
        {
            _key = "";
            _waitMilliseconds = 0;
            _macroSleeper = macroSleeper;
            _keystrokeTransmitter = keystrokeTransmitter;
        }

        public override AbstractParsedMacroCommand Build()
        {
            return new KeyPressMacroCommand(
                _key, _waitMilliseconds, _macroSleeper, _keystrokeTransmitter
            );
        }

        public override AbstractParsedMacroCommandBuilder WithArg(object args)
        {
            if (args is string key)
            {
                _key = key;
            }
            else if (args is int waitMilliseconds)
            {
                _waitMilliseconds = waitMilliseconds;
            }
            return this;
        }
    }


    public class KeyDownMacroCommand : AbstractParsedMacroCommand
    {
        private string _key;

        private AbstractKeystrokeTransmitter _keystrokeTransmitter;

        public KeyDownMacroCommand(
            string key, AbstractKeystrokeTransmitter keystrokeTransmitter
        )
        {
            _key = key;
            _keystrokeTransmitter = keystrokeTransmitter;
        }

        public override void Run()
        {
            _keystrokeTransmitter.Keydown(_key);
        }
    }


    public class KeyDownMacroCommandBuilder : AbstractParsedMacroCommandBuilder
    {
        private string _key;

        private AbstractKeystrokeTransmitter _keystrokeTransmitter;

        public KeyDownMacroCommandBuilder(
            AbstractKeystrokeTransmitter keystrokeTransmitter
        )
        {
            _key = "";
            _keystrokeTransmitter = keystrokeTransmitter;
        }

        public override AbstractParsedMacroCommand Build()
        {
            return new KeyDownMacroCommand(_key, _keystrokeTransmitter);
        }

        public override AbstractParsedMacroCommandBuilder WithArg(object args)
        {
            if (args is string key)
            {
                _key = key;
            }
            return this;
        }
    }


    public class KeyUpMacroCommand : AbstractParsedMacroCommand
    {
        private string _key;

        private AbstractKeystrokeTransmitter _keystrokeTransmitter;

        public KeyUpMacroCommand(
            string key,
            AbstractKeystrokeTransmitter keystrokeTransmitter
        )
        {
            _key = key;
            _keystrokeTransmitter = keystrokeTransmitter;
        }

        public override void Run()
        {
            _keystrokeTransmitter.Keyup(_key);
        }
    }


    public class KeyUpMacroCommandBuilder : AbstractParsedMacroCommandBuilder
    {
        private string _key;

        private AbstractKeystrokeTransmitter _keystrokeTransmitter;

        public KeyUpMacroCommandBuilder(
            AbstractKeystrokeTransmitter keystrokeTransmitter
        )
        {
            _key = "";
            _keystrokeTransmitter = keystrokeTransmitter;
        }

        public override AbstractParsedMacroCommand Build()
        {
            return new KeyUpMacroCommand(_key, _keystrokeTransmitter);
        }

        public override AbstractParsedMacroCommandBuilder WithArg(object args)
        {
            if (args is string key)
            {
                _key = key;
            }
            return this;
        }
    }


    public class WaitMacroCommandParser : AbstractMacroCommandParser
    {
        private AbstractMacroRandom _macroRandom;

        private AbstractBracketContentsParser _bracketContentsParser;

        private AbstractParsedMacroCommandBuilder _macroCommandBuilder;

        public WaitMacroCommandParser(
            AbstractMacroRandom macroRandom,
            AbstractBracketContentsParser bracketContentsParser,
            AbstractParsedMacroCommandBuilder macroCommandBuilder
        )
        {
            _macroRandom = macroRandom;
            _bracketContentsParser = bracketContentsParser;
            _macroCommandBuilder = macroCommandBuilder;
        }

        public override AbstractParsedMacroCommand? Parse(string macroCommand)
        {
            if (macroCommand.ToLower().StartsWith("wait"))
            {
                var contents = _bracketContentsParser.Parse(macroCommand);
                if (
                    contents.Count == 2
                    && int.TryParse(contents[0], out int interval1)
                    && int.TryParse(contents[1], out int interval2)
                )
                {
                    var minInterval = Math.Min(interval1, interval2);
                    var maxInterval = Math.Max(interval1, interval2);
                    var milliseconds = Math.Max(0, _macroRandom.Next(minInterval, maxInterval));
                    return _macroCommandBuilder.WithArg(milliseconds).Build();
                }
            }
            return null;
        }
    }

    
    public class KeyPressMacroCommandParser : AbstractMacroCommandParser
    {
        private AbstractMacroRandom _macroRandom;

        private AbstractBracketContentsParser _bracketContentsParser;

        private AbstractParsedMacroCommandBuilder _macroCommandBuilder;

        public KeyPressMacroCommandParser(
            AbstractMacroRandom macroRandom,
            AbstractBracketContentsParser bracketContentsParser,
            AbstractParsedMacroCommandBuilder macroCommandBuilder
        )
        {
            _macroRandom = macroRandom;
            _bracketContentsParser = bracketContentsParser;
            _macroCommandBuilder = macroCommandBuilder;
        }

        public override AbstractParsedMacroCommand? Parse(string macroCommand)
        {
            if (macroCommand.ToLower().StartsWith("key press"))
            {
                var contents = _bracketContentsParser.Parse(macroCommand);
                if (
                    contents.Count >= 3
                    && contents.All((content) => { return content != ""; })
                    && int.TryParse(contents[contents.Count - 2], out int interval1)
                    && int.TryParse(contents[contents.Count - 1], out int interval2)
                )
                {
                    var keyIndex = _macroRandom.Next(0, contents.Count - 3);
                    var key = contents[keyIndex];
                    var minInterval = Math.Min(interval1, interval2);
                    var maxInterval = Math.Max(interval1, interval2);
                    var milliseconds = Math.Max(0, _macroRandom.Next(minInterval, maxInterval));
                    return _macroCommandBuilder.WithArg(key).WithArg(milliseconds).Build();
                }
            }
            return null;
        }
    }


    public class KeyDownMacroCommandParser : AbstractMacroCommandParser
    {
        private AbstractBracketContentsParser _bracketContentsParser;

        private AbstractParsedMacroCommandBuilder _macroCommandBuilder;

        public KeyDownMacroCommandParser(
            AbstractBracketContentsParser bracketContentsParser,
            AbstractParsedMacroCommandBuilder macroCommandBuilder
        )
        {
            _bracketContentsParser = bracketContentsParser;
            _macroCommandBuilder = macroCommandBuilder;
        }

        public override AbstractParsedMacroCommand? Parse(string macroCommand)
        {
            if (macroCommand.ToLower().StartsWith("key down"))
            {
                var contents = _bracketContentsParser.Parse(macroCommand);
                if (contents.Count == 1 && contents[0] != "")
                {
                    return _macroCommandBuilder.WithArg(contents[0]).Build();
                }
            }
            return null;
        }
    }


    public class KeyUpMacroCommandParser : AbstractMacroCommandParser
    {
        private AbstractBracketContentsParser _bracketContentsParser;

        private AbstractParsedMacroCommandBuilder _macroCommandBuilder;

        public KeyUpMacroCommandParser(
            AbstractBracketContentsParser bracketContentsParser,
            AbstractParsedMacroCommandBuilder macroCommandBuilder
        )
        {
            _bracketContentsParser = bracketContentsParser;
            _macroCommandBuilder = macroCommandBuilder;
        }

        public override AbstractParsedMacroCommand? Parse(string macroCommand)
        {
            if (macroCommand.ToLower().StartsWith("key up"))
            {
                var contents = _bracketContentsParser.Parse(macroCommand);
                if (contents.Count == 1 && contents[0] != "")
                {
                    return _macroCommandBuilder.WithArg(contents[0]).Build();
                }
            }
            return null;
        }
    }


    public class MacroCommandsExecutor : AbstractMacroCommandsExecutor
    {
        List<AbstractMacroCommandParser> _macroCommandParsers;

        public MacroCommandsExecutor(
            List<AbstractMacroCommandParser> macroCommandParsers
        )
        {
            _macroCommandParsers = macroCommandParsers;
        }

        public override void Execute(List<string> macroCommands)
        {
            for (int i = 0; i < macroCommands.Count; i++)
            for (int j = 0; j < _macroCommandParsers.Count; j++)
            {
                var parsedMacroCommand = _macroCommandParsers[j].Parse(macroCommands[i]);
                if (parsedMacroCommand != null)
                {
                    parsedMacroCommand.Run();
                    break;
                }
            }
        }
    }


    public class MacroCommandsExecutorBuilder : AbstractMacroCommandsExecutorBuilder
    {
        private AbstractKeystrokeTransmitter? _keystrokeTransmitter;

        public override AbstractMacroCommandsExecutor Build()
        {
            return new MacroCommandsExecutor(
                [
                    new WaitMacroCommandParser(
                        new MacroRandom(),
                        new BracketContentsParser(),
                        new WaitMacroCommandBuilder(new MacroSleeper())
                    ),
                    new KeyPressMacroCommandParser(
                        new MacroRandom(),
                        new BracketContentsParser(),
                        new KeyPressMacroCommandBuilder(new MacroSleeper(), _keystrokeTransmitter!)
                    ),
                    new KeyDownMacroCommandParser(
                        new BracketContentsParser(),
                        new KeyDownMacroCommandBuilder(_keystrokeTransmitter!)
                    ),
                    new KeyUpMacroCommandParser(
                        new BracketContentsParser(),
                        new KeyUpMacroCommandBuilder(_keystrokeTransmitter!)
                    )
                ]
            );
        }

        public override AbstractMacroCommandsExecutorBuilder WithArg(object arg)
        {
            if (arg is AbstractKeystrokeTransmitter keystrokeTransmitter)
            {
                _keystrokeTransmitter = keystrokeTransmitter;
            }
            return this;
        }
    }
}
