using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using System.Collections.Concurrent;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters
{
    public enum CashShopOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }


    public enum CashShopExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        TimedOut,
        MaxNum
    }


    public class CashShopExecutorThreadHelper : AbstractKeystrokeTransmitterThreadHelper
    {
        private AbstractTimestamp _cashShopStopwatch;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private double _cashShopAttemptDelay;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        private int _cashShopTimeout;

        private string _cashShopKey;

        public CashShopExecutorThreadHelper(
            AbstractTimestamp cashShopStopwatch,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractMacroCommandsExecutorBuilder executorBuilder,
            double cashShopAttemptDelay
        )
        {
            _cashShopStopwatch = cashShopStopwatch;
            _threadState = threadState;
            _macroCommandsExecutorBuilder = executorBuilder;
            _cashShopAttemptDelay = cashShopAttemptDelay;
            _macroCommandsExecutor = null;
            _cashShopTimeout = 0;
            _cashShopKey = "";
        }

        public override void Reset()
        {
            _cashShopStopwatch.SetTimestamp();
        }

        public override bool Transmit()
        {
            if (_macroCommandsExecutor == null)
            {
                return true;
            }
            var timestamp = _cashShopStopwatch.GetTimestamp();
            if (timestamp < _cashShopAttemptDelay)
            {
                _macroCommandsExecutor.Execute(["key press {" + _cashShopKey + "} {100} {150}"]);
                _macroCommandsExecutor.Execute(["wait {100} {150}"]);
            }
            else if (timestamp > (_cashShopAttemptDelay + _cashShopTimeout))
            {
                _macroCommandsExecutor.Execute(["key press {ESCAPE} {100} {150}"]);
                _macroCommandsExecutor.Execute(["wait {900} {1100}"]);
                _macroCommandsExecutor.Execute(["key press {ENTER} {100} {150}"]);
                _threadState.SetState((int)CashShopExecutorThreadedUpdate.TimedOut);
            }
            else
            {
                Thread.Yield();
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
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                _cashShopTimeout = maplestoryBotConfiguration.MacroSettings.CashShopTimeout;
                _cashShopKey = maplestoryBotConfiguration.MacroKeySettings.CashShopKey;
            }
        }
    }


    public class CashShopExecutorThread : AbstractThread
    {
        private AbstractResetEvent _executionEvent;

        private AbstractKeystrokeTransmitterThreadHelper _cashShopExecutorThreadHelper;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractThreadRunningState _transmittingState;

        public CashShopExecutorThread(
            AbstractResetEvent executionEvent,
            AbstractKeystrokeTransmitterThreadHelper cashShopExecutorThreadHelpers,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractThreadRunningState transmittingState,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _executionEvent = executionEvent;
            _cashShopExecutorThreadHelper = cashShopExecutorThreadHelpers;
            _threadState = threadState;
            _transmittingState = transmittingState;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _executionEvent.WaitOne();
                _transmittingState.SetRunning(true);
                _cashShopExecutorThreadHelper.Reset();
                while (_threadState.GetState() == (int)CashShopExecutorThreadedUpdate.Started)
                {
                    if (!_cashShopExecutorThreadHelper.Transmit())
                    {
                        break;
                    }
                }
                _cashShopExecutorThreadHelper.Reset();
                _transmittingState.SetRunning(false);
            }
        }

        public override void Stop()
        {
            base.Stop();
            Inject(CashShopOrchestratorThreadInjectType.Stop, null);
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is CashShopOrchestratorThreadInjectType injectType)
            {
                if (injectType == CashShopOrchestratorThreadInjectType.Start)
                {
                    _threadState.SetState((int)CashShopExecutorThreadedUpdate.Starting);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)CashShopExecutorThreadedUpdate.Started);
                    _executionEvent.Set();
                }
                else if (injectType == CashShopOrchestratorThreadInjectType.Stop)
                {
                    _threadState.SetState((int)CashShopExecutorThreadedUpdate.Stopping);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)CashShopExecutorThreadedUpdate.Stopped);
                }
            }
            else
            {
                _cashShopExecutorThreadHelper.Inject(dataType, value);
            }
        }

        public override object? State()
        {
            return _threadState;
        }
    }


    public class CashShopOrchestratorThread : AbstractOrchestratorThread<CashShopOrchestratorThreadInjectType>
    {
        public CashShopOrchestratorThread(
            AbstractThread cashShopExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(cashShopExecutorThread, runningState, threadStates)
        { }
    }


    public class CashShopOrchestratorThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                (int)CashShopExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.CashShop
            );
            return new CashShopOrchestratorThread(
                new CashShopExecutorThread(
                    new ExecutionEvent(),
                    new CashShopExecutorThreadHelper(
                        new StopwatchTimestamp(),
                        threadState,
                        new MacroCommandsExecutorBuilder(),
                        6
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


    public class CashShopOrchestratorSystem : AbstractOrchestratorSystem
    {
        public CashShopOrchestratorSystem(
            List<AbstractThreadFactory> threadFactories
        ) : base(threadFactories)
        { }
    }


    public class CashShopOrchestratorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new CashShopOrchestratorSystem(
                [new CashShopOrchestratorThreadFactory()]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
