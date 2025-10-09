using System.Diagnostics;
using System.Text;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.LibraryWrappers;
using MaplestoryBotNetTests.Systems.ScreenCapture.Tests.Mocks;
using MaplestoryBotNetTests.WindowsLibrary.Tests;
using ScreenCapture.NET;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.ScreenCapture.Tests
{
    /**
     * @class GameScreenCaptureHelperTest
     * 
     * @brief Unit tests for verifying proper screen capture and image processing functionality
     * 
     * This test class validates that the bot correctly captures game screen content, manages
     * multiple display configurations, and processes captured images for reliable game state
     * detection during automation.
     */
    internal class GameScreenCaptureHelperTest
    {
        private List<Display> _displays = [];

        private List<MockScreenCapture> _captures = [];

        private List<MockCaptureZone> _captureZones = [];

        private MockScreenCaptureService _screenCaptureService = new MockScreenCaptureService();

        /**
         * @brief Sets up test display configurations with mock capture zones
         * 
         * @param displayCount Number of virtual displays to create for testing
         * 
         * Creates simulated display configurations that represent different monitor setups
         * users might have, ensuring the bot can handle various multi-display environments
         * during gameplay automation.
         */
        private void _setupDisplayFixture(int displayCount)
        {
            _displays.Clear();
            for (int i = 0; i < displayCount; i++) {
                var display = new Display(i, "meow_" + i, 1000, 1000, Rotation.None, new GraphicsCard());
                _captureZones.Add(new MockCaptureZone());
                _captureZones[i].Display = display;
                _captureZones[i].RawBufferArray = new byte[i];
                _displays.Add(display);
            }
        }

        /**
         * @brief Configures mock screen capture service with test displays
         * 
         * @param displays List of display configurations to simulate
         * 
         * @return Configured mock screen capture service
         * 
         * Sets up a simulated screen capture service that mimics how the bot would
         * interact with different displays, ensuring reliable screen content capture
         * regardless of the user's hardware configuration.
         */
        private MockScreenCaptureService _screenCaptureFixture(List<Display> displays)
        {
            _screenCaptureService = new MockScreenCaptureService();
            for (int i = 0; i < displays.Count; i++)
            {
                _captures.Add(new MockScreenCapture());
                _captures[i].Display = displays[i];
                _captures[i].RegisterCaptureZoneReturn.Add(_captureZones[i]);
                _captures[i].CaptureScreenReturn.Add(true);
                _screenCaptureService.GetScreenCaptureReturn.Add(_captures[i]);
            }
            return _screenCaptureService;

        }

        private MONITORINFOEX _monitorInfoFixture(int index)
        {
            var monitorInfo = new MONITORINFOEX();
            string deviceName = "meow_" + index;
            byte[] deviceNameBytes = Encoding.ASCII.GetBytes(deviceName);
            Array.Copy(deviceNameBytes, monitorInfo.szDevice, deviceNameBytes.Length);
            return monitorInfo;
        }


        /**
         * @brief Creates a complete test environment for screen capture testing
         * 
         * @return Configured GameScreenCaptureHelper instance
         * 
         * Prepares a comprehensive test environment that simulates the bot's complete
         * screen capture pipeline, from display detection to image capture and processing.
         */
        private GameScreenCaptureHelper _gameScreenCaptureHelperFixture()
        {
            _displays.Clear();
            _captures.Clear();
            _captureZones.Clear();
            _setupDisplayFixture(5);
            _screenCaptureService = _screenCaptureFixture(_displays);
            var helper = new GameScreenCaptureHelper(
                _screenCaptureService, new MockWindowsLibrary()
            );
            helper.RegisterScreenCaptures(_displays);
            return helper;
        }

        /**
         * @brief Tests correct registration of multiple display configurations
         * 
         * Validates that the bot properly detects and registers all available displays,
         * ensuring it can capture game content from any monitor where MapleStory might
         * be running during automation sessions.
         */
        private void _testRegisterScreenCapturesRegistersAllKnownDisplays()
        {
            var helper = _gameScreenCaptureHelperFixture();
            for (int i = 0; i < 5; i++)
            {
                var capture = helper.GetScreenCapture(0x1234, _monitorInfoFixture(i));
                var captureZone = helper.GetCaptureZone(0x1234, _monitorInfoFixture(i));
                Debug.Assert(capture != null);
                Debug.Assert(captureZone != null);
                Debug.Assert(capture.Display == _displays[i]);
                Debug.Assert(captureZone.Display == _displays[i]);
            }
        }

        /**
         * @brief Tests correct updating of screen capture areas
         * 
         * Validates that the bot can dynamically adjust screen capture regions based on
         * changing game conditions, ensuring it always monitors the correct areas of the
         * screen for important game state information.
         */
        private void _testUpdateScreenCaptureUpdatesTheCaptureArea()
        {
            var helper = _gameScreenCaptureHelperFixture();
            var capture = helper.GetScreenCapture(0x1234, _monitorInfoFixture(0));
            var captureZone = helper.GetCaptureZone(0x1234, _monitorInfoFixture(0));
            var captureArea = new Tuple<int, int, int, int>(12, 23, 34, 45);
            Debug.Assert(capture != null);
            Debug.Assert(captureZone != null);
            var mockCapture = (MockScreenCapture)capture;
            var mockCaptureRef = new TestUtilities().Reference(mockCapture);
            var result = helper.UpdateCaptureZone(capture, captureArea);
            var updateCaptureZoneIndex = mockCapture.CallOrder.IndexOf(mockCaptureRef + "UpdateCaptureZone");
            var captureScreenIndex = mockCapture.CallOrder.IndexOf(mockCaptureRef + "CaptureScreen");
            Debug.Assert(result == captureZone);
            Debug.Assert(mockCapture.UpdateCaptureZoneCalls == 1);
            Debug.Assert(mockCapture.UpdateCaptureZoneCallArg_captureZone[0] == captureZone);
            Debug.Assert(mockCapture.UpdateCaptureZoneCallArg_x[0] == 12);
            Debug.Assert(mockCapture.UpdateCaptureZoneCallArg_y[0] == 23);
            Debug.Assert(mockCapture.UpdateCaptureZoneCallArg_width[0] == 34);
            Debug.Assert(mockCapture.UpdateCaptureZoneCallArg_height[0] == 45);
            Debug.Assert(updateCaptureZoneIndex + 1 == captureScreenIndex);
        }

        /**
         * @brief Tests optimization of screen capture updates when area remains unchanged
         * 
         * Validates that the bot intelligently skips unnecessary screen capture operations
         * when the capture area dimensions haven't changed, optimizing performance and
         * reducing resource usage during continuous gameplay monitoring.
         */
        private void _testUpdateScreenCaptureUpdatesOnlyIfCaptureAreaIsDifferent()
        {
            var helper = _gameScreenCaptureHelperFixture();
            var capture = helper.GetScreenCapture(0x1234, _monitorInfoFixture(0));
            var captureZone = helper.GetCaptureZone(0x1234, _monitorInfoFixture(0));
            Debug.Assert(capture != null);
            Debug.Assert(captureZone != null);
            var mockCaptureZone = (MockCaptureZone)captureZone;
            mockCaptureZone.X = 12;
            mockCaptureZone.Y = 23;
            mockCaptureZone.Width = 34;
            mockCaptureZone.Height = 45;
            var captureArea = new Tuple<int, int, int, int>(12, 23, 34, 45);
            helper.UpdateCaptureZone(capture, captureArea);
            helper.UpdateCaptureZone(capture, captureArea);
            var mockCapture = (MockScreenCapture) capture;
            Debug.Assert(mockCapture.UpdateCaptureZoneCalls == 0);
            Debug.Assert(mockCapture.CaptureScreenCalls == 0);
        }

        /**
         * @brief Executes all screen capture functionality tests
         * 
         * Runs the complete test suite to ensure the bot will correctly handle screen
         * capture operations across various display configurations, providing confidence
         * that visual monitoring features will work reliably during gameplay automation.
         */
        public void Run()
        {
            _testRegisterScreenCapturesRegistersAllKnownDisplays();
            _testUpdateScreenCaptureUpdatesTheCaptureArea();
            _testUpdateScreenCaptureUpdatesOnlyIfCaptureAreaIsDifferent();
        }
    }

    /**
     * @class GameCaptureImageGeneratorTest
     * 
     * @brief Unit tests for verifying proper image generation from screen capture data
     * 
     * This test class validates that the bot correctly converts raw screen capture data
     * into properly formatted images, ensuring accurate visual representation of game
     * content for reliable UI element detection and analysis during automation.
     */
    internal class GameCaptureImageGeneratorTest
    {
        /**
         * @brief Tests correct image generation from captured screen data
         * 
         * Validates that the bot properly converts raw screen capture data into usable
         * image formats, ensuring accurate visual analysis of game elements for
         * reliable automation decision-making.
         */
        private void _testGenerateImageGeneratesUsableImage()
        {
            var helper = new GameCaptureImageGenerator();
            var captureZone = new MockCaptureZone();
            captureZone.Width = 2;
            captureZone.Height = 2;
            captureZone.RawBufferArray = [12, 23, 34, 45, 23, 34, 45, 56, 34, 45, 56, 67, 45, 56, 67, 78];
            var imageLock = new MockImageLock();
            captureZone.LockReturn.Add(new MockImageLock());
            var result = helper.GenerateImage(captureZone);
            Debug.Assert(result.Frames.Count == 1);
            Debug.Assert(result.Frames[0].Width == 2);
            Debug.Assert(result.Frames[0].Height == 2);
            Debug.Assert(result.Frames[0].PixelBuffer[0, 0].Bgra == 0x2D22170C);
            Debug.Assert(result.Frames[0].PixelBuffer[1, 0].Bgra == 0x382D2217);
            Debug.Assert(result.Frames[0].PixelBuffer[0, 1].Bgra == 0x43382D22);
            Debug.Assert(result.Frames[0].PixelBuffer[1, 1].Bgra == 0x4E43382D);
        }

        /**
         * @brief Executes the image generation test
         * 
         * Runs the test to ensure the bot will correctly convert raw screen capture data
         * into properly formatted images, providing confidence that visual analysis
         * features will work reliably during gameplay automation.
         */
        public void Run()
        {
            _testGenerateImageGeneratesUsableImage();
        }
    }


    /**
     * @class GameMonitorDisplaysHelperTest
     * 
     * @brief Unit tests for verifying proper display detection and registration functionality
     * 
     * This test class validates that the bot correctly identifies and manages all available
     * display configurations, ensuring reliable screen monitoring across various hardware
     * setups and graphics card configurations during gameplay automation.
     */
    internal class GameMonitorDisplaysHelperTest
    {
        private List<GraphicsCard> _graphicsCards = [];

        private List<List<Display>> _displays = [];

        /**
         * @brief Configures a mock screen capture service with test displays and graphics cards
         * 
         * @param displayCount Number of display configurations to simulate
         * 
         * @return Configured mock screen capture service
         * 
         * Creates a simulated hardware environment with multiple graphics cards and displays,
         * ensuring the bot can correctly handle various hardware configurations that users
         * might have when running the game across different monitor setups.
         */
        private MockScreenCaptureService _serviceFixture(int displayCount)
        {
            MockScreenCaptureService service = new MockScreenCaptureService();
            for (int i = 0; i < displayCount; i++)
            {
                _graphicsCards.Add(new GraphicsCard());
                _displays.Add(new List<Display> { new Display() });
                service.GetDisplaysReturn.Add(_displays[i]);
                service.GetDisplaysReturn.Add(_displays[i]);
            }
            service.GetGraphicsCardsReturn.Add(_graphicsCards);
            service.GetGraphicsCardsReturn.Add(_graphicsCards);
            return service;
        }

        /**
         * @brief Creates a complete test environment for display monitoring testing
         * 
         * @return Configured GameMonitorDisplaysHelper instance
         * 
         * Prepares a comprehensive test environment that simulates the bot's display
         * detection system, ensuring it can properly identify and manage all available
         * monitors and graphics hardware for optimal game monitoring.
         */
        private GameMonitorDisplaysHelper _helperFixture()
        {
            _graphicsCards.Clear();
            _displays.Clear();
            return new GameMonitorDisplaysHelper(_serviceFixture(5));
        }

        /**
         * @brief Tests correct registration of all available displays
         * 
         * Validates that the bot properly detects and registers all connected displays,
         * ensuring it can monitor game content across multiple monitors and provide
         * comprehensive coverage of the player's gaming environment.
         */
        private void _testRegisterDisplaysRegistersAllKnownDisplays()
        {
            var helper = _helperFixture();
            helper.RegisterDisplays();
            var result = helper.GetDisplays();
            Debug.Assert(_displays.Count == result.Count());
            for (int i = 0; i < _displays.Count; i++)
                Debug.Assert(_displays[i][0] == result.ElementAt(i));
        }

        /**
         * @brief Tests idempotent behavior of display registration
         * 
         * Validates that repeatedly calling display registration doesn't create duplicate
         * entries or cause errors, ensuring stable operation even if the display detection
         * process is triggered multiple times during bot operation.
         */
        private void _testRegisterDisplaysRegisteringMultipleTimes()
        {
            var helper = _helperFixture();
            helper.RegisterDisplays();
            helper.RegisterDisplays();
            var result = helper.GetDisplays();
            Debug.Assert(_displays.Count == result.Count());
            for (int i = 0; i < _displays.Count; i++)
                Debug.Assert(_displays[i][0] == result.ElementAt(i));
        }

        /**
         * @brief Executes all display monitoring functionality tests
         * 
         * Runs the complete test suite to ensure the bot will correctly detect and manage
         * all available displays and graphics hardware, providing confidence that the
         * screen monitoring system will work reliably across various hardware configurations.
         */
        public void Run()
        {
            _testRegisterDisplaysRegistersAllKnownDisplays();
            _testRegisterDisplaysRegisteringMultipleTimes();
        }
    }


    /**
     * @class GameCaptureAreaHelperTest
     * 
     * @brief Unit tests for verifying proper game window capture area calculation
     * 
     * This test class validates that the bot correctly calculates capture areas for game windows,
     * ensuring accurate screen region detection and monitoring for gameplay automation across
     * different window configurations and monitor setups.
     */
    internal class GameCaptureAreaHelperTest
    {
        private MockWindowsLibrary _windowsLibrary = new MockWindowsLibrary();

        /**
         * @brief Configures mock Windows API library with test data
         * 
         * @param lpPoint Simulated client point coordinates for testing
         * @param lpRect Simulated client rectangle dimensions for testing
         * 
         * Sets up a mock Windows API interface that simulates system calls for window
         * coordinate translation.
         */
        private void _windowsLibraryFixture(POINTSTRUCT lpPoint, RECT lpRect)
        {
            _windowsLibrary = new MockWindowsLibrary();
            _windowsLibrary.ClientToScreenRefCallArg_lpPoint.Add(lpPoint);
            _windowsLibrary.GetClientRectOutCallArg_lpRect.Add(lpRect);
            _windowsLibrary.ClientToScreenReturn.Add(true);
            _windowsLibrary.GetClientRectReturn.Add(true);
        }

        /**
         * @brief Creates a complete test environment for capture area calculation
         * 
         * @param lpPoint Simulated client point coordinates
         * @param lpRect Simulated client rectangle dimensions
         * @param rcMonitor Simulated monitor dimensions
         * @return Configured GameCaptureAreaHelper instance
         * 
         * Prepares a comprehensive test environment that simulates the bot's complete
         * window capture area calculation pipeline.
         */
        private GameScreenCaptureHelper _captureAreafixture(
            POINTSTRUCT lpPoint, RECT lpRect
        ) {
            _windowsLibraryFixture(lpPoint, lpRect);
            return new GameScreenCaptureHelper(
                new MockScreenCaptureService(), _windowsLibrary
            );
        }

        /**
         * @brief Tests correct identification and handling of game window handles
         * 
         * Validates that the bot properly uses the game window handle and translates
         * them into screen coordinates, ensuring it can reliably locate and monitor
         * the game window even if it moves or resizes during gameplay.
         */
        private void _testGetCaptureAreaCapturesWindowHandle()
        {
            var areaFixture = _captureAreafixture(
                new POINTSTRUCT(12, 34),
                new RECT { left = 67, top = 12, right = 1067, bottom = 2012 }
            );
            var monitorInfo = new MONITORINFOEX { rcMonitor = new RECT { left = 5678, top = 2345 } };
            var captureArea = areaFixture.GetCaptureArea(0x1234, monitorInfo);
            Debug.Assert(_windowsLibrary.GetClientRectCalls == 1);
            Debug.Assert(_windowsLibrary.ClientToScreenCalls == 1);
            Debug.Assert(_windowsLibrary.GetClientRectCallArg_hWnd[0] == 0x1234);
            Debug.Assert(_windowsLibrary.ClientToScreenCallArg_hWnd[0] == 0x1234);
        }

        /**
         * @brief Tests accurate calculation of game window capture dimensions
         * 
         * Validates that the bot correctly calculates the precise dimensions and position
         * of the game window relative to the monitor, ensuring accurate screen capture
         * regions for reliable game state monitoring and interaction.
         */
        private void _testGetCaptureAreaReturnsCorrectDimensions()
        {
            var areaFixture = _captureAreafixture(
                new POINTSTRUCT(12, 34),
                new RECT { left = 67, top = 12, right = 1067, bottom = 2012 }
            );
            var monitorInfo = new MONITORINFOEX { rcMonitor = new RECT { left = 5678, top = 2345 } };
            var captureArea = areaFixture.GetCaptureArea(0x1234, monitorInfo);
            Debug.Assert(captureArea != null);
            Debug.Assert(captureArea.Item1 == -5666);
            Debug.Assert(captureArea.Item2 == -2311);
            Debug.Assert(captureArea.Item3 == 1000);
            Debug.Assert(captureArea.Item4 == 2000);
        }

        /**
         * @brief Executes all capture area calculation tests
         * 
         * Runs the complete test suite to ensure the bot will correctly calculate
         * game window capture areas across various window configurations and monitor
         * setups.
         */
        public void Run()
        {
            _testGetCaptureAreaCapturesWindowHandle();
            _testGetCaptureAreaReturnsCorrectDimensions();
        }
    }


    /**
     * @class GameScreenCaptureTest
     * @brief Unit tests for verifying the complete screen capture pipeline functionality
     * 
     * This test class validates the entire screen capture process from initialization to
     * image generation, ensuring the bot can reliably capture and process game screen
     * content for accurate visual detection and automation decision-making.
     */
    internal class GameScreenCaptureModuleTest
    {
        private MockWindowsLibrary _windowsLibrary = new MockWindowsLibrary();

        private MockCaptureMonitorHelper _captureMonitorHelper = new MockCaptureMonitorHelper();

        private MockMonitorDisplaysHelper _monitorDisplaysHelper = new MockMonitorDisplaysHelper();

        private MockScreenCaptureHelper _screenCaptureHelper = new MockScreenCaptureHelper();

        private MockCaptureImageGenerator _captureImageGenerator = new MockCaptureImageGenerator();

        private MockScreenCapture _screenCapture = new MockScreenCapture();

        private MockCaptureZone _captureZone = new MockCaptureZone();

        private MONITORINFOEX _monitorInfo = new MONITORINFOEX();

        private Tuple<int, int, int, int> _captureArea = new Tuple<int, int, int, int>(0, 0, 0, 0);

        private List<Display> _displays = [];

        private SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgra32> _image = 
            new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgra32>(2, 2)
        ;

        /**
         * @brief Initializes fresh mock instances for each test
         * 
         * Ensures each test starts with clean mock objects, preventing test interference
         * and guaranteeing consistent test results regardless of execution order.
         */
        private void _setupNewFixtures()
        {
            _monitorInfo = new MONITORINFOEX();
            _screenCapture = new MockScreenCapture();
            _captureArea = new Tuple<int, int, int, int>(12, 34, 56, 78);
            _captureZone = new MockCaptureZone();
            _windowsLibrary = new MockWindowsLibrary();
            _captureMonitorHelper = new MockCaptureMonitorHelper();
            _monitorDisplaysHelper = new MockMonitorDisplaysHelper();
            _screenCaptureHelper = new MockScreenCaptureHelper();
            _captureImageGenerator = new MockCaptureImageGenerator();
        }

        /**
         * @brief Configures mock return values for test operations
         * 
         * Sets up predefined responses for mock method calls, ensuring predictable
         * behavior during tests and reliable validation of the screen capture pipeline.
         */
        private void _setupFixtureReturns()
        {
            _windowsLibrary.SetProcessDpiAwarenessReturn.Add(1);
            _monitorDisplaysHelper.GetDisplaysReturn.Add(_displays);
            _captureMonitorHelper.GetMonitorInfoReturn.Add(_monitorInfo);
            _screenCaptureHelper.GetScreenCaptureReturn.Add(_screenCapture);
            _screenCaptureHelper.GetCaptureAreaReturn.Add(_captureArea);
            _screenCaptureHelper.UpdateCaptureZoneReturn.Add(_captureZone);
            _captureImageGenerator.GenerateImageReturn.Add(_image);
            _screenCapture.CaptureScreenReturn.Add(true);
        }

        /**
         * @brief Synchronizes call tracking across all mock objects
         * 
         * Ensures consistent call order tracking across all mock components, enabling
         * precise validation of operation sequencing within the screen capture pipeline.
         */
        private void _setupCallOrders()
        {
            List<string> callOrder = [];
            _windowsLibrary.CallOrder = callOrder;
            _captureMonitorHelper.CallOrder = callOrder;
            _monitorDisplaysHelper.CallOrder = callOrder;
            _screenCaptureHelper.CallOrder = callOrder;
            _captureImageGenerator.CallOrder = callOrder;
            _screenCapture.CallOrder = callOrder;
            _captureZone.CallOrder = callOrder;
        }

        /**
         * @brief Creates a complete test environment for screen capture testing
         * 
         * @return Configured GameScreenCapture instance
         * 
         * Prepares a comprehensive test environment with all dependencies mocked,
         * enabling thorough testing of the screen capture system's integration points.
         */
        private GameScreenCaptureModule _fixture()
        {
            _setupNewFixtures();
            _setupFixtureReturns();
            return new GameScreenCaptureModule(
                _windowsLibrary,
                _captureMonitorHelper,
                _monitorDisplaysHelper,
                _screenCaptureHelper,
                _captureImageGenerator
            );
        }

        /**
         * @brief Tests correct DPI awareness configuration during initialization
         * 
         * Validates that the bot properly configures system DPI awareness settings,
         * ensuring accurate screen scaling and reliable visual detection across
         * different display configurations and resolution settings.
         */
        private void _testInitializeSetsDpiAwareness()
        {
            var screenCapture = _fixture();
            screenCapture.Initialize();
            Debug.Assert(_windowsLibrary.SetProcessDpiAwarenessCalls == 1);
            Debug.Assert(_windowsLibrary.SetProcessDpiAwarenessCallArg_value[0] == 2);
        }

        /**
         * @brief Tests correct screen capture registration during initialization
         * 
         * Validates that the bot properly registers all available screen capture
         * sources during initialization, ensuring comprehensive game monitoring
         * capabilities from the moment the bot starts operating.
         */
        private void _testInitializeSetsScreenCaptures()
        {
            var screenCapture = _fixture();
            screenCapture.Initialize();
            Debug.Assert(_screenCaptureHelper.RegisterScreenCapturesCalls == 1);
            Debug.Assert(_screenCaptureHelper.RegisterScreenCapturesCallArg_displays[0] == _displays);
        }

        /**
         * @brief Tests correct display detection during initialization
         * 
         * Validates that the bot properly detects and registers all connected
         * displays during initialization, ensuring immediate monitoring capability
         * across all available screens without manual configuration.
         */
        private void _testInitializeRegistersDisplays()
        {
            var screenCapture = _fixture();
            var callReference = new TestUtilities().Reference(_monitorDisplaysHelper);
            screenCapture.Initialize();
            Debug.Assert(_monitorDisplaysHelper.CallOrder.Count == 2);
            Debug.Assert(_monitorDisplaysHelper.CallOrder[0] == callReference + "RegisterDisplays");
            Debug.Assert(_monitorDisplaysHelper.CallOrder[1] == callReference + "GetDisplays");
        }

        /**
         * @brief Tests correct monitor information retrieval during capture
         * 
         * Validates that the bot properly retrieves monitor information for the
         * specified game window, ensuring accurate display identification and
         * appropriate capture area calculation.
         */
        private void _testCaptureGetsMonitorInfoFromWindow()
        {
            var screenCapture = _fixture();
            screenCapture.Capture(0x1234);
            Debug.Assert(_captureMonitorHelper.GetMonitorInfoCalls == 1);
            Debug.Assert(_captureMonitorHelper.GetMonitorInfoCallArg_windowHandle[0] == 0x1234);
        }

        /**
         * @brief Tests correct capture area calculation during capture
         * 
         * Validates that the bot properly calculates the capture area based on
         * window position and monitor information, ensuring the correct screen
         * region is monitored for game content.
         */
        private void _testCaptureGetsCaptureAreaFromWindowAndMonitor()
        {
            var screenCapture = _fixture();
            screenCapture.Capture(0x1234);
            Debug.Assert(_screenCaptureHelper.GetCaptureAreaCalls == 1);
            Debug.Assert(_screenCaptureHelper.GetCaptureAreaCallArg_windowHandle[0] == 0x1234);
            Debug.Assert(_screenCaptureHelper.GetCaptureAreaCallArg_monitorInfo[0] == _monitorInfo);
        }

        /**
         * @brief Tests correct screen capture instance retrieval
         * 
         * Validates that the bot properly retrieves the appropriate screen capture
         * instance for the specified window and monitor, ensuring game content is
         * captured from the correct display.
         */
        private void _testCaptureGetsScreenCaptureFromWindowAndMonitor()
        {
            var screenCapture = _fixture();
            screenCapture.Capture(0x1234);
            Debug.Assert(_screenCaptureHelper.GetScreenCaptureCalls == 1);
            Debug.Assert(_screenCaptureHelper.GetScreenCaptureCallArg_windowHandle[0] == 0x1234);
            Debug.Assert(_screenCaptureHelper.GetScreenCaptureCallArg_monitorInfo[0] == _monitorInfo);
        }

        /**
         * @brief Tests correct capture zone configuration during capture
         * 
         * Validates that the bot properly updates the capture zone with the
         * calculated capture area, ensuring the screen capture instance monitors
         * the correct region of the display for game content.
         */
        private void _testCaptureUpdatesCaptureZoneWithScreenCaptureAndCaptureArea()
        {
            var screenCapture = _fixture();
            screenCapture.Capture(0x1234);
            Debug.Assert(_screenCaptureHelper.UpdateCaptureZoneCalls == 1);
            Debug.Assert(_screenCaptureHelper.UpdateCaptureZoneCallArg_capture[0] == _screenCapture);
            Debug.Assert(_screenCaptureHelper.UpdateCaptureZoneCallArg_captureArea[0] == _captureArea);
        }

        /**
         * @brief Tests correct image generation from captured content
         * 
         * Validates that the bot properly generates usable images from captured
         * screen content, ensuring accurate visual representation of game elements
         * for reliable UI detection and analysis.
         */
        private void _testCaptureGeneratesImageFromCaptureZone()
        {
            var screenCapture = _fixture();
            var result = screenCapture.Capture(0x1234);
            Debug.Assert(_image == result);
            Debug.Assert(_captureImageGenerator.GenerateImageCalls == 1);
            Debug.Assert(_captureImageGenerator.GenerateImageCallArg_captureZone[0] == _captureZone);
        }

        /**
         * @brief Tests correct operation sequencing during capture
         * 
         * Validates that the bot performs screen capture before image generation,
         * ensuring the most current game content is used for visual analysis and
         * decision-making during automation.
         */
        private void _testCapturePerformsScreenCaptureBeforeGeneratingImage()
        {
            var utils = new TestUtilities();
            var screenCapture = _fixture();
            var captureScreenRef = utils.Reference(_screenCapture) + "CaptureScreen";
            var generateImageRef = utils.Reference(_captureImageGenerator) + "GenerateImage";
            _setupCallOrders();
            var callOrder = _windowsLibrary.CallOrder;
            screenCapture.Capture(0x1234);
            var captureScreenIndex = callOrder.IndexOf(captureScreenRef);
            var generateImageIndex = callOrder.IndexOf(generateImageRef);
            Debug.Assert(captureScreenIndex != -1);
            Debug.Assert(generateImageIndex != -1);
            Debug.Assert(captureScreenIndex + 1 == generateImageIndex);
            Debug.Assert(generateImageIndex == callOrder.Count - 1);
        }


        /**
         * @brief Executes all screen capture functionality tests
         * 
         * Runs the complete test suite to ensure the bot will correctly handle all
         * aspects of screen capture operations, providing confidence that visual
         * monitoring features will work reliably during gameplay automation.
         */
        public void Run()
        {
            _testInitializeSetsDpiAwareness();
            _testInitializeSetsScreenCaptures();
            _testInitializeRegistersDisplays();
            _testCaptureGetsMonitorInfoFromWindow();
            _testCaptureGetsCaptureAreaFromWindowAndMonitor();
            _testCaptureGetsScreenCaptureFromWindowAndMonitor();
            _testCaptureUpdatesCaptureZoneWithScreenCaptureAndCaptureArea();
            _testCaptureGeneratesImageFromCaptureZone();
            _testCapturePerformsScreenCaptureBeforeGeneratingImage();
        }
    }


    /**
     * @class GameScreenCaptureOrchestratorTest
     * 
     * @brief Unit tests for verifying the complete screen capture orchestration process
     * 
     * This test class validates the high-level coordination of screen capture operations,
     * ensuring the bot properly manages window detection, capture initialization, and
     * image retrieval to provide reliable game monitoring throughout automation sessions.
     */
    internal class GameScreenCaptureOrchestratorTest
    {
        private MockWindowsLibrary _windowsLibrary = new MockWindowsLibrary();

        private MockScreenCaptureModule _screenCaptureModule = new MockScreenCaptureModule();

        private MockWindowHandleHelper _windowHandleHelper = new MockWindowHandleHelper();

        private SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgra32> _image = 
            new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgra32>(2, 2)
        ;

        /**
          * @brief Synchronizes call tracking across all mock objects
          * 
          * Ensures consistent call order tracking across all mock components, enabling
          * precise validation of operation sequencing within the screen capture orchestration
          * process and ensuring proper coordination between different system components.
          */
        private void _setupCallOrders()
        {
            List<string> callOrder = [];
            _windowsLibrary.CallOrder = callOrder;
            _screenCaptureModule.CallOrder = callOrder;
            _windowHandleHelper.CallOrder = callOrder;
        }

        /**
         * @brief Initializes fresh mock instances for each test
         * 
         * Ensures each test starts with clean mock objects, preventing test interference
         * and guaranteeing consistent test results regardless of execution order.
         */
        private void _setupNewTestObjects()
        {
            _windowsLibrary = new MockWindowsLibrary();
            _screenCaptureModule = new MockScreenCaptureModule();
            _windowHandleHelper = new MockWindowHandleHelper();
        }

        /**
         * @brief Configures mock return values for test operations
         * 
         * Sets up predefined responses for mock method calls, ensuring predictable
         * behavior during tests and reliable validation of the screen capture orchestration
         * process under various conditions.
         */
        private void _setupCallReturns()
        {
            _windowsLibrary.IsWindowReturn.Add(false);
            _windowsLibrary.IsWindowReturn.Add(true);
            _windowsLibrary.IsWindowReturn.Add(true);
            _windowsLibrary.IsWindowReturn.Add(true);
            _windowsLibrary.IsWindowReturn.Add(true);
            _windowHandleHelper.FindByProcessReturn.Add(0x1234);
            _screenCaptureModule.CaptureReturn.Add(_image);
            _screenCaptureModule.CaptureReturn.Add(_image);
        }

        /**
         * @brief Creates a standard test environment for screen capture orchestration
         * 
         * @return Configured GameScreenCaptureOrchestrator instance
         * 
         * Prepares a comprehensive test environment with all dependencies mocked,
         * enabling thorough testing of the screen capture orchestration system's
         * integration points and operational flow.
         */
        private GameScreenCaptureOrchestrator _fixture()
        {
            _setupNewTestObjects();
            _setupCallOrders();
            _setupCallReturns();
            return new GameScreenCaptureOrchestrator(
                _windowsLibrary, _screenCaptureModule, _windowHandleHelper
            );
        }


        /**
         * @brief Creates a test environment with simulated capture failures
         * 
         * @return Configured GameScreenCaptureOrchestrator instance with failure scenario
         * 
         * Prepares a specialized test environment that simulates image capture failures,
         * ensuring the bot properly handles error conditions and maintains reliability
         * when encountering temporary issues with screen capture operations.
         */
        private GameScreenCaptureOrchestrator _fixtureImageFail()
        {
            var captureOrchestrator = _fixture();
            _windowsLibrary.IsWindowReturn.Add(true);
            _windowsLibrary.IsWindowReturn.Add(true);
            _windowsLibrary.IsWindowReturn.Add(false);
            _windowsLibrary.IsWindowReturn.Add(true);
            _windowsLibrary.IsWindowReturn.Add(true);
            _windowHandleHelper.FindByProcessReturn.Add(0x2345);
            _screenCaptureModule.CaptureReturn.Add(null);
            _screenCaptureModule.CaptureReturn.Add(_image);
            captureOrchestrator.Capture("MeowScratch");
            captureOrchestrator.Capture("MeowScratch");
            return captureOrchestrator;
        }

        /**
          * @brief Tests correct initialization sequencing during first capture
          * 
          * Validates that the bot properly initializes screen capture components before
          * attempting to capture game content, ensuring all systems are ready for
          * reliable operation from the first capture attempt.
          */
        private void _testCapturePerformsInitializationBeforeScreenCapture()
        {
            var utils = new TestUtilities();
            var captureOrchestrator = _fixture();
            var windowsLibraryRef = utils.Reference(_windowsLibrary);
            var screenCaptureModuleRef = utils.Reference(_screenCaptureModule);
            var callOrder = _windowsLibrary.CallOrder;
            captureOrchestrator.Capture("MeowScratch");
            var initializeIndex = callOrder.IndexOf(screenCaptureModuleRef + "Initialize");
            var captureIndex = callOrder.IndexOf(screenCaptureModuleRef + "Capture");
            Debug.Assert(initializeIndex != -1);
            Debug.Assert(captureIndex != -1);
            Debug.Assert(initializeIndex < captureIndex);
        }

        /**
         * @brief Tests optimization of subsequent capture operations
         * 
         * Validates that the bot skips unnecessary initialization on subsequent capture
         * attempts after successful initialization.
         */
        private void _testCaptureAfterFindingWindowOnlyCapturesAndDoesntInitialize()
        {
            var utils = new TestUtilities();
            var captureOrchestrator = _fixture();
            var windowsLibraryRef = utils.Reference(_windowsLibrary);
            var screenCaptureModuleRef = utils.Reference(_screenCaptureModule);
            var callOrder = _windowsLibrary.CallOrder;
            captureOrchestrator.Capture("MeowScratch");
            callOrder.Clear();
            captureOrchestrator.Capture("MeowScratch");
            var initializeIndex = callOrder.IndexOf(screenCaptureModuleRef + "Initialize");
            var captureIndex = callOrder.IndexOf(screenCaptureModuleRef + "Capture");
            Debug.Assert(initializeIndex == -1);
            Debug.Assert(captureIndex != -1);
        }

        /**
         * @brief Tests correct image output from capture operations
         * 
         * Validates that the bot properly returns captured images from successful
         * screen capture operations, ensuring reliable visual data for game state
         * analysis and automation decision-making.
         */
        private void _testCaptureOutputsResultOfScreenModule()
        {
            var captureOrchestrator = _fixture();
            var result = captureOrchestrator.Capture("MeowScratch");
            Debug.Assert(result == _image);
        }


        /**
         * @brief Tests proper handling of capture failures
         * 
         * Validates that the bot gracefully handles screen capture failures by
         * returning null results, ensuring the automation system can respond
         * appropriately when temporary issues prevent successful image capture.
         */
        private void _testCaptureOutputsNullIfCaptureFails()
        {
            var captureOrchestrator = _fixture();
            _screenCaptureModule.CaptureReturn.Clear();
            _screenCaptureModule.CaptureReturn.Add(null);
            var result = captureOrchestrator.Capture("MeowScratch");
            Debug.Assert(result == null);
        }

        /**
         * @brief Tests correct window handle usage during capture
         * 
         * Validates that the bot properly uses the correct window handle for screen
         * capture operations after identifying the game window, ensuring accurate
         * targeting of the game application for content monitoring.
         */
        private void _testCaptureUsesHandleAfterDetectingWindow()
        {
            var captureOrchestrator = _fixture();
            captureOrchestrator.Capture("MeowScratch");
            Debug.Assert(_screenCaptureModule.CaptureCalls == 1);
            Debug.Assert(_screenCaptureModule.CaptureCallArg_windowHandle[0] == 0x1234);
        }

        /**
         * @brief Tests window validation during capture operations
         * 
         * Validates that the bot verifies window existence during
         * capture operations, ensuring it can adapt to game window changes
         * and maintain reliable monitoring throughout automation sessions.
         */
        private void _testCaptureUsesHandleToCheckForWindowAfterDetectingWindow()
        {
            var captureOrchestrator = _fixture();
            captureOrchestrator.Capture("MeowScratch");
            Debug.Assert(_windowsLibrary.IsWindowCalls == 3);
            Debug.Assert(_windowsLibrary.IsWindowCallArg_hWnd[0] == 0x0000);
            Debug.Assert(_windowsLibrary.IsWindowCallArg_hWnd[1] == 0x1234);
            Debug.Assert(_windowsLibrary.IsWindowCallArg_hWnd[2] == 0x1234);
            captureOrchestrator.Capture("MeowScratch");
            Debug.Assert(_windowsLibrary.IsWindowCalls == 5);
            Debug.Assert(_windowsLibrary.IsWindowCallArg_hWnd[3] == 0x1234);
            Debug.Assert(_windowsLibrary.IsWindowCallArg_hWnd[4] == 0x1234);
        }

        /**
         * @brief Tests correct process name usage for window detection
         * 
         * Validates that the bot properly uses the specified process name to
         * locate the game window, ensuring accurate targeting of the correct
         * application for screen capture operations.
         */
        private void _testCaptureHandleIsFoundUsingProcessName()
        {
            var captureOrchestrator = _fixture();
            captureOrchestrator.Capture("MeowScratch");
            Debug.Assert(_windowHandleHelper.FindByProcessCalls == 1);
            Debug.Assert(_windowHandleHelper.FindByProcessCallArg_processName[0] == "MeowScratch");
        }


        /**
         * @brief Tests recovery from capture failures
         * 
         * Validates that the bot properly recovers from screen capture failures
         * by reinitializing components and reattempting window detection,
         * ensuring resilience and continuous operation despite temporary issues.
         */
        private void _testCaptureResetsAfterDetectingNoCapturedImage()
        {
            var captureOrchestrator = _fixtureImageFail();
            var noImageResult = captureOrchestrator.Capture("MeowScratch");
            Debug.Assert(noImageResult == null);
            Debug.Assert(_windowHandleHelper.FindByProcessCalls == 1);
            Debug.Assert(_screenCaptureModule.InitializeCalls == 1);
            var imageResult = captureOrchestrator.Capture("MeowScratch");
            Debug.Assert(imageResult == _image);
            Debug.Assert(_windowHandleHelper.FindByProcessCalls == 2);
            Debug.Assert(_screenCaptureModule.InitializeCalls == 2);
            Debug.Assert(_windowHandleHelper.FindByProcessCallArg_processName[1] == "MeowScratch");
            Debug.Assert(_screenCaptureModule.CaptureCallArg_windowHandle.Last() == 0x2345);
        }

        /**
         * @brief Executes all screen capture orchestration tests
         * 
         * Runs the complete test suite to ensure the bot will correctly orchestrate
         * all aspects of screen capture operations, providing confidence that the
         * complete monitoring pipeline will work reliably during gameplay automation.
         */
        public void Run()
        {
            _testCapturePerformsInitializationBeforeScreenCapture();
            _testCaptureAfterFindingWindowOnlyCapturesAndDoesntInitialize();
            _testCaptureOutputsResultOfScreenModule();
            _testCaptureOutputsNullIfCaptureFails();
            _testCaptureUsesHandleAfterDetectingWindow();
            _testCaptureUsesHandleToCheckForWindowAfterDetectingWindow();
            _testCaptureHandleIsFoundUsingProcessName();
            _testCaptureResetsAfterDetectingNoCapturedImage();
        }
    }


    public class CaptureModuleTestSuite
    {
        public void Run()
        {
            new GameScreenCaptureHelperTest().Run();
            new GameCaptureImageGeneratorTest().Run();
            new GameMonitorDisplaysHelperTest().Run();
            new GameCaptureAreaHelperTest().Run();
            new GameScreenCaptureModuleTest().Run();
            new GameScreenCaptureOrchestratorTest().Run();
        }
    }
}
