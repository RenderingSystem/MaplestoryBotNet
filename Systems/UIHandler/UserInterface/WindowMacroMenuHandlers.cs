using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows.Controls;


namespace MaplestoryBotNet.Systems.UIHandler
{
    public class WindowSaveMenuModifierParameters
    {
        public string InitialDirectory = "";

        public string SaveContent = "";
    }


    public class WindowSaveMenuModifier : AbstractWindowStateModifier
    {
        private AbstractSaveFileDialog _saveFileDialog;

        public WindowSaveMenuModifier(AbstractSaveFileDialog saveFileDialog)
        {
            _saveFileDialog = saveFileDialog;
        }

        public override void Modify(object? value)
        {
            if (value is WindowSaveMenuModifierParameters parameters)
            {
                _saveFileDialog.Prompt(
                    parameters.InitialDirectory,
                    parameters.SaveContent
                );
            }
        }
    }


    public class WindowSaveMenuActionHandler : AbstractWindowActionHandler
    {
        private Button _saveButton;

        private ListBox _macroListBox;

        private AbstractMacroDataSerializer _macroDataSerializer;

        private AbstractWindowStateModifier _windowSaveMenuModifier;

        private string? _initialDirectory;

        public WindowSaveMenuActionHandler(
            Button saveButton,
            ListBox listBox,
            AbstractMacroDataSerializer macroDataSerializer,
            AbstractWindowStateModifier windowSaveMenuHandler
        )
        {
            _saveButton = saveButton;
            _macroListBox = listBox;
            _macroDataSerializer = macroDataSerializer;
            _windowSaveMenuModifier = windowSaveMenuHandler;
            _initialDirectory = null;
            _saveButton.Click += OnEvent;
        }

        private MacroData _getListBoxMacroData()
        {
            var macroData = new MacroData();
            var commands = new string[_macroListBox.Items.Count];
            for (int i = 0; i < _macroListBox.Items.Count; i++)
            {
                commands[i] = ((ComboBox)_macroListBox.Items[i]).Text;
            }
            macroData.Macro = commands;
            return macroData;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            var initialDirectory = _initialDirectory ?? "";
            var macroData = _getListBoxMacroData();
            var parameters = new WindowSaveMenuModifierParameters
            {
                InitialDirectory = initialDirectory,
                SaveContent = _macroDataSerializer.SerializeMacroData(macroData)
            };
            _windowSaveMenuModifier.Modify(parameters);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowSaveMenuModifier;
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (
                dataType == SystemInjectType.ConfigurationUpdate
                && data is MaplestoryBotConfiguration configuration
            )
            {
                _initialDirectory = configuration.MacroDirectory;
            }
        }
    }


    public class WindowSaveMenuActionHandlerFacade : AbstractWindowActionHandler
    {
        private WindowSaveMenuActionHandler _saveMenuActionHandler;
        public WindowSaveMenuActionHandlerFacade(
            Button saveButton,
            ListBox macroListBox
        )
        {
            _saveMenuActionHandler = new WindowSaveMenuActionHandler(
                saveButton,
                macroListBox,
                new MacroDataSerializer(),
                new WindowSaveMenuModifier(
                    new WindowSaveFileDialog("Save Macro", "JSON files (*.json)|*.json", ".json")
                )
            );
        }
        public override void OnEvent(object? sender, EventArgs e)
        {
            _saveMenuActionHandler.OnEvent(sender, e);
        }
        public override AbstractWindowStateModifier Modifier()
        {
            return _saveMenuActionHandler.Modifier();
        }
        public override void Inject(SystemInjectType dataType, object? data)
        {
            _saveMenuActionHandler.Inject(dataType, data);
        }
    }


    public class WindowLoadMenuModifierParameters
    {
        public string InitialDirectory = "";
    }


    public class WindowLoadMenuModifier : AbstractWindowStateModifier
    {
        private AbstractLoadFileDialog _loadFileDialog;

        private string _loadedText;

        public WindowLoadMenuModifier(
            AbstractLoadFileDialog loadFileDialog
        )
        {
            _loadFileDialog = loadFileDialog;
            _loadedText = "";
        }

        public override void Modify(object? value)
        {
            if (value is WindowLoadMenuModifierParameters parameters)
            {
                _loadedText = _loadFileDialog.Prompt(parameters.InitialDirectory);
            }
        }

        public override object? State(int stateType)
        {
            return _loadedText;
        }
    }


    public class WindowLoadMenuActionHandler : AbstractWindowActionHandler
    {
        private Button _saveButton;

        private ListBox _macroListBox;

