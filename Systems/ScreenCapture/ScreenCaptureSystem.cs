using MaplestoryBotNet.ThreadingUtils;
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
        private string _processName;

        private AbstractScreenCaptureOrchestrator _orchestrator;

        private AbstractScreenCaptureStore _store;

        public GameScreenCaptureStoreThread(
            string processName,
            AbstractScreenCaptureOrchestrator orchestrator,
            AbstractScreenCaptureStore store,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _processName = processName;
            _orchestrator = orchestrator;
            _store = store;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                var nextImage = _orchestrator.Capture(_processName);
                if (nextImage != null)
                    _store.SetLatest(nextImage);
            }
        }
    }


    public abstract class AbstractScreenCapturePublisher
    {
        public abstract void Publish(Image<Bgra32> image, bool updated);

        public abstract void NotifyComplete();
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
    }


    public class GameScreenCapturePublisherThread : AbstractThread
    {
        private AbstractScreenCaptureStore _store;

        private AbstractScreenCapturePublisher _publisher;

        private Image<Bgra32>? _latestImage;

        public GameScreenCapturePublisherThread(
            AbstractScreenCaptureStore store,
            AbstractScreenCapturePublisher publisher,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _store = store;
            _publisher = publisher;
            _latestImage = null;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                var latest = _store.GetLatest();
                bool imageChanged = false;
                if (latest != null && latest != _latestImage)
                {
                    _latestImage = latest;
                    imageChanged = true;
                }
                if (_latestImage != null)
                {
                    _publisher.Publish(_latestImage, imageChanged);
                }
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


    public abstract class AbstractScreenCaptureSubscriber
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

        public override void InitializeSystem()
        {
			_publisherThread = _publisherThreadFactory.CreateThread();
        }

        public override void StartSystem()
        {
            if (_publisherThread != null)
                _publisherThread.ThreadStart();
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

        public override void InitializeSystem()
        {
            for (int i = 0; i < _subscriberThreadFactories.Count; i++)
            {
                var subscriberFactory = _subscriberThreadFactories[i];
                var subscriberThread = subscriberFactory.CreateThread();
                _subscriberThreads.Add(subscriberThread);
            }
        }

        public override void StartSystem()
        {
            for (int i = 0; i < _subscriberThreads.Count; i++)
            {
                var subscriberThread = _subscriberThreads[i];
                subscriberThread.ThreadStart();
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

        public override void InitializeSystem()
        {
            for (int i = 0; i < _subSystems.Count; i++)
            {
                var subSystem = _subSystems[i];
                subSystem.InitializeSystem();
            }
        }


        public override void StartSystem()
        {
            for (int i = 0; i < _subSystems.Count; i++)
            {
                var subSystem = _subSystems[i];
                subSystem.StartSystem();
            }
        }
    }


    public class GameScreenCaptureSystemFacade : AbstractSystem
    {
        private GameScreenCaptureSystem _captureSystem;
        private AbstractScreenCaptureStore _store()
        {
            return new GameScreenCaptureStore();
        }

        private List<AbstractScreenCaptureSubscriber> _subscribers()
        {
            var semaphore = new SemaphoreSlim(0, 1);
            return [new NullScreenCaptureSubscriber(semaphore)];
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
            for (int i = 0; i < subscribers.Count; i++) {
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

        public GameScreenCaptureSystemFacade()
        {
            var store = _store();
            var subscribers = _subscribers();
            var publisher = _publisher(subscribers);
            var publisherThreadFactory = _publisherThreadFactory(store, publisher);
            var subscriberThreadFactories = _subscriberThreadFactories(publisher, subscribers);
            var publisherSystem = _publisherSystem(publisherThreadFactory);
            var subscriberSystem = _subscriberSystem(subscriberThreadFactories);
            _captureSystem = new GameScreenCaptureSystem([publisherSystem, subscriberSystem]);
        }

        public override void InitializeSystem()
        {
            _captureSystem.InitializeSystem();
        }

        public override void StartSystem()
        {
            _captureSystem.StartSystem();
        }
    }
}
