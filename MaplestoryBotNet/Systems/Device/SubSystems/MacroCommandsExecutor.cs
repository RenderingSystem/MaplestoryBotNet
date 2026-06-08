using Interception;
using MaplestoryBotNet.LibraryWrappers;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.ThreadingUtils;
using System.Globalization;


namespace MaplestoryBotNet.Systems.Device.SubSystems
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
        public abstract void InjectKeyboardDevice(DeviceContext keyboardDevice);

        public abstract void Keydown(string keystroke);

        public abstract void Keyup(string keystroke);
    }


    public abstract class AbstractKeystrokeTransmitterBuilder
    {
        public abstract AbstractKeystrokeTransmitterBuilder WithArg(object arg);

        public abstract AbstractKeystrokeTransmitter Build();
    }


    public enum MouseButton
    {
        Left = 0,
        Middle,
        Right,
        MaxNum
    }


    public abstract class AbstractMouseTransmitter
    {
        public abstract void InjectMouseDevice(DeviceContext mouseDevice);

        public abstract void MouseMove(int x, int y);

        public abstract void MouseDown(MouseButton button);

        public abstract void MouseUp(MouseButton button);
    }


    public abstract class AbstractMouseTransmitterBuilder
    {
        public abstract AbstractMouseTransmitterBuilder WithArg(object arg);

        public abstract AbstractMouseTransmitter Build();
    }


    public abstract class AbstractKeystrokeConverter
    {
        public abstract InterceptionInterop.KeyStroke ConvertToKeydown(string stroke);

        public abstract InterceptionInterop.KeyStroke ConvertToKeyup(string stroke);

    }


    public enum KeystrokeTransmitterThreadType
    {
        Runeing = 0,
        Botting,
        Solving,
        Macro,
        Login,
        Ailment,
        MaxNum
    }


    public abstract class AbstractKeystrokeTransmitterThreadState
    {
        public abstract int GetState();

        public abstract void SetState(int state);

        public abstract KeystrokeTransmitterThreadType Type();
    }


    public abstract class AbstractKeystrokeTransmitterThreadHelper : IDataInjectable
    {
        public abstract bool Transmit();

        public abstract void Reset();

        public abstract void Inject(object dataType, object? data);
    }


    public class MacroSleeper : AbstractMacroSleeper
    {
        public override void Sleep(int milliseconds)
        {
            if (milliseconds > 0)
            {
                Thread.Sleep(milliseconds);
            }
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


    public class KeystrokeTransmitterThreadState : AbstractKeystrokeTransmitterThreadState
    {
        private volatile int _threadState;

        private KeystrokeTransmitterThreadType _threadType;

        public KeystrokeTransmitterThreadState(
            int threadState, KeystrokeTransmitterThreadType threadType
        )
        {
            _threadState = threadState;
            _threadType = threadType;
        }

        public override int GetState()
        {
            return _threadState;
        }

        public override void SetState(int state)
        {
            _threadState = state;
        }

        public override KeystrokeTransmitterThreadType Type()
        {
            return _threadType;
        }
    }


    public class KeystrokeTransmitter : AbstractKeystrokeTransmitter
    {
        private AbstractInterceptionLibrary _interceptionLibrary;

        private KeyboardMapping _keyboardMapping;

        private AbstractKeystrokeConverter _keystrokeConverter;

        private LockObject _sendLock;

        private volatile DeviceContext? _keyboardDevice;

        public KeystrokeTransmitter(
            AbstractInterceptionLibrary interceptionLibrary,
            AbstractKeystrokeConverter keystrokeConverter,
            KeyboardMapping KeyboardMapping,
            LockObject sendLock
        )
        {
            _interceptionLibrary = interceptionLibrary;
            _keyboardMapping = KeyboardMapping;
            _keystrokeConverter = keystrokeConverter;
            _keyboardDevice = null;
            _sendLock = sendLock;
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

        public override void InjectKeyboardDevice(DeviceContext keyboardDevice)
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
        private KeyboardMapping? _keyboardMapping = null;

        private LockObject? _sendLock = null;

        public override AbstractKeystrokeTransmitterBuilder WithArg(object arg)
        {
            if (arg is KeyboardMapping keyboardMapping)
            {
                _keyboardMapping = (KeyboardMapping)keyboardMapping.Copy();
            }
            else if (arg is LockObject sendLock)
            {
                _sendLock = sendLock;
            }
            return this;
        }

        public override AbstractKeystrokeTransmitter Build()
        {
            return new KeystrokeTransmitter(
                new InterceptionLibrary(),
                new KeystrokeConverter(),
                _keyboardMapping ?? new KeyboardMapping(),
                _sendLock ?? new LockObject()
            );
        }
    }


    public class MouseTransmitter : AbstractMouseTransmitter
    {
        private AbstractInterceptionLibrary _interceptionLibrary;

        private LockObject _sendLock;

        private volatile DeviceContext? _mouseDevice;

        public MouseTransmitter(
            AbstractInterceptionLibrary interceptionLibrary,
            LockObject sendLock
        )
        {
            _interceptionLibrary = interceptionLibrary;
            _sendLock = sendLock;
        }

        private void _sendMouseStroke(InterceptionInterop.MouseStroke mousestroke)
        {
            var mouseDevice = _mouseDevice;
            if (mouseDevice == null)
            {
                return;
            }
            var context = mouseDevice.Context;
            var device = mouseDevice.Device;
            unsafe
            {
                var stroke = (InterceptionInterop.Stroke*)&mousestroke;
                lock (_sendLock)
                {
                    _interceptionLibrary.Send(context, device, stroke, 1);
                }
            }
        }

        public override void InjectMouseDevice(DeviceContext mouseDevice)
        {
            _mouseDevice = mouseDevice;
        }

        public override void MouseMove(int x, int y)
        {
            _sendMouseStroke(
                new InterceptionInterop.MouseStroke
                {
                    State = 0,
                    Flags = InterceptionInterop.MouseFlag.MoveAbsolute,
                    Rolling = 0,
                    X = x,
                    Y = y,
                    Information = 0
                }
            );
        }

        public override void MouseDown(MouseButton button)
        {
            if (button < MouseButton.MaxNum)
            {
                var left = InterceptionInterop.MouseState.LeftButtonDown;
                var middle = InterceptionInterop.MouseState.MiddleButtonDown;
                var right = InterceptionInterop.MouseState.RightButtonDown;
                var mouseStroke = new InterceptionInterop.MouseStroke
                {
                    State = (
                        button == MouseButton.Left ? left :
                        button == MouseButton.Middle ? middle :
                        button == MouseButton.Right ? right : right
                    ),
                    Flags = 0,
                    Rolling = 0,
                    X = 0,
                    Y = 0,
                    Information = 0
                };
                _sendMouseStroke(mouseStroke);
            }
        }

        public override void MouseUp(MouseButton button)
        {
            if (button < MouseButton.MaxNum)
            {
                var left = InterceptionInterop.MouseState.LeftButtonUp;
                var middle = InterceptionInterop.MouseState.MiddleButtonUp;
                var right = InterceptionInterop.MouseState.RightButtonUp;
                var mouseStroke = new InterceptionInterop.MouseStroke
                {
                    State = (
                        button == MouseButton.Left ? left :
                        button == MouseButton.Middle ? middle :
                        button == MouseButton.Right ? right : right
                    ),
                    Flags = 0,
                    Rolling = 0,
                    X = 0,
                    Y = 0,
                    Information = 0
                };
                _sendMouseStroke(mouseStroke);
            }
        }
    }


    public class MouseTransmitterBuilder : AbstractMouseTransmitterBuilder
    {
        private LockObject? _sendLock = null;

        public override AbstractMouseTransmitter Build()
        {   
            return new MouseTransmitter(new InterceptionLibrary(), _sendLock!);
        }

        public override AbstractMouseTransmitterBuilder WithArg(object arg)
        {
            if (arg is LockObject sendLock)
            {
                _sendLock = sendLock;
            }
            return this;
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


    public class MousePressMacroCommand : AbstractParsedMacroCommand
    {
        private MouseButton _mouseButton;

        private int _waitMilliseconds;

        private AbstractMouseTransmitter _mouseTransmitter;

        private AbstractMacroSleeper _macroSleeper;

        public MousePressMacroCommand(
            MouseButton mouseButton,
            int waitMilliseconds,
            AbstractMouseTransmitter mouseTransmitter,
            AbstractMacroSleeper macroSleeper
        )
        {
            _mouseButton = mouseButton;
            _waitMilliseconds = waitMilliseconds;
            _mouseTransmitter = mouseTransmitter;
            _macroSleeper = macroSleeper;
        }

        public override void Run()
        {
            _mouseTransmitter.MouseDown(_mouseButton);
            _macroSleeper.Sleep(_waitMilliseconds);
            _mouseTransmitter.MouseUp(_mouseButton);
        }
    }


    public class MousePressMacroCommandBuilder : AbstractParsedMacroCommandBuilder
    {
        private MouseButton _mouseButton;

        private int _waitMilliseconds;

        private AbstractMacroSleeper _macroSleeper;

        private AbstractMouseTransmitter _mouseTransmitter;

        public MousePressMacroCommandBuilder(
            AbstractMacroSleeper macroSleeper,
            AbstractMouseTransmitter mouseTransmitter
        )
        {
            _mouseButton = MouseButton.MaxNum;
            _waitMilliseconds = 0;
            _macroSleeper = macroSleeper;
            _mouseTransmitter = mouseTransmitter;
        }

        public override AbstractParsedMacroCommand Build()
        {
            return new MousePressMacroCommand(
                _mouseButton, _waitMilliseconds, _mouseTransmitter, _macroSleeper
            );
        }

        public override AbstractParsedMacroCommandBuilder WithArg(object args)
        {
            if (args is MouseButton mouseButton)
            {
                _mouseButton = mouseButton;
            }
            else if (args is int waitMilliseconds)
            {
                _waitMilliseconds = waitMilliseconds;
            }
            return this;
        }
    }


    public class MouseMoveMacroCommand : AbstractParsedMacroCommand
    {
        private int X;

        private int Y;

        private AbstractMouseTransmitter _mouseTransmitter;

        public MouseMoveMacroCommand(
            int x,
            int y,
            AbstractMouseTransmitter mouseTransmitter
        )
        {
            X = x;
            Y = y;
            _mouseTransmitter = mouseTransmitter;
        }

        public override void Run()
        {
            _mouseTransmitter.MouseMove(X, Y);
        }
    }


    public class MouseMoveMacroCommandBuilder : AbstractParsedMacroCommandBuilder
    {
        private int _x;

        private int _y;

        private AbstractMouseTransmitter _mouseTransmitter;

        public MouseMoveMacroCommandBuilder(
            AbstractMouseTransmitter mouseTransmitter
        )
        {
            _mouseTransmitter = mouseTransmitter;
        }

        public override AbstractParsedMacroCommand Build()
        {
            return new MouseMoveMacroCommand(_x, _y, _mouseTransmitter);
        }

        public override AbstractParsedMacroCommandBuilder WithArg(object args)
        {
            if (args is Tuple<int, int> point)
            {
                _x = point.Item1;
                _y = point.Item2;
            }
            return this;
        }
    }


    public class MouseDownMacroCommand : AbstractParsedMacroCommand
    {
        private MouseButton _mouseButton;

        private AbstractMouseTransmitter _mouseTransmitter;

        public MouseDownMacroCommand(
            MouseButton mouseButton,
            AbstractMouseTransmitter mouseTransmitter
        )
        {
            _mouseButton = mouseButton;
            _mouseTransmitter = mouseTransmitter;
        }

        public override void Run()
        {
            _mouseTransmitter.MouseDown(_mouseButton);
        }
    }


    public class MouseDownMacroCommandBuilder : AbstractParsedMacroCommandBuilder
    {
        private MouseButton _mouseButton;

        private AbstractMouseTransmitter _mouseTransmitter;

        public MouseDownMacroCommandBuilder(
            AbstractMouseTransmitter mouseTransmitter
        )
        {
            _mouseButton = MouseButton.MaxNum;
            _mouseTransmitter = mouseTransmitter;
        }

        public override AbstractParsedMacroCommand Build()
        {
            return new MouseDownMacroCommand(_mouseButton, _mouseTransmitter);
        }

        public override AbstractParsedMacroCommandBuilder WithArg(object args)
        {
            if (args is MouseButton mouseButton)
            {
                _mouseButton = mouseButton;
            }
            return this;
        }
    }


    public class MouseUpMacroCommand : AbstractParsedMacroCommand
    {
        private MouseButton _mouseButton;

        private AbstractMouseTransmitter _mouseTransmitter;

        public MouseUpMacroCommand(
            MouseButton mouseButton,
            AbstractMouseTransmitter mouseTransmitter
        )
        {
            _mouseButton = mouseButton;
            _mouseTransmitter = mouseTransmitter;
        }

        public override void Run()
        {
            _mouseTransmitter.MouseUp(_mouseButton);
        }
    }


    public class MouseUpMacroCommandBuilder : AbstractParsedMacroCommandBuilder
    {
        private MouseButton _mouseButton;

        private AbstractMouseTransmitter _mouseTransmitter;

        public MouseUpMacroCommandBuilder(
            AbstractMouseTransmitter mouseTransmitter
        )
        {
            _mouseButton = MouseButton.MaxNum;
            _mouseTransmitter = mouseTransmitter;
        }

        public override AbstractParsedMacroCommand Build()
        {
            return new MouseUpMacroCommand(_mouseButton, _mouseTransmitter);
        }

        public override AbstractParsedMacroCommandBuilder WithArg(object args)
        {
            if (args is MouseButton mouseButton)
            {
                _mouseButton = mouseButton;
            }
            return this;
        }
    }


    public class MousePressMacroCommandParser : AbstractMacroCommandParser
    {
        private AbstractMacroRandom _macroRandom;

        private AbstractBracketContentsParser _bracketContentsParser;

        private AbstractParsedMacroCommandBuilder _macroCommandBuilder;
        
        public MousePressMacroCommandParser(
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
            if (macroCommand.ToLower().StartsWith("mouse press"))
            {
                var contents = _bracketContentsParser.Parse(macroCommand);
                if (
                    contents.Count == 3
                    && contents.All((content) => { return content != ""; })
                    && int.TryParse(contents[1], out int interval1)
                    && int.TryParse(contents[2], out int interval2)
                )
                {
                    MouseButton? mouseButton = (
                        contents[0] == "left" ? MouseButton.Left :
                        contents[0] == "middle" ? MouseButton.Middle :
                        contents[0] == "right" ? MouseButton.Right :
                        null
                    );
                    if (mouseButton != null)
                    {
                        var minInterval = Math.Min(interval1, interval2);
                        var maxInterval = Math.Max(interval1, interval2);
                        var milliseconds = Math.Max(0, _macroRandom.Next(minInterval, maxInterval));
                        return _macroCommandBuilder
                            .WithArg(mouseButton)
                            .WithArg(milliseconds)
                            .Build();
                    }
                }
            }
            return null;
        }
    }


    public class MouseMoveMacroCommandParser : AbstractMacroCommandParser
    {
        private AbstractBracketContentsParser _bracketContentsParser;

        private AbstractParsedMacroCommandBuilder _macroCommandBuilder;

        public MouseMoveMacroCommandParser(
            AbstractBracketContentsParser bracketContentsParser,
            AbstractParsedMacroCommandBuilder macroCommandBuilder
        )
        {
            _bracketContentsParser = bracketContentsParser;
            _macroCommandBuilder = macroCommandBuilder;
        }

        public override AbstractParsedMacroCommand? Parse(string macroCommand)
        {
            if (macroCommand.ToLower().StartsWith("mouse move"))
            {
                var contents = _bracketContentsParser.Parse(macroCommand);
                if (
                    contents.Count == 2
                    && int.TryParse(contents[0], out int x)
                    && int.TryParse(contents[1], out int y)
                )
                {
                    return _macroCommandBuilder.WithArg(new Tuple<int, int>(x, y)).Build();
                }
            }
            return null;
        }
    }


    public class MouseDownMacroCommandParser : AbstractMacroCommandParser
    {
        private AbstractBracketContentsParser _bracketContentsParser;

        private AbstractParsedMacroCommandBuilder _macroCommandBuilder;

        public MouseDownMacroCommandParser(
            AbstractBracketContentsParser bracketContentsParser,
            AbstractParsedMacroCommandBuilder macroCommandBuilder
        )
        {
            _bracketContentsParser = bracketContentsParser;
            _macroCommandBuilder = macroCommandBuilder;
        }

        public override AbstractParsedMacroCommand? Parse(string macroCommand)
        {
            if (macroCommand.ToLower().StartsWith("mouse down"))
            {
                var contents = _bracketContentsParser.Parse(macroCommand);
                if (contents.Count == 1)
                {
                    MouseButton? mouseButton = (
                        contents[0] == "left" ? MouseButton.Left :
                        contents[0] == "middle" ? MouseButton.Middle :
                        contents[0] == "right" ? MouseButton.Right :
                        null
                    );
                    if (mouseButton != null)
                    {
                        return _macroCommandBuilder.WithArg(mouseButton).Build();
                    }
                }
            }
            return null;
        }
    }


    public class MouseUpMacroCommandParser : AbstractMacroCommandParser
    {
        private AbstractBracketContentsParser _bracketContentsParser;

        private AbstractParsedMacroCommandBuilder _macroCommandBuilder;

        public MouseUpMacroCommandParser(
            AbstractBracketContentsParser bracketContentsParser,
            AbstractParsedMacroCommandBuilder macroCommandBuilder
        )
        {
            _bracketContentsParser = bracketContentsParser;
            _macroCommandBuilder = macroCommandBuilder;
        }

        public override AbstractParsedMacroCommand? Parse(string macroCommand)
        {
            if (macroCommand.ToLower().StartsWith("mouse up"))
            {
                var contents = _bracketContentsParser.Parse(macroCommand);
                if (contents.Count == 1)
                {
                    MouseButton? mouseButton = (
                        contents[0] == "left" ? MouseButton.Left :
                        contents[0] == "middle" ? MouseButton.Middle :
                        contents[0] == "right" ? MouseButton.Right :
                        null
                    );
                    if (mouseButton != null)
                    {
                        return _macroCommandBuilder.WithArg(mouseButton).Build();
                    }
                }
            }
            return null;
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

        private AbstractMouseTransmitter? _mouseTransmitter;

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
                    ),
                    new MousePressMacroCommandParser(
                        new MacroRandom(),
                        new BracketContentsParser(),
                        new MousePressMacroCommandBuilder(new MacroSleeper(), _mouseTransmitter!)
                    ),
                    new MouseMoveMacroCommandParser(
                        new BracketContentsParser(),
                        new MouseMoveMacroCommandBuilder(_mouseTransmitter!)
                    ),
                    new MouseDownMacroCommandParser(
                        new BracketContentsParser(),
                        new MouseDownMacroCommandBuilder(_mouseTransmitter!)
                    ),
                    new MouseUpMacroCommandParser(
                        new BracketContentsParser(),
                        new MouseUpMacroCommandBuilder(_mouseTransmitter!)
                    )
                ]
            );
        }

        public override AbstractMacroCommandsExecutorBuilder WithArg(object arg)
        {
            if (arg is TransmitterInfo transmitterInfo)
            {
                _keystrokeTransmitter = transmitterInfo.KeystrokeTransmitter;
                _mouseTransmitter = transmitterInfo.MouseTransmitter;
            }
            return this;
        }
    }
}
