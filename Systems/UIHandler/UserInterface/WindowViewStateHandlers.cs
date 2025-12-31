using MaplestoryBotNet.Systems.UIHandler.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class WindowViewCheckbox : AbstractWindowStateModifier
    {
        private List<MenuItem> _menuItems = [];

        private ViewTypes _selectedViewType;

        private ReaderWriterLockSlim _menuItemsLock;

        public WindowViewCheckbox(
            List<MenuItem> menuItems
        )
        {
            _menuItems = menuItems;
            _menuItemsLock = new ReaderWriterLockSlim();
            _selectedViewType = ViewTypes.ViewTypesMaxNum;
            Modify(ViewTypes.Snapshots);
        }

        public override void Modify(object? value)
        {
            if (value is not ViewTypes inputViewType)
            {
                return;
            }
            try
            {
                _menuItemsLock.EnterWriteLock();
                for (int i = 0; i < _menuItems.Count; i++)
                {
                    _menuItems[i].IsChecked = false;
                }
                for (int i = 0; i < _menuItems.Count; i++)
                {
                    var header = _menuItems[i].Header.ToString();
                    if (
                        header != null
                        && Enum.TryParse<ViewTypes>(header, out var currentViewType)
                        && currentViewType == inputViewType
                    )
                    {
                        _menuItems[i].IsChecked = true;
                        break;
                    }
                }
                _selectedViewType = inputViewType;
            }
            finally
            {
                _menuItemsLock.ExitWriteLock();
            }
        }

        public override object? State(int stateType)
        {
            try
            {
                _menuItemsLock.EnterReadLock();
                return _selectedViewType;
            }
            finally
            {
                _menuItemsLock.ExitReadLock();
            }
        }
    }


    public class WindowViewUpdater : AbstractWindowStateModifier
    {
        private AbstractDispatcher _dispatcher;

        private System.Windows.Controls.Image _image;

        private bool _settingImage;

        private ReaderWriterLockSlim _settingImageLock;

        private AbstractImageSharpConverter _converter;

        private bool SettingImage
        {
            set
            {
                try
                {
                    _settingImageLock.EnterWriteLock();
                    _settingImage = value;
                }
                finally
                {
                    _settingImageLock.ExitWriteLock();
                }
            }

            get
            {
                try
                {
                    _settingImageLock.EnterReadLock();
                    return _settingImage;
                }
                finally
                {
                    _settingImageLock.ExitReadLock();
                }
            }
        }

        public WindowViewUpdater(
            AbstractDispatcher dispatcher,
            AbstractImageSharpConverter converter,
            System.Windows.Controls.Image image
        )
        {
            _dispatcher = dispatcher;
            _converter = converter;
            _image = image;
            _settingImage = false;
            _settingImageLock = new ReaderWriterLockSlim();
        }

        public override void Modify(object? value)
        {
            if (value is Image<Bgra32> imagesharpImage)
            {
                var bitmapSource = _converter.ConvertToBitmap(imagesharpImage);
                bitmapSource.Freeze();
                if (SettingImage == false)
                {
                    SettingImage = true;
                    _dispatcher.Dispatch(
                        () =>
                        {
                            SettingImage = false;
                            _image.Source = bitmapSource;
                        }
                    );
                }
            }
        }
    }


    public class WindowExiter : AbstractWindowStateModifier
    {
        public override void Modify(object? value)
        {
            if (value is List<AbstractSystemWindow> systemWindows)
            {
                foreach (var systemWindow in systemWindows)
                {
                    systemWindow.ShutdownFlag = true;
                    systemWindow.Close();
                }
            }
        }
    }


    public class WindowExitActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _window;

        private MenuItem _exitMenuItem;

        private AbstractWindowStateModifier _windowStateModifier;

        public WindowExitActionHandler(
            AbstractSystemWindow window,
            MenuItem exitMenuItem,
            AbstractWindowStateModifier windowStateModifier
        )
        {
            _window = window;
            _exitMenuItem = exitMenuItem;
            _windowStateModifier = windowStateModifier;
            _exitMenuItem.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowStateModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _windowStateModifier.Modify(new List<AbstractSystemWindow>{ _window });
        }
    }


    public class WindowMenuItemClickActionHandler : AbstractWindowActionHandler
    {
        private List<MenuItem> _menuItems;

        private AbstractWindowStateModifier _windowStateModifier;

        public WindowMenuItemClickActionHandler(
            List<MenuItem> menuItems,
            AbstractWindowStateModifier windowStateModifier
        )
        {
            _menuItems = menuItems;
            _windowStateModifier = windowStateModifier;
            for (int i = 0; i < _menuItems.Count; i++)
            {
                _menuItems[i].Click += OnEvent;
            }
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowStateModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            var source = (MenuItem)((RoutedEventArgs)e).OriginalSource;
            var header = source.Header.ToString();
            if (header != null && Enum.TryParse<ViewTypes>(header, out var viewType))
            {
                _windowStateModifier.Modify(viewType);
            }
        }
    }


    public class WindowViewUpdaterActionHandler : WindowMenuItemClickActionHandler
    {
        public WindowViewUpdaterActionHandler(
            List<MenuItem> menuItems,
            AbstractWindowStateModifier windowStateModifier
        ) : base(menuItems, windowStateModifier) {}
    }


    public class WindowViewCheckboxActionHandler : WindowMenuItemClickActionHandler
    {
        public WindowViewCheckboxActionHandler(
            List<MenuItem> menuItems,
            AbstractWindowStateModifier windowStateModifier
        ) : base(menuItems, windowStateModifier) {}
    }


    public class ApplicationClosingActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _closingWindow;

        private List<AbstractSystemWindow> _windowsToClose;

        private AbstractWindowStateModifier _windowExiter;

        public ApplicationClosingActionHandler(
            AbstractSystemWindow closingWindow,
            List<AbstractSystemWindow> windowsToClose,
            AbstractWindowStateModifier windowExiter
        )
        {
            _closingWindow = closingWindow;
            _windowsToClose = windowsToClose;
            _windowExiter = windowExiter;
            ((Window?)_closingWindow.GetWindow())!.Closing += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowExiter;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _windowExiter.Modify(_windowsToClose);
        }
    }


    public class ApplicationClosingActionHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private AbstractSystemWindow? _closingWindow;

        private List<AbstractSystemWindow>? _windowsToClose;

        public override AbstractWindowActionHandler Build()
        {
            Debug.Assert(_closingWindow != null);
            Debug.Assert(_windowsToClose != null);
            return new ApplicationClosingActionHandler(
                _closingWindow, _windowsToClose, new WindowExiter()
            );
        }

        public override AbstractWindowActionHandlerBuilder WithArgs(object? args)
        {
            if (args is AbstractSystemWindow closingWindow)
            {
                _closingWindow = closingWindow;
            }
            if (args is List<AbstractSystemWindow> windowsToClose)
            {
                _windowsToClose = windowsToClose;
            }
            return this;
        }
    }


    public class WindowExitActionHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private AbstractSystemWindow? _window;

        private MenuItem? _exitMenuItem;

        public WindowExitActionHandlerBuilder()
        {
            _window = null;
            _exitMenuItem = null;
        }

        public override AbstractWindowActionHandler Build()
        {
            Debug.Assert(_window != null);
            Debug.Assert(_exitMenuItem != null);
            return new WindowExitActionHandler(_window, _exitMenuItem, new WindowExiter());
        }

        public override AbstractWindowActionHandlerBuilder WithArgs(object? args)
        {
            if (args is AbstractSystemWindow window)
            {
                _window = window;
            }
            if (args is MenuItem exitMenuItem)
            {
                _exitMenuItem = exitMenuItem;
            }
            return this;
        }
    }


    public class WindowViewUpdaterActionHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private List<MenuItem>? _menuItems;

        private AbstractDispatcher? _dispatcher;

        private System.Windows.Controls.Image? _image;

        public WindowViewUpdaterActionHandlerBuilder()
        {
            _menuItems = null;
            _dispatcher = null;
            _image = null;
        }

        public override AbstractWindowActionHandler Build()
        {
            Debug.Assert(_menuItems != null);
            Debug.Assert(_dispatcher != null);
            Debug.Assert(_image != null);
            return new WindowViewUpdaterActionHandler(
                _menuItems,
                new WindowViewUpdater(
                    _dispatcher,
                    new ImageSharpConverter(),
                    _image
                )
            );
        }

        public override WindowViewUpdaterActionHandlerBuilder WithArgs(object? args)
        {
            if (args is List<MenuItem> menuItems)
            {
                _menuItems = menuItems;
            }
            if (args is AbstractDispatcher dispatcher)
            {
                _dispatcher = dispatcher;
            }
            if (args is System.Windows.Controls.Image image)
            {
                _image = image;
            }
            return this;
        }
    }


    public class WindowViewCheckboxActionHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private List<MenuItem>? _menuItems;

        public WindowViewCheckboxActionHandlerBuilder()
        {
            _menuItems = null;
        }

        private AbstractWindowStateModifier _createModifier()
        {
            Debug.Assert(_menuItems != null);
            var modifier = new WindowViewCheckbox(_menuItems);
            modifier.Initialize();
            return modifier;
        }

        public override AbstractWindowActionHandler Build()
        {
            Debug.Assert(_menuItems != null);
            return new WindowViewCheckboxActionHandler(
                _menuItems, _createModifier()
            );
        }

        public override WindowViewCheckboxActionHandlerBuilder WithArgs(object? args)
        {
            if (args is List<MenuItem> menuItems)
            {
                _menuItems = menuItems;
            }
            return this;
        }
    }
}
