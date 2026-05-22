using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Macro;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace MaplestoryBotNet.Systems.Consumables
{
    public enum PotionThreadType
    {
        Resource = 0,
        Consumable,
        MaxNum
    }


    public class ConsumptionThreadContext
    {
        public List<Consumable> Consumables = [];

        public List<AbstractTimestamp> Stopwatches = [];

        public List<double> Timestamps = [];

        public Queue<int> Next = [];
    }


    public abstract class AbstractConsumptionThreadRefresher : IDataInjectable
    {
        public abstract void Refresh();

        public abstract void Inject(object dataType, object? data);
    }


    public abstract class AbstractConsumptionQueueUpdater : IDataInjectable
    {
        public abstract void Update();

        public abstract void Inject(object dataType, object? data);
    }


    public abstract class AbstractConsumptionExecutor : IDataInjectable
    {
        public abstract bool Execute();

        public abstract void Inject(object dataType, object? data);
    }


    public abstract class AbstractResourceDetectionThreshold
    {
        public abstract int Threshold(Resource resource, Image<Bgra32> image);
    }


    public abstract class AbstractConsumptionThreadHelper : IDataInjectable
    {
        public abstract void Run();

        public abstract void Inject(object dataType, object? data);
    }


    public class ResourceStatusScreenCaptureSubscriber : AbstractScreenCaptureSubscriber
    {
        AbstractThread? _resourceDetectionThread;

        public ResourceStatusScreenCaptureSubscriber(
            SemaphoreSlim semaphore
        ) : base(semaphore)
        {
            _resourceDetectionThread = null;
        }

        public override void ProcessImage()
        {
            _resourceDetectionThread?.Inject(0, _image);
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ThreadDependency &&
                data is AbstractThread resourceDetectionThread &&
                resourceDetectionThread.State() is PotionThreadType.Resource
            )
            {
                _resourceDetectionThread = resourceDetectionThread;
            }
        }
    }


    public class ResourceDetectionThreshold : AbstractResourceDetectionThreshold
    {
        public override int Threshold(Resource resource, Image<Bgra32> image)
        {
            var x = resource.Pixel[0] + resource.Rect[0];
            var y = resource.Pixel[1] + resource.Rect[1];
            return (
                (
                    x >= 0 &&
                    x < image.Width &&
                    y >= 0 &&
                    y < image.Height &&
                    image[x, y] is Bgra32 pixel
                ) &&
                (
                    pixel.R < (resource.Rgb[0] - resource.Tolerance[0]) ||
                    pixel.R > (resource.Rgb[0] + resource.Tolerance[0]) ||
                    pixel.G < (resource.Rgb[1] - resource.Tolerance[1]) ||
                    pixel.G > (resource.Rgb[1] + resource.Tolerance[1]) ||
                    pixel.B < (resource.Rgb[2] - resource.Tolerance[2]) ||
                    pixel.B > (resource.Rgb[2] + resource.Tolerance[2])
                )
            ) ? 1 : 0;
        }
    }


    public class ResourceDetectionThread : AbstractThread
    {
        private AbstractResetEvent _resetEvent;

        private AbstractResourceDetectionThreshold _resourceThreshold;

        private Image<Bgra32>? _image;

        private Resource? _hpResource;

        private Resource? _mpResource;

        private AbstractThread? _consumptionThread;

        public ResourceDetectionThread(
            AbstractResetEvent resetEvent,
            AbstractResourceDetectionThreshold resourceThreshold,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _resetEvent = resetEvent;
            _resourceThreshold = resourceThreshold;
            _image = null;
            _hpResource = null;
            _mpResource = null;
            _consumptionThread = null;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _resetEvent.WaitOne();
                if (
                    _image is Image<Bgra32> currentImage &&
                    _hpResource is Resource hpResource &&
                    _mpResource is Resource mpResource &&
                    _consumptionThread is AbstractThread consumptionThread
                )
                {
                    var hpThreshold = _resourceThreshold.Threshold(hpResource, currentImage);
                    var mpThreshold = _resourceThreshold.Threshold(mpResource, currentImage);
                    consumptionThread.Inject(PotionResourceType.Health, hpThreshold);
                    consumptionThread.Inject(PotionResourceType.Mana, mpThreshold);
                }
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (value is Image<Bgra32> image)
            {
                _image = image;
                _resetEvent.Set();
            }
            else if (
                dataType is SystemInjectType.InjectAction &&
                value is AbstractInjectAction injectAction
            )
            {
                injectAction.GetAction()(SystemInjectType.ThreadDependency, this);
            }
            else if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                value is MaplestoryBotConfiguration configuration
            )
            {
                _hpResource = (Resource)configuration.Hp.Copy();
                _mpResource = (Resource)configuration.Mp.Copy();
            }
            else if (
                dataType is SystemInjectType.ThreadDependency &&
                value is AbstractThread consumptionThread &&
                consumptionThread.State() is PotionThreadType.Consumable
            )
            {
                _consumptionThread = consumptionThread;
            }
        }

        public override object? State()
        {
            return PotionThreadType.Resource;
        }
    }


    public class ResourceDetectionThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            return new ResourceDetectionThread(
                new ExecutionEvent(),
                new ResourceDetectionThreshold(),
                new ThreadRunningState()
            );
        }
    }


    public class ConsumptionThreadRefresher : AbstractConsumptionThreadRefresher
    {
        private object _consumableData;

        private bool _refreshed;

        private AbstractMacroRandom _macroRandom;

        private AbstractTimestampFactory _consumableStopwatchFactory;

        private ConsumptionThreadContext _context;

        public ConsumptionThreadRefresher(
            ConsumptionThreadContext context,
            AbstractMacroRandom macroRandom,
            AbstractTimestampFactory consumableStopwatchFactory
        )
        {
            _consumableData = new
            {
                PotionFrequency = 0.0,
                Consumables = new List<Consumable>()
            };
            _refreshed = false;
            _context = context;
            _macroRandom = macroRandom;
            _consumableStopwatchFactory = consumableStopwatchFactory;
        }

        public override void Refresh()
        {
            if (!_refreshed)
            {
                return;
            }
            var consumableData = _consumableData;
            var potionFrequency = ((dynamic)consumableData).PotionFrequency;
            var consumables = (List<Consumable>)((dynamic)consumableData).Consumables;
            _context.Consumables = consumables.Where(c => c.Active != 0).ToList();
            _context.Stopwatches.Clear();
            _context.Timestamps.Clear();
            _context.Next.Clear();
            foreach (var consumable in _context.Consumables)
            {
                var consumableTimestamp = _consumableStopwatchFactory.Create();
                consumableTimestamp.SetTimestamp();
                _context.Stopwatches.Add(consumableTimestamp);
                var min = Math.Min(consumable.MinDelay, consumable.MaxDelay);
                var max = Math.Max(consumable.MinDelay, consumable.MaxDelay);
                _context.Timestamps.Add(_macroRandom.Next(min * 1000, max * 1000) / 1000.0);
            }
            _refreshed = false;
        }

        public override void Inject(object dataType, object? value)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                value is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                var consumables = new List<Consumable>();
                var potionFrequency = (
                    maplestoryBotConfiguration.MacroSettings.PotionFrequency
                );
                foreach (var consumable in maplestoryBotConfiguration.Consumables)
                {
                    consumables.Add((Consumable)consumable.Copy());
                }
                _consumableData = new
                {
                    PotionFrequency = potionFrequency,
                    Consumables = consumables
                };
                _refreshed = true;
            }
        }
    }


    public class ConsumptionQueueUpdater : AbstractConsumptionQueueUpdater
    {
        private ConsumptionThreadContext _context;

        public ConsumptionQueueUpdater(ConsumptionThreadContext context)
        {
            _context = context;
        }

        public override void Update()
        {
            for (int i = 0; i < _context.Stopwatches.Count; i++)
            {
                if (
                    _context.Stopwatches[i].GetTimestamp() > _context.Timestamps[i] &&
                    _context.Next.Contains(i) == false
                )
                {
                    _context.Next.Enqueue(i);
                }
            }
        }

        public override void Inject(object dataType, object? data)
        {

        }
    }


    public class ResourceExecutor : AbstractConsumptionExecutor
    {
        private PotionResourceType _resourceType;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        private bool _threshold;


        private Resource? _resource;

        public ResourceExecutor(
            PotionResourceType resourceType,
            ConsumptionThreadContext context,
            AbstractMacroCommandsExecutorBuilder macroCommandsExecutorBuilder,
            AbstractMacroRandom macroRandom
        )
        {
            _resourceType = resourceType;
            _macroCommandsExecutorBuilder = macroCommandsExecutorBuilder;
            _threshold = false;
            _resource = null;
        }

        public override bool Execute()
        {
            if (
                _threshold &&
                _resource is Resource resource &&
                resource.Active != 0
            )
            {
                _macroCommandsExecutor?.Execute(
                    ["key press {" + resource.Key + "} {50} {150}"]
                );
                return true;
            }
            return false;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.KeystrokeTransmitter &&
                data is AbstractKeystrokeTransmitter keystrokeTransmitter
            )
            {
                _macroCommandsExecutor = (
                    _macroCommandsExecutorBuilder
                        .WithArg(keystrokeTransmitter)
                        .Build()
                );
            }
            else if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration healthConfiguration &&
                _resourceType is PotionResourceType.Health
            )
            {
                _resource = (Resource)healthConfiguration.Hp.Copy();
            }
            else if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                data is MaplestoryBotConfiguration manaConfiguration &&
                _resourceType is PotionResourceType.Mana
            )
            {
                _resource = (Resource)manaConfiguration.Mp.Copy();
            }
            else if (
                dataType is PotionResourceType resourceType &&
                _resourceType == resourceType &&
                data is int threshold
            )
            {
                _threshold = threshold != 0;
            }
        }
    }


    public class ConsumptionExecutor : AbstractConsumptionExecutor
    {
        private ConsumptionThreadContext _context;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        private AbstractMacroRandom _macroRandom;

        public ConsumptionExecutor(
            ConsumptionThreadContext context,
            AbstractMacroCommandsExecutorBuilder macroCommandsExecutorBuilder,
            AbstractMacroRandom macroRandom
        )
        {
            _context = context;
            _macroCommandsExecutorBuilder = macroCommandsExecutorBuilder;
            _macroRandom = macroRandom;
        }

        public override bool Execute()
        {
            if (_context.Next.Count > 0)
            {
                var consumptionIndex = _context.Next.Dequeue();
                var dequeuedConsumable = _context.Consumables[consumptionIndex];
                var nextKey = dequeuedConsumable.Key;
                _macroCommandsExecutor?.Execute(["key press {" + nextKey + "} {50} {150}"]);
                _context.Stopwatches[consumptionIndex].SetTimestamp();
                var min = Math.Min(dequeuedConsumable.MinDelay, dequeuedConsumable.MaxDelay);
                var max = Math.Max(dequeuedConsumable.MinDelay, dequeuedConsumable.MaxDelay);
                _context.Timestamps[consumptionIndex] = _macroRandom.Next(min * 1000, max * 1000) / 1000.0;
                return true;
            }
            return false;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.KeystrokeTransmitter &&
                data is AbstractKeystrokeTransmitter keystrokeTransmitter
            )
            {
                _macroCommandsExecutor = (
                    _macroCommandsExecutorBuilder
                        .WithArg(keystrokeTransmitter)
                        .Build()
                );
            }
        }
    }


    public class ConsumptionThreadHelper : AbstractConsumptionThreadHelper
    {
        private AbstractConsumptionThreadRefresher _contextRefresher;

        private AbstractConsumptionQueueUpdater _consumptionQueueUpdater;

        private List<AbstractConsumptionExecutor> _consumptionChain;

        public ConsumptionThreadHelper(
            AbstractConsumptionThreadRefresher contextRefresher,
            AbstractConsumptionQueueUpdater consumptionQueueUpdater,
            List<AbstractConsumptionExecutor> consumptionChain
        )
        {
            _contextRefresher = contextRefresher;
            _consumptionQueueUpdater = consumptionQueueUpdater;
            _consumptionChain = consumptionChain;
        }

        public override void Run()
        {
            _contextRefresher.Refresh();
            _consumptionQueueUpdater.Update();
            foreach (var executor in _consumptionChain)
            {
                if (executor.Execute())
                {
                    break;
                }
            }
        }

        public override void Inject(object dataType, object? data)
        {
            _contextRefresher.Inject(dataType, data);
            _consumptionQueueUpdater.Inject(dataType, data);
            foreach (var executor in _consumptionChain)
            {
                executor.Inject(dataType, data);
            }
        }
    }


    public class ConsumptionThread : AbstractThread
    {
        private AbstractTimestamp _consumableTimestamp;

        private AbstractMacroSleeper _macroSleeper;

        private AbstractConsumptionThreadHelper _consumptionThreadHelper;

        private double _potionPeriod = 0.0;

        private MacroExecutorStateTypes _macroState;

        public ConsumptionThread(
            AbstractTimestamp consumableTimestamp,
            AbstractMacroSleeper macroSleeper,
            AbstractConsumptionThreadHelper consumptionThreadHelper,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _consumableTimestamp = consumableTimestamp;
            _macroSleeper = macroSleeper;
            _consumptionThreadHelper = consumptionThreadHelper;
            _macroState = MacroExecutorStateTypes.Idle;
        }

        public override void ThreadLoop()
        {
            while(_runningState.IsRunning())
            {
                _consumableTimestamp.SetTimestamp();
                if (
                    _macroState != MacroExecutorStateTypes.Login &&
                    _macroState != MacroExecutorStateTypes.Idle &&
                    _macroState != MacroExecutorStateTypes.Reset
                )
                {
                    _consumptionThreadHelper.Run();
                }
                var elapsed = _consumableTimestamp.GetTimestamp();
                _macroSleeper.Sleep((int)((_potionPeriod - elapsed) * 1000));
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate &&
                value is MaplestoryBotConfiguration configuration
            )
            {
                var potionFrequency = configuration.MacroSettings.PotionFrequency;
                _potionPeriod = potionFrequency > 0.000001 ? 1 / potionFrequency : 0;
            }
            else if (
                dataType is SystemInjectType.InjectAction &&
                value is AbstractInjectAction injectAction
            )
            {
                injectAction.GetAction()(SystemInjectType.ThreadDependency, this);
            }
            else if (dataType is MacroExecutorStateTypes macroState)
            {
                _macroState = macroState;
            }
            _consumptionThreadHelper.Inject(dataType, value);
        }

        public override object? State()
        {
            return PotionThreadType.Consumable;
        }
    }


    public class ConsumptionThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            var context = new ConsumptionThreadContext();
            return new ConsumptionThread(
                new StopwatchTimestamp(),
                new MacroSleeper(),
                new ConsumptionThreadHelper(
                    new ConsumptionThreadRefresher(
                        context,
                        new MacroRandom(),
                        new StopwatchTimestampFactory()
                    ),
                    new ConsumptionQueueUpdater(context),
                    [
                        new ResourceExecutor(
                            PotionResourceType.Health,
                            context,
                            new MacroCommandsExecutorBuilder(),
                            new MacroRandom()
                        ),
                        new ResourceExecutor(
                            PotionResourceType.Mana,
                            context,
                            new MacroCommandsExecutorBuilder(),
                            new MacroRandom()
                        ),
                        new ConsumptionExecutor(
                            context,
                            new MacroCommandsExecutorBuilder(),
                            new MacroRandom()
                        )
                    ]
                ),
                new ThreadRunningState()
            );
        }
    }


    public class ConsumptionSystem : AbstractSystem
    {
        List<AbstractThreadFactory> _consumptionThreadFactories;

        List<AbstractThread> _consumptionThreads;

        public ConsumptionSystem(
            List<AbstractThreadFactory> consumptionThreadFactories
        )
        {
            _consumptionThreadFactories = consumptionThreadFactories;
            _consumptionThreads = [];
        }

        public override void Initialize()
        {
            foreach (var factory in _consumptionThreadFactories)
            {
                _consumptionThreads.Add(factory.CreateThread());
            }
        }

        public override void Start()
        {
            foreach (var thread in _consumptionThreads)
            {
                thread.Start();
            }
        }

        public override void Inject(object dataType, object? data)
        {
            foreach (var thread in _consumptionThreads)
            {
                thread.Inject(dataType, data);
            }
        }
    }


    public class ConsumptionSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new ConsumptionSystem(
                [
                    new ConsumptionThreadFactory(),
                    new ResourceDetectionThreadFactory()
                ]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
