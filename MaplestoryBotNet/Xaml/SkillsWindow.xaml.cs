using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet.Xaml
{
    public partial class SkillsWindow : Window
    {
        private AbstractSystemWindow? _systemWindow;

        private AbstractSaveFileDialog _saveFileDialog;

        private AbstractLoadFileDialog _loadFileDialog;

        private AbstractWindowActionHandlerRegistry _comboBoxScaleRegistry;

        public SkillsWindow()
        {
            _systemWindow = null;
            InitializeComponent();
            SkillsListBox.Items.Clear();
            SkillsMacroListBox.Items.Clear();
            _saveFileDialog = new WindowSaveFileDialog("Save Skills", "JSON files (*.json)|*.json", ".json");
            _loadFileDialog = new WindowLoadFileDialog("Load Skills", "JSON files (*.json)|*.json");
            _comboBoxScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
        }

        private AbstractWindowActionHandler _instantiateNumericTextBoxPropertyActionHandler(
            TextBox numericTextBox, int maxValue
        )
        {
            return (
                new NumericTextBoxValidationActionHandlerBuilder()
                    .WithArgs(maxValue)
                    .WithArgs(numericTextBox)
                    .Build()
            );
        }

        private AbstractWindowActionHandler _instantiateNumericTextBoxPropertyPasteActionHandler(
            TextBox numericTextBox, int maxValue
        )
        {
            return (
                new NumericTextBoxPasteValidationActionHandlerBuilder()
                    .WithArgs(maxValue)
                    .WithArgs(numericTextBox)
                    .Build()
            );
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemHideActionHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }

        private AbstractWindowActionHandler _instantiatAddSkillActionHandler()
        {
            return new WindowSkillsMenuAddSkillActionHandlerFacade(
                SkillsAddButton,
                SkillsListBox,
                SkillsListBoxTemplate
            );
        }

        private AbstractWindowActionHandler _instantiateRemoveSkillActionHandler()
        {
            return new WindowSkillsMenuRemoveSkillActionHandlerFacade(
                SkillsRemoveButton,
                SkillsListBox
            );
        }

        private AbstractWindowActionHandler _instantiateMacroCommandAddActionHandler()
        {
            return new WindowSkillsMenuMacroCommandAddActionHandlerFacade(
                SkillsAddCommandButton,
                SkillsMacroComboBoxTemplate,
                SkillsMacroListBox,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateMacroCommandRemoveActionHandler()
        {
            return new WindowSkillsMenuMacroCommandRemoveActionHandlerFacade(
                SkillsRemoveCommandButton,
                SkillsMacroListBox,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateSkillDeselectedActionHandler()
        {
            return new WindowSkillsMenuSkillDeselectedActionHandlerFacade(
                SkillsListBox,
                SkillsMacroListBox,
                SkillsMinDelayTextBox,
                SkillsMaxDelayTextBox,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateSkillSelectedActionHandler()
        {
            return new WindowSkillsMenuSkillSelectedActionHandlerFacade(
                SkillsListBox,
                SkillsMacroListBox,
                SkillsMinDelayTextBox,
                SkillsMaxDelayTextBox,
                SkillsMacroComboBoxTemplate,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateSkillSaveActionHandler()
        {
            return new WindowSkillsMenuSkillSaveActionHandlerFacade(
                SkillsSaveButton,
                SkillsListBox
            );
        }

        private AbstractWindowActionHandler _instantiateSkillSaveConfigurationActionHandler()
        {
            return new WindowSkillsMenuSkillSaveConfigurationActionHandlerFacade(
                SkillsSaveButton,
                _saveFileDialog
            );
        }

        private AbstractWindowActionHandler _instantiateSkillSavingActionHandler()
        {
            return new WindowSkillsMenuSkillSavingActionHandlerFacade(
                GetSystemWindow(),
                SkillsListBox
            );
        }

        private AbstractWindowActionHandler _instantiateSkillLoadActionHandler()
        {
            return new WindowSkillsMenuSkillLoadActionHandlerFacade(
                SkillsLoadButton,
                _loadFileDialog
            );
        }

        private AbstractWindowActionHandler _instantiateSkillLoadConfigurationActionHandler()
        {
            return new WindowSkillsMenuSkillLoadConfigurationActionHandlerFacade(
                SkillsListBox,
                SkillsListBoxTemplate,
                _loadFileDialog
            );
        }


        private AbstractWindowActionHandler _instantiateAccessibilityActionHandler()
        {
            return new WindowSkillsMenuAccessibilityActionHandlerFacade(
                SkillsListBox,
                [
                    SkillsMinDelayTextBox,
                    SkillsMaxDelayTextBox,
                    SkillsAddCommandButton,
                    SkillsRemoveCommandButton,
                ]
            );
        }

        public AbstractSystemWindow GetSystemWindow()
        {
            if (_systemWindow == null)
            {
                _systemWindow = new SystemWindow(this);
            }
            return _systemWindow;
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateNumericTextBoxPropertyActionHandler(SkillsMinDelayTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(SkillsMaxDelayTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(SkillsMinDelayTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(SkillsMaxDelayTextBox, 9999),
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiatAddSkillActionHandler(),
                _instantiateRemoveSkillActionHandler(),
                _instantiateMacroCommandAddActionHandler(),
                _instantiateMacroCommandRemoveActionHandler(),
                _instantiateSkillDeselectedActionHandler(),
                _instantiateSkillSelectedActionHandler(),
                _instantiateSkillSaveActionHandler(),
                _instantiateSkillSaveConfigurationActionHandler(),
                _instantiateSkillSavingActionHandler(),
                _instantiateSkillLoadActionHandler(),
                _instantiateSkillLoadConfigurationActionHandler(),
                _instantiateAccessibilityActionHandler()
            ];
        }
    }
}
