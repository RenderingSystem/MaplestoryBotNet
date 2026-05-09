using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class ListBoxAilmentsDataTag
    {
        public Ailment Ailment = new Ailment();

        public List<System.Windows.Controls.Image> Images = [];
    }


    public class WindowAilmentsLoadModifier : AbstractWindowStateModifier
    {
        private ListBox _ailmentsListBox;

        private StackPanel _ailmentsStackPanelTemplate;

        public WindowAilmentsLoadModifier(
            ListBox ailmentsListBox, StackPanel ailmentsStackPanelTemplate
        )
        {
            _ailmentsListBox = ailmentsListBox;
            _ailmentsStackPanelTemplate = ailmentsStackPanelTemplate;
        }

        public override void Modify(object? value)
        {
            if (value is not MaplestoryBotConfiguration configuration)
            {
                return;
            }
            _ailmentsListBox.Items.Clear();
            foreach (var ailment in configuration.Ailments)
            {
                var listBoxItem = new ListBoxItem();
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var templateChildren = _ailmentsStackPanelTemplate.Children;
                var templateTextBlock = templateChildren.OfType<TextBlock>().First();
                var checkBox = new CheckBox { IsChecked = ailment.Value.Active != 0 };
                var tag = new ListBoxAilmentsDataTag
                {
                    Ailment = ailment.Value
                };
                var textBlock = new TextBlock
                {
                    Text = ailment.Key,
                    Margin = templateTextBlock.Margin,
                    Foreground = templateTextBlock.Foreground,
                    VerticalAlignment = templateTextBlock.VerticalAlignment
                };
                stackPanel.Children.Add(checkBox);
                stackPanel.Children.Add(textBlock);
                listBoxItem.Content = stackPanel;
                listBoxItem.Tag = tag;
                listBoxItem.Name = ailment.Key;
                _ailmentsListBox.Items.Add(listBoxItem);
            }
            _ailmentsListBox.SelectedIndex = 0;
        }
    }


    public class WindowAilmentsLoadActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _ailmentsWindow;

        private AbstractWindowStateModifier _ailmentsLoadModifier;

        private AbstractConfiguration? _maplestoryBotConfiguration;

        public WindowAilmentsLoadActionHandler(
            AbstractSystemWindow ailmentsWindow,
            AbstractWindowStateModifier ailmentsLoadModifier
        )
        {
            _ailmentsWindow = ailmentsWindow;
            _ailmentsLoadModifier = ailmentsLoadModifier;
            _maplestoryBotConfiguration = null;
            ((Window)_ailmentsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsLoadModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate
                && data is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                _maplestoryBotConfiguration = maplestoryBotConfiguration;
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (_ailmentsWindow.Visible())
            {
                _ailmentsLoadModifier.Modify(_maplestoryBotConfiguration);
            }
        }
    }


    public class WindowAilmentsLoadActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _ailmentsLoadActionHandler;

        public WindowAilmentsLoadActionHandlerFacade(
            ListBox ailmentsListBox,
            StackPanel ailmentsStackPanelTemplate,
            AbstractSystemWindow ailmentsWindow
        )
        {
            _ailmentsLoadActionHandler = new WindowAilmentsLoadActionHandler(
                ailmentsWindow,
                new WindowAilmentsLoadModifier(
                    ailmentsListBox,
                    ailmentsStackPanelTemplate
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsLoadActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _ailmentsLoadActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _ailmentsLoadActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowAilmentsLoadImagesModifier : AbstractWindowStateModifier
    {
        private ListBox _ailmentsListBox;

        private Grid _ailmentsImageGrid;

        public WindowAilmentsLoadImagesModifier(
            ListBox ailmentsListBox,
            Grid ailmentsImageGrid
        )
        {
            _ailmentsListBox = ailmentsListBox;
            _ailmentsImageGrid = ailmentsImageGrid;
        }

        private List<System.Windows.Controls.Image> _findControlImages(string ailmentName)
        {
            var gridChildren = _ailmentsImageGrid.Children;
            var gridImages = gridChildren.OfType<System.Windows.Controls.Image>();
            var gridList = gridImages.Where(i => i.Name.StartsWith(ailmentName));
            gridList.OrderBy(i => int.Parse(i.Name.Substring(ailmentName.Length)));
            return gridList.ToList();
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

        private List<System.Windows.Controls.Image> _createControlImages(
            string ailmentName, List<Image<Bgra32>> ailmentImages
        )
        {
            var controlImages = new List<System.Windows.Controls.Image>();
            for (int i = 0; i < ailmentImages.Count; i++)
            {
                controlImages.Add(
                    new System.Windows.Controls.Image
                    {
                        Name = ailmentName + i.ToString(),
                        Source = _toBitmapSource(ailmentImages[i]),
                        Stretch = Stretch.None,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Visibility = Visibility.Hidden
                    }
                );
                _ailmentsImageGrid.Children.Add(controlImages.Last());
            }
            return controlImages;
        }
        
        private void _assignControlImagesToDataTag(
            string ailmentName,
            List<System.Windows.Controls.Image> controlImages
        )
        {
            if (
                _ailmentsListBox.Items.OfType<ListBoxItem>().ToList() is List<ListBoxItem> listBoxItems &&
                listBoxItems.Find(i => i.Name == ailmentName) is ListBoxItem listBoxItem &&
                listBoxItem.Tag is ListBoxAilmentsDataTag listBoxItemDataTag
            )
            {
                listBoxItemDataTag.Images = controlImages;
            }
        }

        public override void Modify(object? value)
        {
            if (value is not ConfigurationImages configurationImages)
            {
                return;
            }
            foreach (var ailmentImage in configurationImages.AilmentImages)
            {
                var controlImages = _findControlImages(ailmentImage.Key);
                if (controlImages.Count == 0)
                {
                    controlImages = _createControlImages(ailmentImage.Key, ailmentImage.Value);
                };
                _assignControlImagesToDataTag(ailmentImage.Key, controlImages);
            }
        }
    }


    public class WindowAilmentsLoadImagesActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _ailmentsWindow;

        private AbstractWindowStateModifier _ailmentsLoadImagesModifier;

        private AbstractConfiguration? _configurationImages;

        public WindowAilmentsLoadImagesActionHandler(
            AbstractSystemWindow ailmentsWindow,
            AbstractWindowStateModifier ailmentsLoadModifier
        )
        {
            _ailmentsWindow = ailmentsWindow;
            _ailmentsLoadImagesModifier = ailmentsLoadModifier;
            _configurationImages = null;
            ((Window)_ailmentsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsLoadImagesModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is ConfigurationImages configurationImages
            )
            {
                _configurationImages = configurationImages;
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (_ailmentsWindow.Visible())
            {
                _ailmentsLoadImagesModifier.Modify(_configurationImages);
            }
        }
    }


    public class WindowAilmentsLoadImagesActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _ailmentsLoadActionHandler;

        public WindowAilmentsLoadImagesActionHandlerFacade(
            ListBox ailmentsListBox,
            Grid ailmentsImageGrid,
            AbstractSystemWindow ailmentsWindow
        )
        {
            _ailmentsLoadActionHandler = new WindowAilmentsLoadImagesActionHandler(
                ailmentsWindow,
                new WindowAilmentsLoadImagesModifier(
                    ailmentsListBox,
                    ailmentsImageGrid
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsLoadActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _ailmentsLoadActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _ailmentsLoadActionHandler.OnDependencyEvent(sender, e);
        }
    }



    public class WindowAilmentsAnimationModifier : AbstractWindowStateModifier
    {
        private int _imageIndex;

        private System.Windows.Controls.Image? _visibleImage;

        public WindowAilmentsAnimationModifier()
        {
            _imageIndex = 0;
            _visibleImage = null;
        }

        public override void Modify(object? value)
        {
            if (
                value is List<System.Windows.Controls.Image> imageFrames &&
                imageFrames.Count > 0
            )
            {
                _imageIndex = Math.Min(_imageIndex, imageFrames.Count) % imageFrames.Count;
                if (_visibleImage != null)
                {
                    _visibleImage.Visibility = Visibility.Hidden;
                }
                _visibleImage = imageFrames[_imageIndex];
                _visibleImage.Visibility = Visibility.Visible;
                _imageIndex++;
            }
            else
            {
                _imageIndex = 0;
            }
        }
    }


    public class WindowAilmentsAnimationActionHandler : AbstractWindowActionHandler
    {
        private ListBox _ailmentsListBox;

        private AbstractCompositionEventHandler _compositionEventHandler;

        private AbstractSystemWindow _ailmentsWindow;

        private AbstractWindowStateModifier _ailmentsAnimationModifier;

        private AbstractTimestamp _animationStopwatch;

        private double _animationSpeed;

        public WindowAilmentsAnimationActionHandler(
            ListBox ailmentsListBox,
            AbstractCompositionEventHandler compositionEventHandler,
            AbstractSystemWindow ailmentsWindow,
            AbstractTimestamp animationStopwatch,
            AbstractWindowStateModifier ailmentsAnimationModifier,
            double animationSpeed
        )
        {
            _ailmentsListBox = ailmentsListBox;
            _compositionEventHandler = compositionEventHandler;
            _ailmentsWindow = ailmentsWindow;
            _ailmentsAnimationModifier = ailmentsAnimationModifier;
            _animationStopwatch = animationStopwatch;
            _animationSpeed = animationSpeed;
            _ailmentsListBox.SelectionChanged += OnEvent;
            compositionEventHandler.EventHandler(OnEvent);
            ((Window)_ailmentsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsAnimationModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (sender == _ailmentsListBox)
            {
                _ailmentsAnimationModifier.Modify(null);
            }
            else if (
                _ailmentsListBox.SelectedItem is ListBoxItem listBoxItem &&
                listBoxItem.Tag is ListBoxAilmentsDataTag dataTag
            )
            {
                if (_animationStopwatch.GetTimestamp() >= _animationSpeed)
                {
                    _animationStopwatch.SetTimestamp();
                    _ailmentsAnimationModifier.Modify(dataTag.Images);
                }
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (_ailmentsWindow.Visible())
            {
                _compositionEventHandler.Start();
            }
            else
            {
                _compositionEventHandler.Stop();
            }
        }
    }


    public class WindowAilmentsAnimationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _animationActionHandler;

        public WindowAilmentsAnimationActionHandlerFacade(
            ListBox ailmentsListBox,
            AbstractSystemWindow ailmentsWindow,
            double animationSpeed
        )
        {
            var compositionEventHandler = new CompositionEventHandler();
            compositionEventHandler.EventHandler(OnEvent);
            _animationActionHandler = new WindowAilmentsAnimationActionHandler(
                ailmentsListBox,
                compositionEventHandler,
                ailmentsWindow,
                new StopwatchTimestamp(),
                new WindowAilmentsAnimationModifier(),
                animationSpeed
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _animationActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _animationActionHandler.OnEvent(sender, e);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _animationActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowAilmentsSaveConfigurationModifier : AbstractWindowStateModifier
    {
        private TextBox _macroDelayTextBox;

        private TextBox _checkDelayTextBox;

        private TextBox _detectThresholdTextBox;

        private TextBox _allCureKeyTextBox;

        private TextBox _detectRectangleLeft;

        private TextBox _detectRectangleTop;

        private TextBox _detectRectangleRight;

        private TextBox _detectRectangleBottom;

        public WindowAilmentsSaveConfigurationModifier(
            TextBox macroDelayTextBox,
            TextBox checkDelayTextBox,
            TextBox detectThresholdTextBox,
            TextBox allCureKeyTextBox,
            TextBox detectRectangleLeft,
            TextBox detectRectangleTop,
            TextBox detectRectangleRight,
            TextBox detectRectangleBottom
        )
        {
            _macroDelayTextBox = macroDelayTextBox;
            _checkDelayTextBox = checkDelayTextBox;
            _detectThresholdTextBox = detectThresholdTextBox;
            _allCureKeyTextBox = allCureKeyTextBox;
            _detectRectangleLeft = detectRectangleLeft;
            _detectRectangleTop = detectRectangleTop;
            _detectRectangleRight = detectRectangleRight;
            _detectRectangleBottom = detectRectangleBottom;
        }

        public override void Modify(object? value)
        {
            if (value == null)
            {
                return;
            }
            dynamic parameters = value;
            if (
                parameters.Configuration is not MaplestoryBotConfiguration configuration ||
                parameters.RemovedItem is not ListBoxItem listBoxItem
            )
            {
                return;
            }
            var dataTag = (ListBoxAilmentsDataTag)listBoxItem.Tag;
            dataTag.Ailment.ActiveDelay = int.TryParse(_macroDelayTextBox.Text, out int a) ? a : 0;
            dataTag.Ailment.CheckDelay = int.TryParse(_checkDelayTextBox.Text, out int c) ? c : 0;
            dataTag.Ailment.Threshold = int.TryParse(_detectThresholdTextBox.Text, out int th) ? th : 0;
            configuration.MacroKeySettings.AilmentsAllcureKey = _allCureKeyTextBox.Text;
            if (dataTag.Ailment.StaticRect != null)
            {
                dataTag.Ailment.StaticRect = [];
                dataTag.Ailment.StaticRect.Add(int.TryParse(_detectRectangleLeft.Text, out int l) ? l : 0);
                dataTag.Ailment.StaticRect.Add(int.TryParse(_detectRectangleTop.Text, out int t) ? t : 0);
                dataTag.Ailment.StaticRect.Add(int.TryParse(_detectRectangleRight.Text, out int r) ? r : 0);
                dataTag.Ailment.StaticRect.Add(int.TryParse(_detectRectangleBottom.Text, out int b) ? b : 0);
            }
        }
    }


    public class WindowAilmentsSaveConfigurationActionHandler : AbstractWindowActionHandler
    {
        private ListBox _ailmentsListBox;

        private AbstractWindowStateModifier _ailmentsConfigurationModifier;

        private AbstractConfiguration? _configuration;

        public WindowAilmentsSaveConfigurationActionHandler(
            ListBox ailmentsListBox,
            AbstractWindowStateModifier ailmentsConfigurationModifier
        )
        {
            _ailmentsListBox = ailmentsListBox;
            _ailmentsConfigurationModifier = ailmentsConfigurationModifier;
            _ailmentsListBox.SelectionChanged += OnEvent;
            _configuration = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsConfigurationModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs &&
                selectionArgs.RemovedItems.Count > 0
            )
            {
                _ailmentsConfigurationModifier.Modify(
                    new
                    {
                        Configuration = _configuration,
                        RemovedItem = selectionArgs.RemovedItems[0]
                    }
                );
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _configuration = configuration;
            }
        }
    }


    public class WindowAilmentsSaveConfigurationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _ailmentsConfigurationActionHandler;

        public WindowAilmentsSaveConfigurationActionHandlerFacade(
            TextBox macroDelayTextBox,
            TextBox checkDelayTextBox,
            TextBox detectThresholdTextBox,
            TextBox allCureKeyTextBox,
            TextBox detectRectangleLeft,
            TextBox detectRectangleTop,
            TextBox detectRectangleRight,
            TextBox detectRectangleBottom,
            ListBox ailmentsListBox
        )
        {
            _ailmentsConfigurationActionHandler = (
                new WindowAilmentsSaveConfigurationActionHandler(
                    ailmentsListBox,
                    new WindowAilmentsSaveConfigurationModifier(
                        macroDelayTextBox,
                        checkDelayTextBox,
                        detectThresholdTextBox,
                        allCureKeyTextBox,
                        detectRectangleLeft,
                        detectRectangleTop,
                        detectRectangleRight,
                        detectRectangleBottom
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsConfigurationActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _ailmentsConfigurationActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _ailmentsConfigurationActionHandler.Inject(dataType, data);
        }
    }


    public class WindowAilmentsLoadConfigurationModifier : AbstractWindowStateModifier
    {
        private TextBox _macroDelayTextBox;

        private TextBox _checkDelayTextBox;

        private TextBox _detectThresholdTextBox;

        private TextBox _allCureKeyTextBox;

        private TextBox _detectRectangleLeft;

        private TextBox _detectRectangleTop;

        private TextBox _detectRectangleRight;

        private TextBox _detectRectangleBottom;
        public WindowAilmentsLoadConfigurationModifier(
            TextBox macroDelayTextBox,
            TextBox checkDelayTextBox,
            TextBox detectThresholdTextBox,
            TextBox allCureKeyTextBox,
            TextBox detectRectangleLeft,
            TextBox detectRectangleTop,
            TextBox detectRectangleRight,
            TextBox detectRectangleBottom
        )
        {
            _macroDelayTextBox = macroDelayTextBox;
            _checkDelayTextBox = checkDelayTextBox;
            _detectThresholdTextBox = detectThresholdTextBox;
            _allCureKeyTextBox = allCureKeyTextBox;
            _detectRectangleLeft = detectRectangleLeft;
            _detectRectangleTop = detectRectangleTop;
            _detectRectangleRight = detectRectangleRight;
            _detectRectangleBottom = detectRectangleBottom;
        }

        public override void Modify(object? value)
        {
            if (value == null)
            {
                return;
            }
            dynamic parameters = value;
            if (
                parameters?.Configuration is not MaplestoryBotConfiguration configuration ||
                parameters?.AddedItem is not ListBoxItem listBoxItem
            )
            {
                return;
            }
            var dataTag = (ListBoxAilmentsDataTag)listBoxItem.Tag;
            var stackPanel = (StackPanel)listBoxItem.Content;
            _macroDelayTextBox.Text = dataTag.Ailment.ActiveDelay.ToString();
            _checkDelayTextBox.Text = dataTag.Ailment.CheckDelay.ToString();
            _detectThresholdTextBox.Text = dataTag.Ailment.Threshold.ToString();
            _allCureKeyTextBox.Text = configuration.MacroKeySettings.AilmentsAllcureKey;
            _detectRectangleLeft.Text = (
                dataTag.Ailment.StaticRect?.Count >= 1 ?
                dataTag.Ailment.StaticRect[0].ToString() : ""
            );
            _detectRectangleTop.Text = (
                dataTag.Ailment.StaticRect?.Count >= 2 ?
                dataTag.Ailment.StaticRect[1].ToString() : ""
            );
            _detectRectangleRight.Text = (
                dataTag.Ailment.StaticRect?.Count >= 3 ?
                dataTag.Ailment.StaticRect[2].ToString() : ""
            );
            _detectRectangleBottom.Text = (
                dataTag.Ailment.StaticRect?.Count >= 4 ?
                dataTag.Ailment.StaticRect[3].ToString() : ""
            );
        }
    }


    public class WindowAilmentsLoadConfigurationActionHandler : AbstractWindowActionHandler
    {
        private ListBox _ailmentsListBox;

        private AbstractWindowStateModifier _ailmentsLoadConfigurationModifier;

        private AbstractConfiguration? _configuration;

        public WindowAilmentsLoadConfigurationActionHandler(
            ListBox ailmentsListBox,
            AbstractWindowStateModifier ailmentsLoadConfigurationModifier
        )
        {
            _ailmentsListBox = ailmentsListBox;
            _ailmentsLoadConfigurationModifier = ailmentsLoadConfigurationModifier;
            _ailmentsListBox.SelectionChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsLoadConfigurationModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _configuration = configuration;
            }
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs &&
                selectionArgs.AddedItems.Count > 0
            )
            {
                _ailmentsLoadConfigurationModifier.Modify(
                    new
                    {
                        Configuration = _configuration,
                        AddedItem = selectionArgs.AddedItems[0]
                    }
                );
            }
        }
    }


    public class WindowAilmentsLoadConfigurationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _ailmentsLoadConfigurationActionHandler;

        public WindowAilmentsLoadConfigurationActionHandlerFacade(
            TextBox macroDelayTextBox,
            TextBox checkDelayTextBox,
            TextBox detectThresholdTextBox,
            TextBox allCureKeyTextBox,
            TextBox detectRectangleLeft,
            TextBox detectRectangleTop,
            TextBox detectRectangleRight,
            TextBox detectRectangleBottom,
            ListBox ailmentsListBox
        )
        {
            _ailmentsLoadConfigurationActionHandler = (
                new WindowAilmentsLoadConfigurationActionHandler(
                    ailmentsListBox,
                    new WindowAilmentsLoadConfigurationModifier(
                        macroDelayTextBox,
                        checkDelayTextBox,
                        detectThresholdTextBox,
                        allCureKeyTextBox,
                        detectRectangleLeft,
                        detectRectangleTop,
                        detectRectangleRight,
                        detectRectangleBottom
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsLoadConfigurationActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _ailmentsLoadConfigurationActionHandler.Inject(dataType, data);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _ailmentsLoadConfigurationActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowAilmentsCollapseModifier : AbstractWindowStateModifier
    {
        private Grid _macroDelayGrid;

        private Grid _checkDelayGrid;

        private Grid _detectThresholdGrid;

        private Grid _allCureKeyGrid;

        private Grid _detectRectangleGrid;

        public WindowAilmentsCollapseModifier(
            Grid macroDelayGrid,
            Grid checkDelayGrid,
            Grid detectThresholdGrid,
            Grid allCureKeyGrid,
            Grid detectRectangleGrid
            
        )
        {
            _macroDelayGrid = macroDelayGrid;
            _checkDelayGrid = checkDelayGrid;
            _detectThresholdGrid = detectThresholdGrid;
            _allCureKeyGrid = allCureKeyGrid;
            _detectRectangleGrid = detectRectangleGrid;
        }

        public override void Modify(object? value)
        {
            if (value is not ListBoxItem listBoxItem)
            {
                return;
            }
            var dataTag = (ListBoxAilmentsDataTag)listBoxItem.Tag;
            var ailment = dataTag.Ailment;
            _macroDelayGrid.Visibility = Visibility.Visible;
            _checkDelayGrid.Visibility = Visibility.Visible;
            _detectThresholdGrid.Visibility = Visibility.Visible;
            if (ailment.StopBot != null && ailment.StopBot != 0)
            {
                _allCureKeyGrid.Visibility = Visibility.Collapsed;
                _detectRectangleGrid.Visibility = Visibility.Visible;
            }
            else if (ailment.ArrowKeys != null && ailment.ArrowKeys != 0)
            {
                _allCureKeyGrid.Visibility = Visibility.Collapsed;
                _detectRectangleGrid.Visibility = Visibility.Collapsed;
            }
            else if (ailment.AllCure != null && ailment.AllCure != 0)
            {
                _allCureKeyGrid.Visibility = Visibility.Visible;
                _detectRectangleGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                _allCureKeyGrid.Visibility = Visibility.Visible;
                _detectRectangleGrid.Visibility = Visibility.Visible;
            }
        }
    }


    public class WindowAilmentsCollapseActionHandler : AbstractWindowActionHandler
    {
        private ListBox _ailmentsListBox;

        private AbstractWindowStateModifier _ailmentsCollapseModifier;

        public WindowAilmentsCollapseActionHandler(
            ListBox ailmentsListBox,
            AbstractWindowStateModifier ailmentsCollapseModifier
        )
        {
            _ailmentsListBox = ailmentsListBox;
            _ailmentsCollapseModifier = ailmentsCollapseModifier;
            _ailmentsListBox.SelectionChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsCollapseModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs &&
                selectionArgs.AddedItems.Count > 0
            )
            {
                _ailmentsCollapseModifier.Modify(selectionArgs.AddedItems[0]);
            }
        }
    }


    public class WindowAilmentsCollapseActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _ailmentsCollapseActionHandler;

        public WindowAilmentsCollapseActionHandlerFacade(
            Grid macroDelayGrid,
            Grid checkDelayGrid,
            Grid detectThresholdGrid,
            Grid allCureKeyGrid,
            Grid detectRectangleGrid,
            ListBox ailmentsListBox
        )
        {
            _ailmentsCollapseActionHandler = new WindowAilmentsCollapseActionHandler(
                ailmentsListBox,
                new WindowAilmentsCollapseModifier(
                    macroDelayGrid,
                    checkDelayGrid,
                    detectThresholdGrid,
                    allCureKeyGrid,
                    detectRectangleGrid
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsCollapseActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _ailmentsCollapseActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowAilmentsSaveModifier : AbstractWindowStateModifier
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


    public class WindowAilmentsSaveActionHandler : AbstractWindowActionHandler
    {
        private ListBox _ailmentsListBox;

        private AbstractSystemWindow _ailmentsWindow;

        private AbstractWindowStateModifier _ailmentsSaveModifier;

        private AbstractInjectAction? _injectAction;

        private AbstractConfiguration? _maplestoryBotConfiguration;

        public WindowAilmentsSaveActionHandler(
            ListBox ailmentsListBox,
            AbstractSystemWindow ailmentsWindow,
            AbstractWindowStateModifier ailmentsSaveModifier
        )
        {
            _ailmentsListBox = ailmentsListBox;
            _ailmentsWindow = ailmentsWindow;
            _ailmentsSaveModifier = ailmentsSaveModifier;
            _injectAction = null;
            _maplestoryBotConfiguration = null;
            ((Window)_ailmentsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }


        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsSaveModifier;
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
            if (!_ailmentsWindow.Visible())
            {
                _ailmentsListBox.SelectedIndex = -1;
                _ailmentsSaveModifier.Modify(
                    new
                    {
                        Configuration = _maplestoryBotConfiguration,
                        InjectAction = _injectAction
                    }
                );
            }
        }
    }


    public class WindowAilmentsSaveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _ailmentsSaveActionHandler;

        public WindowAilmentsSaveActionHandlerFacade(
            ListBox ailmentsListBox,
            AbstractSystemWindow ailmentsWindow
        )
        {
            _ailmentsSaveActionHandler = new WindowAilmentsSaveActionHandler(
                ailmentsListBox,
                ailmentsWindow,
                new WindowAilmentsSaveModifier()
            );
        }
        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsSaveActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _ailmentsSaveActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _ailmentsSaveActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowAilmentsSaveCheckboxModifier : AbstractWindowStateModifier
    {
        private ListBox _ailmentsListBox;

        public WindowAilmentsSaveCheckboxModifier(ListBox ailmentsListBox)
        {
            _ailmentsListBox = ailmentsListBox;
        }

        public override void Modify(object? value)
        {
            foreach (ListBoxItem listBoxItem in _ailmentsListBox.Items)
            {
                var stackPanel = (StackPanel)listBoxItem.Content;
                var tag = (ListBoxAilmentsDataTag)listBoxItem.Tag;
                var checkBox = stackPanel.Children.OfType<CheckBox>().First()!;
                tag.Ailment.Active = (bool)checkBox.IsChecked! ? 1 : 0;
            }
        }
    }


    public class WindowAilmentsSaveCheckboxActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _ailmentsWindow;

        private AbstractWindowStateModifier _ailmentsSaveCheckboxModifier;

        public WindowAilmentsSaveCheckboxActionHandler(
            AbstractSystemWindow ailmentsWindow,
            AbstractWindowStateModifier ailmentsSaveCheckboxModifier
        )
        {
            _ailmentsWindow = ailmentsWindow;
            _ailmentsSaveCheckboxModifier = ailmentsSaveCheckboxModifier;
            ((Window)_ailmentsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsSaveCheckboxModifier;
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (!_ailmentsWindow.Visible())
            {
                _ailmentsSaveCheckboxModifier.Modify(null);
            }
        }
    }


    public class WindowAilmentsSaveCheckboxActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _ailmentsSaveCheckboxActionHandler;

        public WindowAilmentsSaveCheckboxActionHandlerFacade(
            ListBox ailmentsListBox,
            AbstractSystemWindow ailmentsWindow
        )
        {
            _ailmentsSaveCheckboxActionHandler = new WindowAilmentsSaveCheckboxActionHandler(
                ailmentsWindow,
                new WindowAilmentsSaveCheckboxModifier(ailmentsListBox)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _ailmentsSaveCheckboxActionHandler.Modifier();
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _ailmentsSaveCheckboxActionHandler.OnDependencyEvent(sender, e);
        }
    }
}
