using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;


namespace MaplestoryBotNet.Xaml
{
    public partial class MacroBottingWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        private WindowComboBoxScaleActionHandlerRegistry _comboBoxScaleRegistry;

        private AbstractWindowMapEditMenuState _editMenuState;

        private WindowLoadFileDialog _loadFileDialog;

        public MacroBottingWindow(
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            InitializeComponent();
            MacroListBox.Items.Clear();
            PointMacroListBox.Items.Clear();
            _comboBoxScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
            _loadFileDialog = new WindowLoadFileDialog("Load Macro", "JSON files (*.json)|*.json");
            _editMenuState = editMenuState;
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemHideActionHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }

        private AbstractWindowActionHandler _instantiateLoadMenuActionHandler()
        {
            return new WindowLoadMenuActionHandlerFacade(
                LoadButton,
                _loadFileDialog
            );
        }

        private AbstractWindowActionHandler _instantiateLoadMenuElementActionHandler()
        {
            return new WindowLoadMenuElementActionHandlerFacade(
                _loadFileDialog,
                MacroListBox,
                ComboBoxTemplate,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateWindowAddMacroCommandActionHandler()
        {
            return new WindowAddMacroCommandActionHandlerFacade(
                AddButton,
                MacroListBox,
                ComboBoxTemplate,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateWindowRemoveMacroCommandActionHandler()
        {
            return new WindowRemoveMacroCommandActionHandlerFacade(
                RemoveButton,
                MacroListBox,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateWindowClearMacroCommandActionHandler()
        {
            return new WindowClearMacroCommandsActionHandlerFacade(
                ClearButton,
                MacroListBox,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateSaveMenuActionHandler()
        {
            return new WindowSaveMenuActionHandlerFacade(
                SaveButton,
                MacroListBox
            );
        }

        private AbstractWindowActionHandler _instantiateMacroDisplayLoadingActionHandler()
        {
            return new WindowMacroDisplayLoadingActionHandlerFacade(
                GetSystemWindow(),
                PointMacroListBox,
                MacroNameTextBox,
                PointMacroTemplate,
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateMacroDisplaySavingActionHandler()
        {
            return new WindowMacroCommandLabelSavingActionHandlerFacade(
                GetSystemWindow(),
                MacroNameTextBox,
                PointMacroListBox
            );
        }

        private AbstractWindowActionHandler _instantiateMacroCommandsSaveStateActionHandler()
        {
            return new WindowMacroCommandsSaveStateActionHandlerFacade(
                PointMacroListBox,
                MacroListBox
            );
        }

        private AbstractWindowActionHandler _instantiateMacroCommandsDisplayActionHandler()
        {
            return new WindowMacroCommandsDisplayActionHandlerFacade(
                PointMacroListBox,
                MacroListBox,
                ComboBoxTemplate,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateMacroCommandsAddingActionHandler()
        {
            return new WindowMacroCommandsAddingActionHandlerFacade(
                AddMacroButton,
                PointMacroListBox,
                PointMacroTemplate
            );
        }

        private AbstractWindowActionHandler _instantiateMacroCommandsRemovingActionHandler()
        {
            return new WindowMacroCommandsRemovingActionHandlerFacade(
                RemoveMacroButton,
                PointMacroListBox
            );
        }

        private AbstractWindowActionHandler _instantiateMacroCommandsRemoveButtonAccessActionHandler()
        {
            return new WindowMacroCommandsRemoveButtonAccessActionHandlerFacade(
                GetSystemWindow(),
                PointMacroListBox,
                AddMacroButton,
                RemoveMacroButton
            );
        }

        private AbstractWindowActionHandler _instantiateMacroCommandsProbabilityTextBoxBindingActionHandler()
        {
            return new WindowMacroCommandsProbabilityTextBoxBindingActionHandlerFacade(
                PointMacroListBox
            );
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateLoadMenuActionHandler(),
                _instantiateLoadMenuElementActionHandler(),
                _instantiateWindowAddMacroCommandActionHandler(),
                _instantiateWindowRemoveMacroCommandActionHandler(),
                _instantiateWindowClearMacroCommandActionHandler(),
                _instantiateSaveMenuActionHandler(),
                _instantiateMacroDisplayLoadingActionHandler(),
                _instantiateMacroDisplaySavingActionHandler(),
                _instantiateMacroCommandsSaveStateActionHandler(),
                _instantiateMacroCommandsDisplayActionHandler(),
                _instantiateMacroCommandsAddingActionHandler(),
                _instantiateMacroCommandsRemovingActionHandler(),
                _instantiateMacroCommandsRemoveButtonAccessActionHandler(),
                _instantiateMacroCommandsProbabilityTextBoxBindingActionHandler()
            ];
        }

        public AbstractSystemWindow GetSystemWindow()
        {
            if (_systemWindow == null)
            {
                _systemWindow = new SystemWindow(this);
            }
            return _systemWindow;
        }
    }
}
