using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNetTests.Systems.Tests;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class WindowRuneSolverAPILoadActionHandlerTests
    {
        private MockSystemWindow _systemWindow = new MockSystemWindow();

        private TextBox _ipAddressTextBox = new TextBox();

        private TextBox _portTextBox = new TextBox();

        private TextBox _routeTextBox = new TextBox();

        private TextBox _classTagTextBox = new TextBox();

        private TextBox _leftArrowTextBox = new TextBox();

        private TextBox _upArrowTextBox = new TextBox();

        private TextBox _rightArrowTextBox = new TextBox();

        private TextBox _downArrowTextBox = new TextBox();

        private TextBox _interactKeyTextBox = new TextBox();

        private TextBox _cashShopKeyTextBox = new TextBox();

        private TextBox _cashShopTimeoutTextBox = new TextBox();

        private TextBox _runeRetriesTextBox = new TextBox();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        public AbstractWindowActionHandler _fixture()
        {
            _systemWindow = new MockSystemWindow();
            _systemWindow.GetWindowReturn.Add(new Window());
            _ipAddressTextBox = new TextBox();
            _portTextBox = new TextBox();
            _routeTextBox = new TextBox();
            _classTagTextBox = new TextBox();
            _leftArrowTextBox = new TextBox();
            _upArrowTextBox = new TextBox();
            _rightArrowTextBox = new TextBox();
            _downArrowTextBox = new TextBox();
            _interactKeyTextBox = new TextBox();
            _cashShopKeyTextBox = new TextBox();
            _cashShopTimeoutTextBox = new TextBox();
            _runeRetriesTextBox = new TextBox();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                RuneDetection = new RuneDetection
                {
                    RuneSolverIPAddress = "12",
                    RuneSolverPort = "23",
                    RuneSolverRoute = "34",
                    ClassTag = "45",
                    Left = "78",
                    Up = "89",
                    Right = "90",
                    Down = "01"
                },
                MacroKeySettings = new MacroKeySettings
                {
                    RuneInteractKey = "00",
                    CashShopKey = "234"
                },
                MacroSettings = new MacroSettings
                {
                    CashShopTimeout = 567,
                    CashShopTolerance = 24,
                }
            };
            var handler = new WindowRuneSolverAPILoadActionHandlerFacade(
                _systemWindow,
                _ipAddressTextBox,
                _portTextBox,
                _routeTextBox,
                _classTagTextBox,
                _leftArrowTextBox,
                _upArrowTextBox,
                _rightArrowTextBox,
                _downArrowTextBox,
                _interactKeyTextBox,
                _cashShopKeyTextBox,
                _cashShopTimeoutTextBox,
                _runeRetriesTextBox
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Verifies that when the  API settings window becomes visible,
         * the saved configuration values are loaded into the input text boxes
         * 
         * When users open the  API settings window to configure rune detection
         * for the rune solver, the system must display the currently saved tags in
         * the text boxes. This includes the workspace name, workspace ID, API key,
         * array settings, detected coordinates, and the arrow key mappings (left,
         * up, right, down) used for solving rune puzzles.
         */
        private void _testShowingWindowPopulatesTextBoxes()
        {
            foreach (var visible in new[] { true, false })
            {
                var APILoadActionHandler = _fixture();
                _systemWindow.VisibleReturn.Add(visible);
                APILoadActionHandler.OnDependencyEvent(
                    new object(),
                    new DependencyPropertyChangedEventArgs()
                );
                Debug.Assert(_ipAddressTextBox.Text == (visible ? "12" : ""));
                Debug.Assert(_portTextBox.Text == (visible ? "23" : ""));
                Debug.Assert(_routeTextBox.Text == (visible ? "34" : ""));
                Debug.Assert(_classTagTextBox.Text == (visible ? "45" : ""));
                Debug.Assert(_leftArrowTextBox.Text == (visible ? "78" : ""));
                Debug.Assert(_upArrowTextBox.Text == (visible ? "89" : ""));
                Debug.Assert(_rightArrowTextBox.Text == (visible ? "90" : ""));
                Debug.Assert(_downArrowTextBox.Text == (visible ? "01" : ""));
                Debug.Assert(_interactKeyTextBox.Text == (visible ? "00" : ""));
                Debug.Assert(_cashShopKeyTextBox.Text == (visible ? "234" : ""));
                Debug.Assert(_cashShopTimeoutTextBox.Text == (visible ? "567" : ""));
                Debug.Assert(_runeRetriesTextBox.Text == (visible ? "24" : ""));
            }
        }

        public void Run()
        {
            _testShowingWindowPopulatesTextBoxes();
        }
    }


    public class WindowRuneSolverAPISaveActionHandlerTests
    {
        private MockSystemWindow _systemWindow = new MockSystemWindow();

        private TextBox _ipAddressTextBox = new TextBox();

        private TextBox _portTextBox = new TextBox();

        private TextBox _routeTextBox = new TextBox();

        private TextBox _classTagTextBox = new TextBox();

        private TextBox _leftArrowTextBox = new TextBox();

        private TextBox _upArrowTextBox = new TextBox();

        private TextBox _rightArrowTextBox = new TextBox();

        private TextBox _downArrowTextBox = new TextBox();

        private TextBox _interactKeyTextBox = new TextBox();

        private TextBox _cashShopKeyTextBox = new TextBox();

        private TextBox _cashShopTimeoutTextBox = new TextBox();

        private TextBox _runeRetriesTextBox = new TextBox();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        public AbstractWindowActionHandler _fixture()
        {
            _systemWindow = new MockSystemWindow();
            _systemWindow.GetWindowReturn.Add(new Window());
            _ipAddressTextBox = new TextBox { Text = "12" };
            _portTextBox = new TextBox { Text = "23" };
            _routeTextBox = new TextBox { Text = "34" };
            _classTagTextBox = new TextBox { Text = "45" };
            _leftArrowTextBox = new TextBox { Text = "78" };
            _upArrowTextBox = new TextBox { Text = "89" };
            _rightArrowTextBox = new TextBox { Text = "90" };
            _downArrowTextBox = new TextBox { Text = "01" };
            _interactKeyTextBox = new TextBox { Text = "00" };
            _cashShopKeyTextBox = new TextBox { Text = "234" };
            _cashShopTimeoutTextBox = new TextBox { Text = "567" };
            _runeRetriesTextBox = new TextBox { Text = "24" };
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration();
            var handler = new WindowRuneSolverAPISaveActionHandlerFacade(
                _systemWindow,
                _ipAddressTextBox,
                _portTextBox,
                _routeTextBox,
                _classTagTextBox,
                _leftArrowTextBox,
                _upArrowTextBox,
                _rightArrowTextBox,
                _downArrowTextBox,
                _interactKeyTextBox,
                _cashShopKeyTextBox,
                _cashShopTimeoutTextBox,
                _runeRetriesTextBox
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Verifies that when the  API settings window is closed or hidden,
         * the text box values are saved to the configuration
         * 
         * When users modify their  API settings (workspace name, API key,
         * arrow key mappings for rune puzzles, etc.) and then close the settings window,
         * the system must save the new values to the configuration. This ensures that
         * the rune solver will use the updated settings for future rune detection and
         * puzzle-solving operations.
         */
        private void _testHidingWindowSavesTextBoxData()
        {
            foreach (var visible in new[] { true, false })
            {
                var APILoadActionHandler = _fixture();
                _systemWindow.VisibleReturn.Add(visible);
                APILoadActionHandler.OnDependencyEvent(
                    new object(),
                    new DependencyPropertyChangedEventArgs()
                );
                var runeDetection = _maplestoryBotConfiguration.RuneDetection;
                var macroKeySettings = _maplestoryBotConfiguration.MacroKeySettings;
                var macroSettings = _maplestoryBotConfiguration.MacroSettings;
                Debug.Assert(runeDetection.RuneSolverIPAddress == (!visible ? "12" : ""));
                Debug.Assert(runeDetection.RuneSolverPort == (!visible ? "23" : ""));
                Debug.Assert(runeDetection.RuneSolverRoute == (!visible ? "34" : ""));
                Debug.Assert(runeDetection.ClassTag == (!visible ? "45" : ""));
                Debug.Assert(runeDetection.Left == (!visible ? "78" : ""));
                Debug.Assert(runeDetection.Up == (!visible ? "89" : ""));
                Debug.Assert(runeDetection.Right == (!visible ? "90" : ""));
                Debug.Assert(runeDetection.Down == (!visible ? "01" : ""));
                Debug.Assert(macroKeySettings.RuneInteractKey == (!visible ? "00" : ""));
                Debug.Assert(macroKeySettings.CashShopKey == (!visible ? "234" : ""));
                Debug.Assert(macroSettings.CashShopTimeout == (!visible ? 567 : 60));
                Debug.Assert(macroSettings.CashShopTolerance == (!visible ? 24 : 3));
            }
        }

        public void Run()
        {
            _testHidingWindowSavesTextBoxData();
        }
    }


    public class WindowRuneSolverAPIOutputActionHandlerTests
    {
        private TextBox _classTagTextBox = new TextBox();

        private TextBox _leftArrowTextBox = new TextBox();

        private TextBox _upArrowTextBox = new TextBox();

        private TextBox _rightArrowTextBox = new TextBox();

        private TextBox _downArrowTextBox = new TextBox();

        private TextBlock _outputFormatTextBlock = new TextBlock();

        private AbstractWindowActionHandler _fixture()
        {
            _classTagTextBox = new TextBox();
            _leftArrowTextBox = new TextBox();
            _upArrowTextBox = new TextBox();
            _rightArrowTextBox = new TextBox();
            _downArrowTextBox = new TextBox();
            _outputFormatTextBlock = new TextBlock();
            return new WindowRuneSolverAPIOutputActionHandlerFacade(
                _classTagTextBox,
                _leftArrowTextBox,
                _upArrowTextBox,
                _rightArrowTextBox,
                _downArrowTextBox,
                _outputFormatTextBlock
            );
        }

        /**
         * @brief Verifies that when users edit any of the  API format settings,
         * the output format preview updates in real-time
         * 
         * When users configure the  API settings for rune detection (such as the
         * array name, coordinate field names X/Y, and the arrow key class labels for
         * up/down/left/right), the system displays a live preview of the expected JSON
         * output format. This preview helps users understand how their rune detection
         * predictions will be formatted by the  API.
         */
        private void _testChangingConfigurationTextUpdatesOutput()
        {
            var APIOutputActionHandler = _fixture();
            var textBoxes = new[] {
                _classTagTextBox,
                _leftArrowTextBox,
                _upArrowTextBox,
                _rightArrowTextBox,
                _downArrowTextBox
            };
            for (int i = 0; i < textBoxes.Length; i++)
            {
                var textBox = textBoxes[i];
                textBox.Text = i.ToString() + (i + 1).ToString();
                Debug.Assert(
                    _outputFormatTextBlock.Text == (
                        "[\n" +
                        "  {\n" +
                        "    \"" + _classTagTextBox.Text + "\": <" +
                        "\"" + _leftArrowTextBox.Text + "\"|" +
                        "\"" + _upArrowTextBox.Text + "\"|" +
                        "\"" + _rightArrowTextBox.Text + "\"|" +
                        "\"" + _downArrowTextBox.Text + "\">\n" +
                        "  },\n" +
                        "  ...\n" +
                        "]\n"
                    )
                );
            }
        }
        public void Run()
        {
            _testChangingConfigurationTextUpdatesOutput();
        }
    }


    public class WindowRuneSolverAPIInjectActionHandlerTests
    {
        private MockSystemWindow _systemWindow = new MockSystemWindow();

        private MockInjectAction _injectAction = new MockInjectAction();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = new MaplestoryBotConfiguration();

        public AbstractWindowActionHandler _fixture()
        {
            _systemWindow = new MockSystemWindow();
            _injectAction = new MockInjectAction();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration();
            _systemWindow.GetWindowReturn.Add(new Window());
            var handler = new WindowRuneSolverAPIInjectActionHandlerFacade(_systemWindow);
            handler.Inject(SystemInjectType.InjectAction, _injectAction);
            handler.Inject(SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration);
            return handler;
        }

        /**
         * @brief Verifies that when the  API settings window is closed or hidden,
         * the system broadcasts the updated configuration to all dependent components
         * and triggers a configuration save
         * 
         * When users finish editing their  API settings (workspace name, API key,
         * arrow key mappings for rune puzzles, etc.) and close the settings window, the
         * system must inject the updated configuration into all components that rely on
         * rune detection settings (such as the rune solver executor and the  API
         * client).
         */
        private void _testHidingWindowInjectsConfigurationData()
        {
            foreach (var visible in new[] { true, false })
            {
                var dataType = new List<object>();
                var data = new List<object>();
                var APIInjectActionHandler = _fixture();
                _systemWindow.VisibleReturn.Add(visible);
                _injectAction.GetActionReturn.Add(
                    (_, __) => { dataType.Add(_); data.Add(__); }
                );
                APIInjectActionHandler.OnDependencyEvent(
                    new object(),
                    new DependencyPropertyChangedEventArgs()
                );
                if (!visible)
                {
                    Debug.Assert(dataType.Count == 2);
                    Debug.Assert(dataType[0] is SystemInjectType.ConfigurationUpdate);
                    Debug.Assert(dataType[1] is SystemInjectType.ConfigurationSave);
                    Debug.Assert(data.Count == 2);
                    Debug.Assert(data[0] == _maplestoryBotConfiguration);
                    Debug.Assert(data[1] == _maplestoryBotConfiguration);
                }
                else
                {
                    Debug.Assert(dataType.Count == 0);
                    Debug.Assert(data.Count == 0);
                }
            }
        }

        public void Run()
        {
            _testHidingWindowInjectsConfigurationData();
        }
    }


    public class WindowRuneSolverEditorHandlersTestSuite
    {
        public void Run()
        {
            new WindowRuneSolverAPILoadActionHandlerTests().Run();
            new WindowRuneSolverAPISaveActionHandlerTests().Run();
            new WindowRuneSolverAPIInjectActionHandlerTests().Run();
            new WindowRuneSolverAPIOutputActionHandlerTests().Run();
        }
    }
}
