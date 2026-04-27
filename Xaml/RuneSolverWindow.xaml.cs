using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;


namespace MaplestoryBotNet.Xaml
{
    public partial class RuneSolverWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        public RuneSolverWindow()
        {
            InitializeComponent();
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemHideActionHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }

        private AbstractWindowActionHandler _instantiateRoboflowAPILoadActionHandler()
        {
            return new WindowRuneSolverRoboflowAPILoadActionHandlerFacade(
                GetSystemWindow(),
                WorkspaceNameTextBox,
                WorkspaceIDTextbox,
                APIKeyTextBox,
                JsonTagArrayTextBox,
                JsonTagXTextBox,
                JsonTagYTextBox,
                ArrowLeftTextBox,
                ArrowUpTextBox,
                ArrowRightTextBox,
                ArrowDownTextBox
            );
        }

        private AbstractWindowActionHandler _instantiateRoboflowAPISaveActionHandler()
        {
            return new WindowRuneSolverRoboflowAPISaveActionHandlerFacade(
                GetSystemWindow(),
                WorkspaceNameTextBox,
                WorkspaceIDTextbox,
                APIKeyTextBox,
                JsonTagArrayTextBox,
                JsonTagXTextBox,
                JsonTagYTextBox,
                ArrowLeftTextBox,
                ArrowUpTextBox,
                ArrowRightTextBox,
                ArrowDownTextBox
            );
        }

        private AbstractWindowActionHandler _instantiateRoboflowAPIInjectActionHandler()
        {
            return new WindowRuneSolverRoboflowAPIInjectActionHandlerFacade(
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateRoboflowAPIOutputActionHandler()
        {
            return new WindowRuneSolverRoboflowAPIOutputActionHandlerFacade(
                JsonTagArrayTextBox,
                JsonTagXTextBox,
                JsonTagYTextBox,
                ArrowLeftTextBox,
                ArrowUpTextBox,
                ArrowRightTextBox,
                ArrowDownTextBox,
                OutputFormatTextBlock
            );
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateRoboflowAPILoadActionHandler(),
                _instantiateRoboflowAPISaveActionHandler(),
                _instantiateRoboflowAPIInjectActionHandler(),
                _instantiateRoboflowAPIOutputActionHandler()
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
