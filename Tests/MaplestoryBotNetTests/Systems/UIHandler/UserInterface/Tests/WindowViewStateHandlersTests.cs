

using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

    
namespace MaplestoryBotNetTests.UserInterface.Tests
{
    /**
     * @class WindowViewUpdaterTests
     * 
     * @brief Unit tests for image display coordination and pixel data conversion
     * 
     * This test class validates that the view updater correctly handles image
     * data conversion and UI thread coordination, ensuring reliable display
     * of screen capture data in the application's user interface with proper
     * pixel format conversion and thread-safe UI updates.
     */
    public class WindowViewUpdaterTests
    {
        private Image<Bgra32> _imagesharpImage = new Image<Bgra32>(2, 2);

        private MockDispatcher _dispatcher = new MockDispatcher();

        private System.Windows.Controls.Image _image = new System.Windows.Controls.Image();

        /**
         * @brief Creates test environment with image display dependencies
         * 
         * @return Configured WindowViewUpdater instance for testing
         * 
         * Prepares a test environment with predefined image data, UI dispatcher,
         * and image control to validate pixel data conversion and thread-safe
         * image display functionality.
         */
        private AbstractWindowStateModifier _fixture()
        {
            _image = new System.Windows.Controls.Image();
            _dispatcher = new MockDispatcher();
            _imagesharpImage = new Image<Bgra32>(2, 2);
            _imagesharpImage[0, 0] = new Bgra32(12, 23, 34, 45);
            _imagesharpImage[0, 1] = new Bgra32(23, 34, 45, 56);
            _imagesharpImage[1, 0] = new Bgra32(34, 45, 56, 67);
            _imagesharpImage[1, 1] = new Bgra32(45, 56, 67, 78);
            return new WindowViewUpdater(_dispatcher, new ImageSharpConverter(), _image);
        }

        /**
         * @brief Tests UI thread-safe image update dispatching
         * 
         * Validates that image modification requests are correctly dispatched
         * to the UI thread, ensuring thread-safe image updates and preventing
         * cross-thread access violations during screen capture display.
         */
        private void _testModifyDispatchesToDispatcher()
        {
            var windowViewUpdater = _fixture();
            Debug.Assert(_dispatcher.DispatchCalls == 0);
            windowViewUpdater.Modify(_imagesharpImage);
            Debug.Assert(_dispatcher.DispatchCalls == 1);

        }

        /**
         * @brief Tests pixel data conversion and image assignment accuracy
         * 
         * Validates that image data is correctly converted from ImageSharp format
         * to WPF BitmapSource format with proper pixel data preservation, ensuring
         * accurate visual representation of screen capture data in the user interface.
         */
        private void _testDispatchedImageIsAssignedCorrectly()
        {
            var windowViewUpdater = _fixture();
            windowViewUpdater.Modify(_imagesharpImage);
            _dispatcher.DispatchCallArg_action[0]();
            var bitmapSource = (System.Windows.Media.Imaging.BitmapSource)_image.Source;
            Debug.Assert(bitmapSource.PixelWidth == _imagesharpImage.Width);
            Debug.Assert(bitmapSource.PixelHeight == _imagesharpImage.Height);
            var stride = bitmapSource.PixelWidth * 4;
            var pixelData = new byte[stride * bitmapSource.PixelHeight];
            bitmapSource.CopyPixels(pixelData, stride, 0);
            for (int y = 0; y < bitmapSource.PixelHeight; y++)
            {
                for (int x = 0; x < bitmapSource.PixelWidth; x++)
                {
                    var index = y * stride + x * 4;
                    var b = pixelData[index + 0];
                    var g = pixelData[index + 1];
                    var r = pixelData[index + 2];
                    var a = pixelData[index + 3];
                    var originalPixel = _imagesharpImage[x, y];
                    Debug.Assert(b == originalPixel.B);
                    Debug.Assert(g == originalPixel.G);
                    Debug.Assert(r == originalPixel.R);
                    Debug.Assert(a == originalPixel.A);
                }
            }
        }

        /**
         * @brief Executes all view updater functionality validation tests
         * 
         * Runs the complete test suite to ensure the view updater correctly
         * handles image data conversion and UI thread coordination, providing
         * confidence in reliable screen capture display functionality.
         */
        public void Run()
        {
            _testModifyDispatchesToDispatcher();
            _testDispatchedImageIsAssignedCorrectly();
        }
    }


    /**
     * @class WindowExiterTests
     * 
     * @brief Unit tests for application window exit behavior
     * 
     * This test class validates that the window exit handler correctly executes
     * window closure commands, ensuring proper application termination and
     * responsive user interface behavior when exit requests are processed.
     */
    public class WindowExiterTests
    {
        MockSystemWindow _systemWindow = new MockSystemWindow();

        /**
         * @brief Creates test environment for window exit functionality
         * 
         * @return Configured WindowExiter instance for testing
         * 
         * Prepares a test environment with mock window components to isolate
         * exit behavior from actual window management dependencies.
         */
        private AbstractWindowStateModifier _fixture()
        {
            _systemWindow = new MockSystemWindow();
            return new WindowExiter();
        }

