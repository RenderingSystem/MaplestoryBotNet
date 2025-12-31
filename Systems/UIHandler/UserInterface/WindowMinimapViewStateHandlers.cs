using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Windows;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{

    public class WindowViewMinimapUpdaterParameters
    {
        public Image<Bgra32>? FullImage;

        public Rect MinimapRect;
    }


    public class WindowViewMinimapUpdater : AbstractWindowStateModifier
    {
        private System.Windows.Controls.Image _image;

        private AbstractDispatcher _dispatcher;

        private AbstractImageSharpConverter _converter;

        private AbstractSystemWindow _minimapWindow;

        private bool __settingImage;

        private ReaderWriterLockSlim _settingImageLock;

        private bool _settingImage
        {
            set
            {
                try
                {
                    _settingImageLock.EnterWriteLock();
                    __settingImage = value;
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
                    return __settingImage;
                }
                finally
                {
                    _settingImageLock.ExitReadLock();
                }
            }
        }

        public WindowViewMinimapUpdater(
            System.Windows.Controls.Image image,
            AbstractDispatcher dispatcher,
            AbstractImageSharpConverter converter,
            AbstractSystemWindow minimapWindow
        )
        {
            _image = image;
            _dispatcher = dispatcher;
            _converter = converter;
            _minimapWindow = minimapWindow;
            _settingImageLock = new ReaderWriterLockSlim();
            _settingImage = false;
        }

        public override void Modify(object? value)
        {
            if (value is not WindowViewMinimapUpdaterParameters parameters)
            {
                return;
            }
            var croppedImage = _converter.Crop(parameters.FullImage!, parameters.MinimapRect);
            var bitmapSource = _converter.ConvertToBitmap(croppedImage);
            bitmapSource.Freeze();
            if (_settingImage)
            {
                return;
            }
            _settingImage = true;
            _dispatcher.Dispatch(
                () =>
                {
                    _settingImage = false;
                    if (_minimapWindow.Visible())
                    {
                        _image.Source = bitmapSource;
                    }
                }
            );
        }
    }


    public class WindowViewMinimapUpdaterActionHandler : AbstractWindowActionHandler
    {
        private AbstractWindowStateModifier _viewMinimapUpdater;

        public WindowViewMinimapUpdaterActionHandler(
            AbstractWindowStateModifier viewMinimapUpdater
        )
        {
            _viewMinimapUpdater = viewMinimapUpdater;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _viewMinimapUpdater;
        }
    }


    public class WindowViewMinimapUpdaterActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _viewMinimapUpdaterActionHandler;

        public WindowViewMinimapUpdaterActionHandlerFacade(
            System.Windows.Controls.Image image,
            AbstractDispatcher dispatcher,
            AbstractSystemWindow minimapWindow
        )
        {
            _viewMinimapUpdaterActionHandler = new WindowViewMinimapUpdaterActionHandler(
                new WindowViewMinimapUpdater(
                    image,
                    dispatcher,
                    new ImageSharpConverter(),
                    minimapWindow
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _viewMinimapUpdaterActionHandler.Modifier();
        }
    }


    public class GameScreenCaptureMinimapSubscriber : AbstractScreenCaptureSubscriber
    {
        private AbstractWindowStateModifier? __viewModifier;

        private ReaderWriterLockSlim _viewModifierLock;

        private MapModel? __mapModel;

        private ReaderWriterLockSlim _mapModelLock;

        private AbstractWindowStateModifier? _viewModifier
        {
            set
            {
                try
                {
                    _viewModifierLock.EnterWriteLock();
                    __viewModifier = value;
                }
                finally
                {
                    _viewModifierLock.ExitWriteLock();
                }
            }

            get
            {
                try
                {
                    _viewModifierLock.EnterReadLock();
                    return __viewModifier;
                }
                finally
                {
                    _viewModifierLock.ExitReadLock();
                }
            }
        }

        private MapModel? _mapModel
        {
            set
            {
                try
                {
                    _mapModelLock.EnterWriteLock();
                    __mapModel = value;
                }
                finally
                {
                    _mapModelLock.ExitWriteLock();
                }
            }

            get
            {
                try
                {
                    _mapModelLock.EnterReadLock();
                    return __mapModel;
                }
                finally
                {
                    _mapModelLock.ExitReadLock();
                }
            }
        }

        public GameScreenCaptureMinimapSubscriber(SemaphoreSlim semaphore) : base(semaphore)
        {
            _viewModifierLock = new ReaderWriterLockSlim();
            _mapModelLock = new ReaderWriterLockSlim();
            _viewModifier = null;
            _mapModel = null;
        }

        public override void ProcessImage()
        {
            var viewModifier = _viewModifier;
            var mapModel = _mapModel;
            if (viewModifier != null && mapModel != null)
            {
                viewModifier.Modify(
                    new WindowViewMinimapUpdaterParameters
                    {
                        FullImage = _image,
                        MinimapRect = mapModel.GetMapArea()
                    }
                );
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ActionHandler
                && data is WindowViewMinimapUpdaterActionHandlerFacade viewHandler
            )
            {
                _viewModifier = viewHandler.Modifier();
            }
            if (
                dataType is SystemInjectType.MapModel
                && data is MapModel mapModel
            )
            {
                _mapModel = mapModel;
            }
        }
    }
}
