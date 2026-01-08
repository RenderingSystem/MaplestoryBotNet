using MaplestoryBotNet.Systems.Configuration.SubSystems;
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

        public abstract void SetEditingText(bool editing);

        public abstract bool GetEditingText();
    }


    public abstract class AbstractMapCanvasElementFactory
    {
        public abstract FrameworkElement Create();
    }


    public abstract class AbstractMapCanvasFormatter
    {
        public abstract void Format(
            FrameworkElement canvas,
            List<FrameworkElement> textDepdendencies,
            object formatData
        );
    }


    public abstract class AbstractMapCanvasElementLocator
    {
        public abstract FrameworkElement? Locate(AbstractMapModel mapModel, Point point);
    }


    public abstract class AbstractPointElementInformation
    {
        public abstract Rect BoundingRect(FrameworkElement frameworkElement);

        public abstract TextBlock? Label(FrameworkElement frameworkElement);
    }


    public class WindowMapEditMenuState : AbstractWindowMapEditMenuState
    {
        private WindowMapEditMenuStateTypes _currentState = WindowMapEditMenuStateTypes.Select;

        private object? _selected = null;

        private bool _dragging = false;

        private bool _editing = false;

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

        public override void SetEditingText(bool editing)
        {
            _editing = editing;
        }

        public override bool GetEditingText()
        {
            return _editing;
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

        public List<FrameworkElement> ElementDependencies = [];

        public AbstractMapModel ElementModel = new MapModel();
    }


    public class PointElementInformation : AbstractPointElementInformation
    {
        public override Rect BoundingRect(FrameworkElement frameworkElement)
        {
            if (frameworkElement is not Canvas canvasElement)
            {
                return new Rect(0, 0, 1, 1);
            }
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

        public override TextBlock? Label(FrameworkElement frameworkElement)
        {
            if (frameworkElement is not Canvas canvasElement)
            {
                return null;
            }
            foreach (FrameworkElement child in canvasElement.Children)
            {
                if (child is TextBlock childTextBlock)
                {
                    return childTextBlock;
                }
            }
            return null;
        }
    }


    public class WindowMapCanvasPointFormatter : AbstractMapCanvasFormatter
    {
        private AbstractPointElementInformation _pointElementInfo;

        public WindowMapCanvasPointFormatter(
            AbstractPointElementInformation pointElementInfo
        )
        {
            _pointElementInfo = pointElementInfo;
        }

        private void _setupLabelText(
            FrameworkElement createdPoint,
            string pointLabel
        )
        {
            var label = _pointElementInfo.Label(createdPoint);
            if (label != null)
            {
                label.Text = pointLabel;
            }
        }

        private MinimapPointData _minimapPointData(
            FrameworkElement frameworkElement,
            List<FrameworkElement> textDependencies,
            string pointName,
            string elementName
        )
        {
            frameworkElement.Name = elementName;
            return new MinimapPointData
            {
                ElementTexts = new List<FrameworkElement>(textDependencies)
                {
                    _pointElementInfo.Label(frameworkElement)!
                },
                PointName = pointName,
                ElementName = elementName,
                Commands = [
                    new MinimapPointMacros
                    {
                        MacroName = "Default",
                        MacroChance = 100,
                        MacroCommands = []
                    }
                ]
            };
        }

        private void _setupMinimapPoint(
            FrameworkElement createdPoint,
            List<FrameworkElement> textDependencies,
            AbstractMapModel mapModel,
            string pointLabel,
            string elementName
        )
        {
            if (createdPoint is Canvas canvasElement)
            {
                var boundingRect = _pointElementInfo.BoundingRect(canvasElement);
                var pointData = _minimapPointData(
                    canvasElement,
                    textDependencies,
                    pointLabel,
                    elementName
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

        private string _generateElementName(AbstractMapModel mapModel)
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

        private string _generatePointLabel(AbstractMapModel mapModel)
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

        public override void Format(
            FrameworkElement createdPoint,
            List<FrameworkElement> textDependencies,
            object formatData
        )
        {
            if (formatData is not MapModel mapModel)
            {
                return;
            }
            var pointLabel = _generatePointLabel(mapModel);
            var elementName = _generateElementName(mapModel);
            _setupMinimapPoint(
                createdPoint,
                textDependencies,
                mapModel,
                pointLabel,
                elementName
            );
            _setupLabelText(
                createdPoint,
                pointLabel
            );
        }
    }


    public class WindowMapCanvasPointLocator : AbstractMapCanvasElementLocator
    {
        private Canvas _canvas;

        public WindowMapCanvasPointLocator(Canvas canvas)
        {
            _canvas = canvas;
        }

        public override FrameworkElement? Locate(AbstractMapModel mapModel, Point point)
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
                _formatter.Format(
                    _createdPoint,
                    parameters.ElementDependencies,
                    parameters.ElementModel
                );
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


    public class WindowMapCanvasPointDrawingActionHandler : AbstractWindowActionHandler
    {
        private Canvas _canvas;

        private AbstractWindowMapEditMenuState _menuState;

        private TextBox _pointLabelTextBox;

        private AbstractWindowStateModifier _pointDrawer;

        private AbstractMouseEventPositionExtractor _mousePositionExtractor;

        private AbstractMapModel? _mapModel;

        public WindowMapCanvasPointDrawingActionHandler(
            Canvas canvas,
            TextBox pointLabelTextBox,
            AbstractWindowMapEditMenuState menuState,
            AbstractMouseEventPositionExtractor mousePositionExtractor,
            AbstractWindowStateModifier pointDrawer
        )
        {
            _canvas = canvas;
            _pointLabelTextBox = pointLabelTextBox;
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
                        ElementDependencies = [_pointLabelTextBox],
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
            TextBox pointLabelTextBox,
            AbstractWindowMapEditMenuState menuState,
            AbstractMouseEventPositionExtractor mouseEventPositionExtractor
        )
        {
            _mapCanvasPointDrawingActionHandler = new WindowMapCanvasPointDrawingActionHandler(
                canvas,
                pointLabelTextBox,
                menuState,
                mouseEventPositionExtractor,
                new WindowMapCanvasPointDrawer(
                    canvas,
                    new WindowMapCanvasPointFactoryFacade(),
                    new WindowMapCanvasPointFormatter(new PointElementInformation())
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

        public AbstractMapModel ElementModel = new MapModel();
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

        private AbstractWindowMapEditMenuState _menuState;

        private AbstractWindowStateModifier _pointEraser;

        private AbstractMouseEventPositionExtractor _mousePositionExtractor;

        private AbstractMapModel? _mapModel;

        public WindowMapCanvasPointErasingActionHandler(
            Canvas canvas,
            AbstractWindowMapEditMenuState menuState,
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
            AbstractWindowMapEditMenuState menuState,
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
        private AbstractWindowMapEditMenuState _menuState;

        private ToggleButton _addButton;

        private List<ToggleButton> _radioButtons;

        public WindowMapCanvasAddPointButtonModifier(
            AbstractWindowMapEditMenuState menuState,
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
            AbstractWindowMapEditMenuState menuState
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
        private AbstractWindowMapEditMenuState _menuState;

        private ToggleButton _removeButton;

        private List<ToggleButton> _radioButtons;

        public WindowMapRemovePointButtonModifier(
            AbstractWindowMapEditMenuState menuState,
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
            AbstractWindowMapEditMenuState menuState
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

        public AbstractMapModel ElementModel = new MapModel();
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

        private void _assignUITexts(FrameworkElement element, AbstractMapModel mapModel)
        {
            var selectedPoint = mapModel.FindName(element.Name);
            if (selectedPoint != null)
            {
                _menuState.SetEditingText(true);
                _selectedTextBox.Text = selectedPoint.PointData.PointName;
                _selectedTextX.Text = Convert.ToInt32(Canvas.GetLeft(element)).ToString();
                _selectedTextY.Text = Convert.ToInt32(Canvas.GetTop(element)).ToString();
                _menuState.SetEditingText(false);
            }
        }

        private void _clearUITexts()
        {
            _menuState.SetEditingText(true);
            _selectedTextBox.Text = "";
            _selectedTextX.Text = "0";
            _selectedTextY.Text = "0";
            _menuState.SetEditingText(false);
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
                    _clearUITexts();
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

        private AbstractMapModel? _mapModel;

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

        public AbstractMapModel ElementModel = new MapModel();
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

        private void _updateMapModel(
            FrameworkElement draggingElement,
            Point point,
            AbstractMapModel mapModel
        )
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

        private AbstractMapModel? _mapModel;

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
            if (_draggingElement == null)
            {
                return;
            }
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
            if (_menuState.GetState() != WindowMapEditMenuStateTypes.Select)
            {
                return;
            }
            if (_mapModel == null)
            {
                return;
            }
            if (e is not RoutedEventArgs rea)
            {
                return;
            }
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


    public class WindowMapEditButtonAccessibilityModifier : AbstractWindowStateModifier
    {
        AbstractWindowMapEditMenuState _menuState;

        Button _editButton;

        public WindowMapEditButtonAccessibilityModifier(
            Button editButton,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _menuState = menuState;
            _editButton = editButton;
        }

        public override void Modify(object? value)
        {
            _editButton.IsEnabled = _menuState.Selected() != null;
        }
    }


    public class WindowMapEditButtonAccessibilityActionHandler : AbstractWindowActionHandler
    {
        private Canvas _canvas;

        private AbstractWindowStateModifier _mapCanvasEditButtonAccessibilityModifier;

        public WindowMapEditButtonAccessibilityActionHandler(
            Canvas canvas,
            AbstractWindowStateModifier mapCanvasEditButtonAccessibilityModifier
       
        )
        {
            _canvas = canvas;
            _mapCanvasEditButtonAccessibilityModifier = mapCanvasEditButtonAccessibilityModifier;
            _canvas.MouseLeftButtonDown += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasEditButtonAccessibilityModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasEditButtonAccessibilityModifier.Modify(null);
        }
    }


    public class WindowMapEditButtonAccessibilityActionHandlerFacade : AbstractWindowActionHandler
    {
        private WindowMapEditButtonAccessibilityActionHandler _mapEditButtonAccessibilityActionHandler;

        public WindowMapEditButtonAccessibilityActionHandlerFacade(
            Canvas canvas,
            Button editButton,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _mapEditButtonAccessibilityActionHandler = new WindowMapEditButtonAccessibilityActionHandler(
                canvas, new WindowMapEditButtonAccessibilityModifier(editButton, menuState)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapEditButtonAccessibilityActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapEditButtonAccessibilityActionHandler.OnEvent(sender, e);
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _mapEditButtonAccessibilityActionHandler.Inject(dataType, data);
        }
    }


    public class WindowMapCanvasPointLocationModifierParameters
    {
        public Point ElementPoint = new Point();

        public AbstractMapModel ElementModel = new MapModel();
    }


    public class WindowMapCanvasPointLocationModifier : AbstractWindowStateModifier
    {
        private AbstractWindowMapEditMenuState _menuState;

        public WindowMapCanvasPointLocationModifier(
            AbstractWindowMapEditMenuState menuState
        )
        {
            _menuState = menuState;
        }

        private void _updateElement(FrameworkElement selected, Point point)
        {
            Canvas.SetLeft(selected, point.X);
            Canvas.SetTop(selected, point.Y);
        }

        private void _updateMapModel(AbstractMapModel mapModel, Point point, string name)
        {
            var model = mapModel.FindName(name);
            if (model != null)
            {
                model.X = point.X;
                model.Y = point.Y;
                mapModel.Edit(model);
            }
        }

        public override void Modify(object? value)
        {
            if (
                !_menuState.GetEditingText()
                && value is WindowMapCanvasPointLocationModifierParameters parameters)
            {
                var selected = _menuState.Selected();
                if (selected is FrameworkElement selectedElement)
                {
                    _updateMapModel(
                        parameters.ElementModel,
                        parameters.ElementPoint,
                        selectedElement.Name
                    );
                    _updateElement(
                        selectedElement,
                        parameters.ElementPoint
                    );
                }
            }
        }
    }

    
    public class WindowMapCanvasPointLocationActionHandler : AbstractWindowActionHandler
    {
        private TextBox _selectedTextX;

        private TextBox _selectedTextY;

        private AbstractMapModel? _mapModel;

        private AbstractWindowStateModifier _canvasPointLocationModifier;

        public WindowMapCanvasPointLocationActionHandler(
            TextBox selectedTextX,
            TextBox selectedTextY,
            AbstractWindowStateModifier canvasPointLocationModifier
        )
        {
            _selectedTextX = selectedTextX;
            _selectedTextY = selectedTextY;
            _selectedTextX.TextChanged += OnEvent;
            _selectedTextY.TextChanged += OnEvent;
            _mapModel = null;
            _canvasPointLocationModifier = canvasPointLocationModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _canvasPointLocationModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_mapModel == null)
            {
                return;
            }
            if (_selectedTextX.Text != "" && _selectedTextY.Text != "")
            {
                _canvasPointLocationModifier.Modify(
                    new WindowMapCanvasPointLocationModifierParameters
                    {
                        ElementPoint = new Point(
                            Convert.ToInt32(_selectedTextX.Text),
                            Convert.ToInt32(_selectedTextY.Text)
                        ),
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
    }


    public class WindowMapCanvasPointLocationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _canvasPointLocationActionHandler;

        public WindowMapCanvasPointLocationActionHandlerFacade(
            TextBox selectedTextX,
            TextBox selectedTextY,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _canvasPointLocationActionHandler = new WindowMapCanvasPointLocationActionHandler(
                selectedTextX,
                selectedTextY,
                new WindowMapCanvasPointLocationModifier(menuState)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _canvasPointLocationActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _canvasPointLocationActionHandler.OnEvent(sender, e);
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _canvasPointLocationActionHandler.Inject(dataType, data);
        }
    }


    public class WindowMapCanvasDimensionModifierParameters
    {
        public MapModel ElementModel = new MapModel();
    }


    public class WindowMapCanvasDimensionModifier : AbstractWindowStateModifier
    {
        private TextBox _textBoxLeft;

        private TextBox _textBoxTop;

        private TextBox _textBoxRight;

        private TextBox _textBoxBottom;

        public WindowMapCanvasDimensionModifier(
            TextBox textBoxLeft,
            TextBox textBoxTop,
            TextBox textBoxRight,
            TextBox textBoxBottom
        )
        {
            _textBoxLeft = textBoxLeft;
            _textBoxTop = textBoxTop;
            _textBoxRight = textBoxRight;
            _textBoxBottom = textBoxBottom;

        }

        public override void Modify(object? value)
        {
            if (value is WindowMapCanvasDimensionModifierParameters parameters)
            {
                var leftInt = _textBoxLeft.Text == "" ? 0 : Convert.ToInt32(_textBoxLeft.Text);
                var topInt = _textBoxTop.Text == "" ? 0 : Convert.ToInt32(_textBoxTop.Text);
                var rightInt = _textBoxRight.Text == "" ? 0 : Convert.ToInt32(_textBoxRight.Text);
                var bottomInt = _textBoxBottom.Text == "" ? 0 : Convert.ToInt32(_textBoxBottom.Text);
                if (rightInt > leftInt && bottomInt > topInt)
                {
                    parameters.ElementModel.SetMapArea(leftInt, topInt, rightInt, bottomInt);
                }
            }
        }
    }


    public class WindowMapCanvasDimensionActionHandler : AbstractWindowActionHandler
    {
        private TextBox _textBoxLeft;

        private TextBox _textBoxTop;

        private TextBox _textBoxRight;

        private TextBox _textBoxBottom;

        private AbstractWindowStateModifier _mapCanvasDimensionModifier;

        private MapModel? _mapModel;

        public WindowMapCanvasDimensionActionHandler(
            TextBox textBoxLeft,
            TextBox textBoxTop,
            TextBox textBoxRight,
            TextBox textBoxBottom,
            AbstractWindowStateModifier mapCanvasDimensionModifier
        )
        {
            _textBoxLeft = textBoxLeft;
            _textBoxTop = textBoxTop;
            _textBoxRight = textBoxRight;
            _textBoxBottom = textBoxBottom;
            _mapCanvasDimensionModifier = mapCanvasDimensionModifier;
            _textBoxLeft.TextChanged += OnEvent;
            _textBoxTop.TextChanged += OnEvent;
            _textBoxRight.TextChanged += OnEvent;
            _textBoxBottom.TextChanged += OnEvent;
            _mapModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasDimensionModifier;
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

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasDimensionModifier.Modify(
                new WindowMapCanvasDimensionModifierParameters
                {
                    ElementModel = _mapModel!
                }
            );
        }
    }


    public class WindowMapCanvasDimensionActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapCanvasDimensionActionHandler;

        public WindowMapCanvasDimensionActionHandlerFacade(
            TextBox textBoxLeft,
            TextBox textBoxTop,
            TextBox textBoxRight,
            TextBox textBoxBottom
        )
        {
            _mapCanvasDimensionActionHandler = new WindowMapCanvasDimensionActionHandler(
                textBoxLeft,
                textBoxTop,
                textBoxRight,
                textBoxBottom,
                new WindowMapCanvasDimensionModifier(
                    textBoxLeft,
                    textBoxTop,
                    textBoxRight,
                    textBoxBottom
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasDimensionActionHandler.Modifier();
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _mapCanvasDimensionActionHandler.Inject(dataType, data);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasDimensionActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapEditorSaveConfigurationActionHandler : AbstractWindowActionHandler
    {
        private Button _saveButton;

        private AbstractMapModelSerializer _mapModelSerializer;

        private AbstractJsonDataModelConverter _mapModelConverter;

        private AbstractWindowStateModifier _windowSaveDialogModifier;

        private MapModel? _mapModel;

        private string _initialDirectory;

        public WindowMapEditorSaveConfigurationActionHandler(
            Button saveButton,
            AbstractMapModelSerializer mapModelSerializer,
            AbstractJsonDataModelConverter mapModelConverter,
            AbstractWindowStateModifier windowSaveDialogModifier
        )
        {
            _saveButton = saveButton;
            _mapModelSerializer = mapModelSerializer;
            _mapModelConverter = mapModelConverter;
            _windowSaveDialogModifier = windowSaveDialogModifier;
            _saveButton.Click += OnEvent;
            _initialDirectory = "";
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_mapModel == null)
            {
                return;
            }
            if (_initialDirectory == "")
            {
                return;
            }
            var configuration = (
                (ConfigurationMapModel)
                _mapModelConverter.ToConfiguration(_mapModel)!
            );
            var serialized = (
                _mapModelSerializer.Serialize(configuration)
            );
            _windowSaveDialogModifier.Modify(
                new WindowSaveMenuModifierParameters
                {
                    InitialDirectory = _initialDirectory,
                    SaveContent = serialized
                }
            );
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (
                dataType == SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _initialDirectory = configuration.MapDirectory;
            }
            if (
                dataType == SystemInjectType.MapModel
                && data is MapModel mapModel
            )
            {
                _mapModel = mapModel;
            }
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowSaveDialogModifier;
        }
    }


    public class WindowMapEditorSaveConfigurationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _windowMapEditorSaveConfigurationActionHandler;

        public WindowMapEditorSaveConfigurationActionHandlerFacade(Button saveButton)
        {
            _windowMapEditorSaveConfigurationActionHandler = new WindowMapEditorSaveConfigurationActionHandler(
                saveButton,
                new ConfigurationMapModelSerializer(),
                new MapModelConverterFacade(),
                new WindowSaveMenuModifier(
                    new WindowSaveFileDialog(
                        "Save Map",
                        "JSON files (*.json)|*.json",
                        ".json"
                    )
                )
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _windowMapEditorSaveConfigurationActionHandler.OnEvent(sender, e);
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _windowMapEditorSaveConfigurationActionHandler.Inject(dataType, data);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowMapEditorSaveConfigurationActionHandler.Modifier();
        }
    }


    public class WindowMapEditorLoadConfigurationActionHandler : AbstractWindowActionHandler
    {
        private Button _loadButton;

        private AbstractWindowStateModifier _windowLoadDialogModifier;

        private string? _initialDirectory;

        public WindowMapEditorLoadConfigurationActionHandler(
            Button loadButton,
            AbstractWindowStateModifier windowLoadDialogModifier
        )
        {
            _loadButton = loadButton;
            _windowLoadDialogModifier = windowLoadDialogModifier;
            _loadButton.Click += OnEvent;
            _initialDirectory = null;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_initialDirectory == null)
            {
                return;
            }
            _windowLoadDialogModifier.Modify(
                new WindowLoadMenuModifierParameters
                {
                    InitialDirectory = _initialDirectory
                }
            );
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (
                dataType == SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _initialDirectory = configuration.MapDirectory;
            }
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowLoadDialogModifier;
        }
    }


    public class WindowMapEditorLoadConfigurationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapEditorLoadConfigurationActionHandler;

        public WindowMapEditorLoadConfigurationActionHandlerFacade(
            Button loadButton, AbstractLoadFileDialog loadFileDialog
        )
        {
            _mapEditorLoadConfigurationActionHandler = new WindowMapEditorLoadConfigurationActionHandler(
                loadButton, new WindowLoadMenuModifier(loadFileDialog)
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapEditorLoadConfigurationActionHandler.OnEvent(sender, e);
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _mapEditorLoadConfigurationActionHandler.Inject(dataType, data);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapEditorLoadConfigurationActionHandler.Modifier();
        }
    }


    public class WindowMapEditorLoadModelModifierParameters
    {
        public string LoadedConfiguration = "";

        public MapModel ElementModel = new MapModel();
    }


    public class WindowMapEditorLoadModelModifier : AbstractWindowStateModifier
    {
        private AbstractMapModelDeserializer _mapModelDeserializer;

        private AbstractJsonDataModelConverter _mapModelConverter;

        public WindowMapEditorLoadModelModifier(
            AbstractMapModelDeserializer mapModelDeserializer,
            AbstractJsonDataModelConverter mapModelConverter
        )
        {
            _mapModelDeserializer = mapModelDeserializer;
            _mapModelConverter = mapModelConverter;
        }

        public override void Modify(object? value)
        {
            if (value is not WindowMapEditorLoadModelModifierParameters parameters)
            {
                return;
            }
            var deserialized = _mapModelDeserializer.Deserialize(
                parameters.LoadedConfiguration
            );
            var mapModel = (MapModel)_mapModelConverter.ToDataModel(deserialized)!;
            parameters.ElementModel.SetMapModel(mapModel);
        }
    }


    public class WindowMapEditorLoadModelActionHandler : AbstractWindowActionHandler
    {
        private AbstractLoadFileDialog _loadFileDialog;

        private AbstractWindowStateModifier _mapEditorLoadModelModifier;

        private MapModel? _mapModel;

        public WindowMapEditorLoadModelActionHandler(
            AbstractLoadFileDialog loadFileDialog,
            AbstractWindowStateModifier mapEditorLoadModelModifier
        )
        {
            _loadFileDialog = loadFileDialog;
            _mapEditorLoadModelModifier = mapEditorLoadModelModifier;
            _mapModel = null;
            _loadFileDialog.FileLoaded += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapEditorLoadModelModifier;
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

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (e is not FileLoadedEventArgs fileLoadEventArgs)
            {
                return;
            }
            if (_mapModel == null)
            {
                return;
            }
            _mapEditorLoadModelModifier.Modify(
                new WindowMapEditorLoadModelModifierParameters
                {
                    LoadedConfiguration = fileLoadEventArgs.Content,
                    ElementModel = _mapModel
                }
            );
        }
    }


    public class WindowMapEditorLoadModelActionHandlerFacade : AbstractWindowActionHandler
    {
        AbstractWindowActionHandler _mapEditorLoadModelActionHandler;

        public WindowMapEditorLoadModelActionHandlerFacade(
            AbstractLoadFileDialog loadFileDialog
        )
        {
            _mapEditorLoadModelActionHandler = new WindowMapEditorLoadModelActionHandler(
                loadFileDialog,
                new WindowMapEditorLoadModelModifier(
                    new ConfigurationMapModelDeserializer(),
                    new MapModelConverterFacade()
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapEditorLoadModelActionHandler.Modifier();
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _mapEditorLoadModelActionHandler.Inject(dataType, data);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapEditorLoadModelActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapEditorLoadMinimapModifierParameters
    {
        public MapModel ElementModel = new MapModel();
    }


    public class WindowMapEditorLoadMinimapModifier : AbstractWindowStateModifier
    {
        private TextBox _textBoxLeft;

        private TextBox _textBoxTop;

        private TextBox _textBoxRight;

        private TextBox _textBoxBottom;

        public WindowMapEditorLoadMinimapModifier(
            TextBox textBoxLeft,
            TextBox textBoxTop,
            TextBox textBoxRight,
            TextBox textBoxBottom
        )
        {
            _textBoxLeft = textBoxLeft;
            _textBoxTop = textBoxTop;
            _textBoxRight = textBoxRight;
            _textBoxBottom = textBoxBottom;
        }

        public override void Modify(object? value)
        {
            if (value is not WindowMapEditorLoadMinimapModifierParameters parameters)
            {
                return;
            }
            var mapArea = parameters.ElementModel.GetMapArea();
            var left = Convert.ToInt32(Math.Min(mapArea.Left, mapArea.Right));
            var top = Convert.ToInt32(Math.Min(mapArea.Top, mapArea.Bottom));
            var right = Convert.ToInt32(Math.Max(mapArea.Left, mapArea.Right));
            var bottom = Convert.ToInt32(Math.Max(mapArea.Top, mapArea.Bottom));
            if (right == left) right++;
            if (bottom == top) bottom++;
            _textBoxLeft.Text = left.ToString();
            _textBoxTop.Text = top.ToString();
            _textBoxRight.Text = right.ToString();
            _textBoxBottom.Text = bottom.ToString();
        }
    }


    public class WindowMapEditorLoadMinimapActionHandler : AbstractWindowActionHandler
    {
        private AbstractLoadFileDialog _loadFileDialog;

        private AbstractWindowStateModifier _mapEditorLoadMinimapModifier;

        private MapModel? _mapModel;

        public WindowMapEditorLoadMinimapActionHandler(
            AbstractLoadFileDialog loadFileDialog,
            AbstractWindowStateModifier mapEditorLoadMinimapModifier
        )
        {
            _loadFileDialog = loadFileDialog;
            _mapEditorLoadMinimapModifier = mapEditorLoadMinimapModifier;
            _loadFileDialog.FileLoaded += OnEvent;
            _mapModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapEditorLoadMinimapModifier;
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

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (e is not FileLoadedEventArgs fileLoadedEventArgs)
            {
                return;
            }
            if (_mapModel == null)
            {
                return;
            }
            _mapEditorLoadMinimapModifier.Modify(
                new WindowMapEditorLoadMinimapModifierParameters
                {
                    ElementModel = _mapModel
                }
            );
        }
    }


    public class WindowMapEditorLoadMinimapActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapEditorLoadMinimapActionHandler;

        public WindowMapEditorLoadMinimapActionHandlerFacade(
            AbstractLoadFileDialog loadFileDialog,
            TextBox textBoxLeft,
            TextBox textBoxTop,
            TextBox textBoxRight,
            TextBox textBoxBottom
        )
        {
            _mapEditorLoadMinimapActionHandler = new WindowMapEditorLoadMinimapActionHandler(
                loadFileDialog,
                new WindowMapEditorLoadMinimapModifier(
                    textBoxLeft,
                    textBoxTop,
                    textBoxRight,
                    textBoxBottom
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapEditorLoadMinimapActionHandler.Modifier();
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _mapEditorLoadMinimapActionHandler.Inject(dataType, data);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapEditorLoadMinimapActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapCanvasLoadedPointFormatter : AbstractMapCanvasFormatter
    {
        private AbstractPointElementInformation _pointElementInformation;

        public WindowMapCanvasLoadedPointFormatter(
            AbstractPointElementInformation pointElementInformation
        )
        {
            _pointElementInformation = pointElementInformation;
        }

        private void _setupLabelText(
            FrameworkElement createdPoint,
            string pointLabel
        )
        {
            var label = _pointElementInformation.Label(createdPoint);
            if (label != null)
            {
                label.Text = pointLabel;
            }
        }

        private void _addLabelTextElement(
            FrameworkElement createdPoint,
            List<FrameworkElement> textDependencies,
            MinimapPoint minimapPoint
        )
        {
            var label = _pointElementInformation.Label(createdPoint);
            minimapPoint.PointData.ElementTexts.Clear();
            if (label != null)
            {
                minimapPoint.PointData.ElementTexts.Add(label);
            }
            foreach (var element in textDependencies)
            {
                minimapPoint.PointData.ElementTexts.Add(element);
            }
        }

        public override void Format(
            FrameworkElement frameworkElement,
            List<FrameworkElement> textDepdendencies,
            object formatData
        )
        {
            if (formatData is not MinimapPoint minimapPoint)
            {
                return;
            }
            frameworkElement.Name = minimapPoint.PointData.ElementName;
            _addLabelTextElement(frameworkElement, textDepdendencies, minimapPoint);
            _setupLabelText(frameworkElement, minimapPoint.PointData.PointName);
        }
    }


    public class WindowMapCanvasLoadedPointDrawerParameters
    {
        public MinimapPoint LoadedPoint = new MinimapPoint();

        public List<FrameworkElement> ElementDependencies = [];
    }


    public class WindowMapCanvasLoadedPointDrawer : AbstractWindowStateModifier
    {
        private Canvas _canvas;

        private AbstractMapCanvasFormatter _mapCanvasLoadedPointFormatter;

        private AbstractMapCanvasElementFactory _mapCanvasElementFactory;

        private FrameworkElement? _createdPoint;

        public WindowMapCanvasLoadedPointDrawer(
            Canvas canvas,
            AbstractMapCanvasFormatter mapCanvasLoadedPointFormatter,
            AbstractMapCanvasElementFactory mapCanvasElementFactory
        )
        {
            _canvas = canvas;
            _mapCanvasLoadedPointFormatter = mapCanvasLoadedPointFormatter;
            _mapCanvasElementFactory = mapCanvasElementFactory;
            _createdPoint = null;
        }

        public override void Modify(object? value)
        {
            if (value is not WindowMapCanvasLoadedPointDrawerParameters parameters)
            {
                return;
            }
            _createdPoint = _mapCanvasElementFactory.Create();
            _canvas.Children.Add(_createdPoint);
            Canvas.SetLeft(_createdPoint, parameters.LoadedPoint.X);
            Canvas.SetTop(_createdPoint, parameters.LoadedPoint.Y);
            _mapCanvasLoadedPointFormatter.Format(
                _createdPoint,
                parameters.ElementDependencies,
                parameters.LoadedPoint
            );
        }
    }


    public class WindowMapEditorLoadedMinimapPointsActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractLoadFileDialog _fileLoadDialog;

        private TextBox _pointLabelTextBox;

        private AbstractWindowStateModifier _mapCanvasLoadedPointDrawer;

        private MapModel? _mapModel;

        public WindowMapEditorLoadedMinimapPointsActionHandler(
            Canvas mapCanvas,
            TextBox pointLabelTextBox,
            AbstractLoadFileDialog fileLoadDialog,
            AbstractWindowStateModifier mapCanvasLoadedPointDrawer
        )
        {
            _mapCanvas = mapCanvas;
            _fileLoadDialog = fileLoadDialog;
            _pointLabelTextBox = pointLabelTextBox;
            _mapCanvasLoadedPointDrawer = mapCanvasLoadedPointDrawer;
            _mapModel = null;
            _fileLoadDialog.FileLoaded += OnEvent;
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
            return _mapCanvasLoadedPointDrawer;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_mapModel == null)
            {
                return;
            }
            var minimapPoints = _mapModel.Points();
            _mapCanvas.Children.Clear();
            foreach (var minimapPoint in minimapPoints)
            {
                _mapCanvasLoadedPointDrawer.Modify(
                    new WindowMapCanvasLoadedPointDrawerParameters
                    {
                        LoadedPoint = minimapPoint,
                        ElementDependencies = [_pointLabelTextBox]
                    }
                );
                _mapModel.Edit(minimapPoint);
            }
        }
    }


    public class WindowMapEditorLoadedMinimapPointsActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapEditorLoadedMinimapPointsActionHandler;

        public WindowMapEditorLoadedMinimapPointsActionHandlerFacade(
            Canvas mapCanvas,
            TextBox pointLabelTextBox,
            AbstractLoadFileDialog fileLoadDialog
        )
        {
            _mapEditorLoadedMinimapPointsActionHandler = new WindowMapEditorLoadedMinimapPointsActionHandler(
                mapCanvas,
                pointLabelTextBox,
                fileLoadDialog,
                new WindowMapCanvasLoadedPointDrawer(
                    mapCanvas,
                    new WindowMapCanvasLoadedPointFormatter(new PointElementInformation()),
                    new WindowMapCanvasPointFactoryFacade()
                )
            );
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _mapEditorLoadedMinimapPointsActionHandler.Inject(dataType, data);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapEditorLoadedMinimapPointsActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapEditorLoadedMinimapPointsActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapEditorLoadedMenuStateModifier : AbstractWindowStateModifier
    {
        private Button _editButton;

        private TextBox _labelTextBox;

        private TextBox _pointLocationX;

        private TextBox _pointLocationY;

        private AbstractWindowMapEditMenuState _menuState;

        public WindowMapEditorLoadedMenuStateModifier(
            Button editButton,
            TextBox labelTextBox,
            TextBox pointLocationX,
            TextBox pointLocationY,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _editButton = editButton;
            _labelTextBox = labelTextBox;
            _pointLocationX = pointLocationX;
            _pointLocationY = pointLocationY;
            _menuState = menuState;
        }

        public override void Modify(object? value)
        {
            _editButton.IsEnabled = false;
            _labelTextBox.Text = "";
            _pointLocationX.Text = "";
            _pointLocationY.Text = "";
            _menuState.Deselect();
        }
    }


    public class WindowMapEditorLoadedMenuStateActionHandler : AbstractWindowActionHandler
    {
        private AbstractLoadFileDialog _loadFileDialog;

        private AbstractWindowStateModifier _mapEditorLoadedMenuStateModifier;

        public WindowMapEditorLoadedMenuStateActionHandler(
            AbstractLoadFileDialog loadFileDialog,
            AbstractWindowStateModifier mapEditorLoadedMenuStateModifier
        )
        {
            _loadFileDialog = loadFileDialog;
            _mapEditorLoadedMenuStateModifier = mapEditorLoadedMenuStateModifier;
            _loadFileDialog.FileLoaded += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapEditorLoadedMenuStateModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapEditorLoadedMenuStateModifier.Modify(null);
        }
    }


    public class WindowMapEditorLoadedMenuStateActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapEditorLoadedMenuStateActionHandler;

        public WindowMapEditorLoadedMenuStateActionHandlerFacade(
            Button editButton,
            TextBox labelTextBox,
            TextBox locationX,
            TextBox locationY,
            AbstractLoadFileDialog loadFileDialog,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _mapEditorLoadedMenuStateActionHandler = new WindowMapEditorLoadedMenuStateActionHandler(
                loadFileDialog,
                new WindowMapEditorLoadedMenuStateModifier(
                    editButton,
                    locationX,
                    locationY,
                    labelTextBox,
                    menuState
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapEditorLoadedMenuStateActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapEditorLoadedMenuStateActionHandler.OnEvent(sender, e);
        }
    }
}
