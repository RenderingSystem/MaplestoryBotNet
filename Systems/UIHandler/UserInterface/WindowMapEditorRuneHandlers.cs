using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public enum WindowMapEditFrameMenuStateTypes
    {
        SelectFrame = 0,
        AddFrame,
        RemoveFrame,
        AddPoint,
        RemovePoint,
        MaxNum
    }


    public class WindowMapEditMenuFrameSelectedObject
    {
        public object FrameObject = new object();

        public object? PointObject = null;

        public Tuple<double, double>? DragPoint = null;
    }


    public class WindowMapAddFrameButtonActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapCanvasAddButtonActionHandler;

        public WindowMapAddFrameButtonActionHandlerFacade(
            ToggleButton addFrameButton,
            List<ToggleButton> radioButtons,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _mapCanvasAddButtonActionHandler = new WindowMapButtonActionHandler(
                addFrameButton,
                new WindowMapButtonModifier(
                    menuState,
                    addFrameButton,
                    radioButtons,
                    (int)WindowMapEditFrameMenuStateTypes.AddFrame,
                    (int)WindowMapEditFrameMenuStateTypes.SelectFrame
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasAddButtonActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasAddButtonActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapRemoveFrameButtonActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapCanvasRemoveButtonActionHandler;

        public WindowMapRemoveFrameButtonActionHandlerFacade(
            ToggleButton removeFrameButton,
            List<ToggleButton> radioButtons,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _mapCanvasRemoveButtonActionHandler = new WindowMapButtonActionHandler(
                removeFrameButton,
                new WindowMapButtonModifier(
                    menuState,
                    removeFrameButton,
                    radioButtons,
                    (int)WindowMapEditFrameMenuStateTypes.RemoveFrame,
                    (int)WindowMapEditFrameMenuStateTypes.SelectFrame
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasRemoveButtonActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasRemoveButtonActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapAddFramePointButtonActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapCanvasAddButtonActionHandler;

        public WindowMapAddFramePointButtonActionHandlerFacade(
            ToggleButton addFrameButton,
            List<ToggleButton> radioButtons,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _mapCanvasAddButtonActionHandler = new WindowMapButtonActionHandler(
                addFrameButton,
                new WindowMapButtonModifier(
                    menuState,
                    addFrameButton,
                    radioButtons,
                    (int)WindowMapEditFrameMenuStateTypes.AddPoint,
                    (int)WindowMapEditFrameMenuStateTypes.SelectFrame
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasAddButtonActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasAddButtonActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapRemoveFramePointButtonActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapCanvasRemoveButtonActionHandler;

        public WindowMapRemoveFramePointButtonActionHandlerFacade(
            ToggleButton removeFrameButton,
            List<ToggleButton> radioButtons,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _mapCanvasRemoveButtonActionHandler = new WindowMapButtonActionHandler(
                removeFrameButton,
                new WindowMapButtonModifier(
                    menuState,
                    removeFrameButton,
                    radioButtons,
                    (int)WindowMapEditFrameMenuStateTypes.RemovePoint,
                    (int)WindowMapEditFrameMenuStateTypes.SelectFrame
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasRemoveButtonActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasRemoveButtonActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapCanvasFrameDrawerParameters
    {
        public Tuple<double, double> ElementPoint = new Tuple<double, double>(0.0, 0.0);
    }


    public class WindowMapCanvasFrameDrawerModifier : AbstractWindowStateModifier
    {
        private Canvas _canvas;

        private AbstractMapCanvasElementFactory _frameFactory;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowMapCanvasFrameDrawerModifier(
            Canvas canvas,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMapCanvasElementFactory frameFactory
        )
        {
            _canvas = canvas;
            _editMenuState = editMenuState;
            _frameFactory = frameFactory;
        }

        public override void Modify(object? value)
        {
            if (value is not WindowMapCanvasFrameDrawerParameters parameters)
            {
                return;
            }
            if (_editMenuState.GetState() == (int)WindowMapEditFrameMenuStateTypes.AddFrame)
            {
                var createdFrame = _frameFactory.Create();
                _canvas.Children.Add(createdFrame);
                Canvas.SetLeft(createdFrame, parameters.ElementPoint.Item1);
                Canvas.SetTop(createdFrame, parameters.ElementPoint.Item2);
                _editMenuState.Select(
                    new WindowMapEditMenuFrameSelectedObject
                    {
                        FrameObject = createdFrame,
                        DragPoint = parameters.ElementPoint,
                        PointObject = null
                    }
                );
                _editMenuState.SetDragging(true);
            }
        }
    }


    public class WindowMapCanvasFrameDrawerActionHandler : AbstractWindowActionHandler
    {
        private Canvas _canvas;

        private AbstractWindowStateModifier _frameDrawer;

        private AbstractMouseEventDataExtractor _mousePositionExtractor;

        public WindowMapCanvasFrameDrawerActionHandler(
            Canvas canvas,
            TextBox frameLabelTextBox,
            AbstractMouseEventDataExtractor mousePositionExtractor,
            AbstractWindowStateModifier frameDrawer
        )
        {
            _canvas = canvas;
            _mousePositionExtractor = mousePositionExtractor;
            _frameDrawer = frameDrawer;
            _canvas.MouseLeftButtonDown += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameDrawer;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (e is not MouseButtonEventArgs mouseButtonEventArgs)
            {
                return;
            }
            var position = _mousePositionExtractor.GetPosition(
                mouseButtonEventArgs, _canvas
            );
            _frameDrawer.Modify(
                new WindowMapCanvasFrameDrawerParameters
                {
                    ElementPoint = new Tuple<double, double>(position.X, position.Y)
                }
            );
        }
    }


    public class WindowMapCanvasFrameDrawerActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapCanvasAddFrameActionHandler;

        public WindowMapCanvasFrameDrawerActionHandlerFacade(
            Canvas mapCanvas,
            TextBox frameLabelTextBox,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _mapCanvasAddFrameActionHandler = new WindowMapCanvasFrameDrawerActionHandler(
                mapCanvas,
                frameLabelTextBox,
                mousePositionExtractor,
                new WindowMapCanvasFrameDrawerModifier(
                    mapCanvas,
                    editMenuState,
                    new WindowMapCanvasFrameFactoryFacade()
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasAddFrameActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasAddFrameActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _mapCanvasAddFrameActionHandler.Inject(dataType, data);
        }
    }


    public class WindowMapCanvasFrameSelectStateModifierParameters
    {
        public Point MousePosition = new Point();

        public bool LeftMouseDown = false;
    }


    public class WindowMapCanvasFrameSelectStateModifier : AbstractWindowStateModifier
    {
        private Canvas _mapCanvas;

        private AbstractWindowMapEditMenuState _editMenuState;

        private Canvas? _locatedFrame;

        private FrameworkElement? _locatedPoint;

        private FrameworkElement? _locatedGrip;

        public WindowMapCanvasFrameSelectStateModifier(
            Canvas mapCanvas, AbstractWindowMapEditMenuState editMenuState
        )
        {
            _mapCanvas = mapCanvas;
            _editMenuState = editMenuState;
            _locatedFrame = null;
            _locatedPoint = null;
            _locatedGrip = null;
        }

        private HitTestResultBehavior _locateTargetsCallback(HitTestResult result)
        {
            var current = result.VisualHit;
            var gripNames = WindowMapCanvasFrameTypes.GripNames();
            var oppositeNames = WindowMapCanvasFrameTypes.OppositeNames();
            if (current is FrameworkElement element)
            {
                if (
                    element.Parent is Canvas dragBody
                    && dragBody.Name == WindowMapCanvasFrameTypes.CANVAS
                )
                {
                    if (_locatedFrame == null)
                    {
                        _locatedFrame = dragBody;
                        _locatedGrip = null;
                        _locatedPoint = null;
                    }
                    if (gripNames.IndexOf(element.Name) is int gripIndex && gripIndex != -1)
                    {
                        foreach (var child in dragBody.Children)
                        {
                            if (child is not FrameworkElement oppositeGrip)
                            {
                                continue;
                            }
                            if (oppositeGrip.Name != oppositeNames[gripIndex])
                            {
                                continue;
                            }
                            _locatedFrame = dragBody;
                            _locatedGrip = oppositeGrip;
                            _locatedPoint = null;
                            return HitTestResultBehavior.Stop;
                        }
                    }
                }
                if (
                    element.Parent is Canvas pointBody
                    && pointBody.Parent is Canvas pointFrame
                    && pointBody.Name == WindowMapCanvasFrameTypes.POINT
                    && pointFrame.Name == WindowMapCanvasFrameTypes.CANVAS
                )
                {
                    _locatedFrame = pointFrame;
                    _locatedGrip = null;
                    _locatedPoint = pointBody;
                    return HitTestResultBehavior.Stop;
                }
            }
            return HitTestResultBehavior.Continue;
        }

        private void _locateTargets(Point mousePoint)
        {
            _locatedFrame = null;
            _locatedGrip = null;
            _locatedPoint = null;
            VisualTreeHelper.HitTest(
                _mapCanvas,
                null,
                new HitTestResultCallback(_locateTargetsCallback),
                new PointHitTestParameters(mousePoint)
            );
        }

        private WindowMapEditMenuFrameSelectedObject? _locateSelectObject(Point mousePoint)
        {
            _locateTargets(mousePoint);
            return _locatedFrame != null ? (
                new WindowMapEditMenuFrameSelectedObject
                {
                    FrameObject = _locatedFrame,
                    PointObject = _locatedPoint,
                    DragPoint = _locatedGrip != null ? (
                        new Tuple<double, double>(
                            Canvas.GetLeft(_locatedFrame)
                            + _locatedGrip.RenderTransform.Value.OffsetX
                            + _locatedGrip.Width / 2,
                            Canvas.GetTop(_locatedFrame)
                            + _locatedGrip.RenderTransform.Value.OffsetY
                            + _locatedGrip.Height / 2
                        )
                    ) : null
                }
            ) : null;
        }

        public override void Modify(object? value)
        {
            if (value is not WindowMapCanvasFrameSelectStateModifierParameters parameters)
            {
                return;
            }
            if (
                _editMenuState.GetState() == (int)WindowMapEditFrameMenuStateTypes.SelectFrame
                && parameters.LeftMouseDown
                && !_editMenuState.Dragging()
            )
            {
                var selectObject = _locateSelectObject(parameters.MousePosition);
                _editMenuState.Select(selectObject);
                _editMenuState.SetDragging(
                    (selectObject != null)
                    && (selectObject.DragPoint != null || selectObject.PointObject != null)
                );
            }
            else if (!parameters.LeftMouseDown)
            {
                _editMenuState.SetDragging(false);
            }
        }
    }


    public class WindowMapCanvasFrameSelectStateActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractMouseEventDataExtractor _mouseDataExtractor;

        private AbstractWindowStateModifier _mapCanvasFrameSelectStateModifier;

        public WindowMapCanvasFrameSelectStateActionHandler(
            Canvas mapCanvas,
            AbstractMouseEventDataExtractor mousePositionExtractor,
            AbstractWindowStateModifier mapCanvasFrameSelectStateModifier
        )
        {
            _mapCanvas = mapCanvas;
            _mouseDataExtractor = mousePositionExtractor;
            _mapCanvasFrameSelectStateModifier = mapCanvasFrameSelectStateModifier;
            _mapCanvas.MouseLeftButtonDown += OnEvent;
            _mapCanvas.MouseLeftButtonUp += OnEvent;
            EventManager.RegisterClassHandler(
                typeof(UIElement),
                UIElement.MouseLeftButtonUpEvent,
                new MouseButtonEventHandler(OnEvent)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasFrameSelectStateModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (e is not MouseButtonEventArgs mouseEventArgs)
            {
                return;
            }
            var mousePosition = _mouseDataExtractor.GetPosition(mouseEventArgs, _mapCanvas);
            var mouseButtonState = _mouseDataExtractor.GetButtonState(mouseEventArgs.LeftButton);
            _mapCanvasFrameSelectStateModifier.Modify(
                new WindowMapCanvasFrameSelectStateModifierParameters
                {
                    MousePosition = mousePosition,
                    LeftMouseDown = mouseButtonState == MouseButtonState.Pressed
                }
            );
        }
    }


    public class WindowMapCanvasFrameSelectStateActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapCanvasFrameSelectStateActionHandler;

        public WindowMapCanvasFrameSelectStateActionHandlerFacade(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _mapCanvasFrameSelectStateActionHandler = new WindowMapCanvasFrameSelectStateActionHandler(
                mapCanvas,
                mousePositionExtractor,
                new WindowMapCanvasFrameSelectStateModifier(mapCanvas, editMenuState)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasFrameSelectStateActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapCanvasFrameSelectStateActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapCanvasFrameFormatterData
    {
        public Canvas DragObject = new Canvas();

        public Point DragPoint = new Point();

        public Point CurrentPoint = new Point();
    }


    public class WindowMapCanvasFrameDragFormatter : AbstractMapCanvasFormatter
    {
        private Tuple<double, double, double, double> _calculateRectangle(
            Canvas mapCanvas, WindowMapCanvasFrameFormatterData formatterData
        )
        {
            var currPoint = formatterData.CurrentPoint;
            var dragPoint = formatterData.DragPoint;
            var left = Math.Max(0, Math.Min(currPoint.X, dragPoint.X));
            var top = Math.Max(0, Math.Min(currPoint.Y, dragPoint.Y));
            var right = Math.Min(mapCanvas.Width, Math.Max(currPoint.X, dragPoint.X));
            var bottom = Math.Min(mapCanvas.Height, Math.Max(currPoint.Y, dragPoint.Y));
            var width = right - left;
            var height = bottom - top;
            return new Tuple<double, double, double, double>(left, top, width, height);
        }

        private void _formatDragObjectDimensions(
            Canvas dragObject, Tuple<double, double, double, double> rectangle
        )
        {
            Canvas.SetLeft(dragObject, rectangle.Item1);
            Canvas.SetTop(dragObject, rectangle.Item2);
            dragObject.Width = rectangle.Item3;
            dragObject.Height = rectangle.Item4;
        }

        private void _formatChildren(
            Canvas dragObject, Tuple<double, double, double, double> rectangle
        )
        {
            foreach (var child in dragObject.Children)
            {
                if (child is Rectangle rectangleShape)
                {
                    rectangleShape.Width = rectangle.Item3;
                    rectangleShape.Height = rectangle.Item4;
                }
                else if (child is Ellipse ellipse)
                {
                    var wRadius = ellipse.Width / 2;
                    var hRadius = ellipse.Height / 2;
                    ellipse.RenderTransform = (
                        ellipse.Name == WindowMapCanvasFrameTypes.TL ?
                            new TranslateTransform(-wRadius, -hRadius) :
                        ellipse.Name == WindowMapCanvasFrameTypes.TR ?
                            new TranslateTransform(rectangle.Item3 - wRadius, -hRadius) :
                        ellipse.Name == WindowMapCanvasFrameTypes.BL ?
                            new TranslateTransform(-wRadius, rectangle.Item4 - hRadius) :
                        ellipse.Name == WindowMapCanvasFrameTypes.BR ?
                            new TranslateTransform(rectangle.Item3 - wRadius, rectangle.Item4 - hRadius) :
                        ellipse.RenderTransform
                    );
                }
                else if (child is TextBlock textBlock)
                {
                    textBlock.UpdateLayout();
                    Canvas.SetLeft(textBlock, 5);
                    Canvas.SetTop(textBlock, textBlock.ActualHeight + 5);
                    textBlock.Visibility = Visibility.Visible;
                }
            }
        }

        public override void Format(
            FrameworkElement canvas,
            List<FrameworkElement> textDepdendencies,
            object formatData
        )
        {
            if (
                canvas is Canvas mapCanvas
                && formatData is WindowMapCanvasFrameFormatterData frameFormatterData
            )
            {
                var rectangle = _calculateRectangle(mapCanvas, frameFormatterData);
                _formatDragObjectDimensions(frameFormatterData.DragObject, rectangle);
                _formatChildren(frameFormatterData.DragObject, rectangle);
            }
        }
    }


    public class WindowMapCanvasFrameDragModifier : AbstractWindowStateModifier
    {
        private Canvas _mapCanvas;

        private AbstractWindowMapEditMenuState _editMenuState;

        private AbstractMapCanvasFormatter _mapCanvasFrameFormatter;

        public WindowMapCanvasFrameDragModifier(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMapCanvasFormatter mapCanvasFrameFormatter
        )
        {
            _mapCanvas = mapCanvas;
            _editMenuState = editMenuState;
            _mapCanvasFrameFormatter = mapCanvasFrameFormatter;
        }

        public override void Modify(object? value)
        {
            if (
                value is Point currPoint
                && _editMenuState.Dragging()
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject frameSelected
                && frameSelected.FrameObject is Canvas dragObject
                && frameSelected.DragPoint is Tuple<double, double> dragPoint
            )
            {
                _mapCanvasFrameFormatter.Format(
                    _mapCanvas,
                    [],
                    new WindowMapCanvasFrameFormatterData
                    {
                        DragObject = dragObject,
                        DragPoint = new Point(dragPoint.Item1, dragPoint.Item2),
                        CurrentPoint = currPoint
                    }
                );
            }
        }
    }


    public class WindowMapCanvasFrameDragActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractWindowStateModifier _mapCanvasFrameDragModifier;

        private AbstractMouseEventDataExtractor _mousePositionExtractor;

        public WindowMapCanvasFrameDragActionHandler(
            Canvas mapCanvas,
            AbstractWindowStateModifier mapCanvasFrameDragModifier,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _mapCanvas = mapCanvas;
            _mapCanvasFrameDragModifier = mapCanvasFrameDragModifier;
            _mapCanvas.MouseMove += OnEvent;
            _mousePositionExtractor = mousePositionExtractor;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapCanvasFrameDragModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (e is MouseEventArgs mouseEventArgs)
            {
                var position = _mousePositionExtractor.GetPosition(mouseEventArgs, _mapCanvas);
                _mapCanvasFrameDragModifier.Modify(position);
            }
        }
    }


    public class WindowMapCanvasFrameDragActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _frameDragActionHandler;

        public WindowMapCanvasFrameDragActionHandlerFacade(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _frameDragActionHandler = new WindowMapCanvasFrameDragActionHandler(
                mapCanvas,
                new WindowMapCanvasFrameDragModifier(
                    mapCanvas, editMenuState, new WindowMapCanvasFrameDragFormatter()
                ),
                mousePositionExtractor
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameDragActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _frameDragActionHandler.OnEvent(sender, e);
        }
    }


    public class MapCanvasRuneFrameDataTag
    {
        public string FrameName = "";

        public string ElementLabel = "";
    }


    public class FrameworkElementInformation : AbstractFrameworkElementInformation
    {
        public override Rect BoundingRect(FrameworkElement frameworkElement)
        {
            if (frameworkElement is not Canvas canvasElement)
            {
                return new Rect(0.0, 0.0, 0.0, 0.0);
            }
            var left = Canvas.GetLeft(canvasElement);
            var top = Canvas.GetTop(canvasElement);
            var width = canvasElement.Width;
            var height = canvasElement.Height;
            return new Rect(left, top, width, height);
        }

        public override TextBlock? Label(FrameworkElement frameworkElement)
        {
            if (
                frameworkElement is Canvas canvasElement
                && canvasElement.Children.OfType<TextBlock>() is IEnumerable<TextBlock> textBlocks
                && textBlocks.FirstOrDefault() is TextBlock textBlock
            )
            {
                return textBlock;
            }
            return null;
        }
    }


    public class MapCanvasFrameDataFormatter : AbstractMapCanvasFormatter
    {
        private AbstractFrameworkElementInformation _frameElementInfo;

        public MapCanvasFrameDataFormatter(
            AbstractFrameworkElementInformation frameElementInfo
        )
        {
            _frameElementInfo = frameElementInfo;
        }

        private void _setupLabelText(
            FrameworkElement createdFrame,
            string frameLabel
        )
        {
            var label = _frameElementInfo.Label(createdFrame);
            if (label != null)
            {
                label.Text = frameLabel;
            }
        }

        private RuneFrameData _minimapFrameData(
            FrameworkElement createdFrame,
            List<FrameworkElement> textDependencies,
            string frameName,
            string elementLabel
        )
        {
            createdFrame.Tag = new MapCanvasRuneFrameDataTag
            {
                FrameName = frameName,
                ElementLabel = elementLabel,
            };
            return new RuneFrameData
            {
                ElementTexts = new List<FrameworkElement>(textDependencies)
                {
                    _frameElementInfo.Label(createdFrame)!
                },
                FrameName = frameName,
                ElementLabel = elementLabel,
                RuneFrameMacros = [],
                RuneFrameDirections = [],
            };
        }

        private void _setupMinimapFrame(
            FrameworkElement createdFrame,
            List<FrameworkElement> textDependencies,
            AbstractRuneModel runeModel,
            string frameLabel,
            string elementLabel
        )
        {
            if (createdFrame is Canvas canvasElement)
            {
                var frameData = _minimapFrameData(
                    canvasElement,
                    textDependencies,
                    frameLabel,
                    elementLabel
                );
                var minimapPoint = new RuneFrame
                {
                    X = Canvas.GetLeft(canvasElement),
                    Y = Canvas.GetTop(canvasElement),
                    Width = 0.0,
                    Height = 0.0,
                    FrameData = frameData
                };
                runeModel.AddRuneFrame(minimapPoint);
            }
        }

        private string _generateElementLabel(AbstractRuneModel runeModel)
        {
            var runeFrames = runeModel.RuneFrames();
            var elementCount = runeFrames.Count;
            var existingElements = new HashSet<string>(
                runeFrames.Select(f => f.FrameData.ElementLabel)
            );
            while (existingElements.Contains("FT" + elementCount))
            {
                elementCount++;
            }
            return "FT" + elementCount;
        }

        private string _generateFrameName(AbstractRuneModel runeModel)
        {
            var mapPoints = runeModel.RuneFrames();
            var frameCount = mapPoints.Count;
            var existingNames = new HashSet<string>(
                mapPoints.Select(p => p.FrameData.FrameName)
            );
            while (existingNames.Contains("F" + frameCount))
            {
                frameCount++;
            }
            return "F" + frameCount;
        }

        public override void Format(
            FrameworkElement createdFrame,
            List<FrameworkElement> textDependencies,
            object formatData
        )
        {
            if (formatData is not AbstractBottingModel bottingModel)
            {
                return;
            }
            var runeModel = bottingModel.GetRuneModel();
            var frameName = _generateFrameName(runeModel);
            var elementLabel = _generateElementLabel(runeModel);
            _setupMinimapFrame(
                createdFrame,
                textDependencies,
                runeModel,
                frameName,
                elementLabel
            );
            _setupLabelText(
                createdFrame,
                frameName
            );
        }
    }


    public class WindowMapCanvasFrameDataModifier : AbstractWindowStateModifier
    {
        private AbstractWindowMapEditMenuState _editMenuState;

        private TextBox _frameLabelTextBox;

        private AbstractMapCanvasFormatter _frameDataFormatter;

        public WindowMapCanvasFrameDataModifier(
            TextBox frameLabelTextBox,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMapCanvasFormatter frameDataFormatter
        )
        {
            _frameLabelTextBox = frameLabelTextBox;
            _editMenuState = editMenuState;
            _frameDataFormatter = frameDataFormatter;
        }

        public override void Modify(object? value)
        {
            if (
                value is AbstractBottingModel bottingModel
                && _editMenuState.GetState() is int editMenuState
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && editMenuState == (int)WindowMapEditFrameMenuStateTypes.AddFrame
                && selectedObject.FrameObject is FrameworkElement selectedFrame
            )
            {
                _frameDataFormatter.Format(selectedFrame, [_frameLabelTextBox], bottingModel);
            }
        }
    }


    public class WindowMapCanvasFrameDataActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractWindowStateModifier _frameDataAdder;

        private AbstractBottingModel? _bottingModel;

        public WindowMapCanvasFrameDataActionHandler(
            Canvas mapCanvas,
            AbstractWindowStateModifier frameDataAdder
        )
        {
            _mapCanvas = mapCanvas;
            _frameDataAdder = frameDataAdder;
            _mapCanvas.MouseLeftButtonDown += OnEvent;
            _bottingModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameDataAdder;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_bottingModel != null)
            {
                _frameDataAdder.Modify(_bottingModel);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public class WindowMapCanvasFrameDataActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _frameDataActionHandler;

        public WindowMapCanvasFrameDataActionHandlerFacade(
            Canvas mapCanvas,
            TextBox frameLabelTextBox,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _frameDataActionHandler = new WindowMapCanvasFrameDataActionHandler(
                mapCanvas,
                new WindowMapCanvasFrameDataModifier(
                    frameLabelTextBox,
                    editMenuState,
                    new MapCanvasFrameDataFormatter(new FrameworkElementInformation())
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameDataActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _frameDataActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _frameDataActionHandler.Inject(dataType, data);
        }
    }


    public class WindowMapCanvasFrameSelectedTextModifier : AbstractWindowStateModifier
    {
        private TextBox _selectedTextLabel;

        private TextBox _selectedTextLeft;

        private TextBox _selectedTextTop;

        private TextBox _selectedTextRight;

        private TextBox _selectedTextBottom;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowMapCanvasFrameSelectedTextModifier(
            TextBox selectedTextLabel,
            TextBox selectedTextLeft,
            TextBox selectedTextTop,
            TextBox selectedTextRight,
            TextBox selectedTextBottom,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _selectedTextLabel = selectedTextLabel;
            _selectedTextLeft = selectedTextLeft;
            _selectedTextTop = selectedTextTop;
            _selectedTextRight = selectedTextRight;
            _selectedTextBottom = selectedTextBottom;
            _editMenuState = editMenuState;
        }

        private void _assignUITexts(FrameworkElement element)
        {
            if (element.Tag is MapCanvasRuneFrameDataTag selectedTag)
            {
                _editMenuState.SetEditingText(true);
                _selectedTextLabel.Text = selectedTag.FrameName;
                var elementLeft = Canvas.GetLeft(element);
                var elementTop = Canvas.GetTop(element);
                _selectedTextLeft.Text = Math.Round(elementLeft).ToString();
                _selectedTextTop.Text = Math.Round(elementTop).ToString();
                _selectedTextRight.Text = Math.Round(elementLeft + element.Width).ToString();
                _selectedTextBottom.Text = Math.Round(elementTop + element.Height).ToString();
                _editMenuState.SetEditingText(false);
            }
        }

        private void _clearUITexts()
        {
            _editMenuState.SetEditingText(true);
            _selectedTextLabel.Text = "";
            _selectedTextLeft.Text = "";
            _selectedTextTop.Text = "";
            _selectedTextRight.Text = "";
            _selectedTextBottom.Text = "";
            _editMenuState.SetEditingText(false);
        }

        public override void Modify(object? value)
        {
            if (value is not bool clicked)
            {
                return;
            }
            var selected = _editMenuState.Selected();
            if (
                selected is WindowMapEditMenuFrameSelectedObject selectedObject
                && selectedObject.FrameObject is FrameworkElement selectedFrame
            )
            {
                if (clicked || _editMenuState.Dragging())
                {
                    _assignUITexts(selectedFrame);
                }
            }
            else if (selected == null)
            {
                _clearUITexts();
            }
        }
    }


    public class WindowMapCanvasFrameSelectedTextActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractWindowStateModifier _frameSelectedTextModifier;

        public WindowMapCanvasFrameSelectedTextActionHandler(
            Canvas mapCanvas,
            AbstractWindowStateModifier frameSelectedTextModifier
        )
        {
            _mapCanvas = mapCanvas;
            _frameSelectedTextModifier = frameSelectedTextModifier;
            _mapCanvas.MouseLeftButtonDown += OnEvent;
            _mapCanvas.MouseMove += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameSelectedTextModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _frameSelectedTextModifier.Modify(e is MouseButtonEventArgs);
        }
    }


    public class WindowMapCanvasFrameSelectedTextActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _frameSelectedTextActionHandler;

        public WindowMapCanvasFrameSelectedTextActionHandlerFacade(
            Canvas mapCanvas,
            TextBox selectedTextLabel,
            TextBox selectedTextLeft,
            TextBox selectedTextTop,
            TextBox selectedTextRight,
            TextBox selectedTextBottom,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _frameSelectedTextActionHandler = new WindowMapCanvasFrameSelectedTextActionHandler(
                mapCanvas,
                new WindowMapCanvasFrameSelectedTextModifier(
                    selectedTextLabel,
                    selectedTextLeft,
                    selectedTextTop,
                    selectedTextRight,
                    selectedTextBottom,
                    editMenuState
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameSelectedTextActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _frameSelectedTextActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapCanvasFrameButtonAccessModifier : AbstractWindowStateModifier
    {
        private List<ButtonBase> _frameButtons = [];

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowMapCanvasFrameButtonAccessModifier(
            List<ButtonBase> frameButtons,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _frameButtons = frameButtons;
            _editMenuState = editMenuState;
        }

        public override void Modify(object? value)
        {
            var selected = (
                _editMenuState.Selected()
                is WindowMapEditMenuFrameSelectedObject
            );
            foreach (var button in _frameButtons)
            {
                button.IsEnabled = selected;
            }
        }
    }


    public class WindowMapCanvasFrameButtonAccessActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractWindowStateModifier _framePointAccessModifier;

        public WindowMapCanvasFrameButtonAccessActionHandler(
            Canvas mapCanvas,
            AbstractWindowStateModifier framePointAccessModifier
        )
        {
            _mapCanvas = mapCanvas;
            _mapCanvas.MouseLeftButtonDown += OnEvent;
            _framePointAccessModifier = framePointAccessModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointAccessModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointAccessModifier.Modify(null);
        }
    }


    public class WindowMapCanvasFrameButtonAccessActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointAccessActionHandler;

        public WindowMapCanvasFrameButtonAccessActionHandlerFacade(
            Canvas mapCanvas,
            List<ButtonBase> accessButtons,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _framePointAccessActionHandler = new WindowMapCanvasFrameButtonAccessActionHandler(
                mapCanvas,
                new WindowMapCanvasFrameButtonAccessModifier(
                    accessButtons, editMenuState
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointAccessActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointAccessActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapCanvasFrameSelectedDragDataModifier : AbstractWindowStateModifier
    {
        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowMapCanvasFrameSelectedDragDataModifier(
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _editMenuState = editMenuState;
        }

        public override void Modify(object? value)
        {
            if (
                value is AbstractBottingModel bottingModel
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && _editMenuState.Dragging()
                && selectedObject.FrameObject is FrameworkElement selectedFrame
                && selectedFrame.Tag is MapCanvasRuneFrameDataTag tag
                && bottingModel.GetRuneModel() is AbstractRuneModel runeModel
                && runeModel.FindRuneFrameByName(tag.ElementLabel) is RuneFrame runeFrame
            )
            {
                runeFrame.X = Canvas.GetLeft(selectedFrame);
                runeFrame.Y = Canvas.GetTop(selectedFrame);
                runeFrame.Width = selectedFrame.Width;
                runeFrame.Height = selectedFrame.Height;
                runeModel.EditRuneFrame(runeFrame);
            }
        }
    }


    public class WindowMapCanvasFrameSelectedDragDataActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractWindowStateModifier _frameSelectedDragDataModifier;

        private AbstractBottingModel? _bottingModel;

        public WindowMapCanvasFrameSelectedDragDataActionHandler(
            Canvas mapCanvas,
            AbstractWindowStateModifier frameSelectedDragDataModifier
        )
        {
            _mapCanvas = mapCanvas;
            _frameSelectedDragDataModifier = frameSelectedDragDataModifier;
            _mapCanvas.MouseMove += OnEvent;
            _bottingModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameSelectedDragDataModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_bottingModel != null)
            {
                _frameSelectedDragDataModifier.Modify(_bottingModel);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public class WindowMapCanvasFrameSelectedDragDataActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _frameSelectedDragDataActionHandler;

        public WindowMapCanvasFrameSelectedDragDataActionHandlerFacade(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _frameSelectedDragDataActionHandler = new WindowMapCanvasFrameSelectedDragDataActionHandler(
                mapCanvas,
                new WindowMapCanvasFrameSelectedDragDataModifier(editMenuState)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameSelectedDragDataActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _frameSelectedDragDataActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _frameSelectedDragDataActionHandler.Inject(dataType, data);
        }
    }


    public class WindowMapCanvasFrameRemoveParameters
    {
        public Point RemovePosition = new Point();

        public AbstractBottingModel BottingModel = new BottingModel();
    }


    public class WindowMapCanvasFrameRemoveModifier : AbstractWindowStateModifier
    {
        private Canvas _mapCanvas;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowMapCanvasFrameRemoveModifier(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _mapCanvas = mapCanvas;
            _editMenuState = editMenuState;
        }

        private Canvas? _findFrameAtPosition(Point position)
        {
            Canvas? foundFrame = null;
            VisualTreeHelper.HitTest(
                _mapCanvas,
                null,
                new HitTestResultCallback(
                    result =>
                    {
                        var current = result.VisualHit;
                        while (current != null && current != _mapCanvas)
                        {
                            if (
                                current is Canvas canvas
                                && canvas.Parent == _mapCanvas
                                && canvas.Name == WindowMapCanvasFrameTypes.CANVAS)
                            {
                                foundFrame = canvas;
                                return HitTestResultBehavior.Stop;
                            }
                            current = VisualTreeHelper.GetParent(current);
                        }
                        return HitTestResultBehavior.Continue;
                    }
                ),
                new PointHitTestParameters(position)
            );
            return foundFrame;
        }

        private void _deselectRemovedFrame(Canvas removeFrame)
        {
            if (
                _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && selectedObject.FrameObject == removeFrame
            )
            {
                _editMenuState.Select(null);
            }
        }

        public override void Modify(object? value)
        {
            if (
                value is WindowMapCanvasFrameRemoveParameters parameters
                && parameters.RemovePosition is Point removePosition
                && parameters.BottingModel is AbstractBottingModel bottingModel
                && _editMenuState.GetState() == (int)WindowMapEditFrameMenuStateTypes.RemoveFrame
                && _findFrameAtPosition(removePosition) is Canvas removeFrame
                && removeFrame.Tag is MapCanvasRuneFrameDataTag tag
            )
            {
                bottingModel.GetRuneModel().RemoveRuneFrameByName(tag.ElementLabel);
                _mapCanvas.Children.Remove(removeFrame);
                _deselectRemovedFrame(removeFrame);
            }
        }
    }


    public class WindowMapCanvasFrameRemoveActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractWindowStateModifier _frameRemoveModifier;

        private AbstractMouseEventDataExtractor _mousePositionExtractor;

        private AbstractBottingModel? _bottingModel;

        public WindowMapCanvasFrameRemoveActionHandler(
            Canvas mapCanvas,
            AbstractMouseEventDataExtractor mousePositionExtractor,
            AbstractWindowStateModifier frameRemoveModifier
        )
        {
            _mapCanvas = mapCanvas;
            _frameRemoveModifier = frameRemoveModifier;
            _mousePositionExtractor = mousePositionExtractor;
            _bottingModel = null;
            _mapCanvas.MouseLeftButtonDown += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameRemoveModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (e is MouseButtonEventArgs mouseButtonEventArgs)
            {
                _frameRemoveModifier.Modify(
                    new WindowMapCanvasFrameRemoveParameters
                    {
                        RemovePosition = _mousePositionExtractor.GetPosition(mouseButtonEventArgs, _mapCanvas),
                        BottingModel = _bottingModel!
                    }
                );
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public class WindowMapCanvasFrameRemoveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _frameRemoveActionHandler;

        public WindowMapCanvasFrameRemoveActionHandlerFacade(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _frameRemoveActionHandler = new WindowMapCanvasFrameRemoveActionHandler(
                mapCanvas,
                mousePositionExtractor,
                new WindowMapCanvasFrameRemoveModifier(mapCanvas, editMenuState)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameRemoveActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _frameRemoveActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _frameRemoveActionHandler.Inject(dataType, data);
        }
    }


    public class WindowMapCanvasFramePointDrawerParameters
    {
        public Tuple<double, double> ElementPoint = new Tuple<double, double>(0.0, 0.0);

        public AbstractBottingModel ElementModel = new BottingModel();
    }


    public class WindowMapCanvasFramePointDrawerModifier : AbstractWindowStateModifier
    {
        private AbstractMapCanvasElementFactory _framePointFactory;

        private AbstractWindowMapEditMenuState _editMenuState;

        private AbstractFrameworkElementInformation _frameworkElementInfo;

        public WindowMapCanvasFramePointDrawerModifier(
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMapCanvasElementFactory frameFactory,
            AbstractFrameworkElementInformation frameworkElementInfo
        )
        {
            _editMenuState = editMenuState;
            _framePointFactory = frameFactory;
            _frameworkElementInfo = frameworkElementInfo;
        }

        private string _generateMacroName(RuneFrame runeFrame)
        {
            var runeFrameMacros = runeFrame.FrameData.RuneFrameMacros;
            var elementCount = runeFrameMacros.Count;
            var existingElements = new HashSet<string>(
                runeFrameMacros.Select(f => f.MacroName)
            );
            while (existingElements.Contains("M" + elementCount))
            {
                elementCount++;
            }
            return "M" + elementCount;
        }

        private string _generateElementName(RuneFrame runeFrame)
        {
            var runeFrameMacros = runeFrame.FrameData.RuneFrameMacros;
            var elementCount = runeFrameMacros.Count;
            var existingElements = new HashSet<string>(
                runeFrameMacros.Select(f => f.ElementLabel)
            );
            while (existingElements.Contains("MT" + elementCount))
            {
                elementCount++;
            }
            return "MT" + elementCount;
        }

        private void _assignMacroTags(
            FrameworkElement createdFramePoint,
            string macroName,
            string elementLabel
        )
        {
            if (_frameworkElementInfo.Label(createdFramePoint) is TextBlock textBlock)
            {
                textBlock.Text = macroName;
                createdFramePoint.Tag = elementLabel;
            }
        }

        private void _addRuneFrameMacro(
            Canvas selectedFrame,
            AbstractRuneModel runeModel,
            RuneFrame runeFrame,
            string macroName,
            string elementLabel,
            double left,
            double top
        )
        {
            runeFrame.FrameData.RuneFrameMacros.Add(
                new RuneFrameMacro
                {
                    MacroName = macroName,
                    ElementLabel = elementLabel,
                    X = left,
                    Y = top,
                    ScaleX = selectedFrame.Width,
                    ScaleY = selectedFrame.Height,
                    NextRuneFrame = null,
                    Radius = 0.0,
                    PointCommands = [],
                }
            );
            runeModel.EditRuneFrame(runeFrame);
        }

        public override void Modify(object? value)
        {
            if (
                value is WindowMapCanvasFramePointDrawerParameters parameters
                && _editMenuState.GetState() == (int)WindowMapEditFrameMenuStateTypes.AddPoint
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && selectedObject.FrameObject is Canvas selectedFrame
                && selectedFrame.Tag is MapCanvasRuneFrameDataTag runeFrameTag
                && parameters.ElementModel.GetRuneModel() is AbstractRuneModel runeModel
                && runeModel.FindRuneFrameByName(runeFrameTag.ElementLabel) is RuneFrame runeFrame
            )
            {
                var left = (
                    parameters.ElementPoint.Item1 - Canvas.GetLeft(selectedFrame)
                );
                var top = (
                    parameters.ElementPoint.Item2 - Canvas.GetTop(selectedFrame)
                );
                if (left > 0 && left < selectedFrame.Width && top > 0 && top < selectedFrame.Height)
                {
                    var createdFramePoint = (Canvas)_framePointFactory.Create();
                    selectedFrame.Children.Add(createdFramePoint);
                    Canvas.SetLeft(createdFramePoint, left);
                    Canvas.SetTop(createdFramePoint, top);
                    var macroName = _generateMacroName(runeFrame);
                    var elementLabel = _generateElementName(runeFrame);
                    _assignMacroTags(
                        createdFramePoint,
                        macroName,
                        elementLabel
                    );
                    _addRuneFrameMacro(
                        selectedFrame,
                        runeModel,
                        runeFrame,
                        macroName,
                        elementLabel,
                        left,
                        top
                    );
                    _editMenuState.Select(
                        new WindowMapEditMenuFrameSelectedObject
                        {
                            FrameObject = selectedFrame,
                            DragPoint = null,
                            PointObject = createdFramePoint
                        }
                    );
                    _editMenuState.SetDragging(true);
                }
            }
        }
    }


    public class WindowMapCanvasFramePointDrawerActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractWindowStateModifier _framePointDrawerModifier;

        private AbstractMouseEventDataExtractor _mousePositionExtractor;

        private AbstractBottingModel? _bottingModel;

        public WindowMapCanvasFramePointDrawerActionHandler(
            Canvas mapCanvas,
            AbstractWindowStateModifier framePointDrawerModifier,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _mapCanvas = mapCanvas;
            _framePointDrawerModifier = framePointDrawerModifier;
            _mousePositionExtractor = mousePositionExtractor;
            _mapCanvas.MouseLeftButtonDown += OnEvent;
            _bottingModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointDrawerModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is MouseButtonEventArgs mouseButtonEventArgs
                && _bottingModel is AbstractBottingModel bottingModel
            )
            {
                var position = _mousePositionExtractor.GetPosition(
                    mouseButtonEventArgs, _mapCanvas
                );
                _framePointDrawerModifier.Modify(
                    new WindowMapCanvasFramePointDrawerParameters
                    {
                        ElementModel = bottingModel,
                        ElementPoint = new Tuple<double, double>(position.X, position.Y)
                    }
                );
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.BottingModel
                && data is AbstractBottingModel bottingModel
            )
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public class WindowMapCanvasFramePointDrawerActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointDrawerActionHandler;

        public WindowMapCanvasFramePointDrawerActionHandlerFacade(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _framePointDrawerActionHandler = new WindowMapCanvasFramePointDrawerActionHandler(
                mapCanvas,
                new WindowMapCanvasFramePointDrawerModifier(
                    editMenuState,
                    new WindowMapCanvasFramePointFactoryFacade(),
                    new FrameworkElementInformation()
                ),
                mousePositionExtractor
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointDrawerActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointDrawerActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _framePointDrawerActionHandler.Inject(dataType, data);
        }
    }


    public class WindowMapCanvasFramePointDragModifierParameters
    {
        public Point MousePoint = new Point(0, 0);

        public AbstractBottingModel BottingModel = new BottingModel();
    }


    public class WindowMapCanvasFramePointDragModifier : AbstractWindowStateModifier
    {
        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowMapCanvasFramePointDragModifier(
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _editMenuState = editMenuState;
        }

        public override void Modify(object? value)
        {
            if (
                value is WindowMapCanvasFramePointDragModifierParameters parameters
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && _editMenuState.Dragging()
                && selectedObject.FrameObject is Canvas selectedFrame
                && selectedObject.PointObject is Canvas pointObject
                && pointObject.Tag is string pointTag
                && selectedFrame.Tag is MapCanvasRuneFrameDataTag runeFrameTag
                && parameters.BottingModel.GetRuneModel() is AbstractRuneModel runeModel
                && runeModel.FindRuneFrameByName(runeFrameTag.ElementLabel) is RuneFrame runeFrame
                && runeFrame.FrameData.RuneFrameMacros is List<RuneFrameMacro> runeFrameMacros
                && runeFrameMacros.Find((m) => m.ElementLabel == pointTag) is RuneFrameMacro runeFrameMacro
                && parameters.MousePoint.X - Canvas.GetLeft(selectedFrame) is double runePointX
                && parameters.MousePoint.Y - Canvas.GetTop(selectedFrame) is double runePointY
                && runePointX > 0
                && runePointX < selectedFrame.Width
                && runePointY > 0
                && runePointY < selectedFrame.Height
            )
            {
                Canvas.SetLeft(pointObject, runePointX);
                Canvas.SetTop(pointObject, runePointY);
                runeFrameMacro.X = runePointX;
                runeFrameMacro.Y = runePointY;
                runeModel.EditRuneFrame(runeFrame);
            }
        }
    }


    public class WindowMapCanvasFramePointDragActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractWindowStateModifier _framePointDragModifier;

        private AbstractMouseEventDataExtractor _mousePositionExtractor;

        private AbstractBottingModel? _bottingModel;

        public WindowMapCanvasFramePointDragActionHandler(
            Canvas mapCanvas,
            AbstractWindowStateModifier framePointDragModifier,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _mapCanvas = mapCanvas;
            _mapCanvas.MouseMove += OnEvent;
            _framePointDragModifier = framePointDragModifier;
            _mousePositionExtractor = mousePositionExtractor;
            _bottingModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointDragModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (e is MouseEventArgs mouseEventArgs)
            {
                var position = _mousePositionExtractor.GetPosition(mouseEventArgs, _mapCanvas);
                _framePointDragModifier.Modify(
                    new WindowMapCanvasFramePointDragModifierParameters
                    {
                        MousePoint = position,
                        BottingModel = _bottingModel!
                    }
                );
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.BottingModel
                && data is AbstractBottingModel bottingModel
            )
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public class WindowMapCanvasFramePointDragActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointDragActionHandler;

        public WindowMapCanvasFramePointDragActionHandlerFacade(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _framePointDragActionHandler = new WindowMapCanvasFramePointDragActionHandler(
                mapCanvas,
                new WindowMapCanvasFramePointDragModifier(editMenuState),
                mousePositionExtractor
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointDragActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointDragActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _framePointDragActionHandler.Inject(dataType, data);
        }
    }



    public class WindowMapCanvasFramePointScaleModifier : AbstractWindowStateModifier
    {
        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowMapCanvasFramePointScaleModifier(
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _editMenuState = editMenuState;
        }

        public override void Modify(object? value)
        {
            if (
                value is AbstractBottingModel bottingModel
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && _editMenuState.Dragging()
                && selectedObject.FrameObject is Canvas frameObject
                && selectedObject.DragPoint is Tuple<double, double>
                && selectedObject.PointObject is null
                && frameObject.Tag is MapCanvasRuneFrameDataTag runeFrameTag
                && bottingModel.GetRuneModel().FindRuneFrameByName(runeFrameTag.ElementLabel) is RuneFrame runeFrame
                && runeFrame.FrameData.RuneFrameMacros is List<RuneFrameMacro> runeFrameMacros
                && frameObject.Children.OfType<Canvas>().ToList() is List<Canvas> framePoints
            )
            {
                foreach (var framePoint in framePoints)
                {
                    if (runeFrameMacros.Find((m) => m.ElementLabel == (string)framePoint.Tag) is RuneFrameMacro runeFrameMacro)
                    {
                        if (frameObject.Width > 0)
                        {
                            var scaledX = frameObject.Width * (runeFrameMacro.X / runeFrameMacro.ScaleX);
                            Canvas.SetLeft(framePoint, scaledX);
                            runeFrameMacro.X = scaledX;
                            runeFrameMacro.ScaleX = frameObject.Width;
                        }
                        if (frameObject.Height > 0)
                        {
                            var scaledY = frameObject.Height * (runeFrameMacro.Y / runeFrameMacro.ScaleY);
                            Canvas.SetTop(framePoint, scaledY);
                            runeFrameMacro.Y = scaledY;
                            runeFrameMacro.ScaleY = frameObject.Height;
                        }
                    }
                }
                bottingModel.GetRuneModel().EditRuneFrame(runeFrame);
            }
        }
    }


    public class WindowMapCanvasFramePointScaleActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractWindowStateModifier _framePointScaleModifier;

        private AbstractBottingModel? _bottingModel;

        public WindowMapCanvasFramePointScaleActionHandler(
            Canvas mapCanvas,
            AbstractWindowStateModifier framePointScaleModifier
        )
        {
            _mapCanvas = mapCanvas;
            _framePointScaleModifier = framePointScaleModifier;
            _mapCanvas.MouseMove += OnEvent;
            _bottingModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointScaleModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointScaleModifier.Modify(_bottingModel);
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public class WindowMapCanvasFramePointScaleActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointScaleActionHandler;

        public WindowMapCanvasFramePointScaleActionHandlerFacade(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _framePointScaleActionHandler = new WindowMapCanvasFramePointScaleActionHandler(
                mapCanvas,
                new WindowMapCanvasFramePointScaleModifier(editMenuState)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointScaleActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointScaleActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _framePointScaleActionHandler.Inject(dataType, data);
        }
    }


    public class WindowMapCanvasFramePointRemoveModifierParameters
    {
        public Point RemovePoint = new Point(0, 0);

        public AbstractBottingModel BottingModel = new BottingModel();
    }


    public class WindowMapCanvasFramePointRemoveModifier : AbstractWindowStateModifier
    {
        private Canvas _mapCanvas;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowMapCanvasFramePointRemoveModifier(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _mapCanvas = mapCanvas;
            _editMenuState = editMenuState;
        }

        private Canvas? _findFramePointAtPosition(Point position)
        {
            Canvas? foundFramePoint = null;
            VisualTreeHelper.HitTest(
                _mapCanvas,
                null,
                new HitTestResultCallback(
                    result =>
                    {
                        var current = result.VisualHit;
                        while (current != null && current != _mapCanvas)
                        {
                            if (
                                current is Canvas currentCanvas
                                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                                && currentCanvas.Parent == selectedObject.FrameObject
                                && currentCanvas.Name == WindowMapCanvasFrameTypes.POINT
                            )
                            {
                                foundFramePoint = currentCanvas;
                                return HitTestResultBehavior.Stop;
                            }
                            current = VisualTreeHelper.GetParent(current);
                        }
                        return HitTestResultBehavior.Continue;
                    }
                ),
                new PointHitTestParameters(position)
            );
            return foundFramePoint;
        }

        public override void Modify(object? value)
        {
            if (
                value is WindowMapCanvasFramePointRemoveModifierParameters parameters
                && _editMenuState.GetState() is (int)WindowMapEditFrameMenuStateTypes.RemovePoint
                && _findFramePointAtPosition(parameters.RemovePoint) is Canvas removeFramePoint
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && removeFramePoint.Parent is Canvas removeFrame
                && removeFrame.Tag is MapCanvasRuneFrameDataTag runeFrameDataTag
                && parameters.BottingModel.GetRuneModel() is AbstractRuneModel runeModel
                && runeModel.FindRuneFrameByName(runeFrameDataTag.ElementLabel) is RuneFrame runeFrame
                && runeFrame.FrameData.RuneFrameMacros is List<RuneFrameMacro> runeFrameMacros
                && removeFramePoint.Tag is string pointElementLabel
                && runeFrameMacros.Find((m) => m.ElementLabel == pointElementLabel) is RuneFrameMacro runeFrameMacro
            )
            {
                removeFrame.Children.Remove(removeFramePoint);
                if (selectedObject.PointObject == removeFramePoint)
                {
                    selectedObject.PointObject = null;
                }
                runeFrameMacros.Remove(runeFrameMacro);
                runeModel.EditRuneFrame(runeFrame);
            }
        }
    }


    public class WindowMapCanvasFramePointRemoveActionHandler : AbstractWindowActionHandler
    {
        private Canvas _mapCanvas;

        private AbstractWindowStateModifier _framePointRemoveModifier;

        private AbstractMouseEventDataExtractor _mousePositionExtractor;

        private AbstractBottingModel? _bottingModel;

        public WindowMapCanvasFramePointRemoveActionHandler(
            Canvas mapCanvas,
            AbstractWindowStateModifier framePointRemoveModifier,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _mapCanvas = mapCanvas;
            _mapCanvas.MouseLeftButtonDown += OnEvent;
            _framePointRemoveModifier = framePointRemoveModifier;
            _mousePositionExtractor = mousePositionExtractor;
            _bottingModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointRemoveModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (e is MouseButtonEventArgs mouseButtonEventArgs)
            {
                _framePointRemoveModifier.Modify(
                    new WindowMapCanvasFramePointRemoveModifierParameters
                    {
                        RemovePoint = _mousePositionExtractor.GetPosition(mouseButtonEventArgs, _mapCanvas),
                        BottingModel = _bottingModel!
                    }
                );
            }
        }
    }


    public class WindowMapCanvasFramePointRemoveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointRemoveActionHandler;

        public WindowMapCanvasFramePointRemoveActionHandlerFacade(
            Canvas mapCanvas,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMouseEventDataExtractor mousePositionExtractor
        )
        {
            _framePointRemoveActionHandler = new WindowMapCanvasFramePointRemoveActionHandler(
                mapCanvas,
                new WindowMapCanvasFramePointRemoveModifier(mapCanvas, editMenuState),
                mousePositionExtractor
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointRemoveActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _framePointRemoveActionHandler.Inject(dataType, data);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointRemoveActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapCanvasLoadedRuneFrameDrawerModifier : AbstractWindowStateModifier
    {
        private Canvas _mapCanvas;

        private AbstractMapCanvasElementFactory _runeFrameFactory;

        private AbstractMapCanvasElementFactory _runeFramePointFactory;

        private AbstractMapCanvasFormatter _runeFrameDragFormatter;

        public WindowMapCanvasLoadedRuneFrameDrawerModifier(
            Canvas mapCanvas,
            AbstractMapCanvasElementFactory runeFrameFactory,
            AbstractMapCanvasElementFactory runeFramePointFactory,
            AbstractMapCanvasFormatter runeFrameDragFormatter
        )
        {
            _mapCanvas = mapCanvas;
            _runeFrameFactory = runeFrameFactory;
            _runeFramePointFactory = runeFramePointFactory;
            _runeFrameDragFormatter = runeFrameDragFormatter;
        }

        public override void Modify(object? value)
        {
            if (value is not AbstractBottingModel bottingModel)
            {
                return;
            }

            _mapCanvas.Children.Clear();

            foreach (var runeModelFrame in bottingModel.GetRuneModel().RuneFrames())
            {
                var xi = runeModelFrame.X;
                var yi = runeModelFrame.Y;
                var xf = runeModelFrame.X + runeModelFrame.Width;
                var yf = runeModelFrame.Y + runeModelFrame.Height;
                var frameName = runeModelFrame.FrameData.FrameName;
                var elementLabel = runeModelFrame.FrameData.ElementLabel;
                var runeFrame = (Canvas)_runeFrameFactory.Create();
                var runeFrameText = runeFrame.Children.OfType<TextBlock>().First();

                runeFrame.Width = _mapCanvas.Width;
                runeFrame.Height = _mapCanvas.Height;
                runeFrameText.Text = frameName;
                runeFrame.Tag = new MapCanvasRuneFrameDataTag
                {
                    FrameName = frameName,
                    ElementLabel = elementLabel,
                };

                _mapCanvas.Children.Add(runeFrame);
                _runeFrameDragFormatter.Format(
                    _mapCanvas,
                    [],
                    new WindowMapCanvasFrameFormatterData
                    {
                        DragObject = runeFrame,
                        CurrentPoint = new Point(xi, yi),
                        DragPoint = new Point(xf, yf)
                    }
                );

                foreach (var runeFrameMacro in runeModelFrame.FrameData.RuneFrameMacros)
                {
                    var x = runeFrameMacro.X;
                    var y = runeFrameMacro.Y;
                    var scaleX = runeFrameMacro.ScaleX;
                    var scaleY = runeFrameMacro.ScaleY;

                    if (scaleX > 0.0 && scaleY > 0.0)
                    {
                        var runeFramePoint = (Canvas)_runeFramePointFactory.Create();
                        var runeFramePointText = runeFramePoint.Children.OfType<TextBlock>().First();

                        runeFrame.Children.Add(runeFramePoint);
                        Canvas.SetLeft(runeFramePoint, (xf - xi) * (x / scaleX));
                        Canvas.SetTop(runeFramePoint, (yf - yi) * (y / scaleY));
                        runeFramePointText.Text = runeFrameMacro.MacroName;
                        runeFramePoint.Tag = runeFrameMacro.ElementLabel;
                    }
                }
            }
        }
    }


    public class WindowMapCanvasLoadedRuneFrameDrawerActionHandler : AbstractWindowActionHandler
    {
        private AbstractLoadFileDialog _loadFileDialog;

        private AbstractWindowStateModifier _loadedRuneFrameDrawerModifier;

        private AbstractBottingModel? _bottingModel;

        public WindowMapCanvasLoadedRuneFrameDrawerActionHandler(
            AbstractLoadFileDialog loadFileDialog,
            AbstractWindowStateModifier loadedRuneFrameDrawerModifier
        )
        {
            _loadFileDialog = loadFileDialog;
            _loadedRuneFrameDrawerModifier = loadedRuneFrameDrawerModifier;
            _loadFileDialog.FileLoaded += OnEvent;
            _bottingModel = null;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadedRuneFrameDrawerModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _loadedRuneFrameDrawerModifier.Modify(_bottingModel);
        }
    }


    public class WindowMapCanvasLoadedRuneFrameDrawerActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _loadedRuneFrameDrawerActionHandler;

        public WindowMapCanvasLoadedRuneFrameDrawerActionHandlerFacade(
            Canvas mapCanvas,
            AbstractLoadFileDialog loadFileDialog
        )
        {
            _loadedRuneFrameDrawerActionHandler = new WindowMapCanvasLoadedRuneFrameDrawerActionHandler(
                loadFileDialog,
                new WindowMapCanvasLoadedRuneFrameDrawerModifier(
                    mapCanvas,
                    new WindowMapCanvasFrameFactoryFacade(),
                    new WindowMapCanvasFramePointFactoryFacade(),
                    new WindowMapCanvasFrameDragFormatter()
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadedRuneFrameDrawerActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _loadedRuneFrameDrawerActionHandler.Inject(dataType, data);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _loadedRuneFrameDrawerActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMapCanvasLoadedRuneFrameDeselectModifier : AbstractWindowStateModifier
    {
        private List<ToggleButton> _uncheckButtons;

        private List<ButtonBase> _disableButtons;

        private List<TextBox> _clearTexts;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowMapCanvasLoadedRuneFrameDeselectModifier(
            List<ToggleButton> uncheckButtons,
            List<ButtonBase> disableButtons,
            List<TextBox> clearTexts,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _uncheckButtons = uncheckButtons;
            _disableButtons = disableButtons;
            _clearTexts = clearTexts;
            _editMenuState = editMenuState;
        }

        private void _uncheckSelectedButtons()
        {
            foreach (var button in _uncheckButtons)
            {
                button.IsChecked = false;
            }
        }

        private void _disableSelectedButtons()
        {
            foreach (var button in _disableButtons)
            {
                button.IsEnabled = false;
            }
        }

        private void _clearSelectedTexts()
        {
            _editMenuState.SetEditingText(true);
            foreach (var clearText in _clearTexts)
            {
                clearText.Text = "";
            }
            _editMenuState.SetEditingText(false);
        }

        private void _deselectSelectedState()
        {
            _editMenuState.Select(null);
            _editMenuState.SetDragging(false);
        }

        private void _clearMenuState()
        {
            _editMenuState.SetState(
                (int) WindowMapEditFrameMenuStateTypes.SelectFrame
            );
        }

        public override void Modify(object? value)
        {
            _uncheckSelectedButtons();
            _disableSelectedButtons();
            _clearSelectedTexts();
            _deselectSelectedState();
            _clearMenuState();
        }
    }


    public class WindowMapCanvasLoadedRuneFrameDeselectActionHandler : AbstractWindowActionHandler
    {
        private AbstractLoadFileDialog _loadFileDialog;

        private AbstractWindowStateModifier _loadedRuneFrameButtonAccessModifier;

        public WindowMapCanvasLoadedRuneFrameDeselectActionHandler(
            AbstractLoadFileDialog loadFileDialog,
            AbstractWindowStateModifier loadedRuneFrameButtonAccessModifier
        )
        {
            _loadFileDialog = loadFileDialog;
            _loadedRuneFrameButtonAccessModifier = loadedRuneFrameButtonAccessModifier;
            _loadFileDialog.FileLoaded += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadedRuneFrameButtonAccessModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _loadedRuneFrameButtonAccessModifier.Modify(null);
        }
    }


    public class WindowMapCanvasLoadedRuneFrameDeselectActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _loadedRuneFrameButtonAccessActionHandler;

        public WindowMapCanvasLoadedRuneFrameDeselectActionHandlerFacade(
            List<ToggleButton> uncheckButtons,
            List<ButtonBase> disableButtons,
            List<TextBox> clearTexts,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractLoadFileDialog loadFileDialog

        )
        {
            _loadedRuneFrameButtonAccessActionHandler = new WindowMapCanvasLoadedRuneFrameDeselectActionHandler(
                loadFileDialog,
                new WindowMapCanvasLoadedRuneFrameDeselectModifier(
                    uncheckButtons,
                    disableButtons,
                    clearTexts,
                    editMenuState
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadedRuneFrameButtonAccessActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _loadedRuneFrameButtonAccessActionHandler.OnEvent(sender, e);
        }
    }
}
