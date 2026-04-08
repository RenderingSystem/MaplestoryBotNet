using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.Systems.UIHandler.Utilities.Mocks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class ButtonClickFixture
    {
        public static MouseButtonEventArgs Event(Canvas canvas)
        {
            return new MouseButtonEventArgs(
                Mouse.PrimaryDevice,
                Environment.TickCount,
                MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = canvas
            };
        }
    }


    public class MouseMoveFixture
    {
        public static MouseEventArgs Event(Canvas canvas)
        {
            var mouseDevice = Mouse.PrimaryDevice;
            var mouseEventArgs = new MouseEventArgs(mouseDevice, Environment.TickCount)
            {
                RoutedEvent = UIElement.MouseMoveEvent,
                Source = canvas
            };
            return mouseEventArgs;
        }
    }


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
            double x,
            double y,
            double width,
            double height,
            Canvas mapCanvas
        )
        {
            var frameFactory = new WindowMapCanvasFrameFactoryFacade();
            var frameCanvas = (Canvas)frameFactory.Create();
            var rectangle = frameCanvas.Children.OfType<Rectangle>().ToList()[0];
            var ellipses = frameCanvas.Children.OfType<Ellipse>().ToList();
            var textBlock = frameCanvas.Children.OfType<TextBlock>().ToList()[0];
            var tl = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.TL)!;
            var tr = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.TR)!;
            var bl = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.BL)!;
            var br = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.BR)!;
            tl.RenderTransform = new TranslateTransform(-tl.Width / 2, -tl.Height / 2);
            tr.RenderTransform = new TranslateTransform(width - tr.Width / 2, -tr.Height / 2);
            bl.RenderTransform = new TranslateTransform(-bl.Width / 2, height - bl.Height / 2);
            br.RenderTransform = new TranslateTransform(width - br.Width / 2, height - br.Height / 2);
            rectangle.Width = width;
            rectangle.Height = height;
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


    public class MapCanvasAddFramePointButtonActionHandlerTests
    {
        private ToggleButton _addPointButton = new ToggleButton();

        private List<ToggleButton> _otherButtons = [];

        private WindowMapEditMenuState _menuState = new WindowMapEditMenuState();

        private AbstractWindowActionHandler _fixture()
        {
            _addPointButton = new ToggleButton();
            _otherButtons = [new ToggleButton(), new ToggleButton(), new ToggleButton()];
            _menuState = new WindowMapEditMenuState();
            return new WindowMapAddFrameButtonActionHandlerFacade(
                _addPointButton, _otherButtons, _menuState
            );
        }

        /**
         * @brief Verifies that clicking the Add Frame button turns off other mode buttons
         * 
         * When users click the Add Frame button, any other editing mode buttons
         * (like Remove Frame or Select) should automatically turn off. This ensures
         * only one editing mode is active at a time, preventing confusion.
         */
        private void _testTogglingButtonUntogglesOtherButtons()
        {
            var handler = _fixture();
            _otherButtons[0].IsChecked = true;
            _otherButtons[1].IsChecked = false;
            _otherButtons[2].IsChecked = true;
            _addPointButton.IsChecked = true;
            _addPointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_otherButtons[0].IsChecked == false);
            Debug.Assert(_otherButtons[1].IsChecked == false);
            Debug.Assert(_otherButtons[2].IsChecked == false);
        }

        /**
         * @brief Verifies that turning off Add Frame clears all active modes
         * 
         * When users click the Add Frame button again to turn it off, all editing
         * mode buttons should be cleared. This puts the editor in a neutral state
         * where no special editing mode is active.
         */
        private void _testUntogglingButtonUntogglesOtherButtons()
        {
            var handler = _fixture();
            _otherButtons[0].IsChecked = true;
            _otherButtons[1].IsChecked = false;
            _otherButtons[2].IsChecked = true;
            _addPointButton.IsChecked = false;
            _addPointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_otherButtons[0].IsChecked == false);
            Debug.Assert(_otherButtons[1].IsChecked == false);
            Debug.Assert(_otherButtons[2].IsChecked == false);
        }

        /**
         * @brief Verifies that Add Frame button activates frame creation mode
         * 
         * When users click the Add Frame button, the editor should enter frame
         * creation mode. This tells the system that subsequent canvas clicks
         * should create new frames instead of selecting existing ones.
         */
        private void _testTogglingButtonSetsMenuStateToAdd()
        {
            var handler = _fixture();
            _menuState.SetState((int)WindowMapEditMenuStateTypes.Select);
            _addPointButton.IsChecked = true;
            _addPointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_menuState.GetState() == (int)WindowMapEditFrameMenuStateTypes.AddFrame);
        }

        /**
         * @brief Verifies that turning off Add Frame exits creation mode
         * 
         * When users click the Add Frame button to turn it off, the editor should
         * return to normal selection mode. This prevents accidental frame creation
         * when users just want to click on existing frames.
         */
        private void _testUntogglingButtonSetsMenuStateToSelect()
        {
            var handler = _fixture();
            _addPointButton.UpdateLayout();
            _menuState.SetState((int)WindowMapEditMenuStateTypes.Add);
            _addPointButton.IsChecked = false;
            _addPointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_menuState.GetState() == (int)WindowMapEditFrameMenuStateTypes.SelectFrame);
        }

        public void Run()
        {
            _testTogglingButtonUntogglesOtherButtons();
            _testUntogglingButtonUntogglesOtherButtons();
            _testTogglingButtonSetsMenuStateToAdd();
            _testUntogglingButtonSetsMenuStateToSelect();
        }
    }


    public class MapCanvasRemoveFramePointButtonActionHandlerTests
    {
        private ToggleButton _addPointButton = new ToggleButton();

        private List<ToggleButton> _otherButtons = [];

        private WindowMapEditMenuState _menuState = new WindowMapEditMenuState();

        private AbstractWindowActionHandler _fixture()
        {
            _addPointButton = new ToggleButton();
            _otherButtons = [new ToggleButton(), new ToggleButton(), new ToggleButton()];
            _menuState = new WindowMapEditMenuState();
            return new WindowMapRemoveFrameButtonActionHandlerFacade(
                _addPointButton, _otherButtons, _menuState
            );
        }

        /**
         * @brief Verifies that clicking the Remove Frame button turns off other mode buttons
         * 
         * When users click the Remove Frame button, any other editing mode buttons
         * (like Add Frame or Select) should automatically turn off. This ensures
         * only one editing mode is active at a time
         */
        private void _testTogglingButtonUntogglesOtherButtons()
        {
            var handler = _fixture();
            _otherButtons[0].IsChecked = true;
            _otherButtons[1].IsChecked = false;
            _otherButtons[2].IsChecked = true;
            _addPointButton.IsChecked = true;
            _addPointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_otherButtons[0].IsChecked == false);
            Debug.Assert(_otherButtons[1].IsChecked == false);
            Debug.Assert(_otherButtons[2].IsChecked == false);
        }

        /**
         * @brief Verifies that turning off Remove Frame clears all active modes
         * 
         * When users click the Remove Frame button again to turn it off, all editing
         * mode buttons should be cleared. This returns the editor to a neutral state.
         */
        private void _testUntogglingButtonUntogglesOtherButtons()
        {
            var handler = _fixture();
            _otherButtons[0].IsChecked = true;
            _otherButtons[1].IsChecked = false;
            _otherButtons[2].IsChecked = true;
            _addPointButton.IsChecked = false;
            _addPointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_otherButtons[0].IsChecked == false);
            Debug.Assert(_otherButtons[1].IsChecked == false);
            Debug.Assert(_otherButtons[2].IsChecked == false);
        }

        /**
         * @brief Verifies that Remove Frame button activates deletion mode
         * 
         * When users click the Remove Frame button, the editor should enter frame
         * deletion mode. This tells the system that subsequent canvas clicks should
         * remove frames instead of selecting or creating them.
         */
        private void _testTogglingButtonSetsMenuStateToRemove()
        {
            var handler = _fixture();
            _menuState.SetState((int)WindowMapEditMenuStateTypes.Select);
            _addPointButton.IsChecked = true;
            _addPointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_menuState.GetState() == (int)WindowMapEditFrameMenuStateTypes.RemoveFrame);
        }

        /**
         * @brief Verifies that turning off Remove Frame exits deletion mode
         * 
         * When users click the Remove Frame button to turn it off, the editor should
         * return to normal selection mode. This prevents accidental deletions when
         * users just want to select or inspect frames.
         */
        private void _testUntogglingButtonSetsMenuStateToSelect()
        {
            var handler = _fixture();
            _addPointButton.UpdateLayout();
            _menuState.SetState((int)WindowMapEditMenuStateTypes.Add);
            _addPointButton.IsChecked = false;
            _addPointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_menuState.GetState() == (int)WindowMapEditFrameMenuStateTypes.SelectFrame);
        }

        public void Run()
        {
            _testTogglingButtonUntogglesOtherButtons();
            _testUntogglingButtonUntogglesOtherButtons();
            _testTogglingButtonSetsMenuStateToRemove();
            _testUntogglingButtonSetsMenuStateToSelect();
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
            var mouseButtonEventArgs = ButtonClickFixture.Event(_mapCanvas);
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
            var mouseButtonEventArgs = ButtonClickFixture.Event(_mapCanvas);
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
            var mouseButtonEventArgs = ButtonClickFixture.Event(_mapCanvas);
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
            var mouseButtonEventArgs = ButtonClickFixture.Event(_mapCanvas);
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
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
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
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
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
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
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
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
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
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
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

        private List<Point> _dragPoints()
        {
            return new List<Point>
            {
                new Point(9, 7),
                new Point(-100, -100),
                new Point(600, -100),
                new Point(-100, -600),
                new Point(600, 600),
            };
        }

        /**
         * @brief Verifies that dragging updates the frame's position and size
         * 
         * When users drag a frame (by clicking and moving the mouse), the frame should
         * update its position and size based on which grip was used and whether
         * dragging is active.
         */
        private void _testDraggingSelectedFrameAdjustsCanvasDimensions()
        {
            var anchorPoints = _anchorPoints();
            var dragPoints = _dragPoints();
            for (int i = 0; i < anchorPoints.Count; i++)
            for (int j = 0; j < dragPoints.Count; j++)
            for (int dragging = 0; dragging < 2; dragging++)
            {
                var anchorPoint = anchorPoints[i];
                var dragPoint = new Point(
                    Math.Min(0, Math.Max(_mapCanvas.Width, dragPoints[j].X)),
                    Math.Min(0, Math.Max(_mapCanvas.Height, dragPoints[j].Y))
                );
                var frameDragActionHandler = _fixture(dragPoint.X, dragPoint.Y);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _editMenuState.Select(
                    new WindowMapEditMenuFrameSelectedObject
                    {
                        DragObject = frame,
                        DragPoint = new Tuple<double, double>(anchorPoint.X, anchorPoint.Y)
                    }
                );
                _editMenuState.SetDragging(dragging == 1);
                _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
                var left = (dragging == 1) ? dragPoint.X : 100;
                var top = (dragging == 1) ? dragPoint.Y : 100;
                var width = (dragging == 1) ? anchorPoint.X - dragPoint.X : 300;
                var height = (dragging == 1) ? anchorPoint.Y - dragPoint.Y : 300;
                Debug.Assert(Canvas.GetLeft(frame) == left);
                Debug.Assert(Canvas.GetTop(frame) == top);
                Debug.Assert(frame.Width == width);
                Debug.Assert(frame.Height == height);
            }

        }

        /**
         * @brief Verifies that the frame's border rectangle resizes correctly
         * 
         * When users resize a frame by dragging a corner grip, the inner rectangle
         * that represents the frame border should update its dimensions to match
         * the new frame size exactly.
         */
        private void _testDraggingSelectedFrameAdjustsRectangleDimensions()
        {
            var anchorPoints = _anchorPoints();
            var dragPoints = _dragPoints();
            for (int i = 0; i < anchorPoints.Count; i++)
            for (int j = 0; j < dragPoints.Count; j++)
            for (int dragging = 0; dragging < 2; dragging++)
            {
                var anchorPoint = anchorPoints[i];
                var dragPoint = new Point(
                    Math.Min(0, Math.Max(_mapCanvas.Width, dragPoints[j].X)),
                    Math.Min(0, Math.Max(_mapCanvas.Height, dragPoints[j].Y))
                );
                var frameDragActionHandler = _fixture(dragPoint.X, dragPoint.Y);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _editMenuState.Select(
                    new WindowMapEditMenuFrameSelectedObject
                    {
                        DragObject = frame,
                        DragPoint = new Tuple<double, double>(anchorPoint.X, anchorPoint.Y)
                    }
                );
                _editMenuState.SetDragging(dragging == 1);
                _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
                var rectangle = frame.Children.OfType<Rectangle>().ToList()[0];
                var width = (dragging == 1) ? anchorPoint.X - dragPoint.X : 300.0;
                var height = (dragging == 1) ? anchorPoint.Y - dragPoint.Y : 300.0;
                Debug.Assert(rectangle.Width == width);
                Debug.Assert(rectangle.Height == height);
            }
        }

        /**
         * @brief Verifies that corner grips reposition correctly during resize
         * 
         * When users resize a frame by dragging a corner grip, the remaining three
         * corner grips should reposition themselves to maintain their correct
         * positions at the frame's four corners.
         */
        private void _testDraggingSelectedFrameAdjustsGripDimensions()
        {
            var anchorPoints = _anchorPoints();
            var dragPoints = _dragPoints();
            for (int i = 0; i < anchorPoints.Count; i++)
            for (int j = 0; j < dragPoints.Count; j++)
            for (int dragging = 0; dragging < 2; dragging++)
            {
                var anchorPoint = anchorPoints[i];
                var dragPoint = new Point(
                    Math.Min(0, Math.Max(_mapCanvas.Width, dragPoints[j].X)),
                    Math.Min(0, Math.Max(_mapCanvas.Height, dragPoints[j].Y))
                );
                var frameDragActionHandler = _fixture(dragPoint.X, dragPoint.Y);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _editMenuState.Select(
                    new WindowMapEditMenuFrameSelectedObject
                    {
                        DragObject = frame,
                        DragPoint = new Tuple<double, double>(anchorPoint.X, anchorPoint.Y)
                    }
                );
                _editMenuState.SetDragging(dragging == 1);
                _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
                var ellipses = frame.Children.OfType<Ellipse>().ToList();
                var tl = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.TL)!;
                var tr = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.TR)!;
                var bl = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.BL)!;
                var br = ellipses.FirstOrDefault(e => e.Name == WindowMapCanvasFrameTypes.BR)!;
                var width = (dragging == 1) ? anchorPoint.X - dragPoint.X : 300.0;
                var height = (dragging == 1) ? anchorPoint.Y - dragPoint.Y : 300.0;
                Debug.Assert(tl.RenderTransform.Value.OffsetX == -tl.Width / 2);
                Debug.Assert(tl.RenderTransform.Value.OffsetY == -tl.Height / 2);
                Debug.Assert(tr.RenderTransform.Value.OffsetX == width - tr.Width / 2);
                Debug.Assert(tr.RenderTransform.Value.OffsetY == -tr.Height / 2);
                Debug.Assert(bl.RenderTransform.Value.OffsetX == -bl.Width / 2);
                Debug.Assert(bl.RenderTransform.Value.OffsetY == height - bl.Height / 2);
                Debug.Assert(br.RenderTransform.Value.OffsetX == width - br.Width / 2);
                Debug.Assert(br.RenderTransform.Value.OffsetY == height - br.Height / 2);
            }
        }

        /**
         * @brief Verifies that the frame label stays correctly positioned during resize
         * 
         * When users resize or move a frame, the text label that identifies the frame
         * should maintain its position relative to the frame (typically at the top-left
         * corner with a small fixed margin).
         */
        private void _testDraggingSelectedFrameAdjustsLabelDimensions()
        {
            var anchorPoints = _anchorPoints();
            var dragPoints = _dragPoints();
            for (int i = 0; i < anchorPoints.Count; i++)
            for (int j = 0; j < dragPoints.Count; j++)
            {
                var anchorPoint = anchorPoints[i];
                var dragPoint = new Point(
                    Math.Min(0, Math.Max(_mapCanvas.Width, dragPoints[j].X)),
                    Math.Min(0, Math.Max(_mapCanvas.Height, dragPoints[j].Y))
                );
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
                _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
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


    public class WindowMapCanvasFrameDataActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private TextBox _frameLabelTextBox = new TextBox();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private AbstractWindowActionHandler _fixture()
        {
            _mapCanvas = new Canvas();
            _frameLabelTextBox = new TextBox();
            _editMenuState = new WindowMapEditMenuState();
            _bottingModel = new BottingModel();
            return new WindowMapCanvasFrameDataActionHandlerFacade(
                _mapCanvas,
                _frameLabelTextBox,
                _editMenuState
            );
        }

        /**
         * @brief Verifies that each added frame receives a unique identifier
         * 
         * When users add multiple frames to the minimap, each frame should have
         * its own unique label and name for identification. This test ensures that
         * frames are properly numbered sequentially (F0, F1, F2, etc.) and that
         * each frame's data is correctly stored in the botting model.
         */
        private void _testAddedFrameDataFormattedWithUniqueTags()
        {
            for (int i = 1; i <= 10; i++)
            {
                var frameDataActionHandler = _fixture();
                frameDataActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
                _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.AddFrame);
                for (int k = 0; k < i; k++)
                {
                    var frame = FrameFixture.GenerateFrame(0, 0, 100, 100, _mapCanvas);
                    _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { DragObject = frame });
                    _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                    Debug.Assert(frame.Tag is MapCanvasRuneFrameDataTag);
                    var tag = (MapCanvasRuneFrameDataTag)frame.Tag;
                    Debug.Assert(tag.ElementLabel == "FT" + k.ToString());
                    Debug.Assert(tag.FrameName == "F" + k.ToString());
                    var runeFrame = _bottingModel.GetRuneModel().FindRuneFrameByName(tag.ElementLabel);
                    Debug.Assert(runeFrame != null);
                    Debug.Assert(runeFrame.FrameData.ElementLabel == "FT" + k.ToString());
                    Debug.Assert(runeFrame.FrameData.FrameName == "F" + k.ToString());
                    Debug.Assert(frame.Children.OfType<TextBlock>().ToList()[0].Text == "F" + k.ToString());
                }
            }
        }

        /**
         * @brief Verifies that frame text elements are properly linked
         * 
         * When users add a frame, the system should link both the on-canvas text block
         * and the external label text box to the frame's data. This ensures that
         * any changes to the label text are properly synchronized and saved.
         */
        private void _testAddedFrameDataFormattedWithTextDependencies()
        {
            var frameDataActionHandler = _fixture();
            frameDataActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.AddFrame);
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var frame = FrameFixture.GenerateFrame(0, 0, 100, 100, _mapCanvas);
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { DragObject = frame });
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var textBlock = frame.Children.OfType<TextBlock>().ToList()[0];
            var tag = (MapCanvasRuneFrameDataTag)frame.Tag;
            var runeFrame = _bottingModel.GetRuneModel().FindRuneFrameByName(tag.ElementLabel);
            Debug.Assert(runeFrame != null);
            Debug.Assert(runeFrame.FrameData.ElementTexts.Count == 2);
            Debug.Assert(runeFrame.FrameData.ElementTexts.Contains(_frameLabelTextBox));
            Debug.Assert(runeFrame.FrameData.ElementTexts.Contains(textBlock));
        }

        /**
         * @brief Verifies that frame position is saved correctly
         * 
         * When users place a frame at a specific location on the minimap, the system
         * should save the X and Y coordinates. This position data is essential for
         * automation that needs to interact with specific areas of the game world.
         */
        private void _testAddedFrameDataFormattedWithFramePosition()
        {
            var frameDataActionHandler = _fixture();
            frameDataActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.AddFrame);
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var frame = FrameFixture.GenerateFrame(123, 234, 100, 100, _mapCanvas);
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { DragObject = frame });
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var textBlock = frame.Children.OfType<TextBlock>().ToList()[0];
            var tag = (MapCanvasRuneFrameDataTag)frame.Tag;
            var runeFrame = _bottingModel.GetRuneModel().FindRuneFrameByName(tag.ElementLabel);
            Debug.Assert(runeFrame != null);
            Debug.Assert(runeFrame.X == 123);
            Debug.Assert(runeFrame.Y == 234);
        }

        /**
         * @brief Verifies that frames are not saved when in Select mode
         * 
         * When users are in "Select Frame" mode (not "Add Frame" mode), clicking
         * on frames should select them for editing but NOT create new frame data.
         * This prevents accidental duplication of frame data when users are just
         * trying to select existing frames.
         */
        private void _testSelectedFrameDataNotFormatted()
        {
            var frameDataActionHandler = _fixture();
            frameDataActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.SelectFrame);
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var frame = FrameFixture.GenerateFrame(123, 234, 100, 100, _mapCanvas);
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { DragObject = frame });
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            Debug.Assert(frame.Tag is null);
            Debug.Assert(_bottingModel.GetRuneModel().RuneFrames().Count == 0);
        }

        public void Run()
        {
            _testAddedFrameDataFormattedWithUniqueTags();
            _testAddedFrameDataFormattedWithTextDependencies();
            _testAddedFrameDataFormattedWithFramePosition();
            _testSelectedFrameDataNotFormatted();
        }
    }


    public class WindowMapCanvasFrameSelectedTextActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private TextBox _selectedTextLabel = new TextBox();

        private TextBox _selectedTextLeft = new TextBox();

        private TextBox _selectedTextTop = new TextBox();

        private TextBox _selectedTextRight = new TextBox();

        private TextBox _selectedTextBottom = new TextBox();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractWindowActionHandler _fixture()
        {
            _mapCanvas = new Canvas();
            _selectedTextLabel = new TextBox();
            _selectedTextLeft = new TextBox();
            _selectedTextTop = new TextBox();
            _selectedTextRight = new TextBox();
            _selectedTextBottom = new TextBox();
            _editMenuState = new WindowMapEditMenuState();
            return new WindowMapCanvasFrameSelectedTextActionHandlerFacade(
                _mapCanvas,
                _selectedTextLabel,
                _selectedTextLeft,
                _selectedTextTop,
                _selectedTextRight,
                _selectedTextBottom,
                _editMenuState
            );
        }

        /**
         * @brief Verifies that selecting a frame populates all property text boxes
         * 
         * When users click on a frame to select it, the system should display all
         * of its properties in the UI for easy viewing and editing. This allows users to
         * see at a glance where their frame is located and how large it is.
         */
        private void _testSelectingFrameUpdatesTexts()
        {
            var _frameSelectedTextActionHandler = _fixture();
            var frame = FrameFixture.GenerateFrame(123, 234, 345, 456, _mapCanvas);
            frame.Tag = new MapCanvasRuneFrameDataTag{ FrameName = "F0" };
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject{ DragObject = frame });
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            Debug.Assert(_selectedTextLabel.Text == "F0");
            Debug.Assert(_selectedTextLeft.Text == "123");
            Debug.Assert(_selectedTextTop.Text == "234");
            Debug.Assert(_selectedTextRight.Text == "468");
            Debug.Assert(_selectedTextBottom.Text == "690");
        }

        /**
         * @brief Verifies that dragging a frame updates or preserves property text boxes
         * appropriately
         * 
         * When users drag a frame to reposition it, the property text boxes should be
         * updated with the frame's values while dragging. When not dragging, the existing
         * text box values should remain unchanged (not cleared or overwritten).
         */
        private void _testDraggingFrameUpdatesTexts()
        {
            for (int i = 0; i < 2; i++)
            {
                var _frameSelectedTextActionHandler = _fixture();
                var frame = FrameFixture.GenerateFrame(123, 234, 345, 456, _mapCanvas);
                frame.Tag = new MapCanvasRuneFrameDataTag { FrameName = "F0" };
                _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { DragObject = frame });
                _editMenuState.SetDragging(i == 1);
                _selectedTextLabel.Text = "1";
                _selectedTextLeft.Text = "2";
                _selectedTextTop.Text = "3";
                _selectedTextRight.Text = "4";
                _selectedTextBottom.Text = "5";
                _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
                Debug.Assert(_selectedTextLabel.Text == (i == 1 ? "F0" : "1"));
                Debug.Assert(_selectedTextLeft.Text == (i == 1 ? "123" : "2"));
                Debug.Assert(_selectedTextTop.Text == (i == 1 ? "234" : "3"));
                Debug.Assert(_selectedTextRight.Text == (i == 1 ? "468" : "4"));
                Debug.Assert(_selectedTextBottom.Text == (i == 1 ? "690" : "5"));
            }
        }

        /**
         * @brief Verifies that clicking outside a frame clears all property text boxes
         * 
         * When users click on empty canvas space (not on any frame), the system
         * should clear all property text boxes. This provides clear visual feedback
         * that no frame is currently selected.
         */
        private void _testSelectingOutsideFrameClearsTexts()
        {
            var _frameSelectedTextActionHandler = _fixture();
            _selectedTextLabel.Text = "123";
            _selectedTextLeft.Text = "123";
            _selectedTextTop.Text = "123";
            _selectedTextRight.Text = "123";
            _selectedTextBottom.Text = "123";
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            Debug.Assert(_selectedTextLabel.Text == "");
            Debug.Assert(_selectedTextLeft.Text == "");
            Debug.Assert(_selectedTextTop.Text == "");
            Debug.Assert(_selectedTextRight.Text == "");
            Debug.Assert(_selectedTextBottom.Text == "");
        }

        public void Run()
        {
            _testSelectingFrameUpdatesTexts();
            _testDraggingFrameUpdatesTexts();
            _testSelectingOutsideFrameClearsTexts();
        }
    }


    public class WindowMapCanvasFrameSelectedDragDataActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private AbstractWindowActionHandler _fixture()
        {
            _mapCanvas = new Canvas();
            _editMenuState = new WindowMapEditMenuState();
            _bottingModel = new BottingModel();
            return new WindowMapCanvasFrameSelectedDragDataActionHandlerFacade(
                _mapCanvas,
                _editMenuState
            );
        }

        /**
         * @brief Verifies that dragging a frame updates the stored frame dimensions
         * in the botting model
         * 
         * When users click and drag a frame to move or resize it, the system should update
         * the corresponding RuneFrame data in the botting model with the new position and size.
         * This ensures that the automation system always has the latest frame coordinates.
         */
        private void _testDraggingFrameUpdatesSelectedData()
        {
            for (int i = 0; i < 2; i++)
            {
                var frameSelectedDragDataActionHandler = _fixture();
                frameSelectedDragDataActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
                var frame = FrameFixture.GenerateFrame(12, 23, 34, 45, _mapCanvas);
                frame.Tag = new MapCanvasRuneFrameDataTag { ElementLabel = "FT0" };
                var runeFrameData = new RuneFrameData { ElementLabel = "FT0" };
                var runeFrame = new RuneFrame
                {
                    X = 1,
                    Y = 1,
                    Width = 1,
                    Height = 1,
                    FrameData = runeFrameData
                };
                _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
                _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { DragObject = frame });
                _editMenuState.SetDragging(i == 1);
                _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
                Debug.Assert(runeFrame.X == (i == 1 ? 12 : 1));
                Debug.Assert(runeFrame.Y == (i == 1 ? 23 : 1));
                Debug.Assert(runeFrame.Width == (i == 1 ? 34 : 1));
                Debug.Assert(runeFrame.Height == (i == 1 ? 45 : 1));
            }
        }

        public void Run()
        {
            _testDraggingFrameUpdatesSelectedData();
        }
    }


    public class WindowMapEditorRuneHandlersTestSuite
    {
        public void Run()
        {
            new MapCanvasAddFramePointButtonActionHandlerTests().Run();
            new MapCanvasRemoveFramePointButtonActionHandlerTests().Run();
            new WindowMapCanvasFrameDrawerActionHandlerTests().Run();
            new WindowMapCanvasFrameSelectStateActionHandlerTests().Run();
            new WindowMapCanvasFrameDragActionHandlerTests().Run();
            new WindowMapCanvasFrameDataActionHandlerTests().Run();
            new WindowMapCanvasFrameSelectedTextActionHandlerTests().Run();
            new WindowMapCanvasFrameSelectedDragDataActionHandlerTests().Run();
        }
    }
}

