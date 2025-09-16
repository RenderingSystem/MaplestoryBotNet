namespace MaplestoryBotNet.ThreadingUtils
{
    public abstract class AbstractThread
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

        public virtual object? ThreadResult()
        {
            return null;
        }

        public virtual void ThreadStart()
        {
            if (_runningState.IsRunning())
                return;
            _runningState.SetRunning(true);
            _thread = new Thread(ThreadLoop);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public virtual void ThreadStop()
        {
            if (!_runningState.IsRunning())
                return;
            _runningState.SetRunning(false);
        }

        public virtual bool ThreadJoin(int milliseconds)
        {
            if (_thread != null && _thread.IsAlive)
            {
                int timeout = milliseconds < 0 ? Timeout.Infinite : milliseconds;
                return _thread.Join(timeout);
            }
            return true;
        }
    }


    public abstract class AbstractThreadRunningState
    {
        public abstract bool IsRunning();

        public abstract void SetRunning(bool running);
    }


    public class ThreadRunningState : AbstractThreadRunningState
    {
        private object _isRunningLockObject = new object();

        private bool _isRunning = false;

        public override bool IsRunning()
        {
            bool running = false;
            lock (_isRunningLockObject)
            {
                running = _isRunning;
            }
            return running;
        }

        public override void SetRunning(bool running)
        {
            lock (_isRunningLockObject)
            {
                _isRunning = running;
            }
        }
    }


    public abstract class AbstractThreadFactory
    {
        public abstract AbstractThread CreateThread();
    }
}
