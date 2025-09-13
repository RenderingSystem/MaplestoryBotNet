namespace MaplestoryBotNet.Systems
{
    public enum SystemInjectType
    {
        KeyboardDevice = 0
    }


    public abstract class AbstractSystem
    {
        public abstract void InitializeSystem();

        public abstract void StartSystem();

        public virtual void UpdateSystem()
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
                var system = _systems[i];
                system.Inject(dataType, data);
            }
        }
    }
}
