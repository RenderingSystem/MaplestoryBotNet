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
        CashShop,
        MaxNum
    }


    public abstract class AbstractExecutorStateActivator
    {
        public abstract void Activate(MacroExecutorStateTypes stateType);
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


    public class ExecutorStateActivator : AbstractExecutorStateActivator
    {
        private MacroExecutorThreadContext _context;

        private List<Action> _stopActions;

        private List<Action> _startActions;

        public ExecutorStateActivator(MacroExecutorThreadContext context)
        {
            _context = context;
            _stopActions = [];
            _startActions = [];
        }

        private void _updateBottingOrchestrator(MacroExecutorStateTypes stateType)
        {
            _updateOrchestrator(
                shouldRun: (
                    stateType == MacroExecutorStateTypes.Botting ||
                    stateType == MacroExecutorStateTypes.SolvedCheck
                ),
                stoppedStates: [
                    (int)BottingExecutorThreadedUpdate.Stopped
                ],
                runningStates: [
                    (int)BottingExecutorThreadedUpdate.Started
                ],
                controller: _context.BottingController
            );
        }

        private void _updateRuneingOrchestrator(MacroExecutorStateTypes stateType)
        {
            _updateOrchestrator(
                shouldRun: stateType == MacroExecutorStateTypes.Runeing,
                stoppedStates: [
                    (int)RuneingExecutorThreadedUpdate.Stopped
                ],
                runningStates: [
                    (int)RuneingExecutorThreadedUpdate.Started,
                    (int)RuneingExecutorThreadedUpdate.Arrived
                ],
                controller: _context.RuneingController
            );
        }

        private void _updateSolvingOrchestrator(MacroExecutorStateTypes stateType)
        {
            _updateOrchestrator(
                shouldRun: stateType == MacroExecutorStateTypes.Solving,
                stoppedStates: [
                    (int)SolvingExecutorThreadedUpdate.Stopped
                ],
                runningStates: [
                    (int)SolvingExecutorThreadedUpdate.Started,
                    (int)SolvingExecutorThreadedUpdate.Solved,
                    (int)SolvingExecutorThreadedUpdate.Failed
                ],
                controller: _context.SolvingController
            );
        }

        private void _updateCashShopOrchestrator(MacroExecutorStateTypes stateType)
        {
            _updateOrchestrator(
                shouldRun: stateType == MacroExecutorStateTypes.CashShop,
                stoppedStates: [
                    (int)CashShopExecutorThreadedUpdate.Stopped
                ],
                runningStates: [
                    (int)CashShopExecutorThreadedUpdate.Started,
                    (int)CashShopExecutorThreadedUpdate.TimedOut
                ],
                controller: _context.CashShopController
            );
        }

        private void _updateOrchestrator(
            bool shouldRun,
            int[] stoppedStates,
            int[] runningStates,
            AbstractOrchestratorController controller
        )
        {
            if (controller.GetState() is not int currentState)
            {
                return;
            }
            if (shouldRun && !runningStates.Contains(currentState))
            {
                _startActions.Add(controller.StartOrchestrator);
            }
            else if (!shouldRun && !stoppedStates.Contains(currentState))
            {
                _stopActions.Add(controller.StopOrchestrator);
            }
        }

        public override void Activate(MacroExecutorStateTypes stateType)
        {
            _stopActions.Clear();
            _startActions.Clear();
            _updateBottingOrchestrator(stateType);
            _updateRuneingOrchestrator(stateType);
            _updateSolvingOrchestrator(stateType);
            _updateCashShopOrchestrator(stateType);
            foreach (var _ in _stopActions) _();
            foreach (var _ in _startActions) _();
        }
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

        private AbstractExecutorStateActivator _activator;
        public MacroExecutorStateReset(
            MacroExecutorThreadContext context,
            AbstractExecutorStateActivator activator
        )
        {
            _context = context;
            _activator = activator;
        }

        public override int Execute()
        {
            _activator.Activate(MacroExecutorStateTypes.Reset);
            if (_context.BottingModel is AbstractBottingModel bottingModel)
            {
                var runeModel = bottingModel.GetRuneModel();
                var cooldown = runeModel.GetActivation();
                _context.RuneActivationPeriodCurrent = cooldown;
            }
            _context.MissedRuneCount = 0;
            return (int)MacroExecutorStateTypes.Idle;
        }
    }


    public class MacroExecutorStateIdle : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        private AbstractExecutorStateActivator _activator;

        public MacroExecutorStateIdle(
            MacroExecutorThreadContext context,
            AbstractExecutorStateActivator activator
        )
        {
            _context = context;
            _activator = activator;
        }

        public override int Execute()
        {
            _activator.Activate(MacroExecutorStateTypes.Idle);
            _context.RuneingStopwatch.SetTimestamp();
            return (int)MacroExecutorStateTypes.Botting;
        }
    }


    public class MacroExecutorStateBotting : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        private AbstractExecutorStateActivator _activator;

        public MacroExecutorStateBotting(
            MacroExecutorThreadContext context,
            AbstractExecutorStateActivator activator
        )
        {
            _context = context;
            _activator = activator;
        }

        public override int Execute()
        {
            _activator.Activate(MacroExecutorStateTypes.Botting);
            if (
                _context.RuneingStopwatch.GetTimestamp() > _context.RuneActivationPeriodCurrent &&
                _context.BottingModel is AbstractBottingModel bottingModel &&
                bottingModel.GetMapModel() is AbstractMapModel mapModel &&
                mapModel.GetTemplatePosition(_context.RuneKey) is Tuple<int, int> tuple &&
                (tuple.Item1 > -1 || tuple.Item2 > -1)
            )
            {
                return (int)MacroExecutorStateTypes.Runeing;
            }
            else
            {
                return (int)MacroExecutorStateTypes.Botting;
            }
        }
    }
    

    public class MacroExecutorStateRuneing : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        private AbstractExecutorStateActivator _activator;

        public MacroExecutorStateRuneing(
            MacroExecutorThreadContext context,
            AbstractExecutorStateActivator activator
        )
        {
            _context = context;
            _activator = activator;
        }

        public override int Execute()
        {
            _activator.Activate(MacroExecutorStateTypes.Runeing);
            return (
                _context.RuneingController.GetState() == (int)RuneingExecutorThreadedUpdate.Arrived ?
                (int)MacroExecutorStateTypes.Solving :
                (int)MacroExecutorStateTypes.Runeing
            );
        }
    }


    public class MacroExecutorStateSolving : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        private AbstractExecutorStateActivator _activator;

        public MacroExecutorStateSolving(
            MacroExecutorThreadContext context,
            AbstractExecutorStateActivator activator
        )
        {
            _context = context;
            _activator = activator;
        }


        public override int Execute()
        {
            _activator.Activate(MacroExecutorStateTypes.Solving);
            var solving = _context.SolvingController.GetState();
            if (
                solving == (int)SolvingExecutorThreadedUpdate.Solved ||
                solving == (int)SolvingExecutorThreadedUpdate.Failed
            )
            {
                _context.SolvingStopwatch.SetTimestamp();
                return (int)MacroExecutorStateTypes.SolvedCheck;
            }
            else
            {
                return (int)MacroExecutorStateTypes.Solving;
            }
        }
    }


    public class MacroExecutorStateSolvedCheck : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        private AbstractExecutorStateActivator _activator;

        public MacroExecutorStateSolvedCheck(
            MacroExecutorThreadContext context,
            AbstractExecutorStateActivator activator
        )
        {
            _context = context;
            _activator = activator;
        }

        public override int Execute()
        {
            _activator.Activate(MacroExecutorStateTypes.SolvedCheck);
            if (_context.SolvingStopwatch.GetTimestamp() <= _context.SolveCheckTimeout)
            {
                if (
                    _context.BottingModel is AbstractBottingModel bottingModel
                    && bottingModel.GetMapModel() is AbstractMapModel mapModel
                    && mapModel.GetTemplatePosition(_context.RuneKey) is Tuple<int, int> tuple
                    && (tuple.Item1 > -1 || tuple.Item2 > -1)
                )
                {

                    if (++_context.MissedRuneCount < _context.CashShopTolerance)
                    {
                        return (int)MacroExecutorStateTypes.Runeing;
                    }
                    else
                    {
                        _context.MissedRuneCount = 0;
                        return (int)MacroExecutorStateTypes.CashShop;
                    }
                }
                else
                {
                    return (int)MacroExecutorStateTypes.SolvedCheck;
                }
            }
            else
            {
                if (
                    _context.BottingModel is AbstractBottingModel bottingModel
                    && _context.BottingModel.GetRuneModel() is AbstractRuneModel runeModel
                    && runeModel.GetCooldown() is int cooldown
                )
                {
                    _context.RuneActivationPeriodCurrent = cooldown;
                }
                _context.RuneingStopwatch.SetTimestamp();
                _context.MissedRuneCount = 0;
                return (int)MacroExecutorStateTypes.Botting;
            }
        }
    }


    public class MacroExecutorStateCashShop : AbstractExecutorState
    {
        private MacroExecutorThreadContext _context;

        private AbstractExecutorStateActivator _activator;

        public MacroExecutorStateCashShop(
            MacroExecutorThreadContext context,
            AbstractExecutorStateActivator activator
        )
        {
            _context = context;
            _activator = activator;
        }

        public override int Execute()
        {
            _activator.Activate(MacroExecutorStateTypes.CashShop);
            var cashShop = _context.CashShopController.GetState();
            if (cashShop == (int)CashShopExecutorThreadedUpdate.TimedOut)
            {
                return (int)MacroExecutorStateTypes.Botting;
            }
            else
            {
                return (int)MacroExecutorStateTypes.CashShop;
            }
        }
    }


    public class MacroExecutorThreadContext
    {
        public AbstractOrchestratorController BottingController;

        public AbstractOrchestratorController RuneingController;

        public AbstractOrchestratorController SolvingController;

        public AbstractOrchestratorController CashShopController;

        public AbstractBottingModel? BottingModel;

        public AbstractTimestamp RuneingStopwatch;

        public AbstractTimestamp SolvingStopwatch;

        public string RuneKey;

        public int RuneActivationPeriodCurrent;

        public double ExecutionFrequency;

        public double SolveCheckTimeout;

        public int CashShopTolerance;

        public int MissedRuneCount;

        public MacroExecutorThreadContext(
            AbstractOrchestratorController bottingController,
            AbstractOrchestratorController runeingController,
            AbstractOrchestratorController solvingController,
            AbstractOrchestratorController cashShopController,
            AbstractTimestamp runeingStopwatch,
            AbstractTimestamp solvingStopwatch,
            string runeKey
        )
        {
            BottingController = bottingController;
            RuneingController = runeingController;
            SolvingController = solvingController;
            CashShopController = cashShopController;
            BottingModel = null;
            RuneingStopwatch = runeingStopwatch;
            SolvingStopwatch = solvingStopwatch;
            RuneKey = runeKey;
            RuneActivationPeriodCurrent = 0;
            SolveCheckTimeout = 0.0;
            ExecutionFrequency = 0.0;
            CashShopTolerance = 0;
            MissedRuneCount = 0;
        }
    }


    public class MacroExecutorThreadStateMachine : AbstractKeystrokeTransmitterThreadHelper
    {
        private List<AbstractExecutorState> _executorStates;

        private MacroExecutorThreadContext _context;

        private AbstractMacroSleeper _sleeper;

        private AbstractTimestamp _executeTimestamp;

        private AbstractInjectAction? _threadedInjectAction;

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
            var nextState = _executorStates[_macroExecutorState].Execute();
            if (nextState != _macroExecutorState)
            {
                _threadedInjectAction?.GetAction()((MacroExecutorStateTypes)nextState, 0);
            }
            _macroExecutorState = nextState;
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
            _threadedInjectAction?.GetAction()((MacroExecutorStateTypes)_macroExecutorState, 0);
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
                if (state.Type() is KeystrokeTransmitterThreadType.CashShop)
                {
                    _context.CashShopController.SetOrchestrator(thread);
                    _context.CashShopController.SetOrchestratorThreadState(state);
                }
            }
            if (
                dataType is SystemInjectType.Configuration
                && data is MaplestoryBotConfiguration configuration
                && configuration.MacroSettings is MacroSettings macroSettings
            )
            {
                _context.ExecutionFrequency = macroSettings.CheckFrequency;
                _context.SolveCheckTimeout = macroSettings.SolveCheckTimeout;
                _context.CashShopTolerance = macroSettings.CashShopTolerance;
            }
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel
            )
            {
                _context.BottingModel = bottingModel;
            }
            if (dataType is SystemInjectType.InjectAction && data is AbstractInjectAction injectAction)
            {
                _threadedInjectAction = injectAction;
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


    public class MacroOrchestratorThread : AbstractOrchestratorThread<MacroOrchestratorThreadInjectType>
    {
        public MacroOrchestratorThread(
            AbstractThread macroExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(macroExecutorThread, runningState, threadStates)
        { }
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
                new OrchestratorController<
                    CashShopOrchestratorThreadInjectType,
                    CashShopExecutorThreadedUpdate
                >(
                    CashShopOrchestratorThreadInjectType.Start,
                    CashShopExecutorThreadedUpdate.Started,
                    CashShopOrchestratorThreadInjectType.Stop,
                    CashShopExecutorThreadedUpdate.Stopped
                ),
                new StopwatchTimestamp(),
                new StopwatchTimestamp(),
                MapIconInfo.Rune
            );
        }

        public override AbstractThread CreateThread()
        {
            var threadContext = _threadContext();
            var activator = new ExecutorStateActivator(threadContext);
            return new MacroOrchestratorThread(
                new MacroExecutorThread(
                    new ExecutionEvent(),
                    new MacroExecutorThreadStateMachine(
                        [
                            new MacroExecutorStateReset(threadContext, activator),
                            new MacroExecutorStateIdle(threadContext, activator),
                            new MacroExecutorStateBotting(threadContext, activator),
                            new MacroExecutorStateRuneing(threadContext, activator),
                            new MacroExecutorStateSolving(threadContext, activator),
                            new MacroExecutorStateSolvedCheck(threadContext, activator),
                            new MacroExecutorStateCashShop(threadContext, activator),
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


    public class MacroSystem : AbstractOrchestratorSystem
    {
        public MacroSystem(
            List<AbstractThreadFactory> threadFactories
        ) : base(threadFactories)
        { }
    }


    public class MacroSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new MacroSystem(
                [new MacroOrchestratorThreadFactory()]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
