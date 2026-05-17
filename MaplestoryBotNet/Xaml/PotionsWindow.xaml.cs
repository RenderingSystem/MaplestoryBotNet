using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace MaplestoryBotNet.Xaml
{
    public partial class PotionsWindow : Window
    {
        private AbstractSystemWindow? _systemWindow;

        private AbstractPotionsMenuState _hpPotionsMenuState;

        private AbstractPotionsMenuState _mpPotionsMenuState;

        public PotionsWindow()
        {
            _hpPotionsMenuState = new PotionsMenuState();
            _mpPotionsMenuState = new PotionsMenuState();
            InitializeComponent();
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemHideActionHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }

        private AbstractWindowActionHandler _instantiateLoadingHpResourceActionHandler()
        {
            return new WindowPotionsMenuLoadingResourceActionHandlerFacade(
                PotionResourceType.Health,
                HealthPixelThresholdColorRTextBox,
                HealthPixelThresholdColorGTextBox,
                HealthPixelThresholdColorBTextBox,
                HealthPixelToleranceRTextBox,
                HealthPixelToleranceGTextBox,
                HealthPixelToleranceBTextBox,
                HealthPixelXTextBox,
                HealthPixelYTextBox,
                HealthLeftTextBox,
                HealthTopTextBox,
                HealthRightTextBox,
                HealthBotTextBox,
                HealthKeyTextBox,
                HealthActiveCheckBox,
                _hpPotionsMenuState,
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateSavingHpResourceActionHandler()
        {
            return new WindowPotionsMenuSavingResourceActionHandlerFacade(
                PotionResourceType.Health,
                HealthPixelThresholdColorRTextBox,
                HealthPixelThresholdColorGTextBox,
                HealthPixelThresholdColorBTextBox,
                HealthPixelToleranceRTextBox,
                HealthPixelToleranceGTextBox,
                HealthPixelToleranceBTextBox,
                HealthPixelXTextBox,
                HealthPixelYTextBox,
                HealthLeftTextBox,
                HealthTopTextBox,
                HealthRightTextBox,
                HealthBotTextBox,
                HealthKeyTextBox,
                HealthActiveCheckBox,
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateLoadingMpResourceActionHandler()
        {
            return new WindowPotionsMenuLoadingResourceActionHandlerFacade(
                PotionResourceType.Mana,
                ManaPixelThresholdColorRTextBox,
                ManaPixelThresholdColorGTextBox,
                ManaPixelThresholdColorBTextBox,
                ManaPixelToleranceRTextBox,
                ManaPixelToleranceGTextBox,
                ManaPixelToleranceBTextBox,
                ManaPixelXTextBox,
                ManaPixelYTextBox,
                ManaLeftTextBox,
                ManaTopTextBox,
                ManaRightTextBox,
                ManaBotTextBox,
                ManaKeyTextBox,
                ManaActiveCheckBox,
                _mpPotionsMenuState,
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateSavingMpResourceActionHandler()
        {
            return new WindowPotionsMenuSavingResourceActionHandlerFacade(
                PotionResourceType.Mana,
                ManaPixelThresholdColorRTextBox,
                ManaPixelThresholdColorGTextBox,
                ManaPixelThresholdColorBTextBox,
                ManaPixelToleranceRTextBox,
                ManaPixelToleranceGTextBox,
                ManaPixelToleranceBTextBox,
                ManaPixelXTextBox,
                ManaPixelYTextBox,
                ManaLeftTextBox,
                ManaTopTextBox,
                ManaRightTextBox,
                ManaBotTextBox,
                ManaKeyTextBox,
                ManaActiveCheckBox,
                GetSystemWindow()
            );
        }

        public AbstractWindowActionHandler _instantiateTextBoxHpFrameRGBActionHandler()
        {
            return new WindowPotionsMenuTextBoxFrameRGBActionHandlerFacade(
                HealthPixelThresholdColorRTextBox,
                HealthPixelThresholdColorGTextBox,
                HealthPixelThresholdColorBTextBox,
                HealthPixelThresholdColorR,
                HealthPixelThresholdColorG,
                HealthPixelThresholdColorB
            );
        }

        public AbstractWindowActionHandler _instantiateTextBoxMpFrameRGBActionHandler()
        {
            return new WindowPotionsMenuTextBoxFrameRGBActionHandlerFacade(
                ManaPixelThresholdColorRTextBox,
                ManaPixelThresholdColorGTextBox,
                ManaPixelThresholdColorBTextBox,
                ManaPixelThresholdColorR,
                ManaPixelThresholdColorG,
                ManaPixelThresholdColorB
            );
        }

        public AbstractWindowActionHandler _isntantiateHpBarActionHandler()
        {
            return new WindowPotionsMenuResourceBarActionHandlerFacade(
                PotionResourceType.Health,
                HealthLeftTextBox,
                HealthTopTextBox,
                HealthRightTextBox,
                HealthBotTextBox,
                HealthImage,
                new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background),
                GetSystemWindow()
            );
        }

        public AbstractWindowActionHandler _isntantiateMpBarActionHandler()
        {
            return new WindowPotionsMenuResourceBarActionHandlerFacade(
                PotionResourceType.Mana,
                ManaLeftTextBox,
                ManaTopTextBox,
                ManaRightTextBox,
                ManaBotTextBox,
                ManaImage,
                new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background),
                GetSystemWindow()
            );
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

        private AbstractWindowActionHandler _instantiateHpRGBLabelActionHandler()
        {
            return (
                new WindowPotionsMenuRGBLabelActionHandlerFacade(
                    PotionResourceType.Health,
                    HealthPixelXTextBox,
                    HealthPixelYTextBox,
                    HealthLeftTextBox,
                    HealthTopTextBox,
                    HealthPixelColorRLabel,
                    HealthPixelColorGLabel,
                    HealthPixelColorBLabel,
                    new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background),
                    GetSystemWindow()
                )
            );
        }

        private AbstractWindowActionHandler _instantiateMpRGBLabelActionHandler()
        {
            return (
                new WindowPotionsMenuRGBLabelActionHandlerFacade(
                    PotionResourceType.Mana,
                    ManaPixelXTextBox,
                    ManaPixelYTextBox,
                    ManaLeftTextBox,
                    ManaTopTextBox,
                    ManaPixelColorRLabel,
                    ManaPixelColorGLabel,
                    ManaPixelColorBLabel,
                    new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background),
                    GetSystemWindow()
                )
            );
        }

        private AbstractWindowActionHandler _instantiateHpRGBFrameActionHandler()
        {
            return (
                new WindowPotionsMenuRGBFrameActionHandlerFacade(
                    HealthPixelColorRLabel,
                    HealthPixelColorGLabel,
                    HealthPixelColorBLabel,
                    HealthPixelColorR,
                    HealthPixelColorG,
                    HealthPixelColorB,
                    HealthPixelColor
                )
            );
        }

        private AbstractWindowActionHandler _instantiateMpRGBFrameActionHandler()
        {
            return (
                new WindowPotionsMenuRGBFrameActionHandlerFacade(
                    ManaPixelColorRLabel,
                    ManaPixelColorGLabel,
                    ManaPixelColorBLabel,
                    ManaPixelColorR,
                    ManaPixelColorG,
                    ManaPixelColorB,
                    ManaPixelColor
                )
            );
        }

        private AbstractWindowActionHandler _instantiateHpConfigurationUpdateActionHandler()
        {
            return (
                new WindowPotionsMenuConfigurationUpdateActionHandlerFacade(
                    PotionResourceType.Health,
                    HealthPixelThresholdColorRTextBox,
                    HealthPixelThresholdColorGTextBox,
                    HealthPixelThresholdColorBTextBox,
                    HealthPixelToleranceRTextBox,
                    HealthPixelToleranceGTextBox,
                    HealthPixelToleranceBTextBox,
                    HealthPixelXTextBox,
                    HealthPixelYTextBox,
                    HealthLeftTextBox,
                    HealthTopTextBox,
                    HealthRightTextBox,
                    HealthBotTextBox,
                    HealthKeyTextBox,
                    HealthActiveCheckBox,
                    _hpPotionsMenuState
                )
            );
        }

        private AbstractWindowActionHandler _instantiateMpConfigurationUpdateActionHandler()
        {
            return (
                new WindowPotionsMenuConfigurationUpdateActionHandlerFacade(
                    PotionResourceType.Mana,
                    ManaPixelThresholdColorRTextBox,
                    ManaPixelThresholdColorGTextBox,
                    ManaPixelThresholdColorBTextBox,
                    ManaPixelToleranceRTextBox,
                    ManaPixelToleranceGTextBox,
                    ManaPixelToleranceBTextBox,
                    ManaPixelXTextBox,
                    ManaPixelYTextBox,
                    ManaLeftTextBox,
                    ManaTopTextBox,
                    ManaRightTextBox,
                    ManaBotTextBox,
                    ManaKeyTextBox,
                    ManaActiveCheckBox,
                    _mpPotionsMenuState
                )
            );
        }

        private AbstractWindowActionHandler _instantiateSaveConfigurationActionHandler()
        {
            return (
                new WindowPotionsMenuSaveConfigurationActionHandlerFacade(
                    ConsumablesListBox,
                    GetSystemWindow()
                )
            );
        }

        private AbstractWindowActionHandler _instantiateLoadingConsumablesActionHandler()
        {
            return (
                new WindowPotionsMenuLoadingConsumablesActionHandlerFacade(
                    ConsumablesListBox,
                    ConsumableCheckboxTemplate,
                    GetSystemWindow()
                )
            );
        }

        private AbstractWindowActionHandler _instantiateConsumableSelectedActionHandler()
        {
            return new WindowPotionsMenuConsumableSelectedActionHandlerFacade(
                ConsumableMinDelayTextBox,
                ConsumableMaxDelayTextBox,
                ConsumableKeyTextBox,
                ConsumablesListBox
            );
        }

        private AbstractWindowActionHandler _instantiateConsumableDeselectedActionHandler()
        {
            return new WindowPotionsMenuConsumableDeselectedActionHandlerFacade(
                ConsumableMinDelayTextBox,
                ConsumableMaxDelayTextBox,
                ConsumableKeyTextBox,
                ConsumablesListBox
            );
        }

        private AbstractWindowActionHandler _instantiateConsumableItemSaveActionHandler()
        {
            return new WindowPotionsMenuConsumableItemSaveActionHandlerFacade(
                ConsumablesListBox,
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateConsumableRemoveActionHandler()
        {
            return new WindowPotionsMenuConsumableRemoveActionHandlerFacade(
                ConsumablesListBox,
                ConsumableRemoveButton
            );
        }

        private AbstractWindowActionHandler _instantiateConsumableAddActionHandler()
        {
            return new WindowPotionsMenuConsumableAddActionHandlerFacade(
                ConsumableAddButton,
                ConsumablesListBox,
                ConsumableCheckboxTemplate
            );
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthPixelThresholdColorRTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthPixelThresholdColorGTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthPixelThresholdColorBTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthPixelToleranceRTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthPixelToleranceGTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthPixelToleranceBTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthPixelXTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthPixelYTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthLeftTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthTopTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthRightTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthBotTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(HealthKeyTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaPixelThresholdColorRTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaPixelThresholdColorGTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaPixelThresholdColorBTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaPixelToleranceRTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaPixelToleranceGTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaPixelToleranceBTextBox, 255),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaPixelXTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaPixelYTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaLeftTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaTopTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaRightTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaBotTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ManaKeyTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ConsumableMinDelayTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(ConsumableMaxDelayTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(HealthPixelThresholdColorRTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(HealthPixelThresholdColorGTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(HealthPixelThresholdColorBTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(HealthPixelToleranceRTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(HealthPixelToleranceGTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(HealthPixelToleranceBTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(HealthPixelXTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(HealthPixelYTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(HealthLeftTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(HealthTopTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(HealthRightTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(HealthBotTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(HealthKeyTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(ManaPixelThresholdColorRTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(ManaPixelThresholdColorGTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(ManaPixelThresholdColorBTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(ManaPixelToleranceRTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(ManaPixelToleranceGTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(ManaPixelToleranceBTextBox, 255),
                _instantiateNumericTextBoxPropertyActionHandler(ManaPixelXTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(ManaPixelYTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(ManaLeftTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(ManaTopTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(ManaRightTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(ManaBotTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(ManaKeyTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(ConsumableMinDelayTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(ConsumableMaxDelayTextBox, 9999),
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateLoadingHpResourceActionHandler(),
                _instantiateSavingHpResourceActionHandler(),
                _instantiateLoadingMpResourceActionHandler(),
                _instantiateSavingMpResourceActionHandler(),
                _instantiateTextBoxHpFrameRGBActionHandler(),
                _instantiateTextBoxMpFrameRGBActionHandler(),
                _isntantiateHpBarActionHandler(),
                _isntantiateMpBarActionHandler(),
                _instantiateHpRGBLabelActionHandler(),
                _instantiateMpRGBLabelActionHandler(),
                _instantiateHpRGBFrameActionHandler(),
                _instantiateMpRGBFrameActionHandler(),
                _instantiateHpConfigurationUpdateActionHandler(),
                _instantiateMpConfigurationUpdateActionHandler(),
                _instantiateLoadingConsumablesActionHandler(),
                _instantiateConsumableDeselectedActionHandler(),
                _instantiateConsumableSelectedActionHandler(),
                _instantiateConsumableRemoveActionHandler(),
                _instantiateConsumableAddActionHandler(),
                _instantiateConsumableItemSaveActionHandler(),
                _instantiateSaveConfigurationActionHandler(),
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
