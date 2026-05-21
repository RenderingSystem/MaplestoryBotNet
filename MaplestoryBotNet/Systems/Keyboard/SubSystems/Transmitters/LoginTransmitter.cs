using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using System.Collections.Concurrent;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters
{
    public enum LoginOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }


    public enum LoginExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        TimedOut,
        MaxNum
    }


    public class LoginScreenExecutorThreadHelper : AbstractKeystrokeTransmitterThreadHelper
    {
        private AbstractTimestamp _loginStopwatch;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        private int _loginTimeout;

        private bool _loggedIn;

        public LoginScreenExecutorThreadHelper(
            AbstractTimestamp loginStopwatch,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractMacroCommandsExecutorBuilder executorBuilder
        )
        {
            _loginStopwatch = loginStopwatch;
            _threadState = threadState;
            _macroCommandsExecutorBuilder = executorBuilder;
            _macroCommandsExecutor = null;
            _loginTimeout = 0;
            _loggedIn = true;
        }

        public override void Reset()
        {
            _loginStopwatch.SetTimestamp();
        }

        public override bool Transmit()
        {
            if (_macroCommandsExecutor == null)
            {
                return true;
            }
            var timestamp = _loginStopwatch.GetTimestamp();
            if (_loggedIn)
            {
                _macroCommandsExecutor.Execute(
                    [
                        "key press {ESCAPE} {100} {200}",
                        "wait {350} {350}",
                        "key press {ARROW_UP} {100} {200}",
                        "wait {350} {350}",
                        "key press {ENTER} {100} {200}",
                        "wait {350} {350}",
                        "key press {ENTER} {100} {200}",
                        "wait {350} {350}"
                    ]
                );
                _loggedIn = false;
            }
            else if (timestamp > _loginTimeout)
            {
                _macroCommandsExecutor.Execute(["key press {ENTER} {100} {200}"]);
                _threadState.SetState((int)LoginExecutorThreadedUpdate.TimedOut);
                _loggedIn = true;
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
                _loginTimeout = maplestoryBotConfiguration.MacroSettings.LoginTimeout;
            }
        }
    }


    public class LoginExecutorThread : AbstractThread
    {
        private AbstractResetEvent _executionEvent;

        private AbstractKeystrokeTransmitterThreadHelper _loginExecutorThreadHelper;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractThreadRunningState _transmittingState;

        public LoginExecutorThread(
            AbstractResetEvent executionEvent,
            AbstractKeystrokeTransmitterThreadHelper loginExecutorThreadHelpers,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractThreadRunningState transmittingState,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _executionEvent = executionEvent;
            _loginExecutorThreadHelper = loginExecutorThreadHelpers;
            _threadState = threadState;
            _transmittingState = transmittingState;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _executionEvent.WaitOne();
                _transmittingState.SetRunning(true);
                _loginExecutorThreadHelper.Reset();
                while (_threadState.GetState() == (int)LoginExecutorThreadedUpdate.Started)
                {
                    if (!_loginExecutorThreadHelper.Transmit())
                    {
                        break;
                    }
                }
                _loginExecutorThreadHelper.Reset();
                _transmittingState.SetRunning(false);
            }
        }

        public override void Stop()
        {
            base.Stop();
            Inject(LoginOrchestratorThreadInjectType.Stop, null);
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is LoginOrchestratorThreadInjectType injectType)
            {
                if (injectType == LoginOrchestratorThreadInjectType.Start)
                {
                    _threadState.SetState((int)LoginExecutorThreadedUpdate.Starting);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)LoginExecutorThreadedUpdate.Started);
                    _executionEvent.Set();
                }
                else if (injectType == LoginOrchestratorThreadInjectType.Stop)
                {
                    _threadState.SetState((int)LoginExecutorThreadedUpdate.Stopping);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)LoginExecutorThreadedUpdate.Stopped);
                }
            }
            else
            {
                _loginExecutorThreadHelper.Inject(dataType, value);
            }
        }

        public override object? State()
        {
            return _threadState;
        }
    }


    public class LoginOrchestratorThread : AbstractOrchestratorThread<LoginOrchestratorThreadInjectType>
    {
        public LoginOrchestratorThread(
            AbstractThread loginExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(loginExecutorThread, runningState, threadStates)
        { }
    }


    public class LoginOrchestratorThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                (int)LoginExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Login
            );
            return new LoginOrchestratorThread(
                new LoginExecutorThread(
                    new ExecutionEvent(),
                    new LoginScreenExecutorThreadHelper(
                        new StopwatchTimestamp(),
                        threadState,
                        new MacroCommandsExecutorBuilder()
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


    public class LoginOrchestratorSystem : AbstractOrchestratorSystem
    {
        public LoginOrchestratorSystem(
            List<AbstractThreadFactory> threadFactories
        ) : base(threadFactories)
        { }
    }


    public class LoginOrchestratorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new LoginOrchestratorSystem(
                [new LoginOrchestratorThreadFactory()]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
