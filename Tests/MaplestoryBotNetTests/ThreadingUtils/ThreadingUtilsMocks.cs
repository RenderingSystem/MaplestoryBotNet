using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.TestHelpers;
using System.Collections.Concurrent;
using System.Diagnostics;


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
            var callReference = new TestUtilities().Reference(this) + "IsRunning";
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
            var callReference = new TestUtilities().Reference(this) + "SetRunning";
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
            var callReference = new TestUtilities().Reference(this) + "CreateThread";
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
            var callReference = new TestUtilities().Reference(this) + "ThreadLoop";
            CallOrder.Add(callReference);
            ThreadLoopCalls++;
        }

        public int ThreadStartCalls = 0;
        public bool ThreadStartSpy = false;
        public override void Start()
        {
            var callReference = new TestUtilities().Reference(this) + "ThreadStart";
            CallOrder.Add(callReference);
            ThreadStartCalls++;
            if (ThreadStartSpy)
                base.Start();
        }

        public int ThreadStopCalls = 0;
        public bool ThreadStopSpy = false;
        public override void Stop()
        {
            var callReference = new TestUtilities().Reference(this) + "ThreadStop";
            CallOrder.Add(callReference);
            ThreadStopCalls++;
            if (ThreadStopSpy)
                base.Stop();
        }

        public int ThreadJoinCalls = 0;
        public int ThreadJoinIndex = 0;
        public bool ThreadJoinSpy = false;
        public List<int> ThreadJoinCallArg_milliseconds = [];
        public List<bool> ThreadJoinReturn = [];
        public override bool Join(int milliseconds)
        {
            var callReference = new TestUtilities().Reference(this) + "ThreadJoin";
            CallOrder.Add(callReference);
            ThreadJoinCalls++;
            ThreadJoinCallArg_milliseconds.Add(milliseconds);
            if (ThreadJoinSpy)
                return base.Join(milliseconds);
            else if (ThreadJoinIndex < ThreadJoinReturn.Count)
                return ThreadJoinReturn[ThreadJoinIndex++];
            else
                throw new IndexOutOfRangeException();
        }

        public int ThreadResultCalls = 0;
        public int ThreadResultIndex = 0;
        public List<object?> ThreadResultReturn = [];
        public override object? Result()
        {
            var callReference = new TestUtilities().Reference(this) + "ThreadResult";
            CallOrder.Add(callReference);
            ThreadResultCalls++;
            if (ThreadResultIndex < ThreadResultReturn.Count)
                return ThreadResultReturn[ThreadResultIndex++];
            return new IndexOutOfRangeException();
        }

        public int InjectCalls = 0;
        public List<object> InjectCallArg_dataType = [];
        public List<object?> InjectCallArg_data = [];
        public override void Inject(object dataType, object? value)
        {
            var callReference = new TestUtilities().Reference(this) + "ThreadInject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(value);
        }

        public int ThreadStateCalls = 0;
        public int ThreadStateIndex = 0;
        public List<object?> ThreadStateReturn = [];
        public override object? State()
        {
            var callReference = new TestUtilities().Reference(this) + "ThreadState";
            CallOrder.Add(callReference);
            ThreadStateCalls++;
            if (ThreadStateIndex < ThreadStateReturn.Count)
                return ThreadStateReturn[ThreadStateIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockExecutionFlag : AbstractExecutionFlag
    {
        public List<string> CallOrder = [];

        public int FlagCalls = 0;
        public override void Flag()
        {
            var callReference = new TestUtilities().Reference(this) + "Flag";
            CallOrder.Add(callReference);
            FlagCalls++;
        }

        public int FlaggedCalls = 0;
        public int FlaggedIndex = 0;
        public List<bool> FlaggedReturn = [];
        public override bool Flagged()
        {
            var callReference = new TestUtilities().Reference(this) + "Flagged";
            CallOrder.Add(callReference);
            FlagCalls++;
            if (FlaggedIndex < FlaggedReturn.Count)
                return FlaggedReturn[FlaggedIndex++];
            throw new IndexOutOfRangeException();
        }

        public int UnflagCalls = 0;
        public override void Unflag()
        {
            var callReference = new TestUtilities().Reference(this) + "Unflag";
            CallOrder.Add(callReference);
            UnflagCalls++;
        }

        public int WaitCalls = 0;
        public override void Wait()
        {
            var callReference = new TestUtilities().Reference(this) + "Wait";
            CallOrder.Add(callReference);
            WaitCalls++;
        }
    }


    public class MockTimestamp : AbstractTimestamp
    {
        public List<string> CallOrder = [];

        public int GetTimestampCalls = 0;
        public int GetTimestampIndex = 0;
        public List<double> GetTimestampReturn = [];
        public override double GetTimestamp()
        {
            var callReference = new TestUtilities().Reference(this) + "GetTimestamp";
            CallOrder.Add(callReference);
            GetTimestampCalls++;
            if (GetTimestampIndex < GetTimestampReturn.Count)
            {
                return GetTimestampReturn[GetTimestampIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int SetTimestampCalls = 0;
        public override void SetTimestamp()
        {
            var callReference = new TestUtilities().Reference(this) + "SetTimestamp";
            CallOrder.Add(callReference);
            SetTimestampCalls++;
        }
    }


    public class MockResetEvent : AbstractResetEvent
    {
        public List<string> CallOrder = [];

        public int SetCalls = 0;
        public override void Set()
        {
            var callReference = new TestUtilities().Reference(this) + "Set";
            CallOrder.Add(callReference);
            SetCalls++;
        }

        public int WaitOneCalls = 0;
        public override void WaitOne()
        {
            var callReference = new TestUtilities().Reference(this) + "WaitOne";
            CallOrder.Add(callReference);
            WaitOneCalls++;
        }
    }


    public class MockProcessName : AbstractProcessName
    {
        public List<string> CallOrder = [];

        public int GetProcessNameCalls = 0;
        public int GetProcessNameIndex = 0;
        public List<string> GetProcessNameReturn = [];
        public override string GetProcessName()
        {
            var callReference = new TestUtilities().Reference(this) + "GetProcessName";
            CallOrder.Add(callReference);
            GetProcessNameCalls++;
            if (GetProcessNameIndex < GetProcessNameReturn.Count)
            {
                return GetProcessNameReturn[GetProcessNameIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockProcessLauncher : AbstractProcessStarter
    {
        public List<string> CallOrder = [];

        public int LaunchCalls = 0;
        public List<ProcessStartInfo> LaunchCallArg_startInfo = [];
        public override void Start(ProcessStartInfo startInfo)
        {
            var callReference = new TestUtilities().Reference(this) + "Launch";
            CallOrder.Add(callReference);
            LaunchCalls++;
            LaunchCallArg_startInfo.Add(startInfo);
        }
    }


    public class MockProcessMonitor : AbstractProcessMonitor
    {
        public List<string> CallOrder = [];

        public int RunningCalls = 0;
        public int RunningIndex = 0;
        public List<string> RunningCallArg_processName = [];
        public List<List<AbstractProcess>> RunningReturn = [];
        public override List<AbstractProcess> Running(string processName)
        {
            var callReference = new TestUtilities().Reference(this) + "Running";
            CallOrder.Add(callReference);
            RunningCallArg_processName.Add(processName);
            RunningCalls++;
            if (RunningIndex < RunningReturn.Count)
            {
                return RunningReturn[RunningIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockProcess : AbstractProcess
    {
        public List<string> CallOrder = [];

        public int KillCalls = 0;
        public List<int> KillCallArg_waitMilliseconds = []; 
        public override void Kill(int waitMilliseconds)
        {
            var callReference = new TestUtilities().Reference(this) + "Kill";
            CallOrder.Add(callReference);
            KillCalls++;
            KillCallArg_waitMilliseconds.Add(waitMilliseconds);
        }
    }


    public class MockCompositionEventHandler : AbstractCompositionEventHandler
    {
        public List<string> CallOrder = [];

        public int StartCalls = 0;

        public override void Start()
        {
            var callReference = new TestUtilities().Reference(this) + "Start";
            CallOrder.Add(callReference);
            StartCalls++;
        }

        public int StopCalls = 0;
        public override void Stop()
        {
            var callReference = new TestUtilities().Reference(this) + "Stop";
            CallOrder.Add(callReference);
            StopCalls++;
        }

        public int EventHandlerCalls = 0;
        public List<EventHandler> EventHandlerCallArg_tickAction = [];
        public override void EventHandler(EventHandler tickAction)
        {
            var callReference = new TestUtilities().Reference(this) + "Stop";
            CallOrder.Add(callReference);
            EventHandlerCalls++;
            EventHandlerCallArg_tickAction.Add(tickAction);
        }
    }


    public class MockThreadsBuilder : AbstractThreadsBuilder
    {
        public List<string> CallOrder = [];

        public int BuildCalls = 0;
        public int BuildIndex = 0;
        public List<ConcurrentDictionary<string, AbstractThread>> BuildReturn = [];
        public override ConcurrentDictionary<string, AbstractThread>? Build()
        {
            var callReference = new TestUtilities().Reference(this) + "Build";
            CallOrder.Add(callReference);
            BuildCalls++;
            if (BuildIndex < BuildReturn.Count)
            {
                return BuildReturn[BuildIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int WithArgCalls = 0;
        public List<object?> WithArgCallArg_arg = [];
        public override AbstractThreadsBuilder WithArg(object? arg)
        {
            var callReference = new TestUtilities().Reference(this) + "WithArg";
            CallOrder.Add(callReference);
            WithArgCalls++;
            WithArgCallArg_arg.Add(arg);
            return this;
        }
    }
}
