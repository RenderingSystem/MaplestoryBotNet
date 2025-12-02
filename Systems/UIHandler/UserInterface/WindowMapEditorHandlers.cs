using System.Windows.Controls;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class WindowMapEditMenuModifier : AbstractWindowStateModifier
    {
        private AbstractSystemWindow _editWindow;

        public WindowMapEditMenuModifier(AbstractSystemWindow editWindow)
        {
            _editWindow = editWindow;
        }

        public override void Modify(object? value)
        {
            if (value is bool show)
            {
                if (show)
                {
                    _editWindow.Show();
                }
                else
                {
                    _editWindow.Hide();
                }
            }
        }
    }


    public class WindowMapEditMenuActionHandler : AbstractWindowActionHandler
    {
        private Button _editButton;

        private AbstractWindowStateModifier _editWindowModifier;

        public WindowMapEditMenuActionHandler(
            Button editButton,
            AbstractWindowStateModifier editWindowModifier
        )
        {
            _editButton = editButton;
            _editWindowModifier = editWindowModifier;
            _editButton.Click += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _editWindowModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _editWindowModifier.Modify(true);
        }
    }


    public class WindowMapEditMenuActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _editMenuActionHandler;

        public WindowMapEditMenuActionHandlerFacade(
            Button editButton, AbstractSystemWindow editWindow
        )
        {
            _editMenuActionHandler = new WindowMapEditMenuActionHandler(
                editButton, new WindowMapEditMenuModifier(editWindow)
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _editMenuActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _editMenuActionHandler.OnEvent(sender, e);
        }
    }
}
