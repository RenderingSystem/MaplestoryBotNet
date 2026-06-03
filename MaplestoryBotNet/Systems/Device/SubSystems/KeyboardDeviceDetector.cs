using MaplestoryBotNet.LibraryWrappers;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;


namespace MaplestoryBotNet.Systems.Device.SubSystems
{
    public enum InputDeviceTypes {
        Keyboard = 0,
        Mouse,
        MaxNum
    }


    public class DeviceContext
    {
        public nint Context { private set; get; }

        public int Device { private set; get; }

        public DeviceContext(nint context, int device)
        {
            Context = context;
            Device = device;
        }
    }


    public abstract class AbstractDeviceDetector
    {
        public abstract DeviceContext Detect();
    }


    public class KeyboardDeviceDetector : AbstractDeviceDetector
    {
        private AbstractInterceptionLibrary _interceptionLibrary;

        public KeyboardDeviceDetector(AbstractInterceptionLibrary interceptionLibrary)
        {
            _interceptionLibrary = interceptionLibrary;
        }

        public override DeviceContext Detect()
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
            return new DeviceContext(context, device);
        }
    }


    public class MouseDeviceDetector : AbstractDeviceDetector
    {
        private AbstractInterceptionLibrary _interceptionLibrary;
        public MouseDeviceDetector(AbstractInterceptionLibrary interceptionLibrary)
        {
            _interceptionLibrary = interceptionLibrary;
        }
        public override DeviceContext Detect()
        {
            var context = _interceptionLibrary.CreateContext();
            _interceptionLibrary.SetFilter(
                context,
                _interceptionLibrary.IsMouse,
                Interception.InterceptionInterop.Filter.All
            );
            var device = _interceptionLibrary.Wait(context);
            _interceptionLibrary.SetFilter(
                context,
                _interceptionLibrary.IsMouse,
                Interception.InterceptionInterop.Filter.None
            );
            return new DeviceContext(context, device);
        }
    }


    public class KeyboardDeviceDetectorThread : AbstractThread
    {
        private AbstractDeviceDetector _keyboardDeviceDetector;

        private DeviceContext? _keyboardDevice;

        private volatile AbstractWindowActionHandler? _splashScreenActionHandler;

        public KeyboardDeviceDetectorThread(
            AbstractDeviceDetector keyboardDeviceDetector,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _keyboardDeviceDetector = keyboardDeviceDetector;
            _keyboardDevice = null;
            _splashScreenActionHandler = null;
        }

        public override void ThreadLoop()
        {
            var keyboardDevice = _keyboardDeviceDetector.Detect();
            _keyboardDevice = keyboardDevice;
            while (_splashScreenActionHandler == null)
            {
                Thread.Yield();
            }
            _splashScreenActionHandler.Inject(InputDeviceTypes.Keyboard, _keyboardDevice);
        }

        public override object? Result()
        {
            return _keyboardDevice;
        }

        public override void Inject(object dataType, object? value)
        {
            if (
                dataType is SystemInjectType.ActionHandler
                && value is AbstractWindowActionHandler splashScreenActionHandler
                && splashScreenActionHandler.Modifier().State(0) is SplashScreenTypes.StartSplash
            )
            {
                _splashScreenActionHandler = splashScreenActionHandler;
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
        }
    }


    public class MouseDeviceDetectorThread : AbstractThread
    {
        private AbstractDeviceDetector _mouseDeviceDetector;

        private DeviceContext? _mouseDeviceContext;

        private volatile AbstractWindowActionHandler? _splashScreenActionHandler;

        public MouseDeviceDetectorThread(
            AbstractDeviceDetector mouseDeviceDetector,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _mouseDeviceDetector = mouseDeviceDetector;
            _mouseDeviceContext = null;
        }
        public override void ThreadLoop()
        {
            var mouseDevice = _mouseDeviceDetector.Detect();
            _mouseDeviceContext = mouseDevice;
            while (_splashScreenActionHandler == null)
            {
                Thread.Yield();
            }
            _splashScreenActionHandler.Inject(InputDeviceTypes.Mouse, _mouseDeviceContext);
        }
        public override object? Result()
        {
            return _mouseDeviceContext;
        }

        public override void Inject(object dataType, object? value)
        {
            if (
                dataType is SystemInjectType.ActionHandler
                && value is AbstractWindowActionHandler splashScreenActionHandler
                && splashScreenActionHandler.Modifier().State(0) is SplashScreenTypes.StartSplash
            )
            {
                _splashScreenActionHandler = splashScreenActionHandler;
            }
        }
    }


    public class MouseDeviceDetectorThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            return new MouseDeviceDetectorThread(
                new MouseDeviceDetector(new InterceptionLibrary()),
                new ThreadRunningState()
            );
        }
    }


    public class DeviceDetectorSystem : AbstractSystem
    {
        private List<AbstractThreadFactory> _deviceDetectorThreadFactories;

        private List<AbstractThread> _deviceDetectorThreads;

        public DeviceDetectorSystem(
            List<AbstractThreadFactory> deviceDetectorThreadFactories
        )
        {
            _deviceDetectorThreadFactories = deviceDetectorThreadFactories;
            _deviceDetectorThreads = [];
        }

        public override void Initialize()
        {
            foreach (var deviceDetectorThreadFactory in _deviceDetectorThreadFactories)
            {
                _deviceDetectorThreads.Add(deviceDetectorThreadFactory.CreateThread());
            }
        }

        public override void Start()
        {
            foreach (var deviceDetectorThread in _deviceDetectorThreads)
            {
                deviceDetectorThread.Start();
            }
        }

        public override void Inject(object dataType, object? data)
        {
            foreach (var deviceDetectorThread in _deviceDetectorThreads)
            {
                deviceDetectorThread.Inject(dataType, data);
            }
        }
    }


    public class DeviceDetectorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new DeviceDetectorSystem(
                [
                    new KeyboardDeviceDetectorThreadFactory(),
                    new MouseDeviceDetectorThreadFactory()
                ]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }

}
     