using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Controls;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    /**
     * @class WindowSaveMenuActionHandlerTests
     * 
     * @brief Unit tests for macro save menu functionality and file serialization
     * 
     * This test class validates that the save menu properly handles macro data serialization,
     * file dialog interactions, and configuration integration. Ensures users can reliably
     * save their custom macro configurations to disk with proper formatting and directory
     * management for later retrieval and use.
     */
    public class WindowSaveMenuActionHandlerTests
    {
        private Button _saveButton;

        private ListBox _listBox;

        private AbstractMacroDataSerializer _serializer;

        private AbstractWindowStateModifier _windowSaveMenuModifier;

        private MockSaveFileDialog _saveFileDialog;

        /**
         * @brief Initializes test environment with UI components and dependencies
         * 
         * Sets up the basic test environment with button, list box, serializer,
         * and file dialog components needed for testing save menu functionality.
         */
        public WindowSaveMenuActionHandlerTests()
        {
            _saveButton = new Button();
            _listBox = new ListBox();
            _serializer = new MacroDataSerializer();
            _saveFileDialog = new MockSaveFileDialog();
            _windowSaveMenuModifier = new WindowSaveMenuModifier(_saveFileDialog);
        }

        /**
         * @brief Creates complete test environment with sample macro data
         * 
         * @return Configured WindowSaveMenuActionHandler instance ready for testing
         * 
         * Prepares a realistic test scenario with a save button, list box containing
         * sample macro commands (A, B, C), and all necessary serialization components
         * to verify complete save menu functionality.
         */
        private AbstractWindowActionHandler _fixture()
        {
            _saveButton = new Button();
            _listBox = new ListBox();
            _listBox.Items.Add(new ComboBox { Text = "A" });
            _listBox.Items.Add(new ComboBox { Text = "B" });
            _listBox.Items.Add(new ComboBox { Text = "C" });
            _serializer = new MacroDataSerializer();
            _saveFileDialog = new MockSaveFileDialog();
            _windowSaveMenuModifier = new WindowSaveMenuModifier(_saveFileDialog);
            return new WindowSaveMenuActionHandler(
                _saveButton,
                _listBox,
                _serializer,
                _windowSaveMenuModifier
            );
        }

        /**
         * @brief Normalizes JSON strings for consistent comparison
         * 
         * @param json JSON string to normalize
         * 
         * @return Normalized JSON string with consistent formatting
         * 
         * This helper ensures that JSON comparisons focus on content rather than
         * formatting differences, providing reliable test results regardless of
         * whitespace or indentation variations.
         */
        private string _normalize(string json)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IndentCharacter = ' ',
                IndentSize = 0
            };
            var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, options);
        }

        /**
         * @brief Tests complete save menu workflow from button click to file dialog
         * 
         * @test Validates the entire macro saving process
         * 
         * Verifies that clicking the save button triggers the file dialog with
         * correct macro data serialization and proper directory configuration.
         * Ensures users can save their macro sequences with the expected format
         * and in the configured save location.
         */
        private void _testSaveButtonClickOpensSaveFileDialog()
        {
            var handler = _fixture();
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                new MaplestoryBotConfiguration { MacroDirectory = "MEOW" }
            );
            _saveButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_saveFileDialog.PromptCalls == 1);
            var saveContent = _normalize(_saveFileDialog.PromptCallArg_saveContent[0]);
            var initialDirectory = _saveFileDialog.PromptCallArg_initialDirectory[0];
            Debug.Assert(initialDirectory == "MEOW");
            Debug.Assert(saveContent == _normalize("{\"macro\":[\"A\",\"B\",\"C\"]}"));
        }

        /**
         * @brief Executes all save menu functionality tests
         * 
         * Runs the complete test suite to ensure the save menu works correctly,
         * providing confidence that users can reliably save their macro configurations
         * without data loss or formatting issues.
         */
        public void Run()
        {
            _testSaveButtonClickOpensSaveFileDialog();
        }
    }


    /**
     * @class WindowLoadMenuActionHandlerTests
     * 
     * @brief Unit tests for macro load menu functionality
     * 
     * This test class validates that the load menu properly handles macro data deserialization
     * and file dialog interactions, ensuring users can reliably load previously saved macro
     * configurations from disk and have them properly integrated into the UI.
     */
    public class WindowLoadMenuActionHandlerTests
    {
        private Button _saveButton;

        private ListBox _listBox;

        private ComboBox _comboBox;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        private List<string> _expectedContents;

        private AbstractMacroDataDeserializer _macroDataDeserializer;

        private AbstractWindowStateModifier _windowLoadMenuModifier;

        private MockLoadFileDialog _loadFileDialog;

        /**
         * @brief Initializes test environment with UI components and dependencies
         * 
         * Sets up the basic test environment with button, list box, predefined combo options,
         * deserializer, and file dialog components needed for testing load menu functionality.
         */
        public WindowLoadMenuActionHandlerTests()
        {
            _saveButton = new Button();
            _listBox = new ListBox();
            _comboBox = new ComboBox();
            _comboBoxPopupScaleRegistry = new MockWindowActionHandlerRegistry();
            _macroDataDeserializer = new MacroDataDeserializer();
            _loadFileDialog = new MockLoadFileDialog();
            _loadFileDialog.PromptReturn = [];
            _expectedContents = ["D", "E", "F"];
            _windowLoadMenuModifier = new WindowLoadMenuModifier(_loadFileDialog);
        }

        /**
         * @brief Creates complete test environment with sample macro data and file dialog response
         * 
         * @return Configured WindowLoadMenuActionHandler instance ready for testing
         * 
         * Prepares a realistic test scenario with a load button, list box, predefined command options,
         * and a mock file dialog that returns sample macro data (D, E, F) to verify complete
         * load menu functionality including deserialization and UI integration.
         */
        private WindowLoadMenuActionHandler _fixture(
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _saveButton = new Button();
            _listBox = new ListBox();
            _comboBox = new ComboBox();
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            _comboBox.Items.Add(new ComboBoxItem { Content = "A" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "B" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "C" });
            _macroDataDeserializer = new MacroDataDeserializer();
            _loadFileDialog = new MockLoadFileDialog();
            _loadFileDialog.PromptReturn.Add("{\"macro\":[\"D\",\"E\",\"F\"]}");
            _expectedContents = ["D", "E", "F"];
            _windowLoadMenuModifier = new WindowLoadMenuModifier(_loadFileDialog);
            var handler = new WindowLoadMenuActionHandler(
                _saveButton,
                _listBox,
                _comboBoxPopupScaleRegistry,
                new ComboBoxTemplateFactory(_comboBox),
                _macroDataDeserializer,
                _windowLoadMenuModifier
            );
            handler.Inject(
                SystemInjectType.ConfigurationUpdate, new MaplestoryBotConfiguration { MacroDirectory = "MEOW" }
            );
            return handler;
        }

        /**
         * @brief Tests complete load menu workflow from user perspective
         * 
         * Validates that users can load saved macros and have them
         * available with all commands properly displayed, ensuring
         * a seamless transition from storage to active use.
         */
        private void _testLoadButtonClickOpensLoadFileDialog()
        {
            var handler = _fixture(new WindowComboBoxScaleActionHandlerRegistry());
            _saveButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_loadFileDialog.PromptCalls == 1);
            Debug.Assert(_loadFileDialog.PromptCallArg_initialDirectory[0] == "MEOW");
            Debug.Assert(_listBox.Items.Count == 3);
            for (int i = 0; i < _expectedContents.Count; i++)
            {
                Debug.Assert(((ComboBox)_listBox.Items[i]).Text == _expectedContents[i]);
                for (int j = 0; j < _comboBox.Items.Count; j++)
                {
                    Debug.Assert(
                        ((ComboBoxItem)((ComboBox)_listBox.Items[i]).Items[j]).Content.ToString()
                        == ((ComboBoxItem)_comboBox.Items[j]).Content.ToString()
                    );
                }
            }
        }

        /**
         * @brief Tests that ComboBox scaling handlers are properly registered for loaded macros
         * 
         * Validates that when macros are loaded into the UI, each ComboBox instance
         * automatically registers with the scaling system to ensure proper DPI
         * handling, maintaining consistent visual appearance across different displays.
         */
        private void _testLoadButtonClickRegistersComboBoxPopupScalers()
        {
            var mockRegistry = new MockWindowActionHandlerRegistry();
            var handler = _fixture(mockRegistry);
            _saveButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(mockRegistry.RegisterHandlerCalls == 3);
            for (int i = 0; i < _expectedContents.Count; i++)
            {
                var parameters = (WindowComboBoxScaleActionHandlerParameters)mockRegistry.RegisterHandlerCallArg_args[i]!;
                Debug.Assert(parameters.ScaleComboBox == (ComboBox)_listBox.Items[i]);
            }    
        }


        /**
         * @brief Tests proper cleanup and registration order for scaling handlers
         * 
         * Ensures that when loading new macros, existing scaling handlers are
         * cleared before registering new ones, preventing memory leaks and
         * ensuring only current macro ComboBoxes receive scaling adjustments.
         */
        private void _testLoadButtonClickClearsComboBoxPopupScalersBefroreRegisteringNew()
        {
            var mockRegistry = new MockWindowActionHandlerRegistry();
            var handler = _fixture(mockRegistry);
            _saveButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            var reference = new TestUtilities().Reference(mockRegistry);
            var clearCallRef = reference + "ClearHandlers";
            var registerCallRef = reference + "RegisterHandler";
            Debug.Assert(mockRegistry.CallOrder.Count == 4);
            Debug.Assert(mockRegistry.CallOrder[0] == clearCallRef);
            Debug.Assert(mockRegistry.CallOrder[1] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[2] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[3] == registerCallRef);
        }

        /**
         * @brief Executes all load menu functionality tests
         * 
         * Runs the complete test suite to ensure the load menu works correctly,
         * providing confidence that users can reliably load their saved macro
         * configurations and have them properly integrated into the application UI.
         */
        public void Run()
        {
            _testLoadButtonClickOpensLoadFileDialog();
            _testLoadButtonClickClearsComboBoxPopupScalersBefroreRegisteringNew();
            _testLoadButtonClickRegistersComboBoxPopupScalers();
        }
    }


    /**
     * @class WindowAddMacroCommandActionHandlerTests
     * 
     * @brief Unit tests for macro command addition functionality
     * 
     * This test class validates that the add macro command feature properly handles
     * UI interactions, list management, and ComboBox scaling registration. Ensures
     * that users can reliably add new macro commands to their automation sequences
     * with proper positioning and visual consistency.
     */
    public class WindowAddMacroCommandActionHandlerTests
    {
        private Button _addButton;

        private ListBox _listBox;

        private ComboBox _comboBox;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        /**
         * @brief Initializes test environment with UI components and dependencies
         * 
         * Sets up the basic test environment with button, list box, ComboBox template,
         * and scaling registry components needed for testing macro command addition functionality.
         */
        public WindowAddMacroCommandActionHandlerTests()
        {
            _addButton = new Button();
            _listBox = new ListBox();
            _comboBox = new ComboBox();
            _comboBoxPopupScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
        }

        /**
         * @brief Creates test environment with specified scaling registry
         * 
         * @param comboBoxPopupScaleRegistry The registry instance for testing scaling handler registration
         * 
         * @return Configured WindowAddMacroCommandActionHandler instance ready for testing
         * 
         * Prepares a complete test scenario with sample command options (A, B, C),
         * UI components, and the specified scaling registry to verify macro command
         * addition behavior under different registry configurations.
         */
        private AbstractWindowActionHandler _fixture(
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _addButton = new Button();
            _listBox = new ListBox();
            _comboBox = new ComboBox();
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            _comboBox.Items.Add(new ComboBoxItem { Content = "A" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "B" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "C" });
            return new WindowAddMacroCommandActionHandler(
                _addButton,
                new WindowAddMacroCommandModifier(
                    _listBox,
                    _comboBoxPopupScaleRegistry,
                    new ComboBoxTemplateFactory(_comboBox)
                )
            );
        }

        /**
         * @brief Tests basic macro command addition to empty list
         * 
         * @test Validates that clicking add button inserts new command with template options
         * 
         * Verifies that when users click the add button with an empty command list,
         * a new ComboBox is added containing all available command options from the
         * template, ensuring users can start building macro sequences from scratch.
         */
        private void _testAddButtonClickAddsNewCommandToListBox()
        {
            var handler = _fixture(new WindowComboBoxScaleActionHandlerRegistry());
            _addButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_listBox.Items.Count == 1);
            Debug.Assert(_listBox.Items[0] is ComboBox);
            Debug.Assert(((ComboBox)_listBox.Items[0]).Items.Count == 3);
            Debug.Assert(((ComboBoxItem)((ComboBox)_listBox.Items[0]).Items[0]).Content.ToString() == "A");
            Debug.Assert(((ComboBoxItem)((ComboBox)_listBox.Items[0]).Items[1]).Content.ToString() == "B");
            Debug.Assert(((ComboBoxItem)((ComboBox)_listBox.Items[0]).Items[2]).Content.ToString() == "C");
        }

        /**
         * @brief Tests intelligent command insertion relative to selection
         * 
         * @test Validates that new commands are inserted at correct position based on user selection
         * 
         * Verifies that when users have an existing command sequence and select a specific
         * position, new commands are inserted immediately after the selected item rather
         * than at the end, providing precise control over macro sequence construction.
         */
        private void _testAddButtonClickAddsNewCommandBelowSelectedListBoxItem()
        {
            var selectedComboBox = new ComboBox();
            var handler = _fixture(new WindowComboBoxScaleActionHandlerRegistry());
            _listBox.Items.Add(new ComboBox());
            _listBox.Items.Add(selectedComboBox);
            _listBox.Items.Add(new ComboBox());
            _listBox.Items.Add(new ComboBox());
            _listBox.SelectedItem = selectedComboBox;
            _addButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_listBox.Items.Count == 5);
            Debug.Assert(_listBox.Items[2] is ComboBox);
            Debug.Assert(((ComboBox)_listBox.Items[2]).Items.Count == 3);
            Debug.Assert(((ComboBoxItem)((ComboBox)_listBox.Items[2]).Items[0]).Content.ToString() == "A");
            Debug.Assert(((ComboBoxItem)((ComboBox)_listBox.Items[2]).Items[1]).Content.ToString() == "B");
            Debug.Assert(((ComboBoxItem)((ComboBox)_listBox.Items[2]).Items[2]).Content.ToString() == "C");
        }

        /**
         * @brief Tests automatic scaling handler registration for new commands
         * 
         * @test Validates that new macro commands automatically register for proper UI scaling
         * 
         * Verifies that when a new macro command ComboBox is added to the list,
         * it automatically registers with the scaling system to ensure consistent
         * dropdown appearance and behavior across different DPI settings and window scaling.
         */
        private void _testAddButtonClickRegistersComboBoxPopupScaler()
        {
            var registry = new MockWindowActionHandlerRegistry();
            var handler = _fixture(registry);
            _addButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(registry.RegisterHandlerCalls == 1);
            Debug.Assert(
                (
                    (WindowComboBoxScaleActionHandlerParameters)
                    registry.RegisterHandlerCallArg_args[0]!
                ).ScaleComboBox == _listBox.Items[0]
            );
        }

        /**
         * @brief Executes all macro command addition functionality tests
         * 
         * Runs the complete test suite to ensure the add macro command feature
         * works correctly, providing confidence that users can reliably build
         * and modify their automation sequences with proper UI consistency.
         */
        public void Run()
        {
            _testAddButtonClickAddsNewCommandToListBox();
            _testAddButtonClickAddsNewCommandBelowSelectedListBoxItem();
            _testAddButtonClickRegistersComboBoxPopupScaler();
        }
    }


    /**
     * @class WindowRemoveMacroCommandActionHandlerTests
     * 
     * @brief Unit tests for macro command removal functionality
     * 
     * This test class validates that the remove macro command feature properly handles
     * UI interactions, list management cleanup, and ComboBox scaling deregistration.
     * Ensures that users can reliably remove unwanted macro commands from their automation
     * sequences while maintaining system integrity and cleaning up associated resources.
     */
    public class WindowRemoveMacroCommandActionHandlerTests
    {
        private Button _removeButton;

        private ListBox _listBox;

        private ComboBox _comboBox;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        /**
         * @brief Initializes test environment with UI components and dependencies
         * 
         * Sets up the basic test environment with remove button, list box, ComboBox template,
         * and scaling registry components needed for testing macro command removal functionality.
         */
        public WindowRemoveMacroCommandActionHandlerTests()
        {
            _removeButton = new Button();
            _listBox = new ListBox();
            _comboBox = new ComboBox();
            _comboBoxPopupScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
        }

        /**
         * @brief Creates test environment with specified scaling registry
         * 
         * @param comboBoxPopupScaleRegistry The registry instance for testing scaling handler cleanup
         * 
         * @return Configured WindowRemoveMacroCommandActionHandler instance ready for testing
         * 
         * Prepares a complete test scenario with sample command options reference,
         * UI components, and the specified scaling registry to verify macro command
         * removal behavior and associated resource cleanup.
         */
        private AbstractWindowActionHandler _fixture(
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _removeButton = new Button();
            _listBox = new ListBox();
            _comboBox = new ComboBox();
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            _comboBox.Items.Add(new ComboBoxItem { Content = "A" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "B" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "C" });
            return new WindowRemoveMacroCommandActionHandler(
                _removeButton,
                new WindowRemoveMacroCommandModifier(
                    _listBox,
                    _comboBoxPopupScaleRegistry
                )
            );
        }

        /**
         * @brief Tests basic macro command removal from list
         * 
         * @test Validates that clicking remove button deletes commands from the list
         * 
         * Verifies that when users click the remove button with commands in the list,
         * the system properly removes macro commands, ensuring users can clean up
         * unwanted or incorrect commands from their automation sequences.
         */
        private void _testAddButtonClickRemovesCommandFromListBox()
        {
            var handler = _fixture(new WindowComboBoxScaleActionHandlerRegistry());
            var comboBox = new ComboBox();
            _listBox.Items.Add(comboBox);
            _removeButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_listBox.Items.Count == 0);
        }

        /**
         * @brief Tests selective command removal based on user selection
         * 
         * @test Validates that only the selected command is removed while others remain
         * 
         * Verifies that when users select a specific command in the sequence and click remove,
         * only that command is deleted while other commands maintain their positions,
         * providing precise control over macro sequence editing without disrupting the entire flow.
         */
        private void _testRemoveButtonClickRemovesSelectedListBoxItem()
        {
            var selectedComboBox = new ComboBox();
            var handler = _fixture(new WindowComboBoxScaleActionHandlerRegistry());
            _listBox.Items.Add(new ComboBox());
            _listBox.Items.Add(selectedComboBox);
            _listBox.Items.Add(new ComboBox());
            _listBox.Items.Add(new ComboBox());
            _listBox.SelectedItem = selectedComboBox;
            _removeButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_listBox.Items.Count == 3);
            Debug.Assert(_listBox.SelectedItem == null);
            Debug.Assert(_listBox.Items.IndexOf(selectedComboBox) == -1);
        }

        /**
         * @brief Tests proper cleanup of scaling handlers during removal
         * 
         * @test Validates that removed commands deregister from scaling system
         * 
         * Verifies that when a macro command ComboBox is removed from the list,
         * it automatically unregisters from the scaling system to prevent memory leaks
         * and ensure that system resources are properly cleaned up when no longer needed.
         */
        private void _testRemoveButtonClickUnregistersComboBoxPopupScaler()
        {
            var comboBox = new ComboBox();
            var handler = _fixture(new WindowComboBoxScaleActionHandlerRegistry());
            _listBox.Items.Add(comboBox);
            _comboBoxPopupScaleRegistry.RegisterHandler(comboBox);
            _removeButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_listBox.Items.Count == 0);
            Debug.Assert(_comboBoxPopupScaleRegistry.GetHandlers().Count == 0);
        }

        /**
         * @brief Executes all macro command removal functionality tests
         * 
         * Runs the complete test suite to ensure the remove macro command feature
         * works correctly, providing confidence that users can reliably edit and
         * refine their automation sequences while maintaining system stability.
         */
        public void Run()
        {
            _testAddButtonClickRemovesCommandFromListBox();
            _testRemoveButtonClickRemovesSelectedListBoxItem();
            _testRemoveButtonClickUnregistersComboBoxPopupScaler();
        }
    }


    public class WindowSaveLoadMenuHandlersTestSuite
    {
        public void Run()
        {
            new WindowSaveMenuActionHandlerTests().Run();
            new WindowLoadMenuActionHandlerTests().Run();
            new WindowAddMacroCommandActionHandlerTests().Run();
            new WindowRemoveMacroCommandActionHandlerTests().Run();
        }
    }
}
