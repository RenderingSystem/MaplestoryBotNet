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

        public AbstractWindowActionHandler InstantiateWindowMenuItemHideActionHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }
        
        public AbstractWindowActionHandler InstantiateLoadMenuActionHandler()
        {
            return new WindowLoadMenuActionHandlerFacade(
                LoadButton,
                MacroListBox,
                ComboBoxTemplate,
                _comboBoxScaleRegistry
            );
        }

        public AbstractWindowActionHandler InstantiateWindowAddMacroCommandActionHandler()
        {
            return new WindowAddMacroCommandActionHandlerFacade(
                AddButton,
                MacroListBox,
                ComboBoxTemplate,
                _comboBoxScaleRegistry
            );
        }

        public AbstractWindowActionHandler InstantiateWindowRemoveMacroCommandActionHandler()
        {
            return new WindowRemoveMacroCommandActionHandlerFacade(
                RemoveButton,
                MacroListBox,
                _comboBoxScaleRegistry
            );
        }

        public AbstractWindowActionHandler InstantiateWindowClearMacroCommandActionHandler()
        {
            return new WindowClearMacroCommandsActionHandlerFacade(
                ClearButton,
                MacroListBox,
                _comboBoxScaleRegistry
            );
        }

        public AbstractWindowActionHandler InstantiateSaveMenuActionHandler()
        {
            return new WindowSaveMenuActionHandlerFacade(
                SaveButton, MacroListBox
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
    }
}
