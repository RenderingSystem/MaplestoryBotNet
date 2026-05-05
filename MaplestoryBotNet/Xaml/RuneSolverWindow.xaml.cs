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

        private AbstractWindowActionHandler _instantiateAPILoadActionHandler()
        {
            return new WindowRuneSolverAPILoadActionHandlerFacade(
                GetSystemWindow(),
                IPAddressTextbox,
                PortTextBox,
                RouteTextBox,
                ClassTagTextBox,
                ArrowLeftTextBox,
                ArrowUpTextBox,
                ArrowRightTextBox,
                ArrowDownTextBox,
                InteractKeyTextBox
            );
        }

        private AbstractWindowActionHandler _instantiateAPISaveActionHandler()
        {
            return new WindowRuneSolverAPISaveActionHandlerFacade(
                GetSystemWindow(),
                IPAddressTextbox,
                PortTextBox,
                RouteTextBox,
                ClassTagTextBox,
                ArrowLeftTextBox,
                ArrowUpTextBox,
                ArrowRightTextBox,
                ArrowDownTextBox,
                InteractKeyTextBox
            );
        }

        private AbstractWindowActionHandler _instantiateAPIInjectActionHandler()
        {
            return new WindowRuneSolverAPIInjectActionHandlerFacade(
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateAPIOutputActionHandler()
        {
            return new WindowRuneSolverAPIOutputActionHandlerFacade(
                ClassTagTextBox,
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
                _instantiateAPILoadActionHandler(),
                _instantiateAPISaveActionHandler(),
                _instantiateAPIInjectActionHandler(),
                _instantiateAPIOutputActionHandler()
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
