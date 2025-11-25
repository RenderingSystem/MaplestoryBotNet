using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows.Controls;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public abstract class AbstractComboBoxFactory
    {
        public abstract ComboBox Create();
    }


    public class ComboBoxTemplateFactory : AbstractComboBoxFactory
    {
        private ComboBox _template;
        public ComboBoxTemplateFactory(ComboBox template)
        {
            _template = template;
        }

        private List<ComboBoxItem> _comboBoxItems()
        {
            return _template.Items
                .Cast<ComboBoxItem>()
                .Select(item => new ComboBoxItem { Content = item.Content })
                .ToList();
        }

        public override ComboBox Create()
        {
            return new ComboBox
            {
                Width = _template.Width,
                IsEditable = _template.IsEditable,
                FontSize = _template.FontSize,
                ItemsSource = _comboBoxItems()
            };
        }
    }
}
