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
        private MainWindow _mainWindow;

        private ISystemInjectable _keyboardDeviceContextInjectable;

        public SplashScreen(
            MainWindow mainWindow, ISystemInjectable keyboardDeviceContextInjectable)
        {
            _mainWindow = mainWindow;
            _keyboardDeviceContextInjectable = keyboardDeviceContextInjectable;
            InitializeComponent();
        }

        public AbstractWindowActionHandler InstantiateSplashScreenActionHandler()
        {
            Debug.Assert(_mainWindow != null);
            return new WindowSplashScreenCompleteActionHandlerBuilder()
                .WithArgs(new SystemWindow(this))
                .WithArgs(new SystemWindow(_mainWindow))
                .WithArgs(new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background))
                .WithArgs(_keyboardDeviceContextInjectable)
                .Build();
        }
    }
}
 