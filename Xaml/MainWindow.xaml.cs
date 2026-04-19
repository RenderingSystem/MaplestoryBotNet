using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;
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

        private AbstractWindowActionHandler _instantiateMacroWindowMenuItemPopupActionHandler(
            AbstractSystemWindow systemWindow
        )
        {
            return new WindowMenuItemPopupHandlerBuilder()
                .WithArgs(systemWindow)
                .WithArgs(MacroMenuItem)
                .Build();
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemTextActionHandler()
        {
            return new WindowMenuItemStartTextActionHandlerFacade(
                StartMenuItem,
                new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background)
            );
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemStartActionHandler()
        {
            return new WindowMenuItemStartActionHandlerFacade(StartMenuItem);
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers(
            AbstractSystemWindow systemWindow
        )
        {
            return [
                _instantiateWindowExiterActionHandler(),
                _instantiateWindowViewUpdaterActionHandler(),
                _instantiateMacroWindowMenuItemPopupActionHandler(systemWindow),
                _instantiateWindowMenuItemTextActionHandler(),
                _instantiateWindowMenuItemStartActionHandler()
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
