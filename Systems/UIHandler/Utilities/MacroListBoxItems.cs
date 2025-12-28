using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public abstract class AbstractMacroListBoxItemTemplateFactory
    {
        public abstract FrameworkElement Create();
    }


    public abstract class AbstractMacroListBoxItemFormatter
    {
        public abstract void Format(object parameters);
    }


    public abstract class AbstractMacroListBoxItemIndexer
    {
        public abstract int GetIndex(GeneratorPosition position);
    }


    public class WindowMacroListBoxItemTemplateFactory : AbstractMacroListBoxItemTemplateFactory
    {
        private FrameworkElement _pointMacroTemplate;

        public WindowMacroListBoxItemTemplateFactory(
            FrameworkElement pointMacroTemplate
        )
        {
            _pointMacroTemplate = pointMacroTemplate;
        }

        private Grid _generateTemplateGrid()
        {
            return new Grid
            {
                Height = _pointMacroTemplate.Height,
                Width = _pointMacroTemplate.Width,
            };
        }

        private TextBox _generateTemplate(TextBox template)
        {
            return new TextBox
            {
                Tag = template.Tag,
                Text = template.Text,
                FontFamily = template.FontFamily,
                HorizontalAlignment = template.HorizontalAlignment,
                VerticalContentAlignment = template.VerticalContentAlignment,
                HorizontalContentAlignment = template.HorizontalContentAlignment,
                Width = template.Width,
                Height = template.Height,
                FontSize = template.FontSize,
                Background = template.Background,
                Foreground = template.Foreground,
                IsTabStop = template.IsTabStop,
                Margin = template.Margin
            };
        }

        public override FrameworkElement Create()
        {
            var newGrid = _generateTemplateGrid();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(_pointMacroTemplate); i++)
            {
                var child = VisualTreeHelper.GetChild(_pointMacroTemplate, i);
                if (child is TextBox childTextBox)
                {
                    var templateName = _generateTemplate(childTextBox);
                    newGrid.Children.Add(templateName);
                }
            }
            return newGrid;
        }
    }


    public class WindowMacroListBoxItemFormatterParameters
    {
        public FrameworkElement Element = new FrameworkElement();

        public List<string> ElementStrings = [];
    }


    public class WindowMacroListBoxItemFormatter : AbstractMacroListBoxItemFormatter
    {

        public override void Format(object parameters)
        {
            if (parameters is not WindowMacroListBoxItemFormatterParameters formatParameters)
            {
                return;
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(formatParameters.Element); i++)
            {
                if (i >= formatParameters.ElementStrings.Count)
                {
                    break;
                }
                var child = VisualTreeHelper.GetChild(formatParameters.Element, i);
                if (child is TextBox childTextBox)
                {
                    childTextBox.Text = formatParameters.ElementStrings[i];
                }
            }
        }
    }


    public class MacroListBoxItemIndexer : AbstractMacroListBoxItemIndexer
    {
        private ListBox _listBox;

        public MacroListBoxItemIndexer(ListBox listBox)
        {
            _listBox = listBox;
        }

        public override int GetIndex(GeneratorPosition position)
        {
            var iContainerGenerator = (
                (IItemContainerGenerator)_listBox.ItemContainerGenerator
            );
            return iContainerGenerator.IndexFromGeneratorPosition(position);
        }
    }
}
