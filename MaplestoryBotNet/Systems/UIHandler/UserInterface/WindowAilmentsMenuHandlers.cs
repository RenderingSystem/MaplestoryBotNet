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
                    Ailment = (Ailment)ailment.Value.Copy()
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

        private AbstractTimedDispatch _timedDispatch;

        private AbstractSystemWindow _ailmentsWindow;

        private AbstractWindowStateModifier _ailmentsAnimationModifier;

        public WindowAilmentsAnimationActionHandler(
            ListBox ailmentsListBox,
            AbstractTimedDispatch timedDispatch,
            AbstractSystemWindow ailmentsWindow,
            AbstractWindowStateModifier ailmentsAnimationModifier
        )
        {
            _ailmentsListBox = ailmentsListBox;
            _ailmentsListBox.SelectionChanged += OnEvent;
            _timedDispatch = timedDispatch;
            _timedDispatch.Tick(OnEvent);
            _ailmentsWindow = ailmentsWindow;
            ((Window)_ailmentsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
            _ailmentsAnimationModifier = ailmentsAnimationModifier;
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
                _ailmentsAnimationModifier.Modify(dataTag.Images);
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (_ailmentsWindow.Visible())
            {
                _timedDispatch.Start();
            }
            else
            {
                _timedDispatch.Stop();
            }
        }
    }


    public class WindowAilmentsAnimationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _animationActionHandler;

        public WindowAilmentsAnimationActionHandlerFacade(
            ListBox ailmentsListBox,
            AbstractTimedDispatch timedDispatch,
            AbstractSystemWindow ailmentsWindow
        )
        {
            _animationActionHandler = new WindowAilmentsAnimationActionHandler(
                ailmentsListBox,
                timedDispatch,
                ailmentsWindow,
                new WindowAilmentsAnimationModifier()
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

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            _animationActionHandler.OnDependencyEvent(sender, e);
        }
    }
}
