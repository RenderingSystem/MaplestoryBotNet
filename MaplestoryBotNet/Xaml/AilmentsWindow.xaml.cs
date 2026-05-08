using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.ThreadingUtils;
using System.Windows;


namespace MaplestoryBotNet
{
    public partial class AilmentsWindow : Window
    {
        private AbstractSystemWindow? _systemWindow = null;

        public AilmentsWindow()
        {
            InitializeComponent();
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemHideActionHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }

        private AbstractWindowActionHandler _instantiateWindowAilmentsLoadActionHandler()
        {
            return new WindowAilmentsLoadActionHandlerFacade(
                AilmentsListBox,
                AilmentsCheckboxTemplate,
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateWindowAilmentsLoadImagesActionHandler()
        {
            return new WindowAilmentsLoadImagesActionHandlerFacade(
                AilmentsListBox,
                AilmentsImageGrid,
                GetSystemWindow()
            );
        }

        private AbstractWindowActionHandler _instantiateWindowAilmentsAnimationActionHandler()
        {
            return new WindowAilmentsAnimationActionHandlerFacade(
                AilmentsListBox,
                GetSystemWindow(),
                1.0 / 15.0
            );
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers()
        {
            return [
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateWindowAilmentsLoadActionHandler(),
                _instantiateWindowAilmentsLoadImagesActionHandler(),
                _instantiateWindowAilmentsAnimationActionHandler()
            ];
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
