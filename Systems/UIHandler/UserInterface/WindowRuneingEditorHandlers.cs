using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
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
                    listBoxItem.Tag = new List<string>(runeFrameMacro.PointCommands);
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
            _framePointMacrosLoadingActionHandler = new WindowRuneingEditorFramePointMacrosLoadingActionHandler(
                framePointMacrosListBox,
                windowRuneingEditor,
                new WindowRuneingEditorFramePointMacrosLoadingModifier(
                    framePointMacrosListBox,
                    new WindowMacroListBoxItemTemplateFactory(framePointMacroTemplate),
                    editMenuState
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
}
