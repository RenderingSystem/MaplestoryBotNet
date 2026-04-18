using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class WindowRuneingEditorFramePointTag
    {
        public List<string> PointCommands = [];

        public string NextFrame = "";

        public int Radius = 0;
    }


    public class WindowRuneingEditorFramePointMacrosLoadingModifier : AbstractWindowStateModifier
    {
        private ListBox _framePointMacrosListBox;

        private AbstractMacroListBoxItemTemplateFactory _listBoxItemTemplateFactory;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowRuneingEditorFramePointMacrosLoadingModifier(
            ListBox framePointMacrosListBox,
            AbstractMacroListBoxItemTemplateFactory listBoxItemTemplateFactory,
            AbstractWindowMapEditMenuState editMenuState
            
        )
        {
            _framePointMacrosListBox = framePointMacrosListBox;
            _listBoxItemTemplateFactory = listBoxItemTemplateFactory;
            _editMenuState = editMenuState;
        }

        public override void Modify(object? value)
        {
            if (
                value is AbstractBottingModel bottingModel
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && selectedObject.FrameObject is Canvas runeFrameObject
                && runeFrameObject.Tag is MapCanvasRuneFrameDataTag runeFrameDataTag
                && runeFrameDataTag.ElementLabel is string elementLabel
                && bottingModel.GetRuneModel().FindRuneFrameByName(elementLabel) is RuneFrame runeFrame
                && runeFrame.FrameData.RuneFrameMacros is List<RuneFrameMacro> runeFrameMacros
            )
            {
                foreach (var runeFrameMacro in runeFrameMacros)
                {
                    var listBoxItem = new ListBoxItem();
                    var listBoxGrid = _listBoxItemTemplateFactory.Create();
                    var listTextBoxes = (List<TextBox>)listBoxGrid.Tag;
                    listBoxItem.Tag = new WindowRuneingEditorFramePointTag
                    {
                        PointCommands = [.. runeFrameMacro.PointCommands],
                        Radius = (int)runeFrameMacro.Radius,
                        NextFrame = (
                            runeFrameMacro.NextRuneFrame != null ?
                            runeFrameMacro.NextRuneFrame.FrameData.FrameName : ""
                        )
                    };
                    listBoxItem.Content = listBoxGrid;
                    if (listTextBoxes.Count > 0)
                    {
                        listTextBoxes[0].Text = runeFrameMacro.MacroName;
                    }
                    _framePointMacrosListBox.Items.Add(listBoxItem);
                }
                if (_framePointMacrosListBox.Items.Count > 0)
                {
                    _framePointMacrosListBox.SelectedIndex = 0;
                }
            }
        }
    }


    public class WindowRuneingEditorFramePointMacrosLoadingActionHandler : AbstractWindowActionHandler
    {
        private ListBox _framePointMacrosListBox;

        private AbstractSystemWindow _windowRuneingEditor;

        private AbstractWindowStateModifier _framePointMacrosAddModifier;

        private AbstractBottingModel? _bottingModel;

        public WindowRuneingEditorFramePointMacrosLoadingActionHandler(
            ListBox framePointMacrosListBox,
            AbstractSystemWindow windowRuneingEditor,
            AbstractWindowStateModifier framePointMacrosAddModifier
        )
        {
            _framePointMacrosListBox = framePointMacrosListBox;
            _windowRuneingEditor = windowRuneingEditor;
            _framePointMacrosAddModifier = framePointMacrosAddModifier;
            _bottingModel = null;
            var window = (Window)_windowRuneingEditor.GetWindow()!;
            window.IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacrosAddModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_windowRuneingEditor.Visible())
            {
                _framePointMacrosListBox.Items.Clear();
                _framePointMacrosAddModifier.Modify(_bottingModel);
            }
        }
    }


    public class WindowRuneingEditorFramePointMacrosLoadingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacrosLoadingActionHandler;

        public WindowRuneingEditorFramePointMacrosLoadingActionHandlerFacade(
            ListBox framePointMacrosListBox,
            FrameworkElement framePointMacroTemplate,
            AbstractSystemWindow windowRuneingEditor,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _framePointMacrosLoadingActionHandler = (
                new WindowRuneingEditorFramePointMacrosLoadingActionHandler(
                    framePointMacrosListBox,
                    windowRuneingEditor,
                    new WindowRuneingEditorFramePointMacrosLoadingModifier(
                        framePointMacrosListBox,
                        new WindowMacroListBoxItemTemplateFactory(framePointMacroTemplate),
                        editMenuState
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacrosLoadingActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _framePointMacrosLoadingActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            _framePointMacrosLoadingActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowRuneingEditorFramePointMacroAccessModifier : AbstractWindowStateModifier
    {
        private ListBox _framePointMacrosListBox;

        private List<FrameworkElement> _accessElements;

        public WindowRuneingEditorFramePointMacroAccessModifier(
            ListBox framePointMacrosListBox,
            List<FrameworkElement> accessElements
        )
        {
            _framePointMacrosListBox = framePointMacrosListBox;
            _accessElements = accessElements;
        }

        public override void Modify(object? value)
        {
            var enabled = _framePointMacrosListBox.Items.Count > 0;
            foreach (var accessElement in _accessElements)
            {
                accessElement.IsEnabled = enabled;
                if (!enabled && accessElement is TextBox textElement)
                {
                    textElement.Text = "";
                }
            }
        }
    }


    public class WindowRuneingEditorFramePointMacroAccessActionHandler : AbstractWindowActionHandler
    {
        private ListBox _framePointMacrosListBox;

        private AbstractWindowStateModifier _framePointMacroCommandButtonAccessModifier;

        public WindowRuneingEditorFramePointMacroAccessActionHandler(
            ListBox framePointMacrosListBox,
            AbstractWindowStateModifier framePointMacroCommandButtonAccessModifier
        )
        {
            _framePointMacrosListBox = framePointMacrosListBox;
            _framePointMacroCommandButtonAccessModifier = framePointMacroCommandButtonAccessModifier;
            _framePointMacrosListBox.ItemContainerGenerator.ItemsChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroCommandButtonAccessModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointMacroCommandButtonAccessModifier.Modify(null);
        }
    }


    public class WindowRuneingEditorFramePointMacroAccessActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacroAccessActionHandler;

        public WindowRuneingEditorFramePointMacroAccessActionHandlerFacade(
            ListBox framePointMacrosListBox,
            List<FrameworkElement> accessElements
        )
        {
            _framePointMacroAccessActionHandler = (
                new WindowRuneingEditorFramePointMacroAccessActionHandler(
                    framePointMacrosListBox,
                    new WindowRuneingEditorFramePointMacroAccessModifier(
                        framePointMacrosListBox,
                        accessElements
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroAccessActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointMacroAccessActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorFramePointMacroDeselectionModifier : AbstractWindowStateModifier
    {
        private TextBox _nextFrameTextBox;

        private TextBox _radiusTextBox;

        private ListBox _framePointMacroCommandsListBox;

        public WindowRuneingEditorFramePointMacroDeselectionModifier(
            TextBox nextFrameTextBox,
            TextBox radiusTextBox,
            ListBox framePointMacroCommandsListBox
        )
        {
            _nextFrameTextBox = nextFrameTextBox;
            _radiusTextBox = radiusTextBox;
            _framePointMacroCommandsListBox = framePointMacroCommandsListBox;
        }

        public override void Modify(object? value)
        {
            if (
                value is ListBoxItem deselectedItem
                && deselectedItem.Tag is WindowRuneingEditorFramePointTag framePointTag
            )
            {

                framePointTag.NextFrame = _nextFrameTextBox.Text;
                if (int.TryParse(_radiusTextBox.Text, out int parsedRadius))
                {
                    framePointTag.Radius = parsedRadius;
                }
                framePointTag.PointCommands.Clear();
                foreach (ComboBox comboBox in _framePointMacroCommandsListBox.Items)
                {
                    framePointTag.PointCommands.Add(comboBox.Text);
                }
            }
        }
    }


    public class WindowRuneingEditorFramePointMacroDeselectionActionHandler : AbstractWindowActionHandler
    {
        private ListBox _framePointMacrosListBox;

        private AbstractWindowStateModifier _framePointMacroDeselectionModifier;

        public WindowRuneingEditorFramePointMacroDeselectionActionHandler(
            ListBox framePointMacrosListBox,
            AbstractWindowStateModifier framePointMacroDeselectionModifier
        )
        {
            _framePointMacrosListBox = framePointMacrosListBox;
            _framePointMacroDeselectionModifier = framePointMacroDeselectionModifier;
            _framePointMacrosListBox.SelectionChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroDeselectionModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs
                && selectionArgs.RemovedItems.Count > 0
                && selectionArgs.RemovedItems[0] is ListBoxItem deselectedItem
            )
            {
                _framePointMacroDeselectionModifier.Modify(deselectedItem);
            }
        }
    }


    public class WindowRuneingEditorFramePointMacroDeselectionActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacroDeselectionActionHandler;

        public WindowRuneingEditorFramePointMacroDeselectionActionHandlerFacade(
            TextBox nextFrameTextBox,
            TextBox radiusTextBox,
            ListBox framePointMacrosListBox,
            ListBox framePointMacroCommandsListBox
        )
        {
            _framePointMacroDeselectionActionHandler = (
                new WindowRuneingEditorFramePointMacroDeselectionActionHandler(
                    framePointMacrosListBox,
                    new WindowRuneingEditorFramePointMacroDeselectionModifier(
                        nextFrameTextBox,
                        radiusTextBox,
                        framePointMacroCommandsListBox
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroDeselectionActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointMacroDeselectionActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorFramePointMacroSelectionModifier : AbstractWindowStateModifier
    {
        private TextBox _nextFrameTextBox;

        private TextBox _radiusTextBox;

        private ListBox _framePointMacroCommandsListBox;

        private AbstractComboBoxFactory _comboBoxTemplateFactory;

        private AbstractWindowActionHandlerRegistry _scaleRegistry;

        public WindowRuneingEditorFramePointMacroSelectionModifier(
            TextBox nextFrameTextBox,
            TextBox radiusTextBox,
            ListBox framePointMacroCommandsListBox,
            AbstractComboBoxFactory comboBoxFactory,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _nextFrameTextBox = nextFrameTextBox;
            _radiusTextBox = radiusTextBox;
            _framePointMacroCommandsListBox = framePointMacroCommandsListBox;
            _comboBoxTemplateFactory = comboBoxFactory;
            _scaleRegistry = scaleRegistry;
        }

        public override void Modify(object? value)
        {
            if (
                value is ListBoxItem selectedItem
                && selectedItem.Tag is WindowRuneingEditorFramePointTag framePointTag
            )
            {
                foreach (ComboBox comboBox in _framePointMacroCommandsListBox.Items)
                {
                    _scaleRegistry.UnregisterHandler(
                        new WindowComboBoxScaleActionHandlerParameters(comboBox)
                    );
                }
                _framePointMacroCommandsListBox.Items.Clear();
                foreach (var command in framePointTag.PointCommands)
                {
                    if (_comboBoxTemplateFactory.Create() is ComboBox comboBox)
                    {
                        comboBox.Text = command;
                        _framePointMacroCommandsListBox.Items.Add(comboBox);
                        _scaleRegistry.RegisterHandler(
                            new WindowComboBoxScaleActionHandlerParameters(comboBox)
                        );
                    }
                }
                _nextFrameTextBox.Text = framePointTag.NextFrame;
                _radiusTextBox.Text = framePointTag.Radius.ToString();
            }
        }
    }


    public class WindowRuneingEditorFramePointMacroSelectionActionHandler : AbstractWindowActionHandler
    {
        private ListBox _framePointMacrosListBox;

        private AbstractWindowStateModifier _framePointMacroSelectionModifier;

        public WindowRuneingEditorFramePointMacroSelectionActionHandler(
            ListBox framePointMacrosListBox,
            AbstractWindowStateModifier framePointMacroSelectionModifier
        )
        {
            _framePointMacrosListBox = framePointMacrosListBox;
            _framePointMacrosListBox.SelectionChanged += OnEvent;
            _framePointMacroSelectionModifier = framePointMacroSelectionModifier;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroSelectionModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs
                && selectionArgs.AddedItems.Count > 0
                && selectionArgs.AddedItems[0] is ListBoxItem selectedItem
            )
            {
                _framePointMacroSelectionModifier.Modify(selectedItem);
            }
        }
    }

    public class WindowRuneingEditorFramePointMacroSelectionActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacroSelectionActionHandler;

        public WindowRuneingEditorFramePointMacroSelectionActionHandlerFacade(
            ListBox framePointMacrosListBox,
            ListBox framePointMacroCommandsListBox,
            TextBox nextFrameTextBox,
            TextBox radiusTextBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _framePointMacroSelectionActionHandler = (
                new WindowRuneingEditorFramePointMacroSelectionActionHandler(
                    framePointMacrosListBox,
                    new WindowRuneingEditorFramePointMacroSelectionModifier(
                        nextFrameTextBox,
                        radiusTextBox,
                        framePointMacroCommandsListBox,
                        new ComboBoxTemplateFactory(comboBoxTemplate),
                        scaleRegistry
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroSelectionActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointMacroSelectionActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorFramePointMacroCommandAddModifier : AbstractWindowStateModifier
    {
        private ListBox _framePointMacroCommandsListBox;

        private AbstractComboBoxFactory _comboBoxTemplateFactory;

        private AbstractWindowActionHandlerRegistry _scaleRegistry;

        public WindowRuneingEditorFramePointMacroCommandAddModifier(
            ListBox framePointMacroCommandsListBox,
            AbstractComboBoxFactory comboBoxTemplateFactory,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _framePointMacroCommandsListBox = framePointMacroCommandsListBox;
            _comboBoxTemplateFactory = comboBoxTemplateFactory;
            _scaleRegistry = scaleRegistry;
        }

        public override void Modify(object? value)
        {
            var comboBox = _comboBoxTemplateFactory.Create();
            int selectedIndex = _framePointMacroCommandsListBox.SelectedIndex;
            int insertIndex = (
                selectedIndex != -1 ?
                selectedIndex + 1 :
                _framePointMacroCommandsListBox.Items.Count
            );
            _framePointMacroCommandsListBox.Items.Insert(
                insertIndex, comboBox
            );
            _scaleRegistry.RegisterHandler(
                new WindowComboBoxScaleActionHandlerParameters(comboBox)
            );
        }
    }


    public class WindowRuneingEditorFramePointMacroCommandAddActionHandler : AbstractWindowActionHandler
    {
        private Button _macroCommandAddButton;

        private AbstractWindowStateModifier _framePointMacroCommandAddModifier;

        public WindowRuneingEditorFramePointMacroCommandAddActionHandler(
            Button macroCommandAddButton,
            AbstractWindowStateModifier framePointMacroCommandAddModifier
        )
        {
            _macroCommandAddButton = macroCommandAddButton;
            _framePointMacroCommandAddModifier = framePointMacroCommandAddModifier;
            _macroCommandAddButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroCommandAddModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointMacroCommandAddModifier.Modify(null);
        }
    }


    public class WindowRuneingEditorFramePointMacroCommandAddActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacroCommandAddActionHandler;

        public WindowRuneingEditorFramePointMacroCommandAddActionHandlerFacade(
            Button macroCommandAddButton,
            ListBox framePointMacroCommandsListBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _framePointMacroCommandAddActionHandler = (
                new WindowRuneingEditorFramePointMacroCommandAddActionHandler(
                    macroCommandAddButton,
                    new WindowRuneingEditorFramePointMacroCommandAddModifier(
                        framePointMacroCommandsListBox,
                        new ComboBoxTemplateFactory(comboBoxTemplate),
                        scaleRegistry
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroCommandAddActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointMacroCommandAddActionHandler.OnEvent(sender, e);
        }
    }
}
