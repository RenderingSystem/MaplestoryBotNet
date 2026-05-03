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
                    var nextRuneFrame = runeFrameMacro.NextRuneFrame;
                    listBoxItem.Tag = new WindowRuneingEditorFramePointTag
                    {
                        PointCommands = [.. runeFrameMacro.PointCommands],
                        Radius = (int)runeFrameMacro.Radius,
                        NextFrame = (
                            nextRuneFrame != null ?
                            nextRuneFrame.FrameData.FrameName : ""
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


    public class WindowRuneingEditorFrameNameSavingModifier : AbstractWindowStateModifier
    {
        private TextBox _frameNameTextBox;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowRuneingEditorFrameNameSavingModifier(
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
                value is AbstractBottingModel bottingModel
                && _frameNameTextBox.Text is string newFrameName
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && selectedObject.FrameObject is Canvas runeFrameObject
                && runeFrameObject.Tag is MapCanvasRuneFrameDataTag runeFrameDataTag
                && newFrameName != ""
                && bottingModel.GetRuneModel() is AbstractRuneModel runeModel
                && runeModel.RuneFrames().Find((rf) => rf.FrameData.FrameName == newFrameName) is null
                && runeModel.FindRuneFrameByName(runeFrameDataTag.ElementLabel) is RuneFrame runeFrame
            )
            {
                runeFrame.FrameData.FrameName = _frameNameTextBox.Text;
                runeFrameDataTag.FrameName = _frameNameTextBox.Text;
                foreach (var textDependency in runeFrame.FrameData.ElementTexts)
                {
                    if (textDependency is TextBlock textBlock)
                    {
                        textBlock.Text = _frameNameTextBox.Text;
                    }
                    if (textDependency is TextBox textBox)
                    {
                        textBox.Text = _frameNameTextBox.Text;
                    }
                }
                runeModel.EditRuneFrame(runeFrame);
            }
        }
    }


    public class WindowRuneingEditorFrameNameSavingActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _windowRuneingEditor;

        private AbstractWindowStateModifier _frameNameSavingModifier;

        private AbstractBottingModel? _bottingModel;

        public WindowRuneingEditorFrameNameSavingActionHandler(
            AbstractSystemWindow windowRuneingEditor,
            AbstractWindowStateModifier frameNameSavingModifier
        )
        {
            _windowRuneingEditor = windowRuneingEditor;
            _frameNameSavingModifier = frameNameSavingModifier;
            _bottingModel = null;
            var window = (Window)_windowRuneingEditor.GetWindow()!;
            window.IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameNameSavingModifier;
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_windowRuneingEditor.Visible())
            {
                _frameNameSavingModifier.Modify(_bottingModel);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public class WindowRuneingEditorFrameNameSavingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _frameNameSavingActionHandler;

        public WindowRuneingEditorFrameNameSavingActionHandlerFacade(
            AbstractSystemWindow windowRuneingEditor,
            TextBox frameNameTextBox,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _frameNameSavingActionHandler = new WindowRuneingEditorFrameNameSavingActionHandler(
                windowRuneingEditor,
                new WindowRuneingEditorFrameNameSavingModifier(frameNameTextBox, editMenuState)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _frameNameSavingActionHandler.Modifier();
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            _frameNameSavingActionHandler.OnDependencyEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _frameNameSavingActionHandler.Inject(dataType, data);
        }
    }


    public class WindowRuneingEditorFramePointMacrosSavingModifier : AbstractWindowStateModifier
    {
        private ListBox _framePointMacrosListBox;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowRuneingEditorFramePointMacrosSavingModifier(
            ListBox framePointMacrosListBox,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _framePointMacrosListBox = framePointMacrosListBox;
            _editMenuState = editMenuState;
        }

        public override void Modify(object? value)
        {
            _framePointMacrosListBox.SelectedIndex = -1;

            if (
                value is AbstractBottingModel bottingModel
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && selectedObject.FrameObject is Canvas runeFrameObject
                && runeFrameObject.Tag is MapCanvasRuneFrameDataTag runeFrameDataTag
                && runeFrameDataTag.ElementLabel is string elementLabel
                && bottingModel.GetRuneModel() is AbstractRuneModel runeModel
                && runeModel.FindRuneFrameByName(elementLabel) is RuneFrame runeFrame
                && runeFrame.FrameData.RuneFrameMacros is List<RuneFrameMacro> runeFrameMacros
            )
            {
                for (int i = 0; i < runeFrameMacros.Count; i++)
                {
                    var listBoxItem = (ListBoxItem)_framePointMacrosListBox.Items[i];
                    var listTextBoxes = (List<TextBox>)((Grid)listBoxItem.Content).Tag;
                    var framePointTag = (WindowRuneingEditorFramePointTag)listBoxItem.Tag;
                    var runeFrameMacro = runeFrameMacros[i];
                    var textDependencies = runeFrameMacro.TextDependencies;
                    runeFrameMacro.NextRuneFrame = (
                        framePointTag.NextFrame != runeFrameDataTag.FrameName ?
                        runeModel.FindRuneFrameRefByLabel(framePointTag.NextFrame) : null
                    );
                    runeFrameMacro.MacroName = (
                        (listTextBoxes.Count > 0) ?
                        listTextBoxes[0].Text : runeFrameMacro.MacroName
                    );
                    runeFrameMacro.PointCommands = [
                        .. framePointTag.PointCommands
                    ];
                    runeFrameMacro.Radius = framePointTag.Radius;
                    foreach (var textDependency in textDependencies)
                    {
                        if (textDependency is TextBlock textBlock)
                        {
                            textBlock.Text = runeFrameMacro.MacroName;
                        }
                        if (textDependency is TextBox textBox)
                        {
                            textBox.Text = runeFrameMacro.MacroName;
                        }
                    }
                }
                runeModel.EditRuneFrame(runeFrame);
            }
        }
    }


    public class WindowRuneingEditorFramePointMacrosSavingActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _windowRuneingEditor;

        private AbstractWindowStateModifier _framePointMacrosSavingModifier;

        private AbstractBottingModel? _bottingModel;

        public WindowRuneingEditorFramePointMacrosSavingActionHandler(
            AbstractSystemWindow windowRuneingEditor,
            AbstractWindowStateModifier framePointMacrosSavingModifier
        )
        {
            _windowRuneingEditor = windowRuneingEditor;
            _framePointMacrosSavingModifier = framePointMacrosSavingModifier;
            _bottingModel = null;
            var window = (Window)_windowRuneingEditor.GetWindow()!;
            window.IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacrosSavingModifier;
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_windowRuneingEditor.Visible())
            {
                _framePointMacrosSavingModifier.Modify(_bottingModel);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public class WindowRuneingEditorFramePointMacrosSavingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacrosSavingActionHandler;

        public WindowRuneingEditorFramePointMacrosSavingActionHandlerFacade(
            AbstractSystemWindow windowRuneingEditor,
            ListBox framePointMacrosListBox,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _framePointMacrosSavingActionHandler = new WindowRuneingEditorFramePointMacrosSavingActionHandler(
                windowRuneingEditor,
                new WindowRuneingEditorFramePointMacrosSavingModifier(
                    framePointMacrosListBox, editMenuState
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacrosSavingActionHandler.Modifier();
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            _framePointMacrosSavingActionHandler.OnDependencyEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _framePointMacrosSavingActionHandler.Inject(dataType, data);
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
                        var parameters = new WindowComboBoxScaleActionHandlerParameters(comboBox);
                        comboBox.Text = command;
                        _framePointMacroCommandsListBox.Items.Add(comboBox);
                        _scaleRegistry.RegisterHandler(parameters);
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
            var parameters = new WindowComboBoxScaleActionHandlerParameters(comboBox);
            _elementCommandsListBox.Items.Insert(
                insertIndex, comboBox
            );
            _scaleRegistry.RegisterHandler(parameters);
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


    public class WindowRuneingEditorUniformMovementsLoadingModifier : AbstractWindowStateModifier
    {
        private CheckBox _uniformMovementsCheckbox;

        public WindowRuneingEditorUniformMovementsLoadingModifier(
            CheckBox uniformMovementsCheckbox
        )
        {
            _uniformMovementsCheckbox = uniformMovementsCheckbox;
        }

        public override void Modify(object? value)
        {
            if (
                value is AbstractBottingModel bottingModel
                && bottingModel.GetRuneModel() is AbstractRuneModel runeModel
            )
            {
                _uniformMovementsCheckbox.IsChecked = runeModel.GetUniformMovement() != 0;
            }
        }
    }


    public class WindowRuneingEditorUniformMovementsLoadingActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _windowRuneingEditor;

        private AbstractWindowStateModifier _uniformMovementsLoadingModifier;

        private AbstractBottingModel? _bottingModel;

        public WindowRuneingEditorUniformMovementsLoadingActionHandler(
            AbstractSystemWindow windowRuneingEditor,
            AbstractWindowStateModifier uniformMovementsLoadingModifier
        )
        {
            _windowRuneingEditor = windowRuneingEditor;
            _uniformMovementsLoadingModifier = uniformMovementsLoadingModifier;
            _bottingModel = null;
            var window = (Window)_windowRuneingEditor.GetWindow()!;
            window.IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _uniformMovementsLoadingModifier;
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
                _uniformMovementsLoadingModifier.Modify(_bottingModel);
            }
        }
    }


    public class WindowRuneingEditorUniformMovementsLoadingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacrosLoadingActionHandler;

        public WindowRuneingEditorUniformMovementsLoadingActionHandlerFacade(
            CheckBox uniformMovementsCheckbox,
            AbstractSystemWindow windowRuneingEditor
        )
        {
            _framePointMacrosLoadingActionHandler = (
                new WindowRuneingEditorUniformMovementsLoadingActionHandler(
                    windowRuneingEditor,
                    new WindowRuneingEditorUniformMovementsLoadingModifier(
                        uniformMovementsCheckbox
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


    public class WindowRuneingEditorMovementsLoadingModifier : AbstractWindowStateModifier
    {
        private ListBox _movementsListBox;

        private AbstractMacroListBoxItemTemplateFactory _listBoxItemTemplateFactory;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowRuneingEditorMovementsLoadingModifier(
            ListBox movementsListBox,
            AbstractMacroListBoxItemTemplateFactory listBoxItemTemplateFactory,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _movementsListBox = movementsListBox;
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
                && runeFrame.FrameData.RuneFrameDirections is List<RuneFrameDirection> runeFrameDirections
            )
            {
                foreach (var runeFrameDirection in runeFrameDirections)
                {
                    var listBoxItem = new ListBoxItem();
                    var listBoxGrid = _listBoxItemTemplateFactory.Create();
                    var listTextBoxes = (List<TextBox>)listBoxGrid.Tag;
                    listBoxItem.Tag = new WindowRuneingEditorMovementTag
                    {
                        MovementCommands = [.. runeFrameDirection.DirectionCommands],
                        Direction = runeFrameDirection.Direction,
                        Distance = runeFrameDirection.Distance
                    };
                    listBoxItem.Content = listBoxGrid;
                    if (listTextBoxes.Count > 0)
                    {
                        listTextBoxes[0].Text = runeFrameDirection.DirectionName;
                    }
                    _movementsListBox.Items.Add(listBoxItem);
                }
                if (_movementsListBox.Items.Count > 0)
                {
                    _movementsListBox.SelectedIndex = 0;
                }
            }
        }
    }


    public class WindowRuneingEditorMovementsLoadingActionHandler : AbstractWindowActionHandler
    {
        private ListBox _movementsListBox;

        private AbstractSystemWindow _windowRuneingEditor;

        private AbstractWindowStateModifier _movementsAddModifier;

        private AbstractBottingModel? _bottingModel;

        public WindowRuneingEditorMovementsLoadingActionHandler(
            ListBox framePointMacrosListBox,
            AbstractSystemWindow windowRuneingEditor,
            AbstractWindowStateModifier framePointMacrosAddModifier
        )
        {
            _movementsListBox = framePointMacrosListBox;
            _windowRuneingEditor = windowRuneingEditor;
            _movementsAddModifier = framePointMacrosAddModifier;
            _bottingModel = null;
            var window = (Window)_windowRuneingEditor.GetWindow()!;
            window.IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementsAddModifier;
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
                _movementsListBox.Items.Clear();
                _movementsAddModifier.Modify(_bottingModel);
            }
        }
    }


    public class WindowRuneingEditorMovementsLoadingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacrosLoadingActionHandler;

        public WindowRuneingEditorMovementsLoadingActionHandlerFacade(
            ListBox movementsListBox,
            FrameworkElement movementsTemplate,
            AbstractSystemWindow windowRuneingEditor,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _framePointMacrosLoadingActionHandler = (
                new WindowRuneingEditorMovementsLoadingActionHandler(
                    movementsListBox,
                    windowRuneingEditor,
                    new WindowRuneingEditorMovementsLoadingModifier(
                        movementsListBox,
                        new WindowMacroListBoxItemTemplateFactory(movementsTemplate),
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


    public class WindowRuneingEditorMovementsSavingModifier : AbstractWindowStateModifier
    {
        private ListBox _movementsListBox;

        private CheckBox _uniformMovementsCheckbox;

        private AbstractWindowMapEditMenuState _editMenuState;

        public WindowRuneingEditorMovementsSavingModifier(
            ListBox movementsListBox,
            CheckBox uniformMovementsCheckbox,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _movementsListBox = movementsListBox;
            _uniformMovementsCheckbox = uniformMovementsCheckbox;
            _editMenuState = editMenuState;
        }

        private void _assignMovements(AbstractRuneModel runeModel, RuneFrame runeFrame)
        {
            var isChecked = _uniformMovementsCheckbox.IsChecked! == true ? 1 : 0;
            var runeFrameDirections = runeFrame.FrameData.RuneFrameDirections;
            runeFrameDirections.Clear();
            for (int i = 0; i < _movementsListBox.Items.Count; i++)
            {
                var listBoxItem = (ListBoxItem)_movementsListBox.Items[i];
                var listTextBoxes = (List<TextBox>)((Grid)listBoxItem.Content).Tag;
                var movementTag = (WindowRuneingEditorMovementTag)listBoxItem.Tag;
                runeFrameDirections.Add(
                    new RuneFrameDirection
                    {
                        DirectionName = listTextBoxes.Count > 0 ? listTextBoxes[0].Text : "",
                        DirectionCommands = [.. movementTag.MovementCommands],
                        Distance = movementTag.Distance,
                        Direction = movementTag.Direction
                    }
                );
            }
            runeModel.EditRuneFrame(runeFrame);
        }

        public override void Modify(object? value)
        {
            _movementsListBox.SelectedIndex = -1;

            if (
                value is AbstractBottingModel bottingModel
                && _editMenuState.Selected() is WindowMapEditMenuFrameSelectedObject selectedObject
                && selectedObject.FrameObject is Canvas runeFrameObject
                && runeFrameObject.Tag is MapCanvasRuneFrameDataTag runeFrameDataTag
                && runeFrameDataTag.ElementLabel is string elementLabel
                && bottingModel.GetRuneModel() is AbstractRuneModel runeModel
            )
            {
                var isChecked = (bool)_uniformMovementsCheckbox.IsChecked!;
                if (!isChecked)
                {
                    if (runeModel.FindRuneFrameByName(elementLabel) is RuneFrame runeFrame)
                    {
                        _assignMovements(runeModel, runeFrame);
                    }
                }
                else
                {
                    foreach (var runeFrame in runeModel.RuneFrames())
                    {
                        _assignMovements(runeModel, runeFrame);
                    }
                }
                runeModel.SetUniformMovement(isChecked ? 1 : 0);
            }
        }
    }


    public class WindowRuneingEditorMovementsSavingActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _windowRuneingEditor;

        private AbstractWindowStateModifier _movementsSavingModifier;

        private AbstractBottingModel? _bottingModel;

        public WindowRuneingEditorMovementsSavingActionHandler(
            AbstractSystemWindow windowRuneingEditor,
            AbstractWindowStateModifier movementsSavingModifier
        )
        {
            _windowRuneingEditor = windowRuneingEditor;
            _movementsSavingModifier = movementsSavingModifier;
            _bottingModel = null;
            var window = (Window)_windowRuneingEditor.GetWindow()!;
            window.IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementsSavingModifier;
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_windowRuneingEditor.Visible())
            {
                _movementsSavingModifier.Modify(_bottingModel);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.BottingModel && data is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public class WindowRuneingEditorMovementsSavingActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointMacrosSavingActionHandler;

        public WindowRuneingEditorMovementsSavingActionHandlerFacade(
            AbstractSystemWindow windowRuneingEditor,
            ListBox movementsListBox,
            CheckBox uniformMovementsCheckBox,
            AbstractWindowMapEditMenuState editMenuState
        )
        {
            _framePointMacrosSavingActionHandler = new WindowRuneingEditorMovementsSavingActionHandler(
                windowRuneingEditor,
                new WindowRuneingEditorMovementsSavingModifier(
                    movementsListBox, uniformMovementsCheckBox, editMenuState
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _framePointMacrosSavingActionHandler.Modifier();
        }

        public override void OnDependencyEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            _framePointMacrosSavingActionHandler.OnDependencyEvent(sender, e);
        }

        public override void Inject(object dataType, object? data)
        {
            _framePointMacrosSavingActionHandler.Inject(dataType, data);
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


    public class WindowRuneingEditorMovementsDeselectionModifier : AbstractWindowStateModifier
    {
        private ComboBox _directionComboBox;

        private TextBox _distanceTextBox;

        private ListBox _movementCommandsListBox;

        private AbstractWindowActionHandlerRegistry _scaleRegistry;

        public WindowRuneingEditorMovementsDeselectionModifier(
            ComboBox directionComboBox,
            TextBox distanceTextBox,
            ListBox movementCommandsListBox,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _directionComboBox = directionComboBox;
            _distanceTextBox = distanceTextBox;
            _movementCommandsListBox = movementCommandsListBox;
            _scaleRegistry = scaleRegistry;
        }

        public override void Modify(object? value)
        {
            if (
                value is ListBoxItem deselectedItem
                && deselectedItem.Tag is WindowRuneingEditorMovementTag deselectedItemTag
            )
            {
                deselectedItemTag.Direction = (
                    _directionComboBox.Text == "Left" ?
                    RuneFrameDirectionTypes.Left :
                    _directionComboBox.Text == "Right" ?
                    RuneFrameDirectionTypes.Right :
                    RuneFrameDirectionTypes.MaxNum
                );
                if (int.TryParse(_distanceTextBox.Text, out int distance))
                {
                    deselectedItemTag.Distance = distance;
                }
                deselectedItemTag.MovementCommands.Clear();
                foreach (ComboBox comboBox in _movementCommandsListBox.Items)
                {
                    deselectedItemTag.MovementCommands.Add(comboBox.Text);
                }
                _directionComboBox.Text = "";
                _distanceTextBox.Text = "";
                _scaleRegistry.ClearHandlers();
                _movementCommandsListBox.Items.Clear();
            }
        }
    }


    public class WindowRuneingEditorMovementsDeselectionActionHandler : AbstractWindowActionHandler
    {
        private ListBox _movementsListBox;

        private AbstractWindowStateModifier _movementsDeselectionModifier;

        public WindowRuneingEditorMovementsDeselectionActionHandler(
            ListBox movementsListBox,
            AbstractWindowStateModifier movementsDeselectionModifier
        )
        {
            _movementsListBox = movementsListBox;
            _movementsDeselectionModifier = movementsDeselectionModifier;
            _movementsListBox.SelectionChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementsDeselectionModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs
                && selectionArgs.RemovedItems.Count > 0
                && selectionArgs.RemovedItems[0] is ListBoxItem deselectedItem
            )
            {
                _movementsDeselectionModifier.Modify(deselectedItem);
            }
        }
    }


    public class WindowRuneingEditorMovementsDeselectionActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _movementsDeselectionActionHandler;

        public WindowRuneingEditorMovementsDeselectionActionHandlerFacade(
            ComboBox directionComboBox,
            TextBox distanceTextBox,
            ListBox movementsListBox,
            ListBox movementCommandsListBox,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _movementsDeselectionActionHandler = new WindowRuneingEditorMovementsDeselectionActionHandler(
                movementsListBox,
                new WindowRuneingEditorMovementsDeselectionModifier(
                    directionComboBox,
                    distanceTextBox,
                    movementCommandsListBox,
                    scaleRegistry
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementsDeselectionActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementsDeselectionActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorMovementsSelectionModifier : AbstractWindowStateModifier
    {
        private ComboBox _directionComboBox;

        private TextBox _distanceTextBox;

        private ListBox _movementCommandsListBox;

        private AbstractComboBoxFactory _comboBoxFactory;

        private AbstractWindowActionHandlerRegistry _scaleRegistry;

        public WindowRuneingEditorMovementsSelectionModifier(
            ComboBox directionComboBox,
            TextBox distanceTextBox,
            ListBox movementCommandsListBox,
            AbstractComboBoxFactory comboBoxFactory,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _directionComboBox = directionComboBox;
            _distanceTextBox = distanceTextBox;
            _movementCommandsListBox = movementCommandsListBox;
            _comboBoxFactory = comboBoxFactory;
            _scaleRegistry = scaleRegistry;
        }

        public override void Modify(object? value)
        {
            if (
                value is ListBoxItem deselectedItem
                && deselectedItem.Tag is WindowRuneingEditorMovementTag deselectedItemTag
            )
            {
                _directionComboBox.Text = deselectedItemTag.Direction.ToString();
                _distanceTextBox.Text = deselectedItemTag.Distance.ToString();
                foreach (var movementCommand in deselectedItemTag.MovementCommands)
                {
                    var comboBox = _comboBoxFactory.Create();
                    var parameters = new WindowComboBoxScaleActionHandlerParameters(comboBox);
                    comboBox.Text = movementCommand;
                    _scaleRegistry.RegisterHandler(parameters);
                    _movementCommandsListBox.Items.Add(comboBox);
                }
            }
        }
    }


    public class WindowRuneingEditorMovementsSelectionActionHandler : AbstractWindowActionHandler
    {
        private ListBox _movementsListBox;

        private AbstractWindowStateModifier _movementsSelectionModifier;

        public WindowRuneingEditorMovementsSelectionActionHandler(
            ListBox movementsListBox,
            AbstractWindowStateModifier movementsSelectionModifier
        )
        {
            _movementsListBox = movementsListBox;
            _movementsSelectionModifier = movementsSelectionModifier;
            _movementsListBox.SelectionChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementsSelectionModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is SelectionChangedEventArgs selectionArgs
                && selectionArgs.AddedItems.Count > 0
                && selectionArgs.AddedItems[0] is ListBoxItem selectedItem
            )
            {
                _movementsSelectionModifier.Modify(selectedItem);
            }
        }
    }


    public class WindowRuneingEditorMovementsSelectionActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _movementSelectionActionHandler;

        public WindowRuneingEditorMovementsSelectionActionHandlerFacade(
            ComboBox directionComboBox,
            TextBox distanceTextBox,
            ListBox movementsListBox,
            ListBox movementCommandsListBox,
            ComboBox comboBoxTemplate,
            AbstractWindowActionHandlerRegistry scaleRegistry
        )
        {
            _movementSelectionActionHandler = new WindowRuneingEditorMovementsSelectionActionHandler(
                movementsListBox,
                new WindowRuneingEditorMovementsSelectionModifier(
                    directionComboBox,
                    distanceTextBox,
                    movementCommandsListBox,
                    new ComboBoxTemplateFactory(comboBoxTemplate),
                    scaleRegistry
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _movementSelectionActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _movementSelectionActionHandler.OnEvent(sender, e);
        }
    }


    public class WindowRuneingEditorMovementsLoadConfigurationActionHandler : AbstractWindowActionHandler
    {
        private Button _loadButton;

        private AbstractWindowStateModifier _windowLoadDialogModifier;

        private string? _initialDirectory;

        public WindowRuneingEditorMovementsLoadConfigurationActionHandler(
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
                _initialDirectory = configuration.FrameMovementsDirectory;
            }
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowLoadDialogModifier;
        }
    }


    public class WindowRuneingEditorMovementsLoadConfigurationActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _mapEditorLoadConfigurationActionHandler;

        public WindowRuneingEditorMovementsLoadConfigurationActionHandlerFacade(
            Button loadButton, AbstractLoadFileDialog loadFileDialog
        )
        {
            _mapEditorLoadConfigurationActionHandler = (
                new WindowRuneingEditorMovementsLoadConfigurationActionHandler(
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


    public class WindowRuneingEditorMovementsLoadActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _framePointLoadActionHandler;

        public WindowRuneingEditorMovementsLoadActionHandlerFacade(
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


    public class WindowRuneingEditorMovementsSaveActionHandler : AbstractWindowActionHandler
    {
        private Button _saveButton;

        private ListBox _movementsListBox;

        private AbstractMacroDataSerializer _macroDataSerializer;

        private AbstractWindowStateModifier _windowSaveMenuModifier;

        private string? _initialDirectory;

        public WindowRuneingEditorMovementsSaveActionHandler(
            Button saveButton,
            ListBox listBox,
            AbstractMacroDataSerializer macroDataSerializer,
            AbstractWindowStateModifier windowSaveMenuHandler
        )
        {
            _saveButton = saveButton;
            _movementsListBox = listBox;
            _macroDataSerializer = macroDataSerializer;
            _windowSaveMenuModifier = windowSaveMenuHandler;
            _initialDirectory = null;
            _saveButton.Click += OnEvent;
        }

        private ConfigurationMacroData _getListBoxMacroData()
        {
            var macroData = new ConfigurationMacroData();
            var commands = new string[_movementsListBox.Items.Count];
            for (int i = 0; i < _movementsListBox.Items.Count; i++)
            {
                commands[i] = ((ComboBox)_movementsListBox.Items[i]).Text;
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
                _initialDirectory = configuration.FrameMovementsDirectory;
            }
        }
    }


    public class WindowRuneingEditorMovementsSaveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _saveMenuActionHandler;

        public WindowRuneingEditorMovementsSaveActionHandlerFacade(
            Button saveButton,
            ListBox macroCommandsListBox,
            AbstractSaveFileDialog saveFileDialog
        )
        {
            _saveMenuActionHandler = new WindowRuneingEditorMovementsSaveActionHandler(
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
}