        private AbstractComboBoxFactory _comboBoxFactory;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        private AbstractMacroDataDeserializer _macroDataDeserializer;

        private AbstractWindowStateModifier _windowLoadMenuModifier;

        private string? _initialDirectory;

        public WindowLoadMenuActionHandler(
            Button saveButton,
            ListBox macroListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry,
            AbstractComboBoxFactory comboBoxFactory,
            AbstractMacroDataDeserializer macroDataDeserializer,
            AbstractWindowStateModifier windowLoadMenuModifier
        )
        {
            _saveButton = saveButton;
            _macroListBox = macroListBox;
            _comboBoxFactory = comboBoxFactory;
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            _macroDataDeserializer = macroDataDeserializer;
            _windowLoadMenuModifier = windowLoadMenuModifier;
            _saveButton.Click += OnEvent;
            _initialDirectory = null;
        }

        private void _loadMacroDataToListBox(string loadedText)
        {
            if (loadedText != "")
            {
                var macroData = _macroDataDeserializer.DeserializeMacroData(loadedText);
                _macroListBox.Items.Clear();
                _comboBoxPopupScaleRegistry.ClearHandlers();
                foreach (var command in macroData.Macro)
                {
                    _loadComboBoxItem(command);
                }
            }
        }

        private void _loadComboBoxItem(string command)
        {
            var comboBox = _comboBoxFactory.Create();
            comboBox.Text = command;
            var parameters = new WindowComboBoxScaleActionHandlerParameters(comboBox);
            _comboBoxPopupScaleRegistry.RegisterHandler(parameters);
            _macroListBox.Items.Add(comboBox);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            var initialDirectory = _initialDirectory ?? "";
            var parameters = new WindowLoadMenuModifierParameters
            {
                InitialDirectory = initialDirectory
            };
            _windowLoadMenuModifier.Modify(parameters);
            _loadMacroDataToListBox((string)_windowLoadMenuModifier.State(0)!);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowLoadMenuModifier;
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (
                dataType == SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _initialDirectory = configuration.MacroDirectory;
            }
        }
    }


    public class WindowLoadMenuActionHandlerFacade : AbstractWindowActionHandler
    {
        private WindowLoadMenuActionHandler _loadMenuActionHandler;

        public WindowLoadMenuActionHandlerFacade(
            Button loadButton,
            ListBox macroListBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _loadMenuActionHandler = new WindowLoadMenuActionHandler(
                loadButton,
                macroListBox,
                comboBoxPopupScaleRegistry,
                new ComboBoxTemplateFactory(comboBoxTemplate),
                new MacroDataDeserializer(),
                new WindowLoadMenuModifier(
                    new WindowLoadFileDialog("Load Macro", "JSON files (*.json)|*.json")
                )
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _loadMenuActionHandler.OnEvent(sender, e);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadMenuActionHandler.Modifier();
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _loadMenuActionHandler.Inject(dataType, data);
        }
    }


    public class WindowAddMacroCommandModifier : AbstractWindowStateModifier
    {
        private ListBox _macroListBox;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        private AbstractComboBoxFactory _comboBoxFactory;

        public WindowAddMacroCommandModifier(
            ListBox macroListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry,
            AbstractComboBoxFactory comboBoxFactory
        )
        {
            _macroListBox = macroListBox;
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            _comboBoxFactory = comboBoxFactory;
        }

        public override void Modify(object? value)
        {
            var comboBox = _comboBoxFactory.Create();
            var parameters = new WindowComboBoxScaleActionHandlerParameters(comboBox);
            _comboBoxPopupScaleRegistry.RegisterHandler(parameters);
            var selectedIndex = _macroListBox.SelectedIndex >= 0 ?
                _macroListBox.SelectedIndex + 1 :
                _macroListBox.Items.Count;
            _macroListBox.Items.Insert(selectedIndex, comboBox);
        }
    }


    public class WindowAddMacroCommandActionHandler : AbstractWindowActionHandler
    {
        private Button _addButton;

        private AbstractWindowStateModifier _windowAddMacroCommandModifier;

        public WindowAddMacroCommandActionHandler(
            Button addButton,
            AbstractWindowStateModifier windowAddMacroCommandModifier
        )
        {
            _addButton = addButton;
            _windowAddMacroCommandModifier = windowAddMacroCommandModifier;
            _addButton.Click += OnEvent;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _windowAddMacroCommandModifier.Modify(null);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowAddMacroCommandModifier;
        }
    }


    public class WindowAddMacroCommandActionHandlerFacade : AbstractWindowActionHandler
    {
        private WindowAddMacroCommandActionHandler _addMacroCommandActionHandler;

