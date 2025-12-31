using MaplestoryBotNet.Systems;
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

        public MapWindow(
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            InitializeComponent();
            DataContext = this;
            _editMenuState = editMenuState;
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
                MapCanvas, _editMenuState, new MouseEventPositionExtractor()
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
                LocationTextBoxX, LocationTextBoxY, _editMenuState
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
                _instantiateNumericTextBoxPropertyPasteActionHandler(MapAreaLeftTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(MapAreaTopTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(MapAreaRightTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(MapAreaBottomTextBox, 9999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(LocationTextBoxX, Convert.ToInt32(MapCanvas.Width)),
                _instantiateNumericTextBoxPropertyPasteActionHandler(LocationTextBoxY, Convert.ToInt32(MapCanvas.Height)),
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
                _instantiateViewMinimapUpdaterActionHandler()
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
