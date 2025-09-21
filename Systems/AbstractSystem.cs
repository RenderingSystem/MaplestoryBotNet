namespace MaplestoryBotNet.Systems
{
    public enum SystemInjectType
    {
        Configuration = 0,
        ConfigurationUpdate = 1,
        KeyboardDevice = 2,
        KeystrokeTransmitter = 3,
        MacroTranslator = 4,
        AgentData = 5
    }


    public abstract class AbstractSystem
    {
        public abstract void Initialize();

        public abstract void Start();

        public virtual void Update()
        {

        }

        public virtual void Inject(SystemInjectType dataType, object data)
        {

        }
    }


    public abstract class AbstractSystemBuilder
    {
        public abstract AbstractSystemBuilder WithArg(object arg);

        public abstract AbstractSystem Build();
    }


    public abstract class AbstractInjector
    {
        public abstract void Inject(SystemInjectType dataType, object data);
    }


    public class SystemInjector : AbstractInjector
    {
        List<AbstractSystem> _systems;

        public SystemInjector(List<AbstractSystem> systems)
        {
            _systems = systems;
        }

        public override void Inject(SystemInjectType dataType, object data)
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].Inject(dataType, data);
            }
        }
    }


    public class SubSystemInformation
    {
        public AbstractSystemBuilder SystemBuilder;

        public List<SubSystemInformation> BuildDependencies;

        public AbstractSystem? System;

        public int InitializationPriority;

        public int StartPriority;

        public int UpdatePriority;

        public SubSystemInformation(
            AbstractSystemBuilder systemBuilder,
            List<SubSystemInformation> dependencies,
            int initializationPriority,
            int startPriority,
            int updatePriority
        )
        {
            SystemBuilder = systemBuilder;
            BuildDependencies = dependencies;
            System = null;
            InitializationPriority = initializationPriority;
            StartPriority = startPriority;
            UpdatePriority = updatePriority;
        }
    }


    public abstract class AbstractSubSystemInfoList
    {
        public abstract List<SubSystemInformation> GetSubSystemInfo();
    }
}
