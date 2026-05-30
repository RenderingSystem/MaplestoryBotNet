using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities.Models;
using MaplestoryBotNetTests.Systems.Configuration.Tests;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class StackPanelFixture
    {
        public static StackPanel Fixture()
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Focusable = false,
                Children = {
                    new CheckBox
                    {
                        VerticalContentAlignment = VerticalAlignment.Center,
                    },
                    new TextBox
                    {
                        Margin = new Thickness(12, 23, 34, 45),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Width = 123,
                        Height = 234,
                        Background = new SolidColorBrush(Color.FromArgb(23, 34, 45, 56)),
                        Foreground = new SolidColorBrush(Color.FromArgb(34, 45, 56, 67)),
                        FontFamily = new FontFamily("meow")
                    }
                }
            };
        }
    }


    public class ComboBoxFixture
    {
        public static ComboBox Fixture()
        {
            return new ComboBox
            {
                Width = 1234,
                IsEditable = true,
                FontSize = 2345,
                Items =
                {
                    new ComboBoxItem { Content = "meow1" },
                    new ComboBoxItem { Content = "meow2" },
                    new ComboBoxItem { Content = "meow3" },
                    new ComboBoxItem { Content = "meow4" },
                }
            };
        }
    }


    public class SkillListBoxFixture
    {
        public static ListBoxItem Fixture()
        {
            return new ListBoxItem
            {
                Tag = new Skill()
                {
                    Name = "meow",
                    MinDelay = 123,
                    MaxDelay = 234,
                    Macros = ["12", "23", "34"]
                }
            };
        }


        public static List<ListBoxItem> FixtureList()
        {
            return [
                new ListBoxItem
                {
                    Content = new StackPanel
                    {
                        Children = {
                            new CheckBox { IsChecked = true },
                            new TextBox { Text = "456" }
                        }
                    },
                    Tag = new Skill
                    {
                        Macros = ["12", "23", "34"],
                        MinDelay = 234,
                        MaxDelay = 345
                    }
                },
                new ListBoxItem
                {
                    Content = new StackPanel
                    {
                        Children = {
                            new CheckBox { IsChecked = false },
                            new TextBox { Text = "567" }
                        }
                    },
                    Tag = new Skill
                    {
                        Macros = ["23", "34", "45"],
                        MinDelay = 345,
                        MaxDelay = 456
                    }
                },
                new ListBoxItem
                {
                    Content = new StackPanel
                    {
                        Children = {
                            new CheckBox { IsChecked = true },
                            new TextBox { Text = "678" }
                        }
                    },
                    Tag = new Skill
                    {
                        Macros = ["34", "45", "56"],
                        MinDelay = 456,
                        MaxDelay = 567
                    }
                }
            ];
        }
    }


    public class MacroListBoxFixture
    {
        public static List<ListBoxItem> Fixture()
        {
            return [
                new ListBoxItem { Content = new ComboBox { Text = "meow1" } },
                new ListBoxItem { Content = new ComboBox { Text = "meow2" } },
                new ListBoxItem { Content = new ComboBox { Text = "meow3" } },
                new ListBoxItem { Content = new ComboBox { Text = "meow4" } }
            ];
        }
    }


    public class SkillsModelFixture
    {
        public static AbstractSkillsModel Fixture()
        {
            var skillsModel = new SkillsModel();
            skillsModel.SetSkills(
                [
                    new Skill
                    {
                        Active = 1,
                        Macros = ["12", "23", "34"],
                        MinDelay = 234,
                        MaxDelay = 345,
                        Name = "456"
                    },
                    new Skill
                    {
                        Active = 0,
                        Macros = ["23", "34", "45"],
                        MinDelay = 345,
                        MaxDelay = 456,
                        Name = "567"
                    },
                    new Skill
                    {
                        Active = 1,
                        Macros = ["34", "45", "56"],
                        MinDelay = 456,
                        MaxDelay = 567,
                        Name = "678"
                    }
                ]
            );
            return skillsModel;
        }


        public static string Json()
        {
            return """
            {
                "skills": [
                    {
                        "name": "456",
                        "min_delay": 234,
                        "max_delay": 345,
                        "macros": [
                            "12",
                            "23",
                            "34"
                        ],
                        "active": 1
                    },
                    {
                        "name": "567",
                        "min_delay": 345,
                        "max_delay": 456,
                        "macros": [
                            "23",
                            "34",
                            "45"
                        ],
                        "active": 0
                    },
                    {
                        "name": "678",
                        "min_delay": 456,
                        "max_delay": 567,
                        "macros": [
                            "34",
                            "45",
                            "56"
                        ],
                        "active": 1
                    }
                ]
            }
            """;
        }
    }


    public class WindowSkillsMenuAddSkillActionHandlerTests
    {
        private Button _addSkillButton = new Button();

        private ListBox _addSkillListBox = new ListBox();

        private StackPanel _addSkillTemplate = new StackPanel();


        private AbstractWindowActionHandler _fixture()
        {
            _addSkillButton = new Button();
            _addSkillListBox = new ListBox();
            _addSkillTemplate = StackPanelFixture.Fixture();
            return new WindowSkillsMenuAddSkillActionHandlerFacade(
                _addSkillButton,
                _addSkillListBox,
                _addSkillTemplate
            );
        }

        /**
         * @brief Tests that clicking the Add Skill button creates a new skill entry in the list
         * 
         * When the user clicks the Add Skill button in the skills management window, a new
         * skill entry should appear in the list. This new entry allows the user to configure
         * a skill's macro sequence, cooldown delays, and activation settings.
         */
        private void _testClickingButtonAddsSkillListBoxItem()
        {
            var handler = _fixture();
            _addSkillButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_addSkillListBox.Items.Count == 1);
            Debug.Assert(_addSkillListBox.Items[0] is ListBoxItem);
            Debug.Assert(_addSkillListBox.SelectedIndex == 0);
        }

        /**
         * @brief Tests that new skill entries are added at the bottom of the existing list
         * 
         * When the user has multiple skills already configured and clicks the Add Skill
         * button, the new skill should appear after all existing entries, maintaining
         * the chronological order of skill creation.
         */
        private void _testClickingButtonAddsSkillListBoxItemToEnd()
        {
            var handler = _fixture();
            var listBoxItems = new[] { new object(), new object(), new object() };
            _addSkillListBox.Items.Add(listBoxItems[0]);
            _addSkillListBox.Items.Add(listBoxItems[1]);
            _addSkillListBox.Items.Add(listBoxItems[2]);
            _addSkillButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_addSkillListBox.Items.Count == 4);
            Debug.Assert(_addSkillListBox.Items[0] == listBoxItems[0]);
            Debug.Assert(_addSkillListBox.Items[1] == listBoxItems[1]);
            Debug.Assert(_addSkillListBox.Items[2] == listBoxItems[2]);
            Debug.Assert(_addSkillListBox.Items[3] is ListBoxItem);
            Debug.Assert(_addSkillListBox.SelectedIndex == 3);
        }

        /**
         * @brief Tests that new skill entries are inserted below the currently selected skill
         * 
         * When the user has a skill selected in the list and clicks Add Skill, the new skill
         * should be inserted immediately after the selected one. This allows users to organize
         * their skill rotation by adding new skills at specific positions in the list
         * rather than always appending to the end.
         */
        private void _testClickingButtonAddsSkilllistBoxItemBelowIndex()
        {
            var handler = _fixture();
            var listBoxItems = new[] { new object(), new object(), new object() };
            _addSkillListBox.Items.Add(listBoxItems[0]);
            _addSkillListBox.Items.Add(listBoxItems[1]);
            _addSkillListBox.Items.Add(listBoxItems[2]);
            _addSkillListBox.SelectedIndex = 1;
            _addSkillButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_addSkillListBox.Items.Count == 4);
            Debug.Assert(_addSkillListBox.Items[0] == listBoxItems[0]);
            Debug.Assert(_addSkillListBox.Items[1] == listBoxItems[1]);
            Debug.Assert(_addSkillListBox.Items[2] is ListBoxItem);
            Debug.Assert(_addSkillListBox.Items[3] == listBoxItems[2]);
            Debug.Assert(_addSkillListBox.SelectedIndex == 2);
        }

        /**
         * @brief Tests that each new skill entry has the correct visual layout and styling
         * 
         * When the user adds a new skill to the list, the skill entry must display with
         * consistent formatting: a checkbox for enabling/disabling the skill, and a text box
         * for editing the skill's name. The layout must match the template defined in the
         * skills window's XAML to ensure visual consistency across all skill entries.
         */
        private void _testSkillListBoxItemProperties()
        {
            var handler = _fixture();
            _addSkillButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var listBoxItem = (ListBoxItem)_addSkillListBox.Items[0];
            Debug.Assert(listBoxItem.Content is StackPanel);
            var stackPanel = (StackPanel)listBoxItem.Content;
            var checkBox = stackPanel.Children.OfType<CheckBox>().First();
            var textBox = stackPanel.Children.OfType<TextBox>().First();
            var expected = StackPanelFixture.Fixture();
            var expectedCheckBox = expected.Children.OfType<CheckBox>().First();
            var expectedTextBox = expected.Children.OfType<TextBox>().First();
            Debug.Assert(stackPanel.Orientation == expected.Orientation);
            Debug.Assert(stackPanel.Focusable == expected.Focusable);
            Debug.Assert(checkBox.VerticalContentAlignment == expectedCheckBox.VerticalContentAlignment);
            Debug.Assert(textBox.Margin.Left == expectedTextBox.Margin.Left);
            Debug.Assert(textBox.Margin.Top == expectedTextBox.Margin.Top);
            Debug.Assert(textBox.Margin.Right == expectedTextBox.Margin.Right);
            Debug.Assert(textBox.Margin.Bottom == expectedTextBox.Margin.Bottom);
            Debug.Assert(textBox.VerticalContentAlignment == expectedTextBox.VerticalContentAlignment);
            Debug.Assert(textBox.HorizontalContentAlignment == expectedTextBox.HorizontalContentAlignment);
            Debug.Assert(textBox.Width == expectedTextBox.Width);
            Debug.Assert(textBox.Height == expectedTextBox.Height);
            Debug.Assert(((SolidColorBrush)textBox.Background).Color.R == ((SolidColorBrush)expectedTextBox.Background).Color.R);
            Debug.Assert(((SolidColorBrush)textBox.Background).Color.G == ((SolidColorBrush)expectedTextBox.Background).Color.G);
            Debug.Assert(((SolidColorBrush)textBox.Background).Color.B == ((SolidColorBrush)expectedTextBox.Background).Color.B);
            Debug.Assert(((SolidColorBrush)textBox.Background).Color.A == ((SolidColorBrush)expectedTextBox.Background).Color.A);
            Debug.Assert(((SolidColorBrush)textBox.Foreground).Color.R == ((SolidColorBrush)expectedTextBox.Foreground).Color.R);
            Debug.Assert(((SolidColorBrush)textBox.Foreground).Color.G == ((SolidColorBrush)expectedTextBox.Foreground).Color.G);
            Debug.Assert(((SolidColorBrush)textBox.Foreground).Color.B == ((SolidColorBrush)expectedTextBox.Foreground).Color.B);
            Debug.Assert(((SolidColorBrush)textBox.Foreground).Color.A == ((SolidColorBrush)expectedTextBox.Foreground).Color.A);
            Debug.Assert(textBox.FontFamily.ToString() == expectedTextBox.FontFamily.ToString());
        }

        /**
         * @brief Tests that new skill entries receive sequentially numbered default names
         * 
         * When the user adds multiple skills to the list, each new skill should be assigned
         * a unique default name ("skill 0", "skill 1", "skill 2", etc.) to help users
         * distinguish between different skills before they rename them. The numbering should
         * be sequential and deterministic based on the number of skills.
         */
        private void _testSkillListBoxItemText()
        {
            var handler = _fixture();
            for (int i = 0; i < 10; i++)
            {
                _addSkillButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                var addedListBox = (ListBoxItem)_addSkillListBox.SelectedItem;
                var addedStackPanel = (StackPanel)addedListBox.Content;
                var addedTextBox = addedStackPanel.Children.OfType<TextBox>().First();
                Debug.Assert(addedTextBox.Text == "skill " + i.ToString());
                Debug.Assert(addedListBox.Tag is Skill);
                var skill = (Skill)addedListBox.Tag;
                Debug.Assert(skill.Name == "skill " + i.ToString());
                _addSkillListBox.SelectedItem = new Random().Next(i + 1);
            }
        }

        public void Run()
        {
            _testClickingButtonAddsSkillListBoxItem();
            _testClickingButtonAddsSkillListBoxItemToEnd();
            _testClickingButtonAddsSkilllistBoxItemBelowIndex();
            _testSkillListBoxItemProperties();
            _testSkillListBoxItemText();
        }
    }


    public class WindowSkillsMenuRemoveSkillActionHandlerTests
    {
        private Button _removeSkillButton = new Button();

        private ListBox _skillsListBox = new ListBox();

        private AbstractWindowActionHandler _fixture()
        {
            _removeSkillButton = new Button();
            _skillsListBox = new ListBox();
            return new WindowSkillsMenuRemoveSkillActionHandlerFacade(
                _removeSkillButton,
                _skillsListBox
            );
        }

        /**
         * @brief Tests that clicking the Remove button does nothing and does not crash when
         * the list is empty
         * 
         * When the user clicks the Remove Skill button but there are no skills in the list,
         * the system should remain stable and unchanged. No errors should occur, no exceptions
         * should be thrown, and the list should stay empty.
         */
        private void _testClickingRemoveButtonDoesNothing()
        {
            var handler = _fixture();
            _removeSkillButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_skillsListBox.Items.Count == 0);
        }

        /**
         * @brief Tests that clicking the Remove button deletes the currently selected skill
         * 
         * When the user selects a skill in the list and clicks the Remove Skill button, the
         * bot should delete only that specific skill from the list, preserving all other
         * skills in their original order.
         */
        private void _testClickingRemoveButtonRemovesSelectedSkill()
        {
            var handler = _fixture();
            var listBoxItems = new[] { new object(), new object(), new object() };
            _skillsListBox.Items.Add(listBoxItems[0]);
            _skillsListBox.Items.Add(listBoxItems[1]);
            _skillsListBox.Items.Add(listBoxItems[2]);
            _skillsListBox.SelectedIndex = 1;
            _removeSkillButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_skillsListBox.Items.Count == 2);
            Debug.Assert(_skillsListBox.Items[0] == listBoxItems[0]);
            Debug.Assert(_skillsListBox.Items[1] == listBoxItems[2]);
            Debug.Assert(_skillsListBox.SelectedIndex == -1);
        }

        /**
         * @brief Tests that clicking the Remove button deletes the skill when no specific
         * selection is made but items exist in the list
         * 
         * When no skill is explicitly selected but the list contains items, clicking Remove
         * deletes the most recent or last skill in the list as a convenience behavior. This
         * test verifies that behavior.
         */
        private void _testClickingRemoveButtonRemovesLastSkill()
        {
            var handler = _fixture();
            var listBoxItems = new[] { new object(), new object(), new object() };
            _skillsListBox.Items.Add(listBoxItems[0]);
            _skillsListBox.Items.Add(listBoxItems[1]);
            _skillsListBox.Items.Add(listBoxItems[2]);
            _removeSkillButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_skillsListBox.Items.Count == 2);
            Debug.Assert(_skillsListBox.Items[0] == listBoxItems[0]);
            Debug.Assert(_skillsListBox.Items[1] == listBoxItems[1]);
            Debug.Assert(_skillsListBox.SelectedIndex == -1);
        }

        public void Run()
        {
            _testClickingRemoveButtonDoesNothing();
            _testClickingRemoveButtonRemovesSelectedSkill();
            _testClickingRemoveButtonRemovesLastSkill();
        }
    }


    public class WindowSkillsMenuMacroCommandAddActionHandlerTests
    {
        private Button _addMacroButton = new Button();

        private ListBox _macroListBox = new ListBox();

        private ComboBox _comboBoxTemplate = new ComboBox();

        private MockWindowActionHandlerRegistry _mockRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _addMacroButton = new Button();
            _macroListBox = new ListBox();
            _comboBoxTemplate = ComboBoxFixture.Fixture();
            _mockRegistry = new MockWindowActionHandlerRegistry();
            return new WindowSkillsMenuMacroCommandAddActionHandlerFacade(
                _addMacroButton,
                _comboBoxTemplate,
                _macroListBox,
                _mockRegistry
            );
        }

        /**
         * @brief Tests that clicking the Add Macro button creates a new macro command entry in
         * the list
         * 
         * When the user clicks the Add Macro button in the skills window, a new macro command
         * entry should appear in the list. Each macro command represents a key press sequence
         * that the bot will execute when using a skill (e.g., pressing a hotkey to activate
         * an ability). This allows users to chain multiple key presses for a single skill,
         * such as buff sequences or combo attacks.
         */
        private void _testClickingButtonAddsCommandListBoxItem()
        {
            var handler = _fixture();
            _addMacroButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_macroListBox.Items.Count == 1);
            Debug.Assert(_macroListBox.Items[0] is ListBoxItem);
            Debug.Assert(_macroListBox.SelectedIndex == 0);
        }

        /**
         * @brief Tests that new macro command entries are added at the bottom of the existing list
         * 
         * When the user already has multiple macro commands configured for a skill and clicks
         * the Add Macro button, the new command should appear after all existing entries. This
         * maintains the chronological order of macro creation, which is important for skills
         * that require commands to execute in a specific sequence (e.g., buff A before buff B).
         */
        private void _testClickingButtonAddsCommandListBoxItemToEnd()
        {
            var handler = _fixture();
            var listBoxItems = new[] { new object(), new object(), new object() };
            _macroListBox.Items.Add(listBoxItems[0]);
            _macroListBox.Items.Add(listBoxItems[1]);
            _macroListBox.Items.Add(listBoxItems[2]);
            _addMacroButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_macroListBox.Items.Count == 4);
            Debug.Assert(_macroListBox.Items[0] == listBoxItems[0]);
            Debug.Assert(_macroListBox.Items[1] == listBoxItems[1]);
            Debug.Assert(_macroListBox.Items[2] == listBoxItems[2]);
            Debug.Assert(_macroListBox.Items[3] is ListBoxItem);
            Debug.Assert(_macroListBox.SelectedIndex == 3);
        }

        /**
         * @brief Tests that new macro command entries are inserted below the currently selected
         * command
         * 
         * When the user has a macro command selected in the list and clicks Add Macro, the new
         * command should be inserted immediately after the selected one. This allows users to
         * organize their skill's macro sequence by adding new commands at specific positions
         * rather than always appending to the end, which is useful for inserting additional
         * actions into an existing sequence without having to reorder everything.
         */
        private void _testClickingButtonAddsCommandListBoxItemBelowIndex()
        {
            var handler = _fixture();
            var listBoxItems = new[] { new object(), new object(), new object() };
            _macroListBox.Items.Add(listBoxItems[0]);
            _macroListBox.Items.Add(listBoxItems[1]);
            _macroListBox.Items.Add(listBoxItems[2]);
            _macroListBox.SelectedIndex = 1;
            _addMacroButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_macroListBox.Items.Count == 4);
            Debug.Assert(_macroListBox.Items[0] == listBoxItems[0]);
            Debug.Assert(_macroListBox.Items[1] == listBoxItems[1]);
            Debug.Assert(_macroListBox.Items[2] is ListBoxItem);
            Debug.Assert(_macroListBox.Items[3] == listBoxItems[2]);
            Debug.Assert(_macroListBox.SelectedIndex == 2);
        }

        /**
         * @brief Tests that each new macro command entry has the correct combo box properties
         * 
         * Each macro command in the skills window is represented by a combo box that allows
         * users to select or type a key binding for that command. The combo box must have
         * consistent styling and content across all macro entries, including width, font size,
         * editable state, and the list of available key options.
         */
        private void _testMacroListBoxItemProperties()
        {
            var handler = _fixture();
            var expected = ComboBoxFixture.Fixture();
            _addMacroButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_macroListBox.Items.Count == 1);
            var listBoxItem = (ListBoxItem)_macroListBox.Items[0];
            var comboBox = (ComboBox)listBoxItem.Content;
            Debug.Assert(comboBox.Width == expected.Width);
            Debug.Assert(comboBox.IsEditable == expected.IsEditable);
            Debug.Assert(comboBox.FontSize == expected.FontSize);
            Debug.Assert(comboBox.Items.Count == expected.Items.Count);
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                var comboBoxItem = (ComboBoxItem)comboBox.Items[i];
                var expectedItem = (ComboBoxItem)expected.Items[i];
                Debug.Assert(comboBoxItem.Content == expectedItem.Content);
            }
        }

        public void Run()
        {
            _testClickingButtonAddsCommandListBoxItem();
            _testClickingButtonAddsCommandListBoxItemToEnd();
            _testClickingButtonAddsCommandListBoxItemBelowIndex();
            _testMacroListBoxItemProperties();
        }
    }


    public class WindowSkillsMenuMacroCommandRemoveActionHandlerTests
    {
        private Button _removeMacroButton = new Button();

        private ListBox _macroListBox = new ListBox();

        private MockWindowActionHandlerRegistry _mockRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _removeMacroButton = new Button();
            _macroListBox = new ListBox();
            _mockRegistry = new MockWindowActionHandlerRegistry();
            return new WindowSkillsMenuMacroCommandRemoveActionHandlerFacade(
                _removeMacroButton,
                _macroListBox,
                _mockRegistry
            );
        }

        /**
         * @brief Tests that clicking the Remove Macro button does nothing when the list is empty
         * 
         * When the user clicks the Remove Macro button but there are no macro commands in the
         * list, the system should remain stable and unchanged. No errors should occur, no
         * exceptions should be thrown, and the list should stay empty. This prevents the bot
         * from attempting to remove a non-existent item or accessing an invalid index when
         * the list has zero elements.
         */
        private void _testClickingRemoveButtonDoesNothing()
        {
            var handler = _fixture();
            _removeMacroButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_macroListBox.Items.Count == 0);
        }

        /**
         * @brief Tests that clicking the Remove Macro button deletes the currently selected
         * macro command
         * 
         * When the user selects a macro command in the skill's macro sequence list and clicks
         * the Remove Macro button, the bot should delete only that specific command from the
         * list, preserving all other macro commands in their original order. This allows users
         * to remove unwanted key presses from a skill's macro sequence without affecting the
         * rest of the sequence.
         */
        private void _testClickingRemoveButtonRemovesSelectedMacro()
        {
            var handler = _fixture();
            var listBoxItems = new[] {
                new ListBoxItem { Content = new ComboBox() },
                new ListBoxItem { Content = new ComboBox() },
                new ListBoxItem { Content = new ComboBox() },
            };
            _macroListBox.Items.Add(listBoxItems[0]);
            _macroListBox.Items.Add(listBoxItems[1]);
            _macroListBox.Items.Add(listBoxItems[2]);
            _macroListBox.SelectedIndex = 1;
            _removeMacroButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_macroListBox.Items.Count == 2);
            Debug.Assert(_macroListBox.Items[0] == listBoxItems[0]);
            Debug.Assert(_macroListBox.Items[1] == listBoxItems[2]);
            Debug.Assert(_macroListBox.SelectedIndex == -1);
        }

        /**
         * @brief Tests that clicking the Remove Macro button deletes the last macro command
         * when no selection is made
         * 
         * In some UI designs, when no macro command is explicitly selected but the list
         * contains items, clicking Remove may delete the most recent or last command in the
         * list as a convenience behavior. This test verifies that behavior, ensuring users
         * can quickly remove the last added macro command without having to manually select
         * it first.
         */
        private void _testClickingRemoveButtonRemovesLastMacro()
        {
            var handler = _fixture();
            var listBoxItems = new[] {
                new ListBoxItem { Content = new ComboBox() },
                new ListBoxItem { Content = new ComboBox() },
                new ListBoxItem { Content = new ComboBox() },
            };
            _macroListBox.Items.Add(listBoxItems[0]);
            _macroListBox.Items.Add(listBoxItems[1]);
            _macroListBox.Items.Add(listBoxItems[2]);
            _removeMacroButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_macroListBox.Items.Count == 2);
            Debug.Assert(_macroListBox.Items[0] == listBoxItems[0]);
            Debug.Assert(_macroListBox.Items[1] == listBoxItems[1]);
            Debug.Assert(_macroListBox.SelectedIndex == -1);
        }

        public void Run()
        {
            _testClickingRemoveButtonDoesNothing();
            _testClickingRemoveButtonRemovesSelectedMacro();
            _testClickingRemoveButtonRemovesLastMacro();
        }
    }


    public class WindowSkillsMenuSkillSelectedActionHandlerTests
    {
        private ListBox _skillsListBox = new ListBox();

        private ListBox _macroListBox = new ListBox();

        private TextBox _minDelay = new TextBox();

        private TextBox _maxDelay = new TextBox();

        private ComboBox _comboBoxTemplate = ComboBoxFixture.Fixture();

        private MockWindowActionHandlerRegistry _mockRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _skillsListBox = new ListBox();
            _macroListBox = new ListBox();
            _minDelay = new TextBox();
            _maxDelay = new TextBox();
            _comboBoxTemplate = ComboBoxFixture.Fixture();
            _mockRegistry = new MockWindowActionHandlerRegistry();
            return new WindowSkillsMenuSkillSelectedActionHandlerFacade(
                _skillsListBox,
                _macroListBox,
                _minDelay,
                _maxDelay,
                _comboBoxTemplate,
                _mockRegistry
            );
        }

        /**
         * @brief Tests that selecting a skill from the skills list populates all skill
         * configuration fields
         * 
         * When the user clicks on a skill in the skills list, the bot must display that skill's
         * configuration in the UI for editing. This includes populating the minimum delay field,
         * maximum delay field, and rebuilding the macro command list with the skill's saved macro
         * sequences (the key presses the bot will execute when using the skill).
         */
        private void _testSelectingSkillPopulatesSkillInfo()
        {
            var handler = _fixture();
            var skill = (Skill)SkillListBoxFixture.Fixture().Tag;
            _skillsListBox.Items.Add(SkillListBoxFixture.Fixture());
            _macroListBox.Items.Add(new ListBoxItem { Content = new ComboBox() });
            _skillsListBox.SelectedIndex = 0;
            Debug.Assert(_minDelay.Text == skill.MinDelay.ToString());
            Debug.Assert(_maxDelay.Text == skill.MaxDelay.ToString());
            Debug.Assert(_macroListBox.Items.Count == skill.Macros.Count);
            for (int i = 0; i < _macroListBox.Items.Count; i++)
            {
                Debug.Assert(_macroListBox.Items[i] is ListBoxItem);
                var listBoxItem = (ListBoxItem)_macroListBox.Items[i];
                Debug.Assert(listBoxItem.Content is ComboBox);
                var comboBox = (ComboBox)listBoxItem.Content;
                Debug.Assert(comboBox.Text == skill.Macros[i]);
            }
        }

        /**
         * @brief Tests that newly created macro command combo boxes have the correct visual
         * properties
         * 
         * When a skill is selected and its macro command list is rebuilt, each new macro command
         * entry is represented by a combo box that allows users to select or type a key binding
         * for that command. These combo boxes must have consistent styling across all entries,
         * including dimensions, font size, editable state, and the list of available key options.
         */
        private void _testSelectingSkillSetsComboBoxProperties()
        {
            var handler = _fixture();
            var expected = ComboBoxFixture.Fixture();
            var skill = (Skill)expected.Tag;
            _skillsListBox.Items.Add(SkillListBoxFixture.Fixture());
            _skillsListBox.SelectedIndex = 0;
            for (int i = 0; i < _macroListBox.Items.Count; i++)
            {
                var listBoxItem = (ListBoxItem)_macroListBox.Items[i];
                var comboBox = (ComboBox)listBoxItem.Content;
                Debug.Assert(comboBox.Width == expected.Width);
                Debug.Assert(comboBox.IsEditable == expected.IsEditable);
                Debug.Assert(comboBox.FontSize == expected.FontSize);
                Debug.Assert(comboBox.Items.Count == expected.Items.Count);
                for (int j = 0; j < comboBox.Items.Count; j++)
                {
                    var comboBoxItem = (ComboBoxItem)comboBox.Items[j];
                    var expectedItem = (ComboBoxItem)expected.Items[j];
                    Debug.Assert(comboBoxItem.Content == expectedItem.Content);
                }
            }
        }

        public void Run()
        {
            _testSelectingSkillPopulatesSkillInfo();
            _testSelectingSkillSetsComboBoxProperties();
        }
    }


    public class WindowSkillsMenuSkillDeselectedActionHandlerTests
    {
        private ListBox _skillsListBox = new ListBox();

        private ListBox _macroListBox = new ListBox();

        private TextBox _minDelay = new TextBox();

        private TextBox _maxDelay = new TextBox();

        private MockWindowActionHandlerRegistry _mockRegistry = new MockWindowActionHandlerRegistry();

        private AbstractWindowActionHandler _fixture()
        {
            _skillsListBox = new ListBox();
            _macroListBox = new ListBox();
            _minDelay = new TextBox();
            _maxDelay = new TextBox();
            _mockRegistry = new MockWindowActionHandlerRegistry();
            return new WindowSkillsMenuSkillDeselectedActionHandlerFacade(
                _skillsListBox,
                _macroListBox,
                _minDelay,
                _maxDelay,
                _mockRegistry
            );
        }

        /**
         * @brief Tests that deselecting a skill automatically saves all changes made to the
         * skill's configuration
         * 
         * When the user clicks away from a selected skill in the skills list (or the skill loses
         * selection for any reason), the bot must automatically save any changes the user made
         * to that skill's configuration. This includes the minimum delay, maximum delay, and the
         * entire macro command sequence (the key presses the bot executes when using the skill).
         */
        private void _testDeselectingSkillSavesSkillInfo()
        {
            var handler = _fixture();
            var listBoxItem = SkillListBoxFixture.Fixture();
            var macros = MacroListBoxFixture.Fixture();
            var skill = (Skill)listBoxItem.Tag;
            _skillsListBox.Items.Add(listBoxItem);
            _skillsListBox.SelectedIndex = 0;
            foreach (var macroListBoxItem in MacroListBoxFixture.Fixture())
            {
                _macroListBox.Items.Add(macroListBoxItem);
            }
            _minDelay.Text = "678";
            _maxDelay.Text = "789";
            _skillsListBox.SelectedIndex = -1;
            Debug.Assert(skill.MinDelay == 678);
            Debug.Assert(skill.MaxDelay == 789);
            Debug.Assert(skill.Macros.Count == macros.Count);
            for (int i = 0; i < macros.Count; i++)
            {
                Debug.Assert(skill.Macros[i] == ((ComboBox)macros[i].Content).Text);
            }
            Debug.Assert(_macroListBox.Items.Count == 0);

        }

        public void Run()
        {
            _testDeselectingSkillSavesSkillInfo();
        }
    }


    public class WindowSkillsMenuSkillSaveActionHandlerTests
    {
        private Button _saveButton = new Button();

        private ListBox _skillsListBox = new ListBox();

        private AbstractSkillsModel _skillsModel = new SkillsModel();

        private AbstractWindowActionHandler _fixture()
        {
            _saveButton = new Button();
            _skillsListBox = new ListBox();
            _skillsModel = new SkillsModel();
            var handler = new WindowSkillsMenuSkillSaveActionHandlerFacade(
                _saveButton, _skillsListBox
            );
            handler.Inject(SystemInjectType.SkillsModel, _skillsModel);
            return handler;
        }

        /**
         * @brief Tests that clicking the Save button persists all skill configurations to the
         * skills model
         * 
         * When the user clicks the Save button in the skills management window, the bot must
         * capture the current state of every skill in the list and save it to the SkillsModel.
         * For each skill, this includes:
         * 
         * - The skill name from the text box
         * - The active state (enabled/disabled) from the checkbox
         * - The minimum and maximum delay values stored in the skill's Tag
         * - The macro command sequence stored in the skill's Tag
         */
        private void _testClickingSaveButtonSavesAllSkills()
        {
            var handler = _fixture();
            var expected = SkillsModelFixture.Fixture().GetSkills();
            foreach (var skill in SkillListBoxFixture.FixtureList())
            {
                _skillsListBox.Items.Add(skill);
            }
            _saveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var skills = _skillsModel.GetSkills();
            for (int i = 0; i < expected.Count; i++)
            {
                Debug.Assert(skills[i].Name == expected[i].Name);
                Debug.Assert(skills[i].Active == expected[i].Active);
                Debug.Assert(skills[i].MinDelay == expected[i].MinDelay);
                Debug.Assert(skills[i].MaxDelay == expected[i].MaxDelay);
                Debug.Assert(skills[i].Macros.Count == expected[i].Macros.Count);
                for (int j = 0; j < expected[i].Macros.Count; j++)
                {
                    Debug.Assert(skills[i].Macros[j] == expected[i].Macros[j]);
                }
            }
        }

        /**
         * @brief Tests that clicking the Save button refreshes the selected skill's UI display
         * 
         * When the user clicks the Save button, the bot temporarily deselects and then reselects
         * the currently selected skill in the skills list. This refresh cycle triggers the
         * skill selection and deselection handlers, which in turn update the skill's displayed
         * macro command list and delay fields.
         */
        public void _testClickingSaveButtonRefreshesSelectedItem()
        {
            var handler = _fixture();
            var skills = SkillListBoxFixture.FixtureList();
            var eventArgs = new List<object>();
            foreach (var skill in skills)
            {
                _skillsListBox.Items.Add(skill);
            }
            _skillsListBox.SelectedIndex = 0;
            _skillsListBox.SelectionChanged += (_, __) => { eventArgs.Add(__); };
            _saveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(eventArgs.Count == 2);
            Debug.Assert(eventArgs[0] is SelectionChangedEventArgs);
            Debug.Assert(eventArgs[1] is SelectionChangedEventArgs);
            var deselectionEvent = (SelectionChangedEventArgs)eventArgs[0];
            var selectionEvent = (SelectionChangedEventArgs)eventArgs[1];
            Debug.Assert(deselectionEvent.AddedItems.Count == 0);
            Debug.Assert(deselectionEvent.RemovedItems.Count == 1);
            Debug.Assert(deselectionEvent.RemovedItems[0] == skills[0]);
            Debug.Assert(selectionEvent.AddedItems.Count == 1);
            Debug.Assert(selectionEvent.RemovedItems.Count == 0);
            Debug.Assert(selectionEvent.AddedItems[0] == skills[0]);
        }

        public void Run()
        {
            _testClickingSaveButtonSavesAllSkills();
            _testClickingSaveButtonRefreshesSelectedItem();
        }
    }


    public class WindowSkillsMenuSkillSaveConfigurationActionHandlerTests
    {
        private Button _saveButton = new Button();

        private MockSaveFileDialog _saveFileDialog = new MockSaveFileDialog();

        private AbstractSkillsModel _skillsModel = SkillsModelFixture.Fixture();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private AbstractWindowActionHandler _fixture()
        {
            _saveButton = new Button();
            _saveFileDialog = new MockSaveFileDialog();
            _skillsModel = SkillsModelFixture.Fixture();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                SkillsDirectory = "meow"
            };
            var handler = (
                new WindowSkillsMenuSkillSaveConfigurationActionHandlerFacade(
                    _saveButton, _saveFileDialog
                )
            );
            handler.Inject(SystemInjectType.SkillsModel, _skillsModel);
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            return handler;
        }

        /**
         * @brief Tests that clicking the Save button writes the current skill configuration
         * to a JSON file
         * 
         * When the user clicks the Save button in the skills management window, the bot
         * must prompt the user to choose a save location and then write all current skill configurations
         * to a JSON file. This allows users to export their skill loadouts for backup purposes,
         * sharing between different bot instances, or reusing across different characters.
         */
        private void _testClickingSaveButtonSavesConfigurationToFile()
        {
            var handler = _fixture();
            var normalizer = new JsonNormalizer();
            _saveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_saveFileDialog.PromptCalls == 1);
            Debug.Assert(_saveFileDialog.PromptCallArg_initialDirectory[0] == "meow");
            Debug.Assert(
                normalizer.Normalize(_saveFileDialog.PromptCallArg_saveContent[0]) ==
                normalizer.Normalize(SkillsModelFixture.Json())
            );
        }

        public void Run()
        {
            _testClickingSaveButtonSavesConfigurationToFile();
        }
    }


    public class WindowSkillsMenuSkillSavingActionHandlerTests
    {
        private MockSystemWindow _skillsWindow = new MockSystemWindow();

        private ListBox _skillsListBox = new ListBox();

        private AbstractSkillsModel _skillsModel = new SkillsModel();

        private AbstractWindowActionHandler _fixture()
        {
            _skillsWindow = new MockSystemWindow();
            _skillsListBox = new ListBox();
            _skillsModel = new SkillsModel();
            _skillsWindow.GetWindowReturn.Add(new Window());
            var handler = new WindowSkillsMenuSkillSavingActionHandlerFacade(
                _skillsWindow,
                _skillsListBox
            );
            handler.Inject(SystemInjectType.SkillsModel, _skillsModel);
            return handler;
        }

        /**
         * @brief Tests that closing the skills window automatically saves all skill
         * configurations
         * 
         * When the window becomes invisible (closing/hiding), all skills from the list box
         * are saved to the SkillsModel. This includes the skill name, active state, minimum
         * delay, maximum delay, and the complete macro command sequence for each skill. The
         * saved data must match the expected configuration.
         */
        private void _testClosingSkillsWindowSavesAllSkills()
        {
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture();
                var expected = SkillsModelFixture.Fixture().GetSkills();
                foreach (var skill in SkillListBoxFixture.FixtureList())
                {
                    _skillsListBox.Items.Add(skill);
                }
                _skillsWindow.VisibleReturn.Add(visible);
                handler.OnDependencyEvent(0, new DependencyPropertyChangedEventArgs());
                var skills = _skillsModel.GetSkills();
                if (!visible)
                {
                    Debug.Assert(skills.Count == expected.Count);
                    for (int i = 0; i < expected.Count; i++)
                    {
                        Debug.Assert(skills[i].Name == expected[i].Name);
                        Debug.Assert(skills[i].Active == expected[i].Active);
                        Debug.Assert(skills[i].MinDelay == expected[i].MinDelay);
                        Debug.Assert(skills[i].MaxDelay == expected[i].MaxDelay);
                        Debug.Assert(skills[i].Macros.Count == expected[i].Macros.Count);
                        for (int j = 0; j < expected[i].Macros.Count; j++)
                        {
                            Debug.Assert(skills[i].Macros[j] == expected[i].Macros[j]);
                        }
                    }
                }
                else
                {
                    Debug.Assert(skills.Count == 0);
                }
            }
        }

        /**
         * @brief Tests that the skills list refreshes the selected item when the window is closed
         * 
         * When the skills window is closed (becomes invisible), the bot must trigger a refresh
         * of the currently selected skill by temporarily deselecting and reselecting it. This
         * refresh cycle ensures that the UI state is properly updated and any pending changes
         * are committed before the window closes.
         */
        public void _testClosingSkillsWindowRefreshesSelectedItem()
        {
            foreach (var visible in new[] { true, false })
            {
                var handler = _fixture();
                var skills = SkillListBoxFixture.FixtureList();
                var eventArgs = new List<object>();
                foreach (var skill in skills)
                {
                    _skillsListBox.Items.Add(skill);
                }
                _skillsListBox.SelectedIndex = 0;
                _skillsListBox.SelectionChanged += (_, __) => { eventArgs.Add(__); };
                _skillsWindow.VisibleReturn.Add(visible);
                handler.OnDependencyEvent(0, new DependencyPropertyChangedEventArgs());
                if (!visible)
                {
                    Debug.Assert(eventArgs.Count == 2);
                    Debug.Assert(eventArgs[0] is SelectionChangedEventArgs);
                    Debug.Assert(eventArgs[1] is SelectionChangedEventArgs);
                    var deselectionEvent = (SelectionChangedEventArgs)eventArgs[0];
                    var selectionEvent = (SelectionChangedEventArgs)eventArgs[1];
                    Debug.Assert(deselectionEvent.AddedItems.Count == 0);
                    Debug.Assert(deselectionEvent.RemovedItems.Count == 1);
                    Debug.Assert(deselectionEvent.RemovedItems[0] == skills[0]);
                    Debug.Assert(selectionEvent.AddedItems.Count == 1);
                    Debug.Assert(selectionEvent.RemovedItems.Count == 0);
                    Debug.Assert(selectionEvent.AddedItems[0] == skills[0]);
                }
                else
                {
                    Debug.Assert(eventArgs.Count == 0);
                }
            }
        }

        public void Run()
        {
            _testClosingSkillsWindowSavesAllSkills();
            _testClosingSkillsWindowRefreshesSelectedItem();
        }
    }


    public class WindowSkillsMenuSkillLoadActionHandlerTests
    {
        private Button _loadButton = new Button();

        private MockLoadFileDialog _loadFileDialog = new MockLoadFileDialog();

        private AbstractWindowActionHandler _fixture()
        {
            _loadButton = new Button();
            _loadFileDialog = new MockLoadFileDialog();
            return new WindowSkillsMenuSkillLoadActionHandlerFacade(
                _loadButton, _loadFileDialog
            );
        }

        /**
         * @brief Tests that clicking the Load button opens a file dialog for selecting a
         * skill configuration
         * 
         * When the user clicks the Load button in the skills window, the bot must open a file
         * dialog that allows the user to browse for and select a saved skill configuration file.
         * The dialog should start in the configured skills directory ("cool_skills") so users
         * can quickly access their saved skill files without navigating through the file system
         * each time.
         */
        private void _testLoadButtonOpensDialog()
        {
            var handler = _fixture();
            handler.Inject(
                SystemInjectType.ConfigurationUpdate,
                new MaplestoryBotConfiguration { SkillsDirectory = "cool_skills" }
            );
            _loadButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Debug.Assert(_loadFileDialog.PromptCalls == 1);
            Debug.Assert(_loadFileDialog.PromptCallArg_initialDirectory[0] == "cool_skills");
        }

        public void Run()
        {
            _testLoadButtonOpensDialog();
        }
    }


    public class WindowSkillsMenuSkillLoadConfigurationActionHandlerTests
    {
        private ListBox _skillsListBox = new ListBox();

        private StackPanel _skillsTemplate = new StackPanel();

        private MockLoadFileDialog _loadFileDialog = new MockLoadFileDialog();

        private AbstractWindowActionHandler _fixture()
        {
            _skillsListBox = new ListBox();
            _skillsTemplate = StackPanelFixture.Fixture();
            _loadFileDialog = new MockLoadFileDialog();
            var handler =  (
                new WindowSkillsMenuSkillLoadConfigurationActionHandlerFacade(
                    _skillsListBox,
                    _skillsTemplate,
                    _loadFileDialog
                )
            );
            return handler;
        }

        /**
         * @brief Tests that loading a skill configuration file populates the list box with all saved skills
         * 
         * When the user selects a skill configuration JSON file through the load dialog, the bot
         * must parse the file and recreate each skill as a ListBoxItem in the skills list. Each
         * skill entry must display a checkbox for the active state (checked = active, unchecked = inactive)
         * and a text box for the skill name. The underlying Skill object stored in the ListBoxItem's
         * Tag must contain all configuration data including name, active state, min/max delays,
         * and macro command sequences.
         */
        private void _testLoadingFileLoadsSkillsIntoListBox()
        {
            var handler = _fixture();
            var expected = SkillsModelFixture.Fixture().GetSkills();
            _skillsListBox.Items.Add(new object());
            _loadFileDialog.InvokeFileLoaded("", SkillsModelFixture.Json());
            Debug.Assert(_skillsListBox.Items.Count == expected.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                var listBoxItem = (ListBoxItem)_skillsListBox.Items[i];
                var stackPanel = (StackPanel)listBoxItem.Content;
                var checkBox = stackPanel.Children.OfType<CheckBox>().First();
                var textBox = stackPanel.Children.OfType<TextBox>().First();
                var skill = (Skill)listBoxItem.Tag;
                var expectedSkill = expected[i];
                Debug.Assert(skill.Name == expectedSkill.Name);
                Debug.Assert(skill.Active == expectedSkill.Active);
                Debug.Assert(skill.MinDelay == expectedSkill.MinDelay);
                Debug.Assert(skill.MaxDelay == expectedSkill.MaxDelay);
                Debug.Assert(checkBox.IsChecked == (expectedSkill.Active != 0));
                Debug.Assert(textBox.Text == expectedSkill.Name);
                Debug.Assert(skill.Macros.Count == expectedSkill.Macros.Count);
                for (int j = 0; j < expectedSkill.Macros.Count; j++)
                {
                    Debug.Assert(skill.Macros[j] == expectedSkill.Macros[j]);
                }
            }
        }

        /**
         * @brief Tests that after loading skills, the first skill in the list becomes selected
         * 
         * When skills are loaded from a configuration file and populated into the skills list box,
         * the bot must automatically select the first skill in the list. This selection triggers
         * the skill selection handler, which populates the macro command list and delay fields
         * for that skill, making the UI ready for the user to view or edit the loaded skill's
         * properties immediately after loading.
         */
        private void _testLoadingFileSelectsFirstItem()
        {
            var handler = _fixture();
            _loadFileDialog.InvokeFileLoaded("", SkillsModelFixture.Json());
            Debug.Assert(_skillsListBox.SelectedIndex == 0);
        }

        /**
         * @brief Tests that each loaded skill list box item has correctly styled UI elements
         * 
         * When skills are loaded from a configuration file and ListBoxItems are created, each
         * item must follow the visual template defined for skill entries. This includes a
         * horizontal StackPanel containing a CheckBox and a TextBox with specific styling:
         * alignment, dimensions, colors, margins, and font settings.
         */
        private void _testLoadedListBoxItemProperties()
        {
            var handler = _fixture();
            var expectedPanel = StackPanelFixture.Fixture();
            var expectedCheckBox = expectedPanel.Children.OfType<CheckBox>().First();
            var expectedTextBox = expectedPanel.Children.OfType<TextBox>().First();
            var expected = SkillsModelFixture.Fixture().GetSkills();
            _loadFileDialog.InvokeFileLoaded("", SkillsModelFixture.Json());
            Debug.Assert(_skillsListBox.Items.Count == expected.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                var listBoxItem = (ListBoxItem)_skillsListBox.Items[i];
                Debug.Assert(listBoxItem.Content is StackPanel);
                var stackPanel = (StackPanel)listBoxItem.Content;
                var textBox = stackPanel.Children.OfType<TextBox>().First();
                var checkBox = stackPanel.Children.OfType<CheckBox>().First();
                Debug.Assert(stackPanel.Orientation == expectedPanel.Orientation);
                Debug.Assert(stackPanel.Focusable == expectedPanel.Focusable);
                Debug.Assert(checkBox.VerticalContentAlignment == expectedCheckBox.VerticalContentAlignment);
                Debug.Assert(textBox.Margin == expectedTextBox.Margin);
                Debug.Assert(textBox.VerticalContentAlignment == expectedTextBox.VerticalContentAlignment);
                Debug.Assert(textBox.HorizontalContentAlignment == expectedTextBox.HorizontalContentAlignment);
                Debug.Assert(textBox.Width == expectedTextBox.Width);
                Debug.Assert(textBox.Height == expectedTextBox.Height);
                Debug.Assert(((SolidColorBrush)textBox.Background).Color.R == ((SolidColorBrush)expectedTextBox.Background).Color.R);
                Debug.Assert(((SolidColorBrush)textBox.Background).Color.G == ((SolidColorBrush)expectedTextBox.Background).Color.G);
                Debug.Assert(((SolidColorBrush)textBox.Background).Color.B == ((SolidColorBrush)expectedTextBox.Background).Color.B);
                Debug.Assert(((SolidColorBrush)textBox.Background).Color.A == ((SolidColorBrush)expectedTextBox.Background).Color.A);
                Debug.Assert(((SolidColorBrush)textBox.Foreground).Color.R == ((SolidColorBrush)expectedTextBox.Foreground).Color.R);
                Debug.Assert(((SolidColorBrush)textBox.Foreground).Color.G == ((SolidColorBrush)expectedTextBox.Foreground).Color.G);
                Debug.Assert(((SolidColorBrush)textBox.Foreground).Color.B == ((SolidColorBrush)expectedTextBox.Foreground).Color.B);
                Debug.Assert(((SolidColorBrush)textBox.Foreground).Color.A == ((SolidColorBrush)expectedTextBox.Foreground).Color.A);
                Debug.Assert(textBox.FontFamily.ToString() == expectedTextBox.FontFamily.ToString());
            }
        }

        public void Run()
        {
            _testLoadingFileLoadsSkillsIntoListBox();
            _testLoadingFileSelectsFirstItem();
            _testLoadedListBoxItemProperties();
        }
    }


    public class WindowSkillsMenuAccessibilityActionHandlerTests
    {
        private ListBox _skillsListBox = new ListBox();

        private List<FrameworkElement> _accesibilityElements = [];

        private AbstractWindowActionHandler _fixture()
        {
            _skillsListBox = new ListBox();
            _accesibilityElements = [
                new FrameworkElement { IsEnabled = true },
                new FrameworkElement { IsEnabled = true },
                new FrameworkElement { IsEnabled = true }
            ];
            _skillsListBox.Items.Add(new object());
            return new WindowSkillsMenuAccessibilityActionHandlerFacade(
                _skillsListBox,
                _accesibilityElements
            );
        }

        /**
         * @brief Tests that accessibility elements (buttons, controls) enable/disable
         * correctly based on whether a skill is selected in the skills list
         * 
         * When the user is configuring skills for the bot's combat automation, certain UI
         * controls (such as Add Skill, Remove Skill, Edit Macro, etc.) should only be enabled
         * when a skill is actually selected. This prevents users from attempting to perform
         * actions on non-existent skills, which would cause confusion or errors.
         */
        private void _testSkillSelectionEditsElementAccesibility()
        {
            var handler = _fixture();
            foreach (var element in _accesibilityElements)
            {
                Debug.Assert(element.IsEnabled == false);
            }
            _skillsListBox.SelectedIndex = 0;
            foreach (var element in _accesibilityElements)
            {
                Debug.Assert(element.IsEnabled == true);
            }
            _skillsListBox.SelectedIndex = -1;
            foreach (var element in _accesibilityElements)
            {
                Debug.Assert(element.IsEnabled == false);
            }
        }

        public void Run()
        {
            _testSkillSelectionEditsElementAccesibility();
        }
    }


    public class WindowSkillsMenuTestSuite
    {
        public void Run()
        {
            new WindowSkillsMenuAddSkillActionHandlerTests().Run();
            new WindowSkillsMenuRemoveSkillActionHandlerTests().Run();
            new WindowSkillsMenuMacroCommandAddActionHandlerTests().Run();
            new WindowSkillsMenuMacroCommandRemoveActionHandlerTests().Run();
            new WindowSkillsMenuSkillSelectedActionHandlerTests().Run();
            new WindowSkillsMenuSkillDeselectedActionHandlerTests().Run();
            new WindowSkillsMenuSkillSaveActionHandlerTests().Run();
            new WindowSkillsMenuSkillSaveConfigurationActionHandlerTests().Run();
            new WindowSkillsMenuSkillSavingActionHandlerTests().Run();
            new WindowSkillsMenuSkillLoadActionHandlerTests().Run();
            new WindowSkillsMenuSkillLoadConfigurationActionHandlerTests().Run();
            new WindowSkillsMenuAccessibilityActionHandlerTests().Run();
        }
    }
}
