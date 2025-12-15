
using System.Windows;
using System.Windows.Input;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public abstract class AbstractMouseEventPositionExtractor
    {
        public abstract Point GetPosition(
            MouseEventArgs mouseEventArgs,
            IInputElement relativeTo
        );
    }


    public class MouseEventPositionExtractor : AbstractMouseEventPositionExtractor
    {
        public override Point GetPosition(
            MouseEventArgs mouseEventArgs,
            IInputElement relativeTo
        )
        {
            return mouseEventArgs.GetPosition(relativeTo);
        }
    }
}
