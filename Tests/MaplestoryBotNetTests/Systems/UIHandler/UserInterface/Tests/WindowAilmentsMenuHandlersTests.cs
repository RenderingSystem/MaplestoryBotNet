using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
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

        private MockTimedDispatch _timedDispatch = new MockTimedDispatch();

        private MockSystemWindow _ailmentsWindow = new MockSystemWindow();

        private List<List<System.Windows.Controls.Image>> _imageLists = [];

        private AbstractWindowActionHandler _fixture()
        {
            _ailmentsListBox = new ListBox();
            _timedDispatch = new MockTimedDispatch();
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
            return new WindowAilmentsAnimationActionHandlerFacade(
                _ailmentsListBox,
                _timedDispatch,
                _ailmentsWindow
            );
        }

        /**
         * @brief Verifies that when the timed dispatch event fires, exactly one animation
         * frame image becomes visible for the currently selected ailment, cycling through
         * frames in sequence
         * 
         * When the animation timer ticks, the system must display the next animation frame
         * for the selected status ailment. Only one frame should be visible at a time
         * (cycling through each reference image in order), while all other frames for
         * that ailment remain hidden. This creates the appearance of a looping animation
         * that visualizes the status ailment effect. The test cycles through each image
         * index, selecting an ailment and firing the dispatch event multiple times to
         * verify the frame alternates correctly.
         */
        private void _testDispatchEventModifiesVisibility()
        {
            var handler = _fixture();
            for (int i = 0; i < _imageLists.Count; i++)
            for (int j = 0; j < _imageLists[i].Count + 1; j++)
            {
                _ailmentsListBox.SelectedIndex = i;
                handler.OnEvent(null, new EventArgs());
                var visibleList = _imageLists[i].FindAll(i => i.Visibility == Visibility.Visible).ToList();
                Debug.Assert(visibleList.Count == 1);
                Debug.Assert(_imageLists[i].IndexOf(
                    visibleList[0]) == (j % _imageLists[i].Count)
                );
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
                Debug.Assert(_timedDispatch.StartCalls == (visible ? 1 : 0));
                Debug.Assert(_timedDispatch.StopCalls == (visible ? 0 : 1));
            }
        }

        public void Run()
        {
            _testDispatchEventModifiesVisibility();
            _testVisibilityEventTogglesDispatch();
        }
    }


    public class WindowAilmentsMenuHandlersTestSuite
    {
        public void Run()
        {
            new WindowAilmentsLoadActionHandlerTests().Run();
            new WindowAilmentsLoadImageActionHandlerTests().Run();
            new WindowAilmentsAnimationActionHandlerTests().Run();
        }
    }
}
