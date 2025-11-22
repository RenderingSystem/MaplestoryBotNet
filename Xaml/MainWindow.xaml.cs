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

        public AbstractWindowActionHandler InstantiateWindowExiterActionHandler()
        {
            return new WindowExitActionHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .WithArgs(ExitMenuItem)
                .Build();
        }

        public AbstractWindowActionHandler InstantiateWindowViewUpdaterActionHandler()
        {
            return new WindowViewUpdaterActionHandlerBuilder()
                .WithArgs(ViewMenuItems)
                .WithArgs(ImageView)
                .WithArgs(new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background))
                .Build();
        }

        public AbstractWindowActionHandler InstantiateWindowViewCheckboxActionHandler()
        {
            return new WindowViewCheckboxActionHandlerBuilder()
                .WithArgs(ViewMenuItems)
                .Build();
        }

        public AbstractWindowActionHandler InstantiateMacroWindowMenuItemPopupActionHandler(
            AbstractSystemWindow systemWindow
        )
        {
            return new WindowMenuItemPopupHandlerBuilder()
                .WithArgs(systemWindow)
                .WithArgs(MacroMenuItem)
                .Build();
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
