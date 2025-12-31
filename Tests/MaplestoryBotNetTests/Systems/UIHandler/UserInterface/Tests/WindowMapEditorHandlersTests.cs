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

    public class MapCanvasPointAdder
    {
        /**
         * @brief Helper method to add a test point to both model and canvas
         * 
         * @param canvas The parent Canvas where the visual element will be added
         * @param mapModel The data model where the point definition will be stored
         * @param x X-coordinate of the point in canvas units
         * @param y Y-coordinate of the point in canvas units
         * @param xRange Horizontal detection range around the point for hit testing
         * @param yRange Vertical detection range around the point for hit testing
         * @param label Display label shown to users (for tooltips/UI)
         * @param name Unique identifier used to link the model entry with its visual element
         * 
         * Creates a complete point representation with both data model entry
         * and corresponding visual canvas element for comprehensive testing.
         */
        public void AddPoint(
            Canvas canvas,
            MapModel mapModel,
            double x,
            double y,
            double xRange,
            double yRange,
            string label,
            string name
        )
        {
            mapModel.Add(
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
            var child = new Canvas { Name = name };
            canvas.Children.Add(child);
            Canvas.SetLeft(child, x);
            Canvas.SetTop(child, y);
        }
    }


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

        private MockMouseEventPositionExtractor _mousePositionExtractor;

        private MouseButtonEventArgs _mouseButtonEvent;

        private MapModel _mapModel;

        private TextBox _textBox;

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
            _mousePositionExtractor = new MockMouseEventPositionExtractor();
            _mouseButtonEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _canvas,
            };
            _mapModel = new MapModel();
            _textBox = new TextBox();
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
            _mousePositionExtractor = new MockMouseEventPositionExtractor();
            _mouseButtonEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _canvas,
            };
            _mapModel = new MapModel();
            _textBox = new TextBox();
            return new WindowMapCanvasPointDrawingActionHandlerFacade(
                _canvas, _textBox, _menuState, _mousePositionExtractor
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
                    Debug.Assert(minimapPoints[j].PointData.Commands.Count == 1);
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
            Debug.Assert(_canvas.Children.Count == 1);
            var selectedPoint = _mapModel.FindLabel("P0")!;
            selectedPoint.PointData.PointName = "P1";
            _mapModel.Edit(selectedPoint);
            _canvas.RaiseEvent(_mouseButtonEvent);
            Debug.Assert(_canvas.Children.Count == 2);
            Debug.Assert(_mapModel.FindLabel("P2") != null);
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

        private MockMouseEventPositionExtractor _mousePositionExtractor;

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
            _mousePositionExtractor = new MockMouseEventPositionExtractor();
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
            _mousePositionExtractor = new MockMouseEventPositionExtractor();
            _mapModel = new MapModel();
            return new WindowMapCanvasPointErasingActionHandlerFacade(
                _canvas, _menuState, _mousePositionExtractor
            );
        }

        /**
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
                new MapCanvasPointAdder().AddPoint(
                    _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
                );
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonEvent);
                Debug.Assert(_mapModel.Points().Count == 0);
                Debug.Assert(_canvas.Children.Count == 0);
            }
        }

        /**
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
                new MapCanvasPointAdder().AddPoint(
                    _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
                );
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonEvent);
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
            }
        }

        /**
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
                new MapCanvasPointAdder().AddPoint(
                    _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
                );
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonEvent);
                Debug.Assert(_mapModel.Points().Count == 1);
                Debug.Assert(_canvas.Children.Count == 1);
            }
        }

        /**
         * Validates that the erasing handler requires proper dependency injection.
         * Tests that point removal is blocked when the MapModel dependency hasn't
         * been injected, preventing system errors and ensuring robust error handling
         * for missing dependencies.
         */
        private void _testClickingOnValidPointDoesNotRemoveWhenModelNotInjected()
        {
            for (int i = -5; i <= 5; i++)
            for (int j = -5; j <= 5; j++)
            {
                var handler = _fixture();
                _menuState.SetState(WindowMapEditMenuStateTypes.Remove);
                new MapCanvasPointAdder().AddPoint(
                    _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
                );
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
            _testClickingOnValidPointDoesNotRemoveWhenModelNotInjected();
        }
    }


    public class WindowMapCanvasSelectActionHandlerTests
    {
        private Canvas _canvas;

        private TextBox _textBoxX;

        private TextBox _textBoxY;

        private TextBox _textBoxName;

        private AbstractWindowMapEditMenuState _menuState;

        private MapModel _mapModel;

        private MockMouseEventPositionExtractor _mousePositionExtractor;

        private MouseButtonEventArgs _mouseButtonEvent;

        /**
         * @brief Constructs a complete test environment with all necessary components for point
         * selection testing.
         * 
         * Prepares a clean canvas, empty text fields for point information display, a fresh map data model,
         * and mock mouse interaction capabilities. This setup ensures each test starts with a consistent
         * and isolated environment, eliminating interference between test cases and providing reliable results.
         */
        public WindowMapCanvasSelectActionHandlerTests()
        {
            _canvas = new Canvas();
            _textBoxX = new TextBox();
            _textBoxY = new TextBox();
            _textBoxName = new TextBox();
            _menuState = new WindowMapEditMenuState();
            _mapModel = new MapModel();
            _mousePositionExtractor = new MockMouseEventPositionExtractor();
            _mouseButtonEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _canvas,
            };
        }

        /**
         * @brief Creates a configured instance of the selection handler with all test dependencies
         * properly initialized.
         * 
         * Builds and returns a fully configured WindowMapCanvasSelectActionHandlerFacade instance with
         * all necessary UI components, menu state, and mouse input handlers. This method ensures each
         * test receives a fresh handler instance to prevent state contamination between test executions.
         * 
         * @return A ready-to-use selection handler instance configured for testing
         */
        private AbstractWindowActionHandler _fixture()
        {
            _canvas = new Canvas();
            _textBoxX = new TextBox();
            _textBoxY = new TextBox();
            _textBoxName = new TextBox();
            _menuState = new WindowMapEditMenuState();
            _mapModel = new MapModel();
            _mouseButtonEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _canvas,
            };
            return new WindowMapCanvasSelectActionHandlerFacade(
                _canvas,
                _textBoxX,
                _textBoxY,
                _textBoxName,
                _menuState,
                _mousePositionExtractor
            );
        }

        /**
         * @brief Validates that clicking directly on a point or within its defined detection area
         * successfully selects it.
         * 
         * Tests the fundamental selection mechanism by simulating clicks at every position within a point's
         * interactive boundary. This test ensures users don't need pixel-perfect accuracy to select points
         * and that the entire declared interactive area responds consistently to selection attempts.
         */
        private void _testClickingOnValidPointSelects()
        {
            for (int i = -5; i <= 5; i++)
            for (int j = -5; j <= 5; j++)
            {
                var handler = _fixture();
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                _menuState.SetState(WindowMapEditMenuStateTypes.Select);
                new MapCanvasPointAdder().AddPoint(
                    _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
                );
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonEvent);
                Debug.Assert(((FrameworkElement)_menuState.Selected()!).Name == "lol2");
            }
        }

        /**
         * @brief Validates that clicking outside a point's detection area does not trigger selection.
         * 
         * Tests the precision of the selection system by simulating clicks just beyond the point's
         * interactive boundaries. This ensures that users can click near points without unintentionally
         * selecting them, maintaining clean and intentional interaction with the map canvas.
         */
        private void _testClickingOnEmptyPointDoesNotSelect()
        {
            for (int i = -10; i <= 10; i++)
            for (int j = -10; j <= 10; j++)
            {
                if (i >= -5 && i <= 5) continue;
                if (j >= -5 && j <= 5) continue;
                var handler = _fixture();
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                _menuState.SetState(WindowMapEditMenuStateTypes.Select);
                new MapCanvasPointAdder().AddPoint(
                    _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
                );
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonEvent);
                Debug.Assert(_menuState.Selected() == null);
            }
        }


        /**
         * @brief Validates that selecting a point automatically displays its information in the associated
         * edit fields.
         * 
         * Tests the data synchronization between point selection and the user interface by verifying that
         * a point's name, X-coordinate, and Y-coordinate immediately appear in their respective text boxes
         * upon selection. This ensures users can see and edit point details without additional steps.
         */
        private void _testClickingOnValidPointSetsTextBoxValues()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _menuState.SetState(WindowMapEditMenuStateTypes.Select);
            new MapCanvasPointAdder().AddPoint(
                _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
            );
            _mousePositionExtractor.GetPositionReturn.Add(new Point(123, 234));
            _canvas.RaiseEvent(_mouseButtonEvent);
            Debug.Assert(_textBoxName.Text == "lol1");
            Debug.Assert(_textBoxX.Text == "123");
            Debug.Assert(_textBoxY.Text == "234");
        }

        /**
         * @brief Validates that point selection is properly disabled when not in selection mode.
         * 
         * Tests the mode-awareness of the selection system by attempting point selection while in
         * "Add" mode instead of "Select" mode. This ensures the interface prevents conflicting
         * operations and maintains clear separation between different editing functions.
         */
        private void _testClickingOnValidPointDoesNotSelectWhenNotInSelectState()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _menuState.SetState(WindowMapEditMenuStateTypes.Add);
            new MapCanvasPointAdder().AddPoint(
                _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
            );
            _mousePositionExtractor.GetPositionReturn.Add(new Point(123, 234));
            _canvas.RaiseEvent(_mouseButtonEvent);
            Debug.Assert(_menuState.Selected() == null);
            Debug.Assert(_textBoxName.Text == "");
            Debug.Assert(_textBoxX.Text == "");
            Debug.Assert(_textBoxY.Text == "");
        }

        /**
         * @brief Validates that the selection system fails gracefully without proper data model
         * initialization.
         * 
         * Tests the dependency requirements of the selection system by attempting point selection without
         * injecting the necessary data model. This ensures the system doesn't crash or behave unpredictably
         * when underlying data structures are missing, providing robust error handling.
         */
        private void _testClickingOnValidPointDoesNotSelectWhenModelNotInjected()
        {
            var handler = _fixture();
            _menuState.SetState(WindowMapEditMenuStateTypes.Select);
            new MapCanvasPointAdder().AddPoint(
                _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
            );
            _mousePositionExtractor.GetPositionReturn.Add(new Point(123, 234));
            _canvas.RaiseEvent(_mouseButtonEvent);
            Debug.Assert(_menuState.Selected() == null);
            Debug.Assert(_textBoxName.Text == "");
            Debug.Assert(_textBoxX.Text == "");
            Debug.Assert(_textBoxY.Text == "");
        }

        /**
         * @brief Executes the complete battery of point selection validation tests in sequence.
         * 
         * Orchestrates the execution of all individual test scenarios to validate the entire point
         * selection system. This method serves as the main entry point for running the test suite
         * and ensures comprehensive coverage of selection functionality.
         */
        public void Run()
        {
            _testClickingOnValidPointSelects();
            _testClickingOnValidPointSetsTextBoxValues();
            _testClickingOnEmptyPointDoesNotSelect();
            _testClickingOnValidPointDoesNotSelectWhenNotInSelectState();
            _testClickingOnValidPointDoesNotSelectWhenModelNotInjected();
        }
    }

    /**
     * @class WindowMapCanvasDragActionHandlerTests
     * 
     * @brief Validates the point dragging functionality in the map canvas interface to ensure users can
     * reliably drag and reposition points.
     * 
     * This comprehensive test suite verifies the complete point dragging workflow including mouse interaction,
     * boundary constraints, visual synchronization, mode restrictions, and system dependencies. Each test
     * simulates the full drag sequence from mouse down through movement to mouse up, validating that points
     * respond correctly to user dragging gestures under various conditions and constraints.
     */
    public class WindowMapCanvasDragActionHandlerTests
    {
        private Canvas _canvas;

        private TextBox _selectedX;

        private TextBox _selectedY;

        private AbstractWindowMapEditMenuState _menuState;

        private MockMouseEventPositionExtractor _mousePositionExtractor;

        private MapModel _mapModel;

        private MouseButtonEventArgs _mouseButtonDownEvent;

        private MouseButtonEventArgs _mouseButtonUpEvent;

        private MouseEventArgs _mouseMoveEvent;

        /**
         * @brief Constructs a complete test environment for point dragging validation with proper
         * canvas layout.
         * 
         * Initializes a properly sized and arranged canvas for accurate coordinate testing, along with
         * position display fields, menu state controls, map data model, and mock mouse event handlers
         * for all three phases of dragging (down, move, up). This setup ensures tests simulate realistic
         * user interactions with correct spatial context.
         */
        public WindowMapCanvasDragActionHandlerTests()
        {
            _canvas = new Canvas { Width = 300, Height = 300 };
            _canvas.Measure(new Size(300, 300));
            _canvas.Arrange(new Rect(0, 0, 300, 300));
            _selectedX = new TextBox();
            _selectedY = new TextBox();
            _menuState = new WindowMapEditMenuState();
            _mapModel = new MapModel();
            _mousePositionExtractor = new MockMouseEventPositionExtractor();
            _mouseButtonDownEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _canvas,
            };
            _mouseButtonUpEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonUpEvent,
                Source = _canvas,
            };
            _mouseMoveEvent = new MouseEventArgs(
                Mouse.PrimaryDevice, 123
            )
            {
                RoutedEvent = UIElement.MouseMoveEvent,
                Source = _canvas
            };
        }

        /**
         * @brief Creates a fresh instance of the drag action handler configured with all test dependencies.
         * 
         * Builds and returns a fully initialized WindowMapCanvasDragActionHandlerFacade instance with
         * properly sized canvas, coordinate display fields, menu state controller, and mouse input simulation.
         * This method ensures each test receives a clean handler instance with correctly measured and
         * arranged canvas dimensions for accurate coordinate-based testing.
         * 
         * @return A ready-to-use drag action handler instance configured for comprehensive testing
         */
        private AbstractWindowActionHandler _fixture()
        {
            _canvas = new Canvas { Width = 300, Height = 300 };
            _canvas.Measure(new Size(300, 300));
            _canvas.Arrange(new Rect(0, 0, 300, 300));
            _selectedX = new TextBox();
            _selectedY = new TextBox();
            _menuState = new WindowMapEditMenuState();
            _mapModel = new MapModel();
            _mousePositionExtractor = new MockMouseEventPositionExtractor();
            return new WindowMapCanvasDragActionHandlerFacade(
                _canvas,
                _selectedX,
                _selectedY,
                _menuState,
                _mousePositionExtractor
            );
        }


        /**
         * @brief Validates that dragging a point within its detection area correctly moves
         * it to the new location.
         * 
         * Tests the fundamental drag-and-drop mechanism by simulating drag operations starting from
         * every position within a point's interactive boundary. This ensures users can initiate dragging
         * from any part of the point's detection area and that both the visual representation and
         * underlying data model synchronize correctly to the new position.
         */
        private void _testDraggingPointMovesToDraggedLocation()
        {
            for (int i = -5; i <= 5; i++)
            for (int j = -5; j <= 5; j++)
            {
                var handler = _fixture();
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                _menuState.SetState(WindowMapEditMenuStateTypes.Select);
                new MapCanvasPointAdder().AddPoint(
                    _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
                );
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonDownEvent);
                _mousePositionExtractor.GetPositionReturn.Add(new Point(234, 123));
                _mousePositionExtractor.GetPositionReturn.Add(new Point(12, 23));
                _canvas.RaiseEvent(_mouseMoveEvent);
                _canvas.RaiseEvent(_mouseMoveEvent);
                _canvas.RaiseEvent(_mouseButtonUpEvent);
                Debug.Assert(_mapModel.FindName("lol2")!.X == 12);
                Debug.Assert(_mapModel.FindName("lol2")!.Y == 23);
                Debug.Assert(((FrameworkElement)_canvas.Children[0]).Name == "lol2");
                Debug.Assert(Canvas.GetLeft(_canvas.Children[0]) == 12);
                Debug.Assert(Canvas.GetTop(_canvas.Children[0]) == 23);
            }
        }

        /**
         * @brief Validates that dragging operations starting outside point boundaries do not affect
         * point positions.
         * 
         * Tests the precision of drag initiation by simulating drag attempts starting just beyond
         * the point's detection boundaries. This ensures that users must start their drag gesture
         * directly on or very near the point to affect it, preventing accidental movement of points
         * when dragging elsewhere on the canvas.
         */
        private void _testDraggingNothingDoesNotDrag()
        {
            for (int i = -10; i <= 10; i++)
            for (int j = -10; j <= 10; j++)
            {
                if (i >= -5 && i <= 5) continue;
                if (j >= -5 && j <= 5) continue;
                var handler = _fixture();
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                _menuState.SetState(WindowMapEditMenuStateTypes.Select);
                new MapCanvasPointAdder().AddPoint(
                    _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
                );
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123 + i, 234 + j));
                _canvas.RaiseEvent(_mouseButtonDownEvent);
                _mousePositionExtractor.GetPositionReturn.Add(new Point(234, 123));
                _mousePositionExtractor.GetPositionReturn.Add(new Point(12, 23));
                _canvas.RaiseEvent(_mouseMoveEvent);
                _canvas.RaiseEvent(_mouseMoveEvent);
                _canvas.RaiseEvent(_mouseButtonUpEvent);
                Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
                Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
                Debug.Assert(((FrameworkElement)_canvas.Children[0]).Name == "lol2");
                Debug.Assert(Canvas.GetLeft(_canvas.Children[0]) == 123);
                Debug.Assert(Canvas.GetTop(_canvas.Children[0]) == 234);
            }
        }


        /**
         * @brief Validates that points cannot be dragged outside the visible canvas boundaries.
         * 
         * Tests the boundary constraint system by attempting to drag points to positions beyond
         * the canvas edges. This ensures the interface prevents users from losing points outside
         * the visible area and maintains all points within the usable workspace, providing clear
         * visual feedback.
         */
        private void _testDraggingPointOutOfCanvasDoesNotDrag()
        {
            var pointList = new List<Point> {
                new Point(999, 999),
                new Point(999, -999),
                new Point(-999, 999),
                new Point(-999, -999)
            };
            foreach (var point in pointList)
            {
                var handler = _fixture();
                handler.Inject(SystemInjectType.MapModel, _mapModel);
                _menuState.SetState(WindowMapEditMenuStateTypes.Select);
                new MapCanvasPointAdder().AddPoint(
                    _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
                );
                _mousePositionExtractor.GetPositionReturn.Add(new Point(123, 234));
                _canvas.RaiseEvent(_mouseButtonDownEvent);
                _mousePositionExtractor.GetPositionReturn.Add(point);
                _canvas.RaiseEvent(_mouseMoveEvent);
                _canvas.RaiseEvent(_mouseButtonUpEvent);
                Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
                Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
                Debug.Assert(((FrameworkElement)_canvas.Children[0]).Name == "lol2");
                Debug.Assert(Canvas.GetLeft(_canvas.Children[0]) == 123);
                Debug.Assert(Canvas.GetTop(_canvas.Children[0]) == 234);
            }
        }

        /**
         * @brief Validates that point dragging is properly disabled when not in selection mode.
         * 
         * Tests the mode-awareness of the dragging system by attempting point dragging while in
         * "Add" mode instead of "Select" mode. This ensures the interface prevents conflicting
         * operations and maintains clear separation between point creation and point manipulation
         * functions, avoiding mode confusion.
         */
        private void _testDraggingPointDoesNotDragWhenNotInSelectState()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _menuState.SetState(WindowMapEditMenuStateTypes.Add);
            new MapCanvasPointAdder().AddPoint(
                _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
            );
            _mousePositionExtractor.GetPositionReturn.Add(new Point(123, 234));
            _canvas.RaiseEvent(_mouseButtonDownEvent);
            _mousePositionExtractor.GetPositionReturn.Add(new Point(234, 123));
            _mousePositionExtractor.GetPositionReturn.Add(new Point(12, 23));
            _canvas.RaiseEvent(_mouseMoveEvent);
            _canvas.RaiseEvent(_mouseMoveEvent);
            _canvas.RaiseEvent(_mouseButtonUpEvent);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
            Debug.Assert(((FrameworkElement)_canvas.Children[0]).Name == "lol2");
            Debug.Assert(Canvas.GetLeft(_canvas.Children[0]) == 123);
            Debug.Assert(Canvas.GetTop(_canvas.Children[0]) == 234);
        }


        /**
         * @brief Validates that the dragging system fails gracefully without proper data
         * model initialization.
         * 
         * Tests the dependency requirements of the dragging system by attempting point dragging without
         * injecting the necessary data model. This ensures the system doesn't crash or behave unpredictably
         * when underlying data structures are missing, providing robust error handling and preventing
         * data corruption.
         */
        private void _testDraggingPointDoesNotDragWhenModelNotInjected()
        {
            var handler = _fixture();
            _menuState.SetState(WindowMapEditMenuStateTypes.Select);
            new MapCanvasPointAdder().AddPoint(
                _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
            );
            _mousePositionExtractor.GetPositionReturn.Add(new Point(123, 234));
            _canvas.RaiseEvent(_mouseButtonDownEvent);
            _mousePositionExtractor.GetPositionReturn.Add(new Point(234, 123));
            _mousePositionExtractor.GetPositionReturn.Add(new Point(12, 23));
            _canvas.RaiseEvent(_mouseMoveEvent);
            _canvas.RaiseEvent(_mouseMoveEvent);
            _canvas.RaiseEvent(_mouseButtonUpEvent);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
            Debug.Assert(((FrameworkElement)_canvas.Children[0]).Name == "lol2");
            Debug.Assert(Canvas.GetLeft(_canvas.Children[0]) == 123);
            Debug.Assert(Canvas.GetTop(_canvas.Children[0]) == 234);
        }

        /**
         * @brief Executes the complete battery of point dragging validation tests in sequence.
         * 
         * Orchestrates the execution of all individual drag test scenarios to validate the entire
         * point dragging system. This method serves as the main entry point for running the drag test
         * suite and ensures comprehensive coverage of dragging functionality from basic operation
         * to boundary cases and error conditions.
         */
        public void Run()
        {
            _testDraggingPointMovesToDraggedLocation();
            _testDraggingNothingDoesNotDrag();
            _testDraggingPointOutOfCanvasDoesNotDrag();
            _testDraggingPointDoesNotDragWhenNotInSelectState();
            _testDraggingPointDoesNotDragWhenModelNotInjected();
        }
    }


    /**
     * @class WindowMapCanvasEditButtonAccessibilityActionHandlerTests
     * 
     * @brief Validates the dynamic enable/disable behavior of the edit button based on canvas
     * interactions and selection state
     * 
     * This test suite verifies the accessibility rules governing the edit button, ensuring it becomes
     * enabled only when a point is selected and disabled when no point is selected.
     */
    public class WindowMapCanvasEditButtonAccessibilityActionHandlerTests
    {
        private Canvas _canvas;

        private Button _editButton;

        private AbstractWindowMapEditMenuState _menuState;

        private MouseButtonEventArgs _mouseButtonDownEvent;

        /**
         * @brief Constructs a test environment with canvas, edit button, menu state, and mouse event
         * simulation
         * 
         * Initializes the core components required for testing edit button accessibility rules:
         * a canvas for simulating user interactions, an edit button for testing enable/disable states,
         * a menu state controller for managing selection status, and mock mouse events to simulate
         * user clicks on the canvas surface.
         */
        public WindowMapCanvasEditButtonAccessibilityActionHandlerTests()
        {
            _canvas = new Canvas();
            _editButton = new Button();
            _menuState = new WindowMapEditMenuState();
            _mouseButtonDownEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _canvas,
            };
        }


        /**
         * @brief Creates a configured instance of the accessibility action handler with test dependencies
         * 
         * Builds and returns a fully initialized instance with fresh canvas, edit button, and menu
         * state components. This ensures each test receives a clean handler instance with consistent
         * initial conditions for validating accessibility state transitions.
         * 
         * @return A ready-to-use accessibility handler instance configured for testing
         */
        private AbstractWindowActionHandler _fixture()
        {
            _canvas = new Canvas();
            _editButton = new Button();
            _menuState = new WindowMapEditMenuState();
            _mouseButtonDownEvent = new MouseButtonEventArgs(
                Mouse.PrimaryDevice, 123, MouseButton.Left
            )
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = _canvas,
            };
            return new WindowMapEditButtonAccessibilityActionHandlerFacade(
                _canvas, _editButton, _menuState
            );
        }

        /**
         * @brief Validates that clicking on the canvas with a selected point enables the edit button
         * 
         * Tests the positive accessibility case by simulating a canvas click while a point is
         * currently selected in the menu state. This ensures the edit button becomes enabled
         * when users have a valid point selected, allowing them to access editing functionality
         * for the chosen point.
         */
        private void _testClickingOnCanvasEnablesEditButton()
        {
            var handler = _fixture();
            _editButton.IsEnabled = false;
            _menuState.Select(new FrameworkElement());
            _canvas.RaiseEvent(_mouseButtonDownEvent);
            Debug.Assert(_editButton.IsEnabled);
        }

        /**
         * @brief Validates that clicking on the canvas without a selected point disables the edit button
         * 
         * Tests the negative accessibility case by simulating a canvas click while no point is
         * currently selected. This ensures the edit button remains disabled when users don't have
         * a valid point selected, preventing access to editing functionality until a point is chosen.
         */
        private void _testClickingOnCanvasDisablesEditButton()
        {
            var handler = _fixture();
            _editButton.IsEnabled = true;
            _menuState.Deselect();
            _canvas.RaiseEvent(_mouseButtonDownEvent);
            Debug.Assert(!_editButton.IsEnabled);
        }

        /**
         * @brief Executes the complete battery of edit button accessibility validation tests in
         * sequence
         * 
         * Orchestrates the execution of both accessibility test scenarios to validate the
         * edit button state management system.
         */
        public void Run()
        {
            _testClickingOnCanvasEnablesEditButton();
            _testClickingOnCanvasDisablesEditButton();
        }
    }


    /**
    * @class WindowMapCanvasPointLocationActionHandlerTests
    * 
    * @brief Validates the coordinate editing functionality that allows users to move points by typing new
    * coordinates in text boxes
    * 
    * This test suite verifies the bidirectional synchronization between coordinate text fields and point positions,
    * ensuring that users can precisely reposition points by editing X and Y values. Tests validate that coordinate
    * changes only affect selected points, handle empty input gracefully, and require proper system initialization
    * to function correctly, maintaining data integrity throughout the editing workflow.
    */
    public class WindowMapCanvasPointLocationActionHandlerTests
    {
        private Canvas _canvas;

        private MapModel _mapModel;

        private AbstractWindowMapEditMenuState _menuState;

        private TextBox _selectedTextX;

        private TextBox _selectedTextY;

        /**
         * @brief Constructs a test environment with coordinate input fields, map model, and selection state
         * 
         * Initializes the essential components for testing coordinate-based point movement: text boxes for
         * X and Y coordinate input, a map data model for point storage, a canvas for visual representation,
         * and a menu state controller for tracking selection status. This setup provides the foundation for
         * validating text-to-position synchronization under various conditions.
         */
        public WindowMapCanvasPointLocationActionHandlerTests()
        {
            _canvas = new Canvas();
            _mapModel = new MapModel();
            _menuState = new WindowMapEditMenuState();
            _selectedTextX = new TextBox();
            _selectedTextY = new TextBox();
        }

        /**
         * @brief Creates a configured instance of the coordinate editing handler with test dependencies
         * 
         * Builds and returns a fully initialized instance with fresh coordinate text fields pre-populated
         * with test values, a clean map model, and empty selection state. This ensures each test receives
         * a consistent starting environment for validating coordinate editing behavior without interference
         * from previous test executions.
         * 
         * @return A ready-to-use coordinate editing handler instance configured for testing
         */
        private AbstractWindowActionHandler _fixture()
        {
            _canvas = new Canvas();
            _mapModel = new MapModel();
            _menuState = new WindowMapEditMenuState();
            _selectedTextX = new TextBox();
            _selectedTextY = new TextBox();
            _selectedTextX.Text = "123";
            _selectedTextY.Text = "234";
            return new WindowMapCanvasPointLocationActionHandlerFacade(
                _selectedTextX,
                _selectedTextY,
                _menuState
            );
        }

        /**
         * @brief Validates that editing coordinate text boxes moves the currently selected point to
         * new positions
         * 
         * Tests the core coordinate editing functionality by simulating text input in both X and Y coordinate
         * fields while a point is selected. Verifies that the visual canvas element and underlying data model
         * synchronize correctly to reflect the new coordinate values, ensuring users can precisely reposition
         * points through direct coordinate entry.
         */
        private void _testEditingTextMovesSelectedPointLocation()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            new MapCanvasPointAdder().AddPoint(
                _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
            );
            Debug.Assert(_canvas.Children.Count == 1);
            var selected = _canvas.Children[0];
            _menuState.Select(selected);
            _selectedTextX.Text = "12";
            Debug.Assert(Canvas.GetLeft(selected) == 12);
            Debug.Assert(Canvas.GetTop(selected) == 234);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 12);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
            _selectedTextY.Text = "23";
            Debug.Assert(Canvas.GetLeft(selected) == 12);
            Debug.Assert(Canvas.GetTop(selected) == 23);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 12);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 23);
        }

        /**
         * @brief Validates that coordinate editing does not affect points when no point is currently selected
         * 
         * Tests the selection dependency of the coordinate editing system by attempting to modify coordinate
         * values without a point being selected. Ensures that coordinate input is ignored when there is no
         * active selection, preventing accidental modification of points and maintaining clear user intent
         * requirements for editing operations.
         */
        private void _testEditingTextDoesntMovePointWhenNotSelected()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            new MapCanvasPointAdder().AddPoint(
                _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
            );
            Debug.Assert(_canvas.Children.Count == 1);
            var selected = _canvas.Children[0];
            _selectedTextX.Text = "12";
            Debug.Assert(Canvas.GetLeft(selected) == 123);
            Debug.Assert(Canvas.GetTop(selected) == 234);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
            _selectedTextY.Text = "23";
            Debug.Assert(Canvas.GetLeft(selected) == 123);
            Debug.Assert(Canvas.GetTop(selected) == 234);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
        }

        /**
         * @brief Validates that empty coordinate input does not modify point positions
         * 
         * Tests the system's handling of invalid coordinate input by attempting to clear coordinate text
         * fields while a point is selected. Ensures that empty or invalid coordinate values are ignored
         * rather than corrupting point positions, providing robust input validation and preventing data
         * loss from accidental text deletion.
         */
        private void _testEditingTextDoesntMovePointWhenEmpty()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            new MapCanvasPointAdder().AddPoint(
                _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
            );
            Debug.Assert(_canvas.Children.Count == 1);
            var selected = _canvas.Children[0];
            _menuState.Select(selected);
            _selectedTextX.Text = "";
            Debug.Assert(Canvas.GetLeft(selected) == 123);
            Debug.Assert(Canvas.GetTop(selected) == 234);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
            _selectedTextY.Text = "";
            Debug.Assert(Canvas.GetLeft(selected) == 123);
            Debug.Assert(Canvas.GetTop(selected) == 234);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
        }

        /**
         * @brief Validates that coordinate editing requires proper data model initialization
         * to function
         * 
         * Tests the dependency requirements of the coordinate editing system by attempting coordinate
         * modifications without injecting the necessary data model. Ensures the system fails gracefully
         * when underlying data structures are missing, preventing unpredictable behavior and maintaining
         * data integrity despite incomplete system configuration.
         */
        private void _testEditingTextDoesntMovePointWhenModelNotInjected()
        {
            var handler = _fixture();
            new MapCanvasPointAdder().AddPoint(
                _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
            );
            Debug.Assert(_canvas.Children.Count == 1);
            var selected = _canvas.Children[0];
            _menuState.Select(selected);
            _selectedTextX.Text = "12";
            Debug.Assert(Canvas.GetLeft(selected) == 123);
            Debug.Assert(Canvas.GetTop(selected) == 234);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
            _selectedTextY.Text = "23";
            Debug.Assert(Canvas.GetLeft(selected) == 123);
            Debug.Assert(Canvas.GetTop(selected) == 234);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
        }

        /**
         * @brief Validates that programmatic text updates do not trigger point movement
         * 
         * Tests the distinction between user-initiated and programmatic text changes by simulating
         * text box updates while the system is in programmatic editing mode. Verifies that coordinate
         * fields updated by the system (rather than user typing) do not cause point repositioning,
         * ensuring that automatic UI updates don't interfere with user intentions or create
         * feedback loops.
         */
        public void _testEditingTextProgrammaticallyDoesntMovePoint()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            new MapCanvasPointAdder().AddPoint(
                _canvas, _mapModel, 123, 234, 10, 10, "lol1", "lol2"
            );
            Debug.Assert(_canvas.Children.Count == 1);
            var selected = _canvas.Children[0];
            _menuState.Select(selected);
            _menuState.SetEditingText(true);
            _selectedTextX.Text = "12";
            Debug.Assert(Canvas.GetLeft(selected) == 123);
            Debug.Assert(Canvas.GetTop(selected) == 234);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
            _selectedTextY.Text = "23";
            Debug.Assert(Canvas.GetLeft(selected) == 123);
            Debug.Assert(Canvas.GetTop(selected) == 234);
            Debug.Assert(_mapModel.FindName("lol2")!.X == 123);
            Debug.Assert(_mapModel.FindName("lol2")!.Y == 234);
        }

        /**
         * @brief Executes the complete battery of coordinate editing validation tests in sequence
         * 
         * Orchestrates the execution of all coordinate editing test scenarios to validate the entire
         * text-to-position synchronization system. This method serves as the main entry point for
         * running the coordinate editing test suite and ensures comprehensive coverage of coordinate
         * input behavior from basic functionality to edge cases and error conditions.
         */
        public void Run()
        {
            _testEditingTextMovesSelectedPointLocation();
            _testEditingTextDoesntMovePointWhenNotSelected();
            _testEditingTextDoesntMovePointWhenEmpty();
            _testEditingTextDoesntMovePointWhenModelNotInjected();
            _testEditingTextProgrammaticallyDoesntMovePoint();
        }
    }


    /**
     * @class WindowMapCanvasDimensionActionHandlerTests
     * 
     * @brief Unit tests for map canvas dimension boundary textbox validation and synchronization
     *
     * Validates that four boundary textboxes (left, top, right, bottom) correctly update
     * a map model's coordinate area while enforcing logical dimension constraints.
     * Tests ensure that the coordinate system maintains proper relationships where
     * left < right, and top < bottom, preventing invalid geometric configurations.
     */
    public class WindowMapCanvasDimensionActionHandlerTests
    {
        private TextBox _textBoxLeft;

        private TextBox _textBoxTop;

        private TextBox _textBoxRight;

        private TextBox _textBoxBottom;

        private MapModel _mapModel;

        /**
         * @brief Initializes test environment with clean boundary textboxes and map model
         * 
         * Creates fresh instances of four boundary TextBox controls (left, top, right, bottom)
         * and a clean MapModel for each test execution. This ensures isolated testing
         * without residual state, maintaining test independence and reliability.
         */
        public WindowMapCanvasDimensionActionHandlerTests()
        {
            _textBoxLeft = new TextBox();
            _textBoxTop = new TextBox();
            _textBoxRight = new TextBox();
            _textBoxBottom = new TextBox();
            _mapModel = new MapModel();
        }

        /**
         * @brief Creates test fixture with pre-configured dimension textboxes
         * 
         * @tests Test environment setup for map boundary validation scenarios
         * 
         * Constructs a complete test environment with four boundary textboxes configured
         * with initial default values (right=1000, bottom=1000) to establish a valid
         * coordinate system baseline. Creates a specialized facade handler that manages
         * the synchronization between textbox values and map model boundaries.
         * 
         * @returns Configured AbstractWindowActionHandler facade for map dimension synchronization
         */
        private AbstractWindowActionHandler _fixture()
        {
            _textBoxLeft = new TextBox();
            _textBoxTop = new TextBox();
            _textBoxRight = new TextBox();
            _textBoxBottom = new TextBox();
            _textBoxRight.Text = "1000";
            _textBoxBottom.Text = "1000";
            _mapModel = new MapModel();
            return new WindowMapCanvasDimensionActionHandlerFacade(
                _textBoxLeft,
                _textBoxTop,
                _textBoxRight,
                _textBoxBottom
            );
        }

        /**
         * @brief Tests assignment of left boundary value to map model
         * 
         * @tests Left boundary synchronization and model update
         * 
         * Verifies that setting the left boundary textbox value correctly updates
         * the map model's left coordinate.
         */
        private void _testSettingLeftTextBoxAssignsMapArea()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _textBoxLeft.Text = "123";
            Debug.Assert(_mapModel.GetMapArea().Left == 123);
        }


        /**
         * @brief Tests assignment of top boundary value to map model
         * 
         * @tests Top boundary synchronization and model update
         * 
         * Validates that setting the top boundary textbox value correctly updates
         * the map model's top coordinate.
         */
        private void _testSettingTopTextBoxAssignsMapArea()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _textBoxTop.Text = "234";
            Debug.Assert(_mapModel.GetMapArea().Top == 234);
        }

        /**
         * @brief Tests assignment of right boundary value to map model
         * 
         * @tests Right boundary synchronization and model update
         * 
         * Verifies that setting the right boundary textbox value correctly updates
         * the map model's right coordinate.
         */
        private void _testSettingRightTextBoxAssignsMapArea()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _textBoxRight.Text = "345";
            Debug.Assert(_mapModel.GetMapArea().Right == 345);
        }

        /**
         * @brief Tests assignment of bottom boundary value to map model
         * 
         * @tests Bottom boundary synchronization and model update
         * 
         * Validates that setting the bottom boundary textbox value correctly updates
         * the map model's bottom coordinate.
         */
        private void _testSettingBottomTextBoxAssignsMapArea()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _textBoxBottom.Text = "456";
            Debug.Assert(_mapModel.GetMapArea().Bottom == 456);
        }

        /**
         * @brief Tests rejection of invalid left boundary value (left >= right)
         * 
         * @tests Left-right boundary relationship validation
         * 
         * Verifies that setting a left boundary value that is greater than or equal to
         * the right boundary value is correctly rejected, maintaining logical coordinate
         * system constraints.
         */
        private void _testSettingInvalidLeftTextBoxIsRejected()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _textBoxRight.Text = "100";
            _textBoxLeft.Text = "200";
            Debug.Assert(_mapModel.GetMapArea().Left == 0);
        }

        /**
         * @brief Tests rejection of invalid right boundary value (right <= left)
         * 
         * @tests Right-left boundary relationship validation
         * 
         * Validates that setting a right boundary value that is less than or equal to
         * the left boundary value is correctly rejected, maintaining logical coordinate
         * system constraints.
         */
        private void _testSettingInvalidRightTextBoxIsRejected()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _textBoxLeft.Text = "200";
            _textBoxRight.Text = "100";
            Debug.Assert(_mapModel.GetMapArea().Right == 1000);
        }

        /**
         * @brief Tests rejection of invalid top boundary value (top >= bottom)
         * 
         * @tests Top-bottom boundary relationship validation
         * 
         * Verifies that setting a top boundary value that is greater than or equal to
         * the bottom boundary value is correctly rejected, maintaining logical coordinate
         * system constraints.
         */
        private void _testSettingInvalidTopTextBoxIsRejected()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _textBoxBottom.Text = "100";
            _textBoxTop.Text = "200";
            Debug.Assert(_mapModel.GetMapArea().Top == 0);
        }

        /**
         * @brief Tests rejection of invalid bottom boundary value (bottom <= top)
         * 
         * @tests Bottom-top boundary relationship validation
         * 
         * Validates that setting a bottom boundary value that is less than or equal to
         * the top boundary value is correctly rejected, maintaining logical coordinate
         * system constraints.
         */
        private void _testSettingInvalidBottomextBoxIsRejected()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _textBoxTop.Text = "200";
            _textBoxBottom.Text = "100";
            Debug.Assert(_mapModel.GetMapArea().Bottom == 1000);
        }

        /**
         * @brief Executes comprehensive map canvas dimension validation test suite
         * 
         * @tests Complete boundary validation system for map coordinate textboxes
         * 
         * Runs the full battery of map dimension tests covering valid assignment
         * scenarios and invalid relationship rejections for all four boundary
         * coordinates (left, top, right, bottom).
         */
        public void Run()
        {
            _testSettingLeftTextBoxAssignsMapArea();
            _testSettingTopTextBoxAssignsMapArea();
            _testSettingRightTextBoxAssignsMapArea();
            _testSettingBottomTextBoxAssignsMapArea();
            _testSettingInvalidLeftTextBoxIsRejected();
            _testSettingInvalidRightTextBoxIsRejected();
            _testSettingInvalidTopTextBoxIsRejected();
            _testSettingInvalidBottomextBoxIsRejected();
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
            new WindowMapCanvasSelectActionHandlerTests().Run();
            new WindowMapCanvasDragActionHandlerTests().Run();
            new WindowMapCanvasEditButtonAccessibilityActionHandlerTests().Run();
            new WindowMapCanvasPointLocationActionHandlerTests().Run();
            new WindowMapCanvasDimensionActionHandlerTests().Run();
        }
    }
}
