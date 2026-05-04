

using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using System.Collections.Concurrent;

namespace MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters
{
    // TODO
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


    public class CashShopExecutorThread : AbstractThread
    {
        public CashShopExecutorThread(
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
        }

        public override void ThreadLoop()
        {
            throw new NotImplementedException();
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
            return new CashShopOrchestratorThread(
                new CashShopExecutorThread(
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
