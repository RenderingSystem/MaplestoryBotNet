using MaplestoryBotNet.Systems.GPUSelector;
using MaplestoryBotNet.Systems.Device.SubSystems;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public enum SplashScreenTypes
    {
        StartSplash = 0,
        MaxNum
    }


    public class WindowSplashScreenCompleterParameters
    {
        public DeviceContext? KeyboardDeviceContext;

        public DeviceContext? MouseDeviceContext;

        public AbstractGPUSelection? GpuSelection;

        public AbstractInjectAction? InjectAction;

        public bool Completed = false;
    }


    public class WindowSplashScreenCompleter : AbstractWindowStateModifier
    {
        private AbstractSystemWindow _splashScreen;

        private AbstractSystemWindow _mainWindow;

        private AbstractDispatcher _dispatcher;

        public WindowSplashScreenCompleter(
            AbstractSystemWindow splashScreen,
            AbstractSystemWindow mainWindow,
            AbstractDispatcher dispatcher
        )
        {
            _splashScreen = splashScreen;
            _mainWindow = mainWindow;
            _dispatcher = dispatcher;
        }

        public override void Modify(object? value)
        {
            if (value is not WindowSplashScreenCompleterParameters parameters)
            {
                return;
            }
            if (
                parameters.KeyboardDeviceContext is DeviceContext keyboardDeviceContext &&
                parameters.MouseDeviceContext is DeviceContext mouseDeviceContext &&
                parameters.GpuSelection is AbstractGPUSelection gpuSelection &&
                parameters.InjectAction is AbstractInjectAction injectAction &&
                !parameters.Completed
            )
            {
                parameters.Completed = true;
                _dispatcher.Dispatch(
                    () =>
                    {
                        injectAction.GetAction()(
                            SystemInjectType.KeyboardDevice, keyboardDeviceContext
                        );
                        injectAction.GetAction()(
                            SystemInjectType.MouseDevice, mouseDeviceContext
                        );
                        _splashScreen.ShutdownFlag = true;
                        _splashScreen.Close();
                        _mainWindow.Show();
                    }
                );
            }
        }

        public override object? State(int stateType)
        {
            return SplashScreenTypes.StartSplash;
        }
    }


    public class WindowSplashScreenCompleteActionHandler : AbstractWindowActionHandler
    {
        private AbstractWindowStateModifier _splashScreenCompleter;

        private WindowSplashScreenCompleterParameters _parameters;

        public WindowSplashScreenCompleteActionHandler(
            AbstractWindowStateModifier splashScreenCompleter,
            WindowSplashScreenCompleterParameters parameters
        )
        {
            _splashScreenCompleter = splashScreenCompleter;
            _parameters = parameters;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _splashScreenCompleter;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is InputDeviceTypes.Keyboard &&
                data is DeviceContext keyboardDeviceContext
            )
            {
                _parameters.KeyboardDeviceContext = keyboardDeviceContext;
            }
            else if (
                dataType is InputDeviceTypes.Mouse &&
                data is DeviceContext mouseDeviceContext
            )
            {
                _parameters.MouseDeviceContext = mouseDeviceContext;
            }
            else if (data is AbstractGPUSelection gpuSelection)
            {
                _parameters.GpuSelection = gpuSelection;
            }
            else if (
                dataType is SystemInjectType.InjectAction &&
                data is AbstractInjectAction injectAction
            )
            {
                _parameters.InjectAction = injectAction;
            }
            _splashScreenCompleter.Modify(_parameters);
        }
    }


    public class WindowSplashScreenCompleteActionHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private AbstractSystemWindow? _splashScreen;

        private AbstractSystemWindow? _mainWindow;

        private AbstractDispatcher? _dispatcher;

        public WindowSplashScreenCompleteActionHandlerBuilder()
        {
            _splashScreen = null;
            _mainWindow = null;
            _dispatcher = null;
        }

        public override AbstractWindowActionHandler Build()
        {
            return new WindowSplashScreenCompleteActionHandler(
                new WindowSplashScreenCompleter(
                    _splashScreen!,
                    _mainWindow!,
                    _dispatcher!
                ),
                new WindowSplashScreenCompleterParameters()
            );
        }

        public override AbstractWindowActionHandlerBuilder WithArgs(object? args)
        {
            if (args is AbstractSystemWindow systemWindow)
            {
                if (systemWindow.GetWindow()?.GetType().Name == "SplashScreen")
                {
                    _splashScreen = systemWindow;
                }
                else if (systemWindow.GetWindow()?.GetType().Name == "MainWindow")
                {
                    _mainWindow = systemWindow;
                }
            }
            else if (args is AbstractDispatcher dispatcher)
            {
                _dispatcher = dispatcher;
            }
            return this;
        }
    }
}
