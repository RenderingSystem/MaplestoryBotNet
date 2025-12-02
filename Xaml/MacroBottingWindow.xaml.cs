using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet.Xaml
{
    public partial class MacroBottingWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        private WindowComboBoxScaleActionHandlerRegistry _comboBoxScaleRegistry;

        public MacroBottingWindow()
        {
            InitializeComponent();
            MacroListBox.Items.Clear();
            _comboBoxScaleRegistry = new WindowComboBoxScaleActionHandlerRegistry();
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

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateLoadMenuActionHandler(),
                _instantiateWindowAddMacroCommandActionHandler(),
                _instantiateWindowRemoveMacroCommandActionHandler(),
                _instantiateWindowClearMacroCommandActionHandler(),
                _instantiateSaveMenuActionHandler()
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
