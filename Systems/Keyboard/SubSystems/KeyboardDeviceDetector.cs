using MaplestoryBotNet.LibraryWrappers;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNet.UserInterface;
using System.Windows.Input;


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
            _interceptionLibrary.SetFilter(
                context,
                _interceptionLibrary.IsKeyboard,
                Interception.InterceptionInterop.Filter.None
            );
            return new KeyboardDeviceContext(context, device);
        }
    }


    public class KeyboardDeviceDetectorThread : AbstractThread
    {
        private AbstractKeyboardDeviceDetector _keyboardDeviceDetector;

        private KeyboardDeviceContext? _keyboardDevice;

        private ReaderWriterLockSlim _keyboardDeviceLock;

        private AbstractWindowStateModifier? _splashScreenModifier;

        private ReaderWriterLockSlim _splashScreenModifierLock;

        protected AbstractWindowStateModifier? SplashScreenModifier
        {
            get
            {
                try
                {
                    _splashScreenModifierLock.EnterReadLock();
                    return _splashScreenModifier;
                }
                finally
                {
                    _splashScreenModifierLock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _splashScreenModifierLock.EnterWriteLock();
                    _splashScreenModifier = value;
                }
                finally
                {
                    _splashScreenModifierLock.ExitWriteLock();
                }
            }
        }

        protected KeyboardDeviceContext? KeyboardDeviceContext
        {
            get
            {
                try
                {
                    _keyboardDeviceLock.EnterReadLock();
                    return _keyboardDevice;
                }
                finally
                {
                    _keyboardDeviceLock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _keyboardDeviceLock.EnterWriteLock();
                    _keyboardDevice = value;
                }
                finally
                {
                    _keyboardDeviceLock.ExitWriteLock();
                }
            }
        }

        public KeyboardDeviceDetectorThread(
            AbstractKeyboardDeviceDetector keyboardDeviceDetector,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _keyboardDeviceDetector = keyboardDeviceDetector;
            _keyboardDevice = null;
            _keyboardDeviceLock = new ReaderWriterLockSlim();
            _splashScreenModifier = null;
            _splashScreenModifierLock = new ReaderWriterLockSlim();
        }

        public override void ThreadLoop()
        {
            var keyboardDevice = _keyboardDeviceDetector.Detect();
            KeyboardDeviceContext = keyboardDevice;
            while (SplashScreenModifier == null) ;
            SplashScreenModifier.Modify(KeyboardDeviceContext);
        }

        public override object? Result()
        {
            return KeyboardDeviceContext;
        }

        public override void Inject(SystemInjectType dataType, object? value)
        {
            if (
                dataType == SystemInjectType.SplashScreen
                && value is AbstractWindowStateModifier splashScreenModifier
            )
            {
                SplashScreenModifier = splashScreenModifier;
            }
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

        public KeyboardDeviceDetectorSystem(
            AbstractThreadFactory keyboardDeviceDetectorThreadFactory
        )
        {
            _keyboardDeviceDetectorThreadFactory = keyboardDeviceDetectorThreadFactory;
            _keyboardDeviceDetectorThread = null;
        }

        public override void Initialize()
        {
            if (_keyboardDeviceDetectorThread == null)
            {
                _keyboardDeviceDetectorThread = _keyboardDeviceDetectorThreadFactory.CreateThread();
            }
        }

        public override void Start()
        {
            if (_keyboardDeviceDetectorThread != null)
            {
                _keyboardDeviceDetectorThread.Start();
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (_keyboardDeviceDetectorThread != null)
            {
                _keyboardDeviceDetectorThread.Inject(dataType, data);
            }
        }
    }


    public class KeyboardDeviceDetectorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new KeyboardDeviceDetectorSystem(
                new KeyboardDeviceDetectorThreadFactory()
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }

}
     