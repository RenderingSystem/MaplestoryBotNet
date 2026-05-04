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
            _macroCommandsExecutor = null;
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

        private AbstractKeystrokeTransmitterThreadHelper _runeingExecutorThreadHelper;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractThreadRunningState _transmittingState;

        public RuneingExecutorThread(
            AbstractResetEvent executionEvent,
            AbstractKeystrokeTransmitterThreadHelper runeingExecutorThreadHelpers,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractThreadRunningState transmittingState,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _executionEvent = executionEvent;
            _runeingExecutorThreadHelper = runeingExecutorThreadHelpers;
            _threadState = threadState;
            _transmittingState = transmittingState;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _executionEvent.WaitOne();
                _transmittingState.SetRunning(true);
                _runeingExecutorThreadHelper.Reset();
                while (_threadState.GetState() == (int)RuneingExecutorThreadedUpdate.Started)
                {
                    if (!_runeingExecutorThreadHelper.Transmit())
                    {
                        _threadState.SetState((int)RuneingExecutorThreadedUpdate.Arrived);
                    }
                }
                _runeingExecutorThreadHelper.Reset();
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
                _runeingExecutorThreadHelper.Inject(dataType, value);
            }
        }

        public override object? State()
        {
            return _threadState;
        }
    }


    public class RuneingOrchestratorThread : AbstractOrchestratorThread<RuneingOrchestratorThreadInjectType>
    {
        public RuneingOrchestratorThread(
            AbstractThread runeingExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(runeingExecutorThread, runningState, threadStates)
        { }
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


    public class RuneingOrchestratorSystem : AbstractOrchestratorSystem
    {
        public RuneingOrchestratorSystem(
            List<AbstractThreadFactory> threadFactories
        ) : base(threadFactories)
        { }
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
