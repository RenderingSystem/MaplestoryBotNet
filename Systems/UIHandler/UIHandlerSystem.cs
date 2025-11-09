using MaplestoryBotNet.Systems.UIHandler.UserInterface;


namespace MaplestoryBotNet.Systems.UIHandler
{
    class UIHandlerSystem : AbstractSystem
    {
        private List<AbstractWindowActionHandler> _handlers = [];

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (
                dataType == SystemInjectType.ActionHandler
                && data is AbstractWindowActionHandler handler
            )
            {
                _handlers.Add(handler);
            }
            else
            {
                for (int i = 0; i < _handlers.Count; i++)
                {
                    _handlers[i].Inject(dataType, data);
                }
            }
        }
    }


    public class UIHandlerSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new UIHandlerSystem();
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
