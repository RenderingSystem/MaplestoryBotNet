using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.Systems.UIHandler.Utilities.Mocks;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class FrameFixture
    {
        private static void _updateFrameworkElement(FrameworkElement frameworkElement)
        {
            frameworkElement.Measure(
                new Size(
                    double.PositiveInfinity,
                    double.PositiveInfinity
                )
            );
            frameworkElement.Arrange(
                new Rect(
                    0,
                    0,
                    frameworkElement.DesiredSize.Width,
                    frameworkElement.DesiredSize.Height
                )
            );
            frameworkElement.UpdateLayout();
        }

        public static Canvas GenerateFrame(
            double x, double y, double width, double height, Canvas mapCanvas
        )
        {
            var frameFactory = new WindowMapCanvasFrameFactoryFacade();
            var frameCanvas = (Canvas)frameFactory.Create();
            var rectangle = frameCanvas.Children.OfType<Rectangle>().ToList()[0];
            var ellipses = frameCanvas.Children.OfType<Ellipse>().ToList();
            var tl = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.TL)!;
            var tr = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.TR)!;
            var bl = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.BL)!;
            var br = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.BR)!;
            rectangle.Width = width;
            rectangle.Height = height;
            tl.RenderTransform = new TranslateTransform(-tl.Width / 2, -tl.Height / 2);
            tr.RenderTransform = new TranslateTransform(width - tr.Width / 2, -tr.Height / 2);
            bl.RenderTransform = new TranslateTransform(-bl.Width / 2, height - bl.Height / 2);
            br.RenderTransform = new TranslateTransform(width - br.Width / 2, height - br.Height / 2);
            mapCanvas.Children.Add(frameCanvas);
            Canvas.SetLeft(frameCanvas, x);
            Canvas.SetTop(frameCanvas, y);
            frameCanvas.Width = width;
            frameCanvas.Height = height;
            _updateFrameworkElement(rectangle);
            _updateFrameworkElement(tl);
            _updateFrameworkElement(tr);
            _updateFrameworkElement(bl);
            _updateFrameworkElement(br);
            _updateFrameworkElement(frameCanvas);
            _updateFrameworkElement(mapCanvas);
            return frameCanvas;
        }
    }

    public class WindowMapCanvasFrameDrawerActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private TextBox _frameLabelTextBox = new TextBox();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private MockMouseEventDataExtractor _mousePositionExtractor = new MockMouseEventDataExtractor();

        private AbstractWindowActionHandler _fixture()
        {
            _mapCanvas = new Canvas();
            _frameLabelTextBox = new TextBox();
            _editMenuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MockMouseEventDataExtractor();
            _mousePositionExtractor.GetPositionReturn.Add(new Point(123, 234));
            return new WindowMapCanvasFrameDrawerActionHandlerFacade(
                _mapCanvas,
                _frameLabelTextBox,
                _editMenuState,
                _mousePositionExtractor
            );
        }

        private MouseButtonEventArgs _mouseButtonEvent()
        {
            return new MouseButtonEventArgs(
                Mouse.PrimaryDevice,
                Environment.TickCount,
                MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _mapCanvas
            };
        }

        /**
         * @brief Verifies that clicking in Add mode creates a complete frame at the click location
         * 
         * When users click on the minimap canvas while in Add mode, the system should
         * create a fully functional frame with all its components positioned exactly
         * where they clicked.
         */
        private void _testFrameCreatedAtClickedPoint()
        {
            var mapCanvasFrameDrawerActionHandler = _fixture();
            var mouseButtonEventArgs = _mouseButtonEvent();
            _editMenuState.SetState((int)WindowMapEditMenuStateTypes.Add);
            _mapCanvas.RaiseEvent(mouseButtonEventArgs);
            Debug.Assert(_mapCanvas.Children.Count == 1);
            Debug.Assert(_mapCanvas.Children[0] is Canvas);
            var frameCanvas = (Canvas)_mapCanvas.Children[0];
            var ellipses = frameCanvas.Children.OfType<Ellipse>().ToList();
            var rectangle = frameCanvas.Children.OfType<Rectangle>().ToList();
            var textBlock = frameCanvas.Children.OfType<TextBlock>().ToList();
            Debug.Assert(frameCanvas.Name == WindowMapCanvasFrameTypes.CANVAS);
            Debug.Assert(Canvas.GetLeft(frameCanvas) == 123);
            Debug.Assert(Canvas.GetTop(frameCanvas) == 234);
            Debug.Assert(frameCanvas.Children.Count == 6);
            Debug.Assert(ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.TL) != null);
            Debug.Assert(ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.TR) != null);
            Debug.Assert(ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.BL) != null);
            Debug.Assert(ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.BR) != null);
            Debug.Assert(rectangle.Count == 1);
            Debug.Assert(textBlock.Count == 1);
        }

        /**
         * @brief Verifies that after creating a frame, the system enters dragging mode
         * 
         * When users click to create a new frame, the system should automatically
         * enter a dragging state so they can immediately adjust the frame's position
         * or size without clicking again.
         */
        private void _testDragStateAfterAdding()
        {
            var mapCanvasFrameDrawerActionHandler = _fixture();
            var mouseButtonEventArgs = _mouseButtonEvent();
            _editMenuState.SetState((int)WindowMapEditMenuStateTypes.Add);
            Debug.Assert(!_editMenuState.Dragging());
            _mapCanvas.RaiseEvent(mouseButtonEventArgs);
            Debug.Assert(_editMenuState.Dragging());
        }

        /**
         * @brief Verifies that the newly created frame becomes the selected object
         * 
         * When users create a new frame, it should automatically become the selected
         * object in the editor. This allows immediate manipulation without having to
         * click on the frame again to select it.
         */
        private void _testSelectedAfterAdding()
        {
            var mapCanvasFrameDrawerActionHandler = _fixture();
            var mouseButtonEventArgs = _mouseButtonEvent();
            _editMenuState.SetState((int)WindowMapEditMenuStateTypes.Add);
            Debug.Assert(_editMenuState.Selected() == null);
            _mapCanvas.RaiseEvent(mouseButtonEventArgs);
            Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
            var selected = (WindowMapEditMenuFrameSelectedObject)_editMenuState.Selected()!;
            Debug.Assert(selected.DragObject == _mapCanvas.Children[0]);
            Debug.Assert(selected.DragPoint!.Item1 == 123);
            Debug.Assert(selected.DragPoint.Item2 == 234);
        }

        /**
         * @brief Ensures frames are only created when in Add mode
         * 
         * Users should only be able to create new frames when the editor is in
         * "Add" mode. This test verifies that when in "Select" mode (or any other
         * mode), clicking on the canvas does not create new frames.
         */
        private void _testFrameNotCreatedOnWrongMenuState()
        {
            var mapCanvasFrameDrawerActionHandler = _fixture();
            var mouseButtonEventArgs = _mouseButtonEvent();
            _editMenuState.SetState((int)WindowMapEditMenuStateTypes.Select);
            _mapCanvas.RaiseEvent(mouseButtonEventArgs);
            Debug.Assert(_mapCanvas.Children.Count == 0);
            Debug.Assert(!_editMenuState.Dragging());
            Debug.Assert(_editMenuState.Selected() == null);
        }

        public void Run()
        {
            _testFrameCreatedAtClickedPoint();
            _testDragStateAfterAdding();
            _testSelectedAfterAdding();
            _testFrameNotCreatedOnWrongMenuState();
        }
    }


    public class WindowMapCanvasFrameSelectStateActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private MockMouseEventDataExtractor _mousePositionExtractor = new MockMouseEventDataExtractor();

        private MouseButtonEventArgs _mouseButtonEvent()
        {
            return new MouseButtonEventArgs(
                Mouse.PrimaryDevice,
                Environment.TickCount,
                MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _mapCanvas,
            };
        }

        private AbstractWindowActionHandler _fixture(double x, double y, MouseButtonState buttonState)
        {
            _mapCanvas = new Canvas { Width = 500, Height = 500 };
            _editMenuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MockMouseEventDataExtractor();
            _mousePositionExtractor.GetPositionReturn.Add(new Point(x, y));
            _mousePositionExtractor.GetButtonStateReturn.Add(buttonState);
            return new WindowMapCanvasFrameSelectStateActionHandlerFacade(
                _mapCanvas, _editMenuState, _mousePositionExtractor
            );
        }

        /**
         * @brief Verifies that clicking the center of a frame selects it
         * 
         * When users click on the interior (non-grip area) of a frame, the frame
         * should become selected. Unlike grip clicks which enable resizing, center
         * clicks simply select the frame without entering drag mode.
         */
        private void _testSelectingCenterOfFrameSelectsFrame()
        {
            var _frameSelectStateActionHandler = _fixture(250, 250, MouseButtonState.Pressed);
            var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
            _mapCanvas.RaiseEvent(_mouseButtonEvent());
            Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
            var selected = (WindowMapEditMenuFrameSelectedObject) _editMenuState.Selected()!;
            Debug.Assert(selected.DragObject == frame);
            Debug.Assert(selected.DragPoint == null);
        }

        /**
         * @brief Verifies that clicking a corner grip selects the frame for resizing
         * 
         * When users click on any of the four corner grips (TL, TR, BL, BR),
         * the frame should become selected with the opposite grip identified
         * as the anchor point for resizing operations.
         */
        private void _testSelectingGripOfFrameSetsDragState()
        {
            var clickPoints = new List<Point>
            {
                new Point(100, 100),
                new Point(100, 400),
                new Point(400, 100),
                new Point(400, 400)
            };
            var oppositePoints = new List<Point>
            {
                new Point(400, 400),
                new Point(400, 100),
                new Point(100, 400),
                new Point(100, 100),
            };
            foreach (var point in clickPoints)
            {
                var _frameSelectStateActionHandler = _fixture(point.X, point.Y, MouseButtonState.Pressed);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _mapCanvas.RaiseEvent(_mouseButtonEvent());
                Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
                var selected = (WindowMapEditMenuFrameSelectedObject)_editMenuState.Selected()!;
                Debug.Assert(selected.DragObject == frame);
                Debug.Assert(selected.DragPoint!.Item1 == oppositePoints[clickPoints.IndexOf(point)].X);
                Debug.Assert(selected.DragPoint!.Item2 == oppositePoints[clickPoints.IndexOf(point)].Y);
            }
        }

        /**
         * @brief Verifies that overlapping frames correctly select the topmost frame
         * 
         * When multiple frames overlap and users click on a grip that belongs to
         * a lower frame but is visually overlapped by a higher frame, the system
         * should select the topmost frame that contains the grip.
         */
        private void _testSelectingGripUnderFrameSetsDragState()
        {
            var clickPoints = new List<Point>
            {
                new Point(150, 150),
                new Point(150, 350),
                new Point(350, 150),
                new Point(150, 350)
            };
            var oppositePoints = new List<Point>
            {
                new Point(350, 350),
                new Point(350, 150),
                new Point(150, 350),
                new Point(350, 150),
            };
            foreach (var point in clickPoints)
            {
                var _frameSelectStateActionHandler = _fixture(point.X, point.Y, MouseButtonState.Pressed);
                var frame1 = FrameFixture.GenerateFrame(150, 150, 200, 200, _mapCanvas);
                var frame2 = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _mapCanvas.RaiseEvent(_mouseButtonEvent());
                Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
                var selected = (WindowMapEditMenuFrameSelectedObject)_editMenuState.Selected()!;
                Debug.Assert(selected.DragObject == frame1);
                Debug.Assert(selected.DragPoint!.Item1 == oppositePoints[clickPoints.IndexOf(point)].X);
                Debug.Assert(selected.DragPoint!.Item2 == oppositePoints[clickPoints.IndexOf(point)].Y);
            }
        }

        /**
         * @brief Verifies that clicking outside a frame deselects it
         * 
         * When users click on empty canvas space (not on any frame or grip),
         * the currently selected frame should become deselected.
         */
        private void _testSelectingOutsideOfFrameDeselectsFrame()
        {
            var _frameSelectStateActionHandler = _fixture(50, 50, MouseButtonState.Pressed);
            var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject());
            _editMenuState.SetDragging(false);
            _mapCanvas.RaiseEvent(_mouseButtonEvent());
            Debug.Assert(_editMenuState.Selected() == null);
            Debug.Assert(!_editMenuState.Dragging());
        }

        /**
         * @brief Verifies that releasing the mouse button exits drag mode
         * 
         * When users release the left mouse button after dragging a frame or grip,
         * the system should exit drag mode while keeping the frame selected.
         */
        private void _testReleasingMouseUnsetsDragState()
        {
            var _frameSelectStateActionHandler = _fixture(50, 50, MouseButtonState.Released);
            var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
            var selectedObject = new WindowMapEditMenuFrameSelectedObject();
            _editMenuState.Select(selectedObject);
            _editMenuState.SetDragging(true);
            _mapCanvas.RaiseEvent(_mouseButtonEvent());
            Debug.Assert(selectedObject == _editMenuState.Selected());
            Debug.Assert(!_editMenuState.Dragging());
        }

        public void Run()
        {
            _testSelectingCenterOfFrameSelectsFrame();
            _testSelectingGripOfFrameSetsDragState();
            _testSelectingGripUnderFrameSetsDragState();
            _testSelectingOutsideOfFrameDeselectsFrame();
            _testReleasingMouseUnsetsDragState();
        }
    }


    public class WindowMapCanvasFrameDragActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas{ Width = 500, Height = 500 };

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private MockMouseEventDataExtractor _mouseDataExtractor = new MockMouseEventDataExtractor();

        private AbstractWindowActionHandler _fixture(double x, double y)
        {
            _mapCanvas = new Canvas { Width = 500, Height = 500 };
            _editMenuState = new WindowMapEditMenuState();
            _mouseDataExtractor = new MockMouseEventDataExtractor();
            _mouseDataExtractor.GetPositionReturn.Add(new Point(x, y));
            return new WindowMapCanvasFrameDragActionHandlerFacade(
                _mapCanvas, _editMenuState, _mouseDataExtractor
            );
        }

        private MouseEventArgs _mouseMoveEvent()
        {
            var mouseDevice = Mouse.PrimaryDevice;
            var mouseEventArgs = new MouseEventArgs(mouseDevice, Environment.TickCount)
            {
                RoutedEvent = UIElement.MouseMoveEvent,
                Source = _mapCanvas
            };
            return mouseEventArgs;
        }

        private List<Point> _anchorPoints()
        {
            return new List<Point>
            {
                new Point(400, 400),
                new Point(400, 100),
                new Point(100, 400),
                new Point(100, 100),
            };
        }

        /**
         * @brief Verifies that dragging a frame correctly updates its position and size
         * 
         * When users drag a frame by clicking and moving the mouse, the frame should
         * move to the new position and resize appropriately based on which corner
         * grip was used as the anchor point.
         */
        private void _testDraggingSelectedFrameAdjustsCanvasDimensions()
        {
            var anchorPoints = _anchorPoints();
            var dragPoint = new Point(9, 7);
            for (int i = 0; i < anchorPoints.Count; i++)
            {
                var anchorPoint = anchorPoints[i];
                var frameDragActionHandler = _fixture(dragPoint.X, dragPoint.Y);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _editMenuState.Select(
                    new WindowMapEditMenuFrameSelectedObject
                    {
                        DragObject = frame,
                        DragPoint = new Tuple<double, double>(anchorPoint.X, anchorPoint.Y)
                    }
                );
                _editMenuState.SetDragging(true);
                _mapCanvas.RaiseEvent(_mouseMoveEvent());
                var left = Canvas.GetLeft(frame);
                Debug.Assert(Canvas.GetLeft(frame) == dragPoint.X);
                Debug.Assert(Canvas.GetTop(frame) == dragPoint.Y);
                Debug.Assert(frame.Width == anchorPoint.X - dragPoint.X);
                Debug.Assert(frame.Height == anchorPoint.Y - dragPoint.Y);
            }

        }

        /**
         * @brief Verifies that the frame's border rectangle resizes correctly during drag
         * 
         * When users resize a frame by dragging a corner grip, the inner rectangle
         * that represents the frame border should update its dimensions to match
         * the new frame size.
         */
        private void _testDraggingSelectedFrameAdjustsRectangleDimensions()
        {
            var anchorPoints = _anchorPoints();
            var dragPoint = new Point(9, 7);
            for (int i = 0; i < anchorPoints.Count; i++)
            {
                var anchorPoint = anchorPoints[i];
                var frameDragActionHandler = _fixture(dragPoint.X, dragPoint.Y);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _editMenuState.Select(
                    new WindowMapEditMenuFrameSelectedObject
                    {
                        DragObject = frame,
                        DragPoint = new Tuple<double, double>(anchorPoint.X, anchorPoint.Y)
                    }
                );
                _editMenuState.SetDragging(true);
                _mapCanvas.RaiseEvent(_mouseMoveEvent());
                var rectangle = frame.Children.OfType<Rectangle>().ToList()[0];
                Debug.Assert(rectangle.Width == anchorPoint.X - dragPoint.X);
                Debug.Assert(rectangle.Height == anchorPoint.Y - dragPoint.Y);
            }
        }

        /**
         * @brief Verifies that corner grips reposition correctly during frame resize
         * 
         * When users resize a frame by dragging a corner grip, the remaining three
         * corner grips should reposition themselves to maintain their correct
         * positions at the frame's corners.
         */
        private void _testDraggingSelectedFrameAdjustsGripDimensions()
        {
            var anchorPoints = _anchorPoints();
            var dragPoint = new Point(9, 7);
            for (int i = 0; i < anchorPoints.Count; i++)
            {
                var anchorPoint = anchorPoints[i];
                var frameDragActionHandler = _fixture(dragPoint.X, dragPoint.Y);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _editMenuState.Select(
                    new WindowMapEditMenuFrameSelectedObject
                    {
                        DragObject = frame,
                        DragPoint = new Tuple<double, double>(anchorPoint.X, anchorPoint.Y)
                    }
                );
                _editMenuState.SetDragging(true);
                _mapCanvas.RaiseEvent(_mouseMoveEvent());
                var ellipses = frame.Children.OfType<Ellipse>().ToList();
                var tl = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.TL)!;
                var tr = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.TR)!;
                var bl = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.BL)!;
                var br = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.BR)!;
                Debug.Assert(tl.RenderTransform.Value.OffsetX == -tl.Width / 2);
                Debug.Assert(tl.RenderTransform.Value.OffsetY == -tl.Height / 2);
                Debug.Assert(tr.RenderTransform.Value.OffsetX == anchorPoint.X - dragPoint.X - tr.Width / 2);
                Debug.Assert(tr.RenderTransform.Value.OffsetY == -tr.Height / 2);
                Debug.Assert(bl.RenderTransform.Value.OffsetX == -bl.Width / 2);
                Debug.Assert(bl.RenderTransform.Value.OffsetY == anchorPoint.Y - dragPoint.Y - bl.Height / 2);
                Debug.Assert(br.RenderTransform.Value.OffsetX == anchorPoint.X - dragPoint.X - br.Width / 2);
                Debug.Assert(br.RenderTransform.Value.OffsetY == anchorPoint.Y - dragPoint.Y - br.Height / 2);
            }
        }

        /**
         * @brief Verifies that the frame label stays in the correct position during resize
         * 
         * When users resize or move a frame, the text label that identifies the frame
         * should maintain its position relative to the frame (typically at the top-left
         * corner with a small margin).
         */
        private void _testDraggingSelectedFrameAdjustsLabelDimensions()
        {
            var anchorPoints = _anchorPoints();
            var dragPoint = new Point(9, 7);
            for (int i = 0; i < anchorPoints.Count; i++)
            {
                var anchorPoint = anchorPoints[i];
                var frameDragActionHandler = _fixture(dragPoint.X, dragPoint.Y);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _editMenuState.Select(
                    new WindowMapEditMenuFrameSelectedObject
                    {
                        DragObject = frame,
                        DragPoint = new Tuple<double, double>(anchorPoint.X, anchorPoint.Y)
                    }
                );
                _editMenuState.SetDragging(true);
                _mapCanvas.RaiseEvent(_mouseMoveEvent());
                var textBlock = frame.Children.OfType<TextBlock>().ToList()[0];
                Debug.Assert(Canvas.GetLeft(textBlock) == 5);
                Debug.Assert(Canvas.GetTop(textBlock) == textBlock.ActualHeight + 5);
            }
        }

        public void Run()
        {
            _testDraggingSelectedFrameAdjustsCanvasDimensions();
            _testDraggingSelectedFrameAdjustsRectangleDimensions();
            _testDraggingSelectedFrameAdjustsGripDimensions();
            _testDraggingSelectedFrameAdjustsLabelDimensions();
        }
    }


    public class WindowMapEditorRuneHandlersTestSuite
    {
        public void Run()
        {
            new WindowMapCanvasFrameDrawerActionHandlerTests().Run();
            new WindowMapCanvasFrameSelectStateActionHandlerTests().Run();
            new WindowMapCanvasFrameDragActionHandlerTests().Run();
        }
    }
}
