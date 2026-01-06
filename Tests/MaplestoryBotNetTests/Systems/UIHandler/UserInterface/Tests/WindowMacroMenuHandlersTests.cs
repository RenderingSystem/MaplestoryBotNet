using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.Systems.Configuration.Tests;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


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
            var normalizer = new JsonNormalizer();
            var saveContent = normalizer.Normalize(_saveFileDialog.PromptCallArg_saveContent[0]);
            var initialDirectory = _saveFileDialog.PromptCallArg_initialDirectory[0];
            Debug.Assert(initialDirectory == "MEOW");
            Debug.Assert(saveContent == normalizer.Normalize("{\"macro\":[\"A\",\"B\",\"C\"]}"));
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
     * @brief Unit tests for verifying proper file loading functionality in the application's load menu
     * 
     * @test
     * Validates that the load menu correctly opens file dialogs and respects configuration settings,
     * ensuring users can reliably access and load their saved automation configurations.
     * 
     * @details
     * These tests validate critical user workflow functionality where users need to load previously
     * saved automation macro files. The test suite ensures that the load menu provides a reliable
     * and predictable experience, preventing data loss and configuration errors during the automation
     * setup process.
     */
    public class WindowLoadMenuActionHandlerTests
    {
        private Button _loadButton;

        private MockLoadFileDialog _loadFileDialog;

        /**
         * @brief Initializes test environment for load menu functionality testing
         * 
         * @test
         * Sets up the required UI and mock components to simulate the load menu
         * functionality in the application's user interface.
         * 
         * @details
         * This isolated setup ensures each test runs with clean state, allowing
         * precise validation of the load functionality without interference from
         * previous tests or external system dependencies.
         */
        public WindowLoadMenuActionHandlerTests()
        {
            _loadButton = new Button();
            _loadFileDialog = new MockLoadFileDialog();
        }

        /**
         * @brief Creates a fresh test fixture for isolated testing
         * 
         * @test
         * Reinitializes all test dependencies to ensure clean state for each test.
         * 
         * @details
         * By recreating all components for each test, this method ensures complete
         * isolation between test executions, preventing any state leakage that could
         * affect test reliability or produce false results.
         * 
         * @return A freshly configured load menu handler ready for dependency injection
         * and testing
         */
        private AbstractWindowActionHandler _fixture()
        {
            _loadButton = new Button();
            _loadFileDialog = new MockLoadFileDialog();
            return new WindowLoadMenuActionHandlerFacade(_loadButton, _loadFileDialog);
        }

        /**
         * @brief Tests that clicking the load button properly opens the file dialog
         * 
         * @test
         * Validates the complete load workflow from button click to dialog presentation.
         * 
         * @details
         * Successful execution ensures users can reliably access their saved
         * configurations and automation scripts from the correct directory location,
         * enabling efficient workflow and configuration management.
         */
        private void _testLoadButtonOpensDialog()
        {
            var handler = _fixture();
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                new MaplestoryBotConfiguration { MacroDirectory = "cool_macros" }
            );
            _loadButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_loadFileDialog.PromptCalls == 1);
            Debug.Assert(_loadFileDialog.PromptCallArg_initialDirectory[0] == "cool_macros");
        }

        /**
         * @brief Tests that load operations fail gracefully without configuration
         * 
         * @test
         * Validates that the load button does nothing when configuration is missing.
         * 
         * @details
         * This test ensures that if a user tries to load files without proper
         * configuration (specifically the directory path), the system gracefully
         * ignores the request rather than crashing or opening dialogs at undefined
         * locations. This prevents user confusion and ensures load operations only
         * occur when the system knows where to look for files.
         */
        private void _testLoadButtonDoesntOpenDialogWhenConfigurationNotInjected()
        {
            var handler = _fixture();
            _loadButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_loadFileDialog.PromptCalls == 0);
        }


        /**
         * @brief Executes the complete load menu functionality test suite
         * 
         * @test
         * Runs all tests to validate the entire file loading pipeline.
         * 
         * @details
         * This ensures users can efficiently manage and load their automation
         * configurations across multiple gaming sessions.
         */
        public void Run()
        {
            _testLoadButtonOpensDialog();
            _testLoadButtonDoesntOpenDialogWhenConfigurationNotInjected();
        }
    }


    /**
     * @class WindowLoadMenuElementActionHandlerTests
     * 
     * @brief Unit tests for verifying proper macro loading and UI integration in the
     * application's load menu
     * 
     * @test
     * The tests verify the interaction between file loading, UI updates, and scaling
     * systems, ensuring all components work together seamlessly during the macro
     * loading process.
     */
    public class WindowLoadMenuElementActionHandlerTests
    {
        private ListBox _listBox;

        private ComboBox _comboBox;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        private List<string> _expectedContents;

        private AbstractMacroDataDeserializer _macroDataDeserializer;

        private AbstractWindowStateModifier _windowLoadMenuElementModifier;

        private MockLoadFileDialog _loadFileDialog;

        /**
         * @brief Initializes test environment for macro loading functionality testing
         * 
         * @test
         * Sets up the required UI components, mock dialogs, and data structures to
         * simulate the complete macro loading and UI integration workflow.
         * 
         * @details
         * This setup ensures each test starts with clean, isolated components,
         * allowing precise validation of the macro loading process without
         * interference from previous test executions or external dependencies.
         */
        public WindowLoadMenuElementActionHandlerTests()
        {
            _listBox = new ListBox();
            _comboBox = new ComboBox();
            _comboBoxPopupScaleRegistry = new MockWindowActionHandlerRegistry();
            _expectedContents = [];
            _macroDataDeserializer = new MacroDataDeserializer();
            _loadFileDialog = new MockLoadFileDialog();
            _windowLoadMenuElementModifier = new MockWindowStateModifier();
        }

        /**
         * @brief Creates a fresh test fixture with customized registry
         * 
         * @test
         * Builds a complete test environment with specified handler registry
         * for testing different scaling registration scenarios.
         * 
         * @details
         * The ComboBox is populated with sample items ("A", "B", "C") to simulate
         * real UI state. The handler registry parameter allows testing both
         * functional and mock registries for different test scenarios.
         * 
         * @param comboBoxPopupScaleRegistry The handler registry to use for
         *        tracking scaling registrations during tests
         *        
         * @return AbstractWindowActionHandler A fully configured action handler
         *         ready for file loading simulation and validation
         */
        private AbstractWindowActionHandler _fixture(
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _listBox = new ListBox();
            _comboBox = new ComboBox();
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            _comboBox.Items.Add(new ComboBoxItem { Content = "A" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "B" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "C" });
            _macroDataDeserializer = new MacroDataDeserializer();
            _expectedContents = ["a", "b", "c", "d", "e"];
            _windowLoadMenuElementModifier = new WindowLoadMenuElementModifier(
                _listBox,
                new ComboBoxTemplateFactory(_comboBox),
                _comboBoxPopupScaleRegistry,
                _macroDataDeserializer
            );
            return new WindowLoadMenuElementActionHandler(
                _loadFileDialog,
                _windowLoadMenuElementModifier
            );
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
            var registry = new WindowComboBoxScaleActionHandlerRegistry();
            var handler = _fixture(registry);
            _loadFileDialog.InvokeFileLoaded(
                "some_path",
                "{\"macro\": [\"a\", \"b\", \"c\", \"d\", \"e\"]}"
            );
            Debug.Assert(_listBox.Items.Count == 5);
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
            _loadFileDialog.InvokeFileLoaded(
                "some_path",
                "{\"macro\": [\"a\", \"b\", \"c\", \"d\", \"e\"]}"
            );
            Debug.Assert(mockRegistry.RegisterHandlerCalls == 5);
            for (int i = 0; i < _expectedContents.Count; i++)
            {
                var parameters = (
                    (WindowComboBoxScaleActionHandlerParameters)
                    mockRegistry.RegisterHandlerCallArg_args[i]!
                );
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
            _loadFileDialog.InvokeFileLoaded(
                "some_path",
                "{\"macro\": [\"a\", \"b\", \"c\", \"d\", \"e\"]}"
            );
            var reference = new TestUtilities().Reference(mockRegistry);
            var clearCallRef = reference + "ClearHandlers";
            var registerCallRef = reference + "RegisterHandler";
            Debug.Assert(mockRegistry.CallOrder.Count == 6);
            Debug.Assert(mockRegistry.CallOrder[0] == clearCallRef);
            Debug.Assert(mockRegistry.CallOrder[1] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[2] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[3] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[4] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[5] == registerCallRef);
        }


        /**
         * @brief Executes the complete macro loading test suite
         * 
         * @test
         * Runs all macro loading tests to validate the entire file-to-UI pipeline.
         * 
         * @details
         * This test sequence validates the complete macro loading process:
         * 1. Basic macro loading and UI display functionality
         * 2. Proper scaling registration for DPI consistency
         * 3. Clean resource management during macro reloading
         * 
         * This ensures reliable macro management for complex automation setups.
         */
        public void Run()
        {
            _testLoadButtonClickOpensLoadFileDialog();
            _testLoadButtonClickRegistersComboBoxPopupScalers();
            _testLoadButtonClickClearsComboBoxPopupScalersBefroreRegisteringNew();
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
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
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


    /**
     * @class WindowClearMacroCommandActionHandlerTests
     * 
     * @brief Unit tests for bulk macro command clearance functionality
     * 
     * This test class validates that the clear all macro commands feature properly handles
     * complete sequence reset, bulk resource cleanup, and scaling handler deregistration.
     * Ensures that users can reliably reset their entire automation sequence with a single
     * action while maintaining system integrity and cleaning up all associated resources.
     */
    public class WindowClearMacroCommandActionHandlerTests
    {
        private Button _clearButton;

        private ListBox _listBox;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        /**
         * @brief Initializes test environment with UI components and dependencies
         * 
         * Sets up the basic test environment with clear button, list box, and scaling registry
         * components needed for testing bulk macro command clearance functionality.
         */
        public WindowClearMacroCommandActionHandlerTests()
        {
            _clearButton = new Button();
            _listBox = new ListBox();
            _comboBoxPopupScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
        }

        /**
         * @brief Creates test environment with specified scaling registry
         * 
         * @param comboBoxPopupScaleRegistry The registry instance for testing bulk scaling handler cleanup
         * 
         * @return Configured WindowClearMacroCommandsActionHandler instance ready for testing
         * 
         * Prepares a complete test scenario with UI components and the specified scaling registry
         * to verify bulk macro command clearance behavior and comprehensive resource cleanup.
         */
        private AbstractWindowActionHandler _fixture(
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _clearButton = new Button();
            _listBox = new ListBox();
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            return new WindowClearMacroCommandsActionHandler(
                _clearButton,
                new WindowClearMacroCommandsModifier(
                    _listBox,
                    _comboBoxPopupScaleRegistry
                )
            );
        }

        /**
         * @brief Tests complete macro sequence clearance
         * 
         * @test Validates that clicking clear button removes all commands from the list
         * 
         * Verifies that when users click the clear button with multiple commands in the list,
         * the system properly removes all macro commands simultaneously, ensuring users can
         * quickly reset their automation sequences and start fresh without manual deletion.
         */
        private void _testClearButtonClickRemovesAllCommandsFromListBox()
        {
            var handler = _fixture(new WindowComboBoxScaleActionHandlerRegistry());
            for (int i = 0; i < 4; i++)
            {
                _listBox.Items.Add(new ComboBox());
            }
            _clearButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_listBox.Items.Count == 0);
        }

        /**
         * @brief Tests comprehensive scaling handler cleanup during bulk clearance
         * 
         * @test Validates that all removed commands deregister from scaling system
         * 
         * Verifies that when multiple macro command ComboBoxes are cleared from the list,
         * they all automatically unregister from the scaling system to prevent memory leaks
         * and ensure that all system resources are properly cleaned up during bulk operations.
         */
        private void _testClearButtonClickUnregistersAllComboBoxPopupScalers()
        {
            for (int i = 0; i < 4; i++)
            {
                var comboBox = new ComboBox();
                _listBox.Items.Add(comboBox);
                _comboBoxPopupScaleRegistry.RegisterHandler(comboBox);
            }
            _clearButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_comboBoxPopupScaleRegistry.GetHandlers().Count == 0);
        }


        /**
         * @brief Executes all bulk macro command clearance functionality tests
         * 
         * Runs the complete test suite to ensure the clear all macro commands feature
         * works correctly, providing confidence that users can reliably reset their
         * automation sequences while maintaining system stability and resource efficiency.
         */
        public void Run()
        {
            _testClearButtonClickRemovesAllCommandsFromListBox();
            _testClearButtonClickUnregistersAllComboBoxPopupScalers();
        }
    }


    /**
     * @class ListBoxElementAdder
     * 
     * @brief Helper utility for synchronizing macro command data between UI ListBox and
     * MinimapPoint data model.
     * 
     * This utility class provides a unified method for adding test macro commands that ensures
     * both the visual representation in a ListBox and the underlying data model (MinimapPoint)
     * are updated simultaneously. It's designed specifically for test scenarios to maintain
     * consistency between UI state and data state during test execution.
     */
    public class ListBoxElementAdder
    {
        private ListBox _macroLabelsListBox;

        private MinimapPoint _minimapPoint;

        /**
         * @brief Constructs a ListBoxElementAdder with specified UI and data targets.
         * 
         * Initializes the adder with the UI component and data model that will receive
         * synchronized macro command entries. Both parameters are required to ensure
         * proper test data consistency.
         * 
         * @param macroLabelsListBox The ListBox control where visual macro items will be added.
         * @param minimapPoint The MinimapPoint data structure where macro data will be stored.
         */
        public ListBoxElementAdder(
            ListBox macroLabelsListBox,
            MinimapPoint minimapPoint
        )
        {
            _macroLabelsListBox = macroLabelsListBox;
            _minimapPoint = minimapPoint;
        }

        /**
         * @brief Helper method to add a test macro command to the UI and data model.
         * 
         * Creates a visual representation of a macro command in the ListBox and
         * simultaneously adds the corresponding data structure to the minimap point.
         * This ensures both UI and data model are synchronized for testing purposes.
         * 
         * @param name Display name of the macro command
         * @param probability Probability value as a string (will be converted to integer)
         * @param commands List of command strings associated with this macro
         */
        public void Add(
            string name,
            string probability,
            List<string> commands
        )
        {
            var listBoxElement = new Grid();
            listBoxElement.Children.Add(new TextBox { Tag = "MacroNameTag", Text = name });
            listBoxElement.Children.Add(new TextBox { Tag = "ProbabilityTag", Text = probability });
            _macroLabelsListBox.Items.Add(listBoxElement);
            _minimapPoint.PointData.ElementName = "meow";
            _minimapPoint.PointData.Commands.Add(
                new MinimapPointMacros
                {
                    MacroChance = Convert.ToInt32(probability),
                    MacroName = name,
                    MacroCommands = commands
                }
            );
        }
    }


    /**
     * @class WindowMacroDisplayLoadingActionHandlerTests
     * 
     * @brief Unit tests for the macro display loading functionality.
     * 
     * This test suite validates the behavior of the macro display loading action handler,
     * which manages the display and population of macros associated with selected minimap
     * points. Tests cover various scenarios including window visibility changes, selection
     * states, and model injection requirements to ensure robust macro management.
     */
    public class WindowMacroDisplayLoadingActionHandlerTests
    {
        private Window _window;

        private MockSystemWindow _macroWindow;

        private ListBox _macroLabelsListBox;

        private TextBox _macroLabelTextBox;

        private AbstractWindowMapEditMenuState _menuState;

        private Grid _pointMacroTemplate;

        private TextBox _pointMacroTemplateMacro;

        private TextBox _pointMacroTemplateProb;

        private MapModel _mapModel;

        private FrameworkElement _selectedElement;

        private MinimapPointData _minimapPointData;

        private MinimapPoint _minimapPoint;

        /**
         * @brief Constructor initializing all test dependencies.
         * 
         * Creates fresh instances of all required test objects:
         * - UI elements (Window, ListBox, TextBox, Grid)
         * - State controllers (WindowMapEditMenuState)
         * - Data models (MapModel, MinimapPointData, MinimapPoint)
         * - Mock objects (MockSystemWindow)
         * 
         * Each test method starts from this clean state to ensure isolation.
         */
        public WindowMacroDisplayLoadingActionHandlerTests()
        {
            _window = new Window();
            _macroWindow = new MockSystemWindow();
            _macroLabelsListBox = new ListBox();
            _macroLabelTextBox = new TextBox();
            _menuState = new WindowMapEditMenuState();
            _pointMacroTemplate = new Grid();
            _pointMacroTemplateMacro = new TextBox();
            _pointMacroTemplateProb = new TextBox();
            _mapModel = new MapModel();
            _selectedElement = new FrameworkElement();
            _minimapPointData = new MinimapPointData();
            _minimapPoint = new MinimapPoint();
        }

        /**
         * @brief Creates and configures a test handler instance with dependencies.
         * 
         * Sets up a complete test environment:
         * - Configures mock window to return the test window
         * - Tags template elements for identification
         * - Creates and adds a test minimap point to the model
         * - Adds template children to the pointMacroTemplate grid
         * 
         * @return Fully configured test handler instance
         */
        public AbstractWindowActionHandler _fixture()
        {
            _window = new Window();
            _macroWindow = new MockSystemWindow();
            _macroWindow.GetWindowReturn.Add(_window);
            _macroLabelsListBox = new ListBox();
            _macroLabelTextBox = new TextBox();
            _menuState = new WindowMapEditMenuState();
            _pointMacroTemplate = new Grid();
            _pointMacroTemplateMacro = new TextBox{ Tag = "MacroNameTag" };
            _pointMacroTemplateProb = new TextBox{ Tag = "ProbabilityTag" };
            _mapModel = new MapModel();
            _selectedElement = new FrameworkElement{ Name = "meow" };
            _minimapPointData = new MinimapPointData{ ElementName = "meow", PointName = "meow point" };
            _minimapPoint = new MinimapPoint { PointData = _minimapPointData };
            _mapModel.Add(_minimapPoint);
            _pointMacroTemplate.Children.Add(_pointMacroTemplateMacro);
            _pointMacroTemplate.Children.Add(_pointMacroTemplateProb);
            return new WindowMacroDisplayLoadingActionHandlerFacade(
                _macroWindow,
                _macroLabelsListBox,
                _macroLabelTextBox,
                _pointMacroTemplate,
                _menuState
            );
        }

        /**
         * @brief Standard test setup with common configuration.
         * 
         * Prepares a handler for testing with:
         * - Injected MapModel dependency
         * - Mock window set to visible
         * - Menu state with a selected element
         * 
         * This setup represents the typical operational state for macro loading.
         * 
         * @return Handler ready for visibility event testing
         */
        private AbstractWindowActionHandler _testSetup()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _macroWindow.VisibleReturn.Add(true);
            _menuState.Select(_selectedElement);
            return handler;
        }


        /**
         * @brief Helper method to extract text from tagged child elements.
         * 
         * Searches through the visual tree of a ListBoxItem to find a TextBox
         * with a specific tag value. Used to verify proper data binding in
         * dynamically created macro display items.
         * 
         * @param macroLabelsListBoxElement Parent ListBoxItem element
         * @param elementIndex Index in the Commands array for data validation
         * @param tag String tag to identify the target TextBox
         * 
         * @return string Text content of the found TextBox, or empty string if not found
         */
        private string _getTaggedText(
            FrameworkElement macroLabelsListBoxElement,
            int elementIndex,
            string tag
        )
        {
            var minimapPointMacros = _minimapPoint.PointData.Commands[elementIndex];
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(macroLabelsListBoxElement); i++)
            {
                var child = VisualTreeHelper.GetChild(macroLabelsListBoxElement, i);
                if (child is not TextBox childTextBox)
                {
                    continue;
                }
                if (childTextBox.Tag is not string stringTag)
                {
                    continue;
                }
                if (stringTag == tag)
                {
                    return childTextBox.Text;
                }
            }
            return "";
        }

        /**
         * @brief Verifies that the selected minimap point is stored in the ListBox Tag property.
         * 
         * @test Validates that the selected minimap point is attached to the ListBox Tag.
         * 
         * When the macro window becomes visible and a point is selected,
         * the handler should attach the corresponding MinimapPoint to the
         * macro labels ListBox Tag property for future reference.
         */
        private void _testWindowBecomingVisibleSetsSelectedMinimapPointAsLabelTag()
        {
            var handler = _testSetup();
            handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            Debug.Assert(_macroLabelsListBox.Tag is MinimapPoint);
            var taggedMinimapPoint = (MinimapPoint)_macroLabelsListBox.Tag;
            Debug.Assert(taggedMinimapPoint.PointData.ElementName == "meow");
        }

        /**
         * @brief Verifies that the point name is displayed in the label TextBox.
         * 
         * @test Validates that the selected point name is displayed in the macro label TextBox.
         * 
         * When the window becomes visible, the macro label TextBox should display
         * the name of the selected minimap point for user identification.
         */
        private void _testWindowBecomingVisibleSetsTextBoxWithPointName()
        {
            var handler = _testSetup();
            handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            Debug.Assert(_macroLabelTextBox.Text == "meow point");
        }

        /**
         * @brief Verifies that the macro list is cleared before repopulation.
         * 
         * @test Validates that existing macro list items are cleared before loading new data.
         * 
         * When the window becomes visible, the handler should clear any existing
         * items from the macro labels ListBox before loading new macros to prevent
         * stale data from persisting.
         */
        private void _testWindowBecomingVisibleClearsLabelsListBox()
        {
            var handler = _testSetup();
            _macroLabelsListBox.Items.Add(new ListBoxItem());
            handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            Debug.Assert(_macroLabelsListBox.Items.Count == 0);
        }

        /**
         * @brief Verifies that macros are properly loaded into the ListBox.
         * 
         * @test Validates that all macros from the selected point are loaded.
         * 
         * When the window becomes visible and macros exist for the selected point,
         * the handler should create ListBox items for each macro with properly
         * bound macro names and probabilities.
         */
        private void _testWindowBecomingVisiblePopulatesLabelsListBoxWithCommands()
        {
            var handler = _testSetup();
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 12, MacroName = "meow 1" });
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 23, MacroName = "meow 2" });
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 34, MacroName = "meow 3" });
            handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            Debug.Assert(_macroLabelsListBox.Items.Count == 3);
            Debug.Assert(_getTaggedText((FrameworkElement)_macroLabelsListBox.Items[0], 0, "MacroNameTag") == "meow 1");
            Debug.Assert(_getTaggedText((FrameworkElement)_macroLabelsListBox.Items[0], 0, "ProbabilityTag") == "12");
            Debug.Assert(_getTaggedText((FrameworkElement)_macroLabelsListBox.Items[1], 1, "MacroNameTag") == "meow 2");
            Debug.Assert(_getTaggedText((FrameworkElement)_macroLabelsListBox.Items[1], 1, "ProbabilityTag") == "23");
            Debug.Assert(_getTaggedText((FrameworkElement)_macroLabelsListBox.Items[2], 2, "MacroNameTag") == "meow 3");
            Debug.Assert(_getTaggedText((FrameworkElement)_macroLabelsListBox.Items[2], 2, "ProbabilityTag") == "34");
        }

        /**
         * @brief Verifies that the first macro is automatically selected.
         * 
         * @test Validates that the first macro in the list is automatically selected and focused.
         * 
         * When macros are loaded into the ListBox, the handler should automatically
         * select the first item and give focus to the ListBox for loading in the
         * commands of the first item.
         */
        private void _testWindowBecomingVisibleSelectsFirstPopulatedLabel()
        {
            var handler = _testSetup();
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 12, MacroName = "meow 1" });
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 23, MacroName = "meow 2" });
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 34, MacroName = "meow 3" });
            handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            Debug.Assert(_macroLabelsListBox.SelectedIndex == 0);
            Debug.Assert(_macroLabelsListBox.IsFocused);
        }

        /**
         * @brief Verifies that macros are not loaded without model injection.
         * 
         * @test Validates that macro loading requires MapModel injection to function.
         * 
         * The handler requires access to the MapModel to load macros. Without
         * proper model injection, the ListBox should remain empty even when
         * other conditions (visibility, selection) are met.
         */
        private void _testWindowBecomingVisibleDoesNotPopulateLabelsWhenMapModelIsNotInjected()
        {
            var handler = _fixture();
            _macroWindow.VisibleReturn.Add(true);
            _menuState.Select(_selectedElement);
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 12, MacroName = "meow 1" });
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 23, MacroName = "meow 2" });
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 34, MacroName = "meow 3" });
            handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            Debug.Assert(_macroLabelsListBox.Items.Count == 0);
        }

        /**
         * @brief Verifies that macro loading only occurs on window visibility.
         * 
         * @test Validates that macro population only triggers when the window becomes visible.
         * 
         * The handler should only populate macros when the window becomes visible.
         * When the window is or becomes invisible, no action should be taken
         * to prevent unnecessary UI updates.
         */
        private void _testWindowBecomingInvisibleDoesNotPopulateLabels()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _macroWindow.VisibleReturn.Add(false);
            _menuState.Select(_selectedElement);
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 12, MacroName = "meow 1" });
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 23, MacroName = "meow 2" });
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 34, MacroName = "meow 3" });
            handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            Debug.Assert(_macroLabelsListBox.Items.Count == 0);
        }

        /**
         * @brief Verifies that macro loading requires a selected element.
         * 
         * @test Validates that macro population requires a selected minimap point.
         * 
         * Without a selected minimap point, there are no macros to display.
         * The handler should leave the ListBox empty when no element is
         * currently selected, even if the window becomes visible.
         */
        private void _testWindowBecomingVisibleDoesNotPopulateLabelsWhenNoSelection()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.MapModel, _mapModel);
            _macroWindow.VisibleReturn.Add(true);
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 12, MacroName = "meow 1" });
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 23, MacroName = "meow 2" });
            _minimapPointData.Commands.Add(new MinimapPointMacros { MacroChance = 34, MacroName = "meow 3" });
            handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            Debug.Assert(_macroLabelsListBox.Items.Count == 0);
        }


        /**
         * @brief Executes all test cases in the suite.
         * 
         * Runs each test method sequentially to validate the complete behavior
         * of the handler.
         */
        public void Run()
        {
            _testWindowBecomingVisibleSetsSelectedMinimapPointAsLabelTag();
            _testWindowBecomingVisibleSetsTextBoxWithPointName();
            _testWindowBecomingVisibleClearsLabelsListBox();
            _testWindowBecomingVisiblePopulatesLabelsListBoxWithCommands();
            _testWindowBecomingVisibleSelectsFirstPopulatedLabel();
            _testWindowBecomingVisibleDoesNotPopulateLabelsWhenMapModelIsNotInjected();
            _testWindowBecomingVisibleDoesNotPopulateLabelsWhenNoSelection();
            _testWindowBecomingInvisibleDoesNotPopulateLabels();
        }
    }


    /**
     * @class WindowMacroCommandLabelSavingActionHandlerTests
     * 
     * @brief Unit tests for the macro command label saving functionality.
     * 
     * This test suite validates the behavior of the macro command label saving action handler,
     * which manages the persistence of macro command data when the macro window closes.
     * Tests focus on ensuring that macro commands are properly saved to the data model
     * when the window becomes invisible, while also verifying edge cases and UI state management.
     */
    public class WindowMacroCommandLabelSavingActionHandlerTests
    {
        private Window _window;

        private MockSystemWindow _macroWindow;

        private TextBox _macroLabelTextBox;

        private ListBox _macroLabelsListBox;

        private MinimapPointData _minimapPointData;

        private MinimapPoint _minimapPoint;

        private MapModel _mapModel;

        /**
         * @brief Constructor initializing test dependencies with clean state.
         * 
         * Creates fresh instances of all UI components and data structures required for testing:
         * - Window and mock system window for visibility control
         * - UI controls for macro label display and command listing
         * - Data model and minimap point structures
         */
        public WindowMacroCommandLabelSavingActionHandlerTests()
        {
            _window = new Window();
            _macroWindow = new MockSystemWindow();
            _macroLabelTextBox = new TextBox();
            _macroLabelsListBox = new ListBox();
            _minimapPointData = new MinimapPointData();
            _minimapPoint = new MinimapPoint();
            _mapModel = new MapModel();
        }


        /**
         * @brief Helper method to add a test macro command to the UI and data model.
         * 
         * Creates a visual representation of a macro command in the ListBox and
         * simultaneously adds the corresponding data structure to the minimap point.
         * This ensures both UI and data model are synchronized for testing purposes.
         * 
         * @param name Display name of the macro command
         * @param probability Probability value as a string (will be converted to integer)
         * @param commands List of command strings associated with this macro
         */
        private void _addLabelsListBoxElement(
            string name, string probability, List<string> commands
        )
        {
            var listBoxElement = new Grid();
            listBoxElement.Children.Add(new TextBox { Tag = "MacroNameTag", Text = name });
            listBoxElement.Children.Add(new TextBox { Tag = "ProbabilityTag", Text = probability });
            _macroLabelsListBox.Items.Add(listBoxElement);
            _minimapPoint.PointData.ElementName = "meow";
            _minimapPoint.PointData.Commands.Add(
                new MinimapPointMacros
                {
                    MacroChance = Convert.ToInt32(probability),
                    MacroName = name,
                    MacroCommands = commands
                }
            );
        }


        /**
         * @brief Creates and configures a test handler instance with all dependencies.
         * 
         * Sets up a complete test environment with:
         * - Mock window returning the test window instance
         * - UI controls initialized with test data
         * - Minimap point attached to the ListBox Tag property
         * - Fresh data model instance
         * 
         * @return Fully configured test handler ready for testing
         */
        private AbstractWindowActionHandler _fixture()
        {
            _window = new Window();
            _macroWindow = new MockSystemWindow();
            _macroWindow.GetWindowReturn.Add(_window);
            _macroLabelTextBox = new TextBox();
            _macroLabelsListBox = new ListBox();
            _minimapPointData = new MinimapPointData();
            _minimapPoint = new MinimapPoint { PointData = _minimapPointData };
            _mapModel = new MapModel();
            _macroLabelsListBox.Tag = _minimapPoint;
            return new WindowMacroCommandLabelSavingActionHandlerFacade(
                _macroWindow,
                _macroLabelTextBox,
                _macroLabelsListBox
            );
        }

        /**
         * @brief Verifies that macro commands are saved to the data model when the window becomes invisible.
         * 
         * @test Validates that all macro commands in the UI are persisted to the MapModel on window close.
         * 
         * This test ensures that when users close the macro window, their macro command edits
         * (including names, probabilities, and command sequences) are properly saved to the
         * underlying data model for later retrieval and use.
         */
        private void _testWindowBecomingInvisibleSavesMacroCommandsToMapModel()
        {
            var handle = _fixture();
            _minimapPoint.PointData.ElementName = "meow";
            _mapModel.Add(_minimapPoint.Copy());
            handle.Inject(SystemInjectType.MapModel, _mapModel);
            _macroWindow.VisibleReturn.Add(false);
            var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
            adder.Add("meow 1", "12", ["1c1", "1c2", "1c3"]);
            adder.Add("meow 2", "23", ["2c1", "2c2", "2c3"]);
            adder.Add("meow 3", "34", ["3c1", "3c2", "3c3"]);
            handle.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            var savedMinimapPoint = _mapModel.FindName("meow")!;
            Debug.Assert(savedMinimapPoint.PointData.Commands[0].MacroName == "meow 1");
            Debug.Assert(savedMinimapPoint.PointData.Commands[0].MacroChance == 12);
            Debug.Assert(savedMinimapPoint.PointData.Commands[0].MacroCommands[0] == "1c1");
            Debug.Assert(savedMinimapPoint.PointData.Commands[0].MacroCommands[1] == "1c2");
            Debug.Assert(savedMinimapPoint.PointData.Commands[0].MacroCommands[2] == "1c3");
            Debug.Assert(savedMinimapPoint.PointData.Commands[1].MacroName == "meow 2");
            Debug.Assert(savedMinimapPoint.PointData.Commands[1].MacroChance == 23);
            Debug.Assert(savedMinimapPoint.PointData.Commands[1].MacroCommands[0] == "2c1");
            Debug.Assert(savedMinimapPoint.PointData.Commands[1].MacroCommands[1] == "2c2");
            Debug.Assert(savedMinimapPoint.PointData.Commands[1].MacroCommands[2] == "2c3");
            Debug.Assert(savedMinimapPoint.PointData.Commands[2].MacroName == "meow 3");
            Debug.Assert(savedMinimapPoint.PointData.Commands[2].MacroChance == 34);
            Debug.Assert(savedMinimapPoint.PointData.Commands[2].MacroCommands[0] == "3c1");
            Debug.Assert(savedMinimapPoint.PointData.Commands[2].MacroCommands[1] == "3c2");
            Debug.Assert(savedMinimapPoint.PointData.Commands[2].MacroCommands[2] == "3c3");
        }

        /**
         * @brief Verifies that selected macro commands are deselected after save operations.
         * 
         * @test Validates that the ListBox selection is cleared after saving macro commands.
         * 
         * This test ensures that when the window closes and saves data, any selected
         * macro command in the ListBox is deselected to provide a clean state for
         * the next time the window is opened, preventing confusion about which item is active.
         */
        private void _testWindowBecomingInvisibleDeselectsSelectedMacroCommand()
        {
            var handle = _fixture();
            _minimapPoint.PointData.ElementName = "meow";
            _mapModel.Add(_minimapPoint.Copy());
            handle.Inject(SystemInjectType.MapModel, _mapModel);
            _macroWindow.VisibleReturn.Add(false);
            var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
            adder.Add("meow 1", "12", ["1c1", "1c2", "1c3"]);
            adder.Add("meow 2", "23", ["2c1", "2c2", "2c3"]);
            adder.Add("meow 3", "34", ["3c1", "3c2", "3c3"]);
            _macroLabelsListBox.SelectedIndex = 0;
            handle.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            Debug.Assert(_macroLabelsListBox.SelectedIndex == -1);
            Debug.Assert(_macroLabelsListBox.IsFocused);
        }

        /**
         * @brief Verifies that text dependencies are updated with the current macro label.
         * 
         * @test Validates that all registered text elements are updated with the macro label text.
         * 
         * This test ensures that when the window closes, any UI elements that depend on
         * the macro label text (such as TextBox and TextBlock controls) are updated
         * with the current label value, maintaining consistency across the application.
         */
        private void _testWindowBecomingInvisibleUpdatesAnyTextDependencies()
        {
            var elementTextsParameters = new List<List<FrameworkElement>>
            {
                ([new TextBox(), new TextBox(), new TextBox()]),
                ([new TextBlock(), new TextBlock(), new TextBlock()]),
                ([new TextBox(), new TextBlock(), new TextBox()])
            };
            for (int i = 0; i < elementTextsParameters.Count; i++)
            {
                var elementTexts = elementTextsParameters[i];
                var handle = _fixture();
                _minimapPoint.PointData.ElementName = "meow";
                _mapModel.Add(_minimapPoint.Copy());
                handle.Inject(SystemInjectType.MapModel, _mapModel);
                _macroWindow.VisibleReturn.Add(false);
                _macroLabelTextBox.Text = "meowth thats right!";
                _minimapPoint.PointData.ElementTexts = elementTexts;
                handle.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
                for (int j = 0; j < elementTexts.Count; j++)
                {
                    if (elementTexts[j] is TextBox textBox)
                    {
                        Debug.Assert(textBox.Text == "meowth thats right!");
                    }
                    else if (elementTexts[j] is TextBlock textBlock)
                    {
                        Debug.Assert(textBlock.Text == "meowth thats right!");
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }
        }

        /**
         * @brief Verifies that macro commands are not saved when the window becomes visible.
         * 
         * @test Validates that data persistence does not occur on window visibility.
         * 
         * This test ensures that when the macro window opens (becomes visible), no
         * data save operations are triggered, preventing unintended overwrites of
         * existing macro command data during window display operations.
         */
        private void _testWindowBecomingVisibleDoesNotSaveMacroCommands()
        {
            var handle = _fixture();
            _minimapPoint.PointData.ElementName = "meow";
            _mapModel.Add(_minimapPoint.Copy());
            handle.Inject(SystemInjectType.MapModel, _mapModel);
            _macroWindow.VisibleReturn.Add(true);
            var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
            adder.Add("meow 1", "12", ["1c1", "1c2", "1c3"]);
            adder.Add("meow 2", "23", ["2c1", "2c2", "2c3"]);
            adder.Add("meow 3", "34", ["3c1", "3c2", "3c3"]);
            handle.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            var savedMinimapPoint = _mapModel.FindName("meow")!;
            Debug.Assert(savedMinimapPoint.PointData.Commands.Count == 0);
        }

        /**
         * @brief Verifies that macro commands are not saved when the data model is not injected.
         * 
         * @test Validates that data persistence requires proper model injection.
         * 
         * This test ensures that the handler cannot save macro commands without access
         * to the MapModel, preventing data corruption or exceptions when the required
         * dependencies are not properly configured.
         */
        private void _testWindowBecomingInvisibleDoesNotSaveMacroCommandsWhenModelIsNotInjected()
        {
            var handle = _fixture();
            _minimapPoint.PointData.ElementName = "meow";
            _mapModel.Add(_minimapPoint.Copy());
            _macroWindow.VisibleReturn.Add(false);
            var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
            adder.Add("meow 1", "12", ["1c1", "1c2", "1c3"]);
            adder.Add("meow 2", "23", ["2c1", "2c2", "2c3"]);
            adder.Add("meow 3", "34", ["3c1", "3c2", "3c3"]);
            handle.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
            var savedMinimapPoint = _mapModel.FindName("meow")!;
            Debug.Assert(savedMinimapPoint.PointData.Commands.Count == 0);
        }

        /**
         * @brief Executes the complete test suite for macro command label saving functionality.
         * 
         * Runs all test methods to validate the complete behavior of the
         * WindowMacroCommandLabelSavingActionHandler, ensuring reliable data persistence
         * and proper UI state management during window visibility changes.
         */
        public void Run()
        {
            _testWindowBecomingInvisibleSavesMacroCommandsToMapModel();
            _testWindowBecomingInvisibleDeselectsSelectedMacroCommand();
            _testWindowBecomingInvisibleUpdatesAnyTextDependencies();
            _testWindowBecomingInvisibleDoesNotSaveMacroCommandsWhenModelIsNotInjected();
            _testWindowBecomingVisibleDoesNotSaveMacroCommands();
        }
    }


    /**
     * @class WindowMacroCommandsSaveStateActionHandlerTests
     * 
     * @brief Unit tests for the macro commands save state functionality.
     * 
     * This test suite validates the behavior of the macro commands save action handler,
     * which manages the automatic saving of macro command changes when the user switches
     * between different macro labels in the interface. Tests ensure that command state
     * is preserved during navigation to prevent data loss.
     */
    public class WindowMacroCommandsSaveStateActionHandlerTests
    {
        private ListBox _macroLabelsListBox;

        private ListBox _macroCommandsListBox;

        private MinimapPoint _minimapPoint;

        /**
         * @brief Constructor initializing test components with clean state.
         * 
         * 
         * Creates fresh instances of UI components and data structures:
         * - ListBoxes for macro labels and commands display
         * - MinimapPoint for storing macro data relationships
         */
        public WindowMacroCommandsSaveStateActionHandlerTests()
        {
            _macroLabelsListBox = new ListBox();
            _macroCommandsListBox = new ListBox();
            _minimapPoint = new MinimapPoint();
        }


        /**
         * @brief Creates and configures a test handler instance with all dependencies.
         * 
         * Sets up a complete test environment with:
         * - Fresh ListBox instances for macro labels and commands
         * - MinimapPoint data structure attached to the labels ListBox Tag
         * - Handler facade connecting the UI components to the save state logic
         * 
         * @return AbstractWindowActionHandler Fully configured test handler for save state testing
         */
        private AbstractWindowActionHandler _fixture()
        {
            _macroLabelsListBox = new ListBox();
            _macroCommandsListBox = new ListBox();
            _minimapPoint = new MinimapPoint();
            _macroLabelsListBox.Tag = _minimapPoint;
            return new WindowMacroCommandsSaveStateActionHandlerFacade(
                _macroLabelsListBox, _macroCommandsListBox
            );
        }

        /**
         * @brief Verifies that macro commands are automatically saved when switching between macro labels.
         * 
         * @test Validates that command list state is preserved when navigating between different macro labels.
         * 
         * This test ensures that when users switch between different macro labels in the interface,
         * any commands currently displayed in the macro commands list box are automatically saved
         * to the previously selected macro label before loading the new label's commands.
         * This prevents data loss when users edit commands and then navigate to view other macros.
         */
        private void _testChangingSelectionSavesCurrentCommands()
        {
            var handler = _fixture();
            _macroCommandsListBox.Items.Add(new ComboBox { Text = "meow 1" });
            _macroCommandsListBox.Items.Add(new ComboBox { Text = "meow 2" });
            _macroCommandsListBox.Items.Add(new ComboBox { Text = "meow 3" });
            var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
            adder.Add("meow 1", "12", ["1c1", "1c2", "1c3"]);
            adder.Add("meow 2", "23", ["2c1", "2c2", "2c3"]);
            adder.Add("meow 3", "34", ["3c1", "3c2", "3c3"]);
            _macroLabelsListBox.SelectedIndex = 1;
            _macroLabelsListBox.SelectedIndex = 2;
            Debug.Assert(_minimapPoint.PointData.Commands[1].MacroCommands[0] == "meow 1");
            Debug.Assert(_minimapPoint.PointData.Commands[1].MacroCommands[1] == "meow 2");
            Debug.Assert(_minimapPoint.PointData.Commands[1].MacroCommands[2] == "meow 3");
        }

        /**
         * @brief Executes the complete test suite for macro commands save state functionality.
         * 
         * Runs all test methods to validate the automatic save behavior of the
         * macro commands save action handler, ensuring that user command edits
         * are preserved during navigation between different macro labels.
         */
        public void Run()
        {
            _testChangingSelectionSavesCurrentCommands();
        }
    }


    /**
     * @class WindowMacroCommandsDisplayActionHandlerTests
     * 
     * @brief Unit tests for the macro commands display and management functionality.
     * 
     * This test suite validates the behavior of the macro commands display action handler,
     * which manages the display and synchronization of macro commands when users select
     * different macro labels. Tests ensure that command lists are properly loaded,
     * cleared, and associated with scaling registries as users navigate the interface.
     */
    public class WindowMacroCommandsDisplayActionHandlerTests
    {
        private ListBox _macroLabelsListBox;

        private ListBox _macroCommandsListBox;

        private ComboBox _comboBoxTemplate;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        private MinimapPoint _minimapPoint;

        /**
         * @brief Constructor initializing test components with clean state.
         * 
         * Creates fresh instances of all UI components and data structures:
         * - ListBoxes for macro labels and commands
         * - ComboBox template for command item rendering
         * - Scaling registry for command display management
         * - MinimapPoint for storing macro data relationships
         */
        public WindowMacroCommandsDisplayActionHandlerTests()
        {
            _macroLabelsListBox = new ListBox();
            _macroCommandsListBox = new ListBox();
            _comboBoxTemplate = new ComboBox();
            _comboBoxPopupScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
            _minimapPoint = new MinimapPoint();
        }

        /**
         * @brief Creates and configures a test handler instance with all dependencies.
         * 
         * Sets up a complete test environment with:
         * - Fresh UI components (ListBoxes, ComboBox template)
         * - Scaling registry for command display management
         * - MinimapPoint attached to the labels ListBox Tag property
         * - Handler facade connecting all components for testing
         * 
         * @return AbstractWindowActionHandler Fully configured test handler ready for command display testing
         */
        private AbstractWindowActionHandler _fixture()
        {
            _macroLabelsListBox = new ListBox();
            _macroCommandsListBox = new ListBox();
            _comboBoxTemplate = new ComboBox();
            _comboBoxPopupScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
            _minimapPoint = new MinimapPoint();
            _macroLabelsListBox.Tag = _minimapPoint;
            return new WindowMacroCommandsDisplayActionHandlerFacade(
                _macroLabelsListBox,
                _macroCommandsListBox,
                _comboBoxTemplate,
                _comboBoxPopupScaleRegistry
            );
        }

        /**
         * @brief Verifies that the current macro command list is cleared when selecting a new macro label.
         * 
         * @test Validates that the macro commands ListBox is emptied when switching to a new macro label.
         * 
         * This test ensures that when users select a different macro label, any previously
         * displayed commands in the macro commands ListBox are cleared to prevent displaying
         * stale data from the previously selected macro.
         */
        private void _testChangingSelectedMacroClearsCurrentMacroCommands()
        {
            var handler = _fixture();
            var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
            adder.Add("meow 1", "12", []);
            _macroCommandsListBox.Items.Add(new FrameworkElement());
            _macroLabelsListBox.SelectedIndex = 0;
            Debug.Assert(_macroCommandsListBox.Items.Count == 0);

        }

        /**
         * @brief Verifies that the correct command list is loaded when selecting different macro labels.
         * 
         * @test Validates that each macro label's associated commands are properly loaded into the ListBox.
         * 
         * This test ensures that when users switch between different macro labels, the
         * corresponding command list for each macro is accurately loaded and displayed
         * in the macro commands ListBox.
         */
        private void _testChangingSelectedMacroPopulatesCommands()
        {
            var handler = _fixture();
            var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
            adder.Add("meow 1", "12", ["1c1", "1c2", "1c3"]);
            adder.Add("meow 2", "23", ["2c1", "2c2", "2c3"]);
            adder.Add("meow 3", "34", ["3c1", "3c2", "3c3"]);
            _macroLabelsListBox.SelectedIndex = 0;
            Debug.Assert(_macroCommandsListBox.Items.Count == 3);
            Debug.Assert(((ComboBox)_macroCommandsListBox.Items[0]).Text == "1c1");
            Debug.Assert(((ComboBox)_macroCommandsListBox.Items[1]).Text == "1c2");
            Debug.Assert(((ComboBox)_macroCommandsListBox.Items[2]).Text == "1c3");
            _macroLabelsListBox.SelectedIndex = 1;
            Debug.Assert(_macroCommandsListBox.Items.Count == 3);
            Debug.Assert(((ComboBox)_macroCommandsListBox.Items[0]).Text == "2c1");
            Debug.Assert(((ComboBox)_macroCommandsListBox.Items[1]).Text == "2c2");
            Debug.Assert(((ComboBox)_macroCommandsListBox.Items[2]).Text == "2c3");
        }

        /**
         * @brief Verifies that scaling registry handlers are cleared when selecting a new macro label.
         * 
         * @test Validates that the scaling registry is cleared of old handlers when switching macros.
         * 
         * This test ensures that when users switch to a different macro label, any scaling
         * handlers registered for the previous macro's commands are properly cleaned up
         * to prevent resource leaks and ensure accurate scaling for the new command set.
         */
        private void _testChangingSelectedMacroClearsScaleRegistry()
        {
            var handler = _fixture();
            var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
            adder.Add("meow 1", "12", []);
            _comboBoxPopupScaleRegistry.RegisterHandler(
                new WindowComboBoxScaleActionHandlerParameters(new ComboBox())
            );
            Debug.Assert(_comboBoxPopupScaleRegistry.GetHandlers().Count == 1);
            _macroLabelsListBox.SelectedIndex = 0;
            Debug.Assert(_comboBoxPopupScaleRegistry.GetHandlers().Count == 0);
        }

        /**
         * @brief Verifies that scaling registry handlers are assigned for each command in the selected macro.
         * 
         * @test Validates that the scaling registry receives the correct number of handlers for each command.
         * 
         * This test ensures that when a macro label is selected, scaling handlers are
         * properly registered for each command in that macro's command list. The test
         * verifies the registration count matches the command count across multiple
         * test cases with varying command list sizes.
         */
        private void _testChangingSelectedMacroAssignsScaleRegistry()
        {
            for (int commandCount = 1; commandCount < 10; commandCount++)
            {
                var handler = _fixture();
                var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
                var commands = Enumerable.Range(0, commandCount).Select(i => i.ToString()).ToList();
                adder.Add($"meow {commandCount}", commandCount.ToString(), commands);
                _macroLabelsListBox.SelectedIndex = 0;
                Debug.Assert(_comboBoxPopupScaleRegistry.GetHandlers().Count == commandCount);
            }
        }

        /**
         * @brief Executes the complete test suite for macro commands display functionality.
         * 
         * Runs all test methods to validate the comprehensive behavior of the
         * WindowMacroCommandsDisplayActionHandler, ensuring that macro command display,
         * loading, and resource management work correctly during user navigation between
         * different macro labels.
         */
        public void Run()
        {
            _testChangingSelectedMacroClearsCurrentMacroCommands();
            _testChangingSelectedMacroPopulatesCommands();
            _testChangingSelectedMacroClearsScaleRegistry();
            _testChangingSelectedMacroAssignsScaleRegistry();
        }
    }


    /**
     * @class WindowMacroCommandsAddingActionHandlerTests
     * 
     * @brief Unit tests for the macro command adding functionality.
     * 
     * This test suite validates the behavior of the macro commands adding action handler,
     * which manages the addition of new macro commands to the user interface. Tests ensure
     * that users can reliably add new macro commands with proper naming, default values,
     * and UI focus management while maintaining data integrity and unique naming conventions.
     */
    public class WindowMacroCommandsAddingActionHandlerTests
    {
        private Button _macroAddButton;

        private ListBox _macroLabelsListBox;

        private Grid _pointMacroTemplate;

        private TextBox _macroNameTextBox;

        private TextBox _macroProbabilityTextBox;

        private MinimapPointData _minimapPointData;

        private MinimapPoint _minimapPoint;

        /**
         * @brief Constructor initializing test components with clean state.
         * 
         * Creates fresh instances of all UI components and data structures required for testing:
         * - Button for adding macros
         * - ListBox for macro display
         * - Grid template for new macro structure
         * - TextBoxes for macro name and probability
         * - Data structures for storing macro information
         */
        public WindowMacroCommandsAddingActionHandlerTests()
        {
            _macroAddButton = new Button();
            _macroLabelsListBox = new ListBox();
            _pointMacroTemplate = new Grid();
            _macroNameTextBox = new TextBox();
            _macroProbabilityTextBox = new TextBox();
            _minimapPointData = new MinimapPointData();
            _minimapPoint = new MinimapPoint();
        }

        /**
         * @brief Creates and configures a test handler instance with all dependencies.
         * 
         * Sets up a complete test environment with:
         * - Fresh UI components (Button, ListBox, Grid template, TextBoxes)
         * - MinimapPoint data structure with associated point data
         * - Template children added to the grid for proper item rendering
         * - MinimapPoint attached to the ListBox Tag for data binding
         * - Handler facade connecting all components for testing
         * 
         * @return AbstractWindowActionHandler Fully configured test handler for macro adding
         * functionality testing
         */
        private AbstractWindowActionHandler _fixture()
        {
            _macroAddButton = new Button();
            _macroLabelsListBox = new ListBox();
            _pointMacroTemplate = new Grid();
            _macroNameTextBox = new TextBox();
            _macroProbabilityTextBox = new TextBox();
            _minimapPointData = new MinimapPointData();
            _minimapPoint = new MinimapPoint { PointData = _minimapPointData };
            _pointMacroTemplate.Children.Add(_macroNameTextBox);
            _pointMacroTemplate.Children.Add(_macroProbabilityTextBox);
            _macroLabelsListBox.Tag = _minimapPoint;
            return new WindowMacroCommandsAddingActionHandlerFacade(
                _macroAddButton,
                _macroLabelsListBox,
                _pointMacroTemplate
            );
        }

        /**
         * @brief Verifies that clicking the add button creates new macro command entries.
         * 
         * @test Validates that multiple new macro entries are created with correct
         * default values.
         * 
         * This test ensures that users can add multiple new macro commands by clicking
         * the add button, with each new entry receiving proper default values including
         * sequentially numbered names and zero probability initialization.
         */
        private void _testClickingAddButtonAddsNewMacro()
        {
            for (int i = 1; i < 10; i++)
            {
                var handler = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _macroAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                Debug.Assert(_macroLabelsListBox.Items.Count == i);
                for (int j = 0; j < i; j++)
                {
                    var macroName = ((TextBox)((Grid)_macroLabelsListBox.Items[j]).Children[0]).Text;
                    var macroProb = ((TextBox)((Grid)_macroLabelsListBox.Items[j]).Children[1]).Text;
                    Debug.Assert(macroName == "Macro " + j);
                    Debug.Assert(macroProb == "0");
                }
            }
        }

        /**
         * @brief Verifies that newly added macro commands automatically receive focus.
         * 
         * @test Validates that the ListBox focuses on and selects each newly added macro item.
         * 
         * This test ensures that when users add new macro commands, the interface
         * automatically focuses on and selects the newest item, allowing for immediate
         * editing without additional clicks and improving workflow efficiency.
         */
        private void _testClickingAddButtonFocusesOnNewItem()
        {
            for (int i = 1; i < 10; i++)
            {
                var handler = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _macroAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Debug.Assert(_macroLabelsListBox.SelectedIndex == j);
                    Debug.Assert(_macroLabelsListBox.IsFocused);
                }
            }
        }

        /**
         * @brief Verifies that newly added macros receive unique names to prevent conflicts.
         * 
         * @test Validates that macro name generation avoids conflicts with existing names.
         * 
         * This test ensures that when existing macros have been renamed, new macros
         * continue to receive unique, sequentially numbered names to prevent naming
         * conflicts and maintain clear identification of all macro commands.
         */
        private void _testClickingAddButtonAddsUniqueMacroName()
        {
            for (int i = 1; i < 10; i++)
            {
                var handler = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _macroAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                _minimapPointData.Commands[i - 1].MacroName = "Macro " + i;
                _macroAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                var macroNameLabel = (TextBox)((Grid)_macroLabelsListBox.Items[i]).Children[0];
                Debug.Assert(macroNameLabel.Text == "Macro " + (i + 1));
            }
        }

        /**
         * @brief Executes the complete test suite for macro command adding functionality.
         * 
         * Runs all test methods to validate the comprehensive behavior of the
         * WindowMacroCommandsAddingActionHandler, ensuring that users can reliably
         * add, focus, and uniquely name macro commands in their automation workflows.
         */
        public void Run()
        {
            _testClickingAddButtonAddsNewMacro();
            _testClickingAddButtonFocusesOnNewItem();
            _testClickingAddButtonAddsUniqueMacroName();
        }
    }


    /**
     * @class WindowMacroCommandsRemovingActionHandlerTests
     * 
     * @brief Unit tests for the macro command removal functionality.
     * 
     * This test suite validates the behavior of the macro commands removing action handler,
     * which manages the removal of macro commands from both the UI and data model. Tests
     * cover various scenarios including random selection removal, selection index updates,
     * edge cases for last item removal, and handling of no selection states.
     */
    public class WindowMacroCommandsRemovingActionHandlerTests
    {
        private Button _macroRemoveButton;

        private ListBox _macroLabelsListBox;

        private MinimapPointData _minimapPointData;

        private MinimapPoint _minimapPoint;

        /**
         * @brief Constructor initializing test components with clean state.
         * 
         * Creates fresh instances of all UI components and data structures:
         * - Button for removing macros
         * - ListBox for macro display
         * - Data structures for storing macro information
         * 
         * Each test method starts from this clean state to ensure consistent
         * behavior and isolation between test executions.
         */
        public WindowMacroCommandsRemovingActionHandlerTests()
        {
            _macroRemoveButton = new Button();
            _macroLabelsListBox = new ListBox();
            _minimapPointData = new MinimapPointData();
            _minimapPoint = new MinimapPoint { PointData = _minimapPointData };
        }

        /**
         * @brief Creates and configures a test handler instance with all dependencies.
         * 
         * Sets up a complete test environment with:
         * - Fresh UI components (Button, ListBox)
         * - MinimapPoint data structure with associated point data
         * - MinimapPoint attached to the ListBox Tag for data binding
         * - Handler facade connecting the components for testing
         * 
         * @return AbstractWindowActionHandler Fully configured test handler for macro
         * removal functionality testing
         */
        public AbstractWindowActionHandler _fixture()
        {
            _macroRemoveButton = new Button();
            _macroLabelsListBox = new ListBox();
            _minimapPointData = new MinimapPointData();
            _minimapPoint = new MinimapPoint { PointData = _minimapPointData };
            _macroLabelsListBox.Tag = _minimapPoint;
            return new WindowMacroCommandsRemovingActionHandlerFacade(
                _macroRemoveButton, _macroLabelsListBox
            );
        }

        /**
         * @brief Verifies that clicking remove button deletes the selected macro from both UI
         * and data model.
         * 
         * @test Validates that removal works correctly for random selections across various list sizes.
         * 
         * This test ensures that when users remove a selected macro command, it is properly
         * deleted from both the visual ListBox display and the underlying data model.
         * This ensures complete synchronization between UI and data during removal.
         */
        private void _testClickingRemoveButtonRemovesSelectedMacro()
        {
            for (int i = 3; i < 20; i++)
            {
                var handler = _fixture();
                var randomIndex = new Random(0).Next(i);
                var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
                for (int j = 0; j < i; j++)
                {
                    adder.Add($"meow {j}", j.ToString(), [$"{j}c1", $"{j}c2", $"{j}c3"]);
                }
                _macroLabelsListBox.SelectedIndex = randomIndex;
                var selectedItem = _macroLabelsListBox.SelectedItem;
                var selectedData = _minimapPointData.Commands[randomIndex];
                Debug.Assert(_macroLabelsListBox.Items.Count == i);
                Debug.Assert(_minimapPointData.Commands.Count == i);
                _macroRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Debug.Assert(_macroLabelsListBox.Items.Count == i - 1);
                Debug.Assert(_macroLabelsListBox.Items.IndexOf(selectedItem) == -1);
                Debug.Assert(_minimapPointData.Commands.Count == i - 1);
                Debug.Assert(_minimapPointData.Commands.IndexOf(selectedData) == -1);
            }
        }

        /**
         * @brief Verifies that selection index remains on the same position after removal.
         * 
         * @test Validates that when removing a non-last item, selection stays at the same index.
         * 
         * This test ensures that when users remove a macro command from the middle of the list,
         * the selection index remains at the same position (now pointing to the item that
         * shifted into that position), and focus is maintained for continuous operation.
         */
        private void _testClickingRemoveButtonUpdatesSelectedIndex()
        {
            for (int i = 3; i < 20; i++)
            {
                var handler = _fixture();
                var removeIndex = i / 2;
                var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
                for (int j = 0; j < i; j++)
                {
                    adder.Add($"meow {j}", j.ToString(), [$"{j}c1", $"{j}c2", $"{j}c3"]);
                }
                _macroLabelsListBox.SelectedIndex = removeIndex;
                _macroRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Debug.Assert(_macroLabelsListBox.SelectedIndex == removeIndex);
                Debug.Assert(_macroLabelsListBox.IsFocused);
            }
        }

        /**
         * @brief Verifies that selection index moves up when removing the last item.
         * 
         * @test Validates that when removing the last item, selection moves to the new last item.
         * 
         * This test ensures that when users remove the last macro command in the list,
         * the selection index correctly moves to the new last item (index - 1) and
         * focus is maintained for continuous editing or further removal.
         */
        private void _testClickingRemoveButtonUpdatesLastIndex()
        {
            for (int i = 3; i < 20; i++)
            {
                var handler = _fixture();
                var removeIndex = i - 1;
                var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
                for (int j = 0; j < i; j++)
                {
                    adder.Add($"meow {j}", j.ToString(), [$"{j}c1", $"{j}c2", $"{j}c3"]);
                }
                _macroLabelsListBox.SelectedIndex = removeIndex;
                var selectedItem = _macroLabelsListBox.SelectedItem;
                _macroRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Debug.Assert(_macroLabelsListBox.SelectedIndex == removeIndex - 1);
                Debug.Assert(_macroLabelsListBox.IsFocused);
            }
        }

        /**
         * @brief Verifies that no removal occurs when no macro is selected.
         * 
         * @test Validates that the remove button has no effect when no item is selected.
         * 
         * This test ensures that when no macro command is selected in the ListBox,
         * clicking the remove button has no effect on the list contents, selection state,
         * or focus. This prevents accidental data loss when users click remove without
         * first selecting an item.
         */
        private void _testClickingRemoveButtonDoesntRemoveWhenNoneSelected()
        {
            for (int i = 3; i < 20; i++)
            {
                var handler = _fixture();
                var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
                for (int j = 0; j < i; j++)
                {
                    adder.Add($"meow {j}", j.ToString(), [$"{j}c1", $"{j}c2", $"{j}c3"]);
                }
                _macroLabelsListBox.SelectedIndex = -1;
                var selectedItem = _macroLabelsListBox.SelectedItem;
                _macroRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Debug.Assert(_macroLabelsListBox.Items.Count == i);
                Debug.Assert(_macroLabelsListBox.SelectedIndex == -1);
                Debug.Assert(!_macroLabelsListBox.IsFocused);
            }
        }

        /**
         * @brief Executes the complete test suite for macro command removal functionality.
         * 
         * Runs all test methods to validate the comprehensive behavior of the
         * macro commands removing action handler, ensuring that macro command removal
         * works correctly across various scenarios while maintaining proper UI state
         * and data model synchronization.
         */
        public void Run()
        {
            _testClickingRemoveButtonRemovesSelectedMacro();
            _testClickingRemoveButtonUpdatesSelectedIndex();
            _testClickingRemoveButtonUpdatesLastIndex();
            _testClickingRemoveButtonDoesntRemoveWhenNoneSelected();
        }
    }


    /**
     * @class WindowMacroCommandsRemoveButtonAccessActionHandlerTests
     * 
     * @brief Unit tests for the remove button accessibility management functionality.
     * 
     * This test suite validates the behavior of the macro commands remove button access
     * action handler, which dynamically manages the enabled/disabled state of the remove
     * button based on macro command list conditions and user interactions.
     */
    public class WindowMacroCommandsRemoveButtonAccessActionHandlerTests
    {
        private Window _window;

        private MockSystemWindow _macroWindow;

        private ListBox _macroLabelsListBox;

        private Button _addButton;

        private Button _removeButton;

        private MinimapPointData _minimapPointData;

        private MinimapPoint _minimapPoint;

        /**
         * @brief Constructor initializing test components with clean state.
         * 
         * Creates fresh instances of all UI components and data structures required for testing:
         * - Window and mock system window for visibility control
         * - ListBox for macro display
         * - Add and remove buttons for macro management
         * - Data structures for storing macro information
         * 
         * Each test method starts from this clean state to ensure consistent
         * behavior and isolation between test executions.
         */
        public WindowMacroCommandsRemoveButtonAccessActionHandlerTests()
        {
            _window = new Window();
            _macroWindow = new MockSystemWindow();
            _macroLabelsListBox = new ListBox();
            _addButton = new Button();
            _removeButton = new Button();
            _minimapPointData = new MinimapPointData();
            _minimapPoint = new MinimapPoint();
        }

        /**
         * @brief Creates and configures a test handler instance with all dependencies.
         * 
         * Sets up a complete test environment with:
         * - Fresh UI components (Window, ListBox, Buttons)
         * - Mock window returning the test window instance
         * - MinimapPoint data structure with associated point data
         * - MinimapPoint attached to the ListBox Tag for data binding
         * - Handler facade connecting all components for testing
         * 
         * @return AbstractWindowActionHandler Fully configured test handler for remove
         * button accessibility testing
         */
        private AbstractWindowActionHandler _fixture()
        {
            _window = new Window();
            _macroWindow = new MockSystemWindow();
            _macroWindow.GetWindowReturn.Add(_window);
            _macroLabelsListBox = new ListBox();
            _addButton = new Button();
            _removeButton = new Button();
            _minimapPointData = new MinimapPointData();
            _minimapPoint = new MinimapPoint { PointData = _minimapPointData };
            _macroLabelsListBox.Tag = _minimapPoint;
            return new WindowMacroCommandsRemoveButtonAccessActionHandlerFacade(
                _macroWindow,
                _macroLabelsListBox,
                _addButton,
                _removeButton
            );
        }

        /**
         * @brief Verifies that window visibility changes update the remove button accessibility.
         * 
         * @test Validates that the remove button state is recalculated when the window becomes visible.
         * 
         * This test ensures that when the macro window becomes visible, the remove button's
         * enabled state is automatically updated based on the current number of macro commands.
         * The button follows the rule: disabled for 0-1 items, enabled for 2+ items.
         * This ensures the UI provides correct feedback when the window is first displayed.
         */
        private void _testWindowVisibilityUpdatesButtonAccessibility()
        {
            for (int i = 0; i < 3; i++)
            {
                var handler = _fixture();
                var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
                for (int j = 0; j < i; j++)
                {
                    adder.Add("meow " + j, j.ToString(), []);
                }
                _removeButton.IsEnabled = i <= 1;
                handler.OnDependencyEvent(new object(), new DependencyPropertyChangedEventArgs());
                Debug.Assert(_removeButton.IsEnabled == i > 1);
            }
        }

        /**
         * @brief Verifies that adding macros via the add button updates remove button accessibility.
         * 
         * @test Validates that the remove button state updates correctly when new macros are added.
         * 
         * This test ensures that when users add new macro commands using the add button,
         * the remove button's enabled state is automatically recalculated. The button
         * should become enabled once the second macro is added.
         */
        private void _testAddButtonClickUpdatesButtonAccessibility()
        {
            for (int i = 0; i < 3; i++)
            {
                var handler = _fixture();
                var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
                for (int j = 0; j < i; j++)
                {
                    adder.Add("meow " + j, j.ToString(), []);
                }
                _removeButton.IsEnabled = i <= 1;
                _addButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Debug.Assert(_removeButton.IsEnabled == i > 1);
            }
        }


        /**
         * @brief Verifies that removing macros via the remove button updates its own accessibility.
         * 
         * @test Validates that the remove button state updates correctly when macros are removed.
         * 
         * This test ensures that when users remove macro commands using the remove button,
         * the button's enabled state is automatically recalculated. The button should
         * become disabled if the removal would leave 0 or 1 macros in the list.
         */
        private void _testRemoveButtonClickUpdatesButtonAccessibility()
        {
            for (int i = 0; i < 3; i++)
            {
                var handler = _fixture();
                var adder = new ListBoxElementAdder(_macroLabelsListBox, _minimapPoint);
                for (int j = 0; j < i; j++)
                {
                    adder.Add("meow " + j, j.ToString(), []);
                }
                _removeButton.IsEnabled = i <= 1;
                _removeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Debug.Assert(_removeButton.IsEnabled == i > 1);
            }
        }


        /**
         * @brief Executes the complete test suite for remove button accessibility functionality.
         * 
         * Runs all test methods to validate the comprehensive behavior of the
         * macro commands remove button access action handler, ensuring that the remove
         * button's enabled state responds correctly to window visibility changes
         * add operations, and remove operations while maintaining the threshold-based
         * accessibility rule.
         */
        public void Run()
        {
            _testWindowVisibilityUpdatesButtonAccessibility();
            _testAddButtonClickUpdatesButtonAccessibility();
            _testRemoveButtonClickUpdatesButtonAccessibility();
        }
    }


    /**
     * @class WindowMacroCommandsProbabilityTextBoxBindingActionHandlerTests
     * 
     * @brief Unit tests for macro command probability textbox binding and validation
     * 
     * Validates that probability textboxes embedded in macro command ListBox items
     * correctly enforce numeric constraints and validation rules. Tests ensure that
     * both typed input and clipboard paste operations respect the 0-100 probability
     * range and numeric-only requirements. Verifies proper integration of validation
     * handlers with dynamically generated ListBox items.
     */
    public class WindowMacroCommandsProbabilityTextBoxBindingActionHandlerTests
    {
        private ListBox _macroLabelsListBox;

        private Grid _listBoxItem;

        private TextBox _probabilityTextBox;

        /**
         * @brief Initializes test components with empty state
         * 
         * Creates fresh instances of ListBox, Grid (for ListBoxItem simulation),
         * and TextBox for each test run. Ensures isolated test environment without
         * residual state from previous tests, maintaining test reliability and
         * preventing cross-test contamination.
         */
        public WindowMacroCommandsProbabilityTextBoxBindingActionHandlerTests()
        {
            _macroLabelsListBox = new ListBox();
            _listBoxItem = new Grid();
            _probabilityTextBox = new TextBox();
        }

        /**
         * @brief Generates WPF text input events for simulation
         * 
         * @tests Event generation for testing user input simulation
         * 
         * Creates a TextCompositionEventArgs object that simulates a user
         * typing a specific character into the textbox. This method allows
         * unit tests to programmatically trigger the same PreviewTextInput
         * events that would occur during actual user interaction.
         * 
         * @param text The character or string to simulate typing
         * 
         * @returns Configured event arguments for raising PreviewTextInput
         */
        private RoutedEventArgs _generateTextCompositionEvent(
            string text
        )
        {
            return new TextCompositionEventArgs(
                System.Windows.Input.Keyboard.PrimaryDevice,
                new TextComposition(InputManager.Current, _probabilityTextBox, text)
            )
            {
                RoutedEvent = UIElement.PreviewTextInputEvent
            };
        }

        /**
         * @brief Generates WPF paste event arguments for simulation
         * 
         * @tests Event generation for testing paste operation simulation
         * 
         * Creates a DataObjectPastingEventArgs object that simulates a user
         * pasting text from the clipboard into the textbox. This method allows
         * unit tests to programmatically trigger the same paste validation
         * events that would occur during actual user interaction.
         * 
         * @param text The text content to simulate pasting from clipboard
         * @param format The data format (defaults to plain text)
         * 
         * @returns Configured event arguments for raising paste validation
         */
        private DataObjectPastingEventArgs _generateDataObjectPastingEvent(string text)
        {
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Text, text);
            return new DataObjectPastingEventArgs(
                dataObject, false, DataFormats.Text
            )
            {
                RoutedEvent = DataObject.PastingEvent
            };
        }

        /**
         * @brief Creates a test fixture with simulated ListBox item containing probability textbox
         * 
         * @tests Test environment setup for ListBox-item-textbox integration scenarios
         * 
         * Constructs a complete test environment that simulates the actual UI structure where
         * probability textboxes are embedded within ListBox items. Creates a fresh ListBox,
         * Grid container (simulating ListBoxItem), and a TextBox configured with the expected
         * "ProbabilityTag" identifier. The fixture ensures each test runs with isolated,
         * properly structured UI components that mirror production visual tree relationships.

         * @returns Configured AbstractWindowActionHandler facade that manages probability
         *          textbox validation binding for ListBox items
         */
        private AbstractWindowActionHandler _fixture()
        {
            _macroLabelsListBox = new ListBox();
            _listBoxItem = new Grid();
            _probabilityTextBox = new TextBox() { Tag = "ProbabilityTag" };
            _listBoxItem.Children.Add(_probabilityTextBox);
            return new WindowMacroCommandsProbabilityTextBoxBindingActionHandlerFacade(
                _macroLabelsListBox
            );
        }

        /**
         * @brief Tests acceptance of valid integer input within probability range
         * 
         * @tests Numeric character validation for probability textboxes
         * 
         * Validates that probability textboxes correctly accept numeric digits (0-9)
         * when the resulting value remains within the 0-100 probability range.
         * Tests multiple caret positions to ensure validation works correctly
         * regardless of insertion point within existing text.
         */
        private void _testProbabilityTextBoxAcceptsIntegers()
        {
            for (int i = 0; i < 2; i++)
            {
                var handler = _fixture();
                _macroLabelsListBox.Items.Add(_listBoxItem);
                _probabilityTextBox.Text = "1";
                _probabilityTextBox.CaretIndex = i;
                var textCompositionEvent = _generateTextCompositionEvent("2");
                _probabilityTextBox.RaiseEvent(textCompositionEvent);
                Debug.Assert(!textCompositionEvent.Handled);
            }
        }

        /**
         * @brief Tests rejection of non-numeric character input
         * 
         * @tests Input filtering for alphabetic/special characters
         * 
         * Verifies that probability textboxes correctly block alphabetic and special
         * characters, maintaining numeric-only content. Tests multiple insertion
         * points to ensure consistent rejection regardless of caret position.
         * 
         */
        private void _testProbabilityTextBoxRejectsNonIntegers()
        {
            for (int i = 0; i < 2; i++)
            {
                var handler = _fixture();
                _macroLabelsListBox.Items.Add(_listBoxItem);
                _probabilityTextBox.Text = "1";
                _probabilityTextBox.CaretIndex = i;
                var textCompositionEvent = _generateTextCompositionEvent("a");
                _probabilityTextBox.RaiseEvent(textCompositionEvent);
                Debug.Assert(textCompositionEvent.Handled);
            }
        }


        /**
         * @brief Tests rejection of input that would exceed maximum probability (100)
         * 
         * @tests Upper boundary enforcement during character-by-character input
         * 
         * Validates that probability textboxes prevent users from entering values
         * that would exceed the 100% maximum probability limit. Tests all possible
         * insertion points in a near-limit value to ensure comprehensive boundary
         * protection.
         */
        private void _testProbabilityTextBoxRejectsGreaterThan100()
        {
            for (int i = 0; i < 3; i++)
            {
                var handler = _fixture();
                _macroLabelsListBox.Items.Add(_listBoxItem);
                _probabilityTextBox.Text = "10";
                _probabilityTextBox.CaretIndex = i;
                var textCompositionEvent = _generateTextCompositionEvent("1");
                _probabilityTextBox.RaiseEvent(textCompositionEvent);
                Debug.Assert(textCompositionEvent.Handled);
            }
        }

        /**
         * @brief Tests acceptance of valid integer values via clipboard paste
         * 
         * @tests Clipboard paste validation for legitimate probability values
         * 
         * Verifies that probability textboxes accept pasted numeric values that
         * fall within the valid 0-100 range. Tests multiple valid values to ensure
         * the paste validation system correctly allows legitimate data insertion
         * from external sources.
         */
        private void _testProbabilityTextBoxAcceptsIntegerPaste()
        {
            List<int> pasteValues = [10, 20, 30];
            for (int i = 0; i < pasteValues.Count; i++)
            {
                var pasteValue = pasteValues[i];
                var handler = _fixture();
                _macroLabelsListBox.Items.Add(_listBoxItem);
                var dataObjectEvent = _generateDataObjectPastingEvent(pasteValue.ToString());
                _probabilityTextBox.RaiseEvent(dataObjectEvent);
                Debug.Assert(!dataObjectEvent.CommandCancelled);
            }
        }

        /**
         * @brief Tests rejection of non-numeric content via clipboard paste
         * 
         * @tests Clipboard paste filtering for invalid character data
         * 
         * Validates that probability textboxes block paste operations containing
         * alphabetic or special characters. Ensures that invalid data cannot bypass
         * input validation through clipboard operations.
         */
        private void _testProbabilityTextBoxRejectsNonIntegerPaste()
        {
            var handler = _fixture();
            _macroLabelsListBox.Items.Add(_listBoxItem);
            var dataObjectEvent = _generateDataObjectPastingEvent("ab");
            _probabilityTextBox.RaiseEvent(dataObjectEvent);
            Debug.Assert(dataObjectEvent.CommandCancelled);
        }


        /**
         * @brief Tests rejection of paste operations exceeding maximum probability
         * 
         * @tests Upper boundary enforcement for clipboard paste operations
         * 
         * Verifies that probability textboxes block paste operations that would
         * result in values exceeding the 100% maximum probability. Ensures paste
         * operations undergo the same boundary validation as typed input.
         */
        private void _testProbabilityTextBoxRejectsIntegerPasteGreaterThan100()
        {
            var handler = _fixture();
            _macroLabelsListBox.Items.Add(_listBoxItem);
            var dataObjectEvent = _generateDataObjectPastingEvent("101");
            _probabilityTextBox.RaiseEvent(dataObjectEvent);
            Debug.Assert(dataObjectEvent.CommandCancelled);
        }

        /**
         * @brief Executes comprehensive probability textbox validation test suite
         * 
         * @tests Complete validation system for probability textbox interactions
         * 
         * Runs the full battery of probability textbox validation tests covering
         * character input, boundary enforcement, and clipboard operations. Provides
         * end-to-end validation of the probability input system integrated with
         * ListBox item containers.
         */
        public void Run()
        {
            _testProbabilityTextBoxAcceptsIntegers();
            _testProbabilityTextBoxRejectsNonIntegers();
            _testProbabilityTextBoxRejectsGreaterThan100();
            _testProbabilityTextBoxAcceptsIntegerPaste();
            _testProbabilityTextBoxRejectsNonIntegerPaste();
            _testProbabilityTextBoxRejectsIntegerPasteGreaterThan100();
        }
    }


    public class WindowSaveLoadMenuHandlersTestSuite
    {
        public void Run()
        {
            new WindowSaveMenuActionHandlerTests().Run();
            new WindowLoadMenuActionHandlerTests().Run();
            new WindowLoadMenuElementActionHandlerTests().Run();
            new WindowAddMacroCommandActionHandlerTests().Run();
            new WindowRemoveMacroCommandActionHandlerTests().Run();
            new WindowClearMacroCommandActionHandlerTests().Run();
            new WindowMacroDisplayLoadingActionHandlerTests().Run();
            new WindowMacroCommandLabelSavingActionHandlerTests().Run();
            new WindowMacroCommandsSaveStateActionHandlerTests().Run();
            new WindowMacroCommandsDisplayActionHandlerTests().Run();
            new WindowMacroCommandsAddingActionHandlerTests().Run();
            new WindowMacroCommandsRemovingActionHandlerTests().Run();
            new WindowMacroCommandsRemoveButtonAccessActionHandlerTests().Run();
            new WindowMacroCommandsProbabilityTextBoxBindingActionHandlerTests().Run();
        }
    }
}
