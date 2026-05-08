using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.ThreadingUtils;


namespace MaplestoryBotNet.Systems.ScreenProcessing.SubSystems
{
    /*
    public class AilmentProcessorThreadState
    {

    }


    public abstract class AbstractAilmentProcessHandler
    {
        public abstract void Handle(AilmentProcessorThreadState threadState);
    }


    public class AilmentProcessHandler : AbstractAilmentProcessHandler
    {
        private AbstractTimestamp _timestamp;

        private AbstractScreenPositionProcessor _positionProcessor;

        public AilmentProcessHandler(
            AbstractTimestamp timestamp,
            AbstractScreenPositionProcessor positionProcessor
        )
        {
            _timestamp = timestamp;
            _positionProcessor = positionProcessor;
        }

        public override void Handle(
            AilmentProcessorThreadState currentThreadState
        )
        {
            var frequency = currentThreadState.MapIcon.Frequency;
            var period = frequency > 1e-8 ? (1.0 / frequency) : 0.0;
            if (_timestamp.GetTimestamp() <= period)
            {
                return;
            }
            _timestamp.SetTimestamp();
            var parameters = new WindowMinimapPositionModifierParameters
            {
                Model = currentThreadState.BottingModel.GetMapModel(),
                Position = (
                    _positionProcessor.Process(
                        currentThreadState.TemplateMatcher,
                        currentThreadState.RectangleMerger,
                        currentThreadState.Threshold.Value,
                        currentThreadState.MapIcon.Overlap,
                        currentThreadState.CurrentBitmap
                    ) ?? new Tuple<int, int>(-1, -1)
                )
            };
            currentThreadState.PositionUpdater.Modify(parameters);
        }
    }
    */
}
