using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace MaplestoryBotNet.Systems.ScreenCapture
{

    public abstract class AbstractScreenCaptureStore
    {
        public abstract void SetLatest(Image<Bgra32> image);

        public abstract Image<Bgra32>? GetLatest();
    }


    public class GameScreenCaptureStore : AbstractScreenCaptureStore
    {
        private object _imageLockObject = new object();

        private Image<Bgra32>? _image = null;

        public override void SetLatest(Image<Bgra32> image)
        {
            lock (_imageLockObject)
            {
                _image = image;
            }
        }

        public override Image<Bgra32>? GetLatest()
        {
            Image<Bgra32>? image = null;
            lock (_imageLockObject)
            {
                image = _image;
            }
            return image;
        }
    }


    public class GameScreenCaptureStoreThread : AbstractThread
    {
        private string _processName = "";

        private ReaderWriterLockSlim _processNameLock;

        private AbstractScreenCaptureOrchestrator _orchestrator;

        private AbstractScreenCaptureStore _store;

        protected string ProcessName
        {
            get
            {
                try
                {
                    _processNameLock.EnterReadLock();
                    return _processName;
                }
                finally
                {
                    _processNameLock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _processNameLock.EnterWriteLock();
                    _processName = value;
                }
                finally
                {
                    _processNameLock.ExitWriteLock();
                }
            }
        }

        public GameScreenCaptureStoreThread(
            AbstractScreenCaptureOrchestrator orchestrator,
            AbstractScreenCaptureStore store,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _processName = "";
            _processNameLock = new ReaderWriterLockSlim();
            _orchestrator = orchestrator;
            _store = store;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                var processName = ProcessName;
                if (processName != "")
                {
                    var nextImage = _orchestrator.Capture(processName);
                    if (nextImage != null)
                    {
                        _store.SetLatest(nextImage);
                    }
                }
            }
        }

        public override void Inject(SystemInjectType dataType, object? value)
        {
            if (dataType == SystemInjectType.Configuration
                && value is MaplestoryBotConfiguration configuration
            )
            {
                ProcessName = configuration.ProcessName;
            }
        }
    }


    public class GameScreenCaptureStoreThreadFactory : AbstractThreadFactory
    {
        private AbstractScreenCaptureOrchestrator _orchestrator;

        private AbstractScreenCaptureStore _store;

        public GameScreenCaptureStoreThreadFactory(
            AbstractScreenCaptureOrchestrator orchestrator,
            AbstractScreenCaptureStore store
        )
        {
            _orchestrator = orchestrator;
            _store = store;
        }

        public override AbstractThread CreateThread()
        {
            return new GameScreenCaptureStoreThread(
                _orchestrator, _store, new ThreadRunningState()
            );
        }
    }


    public class GameScreenCaptureStoreSystem : AbstractSystem
    {
        private AbstractThreadFactory _storeThreadFactory;

        private AbstractThread? _storeThread;

        public GameScreenCaptureStoreSystem(
            AbstractThreadFactory storeThreadFactory
        )
        {
            _storeThreadFactory = storeThreadFactory;
            _storeThread = null;
        }

        public override void Initialize()
        {
            _storeThread = _storeThreadFactory.CreateThread();
        }

        public override void Start()
        {
            if (_storeThread != null)
            {
                _storeThread.Start();
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (_storeThread != null)
            {
                _storeThread.Inject(dataType, data);
            }
        }
    }


    public abstract class AbstractScreenCapturePublisher : ISystemInjectable
    {
        public abstract void Publish(Image<Bgra32> image, bool updated);

        public abstract void NotifyComplete();

        public abstract void Inject(SystemInjectType dataType, object? data);
    }


    public abstract class AbstractScreenCapturePublisherCountDown
    {
        public abstract void SetCountDown(int countDown);

        public abstract void WaitCountDown();

        public abstract void CountDown();
    }


    public class GameScreenCapturePublisherCountDown : AbstractScreenCapturePublisherCountDown
    {
        CountdownEvent _countDownEvent = new CountdownEvent(1);

        public override void SetCountDown(int countDown)
        {
            _countDownEvent = new CountdownEvent(countDown);
        }

        public override void WaitCountDown()
        {
            _countDownEvent.Wait();
        }

        public override void CountDown()
        {
            _countDownEvent.Signal();
        }
    }


    public class GameScreenCapturePublisher : AbstractScreenCapturePublisher
    {
        private List<AbstractScreenCaptureSubscriber> _subscribers = [];

        
        private AbstractScreenCapturePublisherCountDown _countDown;

        public GameScreenCapturePublisher(
            List<AbstractScreenCaptureSubscriber> subscribers,
            AbstractScreenCapturePublisherCountDown countDown
        )
        {
            _subscribers = subscribers;
            _countDown = countDown;
        }

        public override void Publish(Image<Bgra32> image, bool updated)
        {
            _countDown.SetCountDown(_subscribers.Count);
            for (int i = 0; i < _subscribers.Count; i++)
            {
                _subscribers[i].Notify(image, updated);
            }
            _countDown.WaitCountDown();
        }

        public override void NotifyComplete()
        {
            _countDown.CountDown();
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
        }
    }


    public class GameScreenCapturePublisherThread : AbstractThread
    {
        private AbstractScreenCaptureStore _store;

        private AbstractScreenCapturePublisher _publisher;

        private Image<Bgra32>? _latestImage;

        private bool _imageChanged;

        private AbstractWindowStateModifier? _windowViewCheckbox;

        private ReaderWriterLockSlim _windowViewCheckboxLock;

        private AbstractWindowStateModifier? WindowViewCheckbox
        {
            get
            {
                try
                {
                    _windowViewCheckboxLock.EnterReadLock();
                    return _windowViewCheckbox;
                }
                finally
                {
                    _windowViewCheckboxLock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _windowViewCheckboxLock.EnterWriteLock();
                    _windowViewCheckbox = value;
                }
                finally
                {
                    _windowViewCheckboxLock.ExitWriteLock();
                }
            }
        }

        public GameScreenCapturePublisherThread(
            AbstractScreenCaptureStore store,
            AbstractScreenCapturePublisher publisher,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _store = store;
            _publisher = publisher;
            _latestImage = null;
            _imageChanged = false;
            _windowViewCheckbox = null;
            _windowViewCheckboxLock = new ReaderWriterLockSlim();
        }

        private void _update()
        {
            var latest = _store.GetLatest();
            _imageChanged = false;
            if (latest != null && latest != _latestImage)
            {
                _latestImage = latest;
                _imageChanged = true;
            }
        }

        private void _publish()
        {
            if (_latestImage != null)
            {
                var viewCheckbox = WindowViewCheckbox;
                if (viewCheckbox != null && (ViewTypes?)viewCheckbox.State(0) == ViewTypes.Snapshots)
                {
                    _publisher.Publish(_latestImage, _imageChanged);
                }
            }
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _update();
                _publish();
            }
        }

        public override void Inject(SystemInjectType dataType, object? value)
        {
            if (
                dataType == SystemInjectType.ActionHandler
                && value is WindowViewCheckboxActionHandler windowViewCheckbox
            )
            {
                WindowViewCheckbox = windowViewCheckbox.Modifier();
            }
        }
    }


    public class GameScreenCapturePublisherThreadFactory : AbstractThreadFactory
    {
        private AbstractScreenCaptureStore _store;

        private AbstractScreenCapturePublisher _publisher;

        public GameScreenCapturePublisherThreadFactory(
            AbstractScreenCaptureStore store,
            AbstractScreenCapturePublisher publisher
        )
        {
            _store = store;
            _publisher = publisher;
        }

        public override AbstractThread CreateThread()
        {
            return new GameScreenCapturePublisherThread(
                _store, _publisher, new ThreadRunningState()
            );
        }
    }


    public abstract class AbstractScreenCaptureSubscriber : ISystemInjectable
    {
        protected Image<Bgra32> _image;

        protected bool _updated;

        private SemaphoreSlim _semaphore;

        public AbstractScreenCaptureSubscriber(SemaphoreSlim semaphore)
        {
            _image = new Image<Bgra32>(1, 1);
            _updated = false;
            _semaphore = semaphore;
        }

        public virtual void Notify(
            Image<Bgra32> image, bool updated
        )
        {
            _image = image;
            _updated = updated;
            _semaphore.Release();
        }

        public virtual void WaitForNotification()
        {
            _semaphore.Wait();
        }

        public virtual void Inject(SystemInjectType dataType, object? data)
        {

        }

        public abstract void ProcessImage();
    }


    public class NullScreenCaptureSubscriber : AbstractScreenCaptureSubscriber
    {
        public NullScreenCaptureSubscriber(SemaphoreSlim semaphore) : base(semaphore)
        {

        }

        public override void ProcessImage()
        {
            
        }
    }


    public class GameScreenCaptureSubscriber : AbstractScreenCaptureSubscriber
    {
        AbstractWindowStateModifier? _viewModifier;

        ReaderWriterLockSlim _viewModifierLock;

        public GameScreenCaptureSubscriber(
            SemaphoreSlim semaphore
        ) : base(semaphore)
        {
            _viewModifier = null;
            _viewModifierLock = new ReaderWriterLockSlim();
        }

        public override void ProcessImage()
        {
            AbstractWindowStateModifier? viewModifier = null;
            try
            {
                _viewModifierLock.EnterReadLock();
                viewModifier = _viewModifier;
            }
            finally
            {
                _viewModifierLock.ExitReadLock();
            }
            if (viewModifier != null)
            {
                viewModifier.Modify(_image);
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ActionHandler
                && data is WindowViewUpdaterActionHandler viewModifier
            )
            {
                try
                {
                    _viewModifierLock.EnterWriteLock();
                    _viewModifier = viewModifier.Modifier();
                }
                finally
                {
                    _viewModifierLock.ExitWriteLock();
                }
            }
        }
    }


    public class GameScreenCaptureSubscriberThread : AbstractThread
    {
        private AbstractScreenCaptureSubscriber _subscriber;

        private AbstractScreenCapturePublisher _publisher;

        public GameScreenCaptureSubscriberThread(
            AbstractScreenCaptureSubscriber subscriber,
            AbstractScreenCapturePublisher publisher,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _subscriber = subscriber;
            _publisher = publisher;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _subscriber.WaitForNotification();
                _subscriber.ProcessImage();
                _publisher.NotifyComplete();
            }
        }

        public override void Inject(SystemInjectType dataType, object? value)
        {
            _subscriber.Inject(dataType, value);
            _publisher.Inject(dataType, value);
        }
    }


    public class GameScreenCaptureSubscriberThreadFactory : AbstractThreadFactory
    {
        private AbstractScreenCaptureSubscriber _subscriber;

        private AbstractScreenCapturePublisher _publisher;

        public GameScreenCaptureSubscriberThreadFactory(
            AbstractScreenCaptureSubscriber subscriber,
            AbstractScreenCapturePublisher publisher
        )
        {
            _subscriber = subscriber;
            _publisher = publisher;
        }

        public override AbstractThread CreateThread()
        {
            return new GameScreenCaptureSubscriberThread(
                _subscriber, _publisher, new ThreadRunningState()
            );
        }
    }


    public class GameScreenCapturePublisherSystem : AbstractSystem
    {
        private AbstractThreadFactory _publisherThreadFactory;

        private AbstractThread? _publisherThread;

        public GameScreenCapturePublisherSystem(
            AbstractThreadFactory publisherThreadFactory
        )
        {
            _publisherThreadFactory = publisherThreadFactory;
            _publisherThread = null;
        }

        public override void Initialize()
        {
			_publisherThread = _publisherThreadFactory.CreateThread();
        }

        public override void Start()
        {
            if (_publisherThread != null)
            {
                _publisherThread.Start();
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (_publisherThread != null)
            {
                _publisherThread.Inject(dataType, data);
            }
        }
    }


    public class GameScreenCaptureSubscriberSystem : AbstractSystem
    {
        private List<AbstractThreadFactory> _subscriberThreadFactories;

        private List<AbstractThread> _subscriberThreads;

        public GameScreenCaptureSubscriberSystem(
            List<AbstractThreadFactory> subscriberThreadFactories
        )
        {
            _subscriberThreadFactories = subscriberThreadFactories;
            _subscriberThreads = [];
        }

        public override void Initialize()
        {
            for (int i = 0; i < _subscriberThreadFactories.Count; i++)
            {
                var subscriberFactory = _subscriberThreadFactories[i];
                var subscriberThread = subscriberFactory.CreateThread();
                _subscriberThreads.Add(subscriberThread);
            }
        }

        public override void Start()
        {
            for (int i = 0; i < _subscriberThreads.Count; i++)
            {
                var subscriberThread = _subscriberThreads[i];
                subscriberThread.Start();
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            for (int i = 0; i < _subscriberThreads.Count; i++)
            {
                var subscriberThread = _subscriberThreads[i];
                subscriberThread.Inject(dataType, data);
            }
        }
    }


    public class GameScreenCaptureSystem : AbstractSystem
    {
        private List<AbstractSystem> _subSystems;

        public GameScreenCaptureSystem(
            List<AbstractSystem> subSystems
        )
        {
            _subSystems = subSystems;
        }

        public override void Initialize()
        {
            for (int i = 0; i < _subSystems.Count; i++)
            {
                var subSystem = _subSystems[i];
                subSystem.Initialize();
            }
        }

        public override void Start()
        {
            for (int i = 0; i < _subSystems.Count; i++)
            {
                var subSystem = _subSystems[i];
                subSystem.Start();
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            for (int i = 0; i < _subSystems.Count; i++)
            {
                var subSystem = _subSystems[i];
                subSystem.Inject(dataType, data);
            }
        }
    }


    public class GameScreenCaptureSystemBuilder : AbstractSystemBuilder
    {
        private List<AbstractScreenCaptureSubscriber> _subscribers = [];

        private AbstractScreenCaptureStore _store()
        {
            return new GameScreenCaptureStore();
        }

        private AbstractSystem _storeSystem(AbstractScreenCaptureStore store)
        {
            return new GameScreenCaptureStoreSystem(
                new GameScreenCaptureStoreThreadFactory(
                    new ScreenCaptureOrchestratorFacade(), store
                )
            );
        }

        private AbstractThreadFactory _subscriberThreadFactory(
            AbstractScreenCapturePublisher publisher,
            AbstractScreenCaptureSubscriber subscriber
        )
        {
            return new GameScreenCaptureSubscriberThreadFactory(
                subscriber, publisher
            );
        }

        private List<AbstractThreadFactory> _subscriberThreadFactories(
            AbstractScreenCapturePublisher publisher,
            List<AbstractScreenCaptureSubscriber> subscribers
        )
        {
            var subscriberThreadFactories = new List<AbstractThreadFactory>();
            for (int i = 0; i < subscribers.Count; i++)
            {
                var subscriber = subscribers[i];
                var subscriberThreadFactory = _subscriberThreadFactory(publisher, subscriber);
                subscriberThreadFactories.Add(subscriberThreadFactory);
            }
            return subscriberThreadFactories;
        }

        private AbstractScreenCapturePublisher _publisher(
            List<AbstractScreenCaptureSubscriber> subscribers
        )
        {
            return new GameScreenCapturePublisher(
                subscribers, new GameScreenCapturePublisherCountDown()
            );
        }

        private AbstractThreadFactory _publisherThreadFactory(
            AbstractScreenCaptureStore store,
            AbstractScreenCapturePublisher publisher
        )
        {
            return new GameScreenCapturePublisherThreadFactory(store, publisher);
        }

        private AbstractSystem _publisherSystem(
            AbstractThreadFactory publisherThreadFactory
        )
        {
            return new GameScreenCapturePublisherSystem(publisherThreadFactory);
        }

        private AbstractSystem _subscriberSystem(
            List<AbstractThreadFactory> subscriberThreadFactories
        )
        {
            return new GameScreenCaptureSubscriberSystem(subscriberThreadFactories);
        }

        public override AbstractSystem Build()
        {
            var store = _store();
            var storeSystem = _storeSystem(store);
            var publisher = _publisher(_subscribers);
            var publisherThreadFactory = _publisherThreadFactory(store, publisher);
            var subscriberThreadFactories = _subscriberThreadFactories(publisher, _subscribers);
            var publisherSystem = _publisherSystem(publisherThreadFactory);
            var subscriberSystem = _subscriberSystem(subscriberThreadFactories);
            return new GameScreenCaptureSystem([storeSystem, publisherSystem, subscriberSystem]);
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            if (arg is AbstractScreenCaptureSubscriber)
                _subscribers.Add((AbstractScreenCaptureSubscriber)arg);
            return this;
        }
    }

}
