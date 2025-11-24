using MaplestoryBotNet.LibraryWrappers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class WindowComboBoxScaler : AbstractWindowStateModifier
    {
        private AbstractDpi _hostedMonitorDpi;

        private AbstractDpi _windowDpi;

        public WindowComboBoxScaler(
            AbstractDpi hostedMonitorDpi,
            AbstractDpi windowDpi
        )
        {
            _hostedMonitorDpi = hostedMonitorDpi;
            _windowDpi = windowDpi;
        }

        public override void Modify(object? value)
        {
            if (value is Popup popup)
            {
                var window = Window.GetWindow(popup);
                var monitorDpi = _hostedMonitorDpi.GetDpi(window);
                var windowDpi = _windowDpi.GetDpi(window);
                var adjustedDpiX = windowDpi.Item1 / monitorDpi.Item1;
                var adjustedDpiY = windowDpi.Item2 / monitorDpi.Item2;
                popup.LayoutTransform = new ScaleTransform(adjustedDpiX, adjustedDpiY);
            }
        }
    }


    public class WindowComboBoxScaleActionHandler : AbstractWindowActionHandler
    {
        private AbstractWindowStateModifier _comboBoxPopupScaler;

        private ComboBox _comboBox;

        public WindowComboBoxScaleActionHandler(
            ComboBox comboBox,
            AbstractWindowStateModifier comboBoxPopupScaler
        )
        {
            _comboBox = comboBox;
            _comboBox.DropDownOpened += OnEvent;
            _comboBoxPopupScaler = comboBoxPopupScaler;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            var popup = _comboBox.Template?.FindName("Popup", _comboBox) as Popup;
            if (popup != null)
            {
                _comboBoxPopupScaler.Modify(popup);
            }
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _comboBoxPopupScaler;
        }

        ~WindowComboBoxScaleActionHandler()
        {
            _comboBox.DropDownOpened -= OnEvent;
        }
    }


    public class WindowComboBoxScaleActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _windowComboBoxScaleActionHandler;

        public WindowComboBoxScaleActionHandlerFacade(ComboBox comboBox)
        {
            _windowComboBoxScaleActionHandler = new WindowComboBoxScaleActionHandler(
                comboBox,
                new WindowComboBoxScaler(
                    new HostedMonitorDpi(new WindowsUserLibrary()),
                    new WindowDpi()
                )
            );
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _windowComboBoxScaleActionHandler.OnEvent(sender, e);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _windowComboBoxScaleActionHandler.Modifier();
        }
    }


    public class WindowComboBoxScaleActionHandlerParameters
    {
        public ComboBox ScaleComboBox;

        public WindowComboBoxScaleActionHandlerParameters(
            ComboBox scaleComboBox
        )
        {
            ScaleComboBox = scaleComboBox;
        }
    }


    public class WindowComboBoxScaleActionHandlerRegistry : AbstractWindowActionHandlerRegistry
    {
        private List<AbstractWindowActionHandler> _registry = [];

        public override void RegisterHandler(object? args)
        {
            if (args is WindowComboBoxScaleActionHandlerParameters parameters)
            {
                _registry.Add(
                    new WindowComboBoxScaleActionHandlerFacade(parameters.ScaleComboBox)
                );
            }
        }

        public override void ClearHandlers()
        {
            _registry.Clear();
        }

        public override List<AbstractWindowActionHandler> GetHandlers()
        {
            return [.. _registry];
        }
    }
}

