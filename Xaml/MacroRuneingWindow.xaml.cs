using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;


namespace MaplestoryBotNet.Xaml
{
    public partial class MacroRuneingWindow : Window
    {
        private AbstractWindowMapEditMenuState _editMenuState;

        private AbstractSystemWindow? _systemWindow;
        public MacroRuneingWindow(AbstractWindowMapEditMenuState editMenuState)
        {
            _editMenuState = editMenuState;
            _systemWindow = null;
            InitializeComponent();
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

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateWindowMenuItemHideActionHandler()
            ];
        }
    }
}
