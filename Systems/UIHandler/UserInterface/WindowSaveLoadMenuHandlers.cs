using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;


namespace MaplestoryBotNet.Systems.UIHandler
{
    public abstract class AbstractSaveFileDialog
    {
        public abstract void Prompt(string initialDirectory, string saveContent);
    }


    public class WindowSaveFileDialog : AbstractSaveFileDialog
    {
        private string _title;

        private string _filter;

        private string _defaultExt;

        public WindowSaveFileDialog(string title, string filter, string defaultExt)
        {
            _title = title;
            _filter = filter;
            _defaultExt = defaultExt;
        }

        public override void Prompt(string initialDirectory, string saveContent)
        {
            string resolvedDirectory = Path.GetFullPath(initialDirectory);
            if (!Directory.Exists(resolvedDirectory))
            {
                Directory.CreateDirectory(resolvedDirectory);
            }
            var saveFileDialog = new SaveFileDialog
            {
                Title = _title,
                Filter = _filter,
                DefaultExt = _defaultExt,
                InitialDirectory = resolvedDirectory
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, saveContent);
            }
        }
    }


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

        private ListBox _listBox;

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
            _listBox = listBox;
            _macroDataSerializer = macroDataSerializer;
            _windowSaveMenuModifier = windowSaveMenuHandler;
            _initialDirectory = null;
            _saveButton.Click += OnEvent;
        }

        private MacroData _getListBoxMacroData()
        {
            var macroData = new MacroData();
            var commands = new string[_listBox.Items.Count];
            for (int i = 0; i < _listBox.Items.Count; i++)
            {
                commands[i] = ((ComboBox)_listBox.Items[i]).Text;
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


    public abstract class AbstractLoadFileDialog
    {
        public abstract string Prompt(string initialDirectory);
    }


    public class WindowLoadFileDialog : AbstractLoadFileDialog
    {
        private string _title;

        private string _filter;

        public WindowLoadFileDialog(string title, string filter)
        {
            _title = title;
            _filter = filter;
        }

        public override string Prompt(string initialDirectory)
        {
            string resolvedDirectory = Path.GetFullPath(initialDirectory);
            if (!Directory.Exists(resolvedDirectory))
            {
                Directory.CreateDirectory(resolvedDirectory);
            }
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = resolvedDirectory,
                Title = _title,
                Filter = _filter,
                CheckFileExists = true,
                CheckPathExists = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                return File.ReadAllText(openFileDialog.FileName);
            }
            return "";
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

        private ListBox _listBox;

        private ComboBox _comboBoxTemplate;

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry;

        private AbstractMacroDataDeserializer _macroDataDeserializer;

        private AbstractWindowStateModifier _windowLoadMenuModifier;

        private string? _initialDirectory;

        public WindowLoadMenuActionHandler(
            Button saveButton,
            ListBox listBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry,
            AbstractMacroDataDeserializer macroDataDeserializer,
            AbstractWindowStateModifier windowLoadMenuModifier
        )
        {
            _saveButton = saveButton;
            _listBox = listBox;
            _comboBoxTemplate = comboBoxTemplate;
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
                _listBox.Items.Clear();
                _comboBoxPopupScaleRegistry.ClearHandlers();
                foreach (var command in macroData.Macro)
                {
                    _loadComboBoxItem(command);
                }
            }
        }

        private List<ComboBoxItem> _comboBoxItems()
        {
            return _comboBoxTemplate.Items
                .Cast<ComboBoxItem>()
                .Select(item => new ComboBoxItem{ Content = item.Content })
                .ToList();
        }

        private void _loadComboBoxItem(string command)
        {
            var comboBox = new ComboBox
            {
                Text = command,
                Width = _comboBoxTemplate.Width,
                IsEditable = _comboBoxTemplate.IsEditable,
                FontSize = _comboBoxTemplate.FontSize,
                ItemsSource = _comboBoxItems()
            };
            _comboBoxPopupScaleRegistry.RegisterHandler(
                new WindowComboBoxScaleActionHandlerParameters(comboBox)
            );
            _listBox.Items.Add(comboBox);
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
            ComboBox comboBoxTemplate
        ) 
        {
            _loadMenuActionHandler = new WindowLoadMenuActionHandler(
                loadButton,
                macroListBox,
                comboBoxTemplate,
                new WindowComboBoxScaleActionHandlerRegistry(),
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
}
