using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using System.Collections.Concurrent;
using System.Windows;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters
{
    public enum RuneingOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }


    public enum RuneingExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        Arrived,
        MaxNum
    }


    public class RuneingExecutorThreadHelper : AbstractKeystrokeTransmitterThreadHelper
    {
        private string _characterKey;

        private string _runeKey;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        private AbstractBottingModel? _bottingModel;

        private Point _locatedCharacterPoint;

        private Point _locatedRunePoint;

        public RuneingExecutorThreadHelper(
            string characterKey,
            string runeKey,
            AbstractMacroCommandsExecutorBuilder macroCommandsExecutorBuilder
        )
        {
            _characterKey = characterKey;
            _runeKey = runeKey;
            _macroCommandsExecutorBuilder = macroCommandsExecutorBuilder;
            _bottingModel = null;
            _locatedCharacterPoint = new Point(-1, -1);
            _locatedRunePoint = new Point(-1, -1);
        }

        public override bool Transmit()
        {
            if (
                _bottingModel is AbstractBottingModel bottingModel
                && bottingModel.GetMapModel() is AbstractMapModel mapModel
                && bottingModel.GetRuneModel() is AbstractRuneModel runeModel
            )
            {
                _locatedCharacterPoint = (
                    mapModel.GetTemplatePosition(_characterKey) is Tuple<int, int> characterPosition
                    && characterPosition.Item1 > -1
                    && characterPosition.Item2 > -1
                    && new Point(characterPosition.Item1, characterPosition.Item2) is Point characterPoint
                ) ? characterPoint : _locatedCharacterPoint;

                _locatedRunePoint = (
                    mapModel.GetTemplatePosition(_runeKey) is Tuple<int, int> runePosition
                    && runePosition.Item1 > -1
                    && runePosition.Item2 > -1
                    && new Point(runePosition.Item1, runePosition.Item2) is Point runePoint
                ) ? runePoint : _locatedRunePoint;

                var pointsLocated = (
                    _locatedCharacterPoint.X > -1 &&
                    _locatedCharacterPoint.Y > -1 &&
                    _locatedRunePoint.X > -1 &&
                    _locatedRunePoint.Y > -1
                );

                if (pointsLocated)
                {
                    var nextNavigation = runeModel.NextNavigation(_locatedCharacterPoint, _locatedRunePoint);
                    _macroCommandsExecutor!.Execute(nextNavigation);
                    return nextNavigation.Count > 0;
                }

                return pointsLocated;
            }
            return true;
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
            if (
                dataType is SystemInjectType.KeystrokeTransmitter
                && data is AbstractKeystrokeTransmitter keystrokeTransmitter
            )
            {
                _macroCommandsExecutor = _macroCommandsExecutorBuilder
                    .WithArg(keystrokeTransmitter)
                    .Build();
            }
        }

        public override void Reset()
        {
            _locatedCharacterPoint = new Point(-1, -1);
            _locatedRunePoint = new Point(-1, -1);
        }
    }


    public class RuneingExecutorThread : AbstractThread
    {
        private AbstractResetEvent _executionEvent;

        private AbstractKeystrokeTransmitterThreadHelper _bottingExecutorThreadHelper;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractThreadRunningState _transmittingState;

        public RuneingExecutorThread(
            AbstractResetEvent executionEvent,
            AbstractKeystrokeTransmitterThreadHelper bottingExecutorThreadHelpers,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractThreadRunningState transmittingState,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _executionEvent = executionEvent;
            _bottingExecutorThreadHelper = bottingExecutorThreadHelpers;
            _threadState = threadState;
            _transmittingState = transmittingState;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _executionEvent.WaitOne();
                _transmittingState.SetRunning(true);
                while (_threadState.GetState() == (int)RuneingExecutorThreadedUpdate.Started)
                {
                    _bottingExecutorThreadHelper.Reset();
                    if (!_bottingExecutorThreadHelper.Transmit())
                    {
                        _threadState.SetState((int)RuneingExecutorThreadedUpdate.Arrived);
                    }
                    _bottingExecutorThreadHelper.Reset();
                }
                _transmittingState.SetRunning(false);
            }
        }

        public override void Stop()
        {
            base.Stop();
            Inject(RuneingOrchestratorThreadInjectType.Stop, null);
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is RuneingOrchestratorThreadInjectType injectType)
            {
                if (injectType == RuneingOrchestratorThreadInjectType.Start)
                {
                    _threadState.SetState((int)RuneingExecutorThreadedUpdate.Starting);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)RuneingExecutorThreadedUpdate.Started);
                    _executionEvent.Set();
                }
                else if (injectType == RuneingOrchestratorThreadInjectType.Stop)
                {
                    _threadState.SetState((int)RuneingExecutorThreadedUpdate.Stopping);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)RuneingExecutorThreadedUpdate.Stopped);
                }
            }
            else
            {
                _bottingExecutorThreadHelper.Inject(dataType, value);
            }
        }

        public override object? State()
        {
            return _threadState;
        }
    }


    public class RuneingOrchestratorThread : AbstractThread
    {
        private AbstractThread _runeingExecutorThread;

        private BlockingCollection<int> _threadStates;

        public RuneingOrchestratorThread(
            AbstractThread runeingExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(runningState)
        {
            _runeingExecutorThread = runeingExecutorThread;
            _threadStates = threadStates;
        }

        public override void Start()
        {
            _runeingExecutorThread.Start();
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
            _runeingExecutorThread.Stop();
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
                    _runeingExecutorThread.Inject(
                        (RuneingOrchestratorThreadInjectType)
                        threadState, null
                    );
                }
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is RuneingOrchestratorThreadInjectType injectType)
            {
                _threadStates.Add((int)injectType);
            }
            else
            {
                _runeingExecutorThread.Inject(dataType, value);
                if (
                    dataType is SystemInjectType.InjectAction
                    && value is AbstractInjectAction injectAction
                )
                {
                    injectAction.GetAction()(
                        SystemInjectType.ThreadDependency, this
                    );
                }
            }
        }

        public override object? State()
        {
            return _runeingExecutorThread.State();
        }
    }


    public class RuneingOrchestratorThreadFactory : AbstractThreadFactory
    {
        private string _characterKey;

        private string _runeKey;

        public RuneingOrchestratorThreadFactory(
            string characterKey, string runeKey
        )
        {
            _characterKey = characterKey;
            _runeKey = runeKey;
        }

        public override AbstractThread CreateThread()
        {
            return new RuneingOrchestratorThread(
                new RuneingExecutorThread(
                    new ExecutionEvent(),
                    new RuneingExecutorThreadHelper(
                        _characterKey,
                        _runeKey,
                        new MacroCommandsExecutorBuilder()
                    ),
                    new KeystrokeTransmitterThreadState(
                        (int)RuneingExecutorThreadedUpdate.Stopped,
                        KeystrokeTransmitterThreadType.Runeing
                    ),
                    new ThreadRunningState(),
                    new ThreadRunningState()
                ),
                new ThreadRunningState(),
                new BlockingCollection<int>()
            );
        }
    }


    public class RuneingOrchestratorSystem : AbstractSystem
    {
        private List<AbstractThreadFactory> _threadFactories;

        private List<AbstractThread> _threads;

        public RuneingOrchestratorSystem(
            List<AbstractThreadFactory> threadFactories
        )
        {
            _threadFactories = threadFactories;
            _threads = [];
        }

        public override void Initialize()
        {
            for (int i = 0; i < _threadFactories.Count; i++)
            {
                _threads.Add(_threadFactories[i].CreateThread());
            }
        }

        public override void Start()
        {
            for (int i = 0; i < _threads.Count; i++)
            {
                _threads[i].Start();
            }
        }

        public override void Inject(object dataType, object? data)
        {
            for (int i = 0; i < _threads.Count; i++)
            {
                _threads[i].Inject(dataType, data);
            }
        }
    }


    public class RuneingOrchestratorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new RuneingOrchestratorSystem(
                [new RuneingOrchestratorThreadFactory(MapIconInfo.Character, MapIconInfo.Rune)]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
