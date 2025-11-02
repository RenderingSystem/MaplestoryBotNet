using MaplestoryBotNet.Systems;
using MaplestoryBotNet.UserInterface;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;


namespace MaplestoryBotNet
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
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

        public AbstractWindowActionHandler InstantiateSplashScreenActionHandler(
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
 