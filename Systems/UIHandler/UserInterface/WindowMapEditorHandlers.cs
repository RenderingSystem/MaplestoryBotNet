using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Vortice.Direct3D11;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public enum WindowMapEditMenuStateTypes
    {
        Select = 0,
        Add,
        Remove,
        WindowMapEditMenuStateTypesCount
    }


    public abstract class AbstractWindowMapEditMenuState
    {
        public abstract WindowMapEditMenuStateTypes GetState();

        public abstract void SetState(WindowMapEditMenuStateTypes state);
    }


    public abstract class AbstractMapCanvasElementFactory
    {
        public abstract UIElement Create();
    }


    public class WindowMapEditMenuState : AbstractWindowMapEditMenuState
    {
        public WindowMapEditMenuStateTypes _currentState = WindowMapEditMenuStateTypes.Select;

        public override WindowMapEditMenuStateTypes GetState()
        {
            return _currentState;
        }

        public override void SetState(WindowMapEditMenuStateTypes state)
        {
            _currentState = state;
        }
    }


    public class WindowMapEditMenuModifier : AbstractWindowStateModifier
    {
        private AbstractSystemWindow _mapWindow;

        private AbstractSystemWindow _editWindow;

        public WindowMapEditMenuModifier(
            AbstractSystemWindow mapWindow,
            AbstractSystemWindow editWindow
        )
        {
            _mapWindow = mapWindow;
            _editWindow = editWindow;
        }

        public override void Modify(object? value)
        {
            if (value is bool show)
            {
                if (show)
                {
                    _editWindow.ShowDialog();
                    _editWindow.AttachOwner(_mapWindow);
                }
                else
                {
                    _editWindow.Hide();
                }
            }
        }
    }


    public class WindowMapEditMenuActionHandler : AbstractWindowActionHandler
    {
        private Button _editButton;

        private AbstractWindowStateModifier _editWindowModifier;

        public WindowMapEditMenuActionHandler(
            Button editButton,
            AbstractWindowStateModifier editWindowModifier
        )
        {
            _editButton = editButton;
            _editWindowModifier = editWindowModifier;
            _editButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _editWindowModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _editWindowModifier.Modify(true);
        }
    }


    public class WindowMapEditMenuActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _editMenuActionHandler;

        public WindowMapEditMenuActionHandlerFacade(
            Button editButton,
            AbstractSystemWindow mapWindow,
            AbstractSystemWindow editWindow
        )
        {
            _editMenuActionHandler = new WindowMapEditMenuActionHandler(
                editButton, new WindowMapEditMenuModifier(mapWindow, editWindow)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _editMenuActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _editMenuActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapCanvasDrawerParameters
    {
        public Point ElementPoint = new Point();

        public string ElementText = "";

        public MapModel ElementModel = new MapModel();
    }


    public class WindowMapCanvasPointDrawer : AbstractWindowStateModifier
    {
        private Canvas _canvas;

        private AbstractMapCanvasElementFactory _pointFactory;

        private UIElement? _createdPoint;

        public WindowMapCanvasPointDrawer(
            Canvas canvas,
            AbstractMapCanvasElementFactory pointFactory
        )
        {
            _canvas = canvas;
            _pointFactory = pointFactory;
            _createdPoint = null;
        }

        private void _setupLabelText(MapModel mapModel, string pointLabel)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(_createdPoint); i++)
            {
                var child = VisualTreeHelper.GetChild(_createdPoint, i);
                if (child is TextBlock textBlock)
                {
                    textBlock.Text = pointLabel;
                    return;
                }
            }
        }

        private static Rect _getBoundingRect(Canvas canvasElement)
        {
            var boundingRect = Rect.Empty;
            foreach (UIElement child in canvasElement.Children)
            {
                if (child is not TextBlock)
                {
                    child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    var width = child.DesiredSize.Width;
                    var height = child.DesiredSize.Height;
                    boundingRect.Union(new Rect(0, 0, width, height));
                }
            }
            return boundingRect;
        }

        private MinimapPointData _minimapPointData(
            Canvas canvasElement,
            MapModel mapModel,
            string pointName,
            string elementName
        )
        {
            canvasElement.Name = elementName;
            return new MinimapPointData
            {
                PointName = pointName,
                ElementName = elementName,
                Commands = []
            };
        }

        private void _setupMinimapPoint(
            Point point,
            MapModel mapModel,
            string pointLabel,
            string elementName
        )
        {
            if (_createdPoint is Canvas canvasElement)
            {
                var boundingRect = _getBoundingRect(canvasElement);
                var pointData = _minimapPointData(
                    canvasElement, mapModel, pointLabel, elementName
                );
                var minimapPoint = new MinimapPoint
                {
                    X = Canvas.GetLeft(canvasElement),
                    Y = Canvas.GetTop(canvasElement),
                    XRange = boundingRect.Width,
                    YRange = boundingRect.Height,
                    PointData = pointData
                };
                mapModel.Add(minimapPoint);
            }
        }

        private string _getElementName(MapModel mapModel)
        {
            var mapPoints = mapModel.Points();
            var elementCount = mapPoints.Count;
            var existingElements = new HashSet<string>(
                mapPoints.Select(p => p.PointData.ElementName)
            );
            while (existingElements.Contains("T" + elementCount))
            {
                elementCount++;
            }
            return "T" + elementCount;
        }

        private string _getPointLabel(MapModel mapModel)
        {
            var mapPoints = mapModel.Points();
            var pointCount = mapPoints.Count;
            var existingNames = new HashSet<string>(
                mapPoints.Select(p => p.PointData.PointName)
            );
            while (existingNames.Contains("P" + pointCount))
            {
                pointCount++;
            }
            return "P" + pointCount;
        }

        public override void Modify(object? value)
        {
            if (value is WindowMapCanvasDrawerParameters parameters)
            {
                var pointLabel = _getPointLabel(parameters.ElementModel);
                var elementName = _getElementName(parameters.ElementModel);
                _createdPoint = _pointFactory.Create();
                _canvas.Children.Add(_createdPoint);
                Canvas.SetLeft(_createdPoint, parameters.ElementPoint.X);
                Canvas.SetTop(_createdPoint, parameters.ElementPoint.Y);
                _setupMinimapPoint(
                    parameters.ElementPoint,
                    parameters.ElementModel,
                    pointLabel,
                    elementName
                );
                _setupLabelText(
                    parameters.ElementModel,
                    pointLabel
                );
            }
        }

        public override object? State(int stateType)
        {
            return _createdPoint;
        }
    }


    public class WindowMapCanvasPointEraser : AbstractWindowStateModifier
    {
        private Canvas _canvas;

        public WindowMapCanvasPointEraser(Canvas canvas)
        {
            _canvas = canvas;
        }

        public override void Modify(object? value)
        {
            if (
                value is UIElement element
                && _canvas.Children.Contains(element)
            )
            {
                _canvas.Children.Remove(element);
            }
        }
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

        public override UIElement Create()
        {
            return new Ellipse
            {
                Fill = _fill,
                Stroke = _stroke,
                StrokeThickness = _strokeThickness,
                Width = _radius * 2,
                Height = _radius * 2,
                RenderTransform = new TranslateTransform { X=-_radius, Y=-_radius },
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

        public override UIElement Create()
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

        public override UIElement Create()
        {
            var element = _elementFactory.Create();
            var label = _labelFactory.Create();
            var container = new Canvas();
            container.Children.Add(element);
            container.Children.Add(label);
            return container;
        }
    }


    public class MapCanvasPointDrawingActionHandler : AbstractWindowActionHandler
    {
        private Canvas _canvas;

        private WindowMapEditMenuState _menuState;

        private AbstractWindowStateModifier _pointDrawer;

        private AbstractMouseEventPositionExtractor _mousePositionExtractor;

        private MapModel? _mapModel;

        public MapCanvasPointDrawingActionHandler(
            Canvas canvas,
            WindowMapEditMenuState menuState,
            AbstractMouseEventPositionExtractor mousePositionExtractor,
            AbstractWindowStateModifier pointDrawer
        )
        {
            _canvas = canvas;
            _menuState = menuState;
            _mousePositionExtractor = mousePositionExtractor;
            _pointDrawer = pointDrawer;
            _canvas.MouseLeftButtonDown += OnEvent;
            _mapModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _pointDrawer;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                _menuState.GetState() == WindowMapEditMenuStateTypes.Add
                && _mapModel != null
            )
            {
                var mousePosition = _mousePositionExtractor.GetPosition(
                    (MouseButtonEventArgs)e, _canvas
                );
                _pointDrawer.Modify(
                    new WindowMapCanvasDrawerParameters
                    {
                        ElementPoint = mousePosition,
                        ElementText = "",
                        ElementModel = _mapModel,
                    }
                );
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (
                dataType == SystemInjectType.MapModel
                && data is MapModel mapModel
            )
            {
                _mapModel = mapModel;
            }
        }
    }


    public class MapCanvasCirclePointDrawingActionHandler : AbstractWindowActionHandler
    {
        AbstractWindowActionHandler _mapCanvasPointDrawingActionHandler;

        public MapCanvasCirclePointDrawingActionHandler(
            Canvas canvas,
            WindowMapEditMenuState menuState,
            AbstractMouseEventPositionExtractor mouseEventPositionExtractor
        )
        {
            _mapCanvasPointDrawingActionHandler = new MapCanvasPointDrawingActionHandler(
                canvas,
                menuState,
                mouseEventPositionExtractor,
                new WindowMapCanvasPointDrawer(
                    canvas,
                    new WindowMapCanvasPointFactory(
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
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasPointDrawingActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasPointDrawingActionHandler.OnEvent(sender, e);
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _mapCanvasPointDrawingActionHandler.Inject(dataType, data);
        }
    }


    public class MapCanvasAddPointButtonModifier : AbstractWindowStateModifier
    {
        private WindowMapEditMenuState _menuState;

        private ToggleButton _addButton;

        private List<ToggleButton> _radioButtons;

        public MapCanvasAddPointButtonModifier(
            WindowMapEditMenuState menuState,
            ToggleButton addButton,
            List<ToggleButton> radioButtons

        )
        {
            _menuState = menuState;
            _addButton = addButton;
            _radioButtons = radioButtons;
        }

        public override void Modify(object? value)
        {
            if (value is bool checkedState)
            {
                for (int i = 0; i < _radioButtons.Count; i++)
                {
                    _radioButtons[i].IsChecked = false;
                }
                _addButton.IsChecked = checkedState;
                var nextState = checkedState ?
                    WindowMapEditMenuStateTypes.Add :
                    WindowMapEditMenuStateTypes.Select;
                _menuState.SetState(nextState);
            }
        }
    }


    public class MapCanvasAddPointButtonActionHandler : AbstractWindowActionHandler
    {
        private ToggleButton _addPointButton;

        private AbstractWindowStateModifier _addStateModifier;

        public MapCanvasAddPointButtonActionHandler(
            ToggleButton addPointButton,
            AbstractWindowStateModifier addStateModifier
        )
        {
            _addPointButton = addPointButton;
            _addPointButton.Click += OnEvent;
            _addStateModifier = addStateModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _addStateModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _addStateModifier.Modify(_addPointButton.IsChecked);
        }
    }


    public class MapCanvasAddPointButtonActionHandlerFacade : AbstractWindowActionHandler
    {
        private MapCanvasAddPointButtonActionHandler _mapCanvasAddPointButtonActionHandler;

        public MapCanvasAddPointButtonActionHandlerFacade(
            ToggleButton addPointButton,
            List<ToggleButton> radioButtons,
            WindowMapEditMenuState menuState
        )
        {
            _mapCanvasAddPointButtonActionHandler = new MapCanvasAddPointButtonActionHandler(
                addPointButton,
                new MapCanvasAddPointButtonModifier(menuState, addPointButton, radioButtons)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasAddPointButtonActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasAddPointButtonActionHandler.OnEvent(sender, e);
        }
    }
}
