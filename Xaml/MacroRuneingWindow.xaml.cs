using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet.Xaml
{
    public partial class MacroRuneingWindow : Window
    {
        private AbstractWindowMapEditMenuState _editMenuState;

        private AbstractWindowActionHandlerRegistry _framePointMacroCommandsScaleRegistry;

        private AbstractWindowActionHandlerRegistry _movementCommandsScaleRegistry;

        private AbstractSystemWindow? _systemWindow;

        private AbstractLoadFileDialog _loadFramePointsFileDialog;

        private AbstractSaveFileDialog _saveFramePointsFileDialog;

        private AbstractLoadFileDialog _loadMovementsFileDialog;

        private AbstractSaveFileDialog _saveMovementsFileDialog;

        public MacroRuneingWindow(AbstractWindowMapEditMenuState editMenuState)
        {
            InitializeComponent();
            RuneingPointsListBox.Items.Clear();
            RuneingPointsMacroListBox.Items.Clear();
            RuneingMovementsListBox.Items.Clear();
            RuneingMovementsMacroListBox.Items.Clear();
            _editMenuState = editMenuState;
            _framePointMacroCommandsScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
            _movementCommandsScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
            _systemWindow = null;
            _loadFramePointsFileDialog = new WindowLoadFileDialog("Load Macro", "JSON files (*.json)|*.json");
            _saveFramePointsFileDialog = new WindowSaveFileDialog("Save Macro", "JSON files (*.json)|*.json", ".json");
            _loadMovementsFileDialog = new WindowLoadFileDialog("Load Macro", "JSON files (*.json)|*.json");
            _saveMovementsFileDialog = new WindowSaveFileDialog("Save Macro", "JSON files (*.json)|*.json", ".json");
        }

        public AbstractSystemWindow GetSystemWindow()
        {
            if (_systemWindow == null)
            {
                _systemWindow = new SystemWindow(this);
            }
            return _systemWindow;
        }

        protected AbstractWindowActionHandler _instantiateNumericTextBoxPropertyActionHandler(
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

        protected AbstractWindowActionHandler _instantiateNumericTextBoxPropertyPasteActionHandler(
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

        public AbstractWindowActionHandler _frameNameSavingActionHandler()
        {
            return new WindowRuneingEditorFrameNameSavingActionHandlerFacade(
                GetSystemWindow(),
                RuneingSelectedFrameTextBox,
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

        private AbstractWindowActionHandler _instantiateFramePointMacrosSavingActionHandler()
        {
            return new WindowRuneingEditorFramePointMacrosSavingActionHandlerFacade(
                GetSystemWindow(),
                RuneingPointsListBox,
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
                _framePointMacroCommandsScaleRegistry
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
                _framePointMacroCommandsScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointMacroCommandRemoveActionHandler()
        {
            return new WindowRuneingEditorFramePointMacroCommandRemoveActionHandlerFacade(
                RuneingPointMacroRemoveCommandButton,
                RuneingPointsMacroListBox,
                _framePointMacroCommandsScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointMacroCommandClearActionHandler()
        {
            return new WindowRuneingEditorFramePointMacroCommandClearActionHandlerFacade(
                RuneingPointMacroClearCommandsButton,
                RuneingPointsMacroListBox,
                _framePointMacroCommandsScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointLoadConfigurationActionHandler()
        {
            return new WindowRuneingEditorFramePointLoadConfigurationActionHandlerFacade(
                FramePointsLoadButton,
                _loadFramePointsFileDialog
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointLoadActionHandler()
        {
            return new WindowRuneingEditorFramePointLoadActionHandlerFacade(
                _loadFramePointsFileDialog,
                RuneingPointsMacroListBox,
                RuneingPointMacroComboBoxTemplate,
                _framePointMacroCommandsScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateFramePointSaveActionHandler()
        {
            return new WindowRuneingEditorFramePointSaveActionHandlerFacade(
                FramePointsSaveButton,
                RuneingPointsMacroListBox,
                _saveFramePointsFileDialog
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsLoadingActionHandler()
        {
            return new WindowRuneingEditorMovementsLoadingActionHandlerFacade(
                RuneingMovementsListBox,
                RuneingPointMacroTemplate,
                GetSystemWindow(),
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsSavingActionHandler()
        {
            return new WindowRuneingEditorMovementsSavingActionHandlerFacade(
                GetSystemWindow(),
                RuneingMovementsListBox,
                _editMenuState
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
                _movementCommandsScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsCommandRemoveActionHandler()
        {
            return new WindowRuneingEditorMovementsCommandRemoveActionHandlerFacade(
                MovementRemoveMacroCommandButton,
                RuneingMovementsMacroListBox,
                _movementCommandsScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsCommandClearActionHandler()
        {
            return new WindowRuneingEditorMovementsCommandClearActionHandlerFacade(
                MovementClearMacroCommandsButton,
                RuneingMovementsMacroListBox,
                _movementCommandsScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsDeselectionActionHandler()
        {
            return new WindowRuneingEditorMovementsDeselectionActionHandlerFacade(
                DirectionComboBox,
                DistanceTextBox,
                RuneingMovementsListBox,
                RuneingMovementsMacroListBox,
                _movementCommandsScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsSelectionActionHandler()
        {
            return new WindowRuneingEditorMovementsSelectionActionHandlerFacade(
                DirectionComboBox,
                DistanceTextBox,
                RuneingMovementsListBox,
                RuneingMovementsMacroListBox,
                RuneingMovementComboBoxTemplate,
                _movementCommandsScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsLoadConfigurationActionHandler()
        {
            return new WindowRuneingEditorMovementsLoadConfigurationActionHandlerFacade(
                FrameMovementsLoadButton,
                _loadMovementsFileDialog
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsLoadActionHandler()
        {
            return new WindowRuneingEditorMovementsLoadActionHandlerFacade(
                _loadMovementsFileDialog,
                RuneingMovementsMacroListBox,
                RuneingMovementComboBoxTemplate,
                _movementCommandsScaleRegistry
            );
        }

        private AbstractWindowActionHandler _instantiateMovementsSaveActionHandler()
        {
            return new WindowRuneingEditorMovementsSaveActionHandlerFacade(
                FrameMovementsSaveButton,
                RuneingMovementsMacroListBox,
                _saveMovementsFileDialog
            );
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateNumericTextBoxPropertyActionHandler(RuneingRadiusTextBox, 999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(RuneingRadiusTextBox, 999),
                _instantiateNumericTextBoxPropertyActionHandler(DistanceTextBox, 999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(DistanceTextBox, 999),
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateMovementDirectionComboBoxScaleActionHandler(),
                _instantiateFramePointMacrosLoadingActionHandler(),
                _instantiateMovementsLoadingActionHandler(),
                _frameNameLoadingActionHandler(),
                _instantiateFramePointMacrosSavingActionHandler(),
                _instantiateMovementsSavingActionHandler(),
                _frameNameSavingActionHandler(),
                _instantiateFramePointMacroDeselectionActionHandler(),
                _instantiateFramePointMacroSelectionActionHandler(),
                _instantiateFramePointMacroCommandAddActionHandler(),
                _instantiateFramePointMacroCommandRemoveActionHandler(),
                _instantiateFramePointMacroCommandClearActionHandler(),
                _instantiateFramePointLoadConfigurationActionHandler(),
                _instantiateFramePointLoadActionHandler(),
                _instantiateFramePointSaveActionHandler(),
                _instantiateFramePointMacroAccessActionHandler(),
                _instantiateMovementAddActionHandler(),
                _instantiateMovementRemoveActionHandler(),
                _instantiateMovementsCommandAddActionHandler(),
                _instantiateMovementsCommandRemoveActionHandler(),
                _instantiateMovementsCommandClearActionHandler(),
                _instantiateMovementsDeselectionActionHandler(),
                _instantiateMovementsSelectionActionHandler(),
                _instantiateMovementsLoadConfigurationActionHandler(),
                _instantiateMovementsLoadActionHandler(),
                _instantiateMovementsSaveActionHandler(),
                _instantiateMovementMacroAccessActionHandler(),
            ];
        }
    }
}
