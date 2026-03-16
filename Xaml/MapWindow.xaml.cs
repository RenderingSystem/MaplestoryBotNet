using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace MaplestoryBotNet.Xaml
{
    public partial class MapWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        private AbstractWindowMapEditMenuState _editMenuState;

        private AbstractLoadFileDialog _loadFileDialog;

        public MapWindow(
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            InitializeComponent();
            DataContext = this;
            _editMenuState = editMenuState;
            _loadFileDialog = new WindowLoadFileDialog(
                "Load Map",
                "JSON files (*.json)|*.json"
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

        private AbstractWindowActionHandler _instantiateWindowMenuItemHideActionHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }

        private AbstractWindowActionHandler _instantiateEditMenuActionHandler(
            AbstractSystemWindow editWindow
        )
        {
            return new WindowMapEditMenuActionHandlerFacade(
                EditButton,
                GetSystemWindow(),
                editWindow
            );
        }

        private AbstractWindowActionHandler _instantiateAddPointButtonActionHandler()
        {
            return new WindowMapAddPointButtonActionHandlerFacade(
                AddButton,
                [AddButton, RemoveButton],
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiatePointDrawingActionHandler()
        {
            return new WindowMapCanvasPointDrawingActionHandlerFacade(
                MapCanvas,
                LabelTextBox,
                _editMenuState,
                new MouseEventPositionExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiatePointErasingActionHandler()
        {
            return new WindowMapCanvasPointErasingActionHandlerFacade(
                MapCanvas,
                _editMenuState,
                new MouseEventPositionExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiateRemovePointButtonActionHandler()
        {
            return new WindowMapRemovePointButtonActionHandlerFacade(
                RemoveButton,
                [AddButton, RemoveButton],
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateSelectPointActionHandler()
        {
            return new WindowMapCanvasSelectActionHandlerFacade(
                MapCanvas,
                LocationTextBoxX,
                LocationTextBoxY,
                LabelTextBox,
                _editMenuState,
                new MouseEventPositionExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiateDragPointActionHandler()
        {
            return new WindowMapCanvasDragActionHandlerFacade(
                MapCanvas,
                LocationTextBoxX,
                LocationTextBoxY,
                _editMenuState,
                new MouseEventPositionExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiateEditButtonAccessibilityActionHandler()
        {
            return new WindowMapEditButtonAccessibilityActionHandlerFacade(
                MapCanvas,
                EditButton,
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiatePointLocationActionHandler()
        {
            return new WindowMapCanvasPointLocationActionHandlerFacade(
                LocationTextBoxX,
                LocationTextBoxY,
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateViewMinimapUpdaterActionHandler()
        {
            return new WindowViewMinimapUpdaterActionHandlerFacade(
                MapImage,
                new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Background),
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateMapCanvasDimensionActionHandler()
        {
            return new WindowMapCanvasDimensionActionHandlerFacade(
                MapAreaLeftTextBox,
                MapAreaTopTextBox,
                MapAreaRightTextBox,
                MapAreaBottomTextBox
            );
        }

        private AbstractWindowActionHandler _instantiateSaveConfigurationActionHandler()
        {
            return new WindowMapEditorSaveConfigurationActionHandlerFacade(
                SaveButton
            );
        }

        private AbstractWindowActionHandler _instantiateLoadConfigurationActionHandler()
        {
            return new WindowMapEditorLoadConfigurationActionHandlerFacade(
                LoadButton,
                _loadFileDialog
            );
        }

        private AbstractWindowActionHandler _instantiateLoadModelActionHandler()
        {
            return new WindowMapEditorLoadModelActionHandlerFacade(
                _loadFileDialog
            );
        }

        private AbstractWindowActionHandler _instantiateLoadMinimapActionHandler()
        {
            return new WindowMapEditorLoadMinimapActionHandlerFacade(
                _loadFileDialog,
                MapAreaLeftTextBox,
                MapAreaTopTextBox,
                MapAreaRightTextBox,
                MapAreaBottomTextBox
            );
        }

        public AbstractWindowActionHandler _instantiateLoadMinimapPointsActionHandler()
        {
            return new WindowMapEditorLoadedMinimapPointsActionHandlerFacade(
                MapCanvas,
                LabelTextBox,
                _loadFileDialog
            );
        }

        public AbstractWindowActionHandler _instantiateLoadMenuStateActionHandler()
        {
            return new WindowMapEditorLoadedMenuStateActionHandlerFacade(
                EditButton,
                LabelTextBox,
                LocationTextBoxX,
                LocationTextBoxY,
                _loadFileDialog,
                _editMenuState
            );
        }

        public AbstractWindowActionHandler _instantiateCharacterPositionActionHandler()
        {
            return new WindowMinimapPositionActionHandlerFacade(
                new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Normal),
                CharacterTextBoxX,
                CharacterTextBoxY,
                MapIconInfo.Character,
                MapImage
            );
        }

        public AbstractWindowActionHandler _instantiateRunePositionActionHandler()
        {
            return new WindowMinimapPositionActionHandlerFacade(
                new SystemAsyncDispatcher(Dispatcher, DispatcherPriority.Normal),
                RuneTextBoxX,
                RuneTextBoxY,
                MapIconInfo.Rune,
                MapImage
            );
        }

        public AbstractWindowActionHandler _instantiateLoadCharacterThresholdActionHandler()
        {
            return new WindowMapEditorLoadedThresholdHandlerFacade(
                _loadFileDialog,
                CharacterThreshold,
                MapIconInfo.Character
            );
        }

        public AbstractWindowActionHandler _instantiateLoadRuneThresholdActionHandler()
        {
            return new WindowMapEditorLoadedThresholdHandlerFacade(
                _loadFileDialog,
                RuneThreshold,
                MapIconInfo.Rune
            );
        }

        public AbstractWindowActionHandler _instantiateCharacterThresholdHandlerFacade()
        {
            return new WindowMapEditorThresholdHandlerFacade(
                CharacterThreshold, MapIconInfo.Character
            );
        }

        public AbstractWindowActionHandler _instantiateRuneThresholdHandlerFacade()
        {
            return new WindowMapEditorThresholdHandlerFacade(
                RuneThreshold, MapIconInfo.Rune
            );
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers(
            AbstractSystemWindow editWindow
        )
        {
            return [
                _instantiateNumericTextBoxPropertyActionHandler(MapAreaLeftTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(MapAreaTopTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(MapAreaRightTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(MapAreaBottomTextBox, 9999),
                _instantiateNumericTextBoxPropertyActionHandler(LocationTextBoxX, Convert.ToInt32(MapCanvas.Width)),
                _instantiateNumericTextBoxPropertyActionHandler(LocationTextBoxY, Convert.ToInt32(MapCanvas.Height)),
                _instantiateNumericTextBoxPropertyActionHandler(CharacterThreshold, 999),
                _instantiateNumericTextBoxPropertyActionHandler(RuneThreshold, 999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(MapAreaLeftTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(MapAreaTopTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(MapAreaRightTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(MapAreaBottomTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(LocationTextBoxX, Convert.ToInt32(MapCanvas.Width)),
                _instantiateNumericTextBoxPropertyPasteActionHandler(LocationTextBoxY, Convert.ToInt32(MapCanvas.Height)),
                _instantiateNumericTextBoxPropertyPasteActionHandler(CharacterThreshold, 999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(RuneThreshold, 999),
                _instantiateMapCanvasDimensionActionHandler(),
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateEditMenuActionHandler(editWindow),
                _instantiateAddPointButtonActionHandler(),
                _instantiatePointDrawingActionHandler(),
                _instantiateRemovePointButtonActionHandler(),
                _instantiatePointErasingActionHandler(),
                _instantiateSelectPointActionHandler(),
                _instantiateDragPointActionHandler(),
                _instantiateEditButtonAccessibilityActionHandler(),
                _instantiatePointLocationActionHandler(),
                _instantiateViewMinimapUpdaterActionHandler(),
                _instantiateSaveConfigurationActionHandler(),
                _instantiateLoadConfigurationActionHandler(),
                _instantiateLoadModelActionHandler(),
                _instantiateLoadMinimapActionHandler(),
                _instantiateLoadMinimapPointsActionHandler(),
                _instantiateLoadMenuStateActionHandler(),
                _instantiateCharacterPositionActionHandler(),
                _instantiateRunePositionActionHandler(),
                _instantiateLoadCharacterThresholdActionHandler(),
                _instantiateLoadRuneThresholdActionHandler(),
                _instantiateCharacterThresholdHandlerFacade(),
                _instantiateRuneThresholdHandlerFacade()
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
