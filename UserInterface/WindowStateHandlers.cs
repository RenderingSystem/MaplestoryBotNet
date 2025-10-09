using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace MaplestoryBotNet.UserInterface
{
    public enum ViewTypes
    {
        Snapshots = 0,
        Minimap,
        NCC,
        ViewTypesMaxNum
    }


    public class WindowViewCheckbox : AbstractWindowStateModifier
    {
        private List<MenuItem> _menuItems = [];

        private ViewTypes _selectedViewType = ViewTypes.ViewTypesMaxNum;

        private ReaderWriterLockSlim _menuItemsLock;

        public WindowViewCheckbox(
            List<MenuItem> menuItems
        )
        {
            _menuItems = menuItems;
            _menuItemsLock = new ReaderWriterLockSlim();
            _selectedViewType = ViewTypes.ViewTypesMaxNum;
        }

        public override void Initialize()
        {
            try
            {
                _menuItemsLock.EnterWriteLock();
                for (int i = 0; i < _menuItems.Count; i++)
                {
                    _menuItems[i].IsChecked = false;
                }
                _selectedViewType = ViewTypes.ViewTypesMaxNum;
            }
            finally
            {
                _menuItemsLock.ExitWriteLock();
            }
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
                var isChecked = false;
                for (int i = 0; i < _menuItems.Count; i++)
                {
                    var header = _menuItems[i].Header.ToString();
                    _menuItems[i].IsChecked = (
                        header != null
                        && Enum.TryParse<ViewTypes>(header, out var currentViewType)
                        && currentViewType == inputViewType
                    );
                    isChecked = isChecked || _menuItems[i].IsChecked;
                }
                _selectedViewType = isChecked ? inputViewType : ViewTypes.ViewTypesMaxNum;
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

        private Dispatcher _dispatcher;

        private System.Windows.Controls.Image _image;

        private bool _settingImage;

        private ReaderWriterLockSlim _settingImageLock;

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
            Dispatcher dispatcher,
            System.Windows.Controls.Image image
        )
        {
            _dispatcher = dispatcher;
            _image = image;
            _settingImage = false;
            _settingImageLock = new ReaderWriterLockSlim();
        }

        private BitmapSource _ConvertToBitmap(Image<Bgra32> imageSharpImage)
        {
            var sourceMemoryGroup = imageSharpImage.GetPixelMemoryGroup();
            var sourceSpan = sourceMemoryGroup[0].Span;
            var sourceBytes = MemoryMarshal.AsBytes(sourceSpan);
            int width = imageSharpImage.Width;
            int height = imageSharpImage.Height;
            int stride = width * 4;
            int requiredSize = stride * height;
            var pixelBuffer = new byte[requiredSize];
            sourceBytes.CopyTo(pixelBuffer);
            return BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                pixelBuffer,
                stride
            );
        }

        public override void Modify(object? value)
        {
            if (value is Image<Bgra32> imagesharpImage)
            {
                var bitmapSource = _ConvertToBitmap(imagesharpImage);
                bitmapSource.Freeze();
                if (SettingImage == false)
                {
                    SettingImage = true;
                    _dispatcher.BeginInvoke(
                        () =>
                        {
                            SettingImage = false;
                            _image.Source = bitmapSource;
                        },
                        DispatcherPriority.Background
                    );
                }
            }
        }
    }


    public class WindowExiter : AbstractWindowStateModifier
    {
        public override void Modify(object? value)
        {
            if (value is Window window)
            {
                window.Close();
            }
        }
    }


    public class WindowExitActionHandler : AbstractWindowActionHandler
    {
        private Window _window;

        private MenuItem _exitMenuItem;

        private AbstractWindowStateModifier _windowStateModifier;

        public WindowExitActionHandler(
            Window window,
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
            _windowStateModifier.Modify(_window);
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

        public override void OnEvent(object sender, EventArgs e)
        {
            var source = (MenuItem)((RoutedEventArgs)e).OriginalSource;
            var header = source.Header.ToString();
            if (header != null && Enum.TryParse<ViewTypes>(header, out var viewType))
            {
                _windowStateModifier.Modify(viewType);
            }
        }
    }


    public class WindowExitActionHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private Window? _window;

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
            if (args is Window window)
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

        private Dispatcher? _dispatcher;

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
            return new WindowMenuItemClickActionHandler(
                _menuItems, new WindowViewUpdater(_dispatcher, _image)
            );
        }

        public override WindowViewUpdaterActionHandlerBuilder WithArgs(object? args)
        {
            if (args is List<MenuItem> menuItems)
            {
                _menuItems = menuItems;
            }
            if (args is Dispatcher dispatcher)
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

        public override AbstractWindowActionHandler Build()
        {
            Debug.Assert(_menuItems != null);
            return new WindowMenuItemClickActionHandler(
                _menuItems, new WindowViewCheckbox(_menuItems)
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
