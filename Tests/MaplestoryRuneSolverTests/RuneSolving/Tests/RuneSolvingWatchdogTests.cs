using MaplestoryRuneSolver.RuneSolving;
using MaplestoryRuneSolverTests.RuneSolving.Mocks;
using MaplestoryRuneSolverTests.TestHelpers;
using System.Diagnostics;


namespace MaplestoryRuneSolverTests.RuneSolving.Tests
{
    public class RuneSolvingWatchdogTests
    {
        private MockTimestamp _timestamp = new MockTimestamp();

        private MockProcessMonitor _processMonitor = new MockProcessMonitor();

        private MockThreadSleeper _sleeper = new MockThreadSleeper();

        private List<string> _callOrder = new List<string>();

        private RuneSolvingWatchdog _fixture(int tolerance = 3)
        {
            _processMonitor = new MockProcessMonitor();
            _sleeper = new MockThreadSleeper();
            _callOrder = new List<string>();
            return new RuneSolvingWatchdog(
                _processMonitor, _sleeper, _timestamp, tolerance
            );
        }

        /**
         * @brief Verifies that the watchdog continuously monitors the main botting application
         * and only stops when the bot is no longer running
         * 
         * When users launch the rune solver service alongside the main botting application,
         * the watchdog must repeatedly check if the bot is still alive. As long as the
         * bot is running, the watchdog continues waiting. Once the bot stops (e.g., user
         * closes the bot or it crashes), the watchdog stops monitoring, allowing the
         * rune solver to shut itself down since it's no longer needed.
         */
        private void _testWatchdogRunsUntilProcessIsDead()
        {
            for (int i = 1; i < 10; i++)
            {
                var runeSolvingWatchdog = _fixture();
                var processMonitorRef = new TestUtilities().Reference(_processMonitor);
                var sleeperRef = new TestUtilities().Reference(_sleeper);
                _processMonitor.CallOrder = _callOrder;
                _sleeper.CallOrder = _callOrder;
                for (int j = 0; j < i; j++)
                {
                    _processMonitor.RunningReturn.Add(1);
                    _timestamp.GetTimestampReturn.Add(0);
                }
                _processMonitor.RunningReturn.Add(0);
                runeSolvingWatchdog.Start("12", 34);
                Debug.Assert(_callOrder.Count == (i * 2) + 1);
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(_callOrder[j * 2] == processMonitorRef + "Running");
                    Debug.Assert(_callOrder[(j * 2) + 1] == sleeperRef + "Sleep");
                }
                Debug.Assert(_callOrder[i * 2] == processMonitorRef + "Running");
            }

        }

        /**
         * @brief Verifies that the watchdog checks the correct process name for the bot
         * 
         * When users specify which bot process to monitor the watchdog must look for
         * that exact process name on every status check. This ensures the rune solver
         * only stays alive while the bot is actually running, not some other unrelated
         * process.
         */
        private void _testWatchdogChecksWatchdogProcess()
        {
            for (int i = 1; i < 10; i++)
            {
                var runeSolvingWatchdog = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _processMonitor.RunningReturn.Add(1);
                    _timestamp.GetTimestampReturn.Add(0);
                }
                _processMonitor.RunningReturn.Add(0);
                runeSolvingWatchdog.Start("12", 34);
                Debug.Assert(_processMonitor.RunningCalls == i + 1);
                for (int j = 0; j < i + 1; j++)
                {
                    Debug.Assert(_processMonitor.RunningCallArg_processName[j] == "12");
                }
            }
        }

        /**
         * @brief Verifies that the watchdog respects the configured sleep interval between
         * checks to avoid excessive CPU usage
         * 
         * When users configure a watchdog timeout (e.g., 34 milliseconds), the watchdog
         * must wait that long between each check of the bot's status. This prevents the
         * watchdog from consuming too much CPU by checking hundreds of times per second,
         * while still responding quickly when the bot stops.
         */
        private void _testWatchdogSleep()
        {
            for (int i = 1; i < 10; i++)
            {
                var runeSolvingWatchdog = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _processMonitor.RunningReturn.Add(1);
                    _timestamp.GetTimestampReturn.Add(0);
                }
                _processMonitor.RunningReturn.Add(0);
                runeSolvingWatchdog.Start("12", 34);
                Debug.Assert(_sleeper.SleepCalls == i);
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(_sleeper.SleepCallArg_milliseconds[j] == 34);
                }
            }
        }

        /**
         * @brief Verifies that the watchdog uses a tolerance threshold to determine when
         * the monitored process is unresponsive, rather than failing on a single missed check
         * 
         * When users are botting and the main application becomes temporarily unresponsive
         * (e.g., due to high CPU usage, loading screens, or network latency), the rune solver
         * should not shut down immediately. Instead, the watchdog uses a tolerance threshold
         * that allows multiple consecutive failures before considering the process dead.
         */
        private void _testWatchdogTimeoutOnThreshold()
        {
            for (int i = 1; i < 10; i++)
            for (int j = 1; j < 10; j++)
            {
                var runeSolvingWatchdog = _fixture(j);
                for (int k = 0; k < i; k++)
                {
                    _processMonitor.RunningReturn.Add(1);
                    _timestamp.GetTimestampReturn.Add(0);
                }
                for (int k = 2; k >= 0; k--)
                {
                    _processMonitor.RunningReturn.Add(1);
                    _timestamp.GetTimestampReturn.Add((j * 34 - k) / 1000.0);
                }
                runeSolvingWatchdog.Start("12", 34);
                Debug.Assert(_sleeper.SleepCalls == i + 2);
                for (int k = 0; k < i + 2; k++)
                {
                    Debug.Assert(_sleeper.SleepCallArg_milliseconds[k] == 34);
                }
            }
        }

        public void Run()
        {
            _testWatchdogRunsUntilProcessIsDead();
            _testWatchdogChecksWatchdogProcess();
            _testWatchdogSleep();
            _testWatchdogTimeoutOnThreshold();
        }
    }


    public class RuneSolvingWatchdogTestSuite
    {
        public void Run()
        {
            new RuneSolvingWatchdogTests().Run();
        }
    }
}
