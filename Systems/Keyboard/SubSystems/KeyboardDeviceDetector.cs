using MaplestoryBotNet.LibraryWrappers;
using MaplestoryBotNet.ThreadingUtils;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems
{
    public class KeyboardDeviceContext
    {
        public nint Context { private set; get; }

        public int Device { private set; get; }

        public KeyboardDeviceContext(nint context, int device)
        {
            Context = context;
            Device = device;
        }
    }


    public abstract class AbstractKeyboardDeviceDetector
    {
        public abstract KeyboardDeviceContext Detect();
    }


    public class KeyboardDeviceDetector : AbstractKeyboardDeviceDetector
    {
        private AbstractInterceptionLibrary _interceptionLibrary;

        public KeyboardDeviceDetector(AbstractInterceptionLibrary interceptionLibrary)
        {
            _interceptionLibrary = interceptionLibrary;
        }

        public override KeyboardDeviceContext Detect()
        {
            var context = _interceptionLibrary.CreateContext();
            _interceptionLibrary.SetFilter(
                context,
                _interceptionLibrary.IsKeyboard,
                Interception.InterceptionInterop.Filter.All
            );
            var device = _interceptionLibrary.Wait(context);
            return new KeyboardDeviceContext(context, device);
        }
    }


    public class KeyboardDeviceDetectorThread : AbstractThread
    {
        private AbstractKeyboardDeviceDetector _keyboardDeviceDetector;

        private KeyboardDeviceContext? _keyboardDevice;

        private ReaderWriterLockSlim _keyboardDeviceLock;

        public KeyboardDeviceDetectorThread(
            AbstractKeyboardDeviceDetector keyboardDeviceDetector,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _keyboardDeviceDetector = keyboardDeviceDetector;
            _keyboardDevice = null;
            _keyboardDeviceLock = new ReaderWriterLockSlim();
        }

        public override void ThreadLoop()
        {
            var keyboardDevice = _keyboardDeviceDetector.Detect();
            try
            {
                _keyboardDeviceLock.EnterWriteLock();
                _keyboardDevice = keyboardDevice;
            }
            finally
            {
                _keyboardDeviceLock.ExitWriteLock();
            }
        }

        public override object? ThreadResult()
        {
            KeyboardDeviceContext? keyboardDevice = null;
            try
            {
                _keyboardDeviceLock.EnterReadLock();
                keyboardDevice = _keyboardDevice;
            }
            finally
            {
                _keyboardDeviceLock.ExitReadLock();
            }
            return keyboardDevice;
        }
    }


    public class KeyboardDeviceDetectorThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            return new KeyboardDeviceDetectorThread(
                new KeyboardDeviceDetector(new InterceptionLibrary()),
                new ThreadRunningState()
            );
            throw new NotImplementedException();
        }
    }


    public class KeyboardDeviceDetectorSystem : AbstractSystem
    {
        private AbstractThreadFactory _keyboardDeviceDetectorThreadFactory;

        private AbstractThread? _keyboardDeviceDetectorThread;

        private KeyboardDeviceContext? _keyboardDevice;

        private AbstractInjector _keyboardDeviceInjector;

        public KeyboardDeviceDetectorSystem(
            AbstractThreadFactory keyboardDeviceDetectorThreadFactory,
            AbstractInjector keyboardDeviceInjector
        )
        {
            _keyboardDeviceDetectorThreadFactory = keyboardDeviceDetectorThreadFactory;
            _keyboardDeviceDetectorThread = null;
            _keyboardDevice = null;
            _keyboardDeviceInjector = keyboardDeviceInjector;
        }

        public override void InitializeSystem()
        {
            if (_keyboardDeviceDetectorThread == null)
                _keyboardDeviceDetectorThread = _keyboardDeviceDetectorThreadFactory.CreateThread();
        }

        public override void StartSystem()
        {
            if (_keyboardDeviceDetectorThread != null)
                _keyboardDeviceDetectorThread.ThreadStart();
        }

        public override void UpdateSystem()
        {
            if (_keyboardDevice == null)
            {
                if (_keyboardDeviceDetectorThread != null)
                    _keyboardDevice = (KeyboardDeviceContext?)_keyboardDeviceDetectorThread.ThreadResult();
                if (_keyboardDevice != null)
                    _keyboardDeviceInjector.Inject(SystemInjectType.KeyboardDevice, _keyboardDevice);
            }
        }
    }


    public class KeyboardDeviceDetectorSystemBuilder : AbstractSystemBuilder
    {
        private List<AbstractSystem> _systems = [];

        public override AbstractSystem Build()
        {
            return new KeyboardDeviceDetectorSystem(
                new KeyboardDeviceDetectorThreadFactory(),
                new SystemInjector(_systems)
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            if (arg is AbstractSystem)
                _systems.Add((AbstractSystem)arg);
            return this;
        }
    }

}
     