using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class AilmentParametersFixture
    {
        public static List<Ailment> GetAilmentsParameters()
        {
            return [
                new Ailment
                {
                    ImageDirectory = "a1_dir",
                    Active = 123,
                    ActiveDelay = 234,
                    CheckDelay = 345,
                    Threshold = 456,
                    Overlap = 567,
                    StaticRect = [12, 23, 34, 45],
                    AllCure = 678,
                    ArrowKeys = 789,
                    StopBot = 890,
                },
                new Ailment
                {
                    ImageDirectory = "a2_dir",
                    Active = 0,
                    ActiveDelay = 345,
                    CheckDelay = 456,
                    Threshold = 567,
                    Overlap = 678,
                    StaticRect = [23, 34, 45, 56],
                    AllCure = 789,
                    ArrowKeys = 890,
                    StopBot = 901,
                }
            ];
        }

        public static List<List<Image<Bgra32>>> GetAilmentsImages()
        {
            var images = new List<List<Image<Bgra32>>>
            {
                (
                    [
                        new Image<Bgra32>(2, 2),
                        new Image<Bgra32>(2, 2),
                        new Image<Bgra32>(2, 2),
                        new Image<Bgra32>(2, 2)
                    ]
                ),
                (
                    [
                        new Image<Bgra32>(2, 2),
                        new Image<Bgra32>(2, 2),
                        new Image<Bgra32>(2, 2),
                        new Image<Bgra32>(2, 2)
                    ]
                )
            };
            byte pixelValue = 0;
            for (int i = 0; i < images.Count; i++)
            for (int j = 0; j < images[i].Count; j++)
            for (int y = 0; y < images[i][j].Height; y++)
            for (int x = 0; x < images[i][j].Width; x++)
            {
                images[i][j][x, y] = new Bgra32(
                    pixelValue++,
                    pixelValue++,
                    pixelValue++,
                    pixelValue++
                );
            }
            return images;
        }

        public static List<List<BitmapSource>> GetAilmentsBitmaps()
        {
            var images = GetAilmentsImages();
            var bitmaps = new List<List<BitmapSource>>();
            foreach (var ailmentImages in images)
            {
                var ailmentBitmaps = new List<BitmapSource>();
                foreach (var image in ailmentImages)
                {
                    var bitmapSource = _toBitmapSource(image);
                    ailmentBitmaps.Add(bitmapSource);
                }
                bitmaps.Add(ailmentBitmaps);
            }
            return bitmaps;
        }

        private static BitmapSource _toBitmapSource(Image<Bgra32> image)
        {
            var pixelData = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelData);
            return BitmapSource.Create(
                image.Width,
                image.Height,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                pixelData,
                image.Width * 4
            );
        }
    }


    public class WindowAilmentsLoadActionHandlerTests
    {
        private ListBox _ailmentsListBox = new ListBox();

        private StackPanel _ailmentsStackPanelTemplate = new StackPanel();

        private MockSystemWindow _ailmentsWindow = new MockSystemWindow();

        private MaplestoryBotConfiguration _configuration = new MaplestoryBotConfiguration();

        private AbstractWindowActionHandler _fixture()
        {
            _ailmentsListBox = new ListBox();
            _ailmentsStackPanelTemplate = new StackPanel();
            _ailmentsStackPanelTemplate.Children.Add(new CheckBox());
            _ailmentsStackPanelTemplate.Children.Add(
                new TextBlock
                {
                    Margin = new Thickness(123, 234, 345, 456),
                    Foreground = Brushes.AntiqueWhite,
                    VerticalAlignment = VerticalAlignment.Stretch
                }
            );
            _ailmentsWindow = new MockSystemWindow();
            _configuration = new MaplestoryBotConfiguration();
            _ailmentsWindow.GetWindowReturn.Add(new Window());
            var handler = new WindowAilmentsLoadActionHandlerFacade(
                _ailmentsListBox,
                _ailmentsStackPanelTemplate,
                _ailmentsWindow
            );
            handler.Inject(SystemInjectType.ConfigurationUpdate, _configuration);
            return handler;
        }

        private void _setupAilmentFixture()
        {
            var ailments = AilmentParametersFixture.GetAilmentsParameters();
            _configuration.Ailments["a1"] = ailments[0];
            _configuration.Ailments["a2"] = ailments[1];
        }

        private List<ListBoxItem> _getListBoxItems()
        {
            var listBoxItemList = new List<ListBoxItem>();
            foreach (ListBoxItem listBoxItem in _ailmentsListBox.Items)
            {
                listBoxItemList.Add(listBoxItem);
            }
            return listBoxItemList;
        }

        private List<StackPanel> _getStackPanels()
        {
            var stackPanels = new List<StackPanel>();
            foreach (ListBoxItem listBoxItem in _ailmentsListBox.Items)
            {
                var stackPanel = (StackPanel)listBoxItem.Content;
                stackPanels.Add(stackPanel);
            }
            return stackPanels;
        }

        private List<CheckBox> _getCheckboxes()
        {
            var checkBoxList = new List<CheckBox>();
            foreach (ListBoxItem listBoxItem in _ailmentsListBox.Items)
            {
                var stackPanel = (StackPanel)listBoxItem.Content;
                var checkBox = stackPanel.Children.OfType<CheckBox>().First();
                checkBoxList.Add(checkBox);
            }
            return checkBoxList;
        }

        private List<TextBlock> _getTextBlocks()
        {
            var textBlocks = new List<TextBlock>();
            foreach (ListBoxItem listBoxItem in _ailmentsListBox.Items)
            {
                var stackPanel = (StackPanel)listBoxItem.Content;
                var checkBox = stackPanel.Children.OfType<TextBlock>().First();
                textBlocks.Add(checkBox);
            }
            return textBlocks;
        }

        /**
         * @brief Verifies that the ailments list refreshes correctly when the window
         * becomes visible, and clears stale data when the window is hidden
         * 
         * When users open the ailments configuration window, the system must load the
         * latest ailment configuration from the botting model and display it. When the
         * window is closed or hidden, the list box should be cleared to prevent showing
         * stale data from a previous configuration when the window reopens. This ensures
         * the displayed data always reflects the current configuration state.
         */
        private void _testAilmentsWindowDisplaysAilments()
        {
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture();
                _setupAilmentFixture();
                if (visible)
                {
                    _ailmentsListBox.Items.Add(new ListBoxItem());
                }
                _ailmentsWindow.VisibleReturn.Add(visible);
                handler.OnDependencyEvent(
                    handler, new DependencyPropertyChangedEventArgs()
                );
                var listBoxItemList = _getListBoxItems();
                var stackPanelList = _getStackPanels();
                var checkBoxList = _getCheckboxes();
                var textBlockList = _getTextBlocks();
                if (visible)
                {
                    Debug.Assert(_ailmentsListBox.Items.Count == 2);
                    Debug.Assert(listBoxItemList.Count == 2);
                    Debug.Assert(stackPanelList.Count == 2);
                    Debug.Assert(checkBoxList.Count == 2);
                    Debug.Assert(textBlockList.Count == 2);
                    Debug.Assert(checkBoxList[0].IsChecked == true);
                    Debug.Assert(checkBoxList[1].IsChecked == false);
                    Debug.Assert(textBlockList[0].Text == "a1");
                    Debug.Assert(textBlockList[1].Text == "a2");
                }
                else
                {
                    Debug.Assert(_ailmentsListBox.Items.Count == 0);
                    Debug.Assert(listBoxItemList.Count == 0);
                    Debug.Assert(stackPanelList.Count == 0);
                    Debug.Assert(checkBoxList.Count == 0);
                    Debug.Assert(textBlockList.Count == 0);
                }
            }
        }

        /**
         * @brief Verifies that each ailment list item uses the correct visual styling
         * and layout properties defined in the template
         * 
         * When ailments are loaded into the list box, each item must follow the UI
         * template with proper styling. The stack panel should arrange elements
         * horizontally, and text blocks should have the correct margin, foreground
         * color, and vertical alignment as defined in the fixture.
         */
        private void _testAilmentsWindowDisplaysProperties()
        {
            var handler = _fixture();
            _setupAilmentFixture();
            _ailmentsWindow.VisibleReturn.Add(true);
            handler.OnDependencyEvent(
                handler, new DependencyPropertyChangedEventArgs()
            );
            var stackPanelList = _getStackPanels();
            var checkBoxList = _getCheckboxes();
            var textBlockList = _getTextBlocks();
            foreach (var stackPanel in stackPanelList)
            {
                Debug.Assert(stackPanel.Orientation == Orientation.Horizontal);
            }
            foreach (var textBlock in textBlockList)
            {
                Debug.Assert(textBlock.Margin.Left == 123);
                Debug.Assert(textBlock.Margin.Top == 234);
                Debug.Assert(textBlock.Margin.Right == 345);
                Debug.Assert(textBlock.Margin.Bottom == 456);
                Debug.Assert(textBlock.Foreground == Brushes.AntiqueWhite);
                Debug.Assert(textBlock.VerticalAlignment == VerticalAlignment.Stretch);
            }
        }

        /**
         * @brief Verifies that the first ailment in the list is automatically selected
         * when the ailments window opens
         * 
         * When users open the ailments configuration window, the first ailment should
         * be selected by default. This allows users to immediately view and edit the
         * settings for the first ailment without having to click on it manually.
         */
        private void _testAilmentsWindowSelectsFirstItem()
        {
            var handler = _fixture();
            _setupAilmentFixture();
            _ailmentsWindow.VisibleReturn.Add(true);
            handler.OnDependencyEvent(
                handler, new DependencyPropertyChangedEventArgs()
            );
            Debug.Assert(_ailmentsListBox.SelectedIndex == 0);
        }

        /**
         * @brief Verifies that each list box item stores the full ailment configuration
         * data and associated reference images in its Tag property for later access
         * 
         * When ailments are loaded, each list box item must store a data tag containing
         * the complete ailment configuration (active delay, threshold, static rectangle,
         * etc.) and the reference images used for detecting that ailment. This stored
         * data allows the editor to retrieve and modify ailment settings when the user
         * selects different ailments.
         */
        private void _testAilmentsWindowSetsListBoxItemTag()
        {
            var handler = _fixture();
            _setupAilmentFixture();
            var expecteds = AilmentParametersFixture.GetAilmentsParameters();
            _ailmentsWindow.VisibleReturn.Add(true);
            handler.OnDependencyEvent(
                handler, new DependencyPropertyChangedEventArgs()
            );
            var listBoxItemList = _getListBoxItems();
            var textBlockList = _getTextBlocks();
            for (int i = 0; i < listBoxItemList.Count; i++)
            {
                Debug.Assert(listBoxItemList[i].Tag is ListBoxAilmentsDataTag);
                var tag = (ListBoxAilmentsDataTag)listBoxItemList[i].Tag;
                var expected = expecteds[i];
                var tagJson = JsonSerializer.Serialize(tag.Ailment);
                var expectedJson = JsonSerializer.Serialize(expected);
                Debug.Assert(tagJson == expectedJson);
                Debug.Assert(listBoxItemList[i].Name == textBlockList[i].Text);
            }
        }

        public void Run()
        {
            _testAilmentsWindowDisplaysAilments();
            _testAilmentsWindowDisplaysProperties();
            _testAilmentsWindowSelectsFirstItem();
            _testAilmentsWindowSetsListBoxItemTag();
        }
    }


    public class WindowAilmentsLoadImageActionHandlerTests
    {
        private ListBox _ailmentsListBox = new ListBox();

        private Grid _ailmentsImageGrid = new Grid();

        private MockSystemWindow _ailmentsWindow = new MockSystemWindow();

        private ConfigurationImages _configurationImages = new ConfigurationImages();

        private List<ListBoxItem> _listBoxItems = [];

        private AbstractWindowActionHandler _fixture()
        {
            _ailmentsListBox = new ListBox();
            _ailmentsImageGrid = new Grid();
            _ailmentsWindow = new MockSystemWindow();
            _configurationImages = new ConfigurationImages();
            _listBoxItems = [
                new ListBoxItem
                {
                    Name = "meow1",
                    Tag = new ListBoxAilmentsDataTag()
                },
                new ListBoxItem
                {
                    Name = "meow2",
                    Tag = new ListBoxAilmentsDataTag()
                }
            ];
            _ailmentsListBox.Items.Add(_listBoxItems[0]);
            _ailmentsListBox.Items.Add(_listBoxItems[1]);
            _ailmentsWindow.GetWindowReturn.Add(new Window());
            var handler = new WindowAilmentsLoadImagesActionHandlerFacade(
                _ailmentsListBox,
                _ailmentsImageGrid,
                _ailmentsWindow
            );
            handler.Inject(SystemInjectType.ConfigurationUpdate, _configurationImages);
            return handler;
        }

        private byte[] _getPixels(ImageSource imageSource)
        {
            var bitmapSource = (BitmapSource)imageSource;
            int stride = bitmapSource.PixelWidth * 4;
            int size = stride * bitmapSource.PixelHeight;
            byte[] pixels = new byte[size];
            bitmapSource.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        /**
         * @brief Verifies that when the ailments window becomes visible, all reference
         * images for each ailment are loaded into the grid, displayed as BitmapSource
         * objects, and contain the correct pixel data matching the original fixture values
         * 
         * When users open the ailments configuration window, the system must load all
         * reference images for each status ailment (e.g., Seal, Weakness) to display
         * them in the image grid. Each image should be converted to a BitmapSource with
         * the correct pixel format, and the pixel values must match the expected sequence
         * from the test fixture.
         */
        private void _testLoadingImages()
        {
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture();
                byte pixelValue = 0;
                _ailmentsWindow.VisibleReturn.Add(visible);
                var parameters = AilmentParametersFixture.GetAilmentsImages();
                _configurationImages.AilmentImages["meow1"] = parameters[0];
                _configurationImages.AilmentImages["meow2"] = parameters[1];
                handler.OnDependencyEvent(
                    handler, new DependencyPropertyChangedEventArgs()
                );
                Debug.Assert(_ailmentsListBox.Items.Count == 2);
                for (int i = 0; i < _ailmentsListBox.Items.Count; i++)
                {
                    var listBoxItem = (ListBoxItem)_ailmentsListBox.Items[i];
                    var dataTag = (ListBoxAilmentsDataTag)listBoxItem.Tag;
                    if (visible)
                    {
                        Debug.Assert(dataTag.Images.Count == 4);
                        Debug.Assert(_ailmentsImageGrid.Children.Count == 8);
                        for (int j = 0; j < dataTag.Images.Count; j++)
                        {
                            var bitmapSource = (BitmapSource)dataTag.Images[j].Source;
                            for (int y = 0; y < bitmapSource.PixelHeight; y++)
                            for (int x = 0; x < bitmapSource.PixelWidth; x++)
                            {
                                var pixels = _getPixels(bitmapSource);
                                var index = (y * bitmapSource.PixelWidth * 4) + (x * 4);
                                Debug.Assert(pixels[index + 2] == pixelValue++);
                                Debug.Assert(pixels[index + 1] == pixelValue++);
                                Debug.Assert(pixels[index + 0] == pixelValue++);
                                Debug.Assert(pixels[index + 3] == pixelValue++);
                            }
                            Debug.Assert(dataTag.Images[j].Parent == _ailmentsImageGrid);
                        }
                    }
                    else
                    {
                        Debug.Assert(dataTag.Images.Count == 0);
                        Debug.Assert(_ailmentsImageGrid.Children.Count == 0);
                    }
                }
            }
        }

        /**
         * @brief Verifies that when the ailments window becomes visible, the system
         * reuses existing image controls from the grid if they are already present,
         * instead of recreating them from scratch
         * 
         * When users toggle the ailments window open and closed multiple times, the
         * system should preserve and reuse the existing image controls in the grid
         * rather than creating new ones. This prevents memory leaks and improves
         * performance by avoiding unnecessary recreation of UI elements. The existing
         * images are reassociated with the current ailment data tags when the window
         * reopens.
         */
        private void _testAlreadyLoadedImages()
        {
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture();
                _ailmentsWindow.VisibleReturn.Add(visible);
                var images = new List<System.Windows.Controls.Image>();
                for (int i = 0; i < 2; i++)
                for (int j = 0; j < 4; j++)
                {
                    var name = "meow" + (i + 1).ToString() + j.ToString();
                    images.Add(new System.Windows.Controls.Image { Name = name });
                    _ailmentsImageGrid.Children.Add(images.Last());
                }
                _configurationImages.AilmentImages["meow1"] = [];
                _configurationImages.AilmentImages["meow2"] = [];
                handler.OnDependencyEvent(
                    handler, new DependencyPropertyChangedEventArgs()
                );
                var gridChildren = _ailmentsImageGrid.Children;
                var gridImages = gridChildren.OfType<System.Windows.Controls.Image>().ToList();
                Debug.Assert(gridImages.Count == 8);
                Debug.Assert(_ailmentsListBox.Items.Count == 2);
                if (visible)
                {
                    for (int i = 0; i < _ailmentsListBox.Items.Count; i++)
                    {
                        var item = (ListBoxItem)_ailmentsListBox.Items[i];
                        var tag = (ListBoxAilmentsDataTag)item.Tag;
                        Debug.Assert(tag.Images.Count == 4);
                        foreach (var tagImage in tag.Images)
                        {
                            Debug.Assert(images.IndexOf(tagImage) >= 0);
                            Debug.Assert(gridImages.IndexOf(tagImage) >= 0);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _ailmentsListBox.Items.Count; i++)
                    {
                        var item = (ListBoxItem)_ailmentsListBox.Items[i];
                        var tag = (ListBoxAilmentsDataTag)item.Tag;
                        Debug.Assert(tag.Images.Count == 0);
                    }
                }
            }
        }

        /**
         * @brief Verifies that each loaded ailment reference image control has the
         * correct visual properties for proper display in the UI
         * 
         * When ailments are loaded into the configuration window, each reference image
         * control must be configured with specific display settings to ensure consistent
         * presentation. The images should not stretch to maintain their original aspect
         * ratio and size, and should be centered within their allocated space. Initially,
         * all images are hidden until their corresponding ailment tab is selected.
         */
        private void _testLoadedImageProperties()
        {
            var handler = _fixture();
            _ailmentsWindow.VisibleReturn.Add(true);
            var parameters = AilmentParametersFixture.GetAilmentsImages();
            _configurationImages.AilmentImages["meow1"] = parameters[0];
            _configurationImages.AilmentImages["meow2"] = parameters[1];
            handler.OnDependencyEvent(
                handler, new DependencyPropertyChangedEventArgs()
            );
            for (int i = 0; i < 2; i++)
            for (int j = 0; j < 4; j++)
            {
                var gridChildren = _ailmentsImageGrid.Children;
                var gridImages = gridChildren.OfType<System.Windows.Controls.Image>().ToList();
                var image = gridImages.Find(
                    img => img.Name == "meow" + (i + 1).ToString() + j.ToString()
                );
                Debug.Assert(image != null);
                Debug.Assert(image.Stretch == Stretch.None);
                Debug.Assert(image.HorizontalAlignment == HorizontalAlignment.Center);
                Debug.Assert(image.VerticalAlignment == VerticalAlignment.Center);
                Debug.Assert(image.Visibility == Visibility.Hidden);
            }
        }

        public void Run()
        {
            _testLoadingImages();
            _testAlreadyLoadedImages();
            _testLoadedImageProperties();
        }
    }


    public class WindowAilmentsAnimationActionHandlerTests
    {
        private ListBox _ailmentsListBox = new ListBox();

        private MockCompositionEventHandler _compositionEventHandler = new MockCompositionEventHandler();

        private MockTimestamp _animationStopwatch = new MockTimestamp();

        private MockSystemWindow _ailmentsWindow = new MockSystemWindow();

        private List<List<System.Windows.Controls.Image>> _imageLists = [];

        private AbstractWindowActionHandler _fixture()
        {
            _ailmentsListBox = new ListBox();
            _compositionEventHandler = new MockCompositionEventHandler();
            _animationStopwatch = new MockTimestamp();
            _ailmentsWindow = new MockSystemWindow();
            _imageLists = [
                [
                    new System.Windows.Controls.Image{ Visibility = Visibility.Hidden },
                    new System.Windows.Controls.Image{ Visibility = Visibility.Hidden },
                    new System.Windows.Controls.Image{ Visibility = Visibility.Hidden }
                ],
                [
                    new System.Windows.Controls.Image{ Visibility = Visibility.Hidden },
                    new System.Windows.Controls.Image{ Visibility = Visibility.Hidden },
                    new System.Windows.Controls.Image{ Visibility = Visibility.Hidden }
                ]
            ];
            _ailmentsListBox.Items.Add(
                new ListBoxItem()
                {
                    Tag = new ListBoxAilmentsDataTag
                    {
                        Images = _imageLists[0]
                    }
                }
            );
            _ailmentsListBox.Items.Add(
                new ListBoxItem()
                {
                    Tag = new ListBoxAilmentsDataTag
                    {
                        Images = _imageLists[1]
                    }
                }
            );
            _ailmentsWindow.GetWindowReturn.Add(new Window());
            return new WindowAilmentsAnimationActionHandler(
                _ailmentsListBox,
                _compositionEventHandler,
                _ailmentsWindow,
                _animationStopwatch,
                new WindowAilmentsAnimationModifier(),
                1.0 / 15.0
            );
        }

        /**
         * @brief Verifies that the animation frame only updates when sufficient time
         * has elapsed since the last update, and that the stopwatch timestamp is reset
         * only when a frame update actually occurs
         * 
         * When the animation timer ticks, the system should only advance to the next
         * animation frame if enough time has passed since the last frame change. This
         * prevents the animation from updating too rapidly when events fire faster than
         * the intended frame rate (e.g., when the rendering interval is shorter than the
         * desired 15fps frame duration). The test uses timestamps below and above the
         * 1/15 second threshold to verify that updates occur only when the elapsed
         * time meets or exceeds the frame interval.
         */
        private void _testDispatchEventModifiesVisibility()
        {
            foreach (var timestamp in new[] { 1.0 / 14.0, 1.0 / 15.0, 1.0 / 16.0 })
            {
                var handler = _fixture();
                var expectedSetTimestamps = 0;
                for (int i = 0; i < _imageLists.Count; i++)
                for (int j = 0; j < _imageLists[i].Count + 1; j++)
                {
                    _ailmentsListBox.SelectedIndex = i;
                    _animationStopwatch.GetTimestampReturn.Add(timestamp);
                    handler.OnEvent(null, new EventArgs());
                    var visibleList = _imageLists[i].FindAll(i => i.Visibility == Visibility.Visible).ToList();
                    if (timestamp >= 1.0 / 15.0)
                    {
                        Debug.Assert(visibleList.Count == 1);
                        Debug.Assert(_imageLists[i].IndexOf(visibleList[0]) == (j % _imageLists[i].Count));
                        Debug.Assert(_animationStopwatch.SetTimestampCalls == ++expectedSetTimestamps);
                    }
                    else
                    {
                        Debug.Assert(visibleList.Count == 0);
                        Debug.Assert(_animationStopwatch.SetTimestampCalls == 0);
                    }
                }
            }
        }

        /**
         * @brief Verifies that the animation timer starts when the ailments window becomes
         * visible and stops when the window is hidden
         * 
         * When users open the ailments configuration window, the animation system should
         * start the timer that cycles through the reference images for each status ailment.
         * When the window is closed or hidden, the timer should stop to conserve system
         * resources and prevent unnecessary UI updates. This ensures animations only run
         * when the user is actively viewing the ailments configuration.
         */
        private void _testVisibilityEventTogglesDispatch()
        {
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture();
                _ailmentsWindow.VisibleReturn.Add(visible);
                handler.OnDependencyEvent(handler, new DependencyPropertyChangedEventArgs());
                Debug.Assert(_compositionEventHandler.StartCalls == (visible ? 1 : 0));
                Debug.Assert(_compositionEventHandler.StopCalls == (visible ? 0 : 1));
            }
        }

        public void Run()
        {
            _testDispatchEventModifiesVisibility();
            _testVisibilityEventTogglesDispatch();
        }
    }


    public class WindowAilmentsSaveConfigurationActionHandlerTests
    {
        private TextBox _macroDelayTextBox = new TextBox();

        private TextBox _checkDelayTextBox = new TextBox();

        private TextBox _detectThresholdTextBox = new TextBox();

        private TextBox _allCureKeyTextBox = new TextBox();

        private TextBox _detectRectangleLeft = new TextBox();

        private TextBox _detectRectangleTop = new TextBox();

        private TextBox _detectRectangleRight = new TextBox();

        private TextBox _detectRectangleBottom = new TextBox();

        private ListBox _ailmentsListBox = new ListBox();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = new MaplestoryBotConfiguration();

        private List<ListBoxAilmentsDataTag> _dataTags = [];

        private AbstractWindowActionHandler _fixture()
        {
            _macroDelayTextBox = new TextBox();
            _checkDelayTextBox = new TextBox();
            _detectThresholdTextBox = new TextBox();
            _allCureKeyTextBox = new TextBox();
            _detectRectangleLeft = new TextBox();
            _detectRectangleTop = new TextBox();
            _detectRectangleRight = new TextBox();
            _detectRectangleBottom = new TextBox();
            _ailmentsListBox = new ListBox();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration();
            var handler = new WindowAilmentsSaveConfigurationActionHandlerFacade(
                _macroDelayTextBox,
                _checkDelayTextBox,
                _detectThresholdTextBox,
                _allCureKeyTextBox,
                _detectRectangleLeft,
                _detectRectangleTop,
                _detectRectangleRight,
                _detectRectangleBottom,
                _ailmentsListBox
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            _dataTags = [
                new ListBoxAilmentsDataTag{ Ailment = new Ailment{ StaticRect = [] } },
                new ListBoxAilmentsDataTag{ Ailment = new Ailment{ StaticRect = [] } },
                new ListBoxAilmentsDataTag{ Ailment = new Ailment{ StaticRect = null } }
            ];
            _ailmentsListBox.Items.Add(new ListBoxItem { Tag = _dataTags[0] });
            _ailmentsListBox.Items.Add(new ListBoxItem { Tag = _dataTags[1] });
            _ailmentsListBox.Items.Add(new ListBoxItem { Tag = _dataTags[2] });
            return handler;
        }

        /**
         * @brief Verifies that when the user deselects an ailment (either by selecting
         * a different ailment or clearing the selection), the current UI values are
         * saved to that ailment's configuration data and global settings
         * 
         * When users edit an ailment's settings (active delay, check delay, threshold,
         * all-cure key, and detection rectangle) and then move to a different ailment or
         * deselect entirely, the system must persist those changes to the corresponding
         * ailment data tag. This ensures configuration changes are not lost when
         * navigating between ailments.
         */
        private void _testDeselectingAilmentSavesConfiguration()
        {
            dynamic testData = new[]
            {
                new
                {
                    ActiveDelay = 12,
                    CheckDelay = 23,
                    Threshold = 34,
                    AilmentsAllCureKey = "A",
                    StaticRect = new[] {45, 56, 67, 78},
                },
                new
                {
                    ActiveDelay = 23,
                    CheckDelay = 34,
                    Threshold = 45,
                    AilmentsAllCureKey = "B",
                    StaticRect = new[] {56, 67, 78, 89},
                },
                new
                {
                    ActiveDelay = 12,
                    CheckDelay = 23,
                    Threshold = 34,
                    AilmentsAllCureKey = "C",
                    StaticRect = new[] {67, 78, 89, 90},
                }
            };
            var handler = _fixture();
            _ailmentsListBox.SelectedIndex = 0;
            for (int i = 0; i < testData.Length; i++)
            {
                _macroDelayTextBox.Text = testData[i].ActiveDelay.ToString();
                _checkDelayTextBox.Text = testData[i].CheckDelay.ToString();
                _detectThresholdTextBox.Text = testData[i].Threshold.ToString();
                _allCureKeyTextBox.Text = testData[i].AilmentsAllCureKey;
                _detectRectangleLeft.Text = testData[i].StaticRect[0].ToString();
                _detectRectangleTop.Text = testData[i].StaticRect[1].ToString();
                _detectRectangleRight.Text = testData[i].StaticRect[2].ToString();
                _detectRectangleBottom.Text = testData[i].StaticRect[3].ToString();
                var index = (i + 1 == testData.Length) ? -1 : i + 1;
                var ailment = _dataTags[i].Ailment;
                var keySettings = _maplestoryBotConfiguration.MacroKeySettings;
                _ailmentsListBox.SelectedIndex = index;
                Debug.Assert(ailment.ActiveDelay == (int)testData[i].ActiveDelay);
                Debug.Assert(ailment.CheckDelay == (int)testData[i].CheckDelay);
                Debug.Assert(ailment.Threshold == (int)testData[i].Threshold);
                Debug.Assert(keySettings.AilmentsAllcureKey == (string)testData[i].AilmentsAllCureKey);
                for (int j = 0; j < 4; j++)
                {
                    Debug.Assert(
                        index == -1 ?
                        ailment.StaticRect == null :
                        ailment.StaticRect![j] == (int)testData[i].StaticRect[j]
                    );
                }
            }
        }

        public void Run()
        {
            _testDeselectingAilmentSavesConfiguration();
        }
    }


    public class WindowAilmentsLoadConfigurationActionHandlerTests
    {
        private TextBox _macroDelayTextBox = new TextBox();

        private TextBox _checkDelayTextBox = new TextBox();

        private TextBox _detectThresholdTextBox = new TextBox();

        private TextBox _allCureKeyTextBox = new TextBox();

        private TextBox _detectRectangleLeft = new TextBox();

        private TextBox _detectRectangleTop = new TextBox();

        private TextBox _detectRectangleRight = new TextBox();

        private TextBox _detectRectangleBottom = new TextBox();

        private ListBox _ailmentsListBox = new ListBox();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private List<ListBoxAilmentsDataTag> _dataTags = [];

        private AbstractWindowActionHandler _fixture()
        {
            _macroDelayTextBox = new TextBox();
            _checkDelayTextBox = new TextBox();
            _detectThresholdTextBox = new TextBox();
            _allCureKeyTextBox = new TextBox();
            _detectRectangleLeft = new TextBox();
            _detectRectangleTop = new TextBox();
            _detectRectangleRight = new TextBox();
            _detectRectangleBottom = new TextBox();
            _ailmentsListBox = new ListBox();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                MacroKeySettings = new MacroKeySettings
                {
                    AilmentsAllcureKey = "meow"
                }
            };
            var handler = (
                new WindowAilmentsLoadConfigurationActionHandlerFacade(
                    _macroDelayTextBox,
                    _checkDelayTextBox,
                    _detectThresholdTextBox,
                    _allCureKeyTextBox,
                    _detectRectangleLeft,
                    _detectRectangleTop,
                    _detectRectangleRight,
                    _detectRectangleBottom,
                    _ailmentsListBox
                )
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            _dataTags = [
                new ListBoxAilmentsDataTag
                {
                    Ailment = new Ailment
                    {
                        ActiveDelay = 123,
                        CheckDelay = 234,
                        Threshold = 345,
                        StaticRect = [12, 23, 34, 45]
                    }
                },
                new ListBoxAilmentsDataTag
                {
                    Ailment = new Ailment
                    {
                        ActiveDelay = 234,
                        CheckDelay = 345,
                        Threshold = 456,
                        StaticRect = [23, 34, 45, 56]
                    }
                },
                new ListBoxAilmentsDataTag
                {
                    Ailment = new Ailment
                    {
                        ActiveDelay = 345,
                        CheckDelay = 456,
                        Threshold = 567,
                        StaticRect = null
                    }
                }
            ];
            _ailmentsListBox.Items.Add(new ListBoxItem { Tag = _dataTags[0] });
            _ailmentsListBox.Items.Add(new ListBoxItem { Tag = _dataTags[1] });
            _ailmentsListBox.Items.Add(new ListBoxItem { Tag = _dataTags[2] });
            return handler;
        }

        /**
         * @brief Verifies that when the user selects an ailment from the list box, the
         * UI text boxes are populated with that ailment's saved configuration values
         * 
         * When users click on a status ailment in the ailments list, the system must
         * load that ailment's settings (active delay, check delay, detection threshold,
         * and detection rectangle coordinates) into the corresponding input fields.
         * Global settings like the all-cure key are also loaded from the botting model
         * configuration. This allows users to view and edit the configuration for each
         * ailment individually. If an ailment has no valid detection rectangle (StaticRect
         * is null), the rectangle coordinate fields should be cleared.
         */
        private void _testSelectingAilmentLoadsConfiguration()
        {
            var handler = _fixture();
            _ailmentsListBox.SelectedIndex = 0;
            for (int i = 0; i < _ailmentsListBox.Items.Count; i++)
            {
                _ailmentsListBox.SelectedIndex = i;
                var ailment = _dataTags[i].Ailment;
                Debug.Assert(_macroDelayTextBox.Text == ailment.ActiveDelay.ToString());
                Debug.Assert(_checkDelayTextBox.Text == ailment.CheckDelay.ToString());
                Debug.Assert(_detectThresholdTextBox.Text == ailment.Threshold.ToString());
                Debug.Assert(_allCureKeyTextBox.Text == "meow");
                if (i != 2)
                {
                    Debug.Assert(_detectRectangleLeft.Text == ailment.StaticRect![0].ToString());
                    Debug.Assert(_detectRectangleTop.Text == ailment.StaticRect[1].ToString());
                    Debug.Assert(_detectRectangleRight.Text == ailment.StaticRect[2].ToString());
                    Debug.Assert(_detectRectangleBottom.Text == ailment.StaticRect[3].ToString());
                }
                else
                {
                    Debug.Assert(_detectRectangleLeft.Text == "");
                    Debug.Assert(_detectRectangleTop.Text == "");
                    Debug.Assert(_detectRectangleRight.Text == "");
                    Debug.Assert(_detectRectangleBottom.Text == "");
                }
            }
        }

        public void Run()
        {
            _testSelectingAilmentLoadsConfiguration();
        }
    }


    public class WindowAilmentsCollapseActionHandlerTests
    {
        private Grid _macroDelayGrid = new Grid();

        private Grid _checkDelayGrid = new Grid();

        private Grid _detectThresholdGrid = new Grid();

        private Grid _allCureKeyGrid = new Grid();

        private Grid _detectRectangleGrid = new Grid();

        private ListBox _ailmentsListBox = new ListBox();

        private List<ListBoxItem> _listBoxItems()
        {
            return [
                new ListBoxItem
                {
                    Tag = new ListBoxAilmentsDataTag
                    {
                        Ailment = new Ailment { StopBot = 123 }
                    }
                },
                new ListBoxItem
                {
                    Tag = new ListBoxAilmentsDataTag
                    {
                        Ailment = new Ailment { ArrowKeys = 123 }
                    }
                },
                new ListBoxItem
                {
                    Tag = new ListBoxAilmentsDataTag
                    {
                        Ailment = new Ailment { AllCure = 123 }
                    }
                },
                new ListBoxItem
                {
                    Tag = new ListBoxAilmentsDataTag()
                }
            ];
        }

        private List<dynamic> _expecteds()
        {
            return [
                new
                {
                    MacroDelayGrid = Visibility.Visible,
                    CheckDelayGrid = Visibility.Visible,
                    DetectThresholdGrid = Visibility.Visible,
                    AllCureKeyGrid = Visibility.Collapsed,
                    DetectRectangleGrid = Visibility.Visible
                },
                new
                {
                    MacroDelayGrid = Visibility.Visible,
                    CheckDelayGrid = Visibility.Visible,
                    DetectThresholdGrid = Visibility.Visible,
                    AllCureKeyGrid = Visibility.Collapsed,
                    DetectRectangleGrid = Visibility.Collapsed
                },
                new
                {
                    MacroDelayGrid = Visibility.Visible,
                    CheckDelayGrid = Visibility.Visible,
                    DetectThresholdGrid = Visibility.Visible,
                    AllCureKeyGrid = Visibility.Visible,
                    DetectRectangleGrid = Visibility.Collapsed
                },
                new
                {
                    MacroDelayGrid = Visibility.Visible,
                    CheckDelayGrid = Visibility.Visible,
                    DetectThresholdGrid = Visibility.Visible,
                    AllCureKeyGrid = Visibility.Visible,
                    DetectRectangleGrid = Visibility.Visible
                }
            ];
        }

        private AbstractWindowActionHandler _fixture()
        {
            _macroDelayGrid = new Grid();
            _checkDelayGrid = new Grid();
            _detectThresholdGrid = new Grid();
            _allCureKeyGrid = new Grid();
            _detectRectangleGrid = new Grid();
            _ailmentsListBox = new ListBox();
            return new WindowAilmentsCollapseActionHandlerFacade(
                _macroDelayGrid,
                _checkDelayGrid,
                _detectThresholdGrid,
                _allCureKeyGrid,
                _detectRectangleGrid,
                _ailmentsListBox
            );
        }

        /**
         * @brief Verifies that the visibility of configuration panels collapses or expands
         * based on the selected ailment's properties (StopBot, ArrowKeys, AllCure)
         * 
         * When users select different status ailments in the list box, certain configuration
         * panels should be shown or hidden depending on which special handling properties
         * the ailment supports:
         * 
         * - StopBot: When set, indicates the bot should stop when this ailment is detected
         * - ArrowKeys: When set, indicates arrow key inputs are required to cure the ailment
         * - AllCure: When set, indicates the all-cure key should be used to cleanse the ailment
         */
        private void _testAilmentsCollapse()
        {
            var listBoxItems = _listBoxItems();
            var expecteds = _expecteds();
            for (int i = 0; i < listBoxItems.Count; i++)
            {
                var listBoxItem = listBoxItems[i];
                var expected = expecteds[i];
                var handler = _fixture();
                _ailmentsListBox.Items.Add(listBoxItem);
                _ailmentsListBox.SelectedIndex = 0;
                Debug.Assert(_macroDelayGrid.Visibility == (Visibility)expected.MacroDelayGrid);
                Debug.Assert(_checkDelayGrid.Visibility == (Visibility)expected.CheckDelayGrid);
                Debug.Assert(_detectThresholdGrid.Visibility == (Visibility)expected.DetectThresholdGrid);
                Debug.Assert(_allCureKeyGrid.Visibility == (Visibility)expected.AllCureKeyGrid);
                Debug.Assert(_detectRectangleGrid.Visibility == (Visibility)expected.DetectRectangleGrid);
            }
        }

        public void Run()
        {
            _testAilmentsCollapse();
        }
    }


    public class WindowAilmentsSaveActionHandlerTest
    {
        private ListBox _ailmentsListBox = new ListBox();

        private MockSystemWindow _ailmentsWindow = new MockSystemWindow();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private AbstractWindowActionHandler _fixture()
        {
            _ailmentsListBox = new ListBox();
            _ailmentsWindow = new MockSystemWindow();
            _ailmentsWindow.GetWindowReturn.Add(new Window());
            var handler = new WindowAilmentsSaveActionHandlerFacade(
                _ailmentsListBox,
                _ailmentsWindow
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Verifies that when the ailments window is closed or hidden, the current
         * configuration is saved to the botting model and persisted to disk.
         * 
         * When users finish editing status ailment settings and close the configuration
         * window, the system must save all changes to the botting model and trigger a
         * configuration save to disk.
         */
        private void _testAilmentsSavesToSystem()
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
                _ailmentsListBox.Items.Add(new ListBoxItem());
                _ailmentsListBox.SelectedIndex = 0;
                _ailmentsWindow.VisibleReturn.Add(visible);
                handler.OnDependencyEvent(
                    _ailmentsWindow,
                    new DependencyPropertyChangedEventArgs()
                );
                if (!visible)
                {
                    Debug.Assert(_ailmentsListBox.SelectedIndex == -1);
                    Debug.Assert(injectTypes.Count == 2);
                    Debug.Assert(injectTypes[0] is SystemInjectType.ConfigurationUpdate);
                    Debug.Assert(injectTypes[1] is SystemInjectType.ConfigurationSave);
                    Debug.Assert(configurations[0] == _maplestoryBotConfiguration);
                    Debug.Assert(configurations[1] == _maplestoryBotConfiguration);
                }
                else
                {
                    Debug.Assert(_ailmentsListBox.SelectedIndex == 0);
                    Debug.Assert(injectTypes.Count == 0);
                }
            }
        }

        public void Run()
        {
            _testAilmentsSavesToSystem();
        }
    }


    public class WindowAilmentsSaveCheckboxActionHandlerTests
    {
        private ListBox _ailmentsListBox = new ListBox();

        private MockSystemWindow _ailmentsWindow = new MockSystemWindow();

        public AbstractWindowActionHandler _fixture()
        {
            _ailmentsListBox = new ListBox();
            _ailmentsWindow = new MockSystemWindow();
            _ailmentsWindow.GetWindowReturn.Add(new Window());
            return new WindowAilmentsSaveCheckboxActionHandlerFacade(
                _ailmentsListBox,
                _ailmentsWindow
            );
        }

        private ListBoxItem _createListBoxItem(bool isChecked, Ailment ailment)
        {
            return new ListBoxItem
            {
                Content = new StackPanel
                {
                    Children = {
                        new CheckBox { IsChecked = isChecked }
                    }
                },
                Tag = new ListBoxAilmentsDataTag
                {
                    Ailment = ailment
                }
            };
        }

        private List<Ailment> _ailments()
        {
            return [
                new Ailment{ Active = 123 },
                new Ailment{ Active = 234 },
                new Ailment{ Active = 345 },
                new Ailment{ Active = 456 },
                new Ailment{ Active = 567 },
            ];
        }

        private List<ListBoxItem> _listBoxItems(
            List<Ailment> ailments
        )
        {
            return [
                _createListBoxItem(true, ailments[0]),
                _createListBoxItem(false, ailments[1]),
                _createListBoxItem(true, ailments[2]),
                _createListBoxItem(false, ailments[3]),
                _createListBoxItem(false, ailments[4])
            ];
        }

        /**
         * @brief Verifies that when the ailments window is closed or hidden, the checkbox
         * states (enabled/disabled) are saved to the corresponding ailment's Active flag
         * 
         * When users toggle the checkboxes next to each status ailment in the configuration
         * window, these checkboxes indicate whether automatic response should be active
         * for that ailment (e.g., whether the bot should attempt to cure Seal when detected).
         * When the window is closed or hidden, the system must persist these checkbox states
         * to the ailment's Active property (1 for checked, 0 for unchecked). When the window
         * is simply shown (not closed), the checkbox states should not override the existing
         * configuration values.
         */
        private void _testAilmentsSavesCheckboxesOnClose()
        {
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture();
                var ailments = _ailments();
                var listBoxItems = _listBoxItems(ailments);
                foreach (var listBoxItem in listBoxItems)
                {
                    _ailmentsListBox.Items.Add(listBoxItem);
                }
                _ailmentsWindow.VisibleReturn.Add(visible);
                handler.OnDependencyEvent(
                    handler,
                    new DependencyPropertyChangedEventArgs()
                );
                if (!visible)
                {
                    Debug.Assert(ailments[0].Active == 1);
                    Debug.Assert(ailments[1].Active == 0);
                    Debug.Assert(ailments[2].Active == 1);
                    Debug.Assert(ailments[3].Active == 0);
                    Debug.Assert(ailments[4].Active == 0);
                }
                else
                {
                    Debug.Assert(ailments[0].Active == 123);
                    Debug.Assert(ailments[1].Active == 234);
                    Debug.Assert(ailments[2].Active == 345);
                    Debug.Assert(ailments[3].Active == 456);
                    Debug.Assert(ailments[4].Active == 567);
                }
            }
        }

        public void Run()
        {
            _testAilmentsSavesCheckboxesOnClose();
        }
    }


    public class WindowAilmentsMenuHandlersTestSuite
    {
        public void Run()
        {
            new WindowAilmentsLoadActionHandlerTests().Run();
            new WindowAilmentsLoadImageActionHandlerTests().Run();
            new WindowAilmentsAnimationActionHandlerTests().Run();
            new WindowAilmentsSaveConfigurationActionHandlerTests().Run();
            new WindowAilmentsLoadConfigurationActionHandlerTests().Run();
            new WindowAilmentsCollapseActionHandlerTests().Run();
            new WindowAilmentsSaveActionHandlerTest().Run();
            new WindowAilmentsSaveCheckboxActionHandlerTests().Run();
        }
    }
}
