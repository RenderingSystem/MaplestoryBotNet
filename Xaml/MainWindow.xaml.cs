using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace MaplestoryBotNet
{

    public partial class MainWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        protected List<MenuItem> ViewMenuItems
        {
            get
            {
                return [SnapshotsMenuItem, MinimapMenuItem, NCCMenuItem];
            }
        }

        private AbstractWindowActionHandler _instantiateWindowExiterActionHandler()
        {
            return new WindowExitActionHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .WithArgs(ExitMenuItem)
                .Build();
        }

        private AbstractWindowActionHandler _instantiateWindowViewUpdaterActionHandler()
        {
            return new WindowViewUpdaterActionHandlerBuilder()
                .WithArgs(ViewMenuItems)
                .WithArgs(ImageView)
                .WithArgs(new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background))
                .Build();
        }

        private AbstractWindowActionHandler _instantiateWindowViewCheckboxActionHandler()
        {
            return new WindowViewCheckboxActionHandlerBuilder()
                .WithArgs(ViewMenuItems)
                .Build();
        }

        private AbstractWindowActionHandler _instantiateMacroWindowMenuItemPopupActionHandler(
            AbstractSystemWindow systemWindow
        )
        {
            return new WindowMenuItemPopupHandlerBuilder()
                .WithArgs(systemWindow)
                .WithArgs(MacroMenuItem)
                .Build();
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers(
            AbstractSystemWindow systemWindow
        )
        {
            return [
                _instantiateWindowExiterActionHandler(),
                _instantiateWindowViewUpdaterActionHandler(),
                _instantiateWindowViewCheckboxActionHandler(),
                _instantiateMacroWindowMenuItemPopupActionHandler(systemWindow)
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

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
