
using System.Windows;
using System.Windows.Input;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public abstract class AbstractMouseEventPositionExtractor
    {
        public abstract Point GetPosition(
            MouseButtonEventArgs mouseButtonEvent,
            IInputElement relativeTo
        );
    }


    public class MouseEventPositionExtractor : AbstractMouseEventPositionExtractor
    {
        public override Point GetPosition(
            MouseButtonEventArgs mouseButtonEvent,
            IInputElement relativeTo
        )
        {
            return mouseButtonEvent.GetPosition(relativeTo);
        }
    }
}
