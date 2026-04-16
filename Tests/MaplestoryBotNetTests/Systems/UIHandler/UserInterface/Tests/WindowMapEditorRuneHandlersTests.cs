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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;


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
        public static void UpdateFrameworkElement(FrameworkElement frameworkElement)
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
            UpdateFrameworkElement(rectangle);
            UpdateFrameworkElement(tl);
            UpdateFrameworkElement(tr);
            UpdateFrameworkElement(bl);
            UpdateFrameworkElement(br);
            UpdateFrameworkElement(frameCanvas);
            UpdateFrameworkElement(mapCanvas);
            return frameCanvas;
        }

        public static Canvas GenerateFrame(Rect frameRect, Canvas mapCanvas)
        {
            return GenerateFrame(
                frameRect.X,
                frameRect.Y,
                frameRect.Width,
                frameRect.Height,
                mapCanvas
            );
        }
        
        public static Canvas GenerateFramePoint(
            double x,
            double y,
            Canvas frameCanvas
        )
        {
            var framePointFactory = new WindowMapCanvasFramePointFactoryFacade();
            var framePointCanvas = framePointFactory.Create();
            frameCanvas.Children.Add(framePointCanvas);
            Canvas.SetLeft(framePointCanvas, x);
            Canvas.SetTop(framePointCanvas, y);
            UpdateFrameworkElement(framePointCanvas);
            UpdateFrameworkElement(frameCanvas);
            return (Canvas)framePointCanvas;
        }

        public static Canvas GenerateFramePoint(Point framePoint, Canvas frameCanvas)
        {
            return GenerateFramePoint(
                framePoint.X,
                framePoint.Y,
                frameCanvas
            );
        }
    }


    public class RuneFrameFixture
    {
        public static RuneFrame GenerateRuneFrame(
            Rect frameRect,
            FrameworkElement frame,
            string elementLabel,
            string frameName
        )
        {
            frame.Tag = new MapCanvasRuneFrameDataTag
            {
                ElementLabel = elementLabel,
                FrameName = frameName
            };
            var runeFrame = new RuneFrame
            {
                X = frameRect.X,
                Y = frameRect.Y,
                Width = frameRect.Width,
                Height = frameRect.Height,
                FrameData = new RuneFrameData
                {
                    ElementTexts = [],
                    ElementLabel = elementLabel,
                    FrameName = frameName,
                    RuneFrameMacros = []
                }
            };
            return runeFrame;
        }

        public static RuneFrameMacro GenerateRuneFrameMacro(
            Canvas frameCanvas,
            Canvas framePoint,
            string elementLabel,
            string macroName
        )
        {
            framePoint.Tag = elementLabel;
            return new RuneFrameMacro
            {
                MacroName = macroName,
                ElementLabel = elementLabel,
                X = Canvas.GetLeft(framePoint),
                Y = Canvas.GetTop(framePoint),
                ScaleX = framePoint.Width,
                ScaleY = framePoint.Height,
                NextRuneFrame = null,
                Radius = 0.0,
                PointCommands = []
            };
        }

        public static Canvas FramePointFixture(
            Point currPoint,
            Point prevPoint,
            Point scale,
            Canvas frameCanvas,
            RuneFrame runeFrame,
            string elementLabel,
            string macroName
        )
        {
            var framePoint = FrameFixture.GenerateFramePoint(
                currPoint.X,
                currPoint.Y,
                frameCanvas
            );
            framePoint.Tag = elementLabel;
            var runeFrameMacro = GenerateRuneFrameMacro(
                frameCanvas,
                framePoint,
                elementLabel,
                macroName
            );
            runeFrameMacro.X = prevPoint.X;
            runeFrameMacro.Y = prevPoint.Y;
            runeFrameMacro.ScaleX = scale.X;
            runeFrameMacro.ScaleY = scale.Y;
            runeFrame.FrameData.RuneFrameMacros.Add(runeFrameMacro);
            return framePoint;
        }
    }


    public class MapCanvasAddFrameButtonActionHandlerTests
    {
        private ToggleButton _addFrameButton = new ToggleButton();

        private List<ToggleButton> _otherButtons = [];

        private WindowMapEditMenuState _menuState = new WindowMapEditMenuState();

        private AbstractWindowActionHandler _fixture()
        {
            _addFrameButton = new ToggleButton();
            _otherButtons = [new ToggleButton(), new ToggleButton(), new ToggleButton()];
            _menuState = new WindowMapEditMenuState();
            return new WindowMapAddFrameButtonActionHandlerFacade(
                _addFrameButton, _otherButtons, _menuState
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
            _addFrameButton.IsChecked = true;
            _addFrameButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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
            _addFrameButton.IsChecked = false;
            _addFrameButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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
            _addFrameButton.IsChecked = true;
            _addFrameButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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
            _addFrameButton.UpdateLayout();
            _menuState.SetState((int)WindowMapEditMenuStateTypes.Add);
            _addFrameButton.IsChecked = false;
            _addFrameButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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


    public class MapCanvasRemoveFrameButtonActionHandlerTests
    {
        private ToggleButton _removeFrameButton = new ToggleButton();

        private List<ToggleButton> _otherButtons = [];

        private WindowMapEditMenuState _menuState = new WindowMapEditMenuState();

        private AbstractWindowActionHandler _fixture()
        {
            _removeFrameButton = new ToggleButton();
            _otherButtons = [new ToggleButton(), new ToggleButton(), new ToggleButton()];
            _menuState = new WindowMapEditMenuState();
            return new WindowMapRemoveFrameButtonActionHandlerFacade(
                _removeFrameButton, _otherButtons, _menuState
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
            _removeFrameButton.IsChecked = true;
            _removeFrameButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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
            _removeFrameButton.IsChecked = false;
            _removeFrameButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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
            _removeFrameButton.IsChecked = true;
            _removeFrameButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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
            _removeFrameButton.UpdateLayout();
            _menuState.SetState((int)WindowMapEditMenuStateTypes.Add);
            _removeFrameButton.IsChecked = false;
            _removeFrameButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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
            return new WindowMapAddFramePointButtonActionHandlerFacade(
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
            Debug.Assert(_menuState.GetState() == (int)WindowMapEditFrameMenuStateTypes.AddPoint);
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
        private ToggleButton _removePointButton = new ToggleButton();

        private List<ToggleButton> _otherButtons = [];

        private WindowMapEditMenuState _menuState = new WindowMapEditMenuState();

        private AbstractWindowActionHandler _fixture()
        {
            _removePointButton = new ToggleButton();
            _otherButtons = [new ToggleButton(), new ToggleButton(), new ToggleButton()];
            _menuState = new WindowMapEditMenuState();
            return new WindowMapRemoveFramePointButtonActionHandlerFacade(
                _removePointButton, _otherButtons, _menuState
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
            _removePointButton.IsChecked = true;
            _removePointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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
            _removePointButton.IsChecked = false;
            _removePointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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
            _removePointButton.IsChecked = true;
            _removePointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_menuState.GetState() == (int)WindowMapEditFrameMenuStateTypes.RemovePoint);
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
            _removePointButton.UpdateLayout();
            _menuState.SetState((int)WindowMapEditMenuStateTypes.Add);
            _removePointButton.IsChecked = false;
            _removePointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
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
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.AddFrame);
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
         * @brief Verifies that newly created frames have correct visual styling and dimensions
         *
         * When users create a new frame, all visual elements must have consistent and
         * appropriate styling.
         */
        private void _testFrameCreatedWithCorrectAttributes()
        {
            var mapCanvasFrameDrawerActionHandler = _fixture();
            var mouseButtonEventArgs = ButtonClickFixture.Event(_mapCanvas);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.AddFrame);
            _mapCanvas.RaiseEvent(mouseButtonEventArgs);
            var frameCanvas = (Canvas)_mapCanvas.Children[0];
            var ellipses = frameCanvas.Children.OfType<Ellipse>().ToList();
            var rectangle = frameCanvas.Children.OfType<Rectangle>().ToList();
            var textBlock = frameCanvas.Children.OfType<TextBlock>().ToList();
            for (int i = 0; i < ellipses.Count; i++)
            {
                Debug.Assert(ellipses[i].Fill == Brushes.GhostWhite);
                Debug.Assert(ellipses[i].Stroke == Brushes.GhostWhite);
                Debug.Assert(ellipses[i].StrokeThickness == 1);
                Debug.Assert(ellipses[i].Height == 8.0);
                Debug.Assert(ellipses[i].Width == 8.0);
            }
            if (rectangle[0].Fill is SolidColorBrush rectangleFill)
            {
                Debug.Assert(rectangleFill.Color.A == 40);
                Debug.Assert(rectangleFill.Color.R == 0);
                Debug.Assert(rectangleFill.Color.G == 0);
                Debug.Assert(rectangleFill.Color.B == 255);
                Debug.Assert(rectangle[0].Stroke == Brushes.GhostWhite);
                Debug.Assert(rectangle[0].StrokeThickness == 1);
                Debug.Assert(rectangle[0].Width == 0);
                Debug.Assert(rectangle[0].Height == 0);
                Debug.Assert(rectangle[0].Opacity == 1.0);
            }
            Debug.Assert(textBlock[0].Foreground == Brushes.GhostWhite);
            Debug.Assert(textBlock[0].Background == Brushes.Transparent);
            Debug.Assert(textBlock[0].FontFamily.ToString() == "Courier New");
            Debug.Assert(textBlock[0].FontSize == 10.0);
            Debug.Assert(textBlock[0].RenderTransform.Value.OffsetX == 0.0);
            Debug.Assert(textBlock[0].RenderTransform.Value.OffsetY == -16.0);
        }

        /**
         * @brief Verifies that after creating a frame, the system enters dragging mode
         * 
         * When users click to create a new frame, the system should automatically
         * enter a dragging state so they can immediately adjust the frame's position
         * or size without clicking again.
         */
        private void _testFrameDragStateAfterAdding()
        {
            var mapCanvasFrameDrawerActionHandler = _fixture();
            var mouseButtonEventArgs = ButtonClickFixture.Event(_mapCanvas);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.AddFrame);
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
        private void _testFrameSelectedAfterAdding()
        {
            var mapCanvasFrameDrawerActionHandler = _fixture();
            var mouseButtonEventArgs = ButtonClickFixture.Event(_mapCanvas);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.AddFrame);
            Debug.Assert(_editMenuState.Selected() == null);
            _mapCanvas.RaiseEvent(mouseButtonEventArgs);
            Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
            var selected = (WindowMapEditMenuFrameSelectedObject)_editMenuState.Selected()!;
            Debug.Assert(selected.FrameObject == _mapCanvas.Children[0]);
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
            _editMenuState.SetState((int)123);
            _mapCanvas.RaiseEvent(mouseButtonEventArgs);
            Debug.Assert(_mapCanvas.Children.Count == 0);
            Debug.Assert(!_editMenuState.Dragging());
            Debug.Assert(_editMenuState.Selected() == null);
        }

        public void Run()
        {
            _testFrameCreatedAtClickedPoint();
            _testFrameCreatedWithCorrectAttributes();
            _testFrameDragStateAfterAdding();
            _testFrameSelectedAfterAdding();
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

        private List<int> _menuStates()
        {
            return [(int) WindowMapEditFrameMenuStateTypes.SelectFrame, 123];
        }

        /**
         * @brief Verifies center-clicking a frame selects it only in SelectFrame state,
         * with null DragPoint for selection-only mode
         *
         * When users click the interior of a frame, the frame becomes selected without
         * entering drag mode.
         * Expected result: The frame is highlighted and ready for operations like
         * deletion or property editing, but not actively being dragged.
         */
        private void _testSelectingCenterOfFrameSelectsFrame()
        {
            var menuStates = _menuStates();
            for (int i = 0; i < menuStates.Count; i++)
            {
                var frameSelectStateActionHandler = _fixture(250, 250, MouseButtonState.Pressed);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _editMenuState.SetState(menuStates[i]);
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                if (i == 0)
                {
                    Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
                    var selected = (WindowMapEditMenuFrameSelectedObject)_editMenuState.Selected()!;
                    Debug.Assert(selected.FrameObject == frame);
                    Debug.Assert(selected.DragPoint == null);
                    Debug.Assert(selected.PointObject == null);
                }
                else
                {
                    Debug.Assert(_editMenuState.Selected() == null);
                }
            }
        }

        /**
         * @brief Verifies clicking a corner grip selects frame with opposite grip as
         * resize anchor, only works in selection-only mode
         *
         * When users click and drag a corner grip of a selected frame, the opposite
         * corner remains fixed while the frame resizes.
         * Expected result: The frame is in resize mode with the opposite grip
         * identified as the anchor point.
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
            var menuStates = _menuStates();
            for (int i = 0; i < menuStates.Count; i++)
            foreach (var point in clickPoints)
            {
                var frameSelectStateActionHandler = _fixture(point.X, point.Y, MouseButtonState.Pressed);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _editMenuState.SetState(menuStates[i]);
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                if (i == 0)
                {
                    Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
                    var selected = (WindowMapEditMenuFrameSelectedObject)_editMenuState.Selected()!;
                    Debug.Assert(selected.FrameObject == (i == 0 ? frame : null));
                    Debug.Assert(selected.DragPoint!.Item1 == oppositePoints[clickPoints.IndexOf(point)].X);
                    Debug.Assert(selected.DragPoint!.Item2 == oppositePoints[clickPoints.IndexOf(point)].Y);
                    Debug.Assert(selected.PointObject == null);
                }
                else
                {
                    Debug.Assert(_editMenuState.Selected() == null);
                }
            }
        }

        /**
         * @brief Verifies that clicking on a point marker within a frame enters drag mode,
         * only in SelectFrame state
         *
         * When users click on a circular point marker that belongs to a frame, the system
         * should automatically enter dragging mode to allow immediate repositioning of
         * the point. Unlike clicking on a frame center (which only selects without dragging)
         * or clicking a grip (which enables resize dragging), clicking a point enables
         * drag mode for moving that specific point within its parent frame.
         */
        private void _testSelectingFramePointSetsDragState()
        {
            var menuStates = _menuStates();
            for (int i = 0; i < menuStates.Count; i++)
            {
                var frameSelectStateActionHandler = _fixture(200, 200, MouseButtonState.Pressed);
                var frame = FrameFixture.GenerateFrame(150, 150, 200, 200, _mapCanvas);
                var framePoint = new WindowMapCanvasFramePointFactoryFacade().Create();
                frame.Children.Add(framePoint);
                Canvas.SetLeft(framePoint, 48);
                Canvas.SetTop(framePoint, 48);
                FrameFixture.UpdateFrameworkElement(framePoint);
                _editMenuState.SetState(menuStates[i]);
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                Debug.Assert((i == 0) ? _editMenuState.Dragging() : !_editMenuState.Dragging());
            }
        }

        /**
         * @brief Verifies overlapping frames select the topmost frame containing the
         * clicked grip, only works in selection-only mode
         *
         * When users click on an area where multiple frames overlap, the topmost frame
         * that contains that point is selected.
         * Expected result: The front-most frame receives the selection rather than a
         * lower frame that may also claim that point.
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
            var menuStates = _menuStates();
            for (int i = 0; i < menuStates.Count; i++)
            foreach (var point in clickPoints)
            {
                var frameSelectStateActionHandler = _fixture(point.X, point.Y, MouseButtonState.Pressed);
                var frame1 = FrameFixture.GenerateFrame(150, 150, 200, 200, _mapCanvas);
                var frame2 = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                _editMenuState.SetState(menuStates[i]);
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                if (i == 0)
                {
                    Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
                    var selected = (WindowMapEditMenuFrameSelectedObject)_editMenuState.Selected()!;
                    Debug.Assert(selected.FrameObject == frame1);
                    Debug.Assert(selected.DragPoint!.Item1 == oppositePoints[clickPoints.IndexOf(point)].X);
                    Debug.Assert(selected.DragPoint!.Item2 == oppositePoints[clickPoints.IndexOf(point)].Y);
                    Debug.Assert(selected.PointObject == null);
                }
                else
                {
                    Debug.Assert(_editMenuState.Selected() == null);
                }
            }
        }

        /**
         * @brief Verifies clicking empty canvas space deselects current frame,
         * only works in selection-only mode
         *
         * When users click on blank canvas area away from any frame or grip, the
         * currently selected frame becomes deselected.
         * Expected result: No frame remains selected, allowing the user to start a
         * new selection.
         */
        private void _testSelectingOutsideOfFrameDeselectsFrame()
        {
            var menuStates = _menuStates();
            for (int i = 0; i < menuStates.Count; i++)
            {
                var frameSelectStateActionHandler = _fixture(50, 50, MouseButtonState.Pressed);
                var frame = FrameFixture.GenerateFrame(100, 100, 300, 300, _mapCanvas);
                var selectedObject = new WindowMapEditMenuFrameSelectedObject();
                _editMenuState.SetState(menuStates[i]);
                _editMenuState.Select(selectedObject);
                _editMenuState.SetDragging(false);
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                if (i == 0)
                {
                    Debug.Assert(_editMenuState.Selected() == null);
                    Debug.Assert(!_editMenuState.Dragging());
                }
                else
                {
                    Debug.Assert(_editMenuState.Selected() == selectedObject);
                }
            }
        }

        /**
         * @brief Verifies that clicking on a point marker within a frame selects the frame
         * with the specific point identified, only in SelectFrame state
         *
         * When users click on a circular point marker that belongs to a frame, the system
         * should select the parent frame and also record which specific point was clicked.
         * The selection includes both the frame object and the point object,
         * but no drag point since point selection is for editing rather than resizing.
         */
        private void _testSelectingFramePointWhenClicked()
        {
            var menuStates = _menuStates();
            for (int i = 0; i < menuStates.Count; i++)
            {
                var frameSelectStateActionHandler = _fixture(200, 200, MouseButtonState.Pressed);
                var frame = FrameFixture.GenerateFrame(150, 150, 200, 200, _mapCanvas);
                var framePoint = new WindowMapCanvasFramePointFactoryFacade().Create();
                frame.Children.Add(framePoint);
                Canvas.SetLeft(framePoint, 48);
                Canvas.SetTop(framePoint, 48);
                FrameFixture.UpdateFrameworkElement(framePoint);
                _editMenuState.SetState(menuStates[i]);
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                if (i == 0)
                {
                    Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
                    var selected = (WindowMapEditMenuFrameSelectedObject)_editMenuState.Selected()!;
                    Debug.Assert(selected.FrameObject == frame);
                    Debug.Assert(selected.PointObject == framePoint);
                    Debug.Assert(selected.DragPoint == null);
                }
                else
                {
                    Debug.Assert(_editMenuState.Selected() == null);
                }
            }
        }

        /**
         * @brief Verifies releasing mouse button exits drag mode while keeping frame
         * selected
         *
         * When users release the mouse button after dragging or resizing a frame, the
         * drag operation ends.
         * Expected result: The frame remains selected but is no longer in drag mode,
         * ready for the next user action.
         */
        private void _testReleasingMouseUnsetsDragState()
        {
            var frameSelectStateActionHandler = _fixture(50, 50, MouseButtonState.Released);
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
            _testSelectingFramePointSetsDragState();
            _testSelectingGripUnderFrameSetsDragState();
            _testSelectingOutsideOfFrameDeselectsFrame();
            _testSelectingFramePointWhenClicked();
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
                        FrameObject = frame,
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
                        FrameObject = frame,
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
                        FrameObject = frame,
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
                        FrameObject = frame,
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
                    _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
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
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
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
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
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
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
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
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject{ FrameObject = frame });
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
                _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
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
                var runeFrame = new RuneFrame { X = 1, Y = 1, Width = 1, Height = 1, FrameData = runeFrameData };
                _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
                _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
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


    public class WindowMapCanvasFrameRemoveActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private MockMouseEventDataExtractor _mousePositionExtractor = new MockMouseEventDataExtractor();

        private BottingModel _bottingModel = new BottingModel();

        private AbstractWindowActionHandler _fixture()
        {
            _mapCanvas = new Canvas();
            _editMenuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MockMouseEventDataExtractor();
            _bottingModel = new BottingModel();
            return new WindowMapCanvasFrameRemoveActionHandlerFacade(
                _mapCanvas,
                _editMenuState,
                _mousePositionExtractor
            );
        }

        private List<Point> _removeClickPositions()
        {
            return [
                new Point(150, 150),
                new Point(100, 100),
                new Point(300, 100),
                new Point(100, 300),
                new Point(300, 300),
            ];
        }

        /**
         * @brief Verifies clicking on a frame removes both the visual frame from canvas
         * and its corresponding data model when in RemoveFrame state, while preserving
         * the current selection state
         *
         * When users click on any area of a frame (center, edges) while the edit menu is
         * in RemoveFrame state, the frame is deleted from the canvas and its associated
         * RuneFrame is removed from the botting model. The currently selected frame
         * remains selected and unchanged, as removal of a different frame should not
         * affect the existing selection.
         */
        private void _testClickingFrameRemovesClickedFrame()
        {
            var clickPositions = _removeClickPositions();
            foreach (var clickPosition in clickPositions)
            {
                var frameRemoveActionHandler = _fixture();
                frameRemoveActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
                var frame = FrameFixture.GenerateFrame(100, 100, 200, 200, _mapCanvas);
                var selectedFrame = FrameFixture.GenerateFrame(0, 0, 10, 10, _mapCanvas);
                var selectedObject = new WindowMapEditMenuFrameSelectedObject { FrameObject = selectedFrame };
                frame.Tag = new MapCanvasRuneFrameDataTag { ElementLabel = "FT0" };
                var runeFrame = new RuneFrame { FrameData = new RuneFrameData { ElementLabel = "FT0" } };
                _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
                _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.RemoveFrame);
                _editMenuState.Select(selectedObject);
                _mousePositionExtractor.GetPositionReturn.Add(clickPosition);
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                Debug.Assert(_mapCanvas.Children.Count == 1);
                Debug.Assert(_mapCanvas.Children[0] == selectedFrame);
                Debug.Assert(_bottingModel.GetRuneModel().RuneFrames().Count == 0);
                Debug.Assert(_editMenuState.Selected() == selectedObject);
                Debug.Assert(selectedObject.FrameObject == selectedFrame);
            }
        }

        /**
         * @brief Verifies that when a frame is removed, it is automatically deselected
         * from the edit menu state
         *
         * When users click on a frame to remove it while the edit menu is in RemoveFrame
         * state, and that frame is currently selected, the system clears the selection
         * after removal since the selected frame no longer exists on the canvas.
         */
        private void _testClickingFrameDeselectsRemovedFrame()
        {
            var frameRemoveActionHandler = _fixture();
            frameRemoveActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
            var frame = FrameFixture.GenerateFrame(100, 100, 100, 100, _mapCanvas);
            frame.Tag = new MapCanvasRuneFrameDataTag { ElementLabel = "FT0" };
            var runeFrame = new RuneFrame { FrameData = new RuneFrameData { ElementLabel = "FT0" } };
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.RemoveFrame);
            _mousePositionExtractor.GetPositionReturn.Add(new Point(150, 150));
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            Debug.Assert(_editMenuState.Selected() == null);
        }

        /**
         * @brief Verifies that removing a frame also clears all references to that frame
         * from other frames' macros
         *
         * When users remove a frame that is referenced by another frame's NextRuneFrame
         * macro, those references are set to null to prevent orphaned references.
         * Expected result: Any macro that previously pointed to the removed frame now
         * contains a null reference instead of a dangling reference.
         */
        private void _testClickingFrameRemovesFrameReferences()
        {
            var frameRemoveActionHandler = _fixture();
            frameRemoveActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
            var frame1 = FrameFixture.GenerateFrame(100, 100, 100, 100, _mapCanvas);
            var frame2 = FrameFixture.GenerateFrame(200, 100, 100, 100, _mapCanvas);
            frame1.Tag = new MapCanvasRuneFrameDataTag { ElementLabel = "FT0" };
            frame2.Tag = new MapCanvasRuneFrameDataTag { ElementLabel = "FT1" };
            var runeFrame1 = new RuneFrame
            {
                FrameData = new RuneFrameData { ElementLabel = "FT0" }
            };
            var runeFrame2 = new RuneFrame
            {
                FrameData = new RuneFrameData
                {
                    ElementLabel = "FT0",
                    RuneFrameMacros = [new RuneFrameMacro{NextRuneFrame = runeFrame1}]
                }
            };
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame1);
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame2);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.RemoveFrame);
            _mousePositionExtractor.GetPositionReturn.Add(new Point(150, 150));
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            Debug.Assert(runeFrame2.FrameData.RuneFrameMacros[0].NextRuneFrame == null);
        }

        /**
         * @brief Verifies that when multiple frames overlap at the click position,
         * only the topmost frame is removed
         *
         * When users click on an area where multiple frames overlap in RemoveFrame mode,
         * the system removes the topmost frame (lowest Z-order) at that position.
         * Expected result: The top frame remains on the canvas and in the data model,
         * while only the bottom frame is deleted.
         */
        private void _testClickingFrameRemovesTopFrame()
        {
            var frameRemoveActionHandler = _fixture();
            frameRemoveActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
            var frame1 = FrameFixture.GenerateFrame(100, 100, 100, 100, _mapCanvas);
            var frame2 = FrameFixture.GenerateFrame(100, 100, 100, 100, _mapCanvas);
            Panel.SetZIndex(frame1, 0);
            Panel.SetZIndex(frame2, 1);
            frame1.Tag = new MapCanvasRuneFrameDataTag { ElementLabel = "FT0" };
            frame2.Tag = new MapCanvasRuneFrameDataTag { ElementLabel = "FT1" };
            var runeFrame1 = new RuneFrame { FrameData = new RuneFrameData { ElementLabel = "FT0" } };
            var runeFrame2 = new RuneFrame { FrameData = new RuneFrameData { ElementLabel = "FT1" } };
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame1);
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame2);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.RemoveFrame);
            _mousePositionExtractor.GetPositionReturn.Add(new Point(150, 150));
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            Debug.Assert(_mapCanvas.Children.Count == 1);
            Debug.Assert(_mapCanvas.Children[0] == frame2);
            Debug.Assert(_bottingModel.GetRuneModel().RuneFrames().Count == 1);
            Debug.Assert(_bottingModel.GetRuneModel().RuneFrames()[0].FrameData.ElementLabel == "FT1");
        }

        /**
         * @brief Verifies that frame removal only occurs when edit menu is in
         * RemoveFrame state
         *
         * When users click on a frame while the edit menu is in any state other than
         * RemoveFrame, the frame remains on the canvas and the data model is unchanged.
         * Expected result: The frame persists in both the canvas children collection
         * and the rune model after clicking.
         */
        private void _testClickingFrameDoesntRemoveWhenNotRemoving()
        {
            var frameRemoveActionHandler = _fixture();
            frameRemoveActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
            var frame = FrameFixture.GenerateFrame(100, 100, 100, 100, _mapCanvas);
            frame.Tag = new MapCanvasRuneFrameDataTag { ElementLabel = "FT0" };
            var runeFrame = new RuneFrame { FrameData = new RuneFrameData { ElementLabel = "FT0" } };
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
            _editMenuState.SetState(123);
            _mousePositionExtractor.GetPositionReturn.Add(new Point(150, 150));
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            Debug.Assert(_mapCanvas.Children.Count == 1);
            Debug.Assert(_mapCanvas.Children[0] == frame);
            Debug.Assert(_bottingModel.GetRuneModel().RuneFrames().Count == 1);
            Debug.Assert(_bottingModel.GetRuneModel().RuneFrames()[0].FrameData.ElementLabel == "FT0");
        }

        /**
         * @brief Verifies that clicking on empty canvas space does not remove any frame
         * when in RemoveFrame state
         *
         * When users click on an area of the canvas that is not occupied by any frame
         * while the edit menu is in RemoveFrame state, no deletion occurs and all
         * frames remain intact on the canvas and in the data model.
         */
        private void _testClickingFrameOutsideDoesntRemoveFrame()
        {
            var frameRemoveActionHandler = _fixture();
            frameRemoveActionHandler.Inject(SystemInjectType.BottingModel, _bottingModel);
            var frame = FrameFixture.GenerateFrame(100, 100, 100, 100, _mapCanvas);
            frame.Tag = new MapCanvasRuneFrameDataTag { ElementLabel = "FT0" };
            var runeFrame = new RuneFrame { FrameData = new RuneFrameData { ElementLabel = "FT0" } };
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.RemoveFrame);
            _mousePositionExtractor.GetPositionReturn.Add(new Point(300, 300));
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            Debug.Assert(_mapCanvas.Children.Count == 1);
            Debug.Assert(_mapCanvas.Children[0] == frame);
            Debug.Assert(_bottingModel.GetRuneModel().RuneFrames().Count == 1);
            Debug.Assert(_bottingModel.GetRuneModel().RuneFrames()[0].FrameData.ElementLabel == "FT0");
        }

        public void Run()
        {
            _testClickingFrameRemovesClickedFrame();
            _testClickingFrameDeselectsRemovedFrame();
            _testClickingFrameRemovesFrameReferences();
            _testClickingFrameRemovesTopFrame();
            _testClickingFrameDoesntRemoveWhenNotRemoving();
            _testClickingFrameOutsideDoesntRemoveFrame();
        }
    }


    public class WindowMapCanvasFrameButtonAccessActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private List<ButtonBase> _accessButtons = [];

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractWindowActionHandler _fixture()
        {
            _mapCanvas = new Canvas();
            _accessButtons = [new Button(), new Button(), new Button()];
            _editMenuState = new WindowMapEditMenuState();
            return new WindowMapCanvasFrameButtonAccessActionHandlerFacade(
                _mapCanvas, _accessButtons, _editMenuState
            );
        }

        /**
         * @brief Verifies that access buttons become enabled when a frame is selected
         *
         * When a user selects a frame on the canvas, any associated action buttons
         * (such as edit, delete, or properties buttons) should become enabled to allow
         * operations on the selected frame. The buttons remain disabled until a frame
         * is selected.
         */
        private void _testAccessButtonsEnabledOnSelect()
        {
            var frameButtonAccessActionHandler = _fixture();
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject());
            foreach (var button in _accessButtons)
            {
                button.IsEnabled = false;
            }
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            foreach (var button in _accessButtons)
            {
                Debug.Assert(button.IsEnabled);
            }
        }

        /**
         * @brief Verifies that access buttons become disabled when no frame is selected
         *
         * When a user deselects a frame (by clicking empty canvas space or removing
         * the selected frame), any associated action buttons should become disabled
         * since there is no active frame to operate on. This prevents invalid actions
         * on non-existent selections.
         */
        private void _testAccessButtonsDisabledOnDeselect()
        {
            var frameButtonAccessActionHandler = _fixture();
            foreach (var button in _accessButtons)
            {
                button.IsEnabled = true;
            }
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            foreach (var button in _accessButtons)
            {
                Debug.Assert(!button.IsEnabled);
            }
        }

        public void Run()
        {
            _testAccessButtonsEnabledOnSelect();
            _testAccessButtonsDisabledOnDeselect();
        }
    }


    public class WindowMapCanvasFramePointDrawerActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private MockMouseEventDataExtractor _mousePositionExtractor = new MockMouseEventDataExtractor();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private AbstractWindowActionHandler _fixture(double x, double y)
        {
            _mapCanvas = new Canvas { Width = 300, Height = 300 };
            _editMenuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MockMouseEventDataExtractor();
            _mousePositionExtractor.GetPositionReturn.Add(new Point(x, y));
            _bottingModel = new BottingModel();
            var handler = new WindowMapCanvasFramePointDrawerActionHandlerFacade(
                _mapCanvas,
                _editMenuState,
                _mousePositionExtractor
            );
            handler.Inject(SystemInjectType.BottingModel, _bottingModel);
            return handler;
        }

        private List<int> _editMenuStates()
        {
            return [
                (int) WindowMapEditFrameMenuStateTypes.AddPoint, 123
            ];
        }

        /**
         * @brief Verifies that clicking inside a selected frame while in AddPoint mode
         * creates a point marker at the relative click location within the frame
         *
         * When users click inside a selected frame while the edit menu is in AddPoint
         * state, the system creates a visual point marker (Canvas) positioned relative
         * to the frame's top-left corner. The point's coordinates are local.
         */
        private void _testClickingSelectedFrameAddsPointAtClickedLocation()
        {
            var editMenuStates = _editMenuStates();
            foreach (var state in editMenuStates)
            {
                var frameRect = new Rect(100, 100, 123, 234);
                var framePointDrawerActionHandler = _fixture(150, 150);
                var frame = FrameFixture.GenerateFrame(frameRect, _mapCanvas);
                var runeFrame = RuneFrameFixture.GenerateRuneFrame(frameRect, frame, "FT0", "F0");
                _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
                _editMenuState.SetState(state);
                _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                var frameCanvases = frame.Children.OfType<Canvas>();
                if (state == (int) WindowMapEditFrameMenuStateTypes.AddPoint)
                {
                    Debug.Assert(frameCanvases.Count() == 1);
                    if (frameCanvases.First() is Canvas framePoint)
                    {
                        Debug.Assert(framePoint.Name is WindowMapCanvasFrameTypes.POINT);
                        Debug.Assert(Canvas.GetLeft(framePoint) == 50);
                        Debug.Assert(Canvas.GetTop(framePoint) == 50);
                        var macros = runeFrame.FrameData.RuneFrameMacros;
                        Debug.Assert(macros.Count == 1);
                        Debug.Assert(macros[0].X == 50);
                        Debug.Assert(macros[0].Y == 50);
                        Debug.Assert(macros[0].ScaleX == 123);
                        Debug.Assert(macros[0].ScaleY == 234);
                    }
                }
                else
                {
                    Debug.Assert(frameCanvases.Count() == 0);
                }
            }
        }

        /**
         * @brief Verifies that when a point is added to a selected frame, the new point
         * becomes automatically selected and enters drag mode, only in AddPoint state
         *
         * When users add a point to a selected frame while the edit menu is in AddPoint
         * state, the system should automatically select the newly created point and
         * enter dragging mode. This allows users to immediately reposition the point
         * after creation without requiring an additional click to select it.
         */
        private void _testClickingSelectedFrameSelectsAddedFramePoint()
        {
            var editMenuStates = _editMenuStates();
            foreach (var state in editMenuStates)
            {
                var frameRect = new Rect(100, 100, 123, 234);
                var framePointDrawerActionHandler = _fixture(150, 150);
                var frame = FrameFixture.GenerateFrame(frameRect, _mapCanvas);
                var runeFrame = RuneFrameFixture.GenerateRuneFrame(frameRect, frame, "FT0", "F0");
                _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
                _editMenuState.SetState(state);
                _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
                _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                var frameCanvases = frame.Children.OfType<Canvas>();
                if (state == (int)WindowMapEditFrameMenuStateTypes.AddPoint)
                {
                    Debug.Assert(frameCanvases.Count() == 1);
                    if (frameCanvases.First() is Canvas framePoint)
                    {
                        Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
                        var selectedObject = (WindowMapEditMenuFrameSelectedObject)_editMenuState.Selected()!;
                        Debug.Assert(selectedObject.FrameObject == frame);
                        Debug.Assert(selectedObject.DragPoint == null);
                        Debug.Assert(selectedObject.PointObject == framePoint);
                        Debug.Assert(_editMenuState.Dragging());
                    }
                }
                else
                {
                    Debug.Assert(frameCanvases.Count() == 0);
                    Debug.Assert(_editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject);
                    var selectedObject = (WindowMapEditMenuFrameSelectedObject)_editMenuState.Selected()!;
                    Debug.Assert(selectedObject.FrameObject == frame);
                    Debug.Assert(selectedObject.DragPoint == null);
                    Debug.Assert(selectedObject.PointObject == null);
                    Debug.Assert(!_editMenuState.Dragging());
                }
            }
        }

        /**
         * @brief Verifies that point markers created in AddPoint mode have the correct
         * visual appearance for their grip (center circle)
         *
         * When users add a point to a selected frame, the point marker's circular grip
         * (center dot) must have consistent styling.
         */
        private void _testClickingSelectedFrameAddsCorrectPointGrip()
        {
            var frameRect = new Rect(100, 100, 100, 100);
            var framePointDrawerActionHandler = _fixture(150, 150);
            var frame = FrameFixture.GenerateFrame(frameRect, _mapCanvas);
            var runeFrame = RuneFrameFixture.GenerateRuneFrame(frameRect, frame, "FT0", "F0");
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
            _editMenuState.SetState((int) WindowMapEditFrameMenuStateTypes.AddPoint);
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var frameCanvases = frame.Children.OfType<Canvas>();
            var pointEllipses = frameCanvases.First().Children.OfType<Ellipse>();
            Debug.Assert(pointEllipses.Count() == 1);
            if (
                pointEllipses.First() is Ellipse pointEllipse
                && pointEllipse.Fill is SolidColorBrush pointEllipseFill
            )
            {
                Debug.Assert(pointEllipseFill.Color.A == 255);
                Debug.Assert(pointEllipseFill.Color.R == 0);
                Debug.Assert(pointEllipseFill.Color.G == 255);
                Debug.Assert(pointEllipseFill.Color.B == 0);
                Debug.Assert(pointEllipse.Stroke == Brushes.Transparent);
                Debug.Assert(pointEllipse.StrokeThickness == 1);
                Debug.Assert(pointEllipse.Width == 8);
                Debug.Assert(pointEllipse.Height == 8);
            }
        }

        /**
         * @brief Verifies that point markers created in AddPoint mode have the correct
         * visual appearance for their label
         *
         * When users add a point to a selected frame, the point marker's text label
         * must have consistent styling.
         */
        private void _testClickingSelectedFrameAddsCorrectPointLabel()
        {
            var frameRect = new Rect(100, 100, 100, 100);
            var framePointDrawerActionHandler = _fixture(150, 150);
            var frame = FrameFixture.GenerateFrame(frameRect, _mapCanvas);
            var runeFrame = RuneFrameFixture.GenerateRuneFrame(frameRect, frame, "FT0", "F0");
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
            _editMenuState.SetState((int) WindowMapEditFrameMenuStateTypes.AddPoint);
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var frameCanvases = frame.Children.OfType<Canvas>();
            var pointLabels = frameCanvases.First().Children.OfType<TextBlock>();
            Debug.Assert(pointLabels.Count() == 1);
            if (pointLabels.First() is TextBlock pointLabel)
            {
                Debug.Assert(pointLabel.FontFamily.ToString() == "Courier New");
                Debug.Assert(pointLabel.FontSize == 10);
                Debug.Assert(pointLabel.RenderTransform.Value.OffsetX == 0);
                Debug.Assert(pointLabel.RenderTransform.Value.OffsetY == -16);
                Debug.Assert(pointLabel.Foreground == Brushes.GhostWhite);
                Debug.Assert(pointLabel.Background == Brushes.Transparent);
            }
        }

        /**
         * @brief Verifies that clicking inside an unselected frame does not create a point
         * marker, even when in AddPoint mode
         *
         * When users click inside a frame that is not currently selected while the edit
         * menu is in AddPoint state, the system should not add a point to that frame.
         * Points should only be added to the frame that is actively selected, preventing
         * accidental point creation in non-target frames.
         */
        private void _testClickingUnselectedFrameDoesntAddPointAtClickedLocation()
        {
            var framePointDrawerActionHandler = _fixture(100, 100);
            var frame1 = FrameFixture.GenerateFrame(new Rect(50, 50, 100, 100), _mapCanvas);
            var frame2 = FrameFixture.GenerateFrame(new Rect(150, 150, 100, 100), _mapCanvas);
            var runeFrame1 = RuneFrameFixture.GenerateRuneFrame(new Rect(50, 50, 100, 100), frame1, "FT0", "F0");
            var runeFrame2 = RuneFrameFixture.GenerateRuneFrame(new Rect(150, 150, 100, 100), frame2, "FT1", "F1");
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame1);
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame2);
            _editMenuState.SetState((int) WindowMapEditFrameMenuStateTypes.AddPoint);
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame2 });
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            Debug.Assert(_mapCanvas.Children.OfType<Canvas>().Count() == 2);
            Debug.Assert(frame1.Children.OfType<Canvas>().Count() == 0);
            Debug.Assert(frame2.Children.OfType<Canvas>().Count() == 0);
        }

        /**
         * @brief Verifies that clicking on empty canvas space does not create a point
         * marker, even when a frame is selected and in AddPoint mode
         *
         * When users click on blank canvas area away from any frame while the edit menu
         * is in AddPoint state and a frame is selected, the system should not create
         * a point marker. Points must be created exclusively within the bounds of the
         * selected frame to maintain valid point-frame associations.
         */
        private void _testClickingEmptyCanvasDoesntAddPointAtClickedLocation()
        {
            var framePointDrawerActionHandler = _fixture(100, 100);
            var frame = FrameFixture.GenerateFrame(new Rect(150, 150, 100, 100), _mapCanvas);
            var runeFrame = RuneFrameFixture.GenerateRuneFrame(new Rect(150, 150, 100, 100), frame, "FT0", "F0");
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
            _editMenuState.SetState((int) WindowMapEditFrameMenuStateTypes.AddPoint);
            _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            Debug.Assert(_mapCanvas.Children.OfType<Canvas>().Count() == 1);
            Debug.Assert(frame.Children.OfType<Canvas>().Count() == 0);
        }

        /**
         * @brief Verifies that multiple points added to the same frame receive unique
         * sequential macro names (M0, M1, M2, etc.)
         *
         * When users add multiple points to a selected frame in AddPoint mode, each
         * point should be assigned a unique macro name following a sequential pattern
         * (M0 for the first point, M1 for the second, etc.). This ensures each point
         * can be uniquely identified and referenced.
         */
        private void _testClickingSelectedFrameAddsUniqueFramePointMacroNames()
        {
            for (int i = 1; i <= 10; i++)
            {
                var framePointDrawerActionHandler = _fixture(100, 100);
                var frame = FrameFixture.GenerateFrame(new Rect(50, 50, 100, 100), _mapCanvas);
                var runeFrame = RuneFrameFixture.GenerateRuneFrame(new Rect(150, 150, 100, 100), frame, "FT0", "F0");
                _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
                _editMenuState.SetState((int) WindowMapEditFrameMenuStateTypes.AddPoint);
                _editMenuState.Select(new WindowMapEditMenuFrameSelectedObject { FrameObject = frame });
                for (int j = 0; j < i; j++)
                {
                    _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
                    _mousePositionExtractor.GetPositionReturn.Add(new Point(100, 100));
                }
                var framePoints = frame.Children.OfType<Canvas>();
                Debug.Assert(framePoints.Count() == i);
                for (int j = 0; j < i; j++)
                {
                    var point = framePoints.ElementAt(j);
                    var textBlock = point.Children.OfType<TextBlock>().ToList()[0];
                    var js = j.ToString();
                    var macros = runeFrame.FrameData.RuneFrameMacros;
                    var macro = macros.Find((m) => m.MacroName == "M" + js && m.ElementLabel == "MT" + js);
                    Debug.Assert(point.Tag is string);
                    var pointTag = (string)point.Tag;
                    Debug.Assert(textBlock.Text == "M" + js);
                    Debug.Assert(pointTag == "MT" + js);
                    Debug.Assert(macro != null);
                }
            }
        }

        public void Run()
        {
            _testClickingSelectedFrameAddsPointAtClickedLocation();
            _testClickingSelectedFrameSelectsAddedFramePoint();
            _testClickingSelectedFrameAddsCorrectPointGrip();
            _testClickingSelectedFrameAddsCorrectPointLabel();
            _testClickingSelectedFrameAddsUniqueFramePointMacroNames();
            _testClickingUnselectedFrameDoesntAddPointAtClickedLocation();
            _testClickingEmptyCanvasDoesntAddPointAtClickedLocation();
        }
    }


    public class WindowMapCanvasFramePointDragActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private MockMouseEventDataExtractor _mousePositionExtractor = new MockMouseEventDataExtractor();

        private AbstractBottingModel _bottingModel = new BottingModel();

        public AbstractWindowActionHandler _fixture(double x, double y)
        {
            _mapCanvas = new Canvas { Width = 500, Height = 500 };
            _editMenuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MockMouseEventDataExtractor();
            _mousePositionExtractor.GetPositionReturn.Add(new Point(x, y));
            _bottingModel = new BottingModel();
            var handler = new WindowMapCanvasFramePointDragActionHandlerFacade(
                _mapCanvas,
                _editMenuState,
                _mousePositionExtractor
            );
            handler.Inject(SystemInjectType.BottingModel, _bottingModel);
            return handler;
        }

        private WindowMapEditMenuFrameSelectedObject _selectedObject(
            Canvas frameCanvas, Canvas? framePoint
        )
        {
            return new WindowMapEditMenuFrameSelectedObject
            {
                FrameObject = frameCanvas,
                DragPoint = null,
                PointObject = framePoint
            };
        }

        /**
         * @brief Verifies that dragging a selected frame point moves both the visual point
         * and its corresponding macro data to the new drag position
         *
         * When users drag a selected point marker within a frame while in drag mode,
         * the system should update both the visual position of the point 
         * and the underlying macro coordinates in the rune frame's macro list.
         */
        private void _testDraggingFramePointMovesFramePointToDragPosition()
        {
            var frameRect = new Rect(100, 100, 100, 100);
            var framePointDragActionHandler = _fixture(125, 126);
            var frameCanvas = FrameFixture.GenerateFrame(frameRect, _mapCanvas);
            var framePoint = FrameFixture.GenerateFramePoint(50, 50, frameCanvas);
            var runeFrame = RuneFrameFixture.GenerateRuneFrame(frameRect, frameCanvas, "FT0", "F0");
            var runeFrameMacro = RuneFrameFixture.GenerateRuneFrameMacro(frameCanvas, framePoint, "MT0", "M0");
            runeFrame.FrameData.RuneFrameMacros.Add(runeFrameMacro);
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
            var selectedObject = _selectedObject(frameCanvas, framePoint);
            _editMenuState.Select(selectedObject);
            _editMenuState.SetDragging(true);
            _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
            Debug.Assert(runeFrame.FrameData.RuneFrameMacros.Count == 1);
            Debug.Assert(Canvas.GetLeft(framePoint) == 25);
            Debug.Assert(Canvas.GetTop(framePoint) == 26);
            Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].X == 25);
            Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].Y == 26);
        }

        /**
         * @brief Verifies that when no point is selected (PointObject is null), dragging
         * does not move any frame point
         *
         * When users drag on the canvas while in drag mode but no specific point is
         * selected (e.g., they are dragging a frame or dragging on empty space), the
         * system should not move any point marker. The frame point's position and its
         * associated macro data should remain unchanged.
         */
        private void _testDraggingFramePointKeepsFrameAtSamePosition()
        {
            var frameRect = new Rect(100, 100, 100, 100);
            var framePointDragActionHandler = _fixture(125, 125);
            var frameCanvas = FrameFixture.GenerateFrame(frameRect, _mapCanvas);
            var framePoint = FrameFixture.GenerateFramePoint(50, 51, frameCanvas);
            var runeFrame = RuneFrameFixture.GenerateRuneFrame(frameRect, frameCanvas, "FT0", "F0");
            var runeFrameMacro = RuneFrameFixture.GenerateRuneFrameMacro(frameCanvas, framePoint, "MT0", "M0");
            runeFrame.FrameData.RuneFrameMacros.Add(runeFrameMacro);
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
            var selectedObject = _selectedObject(frameCanvas, null);
            _editMenuState.Select(selectedObject);
            _editMenuState.SetDragging(false);
            _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
            Debug.Assert(runeFrame.FrameData.RuneFrameMacros.Count == 1);
            Debug.Assert(Canvas.GetLeft(framePoint) == 50);
            Debug.Assert(Canvas.GetTop(framePoint) == 51);
            Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].X == 50);
            Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].Y == 51);
        }

        /**
         * @brief Verifies that dragging a frame point outside the parent frame's bounds
         * is prevented, keeping the point constrained within the frame
         *
         * When users attempt to drag a point marker beyond the boundaries of its parent
         * frame, the system should prevent the point from leaving the frame. The point
         * should remain at its original position rather than moving to an invalid
         * location outside the frame.
         */
        private void _testDraggingFramePointOutOfFrameIsPrevented()
        {
            var frameRect = new Rect(100, 100, 100, 100);
            var framePointDragActionHandler = _fixture(50, 50);
            var frameCanvas = FrameFixture.GenerateFrame(frameRect, _mapCanvas);
            var framePoint = FrameFixture.GenerateFramePoint(50, 51, frameCanvas);
            var runeFrame = RuneFrameFixture.GenerateRuneFrame(frameRect, frameCanvas, "FT0", "F0");
            var runeFrameMacro = RuneFrameFixture.GenerateRuneFrameMacro(frameCanvas, framePoint, "MT0", "M0");
            runeFrame.FrameData.RuneFrameMacros.Add(runeFrameMacro);
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame);
            var selectedObject = _selectedObject(frameCanvas, null);
            _editMenuState.Select(selectedObject);
            _editMenuState.SetDragging(true);
            _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
            Debug.Assert(runeFrame.FrameData.RuneFrameMacros.Count == 1);
            Debug.Assert(Canvas.GetLeft(framePoint) == 50);
            Debug.Assert(Canvas.GetTop(framePoint) == 51);
            Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].X == 50);
            Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].Y == 51);
        }

        public void Run()
        {
            _testDraggingFramePointMovesFramePointToDragPosition();
            _testDraggingFramePointKeepsFrameAtSamePosition();
            _testDraggingFramePointOutOfFrameIsPrevented();
        }
    }


    public class WindowMapCanvasFramePointScaleActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private Canvas _frameCanvas = new Canvas();

        private RuneFrame _runeFrame = new RuneFrame();

        private Canvas _framePoint1 = new Canvas();

        private Canvas _framePoint2 = new Canvas();

        private AbstractWindowActionHandler _fixture(
            double x, double y, double width, double height
        )
        {
            var frameRect = new Rect(x, y, width, height);
            _mapCanvas = new Canvas();
            _editMenuState = new WindowMapEditMenuState();
            _bottingModel = new BottingModel();
            _frameCanvas = FrameFixture.GenerateFrame(frameRect, _mapCanvas);
            _runeFrame = RuneFrameFixture.GenerateRuneFrame(frameRect, _frameCanvas, "FT0", "F0");
            _framePoint1 = RuneFrameFixture.FramePointFixture(
                new Point(123, 234), new Point(12, 34), new Point(56, 78),
                _frameCanvas, _runeFrame, "MT0", "M0"
            );
            _framePoint2 = RuneFrameFixture.FramePointFixture(
                new Point(123, 234), new Point(23, 45), new Point(67, 89),
                _frameCanvas, _runeFrame, "MT1", "M1"
            );
            var handler = new WindowMapCanvasFramePointScaleActionHandlerFacade(
                _mapCanvas, _editMenuState
            );
            _bottingModel.GetRuneModel().AddRuneFrame(_runeFrame);
            handler.Inject(SystemInjectType.BottingModel, _bottingModel);
            return handler;
        }

        private WindowMapEditMenuFrameSelectedObject _selectedObject(
            Canvas frameCanvas, Tuple<double, double>? dragPoint
        )
        {
            return new WindowMapEditMenuFrameSelectedObject
            {
                FrameObject = frameCanvas,
                DragPoint = dragPoint,
                PointObject = null
            };
        }

        /**
         * @brief Verifies that when a frame is resized (scaled), all point markers within
         * the frame have their positions proportionally scaled and their scale factors reset
         *
         * When users drag a corner grip to resize a frame while in drag mode, the system
         * should proportionally scale the positions of all child points within that frame.
         * The points' coordinates are recalculated based on the new frame dimensions,
         * while their individual scale factors (ScaleX, ScaleY) are reset to match the
         * new frame size (100, 100 in this test).
         */
        private void _testScalingFrameScalesFramePointPosition()
        {
            var framePointScaleActionHandler = _fixture(100, 100, 100, 100);
            var selectedObject = _selectedObject(_frameCanvas, new Tuple<double, double>(0, 0));
            _editMenuState.Select(selectedObject);
            _editMenuState.SetDragging(true);
            _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
            var runeFrameMacro1 = _runeFrame.FrameData.RuneFrameMacros.Find((m) => m.ElementLabel == "MT0")!;
            var runeFrameMacro2 = _runeFrame.FrameData.RuneFrameMacros.Find((m) => m.ElementLabel == "MT1")!;
            Debug.Assert(Math.Abs(Canvas.GetLeft(_framePoint1) - 21.428571428571427) < 0.00001);
            Debug.Assert(Math.Abs(Canvas.GetTop(_framePoint1) - 43.58974358974359) < 0.00001);
            Debug.Assert(Math.Abs(Canvas.GetLeft(_framePoint2) - 34.32835820895522) < 0.00001);
            Debug.Assert(Math.Abs(Canvas.GetTop(_framePoint2) - 50.56179775280899) < 0.00001);
            Debug.Assert(Math.Abs(runeFrameMacro1.X - 21.428571428571427) < 0.00001);
            Debug.Assert(Math.Abs(runeFrameMacro1.Y - 43.589743589743591) < 0.00001);
            Debug.Assert(Math.Abs(runeFrameMacro2.X - 34.328358208955223) < 0.00001);
            Debug.Assert(Math.Abs(runeFrameMacro2.Y - 50.561797752808992) < 0.00001);
            Debug.Assert(runeFrameMacro1.ScaleX == 100);
            Debug.Assert(runeFrameMacro1.ScaleY == 100);
            Debug.Assert(runeFrameMacro2.ScaleX == 100);
            Debug.Assert(runeFrameMacro2.ScaleY == 100);
        }

        /**
         * @brief Verifies that when a frame has zero width, horizontal scaling of points
         * is skipped to prevent division by zero errors
         *
         * When users attempt to resize a frame that has zero width, the system should
         * skip horizontal scaling calculations for all child points to avoid mathematical
         * errors. Points should retain their original X coordinates and ScaleX values,
         * while vertical scaling (Y coordinates) still applies normally.
         */
        private void _testScalingFrameDoesntScaleOnZeroWidth()
        {
            var framePointScaleActionHandler = _fixture(100, 100, 0, 100);
            var selectedObject = _selectedObject(_frameCanvas, new Tuple<double, double>(0, 0));
            _editMenuState.Select(selectedObject);
            _editMenuState.SetDragging(true);
            _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
            var runeFrameMacro1 = _runeFrame.FrameData.RuneFrameMacros.Find((m) => m.ElementLabel == "MT0")!;
            var runeFrameMacro2 = _runeFrame.FrameData.RuneFrameMacros.Find((m) => m.ElementLabel == "MT1")!;
            Debug.Assert(Canvas.GetLeft(_framePoint1) == 123);
            Debug.Assert(Math.Abs(Canvas.GetTop(_framePoint1) - 43.58974358974359) < 0.00001);
            Debug.Assert(Canvas.GetLeft(_framePoint2) == 123);
            Debug.Assert(Math.Abs(Canvas.GetTop(_framePoint2) - 50.56179775280899) < 0.00001);
            Debug.Assert(runeFrameMacro1.X == 12);
            Debug.Assert(Math.Abs(runeFrameMacro1.Y - 43.589743589743591) < 0.00001);
            Debug.Assert(runeFrameMacro2.X == 23);
            Debug.Assert(Math.Abs(runeFrameMacro2.Y - 50.561797752808992) < 0.00001);
            Debug.Assert(runeFrameMacro1.ScaleX == 56);
            Debug.Assert(runeFrameMacro1.ScaleY == 100);
            Debug.Assert(runeFrameMacro2.ScaleX == 67);
            Debug.Assert(runeFrameMacro2.ScaleY == 100);
        }

        /**
         * @brief Verifies that when a frame has zero height, vertical scaling of points
         * is skipped to prevent division by zero errors
         *
         * When users attempt to resize a frame that has zero height, the system should
         * skip vertical scaling calculations for all child points to avoid mathematical
         * errors. Points should retain their original Y coordinates and ScaleY values,
         * while horizontal scaling (X coordinates) still applies normally.
         */
        private void _testScalingFrameDoesntScaleOnZeroHeight()
        {
            var framePointScaleActionHandler = _fixture(100, 100, 100, 0);
            var selectedObject = _selectedObject(_frameCanvas, new Tuple<double, double>(0, 0));
            _editMenuState.Select(selectedObject);
            _editMenuState.SetDragging(true);
            _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
            var runeFrameMacro1 = _runeFrame.FrameData.RuneFrameMacros.Find((m) => m.ElementLabel == "MT0")!;
            var runeFrameMacro2 = _runeFrame.FrameData.RuneFrameMacros.Find((m) => m.ElementLabel == "MT1")!;
            Debug.Assert(Math.Abs(Canvas.GetLeft(_framePoint1) - 21.428571428571427) < 0.00001);
            Debug.Assert(Canvas.GetTop(_framePoint1) == 234);
            Debug.Assert(Math.Abs(Canvas.GetLeft(_framePoint2) - 34.32835820895522) < 0.00001);
            Debug.Assert(Canvas.GetTop(_framePoint1) == 234);
            Debug.Assert(Math.Abs(runeFrameMacro1.X - 21.428571428571427) < 0.00001);
            Debug.Assert(runeFrameMacro1.Y == 34);
            Debug.Assert(Math.Abs(runeFrameMacro2.X - 34.328358208955223) < 0.00001);
            Debug.Assert(runeFrameMacro2.Y == 45);
            Debug.Assert(runeFrameMacro1.ScaleX == 100);
            Debug.Assert(runeFrameMacro1.ScaleY == 78);
            Debug.Assert(runeFrameMacro2.ScaleX == 100);
            Debug.Assert(runeFrameMacro2.ScaleY == 89);
        }

        /**
         * @brief Verifies that frame point scaling does not occur when the edit menu
         * is not in dragging mode
         *
         * When users are not actively dragging a frame (Dragging = false), mouse move
         * events should not trigger scaling operations on frame points. Points should
         * remain completely unchanged, preserving both their positions and scale factors.
         * This prevents unintended point transformations during normal mouse movement.
         */
        private void _testScalingFrameDoesntScaleWhenNotDragging()
        {
            var frameRect = new Rect(100, 100, 100, 100);
            var framePointScaleActionHandler = _fixture(100, 100, 100, 100);
            var selectedObject = _selectedObject(_frameCanvas, null);
            _editMenuState.Select(selectedObject);
            _editMenuState.SetDragging(false);
            _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
            var runeFrameMacro1 = _runeFrame.FrameData.RuneFrameMacros.Find((m) => m.ElementLabel == "MT0")!;
            var runeFrameMacro2 = _runeFrame.FrameData.RuneFrameMacros.Find((m) => m.ElementLabel == "MT1")!;
            Debug.Assert(Canvas.GetLeft(_framePoint1) == 123);
            Debug.Assert(Canvas.GetTop(_framePoint1) == 234);
            Debug.Assert(Canvas.GetLeft(_framePoint2) == 123);
            Debug.Assert(Canvas.GetTop(_framePoint2) == 234);
            Debug.Assert(runeFrameMacro1.X == 12);
            Debug.Assert(runeFrameMacro1.Y == 34);
            Debug.Assert(runeFrameMacro2.X == 23);
            Debug.Assert(runeFrameMacro2.Y == 45);
            Debug.Assert(runeFrameMacro1.ScaleX == 56);
            Debug.Assert(runeFrameMacro1.ScaleY == 78);
            Debug.Assert(runeFrameMacro2.ScaleX == 67);
            Debug.Assert(runeFrameMacro2.ScaleY == 89);
        }

        /**
         * @brief Verifies that frame point scaling does not occur when no frame is
         * selected in the edit menu
         *
         * When no frame is selected (Selected = null), mouse move events should not
         * trigger scaling operations regardless of dragging state. Points should remain
         * completely unchanged since there is no active frame context for scaling.
         */
        private void _testScalingFrameDoesntScaleWhenNotSelected()
        {
            var frameRect = new Rect(100, 100, 100, 100);
            var framePointScaleActionHandler = _fixture(100, 100, 100, 100);
            _editMenuState.Select(null);
            _editMenuState.SetDragging(false);
            _mapCanvas.RaiseEvent(MouseMoveFixture.Event(_mapCanvas));
            var runeFrameMacro1 = _runeFrame.FrameData.RuneFrameMacros.Find((m) => m.ElementLabel == "MT0")!;
            var runeFrameMacro2 = _runeFrame.FrameData.RuneFrameMacros.Find((m) => m.ElementLabel == "MT1")!;
            Debug.Assert(Canvas.GetLeft(_framePoint1) == 123);
            Debug.Assert(Canvas.GetTop(_framePoint1) == 234);
            Debug.Assert(Canvas.GetLeft(_framePoint2) == 123);
            Debug.Assert(Canvas.GetTop(_framePoint2) == 234);
            Debug.Assert(runeFrameMacro1.X == 12);
            Debug.Assert(runeFrameMacro1.Y == 34);
            Debug.Assert(runeFrameMacro2.X == 23);
            Debug.Assert(runeFrameMacro2.Y == 45);
            Debug.Assert(runeFrameMacro1.ScaleX == 56);
            Debug.Assert(runeFrameMacro1.ScaleY == 78);
            Debug.Assert(runeFrameMacro2.ScaleX == 67);
            Debug.Assert(runeFrameMacro2.ScaleY == 89);
        }


        public void Run()
        {
            _testScalingFrameScalesFramePointPosition();
            _testScalingFrameDoesntScaleOnZeroWidth();
            _testScalingFrameDoesntScaleOnZeroHeight();
            _testScalingFrameDoesntScaleWhenNotDragging();
            _testScalingFrameDoesntScaleWhenNotSelected();
        }
    }


    public class WindowMapCanvasFramePointRemoveActionHandlerTests
    {
        private Canvas _mapCanvas = new Canvas();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private MockMouseEventDataExtractor _mousePositionExtractor = new MockMouseEventDataExtractor();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private Canvas _frameCanvas = new Canvas();

        private RuneFrame _runeFrame = new RuneFrame();

        private Canvas _framePoint1 = new Canvas();

        private Canvas _framePoint2 = new Canvas();

        private AbstractWindowActionHandler _fixture(Point clickPoint)
        {
            var frameRect = new Rect(100, 100, 100, 100);
            _mapCanvas = new Canvas { Width = 1000, Height = 1000 };
            _editMenuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MockMouseEventDataExtractor();
            _bottingModel = new BottingModel();
            _frameCanvas = FrameFixture.GenerateFrame(frameRect, _mapCanvas);
            _runeFrame = RuneFrameFixture.GenerateRuneFrame(frameRect, _frameCanvas, "FT0", "F0");
            _framePoint1 = RuneFrameFixture.FramePointFixture(
                new Point(146, 156), new Point(12, 34), new Point(56, 78),
                _frameCanvas, _runeFrame, "MT0", "M0"
            );
            _framePoint2 = RuneFrameFixture.FramePointFixture(
                new Point(123, 132), new Point(23, 45), new Point(67, 89),
                _frameCanvas, _runeFrame, "MT1", "M1"
            );
            var handler = new WindowMapCanvasFramePointRemoveActionHandlerFacade(
                _mapCanvas,
                _editMenuState,
                _mousePositionExtractor
            );
            _bottingModel.GetRuneModel().AddRuneFrame(_runeFrame);
            _mousePositionExtractor.GetPositionReturn.Add(clickPoint);
            handler.Inject(SystemInjectType.BottingModel, _bottingModel);
            return handler;
        }

        private WindowMapEditMenuFrameSelectedObject _selectedObject(
            Canvas frameCanvas, Canvas? framePoint
        )
        {
            return new WindowMapEditMenuFrameSelectedObject
            {
                FrameObject = frameCanvas,
                DragPoint = null,
                PointObject = framePoint
            };
        }

        /**
         * @brief Verifies that clicking on a selected point while in RemovePoint mode
         * removes both the visual point marker and its corresponding macro data
         *
         * When users click on a point marker that is currently selected while the edit
         * menu is in RemovePoint state, the system should delete the point from the
         * canvas and remove its associated macro from the rune frame's macro list.
         */
        private void _testClickingRemovesClickedFramePoint()
        {
            var framePointRemoveActionHandler = _fixture(new Point(146, 156));
            var selectedObject = _selectedObject(_frameCanvas, _framePoint1);
            _editMenuState.SetState((int) WindowMapEditFrameMenuStateTypes.RemovePoint);
            _editMenuState.Select(selectedObject);
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var runeFrameMacros = _runeFrame.FrameData.RuneFrameMacros;
            var framePoints = _frameCanvas.Children.OfType<Canvas>().ToList();
            Debug.Assert(framePoints.Count == 1);
            Debug.Assert(framePoints[0] == _framePoint2);
            Debug.Assert(runeFrameMacros.Find((m) => m.ElementLabel == "MT0") == null);
            Debug.Assert(runeFrameMacros.Find((m) => m.ElementLabel == "MT1") != null);
            Debug.Assert(selectedObject.PointObject == null);
        }

        /**
         * @brief Verifies that point removal only occurs when the edit menu is in
         * RemovePoint state
         *
         * When users click on a selected point while the edit menu is in any state other
         * than RemovePoint (e.g., an invalid state like 123), the system should not
         * remove the point. The visual point marker, its macro data, and the current
         * selection should all remain unchanged.
         */
        private void _testClickingWhenNotRemovingFrameDoesNotRemove()
        {
            var framePointRemoveActionHandler = _fixture(new Point(146, 156));
            var selectedObject = _selectedObject(_frameCanvas, _framePoint1);
            _editMenuState.SetState(123);
            _editMenuState.Select(selectedObject);
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var runeFrameMacros = _runeFrame.FrameData.RuneFrameMacros;
            var framePoints = _frameCanvas.Children.OfType<Canvas>().ToList();
            Debug.Assert(framePoints.Count == 2);
            Debug.Assert(framePoints.IndexOf(_framePoint1) != -1);
            Debug.Assert(framePoints.IndexOf(_framePoint2) != -1);
            Debug.Assert(runeFrameMacros.Find((m) => m.ElementLabel == "MT0") != null);
            Debug.Assert(runeFrameMacros.Find((m) => m.ElementLabel == "MT1") != null);
            Debug.Assert(selectedObject.PointObject == _framePoint1);
        }

        /**
         * @brief Verifies that clicking on a point in an unselected frame does not remove
         * the point, even when in RemovePoint mode
         *
         * When users click on a point marker that belongs to a frame that is not currently
         * selected, the system should not remove that point. Point removal should only
         * affect points within the actively selected frame, preventing accidental
         * deletion from non-target frames.
         */
        private void _testClickingUnselectedFrameDoesNotRemove()
        {
            _mapCanvas = new Canvas { Width = 1000, Height = 1000 };
            _editMenuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MockMouseEventDataExtractor();
            _bottingModel = new BottingModel();
            var frameRect1 = new Rect(100, 100, 100, 100);
            var frameRect2 = new Rect(200, 100, 100, 100);
            var frameCanvas1 = FrameFixture.GenerateFrame(frameRect1, _mapCanvas);
            var frameCanvas2 = FrameFixture.GenerateFrame(frameRect2, _mapCanvas);
            var runeFrame1 = RuneFrameFixture.GenerateRuneFrame(frameRect1, frameCanvas1, "FT0", "F0");
            var runeFrame2 = RuneFrameFixture.GenerateRuneFrame(frameRect2, frameCanvas2, "FT1", "F1");
            var framePoint1 = RuneFrameFixture.FramePointFixture(
                new Point(150, 150), new Point(12, 34), new Point(56, 78),
                frameCanvas1, runeFrame1, "MT0", "M0"
            );
            var framePoint2 = RuneFrameFixture.FramePointFixture(
                new Point(250, 250), new Point(23, 45), new Point(67, 89),
                frameCanvas2, runeFrame2, "MT1", "M1"
            );
            var handler = new WindowMapCanvasFramePointRemoveActionHandlerFacade(
                _mapCanvas,
                _editMenuState,
                _mousePositionExtractor
            );
            var selectedObject = _selectedObject(frameCanvas1, framePoint1);
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame1);
            _bottingModel.GetRuneModel().AddRuneFrame(runeFrame2);
            _mousePositionExtractor.GetPositionReturn.Add(new Point(250, 250));
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.RemovePoint);
            _editMenuState.Select(selectedObject);
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var runeFrameMacros1 = runeFrame1.FrameData.RuneFrameMacros;
            var runeFrameMacros2 = runeFrame2.FrameData.RuneFrameMacros;
            var framePoints1 = frameCanvas1.Children.OfType<Canvas>().ToList();
            var framePoints2 = frameCanvas2.Children.OfType<Canvas>().ToList();
            Debug.Assert(framePoints1.Count == 1);
            Debug.Assert(framePoints1.IndexOf(framePoint1) != -1);
            Debug.Assert(runeFrameMacros1.Find((m) => m.ElementLabel == "MT0") != null);
            Debug.Assert(framePoints2.Count == 1);
            Debug.Assert(framePoints2.IndexOf(framePoint2) != -1);
            Debug.Assert(runeFrameMacros2.Find((m) => m.ElementLabel == "MT1") != null);
            Debug.Assert(selectedObject.PointObject == framePoint1);
        }

        /**
         * @brief Verifies that clicking on empty canvas space does not remove any point,
         * even when a point is selected and in RemovePoint mode
         *
         * When users click on blank canvas area away from any point marker while the
         * edit menu is in RemovePoint state and a point is selected, the system should
         * not remove any point. Removal should only occur when clicking directly on a
         * point marker, preventing unintended deletions from misclicks on empty space.
         */
        private void _testClickingEmptyCanvasDoesNotRemovePoint()
        {
            var framePointRemoveActionHandler = _fixture(new Point(999, 999));
            var selectedObject = _selectedObject(_frameCanvas, _framePoint1);
            _editMenuState.SetState((int)WindowMapEditFrameMenuStateTypes.RemovePoint);
            _editMenuState.Select(selectedObject);
            _mapCanvas.RaiseEvent(ButtonClickFixture.Event(_mapCanvas));
            var runeFrameMacros = _runeFrame.FrameData.RuneFrameMacros;
            var framePoints = _frameCanvas.Children.OfType<Canvas>().ToList();
            Debug.Assert(framePoints.Count == 2);
            Debug.Assert(framePoints.IndexOf(_framePoint1) != -1);
            Debug.Assert(framePoints.IndexOf(_framePoint2) != -1);
            Debug.Assert(runeFrameMacros.Find((m) => m.ElementLabel == "MT0") != null);
            Debug.Assert(runeFrameMacros.Find((m) => m.ElementLabel == "MT1") != null);
            Debug.Assert(selectedObject.PointObject == _framePoint1);
        }

        public void Run()
        {
            _testClickingRemovesClickedFramePoint();
            _testClickingWhenNotRemovingFrameDoesNotRemove();
            _testClickingUnselectedFrameDoesNotRemove();
            _testClickingEmptyCanvasDoesNotRemovePoint();
        }
    }


    public class WindowMapEditorRuneHandlersTestSuite
    {
        public void Run()
        {
            new MapCanvasAddFrameButtonActionHandlerTests().Run();
            new MapCanvasAddFramePointButtonActionHandlerTests().Run();
            new MapCanvasRemoveFrameButtonActionHandlerTests().Run();
            new MapCanvasRemoveFramePointButtonActionHandlerTests().Run();
            new WindowMapCanvasFrameDrawerActionHandlerTests().Run();
            new WindowMapCanvasFrameSelectStateActionHandlerTests().Run();
            new WindowMapCanvasFrameDragActionHandlerTests().Run();
            new WindowMapCanvasFrameDataActionHandlerTests().Run();
            new WindowMapCanvasFrameSelectedTextActionHandlerTests().Run();
            new WindowMapCanvasFrameSelectedDragDataActionHandlerTests().Run();
            new WindowMapCanvasFrameRemoveActionHandlerTests().Run();
            new WindowMapCanvasFrameButtonAccessActionHandlerTests().Run();
            new WindowMapCanvasFramePointDrawerActionHandlerTests().Run();
            new WindowMapCanvasFramePointDragActionHandlerTests().Run();
            new WindowMapCanvasFramePointScaleActionHandlerTests().Run();
            new WindowMapCanvasFramePointRemoveActionHandlerTests().Run();
        }
    }
}
