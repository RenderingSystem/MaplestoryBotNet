using MaplestoryBotNet.ThreadingUtils;
using System.Collections.Concurrent;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters
{
    public abstract class AbstractOrchestratorThread<TInjectType> : AbstractThread
        where TInjectType : Enum
    {
        protected AbstractThread _executorThread;
        protected BlockingCollection<int> _threadStates;

        protected AbstractOrchestratorThread(
            AbstractThread executorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(runningState)
        {
            _executorThread = executorThread;
            _threadStates = threadStates;
        }

        public override void Start()
        {
            _executorThread.Start();
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
            _executorThread.Stop();
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
                    var injectType = (TInjectType)Enum.ToObject(typeof(TInjectType), threadState);
                    _executorThread.Inject(injectType, null);
                }
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is TInjectType injectType)
            {
                _threadStates.Add(Convert.ToInt32(injectType));
            }
            else
            {
                _executorThread.Inject(dataType, value);
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
            return _executorThread.State();
        }
    }


    public abstract class AbstractOrchestratorSystem : AbstractSystem
    {
        protected List<AbstractThreadFactory> _threadFactories;
        protected List<AbstractThread> _threads;

        protected AbstractOrchestratorSystem(List<AbstractThreadFactory> threadFactories)
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
}
