using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNetTests.Systems.Tests;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;


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
                    Debug.Assert(item0.Tag is WindowRuneingEditorFramePointTag);
                    Debug.Assert(item1.Tag is WindowRuneingEditorFramePointTag);
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
            var item0Commands = ((WindowRuneingEditorFramePointTag)item0.Tag).PointCommands;
            var item1Commands = ((WindowRuneingEditorFramePointTag)item1.Tag).PointCommands;
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
         * @brief Verifies that each macro point stores its correct activation radius for
         * in-game character proximity
         * 
         * When macro points are loaded into the editor, each point stores a radius value
         * that defines the activation range from the character's position to the point
         * in the game world. When the character moves within this radius of the point,
         * the associated macro commands are triggered. A larger radius means the macro
         * activates from farther away, while a smaller radius requires the character to
         * get closer before execution.
         */
        private void _testMacroPointRadius()
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
            Debug.Assert(((WindowRuneingEditorFramePointTag)item0.Tag).Radius == 67);
            Debug.Assert(((WindowRuneingEditorFramePointTag)item1.Tag).Radius == 78);
        }

        /**
         * @brief Verifies that each macro point stores its next frame reference for chaining
         * 
         * When macro points are loaded into the editor, each point may have a NextFrame
         * reference that defines which frame the bot navigates to after completing
         * this macro point's commands. This enables chaining multiple frames together in
         * sequence. An empty string indicates that the macro point does not transition to
         * another frame. Users can edit this value to change the navigation flow.
         */
        private void _testMacroPointNextRuneFrame()
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
            Debug.Assert(((WindowRuneingEditorFramePointTag)item0.Tag).NextFrame == "F1");
            Debug.Assert(((WindowRuneingEditorFramePointTag)item1.Tag).NextFrame == "");
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
        private void _testMacroPointFirstSelected()
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
            _testMacroPointRadius();
            _testMacroPointNextRuneFrame();
            _testMacroPointFirstSelected();
        }
    }


    public class WindowRuneingEditorFramePointMacroAccessActionHandlerTests
    {
        private ListBox _framePointMacrosListBox = new ListBox();

        private List<FrameworkElement> _accessElements = [];

        private AbstractWindowActionHandler _fixture()
        {
            _framePointMacrosListBox = new ListBox();
            _accessElements = new List<FrameworkElement>
            {
                new Button(),
                new ToggleButton(),
                new TextBox{ Text = "lol" }
            };
            return new WindowRuneingEditorFramePointMacroAccessActionHandlerFacade(
                _framePointMacrosListBox,
                _accessElements
            );
        }

        /**
         * @brief Verifies that access elements are enabled or disabled when items are
         * added to or removed from the macro points list box
         * 
         * When users open the Runeing Editor and macro points are loaded, action buttons
         * and controls should be enabled only when there is at least one macro point
         * available for editing.
         */
        private void _testClearingListBoxItemsModifiesAccessElements()
        {
            var framePointMacroAccessActionHandler = _fixture();
            _framePointMacrosListBox.Items.Add(new ListBoxItem());
            foreach (var accessElement in _accessElements)
            {
                Debug.Assert(accessElement.IsEnabled);
            }
            _framePointMacrosListBox.Items.RemoveAt(0);
            foreach (var accessElement in _accessElements)
            {
                Debug.Assert(!accessElement.IsEnabled);
            }
            _framePointMacrosListBox.Items.Add(new ListBoxItem());
            foreach (var accessElement in _accessElements)
            {
                Debug.Assert(accessElement.IsEnabled);
            }
        }

        /**
         * @brief Verifies that text box content is cleared when the macro points list is empty
         * 
         * When there is no macro point from the list box, any text boxes that display
         * macro point data should be automatically cleared. This prevents the editor from
         * showing stale information from a previously selected macro point.
         */
        private void _testClearingListBoxItemsClearsTextBoxes()
        {
            var framePointMacroAccessActionHandler = _fixture();
            _framePointMacrosListBox.Items.Add(new ListBoxItem());
            Debug.Assert(((TextBox)_accessElements[2]).Text == "lol");
            _framePointMacrosListBox.Items.RemoveAt(0);
            Debug.Assert(((TextBox)_accessElements[2]).Text == "");
        }

        public void Run()
        {
            _testClearingListBoxItemsModifiesAccessElements();
            _testClearingListBoxItemsClearsTextBoxes();
        }
    }


    public class WindowRuneingEditorFramePointMacroDeselectionActionHandlerTests
    {
        private TextBox _nextFrameTextBox = new TextBox();

        private TextBox _radiusTextBox = new TextBox();

        private ListBox _framePointMacrosListBox = new ListBox();

        private ListBox _framePointMacroCommandsListBox = new ListBox();

        private AbstractWindowActionHandler _fixture()
        {
            _nextFrameTextBox = new TextBox();
            _radiusTextBox = new TextBox();
            _framePointMacrosListBox = new ListBox();
            _framePointMacroCommandsListBox = new ListBox();
            return new WindowRuneingEditorFramePointMacroDeselectionActionHandlerFacade(
                _nextFrameTextBox,
                _radiusTextBox,
                _framePointMacrosListBox,
                _framePointMacroCommandsListBox
            );
        }

        /**
         * @brief Verifies that changing selection saves the deselected point's data to its
         * ListBoxItem tag before the new selection is shown
         * 
         * When users edit a macro point's command sequence, radius, or next frame reference
         * in the editor UI, then click on a different macro point in the list box, the
         * system must save all pending changes to the previously selected point before
         * loading the newly selected point's data. This test simulates editing each point's
         * data, then selecting the next point, and verifies that the deselected point's tag
         * contains the saved data.
         */
        private void _testChangingSelectionSavesToListBoxItem()
        {
            for (int itemCount = 2; itemCount < 10; itemCount++)
            {
                var framePointMacroCommandsListBox = _fixture();
                for (int j = 0; j < itemCount; j++)
                {
                    var listBoxItem = new ListBoxItem { Tag = new WindowRuneingEditorFramePointTag() };
                    _framePointMacrosListBox.Items.Add(listBoxItem);
                }
                _framePointMacrosListBox.SelectedIndex = 0;
                for (int selectedIndex = 0; selectedIndex < itemCount; selectedIndex++)
                {
                    _framePointMacrosListBox.SelectedIndex = selectedIndex;
                    _framePointMacroCommandsListBox.Items.Clear();
                    _framePointMacroCommandsListBox.Items.Add(new ComboBox { Text = "C" + (selectedIndex + 0).ToString() });
                    _framePointMacroCommandsListBox.Items.Add(new ComboBox { Text = "C" + (selectedIndex + 1).ToString() });
                    _framePointMacroCommandsListBox.Items.Add(new ComboBox { Text = "C" + (selectedIndex + 2).ToString() });
                    _radiusTextBox.Text = (selectedIndex + 3).ToString();
                    _nextFrameTextBox.Text = (selectedIndex + 4).ToString();
                    _framePointMacrosListBox.SelectedIndex = (selectedIndex + 1) % itemCount;
                    var deselectedItem = (ListBoxItem)_framePointMacrosListBox.Items[selectedIndex];
                    var tag = (WindowRuneingEditorFramePointTag)deselectedItem.Tag;
                    Debug.Assert(tag.PointCommands[0] == "C" + (selectedIndex + 0).ToString());
                    Debug.Assert(tag.PointCommands[1] == "C" + (selectedIndex + 1).ToString());
                    Debug.Assert(tag.PointCommands[2] == "C" + (selectedIndex + 2).ToString());
                    Debug.Assert(tag.Radius == selectedIndex + 3);
                    Debug.Assert(tag.NextFrame == (selectedIndex + 4).ToString());
                }
            }
        }

        public void Run()
        {
            _testChangingSelectionSavesToListBoxItem();
        }
    }


    public class WindowRuneingEditorFramePointMacroSelectionActionHandlerTests
    {
        private ListBox _framePointMacrosListBox = new ListBox();

        private ListBox _framePointMacroCommandsListBox = new ListBox();

        private TextBox _nextFrameTextBox = new TextBox();

        private TextBox _radiusTextBox = new TextBox();

        private ComboBox _comboBoxTemplate = new ComboBox();

        private MockWindowActionHandlerRegistry _scaleRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _framePointMacrosListBox = new ListBox();
            _framePointMacroCommandsListBox = new ListBox();
            _nextFrameTextBox = new TextBox();
            _radiusTextBox = new TextBox();
            _comboBoxTemplate = new ComboBox();
            _scaleRegistry = new MockWindowActionHandlerRegistry();
            return new WindowRuneingEditorFramePointMacroSelectionActionHandlerFacade(
                _framePointMacrosListBox,
                _framePointMacroCommandsListBox,
                _nextFrameTextBox,
                _radiusTextBox,
                _comboBoxTemplate,
                _scaleRegistry
            );
        }

        private ListBoxItem _selectItemFixture(int index)
        {
            return new ListBoxItem
            {
                Tag = new WindowRuneingEditorFramePointTag
                {
                    NextFrame = "F" + index.ToString(),
                    PointCommands = [
                        "C" + (index + 0).ToString(),
                        "C" + (index + 1).ToString(),
                        "C" + (index + 2).ToString()
                    ],
                    Radius = index
                }
            };
        }

        /**
         * @brief Verifies that selecting a macro point populates the command list box
         * with the point's associated command sequence
         * 
         * When users click on a macro point in the list box, the command list box should
         * display all automation commands associated with that point. Each command appears
         * as a ComboBox element, allowing users to view and edit the individual commands
         * that execute when the character activates this macro point. The commands are
         * displayed in the same order they will execute.
         */
        private void _testChangingSelectionPopulatesListBoxCommands()
        {
            for (int i = 1; i < 10; i++)
            {
                var framePointMacroSelectionActionHandler = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _framePointMacrosListBox.Items.Add(_selectItemFixture(j));
                }
                for (int j = 0; j < i; j++)
                {
                    _framePointMacrosListBox.SelectedIndex = j;
                    Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 3);
                    Debug.Assert(_framePointMacroCommandsListBox.Items[0] is ComboBox);
                    Debug.Assert(_framePointMacroCommandsListBox.Items[1] is ComboBox);
                    Debug.Assert(_framePointMacroCommandsListBox.Items[2] is ComboBox);
                    var comboBox0 = (ComboBox)_framePointMacroCommandsListBox.Items[0];
                    var comboBox1 = (ComboBox)_framePointMacroCommandsListBox.Items[1];
                    var comboBox2 = (ComboBox)_framePointMacroCommandsListBox.Items[2];
                    Debug.Assert(comboBox0.Text == "C" + (j + 0).ToString());
                    Debug.Assert(comboBox1.Text == "C" + (j + 1).ToString());
                    Debug.Assert(comboBox2.Text == "C" + (j + 2).ToString());
                }
            }
        }

        /**
         * @brief Verifies that selecting a macro point populates the next frame and
         * radius text boxes with the point's configuration values
         * 
         * When users click on a macro point in the list box, the next frame text box
         * should display which frame the bot navigates to after completing this macro
         * point's commands, and the radius text box should display the activation
         * distance from the character to this point. These values allow users to see
         * and modify how the macro point behaves during automation.
         */
        private void _testChangingSelectionPopulatesTextBoxes()
        {
            for (int i = 1; i < 10; i++)
            {
                var framePointMacroSelectionActionHandler = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _framePointMacrosListBox.Items.Add(_selectItemFixture(j));
                }
                for (int j = 0; j < i; j++)
                {
                    _framePointMacrosListBox.SelectedIndex = j;
                    Debug.Assert(_nextFrameTextBox.Text == "F" + j.ToString());
                    Debug.Assert(_radiusTextBox.Text == j.ToString());
                }
            }
        }

        /**
         * @brief Verifies that combobox dropdown scaling for monitor DPI is properly registered
         * and unregistered when macro point selection changes
         * 
         * When macro points are loaded into the editor, each combobox representing a command
         * must be registered with a scale registry that handles dropdown sizing adjustments
         * based on the current monitor's DPI settings. This ensures combobox dropdowns
         * display correctly on high-DPI displays without being cut off or appearing too small.
         */
        private void _testChangingSelectionUpdatesScaleRegistry()
        {
            var framePointMacroSelectionActionHandler = _fixture();
            _framePointMacrosListBox.Items.Add(_selectItemFixture(0));
            _framePointMacrosListBox.Items.Add(_selectItemFixture(3));

            _framePointMacrosListBox.SelectedIndex = 0;
            Debug.Assert(_scaleRegistry.UnregisterHandlerCalls == 0);
            Debug.Assert(_scaleRegistry.RegisterHandlerCalls == 3);
            var scale00 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.RegisterHandlerCallArg_args[0]!;
            var scale01 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.RegisterHandlerCallArg_args[1]!;
            var scale02 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.RegisterHandlerCallArg_args[2]!;
            Debug.Assert(scale00.ScaleComboBox.Text == "C0");
            Debug.Assert(scale01.ScaleComboBox.Text == "C1");
            Debug.Assert(scale02.ScaleComboBox.Text == "C2");

            _scaleRegistry.RegisterHandlerCalls = 0;
            _scaleRegistry.RegisterHandlerCallArg_args = [];
            _scaleRegistry.CallOrder = [];

            _framePointMacrosListBox.SelectedIndex = 1;
            Debug.Assert(_scaleRegistry.UnregisterHandlerCalls == 3);
            Debug.Assert(_scaleRegistry.RegisterHandlerCalls == 3);
            var scale10 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.UnregisterHandlerCallArg_args[0]!;
            var scale11 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.UnregisterHandlerCallArg_args[1]!;
            var scale12 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.UnregisterHandlerCallArg_args[2]!;
            var scale13 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.RegisterHandlerCallArg_args[0]!;
            var scale14 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.RegisterHandlerCallArg_args[1]!;
            var scale15 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.RegisterHandlerCallArg_args[2]!;
            Debug.Assert(scale10.ScaleComboBox == scale00.ScaleComboBox);
            Debug.Assert(scale11.ScaleComboBox == scale01.ScaleComboBox);
            Debug.Assert(scale12.ScaleComboBox == scale02.ScaleComboBox);
            Debug.Assert(scale13.ScaleComboBox.Text == "C3");
            Debug.Assert(scale14.ScaleComboBox.Text == "C4");
            Debug.Assert(scale15.ScaleComboBox.Text == "C5");
        }

        public void Run()
        {
            _testChangingSelectionPopulatesListBoxCommands();
            _testChangingSelectionPopulatesTextBoxes();
            _testChangingSelectionUpdatesScaleRegistry();
        }
    }


    public class WindowRuneingEditorFramePointMacroCommandAddActionHandlerTests
    {
        private Button _macroCommandAddButton = new Button();

        private ListBox _framePointMacroCommandsListBox = new ListBox();

        private ComboBox _comboBoxTemplate = new ComboBox();

        private MockWindowActionHandlerRegistry _scaleRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _macroCommandAddButton = new Button();
            _framePointMacroCommandsListBox = new ListBox();
            _comboBoxTemplate = new ComboBox();
            _scaleRegistry = new MockWindowActionHandlerRegistry();
            return new WindowRuneingEditorFramePointMacroCommandAddActionHandlerFacade(
                _macroCommandAddButton,
                _framePointMacroCommandsListBox,
                _comboBoxTemplate,
                _scaleRegistry
            );
        }

        /**
         * @brief Verifies that adding a command to an empty list box inserts at index 0
         * 
         * When users open the Runeing Editor for a macro point that has no existing commands,
         * clicking the Add Command button should create the first command combobox at the
         * beginning of the list. This gives users a starting point to build their command
         * sequence from scratch.
         */
        private void _testAddingRuneFrameMacroCommandOnEmptyListBox()
        {
            var framePointMacroCommandAddActionHandler = _fixture();
            _macroCommandAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 1);
            Debug.Assert(_framePointMacroCommandsListBox.Items[0] is ComboBox);
        }

        /**
         * @brief Verifies that adding a command inserts below the currently selected item
         * 
         * When users have an existing command sequence and want to insert a new command
         * between two existing commands, they can select the command that should appear
         * above the new one and click Add Command. The new command should appear immediately
         * below the selected command, preserving the logical execution order.
         */
        private void _testAddingRuneFrameMacroCommandOnSelectedIndex()
        {
            var framePointMacroCommandAddActionHandler = _fixture();
            _framePointMacroCommandsListBox.Items.Add(new object());
            _framePointMacroCommandsListBox.Items.Add(new object());
            _framePointMacroCommandsListBox.Items.Add(new object());
            _framePointMacroCommandsListBox.Items.Add(new object());
            _framePointMacroCommandsListBox.SelectedIndex = 1;
            _macroCommandAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 5);
            Debug.Assert(_framePointMacroCommandsListBox.Items[2] is ComboBox);
        }

        /**
         * @brief Verifies that adding a command with no selection appends at the end
         * 
         * When users want to add a command to the end of an existing command sequence
         * without caring about the current selection, clicking Add Command while no item
         * is selected should append the new command at the end of the list.
         */
        private void _testAddingRuneFrameMacroCommandOnPopulatedListBox()
        {
            var framePointMacroCommandAddActionHandler = _fixture();
            _framePointMacroCommandsListBox.Items.Add(new object());
            _framePointMacroCommandsListBox.Items.Add(new object());
            _framePointMacroCommandsListBox.Items.Add(new object());
            _framePointMacroCommandsListBox.Items.Add(new object());
            _macroCommandAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 5);
            Debug.Assert(_framePointMacroCommandsListBox.Items[4] is ComboBox);
        }

        /**
         * @brief Verifies that newly added command comboboxes are registered with the
         * scale registry for proper DPI scaling
         * 
         * When a new command combobox is added to the macro point's command list, it must
         * be registered with the scale registry. The scale registry handles dropdown sizing
         * adjustments based on the current monitor's DPI settings, ensuring combobox
         * dropdowns display correctly on high-DPI displays without being cut off or
         * appearing too small.
         */
        private void _testAddingRuneFrameMacroComboBoxToScaleRegistry()
        {
            var framePointMacroCommandAddActionHandler = _fixture();
            _macroCommandAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 1);
            Debug.Assert(_framePointMacroCommandsListBox.Items[0] is ComboBox);
            Debug.Assert(_scaleRegistry.RegisterHandlerCalls == 1);
            var comboBoxItem = _framePointMacroCommandsListBox.Items[0];
            var scaleBoxItem = (
                (
                    (WindowComboBoxScaleActionHandlerParameters)
                    _scaleRegistry.RegisterHandlerCallArg_args[0]!
                )
                .ScaleComboBox
            );
            Debug.Assert(comboBoxItem == scaleBoxItem);
        }

        public void Run()
        {
            _testAddingRuneFrameMacroCommandOnEmptyListBox();
            _testAddingRuneFrameMacroCommandOnSelectedIndex();
            _testAddingRuneFrameMacroCommandOnPopulatedListBox();
            _testAddingRuneFrameMacroComboBoxToScaleRegistry();
        }
    }


    public class WindowRuneingEditorHandlersTestSuite
    {
        public void Run()
        {
            new WindowRuneingEditorFramePointMacrosLoadingActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacroAccessActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacroDeselectionActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacroSelectionActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacroCommandAddActionHandlerTests().Run();
        }
    }
}
