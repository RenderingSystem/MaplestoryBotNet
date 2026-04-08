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

        public override ComboBox Create()
        {
            var newComboBox = new ComboBox
            {
                Width = _template.Width,
                IsEditable = _template.IsEditable,
                FontSize = _template.FontSize,
            };
            foreach (ComboBoxItem originalItem in _template.Items)
            {
                var newItem = new ComboBoxItem { Content = originalItem.Content };
                newComboBox.Items.Add(newItem);
            }
            return newComboBox;
        }
    }
}
