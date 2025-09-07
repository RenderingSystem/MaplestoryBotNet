namespace MaplestoryBotNet.Systems
{
    public abstract class AbstractSystem
    {
        public abstract void InitializeSystem();

        public abstract void StartSystem();

        public virtual void Inject(List<object> data)
        {

        }
    }
}
