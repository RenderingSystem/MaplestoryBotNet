using MaplestoryBotNet.Systems.ScreenProcessing.SubSystems;
using MaplestoryBotNet.ThreadingUtils;


namespace MaplestoryBotNet.Systems.ScreenProcessing
{
    public class ScreenProcessingSystem : AbstractSystem
    {
        private List<AbstractThreadFactory> _processingThreadFactories;

        private List<AbstractThread> _processingThreads;

        public ScreenProcessingSystem(
            List<AbstractThreadFactory> processingThreadFactories
        )
        {
            _processingThreadFactories = processingThreadFactories;
            _processingThreads = [];
        }

        public override void Initialize()
        {
            for (int i = 0; i < _processingThreadFactories.Count; i++)
            {
                var processingThreadFactory = _processingThreadFactories[i];
                var processingThread = processingThreadFactory.CreateThread();
                _processingThreads.Add(processingThread);
            }
        }

        public override void Start()
        {
            for (int i = 0; i < _processingThreads.Count; i++)
            {
                var subscriberThread = _processingThreads[i];
                subscriberThread.Start();
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (
                dataType == SystemInjectType.InjectAction
                && data is Action<SystemInjectType, object> injectAction
            )
            {
                for (int i = 0; i < _processingThreads.Count; i++)
                {
                    injectAction(
                        SystemInjectType.ThreadDependency,
                        _processingThreads[i]
                    );
                }
            }
            else
            {
                for (int i = 0; i < _processingThreads.Count; i++)
                {
                    var subscriberThread = _processingThreads[i];
                    subscriberThread.Inject(dataType, data);
                }
            }
        }
    }


    public class ScreenProcessingSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new ScreenProcessingSystem(
                [
                    new GameMinimapCharacterProcessorThreadFactory(),
                    new GameMinimapRuneProcessorThreadFactory()
                ]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
