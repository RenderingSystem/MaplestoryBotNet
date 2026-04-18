using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;


namespace MaplestoryBotNet.Xaml
{
    public partial class MacroRuneingWindow : Window
    {
        private AbstractWindowMapEditMenuState _editMenuState;

        private AbstractWindowActionHandlerRegistry _comboBoxScaleRegistry;

        private AbstractSystemWindow? _systemWindow;

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

        private AbstractWindowActionHandler _instantiateComboBoxScaleActionHandler()
        {
            return new WindowComboBoxScaleActionHandlerFacade(DirectionComboBox);
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
                    RuneingRadiusTextBox
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

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateComboBoxScaleActionHandler(),
                _instantiateFramePointMacrosLoadingActionHandler(),
                _instantiateFramePointMacroAccessActionHandler(),
                _instantiateFramePointMacroDeselectionActionHandler(),
                _instantiateFramePointMacroSelectionActionHandler(),
                _instantiateFramePointMacroCommandAddActionHandler()
            ];
        }
    }
}
