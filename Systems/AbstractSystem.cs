using MaplestoryBotNet.UserInterface;

namespace MaplestoryBotNet.Systems
{
    public enum SystemInjectType
    {
        Configuration = 0,
        ConfigurationUpdate,
        KeyboardDevice,
        KeystrokeTransmitter,
        MacroTranslator,
        AgentData,
        ViewModifier,
        ViewCheckbox,
        ShutDown,
        SystemInjectTypeMaxNum
    }


    public abstract class AbstractSystem
    {
        public abstract void Initialize();

        public abstract void Start();

        public virtual void Update()
        {

        }

        public virtual void Inject(SystemInjectType dataType, object? data)
        {

        }

        public virtual object? State()
        {
            return null;
        }
    }


    public abstract class AbstractSystemBuilder
    {
        public abstract AbstractSystemBuilder WithArg(object arg);

        public abstract AbstractSystem Build();
    }


    public abstract class AbstractInjector
    {
        public abstract void Inject(SystemInjectType dataType, object? data);
    }


    public abstract class AbstractApplication
    {
        public abstract void Launch(List<string> args);

        public abstract void ShutDown();

        public abstract AbstractSystem System();
    }


    public class SystemInjector : AbstractInjector
    {
        List<AbstractSystem> _systems;

        public SystemInjector(List<AbstractSystem> systems)
        {
            _systems = systems;
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].Inject(dataType, data);
            }
        }
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

        public abstract AbstractApplicationInitializerBuilder WithViewUpdaterActionHandler(AbstractWindowActionHandler handler);

        public abstract AbstractApplicationInitializerBuilder WithViewCheckboxActionHandler(AbstractWindowActionHandler handler);

        public abstract AbstractApplicationInitializerBuilder WithApplication(AbstractApplication application);
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
}
