using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class StackPanelTemplate
    {
        public static StackPanel Fixture()
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Focusable = false,
                Children = {
                    new CheckBox { VerticalContentAlignment = VerticalAlignment.Center },
                    new TextBox
                    {
                        Margin = new Thickness(12, 23, 34, 45),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Width = 123,
                        Height = 234,
                        Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(23, 34, 45)),
                        Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 45, 56)),
                        FontFamily = new FontFamily("meow")
                    }
                }
            };
        }
    }

    public class PotionsMenuScreenCaptureSubscriberTests
    {
        private Image<Bgra32> _image = new Image<Bgra32>(5, 5);

        private SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);

        private MockWindowActionHandler _hpActionHandler = new MockWindowActionHandler();

        private MockWindowStateModifier _hpModifier = new MockWindowStateModifier();

        private MockWindowActionHandler _mpActionHandler = new MockWindowActionHandler();

        private MockWindowStateModifier _mpModifier = new MockWindowStateModifier();

        private AbstractScreenCaptureSubscriber _fixture()
        {
            _image = new Image<Bgra32>(2, 2);
            _semaphore = new SemaphoreSlim(0, 1);
            _hpActionHandler = new MockWindowActionHandler();
            _hpModifier = new MockWindowStateModifier();
            _hpActionHandler.ModifierReturn.Add(_hpModifier);
            _mpActionHandler = new MockWindowActionHandler();
            _mpModifier = new MockWindowStateModifier();
            _mpActionHandler.ModifierReturn.Add(_mpModifier);
            return new PotionsMenuScreenCaptureSubscriber(_semaphore);
        }

        /**
         * @brief Verifies that when processing a captured screen image, the potions menu
         * subscriber injects the image into both HP and MP bar handlers for color detection
         * 
         * When the screen capture system captures a new frame showing the health and mana
         * bars, the subscriber must forward the image to both the HP and MP bar handlers.
         * Each handler then analyzes the RGB values at the configured pixel coordinates
         * to determine current HP and MP levels, triggering potion usage when thresholds
         * are crossed. This test ensures both handlers receive the image regardless of
         * whether they are configured for health or mana monitoring.
         */
        private void _testProcessingImageInjectsImageIntoBarHandlers()
        {
            var handler = _fixture();
            handler.Notify(_image, true);
            _hpModifier.StateReturn.Add(PotionResourceType.Health);
            _mpModifier.StateReturn.Add(PotionResourceType.Mana);
            handler.Inject(SystemInjectType.ActionHandler, _hpActionHandler);
            handler.Inject(SystemInjectType.ActionHandler, _mpActionHandler);
            Debug.Assert(_hpActionHandler.InjectCalls == 0);
            Debug.Assert(_mpActionHandler.InjectCalls == 0);
            handler.ProcessImage();
            Debug.Assert(_hpActionHandler.InjectCalls == 1);
            Debug.Assert(_hpActionHandler.InjectCallArg_data[0] == _image);
            Debug.Assert(_mpActionHandler.InjectCalls == 1);
            Debug.Assert(_mpActionHandler.InjectCallArg_data[0] == _image);
        }

        /**
         * @brief Verifies that the subscriber only injects images into bar handlers that
         * are configured for valid resource types (Health or Mana)
         * 
         * When action handlers are registered with the subscriber, they may have modifiers
         * that return different state values. Only handlers whose modifiers return
         * PotionResourceType.Health or PotionResourceType.Mana should receive the injected
         * image. Handlers returning other values (e.g., 123 or 234) should be ignored,
         * preventing unnecessary processing or errors from misconfigured handlers.
         */
        private void _testProcessingImageOnlyInjectsImageIntoBarHandlers()
        {
            var handler = _fixture();
            handler.Notify(_image, true);
            _hpModifier.StateReturn.Add(123);
            _mpModifier.StateReturn.Add(234);
            handler.Inject(SystemInjectType.ActionHandler, _hpActionHandler);
            handler.Inject(SystemInjectType.ActionHandler, _mpActionHandler);
            Debug.Assert(_hpActionHandler.InjectCalls == 0);
            Debug.Assert(_mpActionHandler.InjectCalls == 0);
            handler.ProcessImage();
            Debug.Assert(_hpActionHandler.InjectCalls == 0);
            Debug.Assert(_mpActionHandler.InjectCalls == 0);
        }

        public void Run()
        {
            _testProcessingImageInjectsImageIntoBarHandlers();
            _testProcessingImageOnlyInjectsImageIntoBarHandlers();
        }
    }


    public class WindowPotionsMenuLoadingResourceActionHandlerTests
    {
        private TextBox _pixelThresholdRTextBox = new TextBox();

        private TextBox _pixelThresholdGTextBox = new TextBox();

        private TextBox _pixelThresholdBTextBox = new TextBox();

        private TextBox _pixelToleranceRTextBox = new TextBox();

        private TextBox _pixelToleranceGTextBox = new TextBox();

        private TextBox _pixelToleranceBTextBox = new TextBox();

        private TextBox _pixelRelativeXTextBox = new TextBox();

        private TextBox _pixelRelativeYTextBox = new TextBox();

        private TextBox _healthBarLeftTextBox = new TextBox();

        private TextBox _healthBarTopTextBox = new TextBox();

        private TextBox _healthBarRightTextBox = new TextBox();

        private TextBox _healthBarBottomTextBox = new TextBox();

        private TextBox _keyTextBox = new TextBox();

        private CheckBox _activeCheckBox = new CheckBox();

        private MockSystemWindow _potionsWindow = new MockSystemWindow();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private MockPotionsMenuState _potionsMenuState = new MockPotionsMenuState();

        private MockWindowStateModifier _loadingResourceModifier = new MockWindowStateModifier();

        private List<string> _callOrder = [];

        private AbstractWindowActionHandler _fixture(PotionResourceType resourceType)
        {
            _pixelThresholdRTextBox = new TextBox();
            _pixelThresholdGTextBox = new TextBox();
            _pixelThresholdBTextBox = new TextBox();
            _pixelToleranceRTextBox = new TextBox();
            _pixelToleranceGTextBox = new TextBox();
            _pixelToleranceBTextBox = new TextBox();
            _pixelRelativeXTextBox = new TextBox();
            _pixelRelativeYTextBox = new TextBox();
            _healthBarLeftTextBox = new TextBox();
            _healthBarTopTextBox = new TextBox();
            _healthBarRightTextBox = new TextBox();
            _healthBarBottomTextBox = new TextBox();
            _keyTextBox = new TextBox();
            _activeCheckBox = new CheckBox();
            _potionsMenuState = new MockPotionsMenuState();
            _potionsWindow = new MockSystemWindow();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                Hp = new Resource
                {
                    Active = 123,
                    Tolerance = [234, 345, 456],
                    Rgb = [345, 456, 567],
                    Pixel = [456, 567],
                    Key = "678",
                    Rect = [789, 890, 901, 012]
                },
                Mp = new Resource
                {
                    Active = 0,
                    Tolerance = [123, 234, 345],
                    Rgb = [234, 345, 456],
                    Pixel = [567, 678],
                    Key = "789",
                    Rect = [890, 901, 012, 123]
                }
            };
            _potionsWindow.GetWindowReturn.Add(new Window());
            var handler = new WindowPotionsMenuLoadingResourceActionHandlerFacade(
                resourceType,
                _pixelThresholdRTextBox,
                _pixelThresholdGTextBox,
                _pixelThresholdBTextBox,
                _pixelToleranceRTextBox,
                _pixelToleranceGTextBox,
                _pixelToleranceBTextBox,
                _pixelRelativeXTextBox,
                _pixelRelativeYTextBox,
                _healthBarLeftTextBox,
                _healthBarTopTextBox,
                _healthBarRightTextBox,
                _healthBarBottomTextBox,
                _keyTextBox,
                _activeCheckBox,
                _potionsMenuState,
                _potionsWindow
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration
            );
            return handler;
        }

        private Dictionary<PotionResourceType, dynamic> _expecteds()
        {
            return new Dictionary<PotionResourceType, dynamic>
            {
                [PotionResourceType.Health] = new
                {
                    Thresholds = new[] { "345", "456", "567" },
                    Tolerances = new[] { "234", "345", "456" },
                    Pixel = new[] { "456", "567" },
                    Rect = new[] { "789", "890", "901", "12" },
                    Active = true,
                    Key = "678"
                },
                [PotionResourceType.Mana] = new
                {
                    Thresholds = new[] { "234", "345", "456" },
                    Tolerances = new[] { "123", "234", "345" },
                    Pixel = new[] { "567", "678" },
                    Rect = new[] { "890", "901", "12", "123" },
                    Active = false,
                    Key = "789"
                }
            };
        }

        private AbstractWindowActionHandler _editingFixture(PotionResourceType resourceType)
        {
            _potionsMenuState = new MockPotionsMenuState();
            _potionsWindow = new MockSystemWindow();
            _loadingResourceModifier = new MockWindowStateModifier();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration();
            _callOrder = [];
            _loadingResourceModifier.CallOrder = _callOrder;
            _potionsMenuState.CallOrder = _callOrder;
            _potionsWindow.GetWindowReturn.Add(new Window());
            var handler = new WindowPotionsMenuLoadingResourceActionHandler(
                resourceType,
                _loadingResourceModifier,
                _potionsMenuState,
                _potionsWindow
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Verifies that when the potions configuration window is opened, the UI
         * text boxes and checkboxes are populated with the saved HP or MP monitoring
         * settings from the botting configuration
         * 
         * When users open the potions configuration window to adjust health or mana
         * potion settings, the system must load the existing configuration values into
         * the UI controls. This includes RGB threshold values (for detecting low health/mana),
         * tolerance values (for color matching accuracy), pixel coordinates (where to sample
         * the health/mana bar), the health/mana bar rectangle (region on screen),
         * the associated hotkey for the potion, and whether the potion automation is active.
         */
        private void _testOpeningPotionsWindowLoadsResourceConfiguration()
        {
            foreach (var resourceType in new[] { PotionResourceType.Health, PotionResourceType.Mana })
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture(resourceType);
                var expected = _expecteds()[resourceType];
                _potionsWindow.VisibleReturn.Add(visible);
                _activeCheckBox.IsChecked = !expected.Active;
                handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
                Debug.Assert(_pixelThresholdRTextBox.Text == (visible ? (string)expected.Thresholds[0] : ""));
                Debug.Assert(_pixelThresholdGTextBox.Text == (visible ? (string)expected.Thresholds[1] : ""));
                Debug.Assert(_pixelThresholdBTextBox.Text == (visible ? (string)expected.Thresholds[2] : ""));
                Debug.Assert(_pixelToleranceRTextBox.Text == (visible ? (string)expected.Tolerances[0] : ""));
                Debug.Assert(_pixelToleranceGTextBox.Text == (visible ? (string)expected.Tolerances[1] : ""));
                Debug.Assert(_pixelToleranceBTextBox.Text == (visible ? (string)expected.Tolerances[2] : ""));
                Debug.Assert(_pixelRelativeXTextBox.Text == (visible ? (string)expected.Pixel[0] : ""));
                Debug.Assert(_pixelRelativeYTextBox.Text == (visible ? (string)expected.Pixel[1] : ""));
                Debug.Assert(_healthBarLeftTextBox.Text == (visible ? (string)expected.Rect[0] : ""));
                Debug.Assert(_healthBarTopTextBox.Text == (visible ? (string)expected.Rect[1] : ""));
                Debug.Assert(_healthBarRightTextBox.Text == (visible ? (string)expected.Rect[2] : ""));
                Debug.Assert(_healthBarBottomTextBox.Text == (visible ? (string)expected.Rect[3] : ""));
                Debug.Assert(_keyTextBox.Text == (visible ? (string)expected.Key : ""));
                Debug.Assert(_activeCheckBox.IsChecked == (visible ? (bool)expected.Active : (bool)!expected.Active));
            }
        }

        /**
         * @brief Verifies that when the potions window is opened, the editing state is
         * set to true before loading the resource configuration, then set back to false
         * after loading completes
         * 
         * When users open the potions configuration window, the system must enter an
         * "editing" state before populating the UI controls with saved configuration
         * values. This editing flag prevents other components (such as the screen capture
         * subscriber or configuration saver) from reacting to individual UI updates
         * while the entire configuration is being loaded. Once loading is complete,
         * the editing state is cleared, allowing normal UI event handling to resume.
         */
        private void _testOpeningPotionsWindowSetsMenuStateBeforeModification()
        {
            foreach (var resourceType in new[] { PotionResourceType.Health, PotionResourceType.Mana })
            {
                var handler = _editingFixture(resourceType);
                var stateRef = new TestUtilities().Reference(_potionsMenuState);
                var resourceRef = new TestUtilities().Reference(_loadingResourceModifier);
                _potionsWindow.VisibleReturn.Add(true);
                handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
                Debug.Assert(_callOrder.Count == 3);
                Debug.Assert(_callOrder[0] == stateRef + "SetEditingState");
                Debug.Assert(_callOrder[1] == resourceRef + "Modify");
                Debug.Assert(_callOrder[2] == stateRef + "SetEditingState");
                Debug.Assert(_potionsMenuState.SetEditingStateCallArg_state[0] == 1);
                Debug.Assert(_potionsMenuState.SetEditingStateCallArg_state[1] == 0);
            }
        }

        public void Run()
        {
            _testOpeningPotionsWindowLoadsResourceConfiguration();
            _testOpeningPotionsWindowSetsMenuStateBeforeModification();
        }
    }


    public class WindowPotionsMenuSavingResourceActionHandlerTests
    {
        private TextBox _pixelThresholdRTextBox = new TextBox();

        private TextBox _pixelThresholdGTextBox = new TextBox();

        private TextBox _pixelThresholdBTextBox = new TextBox();

        private TextBox _pixelToleranceRTextBox = new TextBox();

        private TextBox _pixelToleranceGTextBox = new TextBox();

        private TextBox _pixelToleranceBTextBox = new TextBox();

        private TextBox _pixelRelativeXTextBox = new TextBox();

        private TextBox _pixelRelativeYTextBox = new TextBox();

        private TextBox _healthBarLeftTextBox = new TextBox();

        private TextBox _healthBarTopTextBox = new TextBox();

        private TextBox _healthBarRightTextBox = new TextBox();

        private TextBox _healthBarBottomTextBox = new TextBox();

        private TextBox _keyTextBox = new TextBox();

        private CheckBox _activeCheckBox = new CheckBox();

        private MockSystemWindow _potionsWindow = new MockSystemWindow();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private AbstractWindowActionHandler _fixture(PotionResourceType resourceType)
        {
            _pixelThresholdRTextBox = new TextBox();
            _pixelThresholdGTextBox = new TextBox();
            _pixelThresholdBTextBox = new TextBox();
            _pixelToleranceRTextBox = new TextBox();
            _pixelToleranceGTextBox = new TextBox();
            _pixelToleranceBTextBox = new TextBox();
            _pixelRelativeXTextBox = new TextBox();
            _pixelRelativeYTextBox = new TextBox();
            _healthBarLeftTextBox = new TextBox();
            _healthBarTopTextBox = new TextBox();
            _healthBarRightTextBox = new TextBox();
            _healthBarBottomTextBox = new TextBox();
            _keyTextBox = new TextBox();
            _activeCheckBox = new CheckBox();
            _potionsWindow = new MockSystemWindow();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                Hp = new Resource { Active = 123 },
                Mp = new Resource { Active = 123 },
            };
            _potionsWindow.GetWindowReturn.Add(new Window());
            var handler = new WindowPotionsMenuSavingResourceActionHandlerFacade(
                resourceType,
                _pixelThresholdRTextBox,
                _pixelThresholdGTextBox,
                _pixelThresholdBTextBox,
                _pixelToleranceRTextBox,
                _pixelToleranceGTextBox,
                _pixelToleranceBTextBox,
                _pixelRelativeXTextBox,
                _pixelRelativeYTextBox,
                _healthBarLeftTextBox,
                _healthBarTopTextBox,
                _healthBarRightTextBox,
                _healthBarBottomTextBox,
                _keyTextBox,
                _activeCheckBox,
                _potionsWindow
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration
            );
            return handler;
        }

        private Dictionary<PotionResourceType, dynamic> _configuration()
        {
            return new Dictionary<PotionResourceType, dynamic>
            {
                [PotionResourceType.Health] = new
                {
                    Thresholds = new[] { "345", "456", "567" },
                    Tolerances = new[] { "234", "345", "456" },
                    Pixel = new[] { "456", "567" },
                    Rect = new[] { "789", "890", "901", "12" },
                    Active = true,
                    Key = "678"
                },
                [PotionResourceType.Mana] = new
                {
                    Thresholds = new[] { "234", "345", "456" },
                    Tolerances = new[] { "123", "234", "345" },
                    Pixel = new[] { "567", "678" },
                    Rect = new[] { "890", "901", "12", "123" },
                    Active = false,
                    Key = "789"
                }
            };
        }

        private void _setupConfiguration(dynamic configuration)
        {
            _pixelThresholdRTextBox.Text = configuration.Thresholds[0];
            _pixelThresholdGTextBox.Text = configuration.Thresholds[1];
            _pixelThresholdBTextBox.Text = configuration.Thresholds[2];
            _pixelToleranceRTextBox.Text = configuration.Tolerances[0];
            _pixelToleranceGTextBox.Text = configuration.Tolerances[1];
            _pixelToleranceBTextBox.Text = configuration.Tolerances[2];
            _pixelRelativeXTextBox.Text = configuration.Pixel[0];
            _pixelRelativeYTextBox.Text = configuration.Pixel[1];
            _healthBarLeftTextBox.Text = configuration.Rect[0];
            _healthBarTopTextBox.Text = configuration.Rect[1];
            _healthBarRightTextBox.Text = configuration.Rect[2];
            _healthBarBottomTextBox.Text = configuration.Rect[3];
            _keyTextBox.Text = configuration.Key;
            _activeCheckBox.IsChecked = configuration.Active;
        }

        /**
         * @brief Verifies that when the potions configuration window is closed or hidden,
         * the UI settings (RGB thresholds, tolerances, pixel coordinates, rectangle bounds,
         * hotkey, and active state) are saved to the corresponding HP or MP resource in
         * the botting configuration
         * 
         * When users modify health or mana potion settings in the configuration window
         * and then close it, the system must persist all changes to the botting model.
         * This includes the RGB threshold values,
         * tolerance values (how closely colors must match), pixel coordinates (where to
         * sample the health/mana bar), the detection rectangle (region containing the
         * health/mana bar), the associated hotkey for the potion, and whether potion
         * automation is active.
         */
        private void _testOpeningPotionsWindowSavesResourceConfiguration()
        {
            foreach (var resourceType in new[] { PotionResourceType.Health, PotionResourceType.Mana })
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture(resourceType);
                _potionsWindow.VisibleReturn.Add(visible);
                var configuration = _configuration()[resourceType];
                _setupConfiguration(configuration);
                handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
                var resource = (resourceType == PotionResourceType.Health) ?
                    _maplestoryBotConfiguration.Hp : _maplestoryBotConfiguration.Mp;
                Debug.Assert(resource.Rgb.Length == 3);
                Debug.Assert(resource.Rgb[0] == (!visible ? int.Parse((string)configuration.Thresholds[0]) : 0));
                Debug.Assert(resource.Rgb[1] == (!visible ? int.Parse((string)configuration.Thresholds[1]) : 0));
                Debug.Assert(resource.Rgb[2] == (!visible ? int.Parse((string)configuration.Thresholds[2]) : 0));
                Debug.Assert(resource.Tolerance.Length == 3);
                Debug.Assert(resource.Tolerance[0] == (!visible ? int.Parse((string)configuration.Tolerances[0]) : 0));
                Debug.Assert(resource.Tolerance[1] == (!visible ? int.Parse((string)configuration.Tolerances[1]) : 0));
                Debug.Assert(resource.Tolerance[2] == (!visible ? int.Parse((string)configuration.Tolerances[2]) : 0));
                Debug.Assert(resource.Pixel.Length == 2);
                Debug.Assert(resource.Pixel[0] == (!visible ? int.Parse((string)configuration.Pixel[0]) : 0));
                Debug.Assert(resource.Pixel[1] == (!visible ? int.Parse((string)configuration.Pixel[1]) : 0));
                Debug.Assert(resource.Rect.Length == 4);
                Debug.Assert(resource.Rect[0] == (!visible ? int.Parse((string)configuration.Rect[0]) : 0));
                Debug.Assert(resource.Rect[1] == (!visible ? int.Parse((string)configuration.Rect[1]) : 0));
                Debug.Assert(resource.Rect[2] == (!visible ? int.Parse((string)configuration.Rect[2]) : 0));
                Debug.Assert(resource.Rect[3] == (!visible ? int.Parse((string)configuration.Rect[3]) : 0));
                Debug.Assert(resource.Key == (!visible ? (string)configuration.Key : ""));
                Debug.Assert(resource.Active == (!visible ? ((bool)configuration.Active ? 1 : 0) : 123));
            }
        }

        public void Run()
        {
            _testOpeningPotionsWindowSavesResourceConfiguration();
        }
    }


    public class WindowPotionsMenuTextBoxFrameRGBActionHandlerTests
    {
        private TextBox _textBoxR = new TextBox();

        private TextBox _textBoxG = new TextBox();

        private TextBox _textBoxB = new TextBox();

        private Frame _frameR = new Frame();

        private Frame _frameG = new Frame();

        private Frame _frameB = new Frame();

        private AbstractWindowActionHandler _fixture()
        {
            return new WindowPotionsMenuTextBoxFrameRGBActionHandlerFacade(
                _textBoxR,
                _textBoxG,
                _textBoxB,
                _frameR,
                _frameG,
                _frameB
            );
        }

        /**
         * @brief Verifies that when users enter RGB values into the text boxes, the
         * corresponding color preview frames update to display the correct colors
         * 
         * When users configure health or mana potion detection settings, they need to
         * specify RGB threshold values (the color that indicates low health/mana). As
         * the user types numeric values into the Red, Green, and Blue text boxes, the
         * system must provide immediate visual feedback by updating the background
         * color of the preview frames.
         */
        private void _testEditingTextBoxUpdatesFrame()
        {
            var handler = _fixture();
            _textBoxR.Text = 123.ToString();
            Debug.Assert(((SolidColorBrush)_frameR.Background).Color.R == 123);
            Debug.Assert(((SolidColorBrush)_frameR.Background).Color.G == 0);
            Debug.Assert(((SolidColorBrush)_frameR.Background).Color.B == 0);
            _textBoxG.Text = 234.ToString();
            Debug.Assert(((SolidColorBrush)_frameG.Background).Color.R == 0);
            Debug.Assert(((SolidColorBrush)_frameG.Background).Color.G == 234);
            Debug.Assert(((SolidColorBrush)_frameG.Background).Color.B == 0);
            _textBoxB.Text = 245.ToString();
            Debug.Assert(((SolidColorBrush)_frameB.Background).Color.R == 0);
            Debug.Assert(((SolidColorBrush)_frameB.Background).Color.G == 0);
            Debug.Assert(((SolidColorBrush)_frameB.Background).Color.B == 245);
        }

        public void Run()
        {
            _testEditingTextBoxUpdatesFrame();
        }
    }


    public class WindowPotionsMenuResourceBarActionHandlerTests
    {
        private TextBox _leftTextBox = new TextBox();

        private TextBox _topTextBox = new TextBox();

        private TextBox _rightTextBox = new TextBox();

        private TextBox _bottomTextBox = new TextBox();

        private System.Windows.Controls.Image _display = new System.Windows.Controls.Image();

        private MockDispatcher _dispatcher = new MockDispatcher();

        private MockSystemWindow _potionsWindow = new MockSystemWindow();

        private MockInjectAction _injectAction = new MockInjectAction();

        private Image<Bgra32> _image = new Image<Bgra32>(100, 100);

        private AbstractWindowActionHandler _fixture(
            PotionResourceType resourceType
        )
        {
            _leftTextBox = new TextBox();
            _topTextBox = new TextBox();
            _rightTextBox = new TextBox();
            _bottomTextBox = new TextBox();
            _display = new System.Windows.Controls.Image();
            _dispatcher = new MockDispatcher();
            _potionsWindow = new MockSystemWindow();
            _injectAction = new MockInjectAction();
            _image = new Image<Bgra32>(100, 100);
            return new WindowPotionsMenuResourceBarActionHandler(
                _potionsWindow,
                new WindowPotionsMenuResourceBarModifier(
                    resourceType,
                    _leftTextBox,
                    _topTextBox,
                    _rightTextBox,
                    _bottomTextBox,
                    _display,
                    _dispatcher
                )
            );
        }

        private void _setupImage()
        {
            byte pixelByte = 0;
            for (int y = 12; y < 17; y++)
            for (int x = 34; x < 39; x++)
            {
                _image[x, y] = new Bgra32(
                    pixelByte++,
                    pixelByte++,
                    pixelByte++,
                    pixelByte++
                );
            }
        }

        /**
         * @brief Verifies that when an inject action is registered, the handler properly
         * injects itself as an ActionHandler dependency for external components
         * 
         * When the potions menu system initializes, other components may need to inject
         * images into the resource bar handler for processing. The handler must register
         * itself as an ActionHandler so that the screen capture subscriber can forward
         * captured images to it. This test ensures that when an InjectAction is provided,
         * the handler correctly invokes it with its own instance as the data payload.
         */
        private void _testInjectActionInjectsActionHandler()
        {
            var dataTypeList = new List<object>();
            var dataList = new List<object?>();
            var handler = _fixture(PotionResourceType.Health);
            _injectAction.GetActionReturn.Add(
                (_, __) => { dataTypeList.Add(_); dataList.Add(__); }
            );
            handler.Inject(SystemInjectType.InjectAction, _injectAction);
            Debug.Assert(dataTypeList.Count == 1);
            Debug.Assert(dataTypeList[0] is SystemInjectType.ActionHandler);
            Debug.Assert(dataList[0] == handler);
        }

        /**
         * @brief Verifies that when the potions window is visible and an image is injected,
         * the resource bar displays a correctly cropped preview of the health/mana bar
         * region with pixel-perfect accuracy
         * 
         * When users configure the health or mana bar detection rectangle and the potions
         * window is visible, the system must crop the captured screen image to the specified
         * rectangle (left, top, right, bottom) and display it in the UI. This allows users
         * to visually verify that the detection area is correctly positioned over the
         * health/mana bar. When the window is hidden, no display update should occur.
         */
        private void _testInjectImageDisplaysWhenVisible()
        {
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture(PotionResourceType.Health);
                _potionsWindow.VisibleReturn.Add(visible);
                _leftTextBox.Text = "34";
                _topTextBox.Text = "12";
                _rightTextBox.Text = "39";
                _bottomTextBox.Text = "17";
                _setupImage();
                handler.Inject(0, _image);
                Debug.Assert(_dispatcher.DispatchCalls == (visible ? 1 : 0));
                if (visible)
                {
                    _dispatcher.DispatchCallArg_action[0]();
                    Debug.Assert(_display.Source is BitmapSource);
                    var bitmapSource = (BitmapSource)_display.Source;
                    Debug.Assert(bitmapSource.PixelWidth == 5);
                    Debug.Assert(bitmapSource.PixelHeight == 5);
                    int stride = bitmapSource.PixelWidth * 4;
                    byte[] pixelData = new byte[stride * bitmapSource.PixelHeight];
                    bitmapSource.CopyPixels(pixelData, stride, 0);
                    byte expectedPixel = 0;
                    for (int y = 0; y < 5; y++)
                    for (int x = 0; x < 5; x++)
                    {
                        int index = (y * stride) + (x * 4);
                        byte displayedB = pixelData[index];
                        byte displayedG = pixelData[index + 1];
                        byte displayedR = pixelData[index + 2];
                        byte displayedA = pixelData[index + 3];
                        var expectedOriginal = _image[34 + x, 12 + y];
                        Debug.Assert(displayedR == expectedPixel++);
                        Debug.Assert(displayedG == expectedPixel++);
                        Debug.Assert(displayedB == expectedPixel++);
                        Debug.Assert(displayedA == expectedPixel++);
                    }
                }
                else
                {
                    Debug.Assert(_display.Source == null);
                }
            }
        }

        /**
         * @brief Verifies that when the crop rectangle has zero width or zero height
         * (due to right not greater than left, or bottom not greater than top),
         * the resource bar does not display an image
         * 
         * When users leave any of the left, top, right, or bottom text boxes empty,
         * the values default to 0. This results in either right == left (zero width)
         * or bottom == top (zero height). A rectangle with zero width or zero height
         * cannot be cropped or displayed. The system must skip the crop operation
         * and leave the display source unchanged.
         */
        private void _testInjectImageHandlesInvalidRectangle()
        {
            foreach (var visible in new[] { true, false })
            {
                var invalidScenarios = new[]
                {
                    (left: "", top: "", right: "", bottom: ""),
                    (left: "", top: "12", right: "39", bottom: ""),
                    (left: "34", top: "", right: "", bottom: "17"),
                    (left: "34", top: "12", right: "", bottom: "17"),
                    (left: "34", top: "12", right: "39", bottom: ""),
                    (left: "50", top: "12", right: "30", bottom: "17"),
                    (left: "34", top: "30", right: "39", bottom: "12")
                };
                foreach (var scenario in invalidScenarios)
                {
                    var handler = _fixture(PotionResourceType.Health);
                    _potionsWindow.VisibleReturn.Add(visible);
                    _leftTextBox.Text = scenario.left;
                    _topTextBox.Text = scenario.top;
                    _rightTextBox.Text = scenario.right;
                    _bottomTextBox.Text = scenario.bottom;
                    _display.Source = null;
                    handler.Inject(0, _image);
                    if (visible)
                    {
                        Debug.Assert(_dispatcher.DispatchCalls == 1);
                        _dispatcher.DispatchCallArg_action[0]();
                        Debug.Assert(_display.Source == null);
                    }
                    else
                    {
                        Debug.Assert(_dispatcher.DispatchCalls == 0);
                        Debug.Assert(_display.Source == null);
                    }
                }
            }
        }

        /**
         * @brief Verifies that the modifier correctly identifies which resource type
         * (Health or Mana) it is associated with for proper UI updates
         * 
         * The potions window has separate configuration areas for health and mana potion
         * settings. The resource bar modifier must know which resource type it belongs to
         * so it can update the correct UI elements and save to the correct configuration
         * section. This test ensures that when creating the handler for Health or Mana,
         * the modifier's state returns the correct resource type.
         */
        private void _testModifierStateType()
        {
            foreach (
                var resourceType in new[]
                {
                    PotionResourceType.Health,
                    PotionResourceType.Mana
                }
            )
            {
                var handler = _fixture(resourceType);
                Debug.Assert(
                    (PotionResourceType)handler.Modifier().State(0)! ==
                    resourceType
                );
            }
        }

        public void Run()
        {
            _testInjectActionInjectsActionHandler();
            _testInjectImageDisplaysWhenVisible();
            _testModifierStateType();
            _testInjectImageHandlesInvalidRectangle();
        }
    }


    public class WindowPotionsMenuRGBLabelActionHandlerTests
    {
        private TextBox _textBoxX = new TextBox();

        private TextBox _textBoxY = new TextBox();

        private TextBox _textBoxLeft = new TextBox();

        private TextBox _textBoxTop = new TextBox();

        private Label _labelR = new Label();

        private Label _labelG = new Label();

        private Label _labelB = new Label();

        private MockDispatcher _dispatcher = new MockDispatcher();

        private MockSystemWindow _potionsWindow = new MockSystemWindow();

        private MockInjectAction _injectAction = new MockInjectAction();

        private Image<Bgra32> _image = new Image<Bgra32>(100, 100);

        private AbstractWindowActionHandler _fixture(PotionResourceType resourceType)
        {
            _textBoxX = new TextBox();
            _textBoxY = new TextBox();
            _textBoxLeft = new TextBox();
            _textBoxTop = new TextBox();
            _labelR = new Label();
            _labelG = new Label();
            _labelB = new Label();
            _dispatcher = new MockDispatcher();
            _potionsWindow = new MockSystemWindow();
            return new WindowPotionsMenuRGBLabelActionHandler(
                _potionsWindow,
                new WindowPotionsMenuRGBLabelModifier(
                    resourceType,
                    _textBoxX,
                    _textBoxY,
                    _textBoxLeft,
                    _textBoxTop,
                    _labelR,
                    _labelG,
                    _labelB,
                    _dispatcher
                )
            );
        }

        /**
         * @brief Verifies that when an inject action is registered, the handler properly
         * injects itself as an ActionHandler dependency for external components
         * 
         * When the potions menu system initializes, other components may need to inject
         * images into the RGB label handler for processing. The handler must register
         * itself as an ActionHandler so that the screen capture subscriber can forward
         * captured images to it.
         */
        private void _testInjectActionInjectsActionHandler()
        {
            var dataTypeList = new List<object>();
            var dataList = new List<object?>();
            var handler = _fixture(PotionResourceType.Health);
            _injectAction.GetActionReturn.Add(
                (_, __) => { dataTypeList.Add(_); dataList.Add(__); }
            );
            handler.Inject(SystemInjectType.InjectAction, _injectAction);
            Debug.Assert(dataTypeList.Count == 1);
            Debug.Assert(dataTypeList[0] is SystemInjectType.ActionHandler);
            Debug.Assert(dataList[0] == handler);
        }

        /**
         * @brief Verifies that the modifier returns the correct resource type
         * (Health or Mana) to identify which potion configuration this handler is
         * responsible for
         * 
         * External modules need to know whether this handler is managing health
         * potion settings or mana potion settings. The modifier's State property
         * provides this identification, allowing the system to route captured
         * images and configuration updates to the appropriate handler.
         */
        private void _testModifierStateType()
        {
            foreach (
                var resourceType in new[]
                {
                    PotionResourceType.Health,
                    PotionResourceType.Mana
                }
            )
            {
                var handler = _fixture(resourceType);
                Debug.Assert(
                    (PotionResourceType)handler.Modifier().State(0)! ==
                    resourceType
                );
            }
        }

        /**
         * @brief Verifies that when the calculated pixel position exceeds image bounds,
         * the RGB labels are cleared (set to null) to prevent displaying invalid data
         * 
         * When the absolute pixel position (left + X, top + Y) falls outside the image
         * dimensions (>= width or >= height), attempting to read the pixel would cause
         * an out-of-bounds exception. The system must detect this condition and clear
         * the labels instead of attempting to read invalid pixel data.
         * Expected result: When the calculated position is outside image bounds, the
         * labels remain null (no RGB values displayed).
         */
        private void _testInjectImageHandlesInvalidDimensions()
        {
            foreach (var visible in new[] { true, false })
            {
                var invalidScenarios = new[]
                {
                    (left: "12", top: "60", x: "45", y: "50"),
                    (left: "70", top: "12", x: "40", y: "30"),
                    (left: "70", top: "60", x: "40", y: "50"),
                    (left: "12", top: "99", x: "45", y: "1"),
                    (left: "99", top: "12", x: "1", y: "30"),
                };
                foreach (var scenario in invalidScenarios)
                {
                    var handler = _fixture(PotionResourceType.Health);
                    _potionsWindow.VisibleReturn.Add(visible);
                    _textBoxLeft.Text = scenario.left;
                    _textBoxTop.Text = scenario.top;
                    _textBoxX.Text = scenario.x;
                    _textBoxY.Text = scenario.y;
                    handler.Inject(0, _image);
                    if (visible)
                    {
                        Debug.Assert(_dispatcher.DispatchCalls == 1);
                        _dispatcher.DispatchCallArg_action[0]();
                        Debug.Assert(_labelR.Content == null);
                        Debug.Assert(_labelG.Content == null);
                        Debug.Assert(_labelB.Content == null);
                    }
                    else
                    {
                        Debug.Assert(_dispatcher.DispatchCalls == 0);
                    }
                }
            }
        }

        /**
         * @brief Verifies that when the potions window is visible and an image is injected,
         * the RGB labels display the correct color values from the pixel at the calculated
         * position (left + X, top + Y), even when some coordinate text boxes are empty
         * 
         * When users configure the detection pixel coordinates, the system must read the
         * RGB values from the captured image at the absolute position (left + X, top + Y)
         * and display them in the corresponding labels. This test covers five scenarios:
         * 
         * - All coordinates provided normally → position (57, 90)
         * - X coordinate empty (defaults to 0) → position (57, 90)
         * - Y coordinate empty (defaults to 0) → position (57, 90)
         * - Left coordinate empty (defaults to 0) → position (57, 90)
         * - Top coordinate empty (defaults to 0) → position (57, 90)
         */
        private void _testInjectImageEditsLabelsWhenVisible()
        {
            var testCases = new[]
            {
                (left: "12", top: "34", x: "45", y: "56"),
                (left: "57", top: "34", x: "", y: "56"),
                (left: "12", top: "90", x: "45", y: ""),
                (left: "", top: "34", x: "57", y: "56"),
                (left: "12", top: "", x: "45", y: "90"),
            };

            foreach (var visible in new[] { true, false })
            foreach (var testCase in testCases)
            {
                var handler = _fixture(PotionResourceType.Health);
                _image[57, 90] = new Bgra32(67, 78, 89, 90);
                _potionsWindow.VisibleReturn.Add(visible);
                _textBoxLeft.Text = testCase.left;
                _textBoxTop.Text = testCase.top;
                _textBoxX.Text = testCase.x;
                _textBoxY.Text = testCase.y;
                handler.Inject(0, _image);
                if (visible)
                {
                    Debug.Assert(_dispatcher.DispatchCalls == 1);
                    Debug.Assert(_labelR.Content == null);
                    Debug.Assert(_labelG.Content == null);
                    Debug.Assert(_labelB.Content == null);
                    _dispatcher.DispatchCallArg_action[0]();
                    Debug.Assert((string)_labelR.Content! == "67");
                    Debug.Assert((string)_labelG.Content! == "78");
                    Debug.Assert((string)_labelB.Content! == "89");
                }
                else
                {
                    Debug.Assert(_dispatcher.DispatchCalls == 0);
                }
            }
        }

        public void Run()
        {
            _testInjectActionInjectsActionHandler();
            _testInjectImageEditsLabelsWhenVisible();
            _testModifierStateType();
            _testInjectImageHandlesInvalidDimensions();
        }
    }


    public class WindowPotionsMenuRGBFrameActionHandlerTests
    {
        private Label _labelR = new Label();

        private Label _labelG = new Label();

        private Label _labelB = new Label();

        private Frame _frameR = new Frame();

        private Frame _frameG = new Frame();

        private Frame _frameB = new Frame();

        private Frame _framePixel = new Frame();

        private AbstractWindowActionHandler _fixture()
        {
            _labelR = new Label() { Content = "0" };
            _labelG = new Label() { Content = "0" };
            _labelB = new Label() { Content = "0" };
            _frameR = new Frame();
            _frameG = new Frame();
            _frameB = new Frame();
            _framePixel = new Frame();
            return new WindowPotionsMenuRGBFrameActionHandlerFacade(
                _labelR,
                _labelG,
                _labelB,
                _frameR,
                _frameG,
                _frameB,
                _framePixel
            );
        }

        /**
         * @brief Verifies that when the RGB label contents are updated, the
         * corresponding color frames update to display the RGB values, and the
         * combined pixel frame shows the mixed color.
         * 
         * When users enter RGB threshold values in the potions configuration window,
         * each label (Red, Green, Blue) controls its respective color frame. The red
         * frame displays the red component (with zero green and blue), the green frame
         * displays the green component, and the blue frame displays the blue component.
         * Additionally, a separate pixel frame displays the combined color, showing
         * users what the actual detection color will look like when all three components
         * are combined.
         */
        private void _testEditingLabelContentUpdatesFrameColor()
        {
            var handler = _fixture();
            _labelR.Content = "123";
            Debug.Assert(((SolidColorBrush)_frameR.Background).Color.R == 123);
            Debug.Assert(((SolidColorBrush)_frameR.Background).Color.G == 0);
            Debug.Assert(((SolidColorBrush)_frameR.Background).Color.B == 0);
            Debug.Assert(((SolidColorBrush)_framePixel.Background).Color.R == 123);
            Debug.Assert(((SolidColorBrush)_framePixel.Background).Color.G == 0);
            Debug.Assert(((SolidColorBrush)_framePixel.Background).Color.B == 0);
            _labelG.Content = "234";
            Debug.Assert(((SolidColorBrush)_frameG.Background).Color.R == 0);
            Debug.Assert(((SolidColorBrush)_frameG.Background).Color.G == 234);
            Debug.Assert(((SolidColorBrush)_frameG.Background).Color.B == 0);
            Debug.Assert(((SolidColorBrush)_framePixel.Background).Color.R == 123);
            Debug.Assert(((SolidColorBrush)_framePixel.Background).Color.G == 234);
            Debug.Assert(((SolidColorBrush)_framePixel.Background).Color.B == 0);
            _labelB.Content = "67";
            Debug.Assert(((SolidColorBrush)_frameB.Background).Color.R == 0);
            Debug.Assert(((SolidColorBrush)_frameB.Background).Color.G == 0);
            Debug.Assert(((SolidColorBrush)_frameB.Background).Color.B == 67);
            Debug.Assert(((SolidColorBrush)_framePixel.Background).Color.R == 123);
            Debug.Assert(((SolidColorBrush)_framePixel.Background).Color.G == 234);
            Debug.Assert(((SolidColorBrush)_framePixel.Background).Color.B == 67);
        }

        public void Run()
        {
            _testEditingLabelContentUpdatesFrameColor();
        }
    }


    public class WindowPotionsMenuSaveConfigurationActionHandlerTests
    {
        private ListBox _consumablesListBox = new ListBox();

        private MockSystemWindow _potionsWindow = new MockSystemWindow();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private AbstractWindowActionHandler _fixture()
        {
            _consumablesListBox = new ListBox();
            _potionsWindow = new MockSystemWindow();
            _potionsWindow.GetWindowReturn.Add(new Window());
            var handler = new WindowPotionsMenuSaveConfigurationActionHandlerFacade(
                _consumablesListBox,
                _potionsWindow
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Verifies that when the potions configuration window is closed or hidden,
         * the current configuration is saved to the botting model and persisted to disk,
         * and the selected consumable item is deselected to ensure changes are saved
         * 
         * When users edit potion settings (HP thresholds, MP thresholds, tolerance values,
         * hotkeys, etc.) for a specific consumable item and then close the window, the
         * system must save all changes to the botting model before deselecting the item.
         * Deselecting the item (setting SelectedIndex = -1) triggers the save operation
         * for the previously selected item, ensuring no pending changes are lost. The
         * configuration is then broadcast to other components and saved to disk.
         */
        private void _testPotionsSavesToSystem()
        {
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture();
                var injectTypes = new List<object>();
                var configurations = new List<object>();
                var injectAction = new InjectAction(
                    (_, __) => { injectTypes.Add(_); configurations.Add(__); }
                );
                handler.Inject(SystemInjectType.InjectAction, injectAction);
                _consumablesListBox.Items.Add(new ListBoxItem());
                _consumablesListBox.SelectedIndex = 0;
                _potionsWindow.VisibleReturn.Add(visible);
                handler.OnDependencyEvent(
                    _potionsWindow,
                    new DependencyPropertyChangedEventArgs()
                );
                if (!visible)
                {
                    Debug.Assert(_consumablesListBox.SelectedIndex == -1);
                    Debug.Assert(injectTypes.Count == 2);
                    Debug.Assert(injectTypes[0] is SystemInjectType.ConfigurationUpdate);
                    Debug.Assert(injectTypes[1] is SystemInjectType.ConfigurationSave);
                    Debug.Assert(configurations[0] == _maplestoryBotConfiguration);
                    Debug.Assert(configurations[1] == _maplestoryBotConfiguration);
                }
                else
                {
                    Debug.Assert(_consumablesListBox.SelectedIndex == 0);
                    Debug.Assert(injectTypes.Count == 0);
                }
            }
        }

        public void Run()
        {
            _testPotionsSavesToSystem();
        }
    }


    public class WindowPotionsMenuLoadingConsumablesActionHandlerTests
    {
        private ListBox _consumablesListBox = new ListBox();

        private StackPanel _consumableTemplate = new StackPanel();

        private MockSystemWindow _potionsWindow = new MockSystemWindow();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = new MaplestoryBotConfiguration();

        private MaplestoryBotConfiguration _configuration()
        {
            return new MaplestoryBotConfiguration
            {
                Consumables = [
                    new Consumable
                    {
                        Active = 123,
                        Key = "234",
                        MinDelay = 345,
                        MaxDelay = 456,
                        Name = "567"
                    },
                    new Consumable
                    {
                        Active = 0,
                        Key = "345",
                        MinDelay = 456,
                        MaxDelay = 567,
                        Name = "678"
                    }
                ]
            };
        }

        private AbstractWindowActionHandler _fixture()
        {
            _consumablesListBox = new ListBox();
            _consumableTemplate = StackPanelTemplate.Fixture();
            _potionsWindow = new MockSystemWindow();
            _maplestoryBotConfiguration = _configuration();
            _potionsWindow.GetWindowReturn.Add(new Window());
            var handler = new WindowPotionsMenuLoadingConsumablesActionHandlerFacade(
                _consumablesListBox,
                _consumableTemplate,
                _potionsWindow
            );
            handler.Inject(SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration);
            return handler;
        }

        /**
         * @brief Verifies that the consumables list box is populated with the correct number
         * of items when the potions window becomes visible, and remains unpopulated when hidden
         * 
         * When users open the potions configuration window, all configured consumable items
         * (petfood, buff potions, etc.) should appear in the list box. The population operation
         * only occurs when the window becomes visible.
         */
        private void _testListBoxPopulation()
        {
            foreach (var visible in new[] { true, false })
            {
                var fixture = _fixture();
                var expected = visible ? 2 : 0;
                _potionsWindow.VisibleReturn.Add(visible);
                fixture.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
                Debug.Assert(_consumablesListBox.Items.Count == expected);
            }
        }

        /**
         * @brief Verifies that each consumable list box item stores its configuration data
         * in the Tag property for later access during save operations
         * 
         * When consumables are loaded into the list box, each list box item must store a
         * data tag containing the complete consumable configuration (name, hotkey,
         * min/max delay, active state). This stored data allows the editor to retrieve
         * and modify consumable settings when the user selects different items.
         */
        private void _testListBoxDataTags()
        {
            var fixture = _fixture();
            var expected = _configuration();
            _potionsWindow.VisibleReturn.Add(true);
            fixture.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            for (int i = 0; i < _consumablesListBox.Items.Count; i++) 
            {
                Debug.Assert(_consumablesListBox.Items[i] is ListBoxItem);
                var listBoxItem = (ListBoxItem)_consumablesListBox.Items[i];
                Debug.Assert(listBoxItem.Tag is ConsumableDataTag);
                var tag = (ConsumableDataTag)listBoxItem.Tag;
                Debug.Assert(tag.Consumable.Active == expected.Consumables[i].Active);
                Debug.Assert(tag.Consumable.Key == expected.Consumables[i].Key);
                Debug.Assert(tag.Consumable.MinDelay == expected.Consumables[i].MinDelay);
                Debug.Assert(tag.Consumable.MaxDelay == expected.Consumables[i].MaxDelay);
                Debug.Assert(tag.Consumable.Name == expected.Consumables[i].Name);
            }
        }

        /**
         * @brief Verifies that each consumable list box item contains the expected UI
         * controls (a checkbox for active state and a text box for the consumable name)
         * 
         * When consumables are loaded, each list box item must display a checkbox allowing
         * users to enable/disable the consumable, and a text box for editing the consumable
         * name. This provides the interface for users to manage their consumable items.
         */
        private void _testListBoxItemContent()
        {
            var fixture = _fixture();
            _potionsWindow.VisibleReturn.Add(true);
            fixture.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            foreach (ListBoxItem listBoxItem in _consumablesListBox.Items)
            {
                Debug.Assert(listBoxItem.Content is StackPanel);
                var stackPanel = (StackPanel)listBoxItem.Content;
                Debug.Assert(stackPanel.Children.Count == 2);
                Debug.Assert(stackPanel.Children.OfType<CheckBox>().ToList().Count == 1);
                Debug.Assert(stackPanel.Children.OfType<TextBox>().ToList().Count == 1);
            }
        }

        /**
         * @brief Verifies that each consumable list box item inherits the correct visual
         * styling from the template (margins, colors, fonts, alignment)
         * 
         * When consumables are loaded, each list box item must be styled consistently
         * using the provided template. This includes orientation, focusability, checkbox
         * alignment, text box dimensions, background colors, foreground colors, and font
         * family.
         */
        private void _testListBoxItemStyling()
        {
            var fixture = _fixture();
            var templateStackPanel = StackPanelTemplate.Fixture();
            var templateCheckBox = templateStackPanel.Children.OfType<CheckBox>().First()!;
            var templateTextBox = templateStackPanel.Children.OfType<TextBox>().First()!;
            var templateBackground = (SolidColorBrush)templateTextBox.Background;
            var templateForeground = (SolidColorBrush)templateTextBox.Foreground;
            _potionsWindow.VisibleReturn.Add(true);
            fixture.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            foreach (ListBoxItem listBoxItem in _consumablesListBox.Items)
            {
                var stackPanel = (StackPanel)listBoxItem.Content;
                var checkBox = stackPanel.Children.OfType<CheckBox>().First()!;
                var textBox = stackPanel.Children.OfType<TextBox>().First()!;
                var background = (SolidColorBrush)textBox.Background;
                var foreground = (SolidColorBrush)textBox.Foreground;
                Debug.Assert(stackPanel.Orientation == templateStackPanel.Orientation);
                Debug.Assert(stackPanel.Focusable == templateStackPanel.Focusable);
                Debug.Assert(checkBox.VerticalContentAlignment == templateCheckBox.VerticalContentAlignment);
                Debug.Assert(textBox.Margin == templateTextBox.Margin);
                Debug.Assert(textBox.VerticalContentAlignment == templateTextBox.VerticalContentAlignment);
                Debug.Assert(textBox.HorizontalContentAlignment == templateTextBox.HorizontalContentAlignment);
                Debug.Assert(textBox.Width == templateTextBox.Width);
                Debug.Assert(textBox.Height == templateTextBox.Height);
                Debug.Assert(background.Color.R == templateBackground.Color.R);
                Debug.Assert(background.Color.G == templateBackground.Color.G);
                Debug.Assert(background.Color.B == templateBackground.Color.B);
                Debug.Assert(background.Color.A == templateBackground.Color.A);
                Debug.Assert(foreground.Color.R == templateForeground.Color.R);
                Debug.Assert(foreground.Color.G == templateForeground.Color.G);
                Debug.Assert(foreground.Color.B == templateForeground.Color.B);
                Debug.Assert(foreground.Color.A == templateForeground.Color.A);
                Debug.Assert(textBox.FontFamily.ToString() == templateTextBox.FontFamily.ToString());
            }
        }

        /**
         * @brief Verifies that each consumable list box item displays the correct initial
         * name and active checkbox state from the configuration
         * 
         * When consumables are loaded, each item's text box should display the consumable
         * name, and the checkbox should reflect whether the consumable is active (non-zero
         * Active value). This allows users to see the current configuration at a glance.
         */
        private void _testInitialNameAndCheckedState()
        {
            var fixture = _fixture();
            var expected = _configuration();
            _potionsWindow.VisibleReturn.Add(true);
            fixture.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            for (int i = 0; i < _consumablesListBox.Items.Count; i++)
            {
                var listBoxItem = (ListBoxItem)_consumablesListBox.Items[i];
                var stackPanel = (StackPanel)listBoxItem.Content;
                var textBox = stackPanel.Children.OfType<TextBox>().First()!;
                var checkBox = stackPanel.Children.OfType<CheckBox>().First()!;
                Debug.Assert(textBox.Text == expected.Consumables[i].Name);
                Debug.Assert(checkBox.IsChecked == (expected.Consumables[i].Active != 0));
            }
        }

        /**
         * @brief Verifies that the first consumable item is automatically selected when
         * the potions window becomes visible, and no selection occurs when hidden
         * 
         * When users open the potions configuration window, the first consumable in the
         * list should be selected by default. This allows users to immediately view and
         * edit the first consumable's settings without having to click on it manually.
         * When the window is hidden, the population logic does not run at all, so no
         * selection occurs.
         */
        private void _testSelectedIndex()
        {
            foreach (var visible in new[] { true, false })
            {
                var fixture = _fixture();
                var expected = _configuration();
                _potionsWindow.VisibleReturn.Add(visible);
                fixture.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
                Debug.Assert(_consumablesListBox.SelectedIndex == (visible ? 0 : -1));
            }
        }

        public void Run()
        {
            _testListBoxPopulation();
            _testListBoxDataTags();
            _testListBoxItemContent();
            _testListBoxItemStyling();
            _testInitialNameAndCheckedState();
            _testSelectedIndex();
        }
    }


    public class WindowPotionsMenuConsumableSelectedActionHandlerTests
    {
        private TextBox _minDelayTextBox = new TextBox();

        private TextBox _maxDelayTextBox = new TextBox();

        private TextBox _keyTextBox = new TextBox();

        private ListBox _consumablesListBox = new ListBox();

        private AbstractWindowActionHandler _fixture()
        {
            _minDelayTextBox = new TextBox();
            _maxDelayTextBox = new TextBox();
            _keyTextBox = new TextBox();
            _consumablesListBox = new ListBox();
            return new WindowPotionsMenuConsumableSelectedActionHandlerFacade(
                _minDelayTextBox,
                _maxDelayTextBox,
                _keyTextBox,
                _consumablesListBox
            );
        }

        private void _setupListBoxItem()
        {
            _consumablesListBox.Items.Add(
                new ListBoxItem
                {
                    Tag = new ConsumableDataTag
                    {
                        Consumable = new Consumable
                        {
                            Active = 123,
                            Key = "234",
                            MaxDelay = 345,
                            MinDelay = 456,
                            Name = "567"
                        }
                    }
                }
            );
        }

        /**
         * @brief Verifies that when a consumable item is selected in the list box, its
         * configuration values (min delay, max delay, and hotkey) are loaded into the
         * corresponding text boxes for editing
         * 
         * When users click on a consumable item in the potions configuration window,
         * the system must display that consumable's current settings in the UI text boxes.
         * This allows users to view and modify the minimum delay (how soon after the
         * previous use to use again), maximum delay (randomized upper bound to appear
         * more human-like), and the hotkey associated with this consumable.
         */
        private void _testSelecingConsumablePopulatesTextBoxes()
        {
            var handler = _fixture();
            _setupListBoxItem();
            _consumablesListBox.SelectedIndex = 0;
            Debug.Assert(_minDelayTextBox.Text == "456");
            Debug.Assert(_maxDelayTextBox.Text == "345");
            Debug.Assert(_keyTextBox.Text == "234");
        }

        public void Run()
        {
            _testSelecingConsumablePopulatesTextBoxes();
        }
    }


    public class WindowPotionsMenuConsumableDeselectedActionHandlerTests
    {
        private TextBox _minDelayTextBox = new TextBox();

        private TextBox _maxDelayTextBox = new TextBox();

        private TextBox _keyTextBox = new TextBox();

        private ListBox _consumablesListBox = new ListBox();

        private Consumable _consumable = new Consumable();
        private AbstractWindowActionHandler _fixture()
        {
            _minDelayTextBox = new TextBox();
            _maxDelayTextBox = new TextBox();
            _keyTextBox = new TextBox();
            _consumablesListBox = new ListBox();
            _consumable = new Consumable
            {
                Active = 123,
                Key = "234",
                MaxDelay = 345,
                MinDelay = 456,
                Name = "567"
            };
            return new WindowPotionsMenuConsumableDeselectedActionHandlerFacade(
                _minDelayTextBox,
                _maxDelayTextBox,
                _keyTextBox,
                _consumablesListBox
            );
        }

        private void _setupListBoxItem()
        {
            _consumablesListBox.Items.Add(
                new ListBoxItem
                {
                    Tag = new ConsumableDataTag
                    {
                        Consumable = _consumable
                    }
                }
            );
        }

        /**
         * @brief Verifies that when a different consumable is selected or the selection
         * is cleared, the currently edited consumable's settings are saved to its data tag
         * 
         * When users modify a consumable's min delay, max delay, or hotkey in the text
         * boxes, these changes must be persisted to the consumable's data tag when the
         * user selects a different consumable or clears the selection. This ensures no
         * configuration changes are lost when navigating between different consumables
         * in the list.
         */
        private void _testDeselectingConsumableSavesTextBoxes()
        {
            var handler = _fixture();
            _setupListBoxItem();
            _minDelayTextBox.Text = "12";
            _maxDelayTextBox.Text = "23";
            _keyTextBox.Text = "34";
            _consumablesListBox.SelectedIndex = 0;
            Debug.Assert(_consumable.MinDelay == 456);
            Debug.Assert(_consumable.MaxDelay == 345);
            Debug.Assert(_consumable.Key == "234");
            _consumablesListBox.SelectedIndex = -1;
            Debug.Assert(_consumable.MinDelay == 12);
            Debug.Assert(_consumable.MaxDelay == 23);
            Debug.Assert(_consumable.Key == "34");
        }

        public void Run()
        {
            _testDeselectingConsumableSavesTextBoxes();
        }
    }


    public class WindowPotionsMenuConsumableItemSaveActionHandlerTests
    {
        private ListBox _consumablesListBox = new ListBox();

        private MockSystemWindow _potionsWindow = new MockSystemWindow();

        private AbstractWindowActionHandler _fixture()
        {
            _consumablesListBox = new ListBox();
            _potionsWindow = new MockSystemWindow();
            _potionsWindow.GetWindowReturn.Add(new Window());
            return new WindowPotionsMenuConsumableItemSaveActionHandlerFacade(
                _consumablesListBox,
                _potionsWindow
            );
        }

        private void _setupListBoxItems()
        {
            foreach (var isChecked in new[] { true, false })
            {
                _consumablesListBox.Items.Add(
                    new ListBoxItem
                    {
                        Tag = new ConsumableDataTag
                        {
                            Consumable = new Consumable
                            {
                                Name = "bark",
                                Active = 123
                            }
                        },
                        Content = new StackPanel
                        {
                            Children =
                            {
                                new CheckBox
                                {
                                    IsChecked = isChecked
                                },
                                new TextBox
                                {
                                    Text = "meow"
                                }
                            }
                        }
                    }
                );
            }
        }

        /**
         * @brief Verifies that when the potions configuration window is closed or hidden,
         * the checkbox states and text box content for each consumable are saved to the
         * corresponding consumable's Active flag and Name property
         * 
         * When users modify consumable items in the potions window, they can toggle the
         * checkbox (indicating whether automatic potion usage is active) and edit the
         * text box (changing the consumable's display name). When the window is closed
         * or hidden, the system must persist both of these values.
         */
        private void _testClosingPotionsMenuSavesListBoxItems()
        {
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture();
                _setupListBoxItems();
                _potionsWindow.VisibleReturn.Add(visible);
                handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
                var listBoxItem0 = (ListBoxItem)_consumablesListBox.Items[0];
                var listBoxItem1 = (ListBoxItem)_consumablesListBox.Items[1];
                var dataTag0 = (ConsumableDataTag)listBoxItem0.Tag;
                var dataTag1 = (ConsumableDataTag)listBoxItem1.Tag;
                Debug.Assert(dataTag0.Consumable.Active == ((!visible) ? 1 : 123));
                Debug.Assert(dataTag1.Consumable.Active == ((!visible) ? 0 : 123));
                Debug.Assert(dataTag0.Consumable.Name == ((!visible) ? "meow" : "bark"));
                Debug.Assert(dataTag1.Consumable.Name == ((!visible) ? "meow" : "bark"));
            }
        }

        public void Run()
        {
            _testClosingPotionsMenuSavesListBoxItems();
        }
    }


    public class WindowPotionsMenuConsumableAddActionHandlerTests
    {
        private Button _addConsumableButton = new Button();

        private ListBox _consumableListBox = new ListBox();

        private StackPanel _consumableTemplate = new StackPanel();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private AbstractWindowActionHandler _fixture()
        {
            _addConsumableButton = new Button();
            _consumableListBox = new ListBox();
            _consumableTemplate = StackPanelTemplate.Fixture();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration();
            var handler = new WindowPotionsMenuConsumableAddActionHandlerFacade(
                _addConsumableButton,
                _consumableListBox,
                _consumableTemplate
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Verifies that clicking the Add Consumable button creates a new consumable
         * item at the end of the list box (or at index 0 if the list is empty)
         * 
         * When users need to add a new potion or consumable item to automate, they click
         * the Add Consumable button. The system must create a new consumable entry in both
         * the UI list box and the configuration model. The new consumable is initialized
         * with default values and appears at the appropriate position in the list.
         */
        private void _testAddingConsumable()
        {
            var handler = _fixture();
            var routedEvent = new RoutedEventArgs(Button.ClickEvent);
            _addConsumableButton.RaiseEvent(routedEvent);
            Debug.Assert(_consumableListBox.Items.Count == 1);
            var listBoxItem = (ListBoxItem)_consumableListBox.Items[0];
            Debug.Assert(listBoxItem.Tag is ConsumableDataTag);
            var dataTag = (ConsumableDataTag)listBoxItem.Tag;
            Debug.Assert(_maplestoryBotConfiguration.Consumables.Count == 1);
            Debug.Assert(_maplestoryBotConfiguration.Consumables[0] == dataTag.Consumable);
        }

        /**
         * @brief Verifies that when a consumable item is selected in the list, clicking
         * Add inserts the new consumable below the currently selected item
         * 
         * When users want to add a new consumable between existing items (e.g., inserting
         * a potion between two existing ones), they can select an item and click Add.
         * The new consumable should be inserted immediately below the selected item,
         * preserving the logical order of consumables in both the UI and configuration model.
         */
        private void _testAddingConsumableAtCenter()
        {
            var handler = _fixture();
            var routedEvent = new RoutedEventArgs(Button.ClickEvent);
            var fillerObjects = new[] { new object(), new object(), new object() };
            var consumableObjects = new[] { new Consumable(), new Consumable(), new Consumable() };
            _consumableListBox.Items.Add(fillerObjects[0]);
            _consumableListBox.Items.Add(fillerObjects[1]);
            _consumableListBox.Items.Add(fillerObjects[2]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[0]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[1]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[2]);
            _consumableListBox.SelectedIndex = 1;
            _addConsumableButton.RaiseEvent(routedEvent);
            Debug.Assert(_consumableListBox.Items.Count == 4);
            Debug.Assert(_consumableListBox.Items[0] == fillerObjects[0]);
            Debug.Assert(_consumableListBox.Items[1] == fillerObjects[1]);
            Debug.Assert(_consumableListBox.Items[2] is ListBoxItem);
            Debug.Assert(_consumableListBox.Items[3] == fillerObjects[2]);
            var listBoxItem = (ListBoxItem)_consumableListBox.Items[2];
            Debug.Assert(listBoxItem.Tag is ConsumableDataTag);
            var dataTag = (ConsumableDataTag)listBoxItem.Tag;
            Debug.Assert(_maplestoryBotConfiguration.Consumables.Count == 4);
            Debug.Assert(_maplestoryBotConfiguration.Consumables[0] == consumableObjects[0]);
            Debug.Assert(_maplestoryBotConfiguration.Consumables[1] == consumableObjects[1]);
            Debug.Assert(_maplestoryBotConfiguration.Consumables[2] == dataTag.Consumable);
            Debug.Assert(_maplestoryBotConfiguration.Consumables[3] == consumableObjects[2]);
        }

        /**
         * @brief Verifies that when no consumable is selected in the list, clicking Add
         * appends the new consumable at the end of the list
         * 
         * When users simply want to add a new consumable to the end of their existing list,
         * clicking Add without any selection should append the new item at the end. This
         * is the most common workflow for adding new potions and consumables.
         */
        private void _testAddingConsumableAtEnd()
        {
            var handler = _fixture();
            var routedEvent = new RoutedEventArgs(Button.ClickEvent);
            var fillerObjects = new[] { new object(), new object(), new object() };
            var consumableObjects = new[] { new Consumable(), new Consumable(), new Consumable() };
            _consumableListBox.Items.Add(fillerObjects[0]);
            _consumableListBox.Items.Add(fillerObjects[1]);
            _consumableListBox.Items.Add(fillerObjects[2]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[0]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[1]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[2]);
            _addConsumableButton.RaiseEvent(routedEvent);
            Debug.Assert(_consumableListBox.Items.Count == 4);
            Debug.Assert(_consumableListBox.Items[0] == fillerObjects[0]);
            Debug.Assert(_consumableListBox.Items[1] == fillerObjects[1]);
            Debug.Assert(_consumableListBox.Items[2] == fillerObjects[2]);
            Debug.Assert(_consumableListBox.Items[3] is ListBoxItem);
            var listBoxItem = (ListBoxItem)_consumableListBox.Items[3];
            Debug.Assert(listBoxItem.Tag is ConsumableDataTag);
            var dataTag = (ConsumableDataTag)listBoxItem.Tag;
            Debug.Assert(_maplestoryBotConfiguration.Consumables.Count == 4);
            Debug.Assert(_maplestoryBotConfiguration.Consumables[0] == consumableObjects[0]);
            Debug.Assert(_maplestoryBotConfiguration.Consumables[1] == consumableObjects[1]);
            Debug.Assert(_maplestoryBotConfiguration.Consumables[2] == consumableObjects[2]);
            Debug.Assert(_maplestoryBotConfiguration.Consumables[3] == dataTag.Consumable);
        }

        /**
         * @brief Verifies that newly added consumable items inherit the correct visual
         * properties (styling, margins, colors, font, alignment) from the template
         * 
         * When a new consumable is added, its UI elements must match the template styling
         * to ensure a consistent, professional appearance. This includes the stack panel
         * orientation, focusability, checkbox alignment, text box dimensions, background
         * colors, foreground colors, and font family. The text box should display a default
         * name like "petfood" to guide the user.
         */
        private void _testAddingConsumableProperties()
        {
            var handler = _fixture();
            var routedEvent = new RoutedEventArgs(Button.ClickEvent);
            var stackPanelTemplate = StackPanelTemplate.Fixture();
            var textBoxTemplate = stackPanelTemplate.Children.OfType<TextBox>().First();
            var checkBoxTemplate = stackPanelTemplate.Children.OfType<CheckBox>().First();
            var backgroundTemplate = (SolidColorBrush)textBoxTemplate.Background;
            var foregroundTemplate = (SolidColorBrush)textBoxTemplate.Foreground;
            _addConsumableButton.RaiseEvent(routedEvent);
            var listBoxItem = (ListBoxItem)_consumableListBox.Items[0];
            Debug.Assert(listBoxItem.Content is StackPanel);
            var stackPanel = (StackPanel)listBoxItem.Content;
            var textBox = stackPanel.Children.OfType<TextBox>().First();
            var checkBox = stackPanel.Children.OfType<CheckBox>().First();
            var background = (SolidColorBrush)textBox.Background;
            var foreground = (SolidColorBrush)textBox.Foreground;
            Debug.Assert(stackPanel.Orientation == stackPanelTemplate.Orientation);
            Debug.Assert(stackPanel.Focusable == stackPanelTemplate.Focusable);
            Debug.Assert(checkBox.VerticalContentAlignment == checkBoxTemplate.VerticalContentAlignment);
            Debug.Assert(textBox.Margin == textBoxTemplate.Margin);
            Debug.Assert(textBox.VerticalContentAlignment == textBoxTemplate.VerticalContentAlignment);
            Debug.Assert(textBox.HorizontalContentAlignment == textBoxTemplate.HorizontalContentAlignment);
            Debug.Assert(textBox.Width == textBoxTemplate.Width);
            Debug.Assert(textBox.Height == textBoxTemplate.Height);
            Debug.Assert(background.Color.R == backgroundTemplate.Color.R);
            Debug.Assert(background.Color.G == backgroundTemplate.Color.G);
            Debug.Assert(background.Color.B == backgroundTemplate.Color.B);
            Debug.Assert(background.Color.A == backgroundTemplate.Color.A);
            Debug.Assert(foreground.Color.R == foregroundTemplate.Color.R);
            Debug.Assert(foreground.Color.G == foregroundTemplate.Color.G);
            Debug.Assert(foreground.Color.B == foregroundTemplate.Color.B);
            Debug.Assert(foreground.Color.A == foregroundTemplate.Color.A);
            Debug.Assert(textBox.FontFamily.ToString() == textBoxTemplate.FontFamily.ToString());
            Debug.Assert(textBox.Text == "petfood");
        }

        public void Run()
        {
            _testAddingConsumable();
            _testAddingConsumableAtCenter();
            _testAddingConsumableAtEnd();
            _testAddingConsumableProperties();
        }
    }


    public class WindowPotionsMenuConsumableRemoveActionHandlerTests
    {
        private Button _removeConsumableButton = new Button();

        private ListBox _consumableListBox = new ListBox();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private AbstractWindowActionHandler _fixture()
        {
            _removeConsumableButton = new Button();
            _consumableListBox = new ListBox();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration();
            var handler = new WindowPotionsMenuConsumableRemoveActionHandlerFacade(
                _consumableListBox,
                _removeConsumableButton
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Verifies that clicking the Remove Consumable button when no item is
         * selected does nothing and does not cause any errors
         * 
         * When users click the Remove Consumable button without first selecting a
         * consumable item from the list, the system should safely ignore the action
         * without crashing or modifying the list.
         */
        private void _testRemovingNothingDoesNothing()
        {
            var handler = _fixture();
            var routedEvent = new RoutedEventArgs(Button.ClickEvent);
            _removeConsumableButton.RaiseEvent(routedEvent);
            Debug.Assert(_consumableListBox.Items.Count == 0);
        }

        /**
         * @brief Verifies that clicking the Remove Consumable button with a specific
         * consumable selected removes only that item from both the list box and the
         * configuration model, preserving the order of remaining items
         * 
         * When users select a specific consumable in the list and click the Remove
         * Consumable button, only the selected consumable should be removed from both
         * the UI list and the configuration model. The remaining consumables should
         * stay in their original relative order, with items after the removed position
         * shifting left to fill the gap.
         */
        private void _testRemovingSelectedIndex()
        {
            var handler = _fixture();
            var routedEvent = new RoutedEventArgs(Button.ClickEvent);
            var fillerObjects = new[] { new object(), new object(), new object() };
            var consumableObjects = new[] { new Consumable(), new Consumable(), new Consumable() };
            _consumableListBox.Items.Add(fillerObjects[0]);
            _consumableListBox.Items.Add(fillerObjects[1]);
            _consumableListBox.Items.Add(fillerObjects[2]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[0]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[1]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[2]);
            _consumableListBox.SelectedIndex = 1;
            _removeConsumableButton.RaiseEvent(routedEvent);
            Debug.Assert(_consumableListBox.Items.Count == 2);
            Debug.Assert(_maplestoryBotConfiguration.Consumables.Count == 2);
            Debug.Assert(_consumableListBox.Items.IndexOf(fillerObjects[1]) == -1);
            Debug.Assert(_maplestoryBotConfiguration.Consumables.IndexOf(consumableObjects[1]) == -1);
            Debug.Assert(_consumableListBox.SelectedIndex == 1);
        }

        /**
         * @brief Verifies that clicking the Remove Consumable button with no selection
         * removes the last item from the list box and configuration model
         * 
         * When users click the Remove Consumable button without any specific consumable
         * selected, the system should remove the last consumable in the list. This
         * provides a predictable behavior for users who want to delete the most recently
         * added consumable or remove items from the end of their consumable list.
         */
        private void _testRemovingLastItem()
        {
            var handler = _fixture();
            var routedEvent = new RoutedEventArgs(Button.ClickEvent);
            var fillerObjects = new[] { new object(), new object(), new object() };
            var consumableObjects = new[] { new Consumable(), new Consumable(), new Consumable() };
            _consumableListBox.Items.Add(fillerObjects[0]);
            _consumableListBox.Items.Add(fillerObjects[1]);
            _consumableListBox.Items.Add(fillerObjects[2]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[0]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[1]);
            _maplestoryBotConfiguration.Consumables.Add(consumableObjects[2]);
            _removeConsumableButton.RaiseEvent(routedEvent);
            Debug.Assert(_consumableListBox.Items.Count == 2);
            Debug.Assert(_maplestoryBotConfiguration.Consumables.Count == 2);
            Debug.Assert(_consumableListBox.Items.IndexOf(fillerObjects[2]) == -1);
            Debug.Assert(_maplestoryBotConfiguration.Consumables.IndexOf(consumableObjects[2]) == -1);
            Debug.Assert(_consumableListBox.SelectedIndex == -1);
        }

        public void Run()
        {
            _testRemovingNothingDoesNothing();
            _testRemovingSelectedIndex();
            _testRemovingLastItem();
        }
    }


    public class WindowPotionsMenuHandlersTests
    {
        public void Run()
        {
            new PotionsMenuScreenCaptureSubscriberTests().Run();
            new WindowPotionsMenuLoadingResourceActionHandlerTests().Run();
            new WindowPotionsMenuSavingResourceActionHandlerTests().Run();
            new WindowPotionsMenuTextBoxFrameRGBActionHandlerTests().Run();
            new WindowPotionsMenuResourceBarActionHandlerTests().Run();
            new WindowPotionsMenuRGBLabelActionHandlerTests().Run();
            new WindowPotionsMenuRGBFrameActionHandlerTests().Run();
            new WindowPotionsMenuSaveConfigurationActionHandlerTests().Run();
            new WindowPotionsMenuLoadingConsumablesActionHandlerTests().Run();
            new WindowPotionsMenuConsumableSelectedActionHandlerTests().Run();
            new WindowPotionsMenuConsumableDeselectedActionHandlerTests().Run();
            new WindowPotionsMenuConsumableItemSaveActionHandlerTests().Run();
            new WindowPotionsMenuConsumableAddActionHandlerTests().Run();
            new WindowPotionsMenuConsumableRemoveActionHandlerTests().Run();
        }
    }
}
