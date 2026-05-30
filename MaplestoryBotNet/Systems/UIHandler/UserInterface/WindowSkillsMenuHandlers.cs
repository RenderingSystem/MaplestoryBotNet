using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.Systems.UIHandler.Utilities.Models;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class WindowSkillsMenuAddSkillModifier : AbstractWindowStateModifier
    {
        private ListBox _skillsListBox;

        private StackPanel _skillsTemplate;

        public WindowSkillsMenuAddSkillModifier(
            ListBox skillsListBox,
            StackPanel skillsTemplate
        )
        {
            _skillsListBox = skillsListBox;
            _skillsTemplate = skillsTemplate;
        }

        private string _newListBoxText()
        {
            var existingNames = new HashSet<string>(
                _skillsListBox.Items.OfType<ListBoxItem>().Select(
                    listBoxItem =>
                    {
                        var stackPanel = (StackPanel)listBoxItem.Content;
                        var textBox = stackPanel.Children.OfType<TextBox>().First();
                        return textBox.Text;
                    }
                )
            );
            int index = _skillsListBox.Items.Count;
            while (existingNames.Contains("skill " + index))
            {
                index++;
            }
            return "skill " + index;
        }

        private ListBoxItem _listBoxItem()
        {
            var checkbox = _skillsTemplate.Children.OfType<CheckBox>().First();
            var textbox = _skillsTemplate.Children.OfType<TextBox>().First();
            var skillName = _newListBoxText();
            return new ListBoxItem
            {
                Content = new StackPanel
                {
                    Orientation = _skillsTemplate.Orientation,
                    Focusable = _skillsTemplate.Focusable,
                    Children = {
                        new CheckBox
                        {
                            VerticalContentAlignment = checkbox.VerticalContentAlignment,
                        },
                        new TextBox
                        {
                            Margin = textbox.Margin,
                            VerticalContentAlignment = textbox.VerticalContentAlignment,
                            HorizontalContentAlignment = textbox.HorizontalContentAlignment,
                            Width = textbox.Width,
                            Height = textbox.Height,
                            Background = textbox.Background,
                            Foreground = textbox.Foreground,
                            FontFamily = textbox.FontFamily,
                            Text = skillName
                        }
                    }
                },
                Tag = new Skill { Name = skillName }
            };
        }

        public override void Modify(object? value)
        {
            var selectedIndex = _skillsListBox.SelectedIndex >= 0 ?
                _skillsListBox.SelectedIndex + 1 :
                _skillsListBox.Items.Count;
            _skillsListBox.Items.Insert(selectedIndex, _listBoxItem());
            _skillsListBox.SelectedIndex = selectedIndex;
        }
    }


    public class WindowSkillsMenuAddSkillActionHandler : AbstractWindowActionHandler
    {
        private Button _addSkillButton;

        private AbstractWindowStateModifier _addSkillModifier;

        public WindowSkillsMenuAddSkillActionHandler(
            Button addSkillButton,
            AbstractWindowStateModifier addSkillModifier
        )
        {
            _addSkillButton = addSkillButton;
            _addSkillModifier = addSkillModifier;
            _addSkillButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _addSkillModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _addSkillModifier.Modify(null);
        }
    }


    public class WindowSkillsMenuAddSkillActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _addSkillActionHandler;

        public WindowSkillsMenuAddSkillActionHandlerFacade(
            Button addSkillButton,
            ListBox addSkillListBox,
            StackPanel addSkillTemplate
        )
        {
            _addSkillActionHandler = new WindowSkillsMenuAddSkillActionHandler(
                addSkillButton,
                new WindowSkillsMenuAddSkillModifier(addSkillListBox, addSkillTemplate)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _addSkillActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _addSkillActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowSkillsMenuRemoveSkillModifier : AbstractWindowStateModifier
    {
        private ListBox _skillsListBox;

        public WindowSkillsMenuRemoveSkillModifier(ListBox skillsListBox)
        {
            _skillsListBox = skillsListBox;
        }

        public override void Modify(object? value)
        {
            var selectedIndex = _skillsListBox.SelectedIndex;
            if (selectedIndex >= 0)
            {
                _skillsListBox.Items.RemoveAt(selectedIndex);
            }
            else if (_skillsListBox.Items.Count > 0)
            {
                _skillsListBox.Items.RemoveAt(_skillsListBox.Items.Count - 1);
            }
        }
    }


    public class WindowSkillsMenuRemoveSkillActionHandler : AbstractWindowActionHandler
    {
        private Button _removeSkillButton;

        private AbstractWindowStateModifier _removeSkillModifier;

        public WindowSkillsMenuRemoveSkillActionHandler(
            Button removeSkillButton,
            AbstractWindowStateModifier removeSkillModifier
        )
        {
            _removeSkillButton = removeSkillButton;
            _removeSkillModifier = removeSkillModifier;
            _removeSkillButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _removeSkillModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _removeSkillModifier.Modify(null);
        }
    }


    public class WindowSkillsMenuRemoveSkillActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _removeSkillActionHandler;

        public WindowSkillsMenuRemoveSkillActionHandlerFacade(
            Button removeSkillButton,
            ListBox skillsListBox
        )
        {
            _removeSkillActionHandler = new WindowSkillsMenuRemoveSkillActionHandler(
                removeSkillButton,
                new WindowSkillsMenuRemoveSkillModifier(skillsListBox)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _removeSkillActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _removeSkillActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowSkillsMenuMacroCommandAddModifier : AbstractWindowStateModifier
    {
        private ListBox _macroListBox;

        private AbstractComboBoxFactory _comboBoxFactory;

        private AbstractWindowActionHandlerRegistry _comboBoxRegistry;

        public WindowSkillsMenuMacroCommandAddModifier(
            ListBox macroListBox,
            AbstractComboBoxFactory comboBoxFactory,
            AbstractWindowActionHandlerRegistry comboBoxRegistry
        )
        {
            _macroListBox = macroListBox;
            _comboBoxFactory = comboBoxFactory;
            _comboBoxRegistry = comboBoxRegistry;
        }

        public override void Modify(object? value)
        {
            var selectedIndex = _macroListBox.SelectedIndex >= 0 ?
                _macroListBox.SelectedIndex + 1 :
                _macroListBox.Items.Count;
            var comboBox = _comboBoxFactory.Create();
            var listBoxItem = new ListBoxItem { Content = comboBox };
            var parameters = new WindowComboBoxScaleActionHandlerParameters(comboBox);
            _macroListBox.Items.Insert(selectedIndex, listBoxItem);
            _macroListBox.SelectedIndex = selectedIndex;
            _comboBoxRegistry.RegisterHandler(parameters);
        }
    }


    public class WindowSkillsMenuMacroCommandAddActionHandler : AbstractWindowActionHandler
    {
        private Button _macroAddButton;

        private AbstractWindowStateModifier _macroCommandAddModifier;

        public WindowSkillsMenuMacroCommandAddActionHandler(
            Button macroAddButton,
            AbstractWindowStateModifier macroCommandAddModifier
        )
        {
            _macroAddButton = macroAddButton;
            _macroCommandAddModifier = macroCommandAddModifier;
            _macroAddButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandAddModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandAddModifier.Modify(null);
        }
    }


    public class WindowSkillsMenuMacroCommandAddActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _macroCommandAddActionHandler;

        public WindowSkillsMenuMacroCommandAddActionHandlerFacade(
            Button macroAddButton,
            ComboBox comboBoxTemplate,
            ListBox macroListBox,
            AbstractWindowActionHandlerRegistry comboBoxRegistry
        )
        {
            _macroCommandAddActionHandler = new WindowSkillsMenuAddSkillActionHandler(
                macroAddButton,
                new WindowSkillsMenuMacroCommandAddModifier(
                    macroListBox,
                    new ComboBoxTemplateFactory(comboBoxTemplate),
                    comboBoxRegistry
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _macroCommandAddActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _macroCommandAddActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowSkillsMenuMacroCommandRemoveModifier : AbstractWindowStateModifier
    {
        private ListBox _macroListBox;

        private AbstractWindowActionHandlerRegistry _comboBoxRegistry;

        public WindowSkillsMenuMacroCommandRemoveModifier(
            ListBox macroListBox,
            AbstractWindowActionHandlerRegistry comboBoxRegistry
        )
        {
            _macroListBox = macroListBox;
            _comboBoxRegistry = comboBoxRegistry;
        }

        public override void Modify(object? value)
        {
            var selectedIndex = _macroListBox.SelectedIndex;
            if (selectedIndex >= 0)
            {
                var comboBox = (ComboBox)((ListBoxItem)_macroListBox.SelectedItem).Content;
                _comboBoxRegistry.UnregisterHandler(comboBox);
                _macroListBox.Items.RemoveAt(selectedIndex);
            }
            else if (_macroListBox.Items.Count > 0)
            {
                var comboBox = (ComboBox)((ListBoxItem)_macroListBox.Items[_macroListBox.Items.Count - 1]).Content;
                _comboBoxRegistry.UnregisterHandler(comboBox);
                _macroListBox.Items.RemoveAt(_macroListBox.Items.Count - 1);
            }
        }
    }


    public class WindowSkillsMenuMacroCommandRemoveActionHandler : AbstractWindowActionHandler
    {
        private Button _removMacroButton;

        private AbstractWindowStateModifier _removeMacroModifier;

        public WindowSkillsMenuMacroCommandRemoveActionHandler(
            Button removeMacroButton,
            AbstractWindowStateModifier removeMacroModifier
        )
        {
            _removMacroButton = removeMacroButton;
            _removeMacroModifier = removeMacroModifier;
            _removMacroButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _removeMacroModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _removeMacroModifier.Modify(null);
        }
    }


    public class WindowSkillsMenuMacroCommandRemoveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _removeMacroActionHandler;

        public WindowSkillsMenuMacroCommandRemoveActionHandlerFacade(
            Button removeMacroButton,
            ListBox macroListBox,
            AbstractWindowActionHandlerRegistry comboBoxRegistry
        )
        {
            _removeMacroActionHandler = (
                new WindowSkillsMenuMacroCommandRemoveActionHandler(
                    removeMacroButton,
                    new WindowSkillsMenuMacroCommandRemoveModifier(
                        macroListBox,
                        comboBoxRegistry
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _removeMacroActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _removeMacroActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowSkillsMenuSkillSelectedModifier :
        AbstractWindowStateModifier
    {
        private ListBox _macroListBox;

        private TextBox _minDelay;

        private TextBox _maxDelay;

        private AbstractComboBoxFactory _comboBoxFactory;

        private AbstractWindowActionHandlerRegistry _comboBoxRegistry;

        public WindowSkillsMenuSkillSelectedModifier(
            ListBox macroListBox,
            TextBox minDelay,
            TextBox maxDelay,
            AbstractComboBoxFactory comboBoxFactory,
            AbstractWindowActionHandlerRegistry comboBoxRegistry
        )
        {
            _macroListBox = macroListBox;
            _minDelay = minDelay;
            _maxDelay = maxDelay;
            _comboBoxFactory = comboBoxFactory;
            _comboBoxRegistry = comboBoxRegistry;
        }

        public override void Modify(object? value)
        {
            if (value is ListBoxItem selectedSkill)
            {
                var skill = (Skill)selectedSkill.Tag;
                _macroListBox.Items.Clear();
                _comboBoxRegistry.ClearHandlers();
                _minDelay.Text = skill.MinDelay.ToString();
                _maxDelay.Text = skill.MaxDelay.ToString();
                foreach (var macro in skill.Macros)
                {
                    var comboBox = _comboBoxFactory.Create();
                    comboBox.Text = macro;
                    _macroListBox.Items.Add(new ListBoxItem { Content = comboBox });
                    var parameters = new WindowComboBoxScaleActionHandlerParameters(comboBox);
                    _comboBoxRegistry.RegisterHandler(parameters);
                }
            }
        }
    }


    public class WindowSkillsMenuSkillSelectedActionHandler :
        AbstractWindowActionHandler
    {
        private ListBox _skillsListBox;

        private AbstractWindowStateModifier _skillsSelectedModifier;

        public WindowSkillsMenuSkillSelectedActionHandler(
            ListBox skillsListBox,
            AbstractWindowStateModifier skillsSelectModifier
        )
        {
            _skillsListBox = skillsListBox;
            _skillsSelectedModifier = skillsSelectModifier;
            _skillsListBox.SelectionChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _skillsSelectedModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs &&
                selectionArgs.AddedItems.Count > 0
            )
            {
                _skillsSelectedModifier.Modify(selectionArgs.AddedItems[0]);
            }
        }
    }


    public class WindowSkillsMenuSkillSelectedActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _skillSelectedActionHandler;

        public WindowSkillsMenuSkillSelectedActionHandlerFacade(
            ListBox skillsListBox,
            ListBox macroListBox,
            TextBox minDelay,
            TextBox maxDelay,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry comboBoxRegistry
        )
        {
            _skillSelectedActionHandler = (
                new WindowSkillsMenuSkillSelectedActionHandler(
                    skillsListBox,
                    new WindowSkillsMenuSkillSelectedModifier(
                        macroListBox,
                        minDelay,
                        maxDelay,
                        new ComboBoxTemplateFactory(comboBoxTemplate),
                        comboBoxRegistry
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _skillSelectedActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _skillSelectedActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowSkillsMenuSkillDeselectedModifier :
        AbstractWindowStateModifier
    {
        private ListBox _macroListBox;

        private TextBox _minDelay;

        private TextBox _maxDelay;

        private AbstractWindowActionHandlerRegistry _comboBoxRegistry;

        public WindowSkillsMenuSkillDeselectedModifier(
            ListBox macroListBox,
            TextBox minDelay,
            TextBox maxDelay,
            AbstractWindowActionHandlerRegistry comboBoxRegistry
        )
        {
            _macroListBox = macroListBox;
            _minDelay = minDelay;
            _maxDelay = maxDelay;
            _comboBoxRegistry = comboBoxRegistry;
        }

        public override void Modify(object? value)
        {
            if (value is ListBoxItem deselectedItem)
            {
                var skill = (Skill)deselectedItem.Tag;
                skill.MinDelay = int.Parse(_minDelay.Text);
                skill.MaxDelay = int.Parse(_maxDelay.Text);
                skill.Macros.Clear();
                foreach (ListBoxItem macro in _macroListBox.Items)
                {
                    skill.Macros.Add(((ComboBox)macro.Content).Text);
                }
                _macroListBox.Items.Clear();
                _comboBoxRegistry.ClearHandlers();
            }
        }
    }


    public class WindowSkillsMenuSkillDeselectedActionHandler :
        AbstractWindowActionHandler
    {
        private ListBox _skillsListBox;

        private AbstractWindowStateModifier _skillDeselectedModifier;

        public WindowSkillsMenuSkillDeselectedActionHandler(
            ListBox skillsListBox,
            AbstractWindowStateModifier skillDeselectedModifier
        )
        {
            _skillsListBox = skillsListBox;
            _skillDeselectedModifier = skillDeselectedModifier;
            _skillsListBox.SelectionChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _skillDeselectedModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs &&
                selectionArgs.RemovedItems.Count > 0
            )
            {
                _skillDeselectedModifier.Modify(selectionArgs.RemovedItems[0]);
            }
        }
    }


    public class WindowSkillsMenuSkillDeselectedActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _deselectedActionHandler;

        public WindowSkillsMenuSkillDeselectedActionHandlerFacade(
            ListBox skillsListBox,
            ListBox macroListBox,
            TextBox minDelay,
            TextBox maxDelay,
            AbstractWindowActionHandlerRegistry comboBoxRegistry
        )
        {
            _deselectedActionHandler = (
                new WindowSkillsMenuSkillDeselectedActionHandler(
                    skillsListBox,
                    new WindowSkillsMenuSkillDeselectedModifier(
                        macroListBox,
                        minDelay,
                        maxDelay,
                        comboBoxRegistry
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _deselectedActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _deselectedActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowSkillsMenuSkillSaveModifier : AbstractWindowStateModifier
    {
        private ListBox _skillsListBox;

        public WindowSkillsMenuSkillSaveModifier(ListBox skillsListBox)
        {
            _skillsListBox = skillsListBox;
        }

        public override void Modify(object? value)
        {
            if (value is AbstractSkillsModel skillsModel)
            {
                var selectedIndex = _skillsListBox.SelectedIndex;
                _skillsListBox.SelectedIndex = -1;
                _skillsListBox.SelectedIndex = selectedIndex;
                var skillsList = new List<Skill>();
                foreach (ListBoxItem listBoxItem in _skillsListBox.Items)
                {
                    var skill = (Skill)listBoxItem.Tag;
                    var stackPanel = (StackPanel)listBoxItem.Content;
                    var checkBox = stackPanel.Children.OfType<CheckBox>().First();
                    var textBox = stackPanel.Children.OfType<TextBox>().First();
                    skill.Active = checkBox.IsChecked == true ? 1 : 0;
                    skill.Name = textBox.Text;
                    skillsList.Add((Skill)listBoxItem.Tag);
                }
                skillsModel.SetSkills(skillsList);
            }
        }
    }


    public class WindowSkillsMenuSkillSaveActionHandler : AbstractWindowActionHandler
    {
        private Button _saveButton;

        private AbstractWindowStateModifier _saveModifier;

        private AbstractSkillsModel? _skillsModel;

        public WindowSkillsMenuSkillSaveActionHandler(
            Button saveButton,
            AbstractWindowStateModifier saveModifier
        )
        {
            _saveButton = saveButton;
            _saveModifier = saveModifier;
            _saveButton.Click += OnEvent;
            _skillsModel = null;
        }
        public override AbstractWindowStateModifier Modifier()
        {
            return _saveModifier;
        }
        public override void OnEvent(object? sender, EventArgs e)
        {
            _saveModifier.Modify(_skillsModel);
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.SkillsModel &&
                data is AbstractSkillsModel skillsModel
            )
            {
                _skillsModel = skillsModel;
            }
        }
    }


    public class WindowSkillsMenuSkillSaveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _saveActionHandler;

        public WindowSkillsMenuSkillSaveActionHandlerFacade(
            Button saveButton,
            ListBox skillsListBox
        )
        {
            _saveActionHandler = (
                new WindowSkillsMenuSkillSaveActionHandler(
                    saveButton,
                    new WindowSkillsMenuSkillSaveModifier(skillsListBox)
                )
            );
        }
        public override AbstractWindowStateModifier Modifier()
        {
            return _saveActionHandler.Modifier();
        }
        public override void OnEvent(object? sender, EventArgs e)
        {
            _saveActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _saveActionHandler.Inject(dataType, data);
        }
    }


    public class WindowSkillsMenuSkillSaveConfigurationActionHandler : AbstractWindowActionHandler
    {
        private Button _saveButton;

        private AbstractSkillsSerializer _skillsSerializer;

        private AbstractJsonDataModelConverter _skillsConverter;

        private AbstractWindowStateModifier _windowSaveDialogModifier;

        private AbstractSkillsModel? _skillsModel;

        private string _initialDirectory;

        public WindowSkillsMenuSkillSaveConfigurationActionHandler(
            Button saveButton,
            AbstractSkillsSerializer skillsSerializer,
            AbstractJsonDataModelConverter skillsConverter,
            AbstractWindowStateModifier windowSaveDialogModifier
        )
        {
            _saveButton = saveButton;
            _skillsSerializer = skillsSerializer;
            _skillsConverter = skillsConverter;
            _windowSaveDialogModifier = windowSaveDialogModifier;
            _saveButton.Click += OnEvent;
            _initialDirectory = "";
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_skillsModel == null)
            {
                return;
            }
            if (_initialDirectory == "")
            {
                return;
            }
            var configuration = (
                (ConfigurationSkills)
                _skillsConverter.ToConfiguration(_skillsModel)!
            );
            var serialized = (
                _skillsSerializer.Serialize(configuration)
            );
            _windowSaveDialogModifier.Modify(
                new WindowSaveMenuModifierParameters
                {
                    InitialDirectory = _initialDirectory,
                    SaveContent = serialized
                }
            );
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _initialDirectory = configuration.SkillsDirectory;
            }
            if (
                dataType is SystemInjectType.SkillsModel &&
                data is AbstractSkillsModel skillsModel
            )
            {
                _skillsModel = skillsModel;
            }
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowSaveDialogModifier;
        }
    }


    public class WindowSkillsMenuSkillSaveConfigurationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _saveConfigurationActionHandler;

        public WindowSkillsMenuSkillSaveConfigurationActionHandlerFacade(
            Button saveButton,
            AbstractSaveFileDialog saveFileDialog
        )
        {
            _saveConfigurationActionHandler = (
                new WindowSkillsMenuSkillSaveConfigurationActionHandler(
                    saveButton,
                    new SkillsSerializer(),
                    new SkillsConverter(),
                    new WindowSaveMenuModifier(saveFileDialog)
                )
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _saveConfigurationActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _saveConfigurationActionHandler.Inject(dataType, data);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _saveConfigurationActionHandler.Modifier();
        }
    }


    public class WindowSkillsMenuSkillSavingActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _skillsWindow;

        private AbstractWindowStateModifier _savingModifier;

        private AbstractSkillsModel? _skillsModel;

        public WindowSkillsMenuSkillSavingActionHandler(
            AbstractSystemWindow skillsWindow,
            AbstractWindowStateModifier savingModifier
        )
        {
            _skillsWindow = skillsWindow;
            _savingModifier = savingModifier;
            ((Window)_skillsWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent; ;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _savingModifier;
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (!_skillsWindow.Visible())
            {
                _savingModifier.Modify(_skillsModel);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.SkillsModel &&
                data is AbstractSkillsModel skillsModel
            )
            {
                _skillsModel = skillsModel;
            }
        }
    }


    public class WindowSkillsMenuSkillSavingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _savingActionHandler;
        public WindowSkillsMenuSkillSavingActionHandlerFacade(
            AbstractSystemWindow skillsWindow,
            ListBox skillsListBox
        )
        {
            _savingActionHandler = (
                new WindowSkillsMenuSkillSavingActionHandler(
                    skillsWindow,
                    new WindowSkillsMenuSkillSaveModifier(skillsListBox)
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _savingActionHandler.Modifier();
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _savingActionHandler.OnDependencyEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _savingActionHandler.Inject(dataType, data);
        }
    }


    public class WindowSkillsMenuSkillLoadActionHandler : AbstractWindowActionHandler
    {
        private Button _loadButton;

        private AbstractWindowStateModifier _windowLoadMenuModifier;

        private string? _initialDirectory;

        public WindowSkillsMenuSkillLoadActionHandler(
            Button loadButton,
            AbstractWindowStateModifier windowLoadMenuModifier
        )
        {
            _loadButton = loadButton;
            _windowLoadMenuModifier = windowLoadMenuModifier;
            _loadButton.Click += OnEvent;
            _initialDirectory = null;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_initialDirectory == null)
            {
                return;
            }
            var parameters = new WindowLoadMenuModifierParameters
            {
                InitialDirectory = _initialDirectory
            };
            _windowLoadMenuModifier.Modify(parameters);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowLoadMenuModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _initialDirectory = configuration.SkillsDirectory;
            }
        }
    }


    public class WindowSkillsMenuSkillLoadActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _loadMenuActionHandler;

        public WindowSkillsMenuSkillLoadActionHandlerFacade(
            Button loadButton,
            AbstractLoadFileDialog loadFileDialog
        )
        {
            _loadMenuActionHandler = new WindowSkillsMenuSkillLoadActionHandler(
                loadButton, new WindowLoadMenuModifier(loadFileDialog)
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

        public override void Inject(object dataType, object? data)
        {
            _loadMenuActionHandler.Inject(dataType, data);
        }
    }


    public class WindowSkillsMenuSkillLoadConfigurationModifier :
        AbstractWindowStateModifier
    {
        private ListBox _skillsListBox;

        private AbstractSkillsDeserializer _skillsDeserializer;

        private StackPanel _skillsTemplate;

        public WindowSkillsMenuSkillLoadConfigurationModifier(
            ListBox skillsListBox,
            StackPanel skillsTemplate,
            AbstractSkillsDeserializer skillsDeserializer

        )
        {
            _skillsListBox = skillsListBox;
            _skillsTemplate = skillsTemplate;
            _skillsDeserializer = skillsDeserializer;
        }

        private ListBoxItem _listBoxItem(ConfigurationSkill skill)
        {
            var checkbox = _skillsTemplate.Children.OfType<CheckBox>().First();
            var textbox = _skillsTemplate.Children.OfType<TextBox>().First();
            return new ListBoxItem
            {
                Content = new StackPanel
                {
                    Orientation = _skillsTemplate.Orientation,
                    Focusable = _skillsTemplate.Focusable,
                    Children =
                    {
                        new CheckBox
                        {
                            VerticalContentAlignment = checkbox.VerticalContentAlignment,
                            IsChecked = skill.Active != 0
                        },
                        new TextBox
                        {
                            Margin = textbox.Margin,
                            VerticalContentAlignment = textbox.VerticalContentAlignment,
                            HorizontalContentAlignment = textbox.HorizontalContentAlignment,
                            Width = textbox.Width,
                            Height = textbox.Height,
                            Background = textbox.Background,
                            Foreground = textbox.Foreground,
                            FontFamily = textbox.FontFamily,
                            Text = skill.Name
                        }
                    }
                },
                Tag = new Skill
                {
                    Name = skill.Name,
                    Active = skill.Active,
                    Macros = [.. skill.Macros],
                    MinDelay = skill.MinDelay,
                    MaxDelay = skill.MaxDelay
                }
            };
        }

        public override void Modify(object? value)
        {
            if (value is string content)
            {
                _skillsListBox.SelectedIndex = -1;
                _skillsListBox.Items.Clear();
                var skills = _skillsDeserializer.DeserializeSkills(content);
                foreach (var skill in skills.Skills)
                {
                    var listBoxItem = _listBoxItem(skill);
                    _skillsListBox.Items.Add(listBoxItem);
                }
                _skillsListBox.SelectedIndex = 0;
            }
        }
    }


    public class WindowSkillsMenuSkillLoadConfigurationActionHandler :
        AbstractWindowActionHandler
    {
        private AbstractLoadFileDialog _loadFileDialog;

        private AbstractWindowStateModifier _loadConfigurationModifier;

        public WindowSkillsMenuSkillLoadConfigurationActionHandler(
            AbstractLoadFileDialog loadFileDialog,
            AbstractWindowStateModifier loadConfigurationModifier
        )
        {
            _loadFileDialog = loadFileDialog;
            _loadConfigurationModifier = loadConfigurationModifier;
            _loadFileDialog.FileLoaded += OnEvent;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is FileLoadedEventArgs fileLoadedEventArgs
                && fileLoadedEventArgs.Success
            )
            {
                _loadConfigurationModifier.Modify(fileLoadedEventArgs.Content);
            }
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadConfigurationModifier;
        }
    }


    public class WindowSkillsMenuSkillLoadConfigurationActionHandlerFacade :
        AbstractWindowActionHandler
    {
        private WindowSkillsMenuSkillLoadConfigurationActionHandler
            _loadConfigurationActionHandler;
        public WindowSkillsMenuSkillLoadConfigurationActionHandlerFacade(
            ListBox skillsListBox,
            StackPanel skillsTemplate,
            AbstractLoadFileDialog loadFileDialog
        )
        {
            _loadConfigurationActionHandler = (
                new WindowSkillsMenuSkillLoadConfigurationActionHandler(
                    loadFileDialog,
                    new WindowSkillsMenuSkillLoadConfigurationModifier(
                        skillsListBox,
                        skillsTemplate, new SkillsDeserializer()
                    )
                )
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _loadConfigurationActionHandler.OnEvent(sender, e);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _loadConfigurationActionHandler.Modifier();
        }
    }


    public class WindowSkillsMenuAccessibilityModifier : AbstractWindowStateModifier
    {
        private List<FrameworkElement> _accessibilityElements;

        public WindowSkillsMenuAccessibilityModifier(
            List<FrameworkElement> accessibilityElements
        )
        {
            _accessibilityElements = accessibilityElements;
        }

        public override void Modify(object? value)
        {
            if (value is bool isAccessible)
            {
                foreach (var element in _accessibilityElements)
                {
                    element.IsEnabled = isAccessible;
                }
            }
        }
    }


    public class WindowSkillsMenuAccessibilityActionHandler : AbstractWindowActionHandler
    {
        private ListBox _skillsListBox;

        private AbstractWindowStateModifier _accessibilityModifier;

        public WindowSkillsMenuAccessibilityActionHandler(
            ListBox skillsListBox,
            AbstractWindowStateModifier accessibilityModifier
        )
        {
            _skillsListBox = skillsListBox;
            _accessibilityModifier = accessibilityModifier;
            _skillsListBox.SelectionChanged += OnEvent;
            OnEvent(null, new EventArgs());
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _accessibilityModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _accessibilityModifier.Modify(_skillsListBox.SelectedIndex >= 0);
        }
    }


    public class WindowSkillsMenuAccessibilityActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _accessibilityActionHandler;
        public WindowSkillsMenuAccessibilityActionHandlerFacade(
            ListBox skillsListBox,
            List<FrameworkElement> accessibilityElements
        )
        {
            _accessibilityActionHandler = (
                new WindowSkillsMenuAccessibilityActionHandler(
                    skillsListBox,
                    new WindowSkillsMenuAccessibilityModifier(accessibilityElements)
                )
            );
        }
        public override AbstractWindowStateModifier Modifier()
        {
            return _accessibilityActionHandler.Modifier();
        }
        public override void OnEvent(object? sender, EventArgs e)
        {
            _accessibilityActionHandler.OnEvent(sender, e);
        }
    }
}
