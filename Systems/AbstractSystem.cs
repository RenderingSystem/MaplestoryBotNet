using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.Windows;
using System.Windows.Threading;

namespace MaplestoryBotNet.Systems
{
    public enum SystemInjectType
    {
        Configuration = 0,
        ConfigurationUpdate,
        KeyboardMapping,
        KeyboardDevice,
        KeystrokeTransmitter,
        MacroTranslator,
        AgentData,
        ShutDown,
        ActionHandler,
        SystemInjectTypeMaxNum,
    }

    public interface ISystemInjectable
    {
        public abstract void Inject(SystemInjectType dataType, object? data);
    }


    public abstract class AbstractSystem : ISystemInjectable
    {
        public virtual void Initialize()
        {

        }

        public virtual void Start()
        {

        }

        public virtual void Update()
        {

        }

        public virtual object? State()
        {
            return null;
        }

        public virtual void Inject(SystemInjectType dataType, object? data)
        {

        }
    }

    public class SystemInjector : ISystemInjectable
    {
        List<AbstractSystem> _systems;

        public SystemInjector(List<AbstractSystem> systems)
        {
            _systems = systems;
        }

        public void Inject(SystemInjectType dataType, object? data)
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].Inject(dataType, data);
            }
        }
    }


    public abstract class AbstractSystemWindow
    {
        protected Window? _window;

        protected bool _shutDownFlag = false;

        public bool ShutdownFlag
        {
            get
            {
                return _shutDownFlag;
            }
            set
            {
                _shutDownFlag = value;
            }
        }

        public virtual void Show()
        {
            _window?.Show();
        }

        public virtual void Hide()
        {
            _window?.Hide();
        }

        public virtual void Close()
        {
            if (ShutdownFlag)
            {
                _window?.Close();
            }
            else
            {
                _window?.Hide();
            }
        }

        public virtual object? GetWindow()
        {
            return _window;
        }
    }


    public class SystemWindow : AbstractSystemWindow
    {
        public SystemWindow(Window window)
        {
            _window = window;
        }
    }
    

    public abstract class AbstractSystemBuilder
    {
        public abstract AbstractSystemBuilder WithArg(object arg);

        public abstract AbstractSystem Build();
    }


    public abstract class AbstractApplication
    {
        public abstract void Launch(List<string> args);

        public abstract void ShutDown();

        public abstract AbstractSystem System();
    }


    public abstract class AbstractSubSystemInfoList
    {
        public abstract List<SystemInformation> GetSubSystemInfo();
    }


    public abstract class AbstractApplicationInitializer
    {
        public abstract void Synchronize();

        public abstract void Initialize();
    }


    public abstract class AbstractApplicationInitializerBuilder
    {
        public abstract AbstractApplicationInitializer Build();

        public abstract AbstractApplicationInitializerBuilder WithArgs(object args);
    }


    public abstract class AbstractDispatcher
    {
        public abstract void Dispatch(Action action);
    }


    public class SystemInformation
    {
        public AbstractSystemBuilder SystemBuilder;

        public List<SystemInformation> BuildDependencies;

        public List<object> BuildObjects;

        public AbstractSystem? System;

        public int InitializationPriority;

        public int StartPriority;

        public int UpdatePriority;

        public SystemInformation(
            AbstractSystemBuilder systemBuilder,
            List<SystemInformation> dependencies,
            List<object> objects,
            int initializationPriority,
            int startPriority,
            int updatePriority
        )
        {
            SystemBuilder = systemBuilder;
            BuildDependencies = dependencies;
            BuildObjects = objects;
            System = null;
            InitializationPriority = initializationPriority;
            StartPriority = startPriority;
            UpdatePriority = updatePriority;
        }
    }


    public class SystemAsyncDispatcher : AbstractDispatcher
    {
        private Dispatcher _dispatcher;

        private DispatcherPriority _priority;

        public SystemAsyncDispatcher(
            Dispatcher dispatcher,
            DispatcherPriority priority
        )
        {
            _dispatcher = dispatcher;
            _priority = priority;
        }

        public override void Dispatch(Action action)
        {
            _dispatcher.BeginInvoke(action, _priority);
        }
    }
}
