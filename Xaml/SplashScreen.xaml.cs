using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;
using System.Windows.Threading;


namespace MaplestoryBotNet
{
    public partial class SplashScreen : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        private ISystemInjectable _keyboardDeviceContextInjectable;

        public SplashScreen(
            ISystemInjectable keyboardDeviceContextInjectable
        )
        {
            _keyboardDeviceContextInjectable = keyboardDeviceContextInjectable;
            InitializeComponent();
        }

        private AbstractWindowActionHandler _instantiateSplashScreenActionHandler(
            AbstractSystemWindow mainSystemWindow
        )
        {
            return new WindowSplashScreenCompleteActionHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .WithArgs(mainSystemWindow)
                .WithArgs(new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background))
                .WithArgs(_keyboardDeviceContextInjectable)
                .Build();
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers(
            AbstractSystemWindow mainSystemWindow
        )
        {
            return [
                _instantiateSplashScreenActionHandler(mainSystemWindow)
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
