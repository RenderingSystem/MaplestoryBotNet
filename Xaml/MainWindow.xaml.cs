using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.UserInterface;


namespace MaplestoryBotNet
{
    public partial class MainWindow : Window
    {
        protected List<MenuItem> ViewMenuItems
        {
            get
            {
                return [SnapshotsMenuItem, MinimapMenuItem, NCCMenuItem];
            }
        }

        public AbstractWindowActionHandler InstantiateWindowExiter()
        {
            return new WindowExitActionHandlerBuilder()
                .WithArgs(new SystemWindow(this))
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

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
