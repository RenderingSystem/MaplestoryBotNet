using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.Systems.UIHandler.Utilities.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    /**
     * @class WindowMapEditMenuActionHandlerTests
     * 
     * @brief Unit tests for map editor menu activation functionality
     * 
     * This test class validates that the map editing menu properly responds to user interaction
     * by launching the dedicated map editor window. Ensures that users can access advanced
     * macro configuration tools through a simple button click.
     */
    public class WindowMapEditMenuActionHandlerTests
    {
        private Button _editButton = new Button();

        private MockSystemWindow _mapWindow = new MockSystemWindow();

        private Window _mapWindowRaw = new Window();

        private MockSystemWindow _editWindow = new MockSystemWindow();

        private Window _editWindowRaw = new Window();

        /**
         * @brief Creates test environment with edit button and window components
         * 
         * @return Configured WindowMapEditMenuActionHandlerFacade instance ready for testing
         * 
         * Prepares a test scenario with a fresh edit button and mock system window
         * to verify that the facade pattern properly mediates between user interaction
         * and window management for map editing functionality.
         */
        private AbstractWindowActionHandler _fixture()
        {
            _editButton = new Button();
            _editWindowRaw = new Window();
            _mapWindowRaw = new Window();
            _editWindow = new MockSystemWindow();
            _mapWindow = new MockSystemWindow();
            _editWindow.GetWindowReturn.Add(_editWindowRaw);
            _mapWindow.GetWindowReturn.Add(_mapWindowRaw);
            _editWindow.CallOrder = _mapWindow.CallOrder;
            return new WindowMapEditMenuActionHandlerFacade(
                _editButton, _mapWindow, _editWindow
            );
        }

        /**
         * @brief Tests map editor window launch functionality
         * 
         * @test Validates that clicking edit button successfully opens map editor
         * 
         * Verifies that when users click the map edit button, the system properly
         * launches the dedicated map editor window, allowing users to access
         * advanced configuration tools for macro customization and navigation settings.
         */
        private void _testClickingEditButtonOpensMapEditor()
        {
            var handler = _fixture();
            _editButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_editWindow.CallOrder.Count == 2);
            Debug.Assert(_editWindow.CallOrder[0] == new TestUtilities().Reference(_editWindow) + "ShowDialog");
            Debug.Assert(_editWindow.CallOrder[1] == new TestUtilities().Reference(_editWindow) + "AttachOwner");
            Debug.Assert(_editWindow.AttachOwnerCallsArg_owner[0] == _mapWindow);
        }

        /**
         * @brief Executes map editor menu functionality tests
         * 
         * Runs the test suite to ensure the map editing menu feature works correctly,
         * providing confidence that users can reliably access map configuration tools
         * through the main interface.
         */
        public void Run()
        {
            _testClickingEditButtonOpensMapEditor();
        }
    }


    /**
     * @class MapCanvasCirclePointDrawingActionHandlerTests
     * 
     * @brief Unit tests for the circular point drawing interaction on the map canvas.
     * 
     * This test suite validates the behavior of the drawing action handler when users click on the canvas
     * to add new map points. It ensures the system correctly interprets UI state, mouse input, and model
     * data to create visually consistent and data-complete circular point markers.
     */
    public class MapCanvasCirclePointDrawingActionHandlerTests
    {
        private Canvas _canvas;

        private WindowMapEditMenuState _menuState;

        private MouseEventPositionExtractorMock _mousePositionExtractor;

        private MouseButtonEventArgs _mouseButtonEvent;

        private MapModel _mapModel;


        /**
         * @brief Initializes common test dependencies.
         * 
         * Sets up a canvas, menu state manager, mouse event mock, and an empty map model.
         * Provides a standard mouse button event configured to simulate a left-click on the canvas.
         */
        public MapCanvasCirclePointDrawingActionHandlerTests()
        {
            _canvas = new Canvas();
            _menuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MouseEventPositionExtractorMock();
            _mouseButtonEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _canvas,
            };
            _mapModel = new MapModel();
        }

        /**
         * @brief Creates a fresh, configured test fixture.
         * 
         * @return A new instance of the MapCanvasCirclePointDrawingActionHandler, ready for testing.
         */
        public AbstractWindowActionHandler _fixture()
        {
            _canvas = new Canvas
            {
                Background = Brushes.Transparent,
                Width = 200,
                Height = 200
            };
            _menuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MouseEventPositionExtractorMock();
            _mouseButtonEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _canvas,
            };
            _mapModel = new MapModel();
            return new WindowMapCanvasPointDrawingActionHandlerFacade(
                _canvas, _menuState, _mousePositionExtractor
            );
        }

        /**
         * @test _testClickingOnCanvasDrawsWhenMenuIsAdding
         * 
         * Validates that canvas clicks create visual point markers when the menu is in "Add" mode.
         * The test sequences multiple clicks and verifies the correct count, positioning, and container type
         * for each added element.
         */
        private void _testClickingOnCanvasDrawsWhenMenuIsAdding()
        {
            for (int i = 1; i < 10; i++)
            {
                var handler = _fixture();
                _menuState.SetState(WindowMapEditMenuStateTypes.Add);
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                for (int j = 0; j < i; j++)
                {
                    _mousePositionExtractor.GetPositionReturn.Add(
                        new Point(j * 12, (j + 1) * 23)
                    );
                }
                for (int j = 0; j < i; j++)
                {
                    _canvas.RaiseEvent(_mouseButtonEvent);
                }
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(_canvas.Children.Count == i);
                    Debug.Assert(_canvas.Children[j] is Canvas);
                    Debug.Assert(Canvas.GetLeft(_canvas.Children[j]) == j * 12.0);
                    Debug.Assert(Canvas.GetTop(_canvas.Children[j]) == (j + 1) * 23.0);
                }
            }
        }

        /**
         * @test _testClickingOnCanvasDoesNotDrawWhenMenuIsNotAdding
         * 
         * Ensures canvas clicks are ignored when the menu state is not "Add" (e.g., in "Select" mode).
         * This prevents unintended point creation during other editing operations.
         */
        private void _testClickingOnCanvasDoesNotDrawWhenMenuIsNotAdding()
        {
            var handler = _fixture();
            _menuState.SetState(WindowMapEditMenuStateTypes.Select);
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _canvas.RaiseEvent(_mouseButtonEvent);
            Debug.Assert(_canvas.Children.Count == 0);
        }

        /**
         * @test _testClickingOnCanvasDoesNotDrawWhenModelIsNotInjected
         * 
         * Confirms that drawing is blocked if the essential MapModel dependency has not been provided
         * via injection. This safeguards against system errors due to missing core data context.
         */
        private void _testClickingOnCanvasDoesNotDrawWhenModelIsNotInjected()
        {
            var handler = _fixture();
            _menuState.SetState(WindowMapEditMenuStateTypes.Add);
            _canvas.RaiseEvent(_mouseButtonEvent);
            Debug.Assert(_canvas.Children.Count == 0);
        }

        /**
         * @test _testClickingOnCanvasAddsCircularPoint
         * 
         * Verifies the precise visual properties of the drawn point: a small, centered, aqua-blue circle
         * with a light blue border. Checks the ellipse's dimensions, colors, stroke, and centering transform.
         */
        private void _testClickingOnCanvasAddsCircularPoint()
        {
            var handler = _fixture();
            _menuState.SetState(WindowMapEditMenuStateTypes.Add);
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _mousePositionExtractor.GetPositionReturn.Add(new Point(123, 234));
            _canvas.RaiseEvent(_mouseButtonEvent);
            var element = (Canvas)_canvas.Children[0];
            var ellipse = element.Children.OfType<Ellipse>().FirstOrDefault();
            Debug.Assert(ellipse != null);
            Debug.Assert(ellipse.Fill == Brushes.Aqua);
            Debug.Assert(ellipse.Stroke == Brushes.LightBlue);
            Debug.Assert(ellipse.StrokeThickness == 1.0);
            Debug.Assert(ellipse.RenderTransform is TranslateTransform);
            Debug.Assert(((TranslateTransform)ellipse.RenderTransform).X == -5.0);
            Debug.Assert(((TranslateTransform)ellipse.RenderTransform).Y == -5.0);
            Debug.Assert(ellipse.Width == 10.0);
            Debug.Assert(ellipse.Height == 10.0);
        }


        /**
         * @test _testClickingOnCanvasAddsPointLabel
         * 
         * Validates that each drawn point is accompanied by a correctly formatted and positioned text label.
         * Ensures labels follow a sequential naming convention ("P0", "P1", etc.) and have the proper
         * font, color, background, and positional offset.
         */
        private void _testClickingOnCanvasAddsPointLabel()
        {
            for (int i = 1; i < 10; i++)
            {
                var handler = _fixture();
                _menuState.SetState(WindowMapEditMenuStateTypes.Add);
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                for (int j = 0; j < i; j++)
                {
                    _mousePositionExtractor.GetPositionReturn.Add(
                        new Point(j * 12, (j + 1) * 23)
                    );
                }
                for (int j = 0; j < i; j++)
                {
                    _canvas.RaiseEvent(_mouseButtonEvent);
                }
                for (int j = 0; j < i; j++)
                {
                    var element = (Canvas)_canvas.Children[j];
                    var textBlock = element.Children.OfType<TextBlock>().FirstOrDefault();
                    Debug.Assert(textBlock != null);
                    Debug.Assert(textBlock.Text == "P" + j.ToString());
                    Debug.Assert(textBlock.FontFamily.ToString() == "Courier New");
                    Debug.Assert(textBlock.FontSize == 10.0);
                    Debug.Assert(textBlock.Foreground == Brushes.White);
                    Debug.Assert(textBlock.Background == Brushes.Transparent);
                    Debug.Assert(textBlock.RenderTransform is TranslateTransform);
                    Debug.Assert(((TranslateTransform)textBlock.RenderTransform).X == 0.0);
                    Debug.Assert(((TranslateTransform)textBlock.RenderTransform).Y == -16.0);
                }
            }
        }


        /**
         * @test _testClickingOnCanvasAddsToModel
         * 
         * Confirms that for every visual point drawn on the canvas, a corresponding data object
         * is created and added to the underlying MapModel. Verifies the model data (coordinates,
         * size, and generated names) matches the intended state for each click.
         */
        private void _testClickingOnCanvasAddsToModel()
        {
            for (int i = 1; i < 10; i++)
            {
                var handler = _fixture();
                _menuState.SetState(WindowMapEditMenuStateTypes.Add);
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                for (int j = 0; j < i; j++)
                {
                    _mousePositionExtractor.GetPositionReturn.Add(
                        new Point(j * 12, (j + 1) * 23)
                    );
                }
                for (int j = 0; j < i; j++)
                {
                    _canvas.RaiseEvent(_mouseButtonEvent);
                }
                var minimapPoints = _mapModel.Points();
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(minimapPoints.Count == i);
                    Debug.Assert(minimapPoints[j].X == j * 12);
                    Debug.Assert(minimapPoints[j].Y == (j + 1) * 23);
                    Debug.Assert(minimapPoints[j].XRange == 10);
                    Debug.Assert(minimapPoints[j].YRange == 10);
                    Debug.Assert(minimapPoints[j].PointData.PointName == "P" + j.ToString());
                    Debug.Assert(minimapPoints[j].PointData.ElementName == "T" + j.ToString());
                    Debug.Assert(minimapPoints[j].PointData.Commands.Count == 0);
                }
            }
        }


        /**
         * @test _testClickingOnCanvasAddsUniquePointLabel
         * 
         * Tests the system's ability to generate unique sequential labels ("P0", "P1", "P2") even when
         * an existing point's label is manually renamed. This ensures label uniqueness is managed
         * correctly to avoid identification conflicts.
         */
        private void _testClickingOnCanvasAddsUniquePointLabel()
        {
            var handler = _fixture();
            _menuState.SetState(WindowMapEditMenuStateTypes.Add);
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _mousePositionExtractor.GetPositionReturn.Add(new Point(12, 23));
            _mousePositionExtractor.GetPositionReturn.Add(new Point(23, 34));
            _canvas.RaiseEvent(_mouseButtonEvent);
            _mapModel.SelectLabel("P0");
            var selectedPoint = _mapModel.SelectedPoint();
            selectedPoint!.PointData.PointName = "P1";
            _mapModel.EditSelected(selectedPoint);
            _mapModel.Deselect();
            _canvas.RaiseEvent(_mouseButtonEvent);
            _mapModel.SelectLabel("P2");
            Debug.Assert(_mapModel.SelectedPoint() != null);
            Debug.Assert(_mapModel.SelectedPoint()!.PointData.PointName == "P2");
        }

        public void Run()
        {
            _testClickingOnCanvasDrawsWhenMenuIsAdding();
            _testClickingOnCanvasDoesNotDrawWhenMenuIsNotAdding();
            _testClickingOnCanvasDoesNotDrawWhenModelIsNotInjected();
            _testClickingOnCanvasAddsCircularPoint();
            _testClickingOnCanvasAddsPointLabel();
            _testClickingOnCanvasAddsToModel();
            _testClickingOnCanvasAddsUniquePointLabel();
        }
    }


    /**
     * @class MapCanvasAddPointButtonActionHandlerTests
     * 
     * @brief Unit tests for map point addition button behavior and state management
     * 
     * This test class validates the exclusive toggle behavior of the map point addition button,
     * ensuring proper UI state synchronization and mutual exclusivity with other editing mode buttons.
     * Verifies that the map editor correctly manages user interaction modes.
     */
    public class MapCanvasAddPointButtonActionHandlerTests
    {
        private ToggleButton _addPointButton = new ToggleButton();

        private List<ToggleButton> _otherButtons = [];

        private WindowMapEditMenuState _menuState = new WindowMapEditMenuState();

        /**
         * @brief Creates test environment with toggle buttons and menu state
         * 
         * @return Configured MapCanvasAddPointButtonActionHandlerFacade instance ready for testing
         * 
         * Prepares a test scenario with one primary add point button, three additional toggle buttons,
         * and a fresh menu state instance to verify exclusive toggle behavior and state coordination.
         */
        private AbstractWindowActionHandler _fixture()
        {
            _addPointButton = new ToggleButton();
            _otherButtons = [new ToggleButton(), new ToggleButton(), new ToggleButton()];
            _menuState = new WindowMapEditMenuState();
            return new WindowMapAddPointButtonActionHandlerFacade(
                _addPointButton, _otherButtons, _menuState
            );
        }

        /**
         * @brief Tests exclusive toggle behavior when enabling point addition mode
         * 
         * @test Validates that activating add point button deactivates all other toggle buttons
         * 
         * Verifies that when users click to activate the add point button (entering point placement mode),
         * all other editing mode buttons are automatically untoggled, maintaining a single active mode
         * in the editing toolbar to prevent conflicting user interactions.
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
         * @brief Tests exclusive toggle behavior when disabling point addition mode
         * 
         * @test Validates that deactivating add point button also deactivates all other toggle buttons
         * 
         * Verifies that even when users click to deactivate the add point button (exiting point placement
         * mode without selecting another mode), all other editing mode buttons are also untoggled,
         * ensuring the toolbar enters a consistent neutral state with no active modes.
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
         * @brief Tests menu state transition when enabling point addition mode
         * 
         * @test Validates that toggling add point button updates central menu state to Add mode
         * 
         * Verifies that when users activate the add point button, the central menu state manager
         * correctly transitions to WindowMapEditMenuStateTypes.Add, ensuring that subsequent user
         * interactions (like canvas clicks) are interpreted as point placement operations rather
         * than selection operations.
         */
        private void _testTogglingButtonSetsMenuStateToAdd()
        {
            var handler = _fixture();
            _menuState.SetState(WindowMapEditMenuStateTypes.Select);
            _addPointButton.IsChecked = true;
            _addPointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_menuState.GetState() == WindowMapEditMenuStateTypes.Add);
        }

        /**
         * @brief Tests menu state transition when disabling point addition mode
         * 
         * @test Validates that untoggling add point button reverts central menu state to Select mode
         * 
         * Verifies that when users deactivate the add point button (without choosing another mode),
         * the central menu state manager automatically transitions back to the default
         * WindowMapEditMenuStateTypes.Select mode, providing a safe fallback state for user interaction.
         */
        private void _testUntogglingButtonSetsMenuStateToSelect()
        {
            var handler = _fixture();
            _addPointButton.UpdateLayout();
            _menuState.SetState(WindowMapEditMenuStateTypes.Add);
            _addPointButton.IsChecked = false;
            _addPointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_menuState.GetState() == WindowMapEditMenuStateTypes.Select);
        }

        /**
         * @brief Executes all point addition button behavior tests
         * 
         * Runs the complete test suite to ensure the add point button provides reliable
         * exclusive toggle functionality and proper state coordination within the map editor,
         * guaranteeing intuitive user experience during map editing operations.
         */
        public void Run()
        {
            _testTogglingButtonUntogglesOtherButtons();
            _testUntogglingButtonUntogglesOtherButtons();
            _testTogglingButtonSetsMenuStateToAdd();
            _testUntogglingButtonSetsMenuStateToSelect();
        }
    }


    public class MapCanvasRemovePointButtonActionHandlerTests
    {
        private ToggleButton _removePointButton = new ToggleButton();

        private List<ToggleButton> _otherButtons = [];

        private WindowMapEditMenuState _menuState = new WindowMapEditMenuState();

        /**
        * @brief Creates test environment with toggle buttons and menu state
        * 
        * @return Configured MapCanvasRemovePointButtonActionHandlerFacade instance ready for testing
        * 
        * Prepares a test scenario with one primary remove point button, three additional toggle buttons,
        * and a fresh menu state instance to verify exclusive toggle behavior and state coordination.
        */
        private AbstractWindowActionHandler _fixture()
        {
            _removePointButton = new ToggleButton();
            _otherButtons = [new ToggleButton(), new ToggleButton(), new ToggleButton()];
            _menuState = new WindowMapEditMenuState();
            return new WindowMapRemovePointButtonActionHandlerFacade(
                _removePointButton, _otherButtons, _menuState
            );
        }

        /**
        * @brief Tests exclusive toggle behavior when enabling point removeition mode
        * 
        * @test Validates that activating remove point button deactivates all other toggle buttons
        * 
        * Verifies that when users click to activate the remove point button (entering point placement mode),
        * all other editing mode buttons are automatically untoggled, maintaining a single active mode
        * in the editing toolbar to prevent conflicting user interactions.
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
        * @brief Tests exclusive toggle behavior when disabling point removeition mode
        * 
        * @test Validates that deactivating remove point button also deactivates all other toggle buttons
        * 
        * Verifies that even when users click to deactivate the remove point button (exiting point placement
        * mode without selecting another mode), all other editing mode buttons are also untoggled,
        * ensuring the toolbar enters a consistent neutral state with no active modes.
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
        * @brief Tests menu state transition when enabling point removeition mode
        * 
        * @test Validates that toggling remove point button updates central menu state to Remove mode
        * 
        * Verifies that when users activate the remove point button, the central menu state manager
        * correctly transitions to WindowMapEditMenuStateTypes.Remove, ensuring that subsequent user
        * interactions (like canvas clicks) are interpreted as point removal operations rather
        * than selection operations.
        */
        private void _testTogglingButtonSetsMenuStateToRemove()
        {
            var handler = _fixture();
            _menuState.SetState(WindowMapEditMenuStateTypes.Select);
            _removePointButton.IsChecked = true;
            _removePointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_menuState.GetState() == WindowMapEditMenuStateTypes.Remove);
        }

        /**
        * @brief Tests menu state transition when disabling point removeition mode
        * 
        * @test Validates that untoggling remove point button reverts central menu state to Select mode
        * 
        * Verifies that when users deactivate the remove point button (without choosing another mode),
        * the central menu state manager automatically transitions back to the default
        * WindowMapEditMenuStateTypes.Select mode, providing a safe fallback state for user interaction.
        */
        private void _testUntogglingButtonSetsMenuStateToSelect()
        {
            var handler = _fixture();
            _removePointButton.UpdateLayout();
            _menuState.SetState(WindowMapEditMenuStateTypes.Remove);
            _removePointButton.IsChecked = false;
            _removePointButton.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
            Debug.Assert(_menuState.GetState() == WindowMapEditMenuStateTypes.Select);
        }

        /**
        * @brief Executes all point removeition button behavior tests
        * 
        * Runs the complete test suite to ensure the remove point button provides reliable
        * exclusive toggle functionality and proper state coordination within the map editor,
        * guaranteeing intuitive user experience during map editing operations.
        */
        public void Run()
        {
            _testTogglingButtonUntogglesOtherButtons();
            _testUntogglingButtonUntogglesOtherButtons();
            _testTogglingButtonSetsMenuStateToRemove();
            _testUntogglingButtonSetsMenuStateToSelect();
        }
    }


    /**
     * @class WindowMapCanvasPointErasingActionHandlerTests
     * 
     * @brief Unit tests for map point removal functionality on the canvas
     * 
     * This test class validates the point erasing action handler's behavior when users
     * interact with the map canvas to delete existing points. It ensures precise hit detection,
     * proper state validation, and correct coordination between UI elements and data model
     * during point removal operations.
     */
    public class WindowMapCanvasPointErasingActionHandlerTests
    {
        private Canvas _canvas;

        private WindowMapEditMenuState _menuState;

        private MouseEventPositionExtractorMock _mousePositionExtractor;

        private MapModel _mapModel;

        private MouseButtonEventArgs _mouseButtonEvent;

        /**
         * @brief Initializes common test dependencies
         * 
         * Sets up a fresh canvas, menu state manager, mouse event extractor mock,
         * empty map model, and configured mouse button event to simulate left-clicks
         * on the canvas for consistent test execution.
         */
        public WindowMapCanvasPointErasingActionHandlerTests()
        {
            _canvas = new Canvas();
            _menuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MouseEventPositionExtractorMock();
            _mapModel = new MapModel();
            _mouseButtonEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _canvas,
            };
        }

        /**
         * @brief Creates a fresh, configured test fixture
         * 
         * @return A new instance of WindowMapCanvasPointErasingActionHandlerFacade ready for testing
         * 
         * Initializes all test components with clean instances to ensure test isolation
         * and prevent state contamination between test methods.
         */
        private AbstractWindowActionHandler _fixture()
        {
            _canvas = new Canvas();
            _menuState = new WindowMapEditMenuState();
            _mousePositionExtractor = new MouseEventPositionExtractorMock();
            _mapModel = new MapModel();
            return new WindowMapCanvasPointErasingActionHandlerFacade(
                _canvas, _menuState, _mousePositionExtractor
            );
        }

        /**
         * @brief Helper method to add a test point to both model and canvas
         * 
         * @param x X-coordinate of the point
         * @param y Y-coordinate of the point
         * @param xRange Horizontal detection range around the point
         * @param yRange Vertical detection range around the point
         * @param label Display label for the point
         * @param name Internal identifier for the point
         * 
         * Creates a complete point representation with both data model entry
         * and corresponding visual canvas element for comprehensive testing.
         */
        private void _addPoint(
            double x,
            double y,
            double xRange,
            double yRange,
            string label,
            string name
        )
        {
            _mapModel.Add(
                new MinimapPoint
                {
                    X = x,
                    Y = y,
                    XRange = xRange,
                    YRange = yRange,
                    PointData = new MinimapPointData
                    {
                        ElementName = name,
                        PointName = label,
                        Commands = []
                    }
                }
            );
            var canvas = new Canvas { Name = name };
            _canvas.Children.Add(canvas);
            Canvas.SetLeft(canvas, x);
            Canvas.SetTop(canvas, y);
        }

        /**
         * @test _testClickingOnValidPointRemovesFromCanvas
         * 
         * Validates precise hit detection within point boundaries when in Remove mode.
         * Tests click coordinates in an 11x11 grid (±5 pixels) around a test point
         * to ensure all clicks within the defined detection range (10x10) successfully
         * trigger point removal from both canvas and model.
         */
        private void _testClickingOnValidPointRemovesFromCanvas()
        {
            for (int i = -5; i <= 5; i++)
            for (int j = -5; j <= 5; j++)
            {
                var handler = _fixture();
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                _menuState.SetState(WindowMapEditMenuStateTypes.Remove);
                _addPoint(123, 234, 10, 10, "lol1", "lol2");
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonEvent);
                Debug.Assert(_mapModel.Points().Count == 0);
                Debug.Assert(_canvas.Children.Count == 0);
                }
        }

        /**
         * @test _testClickingOnEmptyLocationDoesNotRemoveCanvas
         * 
         * Tests that clicks outside point detection boundaries do not trigger removal.
         * Executes clicks in a 21x21 grid but skips the central 11x11 valid area,
         * verifying that only clicks within the precise detection range are processed.
         */
        private void _testClickingOnEmptyLocationDoesNotRemoveCanvas()
        {
            for (int i = -10; i <= 10; i++)
            for (int j = -10; j <= 10; j++)
            {
                if (i >= -5 && i <= 5) continue;
                if (j >= -5 && j <= 5) continue;
                var handler = _fixture();
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                _menuState.SetState(WindowMapEditMenuStateTypes.Remove);
                _addPoint(123, 234, 10, 10, "lol1", "lol2");
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonEvent);
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
                }
        }

        /**
         * @test _testClickingOnValidPointDoesNotRemoveWhenNotInRemoveState
         * 
         * Ensures point removal only occurs when menu is explicitly in Remove mode.
         * Tests that clicks on valid points while in Select mode (or other modes)
         * do not trigger removal, protecting against accidental deletion during
         * normal map navigation and selection operations.
         */
        private void _testClickingOnValidPointDoesNotRemoveWhenNotInRemoveState()
        {
            for (int i = -5; i <= 5; i++)
            for (int j = -5; j <= 5; j++)
            {
                var handler = _fixture();
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                _menuState.SetState(WindowMapEditMenuStateTypes.Select);
                _addPoint(123, 234, 10, 10, "lol1", "lol2");
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonEvent);
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
            }
        }

        /**
         * @test _testClickingOnValidPointDoesNotRemoveWhenNotModelNotInjected
         * 
         * Validates that the erasing handler requires proper dependency injection.
         * Tests that point removal is blocked when the MapModel dependency hasn't
         * been injected, preventing system errors and ensuring robust error handling
         * for missing dependencies.
         */
        private void _testClickingOnValidPointDoesNotRemoveWhenNotModelNotInjected()
        {
            for (int i = -5; i <= 5; i++)
            for (int j = -5; j <= 5; j++)
            {
                var handler = _fixture();
                _menuState.SetState(WindowMapEditMenuStateTypes.Remove);
                _addPoint(123, 234, 10, 10, "lol1", "lol2");
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonEvent);
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
            }
        }

        /**
         * @brief Executes the complete point erasing functionality test suite
         * 
         * Runs all validation tests to ensure the point removal feature works
         * correctly with precise hit detection and proper state validation.
         */
        public void Run()
        {
            _testClickingOnValidPointRemovesFromCanvas();
            _testClickingOnEmptyLocationDoesNotRemoveCanvas();
            _testClickingOnValidPointDoesNotRemoveWhenNotInRemoveState();
            _testClickingOnValidPointDoesNotRemoveWhenNotModelNotInjected();
        }
    }


    public class WindowMapEditorHandlersTestSuite
    {
        public void Run()
        {
            new WindowMapEditMenuActionHandlerTests().Run();
            new MapCanvasCirclePointDrawingActionHandlerTests().Run();
            new MapCanvasAddPointButtonActionHandlerTests().Run();
            new MapCanvasRemovePointButtonActionHandlerTests().Run();
            new WindowMapCanvasPointErasingActionHandlerTests().Run();
        }
    }
}
