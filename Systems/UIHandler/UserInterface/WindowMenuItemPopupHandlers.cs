using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class WindowMenuItemPopupModifier : AbstractWindowStateModifier
    {
        private AbstractSystemWindow _popupWindow;

        public WindowMenuItemPopupModifier(
            AbstractSystemWindow popupWindow
        )
        {
            _popupWindow = popupWindow;
        }

        public override void Modify(object? value)
        {
            if (value is bool show)
            {
                if (show)
                {
                    _popupWindow.Show();
                }
                else
                {
                    _popupWindow.Hide();
                }
            }
        }
    }


    public class WindowMenuItemPopupHandler : AbstractWindowActionHandler
    {
        private AbstractWindowStateModifier _modifier;

        private MenuItem _menuItem;

        public WindowMenuItemPopupHandler(
            AbstractWindowStateModifier modifier,
            MenuItem menuItem
        )
        {
            _modifier = modifier;
            _menuItem = menuItem;
            _menuItem.Click += OnEvent;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _modifier.Modify(true);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _modifier;
        }
    }


    public class WindowMenuItemHideHandler : AbstractWindowActionHandler
    {
        private AbstractWindowStateModifier _modifier;

        private AbstractSystemWindow _systemWindow;

        public WindowMenuItemHideHandler(
            AbstractWindowStateModifier modifier,
            AbstractSystemWindow systemWindow
        )
        {
            _modifier = modifier;
            _systemWindow = systemWindow;
            ((System.Windows.Window?)_systemWindow.GetWindow()!).Closing += OnEvent;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (!_systemWindow.ShutdownFlag)
            {
                ((CancelEventArgs)e).Cancel = true;
                _modifier.Modify(false);
            }
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _modifier;
        }
    }


    public class WindowMenuItemPopupHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private AbstractSystemWindow? _popupWindow = null;

        private MenuItem? _menuItem = null;

        public override AbstractWindowActionHandlerBuilder WithArgs(object? args)
        {
            if (args is AbstractSystemWindow window)
            {
                _popupWindow = window;
            }
            else if (args is MenuItem menuItem)
            {
                _menuItem = menuItem;
            }
            return this;
        }

        public override AbstractWindowActionHandler Build()
        {
            var modifier = new WindowMenuItemPopupModifier(_popupWindow!);
            return new WindowMenuItemPopupHandler(modifier, _menuItem!);
        }
    }


    public class WindowMenuItemHideHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private AbstractSystemWindow? _systemWindow = null;
        public override AbstractWindowActionHandlerBuilder WithArgs(object? args)
        {
            if (args is AbstractSystemWindow window)
            {
                _systemWindow = window;
            }
            return this;
        }

        public override AbstractWindowActionHandler Build()
        {
            var modifier = new WindowMenuItemPopupModifier(_systemWindow!);
            return new WindowMenuItemHideHandler(modifier, _systemWindow!);
        }
    }


    public class WindowMenuItemStartTextTypes
    {
        public static readonly string Starting = "Starting...";

        public static readonly string Stop = "Stop!";

        public static readonly string Stopping = "Stopping...";

        public static readonly string Start = "Start!";

    }


    public class WindowMenuItemTextModifier : AbstractWindowStateModifier
    {
        private MenuItem _menuItem;

        private AbstractDispatcher _dispatcher;

        public WindowMenuItemTextModifier(MenuItem menuItem, AbstractDispatcher dispatcher)
        {
            _menuItem = menuItem;
            _dispatcher = dispatcher;
        }

        public override void Modify(object? value)
        {
            _dispatcher.Dispatch(() => { if (value is string text) { _menuItem.Header = text; } });
        }
    }


    public class WindowMenuItemStartTextActionHandler : AbstractWindowActionHandler
    {
        private AbstractWindowStateModifier _windowMenuItemTextModifier;

        public WindowMenuItemStartTextActionHandler(
            AbstractWindowStateModifier windowMenuItemTextModifier
        )
        {
            _windowMenuItemTextModifier = windowMenuItemTextModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowMenuItemTextModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            var startText = (
                (dataType is KeystrokeTransmitterExecutorThreadedUpdate.Starting) ?
                WindowMenuItemStartTextTypes.Starting :
                (dataType is KeystrokeTransmitterExecutorThreadedUpdate.Started) ?
                WindowMenuItemStartTextTypes.Stop :
                (dataType is KeystrokeTransmitterExecutorThreadedUpdate.Stopping) ?
                WindowMenuItemStartTextTypes.Stopping :
                (dataType is KeystrokeTransmitterExecutorThreadedUpdate.Stopped) ?
                WindowMenuItemStartTextTypes.Start : ""
            );
            if (startText != "")
            {
                _windowMenuItemTextModifier.Modify(startText);
            }
        }
    }


    public class WindowMenuItemStartTextActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _windowMenuItemTextActionHandler;

        public WindowMenuItemStartTextActionHandlerFacade(
            MenuItem startMenuItem, AbstractDispatcher dispatcher
        )
        {
            _windowMenuItemTextActionHandler = new WindowMenuItemStartTextActionHandler(
                new WindowMenuItemTextModifier(startMenuItem, dispatcher)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowMenuItemTextActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _windowMenuItemTextActionHandler.Inject(dataType, data);
        }
    }


    public class WindowMenuItemStartModifier : AbstractWindowStateModifier
    {
        private MenuItem _startMenuItem;

        public WindowMenuItemStartModifier(MenuItem startMenuItem)
        {
            _startMenuItem = startMenuItem;
        }

        public override void Modify(object? value)
        {
            if (value is not AbstractThread orchestratorThread)
            {
                return;
            }
            var header = (string)_startMenuItem.Header;
            var injectType = (
                (header == WindowMenuItemStartTextTypes.Start) ?
                KeystrokeTransmitterOrchestratorThreadInjectType.Start :
                (header == WindowMenuItemStartTextTypes.Stop) ?
                KeystrokeTransmitterOrchestratorThreadInjectType.Stop :
                KeystrokeTransmitterOrchestratorThreadInjectType.MaxNum
            );
            if (injectType != KeystrokeTransmitterOrchestratorThreadInjectType.MaxNum)
            {
                orchestratorThread!.Inject(injectType, 0);
            }
        }
    }


    public class WindowMenuItemStartActionHandler : AbstractWindowActionHandler
    {
        private MenuItem _startMenuItem;

        private AbstractWindowStateModifier _windowMenuItemStartModifier;

        private AbstractThread? _orchestratorThread;

        public WindowMenuItemStartActionHandler(
            MenuItem startMenuItem,
            AbstractWindowStateModifier windowMenuItemStartModifier
        )
        {
            _windowMenuItemStartModifier = windowMenuItemStartModifier;
            _startMenuItem = startMenuItem;
            _startMenuItem.Click += OnEvent;
            _orchestratorThread = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowMenuItemStartModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _windowMenuItemStartModifier.Modify(_orchestratorThread);
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ThreadDependency
                && data is AbstractThread orchestratorThread
                && orchestratorThread.State() is AbstractKeystrokeTransmitterThreadState
            )
            {
                _orchestratorThread = orchestratorThread;
            }
        }
    }


    public class WindowMenuItemStartActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _windowMenuItemStartACtionHandler;

        public WindowMenuItemStartActionHandlerFacade(
            MenuItem startMenuItem
        )
        {
            _windowMenuItemStartACtionHandler = new WindowMenuItemStartActionHandler(
                startMenuItem, new WindowMenuItemStartModifier(startMenuItem)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowMenuItemStartACtionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _windowMenuItemStartACtionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _windowMenuItemStartACtionHandler.Inject(dataType, data);
        }
    }
}
