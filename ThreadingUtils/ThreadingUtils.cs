using MaplestoryBotNet.Systems;
using System.Diagnostics;

namespace MaplestoryBotNet.ThreadingUtils
{
    public abstract class AbstractCountDown
    {
        public abstract void SetCountDown(int countDown);

        public abstract void WaitCountDown();

        public abstract int Count();

        public abstract void CountDown();
    }


    public class ThreadCountDown : AbstractCountDown
    {
        private CountdownEvent _countDownEvent = new CountdownEvent(1);

        public override void SetCountDown(int countDown)
        {
            _countDownEvent = new CountdownEvent(countDown);
        }

        public override void WaitCountDown()
        {
            _countDownEvent.Wait();
        }

        public override int Count()
        {
            return _countDownEvent.CurrentCount;
        }

        public override void CountDown()
        {
            _countDownEvent.Signal();
        }
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


    public class ThreadSafeCountDown : AbstractCountDown
    {
        private volatile CountdownEvent __countDownEvent = new CountdownEvent(1);

        private CountdownEvent _countDownEvent
        {
            get => __countDownEvent;

            set => __countDownEvent = value;
        }

        public override void SetCountDown(int countDown)
        {
            _countDownEvent.Reset(countDown);
        }

        public override void WaitCountDown()
        {
            _countDownEvent.Wait();
        }

        public override int Count()
        {
            return _countDownEvent.CurrentCount;
        }

        public override void CountDown()
        {
            var countDownEvent = _countDownEvent;
            if (countDownEvent.CurrentCount > 0)
            {
                countDownEvent.Signal();
            }
        }
    }


    public abstract class AbstractResetEvent
    {
        public abstract void WaitOne();

        public abstract void Set();
    }


    public abstract class AbstractThread : IDataInjectable
    {
        protected Thread? _thread = null;

        protected bool _isRunning = false;

        protected object _lockObject = new object();

        protected AbstractThreadRunningState _runningState;

        public AbstractThread(AbstractThreadRunningState runningState)
        {
            _runningState = runningState;
        }

        public abstract void ThreadLoop();

        public virtual object? Result()
        {
            return null;
        }

        public virtual object? State()
        {
            return null;
        }

        public virtual void Start()
        {
            if (_runningState.IsRunning())
            {
                return;
            }
            _runningState.SetRunning(true);
            _thread = new Thread(ThreadLoop)
            {
                IsBackground = true
            };
            _thread.Start();
        }

        public virtual void Stop()
        {
            if (!_runningState.IsRunning())
            {
                return;
            }
            _runningState.SetRunning(false);
        }

        public virtual bool Join(int milliseconds)
        {
            if (_thread != null && _thread.IsAlive)
            {
                int timeout = milliseconds < 0 ? Timeout.Infinite : milliseconds;
                return _thread.Join(timeout);
            }
            return true;
        }

        public virtual void Inject(object dataType, object? value)
        {

        }
    }


    public abstract class AbstractThreadRunningState
    {
        public abstract bool IsRunning();

        public abstract void SetRunning(bool running);
    }


    public abstract class AbstractExecutionFlag
    {
        public abstract void Flag();

        public abstract void Unflag();

        public abstract bool Flagged();

        public abstract void Wait();
    }


    public class ThreadExecutionFlag : AbstractExecutionFlag
    {
        private ManualResetEventSlim _executionFlag;

        private object _executionFlagLock;

        private bool _flagged;

        public ThreadExecutionFlag()
        {
            _executionFlag = new ManualResetEventSlim();
            _executionFlagLock = new object();
            _flagged = false;
        }

        public override void Flag()
        {
            lock (_executionFlagLock)
            {
                _flagged = true;
                _executionFlag.Set();
            }
        }

        public override bool Flagged()
        {
            lock (_executionFlagLock)
            {
                return _flagged;
            }
        }

        public override void Unflag()
        {
            lock (_executionFlagLock)
            {
                _flagged = false;
                _executionFlag.Reset();
            }
        }

        public override void Wait()
        {
            _executionFlag.Wait();
        }
    }


    public class ThreadRunningState : AbstractThreadRunningState
    {
        private volatile bool _isRunning = false;

        public override bool IsRunning()
        {
            return _isRunning;
        }

        public override void SetRunning(bool running)
        {
            _isRunning = running;
        }
    }


    public abstract class AbstractThreadFactory
    {
        public abstract AbstractThread CreateThread();
    }


    public class ExecutionEvent : AbstractResetEvent
    {
        private AutoResetEvent _autoResetEvent = (
            new AutoResetEvent(false)
        );

        public override void Set()
        {
            _autoResetEvent.Set();
        }

        public override void WaitOne()
        {
            _autoResetEvent.WaitOne();
        }
    }
}
