using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using System.Collections.Concurrent;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters
{
    public enum AilmentOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }


    public enum AilmentExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        StopBot,
        Cured,
        MaxNum
    }


    public abstract class AbstractAilmentExecutorThreadHandler : IDataInjectable
    {
        public abstract void Handle(int activeDelay);

        public abstract void Inject(object dataType, object? data);
    }


    public class AllCureExecutorThreadHandler : AbstractAilmentExecutorThreadHandler
    {
        private string _allCureKey;

        private AbstractTimestamp _activeStopwatch;

        private AbstractMacroSleeper _macroSleeper;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        public AllCureExecutorThreadHandler(
            AbstractTimestamp activeStopwatch,
            AbstractMacroSleeper macroSleeper,
            AbstractMacroCommandsExecutorBuilder macroCommandsExecutorBuilder
        )
        {
            _allCureKey = "";
            _activeStopwatch = activeStopwatch;
            _macroSleeper = macroSleeper;
            _macroCommandsExecutorBuilder = macroCommandsExecutorBuilder;
        }

        private void _sleepRemaining(int activeDelay)
        {
            var elapsed = _activeStopwatch.GetTimestamp();
            var targetDelay = activeDelay / 1000.0;
            _macroSleeper.Sleep((int)((targetDelay - elapsed) * 1000));
        }


        public override void Handle(int activeDelay)
        {
            _activeStopwatch.SetTimestamp();
            _macroCommandsExecutor!.Execute(
                ["key press {" + _allCureKey + "} {100} {150}"]
            );
            _sleepRemaining(activeDelay);
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.KeystrokeTransmitter &&
                data is AbstractKeystrokeTransmitter keystrokeTransmitter
            )
            {
                _macroCommandsExecutor = _macroCommandsExecutorBuilder
                    .WithArg(keystrokeTransmitter)
                    .Build();
            }
            else if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                var configurationCopy = (
                    (MaplestoryBotConfiguration)maplestoryBotConfiguration.Copy()
                );
                _allCureKey = (
                    configurationCopy.MacroKeySettings.AilmentsAllcureKey
                );
            }
        }
    }


    public class ArrowKeysExecutorThreadHandler : AbstractAilmentExecutorThreadHandler
    {
        private AbstractTimestamp _activeStopwatch;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        public ArrowKeysExecutorThreadHandler(
            AbstractTimestamp activeStopwatch,
            AbstractMacroCommandsExecutorBuilder macroCommandsExecutorBuilder
        )
        {
            _activeStopwatch = activeStopwatch;
            _macroCommandsExecutorBuilder = macroCommandsExecutorBuilder;
        }

        public override void Handle(int activeDelay)
        {
            _activeStopwatch.SetTimestamp();
            while (_activeStopwatch.GetTimestamp() < (activeDelay / 1000.0))
            {
                _macroCommandsExecutor!.Execute(["key press {ARROW_LEFT} {25} {50}"]);
                _macroCommandsExecutor.Execute(["key press {ARROW_RIGHT} {25} {50}"]);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.KeystrokeTransmitter &&
                data is AbstractKeystrokeTransmitter keystrokeTransmitter
            )
            {
                _macroCommandsExecutor = _macroCommandsExecutorBuilder
                    .WithArg(keystrokeTransmitter)
                    .Build();
            }
        }
    }


    public class AilmentExecutorThreadHelper : AbstractKeystrokeTransmitterThreadHelper
    {
        private Dictionary<string, Ailment> _ailments;

        private AbstractBottingModel? _bottingModel;

        private AbstractAilmentExecutorThreadHandler _allCureHandler;

        private AbstractAilmentExecutorThreadHandler _arrowKeysHandler;

        private AbstractKeystrokeTransmitterThreadState _threadState;


        public AilmentExecutorThreadHelper(
            AbstractAilmentExecutorThreadHandler allCureHandler,
            AbstractAilmentExecutorThreadHandler arrowKeysHandler,
            AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _ailments = [];
            _allCureHandler = allCureHandler;
            _arrowKeysHandler = arrowKeysHandler;
            _threadState = threadState;
            _bottingModel = null;
        }

        public override bool Transmit()
        {
            if (
                _bottingModel is AbstractBottingModel bottingModel &&
                bottingModel.GetAilmentsModel() is AbstractAilmentsModel ailmentsModel &&
                ailmentsModel.GetAilments() is List<Tuple<string, int>> ailments &&
                ailments.FindAll(t => t.Item2 > 0) is List<Tuple<string, int>> detectedAilments &&
                detectedAilments.Count > 0   
            )
            {
                var ailment = _getAilment(detectedAilments);
                if (ailment.AllCure != null && ailment.AllCure != 0)
                {
                    _allCureHandler.Handle(ailment.ActiveDelay);
                }
                else if (ailment.ArrowKeys != null && ailment.ArrowKeys != 0)
                {
                    _arrowKeysHandler.Handle(ailment.ActiveDelay);
                }
                else
                {
                    _threadState.SetState((int)AilmentExecutorThreadedUpdate.StopBot);
                }
            }
            else
            {
                _threadState.SetState((int)AilmentExecutorThreadedUpdate.Cured);
            }
            return true;
        }

        private Ailment _getAilment(
            List<Tuple<string, int>> detectedAilments
        )
        {
            var candidates = detectedAilments
                .Select(d => _ailments.GetValueOrDefault(d.Item1))
                .Where(a => a != null)
                .Select(a => (Ailment)a!.Copy())
                .ToList();
            return candidates.FirstOrDefault(a => a.StopBot != null && a.StopBot != 0) ??
                   candidates.FirstOrDefault(a => a.ArrowKeys != null && a.ArrowKeys != 0) ??
                   candidates.FirstOrDefault(a => a.AllCure != null && a.AllCure != 0) ??
                   candidates.FirstOrDefault()!;
        }

        public override void Reset()
        {

        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.BottingModel &&
                data is AbstractBottingModel bottingModel
            )
            {
                _bottingModel = bottingModel;
            }
            else if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                var configurationCopy = (
                    (MaplestoryBotConfiguration) maplestoryBotConfiguration.Copy()
                );
                _ailments = new Dictionary<string, Ailment>(
                    configurationCopy.Ailments
                );
            }
            _allCureHandler.Inject(dataType, data);
            _arrowKeysHandler.Inject(dataType, data);
        }
    }


    public class AilmentExecutorThread : AbstractThread
    {
        private AbstractResetEvent _executionEvent;

        private AbstractKeystrokeTransmitterThreadHelper _ailmentExecutorThreadHelper;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractThreadRunningState _transmittingState;

        public AilmentExecutorThread(
            AbstractResetEvent executionEvent,
            AbstractKeystrokeTransmitterThreadHelper ailmentExecutorThreadHelpers,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractThreadRunningState transmittingState,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _executionEvent = executionEvent;
            _ailmentExecutorThreadHelper = ailmentExecutorThreadHelpers;
            _threadState = threadState;
            _transmittingState = transmittingState;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _executionEvent.WaitOne();
                _transmittingState.SetRunning(true);
                _ailmentExecutorThreadHelper.Reset();
                while (_threadState.GetState() == (int)AilmentExecutorThreadedUpdate.Started)
                {
                    if (!_ailmentExecutorThreadHelper.Transmit())
                    {
                        break;
                    }
                }
                _ailmentExecutorThreadHelper.Reset();
                _transmittingState.SetRunning(false);
            }
        }

        public override void Stop()
        {
            base.Stop();
            Inject(AilmentOrchestratorThreadInjectType.Stop, null);
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is AilmentOrchestratorThreadInjectType injectType)
            {
                if (injectType == AilmentOrchestratorThreadInjectType.Start)
                {
                    _threadState.SetState((int)AilmentExecutorThreadedUpdate.Starting);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)AilmentExecutorThreadedUpdate.Started);
                    _executionEvent.Set();
                }
                else if (injectType == AilmentOrchestratorThreadInjectType.Stop)
                {
                    _threadState.SetState((int)AilmentExecutorThreadedUpdate.Stopping);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)AilmentExecutorThreadedUpdate.Stopped);
                }
            }
            else
            {
                _ailmentExecutorThreadHelper.Inject(dataType, value);
            }
        }

        public override object? State()
        {
            return _threadState;
        }
    }


    public class AilmentOrchestratorThread :
        AbstractOrchestratorThread<AilmentOrchestratorThreadInjectType>
    {
        public AilmentOrchestratorThread(
            AbstractThread ailmentExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(ailmentExecutorThread, runningState, threadStates)
        { }
    }


    public class AilmentOrchestratorThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                (int)AilmentExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Ailment
            );
            return new AilmentOrchestratorThread(
                new AilmentExecutorThread(
                    new ExecutionEvent(),
                    new AilmentExecutorThreadHelper(
                        new AllCureExecutorThreadHandler(
                            new StopwatchTimestamp(),
                            new MacroSleeper(),
                            new MacroCommandsExecutorBuilder()
                        ),
                        new ArrowKeysExecutorThreadHandler(
                            new StopwatchTimestamp(),
                            new MacroCommandsExecutorBuilder()
                        ),
                        threadState
                    ),
                    threadState,
                    new ThreadRunningState(),
                    new ThreadRunningState()
                ),
                new ThreadRunningState(),
                new BlockingCollection<int>()
            );
        }
    }


    public class AilmentOrchestratorSystem : AbstractOrchestratorSystem
    {
        public AilmentOrchestratorSystem(
            List<AbstractThreadFactory> threadFactories
        ) : base(threadFactories)
        { }
    }


    public class AilmentOrchestratorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new AilmentOrchestratorSystem(
                [
                    new AilmentOrchestratorThreadFactory()
                ]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
