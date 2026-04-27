using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNetTests.Systems.Tests;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class WindowRuneSolverRoboflowAPILoadActionHandlerTests
    {
        private MockSystemWindow _systemWindow = new MockSystemWindow();

        private TextBox _workspaceNameTextBox = new TextBox();

        private TextBox _workspaceIDTextBox = new TextBox();

        private TextBox _apiKeyTextBox = new TextBox();

        private TextBox _arrayTextBox = new TextBox();

        private TextBox _xTextBox = new TextBox();

        private TextBox _yTextBox = new TextBox();

        private TextBox _leftArrowTextBox = new TextBox();

        private TextBox _upArrowTextBox = new TextBox();

        private TextBox _rightArrowTextBox = new TextBox();

        private TextBox _downArrowTextBox = new TextBox();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        public AbstractWindowActionHandler _fixture()
        {
            _systemWindow = new MockSystemWindow();
            _systemWindow.GetWindowReturn.Add(new Window());
            _workspaceNameTextBox = new TextBox();
            _workspaceIDTextBox = new TextBox();
            _apiKeyTextBox = new TextBox();
            _arrayTextBox = new TextBox();
            _xTextBox = new TextBox();
            _yTextBox = new TextBox();
            _leftArrowTextBox = new TextBox();
            _upArrowTextBox = new TextBox();
            _rightArrowTextBox = new TextBox();
            _downArrowTextBox = new TextBox();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                RuneDetection = new RuneDetection
                {
                    WorkspaceName = "12",
                    WorkspaceID = "23",
                    APIKey = "34",
                    Array = "45",
                    X = "56",
                    Y = "67",
                    Left = "78",
                    Up = "89",
                    Right = "90",
                    Down = "01"
                }
            };
            var handler = new WindowRuneSolverRoboflowAPILoadActionHandlerFacade(
                _systemWindow,
                _workspaceNameTextBox,
                _workspaceIDTextBox,
                _apiKeyTextBox,
                _arrayTextBox,
                _xTextBox,
                _yTextBox,
                _leftArrowTextBox,
                _upArrowTextBox,
                _rightArrowTextBox,
                _downArrowTextBox
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Verifies that when the Roboflow API settings window becomes visible,
         * the saved configuration values are loaded into the input text boxes
         * 
         * When users open the Roboflow API settings window to configure rune detection
         * for the rune solver, the system must display the currently saved tags in
         * the text boxes. This includes the workspace name, workspace ID, API key,
         * array settings, detected coordinates, and the arrow key mappings (left,
         * up, right, down) used for solving rune puzzles.
         */
        private void _testShowingRoboflowWindowPopulatesTextBoxes()
        {
            foreach (var visible in new[] { true, false })
            {
                var roboflowAPILoadActionHandler = _fixture();
                _systemWindow.VisibleReturn.Add(visible);
                roboflowAPILoadActionHandler.OnDependencyEvent(
                    new object(),
                    new DependencyPropertyChangedEventArgs()
                );
                Debug.Assert(_workspaceNameTextBox.Text == (visible ? "12" : ""));
                Debug.Assert(_workspaceIDTextBox.Text == (visible ? "23" : ""));
                Debug.Assert(_apiKeyTextBox.Text == (visible ? "34" : ""));
                Debug.Assert(_arrayTextBox.Text == (visible ? "45" : ""));
                Debug.Assert(_xTextBox.Text == (visible ? "56" : ""));
                Debug.Assert(_yTextBox.Text == (visible ? "67" : ""));
                Debug.Assert(_leftArrowTextBox.Text == (visible ? "78" : ""));
                Debug.Assert(_upArrowTextBox.Text == (visible ? "89" : ""));
                Debug.Assert(_rightArrowTextBox.Text == (visible ? "90" : ""));
                Debug.Assert(_downArrowTextBox.Text == (visible ? "01" : ""));
            }
        }

        public void Run()
        {
            _testShowingRoboflowWindowPopulatesTextBoxes();
        }
    }


    public class WindowRuneSolverRoboflowAPISaveActionHandlerTests
    {
        private MockSystemWindow _systemWindow = new MockSystemWindow();

        private TextBox _workspaceNameTextBox = new TextBox();

        private TextBox _workspaceIDTextBox = new TextBox();

        private TextBox _apiKeyTextBox = new TextBox();

        private TextBox _arrayTextBox = new TextBox();

        private TextBox _xTextBox = new TextBox();

        private TextBox _yTextBox = new TextBox();

        private TextBox _leftArrowTextBox = new TextBox();

        private TextBox _upArrowTextBox = new TextBox();

        private TextBox _rightArrowTextBox = new TextBox();

        private TextBox _downArrowTextBox = new TextBox();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        public AbstractWindowActionHandler _fixture()
        {
            _systemWindow = new MockSystemWindow();
            _systemWindow.GetWindowReturn.Add(new Window());
            _workspaceNameTextBox = new TextBox { Text = "12" };
            _workspaceIDTextBox = new TextBox { Text = "23" };
            _apiKeyTextBox = new TextBox { Text = "34" };
            _arrayTextBox = new TextBox { Text = "45" };
            _xTextBox = new TextBox { Text = "56" };
            _yTextBox = new TextBox { Text = "67" };
            _leftArrowTextBox = new TextBox { Text = "78" };
            _upArrowTextBox = new TextBox { Text = "89" };
            _rightArrowTextBox = new TextBox { Text = "90" };
            _downArrowTextBox = new TextBox { Text = "01" };
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration();
            var handler = new WindowRuneSolverRoboflowAPISaveActionHandlerFacade(
                _systemWindow,
                _workspaceNameTextBox,
                _workspaceIDTextBox,
                _apiKeyTextBox,
                _arrayTextBox,
                _xTextBox,
                _yTextBox,
                _leftArrowTextBox,
                _upArrowTextBox,
                _rightArrowTextBox,
                _downArrowTextBox
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Verifies that when the Roboflow API settings window is closed or hidden,
         * the text box values are saved to the configuration
         * 
         * When users modify their Roboflow API settings (workspace name, API key,
         * arrow key mappings for rune puzzles, etc.) and then close the settings window,
         * the system must save the new values to the configuration. This ensures that
         * the rune solver will use the updated settings for future rune detection and
         * puzzle-solving operations.
         */
        private void _testHidingRoboflowWindowSavesTextBoxData()
        {
            foreach (var visible in new[] { true, false })
            {
                var roboflowAPILoadActionHandler = _fixture();
                _systemWindow.VisibleReturn.Add(visible);
                roboflowAPILoadActionHandler.OnDependencyEvent(
                    new object(),
                    new DependencyPropertyChangedEventArgs()
                );
                var runeDetection = _maplestoryBotConfiguration.RuneDetection;
                Debug.Assert(runeDetection.WorkspaceName == (!visible ? "12" : ""));
                Debug.Assert(runeDetection.WorkspaceID == (!visible ? "23" : ""));
                Debug.Assert(runeDetection.APIKey == (!visible ? "34" : ""));
                Debug.Assert(runeDetection.Array == (!visible ? "45" : ""));
                Debug.Assert(runeDetection.X == (!visible ? "56" : ""));
                Debug.Assert(runeDetection.Y == (!visible ? "67" : ""));
                Debug.Assert(runeDetection.Left == (!visible ? "78" : ""));
                Debug.Assert(runeDetection.Up == (!visible ? "89" : ""));
                Debug.Assert(runeDetection.Right == (!visible ? "90" : ""));
                Debug.Assert(runeDetection.Down == (!visible ? "01" : ""));
            }
        }

        public void Run()
        {
            _testHidingRoboflowWindowSavesTextBoxData();
        }
    }


    public class WindowRuneSolverRoboflowAPIOutputActionHandlerTests
    {
        private TextBox _arrayTextBox = new TextBox();

        private TextBox _xTextBox = new TextBox();

        private TextBox _yTextBox = new TextBox();

        private TextBox _leftArrowTextBox = new TextBox();

        private TextBox _upArrowTextBox = new TextBox();

        private TextBox _rightArrowTextBox = new TextBox();

        private TextBox _downArrowTextBox = new TextBox();

        private TextBlock _outputFormatTextBlock = new TextBlock();

        private AbstractWindowActionHandler _fixture()
        {
            _arrayTextBox = new TextBox();
            _xTextBox = new TextBox();
            _yTextBox = new TextBox();
            _leftArrowTextBox = new TextBox();
            _upArrowTextBox = new TextBox();
            _rightArrowTextBox = new TextBox();
            _downArrowTextBox = new TextBox();
            _outputFormatTextBlock = new TextBlock();
            return new WindowRuneSolverRoboflowAPIOutputActionHandlerFacade(
                _arrayTextBox,
                _xTextBox,
                _yTextBox,
                _leftArrowTextBox,
                _upArrowTextBox,
                _rightArrowTextBox,
                _downArrowTextBox,
                _outputFormatTextBlock
            );
        }

        /**
         * @brief Verifies that when users edit any of the Roboflow API format settings,
         * the output format preview updates in real-time
         * 
         * When users configure the Roboflow API settings for rune detection (such as the
         * array name, coordinate field names X/Y, and the arrow key class labels for
         * up/down/left/right), the system displays a live preview of the expected JSON
         * output format. This preview helps users understand how their rune detection
         * predictions will be formatted by the Roboflow API.
         */
        private void _testChangingConfigurationTextUpdatesOutput()
        {
            var roboflowAPIOutputActionHandler = _fixture();
            var textBoxes = new[] {
                _arrayTextBox,
                _xTextBox,
                _yTextBox,
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
                        "{\n" +
                        "  \"" + _arrayTextBox.Text + "\":\n" +
                        "  [\n" +
                        "    {\n" +
                        "      \"" + _xTextBox.Text + "\": <integer>\n" +
                        "      \"" + _yTextBox.Text + "\": <integer>\n" +
                        "      \"class\": <" +
                        "\"" + _leftArrowTextBox.Text + "\"|" +
                        "\"" + _upArrowTextBox.Text + "\"|" +
                        "\"" + _rightArrowTextBox.Text + "\"|" +
                        "\"" + _downArrowTextBox.Text + "\">\n" +
                        "    },\n" +
                        "    ...\n" +
                        "  ]\n" +
                        "}"
                    )
                );
            }
        }
        public void Run()
        {
            _testChangingConfigurationTextUpdatesOutput();
        }
    }


    public class WindowRuneSolverRoboflowAPIInjectActionHandlerTests
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
            var handler = new WindowRuneSolverRoboflowAPIInjectActionHandlerFacade(_systemWindow);
            handler.Inject(SystemInjectType.InjectAction, _injectAction);
            handler.Inject(SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration);
            return handler;
        }

        /**
         * @brief Verifies that when the Roboflow API settings window is closed or hidden,
         * the system broadcasts the updated configuration to all dependent components
         * and triggers a configuration save
         * 
         * When users finish editing their Roboflow API settings (workspace name, API key,
         * arrow key mappings for rune puzzles, etc.) and close the settings window, the
         * system must inject the updated configuration into all components that rely on
         * rune detection settings (such as the rune solver executor and the Roboflow API
         * client).
         */
        private void _testHidingRoboflowWindowInjectsConfigurationData()
        {
            foreach (var visible in new[] { true, false })
            {
                var dataType = new List<object>();
                var data = new List<object>();
                var roboflowAPIInjectActionHandler = _fixture();
                _systemWindow.VisibleReturn.Add(visible);
                _injectAction.GetActionReturn.Add(
                    (_, __) => { dataType.Add(_); data.Add(__); }
                );
                roboflowAPIInjectActionHandler.OnDependencyEvent(
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
            _testHidingRoboflowWindowInjectsConfigurationData();
        }
    }


    public class WindowRuneSolverEditorHandlersTestSuite
    {
        public void Run()
        {
            new WindowRuneSolverRoboflowAPILoadActionHandlerTests().Run();
            new WindowRuneSolverRoboflowAPISaveActionHandlerTests().Run();
            new WindowRuneSolverRoboflowAPIInjectActionHandlerTests().Run();
            new WindowRuneSolverRoboflowAPIOutputActionHandlerTests().Run();
        }
    }
}
