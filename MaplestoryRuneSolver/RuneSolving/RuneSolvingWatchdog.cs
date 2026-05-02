using System.Diagnostics;


namespace MaplestoryRuneSolver.RuneSolving
{
    public abstract class AbstractWatchdog
    {
        public abstract void Start(string watchProcessName, int watchdogTimeout);
    }


    public abstract class AbstractProcessMonitor
    {
        public abstract int Running(string processName);
    }


    public abstract class AbstractThreadSleeper
    {
        public abstract void Sleep(int milliseconds);
    }


    public abstract class AbstractProcessName
    {
        public abstract string GetProcessName();
    }


    public abstract class AbstractTimestamp
    {
        public abstract double GetTimestamp();

        public abstract void SetTimestamp();
    }


    public class StopwatchTimestamp : AbstractTimestamp
    {
        private long _startTicks;

        private double _ticksToSeconds;

        private bool _isSet;

        public StopwatchTimestamp()
        {
            _startTicks = 0;
            _ticksToSeconds = 1.0 / Stopwatch.Frequency;
            _isSet = false;
        }

        public override double GetTimestamp()
        {
            if (!_isSet)
            {
                return double.PositiveInfinity;
            }
            long currentTicks = Stopwatch.GetTimestamp();
            long elapsedTicks = currentTicks - _startTicks;
            return elapsedTicks * _ticksToSeconds;
        }

        public override void SetTimestamp()
        {
            _startTicks = Stopwatch.GetTimestamp();
            _isSet = true;
        }
    }


    public class ProcessMonitor : AbstractProcessMonitor
    {
        public override int Running(string processName)
        {
            return Process.GetProcessesByName(processName).Length;
        }
    }


    public class ThreadSleeper : AbstractThreadSleeper
    {
        public override void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }
    }


    public class ProcessName : AbstractProcessName
    {
        public override string GetProcessName()
        {
            var currentProcess = Process.GetCurrentProcess();
            return currentProcess.ProcessName;
        }
    }


    public class RuneSolvingWatchdog : AbstractWatchdog
    {
        private AbstractProcessMonitor _processMonitor;

        private AbstractThreadSleeper _threadSleeper;

        private AbstractTimestamp _pingStopwatch;

        private int _missedPingTolerance;

        public RuneSolvingWatchdog(
            AbstractProcessMonitor processMonitor,
            AbstractThreadSleeper threadSleeper,
            AbstractTimestamp pingStopwatch,
            int missedPingTolerance
        )
        {
            _processMonitor = processMonitor;
            _threadSleeper = threadSleeper;
            _pingStopwatch = pingStopwatch;
            _missedPingTolerance = missedPingTolerance;
        }

        public override void Start(
            string watchProcessName, int watchdogTimeout
        )
        {
            var tolerance = _missedPingTolerance * watchdogTimeout / 1000.0;
            while (
                _processMonitor.Running(watchProcessName) > 0
                && tolerance > _pingStopwatch.GetTimestamp()
            )
            {
                _threadSleeper.Sleep(watchdogTimeout);
            }
        }
    }


    public class RuneSolvingWatchdogFacade : AbstractWatchdog
    {
        private AbstractWatchdog _runeSolvingWatchdog;

        public RuneSolvingWatchdogFacade(AbstractTimestamp pingTimestamp)
        {
            _runeSolvingWatchdog = new RuneSolvingWatchdog(
                new ProcessMonitor(),
                new ThreadSleeper(),
                pingTimestamp,
                3
            );
        }

        public override void Start(string watchProcessName, int watchdogTimeout)
        {
            _runeSolvingWatchdog.Start(watchProcessName, watchdogTimeout);
        }
    }
}
