using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;


namespace MaplestoryBotNet.UserInterface
{
    public class WindowSplashScreenCompleter : AbstractWindowStateModifier
    {
        private AbstractSystemWindow _splashScreen;

        private AbstractSystemWindow _mainWindow;

        private AbstractDispatcher _dispatcher;

        private ISystemInjectable _keyboardDeviceContextInjectable;

        public WindowSplashScreenCompleter(
            AbstractSystemWindow splashScreen,
            AbstractSystemWindow mainWindow,
            AbstractDispatcher dispatcher,
            ISystemInjectable keyboardDeviceContextInjectable
        )
        {
            _splashScreen = splashScreen;
            _mainWindow = mainWindow;
            _dispatcher = dispatcher;
            _keyboardDeviceContextInjectable = keyboardDeviceContextInjectable;
        }

        public override void Modify(object? value)
        {
            if (value is KeyboardDeviceContext keyboardDeviceContext)
            {
                _dispatcher.Dispatch(
                    () =>
                    {
                        _keyboardDeviceContextInjectable.Inject(
                            SystemInjectType.KeyboardDevice, keyboardDeviceContext
                        );
                        _splashScreen.Close();
                        _mainWindow.Show();
                    }
                );
            }
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

        private ISystemInjectable? _keyboardDeviceContextInjectable;

        public WindowSplashScreenCompleteActionHandlerBuilder()
        {
            _splashScreen = null;
            _mainWindow = null;
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
                    _splashScreen, _mainWindow, _dispatcher, _keyboardDeviceContextInjectable
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
            else if (args is ISystemInjectable systemInjectable)
            {
                _keyboardDeviceContextInjectable = systemInjectable;
            }
            return this;
        }
    }
}
