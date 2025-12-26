using MaplestoryBotNet.LibraryWrappers;
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

        MacroBottingWindow? _windowMacroPopup = null;

        List<AbstractWindowActionHandler> _uiHandlers = [];

        AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _editMenuState = new WindowMapEditMenuState();
            _mainWindow = new MainWindow();
            _windowMacroPopup = new MacroBottingWindow(_editMenuState);
            _mainApplication = new MainApplicationFacade();
            _mapWindow = new MapWindow(_editMenuState);
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
                .. _windowMacroPopup!.InstantiateActionHandlers(),
                .. _splashScreen!.InstantiateActionHandlers(_mainWindow!.GetSystemWindow()),
                .. _mainWindow!.InstantiateActionHandlers(_mapWindow!.GetSystemWindow()),
                .. _mapWindow!.InstantiateActionHandlers(_windowMacroPopup.GetSystemWindow()),
                InstantiateApplicationClosingActionHandler()
            ];
        }

        protected List<AbstractSystemWindow> GetPopupWindows()
        {
            return [
                _windowMacroPopup!.GetSystemWindow(),
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
