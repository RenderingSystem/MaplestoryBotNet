using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using System.Collections.Concurrent;
using static Interception.InterceptionInterop;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters
{
    public enum BottingOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }


    public enum BottingExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        MaxNum
    }


    public abstract class AbstractBottingMacroCommandsSelector
    {
        public abstract List<string> SelectMacroCommands(
            List<MinimapPointMacros> macros
        );
    }


    public abstract class AbstractBottingPointDataSelector
    {
        public abstract MinimapPointData? SelectPoint(AbstractBottingModel bottingModel);
    }


    public class BottingRandomMacroCommandsSelector : AbstractBottingMacroCommandsSelector
    {
        AbstractMacroRandom _macroRandom;
        public BottingRandomMacroCommandsSelector(AbstractMacroRandom macroRandom)
        {
            _macroRandom = macroRandom;
        }

        public override List<string> SelectMacroCommands(
            List<MinimapPointMacros> macros
        )
        {
            var totalChance = 0;
            for (int i = 0; i < macros.Count; i++)
            {
                totalChance += macros[i].MacroChance;
            }
            var randomNumber = _macroRandom.Next(0, totalChance - 1);
            var cumulativeChance = 0;
            for (int i = 0; i < macros.Count; i++)
            {
                cumulativeChance += macros[i].MacroChance;
                if (randomNumber < cumulativeChance)
                {
                    return macros[i].MacroCommands;
                }
            }
            return [];
        }
    }


    public class BottingPointDataSelector : AbstractBottingPointDataSelector
    {
        private string _templateKey;

        public BottingPointDataSelector(string templateKey)
        {
            _templateKey = templateKey;
        }

        public override MinimapPointData? SelectPoint(AbstractBottingModel bottingModel)
        {
            var minimapPoints = bottingModel.GetMacroModel().MacroPoints();
            var (charX, charY) = bottingModel.GetMapModel().GetTemplatePosition(_templateKey);
            MinimapPointData? selectedMinimapPoint = null;
            var minDistanceSquared = double.PositiveInfinity;
            for (int i = 0; i < minimapPoints.Count; i++)
            {
                var currMinimapPoint = minimapPoints[i];
                var currX = currMinimapPoint.X + (currMinimapPoint.XRange / 2);
                var currY = currMinimapPoint.Y + (currMinimapPoint.YRange / 2);
                var (vX, vY) = (charX - currX, charY - currY);
                var distanceSquared = (vX * vX) + (vY * vY);
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    selectedMinimapPoint = currMinimapPoint.PointData.Copy();
                }
            }
            return selectedMinimapPoint;
        }
    }


    public class BottingExecutorThreadHelper : AbstractKeystrokeTransmitterThreadHelper
    {
        private AbstractBottingPointDataSelector _pointDataSelector;

        private AbstractBottingMacroCommandsSelector _macroCommandsSelector;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        private AbstractBottingModel? _bottingModel;

        public BottingExecutorThreadHelper(
            AbstractBottingPointDataSelector pointDataSelector,
            AbstractBottingMacroCommandsSelector macroCommandsSelector,
            AbstractMacroCommandsExecutorBuilder executorBuilder

        )
        {
            _pointDataSelector = pointDataSelector;
            _macroCommandsSelector = macroCommandsSelector;
            _macroCommandsExecutorBuilder = executorBuilder;
            _macroCommandsExecutor = null;
        }

        public override bool Transmit()
        {
            var transmitData = _pointDataSelector.SelectPoint(_bottingModel!);
            if (transmitData != null)
            {
                var commands = transmitData.Commands;
                var macroCommands = _macroCommandsSelector.SelectMacroCommands(commands);
                _macroCommandsExecutor!.Execute(macroCommands);
            }
            return true;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.KeystrokeTransmitter
                && data is AbstractKeystrokeTransmitter keystrokeTransmitter
            )
            {
                _macroCommandsExecutor = _macroCommandsExecutorBuilder
                    .WithArg(keystrokeTransmitter)
                    .Build();
            }
            else if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }

        public override void Reset()
        {
            return;
        }
    }


    public class BottingExecutorThread : AbstractThread
    {
        private AbstractResetEvent _executionEvent;

        private AbstractKeystrokeTransmitterThreadHelper _bottingExecutorThreadHelper;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractThreadRunningState _transmittingState;

        public BottingExecutorThread(
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
                while (_threadState.GetState() == (int)BottingExecutorThreadedUpdate.Started)
                {
                    if (!_bottingExecutorThreadHelper.Transmit())
                    {
                        break;
                    }
                }
                _transmittingState.SetRunning(false);
            }
        }

        public override void Stop()
        {
            base.Stop();
            Inject(BottingOrchestratorThreadInjectType.Stop, null);
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is BottingOrchestratorThreadInjectType injectType)
            {
                if (injectType == BottingOrchestratorThreadInjectType.Start)
                {
                    _threadState.SetState((int)BottingExecutorThreadedUpdate.Starting);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)BottingExecutorThreadedUpdate.Started);
                    _executionEvent.Set();
                }
                else if (injectType == BottingOrchestratorThreadInjectType.Stop)
                {
                    _threadState.SetState((int)BottingExecutorThreadedUpdate.Stopping);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)BottingExecutorThreadedUpdate.Stopped);
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


    public class BottingOrchestratorThread : AbstractThread
    {
        private AbstractThread _bottingExecutorThread;

        private BlockingCollection<int> _threadStates;

        public BottingOrchestratorThread(
            AbstractThread bottingExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(runningState)
        {
            _bottingExecutorThread = bottingExecutorThread;
            _threadStates = threadStates;
        }

        public override void Start()
        {
            _bottingExecutorThread.Start();
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
            _bottingExecutorThread.Stop();
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
                    _bottingExecutorThread.Inject(
                        (BottingOrchestratorThreadInjectType)
                        threadState, null
                    );
                }
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is BottingOrchestratorThreadInjectType injectType)
            {
                _threadStates.Add((int)injectType);
            }
            else
            {
                _bottingExecutorThread.Inject(dataType, value);
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
            return _bottingExecutorThread.State();
        }
    }


    public class BottingOrchestratorThreadFactory : AbstractThreadFactory
    {
        private string _templateKey;

        public BottingOrchestratorThreadFactory(string templateKey)
        {
            _templateKey = templateKey;
        }

        public override AbstractThread CreateThread()
        {
            return new BottingOrchestratorThread(
                new BottingExecutorThread(
                    new ExecutionEvent(),
                    new BottingExecutorThreadHelper(
                        new BottingPointDataSelector(_templateKey),
                        new BottingRandomMacroCommandsSelector(new MacroRandom()),
                        new MacroCommandsExecutorBuilder()
                    ),
                    new KeystrokeTransmitterThreadState(
                        (int)BottingExecutorThreadedUpdate.Stopped,
                        KeystrokeTransmitterThreadType.Botting
                    ),
                    new ThreadRunningState(),
                    new ThreadRunningState()
                ),
                new ThreadRunningState(),
                new BlockingCollection<int>()
            );
        }
    }


    public class BottingOrchestratorSystem : AbstractSystem
    {
        private List<AbstractThreadFactory> _threadFactories;

        private List<AbstractThread> _threads;

        public BottingOrchestratorSystem(
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


    public class BottingOrchestratorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new BottingOrchestratorSystem(
                [
                    new BottingOrchestratorThreadFactory(MapIconInfo.Character)
                ]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
