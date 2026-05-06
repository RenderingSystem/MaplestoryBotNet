using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet.Xaml
{
    public partial class RuneSolverWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        public RuneSolverWindow()
        {
            InitializeComponent();
        }

        private AbstractWindowActionHandler _instantiateNumericTextBoxPropertyActionHandler(
            TextBox numericTextBox, int maxValue
        )
        {
            return (
                new NumericTextBoxValidationActionHandlerBuilder()
                    .WithArgs(maxValue)
                    .WithArgs(numericTextBox)
                    .Build()
            );
        }

        private AbstractWindowActionHandler _instantiateNumericTextBoxPropertyPasteActionHandler(
            TextBox numericTextBox, int maxValue
        )
        {
            return (
                new NumericTextBoxPasteValidationActionHandlerBuilder()
                    .WithArgs(maxValue)
                    .WithArgs(numericTextBox)
                    .Build()
            );
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
                InteractKeyTextBox,
                CashShopKey,
                CashShopTimeout,
                RuneRetriesTextBox
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
                InteractKeyTextBox,
                CashShopKey,
                CashShopTimeout,
                RuneRetriesTextBox
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
                _instantiateNumericTextBoxPropertyActionHandler(CashShopTimeout, 999),
                _instantiateNumericTextBoxPropertyActionHandler(RuneRetriesTextBox, 99),
                _instantiateNumericTextBoxPropertyPasteActionHandler(CashShopTimeout, 999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(RuneRetriesTextBox, 99),
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
