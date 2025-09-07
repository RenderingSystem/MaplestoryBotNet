using MaplestoryBotNet.ThreadingUtils;


namespace MaplestoryBotNetTests.ThreadingUtils
{
    public class MockRunningState : AbstractThreadRunningState
    {
        public List<string> CallOrder = [];

        public int IsRunningCalls = 0;
        public int IsRunningIndex = 0;
        public List<bool> IsRunningReturn = [];
        public override bool IsRunning()
        {
            var callReference = new TestUtils().Reference(this) + "IsRunning";
            CallOrder.Add(callReference);
            IsRunningCalls++;
            if (IsRunningIndex < IsRunningReturn.Count)
                return IsRunningReturn[IsRunningIndex++];
            throw new IndexOutOfRangeException();
        }

        public int SetRunningCalls = 0;
        public List<bool> SetRunningCallArg_running = [];
        public override void SetRunning(bool running)
        {
            var callReference = new TestUtils().Reference(this) + "SetRunning";
            CallOrder.Add(callReference);
            SetRunningCalls++;
            SetRunningCallArg_running.Add(running);
        }
    }


    public class MockThreadFactory : AbstractThreadFactory
    {
        public List<string> CallOrder = [];

        public int CreateThreadCalls = 0;
        public int CreateThreadIndex = 0;
        public List<AbstractThread> CreateThreadReturn = [];
        public override AbstractThread CreateThread()
        {
            var callReference = new TestUtils().Reference(this) + "CreateThread";
            CallOrder.Add(callReference);
            CreateThreadCalls++;
            if (CreateThreadIndex < CreateThreadReturn.Count)
                return CreateThreadReturn[CreateThreadIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockThread : AbstractThread
    {
        public List<string> CallOrder = [];

        public MockThread(AbstractThreadRunningState runningState) : base(runningState)
        {

        }

        public int ThreadLoopCalls = 0;
        public override void ThreadLoop()
        {
            var callReference = new TestUtils().Reference(this) + "ThreadLoop";
            CallOrder.Add(callReference);
            ThreadLoopCalls++;
        }

        public int ThreadStartCalls = 0;
        public bool ThreadStartSpy = false;
        public override void ThreadStart()
        {
            var callReference = new TestUtils().Reference(this) + "ThreadStart";
            CallOrder.Add(callReference);
            ThreadStartCalls++;
            if (ThreadStartSpy)
                base.ThreadStart();
        }

        public int ThreadStopCalls = 0;
        public bool ThreadStopSpy = false;
        public override void ThreadStop()
        {
            var callReference = new TestUtils().Reference(this) + "ThreadStop";
            CallOrder.Add(callReference);
            ThreadStopCalls++;
            if (ThreadStopSpy)
                base.ThreadStop();
        }

        public int ThreadJoinCalls = 0;
        public int ThreadJoinIndex = 0;
        public bool ThreadJoinSpy = false;
        public List<int> ThreadJoinCallArg_milliseconds = [];
        public List<bool> ThreadJoinReturn = [];
         public override bool ThreadJoin(int milliseconds)
        {
            var callReference = new TestUtils().Reference(this) + "ThreadJoin";
            CallOrder.Add(callReference);
            ThreadJoinCalls++;
            ThreadJoinCallArg_milliseconds.Add(milliseconds);
            if (ThreadJoinSpy)
                return base.ThreadJoin(milliseconds);
            else if (ThreadJoinIndex < ThreadJoinReturn.Count)
                return ThreadJoinReturn[ThreadJoinIndex++];
            else
                throw new IndexOutOfRangeException();
        }
    }

}
