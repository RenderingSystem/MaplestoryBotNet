using MaplestoryRuneSolver.RuneSolving;
using MaplestoryRuneSolverTests.TestHelpers;
using Microsoft.AspNetCore.Builder;


namespace MaplestoryRuneSolverTests.RuneSolving.Mocks
{
    public class MockProcessName : AbstractProcessName
    {
        public List<string> CallOrder = new List<string>();

        public int GetProcessNameCalls = 0;
        public int GetProcessNameIndex = 0;
        public List<string> GetProcessNameReturn = new List<string>();
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


    public class MockProcessMonitor : AbstractProcessMonitor
    {
        public List<string> CallOrder = new List<string>();

        public int RunningCalls = 0;
        public int RunningIndex = 0;
        public List<string> RunningCallArg_processName = new List<string>();
        public List<int> RunningReturn = new List<int>();
        public override int Running(string processName)
        {
            var callReference = new TestUtilities().Reference(this) + "Running";
            CallOrder.Add(callReference);
            RunningCalls++;
            RunningCallArg_processName.Add(processName);
            if (RunningIndex < RunningReturn.Count)
            {
                return RunningReturn[RunningIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockWebAppHost : AbstractWebAppHost
    {
        public List<string> CallOrder = new List<string>();

        public int StartCalls = 0;
        public int StartIndex = 0;
        public List<string> StartCallArg_url = new List<string>();
        public List<bool> StartReturn = new List<bool>();
        public override bool Start(string url)
        {
            var callReference = new TestUtilities().Reference(this) + "Start";
            CallOrder.Add(callReference);
            StartCalls++;
            StartCallArg_url.Add(url);
            if (StartIndex < StartReturn.Count)
            {
                return StartReturn[StartIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int StopCalls = 0;
        public override void Stop()
        {
            var callReference = new TestUtilities().Reference(this) + "Stop";
            CallOrder.Add(callReference);
            StopCalls++;
        }

        public int WebAppCalls = 0;
        public int WebAppIndex = 0;
        public List<WebApplication?> WebAppReturn = [];
        public override WebApplication? WebApp()
        {
            var callReference = new TestUtilities().Reference(this) + "WebApp";
            CallOrder.Add(callReference);
            if (WebAppIndex < WebAppReturn.Count)
            {
                return WebAppReturn[WebAppIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockWatchdog : AbstractWatchdog
    {
        public List<string> CallOrder = new List<string>();

        public int StartCalls = 0;
        public List<string> StartCallArg_watchProcessName = new List<string>();
        public List<int> StartCallArg_watchdogTimeout = new List<int>();
        public override void Start(string watchProcessName, int watchdogTimeout)
        {
            var callReference = new TestUtilities().Reference(this) + "Start";
            CallOrder.Add(callReference);
            StartCalls++;
            StartCallArg_watchProcessName.Add(watchProcessName);
            StartCallArg_watchdogTimeout.Add(watchdogTimeout);
        }
    }


    public class MockThreadSleeper : AbstractThreadSleeper
    {
        public List<string> CallOrder = new List<string>();

        public int SleepCalls = 0;
        public List<int> SleepCallArg_milliseconds = new List<int>();
        public override void Sleep(int milliseconds)
        {
            var callReference = new TestUtilities().Reference(this) + "Sleep";
            CallOrder.Add(callReference);
            SleepCallArg_milliseconds.Add(milliseconds);
            SleepCalls++;
        }
    }


    public class MockTimestamp : AbstractTimestamp
    {
        public List<string> CallOrder = [];

        public int GetTimestampCalls = 0;
        public List<double> GetTimestampReturn = [];
        public override double GetTimestamp()
        {
            var callReference = new TestUtilities().Reference(this) + "GetTimestamp";
            CallOrder.Add(callReference);
            GetTimestampCalls++;
            if (GetTimestampCalls - 1 < GetTimestampReturn.Count)
            {
                return GetTimestampReturn[GetTimestampCalls - 1];
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
}
