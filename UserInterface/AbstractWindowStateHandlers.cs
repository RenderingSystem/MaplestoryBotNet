
using System.Windows;


namespace MaplestoryBotNet.UserInterface
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


    public abstract class AbstractWindowActionHandler
    {
        public virtual void OnEvent(object? sender, EventArgs e)
        {

        }

        public abstract AbstractWindowStateModifier Modifier();
    }


    public abstract class AbstractWindowActionHandlerBuilder
    {
        public abstract AbstractWindowActionHandlerBuilder WithArgs(object? args);

        public abstract AbstractWindowActionHandler Build();
    }

}

