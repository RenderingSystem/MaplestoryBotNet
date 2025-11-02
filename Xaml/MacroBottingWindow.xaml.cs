using MaplestoryBotNet.Systems;
using MaplestoryBotNet.UserInterface;
using System.Windows;


namespace MaplestoryBotNet.Xaml
{
    /// <summary>
    /// Interaction logic for MacroBottingWindow.xaml
    /// </summary>
    public partial class MacroBottingWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        public MacroBottingWindow()
        {
            InitializeComponent();
        }

        public AbstractWindowActionHandler InstantiateWindowMenuItemHideHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }
        
        public AbstractSystemWindow GetSystemWindow()
        {
            if (_systemWindow == null)
            {
                _systemWindow = new SystemWindow(this);
            }
            return _systemWindow;
        }
    }
}
