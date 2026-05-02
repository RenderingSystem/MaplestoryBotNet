using MaplestoryBotNet.Systems.UIHandler.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class WindowViewUpdater : AbstractWindowStateModifier
    {
        private AbstractDispatcher _dispatcher;

        private System.Windows.Controls.Image _image;

        private volatile bool _settingImage;

        private AbstractImageSharpConverter _converter;

        private bool SettingImage
        {
            set { _settingImage = value; }
            get { return _settingImage; }
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
        private AbstractWindowStateModifier _windowStateModifier;

        public WindowMenuItemClickActionHandler(
            AbstractWindowStateModifier windowStateModifier
        )
        {
            _windowStateModifier = windowStateModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowStateModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _windowStateModifier.Modify(null);
        }
    }


    public class WindowViewUpdaterActionHandler : WindowMenuItemClickActionHandler
    {
        public WindowViewUpdaterActionHandler(
            AbstractWindowStateModifier windowStateModifier
        ) : base(windowStateModifier)
        {
        }
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
        private AbstractDispatcher? _dispatcher;

        private System.Windows.Controls.Image? _image;

        public WindowViewUpdaterActionHandlerBuilder()
        {
            _dispatcher = null;
            _image = null;
        }

        public override AbstractWindowActionHandler Build()
        {
            Debug.Assert(_dispatcher != null);
            Debug.Assert(_image != null);
            return new WindowViewUpdaterActionHandler(
                new WindowViewUpdater(
                    _dispatcher,
                    new ImageSharpConverter(),
                    _image
                )
            );
        }

        public override WindowViewUpdaterActionHandlerBuilder WithArgs(object? args)
        {
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
}
