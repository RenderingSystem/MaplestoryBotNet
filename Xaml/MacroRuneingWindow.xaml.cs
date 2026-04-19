using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using Microsoft.Win32;
using System.Windows;


namespace MaplestoryBotNet.Xaml
{
    public partial class MacroRuneingWindow : Window
    {
        private AbstractWindowMapEditMenuState _editMenuState;

        private AbstractWindowActionHandlerRegistry _comboBoxScaleRegistry;

        private AbstractSystemWindow? _systemWindow;

        private AbstractLoadFileDialog _loadFileDialog;

        private AbstractSaveFileDialog _saveFileDialog;

        public MacroRuneingWindow(AbstractWindowMapEditMenuState editMenuState)
        {
            InitializeComponent();
            RuneingPointsListBox.Items.Clear();
            RuneingPointsMacroListBox.Items.Clear();
            RuneingMovementsListBox.Items.Clear();
            RuneingMovementsMacroListBox.Items.Clear();
            _editMenuState = editMenuState;
            _comboBoxScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
            _systemWindow = null;
            _loadFileDialog = new WindowLoadFileDialog("Load Macro", "JSON files (*.json)|*.json");
            _saveFileDialog = new WindowSaveFileDialog("Save Macro", "JSON files (*.json)|*.json", ".json");
        }

        public AbstractSystemWindow GetSystemWindow()
        {
            if (_systemWindow == null)
            {
                _systemWindow = new SystemWindow(this);
            }
            return _systemWindow;
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemHideActionHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }

        private AbstractWindowActionHandler _instantiateMovementDirectionComboBoxScaleActionHandler()
        {
            return new WindowComboBoxScaleActionHandlerFacade(DirectionComboBox);
        }

        public AbstractWindowActionHandler _frameNameLoadingActionHandler()
        {
            return new WindowRuneingEditorFrameNameLoadingActionHandlerFacade(
                RuneingSelectedFrameTextBox,
                GetSystemWindow(),
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointMacrosLoadingActionHandler()
        {
            return new WindowRuneingEditorFramePointMacrosLoadingActionHandlerFacade(
                RuneingPointsListBox,
                RuneingPointMacroTemplate,
                GetSystemWindow(),
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointMacroAccessActionHandler()
        {
            return new WindowRuneingEditorFramePointMacroAccessActionHandlerFacade(
                RuneingPointsListBox,
                [
                    RuneingPointMacroAddCommandButton,
                    RuneingPointMacroRemoveCommandButton,
                    RuneingPointMacroClearCommandsButton,
                    RuneingNextFrameTextBox,
                    RuneingRadiusTextBox,
                    FramePointsLoadButton,
                    FramePointsSaveButton
                ]
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointMacroSelectionActionHandler()
        {
            return new WindowRuneingEditorFramePointMacroSelectionActionHandlerFacade(
                RuneingPointsListBox,
                RuneingPointsMacroListBox,
                RuneingNextFrameTextBox,
                RuneingRadiusTextBox,
                RuneingPointMacroComboBoxTemplate,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointMacroDeselectionActionHandler()
        {
            return new WindowRuneingEditorFramePointMacroDeselectionActionHandlerFacade(
                RuneingNextFrameTextBox,
                RuneingRadiusTextBox,
                RuneingPointsListBox,
                RuneingPointsMacroListBox
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointMacroCommandAddActionHandler()
        {
            return new WindowRuneingEditorFramePointMacroCommandAddActionHandlerFacade(
                RuneingPointMacroAddCommandButton,
                RuneingPointsMacroListBox,
                RuneingPointMacroComboBoxTemplate,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointMacroCommandRemoveActionHandler()
        {
            return new WindowRuneingEditorFramePointMacroCommandRemoveActionHandlerFacade(
                RuneingPointMacroRemoveCommandButton,
                RuneingPointsMacroListBox,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointMacroCommandClearActionHandler()
        {
            return new WindowRuneingEditorFramePointMacroCommandClearActionHandlerFacade(
                RuneingPointMacroClearCommandsButton,
                RuneingPointsMacroListBox,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointLoadConfigurationActionHandler()
        {
            return new WindowRuneingEditorFramePointLoadConfigurationActionHandlerFacade(
                FramePointsLoadButton,
                _loadFileDialog
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointLoadActionHandler()
        {
            return new WindowRuneingEditorFramePointLoadActionHandlerFacade(
                _loadFileDialog,
                RuneingPointsMacroListBox,
                RuneingPointMacroComboBoxTemplate,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointSaveActionHandler()
        {
            return new WindowRuneingEditorFramePointSaveActionHandlerFacade(
                FramePointsSaveButton,
                RuneingPointsMacroListBox,
                _saveFileDialog
            );
        }

        private AbstractWindowActionHandler _instantiateMovementAddActionHandler()
        {
            return new WindowRuneingEditorMovementAddActionHandlerFacade(
                MovementAddMacroButton,
                RuneingMovementsListBox,
                RuneingMovementTemplate
            );
        }

        private AbstractWindowActionHandler _instantiateMovementRemoveActionHandler()
        {
            return new WindowRuneingEditorMovementRemoveActionHandlerFacade(
                MovementRemoveMacroButton,
                RuneingMovementsListBox
            );
        }

        private AbstractWindowActionHandler _instantiateMovementMacroAccessActionHandler()
        {
            return new WindowRuneingEditorMovementMacroAccessActionHandlerFacade(
                RuneingMovementsListBox,
                [
                    MovementAddMacroCommandButton,
                    MovementRemoveMacroCommandButton,
                    MovementClearMacroCommandsButton,
                    DirectionComboBox,
                    DistanceTextBox,
                    FrameMovementsSaveButton,
                    FrameMovementsLoadButton
                ]
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsCommandAddActionHandler()
        {
            return new WindowRuneingEditorMovementsCommandAddActionHandlerFacade(
                MovementAddMacroCommandButton,
                RuneingMovementsMacroListBox,
                RuneingMovementComboBoxTemplate,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsCommandRemoveActionHandler()
        {
            return new WindowRuneingEditorMovementsCommandRemoveActionHandlerFacade(
                MovementRemoveMacroCommandButton,
                RuneingMovementsMacroListBox,
                _comboBoxScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsCommandClearActionHandler()
        {
            return new WindowRuneingEditorMovementsCommandClearActionHandlerFacade(
                MovementClearMacroCommandsButton,
                RuneingMovementsMacroListBox,
                _comboBoxScaleRegistry
            );
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateMovementDirectionComboBoxScaleActionHandler(),
                _frameNameLoadingActionHandler(),
                _instantiateFramePointMacrosLoadingActionHandler(),
                _instantiateFramePointMacroAccessActionHandler(),
                _instantiateFramePointMacroDeselectionActionHandler(),
                _instantiateFramePointMacroSelectionActionHandler(),
                _instantiateFramePointMacroCommandAddActionHandler(),
                _instantiateFramePointMacroCommandRemoveActionHandler(),
                _instantiateFramePointMacroCommandClearActionHandler(),
                _instantiateFramePointLoadConfigurationActionHandler(),
                _instantiateFramePointLoadActionHandler(),
                _instantiateFramePointSaveActionHandler(),
                _instantiateMovementAddActionHandler(),
                _instantiateMovementRemoveActionHandler(),
                _instantiateMovementMacroAccessActionHandler(),
                _instantiateMovementsCommandAddActionHandler(),
                _instantiateMovementsCommandRemoveActionHandler(),
                _instantiateMovementsCommandClearActionHandler()
            ];
        }
    }
}
