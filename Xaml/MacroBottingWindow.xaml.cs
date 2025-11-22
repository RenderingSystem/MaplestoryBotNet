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
        }

        public AbstractWindowActionHandler InstantiateWindowMenuItemHideActionHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }
        
        public AbstractWindowActionHandler InstantiateLoadMenuActionHandler()
        {
            var handler = new WindowLoadMenuActionHandlerFacade(
                LoadButton, MacroListBox, ComboBoxTemplate
            );
            MacroListBox.Items.Clear();
            return handler;
        }

        public AbstractWindowActionHandler InstantiateSaveMenuActionHandler()
        {
            return new WindowSaveMenuActionHandlerFacade(SaveButton, MacroListBox);
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
