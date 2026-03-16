using ArrayFireNCC;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.ThreadingUtils;


namespace MaplestoryBotNet.Systems.GPUSelector
{
    public abstract class AbstractGPUSelection
    {
        public abstract void SetSelection(int selection);

        public abstract int GetSelection();
    }


    public class GPUSelection : AbstractGPUSelection
    {
        private volatile int _selection;

        protected int Selection
        {
            set => _selection = value;

            get => _selection;
        }

        public GPUSelection()
        {
            _selection = 0;
        }

        public override void SetSelection(int selection)
        {
            Selection = selection;
        }

        public override int GetSelection()
        {
            return Selection;
        }
    }


    public class GPUSelectorThread : AbstractThread
    {
        AbstractAcceleratedDeviceSelectionSystem _deviceSelectionSystem;

        AbstractGPUSelection _gpuSelection;

        private volatile AbstractWindowStateModifier? __modifier;

        private AbstractWindowStateModifier? _modifier
        {
            set => __modifier = value;

            get => __modifier;
        }

        public GPUSelectorThread(
            AbstractThreadRunningState runningState,
            AbstractAcceleratedDeviceSelectionSystem deviceSelectionSystem,
            AbstractGPUSelection gpuSelection
        ) : base(runningState)
        {
            _deviceSelectionSystem = deviceSelectionSystem;
            _gpuSelection = gpuSelection;
            _modifier = null;
        }

        public override void ThreadLoop()
        {
            _deviceSelectionSystem.context_select();
            var selected = _deviceSelectionSystem.context_selected();
            _gpuSelection.SetSelection(selected);
            while (_runningState.IsRunning())
            {
                var modifier = _modifier;
                if (modifier != null)
                {
                    modifier.Modify(_gpuSelection);
                    return;
                }
            }
        }

        public override void Inject(SystemInjectType dataType, object? value)
        {
            if (
                dataType == SystemInjectType.ActionHandler
                && value is WindowSplashScreenCompleteActionHandler handler
            )
            {
                _modifier = handler.Modifier();
            }
        }
    }


    public class GPUSelectorThreadFactory : AbstractThreadFactory
    {
        private AbstractGPUSelection _gpuSelection;

        public GPUSelectorThreadFactory(
            AbstractGPUSelection gpuSelection
        )
        {
            _gpuSelection = gpuSelection;
        }

        public override AbstractThread CreateThread()
        {
            return new GPUSelectorThread(
                new ThreadRunningState(),
                new ArrayFireDeviceSelectionSystemFacade(),
                _gpuSelection
            );
        }
    }


    public class GPUSelectorSystem : AbstractSystem
    {
        AbstractThread? _gpuSelectorThread;

        AbstractThreadFactory _gpuSelectorThreadFactory;

        public GPUSelectorSystem(
            AbstractThreadFactory gpuSelectorThreadFactory
        )
        {
            _gpuSelectorThread = null;
            _gpuSelectorThreadFactory = gpuSelectorThreadFactory;
        }

        public override void Initialize()
        {
            _gpuSelectorThread = _gpuSelectorThreadFactory.CreateThread();
        }

        public override void Start()
        {
            if (_gpuSelectorThread != null)
            {
                _gpuSelectorThread.Start();
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (_gpuSelectorThread != null)
            {
                _gpuSelectorThread.Inject(dataType, data);
            }
        }
    }


    public class GPUSelectorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new GPUSelectorSystem(
                new GPUSelectorThreadFactory(new GPUSelection())
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
