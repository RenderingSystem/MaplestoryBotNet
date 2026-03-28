using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems
{
    public enum KeystrokeTransmitterOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }


    public enum KeystrokeTransmitterExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        KeystrokeTransmitterOrchestratorThreadStateMaxNum
    }


    public abstract class AbstractKeystrokeTransmitterThreadState
    {
        public abstract int GetState();

        public abstract void SetState(int state);
    }


    public abstract class AbstractMacroCommandsSelector
    {
        public abstract List<string> SelectMacroCommands(
            List<MinimapPointMacros> macros
        );
    }


    public abstract class AbstractPointDataSelector
    {
        public abstract MinimapPointData? SelectPoint(MapModel mapModel);
    }


    public abstract class AbstractKeystrokeTransmitterExecutorThreadHelper : IDataInjectable
    {
        public abstract bool Transmit();

        public abstract void Inject(object dataType, object? data);
    }


    public class RandomMacroCommandsSelector : AbstractMacroCommandsSelector
    {
        AbstractMacroRandom _macroRandom;
        public RandomMacroCommandsSelector(
            AbstractMacroRandom macroRandom
        )
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


    public class KeystrokeTransmitterPointDataSelector : AbstractPointDataSelector
    {
        private string _templateKey;
        public KeystrokeTransmitterPointDataSelector(string templateKey)
        {
            _templateKey = templateKey;
        }

        public override MinimapPointData? SelectPoint(MapModel mapModel)
        {
            var minimapPoints = mapModel.MacroPoints();
            var (charX, charY) = mapModel.GetTemplatePosition(_templateKey);
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


    public class KeystrokeTransmitterExecutorThreadHelper : AbstractKeystrokeTransmitterExecutorThreadHelper
    {
        private AbstractPointDataSelector _pointDataSelector;

        private AbstractMacroCommandsSelector _macroCommandsSelector;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        private MapModel? _mapModel;

        public KeystrokeTransmitterExecutorThreadHelper(
            AbstractPointDataSelector pointDataSelector,
            AbstractMacroCommandsSelector macroCommandsSelector,
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
            var transmitData = _pointDataSelector.SelectPoint(_mapModel!);
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
            else if (dataType is SystemInjectType.MapModel && data is MapModel mapModel)
            {
                _mapModel = mapModel;
            }
        }
    }


    public class KeystrokeTransmitterThreadState : AbstractKeystrokeTransmitterThreadState
    {
        private volatile int _threadState;

        public KeystrokeTransmitterThreadState(int threadState)
        {
            _threadState = threadState;
        }

        public override int GetState()
        {
            return _threadState;
        }

        public override void SetState(int state)
        {
            _threadState = state;
        }
    }


    public class KeystrokeTransmitterExecutorThread : AbstractThread
    {
        private AbstractCountDown _receiveCountDown;

        private AbstractCountDown _transmitCountDown;

        private AbstractKeystrokeTransmitterExecutorThreadHelper _keystrokeTransmitterExecutorThreadHelper;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractInjectAction? _threadedInjectAction;

        public KeystrokeTransmitterExecutorThread(
            AbstractCountDown receiveCountDown,
            AbstractCountDown transmitCountDown,
            AbstractKeystrokeTransmitterExecutorThreadHelper keystrokeTransmitterExecutorThreadHelpers,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _receiveCountDown = receiveCountDown;
            _transmitCountDown = transmitCountDown;
            _keystrokeTransmitterExecutorThreadHelper = keystrokeTransmitterExecutorThreadHelpers;
            _threadState = threadState;
            _threadedInjectAction = null;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _receiveCountDown.WaitCountDown();
                if (_runningState.IsRunning())
                {
                    while (_threadState.GetState() == (int)KeystrokeTransmitterExecutorThreadedUpdate.Started)
                    {
                        if (!_keystrokeTransmitterExecutorThreadHelper.Transmit())
                        {
                            break;
                        }
                    }
                }
                _receiveCountDown.SetCountDown(1);
                _transmitCountDown.CountDown();
            }
        }

        public override void Stop()
        {
            base.Stop();
            Inject(KeystrokeTransmitterOrchestratorThreadInjectType.Stop, null);
        }

        private void _threadedStateUpdate(KeystrokeTransmitterExecutorThreadedUpdate newState)
        {
            _threadState.SetState((int) newState);
            if (_threadedInjectAction != null)
            {
                _threadedInjectAction.GetAction()(newState, 0);
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is KeystrokeTransmitterOrchestratorThreadInjectType injectType)
            {
                if (injectType == KeystrokeTransmitterOrchestratorThreadInjectType.Start)
                {
                    _transmitCountDown.SetCountDown(1);
                    _threadedStateUpdate(KeystrokeTransmitterExecutorThreadedUpdate.Starting);
                    _receiveCountDown.CountDown();
                    _transmitCountDown.WaitCountDown();
                    _threadedStateUpdate(KeystrokeTransmitterExecutorThreadedUpdate.Started);
                    _receiveCountDown.CountDown();
                }
                else if (injectType == KeystrokeTransmitterOrchestratorThreadInjectType.Stop)
                {
                    _transmitCountDown.SetCountDown(1);
                    _threadedStateUpdate(KeystrokeTransmitterExecutorThreadedUpdate.Stopping);
                    _receiveCountDown.CountDown();
                    _transmitCountDown.WaitCountDown();
                    _threadedStateUpdate(KeystrokeTransmitterExecutorThreadedUpdate.Stopped);
                }
            }
            else
            {
                _keystrokeTransmitterExecutorThreadHelper.Inject(dataType, value);
                if (
                    dataType is SystemInjectType.InjectAction
                    && value is AbstractInjectAction injectAction
                )
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


    public class KeystrokeTransmitterOrchestratorThread : AbstractThread
    {
        private AbstractCountDown _receiveCountDown;

        private AbstractThread _keystrokeTransmitterExecutorThread;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        public KeystrokeTransmitterOrchestratorThread(
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractCountDown receiveCountDown,
            AbstractThread keystrokeTransmitterExecutorThread,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _threadState = threadState;
            _receiveCountDown = receiveCountDown;
            _keystrokeTransmitterExecutorThread = keystrokeTransmitterExecutorThread;
        }

        public override void Start()
        {
            base.Start();
            _keystrokeTransmitterExecutorThread.Start();
        }

        public override void Stop()
        {
            base.Stop();
            _keystrokeTransmitterExecutorThread.Stop();
            _receiveCountDown.CountDown();
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _receiveCountDown.WaitCountDown();
                if (_runningState.IsRunning())
                {
                    _keystrokeTransmitterExecutorThread.Inject(
                        (KeystrokeTransmitterOrchestratorThreadInjectType) _threadState.GetState(), null
                    );
                }
                _receiveCountDown.SetCountDown(1);
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is KeystrokeTransmitterOrchestratorThreadInjectType injectType)
            {
                _threadState.SetState((int) injectType);
                _receiveCountDown.CountDown();
            }
            else
            {
                _keystrokeTransmitterExecutorThread.Inject(dataType, value);
                if (
                    dataType is SystemInjectType.InjectAction
                    && value is AbstractInjectAction injectAction
                )
                {
                    injectAction.GetAction()(SystemInjectType.ThreadDependency, this);
                }
            }
        }

        public override object? State()
        {
            return _keystrokeTransmitterExecutorThread.State();
        }
    }


    public class KeystrokeTransmitterOrchestratorThreadFactory : AbstractThreadFactory
    {
        private string _templateKey;

        public KeystrokeTransmitterOrchestratorThreadFactory(string templateKey)
        {
            _templateKey = templateKey;
        }

        public override AbstractThread CreateThread()
        {
            return new KeystrokeTransmitterOrchestratorThread(
                new KeystrokeTransmitterThreadState(
                    (int) KeystrokeTransmitterOrchestratorThreadInjectType.None
                ),
                new ThreadSafeCountDown(),
                new KeystrokeTransmitterExecutorThread(
                    new ThreadSafeCountDown(),
                    new ThreadSafeCountDown(),
                    new KeystrokeTransmitterExecutorThreadHelper(
                        new KeystrokeTransmitterPointDataSelector(_templateKey),
                        new RandomMacroCommandsSelector(new MacroRandom()),
                        new MacroCommandsExecutorBuilder()
                    ),
                    new KeystrokeTransmitterThreadState(
                        (int) KeystrokeTransmitterExecutorThreadedUpdate.Stopped
                    ),
                    new ThreadRunningState()
                ),
                new ThreadRunningState()
            );
        }
    }


    public class KeystrokeTransmitterOrchestratorSystem : AbstractSystem
    {
        private List<AbstractThreadFactory> _threadFactories;

        private List<AbstractThread> _threads;

        public KeystrokeTransmitterOrchestratorSystem(
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


    public class KeystrokeTransmitterOrchestratorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new KeystrokeTransmitterOrchestratorSystem(
                [
                    new KeystrokeTransmitterOrchestratorThreadFactory(MapIconInfo.Character)
                ]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
