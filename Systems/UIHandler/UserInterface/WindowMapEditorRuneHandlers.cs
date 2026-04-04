using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class WindowMapEditMenuFrameSelectedObject
    {
        public Tuple<double, double>? DragPoint;

        public object DragObject = new object();
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
            if (value is WindowMapCanvasFrameDrawerParameters parameters)
            {
                var createdFrame = _frameFactory.Create();
                _canvas.Children.Add(createdFrame);
                Canvas.SetLeft(createdFrame, parameters.ElementPoint.Item1);
                Canvas.SetTop(createdFrame, parameters.ElementPoint.Item2);
                _editMenuState.Select(
                    new WindowMapEditMenuFrameSelectedObject
                    {
                        DragPoint = parameters.ElementPoint,
                        DragObject = createdFrame
                    }
                );
                _editMenuState.SetDragging(true);
            }
        }
    }


    public class WindowMapCanvasFrameDrawerActionHandler : AbstractWindowActionHandler
    {
        private Canvas _canvas;

        private AbstractWindowMapEditMenuState _editMenuState;

        private AbstractWindowStateModifier _frameDrawer;

        private AbstractMouseEventDataExtractor _mousePositionExtractor;

        public WindowMapCanvasFrameDrawerActionHandler(
            Canvas canvas,
            TextBox frameLabelTextBox,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractMouseEventDataExtractor mousePositionExtractor,
            AbstractWindowStateModifier frameDrawer
        )
        {
            _canvas = canvas;
            _editMenuState = editMenuState;
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
            if (_editMenuState.GetState() == (int) WindowMapEditMenuStateTypes.Add)
            {
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
                editMenuState,
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

        private FrameworkElement? _locatedGrip;

        public WindowMapCanvasFrameSelectStateModifier(
            Canvas mapCanvas, AbstractWindowMapEditMenuState editMenuState
        )
        {
            _mapCanvas = mapCanvas;
            _editMenuState = editMenuState;
            _locatedFrame = null;
            _locatedGrip = null;
        }

        private HitTestResultBehavior _locateTargetsCallback(HitTestResult result)
        {
            var current = result.VisualHit;
            var gripNames = WindowMapCanvasFrameTypes.GripNames();
            var oppositeNames = WindowMapCanvasFrameTypes.OppositeNames();
            if (
                current is FrameworkElement element
                && element.Parent is Canvas dragBody
                && dragBody.Name == WindowMapCanvasFrameTypes.CANVAS
            )
            {
                if (_locatedFrame == null)
                {
                    _locatedFrame = dragBody;
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
                        return HitTestResultBehavior.Stop;
                    }
                }
            }
            return HitTestResultBehavior.Continue;
        }

        private Tuple<Canvas?, FrameworkElement?> _locateTargets(Point mousePoint)
        {
            _locatedFrame = null;
            _locatedGrip = null;
            VisualTreeHelper.HitTest(
                _mapCanvas,
                null,
                new HitTestResultCallback(_locateTargetsCallback),
                new PointHitTestParameters(mousePoint)
            );
            return new Tuple<Canvas?, FrameworkElement?>(_locatedFrame, _locatedGrip);
        }

        private WindowMapEditMenuFrameSelectedObject? _locateSelectObject(Point mousePoint)
        {
            var targets = _locateTargets(mousePoint);
            if (targets.Item1 != null && targets.Item2 != null)
            {
                return new WindowMapEditMenuFrameSelectedObject
                {
                    DragObject = targets.Item1,
                    DragPoint = new Tuple<double, double>(
                        Canvas.GetLeft(targets.Item1)
                        + targets.Item2.RenderTransform.Value.OffsetX
                        + targets.Item2.Width / 2,
                        Canvas.GetTop(targets.Item1)
                        + targets.Item2.RenderTransform.Value.OffsetY
                        + targets.Item2.Height / 2
                    )
                };
            }
            else if (targets.Item1 != null && targets.Item2 == null)
            {
                return new WindowMapEditMenuFrameSelectedObject
                {
                    DragObject = targets.Item1,
                    DragPoint = null
                };
            }
            return null;
        }

        public override void Modify(object? value)
        {
            if (value is not WindowMapCanvasFrameSelectStateModifierParameters parameters)
            {
                return;
            }
            if (parameters.LeftMouseDown && !_editMenuState.Dragging())
            {
                var selectObject = _locateSelectObject(parameters.MousePosition);
                _editMenuState.Select(selectObject);
                _editMenuState.SetDragging(selectObject != null && selectObject.DragPoint != null);
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


    public class MapCanvasFrameFormatterData
    {
        public Canvas DragObject = new Canvas();

        public Point DragPoint = new Point();

        public Point CurrentPoint = new Point();
    }


    public class MapCanvasFrameFormatter : AbstractMapCanvasFormatter
    {
        private Tuple<double, double, double, double> _calculateRectangle(
            Canvas mapCanvas, MapCanvasFrameFormatterData formatterData
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
                            new TranslateTransform(- wRadius, - hRadius) :
                        ellipse.Name == WindowMapCanvasFrameTypes.TR ?
                            new TranslateTransform(rectangle.Item3 - wRadius, - hRadius) :
                        ellipse.Name == WindowMapCanvasFrameTypes.BL ?
                            new TranslateTransform(- wRadius, rectangle.Item4 - hRadius) :
                        ellipse.Name == WindowMapCanvasFrameTypes.BR ?
                            new TranslateTransform(rectangle.Item3 - wRadius, rectangle.Item4 - hRadius) :
                        ellipse.RenderTransform
                    );
                }
                else if (child is TextBlock textBlock)
                {
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
                && formatData is MapCanvasFrameFormatterData frameFormatterData
            )
            {
                var rectangle = _calculateRectangle(mapCanvas, frameFormatterData);
                _formatDragObjectDimensions(frameFormatterData.DragObject, rectangle);
                _formatChildren(frameFormatterData.DragObject, rectangle); ;
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
                && frameSelected.DragObject is Canvas dragObject
                && frameSelected.DragPoint is Tuple<double, double> dragPoint
            )
            {
                _mapCanvasFrameFormatter.Format(
                    _mapCanvas,
                    [],
                    new MapCanvasFrameFormatterData
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
                    mapCanvas, editMenuState, new MapCanvasFrameFormatter()
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
}
