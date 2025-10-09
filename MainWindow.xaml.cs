using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.UserInterface;


namespace MaplestoryBotNet
{
    public partial class MainWindow : Window
    {
        AbstractApplication? _mainApplication;

        AbstractApplicationInitializer? _mainInitializer;

        AbstractWindowActionHandler? _windowExitActionHandler;

        AbstractWindowActionHandler? _windowViewUpdaterActionHandler;

        AbstractWindowActionHandler? _windowViewCheckboxActionHandler;

        protected List<MenuItem> ViewMenuItems
        {
            get
            {
                return [SnapshotsMenuItem, MinimapMenuItem, NCCMenuItem];
            }
        }

        public void InitializeMainApplication()
        {
            _mainApplication = new MainApplicationFacade();
        }

        public void InitializeWindowExiter()
        {
            _windowExitActionHandler = new WindowExitActionHandlerBuilder()
                .WithArgs(this)
                .WithArgs(ExitMenuItem)
                .Build();
        }

        public void InitializeWindowViewModifier()

        {
            _windowViewUpdaterActionHandler = new WindowViewUpdaterActionHandlerBuilder()
                .WithArgs(ViewMenuItems)
                .WithArgs(ImageView)
                .WithArgs(Dispatcher)
                .Build();
        }

        public void InitializeWindowViewCheckbox()
        {
            _windowViewCheckboxActionHandler = new WindowViewCheckboxActionHandlerBuilder()
                .WithArgs(ViewMenuItems)
                .Build();
        }

        public void InitializeApplicationInitializer()
        {
            Debug.Assert(_mainApplication != null);
            Debug.Assert(_windowViewUpdaterActionHandler != null);
            Debug.Assert(_windowViewCheckboxActionHandler != null);
            _mainInitializer = new MainApplicationInitializerBuilder()
                .WithApplication(_mainApplication)
                .WithViewUpdaterActionHandler(_windowViewUpdaterActionHandler)
                .WithViewCheckboxActionHandler(_windowViewCheckboxActionHandler)
                .Build();
        }

        public void StartApplication()
        {
            Debug.Assert(_mainApplication != null);
            Debug.Assert(_mainInitializer != null);
            _mainApplication.Launch([]);
            _mainInitializer.Synchronize();
            _mainInitializer.Initialize();
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeMainApplication();
            InitializeWindowExiter();
            InitializeWindowViewModifier();
            InitializeWindowViewCheckbox();
            InitializeApplicationInitializer();
            StartApplication();
        }
    }
}
