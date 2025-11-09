using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNetTests.Systems.ScreenCapture.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.ScreenCapture.Tests
{
    /**
     * @class GameScreenCaptureStoreTests
     * 
     * @brief Unit tests for verifying proper storage and retrieval of captured screen images
     * 
     * This test class validates that the bot correctly stores and retrieves the most recent
     * screen captures, ensuring that the latest game state information is always available
     * for visual analysis and automation decision-making.
     */
    internal class GameScreenCaptureStoreTests
    {
        /**
         * @brief Tests basic storage and retrieval functionality of captured images
         * 
         * Validates that the bot can properly store screen captures and retrieve them
         * when needed.
         */
        private void _testSetterGetter()
        {
            var store = new GameScreenCaptureStore();
            var image = new Image<Bgra32>(1, 1);
            store.SetLatest(image);
            var result = store.GetLatest();
            Debug.Assert(result == image);
        }

        /**
         * @brief Executes all screen capture storage tests
         * 
         * Runs the complete test suite to ensure the bot will correctly handle
         * screen capture storage operations.
         */
        public void Run()
        {
            _testSetterGetter();
        }
    }


    /**
     * @class GameScreenCaptureStoreThreadTests
     * 
     * @brief Unit tests for verifying continuous screen capture and storage functionality
     * 
     * This test class validates that the bot correctly implements continuous screen capture
     * with proper storage management, ensuring reliable availability of up-to-date game
     * state information for automation processes.
     */
    internal class GameScreenCaptureStoreThreadTests
    {
        MockScreenCaptureOrchestrator _screenCaptureOrchestrator = new MockScreenCaptureOrchestrator();

        MockRunningState _runningState = new MockRunningState();

        GameScreenCaptureStore _store = new GameScreenCaptureStore();

        MaplestoryBotConfiguration _configuration = new MaplestoryBotConfiguration { ProcessName = "meow" };

        Image<Bgra32> _image = new Image<Bgra32>(1, 1);

        /**
         * @brief Configures the running state simulation for thread lifecycle testing
         * 
         * @param loopCount Number of active capture cycles to simulate before stopping
         * 
         * Sets up a controlled test scenario that simulates the thread's running state
         * transitions, allowing precise testing of capture behavior during different
         * operational phases without relying on timing or external signals.
         */
        private void _setupThreadStates(int loopCount)
        {
            _runningState.IsRunningReturn.Add(false);
            for (int i = 0; i < loopCount; i++)
            {
                _runningState.IsRunningReturn.Add(true);
            }
            _runningState.IsRunningReturn.Add(false);
        }

        /**
         * @brief Creates a complete test environment for threaded capture testing
         * 
         * @return Configured GameScreenCaptureStoreThread instance
         * 
         * Prepares a comprehensive test environment that simulates the bot's complete
         * threaded capture and storage pipeline, ensuring all components work together
         * to provide reliable game state monitoring.
         */
        private GameScreenCaptureStoreThread _fixture()
        {
            _screenCaptureOrchestrator = new MockScreenCaptureOrchestrator();
            _runningState = new MockRunningState();
            _store = new GameScreenCaptureStore();
            _image = new Image<Bgra32>(1, 1);
            return new GameScreenCaptureStoreThread(
                _screenCaptureOrchestrator, _store, _runningState
            );
        }

        /**
         * @brief Tests correct process targeting during screen capture
         * 
         * Validates that the bot correctly identifies and captures from the specified
         * game process, ensuring that automation focuses on the intended game
         * rather than other applications running on the system.
         */
        private void _testCaptureUsesTheCorrectProcess()
        {
            var storeThread = _fixture();
            _screenCaptureOrchestrator.CaptureReturn.Add(_image);
            _setupThreadStates(1);
            storeThread.Inject(SystemInjectType.Configuration, _configuration);
            storeThread.Start();
            storeThread.Join(10000);
            Debug.Assert(_screenCaptureOrchestrator.CaptureCalls == 1);
            Debug.Assert(_screenCaptureOrchestrator.CaptureCallArg_processName[0] == "meow");
        }

        /**
         * @brief Tests capture prevention when target process is unspecified
         * 
         * Validates that the bot correctly prevents screen capture attempts when
         * no target process has been configured, ensuring system resources are not
         * wasted on undefined capture operations and preventing potential errors
         * from unconfigured capture targets.
         */
        private void _testCaptureNotCalledIfNoProcessNameIsInjected()
        {
            var storeThread = _fixture();
            _screenCaptureOrchestrator.CaptureReturn.Add(_image);
            _setupThreadStates(1);
            storeThread.Start();
            storeThread.Join(10000);
            Debug.Assert(_screenCaptureOrchestrator.CaptureCalls == 0);
        }

        /**
         * @brief Tests proper storage of valid screen captures
         * 
         * Validates that the bot correctly stores valid screen captures in the
         * image store, ensuring that recent game state information is always
         * available for visual analysis and automation decision-making.
         */
        private void _testCaptureForLatestNonNullImage()
        {
            var storeThread = _fixture();
            _screenCaptureOrchestrator.CaptureReturn.Add(_image);
            _setupThreadStates(1);
            storeThread.Inject(SystemInjectType.Configuration, _configuration);
            storeThread.Start();
            storeThread.Join(10000);
            Debug.Assert(_store.GetLatest() == _image);
        }

        /**
         * @brief Tests proper handling of failed screen captures
         * 
         * Validates that the bot correctly handles failed capture attempts by
         * preserving the last successful capture, ensuring that automation
         * processes always have valid game state information to work with.
         */
        private void _testCaptureForLatestNullImage()
        {
            var storeThread = _fixture();
            _screenCaptureOrchestrator.CaptureReturn.Add(_image);
            _screenCaptureOrchestrator.CaptureReturn.Add(null);
            _setupThreadStates(2);
            storeThread.Inject(SystemInjectType.Configuration, _configuration);
            storeThread.Start();
            storeThread.Join(10000);
            Debug.Assert(_store.GetLatest() == _image);
        }

        /**
         * @brief Tests proper handling of initial capture failures
         * 
         * Validates that the bot correctly handles situations where no successful
         * captures have occurred.
         */
        private void _testCaptureForImageNotFound()
        {
            var storeThread = _fixture();
            _screenCaptureOrchestrator.CaptureReturn.Add(null);
            _setupThreadStates(1);
            storeThread.Inject(SystemInjectType.Configuration, _configuration);
            storeThread.Start();
            storeThread.Join(10000);
            Debug.Assert(_store.GetLatest() == null);
        }

        /**
         * @brief Tests proper updating of stored captures with new content
         * 
         * Validates that the bot correctly updates the stored capture with the
         * most recent game state information, ensuring that automation processes
         * always work with the most current visual data available.
         */
        private void _testCaptureLatestImage()
        {
            var storeThread = _fixture();
            var oldImage = new Image<Bgra32>(1, 1);
            var newImage = new Image<Bgra32>(1, 1);
            _screenCaptureOrchestrator.CaptureReturn.Add(oldImage);
            _screenCaptureOrchestrator.CaptureReturn.Add(newImage);
            _setupThreadStates(2);
            storeThread.Inject(SystemInjectType.Configuration, _configuration);
            storeThread.Start();
            storeThread.Join(10000);
            Debug.Assert(_store.GetLatest() == newImage);
        }

        /**
         * @brief Tests consistent capture performance during operation
         * 
         * Validates that the bot maintains consistent capture performance
         * throughout its operation, ensuring reliable game state monitoring.
         */
        private void _testCaptureAttemptsForEveryRunningLoop()
        {
            for (int i = 0; i < 10; i++)
            {
                var storeThread = _fixture();
                _setupThreadStates(i);
                storeThread.Inject(SystemInjectType.Configuration, _configuration);
                for (int j = 0; j < i; j++)
                    _screenCaptureOrchestrator.CaptureReturn.Add(new Image<Bgra32>(1, 1));
                storeThread.Start();
                storeThread.Join(10000);
                Debug.Assert(_screenCaptureOrchestrator.CaptureCalls == i);
            }
        }

        /**
         * @brief Executes all threaded capture and storage tests
         * 
         * Runs the complete test suite to ensure the bot will correctly implement
         * continuous screen capture with proper storage management, providing
         * confidence that up-to-date game state information will always be
         * available for automation processes.
         */
        public void Run()
        {
            _testCaptureUsesTheCorrectProcess();
            _testCaptureNotCalledIfNoProcessNameIsInjected();
            _testCaptureForLatestNonNullImage();
            _testCaptureForLatestNullImage();
            _testCaptureForImageNotFound();
            _testCaptureLatestImage();
            _testCaptureAttemptsForEveryRunningLoop();
        }
    }


    /**
     * @class GameScreenCaptureStoreSystemTests
     * 
     * @brief Unit tests for screen capture store system thread management
     * 
     * This test class validates that the screen capture store system properly manages
     * its background thread lifecycle, ensuring reliable coordination between image
     * capture operations and shared image storage access for multiple consumer threads.
     */
    public class GameScreenCaptureStoreSystemTest
    {
        private MockThreadFactory _storeThreadFactory = new MockThreadFactory();

        private MockThread _storeThread = new MockThread(new ThreadRunningState());

        /**
         * @brief Creates test environment with mock thread dependencies
         * 
         * @return Configured GameScreenCaptureStoreSystem instance for testing
         * 
         * Prepares a test environment with mock thread factory and thread components
         * to isolate store system behavior from actual thread execution while
         * maintaining the shared image storage coordination semantics.
         */
        private GameScreenCaptureStoreSystem _fixture()
        {
            _storeThreadFactory = new MockThreadFactory();
            _storeThread = new MockThread(new ThreadRunningState());
            _storeThreadFactory.CreateThreadReturn.Add(_storeThread);
            return new GameScreenCaptureStoreSystem(_storeThreadFactory);
        }

        /**
         * @brief Tests background thread creation during system initialization
         * 
         * Validates that the store system creates its background capture thread
         * during initialization, establishing the foundation for continuous image
         * capture and shared storage management for multiple consumer threads.
         */
        private void _testInitializeCreatesStoreThread()
        {
            var gameScreenCaptureStoreSystem = _fixture();
            gameScreenCaptureStoreSystem.Initialize();
            Debug.Assert(_storeThreadFactory.CreateThreadCalls == 1);
        }

        /**
         * @brief Tests background thread activation during system startup
         * 
         * Validates that the store system activates its background capture thread
         * during startup, ensuring continuous image capture begins and the shared
         * image store starts receiving updates for consumer thread access.
         */
        private void _testStartStartsStoreThread()
        {
            var gameScreenCaptureStoreSystem = _fixture();
            gameScreenCaptureStoreSystem.Initialize();
            gameScreenCaptureStoreSystem.Start();
            Debug.Assert(_storeThread.ThreadStartCalls == 1);
        }

        /**
         * @brief Tests configuration propagation to background thread
         * 
         * Validates that the store system correctly propagates configuration data
         * to its background capture thread, ensuring capture parameters and process
         * targeting are properly communicated for consistent shared image generation.
         */
        private void _testInjectInjectsToStoreThread()
        {
            var gameScreenCaptureStoreSystem = _fixture();
            gameScreenCaptureStoreSystem.Initialize();
            gameScreenCaptureStoreSystem.Inject((SystemInjectType)0x1234, 0x2345);
            Debug.Assert(_storeThread.InjectCalls == 1);
            Debug.Assert((int)_storeThread.InjectCallArg_dataType[0] == 0x1234);
            Debug.Assert((int?)_storeThread.InjectCallArg_data[0] == 0x2345);
        }

        /**
         * @brief Executes all store system thread management tests
         * 
         * Runs the complete test suite to ensure the screen capture store system
         * properly manages background thread lifecycle and configuration propagation,
         * providing confidence in reliable shared image storage for multiple consumers.
         */
        public void Run()
        {
            _testInitializeCreatesStoreThread();
            _testStartStartsStoreThread();
            _testInjectInjectsToStoreThread();
        }
    }


    /**
     * @class GameScreenCapturePublisherTest
     * 
     * @brief Unit tests for verifying proper image distribution to multiple subscribers
     * 
     * This test class validates that the bot correctly distributes screen captures to all
     * registered subscribers, ensuring coordinated processing of game state information
     * across different automation components while maintaining proper synchronization.
     */
    public class GameScreenCapturePublisherTest
    {

        private List<AbstractScreenCaptureSubscriber> _subscribers = [];

        private SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);

        private Image<Bgra32> _image = new Image<Bgra32>(1, 1);

        private MockScreenCapturePublisherCountDown _countDown = new MockScreenCapturePublisherCountDown();

        private List<string> _callOrder = [];

        /**
         * @brief Creates a complete test environment for publisher testing
         * 
         * @return Configured GameScreenCapturePublisher instance
         * 
         * Prepares a comprehensive test environment with multiple subscribers and
         * synchronization mechanisms, ensuring reliable testing of image distribution
         * and subscriber coordination under various conditions.
         */
        private GameScreenCapturePublisher _fixture()
        {
            _semaphore = new SemaphoreSlim(0, 3);
            _subscribers = [
                new MockScreenCaptureSubscriber(_semaphore),
                new MockScreenCaptureSubscriber(_semaphore),
                new MockScreenCaptureSubscriber(_semaphore)
            ];
            _image = new Image<Bgra32>(1,1);
            _countDown = new MockScreenCapturePublisherCountDown();
            _setupSubscribers();
            _setupCallOrder();
            return new GameScreenCapturePublisher(_subscribers, _countDown);
        }

        /**
         * @brief Configures subscriber monitoring for test verification
         * 
         * Enables monitoring capabilities on all test subscribers, allowing precise
         * verification of notification delivery and processing across the entire
         * subscriber ecosystem during image distribution tests.
         */
        private void _setupSubscribers()
        {
            for (int i = 0; i < _subscribers.Count; i++)
            {
                var subscriber = (MockScreenCaptureSubscriber)_subscribers[i];
                subscriber.NotifySpy = true;
                subscriber.WaitForNotificationSpy = true;
            }
        }


        /**
         * @brief Synchronizes call tracking across all test components
         * 
         * Establishes consistent call order tracking across all publisher components,
         * enabling precise validation of operation sequencing and coordination in
         * subscriber notification processes.
         */
        private void _setupCallOrder()
        {
            _callOrder = new List<string>();
            for (int i = 0; i < _subscribers.Count; i++) {
                var subscriber = (MockScreenCaptureSubscriber)_subscribers[i];
                subscriber.CallOrder = _callOrder;
            }
            _countDown.CallOrder = _callOrder;
        }

        /**
         * @brief Tests comprehensive notification delivery to all subscribers
         * 
         * Validates that the bot correctly delivers screen captures to all registered
         * subscribers regardless of update status, ensuring all automation components
         * receive consistent game state information for coordinated decision-making.
         */
        private void _testPublishNotifiesAllSubscribersOfUpdatedImage()
        {
            List<bool> updatedList = [true, false];
            for (int i = 0; i < updatedList.Count; i++)
            {
                var updated = updatedList[i];
                var publisher = _fixture();
                publisher.Publish(_image, updated);
                for (int j = 0; j < _subscribers.Count; j++)
                {
                    var subscriber = (MockScreenCaptureSubscriber)_subscribers[i];
                    Debug.Assert(subscriber.NotifyCalls == 1);
                    Debug.Assert(subscriber.NotifyCallArg_image[0] == _image);
                    Debug.Assert(subscriber.NotifyCallArg_updated[0] == updated);
                }
            }
        }


        /**
         * @brief Tests proper synchronization between notification and processing
         * 
         * Validates that the bot correctly coordinates between image distribution
         * and subscriber processing, ensuring subscribers complete their work before
         * the publisher continues operation, preventing race conditions and ensuring
         * reliable automation performance.
         */
        private void _testPublishWaitsForCountDownAfterNotifyingSubscribers()
        {
            var publisher = _fixture();
            var utils = new TestUtilities();
            publisher.Publish(_image, false);
            var setCountDownIndex = _callOrder.IndexOf(utils.Reference(_countDown) + "SetCountDown");
            var waitCountDownIndex = _callOrder.IndexOf(utils.Reference(_countDown) + "WaitCountDown");
            Debug.Assert(setCountDownIndex != -1);
            Debug.Assert(waitCountDownIndex != -1);
            for (int i = 0; i < _subscribers.Count; i++)
            {
                var subscriber = (MockScreenCaptureSubscriber)_subscribers[i];
                var subscriberNotifyIndex = _callOrder.IndexOf(utils.Reference(subscriber) + "Notify");
                Debug.Assert(subscriberNotifyIndex != -1);
                Debug.Assert(subscriberNotifyIndex > setCountDownIndex);
                Debug.Assert(subscriberNotifyIndex < waitCountDownIndex);
                Debug.Assert(subscriber.NotifyCalls == 1);
            }
            Debug.Assert(_countDown.SetCountDownCalls == 1);
            Debug.Assert(_countDown.WaitCountDownCalls == 1);
        }

        /**
         * @brief Executes all publisher functionality tests
         * 
         * Runs the complete test suite to ensure the bot will correctly distribute
         * screen captures to all subscribers with proper synchronization, providing
         * confidence that multiple automation components will work together reliably
         * with consistent game state information.
         */
        public void Run()
        {
            _testPublishNotifiesAllSubscribersOfUpdatedImage();
            _testPublishWaitsForCountDownAfterNotifyingSubscribers();
        }
    }


    /**
     * @brief Tests comprehensive image publishing across multiple capture cycles
     * 
     * Validates that the bot correctly publishes all available screen captures
     * from the store, handling both updated and unchanged images appropriately
     * to ensure all subscribers receive consistent game state information
     * throughout the automation session.
     */
    internal class GameScreenCapturePublisherThreadTest
    {
        MockRunningState _runningState = new MockRunningState();

        MockScreenCaptureStore _store = new MockScreenCaptureStore();

        MockScreenCapturePublisher _publisher = new MockScreenCapturePublisher();

        AbstractWindowActionHandler _viewCheckbox = new MockWindowActionHandler();

        MockWindowStateModifier _modifier = new MockWindowStateModifier();

        /**
         * @brief Configures the running state simulation for thread lifecycle testing
         * 
         * @param loopCount Number of active capture cycles to simulate before stopping
         * 
         * Sets up a controlled test scenario that simulates the thread's running state
         * transitions, allowing precise testing of capture behavior during different
         * operational phases without relying on timing or external signals.
         */
        private void _setupThreadStates(int loopCount)
        {
            _runningState.IsRunningReturn.Add(false);
            for (int i = 0; i < loopCount; i++)
            {
                _runningState.IsRunningReturn.Add(true);
            }
            _runningState.IsRunningReturn.Add(false);
        }

        /**
         * @brief Creates a complete test environment for publisher thread testing
         * 
         * @return Configured GameScreenCapturePublisherThread instance
         * 
         * Prepares a comprehensive test environment that simulates the bot's complete
         * publishing pipeline, ensuring all components work together to reliably
         * distribute game state information to all subscribers during operation.
         */
        private GameScreenCapturePublisherThread _fixture()
        {
            _publisher = new MockScreenCapturePublisher();
            _runningState = new MockRunningState();
            _store = new MockScreenCaptureStore();
            _modifier = new MockWindowStateModifier();
            _viewCheckbox = new WindowViewCheckboxActionHandler([], _modifier);
            return new GameScreenCapturePublisherThread(
                _store, _publisher, _runningState
            );
        }

        /**
         * @brief Tests comprehensive image publishing across multiple capture cycles
         * 
         * Validates that the bot correctly publishes all available screen captures
         * from the store, handling both updated and unchanged images appropriately
         * to ensure all subscribers receive consistent game state information
         * throughout the automation session.
         */
        private void _testPublisherPublishesLatestImage()
        {
            var publisherThread = _fixture();
            _setupThreadStates(3);
            var oldImage = new Image<Bgra32>(1, 1);
            var newImage = new Image<Bgra32>(1, 1);
            _store.GetLatestReturn.Add(oldImage);
            _store.GetLatestReturn.Add(newImage);
            _store.GetLatestReturn.Add(null);
            _modifier.StateReturn.Add(ViewTypes.Snapshots);
            _modifier.StateReturn.Add(ViewTypes.Snapshots);
            _modifier.StateReturn.Add(ViewTypes.Snapshots);
            publisherThread.Inject(SystemInjectType.ActionHandler, _viewCheckbox);
            publisherThread.Start();
            publisherThread.Join(10000);
            Debug.Assert(_publisher.PublishCalls == 3);
            Debug.Assert(_publisher.PublishCallArg_image[0] == oldImage);
            Debug.Assert(_publisher.PublishCallArg_updated[0] == true);
            Debug.Assert(_publisher.PublishCallArg_image[1] == newImage);
            Debug.Assert(_publisher.PublishCallArg_updated[1] == true);
            Debug.Assert(_publisher.PublishCallArg_image[2] == newImage);
            Debug.Assert(_publisher.PublishCallArg_updated[2] == false);
        }

        /**
         * @brief Tests publishing prevention when view control dependency is missing
         * 
         * Validates that the publisher correctly prevents image distribution when
         * the required view control dependency has not been provided, ensuring
         * system resources are not wasted on unauthorized publishing operations
         * and maintaining proper access control over image distribution.
         */
        private void _testPublisherDoesNotpublishWithoutViewCheckboxModifier()
        {
            var publisherThread = _fixture();
            _setupThreadStates(3);
            var oldImage = new Image<Bgra32>(1, 1);
            var newImage = new Image<Bgra32>(1, 1);
            _store.GetLatestReturn.Add(oldImage);
            _store.GetLatestReturn.Add(newImage);
            _store.GetLatestReturn.Add(null);
            _modifier.StateReturn.Add(ViewTypes.Snapshots);
            _modifier.StateReturn.Add(ViewTypes.Snapshots);
            _modifier.StateReturn.Add(ViewTypes.Snapshots);
            publisherThread.Start();
            publisherThread.Join(10000);
            Debug.Assert(_publisher.PublishCalls == 0);
        }

        /**
         * @brief Tests proper handling of initial empty image store conditions
         * 
         * Validates that the bot correctly handles situations where no screen capturess
         * are initially available, ensuring the publishing thread waits for valid
         * game state information before beginning distribution to subscribers.
         */
        private void _testPublisherStartsPublishingAfterFirstLatestImage()
        {
            var publisherThread = _fixture();
            _setupThreadStates(2);
            var newImage = new Image<Bgra32>(1, 1);
            _store.GetLatestReturn.Add(null);
            _store.GetLatestReturn.Add(newImage);
            _modifier.StateReturn.Add(ViewTypes.Snapshots);
            publisherThread.Inject(SystemInjectType.ActionHandler, _viewCheckbox);
            publisherThread.Start();
            publisherThread.Join(10000);
            Debug.Assert(_publisher.PublishCalls == 1);
            Debug.Assert(_publisher.PublishCallArg_image[0] == newImage);
            Debug.Assert(_publisher.PublishCallArg_updated[0] == true);
        }

        /**
         * @brief Executes all publisher thread functionality tests
         * 
         * Runs the complete test suite to ensure the bot will correctly implement
         * continuous image publishing from the capture store, providing confidence
         * that game state information will be reliably distributed to all subscribers
         * throughout the automation session.
         */
        public void Run()
        {
            _testPublisherPublishesLatestImage();
            _testPublisherDoesNotpublishWithoutViewCheckboxModifier();
            _testPublisherStartsPublishingAfterFirstLatestImage();
        }
    }


    /**
     * @class GameScreenCaptureSubscriberThreadTest
     * @brief Unit tests for verifying subscriber thread processing functionality
     * 
     * This test class validates that subscriber threads correctly process screen captures
     * and coordinate with the publisher, ensuring reliable image processing and proper
     * synchronization between different components of the screen capture system.
     */
    internal class GameScreenCaptureSubscriberThreadTest
    {
        MockRunningState _runningState;

        MockScreenCaptureSubscriber _subscriber;

        MockScreenCapturePublisher _publisher;

        SemaphoreSlim _semaphore;

        List<string> _callOrder = [];

        public GameScreenCaptureSubscriberThreadTest()
        {
            _semaphore = new SemaphoreSlim(0, 1);
            _runningState = new MockRunningState();
            _subscriber = new MockScreenCaptureSubscriber(_semaphore);
            _publisher = new MockScreenCapturePublisher();
        }

        /**
         * @brief Creates a test environment for subscriber thread testing
         * 
         * @return Configured GameScreenCaptureSubscriberThread instance
         * 
         * Prepares a test environment with synchronized call tracking to verify
         * the subscriber thread's interaction with the publisher and its
         * processing of screen capture notifications.
         */
        private GameScreenCaptureSubscriberThread _fixture()
        {
            _semaphore = new SemaphoreSlim(0, 1);
            _runningState = new MockRunningState();
            _subscriber = new MockScreenCaptureSubscriber(_semaphore);
            _publisher = new MockScreenCapturePublisher();
            _setupCallOrder();
            return new GameScreenCaptureSubscriberThread(
                _subscriber, _publisher, _runningState
            );

        }

        /**
         * @brief Configures thread running states for testing
         * 
         * @param loopCount Number of processing cycles to simulate
         * 
         * Sets up running state transitions to simulate different numbers
         * of processing cycles, enabling testing of the subscriber thread's
         * behavior across various operational scenarios.
         */
        private void _setupThreadStates(int loopCount)
        {
            _runningState.IsRunningReturn.Add(false);
            for (int i = 0; i < loopCount; i++)
            {
                _runningState.IsRunningReturn.Add(true);
            }
            _runningState.IsRunningReturn.Add(false);
        }

        /**
         * @brief Synchronizes call tracking across test components
         * 
         * Establishes consistent call order tracking to verify the
         * sequence of operations between the subscriber and publisher,
         * ensuring proper coordination during image processing.
         */
        private void _setupCallOrder()
        {
            _callOrder = [];
            _subscriber.CallOrder = _callOrder;
            _publisher.CallOrder = _callOrder;
        }

        /**
         * @brief Tests the complete subscriber processing sequence
         * 
         * Validates that the subscriber thread correctly follows the
         * wait-process-notify sequence for each processing cycle,
         * ensuring proper coordination with the publisher and
         * reliable image processing throughout operation.
         */
        private void _testSubscriberProcessingSequence()
        {
            var utils = new TestUtilities();
            for (int i = 0; i < 10; i++)
            {
                var subscriberThread = _fixture();
                _setupThreadStates(i);
                var subscriberRef = utils.Reference(_subscriber);
                var publisherRef = utils.Reference(_publisher);
                subscriberThread.Start();
                subscriberThread.Join(10000);
                Debug.Assert(_callOrder.Count == (3 * i));
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(_callOrder[(j * 3) + 0] == subscriberRef + "WaitForNotification");
                    Debug.Assert(_callOrder[(j * 3) + 1] == subscriberRef + "ProcessImage");
                    Debug.Assert(_callOrder[(j * 3) + 2] == publisherRef + "NotifyComplete");
                }
            }
        }

        /**
         * @brief Tests configuration propagation to publisher and subscriber components
         * 
         * Validates that the subscriber thread correctly propagates injected configuration
         * data to both the publisher and subscriber components, ensuring consistent
         * system-wide configuration and proper coordination between all screen capture
         * processing elements during operation.
         */
        private void _testInjectPassesInjectedDataOntoPublisherAndSubscriber()
        {
            var subscriberThread = _fixture();
            subscriberThread.Inject((SystemInjectType)0x1234, 0x2345);
            Debug.Assert(_subscriber.InjectCalls == 1);
            Debug.Assert((int)_subscriber.InjectCallArg_dataType[0] == 0x1234);
            Debug.Assert((int?)_subscriber.InjectCallArg_data[0] == 0x2345);
            Debug.Assert(_publisher.InjectCalls == 1);
            Debug.Assert((int)_publisher.InjectCallArg_dataType[0] == 0x1234);
            Debug.Assert((int?)_publisher.InjectCallArg_data[0] == 0x2345);
        }       

        /**
         * @brief Executes the subscriber thread processing test
         * 
         * Runs the test to ensure the subscriber thread corr   ectly
         * processes screen captures and coordinates with the publisher,
         */
        public void Run()
        {
            _testSubscriberProcessingSequence();
            _testInjectPassesInjectedDataOntoPublisherAndSubscriber();
        }
    }


    /**
     * @class GameScreenCapturePublisherSystemTest
     * 
     * @brief Unit tests for verifying the screen capture publisher system initialization and management
     * 
     * This test class validates that the screen capture publisher system correctly initializes
     * and manages its worker threads, ensuring proper startup sequencing and resource management
     * for reliable screen capture distribution across all subscribers.
     */
    public class GameScreenCapturePublisherSystemTest
    {
        private MockThreadFactory _mockThreadFactory;

        private MockRunningState _runningState;

        private MockThread _mockThread;

        /**
         * @brief Initializes test components with default configurations
         * 
         * Sets up the basic test environment with mock thread factory, running state,
         * and thread components to simulate the publisher system's operational
         * dependencies for comprehensive testing.
         */
        public GameScreenCapturePublisherSystemTest()
        {
            _mockThreadFactory = new MockThreadFactory();
            _runningState = new MockRunningState();
            _mockThread = new MockThread(_runningState);
        }

        /**
         * @brief Creates a complete test environment for publisher system testing
         * 
         * @return Configured GameScreenCapturePublisherSystem instance
         * 
         * Prepares a comprehensive test environment with synchronized call tracking
         * to verify the publisher system's thread management and initialization
         * behavior under various operational conditions.
         */
        private GameScreenCapturePublisherSystem _fixture()
        {
            _runningState = new MockRunningState();
            _mockThreadFactory = new MockThreadFactory();
            _mockThread = new MockThread(_runningState);
            _mockThreadFactory.CreateThreadReturn.Add(_mockThread);
            return new GameScreenCapturePublisherSystem(_mockThreadFactory);
        }

        /**
         * @brief Tests correct system initialization and thread startup sequencing
         * 
         * Validates that the publisher system properly initializes its components
         * and starts the worker thread only when explicitly requested, ensuring
         * controlled startup behavior and proper resource management during
         * system initialization.
         */
        private void _testPublisherSystemInitializesAndStartsPublisherThread()
        {
            var publisherSystem = _fixture();
            publisherSystem.Initialize();
            Debug.Assert(_mockThreadFactory.CreateThreadCalls == 1);
            Debug.Assert(_mockThread.ThreadStartCalls == 0);
            publisherSystem.Start();
            Debug.Assert(_mockThreadFactory.CreateThreadCalls == 1);
            Debug.Assert(_mockThread.ThreadStartCalls == 1);
        }

        /**
         * @brief Tests configuration propagation to publisher worker thread
         * 
         * Validates that the publisher system correctly forwards all configuration
         * and runtime data to its worker thread, ensuring consistent system-wide
         * settings and proper coordination between the publisher system and its
         * active publishing components during screen capture distribution.
         */
        private void _testPublisherSystemInjectsToPublisherThread()
        {
            var publisherSystem = _fixture();
            publisherSystem.Initialize();
            publisherSystem.Inject((SystemInjectType)0x1234, 0x2345);
            Debug.Assert(_mockThread.InjectCalls == 1);
            Debug.Assert((int)_mockThread.InjectCallArg_dataType[0] == 0x1234);
            Debug.Assert((int?)_mockThread.InjectCallArg_data[0] == 0x2345);
        }

        /**
         * @brief Executes the publisher system initialization test
         * 
         * Runs the test to ensure the publisher system correctly initializes
         * and manages its worker threads, providing confidence in the reliability
         * of the screen capture distribution system during automation operations.
         */
        public void Run()
        {
            _testPublisherSystemInitializesAndStartsPublisherThread();
            _testPublisherSystemInjectsToPublisherThread();
        }
    }


    /**
     * @class GameScreenCaptureSubscriberSystemTest
     * 
     * @brief Unit tests for verifying the screen capture subscriber system initialization and management
     * 
     * This test class validates that the screen capture subscriber system correctly initializes
     * and manages multiple subscriber threads, ensuring proper startup sequencing and resource
     * management for reliable screen capture processing across all subscriber components.
     */
    internal class GameScreenCaptureSubscriberSystemTest
    {

        private List<AbstractThreadFactory> _mockThreadFactories = [];

        private List<AbstractThread> _mockThreads = [];

        /**
         * @brief Creates a complete test environment for subscriber system testing
         * 
         * @return Configured GameScreenCaptureSubscriberSystem instance
         * 
         * Prepares a comprehensive test environment with multiple thread factories
         * and threads to verify the subscriber system's ability to manage multiple
         * subscriber components and their initialization sequences.
         */
        private GameScreenCaptureSubscriberSystem _fixture()
        {
            _mockThreadFactories = [
                new MockThreadFactory(),
                new MockThreadFactory(),
                new MockThreadFactory()
            ];
            _mockThreads = [
                new MockThread(new MockRunningState()),
                new MockThread(new MockRunningState()),
                new MockThread(new MockRunningState())
            ];
            for (int i = 0; i < _mockThreadFactories.Count; i++) {
                var mockThreadFactory = (MockThreadFactory)_mockThreadFactories[i];
                var mockThread = _mockThreads[i];
                mockThreadFactory.CreateThreadReturn.Add(mockThread);
            }
            return new GameScreenCaptureSubscriberSystem(_mockThreadFactories);
        }


        /**
         * @brief Tests correct system initialization and multi-thread startup sequencing
         * 
         * Validates that the subscriber system properly initializes all subscriber components
         * and starts their worker threads only when explicitly requested, ensuring controlled
         * startup behavior and proper resource management across multiple subscriber instances
         * during system initialization.
         */
        private void _testSubscriberSystemInitializesAndStartsSubscriberThreads()
        {
            var subscriberSystem = _fixture();
            subscriberSystem.Initialize();
            for (int i = 0; i < _mockThreadFactories.Count; i++)
            {
                var mockThreadFactory = (MockThreadFactory)_mockThreadFactories[i];
                var mockThread = (MockThread)_mockThreads[i];
                Debug.Assert(mockThreadFactory.CreateThreadCalls == 1);
                Debug.Assert(mockThread.ThreadStartCalls == 0);
            }
            subscriberSystem.Start();
            for (int i = 0; i < _mockThreadFactories.Count; i++)
            {
                var mockThreadFactory = (MockThreadFactory)_mockThreadFactories[i];
                var mockThread = (MockThread)_mockThreads[i];
                Debug.Assert(mockThreadFactory.CreateThreadCalls == 1);
                Debug.Assert(mockThread.ThreadStartCalls == 1);
            }

        }

        /**
         * @brief Tests configuration propagation to all subscriber worker threads
         * 
         * Validates that the subscriber system correctly forwards all configuration
         * and runtime data to every subscriber worker thread, ensuring consistent
         * system-wide settings and proper coordination across all subscriber
         * components during screen capture processing operations.
         */
        private void _testSubscriberSystemInjectsToSubscriberThreads()
        {
            var subscriberSystem = _fixture();
            subscriberSystem.Initialize();
            subscriberSystem.Inject((SystemInjectType)0x1234, 0x2345);
            for (int i = 0; i < _mockThreadFactories.Count; i++)
            {
                var mockThread = (MockThread)_mockThreads[i];
                Debug.Assert(mockThread.InjectCalls == 1);
                Debug.Assert((int)mockThread.InjectCallArg_dataType[0] == 0x1234);
                Debug.Assert((int?)mockThread.InjectCallArg_data[0] == 0x2345);
            }
        }

        /**
         * @brief Executes the subscriber system initialization test
         * 
         * Runs the test to ensure the subscriber system correctly initializes
         * and manages multiple subscriber threads, providing confidence in the
         * reliability of the screen capture processing system during automation
         * operations with multiple subscriber components.
         */
        public void Run()
        {
            _testSubscriberSystemInitializesAndStartsSubscriberThreads();
            _testSubscriberSystemInjectsToSubscriberThreads();
        }
    }


    /**
     * @class GameScreenCaptureSystemTests
     * 
     * @brief Unit tests for verifying the complete screen capture system coordination
     * 
     * This test class validates that the main screen capture system properly coordinates
     * all subsystem components, ensuring that initialization and startup sequences occur
     * in the correct order across all connected components for reliable game monitoring.
     */
    public class GameScreenCaptureSystemTests
    {
        private List<AbstractSystem> _mockSubSystems = [];

        /**
         * @brief Creates a complete test environment for system coordination testing
         * 
         * @return Configured GameScreenCaptureSystem instance
         * 
         * Prepares a comprehensive test environment with multiple subsystem components
         * to verify the main system's ability to coordinate initialization and startup
         * sequences across all connected components.
         */
        private GameScreenCaptureSystem _fixture()
        {
            _mockSubSystems = [new MockSystem(), new MockSystem(), new MockSystem()];
            return new GameScreenCaptureSystem(_mockSubSystems);
        }

        /**
         * @brief Tests correct system-wide initialization and startup sequencing
         * 
         * Validates that the main screen capture system properly coordinates initialization
         * and startup across all subsystem components.
         */
        private void _testCaptureSystemInitializesAndStartsAllSubSystems()
        {
            var captureSystem = _fixture();
            captureSystem.Initialize();
            for (int i = 0; i < _mockSubSystems.Count; i++)
            {
                var mockSystem = (MockSystem)_mockSubSystems[i];
                Debug.Assert(mockSystem.InitializeSystemCalls == 1);
                Debug.Assert(mockSystem.StartSystemCalls == 0);
            }
            captureSystem.Start();
            for (int i = 0; i < _mockSubSystems.Count; i++)
            {
                var mockSystem = (MockSystem)_mockSubSystems[i];
                Debug.Assert(mockSystem.InitializeSystemCalls == 1);
                Debug.Assert(mockSystem.StartSystemCalls == 1);
            }
        }

        /**
         * @brief Tests configuration propagation to all subsystem components
         * 
         * Validates that the main screen capture system correctly forwards all configuration
         * and runtime data to every subsystem component, ensuring consistent system-wide
         * settings and proper coordination across the entire screen capture pipeline
         * during game monitoring operations.
         */
        private void _testCaptureSystemInjectsToSubSystems()
        {
            var captureSystem = _fixture();
            captureSystem.Initialize();
            captureSystem.Inject((SystemInjectType)0x1234, 0x2345);
            for (int i = 0; i < _mockSubSystems.Count; i++)
            {
                var mockSystem = (MockSystem)_mockSubSystems[i];
                Debug.Assert(mockSystem.InjectCalls == 1);
                Debug.Assert((int)mockSystem.InjectCallArg_dataType[0] == 0x1234);
                Debug.Assert((int?)mockSystem.InjectCallArg_data[0] == 0x2345);
            }
        }

        /**
         * @brief Executes the complete system coordination test
         * 
         * Runs the test to ensure the main screen capture system correctly coordinates
         * all subsystem components, providing confidence that the complete monitoring
         * system will initialize and start reliably.
         */
        public void Run()
        {
            _testCaptureSystemInitializesAndStartsAllSubSystems();
            _testCaptureSystemInjectsToSubSystems();
        }
    }


    public class ScreenCaptureSystemTestSuite
    {
        public void Run()
        {
            new GameScreenCaptureStoreTests().Run();
            new GameScreenCaptureStoreThreadTests().Run();
            new GameScreenCaptureStoreSystemTest().Run();
            new GameScreenCapturePublisherTest().Run();
            new GameScreenCapturePublisherThreadTest().Run();
            new GameScreenCapturePublisherSystemTest().Run();
            new GameScreenCaptureSubscriberThreadTest().Run();
            new GameScreenCaptureSubscriberSystemTest().Run();
            new GameScreenCaptureSystemTests().Run();
        }
    }

}
