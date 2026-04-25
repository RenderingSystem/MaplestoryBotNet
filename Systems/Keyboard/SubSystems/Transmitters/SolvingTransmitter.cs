using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using System.Collections.Concurrent;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters
{
    public enum SolvingOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }

    public enum SolvingExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        Solved,
        MaxNum
    }


    public class SolvingExecutorThread : AbstractThread
    {
        public SolvingExecutorThread(AbstractThreadRunningState runningState) : base(runningState)
        {
        }

        public override void ThreadLoop()
        {
        }

        public override void Inject(object dataType, object? value)
        {
        }

        public override object? State()
        {
            return (int)SolvingExecutorThreadedUpdate.Stopped;
        }
    }


    public class SolvingOrchestratorThread : AbstractThread
    {
        private AbstractThread _solvingExecutorThread;

        private BlockingCollection<int> _threadStates;

        public SolvingOrchestratorThread(
            AbstractThread solvingExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(runningState)
        {
            _solvingExecutorThread = solvingExecutorThread;
            _threadStates = threadStates;
        }

        public override void Start()
        {
            _solvingExecutorThread.Start();
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
            _solvingExecutorThread.Stop();
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
                    _solvingExecutorThread.Inject(
                        (SolvingOrchestratorThreadInjectType)
                        threadState, null
                    );
                }
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is SolvingOrchestratorThreadInjectType injectType)
            {
                _threadStates.Add((int)injectType);
            }
            else
            {
                _solvingExecutorThread.Inject(dataType, value);
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
            return _solvingExecutorThread.State();
        }
    }


    public class SolvingOrchestratorThreadFactory : AbstractThreadFactory
    {
        private string _characterKey;

        private string _runeKey;

        public SolvingOrchestratorThreadFactory(
            string characterKey, string runeKey
        )
        {
            _characterKey = characterKey;
            _runeKey = runeKey;
        }

        public override AbstractThread CreateThread()
        {
            return new SolvingOrchestratorThread(
                new SolvingExecutorThread(new ThreadRunningState()),
                new ThreadRunningState(),
                new BlockingCollection<int>()
            );
        }
    }


    public class SolvingOrchestratorSystem : AbstractSystem
    {
        private List<AbstractThreadFactory> _threadFactories;

        private List<AbstractThread> _threads;

        public SolvingOrchestratorSystem(
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


    public class SolvingOrchestratorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new SolvingOrchestratorSystem(
                [new SolvingOrchestratorThreadFactory(MapIconInfo.Character, MapIconInfo.Rune)]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
