using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.Systems.Configuration.Tests;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class SelectedFixture
    {
        public static WindowMapEditMenuFrameSelectedObject Object(
            string elementLabel, string frameName
        )
        {
            return new WindowMapEditMenuFrameSelectedObject
            {
                FrameObject = new Canvas
                {
                    Tag = new MapCanvasRuneFrameDataTag
                    {
                        ElementLabel = elementLabel,
                        FrameName = frameName
                    }
                }
            };
        }

        public static ListBoxItem ListBoxFixture(string text)
        {
            var listTextBox = new TextBox { Text = text };
            var listBoxGrid = new Grid { Tag = new List<TextBox> { listTextBox } };
            listBoxGrid.Children.Add(listTextBox);
            return new ListBoxItem
            {
                Content = listBoxGrid,
                Tag = new WindowRuneingEditorMovementTag()
            };
        }
    }


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
                _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
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
                _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
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
                _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
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
            _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
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
            _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
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
            _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
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
            _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
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
            _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
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
        private ListBox _accessListBox = new ListBox();

        private List<FrameworkElement> _accessElements = [];

        private AbstractWindowActionHandler _fixture()
        {
            _accessListBox = new ListBox();
            _accessElements = new List<FrameworkElement>
            {
                new Button(),
                new ToggleButton(),
                new TextBox{ Text = "lol" }
            };
            return new WindowRuneingEditorFramePointMacroAccessActionHandlerFacade(
                _accessListBox,
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
            _accessListBox.Items.Add(new ListBoxItem());
            foreach (var accessElement in _accessElements)
            {
                Debug.Assert(accessElement.IsEnabled);
            }
            _accessListBox.Items.RemoveAt(0);
            foreach (var accessElement in _accessElements)
            {
                Debug.Assert(!accessElement.IsEnabled);
            }
            _accessListBox.Items.Add(new ListBoxItem());
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
            _accessListBox.Items.Add(new ListBoxItem());
            Debug.Assert(((TextBox)_accessElements[2]).Text == "lol");
            _accessListBox.Items.RemoveAt(0);
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
            var item1 = new object();
            var item2 = new object();
            var item3 = new object();
            var item4 = new object();
            _framePointMacroCommandsListBox.Items.Add(item1);
            _framePointMacroCommandsListBox.Items.Add(item2);
            _framePointMacroCommandsListBox.Items.Add(item3);
            _framePointMacroCommandsListBox.Items.Add(item4);
            _framePointMacroCommandsListBox.SelectedIndex = 1;
            _macroCommandAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 5);
            Debug.Assert(_framePointMacroCommandsListBox.Items[0] == item1);
            Debug.Assert(_framePointMacroCommandsListBox.Items[1] == item2);
            Debug.Assert(_framePointMacroCommandsListBox.Items[2] is ComboBox);
            Debug.Assert(_framePointMacroCommandsListBox.Items[3] == item3);
            Debug.Assert(_framePointMacroCommandsListBox.Items[4] == item4);
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
            var item1 = new object();
            var item2 = new object();
            var item3 = new object();
            var item4 = new object();
            _framePointMacroCommandsListBox.Items.Add(item1);
            _framePointMacroCommandsListBox.Items.Add(item2);
            _framePointMacroCommandsListBox.Items.Add(item3);
            _framePointMacroCommandsListBox.Items.Add(item4);
            _macroCommandAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 5);
            Debug.Assert(_framePointMacroCommandsListBox.Items[0] == item1);
            Debug.Assert(_framePointMacroCommandsListBox.Items[1] == item2);
            Debug.Assert(_framePointMacroCommandsListBox.Items[2] == item3);
            Debug.Assert(_framePointMacroCommandsListBox.Items[3] == item4);
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


    public class WindowRuneingEditorFramePointMacroCommandRemoveActionHandlerTests
    {
        private Button _macroCommandRemoveButton = new Button();

        private ListBox _framePointMacroCommandsListBox = new ListBox();

        private MockWindowActionHandlerRegistry _scaleRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _macroCommandRemoveButton = new Button();
            _framePointMacroCommandsListBox = new ListBox();
            _scaleRegistry = new MockWindowActionHandlerRegistry();
            return new WindowRuneingEditorFramePointMacroCommandRemoveActionHandlerFacade(
                _macroCommandRemoveButton,
                _framePointMacroCommandsListBox,
                _scaleRegistry
            );
        }

        /**
         * @brief Verifies that removing the only command from a list box empties the list
         * 
         * When users have a macro point with exactly one command and they click the Remove
         * Command button, the list box should become completely empty. This allows users
         * to start building a new command sequence from scratch or delete the last command
         * from a macro point.
         */
        private void _testRemovingRuneFrameMacroCommandOnSingleElement()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            _framePointMacroCommandsListBox.Items.Add(new ComboBox());
            _macroCommandRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 0);
        }

        /**
         * @brief Verifies that removing a selected command removes the correct item and
         * preserves the order of remaining items
         * 
         * When users select a specific command in the middle of the command sequence and
         * click Remove Command, only the selected command should be removed. The remaining
         * commands should stay in their original relative order, with items after the
         * removed position shifting to fill the gap.
         */
        private void _testRemovingRuneFrameMacroCommandOnSelectedIndex()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item0 = new ComboBox();
            var item1 = new ComboBox();
            var item3 = new ComboBox();
            _framePointMacroCommandsListBox.Items.Add(item0);
            _framePointMacroCommandsListBox.Items.Add(item1);
            _framePointMacroCommandsListBox.Items.Add(new ComboBox());
            _framePointMacroCommandsListBox.Items.Add(item3);
            _framePointMacroCommandsListBox.SelectedIndex = 2;
            _macroCommandRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 3);
            Debug.Assert(_framePointMacroCommandsListBox.Items[0] == item0);
            Debug.Assert(_framePointMacroCommandsListBox.Items[1] == item1);
            Debug.Assert(_framePointMacroCommandsListBox.Items[2] == item3);
        }

        /**
         * @brief Verifies that removing a command with no selection removes the last command
         * 
         * When users click Remove Command without any command selected, the system should
         * remove the last command in the list. This provides a predictable behavior for
         * users who want to delete commands from the end of their sequence without having
         * to manually select them first.
         */
        private void _testRemovingRuneFrameMacroCommandOnPopulatedListBox()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item0 = new ComboBox();
            var item1 = new ComboBox();
            var item2 = new ComboBox();
            var item3 = new ComboBox();
            _framePointMacroCommandsListBox.Items.Add(item0);
            _framePointMacroCommandsListBox.Items.Add(item1);
            _framePointMacroCommandsListBox.Items.Add(item2);
            _framePointMacroCommandsListBox.Items.Add(item3);
            _macroCommandRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 3);
            Debug.Assert(_framePointMacroCommandsListBox.Items[0] == item0);
            Debug.Assert(_framePointMacroCommandsListBox.Items[1] == item1);
            Debug.Assert(_framePointMacroCommandsListBox.Items[2] == item2);
        }

        /**
         * @brief Verifies that clicking Remove Command on an empty list box does nothing
         * 
         * When users click the Remove Command button while the command list is already empty,
         * the operation should be safely ignored without causing errors or exceptions.
         */
        private void _testRemovingRuneFrameMacroCommandOnEmptyListBox()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item = new ComboBox();
            _macroCommandRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 0);
        }

        /**
         * @brief Verifies that removed command comboboxes are unregistered from the
         * scale registry to prevent memory leaks
         * 
         * When a command combobox is removed from the macro point's command list, it must
         * be unregistered from the scale registry. The scale registry holds references to
         * comboboxes for DPI scaling adjustments, and failing to unregister would keep
         * those references alive.
         */
        private void _testRemovingRuneFrameMacroCommandUnregistersFromRegistry()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item = new ComboBox();
            _framePointMacroCommandsListBox.Items.Add(item);
            _macroCommandRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 0);
            Debug.Assert(_scaleRegistry.UnregisterHandlerCalls == 1);
            Debug.Assert(
                (
                    (WindowComboBoxScaleActionHandlerParameters)
                    _scaleRegistry.UnregisterHandlerCallArg_args[0]!
                )
                .ScaleComboBox == item
            );
        }

        public void Run()
        {
            _testRemovingRuneFrameMacroCommandOnSingleElement();
            _testRemovingRuneFrameMacroCommandOnSelectedIndex();
            _testRemovingRuneFrameMacroCommandOnPopulatedListBox();
            _testRemovingRuneFrameMacroCommandOnEmptyListBox();
            _testRemovingRuneFrameMacroCommandUnregistersFromRegistry();
        }
    }


    public class WindowRuneingEditorFramePointMacroCommandClearActionHandlerTests
    {
        private Button _macroCommandClearButton = new Button();

        private ListBox _framePointMacroCommandsListBox = new ListBox();

        private MockWindowActionHandlerRegistry _scaleRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _macroCommandClearButton = new Button();
            _framePointMacroCommandsListBox = new ListBox();
            _scaleRegistry = new MockWindowActionHandlerRegistry();
            return new WindowRuneingEditorFramePointMacroCommandClearActionHandlerFacade(
                _macroCommandClearButton,
                _framePointMacroCommandsListBox,
                _scaleRegistry
            );
        }

        /**
         * @brief Verifies that clicking the clear button removes all commands from the list box
         * 
         * When users want to start over with a fresh command sequence for a macro point,
         * clicking the Clear Commands button should remove every command from the list box
         * in a single operation. This provides a convenient way to wipe the entire command
         * sequence without having to delete each command individually.
         */
        private void _testClearingRuneFrameMacroCommandsClearsListBox()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item0 = new ComboBox();
            var item1 = new ComboBox();
            var item2 = new ComboBox();
            var item3 = new ComboBox();
            _framePointMacroCommandsListBox.Items.Add(item0);
            _framePointMacroCommandsListBox.Items.Add(item1);
            _framePointMacroCommandsListBox.Items.Add(item2);
            _framePointMacroCommandsListBox.Items.Add(item3);
            _macroCommandClearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 0);
        }

        /**
         * @brief Verifies that every cleared command combobox is unregistered from the
         * scale registry to prevent memory leaks
         * 
         * When the Clear Commands button is clicked and all command comboboxes are removed
         * from the list box, each combobox must be individually unregistered from the
         * scale registry. The scale registry holds references to comboboxes for DPI scaling
         * adjustments, and failing to unregister would keep those references alive.
         */
        private void _testClearingRuneFrameMacroCommandsUnregistersFromRegistry()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item0 = new ComboBox();
            var item1 = new ComboBox();
            var item2 = new ComboBox();
            var item3 = new ComboBox();
            _framePointMacroCommandsListBox.Items.Add(item0);
            _framePointMacroCommandsListBox.Items.Add(item1);
            _framePointMacroCommandsListBox.Items.Add(item2);
            _framePointMacroCommandsListBox.Items.Add(item3);
            _macroCommandClearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_framePointMacroCommandsListBox.Items.Count == 0);
            Debug.Assert(_scaleRegistry.UnregisterHandlerCalls == 4);
            var unregisterItem0 = (
                (WindowComboBoxScaleActionHandlerParameters)
                _scaleRegistry.UnregisterHandlerCallArg_args[0]!
            );
            var unregisterItem1 = (
                (WindowComboBoxScaleActionHandlerParameters)
                _scaleRegistry.UnregisterHandlerCallArg_args[1]!
            );
            var unregisterItem2 = (
                (WindowComboBoxScaleActionHandlerParameters)
                _scaleRegistry.UnregisterHandlerCallArg_args[2]!
            );
            var unregisterItem3 = (
                (WindowComboBoxScaleActionHandlerParameters)
                _scaleRegistry.UnregisterHandlerCallArg_args[3]!
            );
            Debug.Assert(unregisterItem0.ScaleComboBox == item0);
            Debug.Assert(unregisterItem1.ScaleComboBox == item1);
            Debug.Assert(unregisterItem2.ScaleComboBox == item2);
            Debug.Assert(unregisterItem3.ScaleComboBox == item3);
        }

        public void Run()
        {
            _testClearingRuneFrameMacroCommandsClearsListBox();
            _testClearingRuneFrameMacroCommandsUnregistersFromRegistry();
        }
    }


    public class WindowRuneingEditorFrameNameLoadingActionHandlerTests
    {
        private TextBox _frameNameTextBox = new TextBox();

        private MockSystemWindow _windowRuneingEditor = new MockSystemWindow();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractWindowActionHandler _fixture()
        {
            _frameNameTextBox = new TextBox();
            _windowRuneingEditor = new MockSystemWindow();
            _editMenuState = new WindowMapEditMenuState();
            _windowRuneingEditor.GetWindowReturn.Add(new Window());
            return new WindowRuneingEditorFrameNameLoadingActionHandlerFacade(
                _frameNameTextBox,
                _windowRuneingEditor,
                _editMenuState
            );
        }

        /**
         * @brief Verifies that the frame name text box is populated with the selected
         * frame's name when the editor becomes visible
         * 
         * When users open the Runeing Editor window while a frame is selected on the map
         * canvas, the frame name text box should automatically display that frame's name.
         * This provides immediate visual confirmation of which frame is being edited and
         * allows users to identify the frame they are working on.
         */
        private void _testFrameNamePopulatedWhenEditorIsVisible()
        {
            for (int i = 0; i < 2; i++)
            {
                var framePointMacrosLoadingActionHandler = _fixture();
                _windowRuneingEditor.VisibleReturn.Add(i == 1);
                _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
                framePointMacrosLoadingActionHandler.OnDependencyEvent(
                    framePointMacrosLoadingActionHandler,
                    new DependencyPropertyChangedEventArgs()
                );
                var expectedText = (i == 1) ? "F0" : "";
                Debug.Assert(_frameNameTextBox.Text == expectedText);
            }
        }

        public void Run()
        {
            _testFrameNamePopulatedWhenEditorIsVisible();
        }
    }


    public class WindowRuneingEditorFramePointMacrosSavingActionHandlerTests
    {
        private MockSystemWindow _windowRuneingEditor = new MockSystemWindow();

        private ListBox _framePointMacrosListBox = new ListBox();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private ListBoxItem _listBoxItemFixture(
            WindowRuneingEditorFramePointTag listBoxItemTag,
            List<TextBox> listTextBox
        )
        {
            var listBoxGrid = new Grid
            {
                Tag = listTextBox
            };
            var listBoxItem = new ListBoxItem
            {
                Tag = listBoxItemTag,
                Content = listBoxGrid
            };
            return listBoxItem;
        }

        private AbstractWindowActionHandler _fixture(string nextFrame0, string nextFrame1)
        {
            _windowRuneingEditor = new MockSystemWindow();
            _windowRuneingEditor.GetWindowReturn.Add(new Window());
            _framePointMacrosListBox = new ListBox();
            _editMenuState = new WindowMapEditMenuState();
            _bottingModel = DataFixtures.BottingModelFixture();
            var handler = new WindowRuneingEditorFramePointMacrosSavingActionHandlerFacade(
                _windowRuneingEditor,
                _framePointMacrosListBox,
                _editMenuState
            );
            handler.Inject(SystemInjectType.BottingModel, _bottingModel);
            _framePointMacrosListBox.Items.Add(
                _listBoxItemFixture(
                    new WindowRuneingEditorFramePointTag
                    {
                        NextFrame = nextFrame0,
                        PointCommands = ["C1", "C2", "C3"],
                        Radius = 321,
                    },
                    [new TextBox { Text = "Meow 1" }]
                )
            );
            _framePointMacrosListBox.Items.Add(
                _listBoxItemFixture(
                    new WindowRuneingEditorFramePointTag
                    {
                        NextFrame = nextFrame1,
                        PointCommands = ["C2", "C3", "C4"],
                        Radius = 432
                    },
                    [new TextBox { Text = "Meow 2" }]
                )
            );
            return handler;
        }

        /**
         * @brief Verifies that point command sequences are saved to the botting model
         * when the editor closes
         * 
         * When users edit the command sequences of frame point macros in the editor,
         * these changes must be persisted to the rune frame's macro list when the
         * editor window closes. The command list defines the automation actions that
         * execute when the bot reaches this point.
         */
        private void _testClosingEditorSavesPointCommands()
        {
            for (int i = 0; i < 2; i++)
            {
                var nextFrame = "F" + ((i + 1) % 2).ToString();
                var framePointMacrosSavingActionHandler = _fixture(nextFrame, nextFrame);
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(false);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].PointCommands.Count == 3);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].PointCommands[0] == "C1");
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].PointCommands[1] == "C2");
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].PointCommands[2] == "C3");
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].PointCommands.Count == 3);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].PointCommands[0] == "C2");
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].PointCommands[1] == "C3");
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].PointCommands[2] == "C4");
            }
        }

        /**
         * @brief Verifies that point radius values are saved to the botting model
         * when the editor closes
         * 
         * When users modify the activation radius of a frame point macro, this value
         * must be saved to the rune frame's macro list when the editor closes. The
         * radius determines how close the character must get to the point before the
         * macro commands are triggered.
         */
        private void _testClosingEditorSavesRadius()
        {
            for (int i = 0; i < 2; i++)
            {
                var nextFrame = "F" + ((i + 1) % 2).ToString();
                var framePointMacrosSavingActionHandler = _fixture(nextFrame, nextFrame);
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(false);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].Radius == 321);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].Radius == 432);
            }
        }

        /**
         * @brief Verifies that macro names are saved to the botting model and that
         * text dependencies are properly updated when the editor closes
         * 
         * When users edit the name of a frame point macro in the editor's text box,
         * the macro name must be saved to the rune frame's macro list. Additionally,
         * any text dependencies (TextBlock and TextBox elements bound to this macro)
         * must be updated to reflect the new name, ensuring visual and data model
         * synchronization.
         */
        private void _testClosingEditorSavesMacroName()
        {
            for (int i = 0; i < 2; i++)
            {
                var nextFrame = "F" + ((i + 1) % 2).ToString();
                var framePointMacrosSavingActionHandler = _fixture(nextFrame, nextFrame);
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(false);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].TextDependencies.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].MacroName == "Meow 1");
                Debug.Assert(((TextBlock)runeFrame.FrameData.RuneFrameMacros[0].TextDependencies[0]).Text == "Meow 1");
                Debug.Assert(((TextBox)runeFrame.FrameData.RuneFrameMacros[0].TextDependencies[1]).Text == "Meow 1");
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].TextDependencies.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].MacroName == "Meow 2");
                Debug.Assert(((TextBlock)runeFrame.FrameData.RuneFrameMacros[1].TextDependencies[0]).Text == "Meow 2");
                Debug.Assert(((TextBox)runeFrame.FrameData.RuneFrameMacros[1].TextDependencies[1]).Text == "Meow 2");
            }
        }

        /**
         * @brief Verifies that next frame references are saved to the botting model
         * when the editor closes
         * 
         * When users set a "Next Frame" reference for a frame point macro (indicating
         * which frame the bot should navigate to after completing this point's commands),
         * this reference must be saved as a proper RuneFrame object reference. The
         * reference should point to the correct target frame by its element label.
         */
        private void _testClosingEditorSavesNextRuneFrame()
        {
            for (int i = 0; i < 2; i++)
            {
                var nextFrame = "F" + ((i + 1) % 2).ToString();
                var framePointMacrosSavingActionHandler = _fixture(nextFrame, nextFrame);
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(false);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                var nextRuneFrame = runeModel.FindRuneFrameByName("FT" + ((i + 1) % 2).ToString())!;
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros.Count == 2);
                var nextRuneFrame0 = runeFrame.FrameData.RuneFrameMacros[0].NextRuneFrame!;
                var nextRuneFrame1 = runeFrame.FrameData.RuneFrameMacros[1].NextRuneFrame!;
                Debug.Assert(nextRuneFrame.FrameData.FrameName == "F" + ((i + 1) % 2).ToString());
                Debug.Assert(nextRuneFrame.FrameData.ElementLabel == "FT" + ((i + 1) % 2).ToString());
                Debug.Assert(nextRuneFrame0.FrameData.FrameName == nextRuneFrame.FrameData.FrameName);
                Debug.Assert(nextRuneFrame1.FrameData.FrameName == nextRuneFrame.FrameData.FrameName);
                Debug.Assert(nextRuneFrame0.FrameData.ElementLabel == nextRuneFrame.FrameData.ElementLabel);
                Debug.Assert(nextRuneFrame1.FrameData.ElementLabel == nextRuneFrame.FrameData.ElementLabel);
            }
        }

        /**
         * @brief Verifies that a macro cannot reference itself as its own next frame
         * 
         * When users attempt to set a frame point macro's "Next Frame" to the same frame
         * that contains the macro (creating a self-reference loop), the system should
         * prevent this and set the NextRuneFrame to null instead.
         */
        private void _testClosingEditorDoesntSaveSameRuneFrame()
        {
            for (int i = 0; i < 2; i++)
            {
                var nextFrame = "F" + i.ToString();
                var framePointMacrosSavingActionHandler = _fixture(nextFrame, nextFrame);
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(false);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].NextRuneFrame == null);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].NextRuneFrame == null);
            }
        }

        /**
         * @brief Verifies that invalid next frame references are not saved
         * 
         * When users enter a next frame value that does not correspond to any existing
         * frame in the botting model (e.g., a typo or a frame that was deleted), the
         * system should ignore this invalid reference and set NextRuneFrame to null.
         * This prevents broken references that would cause navigation errors during
         * automation.
         */
        private void _testClosingEditorDoesntSaveInvalidRuneFrame()
        {
            for (int i = 0; i < 2; i++)
            {
                var framePointMacrosSavingActionHandler = _fixture("meow1", "meow2");
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(false);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].NextRuneFrame == null);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].NextRuneFrame == null);
            }
        }

        /**
         * @brief Verifies that opening the editor does NOT save frame point macro data
         * 
         * When users open the Runeing Editor window (becomes visible), the system should
         * load the existing data from the botting model into the editor UI, but should
         * NOT save or overwrite any data in the botting model. This test ensures that the
         * visibility event handler distinguishes between opening (loading data) and closing
         * (saving data) operations.
         */
        private void _testOpeningEditorDoesntSaveFramePointMacros()
        {
            for (int i = 0; i < 2; i++)
            {
                var nextFrame = "F" + ((i + 1) % 2).ToString();
                var framePointMacrosSavingActionHandler = _fixture(nextFrame, nextFrame);
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(true);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                var nextRuneFrame = runeModel.FindRuneFrameRefByLabel("F" + ((i + 1) % 2).ToString())!;
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].MacroName == "M" + ((2 * i) + 0).ToString());
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].MacroName == "M" + ((2 * i) + 1).ToString());
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].NextRuneFrame == ((i == 0) ? nextRuneFrame : null));
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].NextRuneFrame == ((i == 1) ? nextRuneFrame : null));
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].Radius == ((i == 0) ? 67 : 89));
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].Radius == ((i == 0) ? 78 : 90));
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].PointCommands.Count == 3);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].PointCommands[0] == ((i == 0) ? "C345" : "C456"));
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].PointCommands[1] == ((i == 0) ? "C456" : "C567"));
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[0].PointCommands[2] == ((i == 0) ? "C567" : "C678"));
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].PointCommands.Count == 3);
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].PointCommands[0] == ((i == 0) ? "C456" : "C567"));
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].PointCommands[1] == ((i == 0) ? "C567" : "C678"));
                Debug.Assert(runeFrame.FrameData.RuneFrameMacros[1].PointCommands[2] == ((i == 0) ? "C678" : "C789"));
            }
        }

        public void Run()
        {
            _testClosingEditorSavesPointCommands();
            _testClosingEditorSavesRadius();
            _testClosingEditorSavesMacroName();
            _testClosingEditorSavesNextRuneFrame();
            _testClosingEditorDoesntSaveSameRuneFrame();
            _testClosingEditorDoesntSaveInvalidRuneFrame();
            _testOpeningEditorDoesntSaveFramePointMacros();
        }
    }


    public class WindowRuneingEditorFramePointLoadConfigurationActionHandlerTests
    {
        private Button _loadButton = new Button();

        private MockLoadFileDialog _loadFileDialog = new MockLoadFileDialog();

        private MaplestoryBotConfiguration _configuration = new MaplestoryBotConfiguration();

        private AbstractWindowActionHandler _fixture()
        {
            _loadButton = new Button();
            _loadFileDialog = new MockLoadFileDialog();
            _configuration = new MaplestoryBotConfiguration
            {
                FramePointsDirectory = "cool_frame_points"
            };
            return new WindowRuneingEditorFramePointLoadConfigurationActionHandlerFacade(
                _loadButton,
                _loadFileDialog
            );
        }

        /**
         * @brief Verifies that clicking the load button opens a file dialog with the
         * correct initial directory from the configuration
         * 
         * When users click the Load button in the Runeing Editor, the system should prompt
         * the user to select a frame point configuration file to load. The file dialog
         * should start in the directory specified by the FramePointsDirectory configuration
         * value, making it easy for users to find their saved configuration files without
         * navigating through the file system each time.
         */
        private void _testLoadButtonClickOpensLoadMenu()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.ConfigurationUpdate, _configuration);
            _loadButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_loadFileDialog.PromptCalls == 1);
            Debug.Assert(_loadFileDialog.PromptCallArg_initialDirectory[0] == "cool_frame_points");
        }

        public void Run()
        {
            _testLoadButtonClickOpensLoadMenu();
        }
    }


    public class WindowRuneingEditorFramePointLoadActionHandlerTests
    {
        private ListBox _listBox = new ListBox();

        private ComboBox _comboBox = new ComboBox();

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry = new MockWindowActionHandlerRegistry();

        private List<string> _expectedContents = [];

        private MockLoadFileDialog _loadFileDialog = new MockLoadFileDialog();

        private AbstractWindowActionHandler _fixture(
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _listBox = new ListBox();
            _comboBox = new ComboBox();
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            _comboBox.Items.Add(new ComboBoxItem { Content = "A" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "B" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "C" });
            _expectedContents = ["a", "b", "c", "d", "e"];
            return new WindowRuneingEditorFramePointLoadActionHandlerFacade(
                _loadFileDialog,
                _listBox,
                _comboBox,
                _comboBoxPopupScaleRegistry
            );
        }

        /**
         * @brief Tests complete load menu workflow from user perspective
         * 
         * Validates that users can load saved macros and have them
         * available with all commands properly displayed, ensuring
         * a seamless transition from storage to active use.
         */
        private void _testLoadButtonClickOpensLoadFileDialog()
        {
            var registry = new WindowComboBoxScaleActionHandlerRegistry();
            var handler = _fixture(registry);
            _loadFileDialog.InvokeFileLoaded(
                "some_path",
                "{\"macro\": [\"a\", \"b\", \"c\", \"d\", \"e\"]}"
            );
            Debug.Assert(_listBox.Items.Count == 5);
            for (int i = 0; i < _expectedContents.Count; i++)
            {
                Debug.Assert(((ComboBox)_listBox.Items[i]).Text == _expectedContents[i]);
                for (int j = 0; j < _comboBox.Items.Count; j++)
                {
                    Debug.Assert(
                        ((ComboBoxItem)((ComboBox)_listBox.Items[i]).Items[j]).Content.ToString()
                        == ((ComboBoxItem)_comboBox.Items[j]).Content.ToString()
                    );
                }
            }
        }

        /**
         * @brief Tests that ComboBox scaling handlers are properly registered for loaded macros
         * 
         * Validates that when macros are loaded into the UI, each ComboBox instance
         * automatically registers with the scaling system to ensure proper DPI
         * handling, maintaining consistent visual appearance across different displays.
         */
        private void _testLoadButtonClickRegistersComboBoxPopupScalers()
        {
            var mockRegistry = new MockWindowActionHandlerRegistry();
            var handler = _fixture(mockRegistry);
            _loadFileDialog.InvokeFileLoaded(
                "some_path",
                "{\"macro\": [\"a\", \"b\", \"c\", \"d\", \"e\"]}"
            );
            Debug.Assert(mockRegistry.RegisterHandlerCalls == 5);
            for (int i = 0; i < _expectedContents.Count; i++)
            {
                var parameters = (
                    (WindowComboBoxScaleActionHandlerParameters)
                    mockRegistry.RegisterHandlerCallArg_args[i]!
                );
                Debug.Assert(parameters.ScaleComboBox == (ComboBox)_listBox.Items[i]);
            }
        }


        /**
         * @brief Tests proper cleanup and registration order for scaling handlers
         * 
         * Ensures that when loading new macros, existing scaling handlers are
         * cleared before registering new ones, preventing memory leaks and
         * ensuring only current macro ComboBoxes receive scaling adjustments.
         */
        private void _testLoadButtonClickClearsComboBoxPopupScalersBefroreRegisteringNew()
        {
            var mockRegistry = new MockWindowActionHandlerRegistry();
            var handler = _fixture(mockRegistry);
            _loadFileDialog.InvokeFileLoaded(
                "some_path",
                "{\"macro\": [\"a\", \"b\", \"c\", \"d\", \"e\"]}"
            );
            var reference = new TestUtilities().Reference(mockRegistry);
            var clearCallRef = reference + "ClearHandlers";
            var registerCallRef = reference + "RegisterHandler";
            Debug.Assert(mockRegistry.CallOrder.Count == 6);
            Debug.Assert(mockRegistry.CallOrder[0] == clearCallRef);
            Debug.Assert(mockRegistry.CallOrder[1] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[2] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[3] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[4] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[5] == registerCallRef);
        }

        public void Run()
        {
            _testLoadButtonClickOpensLoadFileDialog();
            _testLoadButtonClickRegistersComboBoxPopupScalers();
            _testLoadButtonClickClearsComboBoxPopupScalersBefroreRegisteringNew();
        }
    }


    public class WindowRuneingEditorFramePointSaveActionHandlerTests
    {
        private Button _saveButton = new Button();

        private ListBox _listBox = new ListBox();

        private MockSaveFileDialog _saveFileDialog = new MockSaveFileDialog();

        private AbstractWindowActionHandler _fixture()
        {
            _saveButton = new Button();
            _listBox = new ListBox();
            _listBox.Items.Add(new ComboBox { Text = "A" });
            _listBox.Items.Add(new ComboBox { Text = "B" });
            _listBox.Items.Add(new ComboBox { Text = "C" });
            _saveFileDialog = new MockSaveFileDialog();
            return new WindowRuneingEditorFramePointSaveActionHandlerFacade(
                _saveButton, _listBox, _saveFileDialog
            );
        }

        /**
         * @brief Tests complete save menu workflow from button click to file dialog
         * 
         * @test Validates the entire macro saving process
         * 
         * Verifies that clicking the save button triggers the file dialog with
         * correct macro data serialization and proper directory configuration.
         * Ensures users can save their macro sequences with the expected format
         * and in the configured save location.
         */
        private void _testSaveButtonClickOpensSaveFileDialog()
        {
            var handler = _fixture();
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                new MaplestoryBotConfiguration { FramePointsDirectory = "MEOW" }
            );
            _saveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_saveFileDialog.PromptCalls == 1);
            var normalizer = new JsonNormalizer();
            var saveContent = normalizer.Normalize(_saveFileDialog.PromptCallArg_saveContent[0]);
            var initialDirectory = _saveFileDialog.PromptCallArg_initialDirectory[0];
            Debug.Assert(initialDirectory == "MEOW");
            Debug.Assert(saveContent == normalizer.Normalize("{\"macro\":[\"A\",\"B\",\"C\"]}"));
        }

        public void Run()
        {
            _testSaveButtonClickOpensSaveFileDialog();
        }
    }


    public class WindowRuneingEditorMovementAddActionHandlerTests
    {
        private Button _movementsAddButton = new Button();

        private ListBox _movementsListBox = new ListBox();

        private Grid _listBoxGrid = new Grid();

        private TextBox _listTextBox = new TextBox();

        private AbstractWindowActionHandler _fixture()
        {
            _movementsAddButton = new Button();
            _movementsListBox = new ListBox();
            _listBoxGrid = new Grid();
            _listTextBox = new TextBox();
            _listBoxGrid.Children.Add(_listTextBox);
            return new WindowRuneingEditorMovementAddActionHandlerFacade(
                _movementsAddButton,
                _movementsListBox,
                _listBoxGrid
            );
        }
        
        /**
         * @brief Verifies that clicking the add button repeatedly creates multiple movement
         * entries with sequentially numbered names
         * 
         * When users click the Add Movement button multiple times, each click should create
         * a new movement entry in the list box. Each new movement should receive a unique
         * auto-generated name following the pattern "Move 0", "Move 1", "Move 2", etc.
         * Every list box item must contain a properly configured Grid with a TextBox that
         * displays the movement name and a WindowRuneingEditorMovementTag for identification.
         */
        private void _testClickingAddButtonAddsListBoxItem()
        {
            for (int i = 1; i < 10; i++)
            {
                var movementAddActionHandler = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _movementsAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                Debug.Assert(_movementsListBox.Items.Count == i);
                for (int j = 0; j < _movementsListBox.Items.Count; j++)
                {
                    var listBoxItem = (ListBoxItem)_movementsListBox.Items[j];
                    Debug.Assert(listBoxItem.Tag is WindowRuneingEditorMovementTag);
                    Debug.Assert(listBoxItem.Content is Grid);
                    var listBoxGrid = (Grid)listBoxItem.Content;
                    Debug.Assert(listBoxGrid.Tag is List<TextBox>);
                    var listTextBoxes = (List<TextBox>)listBoxGrid.Tag;
                    var listBoxGridTextBoxes = listBoxGrid.Children.OfType<TextBox>().ToList();
                    Debug.Assert(listTextBoxes.Count == 1);
                    Debug.Assert(listBoxGridTextBoxes.Count == 1);
                    Debug.Assert(listTextBoxes[0].Text == "Move " + j.ToString());
                    Debug.Assert(listTextBoxes[0] == listBoxGridTextBoxes[0]);
                }
            }
        }

        /**
         * @brief Verifies that a new movement is inserted below the currently selected item
         * 
         * When users have existing movements in the list and select a specific movement,
         * clicking the Add Movement button should insert the new movement directly below
         * the selected one.
         */
        private void _testClickingAddButtonAddsListBoxItemBelowSelected()
        {
            var movementAddActionHandler = _fixture();
            var item0 = SelectedFixture.ListBoxFixture("Move 0");
            var item1 = SelectedFixture.ListBoxFixture("Move 1");
            var item2 = SelectedFixture.ListBoxFixture("Move 2");
            var item3 = SelectedFixture.ListBoxFixture("Move 3");
            _movementsListBox.Items.Add(item0);
            _movementsListBox.Items.Add(item1);
            _movementsListBox.Items.Add(item2);
            _movementsListBox.Items.Add(item3);
            _movementsListBox.SelectedIndex = 1;
            _movementsAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementsListBox.Items.Count == 5);
            Debug.Assert(_movementsListBox.Items[0] == item0);
            Debug.Assert(_movementsListBox.Items[1] == item1);
            Debug.Assert(_movementsListBox.Items[3] == item2);
            Debug.Assert(_movementsListBox.Items[4] == item3);
            var listBoxItem = (ListBoxItem)_movementsListBox.Items[2];
            Debug.Assert(listBoxItem.Tag is WindowRuneingEditorMovementTag);
            Debug.Assert(listBoxItem.Content is Grid);
            var listBoxGrid = (Grid)listBoxItem.Content;
            Debug.Assert(listBoxGrid.Tag is List<TextBox>);
            var listTextBoxes = (List<TextBox>)listBoxGrid.Tag;
            var listBoxGridTextBoxes = listBoxGrid.Children.OfType<TextBox>().ToList();
            Debug.Assert(listTextBoxes.Count == 1);
            Debug.Assert(listBoxGridTextBoxes.Count == 1);
            Debug.Assert(listTextBoxes[0].Text == "Move 4");
            Debug.Assert(listTextBoxes[0] == listBoxGridTextBoxes[0]);
        }

        /**
         * @brief Verifies that after adding a new movement, the newly added item becomes
         * the selected item in the list box
         * 
         * When users click the Add Movement button, the system should automatically select
         * the newly created movement entry. This provides immediate visual feedback that
         * the addition was successful and allows users to start editing the new movement.
         */
        private void _testClickingAddButtonSelectsAddedListBoxItem()
        {
            var movementAddActionHandler = _fixture();
            for (int j = 0; j < 10; j++)
            {
                _movementsAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Debug.Assert(_movementsListBox.SelectedIndex == j);
            }
            _movementsListBox.SelectedIndex = 5;
            _movementsAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementsListBox.SelectedIndex == 6);
        }

        public void Run()
        {
            _testClickingAddButtonAddsListBoxItem();
            _testClickingAddButtonAddsListBoxItemBelowSelected();
            _testClickingAddButtonSelectsAddedListBoxItem();
        }
    }


    public class WindowRuneingEditorMovementRemoveActionHandlerTests
    {
        private Button _movementsRemoveButton = new Button();

        private ListBox _movementsListBox = new ListBox();

        private AbstractWindowActionHandler _fixture()
        {
            _movementsRemoveButton = new Button();
            _movementsListBox = new ListBox();
            return new WindowRuneingEditorMovementRemoveActionHandlerFacade(
                _movementsRemoveButton,
                _movementsListBox
            );
        }

        /**
         * @brief Verifies that clicking the remove button with no selection removes the
         * last item from the list box
         * 
         * When users click the Remove Movement button without any specific movement
         * selected, the system should remove the last movement in the list. This provides
         * a predictable behavior for users who want to delete the most recently added
         * movement or remove items from the end of their movement sequence.
         */
        private void _testClickingRemoveButtonRemovesListBoxItem()
        {
            var movementAddActionHandler = _fixture();
            var item0 = SelectedFixture.ListBoxFixture("Move 0");
            var item1 = SelectedFixture.ListBoxFixture("Move 1");
            var item2 = SelectedFixture.ListBoxFixture("Move 2");
            var item3 = SelectedFixture.ListBoxFixture("Move 3");
            _movementsListBox.Items.Add(item0);
            _movementsListBox.Items.Add(item1);
            _movementsListBox.Items.Add(item2);
            _movementsListBox.Items.Add(item3);
            _movementsRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementsListBox.Items.Count == 3);
            Debug.Assert(_movementsListBox.Items[0] == item0);
            Debug.Assert(_movementsListBox.Items[1] == item1);
            Debug.Assert(_movementsListBox.Items[2] == item2);
        }

        /**
         * @brief Verifies that clicking the remove button with a selected item removes
         * only that specific movement from the list
         * 
         * When users select a specific movement in the list and click the Remove Movement
         * button, only the selected movement should be removed. The remaining movements
         * should stay in their original relative order, with items after the removed
         * position shifting to fill the gap.
         */
        private void _testClickingRemoveButtonRemovesSelectedListBoxItem()
        {
            var movementAddActionHandler = _fixture();
            var item0 = SelectedFixture.ListBoxFixture("Move 0");
            var item1 = SelectedFixture.ListBoxFixture("Move 1");
            var item2 = SelectedFixture.ListBoxFixture("Move 2");
            var item3 = SelectedFixture.ListBoxFixture("Move 3");
            _movementsListBox.Items.Add(item0);
            _movementsListBox.Items.Add(item1);
            _movementsListBox.Items.Add(item2);
            _movementsListBox.Items.Add(item3);
            _movementsListBox.SelectedIndex = 2;
            _movementsRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementsListBox.Items.Count == 3);
            Debug.Assert(_movementsListBox.Items[0] == item0);
            Debug.Assert(_movementsListBox.Items[1] == item1);
            Debug.Assert(_movementsListBox.Items[2] == item3);
        }

        /**
         * @brief Verifies that clicking the remove button on an empty list box does nothing
         * 
         * When users click the Remove Movement button while the movement list is already
         * empty, the operation should be safely ignored without causing errors or
         * exceptions. This prevents crashes and provides a smooth user experience.
         */
        private void _testClickingRemoveButtonOnEmptyListBox()
        {
            var movementAddActionHandler = _fixture();
            _movementsRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementsListBox.Items.Count == 0);
        }

        public void Run()
        {
            _testClickingRemoveButtonRemovesListBoxItem();
            _testClickingRemoveButtonRemovesSelectedListBoxItem();
            _testClickingRemoveButtonOnEmptyListBox();
        }
    }


    public class WindowRuneingEditorMovementMacroAccessActionHandlerTests
    {
        private ListBox _movementsListBox = new ListBox();

        private List<FrameworkElement> _accessElements = [];

        private AbstractWindowActionHandler _fixture()
        {
            _accessElements = [
                new TextBox(),
                new ComboBox(),
                new Button(),
                new ToggleButton()
            ];
            _movementsListBox = new ListBox();
            return new WindowRuneingEditorMovementMacroAccessActionHandlerFacade(
                _movementsListBox, _accessElements
            );
        }

        /**
         * @brief Verifies that access elements are enabled when a movement is selected
         * and disabled when no movement is selected
         * 
         * When users click on a movement in the movement list box, all action elements
         * (such as edit fields, command buttons, and toggle options) should become
         * enabled, allowing users to view and modify the selected movement's properties.
         * When the selection is cleared (no movement selected), all access elements
         * should become disabled to prevent users from attempting operations on a
         * non-existent selection.
         */
        private void _testSelectingListBoxItemChangesElementAccess()
        {
            var movementMacroAccessActionHandler = _fixture();
            _movementsListBox.Items.Add(new object());
            _movementsListBox.Items.Add(new object());
            _movementsListBox.Items.Add(new object());
            foreach (var accessElement in _accessElements)
            {
                accessElement.IsEnabled = false;
            }
            _movementsListBox.SelectedIndex = 1;
            foreach (var accessElement in _accessElements)
            {
                Debug.Assert(accessElement.IsEnabled);
            }
            _movementsListBox.SelectedIndex = -1;
            foreach (var accessElement in _accessElements)
            {
                Debug.Assert(!accessElement.IsEnabled);
            }
        }

        /**
         * @brief Verifies that text-based access elements are cleared when no movement
         * is selected
         * 
         * When users deselect a movement, any text boxes and combo boxes that displayed
         * the previously selected movement's data should be cleared. This prevents the
         * editor from showing stale information from a movement that is no longer
         * selected.
         */
        private void _testDeselectingListBoxItemClearsElementTexts()
        {
            var movementMacroAccessActionHandler = _fixture();
            _movementsListBox.Items.Add(new object());
            _movementsListBox.Items.Add(new object());
            _movementsListBox.Items.Add(new object());
            _movementsListBox.SelectedIndex = 1;
            ((TextBox)_accessElements[0]).Text = "lol1";
            ((ComboBox)_accessElements[1]).Text = "lol2";
            _movementsListBox.SelectedIndex = -1;
            Debug.Assert(((TextBox)_accessElements[0]).Text == "");
            Debug.Assert(((ComboBox)_accessElements[1]).Text == "");
        }

        public void Run()
        {
            _testSelectingListBoxItemChangesElementAccess();
            _testDeselectingListBoxItemClearsElementTexts();
        }
    }


    public class WindowRuneingEditorMovementsCommandAddActionHandlerTests
    {
        private Button _movementAddButton = new Button();

        private ListBox _movementCommandsListBox = new ListBox();

        private ComboBox _comboBoxTemplate = new ComboBox();

        private MockWindowActionHandlerRegistry _scaleRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _movementAddButton = new Button();
            _movementCommandsListBox = new ListBox();
            _comboBoxTemplate = new ComboBox();
            _scaleRegistry = new MockWindowActionHandlerRegistry();
            return new WindowRuneingEditorMovementsCommandAddActionHandlerFacade(
                _movementAddButton,
                _movementCommandsListBox,
                _comboBoxTemplate,
                _scaleRegistry
            );
        }


        /**
         * @brief Verifies that adding a command to an empty command list inserts at index 0
         * 
         * When users select a movement that has no existing commands and click the Add
         * Command button, the first command combobox should be inserted at the beginning
         * of the list.
         */
        private void _testAddingMovementCommandOnEmptyListBox()
        {
            var framePointMacroCommandAddActionHandler = _fixture();
            _movementAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 1);
            Debug.Assert(_movementCommandsListBox.Items[0] is ComboBox);
        }

        /**
         * @brief Verifies that adding a command inserts below the currently selected command
         * 
         * When users have selected a movement with an existing command sequence and want
         * to insert a new command between two existing commands, they can select the
         * command that should appear above the new one and click Add Command. The new
         * command should appear immediately below the selected command.
         */
        private void _testAddingMovementCommandOnSelectedIndex()
        {
            var framePointMacroCommandAddActionHandler = _fixture();
            var item1 = new object();
            var item2 = new object();
            var item3 = new object();
            var item4 = new object();
            _movementCommandsListBox.Items.Add(item1);
            _movementCommandsListBox.Items.Add(item2);
            _movementCommandsListBox.Items.Add(item3);
            _movementCommandsListBox.Items.Add(item4);
            _movementCommandsListBox.SelectedIndex = 1;
            _movementAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 5);
            Debug.Assert(_movementCommandsListBox.Items[0] == item1);
            Debug.Assert(_movementCommandsListBox.Items[1] == item2);
            Debug.Assert(_movementCommandsListBox.Items[2] is ComboBox);
            Debug.Assert(_movementCommandsListBox.Items[3] == item3);
            Debug.Assert(_movementCommandsListBox.Items[4] == item4);
        }

        /**
         * @brief Verifies that adding a command with no command selected appends at the end
         * 
         * When users have selected a movement and want to add a command to the end of its
         * command sequence without caring about the current selection, clicking Add Command
         * while no command is selected should append the new command at the end of the list.
         */
        private void _testAddingMovementCommandOnPopulatedListBox()
        {
            var framePointMacroCommandAddActionHandler = _fixture();
            var item1 = new object();
            var item2 = new object();
            var item3 = new object();
            var item4 = new object();
            _movementCommandsListBox.Items.Add(item1);
            _movementCommandsListBox.Items.Add(item2);
            _movementCommandsListBox.Items.Add(item3);
            _movementCommandsListBox.Items.Add(item4);
            _movementAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 5);
            Debug.Assert(_movementCommandsListBox.Items[0] == item1);
            Debug.Assert(_movementCommandsListBox.Items[1] == item2);
            Debug.Assert(_movementCommandsListBox.Items[2] == item3);
            Debug.Assert(_movementCommandsListBox.Items[3] == item4);
            Debug.Assert(_movementCommandsListBox.Items[4] is ComboBox);
        }

        /**
         * @brief Verifies that newly added command comboboxes are registered with the
         * scale registry for proper DPI scaling
         * 
         * When a new command combobox is added to a selected movement's command list,
         * it must be registered with the scale registry. The scale registry handles
         * dropdown sizing adjustments based on the current monitor's DPI settings,
         * ensuring combobox dropdowns display correctly on high-DPI displays without
         * being cut off or appearing too small.
         */
        private void _testAddingMovementComboBoxToScaleRegistry()
        {
            var framePointMacroCommandAddActionHandler = _fixture();
            _movementAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 1);
            Debug.Assert(_movementCommandsListBox.Items[0] is ComboBox);
            Debug.Assert(_scaleRegistry.RegisterHandlerCalls == 1);
            var comboBoxItem = _movementCommandsListBox.Items[0];
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
            _testAddingMovementCommandOnEmptyListBox();
            _testAddingMovementCommandOnSelectedIndex();
            _testAddingMovementCommandOnPopulatedListBox();
            _testAddingMovementComboBoxToScaleRegistry();
        }
    }


    public class WindowRuneingEditorMovementsCommandRemoveActionHandlerTests
    {
        private Button _movementRemoveButton = new Button();

        private ListBox _movementCommandsListBox = new ListBox();

        private MockWindowActionHandlerRegistry _scaleRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _movementRemoveButton = new Button();
            _movementCommandsListBox = new ListBox();
            _scaleRegistry = new MockWindowActionHandlerRegistry();
            return new WindowRuneingEditorMovementsCommandRemoveActionHandlerFacade(
                _movementRemoveButton,
                _movementCommandsListBox,
                _scaleRegistry
            );
        }

        /**
         * @brief Verifies that removing the only command from a movement's command list
         * empties the list
         * 
         * When users select a movement that has exactly one command and click the Remove
         * Command button, the command list should become completely empty. This allows
         * users to delete the last remaining command.
         */
        private void _testRemovingMovementCommandOnSingleElement()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            _movementCommandsListBox.Items.Add(new ComboBox());
            _movementRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 0);
        }

        /**
         * @brief Verifies that removing a selected command removes the correct item and
         * preserves the order of remaining commands for the selected movement
         * 
         * When users select a specific command in a movement's command sequence and click
         * Remove Command, only the selected command should be removed. The remaining
         * commands should stay in their original relative order, with commands after the
         * removed position shifting left to fill the gap.
         */
        private void _testRemovingMovementCommandOnSelectedIndex()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item0 = new ComboBox();
            var item1 = new ComboBox();
            var item3 = new ComboBox();
            _movementCommandsListBox.Items.Add(item0);
            _movementCommandsListBox.Items.Add(item1);
            _movementCommandsListBox.Items.Add(new ComboBox());
            _movementCommandsListBox.Items.Add(item3);
            _movementCommandsListBox.SelectedIndex = 2;
            _movementRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 3);
            Debug.Assert(_movementCommandsListBox.Items[0] == item0);
            Debug.Assert(_movementCommandsListBox.Items[1] == item1);
            Debug.Assert(_movementCommandsListBox.Items[2] == item3);
        }

        /**
         * @brief Verifies that removing a command with no selection removes the last command
         * from the selected movement's command list
         * 
         * When users click Remove Command without any command selected, the system should
         * remove the last command in the list. This provides a predictable behavior for
         * users who want to delete commands from the end of their sequence without having
         * to manually select them first.
         */
        private void _testRemovingMovementCommandOnPopulatedListBox()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item0 = new ComboBox();
            var item1 = new ComboBox();
            var item2 = new ComboBox();
            var item3 = new ComboBox();
            _movementCommandsListBox.Items.Add(item0);
            _movementCommandsListBox.Items.Add(item1);
            _movementCommandsListBox.Items.Add(item2);
            _movementCommandsListBox.Items.Add(item3);
            _movementRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 3);
            Debug.Assert(_movementCommandsListBox.Items[0] == item0);
            Debug.Assert(_movementCommandsListBox.Items[1] == item1);
            Debug.Assert(_movementCommandsListBox.Items[2] == item2);
        }

        /**
         * @brief Verifies that clicking Remove Command on an empty command list does nothing
         * 
         * When users click the Remove Command button while the selected movement's command
         * list is already empty, the operation should be safely ignored without causing
         * errors or exceptions. This prevents crashes and provides a smooth user experience
         * even when buttons are clicked in unexpected states.
         */
        private void _testRemovingMovementCommandOnEmptyListBox()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            _movementRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 0);
        }

        /**
         * @brief Verifies that removed command comboboxes are unregistered from the
         * scale registry to prevent memory leaks
         * 
         * When a command combobox is removed from a selected movement's command list,
         * it must be unregistered from the scale registry. The scale registry holds
         * references to comboboxes for DPI scaling adjustments, and failing to unregister
         * would keep those references alive.
         */
        private void _testRemovingMovementCommandUnregistersFromRegistry()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item = new ComboBox();
            _movementCommandsListBox.Items.Add(item);
            _movementRemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 0);
            Debug.Assert(_scaleRegistry.UnregisterHandlerCalls == 1);
            Debug.Assert(
                (
                    (WindowComboBoxScaleActionHandlerParameters)
                    _scaleRegistry.UnregisterHandlerCallArg_args[0]!
                )
                .ScaleComboBox == item
            );
        }

        public void Run()
        {
            _testRemovingMovementCommandOnSingleElement();
            _testRemovingMovementCommandOnSelectedIndex();
            _testRemovingMovementCommandOnPopulatedListBox();
            _testRemovingMovementCommandOnEmptyListBox();
            _testRemovingMovementCommandUnregistersFromRegistry();
        }
    }


    public class WindowRuneingEditorMovementsCommandClearActionHandlerTests
    {
        private Button _movementsClearButton = new Button();

        private ListBox _movementCommandsListBox = new ListBox();

        private MockWindowActionHandlerRegistry _scaleRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _movementsClearButton = new Button();
            _movementCommandsListBox = new ListBox();
            _scaleRegistry = new MockWindowActionHandlerRegistry();
            return new WindowRuneingEditorMovementsCommandClearActionHandlerFacade(
                _movementsClearButton,
                _movementCommandsListBox,
                _scaleRegistry
            );
        }

        /**
         * @brief Verifies that clicking the clear button removes all commands from the
         * selected movement's command list
         * 
         * When users select a movement and want to start over with a fresh command sequence,
         * clicking the Clear Commands button should remove every command from the command
         * list in a single operation. This provides a convenient way to wipe the entire
         * command sequence for the selected movement without having to delete each command
         * individually.
         */
        private void _testClearingMovementCommandsClearsListBox()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item0 = new ComboBox();
            var item1 = new ComboBox();
            var item2 = new ComboBox();
            var item3 = new ComboBox();
            _movementCommandsListBox.Items.Add(item0);
            _movementCommandsListBox.Items.Add(item1);
            _movementCommandsListBox.Items.Add(item2);
            _movementCommandsListBox.Items.Add(item3);
            _movementsClearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 0);
        }

        /**
         * @brief Verifies that every cleared command combobox is unregistered from the
         * scale registry to prevent memory leaks
         * 
         * When the Clear Commands button is clicked and all command comboboxes are removed
         * from the selected movement's command list, each combobox must be individually
         * unregistered from the scale registry. The scale registry holds references to
         * comboboxes for DPI scaling adjustments, and failing to unregister would keep
         * those references alive.
         */
        private void _testClearingMovementCommandsUnregistersFromRegistry()
        {
            var framePointMacroCommandRemoveActionHandler = _fixture();
            var item0 = new ComboBox();
            var item1 = new ComboBox();
            var item2 = new ComboBox();
            var item3 = new ComboBox();
            _movementCommandsListBox.Items.Add(item0);
            _movementCommandsListBox.Items.Add(item1);
            _movementCommandsListBox.Items.Add(item2);
            _movementCommandsListBox.Items.Add(item3);
            _movementsClearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_movementCommandsListBox.Items.Count == 0);
            Debug.Assert(_scaleRegistry.UnregisterHandlerCalls == 4);
            var unregisterItem0 = (
                (WindowComboBoxScaleActionHandlerParameters)
                _scaleRegistry.UnregisterHandlerCallArg_args[0]!
            );
            var unregisterItem1 = (
                (WindowComboBoxScaleActionHandlerParameters)
                _scaleRegistry.UnregisterHandlerCallArg_args[1]!
            );
            var unregisterItem2 = (
                (WindowComboBoxScaleActionHandlerParameters)
                _scaleRegistry.UnregisterHandlerCallArg_args[2]!
            );
            var unregisterItem3 = (
                (WindowComboBoxScaleActionHandlerParameters)
                _scaleRegistry.UnregisterHandlerCallArg_args[3]!
            );
            Debug.Assert(unregisterItem0.ScaleComboBox == item0);
            Debug.Assert(unregisterItem1.ScaleComboBox == item1);
            Debug.Assert(unregisterItem2.ScaleComboBox == item2);
            Debug.Assert(unregisterItem3.ScaleComboBox == item3);
        }

        public void Run()
        {
            _testClearingMovementCommandsClearsListBox();
            _testClearingMovementCommandsUnregistersFromRegistry();
        }
    }

    
    public class WindowRuneingEditorMovementsDeselectionActionHandlerTests
    {
        private ComboBox _directionComboBox = new ComboBox();

        private TextBox _distanceTextBox = new TextBox();

        private ListBox _movementsListBox = new ListBox();

        private ListBox _movementCommandsListBox = new ListBox();

        private MockWindowActionHandlerRegistry _scaleRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _directionComboBox = new ComboBox();
            _distanceTextBox = new TextBox();
            _movementsListBox = new ListBox();
            _movementCommandsListBox = new ListBox();
            _scaleRegistry = new MockWindowActionHandlerRegistry();
            return new WindowRuneingEditorMovementsDeselectionActionHandlerFacade(
                _directionComboBox,
                _distanceTextBox,
                _movementsListBox,
                _movementCommandsListBox,
                _scaleRegistry
            );
        }

        /**
         * @brief Verifies that when users select a different movement, the previously
         * selected movement's data (commands, distance, direction) is saved to its
         * ListBoxItem tag, and the editor UI is cleared for the new selection
         * 
         * When users click on a movement in the movement list box, any changes made to
         * that movement's command sequence, distance value, or direction setting must be
         * saved to the movement's data tag before switching to a different movement.
         * After saving, the editor UI (command list box, distance text box, direction
         * combo box) should be cleared to prepare for loading the newly selected
         * movement's data.
         */
        private void _testDeselectingMovementSavesToListBoxItem()
        {
            for (int itemCount = 2; itemCount < 10; itemCount++)
            {
                var movementDeselectionActionHandler = _fixture();
                for (int j = 0; j < itemCount; j++)
                {
                    var listBoxItem = new ListBoxItem { Tag = new WindowRuneingEditorMovementTag() };
                    _movementsListBox.Items.Add(listBoxItem);
                }
                for (int selectedIndex = 0; selectedIndex < itemCount; selectedIndex++)
                {
                    _movementsListBox.SelectedIndex = selectedIndex;
                    _movementCommandsListBox.Items.Add(new ComboBox { Text = "C" + (selectedIndex + 0).ToString() });
                    _movementCommandsListBox.Items.Add(new ComboBox { Text = "C" + (selectedIndex + 1).ToString() });
                    _movementCommandsListBox.Items.Add(new ComboBox { Text = "C" + (selectedIndex + 2).ToString() });
                    _distanceTextBox.Text = (selectedIndex + 3).ToString();
                    _directionComboBox.Text = ((selectedIndex % 2) == 0) ? "Left" : "Right";
                    _movementsListBox.SelectedIndex = (selectedIndex + 1) % itemCount;
                    var deselectedItem = (ListBoxItem)_movementsListBox.Items[selectedIndex];
                    var tag = (WindowRuneingEditorMovementTag)deselectedItem.Tag;
                    Debug.Assert(tag.MovementCommands[0] == "C" + (selectedIndex + 0).ToString());
                    Debug.Assert(tag.MovementCommands[1] == "C" + (selectedIndex + 1).ToString());
                    Debug.Assert(tag.MovementCommands[2] == "C" + (selectedIndex + 2).ToString());
                    Debug.Assert(tag.Distance == selectedIndex + 3);
                    Debug.Assert(tag.Direction == (RuneFrameDirectionTypes)(((selectedIndex % 2) == 0) ? 0 : 1));
                    Debug.Assert(_scaleRegistry.ClearHandlersCalls == selectedIndex + 1);
                    Debug.Assert(_movementCommandsListBox.Items.Count == 0);
                    Debug.Assert(_distanceTextBox.Text == "");
                    Debug.Assert(_directionComboBox.Text == "");
                }
            }
        }

        public void Run()
        {
            _testDeselectingMovementSavesToListBoxItem();
        }
    }


    public class WindowRuneingEditorMovementsSelectionActionHandlerTests
    {
        private ComboBox _directionComboBox = new ComboBox();

        private TextBox _distanceTextBox = new TextBox();

        private ListBox _movementsListBox = new ListBox();

        private ListBox _movementCommandsListBox = new ListBox();

        private ComboBox _comboBoxTemplate = new ComboBox();

        private MockWindowActionHandlerRegistry _scaleRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _directionComboBox = new ComboBox();
            _distanceTextBox = new TextBox();
            _movementsListBox = new ListBox();
            _movementCommandsListBox = new ListBox();
            _comboBoxTemplate = new ComboBox();
            _scaleRegistry = new MockWindowActionHandlerRegistry();
            return new WindowRuneingEditorMovementsSelectionActionHandlerFacade(
                _directionComboBox,
                _distanceTextBox,
                _movementsListBox,
                _movementCommandsListBox,
                _comboBoxTemplate,
                _scaleRegistry
            );
        }

        private ListBoxItem _selectItemFixture(int index)
        {
            return new ListBoxItem
            {
                Tag = new WindowRuneingEditorMovementTag
                {
                    Distance = index,
                    MovementCommands = [
                        "C" + (index + 0).ToString(),
                        "C" + (index + 1).ToString(),
                        "C" + (index + 2).ToString()
                    ],
                    Direction = (RuneFrameDirectionTypes)(index % 2)
                }
            };
        }

        /**
         * @brief Verifies that selecting a movement populates the command list box with
         * the movement's associated command sequence
         * 
         * When users click on a movement in the movement list box, the command list box
         * should display all automation commands associated with that movement. Each command
         * appears as a ComboBox element, allowing users to view and edit the individual
         * commands that execute when the character activates this movement. The commands
         * are displayed in the same order they will execute.
         */
        private void _testSelectingMovementPopulatesMovementCommands()
        {
            for (int i = 1; i < 10; i++)
            {
                var movementSelectionActionHandler = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _movementsListBox.Items.Add(_selectItemFixture(j));
                }
                for (int j = 0; j < i; j++)
                {
                    _movementCommandsListBox.Items.Clear();
                    _movementsListBox.SelectedIndex = j;
                    Debug.Assert(_movementCommandsListBox.Items.Count == 3);
                    Debug.Assert(_movementCommandsListBox.Items[0] is ComboBox);
                    Debug.Assert(_movementCommandsListBox.Items[1] is ComboBox);
                    Debug.Assert(_movementCommandsListBox.Items[2] is ComboBox);
                    var comboBox0 = (ComboBox)_movementCommandsListBox.Items[0];
                    var comboBox1 = (ComboBox)_movementCommandsListBox.Items[1];
                    var comboBox2 = (ComboBox)_movementCommandsListBox.Items[2];
                    Debug.Assert(comboBox0.Text == "C" + (j + 0).ToString());
                    Debug.Assert(comboBox1.Text == "C" + (j + 1).ToString());
                    Debug.Assert(comboBox2.Text == "C" + (j + 2).ToString());
                }
            }
        }

        /**
         * @brief Verifies that selecting a movement populates the direction combo box
         * and distance text box with the movement's configuration values
         * 
         * When users click on a movement in the movement list box, the direction combo box
         * should display which direction the character should move (Left or Right) after
         * completing the movement's command sequence. The distance text box should display
         * how far the character should move in that direction.
         */
        private void _testChangingSelectionPopulatesTextBoxes()
        {
            for (int i = 1; i < 10; i++)
            {
                var framePointMacroSelectionActionHandler = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _movementsListBox.Items.Add(_selectItemFixture(j));
                }
                for (int j = 0; j < i; j++)
                {
                    _movementCommandsListBox.Items.Clear();
                    _movementsListBox.SelectedIndex = j;
                    Debug.Assert(_directionComboBox.Text == ((RuneFrameDirectionTypes)(j % 2)).ToString());
                    Debug.Assert(_distanceTextBox.Text == j.ToString());
                }
            }
        }

        /**
         * @brief Verifies that each command combobox is registered with the scale registry
         * when a movement is selected
         * 
         * When a movement is selected and its command comboboxes are added to the command
         * list box, each combobox must be registered with the scale registry. The scale
         * registry handles dropdown sizing adjustments based on the current monitor's
         * DPI settings, ensuring combobox dropdowns display correctly on high-DPI displays
         * without being cut off or appearing too small.
         */
        private void _testChangingSelectionUpdatesScaleRegistry()
        {
            var framePointMacroSelectionActionHandler = _fixture();
            _movementsListBox.Items.Add(_selectItemFixture(0));
            _movementsListBox.Items.Add(_selectItemFixture(3));
            for (int i = 0; i < _movementsListBox.Items.Count; i++)
            {
                _scaleRegistry.RegisterHandlerCalls = 0;
                _scaleRegistry.RegisterHandlerCallArg_args = [];
                _scaleRegistry.CallOrder = [];
                _movementCommandsListBox.Items.Clear();
                _movementsListBox.SelectedIndex = i;
                Debug.Assert(_scaleRegistry.RegisterHandlerCalls == 3);
                var scale0 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.RegisterHandlerCallArg_args[0]!;
                var scale1 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.RegisterHandlerCallArg_args[1]!;
                var scale2 = (WindowComboBoxScaleActionHandlerParameters)_scaleRegistry.RegisterHandlerCallArg_args[2]!;
                Debug.Assert(scale0.ScaleComboBox.Text == "C" + ((3 * i) + 0).ToString());
                Debug.Assert(scale1.ScaleComboBox.Text == "C" + ((3 * i) + 1).ToString());
                Debug.Assert(scale2.ScaleComboBox.Text == "C" + ((3 * i) + 2).ToString());
            }
        }

        public void Run()
        {
            _testSelectingMovementPopulatesMovementCommands();
            _testChangingSelectionPopulatesTextBoxes();
            _testChangingSelectionUpdatesScaleRegistry();
        }
    }


    public class WindowRuneingEditorMovementsLoadConfigurationActionHandlerTests
    {
        private Button _loadButton = new Button();

        private MockLoadFileDialog _loadFileDialog = new MockLoadFileDialog();

        private MaplestoryBotConfiguration _configuration = new MaplestoryBotConfiguration();

        private AbstractWindowActionHandler _fixture()
        {
            _loadButton = new Button();
            _loadFileDialog = new MockLoadFileDialog();
            _configuration = new MaplestoryBotConfiguration
            {
                FrameMovementsDirectory = "cool_movements"
            };
            return new WindowRuneingEditorMovementsLoadConfigurationActionHandlerFacade(
                _loadButton,
                _loadFileDialog
            );
        }

        /**
         * @brief Verifies that clicking the load button opens a file dialog with the
         * correct initial directory from the configuration
         * 
         * When users click the Load button in the Runeing Editor's movements section,
         * the system should prompt the user to select a movement configuration file to
         * load. The file dialog should start in the directory specified by the
         * FrameMovementsDirectory configuration value, making it easy for users to find
         * their saved movement configuration files without navigating through the file
         * system each time.
         */
        private void _testLoadButtonClickOpensLoadMenu()
        {
            var handler = _fixture();
            handler.Inject(SystemInjectType.ConfigurationUpdate, _configuration);
            _loadButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_loadFileDialog.PromptCalls == 1);
            Debug.Assert(_loadFileDialog.PromptCallArg_initialDirectory[0] == "cool_movements");
        }

        public void Run()
        {
            _testLoadButtonClickOpensLoadMenu();
        }
    }


    public class WindowRuneingEditorMovementsLoadActionHandlerTests
    {
        private ListBox _listBox = new ListBox();

        private ComboBox _comboBox = new ComboBox();

        private AbstractWindowActionHandlerRegistry _comboBoxPopupScaleRegistry = new MockWindowActionHandlerRegistry();

        private List<string> _expectedContents = [];

        private MockLoadFileDialog _loadFileDialog = new MockLoadFileDialog();

        private AbstractWindowActionHandler _fixture(
            AbstractWindowActionHandlerRegistry comboBoxPopupScaleRegistry
        )
        {
            _listBox = new ListBox();
            _comboBox = new ComboBox();
            _comboBoxPopupScaleRegistry = comboBoxPopupScaleRegistry;
            _comboBox.Items.Add(new ComboBoxItem { Content = "A" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "B" });
            _comboBox.Items.Add(new ComboBoxItem { Content = "C" });
            _expectedContents = ["a", "b", "c", "d", "e"];
            return new WindowRuneingEditorMovementsLoadActionHandlerFacade(
                _loadFileDialog,
                _listBox,
                _comboBox,
                _comboBoxPopupScaleRegistry
            );
        }

        /**
         * @brief Verifies that loading a configuration file populates the list box with
         * movement entries matching the loaded data
         * 
         * When users load a movement configuration file, each movement from the file
         * should appear as a ComboBox in the list box. Each ComboBox should contain the
         * same dropdown items as the template ComboBox (e.g., "A", "B", "C"), and its
         * selected text should match the movement value from the configuration file
         * (e.g., "a", "b", "c", "d", "e"). This allows users to view and edit the
         * movement sequence for the selected movement.
         */
        private void _testLoadButtonClickOpensLoadFileDialog()
        {
            var registry = new WindowComboBoxScaleActionHandlerRegistry();
            var handler = _fixture(registry);
            _loadFileDialog.InvokeFileLoaded(
                "some_path",
                "{\"macro\": [\"a\", \"b\", \"c\", \"d\", \"e\"]}"
            );
            Debug.Assert(_listBox.Items.Count == 5);
            for (int i = 0; i < _expectedContents.Count; i++)
            {
                Debug.Assert(((ComboBox)_listBox.Items[i]).Text == _expectedContents[i]);
                for (int j = 0; j < _comboBox.Items.Count; j++)
                {
                    Debug.Assert(
                        ((ComboBoxItem)((ComboBox)_listBox.Items[i]).Items[j]).Content.ToString()
                        == ((ComboBoxItem)_comboBox.Items[j]).Content.ToString()
                    );
                }
            }
        }

        /**
         * @brief Verifies that each newly created movement ComboBox is registered with
         * the scale registry for proper DPI scaling
         * 
         * When movement ComboBoxes are created and added to the list box, each ComboBox
         * must be registered with the scale registry. The scale registry handles dropdown
         * sizing adjustments based on the current monitor's DPI settings, ensuring
         * combobox dropdowns display correctly on high-DPI displays without being cut
         * off or appearing too small.
         */
        private void _testLoadButtonClickRegistersComboBoxPopupScalers()
        {
            var mockRegistry = new MockWindowActionHandlerRegistry();
            var handler = _fixture(mockRegistry);
            _loadFileDialog.InvokeFileLoaded(
                "some_path",
                "{\"macro\": [\"a\", \"b\", \"c\", \"d\", \"e\"]}"
            );
            Debug.Assert(mockRegistry.RegisterHandlerCalls == 5);
            for (int i = 0; i < _expectedContents.Count; i++)
            {
                var parameters = (
                    (WindowComboBoxScaleActionHandlerParameters)
                    mockRegistry.RegisterHandlerCallArg_args[i]!
                );
                Debug.Assert(parameters.ScaleComboBox == (ComboBox)_listBox.Items[i]);
            }
        }

        /**
         * @brief Verifies that the scale registry is cleared before registering new
         * ComboBoxes when loading a configuration file
         * 
         * When loading a movement configuration file, any previously registered
         * ComboBoxes from previous loads must be cleared from the scale registry before
         * registering the new ones. This prevents memory leaks and stale references to
         * disposed ComboBoxes.
         */
        private void _testLoadButtonClickClearsComboBoxPopupScalersBefroreRegisteringNew()
        {
            var mockRegistry = new MockWindowActionHandlerRegistry();
            var handler = _fixture(mockRegistry);
            _loadFileDialog.InvokeFileLoaded(
                "some_path",
                "{\"macro\": [\"a\", \"b\", \"c\", \"d\", \"e\"]}"
            );
            var reference = new TestUtilities().Reference(mockRegistry);
            var clearCallRef = reference + "ClearHandlers";
            var registerCallRef = reference + "RegisterHandler";
            Debug.Assert(mockRegistry.CallOrder.Count == 6);
            Debug.Assert(mockRegistry.CallOrder[0] == clearCallRef);
            Debug.Assert(mockRegistry.CallOrder[1] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[2] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[3] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[4] == registerCallRef);
            Debug.Assert(mockRegistry.CallOrder[5] == registerCallRef);
        }

        public void Run()
        {
            _testLoadButtonClickOpensLoadFileDialog();
            _testLoadButtonClickRegistersComboBoxPopupScalers();
            _testLoadButtonClickClearsComboBoxPopupScalersBefroreRegisteringNew();
        }
    }


    public class WindowRuneingEditorMovementsSaveActionHandlerTests
    {
        private Button _saveButton = new Button();

        private ListBox _listBox = new ListBox();

        private MockSaveFileDialog _saveFileDialog = new MockSaveFileDialog();

        private AbstractWindowActionHandler _fixture()
        {
            _saveButton = new Button();
            _listBox = new ListBox();
            _listBox.Items.Add(new ComboBox { Text = "A" });
            _listBox.Items.Add(new ComboBox { Text = "B" });
            _listBox.Items.Add(new ComboBox { Text = "C" });
            _saveFileDialog = new MockSaveFileDialog();
            return new WindowRuneingEditorMovementsSaveActionHandlerFacade(
                _saveButton, _listBox, _saveFileDialog
            );
        }

        /**
         * @brief Verifies that clicking the save button opens a file dialog with the
         * correct initial directory and saves the serialized movement data
         * 
         * When users click the Save button in the Runeing Editor's movements section,
         * the system should prompt the user to choose a location and filename for saving
         * the movement configuration. The file dialog should start in the directory
         * specified by the FramePointsDirectory configuration value, making it easy for
         * users to save their movement configuration files alongside their other
         * configuration files.
         */
        private void _testSaveButtonClickOpensSaveFileDialog()
        {
            var handler = _fixture();
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                new MaplestoryBotConfiguration { FrameMovementsDirectory = "MEOW" }
            );
            _saveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_saveFileDialog.PromptCalls == 1);
            var normalizer = new JsonNormalizer();
            var saveContent = normalizer.Normalize(_saveFileDialog.PromptCallArg_saveContent[0]);
            var initialDirectory = _saveFileDialog.PromptCallArg_initialDirectory[0];
            Debug.Assert(initialDirectory == "MEOW");
            Debug.Assert(saveContent == normalizer.Normalize("{\"macro\":[\"A\",\"B\",\"C\"]}"));
        }

        public void Run()
        {
            _testSaveButtonClickOpensSaveFileDialog();
        }
    }


    public class WindowRuneingEditorMovementsLoadingActionHandlerTests
    {
        private ListBox _MovementsListBox = new ListBox();

        private FrameworkElement _movementsTemplate = new FrameworkElement();

        private MockSystemWindow _windowRuneingEditor = new MockSystemWindow();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private Grid _movementsTemplateGrid()
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
            _MovementsListBox = new ListBox();
            _movementsTemplate = _movementsTemplateGrid();
            _windowRuneingEditor = new MockSystemWindow();
            _editMenuState = new WindowMapEditMenuState();
            _bottingModel = DataFixtures.BottingModelFixture();
            _windowRuneingEditor.GetWindowReturn.Add(new Window());
            var handler = new WindowRuneingEditorMovementsLoadingActionHandlerFacade(
                _MovementsListBox,
                _movementsTemplate,
                _windowRuneingEditor,
                _editMenuState
            );
            handler.Inject(SystemInjectType.BottingModel, _bottingModel);
            return handler;
        }

        /**
         * @brief Verifies that movements are populated in the list box when the editor
         * becomes visible
         * 
         * When users open the Runeing Editor window while a frame is selected, all movements
         * belonging to that frame should appear as list items. Each movement is displayed
         * as a Grid containing editable text boxes for the movement name, and stores
         * references to its text boxes in the Tag property for later updates.
         */
        private void _testMovementsPopulatedWhenEditorIsVisible()
        {
            for (int i = 0; i < 2; i++)
            {
                var movementsLoadingActionHandler = _fixture();
                _windowRuneingEditor.VisibleReturn.Add(i == 1);
                _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
                movementsLoadingActionHandler.OnDependencyEvent(
                    movementsLoadingActionHandler,
                    new DependencyPropertyChangedEventArgs()
                );
                if (i == 1)
                {
                    Debug.Assert(_MovementsListBox.Items.Count == 2);
                    var item0 = (ListBoxItem)_MovementsListBox.Items[0];
                    var item1 = (ListBoxItem)_MovementsListBox.Items[1];
                    Debug.Assert(item0.Content is Grid);
                    Debug.Assert(item1.Content is Grid);
                    Debug.Assert(item0.Tag is WindowRuneingEditorMovementTag);
                    Debug.Assert(item1.Tag is WindowRuneingEditorMovementTag);
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
                    Debug.Assert(_MovementsListBox.Items.Count == 0);
                }
            }
        }

        /**
         * @brief Verifies that existing movements are cleared from the list box before
         * loading new ones when the editor becomes visible
         * 
         * When the OnDependencyEvent is triggered (when the window becomes visible),
         * the list box should be completely cleared of any existing movement items before
         * populating with new ones. This ensures the list box starts empty before any
         * new population logic runs, preventing duplicate entries or stale data from
         * previous selections. The clearing behavior happens regardless of whether the
         * window is visible or a frame is selected.
         */
        private void _testMovementsCleared()
        {
            for (int i = 1; i < 10; i++)
            for (int j = 0; j < 2; j++)
            {
                var movementsLoadingActionHandler = _fixture();
                movementsLoadingActionHandler.Inject(
                    SystemInjectType.BottingModel, new BottingModel()
                );
                _windowRuneingEditor.VisibleReturn.Add(j == 1);
                _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
                for (int k = 0; k < i; k++)
                    _MovementsListBox.Items.Add(new object());
                movementsLoadingActionHandler.OnDependencyEvent(
                    movementsLoadingActionHandler,
                    new DependencyPropertyChangedEventArgs()
                );
                Debug.Assert(_MovementsListBox.Items.Count == (j == 1 ? 0 : i));
            }
        }

        /**
         * @brief Verifies that movement names are correctly displayed in the editor
         * 
         * When users open the Runeing Editor window, each movement displays its name
         * (e.g., "D0", "D1") in a text box within its grid item. This allows users to
         * identify and edit the movement's name directly in the editor interface.
         */
        private void _testMovementNamesWhenEditorIsVisible()
        {
            for (int i = 0; i < 2; i++)
            {
                var framePointMacrosLoadingActionHandler = _fixture();
                _windowRuneingEditor.VisibleReturn.Add(i == 1);
                _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
                framePointMacrosLoadingActionHandler.OnDependencyEvent(
                    framePointMacrosLoadingActionHandler,
                    new DependencyPropertyChangedEventArgs()
                );
                if (i == 1)
                {
                    var item0 = (ListBoxItem)_MovementsListBox.Items[0];
                    var item1 = (ListBoxItem)_MovementsListBox.Items[1];
                    var itemGrid0 = (Grid)item0.Content;
                    var itemGrid1 = (Grid)item1.Content;
                    var itemList0 = (List<TextBox>)itemGrid0.Tag;
                    var itemList1 = (List<TextBox>)itemGrid1.Tag;
                    Debug.Assert(itemList0[0].Text == "D0");
                    Debug.Assert(itemList1[0].Text == "D1");
                }
                else
                {
                    Debug.Assert(_MovementsListBox.Items.Count == 0);
                }
            }
        }

        /**
         * @brief Verifies that movement text boxes have the correct visual properties
         * 
         * When movements are loaded into the editor, each text box within the movement
         * grid must display with consistent styling for proper visibility and usability.
         * The margin, colors, font, and height should match the design specifications.
         */
        private void _testMovementTextBoxProperties()
        {
            var framePointMacrosLoadingActionHandler = _fixture();
            _windowRuneingEditor.VisibleReturn.Add(true);
            _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
            framePointMacrosLoadingActionHandler.OnDependencyEvent(
                framePointMacrosLoadingActionHandler,
                new DependencyPropertyChangedEventArgs()
            );
            var item0 = (ListBoxItem)_MovementsListBox.Items[0];
            var item1 = (ListBoxItem)_MovementsListBox.Items[1];
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
         * @brief Verifies that movement command sequences are correctly loaded and stored
         * 
         * When movements are loaded into the editor, each movement must store its
         * associated command sequence (the automation commands that execute when the
         * movement is triggered). These commands are stored in the Tag property of each
         * list box item, allowing the editor to later display and edit the command list.
         */
        private void _testMovementCommands()
        {
            var framePointMacrosLoadingActionHandler = _fixture();
            _windowRuneingEditor.VisibleReturn.Add(true);
            _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
            framePointMacrosLoadingActionHandler.OnDependencyEvent(
                framePointMacrosLoadingActionHandler,
                new DependencyPropertyChangedEventArgs()
            );
            var item0 = (ListBoxItem)_MovementsListBox.Items[0];
            var item1 = (ListBoxItem)_MovementsListBox.Items[1];
            var item0Commands = ((WindowRuneingEditorMovementTag)item0.Tag).MovementCommands;
            var item1Commands = ((WindowRuneingEditorMovementTag)item1.Tag).MovementCommands;
            Debug.Assert(item0Commands.Count == 3);
            Debug.Assert(item0Commands[0] == "C123");
            Debug.Assert(item0Commands[1] == "C234");
            Debug.Assert(item0Commands[2] == "C345");
            Debug.Assert(item1Commands.Count == 3);
            Debug.Assert(item1Commands[0] == "C234");
            Debug.Assert(item1Commands[1] == "C345");
            Debug.Assert(item1Commands[2] == "C456");
        }

        /**
         * @brief Verifies that movement distance values are correctly loaded
         * 
         * When movements are loaded into the editor, each movement must store its
         * distance value - the maximum movement distance allowed for a single command,
         * ensuring the bot can move toward the target point without falling off platforms.
         */
        private void _testMovementDistance()
        {
            var framePointMacrosLoadingActionHandler = _fixture();
            _windowRuneingEditor.VisibleReturn.Add(true);
            _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
            framePointMacrosLoadingActionHandler.OnDependencyEvent(
                framePointMacrosLoadingActionHandler,
                new DependencyPropertyChangedEventArgs()
            );
            var item0 = (ListBoxItem)_MovementsListBox.Items[0];
            var item1 = (ListBoxItem)_MovementsListBox.Items[1];
            Debug.Assert(((WindowRuneingEditorMovementTag)item0.Tag).Distance == 123);
            Debug.Assert(((WindowRuneingEditorMovementTag)item1.Tag).Distance == 234);
        }

        /**
         * @brief Verifies that movement direction values are correctly loaded
         * 
         * When movements are loaded into the editor, each movement must store its
         * direction value - which direction the character should move (Left, Right)
         * after completing the movement's command sequence.
         */
        private void _testMovementDirection()
        {
            var framePointMacrosLoadingActionHandler = _fixture();
            _windowRuneingEditor.VisibleReturn.Add(true);
            _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
            framePointMacrosLoadingActionHandler.OnDependencyEvent(
                framePointMacrosLoadingActionHandler,
                new DependencyPropertyChangedEventArgs()
            );
            var item0 = (ListBoxItem)_MovementsListBox.Items[0];
            var item1 = (ListBoxItem)_MovementsListBox.Items[1];
            Debug.Assert(((WindowRuneingEditorMovementTag)item0.Tag).Direction == (RuneFrameDirectionTypes)234);
            Debug.Assert(((WindowRuneingEditorMovementTag)item1.Tag).Direction == (RuneFrameDirectionTypes)345);
        }

        /**
         * @brief Verifies that the first movement is automatically selected after loading
         * 
         * When movements are loaded into the editor, the first movement in the list
         * should be automatically selected. This provides immediate visual feedback to
         * users that movements are ready for editing and allows them to start working
         * with the first movement without having to manually click on it.
         */
        private void _testMovementFirstSelected()
        {
            var framePointMacrosLoadingActionHandler = _fixture();
            _windowRuneingEditor.VisibleReturn.Add(true);
            _editMenuState.Select(SelectedFixture.Object("FT0", "F0"));
            framePointMacrosLoadingActionHandler.OnDependencyEvent(
                framePointMacrosLoadingActionHandler,
                new DependencyPropertyChangedEventArgs()
            );
            Debug.Assert(_MovementsListBox.SelectedIndex == 0);
        }

        public void Run()
        {
            _testMovementsPopulatedWhenEditorIsVisible();
            _testMovementsCleared();
            _testMovementNamesWhenEditorIsVisible();
            _testMovementTextBoxProperties();
            _testMovementCommands();
            _testMovementDistance();
            _testMovementDirection();
            _testMovementFirstSelected();
        }
    }


    public class WindowRuneingEditorMovementsSavingActionHandlerTests
    {
        private MockSystemWindow _windowRuneingEditor = new MockSystemWindow();

        private ListBox _movementsListBox = new ListBox();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private ListBoxItem _listBoxItemFixture(
            WindowRuneingEditorMovementTag listBoxItemTag,
            List<TextBox> listTextBox
        )
        {
            var listBoxGrid = new Grid
            {
                Tag = listTextBox
            };
            var listBoxItem = new ListBoxItem
            {
                Tag = listBoxItemTag,
                Content = listBoxGrid
            };
            return listBoxItem;
        }

        private AbstractWindowActionHandler _fixture(string nextFrame0, string nextFrame1)
        {
            _windowRuneingEditor = new MockSystemWindow();
            _windowRuneingEditor.GetWindowReturn.Add(new Window());
            _movementsListBox = new ListBox();
            _editMenuState = new WindowMapEditMenuState();
            _bottingModel = DataFixtures.BottingModelFixture();
            var handler = new WindowRuneingEditorMovementsSavingActionHandlerFacade(
                _windowRuneingEditor,
                _movementsListBox,
                _editMenuState
            );
            handler.Inject(SystemInjectType.BottingModel, _bottingModel);
            _movementsListBox.Items.Add(
                _listBoxItemFixture(
                    new WindowRuneingEditorMovementTag
                    {
                        Direction = RuneFrameDirectionTypes.Left,
                        Distance = 123,
                        MovementCommands = ["C1", "C2", "C3"]
                    },
                    [new TextBox { Text = "Meow 1" }]
                )
            );
            _movementsListBox.Items.Add(
                _listBoxItemFixture(
                    new WindowRuneingEditorMovementTag
                    {
                        Direction = RuneFrameDirectionTypes.Right,
                        Distance = 234,
                        MovementCommands = ["C2", "C3", "C4"]
                    },
                    [new TextBox { Text = "Meow 2" }]
                )
            );
            return handler;
        }

        /**
         * @brief Verifies that movement command sequences are saved to the botting model
         * when the editor closes
         * 
         * When users edit the command sequences of movements in the editor, these changes
         * must be persisted to the rune frame's direction commands when the editor window
         * closes. The command list defines the automation actions that execute when the
         * bot moves in that direction.
         */
        private void _testClosingEditorSavesMovementCommands()
        {
            for (int i = 0; i < 2; i++)
            {
                var nextFrame = "F" + ((i + 1) % 2).ToString();
                var framePointMacrosSavingActionHandler = _fixture(nextFrame, nextFrame);
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(false);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[0].DirectionCommands.Count == 3);
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[0].DirectionCommands[0] == "C1");
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[0].DirectionCommands[1] == "C2");
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[0].DirectionCommands[2] == "C3");
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[1].DirectionCommands.Count == 3);
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[1].DirectionCommands[0] == "C2");
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[1].DirectionCommands[1] == "C3");
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[1].DirectionCommands[2] == "C4");
            }
        }

        /**
         * @brief Verifies that movement distance values are saved to the botting model
         * when the editor closes
         * 
         * When users modify the distance value of a movement (the maximum safe movement
         * distance that prevents the character from falling off platforms), this value
         * must be saved to the rune frame's direction list when the editor closes.
         */
        private void _testClosingEditorSavesDistance()
        {
            for (int i = 0; i < 2; i++)
            {
                var nextFrame = "F" + ((i + 1) % 2).ToString();
                var framePointMacrosSavingActionHandler = _fixture(nextFrame, nextFrame);
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(false);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[0].Distance == 123);
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[1].Distance == 234);
            }
        }

        /**
         * @brief Verifies that movement names are saved to the botting model when the
         * editor closes
         * 
         * When users edit the name of a movement in the editor's text box, the movement
         * name must be saved to the rune frame's direction list as DirectionName. This
         * allows users to identify and reference movements by custom names in the editor.
         */
        private void _testClosingEditorMovementName()
        {
            for (int i = 0; i < 2; i++)
            {
                var nextFrame = "F" + ((i + 1) % 2).ToString();
                var framePointMacrosSavingActionHandler = _fixture(nextFrame, nextFrame);
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(false);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections.Count == 2);
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[0].DirectionName == "Meow 1");
                Debug.Assert(runeFrame.FrameData.RuneFrameDirections[1].DirectionName == "Meow 2");
            }
        }

        /**
         * @brief Verifies that opening the editor does NOT save movement data
         * 
         * When users open the Runeing Editor window (becomes visible), the system should
         * load existing data from the botting model into the editor UI, but should NOT
         * save or overwrite any data in the botting model. The data displayed in the
         * editor comes from the botting model via the loading action handler, not the
         * other way around.
         */
        private void _testOpeningEditorDoesntSaveMovements()
        {
            for (int i = 0; i < 2; i++)
            {
                var nextFrame = "F" + ((i + 1) % 2).ToString();
                var framePointMacrosSavingActionHandler = _fixture(nextFrame, nextFrame);
                var runeModel = _bottingModel.GetRuneModel();
                _windowRuneingEditor.VisibleReturn.Add(true);
                _editMenuState.Select(SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString()));
                framePointMacrosSavingActionHandler.OnDependencyEvent(
                    framePointMacrosSavingActionHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                var nextRuneFrame = runeModel.FindRuneFrameRefByLabel("F" + ((i + 1) % 2).ToString())!;
                var runeFrameDirections = runeFrame.FrameData.RuneFrameDirections;
                Debug.Assert(runeFrameDirections.Count == 2);
                Debug.Assert(runeFrameDirections[0].Distance == (i == 0 ? 123 : 345));
                Debug.Assert(runeFrameDirections[1].Distance == (i == 0 ? 234 : 456));
                Debug.Assert(runeFrameDirections[0].DirectionName == "D" + ((2 * i) + 0).ToString());
                Debug.Assert(runeFrameDirections[1].DirectionName == "D" + ((2 * i) + 1).ToString());
                Debug.Assert(runeFrameDirections[1].Direction == (RuneFrameDirectionTypes)(i == 0 ? 345 : 567));
                Debug.Assert(runeFrameDirections[0].Direction == (RuneFrameDirectionTypes)(i == 0 ? 234 : 456));
            }
        }

        public void Run()
        {
            _testClosingEditorSavesMovementCommands();
            _testClosingEditorSavesDistance();
            _testClosingEditorMovementName();
            _testOpeningEditorDoesntSaveMovements();
        }
    }


    public class WindowRuneingEditorFrameNameSavingActionHandlerTests
    {
        private MockSystemWindow _windowRuneingEditor = new MockSystemWindow();

        private TextBox _frameNameTextBox = new TextBox();

        private AbstractWindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private AbstractWindowActionHandler _fixture()
        {
            _windowRuneingEditor = new MockSystemWindow();
            _frameNameTextBox = new TextBox { Text = "meow" };
            _editMenuState = new WindowMapEditMenuState();
            _bottingModel = DataFixtures.BottingModelFixture();
            _windowRuneingEditor.GetWindowReturn.Add(new Window());
            var handler = new WindowRuneingEditorFrameNameSavingActionHandlerFacade(
                _windowRuneingEditor,
                _frameNameTextBox,
                _editMenuState
            );
            handler.Inject(SystemInjectType.BottingModel, _bottingModel);
            return handler;
        }

        /**
         * @brief Verifies that the frame name is saved to the botting model and all
         * text dependencies when the editor closes
         * 
         * When users edit the frame name text box in the Runeing Editor and close the
         * window, the new frame name must be saved to the rune frame. Additionally, any
         * text dependencies that display the frame name in the UI must be updated to
         * reflect the new name, ensuring visual and data model synchronization across
         * all bound controls.
         */
        private void _testClosingEditorSavesFrameName()
        {
            for (int i = 0; i < 2; i++)
            {
                var frameNameSavingHandler = _fixture();
                var selectedFixture = SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString());
                var selectedDataTag = (MapCanvasRuneFrameDataTag)((Canvas)selectedFixture.FrameObject).Tag;
                _editMenuState.Select(selectedFixture);
                _windowRuneingEditor.VisibleReturn.Add(false);
                frameNameSavingHandler.OnDependencyEvent(
                    frameNameSavingHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeModel = _bottingModel.GetRuneModel();
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                Debug.Assert(runeFrame.FrameData.FrameName == "meow");
                Debug.Assert(selectedDataTag.FrameName == "meow");
                Debug.Assert(runeFrame.FrameData.ElementTexts.Count == 2);
                Debug.Assert(((TextBlock)runeFrame.FrameData.ElementTexts[0]).Text == "meow");
                Debug.Assert(((TextBox)runeFrame.FrameData.ElementTexts[1]).Text == "meow");
            }
        }

        /**
         * @brief Verifies that opening the editor does NOT save frame name changes
         * 
         * When users open the Runeing Editor window (becomes visible), the system should
         * load existing data from the botting model into the editor UI, but should NOT
         * save or overwrite any data in the botting model. The data displayed in the
         * editor comes from the botting model via the loading action handler, not the
         * other way around.
         */
        private void _testOpeningEditorDoesntSaveFrameName()
        {
            for (int i = 0; i < 2; i++)
            {
                var frameNameSavingHandler = _fixture();
                var selectedFixture = SelectedFixture.Object("FT" + i.ToString(), "F" + i.ToString());
                var selectedDataTag = (MapCanvasRuneFrameDataTag)((Canvas)selectedFixture.FrameObject).Tag;
                _editMenuState.Select(selectedFixture);
                _windowRuneingEditor.VisibleReturn.Add(true);
                frameNameSavingHandler.OnDependencyEvent(
                    frameNameSavingHandler, new DependencyPropertyChangedEventArgs()
                );
                var runeModel = _bottingModel.GetRuneModel();
                var runeFrame = runeModel.FindRuneFrameByName("FT" + i.ToString())!;
                Debug.Assert(runeFrame.FrameData.FrameName == "F" + i.ToString());
                Debug.Assert(selectedDataTag.FrameName == "F" + i.ToString());
                Debug.Assert(runeFrame.FrameData.ElementTexts.Count == 2);
                Debug.Assert(((TextBlock)runeFrame.FrameData.ElementTexts[0]).Text == "");
                Debug.Assert(((TextBox)runeFrame.FrameData.ElementTexts[1]).Text == "");
            }
        }

        public void Run()
        {
            _testClosingEditorSavesFrameName();
            _testOpeningEditorDoesntSaveFrameName();
        }
    }


    public class WindowRuneingEditorHandlersTestSuite
    {
        public void Run()
        {
            new WindowRuneingEditorFrameNameLoadingActionHandlerTests().Run();
            new WindowRuneingEditorFrameNameSavingActionHandlerTests().Run();

            new WindowRuneingEditorFramePointMacrosLoadingActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacrosSavingActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacroAccessActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacroDeselectionActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacroSelectionActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacroCommandAddActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacroCommandRemoveActionHandlerTests().Run();
            new WindowRuneingEditorFramePointMacroCommandClearActionHandlerTests().Run();
            new WindowRuneingEditorFramePointLoadConfigurationActionHandlerTests().Run();
            new WindowRuneingEditorFramePointLoadActionHandlerTests().Run();
            new WindowRuneingEditorFramePointSaveActionHandlerTests().Run();

            new WindowRuneingEditorMovementsLoadingActionHandlerTests().Run();
            new WindowRuneingEditorMovementsSavingActionHandlerTests().Run();
            new WindowRuneingEditorMovementAddActionHandlerTests().Run();
            new WindowRuneingEditorMovementRemoveActionHandlerTests().Run();
            new WindowRuneingEditorMovementMacroAccessActionHandlerTests().Run();
            new WindowRuneingEditorMovementsCommandAddActionHandlerTests().Run();
            new WindowRuneingEditorMovementsCommandRemoveActionHandlerTests().Run();
            new WindowRuneingEditorMovementsCommandClearActionHandlerTests().Run();
            new WindowRuneingEditorMovementsDeselectionActionHandlerTests().Run();
            new WindowRuneingEditorMovementsSelectionActionHandlerTests().Run();
            new WindowRuneingEditorMovementsLoadConfigurationActionHandlerTests().Run();
            new WindowRuneingEditorMovementsLoadActionHandlerTests().Run();
            new WindowRuneingEditorMovementsSaveActionHandlerTests().Run();
        }
    }
}
