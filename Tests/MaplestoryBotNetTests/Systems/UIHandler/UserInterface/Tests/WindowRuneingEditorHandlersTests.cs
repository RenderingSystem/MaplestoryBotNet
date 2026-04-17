using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNetTests.Systems.Tests;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class WindowRuneingEditorFramePointMacrosLoadingActionHandlerTests
    {
        private ListBox _framePointMacrosListBox = new ListBox();

        private FrameworkElement _framePointMacroTemplate = new FrameworkElement();

        private MockSystemWindow _windowRuneingEditor = new MockSystemWindow();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private Grid _framePointMacroTemplateGrid()
        {
            var grid = new Grid { Width = 123, Height = 234 };
            var textBox = new TextBox
            {
                Margin = new Thickness(12, 23, 34, 45),
                Background = Brushes.Black,
                Foreground = Brushes.Lime,
                FontFamily = new FontFamily("Courier New"),
                Height = 22
            };
            grid.Children.Add(textBox);
            return grid;
        }

        private AbstractWindowActionHandler _fixture()
        {
            _framePointMacrosListBox = new ListBox();
            _framePointMacroTemplate = _framePointMacroTemplateGrid();
            _windowRuneingEditor = new MockSystemWindow();
            _editMenuState = new WindowMapEditMenuState();
            _bottingModel = DataFixtures.BottingModelFixture();
            _windowRuneingEditor.GetWindowReturn.Add(new Window());
            var handler = new WindowRuneingEditorFramePointMacrosLoadingActionHandlerFacade(
                _framePointMacrosListBox,
                _framePointMacroTemplate,
                _windowRuneingEditor,
                _editMenuState
            );
            handler.Inject(SystemInjectType.BottingModel, _bottingModel);
            return handler;
        }

        private WindowMapEditMenuFrameSelectedObject _selectedObject(string elementLabel)
        {
            return new WindowMapEditMenuFrameSelectedObject
            {
                FrameObject = new Canvas
                {
                    Tag = new MapCanvasRuneFrameDataTag { ElementLabel = elementLabel }
                }
            };
        }

        /**
         * @brief Verifies that macro points are populated in the list box when the editor
         * becomes visible
         * 
         * When users open the Runeing Editor window while a frame is selected, all point
         * macros belonging to that frame should appear as list items. Each macro is
         * displayed as a Grid containing editable text boxes for macro configuration.
         * The Grid stores references to its text boxes in the Tag property for later
         * updates.
         */
        private void _testMacroPointsPopulatedWhenEditorIsVisible()
        {
            for (int i = 0; i < 2; i++)
            {
                var framePointMacrosLoadingActionHandler = _fixture();
                _windowRuneingEditor.VisibleReturn.Add(i == 1);
                _editMenuState.Select(_selectedObject("FT0"));
                framePointMacrosLoadingActionHandler.OnDependencyEvent(
                    framePointMacrosLoadingActionHandler,
                    new DependencyPropertyChangedEventArgs()
                );
                if (i == 1)
                {
                    Debug.Assert(_framePointMacrosListBox.Items.Count == 2);
                    var item0 = (ListBoxItem)_framePointMacrosListBox.Items[0];
                    var item1 = (ListBoxItem)_framePointMacrosListBox.Items[1];
                    Debug.Assert(item0.Content is Grid);
                    Debug.Assert(item1.Content is Grid);
                    Debug.Assert(item0.Tag is List<string>);
                    Debug.Assert(item1.Tag is List<string>);
                    var itemGrid0 = (Grid)item0.Content;
                    var itemGrid1 = (Grid)item1.Content;
                    Debug.Assert(itemGrid0.Tag is List<TextBox>);
                    Debug.Assert(itemGrid1.Tag is List<TextBox>);
                    var itemList0 = (List<TextBox>)itemGrid0.Tag;
                    var itemList1 = (List<TextBox>)itemGrid1.Tag;
                    var itemGrid0TextList = itemGrid0.Children.OfType<TextBox>().ToList();
                    var itemGrid1TextList = itemGrid1.Children.OfType<TextBox>().ToList();
                    Debug.Assert(itemList0.Count == 1);
                    Debug.Assert(itemList1.Count == 1);
                    Debug.Assert(itemGrid0TextList.Count == itemList0.Count);
                    Debug.Assert(itemGrid1TextList.Count == itemList1.Count);
                    for (int j = 0; j < itemList0.Count; j++)
                    {
                        Debug.Assert(itemGrid0TextList.IndexOf(itemList0[j]) != -1);
                    }
                    for (int j = 0; j < itemList1.Count; j++)
                    {
                        Debug.Assert(itemGrid1TextList.IndexOf(itemList1[j]) != -1);
                    }
                }
                else
                {
                    Debug.Assert(_framePointMacrosListBox.Items.Count == 0);
                }
            }
        }

        /**
         * @brief Verifies that existing macro points are cleared from the list box
         * regardless of editor visibility state
         * 
         * When the visibily event is triggered, the list box should be completely cleared
         * of any existing macro point items. This ensures the list box starts empty
         * before any new population logic runs.
         */
        private void _testMacroPointsCleared()
        {
            for (int i = 1; i < 10; i++)
            for (int j = 0; j < 2; j++)
            {
                var framePointMacrosLoadingActionHandler = _fixture();
                framePointMacrosLoadingActionHandler.Inject(
                    SystemInjectType.BottingModel, new BottingModel()
                );
                _windowRuneingEditor.VisibleReturn.Add(j == 1);
                _editMenuState.Select(_selectedObject("FT0"));
                for (int k = 0; k < i; k++)
                    _framePointMacrosListBox.Items.Add(new object());
                framePointMacrosLoadingActionHandler.OnDependencyEvent(
                    framePointMacrosLoadingActionHandler,
                    new DependencyPropertyChangedEventArgs()
                );
                Debug.Assert(_framePointMacrosListBox.Items.Count == (j == 1 ? 0 : i));
            }
        }

        /**
         * @brief Verifies that macro point names are correctly displayed in the editor
         * 
         * When users open the Runeing Editor window, each point macro displays its
         * macro name (e.g., "M0", "M1") in a text box within its grid item. This allows
         * users to identify and edit the macro's name directly in the editor interface.
         */
        private void _testMacroPointNamesWhenEditorIsVisible()
        {
            for (int i = 0; i < 2; i++)
            {
                var framePointMacrosLoadingActionHandler = _fixture();
                _windowRuneingEditor.VisibleReturn.Add(i == 1);
                _editMenuState.Select(_selectedObject("FT0"));
                framePointMacrosLoadingActionHandler.OnDependencyEvent(
                    framePointMacrosLoadingActionHandler,
                    new DependencyPropertyChangedEventArgs()
                );
                if (i == 1)
                {
                    var item0 = (ListBoxItem)_framePointMacrosListBox.Items[0];
                    var item1 = (ListBoxItem)_framePointMacrosListBox.Items[1];
                    var itemGrid0 = (Grid)item0.Content;
                    var itemGrid1 = (Grid)item1.Content;
                    var itemList0 = (List<TextBox>)itemGrid0.Tag;
                    var itemList1 = (List<TextBox>)itemGrid1.Tag;
                    Debug.Assert(itemList0[0].Text == "M0");
                    Debug.Assert(itemList1[0].Text == "M1");
                }
                else
                {
                    Debug.Assert(_framePointMacrosListBox.Items.Count == 0);
                }
            }
        }

        /**
         * @brief Verifies that macro point text boxes have the correct visual properties
         * 
         * When macro points are loaded into the editor, each text box within the macro
         * point grid must display with consistent styling for proper visibility and
         * usability.
         */
        private void _testMacroPointTextBoxProperties()
        {
            var framePointMacrosLoadingActionHandler = _fixture();
            _windowRuneingEditor.VisibleReturn.Add(true);
            _editMenuState.Select(_selectedObject("FT0"));
            framePointMacrosLoadingActionHandler.OnDependencyEvent(
                framePointMacrosLoadingActionHandler,
                new DependencyPropertyChangedEventArgs()
            );
            var item0 = (ListBoxItem)_framePointMacrosListBox.Items[0];
            var item1 = (ListBoxItem)_framePointMacrosListBox.Items[1];
            var itemGrid0 = (Grid)item0.Content;
            var itemGrid1 = (Grid)item1.Content;
            var itemList0 = (List<TextBox>)itemGrid0.Tag;
            var itemList1 = (List<TextBox>)itemGrid1.Tag;
            var itemListList = new List<List<TextBox>> { itemList0, itemList1 };
            foreach (var itemList in itemListList)
            foreach (var textBox in itemList)
            {
                Debug.Assert(textBox.Margin.Left == 12);
                Debug.Assert(textBox.Margin.Top == 23);
                Debug.Assert(textBox.Margin.Right == 34);
                Debug.Assert(textBox.Margin.Bottom == 45);
                Debug.Assert(textBox.Background == Brushes.Black);
                Debug.Assert(textBox.Foreground == Brushes.Lime);
                Debug.Assert(textBox.FontFamily.ToString() == "Courier New");
                Debug.Assert(textBox.Height == 22);
            }
        }

        /**
         * @brief Verifies that macro point command sequences are correctly loaded
         * and stored
         * 
         * When macro points are loaded into the editor, each macro point must also store
         * its associated command sequence (the automation commands that execute when the
         * macro is triggered). These commands are stored in the Tag property of each
         * list box item, allowing the editor to later display and edit the command list.
         */
        private void _testMacroPointCommands()
        {
            var framePointMacrosLoadingActionHandler = _fixture();
            _windowRuneingEditor.VisibleReturn.Add(true);
            _editMenuState.Select(_selectedObject("FT0"));
            framePointMacrosLoadingActionHandler.OnDependencyEvent(
                framePointMacrosLoadingActionHandler,
                new DependencyPropertyChangedEventArgs()
            );
            var item0 = (ListBoxItem)_framePointMacrosListBox.Items[0];
            var item1 = (ListBoxItem)_framePointMacrosListBox.Items[1];
            var item0Commands = (List<string>)item0.Tag;
            var item1Commands = (List<string>)item1.Tag;
            Debug.Assert(item0Commands.Count == 3);
            Debug.Assert(item0Commands[0] == "C345");
            Debug.Assert(item0Commands[1] == "C456");
            Debug.Assert(item0Commands[2] == "C567");
            Debug.Assert(item1Commands.Count == 3);
            Debug.Assert(item1Commands[0] == "C456");
            Debug.Assert(item1Commands[1] == "C567");
            Debug.Assert(item1Commands[2] == "C678");
        }

        /**
         * @brief Verifies that the first macro point is automatically selected after loading
         * 
         * When macro points are loaded into the editor, the first macro point in the list
         * should be automatically selected. This provides immediate visual feedback to
         * users that macros are ready for editing and allows them to start working with
         * the first macro without having to manually click on it. Auto-selecting the
         * first item also ensures that property editors or detail panels have a default
         * item to display when the list loads.
         */
        private void _testFirstMacroPointSelected()
        {
            var framePointMacrosLoadingActionHandler = _fixture();
            _windowRuneingEditor.VisibleReturn.Add(true);
            _editMenuState.Select(_selectedObject("FT0"));
            framePointMacrosLoadingActionHandler.OnDependencyEvent(
                framePointMacrosLoadingActionHandler,
                new DependencyPropertyChangedEventArgs()
            );
            Debug.Assert(_framePointMacrosListBox.SelectedIndex == 0);
        }

        public void Run()
        {
            _testMacroPointsPopulatedWhenEditorIsVisible();
            _testMacroPointsCleared();
            _testMacroPointNamesWhenEditorIsVisible();
            _testMacroPointTextBoxProperties();
            _testMacroPointCommands();
            _testFirstMacroPointSelected();
        }
    }


    public class WindowRuneingEditorHandlersTestSuite
    {
        public void Run()
        {
            new WindowRuneingEditorFramePointMacrosLoadingActionHandlerTests().Run();
        }
    }
}
