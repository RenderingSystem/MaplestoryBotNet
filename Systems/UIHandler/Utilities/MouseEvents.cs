
using System.Windows;
using System.Windows.Input;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public abstract class AbstractMouseEventDataExtractor
    {
        public abstract Point GetPosition(
            MouseEventArgs mouseEventArgs,
            IInputElement relativeTo
        );

        public abstract MouseButtonState GetButtonState(
            MouseButtonState buttonState
        );
    }


    public class MouseEventDataExtractor : AbstractMouseEventDataExtractor
    {
        public override Point GetPosition(
            MouseEventArgs mouseEventArgs,
            IInputElement relativeTo
        )
        {
            return mouseEventArgs.GetPosition(relativeTo);
        }

        public override MouseButtonState GetButtonState(
            MouseButtonState buttonState
        )
        {
            return buttonState;
        }
    }
}
