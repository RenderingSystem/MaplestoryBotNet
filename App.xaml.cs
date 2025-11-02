using MaplestoryBotNet.Systems;
using MaplestoryBotNet.UserInterface;
using MaplestoryBotNet.Xaml;
using System.Diagnostics;
using System.Windows;

namespace MaplestoryBotNet
{
    public partial class App : Application
    {
        AbstractApplication? _mainApplication = null;

        AbstractApplicationInitializer? _mainInitializer = null;

        MainWindow? _mainWindow = null;

        SplashScreen? _splashScreen = null;

        MacroBottingWindow? _windowMacroPopup = null;

        AbstractWindowActionHandler? _windowViewUpdaterActionHandler = null;

        AbstractWindowActionHandler? _windowViewCheckboxActionHandler = null;

        AbstractWindowActionHandler? _windowSplashScreenActionHandler = null;

        AbstractWindowActionHandler? _windowMacroMenuItemPopupHandler = null;

        AbstractWindowActionHandler? _windowMacroMenuItemVisibilityHandler = null;

        AbstractWindowActionHandler? _windowExiter = null;

        AbstractWindowActionHandler? _applicationClosingActionHandler = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _mainWindow = CreateMainWindow();
            _windowMacroPopup = CreateMacroBottingWindow();
            _mainApplication = CreateMainApplication();
            _splashScreen = CreateSplashScreen();
            _splashScreen.Show();
            CreateMainWindowActionHandlers();
            Initialize();
        }

        protected MainWindow CreateMainWindow()
        {
            return new MainWindow();
        }

        protected MacroBottingWindow CreateMacroBottingWindow()
        {
            return new MacroBottingWindow();
        }

        protected AbstractApplication CreateMainApplication()
        {
            return new MainApplicationFacade();
        }

        protected SplashScreen CreateSplashScreen()
        {
            Debug.Assert(_mainWindow != null);
            Debug.Assert(_mainApplication != null);
            return new SplashScreen(_mainApplication.System());
        }

        protected AbstractApplicationInitializer CreateApplicationInitializer(
        )
        {
            Debug.Assert(_mainApplication != null);
            Debug.Assert(_windowViewUpdaterActionHandler != null);
            Debug.Assert(_windowViewCheckboxActionHandler != null);
            Debug.Assert(_windowSplashScreenActionHandler != null);
            return new MainApplicationInitializerBuilder()
                .WithApplication(_mainApplication)
                .WithViewUpdaterActionHandler(_windowViewUpdaterActionHandler)
                .WithViewCheckboxActionHandler(_windowViewCheckboxActionHandler)
                .WithSplashScreenCompleteActionHandler(_windowSplashScreenActionHandler)
                .Build();
        }
        public AbstractWindowActionHandler InstantiateApplicationClosingActionHandler(
            List<AbstractSystemWindow> closingSystemWindows
        )
        {
            Debug.Assert(_mainWindow != null);
            return new ApplicationClosingActionHandlerBuilder()
                .WithArgs(_mainWindow.GetSystemWindow())
                .WithArgs(closingSystemWindows)
                .Build();
        }

        protected void CreateMainWindowActionHandlers()
        {
            Debug.Assert(_mainWindow != null);
            Debug.Assert(_splashScreen != null);
            Debug.Assert(_windowMacroPopup != null);
            _windowViewUpdaterActionHandler = _mainWindow.InstantiateWindowViewUpdaterActionHandler();
            _windowViewCheckboxActionHandler = _mainWindow.InstantiateWindowViewCheckboxActionHandler();
            _windowMacroMenuItemPopupHandler = _mainWindow.InstantiateMacroWindowMenuItemPopupHandler(_windowMacroPopup.GetSystemWindow());
            _windowSplashScreenActionHandler = _splashScreen.InstantiateSplashScreenActionHandler(_mainWindow.GetSystemWindow());
            _windowMacroMenuItemVisibilityHandler = _windowMacroPopup.InstantiateWindowMenuItemHideHandler();
            _applicationClosingActionHandler = InstantiateApplicationClosingActionHandler([_windowMacroPopup.GetSystemWindow()]);
            _windowExiter = _mainWindow.InstantiateWindowExiter();
        }

        protected void Initialize()
        {
            Debug.Assert(_mainApplication != null);
            _mainInitializer = CreateApplicationInitializer();
            _mainApplication.Launch([]);
            _mainInitializer.Synchronize();
            _mainInitializer.Initialize();
        }
    }

}
