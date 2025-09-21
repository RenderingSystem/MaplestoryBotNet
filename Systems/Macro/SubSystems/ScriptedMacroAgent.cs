
using MaplestoryBotNet.ThreadingUtils;


namespace MaplestoryBotNet.Systems.Macro.SubSystems
{
    public class ScriptedMacroAgent : AbstractMacroAgent
    {
        private AbstractMacroTranslator _macroTranslator;
        private List<AbstractMacroAction> _macroActions;
        private ReaderWriterLockSlim _macroActionsLock;
        private bool _pauseRunning;
        private ReaderWriterLockSlim _pauseRunningLock;

        public ScriptedMacroAgent(AbstractMacroTranslator macroTranslator)
        {
            _macroTranslator = macroTranslator;
            _macroActions = [];
            _macroActionsLock = new ReaderWriterLockSlim();
            _pauseRunning = false;
            _pauseRunningLock = new ReaderWriterLockSlim();
        }

        public  List<AbstractMacroAction> MacroActions
        {
            get
            {
                _macroActionsLock.EnterReadLock();
                try
                {
                    return [.. _macroActions];
                }
                finally
                {
                    _macroActionsLock.ExitReadLock();
                }
            }
            private set
            {
                _macroActionsLock.EnterWriteLock();
                try
                {
                    _macroActions = value;
                }
                finally
                {
                    _macroActionsLock.ExitWriteLock();
                }
            }
        }

        public bool PauseRunning
        {
            get
            {
                _pauseRunningLock.EnterReadLock();
                try
                {
                    return _pauseRunning;
                }
                finally
                {
                    _pauseRunningLock.ExitReadLock();
                }
            }
            private set
            {
                _pauseRunningLock.EnterWriteLock();
                try
                {
                    _pauseRunning = value;
                }
                finally
                {
                    _pauseRunningLock.ExitWriteLock();
                }
            }
        }

        public override void Execute()
        {
            if (!PauseRunning)
            {
                var actions = MacroActions;
                foreach (var action in actions)
                {
                    if (PauseRunning)
                    {
                        break;
                    }
                    action.Execute();
                }
            }
        }

        public override void Update(object data)
        {
            if (data is string macroString)
            {
                MacroActions = _macroTranslator.Translate(macroString);
            }
            if (data is bool pauseState)
            {
                PauseRunning = pauseState;
            }
        }
    }
}
