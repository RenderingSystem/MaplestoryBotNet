using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet
{
    public partial class AilmentsWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        public AilmentsWindow()
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

        private AbstractWindowActionHandler _instantiateWindowAilmentsLoadActionHandler()
        {
            return new WindowAilmentsLoadActionHandlerFacade(
                AilmentsListBox,
                AilmentsCheckboxTemplate,
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateWindowAilmentsLoadImagesActionHandler()
        {
            return new WindowAilmentsLoadImagesActionHandlerFacade(
                AilmentsListBox,
                AilmentsImageGrid,
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateWindowAilmentsAnimationActionHandler()
        {
            return new WindowAilmentsAnimationActionHandlerFacade(
                AilmentsListBox,
                GetSystemWindow(),
                1.0 / 15.0
            );
        }

        private AbstractWindowActionHandler _instantiateWindowAilmentsLoadConfigurationActionHandler()
        {
            return new WindowAilmentsLoadConfigurationActionHandlerFacade(
                MacroDelayTextBox,
                CheckDelayTextBox,
                DetectThresholdTextBox,
                AllCureKeyTextBox,
                DetectRectangleLeft,
                DetectRectangleTop,
                DetectRectangleRight,
                DetectRectangleBottom,
                AilmentsListBox
            );
        }

        private AbstractWindowActionHandler _instantiateWindowAilmentsSaveConfigurationActionHandler()
        {
            return new WindowAilmentsSaveConfigurationActionHandlerFacade(
                MacroDelayTextBox,
                CheckDelayTextBox,
                DetectThresholdTextBox,
                AllCureKeyTextBox,
                DetectRectangleLeft,
                DetectRectangleTop,
                DetectRectangleRight,
                DetectRectangleBottom,
                AilmentsListBox
            );
        }

        private AbstractWindowActionHandler _instantiateWindowAilmentsCollapseActionHandler()
        {
            return new WindowAilmentsCollapseActionHandlerFacade(
                MacroDelayGrid,
                CheckDelayGrid,
                DetectThresholdGrid,
                AllCureKeyGrid,
                DetectRectangleGrid,
                AilmentsListBox
            );
        }

        private AbstractWindowActionHandler _instantiateWindowAilmentsSaveActionHandler()
        {
            return new WindowAilmentsSaveActionHandlerFacade(
                AilmentsListBox,
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateWindowAilmentsSaveCheckboxActionHandler()
        {
            return new WindowAilmentsSaveCheckboxActionHandlerFacade(
                AilmentsListBox,
                GetSystemWindow()
            );
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateNumericTextBoxPropertyActionHandler(MacroDelayTextBox, 99999),
                _instantiateNumericTextBoxPropertyActionHandler(CheckDelayTextBox, 99999),
                _instantiateNumericTextBoxPropertyActionHandler(DetectThresholdTextBox, 999),
                _instantiateNumericTextBoxPropertyActionHandler(DetectRectangleLeft, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(DetectRectangleTop, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(DetectRectangleRight, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(DetectRectangleBottom, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(MacroDelayTextBox, 99999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(CheckDelayTextBox, 99999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(DetectThresholdTextBox, 999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(DetectRectangleLeft, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(DetectRectangleTop, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(DetectRectangleRight, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(DetectRectangleBottom, 9999),
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateWindowAilmentsSaveConfigurationActionHandler(),
                _instantiateWindowAilmentsLoadActionHandler(),
                _instantiateWindowAilmentsLoadImagesActionHandler(),
                _instantiateWindowAilmentsLoadConfigurationActionHandler(),
                _instantiateWindowAilmentsAnimationActionHandler(),
                _instantiateWindowAilmentsCollapseActionHandler(),
                _instantiateWindowAilmentsSaveCheckboxActionHandler(),
                _instantiateWindowAilmentsSaveActionHandler()
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
