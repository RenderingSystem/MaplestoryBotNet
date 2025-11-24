using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;


namespace MaplestoryBotNet.Xaml
{
    public partial class MacroBottingWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        public MacroBottingWindow()
        {
            InitializeComponent();
            MacroListBox.Items.Clear();
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
                LoadButton, MacroListBox, ComboBoxTemplate
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
