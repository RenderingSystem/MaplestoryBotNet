using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


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

        public abstract object? Selected();

        public abstract void Select(object selected);

        public abstract void Deselect();

        public abstract void SetDragging(bool dragging);

        public abstract bool Dragging();
    }


    public abstract class AbstractMapCanvasElementFactory
    {
        public abstract FrameworkElement Create();
    }


    public abstract class AbstractMapCanvasFormatter
    {
        public abstract void Format(FrameworkElement canvas, MapModel mapModel);
    }


    public abstract class AbstractMapCanvasElementLocator
    {
        public abstract FrameworkElement? Locate(MapModel mapModel, Point point);
    }


    public class WindowMapEditMenuState : AbstractWindowMapEditMenuState
    {
        private WindowMapEditMenuStateTypes _currentState = WindowMapEditMenuStateTypes.Select;

        private object? _selected = null;

        private bool _dragging = false;

        public override WindowMapEditMenuStateTypes GetState()
        {
            return _currentState;
        }

        public override void SetState(WindowMapEditMenuStateTypes state)
        {
            _currentState = state;
        }

        public override void Select(object selected)
        {
            _selected = selected;
        }

        public override void Deselect()
        {
            _selected = null;
        }

        public override object? Selected()
        {
            return _selected;
        }

        public override void SetDragging(bool dragging)
        {
            _dragging = dragging;
        }

        public override bool Dragging()
        {
            return _dragging;
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

        public MapModel ElementModel = new MapModel();
    }


    public class WindowMapCanvasPointFormatter : AbstractMapCanvasFormatter
    {
        private void _setupLabelText(
            FrameworkElement createdPoint, MapModel mapModel, string pointLabel
        )
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(createdPoint); i++)
            {
                var child = VisualTreeHelper.GetChild(createdPoint, i);
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
            foreach (FrameworkElement child in canvasElement.Children)
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
            FrameworkElement createdPoint,
            MapModel mapModel,
            string pointLabel,
            string elementName
        )
        {
            if (createdPoint is Canvas canvasElement)
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

        private string _generateElementName(MapModel mapModel)
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

        private string _generatePointLabel(MapModel mapModel)
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

        public override void Format(FrameworkElement createdPoint, MapModel mapModel)
        {
            var pointLabel = _generatePointLabel(mapModel);
            var elementName = _generateElementName(mapModel);
            _setupMinimapPoint(createdPoint, mapModel, pointLabel, elementName);
            _setupLabelText(createdPoint, mapModel, pointLabel);
        }
    }


    public class WindowMapCanvasPointLocator : AbstractMapCanvasElementLocator
    {
        private Canvas _canvas;

        public WindowMapCanvasPointLocator(Canvas canvas)
        {
            _canvas = canvas;
        }

        public override FrameworkElement? Locate(MapModel mapModel, Point point)
        {
            var pointHit = mapModel.Points().LastOrDefault(
                p =>
                (
                    point.X >= p.X - p.XRange / 2 && point.X <= p.X + p.XRange / 2 &&
                    point.Y >= p.Y - p.YRange / 2 && point.Y <= p.Y + p.YRange / 2
                )
            );
            if (pointHit != null)
            {
                return _canvas.Children.OfType<Canvas>().FirstOrDefault(
                    c => c.Name == pointHit.PointData.ElementName
                );
            }
            return null;
        }
    }


    public class WindowMapCanvasPointDrawer : AbstractWindowStateModifier
    {
        private Canvas _canvas;

        private AbstractMapCanvasElementFactory _pointFactory;

        private FrameworkElement? _createdPoint;

        private AbstractMapCanvasFormatter _formatter;

        public WindowMapCanvasPointDrawer(
            Canvas canvas,
            AbstractMapCanvasElementFactory pointFactory,
            AbstractMapCanvasFormatter formatter

        )
        {
            _canvas = canvas;
            _pointFactory = pointFactory;
            _createdPoint = null;
            _formatter = formatter;
        }

        public override void Modify(object? value)
        {
            if (value is WindowMapCanvasDrawerParameters parameters)
            {
                _createdPoint = _pointFactory.Create();
                _canvas.Children.Add(_createdPoint);
                Canvas.SetLeft(_createdPoint, parameters.ElementPoint.X);
                Canvas.SetTop(_createdPoint, parameters.ElementPoint.Y);
                _formatter.Format(_createdPoint, parameters.ElementModel);
            }
        }

        public override object? State(int stateType)
        {
            return _createdPoint;
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

        public override FrameworkElement Create()
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


    public class WindowMapCanvasPointDrawingActionHandler : AbstractWindowActionHandler
    {
        private Canvas _canvas;

        private WindowMapEditMenuState _menuState;

        private AbstractWindowStateModifier _pointDrawer;

        private AbstractMouseEventPositionExtractor _mousePositionExtractor;

        private MapModel? _mapModel;

        public WindowMapCanvasPointDrawingActionHandler(
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
            if (_menuState.GetState() == WindowMapEditMenuStateTypes.Add && _mapModel != null)
            {
                _pointDrawer.Modify(
                    new WindowMapCanvasDrawerParameters
                    {
                        ElementPoint = _mousePositionExtractor.GetPosition((MouseButtonEventArgs)e, _canvas),
                        ElementModel = _mapModel,
                    }
                );
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (dataType == SystemInjectType.MapModel && data is MapModel mapModel)
            {
                _mapModel = mapModel;
            }
        }
    }


    public class WindowMapCanvasPointDrawingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapCanvasPointDrawingActionHandler;

        public WindowMapCanvasPointDrawingActionHandlerFacade(
            Canvas canvas,
            WindowMapEditMenuState menuState,
            AbstractMouseEventPositionExtractor mouseEventPositionExtractor
        )
        {
            _mapCanvasPointDrawingActionHandler = new WindowMapCanvasPointDrawingActionHandler(
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
                    ),
                    new WindowMapCanvasPointFormatter()
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


    public class WindowMapCanvasEraserParameters
    {
        public Point ElementPoint = new Point();

        public MapModel ElementModel = new MapModel();
    }


    public class WindowMapCanvasPointEraser : AbstractWindowStateModifier
    {
        private Canvas _canvas;

        private AbstractMapCanvasElementLocator _pointLocator;

        public WindowMapCanvasPointEraser(
            Canvas canvas,
            AbstractMapCanvasElementLocator pointLocator
        )
        {
            _canvas = canvas;
            _pointLocator = pointLocator;
        }

        public override void Modify(object? value)
        {
            if (value is WindowMapCanvasEraserParameters parameters)
            {
                var pointUI = _pointLocator.Locate(
                    parameters.ElementModel, parameters.ElementPoint
                );
                if (pointUI != null)
                {
                    _canvas.Children.Remove(pointUI);
                    parameters.ElementModel.Remove(pointUI.Name);
                }
            }
        }
    }


    public class WindowMapCanvasPointErasingActionHandler : AbstractWindowActionHandler
    {
        private Canvas _canvas;

        private WindowMapEditMenuState _menuState;

        private AbstractWindowStateModifier _pointEraser;

        private AbstractMouseEventPositionExtractor _mousePositionExtractor;

        private MapModel? _mapModel;

        public WindowMapCanvasPointErasingActionHandler(
            Canvas canvas,
            WindowMapEditMenuState menuState,
            AbstractMouseEventPositionExtractor mouseEventPositionExtractor,
            AbstractWindowStateModifier pointEraser
        )
        {
            _canvas = canvas;
            _menuState = menuState;
            _pointEraser = pointEraser;
            _mousePositionExtractor = mouseEventPositionExtractor;
            _canvas.MouseLeftButtonDown += OnEvent;
            _mapModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _pointEraser;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_menuState.GetState() == WindowMapEditMenuStateTypes.Remove && _mapModel != null)
            {
                _pointEraser.Modify(
                    new WindowMapCanvasEraserParameters
                    {
                        ElementPoint = _mousePositionExtractor.GetPosition((MouseButtonEventArgs)e, _canvas),
                        ElementModel = _mapModel,
                    }
                );
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (dataType == SystemInjectType.MapModel && data is MapModel mapModel)
            {
                _mapModel = mapModel;
            }
        }
    }


    public class WindowMapCanvasPointErasingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapCanvasPointEraserActionHandler;

        public WindowMapCanvasPointErasingActionHandlerFacade(
            Canvas canvas,
            WindowMapEditMenuState menuState,
            AbstractMouseEventPositionExtractor mouseEventPositionExtractor
        )
        {
            _mapCanvasPointEraserActionHandler = new WindowMapCanvasPointErasingActionHandler(
                canvas,
                menuState,
                mouseEventPositionExtractor,
                new WindowMapCanvasPointEraser(
                    canvas,
                    new WindowMapCanvasPointLocator(canvas)
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasPointEraserActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasPointEraserActionHandler.OnEvent(sender, e);
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _mapCanvasPointEraserActionHandler.Inject(dataType, data);
        }
    }


    public class WindowMapCanvasAddPointButtonModifier : AbstractWindowStateModifier
    {
        private WindowMapEditMenuState _menuState;

        private ToggleButton _addButton;

        private List<ToggleButton> _radioButtons;

        public WindowMapCanvasAddPointButtonModifier(
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


    public class WindowMapAddPointButtonActionHandler : AbstractWindowActionHandler
    {
        private ToggleButton _addPointButton;

        private AbstractWindowStateModifier _addStateModifier;

        public WindowMapAddPointButtonActionHandler(
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


    public class WindowMapAddPointButtonActionHandlerFacade : AbstractWindowActionHandler
    {
        private WindowMapAddPointButtonActionHandler _mapCanvasAddPointButtonActionHandler;

        public WindowMapAddPointButtonActionHandlerFacade(
            ToggleButton addPointButton,
            List<ToggleButton> radioButtons,
            WindowMapEditMenuState menuState
        )
        {
            _mapCanvasAddPointButtonActionHandler = new WindowMapAddPointButtonActionHandler(
                addPointButton,
                new WindowMapCanvasAddPointButtonModifier(menuState, addPointButton, radioButtons)
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


    public class WindowMapRemovePointButtonModifier : AbstractWindowStateModifier
    {
        private WindowMapEditMenuState _menuState;

        private ToggleButton _removeButton;

        private List<ToggleButton> _radioButtons;

        public WindowMapRemovePointButtonModifier(
            WindowMapEditMenuState menuState,
            ToggleButton addButton,
            List<ToggleButton> radioButtons

        )
        {
            _menuState = menuState;
            _removeButton = addButton;
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
                _removeButton.IsChecked = checkedState;
                var nextState = checkedState ?
                    WindowMapEditMenuStateTypes.Remove :
                    WindowMapEditMenuStateTypes.Select;
                _menuState.SetState(nextState);
            }
        }
    }


    public class WindowMapRemovePointButtonActionHandler : AbstractWindowActionHandler
    {
        private ToggleButton _removePointButton;

        private AbstractWindowStateModifier _addStateModifier;

        public WindowMapRemovePointButtonActionHandler(
            ToggleButton removePointButton,
            AbstractWindowStateModifier addStateModifier
        )
        {
            _removePointButton = removePointButton;
            _removePointButton.Click += OnEvent;
            _addStateModifier = addStateModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _addStateModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _addStateModifier.Modify(_removePointButton.IsChecked);
        }
    }


    public class WindowMapRemovePointButtonActionHandlerFacade : AbstractWindowActionHandler
    {
        private WindowMapRemovePointButtonActionHandler _mapCanvasRemovePointButtonActionHandler;

        public WindowMapRemovePointButtonActionHandlerFacade(
            ToggleButton removePointButton,
            List<ToggleButton> radioButtons,
            WindowMapEditMenuState menuState
        )
        {
            _mapCanvasRemovePointButtonActionHandler = new WindowMapRemovePointButtonActionHandler(
                removePointButton,
                new WindowMapRemovePointButtonModifier(menuState, removePointButton, radioButtons)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasRemovePointButtonActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasRemovePointButtonActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapCanvasSelectModifierParameters
    {
        public Point ElementPoint = new Point(0, 0);

        public MapModel ElementModel = new MapModel();
    }


    public class WindowMapCanvasSelectModifier : AbstractWindowStateModifier
    {
        private AbstractMapCanvasElementLocator _pointLocator;

        private AbstractWindowMapEditMenuState _menuState;

        private TextBox _selectedTextX;

        private TextBox _selectedTextY;

        private TextBox _selectedTextBox;

        public WindowMapCanvasSelectModifier(
            AbstractMapCanvasElementLocator pointLocator,
            TextBox selectedTextX,
            TextBox selectedTextY,
            TextBox selectedTextLabel,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _pointLocator = pointLocator;
            _selectedTextX = selectedTextX;
            _selectedTextY = selectedTextY;
            _selectedTextBox = selectedTextLabel;
            _menuState = menuState;
        }

        private void _assignUITexts(FrameworkElement element, MapModel mapModel)
        {
            var selectedPoint = mapModel.FindName(element.Name);
            if (selectedPoint != null)
            {
                _selectedTextBox.Text = selectedPoint.PointData.PointName;
                _selectedTextX.Text = Convert.ToInt32(Canvas.GetLeft(element)).ToString();
                _selectedTextY.Text = Convert.ToInt32(Canvas.GetTop(element)).ToString();
            }
        }

        public override void Modify(object? value)
        {
            if (value is WindowMapCanvasSelectModifierParameters parameters)
            {
                var pointUI = _pointLocator.Locate(
                    parameters.ElementModel, parameters.ElementPoint
                );
                if (pointUI != null)
                {
                    _menuState.Select(pointUI);
                    _assignUITexts(pointUI, parameters.ElementModel);
                }
                else
                {
                    _menuState.Deselect();
                }
            }
        }
    }


    public class WindowMapCanvasSelectActionHandler : AbstractWindowActionHandler
    {
        private Canvas _canvas;

        private AbstractWindowMapEditMenuState _menuState;

        private AbstractWindowStateModifier _mapCanvasSelectModifier;

        private AbstractMouseEventPositionExtractor _mousePositionExtractor;

        private MapModel? _mapModel;

        public WindowMapCanvasSelectActionHandler(
            Canvas canvas,
            AbstractWindowMapEditMenuState menuState,
            AbstractWindowStateModifier mapCanvasSelectModifier,
            AbstractMouseEventPositionExtractor mousePositionExtractor
        )
        {
            _canvas = canvas;
            _menuState = menuState;
            _mapCanvasSelectModifier = mapCanvasSelectModifier;
            _mousePositionExtractor = mousePositionExtractor;
            _mapModel = null;
            _canvas.MouseLeftButtonDown += OnEvent;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_menuState.GetState() == WindowMapEditMenuStateTypes.Select && _mapModel != null)
            {
                _mapCanvasSelectModifier.Modify(
                    new WindowMapCanvasSelectModifierParameters
                    {
                        ElementPoint = _mousePositionExtractor.GetPosition((MouseButtonEventArgs)e, _canvas),
                        ElementModel = _mapModel
                    }
                );
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (dataType == SystemInjectType.MapModel && data is MapModel mapModel)
            {
                _mapModel = mapModel;
            }
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasSelectModifier;
        }
    }


    public class WindowMapCanvasSelectActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapCanvasSelectActionHandler;

        public WindowMapCanvasSelectActionHandlerFacade(
            Canvas canvas,
            TextBox selectedTextX,
            TextBox selectedTextY,
            TextBox selectedTextBox,
            AbstractWindowMapEditMenuState menuState,
            AbstractMouseEventPositionExtractor mousePositionExtractor
        )
        {
            _mapCanvasSelectActionHandler = new WindowMapCanvasSelectActionHandler(
                canvas,
                menuState,
                new WindowMapCanvasSelectModifier(
                    new WindowMapCanvasPointLocator(canvas),
                    selectedTextX,
                    selectedTextY,
                    selectedTextBox,
                    menuState
                ),
                mousePositionExtractor
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasSelectActionHandler.OnEvent(sender, e);
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _mapCanvasSelectActionHandler.Inject(dataType, data);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasSelectActionHandler.Modifier();
        }
    }


    public class WindowMapCanvasDragModifierParameters
    {
        public Point ElementPoint = new Point();

        public FrameworkElement ElementDragging = new FrameworkElement();

        public MapModel ElementModel = new MapModel();
    }


    public class WindowMapCanvasDragModifier : AbstractWindowStateModifier
    {
        private TextBox _selectedTextX;

        private TextBox _selectedTextY;

        public WindowMapCanvasDragModifier(
            AbstractWindowMapEditMenuState menuState,
            TextBox selectedTextX,
            TextBox selectedTextY
        )
        {
            _selectedTextX = selectedTextX;
            _selectedTextY = selectedTextY;
        }

        private void _updateSelectedText(FrameworkElement draggingElement, Point point)
        {
            Canvas.SetLeft(draggingElement, point.X);
            Canvas.SetTop(draggingElement, point.Y);
            _selectedTextX.Text = Convert.ToInt32(point.X).ToString();
            _selectedTextY.Text = Convert.ToInt32(point.Y).ToString();
        }

        private void _updateMapModel(FrameworkElement draggingElement, Point point, MapModel mapModel)
        {
            var draggingPoint = mapModel.FindName(draggingElement.Name);
            if (draggingPoint != null)
            {
                draggingPoint.X = point.X;
                draggingPoint.Y = point.Y;
                mapModel.Edit(draggingPoint);
            }
        }

        public override void Modify(object? value)
        {
            if (value is WindowMapCanvasDragModifierParameters parameters)
            {
                _updateSelectedText(
                    parameters.ElementDragging,
                    parameters.ElementPoint
                );
                _updateMapModel(
                    parameters.ElementDragging,
                    parameters.ElementPoint,
                    parameters.ElementModel
                );
            }
        }
    }


    public class WindowMapCanvasDragActionHandler : AbstractWindowActionHandler
    {
        private Canvas _canvas;

        private AbstractWindowMapEditMenuState _menuState;

        private AbstractWindowStateModifier _mapCanvasDragModifier;

        private AbstractMapCanvasElementLocator _mapCanvasElementLocator;

        private AbstractMouseEventPositionExtractor _mousePositionExtractor;

        private MapModel? _mapModel;

        private FrameworkElement? _draggingElement;

        public WindowMapCanvasDragActionHandler(
            Canvas canvas,
            AbstractWindowMapEditMenuState menuState,
            AbstractWindowStateModifier mapCanvasDragModifier,
            AbstractMapCanvasElementLocator mapCanvasElementLocator,
            AbstractMouseEventPositionExtractor mousePositionExtractor
        )
        {
            _canvas = canvas;
            _menuState = menuState;
            _mapCanvasDragModifier = mapCanvasDragModifier;
            _mapCanvasElementLocator = mapCanvasElementLocator;
            _mousePositionExtractor = mousePositionExtractor;
            _draggingElement = null;
            _mapModel = null;
            _canvas.MouseLeftButtonDown += OnEvent;
            _canvas.MouseLeftButtonUp += OnEvent;
            _canvas.MouseMove += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasDragModifier;
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (dataType == SystemInjectType.MapModel && data is MapModel mapModel)
            {
                _mapModel = mapModel;
            }
        }

        private void _handleMouseLeftButtonDown(MouseButtonEventArgs eventArgs)
        {
            var mousePoint = _mousePositionExtractor.GetPosition(eventArgs, _canvas);
            _draggingElement = _mapCanvasElementLocator.Locate(_mapModel!, mousePoint);
        }

        private void _handleMouseLeftButtonUp(MouseButtonEventArgs eventArgs)
        {
            _draggingElement = null;
        }

        private void _handleMouseMove(MouseEventArgs eventArgs)
        {
            if (_draggingElement == null) return;
            var mousePoint = _mousePositionExtractor.GetPosition(eventArgs, _canvas);
            if (
                mousePoint.X >= 0
                && mousePoint.X <= _canvas.ActualWidth
                && mousePoint.Y >= 0
                && mousePoint.Y <= _canvas.ActualHeight
            )
            {
                _mapCanvasDragModifier.Modify(
                    new WindowMapCanvasDragModifierParameters
                    {
                        ElementPoint = mousePoint,
                        ElementDragging = _draggingElement,
                        ElementModel = _mapModel!,
                    }
                );
            }
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_menuState.GetState() != WindowMapEditMenuStateTypes.Select) return;
            if (_mapModel == null) return;
            if (e is not RoutedEventArgs rea) return;
            if (rea.RoutedEvent == UIElement.MouseLeftButtonDownEvent)
            {
                _handleMouseLeftButtonDown((MouseButtonEventArgs) e);
            }
            if (rea.RoutedEvent == UIElement.MouseLeftButtonUpEvent)
            {
                _handleMouseLeftButtonUp((MouseButtonEventArgs) e);
            }
            if (rea.RoutedEvent == UIElement.MouseMoveEvent)
            {
                _handleMouseMove((MouseEventArgs) e);
            }
        }
    }


    public class WindowMapCanvasDragActionHandlerFacade : AbstractWindowActionHandler
    {
        AbstractWindowActionHandler _mapCanvasDragActionHandler;

        public WindowMapCanvasDragActionHandlerFacade(
            Canvas canvas,
            TextBox selectedX,
            TextBox selectedY,
            AbstractWindowMapEditMenuState menuState,
            AbstractMouseEventPositionExtractor mousePositionExtractor
        )
        {
            _mapCanvasDragActionHandler = new WindowMapCanvasDragActionHandler(
                canvas,
                menuState,
                new WindowMapCanvasDragModifier(menuState, selectedX, selectedY),
                new WindowMapCanvasPointLocator(canvas),
                mousePositionExtractor
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasDragActionHandler.Modifier();
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _mapCanvasDragActionHandler.Inject(dataType, data);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasDragActionHandler.OnEvent(sender, e);
        }
    }
}
