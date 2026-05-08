using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Xaml;
using System.Windows;


namespace MaplestoryBotNet
{
    public partial class App : Application
    {
        AbstractApplication? _mainApplication = null;

        AbstractApplicationInitializer? _mainInitializer = null;

        MainWindow? _mainWindow = null;

        SplashScreen? _splashScreen = null;

        MapWindow? _mapWindow = null;

        RuneSolverWindow? _runeSolverWindow = null;

        AilmentsWindow? _ailmentsWindow = null;

        MacroBottingWindow? _windowMacroBottingPopup = null;

        MacroRuneingWindow? _windowMacroRuneingPopup = null;

        List<AbstractWindowActionHandler> _uiHandlers = [];

        AbstractWindowMapEditMenuState _bottingEditMenuState = new WindowMapEditMenuState();

        AbstractWindowMapEditMenuState _runeingEditMenuState = new WindowMapEditMenuState();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _bottingEditMenuState = new WindowMapEditMenuState();
            _runeingEditMenuState = new WindowMapEditMenuState();
            _mainWindow = new MainWindow();
            _windowMacroBottingPopup = new MacroBottingWindow(_bottingEditMenuState);
            _windowMacroRuneingPopup = new MacroRuneingWindow(_runeingEditMenuState);
            _runeSolverWindow = new RuneSolverWindow();
            _ailmentsWindow = new AilmentsWindow();
            _mainApplication = new MainApplicationFacade();
            _mapWindow = new MapWindow(_bottingEditMenuState, _runeingEditMenuState);
            _splashScreen = new SplashScreen(_mainApplication.System());
            Initialize();
        }

        protected AbstractApplicationInitializer CreateApplicationInitializer(
        )
        {
            return new MainApplicationInitializerBuilder()
                .WithArgs(_mainApplication!)
                .WithArgs(_uiHandlers!)
                .Build();
        }

        public AbstractWindowActionHandler InstantiateApplicationClosingActionHandler()
        {
            return new ApplicationClosingActionHandlerBuilder()
                .WithArgs(_mainWindow!.GetSystemWindow())
                .WithArgs(GetPopupWindows())
                .Build();
        }

        protected List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                .. _windowMacroBottingPopup!.InstantiateActionHandlers(),
                .. _windowMacroRuneingPopup!.InstantiateActionHandlers(),
                .. _runeSolverWindow!.InstantiateActionHandlers(),
                .. _ailmentsWindow!.InstantiateActionHandlers(),
                .. _splashScreen!.InstantiateActionHandlers(
                    _mainWindow!.GetSystemWindow()
                ),
                .. _mainWindow!.InstantiateActionHandlers(
                    _mapWindow!.GetSystemWindow(),
                    _runeSolverWindow.GetSystemWindow(),
                    _ailmentsWindow.GetSystemWindow()
                ),
                .. _mapWindow!.InstantiateActionHandlers(
                    _windowMacroBottingPopup.GetSystemWindow(),
                    _windowMacroRuneingPopup.GetSystemWindow()
                ),
                InstantiateApplicationClosingActionHandler()
            ];
        }

        protected List<AbstractSystemWindow> GetPopupWindows()
        {
            return [
                _windowMacroBottingPopup!.GetSystemWindow(),
                _windowMacroRuneingPopup!.GetSystemWindow(),
                _runeSolverWindow!.GetSystemWindow(),
                _ailmentsWindow!.GetSystemWindow(),
                _mapWindow!.GetSystemWindow()
            ];
        }

        protected void Initialize()
        {
            _splashScreen!.Show();
            _uiHandlers = InstantiateActionHandlers();
            _mainInitializer = CreateApplicationInitializer();
            _mainApplication!.Launch();
            _mainInitializer.Synchronize();
            _mainInitializer.Initialize();
        }
    }
}
