using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.Systems.ScreenProcessing.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;


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

        private volatile bool __settingImage;

        private bool _settingImage
        {
            set => __settingImage = value;

            get => __settingImage;
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
            var dispatchLambda = () =>
            {
                _settingImage = false;
                if (_minimapWindow.Visible())
                {
                    _image.Source = bitmapSource;
                }
            };
            bitmapSource.Freeze();
            if (!_settingImage)
            {
                _settingImage = true;
                _dispatcher.Dispatch(dispatchLambda);
            }
        }
    }


    public abstract class AbstractImageToCoordinatesConverter
    {
        public abstract (int mapX, int mapY) Convert(
            double imageX,
            double imageY,
            double imageWidth,
            double imageHeight,
            double coordinateWidth,
            double coordinateHeight
        );
    }


    public class ImageToCoordinatesConverter : AbstractImageToCoordinatesConverter
    {
        public override (int mapX, int mapY) Convert(
            double imageX,
            double imageY,
            double imageWidth,
            double imageHeight,
            double coordinateWidth,
            double coordinateHeight
        )
        {
            double scaleX = coordinateWidth / imageWidth;
            double scaleY = coordinateHeight / imageHeight;
            double scale = Math.Min(scaleX, scaleY);
            double scaledImageWidth = imageWidth * scale;
            double scaledImageHeight = imageHeight * scale;
            double offsetX = (coordinateWidth - scaledImageWidth) / 2;
            double offsetY = (coordinateHeight - scaledImageHeight) / 2;
            int mapX = (int)Math.Round(offsetX + (imageX * scale));
            int mapY = (int)Math.Round(offsetY + (imageY * scale));
            return (mapX, mapY);
        }
    }


    public abstract class AbstractPositionUpdater
    {
        public abstract void Update(
            int x, int y, AbstractMapModel mapModel, string templateKey
        );
    }


    public class PositionUpdater : AbstractPositionUpdater
    {
        private AbstractImageToCoordinatesConverter _converter;

        private TextBox _textBoxX;

        private TextBox _textBoxY;

        private System.Windows.Controls.Image _mapImage;

        public PositionUpdater(
            AbstractImageToCoordinatesConverter converter,
            TextBox textBoxX,
            TextBox textBoxY,
            System.Windows.Controls.Image mapImage
        )
        {
            _converter = converter;
            _textBoxX = textBoxX;
            _textBoxY = textBoxY;
            _mapImage = mapImage;
        }

        public override void Update(
            int x, int y, AbstractMapModel mapModel, string templateKey
        )
        {
            var sourceImage = _mapImage.Source;
            var (mapX, mapY) = (x >= 0 && y >= 0) ? (
                _converter.Convert(
                    x,
                    y,
                    sourceImage.Width,
                    sourceImage.Height,
                    _mapImage.Width,
                    _mapImage.Height
                )
            ) : (x, y);
            mapModel.SetTemplatePosition(templateKey, mapX, mapY);
            _textBoxX.Text = mapX.ToString();
            _textBoxY.Text = mapY.ToString();
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


    public class WindowMinimapPositionModifierParameters
    {
        public Tuple<int, int> Position = new Tuple<int, int>(0, 0);

        public AbstractMapModel Model = new MapModel();
    }


    public class WindowMinimapPositionModifier : AbstractWindowStateModifier
    {
        private AbstractDispatcher _dispatcher;

        private AbstractPositionUpdater _positionUpdater;

        private bool _settingImage;

        private string _templateKey;

        public WindowMinimapPositionModifier(
            AbstractDispatcher dispatcher,
            AbstractPositionUpdater positionUpdater,
            string templateKey
        )
        {
            _dispatcher = dispatcher;
            _positionUpdater = positionUpdater;
            _templateKey = templateKey;
            _settingImage = false;
        }

        public override void Modify(object? value)
        {
            if (
                value is WindowMinimapPositionModifierParameters parameters
                && !_settingImage
            )
            {
                _settingImage = true;
                _dispatcher.Dispatch(
                    () =>
                    {
                        _positionUpdater.Update(
                            parameters.Position.Item1,
                            parameters.Position.Item2,
                            parameters.Model,
                            _templateKey
                        );
                        _settingImage = false;
                    }
                );
            }
        }

        public override object? State(int stateType)
        {
            return _templateKey;
        }
    }


    public class WindowMinimapPositionActionHandler : AbstractWindowActionHandler
    {
        private AbstractWindowStateModifier _minimapPositionModifier;
        public WindowMinimapPositionActionHandler(
            AbstractWindowStateModifier minimapPositionUpdater
        )
        {
            _minimapPositionModifier = minimapPositionUpdater;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _minimapPositionModifier;
        }
    }


    public class WindowMinimapPositionActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _minimapPositionActionHandler;

        public WindowMinimapPositionActionHandlerFacade(
            AbstractDispatcher dispatcher,
            TextBox textBoxX,
            TextBox textBoxY,
            string templateKey,
            System.Windows.Controls.Image mapImage
        )
        {
            _minimapPositionActionHandler = new WindowMinimapPositionActionHandler(
                new WindowMinimapPositionModifier(
                    dispatcher,
                    new PositionUpdater(
                        new ImageToCoordinatesConverter(),
                        textBoxX,
                        textBoxY,
                        mapImage
                    ),
                    templateKey
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _minimapPositionActionHandler.Modifier();
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
        private volatile AbstractWindowStateModifier? __viewModifier;

        private volatile AbstractBottingModel? __bottingModel;

        private AbstractWindowStateModifier? _viewModifier
        {
            set => __viewModifier = value;

            get => __viewModifier;
        }

        private AbstractBottingModel? _bottingModel
        {
            set => __bottingModel = value;

            get => __bottingModel;
        }

        public GameScreenCaptureMinimapSubscriber(SemaphoreSlim semaphore) : base(semaphore)
        {
            _viewModifier = null;
            _bottingModel = null;
        }

        public override void ProcessImage()
        {
            var viewModifier = _viewModifier;
            var bottingModel = _bottingModel;
            if (viewModifier != null && bottingModel != null)
            {
                viewModifier.Modify(
                    new WindowViewMinimapUpdaterParameters
                    {
                        FullImage = _image,
                        MinimapRect = bottingModel.GetMapModel().GetMapArea()
                    }
                );
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ActionHandler
                && data is WindowViewMinimapUpdaterActionHandlerFacade viewHandler
            )
            {
                _viewModifier = viewHandler.Modifier();
            }
            if (
                dataType is SystemInjectType.BottingModel
                && data is AbstractBottingModel bottingModel
            )
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public abstract class AbstractImageCropper
    {
        public abstract Bitmap Crop(Image<Bgra32> fullImage, Rect cropRect);
    }


    public class ImageCropper : AbstractImageCropper
    {
        private AbstractImageSharpConverter _converter;

        public ImageCropper(
            AbstractImageSharpConverter converter
        )
        {
            _converter = converter;
        }

        public override Bitmap Crop(Image<Bgra32> fullImage, Rect cropRect)
        {
            var croppedImage = _converter.Crop(fullImage, cropRect);
            var bitmap = new Bitmap(
                croppedImage.Width,
                croppedImage.Height,
                PixelFormat.Format32bppArgb
            );
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat
            );
            unsafe
            {
                var span = new Span<byte>(
                    bitmapData.Scan0.ToPointer(),
                    bitmapData.Height * bitmapData.Stride
                );
                croppedImage.CopyPixelDataTo(span);
            }
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }
    }


    public class GameMinimapProcessingSubscriber : AbstractScreenCaptureSubscriber
    {
        protected AbstractImageCropper _cropper;

        protected volatile AbstractBottingModel? __bottingModel;

        protected volatile AbstractThread? __processorThread;

        private string _templateKey;

        protected AbstractThread? _processorThread
        {
            set => __processorThread = value;

            get => __processorThread;
        }

        protected AbstractBottingModel? _bottingModel
        {
            set => __bottingModel = value;

            get => __bottingModel;
        }

        public GameMinimapProcessingSubscriber(
            SemaphoreSlim semaphore,
            string templateKey
        ) : base(semaphore)
        {
            _templateKey = templateKey;
            _cropper = new ImageCropper(new ImageSharpConverter());
            _bottingModel = null;
            _processorThread = null;
        }

        public override void ProcessImage()
        {
            var bottingModel = _bottingModel;
            var procesorThread = _processorThread;
            if (bottingModel != null && procesorThread != null)
            {
                var bitmap = _cropper.Crop(_image, bottingModel.GetMapModel().GetMapArea());
                procesorThread.Inject((SystemInjectType)0x7FFFFFFF, bitmap);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ThreadDependency
                && data is AbstractThread processorThread
                && processorThread.State() is GameMinimapProcessorThreadState threadState
                && threadState.TemplateKey == _templateKey
            )
            {
                _processorThread = processorThread;
            }
            else if (
                dataType is SystemInjectType.BottingModel
                && data is AbstractBottingModel bottingModel
            )
            {
                _bottingModel = bottingModel;
            }
        }
    }
}
