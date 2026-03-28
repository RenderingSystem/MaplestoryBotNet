using MaplestoryBotNet.Systems.GPUSelector;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using System.Diagnostics;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class WindowSplashScreenCompleter : AbstractWindowStateModifier
    {
        private AbstractSystemWindow _splashScreen;

        private AbstractSystemWindow _mainWindow;

        private AbstractDispatcher _dispatcher;

        private IDataInjectable _keyboardDeviceInjectable;

        private KeyboardDeviceContext? _keyboardDeviceContext;

        private AbstractGPUSelection? _gpuSelection;

        public WindowSplashScreenCompleter(
            AbstractSystemWindow splashScreen,
            AbstractSystemWindow mainWindow,
            AbstractDispatcher dispatcher,
            IDataInjectable keyboardDeviceInjectable
        )
        {
            _splashScreen = splashScreen;
            _mainWindow = mainWindow;
            _dispatcher = dispatcher;
            _keyboardDeviceInjectable = keyboardDeviceInjectable;
            _keyboardDeviceContext = null;
            _gpuSelection = null;
        }

        public override void Modify(object? value)
        {
            if (value is KeyboardDeviceContext keyboardDeviceContext)
            {
                _keyboardDeviceContext = keyboardDeviceContext;
            }
            if (value is AbstractGPUSelection gpuSelection)
            {
                _gpuSelection = gpuSelection;
            }
            if (_keyboardDeviceContext == null)
            {
                return;
            }
            if (_gpuSelection == null)
            {
                return;
            }
            _dispatcher.Dispatch(
                () =>
                {
                    _keyboardDeviceInjectable.Inject(
                        SystemInjectType.KeyboardDevice, _keyboardDeviceContext
                    );
                    _splashScreen.ShutdownFlag = true;
                    _splashScreen.Close();
                    _mainWindow.Show();
                }
            );
        }
    }


    public class WindowSplashScreenCompleteActionHandler : AbstractWindowActionHandler
    {
        private AbstractWindowStateModifier _splashScreenCompleter;

        public WindowSplashScreenCompleteActionHandler(
            AbstractWindowStateModifier splashScreenCompleter
        )
        {
            _splashScreenCompleter = splashScreenCompleter;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _splashScreenCompleter;
        }
    }


    public class WindowSplashScreenCompleteActionHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private AbstractSystemWindow? _splashScreen;

        private AbstractSystemWindow? _mainWindow;

        private AbstractDispatcher? _dispatcher;

        private IDataInjectable? _keyboardDeviceContextInjectable;

        public WindowSplashScreenCompleteActionHandlerBuilder()
        {
            _splashScreen = null;
            _mainWindow = null;
            _dispatcher = null;
            _keyboardDeviceContextInjectable = null;
        }

        public override AbstractWindowActionHandler Build()
        {
            Debug.Assert(_splashScreen != null);
            Debug.Assert(_mainWindow != null);
            Debug.Assert(_dispatcher != null);
            Debug.Assert(_keyboardDeviceContextInjectable != null);
            return new WindowSplashScreenCompleteActionHandler(
                new WindowSplashScreenCompleter(
                    _splashScreen,
                    _mainWindow,
                    _dispatcher,
                    _keyboardDeviceContextInjectable
                )
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
            else if (args is IDataInjectable systemInjectable)
            {
                _keyboardDeviceContextInjectable = systemInjectable;
            }
            return this;
        }
    }
}
