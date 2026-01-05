using MaplestoryBotNet.Systems;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;


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
}
