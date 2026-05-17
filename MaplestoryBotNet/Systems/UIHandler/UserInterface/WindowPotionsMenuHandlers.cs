using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.Xaml;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public enum PotionResourceType
    {
        Health = 0,
        Mana,
        MaxNum
    }


    public abstract class AbstractPotionsMenuState
    {
        public abstract void SetEditingState(int state);

        public abstract int GetEditingState();
    }


    public abstract class AbstractConsumableStackPanelFactory
    {
        public abstract StackPanel Create();
    }


    public class PotionsMenuState : AbstractPotionsMenuState
    {
        public int _editingTextBoxes = 0;

        public override void SetEditingState(int editingTextBoxes)
        {
            _editingTextBoxes = editingTextBoxes;
        }

        public override int GetEditingState()
        {
            return _editingTextBoxes;
        }
    }


    public class PotionsMenuScreenCaptureSubscriber : AbstractScreenCaptureSubscriber
    {
        private List<AbstractWindowActionHandler> _resourceBarActionHandlers;

        public PotionsMenuScreenCaptureSubscriber(
            SemaphoreSlim semaphore
        ) : base(semaphore)
        {
            _resourceBarActionHandlers = [];
        }

        public override void ProcessImage()
        {
            foreach (var handler in _resourceBarActionHandlers)
            {
                handler.Inject(0, _image);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ActionHandler &&
                data is AbstractWindowActionHandler actionHandler &&
                actionHandler.Modifier() is AbstractWindowStateModifier modifier &&
                modifier.State(0) is PotionResourceType
            )
            {
                _resourceBarActionHandlers.Add(actionHandler);
            }
        }
    }


    public class WindowPotionsMenuLoadingResourceModifier :
        AbstractWindowStateModifier
    {
        private TextBox _pixelThresholdRTextBox;

        private TextBox _pixelThresholdGTextBox;

        private TextBox _pixelThresholdBTextBox;

        private TextBox _pixelToleranceRTextBox;

        private TextBox _pixelToleranceGTextBox;

        private TextBox _pixelToleranceBTextBox;

        private TextBox _pixelRelativeXTextBox;

        private TextBox _pixelRelativeYTextBox;

        private TextBox _healthBarLeftTextBox;

        private TextBox _healthBarTopTextBox;

        private TextBox _healthBarRightTextBox;

        private TextBox _healthBarBottomTextBox;

        private TextBox _keyTextBox;

        private CheckBox _activeCheckBox;

        public WindowPotionsMenuLoadingResourceModifier(
            TextBox pixelThresholdRTextBox,
            TextBox pixelThresholdGTextBox,
            TextBox pixelThresholdBTextBox,
            TextBox pixelToleranceRTextBox,
            TextBox pixelToleranceGTextBox,
            TextBox pixelToleranceBTextBox,
            TextBox pixelRelativeXTextBox,
            TextBox pixelRelativeYTextBox,
            TextBox healthBarLeftTextBox,
            TextBox healthBarTopTextBox,
            TextBox healthBarRightTextBox,
            TextBox healthBarBottomTextBox,
            TextBox keyTextBox,
            CheckBox activeCheckBox
        )
        {
            _pixelThresholdRTextBox = pixelThresholdRTextBox;
            _pixelThresholdGTextBox = pixelThresholdGTextBox;
            _pixelThresholdBTextBox = pixelThresholdBTextBox;
            _pixelToleranceRTextBox = pixelToleranceRTextBox;
            _pixelToleranceGTextBox = pixelToleranceGTextBox;
            _pixelToleranceBTextBox = pixelToleranceBTextBox;
            _pixelRelativeXTextBox = pixelRelativeXTextBox;
            _pixelRelativeYTextBox = pixelRelativeYTextBox;
            _healthBarLeftTextBox = healthBarLeftTextBox;
            _healthBarTopTextBox = healthBarTopTextBox;
            _healthBarRightTextBox = healthBarRightTextBox;
            _healthBarBottomTextBox = healthBarBottomTextBox;
            _keyTextBox = keyTextBox;
            _activeCheckBox = activeCheckBox;
        }

        public override void Modify(object? value)
        {
            if (value is not Resource resource)
            {
                return;
            }
            _pixelThresholdRTextBox.Text = resource.Rgb[0].ToString();
            _pixelThresholdGTextBox.Text = resource.Rgb[1].ToString();
            _pixelThresholdBTextBox.Text = resource.Rgb[2].ToString();
            _pixelToleranceRTextBox.Text = resource.Tolerance[0].ToString();
            _pixelToleranceGTextBox.Text = resource.Tolerance[1].ToString();
            _pixelToleranceBTextBox.Text = resource.Tolerance[2].ToString();
            _pixelRelativeXTextBox.Text = resource.Pixel[0].ToString();
            _pixelRelativeYTextBox.Text = resource.Pixel[1].ToString();
            _healthBarLeftTextBox.Text = resource.Rect[0].ToString();
            _healthBarTopTextBox.Text = resource.Rect[1].ToString();
            _healthBarRightTextBox.Text = resource.Rect[2].ToString();
            _healthBarBottomTextBox.Text = resource.Rect[3].ToString();
            _keyTextBox.Text = resource.Key;
            _activeCheckBox.IsChecked = resource.Active != 0;
        }
    }


    public class WindowPotionsMenuLoadingResourceActionHandler :
        AbstractWindowActionHandler
    {
        private PotionResourceType _resourceType;

        private AbstractWindowStateModifier _loadResourceModifier;

        private AbstractPotionsMenuState _potionsMenuState;

        private AbstractSystemWindow _potionsWindow;

        private MaplestoryBotConfiguration? _maplestoryBotConfiguration;

        public WindowPotionsMenuLoadingResourceActionHandler(
            PotionResourceType resourceType,
            AbstractWindowStateModifier loadResourceModifier,
            AbstractPotionsMenuState potionsMenuState,
            AbstractSystemWindow potionsWindow
        )
        {
            _resourceType = resourceType;
            _loadResourceModifier = loadResourceModifier;
            _potionsMenuState = potionsMenuState;
            _potionsWindow = potionsWindow;
            _maplestoryBotConfiguration = null;
            ((Window)_potionsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadResourceModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _maplestoryBotConfiguration = configuration;
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (
                _potionsWindow.Visible() &&
                _maplestoryBotConfiguration is MaplestoryBotConfiguration configuration &&
                (
                    _resourceType is PotionResourceType.Health ? configuration.Hp :
                    _resourceType is PotionResourceType.Mana ? configuration.Mp :
                    null
                ) is Resource resource
            )
            {
                _potionsMenuState.SetEditingState(1);
                _loadResourceModifier.Modify(resource);
                _potionsMenuState.SetEditingState(0);
            }
        }
    }


    public class WindowPotionsMenuLoadingResourceActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _loadingResourceActionHandler;

        public WindowPotionsMenuLoadingResourceActionHandlerFacade(
            PotionResourceType resourceType,
            TextBox pixelThresholdRTextBox,
            TextBox pixelThresholdGTextBox,
            TextBox pixelThresholdBTextBox,
            TextBox pixelToleranceRTextBox,
            TextBox pixelToleranceGTextBox,
            TextBox pixelToleranceBTextBox,
            TextBox pixelRelativeXTextBox,
            TextBox pixelRelativeYTextBox,
            TextBox healthBarLeftTextBox,
            TextBox healthBarTopTextBox,
            TextBox healthBarRightTextBox,
            TextBox healthBarBottomTextBox,
            TextBox keyTextBox,
            CheckBox activeCheckBox,
            AbstractPotionsMenuState potionsMenuState,
            AbstractSystemWindow potionsWindow
        )
        {
            _loadingResourceActionHandler = (
                new WindowPotionsMenuLoadingResourceActionHandler(
                    resourceType,
                    new WindowPotionsMenuLoadingResourceModifier(
                        pixelThresholdRTextBox,
                        pixelThresholdGTextBox,
                        pixelThresholdBTextBox,
                        pixelToleranceRTextBox,
                        pixelToleranceGTextBox,
                        pixelToleranceBTextBox,
                        pixelRelativeXTextBox,
                        pixelRelativeYTextBox,
                        healthBarLeftTextBox,
                        healthBarTopTextBox,
                        healthBarRightTextBox,
                        healthBarBottomTextBox,
                        keyTextBox,
                        activeCheckBox
                    ),
                    potionsMenuState,
                    potionsWindow
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadingResourceActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _loadingResourceActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _loadingResourceActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowPotionsMenuSavingResourceModifier :
        AbstractWindowStateModifier
    {
        private TextBox _pixelThresholdRTextBox;

        private TextBox _pixelThresholdGTextBox;

        private TextBox _pixelThresholdBTextBox;

        private TextBox _pixelToleranceRTextBox;

        private TextBox _pixelToleranceGTextBox;

        private TextBox _pixelToleranceBTextBox;

        private TextBox _pixelRelativeXTextBox;

        private TextBox _pixelRelativeYTextBox;

        private TextBox _healthBarLeftTextBox;

        private TextBox _healthBarTopTextBox;

        private TextBox _healthBarRightTextBox;

        private TextBox _healthBarBottomTextBox;

        private TextBox _keyTextBox;

        private CheckBox _activeCheckBox;

        public WindowPotionsMenuSavingResourceModifier(
            TextBox pixelThresholdRTextBox,
            TextBox pixelThresholdGTextBox,
            TextBox pixelThresholdBTextBox,
            TextBox pixelToleranceRTextBox,
            TextBox pixelToleranceGTextBox,
            TextBox pixelToleranceBTextBox,
            TextBox pixelRelativeXTextBox,
            TextBox pixelRelativeYTextBox,
            TextBox healthBarLeftTextBox,
            TextBox healthBarTopTextBox,
            TextBox healthBarRightTextBox,
            TextBox healthBarBottomTextBox,
            TextBox keyTextBox,
            CheckBox activeCheckBox
        )
        {
            _pixelThresholdRTextBox = pixelThresholdRTextBox;
            _pixelThresholdGTextBox = pixelThresholdGTextBox;
            _pixelThresholdBTextBox = pixelThresholdBTextBox;
            _pixelToleranceRTextBox = pixelToleranceRTextBox;
            _pixelToleranceGTextBox = pixelToleranceGTextBox;
            _pixelToleranceBTextBox = pixelToleranceBTextBox;
            _pixelRelativeXTextBox = pixelRelativeXTextBox;
            _pixelRelativeYTextBox = pixelRelativeYTextBox;
            _healthBarLeftTextBox = healthBarLeftTextBox;
            _healthBarTopTextBox = healthBarTopTextBox;
            _healthBarRightTextBox = healthBarRightTextBox;
            _healthBarBottomTextBox = healthBarBottomTextBox;
            _keyTextBox = keyTextBox;
            _activeCheckBox = activeCheckBox;
        }

        public override void Modify(object? value)
        {
            if (value is not Resource resource)
            {
                return;
            }
            resource.Rect = [
                int.Parse(_healthBarLeftTextBox.Text == "" ? "0" : _healthBarLeftTextBox.Text),
                int.Parse(_healthBarTopTextBox.Text == "" ? "0" : _healthBarTopTextBox.Text),
                int.Parse(_healthBarRightTextBox.Text == "" ? "0" : _healthBarRightTextBox.Text),
                int.Parse(_healthBarBottomTextBox.Text == "" ? "0" : _healthBarBottomTextBox.Text),
            ];
            resource.Pixel = [
                int.Parse(_pixelRelativeXTextBox.Text == "" ? "0" : _pixelRelativeXTextBox.Text),
                int.Parse(_pixelRelativeYTextBox.Text == "" ? "0" : _pixelRelativeYTextBox.Text)
            ];
            resource.Rgb = [
                int.Parse(_pixelThresholdRTextBox.Text == "" ? "0" : _pixelThresholdRTextBox.Text),
                int.Parse(_pixelThresholdGTextBox.Text == "" ? "0" : _pixelThresholdGTextBox.Text),
                int.Parse(_pixelThresholdBTextBox.Text == "" ? "0" : _pixelThresholdBTextBox.Text)
            ];
            resource.Tolerance = [
                int.Parse(_pixelToleranceRTextBox.Text == "" ? "0" : _pixelToleranceRTextBox.Text),
                int.Parse(_pixelToleranceGTextBox.Text == "" ? "0" : _pixelToleranceGTextBox.Text),
                int.Parse(_pixelToleranceBTextBox.Text == "" ? "0" : _pixelToleranceBTextBox.Text)
            ];
            resource.Key = _keyTextBox.Text;
            resource.Active = _activeCheckBox.IsChecked is true ? 1 : 0;
        }
    }


    public class WindowPotionsMenuSavingResourceActionHandler :
        AbstractWindowActionHandler
    {
        private PotionResourceType _resourceType;

        private AbstractWindowStateModifier _savingResourceModifier;

        private AbstractSystemWindow _potionsWindow;

        private MaplestoryBotConfiguration? _maplestoryBotConfiguration;

        public WindowPotionsMenuSavingResourceActionHandler(
            PotionResourceType resourceType,
            AbstractWindowStateModifier savingResourceModifier,
            AbstractSystemWindow potionsWindow
        )
        {
            _resourceType = resourceType;
            _savingResourceModifier = savingResourceModifier;
            _potionsWindow = potionsWindow;
            _maplestoryBotConfiguration = null;
            ((Window)_potionsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;

        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _savingResourceModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _maplestoryBotConfiguration = configuration;
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (
                !_potionsWindow.Visible() &&
                _maplestoryBotConfiguration is MaplestoryBotConfiguration configuration &&
                (
                    _resourceType is PotionResourceType.Health ? configuration.Hp :
                    _resourceType is PotionResourceType.Mana ? configuration.Mp :
                    null
                ) is Resource resource
            )
            {
                _savingResourceModifier.Modify(resource);
            }
        }
    }


    public class WindowPotionsMenuSavingResourceActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _savingResourceActionHandler;

        public WindowPotionsMenuSavingResourceActionHandlerFacade(
            PotionResourceType resourceType,
            TextBox pixelThresholdRTextBox,
            TextBox pixelThresholdGTextBox,
            TextBox pixelThresholdBTextBox,
            TextBox pixelToleranceRTextBox,
            TextBox pixelToleranceGTextBox,
            TextBox pixelToleranceBTextBox,
            TextBox pixelRelativeXTextBox,
            TextBox pixelRelativeYTextBox,
            TextBox healthBarLeftTextBox,
            TextBox healthBarTopTextBox,
            TextBox healthBarRightTextBox,
            TextBox healthBarBottomTextBox,
            TextBox keyTextBox,
            CheckBox activeCheckBox,
            AbstractSystemWindow potionsWindow
        )
        {
            _savingResourceActionHandler = (
                new WindowPotionsMenuSavingResourceActionHandler(
                    resourceType,
                    new WindowPotionsMenuSavingResourceModifier(
                        pixelThresholdRTextBox,
                        pixelThresholdGTextBox,
                        pixelThresholdBTextBox,
                        pixelToleranceRTextBox,
                        pixelToleranceGTextBox,
                        pixelToleranceBTextBox,
                        pixelRelativeXTextBox,
                        pixelRelativeYTextBox,
                        healthBarLeftTextBox,
                        healthBarTopTextBox,
                        healthBarRightTextBox,
                        healthBarBottomTextBox,
                        keyTextBox,
                        activeCheckBox
                    ),
                    potionsWindow
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _savingResourceActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _savingResourceActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _savingResourceActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowPotionsMenuTextBoxFrameRGBModifier :
        AbstractWindowStateModifier
    {
        private TextBox _textBoxR;

        private TextBox _textBoxG;

        private TextBox _textBoxB;

        private Frame _frameR;

        private Frame _frameG;

        private Frame _frameB;

        public WindowPotionsMenuTextBoxFrameRGBModifier(
            TextBox textBoxR,
            TextBox textBoxG,
            TextBox textBoxB,
            Frame frameR,
            Frame frameG,
            Frame frameB
        )
        {
            _textBoxR = textBoxR;
            _textBoxG = textBoxG;
            _textBoxB = textBoxB;
            _frameR = frameR;
            _frameG = frameG;
            _frameB = frameB;
        }

        public override void Modify(object? value)
        {
            var r = byte.Parse(_textBoxR.Text != "" ? _textBoxR.Text : "0");
            var g = byte.Parse(_textBoxG.Text != "" ? _textBoxG.Text : "0");
            var b = byte.Parse(_textBoxB.Text != "" ? _textBoxB.Text : "0");
            var brushR = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, 0, 0));
            var brushG = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, g, 0));
            var brushB = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, b));
            _frameR.Background = brushR;
            _frameG.Background = brushG;
            _frameB.Background = brushB;
        }
    }


    public class WindowPotionsMenuTextBoxFrameRGBActionHandler :
        AbstractWindowActionHandler
    {
        private TextBox _textBoxR;

        private TextBox _textBoxG;

        private TextBox _textBoxB;

        private AbstractWindowStateModifier _textBoxFrameRgbModifier;

        public WindowPotionsMenuTextBoxFrameRGBActionHandler(
            TextBox textBoxR,
            TextBox textBoxG,
            TextBox textBoxB,
            AbstractWindowStateModifier textBoxFrameRgbModifier
        )
        {
            _textBoxR = textBoxR;
            _textBoxG = textBoxG;
            _textBoxB = textBoxB;
            _textBoxR.TextChanged += OnEvent;
            _textBoxG.TextChanged += OnEvent;
            _textBoxB.TextChanged += OnEvent;
            _textBoxFrameRgbModifier = textBoxFrameRgbModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _textBoxFrameRgbModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _textBoxFrameRgbModifier.Modify(null);
        }
    }


    public class WindowPotionsMenuTextBoxFrameRGBActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _textBoxFrameRGBActionHandler;

        public WindowPotionsMenuTextBoxFrameRGBActionHandlerFacade(
            TextBox textBoxR,
            TextBox textBoxG,
            TextBox textBoxB,
            Frame frameR,
            Frame frameG,
            Frame frameB
        )
        {
            _textBoxFrameRGBActionHandler = (
                new WindowPotionsMenuTextBoxFrameRGBActionHandler(
                    textBoxR,
                    textBoxG,
                    textBoxB,
                    new WindowPotionsMenuTextBoxFrameRGBModifier(
                        textBoxR,
                        textBoxG,
                        textBoxB,
                        frameR,
                        frameG,
                        frameB
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _textBoxFrameRGBActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _textBoxFrameRGBActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowPotionsMenuResourceBarModifier :
        AbstractWindowStateModifier
    {
        private PotionResourceType _resourceType;

        private TextBox _leftTextBox;

        private TextBox _topTextBox;

        private TextBox _rightTextBox;

        private TextBox _bottomTextBox;

        private System.Windows.Controls.Image _resourceBarImage;

        private AbstractDispatcher _dispatcher;

        private byte[] _pixelBuffer;

        private bool _isUpdating;

        public WindowPotionsMenuResourceBarModifier(
            PotionResourceType resourceType,
            TextBox leftTextBox,
            TextBox topTextBox,
            TextBox rightTextBox,
            TextBox bottomTextBox,
            System.Windows.Controls.Image resourceBarImage,
            AbstractDispatcher dispatcher
        )
        {
            _resourceType = resourceType;
            _leftTextBox = leftTextBox;
            _topTextBox = topTextBox;
            _rightTextBox = rightTextBox;
            _bottomTextBox = bottomTextBox;
            _resourceBarImage = resourceBarImage;
            _dispatcher = dispatcher;
            _pixelBuffer = null!;
            _isUpdating = false;
        }

        private BitmapSource _toBitmapSource(Image<Bgra32> image)
        {
            int bufferSize = image.Width * image.Height * 4;
            if (_pixelBuffer == null || _pixelBuffer.Length != bufferSize)
            {
                _pixelBuffer = new byte[bufferSize];
            }
            image.CopyPixelDataTo(_pixelBuffer);
            return BitmapSource.Create(
                image.Width,
                image.Height,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                _pixelBuffer,
                image.Width * 4
            );
        }

        public override void Modify(object? value)
        {
            if (_isUpdating)
            {
                return;
            }
            _isUpdating = true;
            _dispatcher.Dispatch(
                () =>
                {
                    if (value is not Image<Bgra32> image)
                    {
                        return;
                    }
                    var left = int.Parse(_leftTextBox.Text == "" ? "0" : _leftTextBox.Text);
                    var top = int.Parse(_topTextBox.Text == "" ? "0" : _topTextBox.Text);
                    var right = int.Parse(_rightTextBox.Text == "" ? "0" : _rightTextBox.Text);
                    var bottom = int.Parse(_bottomTextBox.Text == "" ? "0" : _bottomTextBox.Text);
                    var x = Math.Min(left, image.Width);
                    var y = Math.Min(top, image.Height);
                    var width = Math.Max(right - left, 0);
                    var height = Math.Max(bottom - top, 0);
                    if (width > 0 && height > 0)
                    {
                        var cropRectangle = new Rectangle(x, y, width, height);
                        using var croppedImage = image.Clone(ctx => ctx.Crop(cropRectangle));
                        var bitmapSource = _toBitmapSource(croppedImage);
                        _resourceBarImage.Source = bitmapSource;
                    }
                    _isUpdating = false;
                }
            );
        }

        public override object? State(int stateType)
        {
            return _resourceType;
        }
    }


    public class WindowPotionsMenuResourceBarActionHandler :
        AbstractWindowActionHandler
    {
        private AbstractSystemWindow _potionsWindow;

        private AbstractWindowStateModifier _resourceBarModifier;

        public WindowPotionsMenuResourceBarActionHandler(
            AbstractSystemWindow potionsWindow,
            AbstractWindowStateModifier resourceBarModifier
        )
        {
            _potionsWindow = potionsWindow;
            _resourceBarModifier = resourceBarModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _resourceBarModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.InjectAction &&
                data is AbstractInjectAction injectAction
            )
            {
                injectAction.GetAction()(SystemInjectType.ActionHandler, this);
            }
            if (data is Image<Bgra32> image)
            {
                if (_potionsWindow.Visible())
                {
                    _resourceBarModifier.Modify(image);
                }
            }
        }
    }


    public class WindowPotionsMenuResourceBarActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _resourceBarActionHandler;

        public WindowPotionsMenuResourceBarActionHandlerFacade(
            PotionResourceType resourceType,
            TextBox leftTextBox,
            TextBox topTextBox,
            TextBox rightTextBox,
            TextBox bottomTextBox,
            System.Windows.Controls.Image resourceBarImage,
            AbstractDispatcher dispatcher,
            AbstractSystemWindow potionsWindow
        )
        {
            _resourceBarActionHandler = (
                new WindowPotionsMenuResourceBarActionHandler(
                    potionsWindow,
                    new WindowPotionsMenuResourceBarModifier(
                        resourceType,
                        leftTextBox,
                        topTextBox,
                        rightTextBox,
                        bottomTextBox,
                        resourceBarImage,
                        dispatcher
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _resourceBarActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _resourceBarActionHandler.Inject(dataType, data);
        }
    }


    public class WindowPotionsMenuRGBLabelModifier :
        AbstractWindowStateModifier
    {
        private PotionResourceType _resourceType;

        private TextBox _textBoxX;

        private TextBox _textBoxY;

        private TextBox _textBoxLeft;

        private TextBox _textBoxTop;

        private Label _labelR;

        private Label _labelG;

        private Label _labelB;

        private AbstractDispatcher _dispatcher;

        private bool _isUpdating;

        public WindowPotionsMenuRGBLabelModifier(
            PotionResourceType resourceType,
            TextBox textBoxX,
            TextBox textBoxY,
            TextBox textBoxLeft,
            TextBox textBoxTop,
            Label labelR,
            Label labelG,
            Label labelB,
            AbstractDispatcher dispatcher
        )
        {
            _resourceType = resourceType;
            _textBoxX = textBoxX;
            _textBoxY = textBoxY;
            _textBoxLeft = textBoxLeft;
            _textBoxTop = textBoxTop;
            _labelR = labelR;
            _labelG = labelG;
            _labelB = labelB;
            _dispatcher = dispatcher;
            _isUpdating = false;
        }

        public override void Modify(object? value)
        {
            if (_isUpdating)
            {
                return;
            }
            _isUpdating = true;
            _dispatcher.Dispatch(
                () =>
                {
                    if (value is not Image<Bgra32> image)
                    {
                        return;
                    }
                    var left = int.Parse(_textBoxLeft.Text == "" ? "0" : _textBoxLeft.Text);
                    var top = int.Parse(_textBoxTop.Text == "" ? "0" : _textBoxTop.Text);
                    var relX = int.Parse(_textBoxX.Text == "" ? "0" : _textBoxX.Text);
                    var relY = int.Parse(_textBoxY.Text == "" ? "0" : _textBoxY.Text);
                    var x = left + relX;
                    var y = top + relY;
                    if (x < image.Width && y < image.Height)
                    {
                        var pixel = image[x, y];
                        _labelR.Content = pixel.R.ToString();
                        _labelG.Content = pixel.G.ToString();
                        _labelB.Content = pixel.B.ToString();
                    }
                    _isUpdating = false;
                }
            );
        }

        public override object? State(int stateType)
        {
            return _resourceType;
        }
    }


    public class WindowPotionsMenuRGBLabelActionHandler :
        AbstractWindowActionHandler
    {
        private AbstractSystemWindow _potionsWindow;

        private AbstractWindowStateModifier _rgbLabelModifier;

        public WindowPotionsMenuRGBLabelActionHandler(
            AbstractSystemWindow potionsWindow,
            AbstractWindowStateModifier resourceBarModifier
        )
        {
            _potionsWindow = potionsWindow;
            _rgbLabelModifier = resourceBarModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _rgbLabelModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.InjectAction &&
                data is AbstractInjectAction injectAction
            )
            {
                injectAction.GetAction()(SystemInjectType.ActionHandler, this);
            }
            if (data is Image<Bgra32> image)
            {
                if (_potionsWindow.Visible())
                {
                    _rgbLabelModifier.Modify(image);
                }
            }
        }
    }


    public class WindowPotionsMenuRGBLabelActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _resourceBarActionHandler;

        public WindowPotionsMenuRGBLabelActionHandlerFacade(
            PotionResourceType resourceType,
            TextBox textBoxX,
            TextBox textBoxY,
            TextBox textBoxLeft,
            TextBox textBoxTop,
            Label labelR,
            Label labelG,
            Label labelB,
            AbstractDispatcher dispatcher,
            AbstractSystemWindow potionsWindow
        )
        {
            _resourceBarActionHandler = (
                new WindowPotionsMenuRGBLabelActionHandler(
                    potionsWindow,
                    new WindowPotionsMenuRGBLabelModifier(
                        resourceType,
                        textBoxX,
                        textBoxY,
                        textBoxLeft,
                        textBoxTop,
                        labelR,
                        labelG,
                        labelB,
                        dispatcher
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _resourceBarActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _resourceBarActionHandler.Inject(dataType, data);
        }
    }


    public class WindowPotionsMenuRGBFrameModifier : AbstractWindowStateModifier
    {
        private Label _labelR;

        private Label _labelG;

        private Label _labelB;

        private Frame _frameR;

        private Frame _frameG;

        private Frame _frameB;

        private Frame _framePixel;

        public WindowPotionsMenuRGBFrameModifier(
            Label labelR,
            Label labelG,
            Label labelB,
            Frame frameR,
            Frame frameG,
            Frame frameB,
            Frame framePixel
        )
        {
            _labelR = labelR;
            _labelG = labelG;
            _labelB = labelB;
            _frameR = frameR;
            _frameG = frameG;
            _frameB = frameB;
            _framePixel = framePixel;
        }

        public override void Modify(object? value)
        {
            var r = byte.Parse((string)_labelR.Content);
            var g = byte.Parse((string)_labelG.Content);
            var b = byte.Parse((string)_labelB.Content);
            _frameR.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, 0, 0));
            _frameG.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, g, 0));
            _frameB.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, b));
            _framePixel.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        }
    }


    public class WindowPotionsMenuRGBFrameActionHandler : AbstractWindowActionHandler
    {
        private Label _labelR;

        private Label _labelG;

        private Label _labelB;

        private AbstractWindowStateModifier _rgbFrameModifier;

        public WindowPotionsMenuRGBFrameActionHandler(
            Label labelR,
            Label labelG,
            Label labelB,
            AbstractWindowStateModifier rgbFrameModifier
        )
        {
            _labelR = labelR;
            _labelG = labelG;
            _labelB = labelB;
            _rgbFrameModifier = rgbFrameModifier;
            _attachHandler(_labelR);
            _attachHandler(_labelG);
            _attachHandler(_labelB);
        }

        private void _attachHandler(Label label)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(
                Label.ContentProperty, typeof(Label)
            );
            descriptor.AddValueChanged(label, OnEvent);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _rgbFrameModifier.Modify(null);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _rgbFrameModifier;
        }
    }


    public class WindowPotionsMenuRGBFrameActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _rgbFrameActionHandler;

        public WindowPotionsMenuRGBFrameActionHandlerFacade(
            Label labelR,
            Label labelG,
            Label labelB,
            Frame frameR,
            Frame frameG,
            Frame frameB,
            Frame framePixel
        )
        {
            _rgbFrameActionHandler = new WindowPotionsMenuRGBFrameActionHandler(
                labelR,
                labelG,
                labelB,
                new WindowPotionsMenuRGBFrameModifier(
                    labelR,
                    labelG,
                    labelB,
                    frameR,
                    frameG,
                    frameB,
                    framePixel
                )
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _rgbFrameActionHandler.OnEvent(sender, e);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _rgbFrameActionHandler.Modifier();
        }
    }


    public class WindowPotionsMenuConfigurationUpdateActionHandler : AbstractWindowActionHandler
    {
        private PotionResourceType _resourceType;

        private TextBox _pixelThresholdRTextBox;

        private TextBox _pixelThresholdGTextBox;

        private TextBox _pixelThresholdBTextBox;

        private TextBox _pixelToleranceRTextBox;

        private TextBox _pixelToleranceGTextBox;

        private TextBox _pixelToleranceBTextBox;

        private TextBox _pixelRelativeXTextBox;

        private TextBox _pixelRelativeYTextBox;

        private TextBox _healthBarLeftTextBox;

        private TextBox _healthBarTopTextBox;

        private TextBox _healthBarRightTextBox;

        private TextBox _healthBarBottomTextBox;

        private TextBox _keyTextBox;

        private CheckBox _activeCheckBox;

        private AbstractPotionsMenuState _potionsMenuState;

        private AbstractWindowStateModifier _configurationModifier;

        private MaplestoryBotConfiguration? _maplestoryBotConfiguration;

        public WindowPotionsMenuConfigurationUpdateActionHandler(
            PotionResourceType resourceType,
            TextBox pixelThresholdRTextBox,
            TextBox pixelThresholdGTextBox,
            TextBox pixelThresholdBTextBox,
            TextBox pixelToleranceRTextBox,
            TextBox pixelToleranceGTextBox,
            TextBox pixelToleranceBTextBox,
            TextBox pixelRelativeXTextBox,
            TextBox pixelRelativeYTextBox,
            TextBox healthBarLeftTextBox,
            TextBox healthBarTopTextBox,
            TextBox healthBarRightTextBox,
            TextBox healthBarBottomTextBox,
            TextBox keyTextBox,
            CheckBox activeCheckBox,
            AbstractPotionsMenuState potionsMenuState,
            AbstractWindowStateModifier configurationModifier
        )
        {
            _resourceType = resourceType;
            _pixelThresholdRTextBox = pixelThresholdRTextBox;
            _pixelThresholdGTextBox = pixelThresholdGTextBox;
            _pixelThresholdBTextBox = pixelThresholdBTextBox;
            _pixelToleranceRTextBox = pixelToleranceRTextBox;
            _pixelToleranceGTextBox = pixelToleranceGTextBox;
            _pixelToleranceBTextBox = pixelToleranceBTextBox;
            _pixelRelativeXTextBox = pixelRelativeXTextBox;
            _pixelRelativeYTextBox = pixelRelativeYTextBox;
            _healthBarLeftTextBox = healthBarLeftTextBox;
            _healthBarTopTextBox = healthBarTopTextBox;
            _healthBarRightTextBox = healthBarRightTextBox;
            _healthBarBottomTextBox = healthBarBottomTextBox;
            _keyTextBox = keyTextBox;
            _activeCheckBox = activeCheckBox;
            _potionsMenuState = potionsMenuState;
            _configurationModifier = configurationModifier;
            _pixelThresholdRTextBox.TextChanged += OnEvent;
            _pixelThresholdGTextBox.TextChanged += OnEvent;
            _pixelThresholdBTextBox.TextChanged += OnEvent;
            _pixelToleranceRTextBox.TextChanged += OnEvent;
            _pixelToleranceGTextBox.TextChanged += OnEvent;
            _pixelToleranceBTextBox.TextChanged += OnEvent;
            _pixelRelativeXTextBox.TextChanged += OnEvent;
            _pixelRelativeYTextBox.TextChanged += OnEvent;
            _healthBarLeftTextBox.TextChanged += OnEvent;
            _healthBarTopTextBox.TextChanged += OnEvent;
            _healthBarRightTextBox.TextChanged += OnEvent;
            _healthBarBottomTextBox.TextChanged += OnEvent;
            _keyTextBox.TextChanged += OnEvent;
            _activeCheckBox.Checked += OnEvent;
            _activeCheckBox.Unchecked += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _configurationModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                _potionsMenuState.GetEditingState() == 0 &&
                _maplestoryBotConfiguration is MaplestoryBotConfiguration configuration &&
                (
                    _resourceType == PotionResourceType.Health ? _maplestoryBotConfiguration.Hp :
                    _resourceType == PotionResourceType.Mana ? _maplestoryBotConfiguration.Mp :
                    null
                ) is Resource resource
            )
            {
                _configurationModifier.Modify(resource);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                _maplestoryBotConfiguration = maplestoryBotConfiguration;
            }
        }
    }


    public class WindowPotionsMenuConfigurationUpdateActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _configurationActionHandler;

        public WindowPotionsMenuConfigurationUpdateActionHandlerFacade(
            PotionResourceType resourceType,
            TextBox pixelThresholdRTextBox,
            TextBox pixelThresholdGTextBox,
            TextBox pixelThresholdBTextBox,
            TextBox pixelToleranceRTextBox,
            TextBox pixelToleranceGTextBox,
            TextBox pixelToleranceBTextBox,
            TextBox pixelRelativeXTextBox,
            TextBox pixelRelativeYTextBox,
            TextBox healthBarLeftTextBox,
            TextBox healthBarTopTextBox,
            TextBox healthBarRightTextBox,
            TextBox healthBarBottomTextBox,
            TextBox keyTextBox,
            CheckBox activeCheckBox,
            AbstractPotionsMenuState potionsMenuState
        )
        {
            _configurationActionHandler = new WindowPotionsMenuConfigurationUpdateActionHandler(
                resourceType,
                pixelThresholdRTextBox,
                pixelThresholdGTextBox,
                pixelThresholdBTextBox,
                pixelToleranceRTextBox,
                pixelToleranceGTextBox,
                pixelToleranceBTextBox,
                pixelRelativeXTextBox,
                pixelRelativeYTextBox,
                healthBarLeftTextBox,
                healthBarTopTextBox,
                healthBarRightTextBox,
                healthBarBottomTextBox,
                keyTextBox,
                activeCheckBox,
                potionsMenuState,
                new WindowPotionsMenuSavingResourceModifier(
                    pixelThresholdRTextBox,
                    pixelThresholdGTextBox,
                    pixelThresholdBTextBox,
                    pixelToleranceRTextBox,
                    pixelToleranceGTextBox,
                    pixelToleranceBTextBox,
                    pixelRelativeXTextBox,
                    pixelRelativeYTextBox,
                    healthBarLeftTextBox,
                    healthBarTopTextBox,
                    healthBarRightTextBox,
                    healthBarBottomTextBox,
                    keyTextBox,
                    activeCheckBox
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _configurationActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _configurationActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _configurationActionHandler.Inject(dataType, data);
        }
    }


    public class WindowPotionsMenuSaveConfigurationModifier : AbstractWindowStateModifier
    {
        public override void Modify(object? value)
        {
            if (value == null)
            {
                return;
            }
            dynamic parameters = value;
            if (
                parameters.InjectAction is AbstractInjectAction injectAction
                && parameters.Configuration is MaplestoryBotConfiguration configuration
            )
            {
                var action = injectAction.GetAction();
                action(SystemInjectType.ConfigurationUpdate, configuration);
                action(SystemInjectType.ConfigurationSave, configuration);
            }
        }
    }


    public class WindowPotionsMenuSaveConfigurationActionHandler : AbstractWindowActionHandler
    {
        private ListBox _consumablesListBox;

        private AbstractSystemWindow _potionsWindow;

        private AbstractWindowStateModifier _potionsSaveModifier;

        private AbstractInjectAction? _injectAction;

        private AbstractConfiguration? _maplestoryBotConfiguration;

        public WindowPotionsMenuSaveConfigurationActionHandler(
            ListBox consumablesListBox,
            AbstractSystemWindow ailmentsWindow,
            AbstractWindowStateModifier _potionsSaveModifier
        )
        {
            _consumablesListBox = consumablesListBox;
            _potionsWindow = ailmentsWindow;
            this._potionsSaveModifier = _potionsSaveModifier;
            _injectAction = null;
            _maplestoryBotConfiguration = null;
            ((Window)_potionsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }


        public override AbstractWindowStateModifier Modifier()
        {
            return _potionsSaveModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                _maplestoryBotConfiguration = maplestoryBotConfiguration;
            }
            if (
                dataType is SystemInjectType.InjectAction
                && data is AbstractInjectAction injectAction
            )
            {
                _injectAction = injectAction;
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (!_potionsWindow.Visible())
            {
                _consumablesListBox.SelectedIndex = -1;
                _potionsSaveModifier.Modify(
                    new
                    {
                        Configuration = _maplestoryBotConfiguration,
                        InjectAction = _injectAction
                    }
                );
            }
        }
    }


    public class WindowPotionsMenuSaveConfigurationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _potionsSaveActionHandler;

        public WindowPotionsMenuSaveConfigurationActionHandlerFacade(
            ListBox consumablesListBox,
            AbstractSystemWindow potionsWindow
        )
        {
            _potionsSaveActionHandler = new WindowPotionsMenuSaveConfigurationActionHandler(
                consumablesListBox,
                potionsWindow,
                new WindowPotionsMenuSaveConfigurationModifier()
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _potionsSaveActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _potionsSaveActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _potionsSaveActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class ConsumableStackPanelFactory : AbstractConsumableStackPanelFactory
    {
        private StackPanel _consumableTemplate;

        public ConsumableStackPanelFactory(StackPanel consumableTemplate)
        {
            _consumableTemplate = consumableTemplate;
        }

        public override StackPanel Create()
        {
            var consumableTemplate = _consumableTemplate;
            var checkboxTemplate = consumableTemplate.Children.OfType<CheckBox>().First();
            var textBoxTemplate = consumableTemplate.Children.OfType<TextBox>().First();
            var stackPanel = new StackPanel
            {
                Orientation = consumableTemplate.Orientation,
                Focusable = consumableTemplate.Focusable
            };
            var checkBox = new CheckBox
            {
                VerticalContentAlignment = checkboxTemplate.VerticalContentAlignment,
            };
            var textBox = new TextBox
            {
                Margin = textBoxTemplate.Margin,
                VerticalContentAlignment = textBoxTemplate.VerticalContentAlignment,
                HorizontalContentAlignment = textBoxTemplate.HorizontalContentAlignment,
                Width = textBoxTemplate.Width,
                Height = textBoxTemplate.Height,
                Background = textBoxTemplate.Background,
                Foreground = textBoxTemplate.Foreground,
                FontFamily = textBoxTemplate.FontFamily
            };
            stackPanel.Children.Add(checkBox);
            stackPanel.Children.Add(textBox);
            return stackPanel;
        }
    }


    public class ConsumableDataTag
    {
        public Consumable Consumable = new Consumable();
    }


    public class WindowPotionsMenuLoadingConsumablesModifier : AbstractWindowStateModifier
    {
        private ListBox _consumablesListBox;

        private AbstractConsumableStackPanelFactory _consumableStackPanelFactory;

        public WindowPotionsMenuLoadingConsumablesModifier(
            ListBox consumablesListBox,
            AbstractConsumableStackPanelFactory stackPanelFactory
        )
        {
            _consumablesListBox = consumablesListBox;
            _consumableStackPanelFactory = stackPanelFactory;
        }

        public override void Modify(object? value)
        {
            if (value is not MaplestoryBotConfiguration configuration)
            {
                return;
            }
            _consumablesListBox.Items.Clear();
            foreach (var consumable in configuration.Consumables)
            {
                var stackPanel = _consumableStackPanelFactory.Create();
                var listBoxItem = new ListBoxItem
                {
                    Content = stackPanel,
                    Tag = new ConsumableDataTag { Consumable = consumable }
                };
                _consumablesListBox.Items.Add(listBoxItem);
                var textBox = stackPanel.Children.OfType<TextBox>().First();
                var checkBox = stackPanel.Children.OfType<CheckBox>().First();
                textBox.Text = consumable.Name;
                checkBox.IsChecked = consumable.Active != 0;
            }
            _consumablesListBox.SelectedIndex = 0;
        }
    }


    public class WindowPotionsMenuLoadingConsumablesActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _potionsWindow;

        private AbstractWindowStateModifier _loadingConsumablesModifier;

        private MaplestoryBotConfiguration? _maplestoryBotConfiguration;

        public WindowPotionsMenuLoadingConsumablesActionHandler(
            AbstractSystemWindow potionsWindow,
            AbstractWindowStateModifier loadingConsumablesModifier
        )
        {
            _potionsWindow = potionsWindow;
            _loadingConsumablesModifier = loadingConsumablesModifier;
            _maplestoryBotConfiguration = null;
            ((Window)_potionsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadingConsumablesModifier;
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (_potionsWindow.Visible())
            {
                if (_maplestoryBotConfiguration is MaplestoryBotConfiguration configuration)
                {
                    _loadingConsumablesModifier.Modify(configuration);
                }
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _maplestoryBotConfiguration = configuration;
            }
        }
    }


    public class WindowPotionsMenuLoadingConsumablesActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _loadingConsumablesActionHandler;

        public WindowPotionsMenuLoadingConsumablesActionHandlerFacade(
            ListBox consumablesListBox,
            StackPanel consumableTemplate,
            AbstractSystemWindow potionsWindow
        )
        {
            _loadingConsumablesActionHandler = (
                new WindowPotionsMenuLoadingConsumablesActionHandler(
                    potionsWindow,
                    new WindowPotionsMenuLoadingConsumablesModifier(
                        consumablesListBox,
                        new ConsumableStackPanelFactory(consumableTemplate)
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadingConsumablesActionHandler.Modifier();
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _loadingConsumablesActionHandler.OnDependencyEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _loadingConsumablesActionHandler.Inject(dataType, data);
        }
    }


    public class WindowPotionsMenuConsumableSelectedModifier : AbstractWindowStateModifier
    {
        private TextBox _minDelayTextBox;

        private TextBox _maxDelayTextBox;

        private TextBox _keyTextBox;

        public WindowPotionsMenuConsumableSelectedModifier(
            TextBox minDelayTextBox,
            TextBox maxDelayTextBox,
            TextBox keyTextBox
        )
        {
            _minDelayTextBox = minDelayTextBox;
            _maxDelayTextBox = maxDelayTextBox;
            _keyTextBox = keyTextBox;
        }

        public override void Modify(object? value)
        {
            if (value is ListBoxItem selectedItem)
            {
                var dataTag = (ConsumableDataTag)selectedItem.Tag;
                var consumable = dataTag.Consumable;
                _minDelayTextBox.Text = consumable.MinDelay.ToString();
                _maxDelayTextBox.Text = consumable.MaxDelay.ToString();
                _keyTextBox.Text = consumable.Key;
            }
        }
    }


    public class WindowPotionsMenuConsumableSelectedActionHandler :
        AbstractWindowActionHandler
    {
        private ListBox _consumablesListBox;

        private AbstractWindowStateModifier _consumableSelectedModifier;

        public WindowPotionsMenuConsumableSelectedActionHandler(
            ListBox consumablesListBox,
            AbstractWindowStateModifier consumableSelectedModifier
        )
        {
            _consumablesListBox = consumablesListBox;
            _consumablesListBox.SelectionChanged += OnEvent;
            _consumableSelectedModifier = consumableSelectedModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _consumableSelectedModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs &&
                selectionArgs.AddedItems.Count > 0
            )
            {
                _consumableSelectedModifier.Modify(selectionArgs.AddedItems[0]);
            }
        }
    }


    public class WindowPotionsMenuConsumableSelectedActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _consumableSelectedActionHandler;

        public WindowPotionsMenuConsumableSelectedActionHandlerFacade(
            TextBox minDelayTextBox,
            TextBox maxDelayTextBox,
            TextBox keyTextBox,
            ListBox consumablesListBox
        )
        {
            _consumableSelectedActionHandler = (
                new WindowPotionsMenuConsumableSelectedActionHandler(
                    consumablesListBox,
                    new WindowPotionsMenuConsumableSelectedModifier(
                        minDelayTextBox,
                        maxDelayTextBox,
                        keyTextBox
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _consumableSelectedActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _consumableSelectedActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowPotionsMenuConsumableDeselectedModifier :
        AbstractWindowStateModifier
    {
        private TextBox _minDelayTextBox;

        private TextBox _maxDelayTextBox;

        private TextBox _keyTextBox;

        public WindowPotionsMenuConsumableDeselectedModifier(
            TextBox minDelayTextBox,
            TextBox maxDelayTextBox,
            TextBox keyTextBox
        )
        {
            _minDelayTextBox = minDelayTextBox;
            _maxDelayTextBox = maxDelayTextBox;
            _keyTextBox = keyTextBox;
        }

        public override void Modify(object? value)
        {
            if (value is ListBoxItem deselectedItem)
            {
                var dataTag = (ConsumableDataTag)deselectedItem.Tag;
                dataTag.Consumable.MinDelay = int.Parse(_minDelayTextBox.Text);
                dataTag.Consumable.MaxDelay = int.Parse(_maxDelayTextBox.Text);
                dataTag.Consumable.Key = _keyTextBox.Text;
            }
        }
    }


    public class WindowPotionsMenuConsumableDeselectedActionHandler :
        AbstractWindowActionHandler
    {
        private ListBox _consumablesListBox;

        private AbstractWindowStateModifier _consumableDeselectedModifier;

        public WindowPotionsMenuConsumableDeselectedActionHandler(
            ListBox consumablesListBox,
            AbstractWindowStateModifier consumableDeselectedModifier
        )
        {
            _consumablesListBox = consumablesListBox;
            _consumableDeselectedModifier = consumableDeselectedModifier;
            _consumablesListBox.SelectionChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _consumableDeselectedModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs &&
                selectionArgs.RemovedItems.Count > 0
            )
            {
                _consumableDeselectedModifier.Modify(selectionArgs.RemovedItems[0]);
            }
        }
    }


    public class WindowPotionsMenuConsumableDeselectedActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _consumableDeselectedActionHandler;

        public WindowPotionsMenuConsumableDeselectedActionHandlerFacade(
            TextBox minDelayTextBox,
            TextBox maxDelayTextBox,
            TextBox keyTextBox,
            ListBox consumablesListBox
        )
        {
            _consumableDeselectedActionHandler = (
                new WindowPotionsMenuConsumableDeselectedActionHandler(
                    consumablesListBox,
                    new WindowPotionsMenuConsumableDeselectedModifier(
                        minDelayTextBox,
                        maxDelayTextBox,
                        keyTextBox
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _consumableDeselectedActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _consumableDeselectedActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowPotionsMenuConsumableItemSaveModifier :
        AbstractWindowStateModifier
    {
        private ListBox _consumablesListBox;

        public WindowPotionsMenuConsumableItemSaveModifier(
            ListBox consumablesListBox
        )
        {
            _consumablesListBox = consumablesListBox;
        }

        public override void Modify(object? value)
        {
            foreach (ListBoxItem listBoxItem in _consumablesListBox.Items)
            {
                var dataTag = (ConsumableDataTag)listBoxItem.Tag;
                var stackPanel = (StackPanel)listBoxItem.Content;
                var checkBox = stackPanel.Children.OfType<CheckBox>().First();
                var textBox = stackPanel.Children.OfType<TextBox>().First();
                dataTag.Consumable.Active = (bool)checkBox.IsChecked! ? 1 : 0;
                dataTag.Consumable.Name = textBox.Text;
            }
        }
    }

    public class WindowPotionsMenuConsumableItmSaveActionHandler :
        AbstractWindowActionHandler
    {
        private AbstractSystemWindow _potionsWindow;

        private AbstractWindowStateModifier _consumableCheckboxSaveModifier;

        public WindowPotionsMenuConsumableItmSaveActionHandler(
            AbstractSystemWindow potionsWindow,
            AbstractWindowStateModifier consumableCheckBoxSaveModifier
        )
        {
            _potionsWindow = potionsWindow;
            ((Window)_potionsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
            _consumableCheckboxSaveModifier = consumableCheckBoxSaveModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _consumableCheckboxSaveModifier;
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (!_potionsWindow.Visible())
            {
                _consumableCheckboxSaveModifier.Modify(null);
            }
        }
    }


    public class WindowPotionsMenuConsumableItemSaveActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _consumableCheckboxSaveActionHandler;

        public WindowPotionsMenuConsumableItemSaveActionHandlerFacade(
            ListBox consumablesListBox,
            AbstractSystemWindow potionsWindow
        )
        {
            _consumableCheckboxSaveActionHandler = (
                new WindowPotionsMenuConsumableItmSaveActionHandler(
                    potionsWindow,
                    new WindowPotionsMenuConsumableItemSaveModifier(consumablesListBox)
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _consumableCheckboxSaveActionHandler.Modifier();
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _consumableCheckboxSaveActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowPotionsMenuConsumableAddModifier : AbstractWindowStateModifier
    {
        private ListBox _consumableListBox;

        private AbstractConsumableStackPanelFactory _consumableStackPanelFactory;

        public WindowPotionsMenuConsumableAddModifier(
            ListBox consumableListBox,
            AbstractConsumableStackPanelFactory consumableStackPanelFactory
        )
        {
            _consumableListBox = consumableListBox;
            _consumableStackPanelFactory = consumableStackPanelFactory;
        }

        public override void Modify(object? value)
        {
            if (value is not MaplestoryBotConfiguration configuration)
            {
                return;
            }
            var selectedIndex = _consumableListBox.SelectedIndex;
            var consumable = new Consumable { Name = "petfood" };
            var stackPanel = _consumableStackPanelFactory.Create();
            var textBox = stackPanel.Children.OfType<TextBox>().First();
            var listBoxItem = new ListBoxItem
            {
                Content = stackPanel,
                Tag = new ConsumableDataTag { Consumable = consumable }
            };
            if (selectedIndex == -1)
            {
                configuration.Consumables.Add(consumable);
                _consumableListBox.Items.Add(listBoxItem);
            }
            else
            {
                configuration.Consumables.Insert(selectedIndex + 1, consumable);
                _consumableListBox.Items.Insert(selectedIndex + 1, listBoxItem);
            }
            textBox.Text = consumable.Name;
        }
    }


    public class WindowPotionsMenuConsumableAddActionHandler : AbstractWindowActionHandler
    {
        private Button _addConsumableButton;

        private AbstractWindowStateModifier _consumableAddModifier;

        private MaplestoryBotConfiguration? _maplestoryBotConfiguration;

        public WindowPotionsMenuConsumableAddActionHandler(
            Button addConsumableButton,
            AbstractWindowStateModifier consumableAddModifier
        )
        {
            _addConsumableButton = addConsumableButton;
            _consumableAddModifier = consumableAddModifier;
            _maplestoryBotConfiguration = null;
            _addConsumableButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _consumableAddModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                _maplestoryBotConfiguration = maplestoryBotConfiguration;
            }
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_maplestoryBotConfiguration != null)
            {
                _consumableAddModifier.Modify(_maplestoryBotConfiguration);
            }
        }
    }


    public class WindowPotionsMenuConsumableAddActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _consumableAddActionHandler;

        public WindowPotionsMenuConsumableAddActionHandlerFacade(
            Button addConsumableButton,
            ListBox consumableListBox,
            StackPanel consumableTemplate
        )
        {
            _consumableAddActionHandler = new WindowPotionsMenuConsumableAddActionHandler(
                addConsumableButton,
                new WindowPotionsMenuConsumableAddModifier(
                    consumableListBox,
                    new ConsumableStackPanelFactory(consumableTemplate)
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _consumableAddActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _consumableAddActionHandler.Inject(dataType, data);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _consumableAddActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowPotionsMenuConsumableRemoveModifier : AbstractWindowStateModifier
    {
        private ListBox _consumableListBox;

        public WindowPotionsMenuConsumableRemoveModifier(
            ListBox consumableListBox
        )
        {
            _consumableListBox = consumableListBox;
        }

        public override void Modify(object? value)
        {
            if (value is not MaplestoryBotConfiguration configuration)
            {
                return;
            }
            if (_consumableListBox.Items.Count == 0)
            {
                return;
            }
            var selectedIndex = (
                _consumableListBox.SelectedIndex == -1 ?
                _consumableListBox.Items.Count - 1 :
                _consumableListBox.SelectedIndex
            );
            configuration.Consumables.RemoveAt(selectedIndex);
            _consumableListBox.Items.RemoveAt(selectedIndex);
            _consumableListBox.SelectedIndex = selectedIndex;
        }
    }


    public class WindowPotionsMenuConsumableRemoveActionHandler : AbstractWindowActionHandler
    {
        private Button _removeConsumableButton;

        private AbstractWindowStateModifier _consumableRemoveModifier;

        private MaplestoryBotConfiguration? _maplestoryBotConfiguration;

        public WindowPotionsMenuConsumableRemoveActionHandler(
            Button removeConsumableButton,
            AbstractWindowStateModifier consumableRemoveModifier
        )
        {
            _removeConsumableButton = removeConsumableButton;
            _removeConsumableButton.Click += OnEvent;
            _consumableRemoveModifier = consumableRemoveModifier;
            _maplestoryBotConfiguration = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _consumableRemoveModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _maplestoryBotConfiguration = configuration;
            }
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_maplestoryBotConfiguration != null)
            {
                _consumableRemoveModifier.Modify(_maplestoryBotConfiguration);
            }
        }
    }


    public class WindowPotionsMenuConsumableRemoveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _consumableRemoveActionHandler;

        public WindowPotionsMenuConsumableRemoveActionHandlerFacade(
            ListBox consumableListBox,
            Button removeConsumableButton
        )
        {
            _consumableRemoveActionHandler = (
                new WindowPotionsMenuConsumableRemoveActionHandler(
                    removeConsumableButton,
                    new WindowPotionsMenuConsumableRemoveModifier(consumableListBox)
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _consumableRemoveActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _consumableRemoveActionHandler.Inject(dataType, data);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _consumableRemoveActionHandler.OnEvent(sender, e);
        }
    }

}
