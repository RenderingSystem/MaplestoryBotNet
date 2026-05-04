using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.ProcessWatchdog;
using MaplestoryBotNetTests.TestHelpers;
using System.Net.Http;


namespace MaplestoryBotNetTests.Systems.ProcessWatchdog.Tests.Mocks
{
    public class MockRuneSolverClient : AbstractRuneSolverClient
    {
        public List<string> CallOrder = [];

        public int GetCalls = 0;
        public int GetIndex = 0;
        public List<string> GetCallArg_url = [];
        public List<int> GetCallArg_timeoutMilliseconds = [];
        public List<HttpResponseMessage?> GetReturn = [];
        public override HttpResponseMessage? Get(
            string url, int timeoutMilliseconds
        )
        {
            var callReference = new TestUtilities().Reference(this) + "Get";
            CallOrder.Add(callReference);
            GetCalls++;
            GetCallArg_url.Add(url);
            GetCallArg_timeoutMilliseconds.Add(timeoutMilliseconds);
            if (GetIndex < GetReturn.Count)
            {
                return GetReturn[GetIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }



    public class MockRuneSolverPinger : AbstractRuneSolverPinger
    {
        public List<string> CallOrder = [];

        public int PingCalls = 0;
        public int PingIndex = 0;
        public List<RuneDetection> PingCallArg_runeDetection = [];
        public List<int> PingCallArg_clientWatchdogTimeout = [];
        public List<HttpResponseMessage?> PingReturn = [];
        public override HttpResponseMessage? Ping(
            RuneDetection runeDetection,
            int clientWatchdogTimeout
        )
        {
            var callReference = new TestUtilities().Reference(this) + "Ping";
            CallOrder.Add(callReference);
            PingCalls++;
            PingCallArg_runeDetection.Add(runeDetection);
            PingCallArg_clientWatchdogTimeout.Add(clientWatchdogTimeout);
            if (PingIndex < PingReturn.Count)
            {
                return PingReturn[PingIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockRuneSolverController : AbstractRuneSolverController
    {
        public List<string> CallOrder = [];

        public int StartCalls = 0;
        public int StartIndex = 0;
        public List<RuneServerSettings> StartCallArg_runeServerSettings = [];
        public List<RuneDetection> StartCallArg_runeDetection = [];
        public override void Start(
            RuneServerSettings runeServerSettings,
            RuneDetection runeDetection
        )
        {
            var callReference = new TestUtilities().Reference(this) + "Start";
            CallOrder.Add(callReference);
            StartCalls++;
            StartCallArg_runeServerSettings.Add(runeServerSettings);
            StartCallArg_runeDetection.Add(runeDetection);
        }

        public int PurgeCalls = 0;
        public List<RuneServerSettings> PurgeCallArg_runeServerSettings = [];
        public override void Purge(
            RuneServerSettings runeServerSettings
        )
        {
            var callReference = new TestUtilities().Reference(this) + "Purge";
            CallOrder.Add(callReference);
            PurgeCalls++;
            PurgeCallArg_runeServerSettings.Add(runeServerSettings);
        }
    }


    public class MockRuneSolverWatchdogTimer : AbstractRuneSolverWatchdogTimer
    {
        public List<string> CallOrder = [];

        public int SetStopwatchCalls = 0;
        public override void SetStopwatch()
        {
            var callReference = new TestUtilities().Reference(this) + "SetStopwatch";
            CallOrder.Add(callReference);
            SetStopwatchCalls++;
        }

        public int SleepRemainingCalls = 0;
        public List<RuneServerSettings> SleepRemainingCallArg_runeServerSettings = [];
        public override void SleepRemaining(
            RuneServerSettings runeServerSettings
        )
        {
            var callReference = new TestUtilities().Reference(this) + "SleepRemaining";
            CallOrder.Add(callReference);
            SleepRemainingCalls++;
            SleepRemainingCallArg_runeServerSettings.Add(runeServerSettings);
        }
    }


    public class MockRuneSolverSupervisor : AbstractRuneSolverSupervisor
    {
        public List<string> CallOrder = [];

        public int EnsureRunningCalls = 0;
        public List<RuneDetection> EnsureRunningCallArg_runeDetection = [];
        public List<RuneServerSettings> EnsureRunningCallArg_runeServerSettings = [];
        public override void EnsureRunning(
            RuneDetection runeDetection,
            RuneServerSettings runeServerSettings
        )
        {
            var callReference = new TestUtilities().Reference(this) + "EnsureRunning";
            CallOrder.Add(callReference);
            EnsureRunningCalls++;
            EnsureRunningCallArg_runeDetection.Add(runeDetection);
            EnsureRunningCallArg_runeServerSettings.Add(runeServerSettings);
        }
    }


    public class MockRuneSolverHealthMonitor : AbstractRuneSolverHealthMonitor
    {
        public List<string> CallOrder = [];

        public int NeedsLaunchCalls = 0;
        public int NeedsLaunchIndex = 0;
        public List<RuneDetection> NeedsLaunchCallArg_runeDetection = [];
        public List<RuneServerSettings> NeedsLaunchCallArg_runeServerSettings = [];
        public List<bool> NeedsLaunchReturn = [];
        public override bool NeedsLaunch(
            RuneDetection runeDetection,
            RuneServerSettings runeServerSettings
        )
        {
            var callReference = new TestUtilities().Reference(this) + "NeedsLaunch";
            CallOrder.Add(callReference);
            NeedsLaunchCalls++;
            NeedsLaunchCallArg_runeDetection.Add(runeDetection);
            NeedsLaunchCallArg_runeServerSettings.Add(runeServerSettings);
            if (NeedsLaunchIndex < NeedsLaunchReturn.Count)
            {
                return NeedsLaunchReturn[NeedsLaunchIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }
}