        public WindowAddMacroCommandActionHandlerFacade(
            Button addButton,
            ListBox macroListBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _addMacroCommandActionHandler = new WindowAddMacroCommandActionHandler(
                addButton,
                new WindowAddMacroCommandModifier(
                    macroListBox,
                    comboBoxPopupScaleRegistry,
                    new ComboBoxTemplateFactory(comboBoxTemplate)
                )
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _addMacroCommandActionHandler.OnEvent(sender, e);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _addMacroCommandActionHandler.Modifier();
        }
    }


    public class WindowRemoveMacroCommandModifier : AbstractWindowStateModifier
    {
        private ListBox _macroListBox;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        public WindowRemoveMacroCommandModifier(
            ListBox macroListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _macroListBox = macroListBox;
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
        }

        public override void Modify(object? value)
        {
            if (_macroListBox.Items.Count > 0)
            {
                var selectedIndex = _macroListBox.SelectedIndex >= 0 ?
                    _macroListBox.SelectedIndex :
                    _macroListBox.Items.Count - 1;
                var selectedComboBox = (ComboBox)_macroListBox.Items[selectedIndex];
                _macroListBox.Items.RemoveAt(selectedIndex);
                _comboBoxPopupScaleRegistry.UnregisterHandler(selectedComboBox);
            }
        }
    }


    public class WindowRemoveMacroCommandActionHandler : AbstractWindowActionHandler
    {
        private Button _removeButton;
        private AbstractWindowStateModifier _windowRemoveMacroCommandModifier;
        public WindowRemoveMacroCommandActionHandler(
            Button removeButton,
            AbstractWindowStateModifier windowRemoveMacroCommandModifier
        )
        {
            _removeButton = removeButton;
            _windowRemoveMacroCommandModifier = windowRemoveMacroCommandModifier;
            _removeButton.Click += OnEvent;
        }
        public override void OnEvent(object? sender, EventArgs e)
        {
            _windowRemoveMacroCommandModifier.Modify(null);
        }
        public override AbstractWindowStateModifier Modifier()
        {
            return _windowRemoveMacroCommandModifier;
        }
    }


    public class WindowRemoveMacroCommandActionHandlerFacade : AbstractWindowActionHandler
    {
        private WindowRemoveMacroCommandActionHandler _removeMacroCommandActionHandler;

        public WindowRemoveMacroCommandActionHandlerFacade(
            Button removeButton,
            ListBox macroListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _removeMacroCommandActionHandler = new WindowRemoveMacroCommandActionHandler(
                removeButton,
                new WindowRemoveMacroCommandModifier(
                    macroListBox,
                    comboBoxPopupScaleRegistry
                )
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _removeMacroCommandActionHandler.OnEvent(sender, e);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _removeMacroCommandActionHandler.Modifier();
        }
    }


    public class WindowClearMacroCommandsModifier : AbstractWindowStateModifier
    {
        private ListBox _macroListBox;
        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;
        public WindowClearMacroCommandsModifier(
            ListBox macroListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _macroListBox = macroListBox;
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
        }

        public override void Modify(object? value)
        {
            _comboBoxPopupScaleRegistry.ClearHandlers();
            _macroListBox.Items.Clear();
        }
    }


    public class WindowClearMacroCommandsActionHandler : AbstractWindowActionHandler
    {
        private Button _clearButton;

        private AbstractWindowStateModifier _windowClearMacroCommandsModifier;

        public WindowClearMacroCommandsActionHandler(
            Button clearButton,
            AbstractWindowStateModifier windowClearMacroCommandsModifier
        )
        {
            _clearButton = clearButton;
            _windowClearMacroCommandsModifier = windowClearMacroCommandsModifier;
            _clearButton.Click += OnEvent;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _windowClearMacroCommandsModifier.Modify(null);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowClearMacroCommandsModifier;
        }
    }


    public class WindowClearMacroCommandsActionHandlerFacade : AbstractWindowActionHandler
    {
        private WindowClearMacroCommandsActionHandler _clearMacroCommandsActionHandler;

        public WindowClearMacroCommandsActionHandlerFacade(
            Button clearButton,
            ListBox macroListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _clearMacroCommandsActionHandler = new WindowClearMacroCommandsActionHandler(
                clearButton,
                new WindowClearMacroCommandsModifier(
                    macroListBox,
                    comboBoxPopupScaleRegistry
                )
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _clearMacroCommandsActionHandler.OnEvent(sender, e);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _clearMacroCommandsActionHandler.Modifier();
        }
    }
}
