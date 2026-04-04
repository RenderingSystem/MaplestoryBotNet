using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;



namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public class WindowMapCanvasDrawerParameters
    {
        public Point ElementPoint = new Point();

        public List<FrameworkElement> ElementDependencies = [];

        public AbstractBottingModel ElementModel = new BottingModel();
    }


    public abstract class AbstractMapCanvasFormatter
    {
        public abstract void Format(
            FrameworkElement canvas,
            List<FrameworkElement> textDepdendencies,
            object formatData
        );
    }


    public abstract class AbstractPointElementInformation
    {
        public abstract Rect BoundingRect(FrameworkElement frameworkElement);

        public abstract TextBlock? Label(FrameworkElement frameworkElement);
    }


    public abstract class AbstractMapCanvasElementFactory
    {
        public abstract FrameworkElement Create();
    }


    public abstract class AbstractMapCanvasElementLocator
    {
        public abstract FrameworkElement? Locate(AbstractMacroModel macroModel, Point point);
    }


    public class WindowMapCanvasCircleFactory : AbstractMapCanvasElementFactory
    {
        private Brush _fill;

        private Brush _stroke;

        private int _strokeThickness;

        private double _radius;
        public WindowMapCanvasCircleFactory(
            Brush fill,
            Brush stroke,
            int strokeThickness,
            double radius
        )
        {
            _fill = fill;
            _stroke = stroke;
            _strokeThickness = strokeThickness;
            _radius = radius;
        }

        public override FrameworkElement Create()
        {
            return new Ellipse
            {
                Fill = _fill,
                Stroke = _stroke,
                StrokeThickness = _strokeThickness,
                Width = _radius * 2,
                Height = _radius * 2,
                RenderTransform = new TranslateTransform { X = -_radius, Y = -_radius },
            };
        }
    }


    public class WindowMapCanvasLabelFactory : AbstractMapCanvasElementFactory
    {
        private string _text;

        private string _fontFamily;

        private double _fontSize;

        private double _offsetX;

        private double _offsetY;

        private Brush _foreground;

        private Brush _background;

        public WindowMapCanvasLabelFactory(
            string text,
            string fontFamily,
            double fontSize,
            double offsetX,
            double offsetY,
            Brush foreground,
            Brush background
        )
        {
            _text = text;
            _fontFamily = fontFamily;
            _fontSize = fontSize;
            _offsetX = offsetX;
            _offsetY = offsetY;
            _foreground = foreground;
            _background = background;
        }

        public override FrameworkElement Create()
        {
            return new TextBlock
            {
                Text = _text,
                FontFamily = new FontFamily(_fontFamily),
                FontSize = _fontSize,
                Foreground = _foreground,
                Background = _background,
                RenderTransform = new TranslateTransform { X = _offsetX, Y = _offsetY }
            };
        }
    }


    public class WindowMapCanvasPointFactory : AbstractMapCanvasElementFactory
    {
        private AbstractMapCanvasElementFactory _elementFactory;

        private AbstractMapCanvasElementFactory _labelFactory;

        public WindowMapCanvasPointFactory(
            AbstractMapCanvasElementFactory elementFactory,
            AbstractMapCanvasElementFactory labelFactory
        )
        {
            _elementFactory = elementFactory;
            _labelFactory = labelFactory;
        }

        public override FrameworkElement Create()
        {
            var element = _elementFactory.Create();
            var label = _labelFactory.Create();
            var container = new Canvas();
            container.Children.Add(element);
            container.Children.Add(label);
            return container;
        }
    }


    public class WindowMapCanvasPointFactoryFacade : AbstractMapCanvasElementFactory
    {
        private AbstractMapCanvasElementFactory _mapCanvasPointFactory;

        public WindowMapCanvasPointFactoryFacade()
        {
            _mapCanvasPointFactory = new WindowMapCanvasPointFactory(
                new WindowMapCanvasCircleFactory(
                    Brushes.Aqua,
                    Brushes.LightBlue,
                    1,
                    5
                ),
                new WindowMapCanvasLabelFactory(
                    "Lorem Ipsum",
                    "Courier New",
                    10.0,
                    0.0,
                    -16.0,
                    Brushes.White,
                    Brushes.Transparent
                )
            );
        }

        public override FrameworkElement Create()
        {
            return _mapCanvasPointFactory.Create();
        }
    }


    public class WindowMapCanvasRectangleFactory : AbstractMapCanvasElementFactory
    {
        private Brush _fill;

        private Brush _stroke;

        private int _strokeThickness;

        private int _width;

        private int _height;

        private double _opacity;

        public WindowMapCanvasRectangleFactory(
            Brush fill,
            Brush stroke,
            int strokeThickness,
            int width,
            int height,
            double opacity
        )
        {
            _fill = fill;
            _stroke = stroke;
            _strokeThickness = strokeThickness;
            _width = width;
            _height = height;
            _opacity = opacity;
        }

        public override FrameworkElement Create()
        {
            return new Rectangle
            {
                Fill = _fill,
                Stroke = _stroke,
                StrokeThickness = _strokeThickness,
                Width = _width,
                Height = _height,
                Opacity = _opacity
            };
        }
    }


    public class WindowMapCanvasFrameTypes
    {
        public const string CANVAS = "FrameCanvas";

        public const string TL = "FrameTL";

        public const string TR = "FrameTR";

        public const string BL = "FrameBL";

        public const string BR = "FrameBR";

        public static List<string> GripNames()
        {
            return [TL, TR, BL, BR];
        }

        public static List<string> OppositeNames()
        {
            return [BR, BL, TR, TL];
        }
    }


    public class WindowMapCanvasFrameFactory : AbstractMapCanvasElementFactory
    {
        AbstractMapCanvasElementFactory _rectangleFactory;

        AbstractMapCanvasElementFactory _gripFactory;

        AbstractMapCanvasElementFactory _labelFactory;

        public WindowMapCanvasFrameFactory(
            AbstractMapCanvasElementFactory rectangleFactory,
            AbstractMapCanvasElementFactory gripFactory,
            AbstractMapCanvasElementFactory labelFactory
        )
        {
            _rectangleFactory = rectangleFactory;
            _gripFactory = gripFactory;
            _labelFactory = labelFactory;
        }

        public override FrameworkElement Create()
        {
            var rectangle = _rectangleFactory.Create();
            var gripTL = _gripFactory.Create();
            var gripTR = _gripFactory.Create();
            var gripBL = _gripFactory.Create();
            var gripBR = _gripFactory.Create();
            var label = _labelFactory.Create();
            var container = new Canvas { Name = WindowMapCanvasFrameTypes.CANVAS };
            gripTL.Name = WindowMapCanvasFrameTypes.TL;
            gripTR.Name = WindowMapCanvasFrameTypes.TR;
            gripBL.Name = WindowMapCanvasFrameTypes.BL;
            gripBR.Name = WindowMapCanvasFrameTypes.BR;
            container.Children.Add(rectangle);
            container.Children.Add(gripTL);
            container.Children.Add(gripTR);
            container.Children.Add(gripBL);
            container.Children.Add(gripBR);
            container.Children.Add(label);
            label.Visibility = Visibility.Hidden;
            return container;
        }
    }


    public class WindowMapCanvasFrameFactoryFacade : AbstractMapCanvasElementFactory
    {
        AbstractMapCanvasElementFactory _mapCanvasFrameFactory;

        public WindowMapCanvasFrameFactoryFacade()
        {
            _mapCanvasFrameFactory = new WindowMapCanvasFrameFactory(
                new WindowMapCanvasRectangleFactory(
                    new SolidColorBrush(Color.FromArgb(40, 0, 0, 255)),
                    Brushes.GhostWhite,
                    1,
                    0,
                    0,
                    1.0
                ),
                new WindowMapCanvasCircleFactory(
                    Brushes.GhostWhite,
                    Brushes.GhostWhite,
                    1,
                    4.0
                ),
                new WindowMapCanvasLabelFactory(
                    "Lorem Ipsum",
                    "Courier New",
                    10.0,
                    0.0,
                    -16.0,
                    Brushes.GhostWhite,
                    Brushes.Transparent
                )
            );
        }

        public override FrameworkElement Create()
        {
            return _mapCanvasFrameFactory.Create();
        }
    }
}
