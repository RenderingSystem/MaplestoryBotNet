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
                .WithArgs(ImageView)
                .WithArgs(new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background))
                .Build();
        }

        private AbstractWindowActionHandler _instantiateMapWindowMenuItemPopupActionHandler(
            AbstractSystemWindow mapWindow
        )
        {
            return new WindowMenuItemPopupHandlerBuilder()
                .WithArgs(mapWindow)
                .WithArgs(MacroMenuItem)
                .Build();
        }

        private AbstractWindowActionHandler _instantiateSolverWindowMenuItemPopupActionHandler(
            AbstractSystemWindow solverWindow
        )
        {
            return new WindowMenuItemPopupHandlerBuilder()
                .WithArgs(solverWindow)
                .WithArgs(RuneSolverMenuItem)
                .Build();
        }

        private AbstractWindowActionHandler _instantiateAilmentsWindowMenuItemPopupActionHandler(
            AbstractSystemWindow ailmentsWindow
        )
        {
            return new WindowMenuItemPopupHandlerBuilder()
                .WithArgs(ailmentsWindow)
                .WithArgs(AilmentsMenuItem)
                .Build();
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemTextActionHandler()
        {
            return new WindowMenuItemStartTextActionHandlerFacade(
                StartMenuItem,
                new SystemAsyncDispatcher(
                    Dispatcher,
                    DispatcherPriority.Background
                )
            );
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemStartActionHandler()
        {
            return new WindowMenuItemStartActionHandlerFacade(StartMenuItem);
        }

        private AbstractWindowActionHandler _instantiateWindowBottingTextStatusActionHandler()
        {
            return new WindowBottingTextStatusActionHandlerFacade(
                [
                    ResettingText,
                    IdleText,
                    BottingText,
                    RuneingText,
                    SolvingText,
                    SolvedCheckText,
                    CashShopText
                ],
                new SystemAsyncDispatcher(
                    Dispatcher,
                    DispatcherPriority.Background
                )
            );
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers(
            AbstractSystemWindow mapWindow,
            AbstractSystemWindow solverWindow,
            AbstractSystemWindow ailmentsWindow
        )
        {
            return [
                _instantiateWindowExiterActionHandler(),
                _instantiateWindowViewUpdaterActionHandler(),
                _instantiateMapWindowMenuItemPopupActionHandler(mapWindow),
                _instantiateSolverWindowMenuItemPopupActionHandler(solverWindow),
                _instantiateAilmentsWindowMenuItemPopupActionHandler(ailmentsWindow),
                _instantiateWindowMenuItemTextActionHandler(),
                _instantiateWindowMenuItemStartActionHandler(),
                _instantiateWindowBottingTextStatusActionHandler()
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
