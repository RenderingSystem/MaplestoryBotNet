using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


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

        private ListBox _macroCommandsListBox;

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
            _macroCommandsListBox = listBox;
            _macroDataSerializer = macroDataSerializer;
            _windowSaveMenuModifier = windowSaveMenuHandler;
            _initialDirectory = null;
            _saveButton.Click += OnEvent;
        }

        private MacroData _getListBoxMacroData()
        {
            var macroData = new MacroData();
            var commands = new string[_macroCommandsListBox.Items.Count];
            for (int i = 0; i < _macroCommandsListBox.Items.Count; i++)
            {
                commands[i] = ((ComboBox)_macroCommandsListBox.Items[i]).Text;
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
            ListBox macroCommandsListBox
        )
        {
            _saveMenuActionHandler = new WindowSaveMenuActionHandler(
                saveButton,
                macroCommandsListBox,
                new MacroDataSerializer(),
                new WindowSaveMenuModifier(
                    new WindowSaveFileDialog(
                        "Save Macro",
                        "JSON files (*.json)|*.json",
                        ".json"
                    )
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

        private ListBox _macroCommandsListBox;

        private AbstractComboBoxFactory _comboBoxFactory;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        private AbstractMacroDataDeserializer _macroDataDeserializer;

        private AbstractWindowStateModifier _windowLoadMenuModifier;

        private string? _initialDirectory;

        public WindowLoadMenuActionHandler(
            Button saveButton,
            ListBox macroCommandsListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry,
            AbstractComboBoxFactory comboBoxFactory,
            AbstractMacroDataDeserializer macroDataDeserializer,
            AbstractWindowStateModifier windowLoadMenuModifier
        )
        {
            _saveButton = saveButton;
            _macroCommandsListBox = macroCommandsListBox;
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
                _macroCommandsListBox.Items.Clear();
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
            _macroCommandsListBox.Items.Add(comboBox);
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
            ListBox macroCommandsListBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _loadMenuActionHandler = new WindowLoadMenuActionHandler(
                loadButton,
                macroCommandsListBox,
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
        private ListBox _macroCommandsListBox;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        private AbstractComboBoxFactory _comboBoxFactory;

        public WindowAddMacroCommandModifier(
            ListBox macroCommandsListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry,
            AbstractComboBoxFactory comboBoxFactory
        )
        {
            _macroCommandsListBox = macroCommandsListBox;
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            _comboBoxFactory = comboBoxFactory;
        }

        public override void Modify(object? value)
        {
            var comboBox = _comboBoxFactory.Create();
            var parameters = new WindowComboBoxScaleActionHandlerParameters(comboBox);
            _comboBoxPopupScaleRegistry.RegisterHandler(parameters);
            var selectedIndex = _macroCommandsListBox.SelectedIndex >= 0 ?
                _macroCommandsListBox.SelectedIndex + 1 :
                _macroCommandsListBox.Items.Count;
            _macroCommandsListBox.Items.Insert(selectedIndex, comboBox);
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
            ListBox macroCommandsListBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _addMacroCommandActionHandler = new WindowAddMacroCommandActionHandler(
                addButton,
                new WindowAddMacroCommandModifier(
                    macroCommandsListBox,
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
        private ListBox _macroCommandsListBox;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        public WindowRemoveMacroCommandModifier(
            ListBox macroCommandsListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _macroCommandsListBox = macroCommandsListBox;
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
        }

        public override void Modify(object? value)
        {
            if (_macroCommandsListBox.Items.Count > 0)
            {
                var selectedIndex = _macroCommandsListBox.SelectedIndex >= 0 ?
                    _macroCommandsListBox.SelectedIndex :
                    _macroCommandsListBox.Items.Count - 1;
                var selectedComboBox = (ComboBox)_macroCommandsListBox.Items[selectedIndex];
                _macroCommandsListBox.Items.RemoveAt(selectedIndex);
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
            ListBox macroCommandsListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _removeMacroCommandActionHandler = new WindowRemoveMacroCommandActionHandler(
                removeButton,
                new WindowRemoveMacroCommandModifier(
                    macroCommandsListBox,
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
        private ListBox _macroCommandsListBox;
        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;
        public WindowClearMacroCommandsModifier(
            ListBox macroCommandsListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _macroCommandsListBox = macroCommandsListBox;
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
        }

        public override void Modify(object? value)
        {
            _comboBoxPopupScaleRegistry.ClearHandlers();
            _macroCommandsListBox.Items.Clear();
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
            ListBox macroCommandsListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _clearMacroCommandsActionHandler = new WindowClearMacroCommandsActionHandler(
                clearButton,
                new WindowClearMacroCommandsModifier(
                    macroCommandsListBox,
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


    public class WindowMacroDisplayLoadingModifierParameters
    {
        public AbstractMapModel ElementModel = new MapModel();
    }


    public class WindowMacroDisplayLoadingModifier : AbstractWindowStateModifier
    {
        private AbstractWindowMapEditMenuState _menuState;

        private AbstractMacroListBoxItemTemplateFactory _listBoxItemFactory;

        private AbstractMacroListBoxItemFormatter _listBoxItemFormatter;

        private ListBox _macroLabelsListBox;

        private TextBox _macroLabelTextBox;

        public WindowMacroDisplayLoadingModifier(
            ListBox macroLabelsListBox,
            TextBox macroLabelTextBox,
            AbstractWindowMapEditMenuState menuState,
            AbstractMacroListBoxItemFormatter listBoxItemFormatter,
            AbstractMacroListBoxItemTemplateFactory listBoxItemFactory
        )
        {
            _macroLabelsListBox = macroLabelsListBox;
            _macroLabelTextBox = macroLabelTextBox;
            _menuState = menuState;
            _listBoxItemFormatter = listBoxItemFormatter;
            _listBoxItemFactory = listBoxItemFactory;
        }

        private void _formatNewListBoxItem(
            FrameworkElement newListBoxItem,
            MinimapPointMacros minimapPointMacros
        )
        {
            _listBoxItemFormatter.Format(
                new WindowMacroListBoxItemFormatterParameters
                {
                    Element = newListBoxItem,
                    ElementStrings = [
                        minimapPointMacros.MacroName,
                        minimapPointMacros.MacroChance.ToString()
                    ]
                }
            );
        }

        public override void Modify(object? value)
        {
            if (value is not WindowMacroDisplayLoadingModifierParameters parameters)
            {
                return;
            }
            var selected = _menuState.Selected();
            if (selected is not FrameworkElement selectedElement)
            {
                return;
            }
            var minimapPoint = parameters.ElementModel.FindName(selectedElement.Name);
            if (minimapPoint == null)
            {
                return;
            }
            _macroLabelsListBox.Items.Clear();
            _macroLabelsListBox.Tag = minimapPoint;
            _macroLabelTextBox.Text = minimapPoint.PointData.PointName;
            for (int i = 0; i < minimapPoint.PointData.Commands.Count; i++)
            {
                var newListBoxItem = _listBoxItemFactory.Create();
                _formatNewListBoxItem(newListBoxItem, minimapPoint.PointData.Commands[i]);
                _macroLabelsListBox.Items.Add(newListBoxItem);
            }
            if (_macroLabelsListBox.Items.Count > 0)
            {
                _macroLabelsListBox.SelectedIndex = 0;
                _macroLabelsListBox.Focus();
            }
        }
    }


    public class WindowMacroDisplayLoadingActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _macroWindow;

        private AbstractWindowStateModifier _macroCommandsDisplayLoadingModifier;

        private AbstractMapModel? _mapModel;

        public WindowMacroDisplayLoadingActionHandler(
            AbstractSystemWindow macroWindow,
            AbstractWindowStateModifier macroCommandsDisplayLoadingModifier
        )
        {
            _macroWindow = macroWindow;
            ((Window?)_macroWindow.GetWindow())!.IsVisibleChanged += OnDependencyEvent;
            _macroCommandsDisplayLoadingModifier = macroCommandsDisplayLoadingModifier;
            _mapModel = null;
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (!_macroWindow.Visible())
            {
                return;
            }
            if (_mapModel == null)
            {
                return;
            }
            _macroCommandsDisplayLoadingModifier.Modify(
                new WindowMacroDisplayLoadingModifierParameters
                {
                    ElementModel = _mapModel
                }
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsDisplayLoadingModifier;
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (dataType == SystemInjectType.MapModel && data is MapModel mapModel)
            {
                _mapModel = mapModel;
            }
        }
    }


    public class WindowMacroDisplayLoadingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _macroCommandsDisplayLoadingActionHandler;

        public WindowMacroDisplayLoadingActionHandlerFacade(
            AbstractSystemWindow macroWindow,
            ListBox macroLabelListBox,
            TextBox macroLabelTextBox,
            FrameworkElement listBoxItemTemplate,
            AbstractWindowMapEditMenuState menuState
        )
        {
            _macroCommandsDisplayLoadingActionHandler = new WindowMacroDisplayLoadingActionHandler(
                macroWindow,
                new WindowMacroDisplayLoadingModifier(
                    macroLabelListBox,
                    macroLabelTextBox,
                    menuState,
                    new WindowMacroListBoxItemFormatter(),
                    new WindowMacroListBoxItemTemplateFactory(listBoxItemTemplate)
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsDisplayLoadingActionHandler.Modifier();
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _macroCommandsDisplayLoadingActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            _macroCommandsDisplayLoadingActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowMacroCommandLabelSavingModifierParameters
    {
        public FrameworkElement Element = new FrameworkElement();

        public int ElementIndex = 0;

        public MinimapPoint ElementPoint = new MinimapPoint();
    }


    public class WindowMacroCommandLabelSavingModifier : AbstractWindowStateModifier
    {
        public WindowMacroCommandLabelSavingModifier()
        {

        }

        private void _updateMinimapPointVariables(
            FrameworkElement macroLabelsListBoxElement,
            int elementIndex,
            MinimapPoint minimapPoint
        )
        {
            var minimapPointMacros = minimapPoint.PointData.Commands[elementIndex];
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
                if (stringTag == "MacroNameTag")
                {
                    minimapPointMacros.MacroName = childTextBox.Text;
                }
                if (stringTag == "ProbabilityTag")
                {
                    minimapPointMacros.MacroChance = Convert.ToInt32(childTextBox.Text);
                }
            }
        }

        public override void Modify(object? value)
        {
            if (value is WindowMacroCommandLabelSavingModifierParameters parameters)
            {
                _updateMinimapPointVariables(
                    parameters.Element,
                    parameters.ElementIndex,
                    parameters.ElementPoint
                );
            }
        }
    }


    public class WindowMacroCommandLabelSavingActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _macroWindow;

        private TextBox _macroLabelTextBox;

        private ListBox _macroLabelsListBox;

        private AbstractWindowStateModifier _macroCommandsDisplaySavingModifier;

        private MapModel? _mapModel;

        public WindowMacroCommandLabelSavingActionHandler(
            AbstractSystemWindow macroWindow,
            TextBox macroLabelTextBox,
            ListBox macroLabelsListBox,
            AbstractWindowStateModifier macroCommandsDisplaySavingModifier
        )
        {
            _macroWindow = macroWindow;
            _macroLabelTextBox = macroLabelTextBox;
            _macroLabelsListBox = macroLabelsListBox;
            _macroCommandsDisplaySavingModifier = macroCommandsDisplaySavingModifier;
            _mapModel = null;
            ((Window?)_macroWindow.GetWindow())!.IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsDisplaySavingModifier;
        }

        private void _updateTextDependencies()
        {
            var minimapPoint = (MinimapPoint)_macroLabelsListBox.Tag;
            string newText = _macroLabelTextBox.Text;
            if (string.IsNullOrEmpty(newText))
            {
                return;
            }
            minimapPoint.PointData.PointName = newText;
            foreach (var frameworkElement in minimapPoint.PointData.ElementTexts)
            {
                if (frameworkElement is TextBox textBox)
                {
                    textBox.Text = newText;
                }
                if (frameworkElement is TextBlock textBlock)
                {
                    textBlock.Text = newText;
                }
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (_macroWindow.Visible())
            {
                return;
            }
            if (_mapModel == null)
            {
                return;
            }
            var minimapPoint = (MinimapPoint)_macroLabelsListBox.Tag;
            for (int i = 0; i < _macroLabelsListBox.Items.Count; i++)
            {
                var element = (FrameworkElement)_macroLabelsListBox.Items[i];
                _macroCommandsDisplaySavingModifier.Modify(
                    new WindowMacroCommandLabelSavingModifierParameters
                    {
                        Element = element,
                        ElementIndex = i,
                        ElementPoint = minimapPoint
                    }
                );
            }
            _updateTextDependencies();
            _macroLabelsListBox.SelectedIndex = -1;
            _macroLabelsListBox.Focus();
            _mapModel.Edit(minimapPoint);
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (dataType == SystemInjectType.MapModel && data is MapModel mapModel)
            {
                _mapModel = mapModel;
            }
        }
    }


    public class WindowMacroCommandLabelSavingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _macroCommandsDisplaySavingActionHandler;

        public WindowMacroCommandLabelSavingActionHandlerFacade(
            AbstractSystemWindow macroWindow,
            TextBox macroLabelTextBox,
            ListBox macroLabelsListBox
        )
        {
            _macroCommandsDisplaySavingActionHandler = new WindowMacroCommandLabelSavingActionHandler(
                macroWindow,
                macroLabelTextBox,
                macroLabelsListBox,
                new WindowMacroCommandLabelSavingModifier()
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsDisplaySavingActionHandler.Modifier();
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            _macroCommandsDisplaySavingActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _macroCommandsDisplaySavingActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowMacroCommandsSaveStateModifier : AbstractWindowStateModifier
    {
        private ListBox _macroLabelsListBox;

        private ListBox _macroCommandsListBox;

        private int _previousIndex = -1;

        public WindowMacroCommandsSaveStateModifier(
            ListBox macroLabelsListBox,
            ListBox macroCommandsListBox
        )
        {
            _macroLabelsListBox = macroLabelsListBox;
            _macroCommandsListBox = macroCommandsListBox;
        }

        public List<string> _extractMacroCommands()
        {
            var macroCommands = new List<string>();
            for (int i = 0; i < _macroCommandsListBox.Items.Count; i++)
            {
                var comboBox = (ComboBox)_macroCommandsListBox.Items[i];
                macroCommands.Add(comboBox.Text);
            }
            return macroCommands;
        }

        public override void Modify(object? value)
        {
            var minimapPoint = (MinimapPoint)_macroLabelsListBox.Tag;
            var selectedIndex = _macroLabelsListBox.SelectedIndex;
            if (_previousIndex >= 0)
            {
                var macroCommands = _extractMacroCommands();
                var minimapPointMacros = minimapPoint.PointData.Commands[_previousIndex];
                minimapPointMacros.MacroCommands = macroCommands;
            }
            _previousIndex = selectedIndex;
        }
    }


    public class WindowMacroCommandsSaveStateActionHandler : AbstractWindowActionHandler
    {
        private ListBox _macroLabelsListBox;

        private AbstractWindowStateModifier _macroCommandsSaveStateModifier;

        public WindowMacroCommandsSaveStateActionHandler(
            ListBox macroLabelsListBox,
            AbstractWindowStateModifier macroCommandsSaveStateModifier
        )
        {
            _macroLabelsListBox = macroLabelsListBox;
            _macroCommandsSaveStateModifier = macroCommandsSaveStateModifier;
            _macroLabelsListBox.SelectionChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsSaveStateModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandsSaveStateModifier.Modify(null);
        }
    }


    public class WindowMacroCommandsSaveStateActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _macroCommandsSaveStateActionHandler;

        public WindowMacroCommandsSaveStateActionHandlerFacade(
            ListBox macroLabelsListBox,
            ListBox macroCommandsListBox
        )
        {
            _macroCommandsSaveStateActionHandler = new WindowMacroCommandsSaveStateActionHandler(
                macroLabelsListBox,
                new WindowMacroCommandsSaveStateModifier(
                    macroLabelsListBox, macroCommandsListBox
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsSaveStateActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandsSaveStateActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMacroCommandsDisplayModifier : AbstractWindowStateModifier
    {
        public WindowMacroCommandsDisplayModifier(
        )
        {
        }

        public override void Modify(object? value)
        {
        }
    }


    public class WindowMacroCommandsDisplayActionHandler : AbstractWindowActionHandler
    {
        private ListBox _macroLabelsListBox;

        private ListBox _macroCommandsListBox;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        private AbstractComboBoxFactory _comboBoxFactory;

        private AbstractWindowStateModifier _macroCommandsDisplayModifier;

        public WindowMacroCommandsDisplayActionHandler(
            ListBox macroLabelsListBox,
            ListBox macroCommandsListBox,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry,
            AbstractComboBoxFactory comboBoxFactory,
            AbstractWindowStateModifier macroCommandsDisplayModifier
        )
        {
            _macroLabelsListBox = macroLabelsListBox;
            _macroCommandsListBox = macroCommandsListBox;
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            _comboBoxFactory = comboBoxFactory;
            _macroCommandsDisplayModifier = macroCommandsDisplayModifier;
            _macroLabelsListBox.SelectionChanged += OnEvent;
        }

        private void _setupComboBoxItem(string macroCommand)
        {
            var comboBox = _comboBoxFactory.Create();
            comboBox.Text = macroCommand;
            var parameters = new WindowComboBoxScaleActionHandlerParameters(comboBox);
            _comboBoxPopupScaleRegistry.RegisterHandler(parameters);
            _macroCommandsListBox.Items.Add(comboBox);
        }

        private void _setupMacroData(int selectedIndex)
        {
            if (selectedIndex >= 0)
            {
                var minimapPoint = (MinimapPoint)_macroLabelsListBox.Tag;
                var minimapPointMacros = minimapPoint.PointData.Commands[selectedIndex];
                for (int i = 0; i < minimapPointMacros.MacroCommands.Count; i++)
                {
                    var macroCommand = minimapPointMacros.MacroCommands[i];
                    _setupComboBoxItem(macroCommand);
                }
            }
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            var selectedIndex = _macroLabelsListBox.SelectedIndex;
            _comboBoxPopupScaleRegistry.ClearHandlers();
            _macroCommandsListBox.Items.Clear();
            _setupMacroData(selectedIndex);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsDisplayModifier;
        }
    }


    public class WindowMacroCommandsDisplayActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _macroCommandsDisplayActionHandler;

        public WindowMacroCommandsDisplayActionHandlerFacade(
            ListBox macroLabelsListBox,
            ListBox macroCommandsListBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _macroCommandsDisplayActionHandler = new WindowMacroCommandsDisplayActionHandler(
                macroLabelsListBox,
                macroCommandsListBox,
                comboBoxPopupScaleRegistry,
                new ComboBoxTemplateFactory(comboBoxTemplate),
                new WindowMacroCommandsDisplayModifier()
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandsDisplayActionHandler.OnEvent(sender, e);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsDisplayActionHandler.Modifier();
        }
    }


    public class WindowMacroCommandsAddingModifier : AbstractWindowStateModifier
    {
        private ListBox _macroLabelsListBox;

        private AbstractMacroListBoxItemTemplateFactory _listBoxItemFactory;

        private AbstractMacroListBoxItemFormatter _listBoxItemFormatter;

        public WindowMacroCommandsAddingModifier(
            ListBox macroLabelsListBox,
            AbstractMacroListBoxItemTemplateFactory listBoxItemFactory,
            AbstractMacroListBoxItemFormatter listBoxItemFormatter
        )
        {
            _macroLabelsListBox = macroLabelsListBox;
            _listBoxItemFactory = listBoxItemFactory;
            _listBoxItemFormatter = listBoxItemFormatter;
        }

        private void _formatNewListBoxItem(
            FrameworkElement newListBoxItem, string macroName
        )
        {
            _listBoxItemFormatter.Format(
                new WindowMacroListBoxItemFormatterParameters
                {
                    Element = newListBoxItem,
                    ElementStrings = [macroName, "0"]
                }
            );
        }

        private void _addNewListBoxItem(FrameworkElement newListBoxItem)
        {
            _macroLabelsListBox.Items.Add(newListBoxItem);
            _macroLabelsListBox.SelectedIndex = _macroLabelsListBox.Items.Count - 1;
            _macroLabelsListBox.Focus();
        }

        private string _generateMacroName()
        {
            var minimapPoint = (MinimapPoint)_macroLabelsListBox.Tag;
            var existingElements = new HashSet<string>();
            var elementCount = minimapPoint.PointData.Commands.Count;
            for (int i = 0; i < minimapPoint.PointData.Commands.Count; i++)
            {
                existingElements.Add(minimapPoint.PointData.Commands[i].MacroName);
            }
            while (existingElements.Contains("Macro " + elementCount))
            {
                elementCount++;
            }
            return "Macro " + elementCount;
        }

        private void _addNewMinimapPointMacro(string macroName)
        {
            var minimapPoint = (MinimapPoint)_macroLabelsListBox.Tag;
            minimapPoint.PointData.Commands.Add(
                new MinimapPointMacros
                {
                    MacroChance = 0,
                    MacroName = macroName,
                    MacroCommands = []
                }
            );
        }

        public override void Modify(object? value)
        {
            var newListBoxItem = _listBoxItemFactory.Create();
            var macroName = _generateMacroName();
            _addNewMinimapPointMacro(macroName);
            _formatNewListBoxItem(newListBoxItem, macroName);
            _addNewListBoxItem(newListBoxItem);
        }
    }


    public class WindowMacroCommandsAddingActionHandler : AbstractWindowActionHandler
    {
        private Button _macroAddButton;

        private AbstractWindowStateModifier _macroCommandsAddingModifier;

        public WindowMacroCommandsAddingActionHandler(
            Button macroAddButton,
            AbstractWindowStateModifier macroCommandsAddingModifier
        )
        {
            _macroAddButton = macroAddButton;
            _macroCommandsAddingModifier = macroCommandsAddingModifier;
            _macroAddButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsAddingModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandsAddingModifier.Modify(null);
        }
    }


    public class WindowMacroCommandsAddingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _macroCommandsAddingActionHandler;

        public WindowMacroCommandsAddingActionHandlerFacade(
            Button macroAddButton,
            ListBox macroLabelsListBox,
            FrameworkElement pointMacroTemplate
        )
        {
            _macroCommandsAddingActionHandler = new WindowMacroCommandsAddingActionHandler(
                macroAddButton,
                new WindowMacroCommandsAddingModifier(
                    macroLabelsListBox,
                    new WindowMacroListBoxItemTemplateFactory(pointMacroTemplate),
                    new WindowMacroListBoxItemFormatter()
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsAddingActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandsAddingActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMacroCommandsRemovingModifier : AbstractWindowStateModifier
    {
        private ListBox _macroLabelsListBox;

        public WindowMacroCommandsRemovingModifier(
            ListBox macroLabelsListBox
        )
        {
            _macroLabelsListBox = macroLabelsListBox;
        }

        public override void Modify(object? value)
        {
            var selectedIndex = _macroLabelsListBox.SelectedIndex;
            var minimapPoint = (MinimapPoint)_macroLabelsListBox.Tag;
            if (selectedIndex != -1)
            {
                _macroLabelsListBox.Items.RemoveAt(selectedIndex);
                minimapPoint.PointData.Commands.RemoveAt(selectedIndex);
                _macroLabelsListBox.SelectedIndex = Math.Min(
                    _macroLabelsListBox.Items.Count - 1, selectedIndex
                );
                _macroLabelsListBox.Focus();
            }
        }
    }


    public class WindowMacroCommandsRemovingActionHandler : AbstractWindowActionHandler
    {
        private Button _macroRemoveButton;

        private AbstractWindowStateModifier _macroCommandsRemovingModifier;

        public WindowMacroCommandsRemovingActionHandler(
            Button macroRemoveButton,
            AbstractWindowStateModifier macroCommandsRemovingModifier
        )
        {
            _macroRemoveButton = macroRemoveButton;
            _macroCommandsRemovingModifier = macroCommandsRemovingModifier;
            _macroRemoveButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsRemovingModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandsRemovingModifier.Modify(null);
        }
    }


    public class WindowMacroCommandsRemovingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _macroCommandsRemovingActionHandler;

        public WindowMacroCommandsRemovingActionHandlerFacade(
            Button removeButton, ListBox macroLabelsListBox
        )
        {
            _macroCommandsRemovingActionHandler = new WindowMacroCommandsRemovingActionHandler(
                removeButton, new WindowMacroCommandsRemovingModifier(macroLabelsListBox)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsRemovingActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandsRemovingActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowMacroCommandsRemoveButtonAccessModifier : AbstractWindowStateModifier
    {
        private ListBox _macroLabelsListBox;

        private Button _removeButton;

        public WindowMacroCommandsRemoveButtonAccessModifier(
            ListBox macroLabelsListBox,
            Button removeButton
        )
        {
            _macroLabelsListBox = macroLabelsListBox;
            _removeButton = removeButton;
        }

        public override void Modify(object? value)
        {
            _removeButton.IsEnabled = _macroLabelsListBox.Items.Count > 1;
        }
    }


    public class WindowMacroCommandsRemoveButtonAccessActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _macroWindow;

        private Button _addButton;

        private Button _removeButton;

        private AbstractWindowStateModifier _macroCommandsRemoveButtonAccessModifier;

        public WindowMacroCommandsRemoveButtonAccessActionHandler(
            AbstractSystemWindow macroWindow,
            ListBox macroLabelsListBox,
            Button addButton,
            Button removeButton,
            AbstractWindowStateModifier macroCommandsRemoveButtonAccessModifier
        )
        {
            _macroWindow = macroWindow;
            _addButton = addButton;
            _removeButton = removeButton;
            _macroCommandsRemoveButtonAccessModifier = macroCommandsRemoveButtonAccessModifier;
            ((Window?)_macroWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
            _removeButton.Click += OnEvent;
            _addButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsRemoveButtonAccessModifier;
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            _macroCommandsRemoveButtonAccessModifier.Modify(null);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandsRemoveButtonAccessModifier.Modify(null);
        }
    }


    public class WindowMacroCommandsRemoveButtonAccessActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _macroCommandsRemoveButtonAccessActionHandler;

        public WindowMacroCommandsRemoveButtonAccessActionHandlerFacade(
            AbstractSystemWindow macroWindow,
            ListBox macroLabelsListBox,
            Button addButton,
            Button removeButton
        )
        {
            _macroCommandsRemoveButtonAccessActionHandler = (
                new WindowMacroCommandsRemoveButtonAccessActionHandler(
                    macroWindow,
                    macroLabelsListBox,
                    addButton,
                    removeButton,
                    new WindowMacroCommandsRemoveButtonAccessModifier(
                        macroLabelsListBox,
                        removeButton
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandsRemoveButtonAccessActionHandler.Modifier();
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            _macroCommandsRemoveButtonAccessActionHandler.OnDependencyEvent(sender, e);
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandsRemoveButtonAccessActionHandler.OnEvent(sender, e);
        }
    }
}
