using MaplestoryBotNet.Systems.Configuration.SubSystems;
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


    public class WindowRuneingEditorFrameNameLoadingModifier : AbstractWindowStateModifier
    {
        private TextBox _frameNameTextBox;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowRuneingEditorFrameNameLoadingModifier(
            TextBox frameNameTextBox,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _frameNameTextBox = frameNameTextBox;
            _editMenuState = editMenuState;
        }

        public override void Modify(object? value)
        {
            if (
                _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && selectedObject.FrameObject is Canvas runeFrameObject
                && runeFrameObject.Tag is MapCanvasRuneFrameDataTag runeFrameDataTag
                && runeFrameDataTag.FrameName is string frameName
            )
            {
                _frameNameTextBox.Text = frameName;
            }
        }
    }


    public class WindowRuneingEditorFrameNameLoadingActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _windowRuneingEditor;

        private AbstractWindowStateModifier _frameNameLoadingModifier;

        public WindowRuneingEditorFrameNameLoadingActionHandler(
            AbstractSystemWindow windowRuneingEditor,
            AbstractWindowStateModifier frameNameLoadingModifier
        )
        {
            _windowRuneingEditor = windowRuneingEditor;
            _frameNameLoadingModifier = frameNameLoadingModifier;
            var window = (Window)_windowRuneingEditor.GetWindow()!;
            window.IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameNameLoadingModifier;
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_windowRuneingEditor.Visible())
            {
                _frameNameLoadingModifier.Modify(null);
            }
        }
    }


    public class WindowRuneingEditorFrameNameLoadingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _frameNameLoadingActionHandler;

        public WindowRuneingEditorFrameNameLoadingActionHandlerFacade(
            TextBox frameNameTextBox,
            AbstractSystemWindow windowRuneingEditor,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _frameNameLoadingActionHandler = new WindowRuneingEditorFrameNameLoadingActionHandler(
                windowRuneingEditor,
                new WindowRuneingEditorFrameNameLoadingModifier(
                    frameNameTextBox,
                    editMenuState
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameNameLoadingActionHandler.Modifier();
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            _frameNameLoadingActionHandler.OnDependencyEvent(sender, e);
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


    public class WindowRuneingEditorElementCommandAddModifier : AbstractWindowStateModifier
    {
        private ListBox _elementCommandsListBox;

        private AbstractComboBoxFactory _comboBoxTemplateFactory;

        private AbstractWindowActionHandlerRegistry _scaleRegistry;

        public WindowRuneingEditorElementCommandAddModifier(
            ListBox elementCommandsListBox,
            AbstractComboBoxFactory comboBoxTemplateFactory,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _elementCommandsListBox = elementCommandsListBox;
            _comboBoxTemplateFactory = comboBoxTemplateFactory;
            _scaleRegistry = scaleRegistry;
        }

        public override void Modify(object? value)
        {
            var comboBox = _comboBoxTemplateFactory.Create();
            var selectedIndex = (
                _elementCommandsListBox.SelectedIndex
            );
            var insertIndex = (
                selectedIndex != -1 ?
                selectedIndex + 1 :
                _elementCommandsListBox.Items.Count
            );
            _elementCommandsListBox.Items.Insert(
                insertIndex, comboBox
            );
            _scaleRegistry.RegisterHandler(
                new WindowComboBoxScaleActionHandlerParameters(comboBox)
            );
        }
    }


    public class WindowRuneingEditorElementCommandAddActionHandler : AbstractWindowActionHandler
    {
        private Button _macroCommandAddButton;

        private AbstractWindowStateModifier _framePointMacroCommandAddModifier;

        public WindowRuneingEditorElementCommandAddActionHandler(
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
                new WindowRuneingEditorElementCommandAddActionHandler(
                    macroCommandAddButton,
                    new WindowRuneingEditorElementCommandAddModifier(
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


    public class WindowRuneingEditorElementCommandRemoveModifier : AbstractWindowStateModifier
    {
        private ListBox _elementCommandsListBox;

        private AbstractWindowActionHandlerRegistry _scaleRegistry;

        public WindowRuneingEditorElementCommandRemoveModifier(
            ListBox elementCommandsListBox,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _elementCommandsListBox = elementCommandsListBox;
            _scaleRegistry = scaleRegistry;
        }

        public override void Modify(object? value)
        {
            var selectedIndex = (
                _elementCommandsListBox.SelectedIndex
            );
            if (_elementCommandsListBox.Items.Count > 0)
            {
                var removeIndex = (
                    selectedIndex != -1 ?
                    selectedIndex :
                    _elementCommandsListBox.Items.Count - 1
                );
                var comboBox = (
                    (ComboBox)_elementCommandsListBox.Items[removeIndex]
                );
                _elementCommandsListBox.Items.Remove(
                    comboBox
                );
                _scaleRegistry.UnregisterHandler(
                    new WindowComboBoxScaleActionHandlerParameters(comboBox)
                );
            }
        }
    }


    public class WindowRuneingEditorElementCommandRemoveActionHandler : AbstractWindowActionHandler
    {
        private Button _macroCommandRemoveButton;

        private AbstractWindowStateModifier _framePointMacroCommandRemoveModifier;

        public WindowRuneingEditorElementCommandRemoveActionHandler(
            Button macroCommandRemoveButton,
            AbstractWindowStateModifier framePointMacroCommandRemoveModifier
        )
        {
            _macroCommandRemoveButton = macroCommandRemoveButton;
            _framePointMacroCommandRemoveModifier = framePointMacroCommandRemoveModifier;
            _macroCommandRemoveButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroCommandRemoveModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointMacroCommandRemoveModifier.Modify(null);
        }
    }


    public class WindowRuneingEditorFramePointMacroCommandRemoveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacroCommandRemoveActionHandler;

        public WindowRuneingEditorFramePointMacroCommandRemoveActionHandlerFacade(
            Button macroCommandRemoveButton,
            ListBox framePointMacroCommandsListBox,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _framePointMacroCommandRemoveActionHandler = (
                new WindowRuneingEditorElementCommandRemoveActionHandler(
                    macroCommandRemoveButton,
                    new WindowRuneingEditorElementCommandRemoveModifier(
                        framePointMacroCommandsListBox,
                        scaleRegistry
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroCommandRemoveActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointMacroCommandRemoveActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorElementCommandClearModifier : AbstractWindowStateModifier
    {
        private ListBox _framePointMacroCommandsListBox;

        private AbstractWindowActionHandlerRegistry _scaleRegistry;

        public WindowRuneingEditorElementCommandClearModifier(
            ListBox framePointMacroCommandsListBox,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _framePointMacroCommandsListBox = framePointMacroCommandsListBox;
            _scaleRegistry = scaleRegistry;
        }

        public override void Modify(object? value)
        {
            foreach (ComboBox comboBox in _framePointMacroCommandsListBox.Items)
            {
                var parameters = new WindowComboBoxScaleActionHandlerParameters(comboBox);
                _scaleRegistry.UnregisterHandler(parameters);
            }
            _framePointMacroCommandsListBox.Items.Clear();
        }
    }


    public class WindowRuneingEditorElementCommandClearActionHandler : AbstractWindowActionHandler
    {
        private Button _macroCommandRemoveButton;

        private AbstractWindowStateModifier _framePointMacroCommandClearModifier;

        public WindowRuneingEditorElementCommandClearActionHandler(
            Button macroCommandRemoveButton,
            AbstractWindowStateModifier framePointMacroCommandClearModifier
        )
        {
            _macroCommandRemoveButton = macroCommandRemoveButton;
            _framePointMacroCommandClearModifier = framePointMacroCommandClearModifier;
            _macroCommandRemoveButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroCommandClearModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointMacroCommandClearModifier.Modify(null);
        }
    }


    public class WindowRuneingEditorFramePointMacroCommandClearActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacroCommandClearActionHandler;

        public WindowRuneingEditorFramePointMacroCommandClearActionHandlerFacade(
            Button macroCommandClearButton,
            ListBox framePointMacroCommandsListBox,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _framePointMacroCommandClearActionHandler = (
                new WindowRuneingEditorElementCommandClearActionHandler(
                    macroCommandClearButton,
                    new WindowRuneingEditorElementCommandClearModifier(
                        framePointMacroCommandsListBox,
                        scaleRegistry
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacroCommandClearActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointMacroCommandClearActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorFramePointLoadConfigurationActionHandler : AbstractWindowActionHandler
    {
        private Button _loadButton;

        private AbstractWindowStateModifier _windowLoadDialogModifier;

        private string? _initialDirectory;

        public WindowRuneingEditorFramePointLoadConfigurationActionHandler(
            Button loadButton,
            AbstractWindowStateModifier windowLoadDialogModifier
        )
        {
            _loadButton = loadButton;
            _windowLoadDialogModifier = windowLoadDialogModifier;
            _loadButton.Click += OnEvent;
            _initialDirectory = null;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (_initialDirectory == null)
            {
                return;
            }
            _windowLoadDialogModifier.Modify(
                new WindowLoadMenuModifierParameters
                {
                    InitialDirectory = _initialDirectory
                }
            );
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration configuration
            )
            {
                _initialDirectory = configuration.FramePointsDirectory;
            }
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowLoadDialogModifier;
        }
    }


    public class WindowRuneingEditorFramePointLoadConfigurationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapEditorLoadConfigurationActionHandler;

        public WindowRuneingEditorFramePointLoadConfigurationActionHandlerFacade(
            Button loadButton, AbstractLoadFileDialog loadFileDialog
        )
        {
            _mapEditorLoadConfigurationActionHandler = (
                new WindowRuneingEditorFramePointLoadConfigurationActionHandler(
                    loadButton, new WindowLoadMenuModifier(loadFileDialog)
                )
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _mapEditorLoadConfigurationActionHandler.OnEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _mapEditorLoadConfigurationActionHandler.Inject(dataType, data);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _mapEditorLoadConfigurationActionHandler.Modifier();
        }
    }


    public class WindowRuneingEditorFramePointLoadActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointLoadActionHandler;

        public WindowRuneingEditorFramePointLoadActionHandlerFacade(
            AbstractLoadFileDialog loadFileDialog,
            ListBox macroCommandsListBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _framePointLoadActionHandler = new WindowLoadMenuElementActionHandler(
                loadFileDialog,
                new WindowLoadMenuElementModifier(
                    macroCommandsListBox,
                    new ComboBoxTemplateFactory(comboBoxTemplate),
                    comboBoxPopupScaleRegistry,
                    new MacroDataDeserializer()
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointLoadActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _framePointLoadActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorFramePointSaveActionHandler : AbstractWindowActionHandler
    {
        private Button _saveButton;

        private ListBox _macroCommandsListBox;

        private AbstractMacroDataSerializer _macroDataSerializer;

        private AbstractWindowStateModifier _windowSaveMenuModifier;

        private string? _initialDirectory;

        public WindowRuneingEditorFramePointSaveActionHandler(
            Button saveButton,
            ListBox listBox,
            AbstractMacroDataSerializer macroDataSerializer,
            AbstractWindowStateModifier windowSaveMenuHandler
        )
        {
            _saveButton = saveButton;
            _macroCommandsListBox = listBox;
            _macroDataSerializer = macroDataSerializer;
            _windowSaveMenuModifier = windowSaveMenuHandler;
            _initialDirectory = null;
            _saveButton.Click += OnEvent;
        }

        private ConfigurationMacroData _getListBoxMacroData()
        {
            var macroData = new ConfigurationMacroData();
            var commands = new string[_macroCommandsListBox.Items.Count];
            for (int i = 0; i < _macroCommandsListBox.Items.Count; i++)
            {
                commands[i] = ((ComboBox)_macroCommandsListBox.Items[i]).Text;
            }
            macroData.Macro = commands;
            return macroData;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            var initialDirectory = _initialDirectory ?? "";
            var macroData = _getListBoxMacroData();
            var serialized = _macroDataSerializer.SerializeMacroData(macroData);
            var parameters = new WindowSaveMenuModifierParameters
            {
                InitialDirectory = initialDirectory,
                SaveContent = serialized
            };
            _windowSaveMenuModifier.Modify(parameters);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowSaveMenuModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate
                && data is MaplestoryBotConfiguration configuration
            )
            {
                _initialDirectory = configuration.FramePointsDirectory;
            }
        }
    }


    public class WindowRuneingEditorFramePointSaveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _saveMenuActionHandler;

        public WindowRuneingEditorFramePointSaveActionHandlerFacade(
            Button saveButton,
            ListBox macroCommandsListBox,
            AbstractSaveFileDialog saveFileDialog
        )
        {
            _saveMenuActionHandler = new WindowRuneingEditorFramePointSaveActionHandler(
                saveButton,
                macroCommandsListBox,
                new MacroDataSerializer(),
                new WindowSaveMenuModifier(saveFileDialog)
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _saveMenuActionHandler.OnEvent(sender, e);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _saveMenuActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _saveMenuActionHandler.Inject(dataType, data);
        }
    }


    public class WindowRuneingEditorMovementTag
    {
        public List<string> MovementCommands = [];

        public RuneFrameDirectionTypes Direction = RuneFrameDirectionTypes.Left;

        public int Distance = 0;
    }


    public class WindowRuneingEditorMovementAddModifier : AbstractWindowStateModifier
    {
        private ListBox _movementsListBox;

        private AbstractMacroListBoxItemTemplateFactory _listBoxItemTemplateFactory;

        public WindowRuneingEditorMovementAddModifier(
            ListBox movementsListBox,
            AbstractMacroListBoxItemTemplateFactory listBoxItemTemplateFactory
        )
        {
            _movementsListBox = movementsListBox;
            _listBoxItemTemplateFactory = listBoxItemTemplateFactory;
        }

        private HashSet<string> _getMoveNames()
        {
            var moveNamesList = new List<string>();
            foreach (ListBoxItem listBoxItem in _movementsListBox.Items)
            {
                var listBoxGrid = (Grid)listBoxItem.Content;
                var listTextBoxes = (List<TextBox>)listBoxGrid.Tag;
                if (listTextBoxes.Count > 0)
                {
                    moveNamesList.Add(listTextBoxes[0].Text);
                }
            }
            return moveNamesList.ToHashSet();
        }

        private string _generateMoveName()
        {
            var existingElements = _getMoveNames();
            var elementCount = existingElements.Count;
            while (existingElements.Contains("Move " + elementCount))
            {
                elementCount++;
            }
            return "Move " + elementCount;
        }

        private ListBoxItem _generateListBoxItem()
        {
            var listBoxItem = new ListBoxItem();
            var listBoxGrid = _listBoxItemTemplateFactory.Create();
            var listTextBoxes = (List<TextBox>)listBoxGrid.Tag;
            listBoxItem.Tag = new WindowRuneingEditorMovementTag();
            listBoxItem.Content = listBoxGrid;
            if (listTextBoxes.Count > 0)
            {
                listTextBoxes[0].Text = _generateMoveName();
            }
            return listBoxItem;
        }

        public override void Modify(object? value)
        {
            var listBoxItem = _generateListBoxItem();
            var selectedIndex = (
                _movementsListBox.SelectedIndex
            );
            var insertIndex = (
                selectedIndex != -1 ?
                selectedIndex + 1 :
                _movementsListBox.Items.Count
            );
            _movementsListBox.Items.Insert(
                insertIndex, listBoxItem
            );
            _movementsListBox.SelectedIndex = insertIndex;
        }
    }


    public class WindowRuneingEditorMovementAddActionHandler : AbstractWindowActionHandler
    {
        private Button _movementsAddButton;

        private AbstractWindowStateModifier _movementAddModifier;

        public WindowRuneingEditorMovementAddActionHandler(
            Button movementsAddButton,
            AbstractWindowStateModifier movementAddModifier
        )
        {
            _movementsAddButton = movementsAddButton;
            _movementAddModifier = movementAddModifier;
            _movementsAddButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementAddModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementAddModifier.Modify(null);
        }
    }


    public class WindowRuneingEditorMovementAddActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _movementAddActionHandler;

        public WindowRuneingEditorMovementAddActionHandlerFacade(
            Button movementsAddButton,
            ListBox movementsListBox,
            Grid listBoxGrid
        )
        {
            _movementAddActionHandler = (
                new WindowRuneingEditorMovementAddActionHandler(
                    movementsAddButton,
                    new WindowRuneingEditorMovementAddModifier(
                        movementsListBox,
                        new WindowMacroListBoxItemTemplateFactory(listBoxGrid)
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementAddActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementAddActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorMovementRemoveModifier : AbstractWindowStateModifier
    {
        private ListBox _movementsListBox;

        public WindowRuneingEditorMovementRemoveModifier(
            ListBox movementsListBox
        )
        {
            _movementsListBox = movementsListBox;
        }

        public override void Modify(object? value)
        {
            if (_movementsListBox.Items.Count > 0)
            {
                var removeIndex = (
                    _movementsListBox.SelectedIndex != -1 ?
                    _movementsListBox.SelectedIndex :
                    _movementsListBox.Items.Count - 1
                );
                _movementsListBox.Items.RemoveAt(removeIndex);
            }
        }
    }


    public class WindowRuneingEditorMovementRemoveActionHandler : AbstractWindowActionHandler
    {
        private Button _movementsRemoveButton;

        private AbstractWindowStateModifier _movementRemoveModifier;

        public WindowRuneingEditorMovementRemoveActionHandler(
            Button movementsRemoveButton,
            AbstractWindowStateModifier movementRemoveModifier
        )
        {
            _movementsRemoveButton = movementsRemoveButton;
            _movementRemoveModifier = movementRemoveModifier;
            _movementsRemoveButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementRemoveModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementRemoveModifier.Modify(null);
        }
    }


    public class WindowRuneingEditorMovementRemoveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _movementRemoveActionHandler;

        public WindowRuneingEditorMovementRemoveActionHandlerFacade(
            Button movementsRemoveButton,
            ListBox movementsListBox
        )
        {
            _movementRemoveActionHandler = (
                new WindowRuneingEditorMovementRemoveActionHandler(
                    movementsRemoveButton,
                    new WindowRuneingEditorMovementRemoveModifier(movementsListBox)
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementRemoveActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementRemoveActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorMovementMacroAccessModifier : AbstractWindowStateModifier
    {
        private ListBox _movementsListBox;

        private List<FrameworkElement> _accessElements;

        public WindowRuneingEditorMovementMacroAccessModifier(
            ListBox movementsListBox,
            List<FrameworkElement> accessElements

        )
        {
            _movementsListBox = movementsListBox;
            _accessElements = accessElements;
        }

        public override void Modify(object? value)
        {
            var enabled = _movementsListBox.SelectedItem != null;
            foreach (var accessElement in _accessElements)
            {
                accessElement.IsEnabled = enabled;
                if (!enabled)
                {
                    if (accessElement is TextBox textElement)
                    {
                        textElement.Text = "";
                    }
                    if (accessElement is ComboBox comboBoxElement)
                    {
                        comboBoxElement.Text = "";
                    }
                }
            }
        }
    }


    public class WindowRuneingEditorMovementMacroAccessActionHandler : AbstractWindowActionHandler
    {
        private ListBox _movementsListBox;

        private AbstractWindowStateModifier _movementMacroAccessModifier;

        public WindowRuneingEditorMovementMacroAccessActionHandler(
            ListBox movementsListBox,
            AbstractWindowStateModifier movementMacroAccessModifier
        )
        {
            _movementsListBox = movementsListBox;
            _movementMacroAccessModifier = movementMacroAccessModifier;
            _movementsListBox.SelectionChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementMacroAccessModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementMacroAccessModifier.Modify(null);
        }
    }


    public class WindowRuneingEditorMovementMacroAccessActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _movementMacroAccessActionHandler;

        public WindowRuneingEditorMovementMacroAccessActionHandlerFacade(
            ListBox movementsListBox,
            List<FrameworkElement> accessElements
        )
        {
            _movementMacroAccessActionHandler = (
                new WindowRuneingEditorMovementMacroAccessActionHandler(
                    movementsListBox,
                    new WindowRuneingEditorMovementMacroAccessModifier(
                        movementsListBox,
                        accessElements
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementMacroAccessActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementMacroAccessActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorMovementsCommandAddActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _movementCommandAddActionHandler;

        public WindowRuneingEditorMovementsCommandAddActionHandlerFacade(
            Button movementCommandAddButton,
            ListBox movementCommandsListBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _movementCommandAddActionHandler = (
                new WindowRuneingEditorElementCommandAddActionHandler(
                    movementCommandAddButton,
                    new WindowRuneingEditorElementCommandAddModifier(
                        movementCommandsListBox,
                        new ComboBoxTemplateFactory(comboBoxTemplate),
                        scaleRegistry
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementCommandAddActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementCommandAddActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorMovementsCommandRemoveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _movementsCommandRemoveActionHandler;

        public WindowRuneingEditorMovementsCommandRemoveActionHandlerFacade(
            Button macroCommandRemoveButton,
            ListBox framePointMacroCommandsListBox,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _movementsCommandRemoveActionHandler = (
                new WindowRuneingEditorElementCommandRemoveActionHandler(
                    macroCommandRemoveButton,
                    new WindowRuneingEditorElementCommandRemoveModifier(
                        framePointMacroCommandsListBox,
                        scaleRegistry
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementsCommandRemoveActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementsCommandRemoveActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorMovementsCommandClearActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _movementsCommandClearActionHandler;

        public WindowRuneingEditorMovementsCommandClearActionHandlerFacade(
            Button movementsClearButton,
            ListBox movementCommandsListBox,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _movementsCommandClearActionHandler = (
                new WindowRuneingEditorElementCommandClearActionHandler(
                    movementsClearButton,
                    new WindowRuneingEditorElementCommandClearModifier(
                        movementCommandsListBox,
                        scaleRegistry
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementsCommandClearActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementsCommandClearActionHandler.OnEvent(sender, e);
        }
    }
}
