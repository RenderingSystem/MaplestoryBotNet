using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Configuration;


namespace MaplestoryBotNet.Systems.Macro.SubSystems
{
    public abstract class AbstractMacroSleeper
    {
        public abstract void Sleep(int milliseconds);
    }


    public abstract class AbstractMacroRandom
    {
        public abstract int Next(int minValue, int maxValue);
    }


    public abstract class AbstractMacroAction
    {
        public abstract void Execute();
    }


    public abstract class AbstractMacroTranslator : ISystemInjectable
    {
        public abstract List<AbstractMacroAction> Translate(string macroText);

        public virtual void Inject(SystemInjectType dataType, object? data)
        {
        }
    }


    public abstract class AbstractMacroTranslatorFactory
    {
        public abstract AbstractMacroTranslator Create();
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
            var random = new Random();
            return random.Next(minValue, maxValue + 1);
        }
    }


    public class DelayMacroAction : AbstractMacroAction
    {
        private AbstractMacroSleeper _macroSleeper;

        public int Delay { private set; get; }

        public DelayMacroAction(
            AbstractMacroSleeper macroSleeper, int delay
        )
        {
            _macroSleeper = macroSleeper;
            Delay = delay;
        }

        public override void Execute()
        {
            _macroSleeper.Sleep(Delay);
        }
    }


    public class KeyDownMacroAction : AbstractMacroAction
    {
        private AbstractKeystrokeTransmitter _keystrokeTransmitter;

        public string Keystroke { private set; get; }

        public KeyDownMacroAction(
            AbstractKeystrokeTransmitter keystrokeTransmitter,
            string keystroke
        )
        {
            _keystrokeTransmitter = keystrokeTransmitter;
            Keystroke = keystroke;
        }

        public override void Execute()
        {
            _keystrokeTransmitter.Keydown(Keystroke);
        }
    }


    public class KeyUpMacroAction : AbstractMacroAction
    {
        private AbstractKeystrokeTransmitter _keystrokeTransmitter;

        public string Keystroke { private set; get; }

        public KeyUpMacroAction(
            AbstractKeystrokeTransmitter keystrokeTransmitter,
            string keystroke
        )
        {
            _keystrokeTransmitter = keystrokeTransmitter;
            Keystroke = keystroke;
        }
        public override void Execute()
        {
            _keystrokeTransmitter.Keyup(Keystroke);
        }
    }


    public class KeyPressMacroAction : AbstractMacroAction
    {
        private AbstractKeystrokeTransmitter _keystrokeTransmitter;

        private AbstractMacroSleeper _macroSleeper;

        public int Delay { private set; get; }

        public string Keystroke { private set; get; }

        public KeyPressMacroAction(
            AbstractKeystrokeTransmitter keystrokeTransmitter,
            AbstractMacroSleeper macroSleeper,
            int delay,
            string keystroke
        )
        {
            _keystrokeTransmitter = keystrokeTransmitter;
            _macroSleeper = macroSleeper;
            Delay = delay;
            Keystroke = keystroke;
        }
        public override void Execute()
        {
            _keystrokeTransmitter.Keydown(Keystroke);
            _macroSleeper.Sleep(Delay);
            _keystrokeTransmitter.Keyup(Keystroke);
        }
    }


    public class DelayMacroTranslator : AbstractMacroTranslator
    {
        private AbstractMacroSleeper _macroSleeper;

        private AbstractMacroRandom _macroRandom;

        public DelayMacroTranslator(
            AbstractMacroSleeper macroSleeper,
            AbstractMacroRandom macroRandom
        )
        {
            _macroSleeper = macroSleeper;
            _macroRandom = macroRandom;
        }

        public override List<AbstractMacroAction> Translate(string macroText)
        {
            try
            {
                var macroTextSplit = macroText.ToLower().Split(
                    "*", StringSplitOptions.RemoveEmptyEntries
                );
                var macroCommand = macroTextSplit[0];
                var macroParametersSplit = macroTextSplit[1].Split(
                    " ", StringSplitOptions.RemoveEmptyEntries
                );
                var macroParametersSplitCount = macroParametersSplit.Count();
                if (macroCommand == "delay")
                {
                    var delayMin = int.Parse(macroParametersSplit[0].Trim());
                    var delayTime = delayMin;
                    if (macroParametersSplit.Count() > 1)
                    {
                        var delayMax = int.Parse(macroParametersSplit[1].Trim());
                        delayTime = _macroRandom.Next(delayTime, delayMax);
                    }
                    return [new DelayMacroAction(_macroSleeper, delayTime)];
                }
                return [];
            }
            catch
            {
                return [];
            }
        }
    }


    public class KeydownMacroTranslator : AbstractMacroTranslator
    {
        private AbstractKeystrokeTransmitter? _keystrokeTransmitter;

        private KeyboardMapping? _keyboardMapping;

        public override List<AbstractMacroAction> Translate(string macroText)
        {
            try
            {
                var macroTextSplit = macroText.ToLower().Split(
                    "*", StringSplitOptions.RemoveEmptyEntries
                );
                var macroCommand = macroTextSplit[0];
                var macroParametersSplit = macroTextSplit[1].Split(
                    " ", StringSplitOptions.RemoveEmptyEntries
                );
                var macroKey = macroParametersSplit[0].Trim();
                if (
                    macroCommand == "keydown"
                    && _keystrokeTransmitter != null
                    && _keyboardMapping != null
                    && _keyboardMapping.GetMapping(macroKey).Length > 0
                )
                {
                    return [new KeyDownMacroAction(_keystrokeTransmitter, macroKey)];
                }
                return [];
            }
            catch
            {
                return [];
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (data is AbstractKeystrokeTransmitter keystrokeTransmitter)
            {
                _keystrokeTransmitter = keystrokeTransmitter;
            }
            if (data is KeyboardMapping keyboardMapping)
            {
                _keyboardMapping = keyboardMapping;
            }
        }
    }


    public class KeyupMacroTranslator : AbstractMacroTranslator
    {
        private AbstractKeystrokeTransmitter? _keystrokeTransmitter;

        private KeyboardMapping? _keyboardMapping;

        public override List<AbstractMacroAction> Translate(string macroText)
        {
            try
            {
                var macroTextSplit = macroText.ToLower().Split(
                    "*", StringSplitOptions.RemoveEmptyEntries
                );
                var macroParametersSplit = macroTextSplit[1].Split(
                    " ", StringSplitOptions.RemoveEmptyEntries
                );
                var macroKey = macroParametersSplit[0].Trim();
                if (
                    macroTextSplit[0] == "keyup"
                    && _keystrokeTransmitter != null
                    && _keyboardMapping != null
                    && _keyboardMapping.GetMapping(macroKey).Length > 0
                )
                {
                    return [new KeyUpMacroAction(_keystrokeTransmitter, macroKey)];
                }
                return [];
            }
            catch
            {
                return [];
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (data is AbstractKeystrokeTransmitter keystrokeTransmitter)
            {
                _keystrokeTransmitter = keystrokeTransmitter;
            }
            if (data is KeyboardMapping keyboardMapping)
            {
                _keyboardMapping = keyboardMapping;
            }
        }
    }


    public class KeypressMacroTranslator : AbstractMacroTranslator, ISystemInjectable 
    {
        private AbstractKeystrokeTransmitter? _keystrokeTransmitter;

        private KeyboardMapping? _keyboardMapping;

        private AbstractMacroSleeper _macroSleeper;

        private AbstractMacroRandom _macroRandom;

        public KeypressMacroTranslator(
            AbstractMacroSleeper macroSleeper,
            AbstractMacroRandom macroRandom)
        {
            _macroSleeper = macroSleeper;
            _macroRandom = macroRandom;
            _keystrokeTransmitter = null;
            _keyboardMapping = null;
        }

        public override List<AbstractMacroAction> Translate(string macroText)
        {
            try
            {
                var macroTextSplit = macroText.ToLower().Split(
                    "*", StringSplitOptions.RemoveEmptyEntries
                );
                var macroParametersSplit = macroTextSplit[1].Split(
                    " ", StringSplitOptions.RemoveEmptyEntries
                );
                if (
                    macroTextSplit[0] == "keypress"
                    && _keystrokeTransmitter != null
                    && _keyboardMapping != null
                )
                {
                    var delayMin = int.Parse(macroParametersSplit[0]);
                    var next = int.TryParse(macroParametersSplit[1], out int delayMax) ? 2 : 1;
                    var count = (macroParametersSplit.Length - next);
                    var index = (count > 1) ? _macroRandom.Next(next, macroParametersSplit.Length) : next;
                    var delay = (next == 2) ? _macroRandom.Next(delayMin, delayMax) : delayMin;
                    var keystroke = macroParametersSplit[index];
                    for (int i = next; i < macroParametersSplit.Length; i++)
                    {
                        var macroKey = macroParametersSplit[i];
                        if (_keyboardMapping.GetMapping(macroKey).Length == 0)
                            return [];
                    }
                    return [
                        new KeyPressMacroAction(
                            _keystrokeTransmitter, _macroSleeper, delay, keystroke
                        )
                    ];
                }
                return [];
            }
            catch
            {
                return [];
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (data is AbstractKeystrokeTransmitter keystrokeTransmitter)
            {
                _keystrokeTransmitter = keystrokeTransmitter;
            }
            if (data is KeyboardMapping keyboardMapping)
            {
                _keyboardMapping = keyboardMapping;
            }
        }
    }


    public class MacroTranslator : AbstractMacroTranslator
    {
        private List<AbstractMacroTranslator> _translators;

        private KeyboardMapping? _keyboardMapping;

        private AbstractKeystrokeTransmitter? _keystrokeTransmitter;

        public MacroTranslator(List<AbstractMacroTranslator> translators)
        {
            _translators = translators;
            _keyboardMapping = null;
            _keystrokeTransmitter = null;
        }

        public override List<AbstractMacroAction> Translate(string macroText)
        {
            if (_keyboardMapping == null || _keystrokeTransmitter == null)
                return [];
            var translated = new List<AbstractMacroAction>();
            try
            {
                var macroLines = macroText.Split(
                    [Environment.NewLine, "\n", "\r\n"],
                    StringSplitOptions.RemoveEmptyEntries
                );
                foreach (var macroLine in macroLines)
                {
                    foreach (var translator in _translators)
                    {
                        var actions = translator.Translate(macroLine);
                        if (actions.Count > 0)
                        {
                            translated.AddRange(actions);
                            break;
                        }
                    }
                }
            }
            catch
            {
                return [];
            }
            return translated;
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (data is AbstractKeystrokeTransmitter keystrokeTransmitter)
            {
                _keystrokeTransmitter = keystrokeTransmitter;
            }
            if (data is KeyboardMapping keyboardMapping)
            {
                _keyboardMapping = keyboardMapping;
            }
            if (data is AbstractKeystrokeTransmitter || data is KeyboardMapping)
            {
                for (int i = 0; i < _translators.Count; i++)
                {
                    _translators[i].Inject(dataType, data);
                }
            }
        }
    }


    public class MacroTranslatorFactory : AbstractMacroTranslatorFactory
    {
        public override AbstractMacroTranslator Create()
        {
            return new MacroTranslator(
                [
                    new DelayMacroTranslator(new MacroSleeper(), new MacroRandom()),
                    new KeydownMacroTranslator(),
                    new KeyupMacroTranslator(),
                    new KeypressMacroTranslator(new MacroSleeper(), new MacroRandom())
                ]
            );
        }
    }
}
