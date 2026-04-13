using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using static BottingTabHandlersContainer;


namespace MaplestoryBotNet.Xaml
{
    public partial class MapWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        private AbstractWindowMapEditMenuState _bottingEditMenuState;

        private AbstractWindowMapEditMenuState _runeingEditMenuState;

        private AbstractLoadFileDialog _loadFileDialog;

        private AbstractSaveFileDialog _saveFileDialog;

        public MapWindow(
            AbstractWindowMapEditMenuState bottingEditMenuState,
            AbstractWindowMapEditMenuState runeingEditMenuState
        )
        {
            InitializeComponent();
            DataContext = this;
            _bottingEditMenuState = bottingEditMenuState;
            _runeingEditMenuState = runeingEditMenuState;
            _loadFileDialog = new WindowLoadFileDialog("Load Map", "JSON files (*.json)|*.json");
            _saveFileDialog = new WindowSaveFileDialog("Save Map", "JSON files (*.json)|*.json", ".json");
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers(
            AbstractSystemWindow editWindow
        )
        {
            return [
                .. new SaveLoadHandlersContainer(
                    SaveButton,
                    LoadButton,
                    _loadFileDialog,
                    _saveFileDialog
                ).Instantiate(),

                .. new BottingTabHandlersContainer(
                    BottingAddButton,
                    BottingRemoveButton,
                    BottingEditButton,
                    BottingLabelTextBox,
                    BottingLocationTextBoxX,
                    BottingLocationTextBoxY,
                    BottingCharacterTextBoxX,
                    BottingCharacterTextBoxY,
                    BottingCharacterThreshold,
                    BottingTabItem,
                    BottingMapCanvas,
                    MapImage,
                    MapTabControl,
                    Dispatcher,
                    _loadFileDialog,
                    _bottingEditMenuState,
                    GetSystemWindow()
                ).Instantiate(editWindow),

                .. new RuneingTabHandlersContainer(
                    RuneingAddButton,
                    RuneingRemoveButton,
                    RuneingAddPointButton,
                    RuneingRemovePointButton,
                    RuneingEditButton,
                    RuneingFrameLeftTextBox,
                    RuneingFrameTopTextBox,
                    RuneingFrameRightTextBox,
                    RuneingFrameBottomTextBox,
                    RuneingLabelTextBox,
                    RuneingRuneTextBoxX,
                    RuneingRuneTextBoxY,
                    RuneingRuneThreshold,
                    RuneingTabItem,
                    RuneingMapCanvas,
                    MapImage,
                    MapTabControl,
                    Dispatcher,
                    _runeingEditMenuState,
                    _loadFileDialog
                ).Instantiate(),

                .. new MapAreaHandlersContainer(
                    MapAreaLeftTextBox,
                    MapAreaTopTextBox,
                    MapAreaRightTextBox,
                    MapAreaBottomTextBox,
                    _loadFileDialog
                ).Instantiate(),
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


public abstract class AbstractMapWindowHandlersContainer
{
    protected AbstractWindowActionHandler _instantiateNumericTextBoxPropertyActionHandler(
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

    protected AbstractWindowActionHandler _instantiateNumericTextBoxPropertyPasteActionHandler(
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

    protected AbstractWindowActionHandler _instantiateTabControlCanvasActionHandler(
        TabControl tabControl, TabItem tabItem, Canvas mapCanvas
    )
    {
        return new WindowMapEditorTabControlCanvasActionHandlerFacade(
            tabControl, tabItem, mapCanvas
        );
    }
}


public class MapAreaHandlersContainer : AbstractMapWindowHandlersContainer
{
    private TextBox _mapAreaLeftTextBox;

    private TextBox _mapAreaTopTextBox;

    private TextBox _mapAreaRightTextBox;

    private TextBox _mapAreaBottomTextBox;

    private AbstractLoadFileDialog _loadFileDialog;

    public MapAreaHandlersContainer(
        TextBox mapAreaLeftTextBox,
        TextBox mapAreaTopTextBox,
        TextBox mapAreaRightTextBox,
        TextBox mapAreaBottomTextBox,
        AbstractLoadFileDialog loadFileDialog
    )
    {
        _mapAreaLeftTextBox = mapAreaLeftTextBox;
        _mapAreaTopTextBox = mapAreaTopTextBox;
        _mapAreaRightTextBox = mapAreaRightTextBox;
        _mapAreaBottomTextBox = mapAreaBottomTextBox;
        _loadFileDialog = loadFileDialog;
    }

    private AbstractWindowActionHandler _instantiateMapCanvasDimensionActionHandler()
    {
        return new WindowMapCanvasDimensionActionHandlerFacade(
            _mapAreaLeftTextBox,
            _mapAreaTopTextBox,
            _mapAreaRightTextBox,
            _mapAreaBottomTextBox
        );
    }

    private AbstractWindowActionHandler _instantiateLoadMinimapActionHandler()
    {
        return new WindowMapEditorLoadMinimapActionHandlerFacade(
            _loadFileDialog,
            _mapAreaLeftTextBox,
            _mapAreaTopTextBox,
            _mapAreaRightTextBox,
            _mapAreaBottomTextBox
        );
    }

    public List<AbstractWindowActionHandler> Instantiate()
    {
        return [
            _instantiateNumericTextBoxPropertyActionHandler(_mapAreaLeftTextBox, 9999),
            _instantiateNumericTextBoxPropertyActionHandler(_mapAreaTopTextBox, 9999),
            _instantiateNumericTextBoxPropertyActionHandler(_mapAreaRightTextBox, 9999),
            _instantiateNumericTextBoxPropertyActionHandler(_mapAreaBottomTextBox, 9999),
            _instantiateNumericTextBoxPropertyPasteActionHandler(_mapAreaLeftTextBox, 9999),
            _instantiateNumericTextBoxPropertyPasteActionHandler(_mapAreaTopTextBox, 9999),
            _instantiateNumericTextBoxPropertyPasteActionHandler(_mapAreaRightTextBox, 9999),
            _instantiateNumericTextBoxPropertyPasteActionHandler(_mapAreaBottomTextBox, 9999),
            _instantiateMapCanvasDimensionActionHandler(),
            _instantiateLoadMinimapActionHandler()
        ];
    }
}


public class SaveLoadHandlersContainer : AbstractMapWindowHandlersContainer
{
    private Button _saveButton;

    private Button _loadButton;

    private AbstractLoadFileDialog _loadFileDialog;

    private AbstractSaveFileDialog _saveFileDialog;

    public SaveLoadHandlersContainer(
        Button saveButton,
        Button loadButton,
        AbstractLoadFileDialog loadFileDialog,
        AbstractSaveFileDialog saveFileDialog
    )
    {
        _saveButton = saveButton;
        _loadButton = loadButton;
        _loadFileDialog = loadFileDialog;
        _saveFileDialog = saveFileDialog;
    }

    private AbstractWindowActionHandler _instantiateSaveConfigurationActionHandler()
    {
        return new WindowMapEditorSaveConfigurationActionHandlerFacade(
            _saveButton, _saveFileDialog
        );
    }

    private AbstractWindowActionHandler _instantiateLoadConfigurationActionHandler()
    {
        return new WindowMapEditorLoadConfigurationActionHandlerFacade(
            _loadButton, _loadFileDialog
        );
    }

    public List<AbstractWindowActionHandler> Instantiate()
    {
        return [
            _instantiateSaveConfigurationActionHandler(),
            _instantiateLoadConfigurationActionHandler()
        ];
    }
}


public class BottingTabHandlersContainer : AbstractMapWindowHandlersContainer
{
    private ToggleButton _addButton;

    private ToggleButton _removeButton;

    private Button _editButton;

    private TextBox _labelTextBox;

    private TextBox _locationTextBoxX;

    private TextBox _locationTextBoxY;

    private TextBox _characterTextBoxX;

    private TextBox _characterTextBoxY;

    private TextBox _characterThreshold;

    private TabItem _tabItem;

    private Canvas _mapCanvas;

    private Image _mapImage;

    private TabControl _tabControl;

    private Dispatcher _dispatcher;

    private AbstractLoadFileDialog _loadFileDialog;

    private AbstractWindowMapEditMenuState _editMenuState;

    private AbstractSystemWindow _systemWindow;

    public BottingTabHandlersContainer(
        ToggleButton addButton,
        ToggleButton removeButton,
        Button editButton,
        TextBox labelTextBox,
        TextBox locationTextBoxX,
        TextBox locationTextBoxY,
        TextBox characterTextBoxX,
        TextBox characterTextBoxY,
        TextBox characterThreshold,
        TabItem tabItem,
        Canvas mapCanvas,
        Image mapImage,
        TabControl tabControl,
        Dispatcher dispatcher,
        AbstractLoadFileDialog loadFileDialog,
        AbstractWindowMapEditMenuState editMenuState,
        AbstractSystemWindow systemWindow
    )
    {
        _addButton = addButton;
        _removeButton = removeButton;
        _editButton = editButton;
        _labelTextBox = labelTextBox;
        _locationTextBoxX = locationTextBoxX;
        _locationTextBoxY = locationTextBoxY;
        _characterTextBoxX = characterTextBoxX;
        _characterTextBoxY = characterTextBoxY;
        _characterThreshold = characterThreshold;
        _tabItem = tabItem;
        _mapCanvas = mapCanvas;
        _mapImage = mapImage;
        _tabControl = tabControl;
        _dispatcher = dispatcher;
        _loadFileDialog = loadFileDialog;
        _editMenuState = editMenuState;
        _systemWindow = systemWindow;
    }

    private AbstractWindowActionHandler _instantiateWindowMenuItemHideActionHandler()
    {
        return new WindowMenuItemHideHandlerBuilder()
            .WithArgs(_systemWindow)
            .Build();
    }

    private AbstractWindowActionHandler _instantiateEditMenuActionHandler(
        AbstractSystemWindow editWindow
    )
    {
        return new WindowMapEditMenuActionHandlerFacade(
            _editButton,
            _systemWindow,
            editWindow
        );
    }

    private AbstractWindowActionHandler _instantiateAddPointButtonActionHandler()
    {
        return new WindowMapAddButtonActionHandlerFacade(
            _addButton,
            [_addButton, _removeButton],
            _editMenuState
        );
    }

    private AbstractWindowActionHandler _instantiatePointDrawingActionHandler()
    {
        return new WindowMapCanvasPointDrawingActionHandlerFacade(
            _mapCanvas,
            _labelTextBox,
            _editMenuState,
            new MouseEventDataExtractor()
        );
    }

    private AbstractWindowActionHandler _instantiatePointErasingActionHandler()
    {
        return new WindowMapCanvasPointErasingActionHandlerFacade(
            _mapCanvas,
            _editMenuState,
            new MouseEventDataExtractor()
        );
    }

    private AbstractWindowActionHandler _instantiateRemovePointButtonActionHandler()
    {
        return new WindowMapRemoveButtonActionHandlerFacade(
            _removeButton,
            [_addButton, _removeButton],
            _editMenuState
        );
    }

    private AbstractWindowActionHandler _instantiateSelectPointActionHandler()
    {
        return new WindowMapCanvasSelectActionHandlerFacade(
            _mapCanvas,
            _locationTextBoxX,
            _locationTextBoxY,
            _labelTextBox,
            _editMenuState,
            new MouseEventDataExtractor()
        );
    }

    private AbstractWindowActionHandler _instantiateDragPointActionHandler()
    {
        return new WindowMapCanvasDragActionHandlerFacade(
            _mapCanvas,
            _locationTextBoxX,
            _locationTextBoxY,
            _editMenuState,
            new MouseEventDataExtractor()
        );
    }

    private AbstractWindowActionHandler _instantiateEditButtonAccessibilityActionHandler()
    {
        return new WindowMapEditButtonAccessibilityActionHandlerFacade(
            _mapCanvas,
            _editButton,
            _editMenuState
        );
    }

    private AbstractWindowActionHandler _instantiatePointLocationActionHandler()
    {
        return new WindowMapCanvasPointLocationActionHandlerFacade(
            _locationTextBoxX,
            _locationTextBoxY,
            _editMenuState
        );
    }

    private AbstractWindowActionHandler _instantiateViewMinimapUpdaterActionHandler()
    {
        return new WindowViewMinimapUpdaterActionHandlerFacade(
            _mapImage,
            new SystemAsyncDispatcher(_dispatcher, DispatcherPriority.Background),
            _systemWindow
        );
    }

    private AbstractWindowActionHandler _instantiateLoadModelActionHandler()
    {
        return new WindowMapEditorLoadModelActionHandlerFacade(
            _loadFileDialog
        );
    }

    private AbstractWindowActionHandler _instantiateLoadMinimapPointsActionHandler()
    {
        return new WindowMapEditorLoadedMinimapPointsActionHandlerFacade(
            _mapCanvas,
            _labelTextBox,
            _loadFileDialog
        );
    }

    private AbstractWindowActionHandler _instantiateLoadMenuStateActionHandler()
    {
        return new WindowMapEditorLoadedMenuStateActionHandlerFacade(
            _editButton,
            _labelTextBox,
            _locationTextBoxX,
            _locationTextBoxY,
            _loadFileDialog,
            _editMenuState
        );
    }

    private AbstractWindowActionHandler _instantiateCharacterPositionActionHandler()
    {
        return new WindowMinimapPositionActionHandlerFacade(
            new SystemAsyncDispatcher(_dispatcher, DispatcherPriority.Normal),
            _characterTextBoxX,
            _characterTextBoxY,
            MapIconInfo.Character,
            _mapImage
        );
    }

    private AbstractWindowActionHandler _instantiateLoadCharacterThresholdActionHandler()
    {
        return new WindowMapEditorLoadedThresholdHandlerFacade(
            _loadFileDialog,
            _characterThreshold,
            MapIconInfo.Character
        );
    }

    private AbstractWindowActionHandler _instantiateCharacterThresholdHandler()
    {
        return new WindowMapEditorThresholdHandlerFacade(
            _characterThreshold, MapIconInfo.Character
        );
    }

    public List<AbstractWindowActionHandler> Instantiate(
        AbstractSystemWindow editWindow
    )
    {
        return [
            _instantiateNumericTextBoxPropertyActionHandler(_locationTextBoxX, Convert.ToInt32(_mapCanvas.Width)),
            _instantiateNumericTextBoxPropertyActionHandler(_locationTextBoxY, Convert.ToInt32(_mapCanvas.Height)),
            _instantiateNumericTextBoxPropertyActionHandler(_characterThreshold, 999),
            _instantiateNumericTextBoxPropertyPasteActionHandler(_locationTextBoxX, Convert.ToInt32(_mapCanvas.Width)),
            _instantiateNumericTextBoxPropertyPasteActionHandler(_locationTextBoxY, Convert.ToInt32(_mapCanvas.Height)),
            _instantiateNumericTextBoxPropertyPasteActionHandler(_characterThreshold, 999),
            _instantiateTabControlCanvasActionHandler(_tabControl, _tabItem, _mapCanvas),
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
            _instantiateLoadModelActionHandler(),
            _instantiateLoadMinimapPointsActionHandler(),
            _instantiateLoadMenuStateActionHandler(),
            _instantiateCharacterPositionActionHandler(),
            _instantiateLoadCharacterThresholdActionHandler(),
            _instantiateCharacterThresholdHandler(),
        ];
    }


    public class RuneingTabHandlersContainer : AbstractMapWindowHandlersContainer
    {
        private ToggleButton _addButton;

        private ToggleButton _removeButton;

        private ToggleButton _addPointButton;

        private ToggleButton _removePointButton;

        private Button _editButton;

        private TextBox _frameTextBoxLeft;

        private TextBox _frameTextBoxTop;

        private TextBox _frameTextBoxRight;

        private TextBox _frameTextBoxBottom;

        private TextBox _labelTextBox;

        private TextBox _runeTextBoxX;

        private TextBox _runeTextBoxY;

        private TextBox _runeThreshold;

        private TabControl _tabControl;

        private TabItem _tabItem;

        private Canvas _mapCanvas;

        private Image _mapImage;

        private Dispatcher _dispatcher;

        private AbstractWindowMapEditMenuState _editMenuState;

        private AbstractLoadFileDialog _loadFileDialog;

        public RuneingTabHandlersContainer(
            ToggleButton addButton,
            ToggleButton removeButton,
            ToggleButton addPointButton,
            ToggleButton removePointButton,
            Button editButton,
            TextBox frameTextBoxLeft,
            TextBox frameTextBoxTop,
            TextBox frameTextBoxRight,
            TextBox frameTextBoxBottom,
            TextBox labelTextBox,
            TextBox runeTextBoxX,
            TextBox runeTextBoxY,
            TextBox runeThreshold,
            TabItem tabItem,
            Canvas mapCanvas,
            Image mapImage,
            TabControl tabControl,
            Dispatcher dispatcher,
            AbstractWindowMapEditMenuState editMenuState,
            AbstractLoadFileDialog loadFileDialog
        )
        {
            _addButton = addButton;
            _removeButton = removeButton;
            _addPointButton = addPointButton;
            _removePointButton = removePointButton;
            _editButton = editButton;
            _frameTextBoxLeft = frameTextBoxLeft;
            _frameTextBoxTop = frameTextBoxTop;
            _frameTextBoxRight = frameTextBoxRight;
            _frameTextBoxBottom = frameTextBoxBottom;
            _labelTextBox = labelTextBox;
            _runeTextBoxX = runeTextBoxX;
            _runeTextBoxY = runeTextBoxY;
            _runeThreshold = runeThreshold;
            _tabItem = tabItem;
            _mapCanvas = mapCanvas;
            _mapImage = mapImage;
            _tabControl = tabControl;
            _dispatcher = dispatcher;
            _editMenuState = editMenuState;
            _loadFileDialog = loadFileDialog;
        }

        private AbstractWindowActionHandler _instantiateRunePositionActionHandler()
        {
            return new WindowMinimapPositionActionHandlerFacade(
                new SystemAsyncDispatcher(_dispatcher, DispatcherPriority.Normal),
                _runeTextBoxX,
                _runeTextBoxY,
                MapIconInfo.Rune,
                _mapImage
            );
        }

        private AbstractWindowActionHandler _instantiateLoadRuneThresholdActionHandler()
        {
            return new WindowMapEditorLoadedThresholdHandlerFacade(
                _loadFileDialog,
                _runeThreshold,
                MapIconInfo.Rune
            );
        }


        private AbstractWindowActionHandler _instantiateRuneThresholdHandler()
        {
            return new WindowMapEditorThresholdHandlerFacade(
                _runeThreshold, MapIconInfo.Rune
            );
        }

        private AbstractWindowActionHandler _instantiateAddFrameButtonActionHandler()
        {
            return new WindowMapAddFrameButtonActionHandlerFacade(
                _addButton,
                [_addButton, _removeButton, _addPointButton, _removePointButton],
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateFrameDrawerActionHandler()
        {
            return new WindowMapCanvasFrameDrawerActionHandlerFacade(
                _mapCanvas,
                _labelTextBox,
                _editMenuState,
                new MouseEventDataExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiateFrameSelectStateActionHandler()
        {
            return new WindowMapCanvasFrameSelectStateActionHandlerFacade(
                _mapCanvas,
                _editMenuState,
                new MouseEventDataExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiateFrameDragActionHandler()
        {
            return new WindowMapCanvasFrameDragActionHandlerFacade(
                _mapCanvas,
                _editMenuState,
                new MouseEventDataExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiateRemoveFrameButtonActionHandler()
        {
            return new WindowMapRemoveFrameButtonActionHandlerFacade(
                _removeButton,
                [_addButton, _removeButton, _addPointButton, _removePointButton],
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateFrameDataActionHandler()
        {
            return new WindowMapCanvasFrameDataActionHandlerFacade(
                _mapCanvas,
                _labelTextBox,
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateFrameSelectedTextActionHandler()
        {
            return new WindowMapCanvasFrameSelectedTextActionHandlerFacade(
                _mapCanvas,
                _labelTextBox,
                _frameTextBoxLeft,
                _frameTextBoxTop,
                _frameTextBoxRight,
                _frameTextBoxBottom,
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateFrameSelectedDragDataActionHandler()
        {
            return new WindowMapCanvasFrameSelectedDragDataActionHandlerFacade(
                _mapCanvas,
                _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateFrameRemoveActionHandler()
        {
            return new WindowMapCanvasFrameRemoveActionHandlerFacade(
                _mapCanvas,
                _editMenuState,
                new MouseEventDataExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiateFrameButtonAccessActionHandler()
        {
            return new WindowMapCanvasFrameButtonAccessActionHandlerFacade(
                _mapCanvas,
                [_addPointButton, _removePointButton, _editButton],
                _editMenuState
            );
        }

        public AbstractWindowActionHandler _instantiateAddFramePointButtonActionHandler()
        {
            return new WindowMapAddFramePointButtonActionHandlerFacade(
                _addPointButton,
                [_addButton, _removeButton, _addPointButton, _removePointButton],
                _editMenuState
            );
        }

        public AbstractWindowActionHandler _instantiateRemoveFramePointButtonActionHandler()
        {
            return new WindowMapRemoveFramePointButtonActionHandlerFacade(
                _removePointButton,
                [_addButton, _removeButton, _addPointButton, _removePointButton],
                _editMenuState
            );
        }

        public AbstractWindowActionHandler _instantiateFramePointDrawerActionHandler()
        {
            return new WindowMapCanvasFramePointDrawerActionHandlerFacade(
                _mapCanvas,
                _editMenuState,
                new MouseEventDataExtractor()
            );
        }

        public AbstractWindowActionHandler _instantiateFramePointDragActionHandler()
        {
            return new WindowMapCanvasFramePointDragActionHandlerFacade(
                _mapCanvas,
                _editMenuState,
                new MouseEventDataExtractor()
            );
        }

        public AbstractWindowActionHandler _instantiateFramePointScaleActionHandler()
        {
            return new WindowMapCanvasFramePointScaleActionHandlerFacade(
                _mapCanvas,
                _editMenuState
            );
        }

        public AbstractWindowActionHandler _instantiateFramePointRemoveActionHandler()
        {
            return new WindowMapCanvasFramePointRemoveActionHandlerFacade(
                _mapCanvas, _editMenuState, new MouseEventDataExtractor()
            );
        }

        public List<AbstractWindowActionHandler> Instantiate()
        {
            return [
                _instantiateNumericTextBoxPropertyActionHandler(_runeThreshold, 999),
                _instantiateNumericTextBoxPropertyActionHandler(_frameTextBoxLeft, Convert.ToInt32(_mapCanvas.Width)),
                _instantiateNumericTextBoxPropertyActionHandler(_frameTextBoxTop, Convert.ToInt32(_mapCanvas.Height)),
                _instantiateNumericTextBoxPropertyActionHandler(_frameTextBoxRight, Convert.ToInt32(_mapCanvas.Width)),
                _instantiateNumericTextBoxPropertyActionHandler(_frameTextBoxBottom, Convert.ToInt32(_mapCanvas.Height)),
                _instantiateNumericTextBoxPropertyPasteActionHandler(_runeThreshold, 999),
                _instantiateNumericTextBoxPropertyPasteActionHandler(_frameTextBoxLeft, Convert.ToInt32(_mapCanvas.Width)),
                _instantiateNumericTextBoxPropertyPasteActionHandler(_frameTextBoxTop, Convert.ToInt32(_mapCanvas.Height)),
                _instantiateNumericTextBoxPropertyPasteActionHandler(_frameTextBoxRight, Convert.ToInt32(_mapCanvas.Width)),
                _instantiateNumericTextBoxPropertyPasteActionHandler(_frameTextBoxBottom, Convert.ToInt32(_mapCanvas.Height)),
                _instantiateTabControlCanvasActionHandler(_tabControl, _tabItem, _mapCanvas),
                _instantiateRunePositionActionHandler(),
                _instantiateLoadRuneThresholdActionHandler(),
                _instantiateRuneThresholdHandler(),
                _instantiateAddFrameButtonActionHandler(),
                _instantiateRemoveFrameButtonActionHandler(),
                _instantiateAddFramePointButtonActionHandler(),
                _instantiateRemoveFramePointButtonActionHandler(),
                _instantiateFrameRemoveActionHandler(),
                _instantiateFrameDrawerActionHandler(),
                _instantiateFramePointDrawerActionHandler(),
                _instantiateFrameSelectStateActionHandler(),
                _instantiateFrameDragActionHandler(),
                _instantiateFrameDataActionHandler(),
                _instantiateFrameSelectedTextActionHandler(),
                _instantiateFrameSelectedDragDataActionHandler(),
                _instantiateFrameButtonAccessActionHandler(),
                _instantiateFramePointDragActionHandler(),
                _instantiateFramePointScaleActionHandler(),
                _instantiateFramePointRemoveActionHandler()
            ];
        }
    }
}