        /**
         * @brief Tests window closure execution during application exit
         * 
         * Validates that the window exit handler correctly triggers window closure
         * when the exit command is processed, ensuring proper application termination
         * and resource cleanup when the user initiates application exit.
         */
        private void _testModifyClosesWindow()
        {
            var windowExiter = _fixture();
            windowExiter.Modify(new List<AbstractSystemWindow> { _systemWindow });
            Debug.Assert(_systemWindow.CloseCalls == 1);
        }

        /**
         * @brief Executes window exit functionality validation test
         * 
         * Runs the test to ensure the window exit handler properly closes
         * application windows, providing confidence in reliable application
         * termination behavior.
         */
        public void Run()
        {
            _testModifyClosesWindow();
        }
    }


    /**
     * @class WindowExitActionHandlerTests
     * 
     * @brief Unit tests for exit menu item click handling and window closure coordination
     * 
     * This test class validates that the exit action handler correctly processes menu item
     * click events and coordinates with the window state modifier to ensure proper
     * application termination when the exit menu option is selected by the user.
     */
    public class WindowExitActionHandlerTests
    {
        private MenuItem _exitMenuItem = new MenuItem();

        private MockSystemWindow _systemWindow = new MockSystemWindow();

        private MockWindowStateModifier _windowStateModifier = new MockWindowStateModifier();

        /**
         * @brief Creates test environment with exit menu dependencies
         * 
         * @return Configured WindowExitActionHandler instance for testing
         * 
         * Prepares a test environment with menu item, window, and state modifier
         * components to validate exit functionality without actual UI dependencies.
         */
        private WindowExitActionHandler _fixture()
        {
            _exitMenuItem = new MenuItem();
            _systemWindow = new MockSystemWindow();
            _windowStateModifier = new MockWindowStateModifier();
            return new WindowExitActionHandler(
                _systemWindow,
                _exitMenuItem,
                _windowStateModifier
            );
        }

        /**
         * @brief Tests menu click event processing and window closure coordination
         * 
         * Validates that clicking the exit menu item correctly triggers the window
         * state modifier to initiate window closure, ensuring proper event handling
         * and coordination between UI events and application termination logic.
         */
        private void _testExitActionHandlerInvokesWindowClose()
        {
            var windowExiterActionHandler = _fixture();
            _exitMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Debug.Assert(_windowStateModifier.ModifyCalls == 1);
            Debug.Assert(((List<AbstractSystemWindow>)_windowStateModifier.ModifyCallArg_value[0]!)[0] == _systemWindow);
        }

        /**
         * @brief Tests retrieval of window state modifier from action handler
         * 
         * Validates that the exit action handler correctly provides access to
         * its associated window state modifier, ensuring proper encapsulation
         * and accessibility of modifier components.
         */
        private void _testGetModifierObtainsModifierFromHandler()
        {
            var windowExiterActionHandler = _fixture();
            var modifier = windowExiterActionHandler.Modifier();
            Debug.Assert(modifier == _windowStateModifier);
        }

        /**
         * @brief Executes exit action handler functionality validation test
         * 
         * Runs the test to ensure the exit action handler properly processes
         * menu click events and coordinates window closure, providing confidence
         * in reliable application exit behavior.
         */
        public void Run()
        {
            _testExitActionHandlerInvokesWindowClose();
            _testGetModifierObtainsModifierFromHandler();
        }
    }


    /**
     * @brief Tests for complete application closing behavior
     * 
     * Verifies that when users close the main application window,
     * all open windows in the application close properly and clean up their resources.
     */
    public class ApplicationClosingActionHandlerTests
    {
        private SystemWindow _closingWindow = new SystemWindow(new Window());

        private Window _window = new Window();

        private List<AbstractSystemWindow> _windowsToClose = [];

        /**
         * @brief Prepares the test scenario with windows and closing handler
         * 
         * @return Configured application closing handler ready for testing
         */
        public ApplicationClosingActionHandler _fixture()
        {
            _window = new Window();
            _closingWindow = new SystemWindow(_window);
            _windowsToClose = [
                new MockSystemWindow(),
                new MockSystemWindow(),
                new MockSystemWindow()
            ];
            return (ApplicationClosingActionHandler) new ApplicationClosingActionHandlerBuilder()
                .WithArgs(_closingWindow)
                .WithArgs(_windowsToClose)
                .Build();
        }

        /**
         * @brief Verifies closing main window triggers complete application shutdown
         * 
         * @test Ensures all windows close when user exits via main window
         * 
         * When users close the main application window, this test confirms that
         * all other open windows in the application also close completely,
         * ensuring a clean application exit without leftover windows.
         */
        public void _testClosingWindowInvokesSubWindowExiters()
        {
            var handler = _fixture();
            _window.Close();
            foreach (var systemWindow in _windowsToClose)
            {
                Debug.Assert(systemWindow.ShutdownFlag == true);
                Debug.Assert(((MockSystemWindow)systemWindow).CloseCalls == 1);
            }
        }

        /**
         * @brief Executes all application closing behavior tests
         */
        public void Run()
        {
            _testClosingWindowInvokesSubWindowExiters();
        }
    }


    public class WindowViewStateHandlersTestSuite
    {
        public void Run()
        {
            new WindowViewUpdaterTests().Run();
            new WindowExiterTests().Run();
            new WindowExitActionHandlerTests().Run();
            new ApplicationClosingActionHandlerTests().Run();
        }
    }
}
