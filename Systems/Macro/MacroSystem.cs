using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using System.Collections.Concurrent;


namespace MaplestoryBotNet.Systems.Macro
{
    public enum MacroOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }


    public enum MacroExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        MaxNum
    }


    public enum MacroExecutorStateTypes
    {
        Reset = 0,
        Idle,
        Botting,
        Runeing,
        Solving,
        SolvedCheck,
        MaxNum
    }


    public abstract class AbstractOrchestratorController
    {
        public abstract void StartOrchestrator();

        public abstract void StopOrchestrator();

        public abstract void SetOrchestrator(AbstractThread orchestrator);

        public abstract void SetOrchestratorThreadState(
            AbstractKeystrokeTransmitterThreadState threadState
        );

        public abstract int? GetState();
    }


    public abstract class AbstractExecutorState
    {
        public abstract int Execute();
    }


    public class OrchestratorController<InjectType, UpdateType> :
        AbstractOrchestratorController
        where InjectType : Enum
        where UpdateType : Enum
    {
        private InjectType _startValue;
        private UpdateType _startValueCheck;
        private InjectType _stopValue;
        private UpdateType _stopValueCheck;
        private AbstractThread? _orchestrator;
        private AbstractKeystrokeTransmitterThreadState? _threadState;

        public OrchestratorController(
            InjectType startValue,
            UpdateType startValueCheck,
            InjectType stopValue,
            UpdateType stopValueCheck
        )
        {
            _startValue = startValue;
            _startValueCheck = startValueCheck;
            _stopValue = stopValue;
            _stopValueCheck = stopValueCheck;
            _orchestrator = null;
            _threadState = null;
        }

        public override void StartOrchestrator()
        {
            if (_orchestrator != null && _threadState != null)
            {
                _orchestrator.Inject(_startValue, 0);
                while (_threadState.GetState() != Convert.ToInt32(_startValueCheck))
                {
                    Thread.Yield();
                }
            }
        }

        public override void StopOrchestrator()
        {
            if (_orchestrator != null && _threadState != null)
            {
                _orchestrator.Inject(_stopValue, 0);
                while (_threadState.GetState() != Convert.ToInt32(_stopValueCheck))
                {
                    Thread.Yield();
                }
            }
        }

        public override void SetOrchestrator(AbstractThread orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public override void SetOrchestratorThreadState(
            AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _threadState = threadState;
        }

        public override int? GetState()
        {
            return _threadState?.GetState();
        }
    }


    public class MacroExecutorStateReset : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        public MacroExecutorStateReset(MacroExecutorThreadContext context)
        {
            _context = context;
        }

        public override int Execute()
        {
            var botting = _context.BottingController.GetState();
            var runeing = _context.RuneingController.GetState();
            var solving = _context.SolvingController.GetState();
            if (botting != (int)BottingExecutorThreadedUpdate.Stopped)
            {
                _context.BottingController.StopOrchestrator();
            }
            if (runeing != (int)RuneingExecutorThreadedUpdate.Stopped)
            {
                _context.RuneingController.StopOrchestrator();
            }
            if (solving != (int)SolvingExecutorThreadedUpdate.Stopped)
            {
                _context.SolvingController.StopOrchestrator();
            }
            if (_context.BottingModel is AbstractBottingModel bottingModel)
            {
                var runeModel = bottingModel.GetRuneModel();
                var cooldown = runeModel.GetCooldown();
                _context.RuneActivationPeriodCurrent = cooldown;
            }
            return (int)MacroExecutorStateTypes.Idle;
        }
    }


    public class MacroExecutorStateIdle : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        public MacroExecutorStateIdle(MacroExecutorThreadContext context)
        {
            _context = context;
        }

        public override int Execute()
        {
            var botting = _context.BottingController.GetState();
            var runeing = _context.RuneingController.GetState();
            var solving = _context.SolvingController.GetState();
            if (botting != (int)BottingExecutorThreadedUpdate.Stopped)
            {
                _context.BottingController.StopOrchestrator();
            }
            if (runeing != (int)RuneingExecutorThreadedUpdate.Stopped)
            {
                _context.RuneingController.StopOrchestrator();
            }
            if (solving != (int)SolvingExecutorThreadedUpdate.Stopped)
            {
                _context.SolvingController.StopOrchestrator();
            }
            _context.RuneingStopwatch.SetTimestamp();
            return (int)MacroExecutorStateTypes.Botting;
        }
    }


    public class MacroExecutorStateBotting : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        public MacroExecutorStateBotting(MacroExecutorThreadContext context)
        {
            _context = context;
        }

        public override int Execute()
        {
            var botting = _context.BottingController.GetState();
            var runeing = _context.RuneingController.GetState();
            var solving = _context.SolvingController.GetState();
            var timestamp = _context.RuneingStopwatch.GetTimestamp();
            var activation = _context.RuneActivationPeriodCurrent;
            if (solving != (int)SolvingExecutorThreadedUpdate.Stopped)
            {
                _context.SolvingController.StopOrchestrator();
            }
            if (runeing != (int)RuneingExecutorThreadedUpdate.Stopped)
            {
                _context.RuneingController.StopOrchestrator();
            }
            if (botting != (int)BottingExecutorThreadedUpdate.Started)
            {
                _context.BottingController.StartOrchestrator();
            }
            return (
                timestamp > activation ? 
                (int)MacroExecutorStateTypes.Runeing :
                (int)MacroExecutorStateTypes.Botting
            );
        }
    }
    

    public class MacroExecutorStateRuneing : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        public MacroExecutorStateRuneing(MacroExecutorThreadContext context)
        {
            _context = context;
        }

        public override int Execute()
        {
            var botting = _context.BottingController.GetState();
            var runeing = _context.RuneingController.GetState();
            var solving = _context.SolvingController.GetState();
            if (botting != (int)BottingExecutorThreadedUpdate.Stopped)
            {
                _context.BottingController.StopOrchestrator();
            }
            if (solving != (int)SolvingExecutorThreadedUpdate.Stopped)
            {
                _context.SolvingController.StopOrchestrator();
            }
            if (
                runeing != (int)RuneingExecutorThreadedUpdate.Started &&
                runeing != (int)RuneingExecutorThreadedUpdate.Arrived
            )
            {
                _context.RuneingController.StartOrchestrator();
            }
            return (
                runeing == (int)RuneingExecutorThreadedUpdate.Arrived ?
                (int)MacroExecutorStateTypes.Solving :
                (int)MacroExecutorStateTypes.Runeing
            );
        }
    }


    public class MacroExecutorStateSolving : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        public MacroExecutorStateSolving(MacroExecutorThreadContext context)
        {
            _context = context;
        }


        public override int Execute()
        {
            var botting = _context.BottingController.GetState();
            var runeing = _context.RuneingController.GetState();
            var solving = _context.SolvingController.GetState();
            var switchState = solving == (int)SolvingExecutorThreadedUpdate.Solved;
            if (botting != (int)BottingExecutorThreadedUpdate.Stopped)
            {
                _context.BottingController.StopOrchestrator();
            }
            if (runeing != (int)RuneingExecutorThreadedUpdate.Stopped)
            {
                _context.RuneingController.StopOrchestrator();
            }
            if (
                solving != (int)SolvingExecutorThreadedUpdate.Started &&
                solving != (int)SolvingExecutorThreadedUpdate.Solved
            )
            {
                _context.SolvingController.StartOrchestrator();
            }
            if (switchState)
            {
                _context.SolvingStopwatch.SetTimestamp();
            }
            return (
                solving != (int)SolvingExecutorThreadedUpdate.Solved ?
                (int)MacroExecutorStateTypes.Solving :
                (int)MacroExecutorStateTypes.SolvedCheck
            );
        }
    }


    public class MacroExecutorStateSolvedCheck : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        public MacroExecutorStateSolvedCheck(MacroExecutorThreadContext context)
        {
            _context = context;
        }

        public override int Execute()
        {
            var botting = _context.BottingController.GetState();
            var runeing = _context.RuneingController.GetState();
            var solving = _context.SolvingController.GetState();
            if (solving != (int)SolvingExecutorThreadedUpdate.Stopped)
            {
                _context.SolvingController.StopOrchestrator();
            }
            if (runeing != (int)RuneingExecutorThreadedUpdate.Stopped)
            {
                _context.RuneingController.StopOrchestrator();
            }
            if (botting != (int)BottingExecutorThreadedUpdate.Started)
            {
                _context.BottingController.StartOrchestrator();
            }
            if (_context.SolvingStopwatch.GetTimestamp() <= _context.SolvedCheckTimeout)
            {
                return (
                    _context.BottingModel is AbstractBottingModel bottingModel
                    && bottingModel.GetMapModel() is AbstractMapModel mapModel
                    && mapModel.GetTemplatePosition(_context.RuneKey) is Tuple<int, int> tuple
                    && (tuple.Item1 > -1 || tuple.Item2 > -1)
                ) ?
                (int)MacroExecutorStateTypes.Runeing :
                (int)MacroExecutorStateTypes.SolvedCheck;
            }
            else
            {
                _context.RuneActivationPeriodCurrent = _context.RuneActivationPeriod;
                _context.RuneingStopwatch.SetTimestamp();
                return (int)MacroExecutorStateTypes.Botting;
            }
        }
    }


    public class MacroExecutorThreadContext
    {
        public AbstractOrchestratorController BottingController;

        public AbstractOrchestratorController RuneingController;

        public AbstractOrchestratorController SolvingController;

        public AbstractBottingModel? BottingModel;

        public AbstractTimestamp RuneingStopwatch;

        public AbstractTimestamp SolvingStopwatch;

        public string RuneKey;

        public int RuneActivationPeriod;

        public int RuneActivationPeriodCurrent;

        public double SolvedCheckTimeout;

        public double ExecutionFrequency;

        public double SolveCheckTimeout;

        public MacroExecutorThreadContext(
            AbstractOrchestratorController bottingController,
            AbstractOrchestratorController runeingController,
            AbstractOrchestratorController solvingController,
            AbstractTimestamp runeingStopwatch,
            AbstractTimestamp solvingStopwatch,
            string runeKey
        )
        {
            BottingController = bottingController;
            RuneingController = runeingController;
            SolvingController = solvingController;
            BottingModel = null;
            RuneingStopwatch = runeingStopwatch;
            SolvingStopwatch = solvingStopwatch;
            RuneKey = runeKey;
            RuneActivationPeriod = 0;
            RuneActivationPeriodCurrent = 0;
            SolvedCheckTimeout = 0.0;
            ExecutionFrequency = 0.0;
        }
    }


    public class MacroExecutorThreadStateMachine : AbstractKeystrokeTransmitterThreadHelper
    {
        private List<AbstractExecutorState> _executorStates;

        private MacroExecutorThreadContext _context;

        private AbstractMacroSleeper _sleeper;

        private AbstractTimestamp _executeTimestamp;

        private int _macroExecutorState;

        public MacroExecutorThreadStateMachine(
            List<AbstractExecutorState> executorStates,
            MacroExecutorThreadContext context,
            AbstractMacroSleeper sleeper,
            AbstractTimestamp sleepTimesamp,
            int macroExecutorState
        )
        {
            _executorStates = executorStates;
            _context = context;
            _sleeper = sleeper;
            _executeTimestamp = sleepTimesamp;
            _macroExecutorState = macroExecutorState;
        }

        private void _transmit()
        {
            _executeTimestamp.SetTimestamp();
            _macroExecutorState = _executorStates[_macroExecutorState].Execute();
        }

        private void _transmitSleep()
        {
            var freq = _context.ExecutionFrequency;
            var period = freq > 0.00001 ? (1 / freq) : 0;
            var elapsed = _executeTimestamp.GetTimestamp();
            var sleep = (period - elapsed) * 1000;
            if (sleep > 0)
            {
                _sleeper.Sleep((int)sleep);
            }
        }

        public override bool Transmit()
        {
            _transmit();
            _transmitSleep();
            return true;
        }

        public override void Reset()
        {
            _macroExecutorState = (int)MacroExecutorStateTypes.Reset;
            _macroExecutorState = _executorStates[_macroExecutorState].Execute();
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ThreadDependency
                && data is AbstractThread thread
                && thread.State() is AbstractKeystrokeTransmitterThreadState state
            )
            {
                if (state.Type() is KeystrokeTransmitterThreadType.Botting)
                {
                    _context.BottingController.SetOrchestrator(thread);
                    _context.BottingController.SetOrchestratorThreadState(state);

                }
                if (state.Type() is KeystrokeTransmitterThreadType.Runeing)
                {
                    _context.RuneingController.SetOrchestrator(thread);
                    _context.RuneingController.SetOrchestratorThreadState(state);
                }
                if (state.Type() is KeystrokeTransmitterThreadType.Solving)
                {
                    _context.SolvingController.SetOrchestrator(thread);
                    _context.SolvingController.SetOrchestratorThreadState(state);
                }
            }
            if (
                dataType is SystemInjectType.Configuration
                && data is MaplestoryBotConfiguration configuration
                && configuration.MacroSettings is MacroSettings macroSettings
            )
            {
                _context.ExecutionFrequency = macroSettings.CheckFrequency;
                _context.RuneActivationPeriod = macroSettings.RuneActivationPeriod;
                _context.SolveCheckTimeout = macroSettings.SolveCheckTimeout;
            }
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel
            )
            {
                _context.BottingModel = bottingModel;
            }
        }
    }


    public class MacroExecutorThread : AbstractThread
    {
        private AbstractResetEvent _executionEvent;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractKeystrokeTransmitterThreadHelper _macroExecutorThreadHelper;

        private AbstractThreadRunningState _transmittingState;

        private AbstractInjectAction? _threadedInjectAction;

        public MacroExecutorThread(
            AbstractResetEvent executionEvent,
            AbstractKeystrokeTransmitterThreadHelper macroExecutorThreadHelper,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractThreadRunningState transmittingState,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _executionEvent = executionEvent;
            _threadState = threadState;
            _transmittingState = transmittingState;
            _macroExecutorThreadHelper = macroExecutorThreadHelper;
            _threadedInjectAction = null;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _executionEvent.WaitOne();
                _transmittingState.SetRunning(true);
                _macroExecutorThreadHelper.Reset();
                while (_threadState.GetState() == (int)MacroExecutorThreadedUpdate.Started)
                {
                    if (!_macroExecutorThreadHelper.Transmit())
                    {
                        break;
                    }
                }
                _macroExecutorThreadHelper.Reset();
                _transmittingState.SetRunning(false);
            }
        }

        private void _threadedStateUpdate(MacroExecutorThreadedUpdate newState)
        {
            _threadState.SetState((int)newState);
            if (_threadedInjectAction != null)
            {
                _threadedInjectAction.GetAction()(newState, 0);
            }
        }

        public override void Stop()
        {
            base.Stop();
            Inject(MacroOrchestratorThreadInjectType.Stop, null);
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is MacroOrchestratorThreadInjectType injectType)
            {
                if (injectType == MacroOrchestratorThreadInjectType.Start)
                {
                    _threadedStateUpdate(MacroExecutorThreadedUpdate.Starting);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadedStateUpdate(MacroExecutorThreadedUpdate.Started);
                    _executionEvent.Set();
                }
                else if (injectType == MacroOrchestratorThreadInjectType.Stop)
                {
                    _threadedStateUpdate(MacroExecutorThreadedUpdate.Stopping);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadedStateUpdate(MacroExecutorThreadedUpdate.Stopped);
                }
            }
            else
            {
                _macroExecutorThreadHelper.Inject(dataType, value);
                if (dataType is SystemInjectType.InjectAction && value is AbstractInjectAction injectAction)
                {
                    _threadedInjectAction = injectAction;
                }
            }
        }

        public override object? State()
        {
            return _threadState;
        }
    }


    public class MacroOrchestratorThread : AbstractThread
    {
        private AbstractThread _macroExecutorThread;

        private BlockingCollection<int> _threadStates;

        public MacroOrchestratorThread(
            AbstractThread macroExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(runningState)
        {
            _macroExecutorThread = macroExecutorThread;
            _threadStates = threadStates;
        }
        public override void Start()
        {
            base.Start();
            _macroExecutorThread.Start();
        }

        public override void Stop()
        {
            base.Stop();
            _macroExecutorThread.Stop();
            _threadStates.Add(0);
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                foreach (var threadState in _threadStates.GetConsumingEnumerable())
                {
                    if (!_runningState.IsRunning())
                    {
                        break;
                    }
                    _macroExecutorThread.Inject(
                        (MacroOrchestratorThreadInjectType)
                        threadState, null
                    );
                }
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is MacroOrchestratorThreadInjectType injectType)
            {
                _threadStates.Add((int)injectType);
            }
            else
            {
                _macroExecutorThread.Inject(dataType, value);
                if (dataType is SystemInjectType.InjectAction && value is AbstractInjectAction injectAction)
                {
                    injectAction.GetAction()(SystemInjectType.ThreadDependency, this);
                }
            }
        }

        public override object? State()
        {
            return _macroExecutorThread.State();
        }
    }


    public class MacroOrchestratorThreadFactory : AbstractThreadFactory
    {
        private MacroExecutorThreadContext _threadContext()
        {
            return  new MacroExecutorThreadContext(
                new OrchestratorController<
                    BottingOrchestratorThreadInjectType,
                    BottingExecutorThreadedUpdate
                >(
                    BottingOrchestratorThreadInjectType.Start,
                    BottingExecutorThreadedUpdate.Started,
                    BottingOrchestratorThreadInjectType.Stop,
                    BottingExecutorThreadedUpdate.Stopped
                ),
                new OrchestratorController<
                    RuneingOrchestratorThreadInjectType,
                    RuneingExecutorThreadedUpdate
                >(
                    RuneingOrchestratorThreadInjectType.Start,
                    RuneingExecutorThreadedUpdate.Started,
                    RuneingOrchestratorThreadInjectType.Stop,
                    RuneingExecutorThreadedUpdate.Stopped
                ),
                new OrchestratorController<
                    SolvingOrchestratorThreadInjectType,
                    SolvingExecutorThreadedUpdate
                >(
                    SolvingOrchestratorThreadInjectType.Start,
                    SolvingExecutorThreadedUpdate.Started,
                    SolvingOrchestratorThreadInjectType.Stop,
                    SolvingExecutorThreadedUpdate.Stopped
                ),
                new StopwatchTimestamp(),
                new StopwatchTimestamp(),
                MapIconInfo.Rune
            );
        }

        public override AbstractThread CreateThread()
        {
            var threadContext = _threadContext();
            return new MacroOrchestratorThread(
                new MacroExecutorThread(
                    new ExecutionEvent(),
                    new MacroExecutorThreadStateMachine(
                        [
                            new MacroExecutorStateReset(threadContext),
                            new MacroExecutorStateIdle(threadContext),
                            new MacroExecutorStateBotting(threadContext),
                            new MacroExecutorStateRuneing(threadContext),
                            new MacroExecutorStateSolving(threadContext),
                            new MacroExecutorStateSolvedCheck(threadContext),
                        ],
                        threadContext,
                        new MacroSleeper(),
                        new StopwatchTimestamp(),
                        (int)MacroExecutorStateTypes.Idle
                    ),
                    new KeystrokeTransmitterThreadState(
                        (int)MacroExecutorThreadedUpdate.Stopped,
                        KeystrokeTransmitterThreadType.Macro
                    ),
                    new ThreadRunningState(),
                    new ThreadRunningState()
                ),
                new ThreadRunningState(),
                new BlockingCollection<int>()
            );
        }
    }


    public class MacroSystem : AbstractSystem
    {
        public AbstractThread? _macroOrchestratorThread;

        public AbstractThreadFactory _macroOrchestatorThreadFactory;

        public MacroSystem(AbstractThreadFactory macroOrchestratorThreadFactory)
        {
            _macroOrchestatorThreadFactory = macroOrchestratorThreadFactory;
        }

        public override void Initialize()
        {
            if (_macroOrchestratorThread == null)
            {
                _macroOrchestratorThread = _macroOrchestatorThreadFactory.CreateThread();
            }
        }

        public override void Start()
        {
            if (_macroOrchestratorThread != null)
            {
                _macroOrchestratorThread.Start();
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (_macroOrchestratorThread != null)
            {
                _macroOrchestratorThread.Inject(dataType, data);
            }
        }
    }


    public class MacroSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new MacroSystem(new MacroOrchestratorThreadFactory());
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
