
using MaplestoryBotNet.Systems;
using System.Windows;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public enum ViewTypes
    {
        Snapshots = 0,
        Minimap,
        NCC,
        ViewTypesMaxNum
    }


    public abstract class AbstractWindowStateModifier
    {
        public virtual void Initialize()
        {

        }

        public abstract void Modify(object? value);

        public virtual object? State(int stateType)
        {
            return null;
        }
    }


    public class NullWindowStateModifier : AbstractWindowStateModifier
    {
        public NullWindowStateModifier()
        {
        }

        public override void Modify(object? value)
        {
        }
    }


    public abstract class AbstractWindowActionHandler : ISystemInjectable
    {
        public virtual void OnEvent(object? sender, EventArgs e)
        {

        }

        public virtual void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {

        }

        public abstract AbstractWindowStateModifier Modifier();

        public virtual void Inject(SystemInjectType dataType, object? data)
        {

        }
    }


    public abstract class AbstractWindowActionHandlerBuilder
    {
        public abstract AbstractWindowActionHandlerBuilder WithArgs(object? args);

        public abstract AbstractWindowActionHandler Build();
    }


    public abstract class AbstractWindowActionHandlerRegistry
    {
        public abstract void RegisterHandler(object? args);

        public abstract void UnregisterHandler(object? args);

        public abstract void ClearHandlers();

        public abstract List<AbstractWindowActionHandler> GetHandlers();
    }
}

