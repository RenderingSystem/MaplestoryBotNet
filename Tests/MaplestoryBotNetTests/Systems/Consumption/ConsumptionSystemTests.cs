using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Consumables;
using MaplestoryBotNet.Systems.Macro;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Consumption.Mocks;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.Consumption
{
    public class ResourceStatusScreenCaptureSubscriberTests
    {
        private Image<Bgra32> _image = new Image<Bgra32>(2, 2);

        private MockThread _resourceDetectionThread = new MockThread(new ThreadRunningState());

        private AbstractScreenCaptureSubscriber _fixture()
        {
            _image = new Image<Bgra32>(2, 2);
            _resourceDetectionThread = new MockThread(new ThreadRunningState());
            var handler = new ResourceStatusScreenCaptureSubscriber(
                new SemaphoreSlim(0, 1)
            );
            handler.Notify(_image, true);
            return handler;
        }

        /**
         * @brief Verifies that the screen capture subscriber injects captured images
         * only into threads that are correctly identified as resource detection threads
         * 
         * When the screen capture system captures a new frame showing health and mana
         * bars, the resource status subscriber must forward the image to the resource
         * detection thread for processing. However, the subscriber should only inject
         * the image into threads that have the correct thread type, ignoring any other
         * threads that may be registered.
         */
        private void _testScreenCaptureImageInjected()
        {
            foreach (var threadType in new[] { PotionThreadType.Resource, (PotionThreadType)123 })
            {
                var subscriber = _fixture();
                _resourceDetectionThread.ThreadStateReturn.Add(threadType);
                subscriber.Inject(SystemInjectType.ThreadDependency, _resourceDetectionThread);
                Debug.Assert(_resourceDetectionThread.InjectCalls == 0);
                subscriber.ProcessImage();
                if (threadType == PotionThreadType.Resource)
                {
                    Debug.Assert(_resourceDetectionThread.InjectCalls == 1);
                    Debug.Assert(_resourceDetectionThread.InjectCallArg_data[0] == _image);
                }
                else
                {
                    Debug.Assert(_resourceDetectionThread.InjectCalls == 0);
                }
            }
        }

        public void Run()
        {
            _testScreenCaptureImageInjected();
        }
    }


        public class ResourceDetectionThresholdTests
        {
            private AbstractResourceDetectionThreshold _fixture()
            {
                return new ResourceDetectionThreshold();
            }

            private Resource _createResource(int x, int y, int[] rgb, int[] tolerance)
            {
                return new Resource
                {
                    Pixel = [x, y],
                    Rect = [0, 0, 100, 100],
                    Rgb = rgb,
                    Tolerance = tolerance
                };
            }

            private Image<Bgra32> _createTestImage(int width, int height, byte r, byte g, byte b)
            {
                var image = new Image<Bgra32>(width, height);
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        image[x, y] = new Bgra32(r, g, b, 255);
                    }
                return image;
            }

            /**
             * @brief Verifies that the resource detection threshold correctly identifies when
             * a pixel's RGB values fall inside or outside the configured tolerance range
             * 
             * The threshold determines whether a health or mana bar has crossed the configured
             * threshold by checking if the sampled pixel's color is within the acceptable
             * tolerance range. Values at the exact lower bound (95), exact RGB match (100),
             * and exact upper bound (105) should be considered inside (0). Values below lower
             * bound (94) or above upper bound (106) should be considered outside (1),
             * triggering potion usage.
             */
            private void _testThresholdHandlesDifferentToleranceValues()
            {
                var testCases = new[]
                {
                    (
                        rgb: new[] { 100, 100, 100 },
                        tolerance: new[] { 5, 5, 5 },
                        pixel: new[] { 95, 95, 95 },  // Lower bound
                        expected: 0
                    ),
                    (
                        rgb: new[] { 100, 100, 100 },
                        tolerance: new[] { 5, 5, 5 },
                        pixel: new[] { 100, 100, 100 }, // Exactly rgb
                        expected: 0
                    ),
                    (
                        rgb: new[] { 100, 100, 100 },
                        tolerance: new[] { 5, 5, 5 },
                        pixel: new[] { 105, 105, 105 }, // Upper bound
                        expected: 0
                    ),
                    (
                        rgb: new[] { 100, 100, 100 },
                        tolerance: new[] { 5, 5, 5 },
                        pixel: new[] { 94, 94, 94 },  // Below lower bound
                        expected: 1
                    ),
                    (
                        rgb: new[] { 100, 100, 100 },
                        tolerance: new[] { 5, 5, 5 },
                        pixel: new[] { 106, 106, 106 }, // Above upper bound
                        expected: 1
                    )
                };
                for (int i = 0; i < testCases.Length; i++)
                {
                    var tc = testCases[i];
                    var resource = _createResource(0, 0, tc.rgb, tc.tolerance);
                    var image = _createTestImage(
                        100,
                        100,
                        (byte)tc.pixel[0],
                        (byte)tc.pixel[1],
                        (byte)tc.pixel[2]
                    );
                    var threshold = _fixture();
                    var result = threshold.Threshold(resource, image);
                    Debug.Assert(result == tc.expected);
                }
            }

            /**
             * @brief Verifies that the threshold safely handles pixel coordinates that fall
             * outside the image boundaries without crashing or returning false positives
             * 
             * When users configure detection pixel coordinates that are outside the captured
             * image dimensions (e.g., negative values or values exceeding image width/height),
             * the threshold must gracefully return 0 (no trigger) rather than throwing an
             * exception or returning an incorrect detection. This prevents configuration
             * errors from crashing the bot or causing unintended potion usage.
             */
            private void _testThresholdWithOutOfBoundsCoordinates()
            {
                var testCases = new[]
                {
                    (x: -1, y: 0), // Negative X
                    (x: 0, y: -1), // Negative Y
                    (x: 1000, y: 0), // X beyond width
                    (x: 0, y: 1000), // Y beyond height
                    (x: -100, y: -100), // Both negative
                };
                foreach (var tc in testCases)
                {
                    var resource = _createResource(tc.x, tc.y, [100, 100, 100], [10, 10, 10]);
                    var image = _createTestImage(100, 100, 50, 50, 50);
                    var threshold = _fixture();
                    var result = threshold.Threshold(resource, image);
                    Debug.Assert(result == 0);
                }
            }

            /**
             * @brief Verifies that the threshold correctly samples pixels at valid coordinates
             * across different image sizes and detection rectangle positions
             * 
             * When users configure detection pixels at various positions within different
             * image dimensions (e.g., corner of a small image, center of a large image,
             * edge of the image), the threshold must correctly access the pixel at that
             * exact coordinate. This test ensures the coordinate calculation works correctly
             * across a range of valid image sizes and pixel positions.
             */
            private void _testThresholdWithDifferentCoordinates()
            {
                var testCases = new[]
                {
                    (x: 10, y: 20, width: 100, height: 100),
                    (x: 0, y: 0, width: 50, height: 50),
                    (x: 99, y: 99, width: 100, height: 100),
                    (x: 0, y: 0, width: 1, height: 1),
                };
                foreach (var tc in testCases)
                {
                    var resource = _createResource(tc.x, tc.y, [100, 100, 100], [10, 10, 10]);
                    var imageInside = _createTestImage(tc.width, tc.height, 95, 95, 95);
                    var threshold = _fixture();
                    Debug.Assert(threshold.Threshold(resource, imageInside) == 0);
                }
                foreach (var tc in testCases)
                {
                    var resource = _createResource(tc.x, tc.y, [100, 100, 100], [10, 10, 10]);
                    var imageOutside = _createTestImage(tc.width, tc.height, 50, 50, 50);
                    var threshold = _fixture();
                    Debug.Assert(threshold.Threshold(resource, imageOutside) == 1);
                }
            }

            public void Run()
            {
                _testThresholdHandlesDifferentToleranceValues();
                _testThresholdWithOutOfBoundsCoordinates();
                _testThresholdWithDifferentCoordinates();
            }
        }


    public class ResourceDetectionThreadTests
    {
        private Image<Bgra32> _image = new Image<Bgra32>(100, 100);

        private Resource _hpResource = new Resource();

        private Resource _mpResource = new Resource();

        private MockThread _consumptionThread = new MockThread(new ThreadRunningState());

        private MockResetEvent _resetEvent = new MockResetEvent();

        private MockResourceDetectionThreshold _resourceThreshold = (
            new MockResourceDetectionThreshold()
        );

        private MockRunningState _runningState = new MockRunningState();

        private MaplestoryBotConfiguration _configuration = (
            new MaplestoryBotConfiguration()
        );

        private List<string> _callOrder = [];

        private AbstractThread _fixture()
        {
            _image = new Image<Bgra32>(100, 100);
            _hpResource = new Resource
            {
                Active = 123,
                Key = "234",
                Pixel = [345, 456],
                Rect = [567, 678, 789, 890],
                Rgb = [12, 23, 34],
                Tolerance = [23, 34, 45]
            };
            _mpResource = new Resource
            {
                Active = 234,
                Key = "345",
                Pixel = [456, 567],
                Rect = [67, 78, 89, 90],
                Rgb = [12, 23, 34],
                Tolerance = [23, 34, 45]
            };
            _consumptionThread = new MockThread(new ThreadRunningState());
            _consumptionThread.ThreadStateReturn.Add(PotionThreadType.Consumable);
            _resetEvent = new MockResetEvent();
            _resourceThreshold = new MockResourceDetectionThreshold();
            _runningState = new MockRunningState();
            _configuration = new MaplestoryBotConfiguration
            {
                Hp = _hpResource,
                Mp = _mpResource,
            };
            _callOrder = [];
            var thread = new ResourceDetectionThread(
                _resetEvent,
                _resourceThreshold,
                _runningState
            );
            thread.Inject(SystemInjectType.ConfigurationUpdate, _configuration);
            thread.Inject(SystemInjectType.ThreadDependency, _consumptionThread);
            return thread;
        }

        /**
         * @brief The bot instantly processes each new game screenshot as it arrives
         * 
         * Every time the screen capture system takes a new screenshot of MapleStory,
         * the resource detection thread wakes up immediately to analyze it. There is no
         * polling delay or waiting between checks - the bot processes each frame the
         * moment it becomes available, ensuring health and mana are monitored
         * continuously.
         */
        private void _testInjectingImageSetsResetEvent()
        {
            var thread = _fixture();
            thread.Inject(0, _image);
            Debug.Assert(_resetEvent.SetCalls == 1);
        }

        /**
         * @brief The resource detection thread automatically connects to the potion system
         * 
         * When the bot starts up, the resource detection thread registers itself with
         * the dependency injection system. The consumption thread then discovers it
         * automatically, establishing communication without any manual configuration
         * or hard-coded references between components.
         */
        private void _testInjectingActionInjectsThreadDependency()
        {
            var dataTypeList = new List<object>();
            var dataList = new List<object?>();
            var thread = _fixture();
            var injectAction = new MockInjectAction();
            injectAction.GetActionReturn.Add(
                (_, __) => { dataTypeList.Add(_); dataList.Add(__); }
            );
            thread.Inject(SystemInjectType.InjectAction, injectAction);
            Debug.Assert(dataTypeList.Count == 1);
            Debug.Assert(dataTypeList[0] is SystemInjectType.ThreadDependency);
            Debug.Assert(dataList[0] == thread);
        }

        /**
         * @brief The bot continuously monitors health and mana across every game frame
         * 
         * During extended grinding sessions, the bot processes each incoming game
         * screenshot through a complete analysis loop: waiting for the next frame,
         * checking the health threshold by sampling the configured pixel region,
         * checking the mana threshold, and sending both results to the potion system.
         * This test verifies the loop runs correctly across multiple iterations,
         * ensuring the bot never skips frames.
         */
        public void _testThreadInjectLoop()
        {
            for (int i = 1; i < 10; i++)
            {
                var thread = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _resourceThreshold.ThresholdReturn.Add(123);
                    _resourceThreshold.ThresholdReturn.Add(234);
                }
                _runningState.IsRunningReturn.Add(false);
                for (int j = 0; j < i; j++)
                {
                    _runningState.IsRunningReturn.Add(true);
                }
                _runningState.IsRunningReturn.Add(false);
                thread.Inject(0, _image);
                _resetEvent.CallOrder = _callOrder;
                _resourceThreshold.CallOrder = _callOrder;
                _consumptionThread.CallOrder = _callOrder;
                var resetRef = new TestUtilities().Reference(_resetEvent);
                var resourceRef = new TestUtilities().Reference(_resourceThreshold);
                var consumptionRef = new TestUtilities().Reference(_consumptionThread);
                thread.Start();
                thread.Join(10000);
                Debug.Assert(_callOrder.Count == 5 * i);
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(_callOrder[5 * j + 0] == resetRef + "WaitOne");
                    Debug.Assert(_callOrder[5 * j + 1] == resourceRef + "Threshold");
                    Debug.Assert(_callOrder[5 * j + 2] == resourceRef + "Threshold");
                    Debug.Assert(_callOrder[5 * j + 3] == consumptionRef + "ThreadInject");
                    Debug.Assert(_callOrder[5 * j + 4] == consumptionRef + "ThreadInject");
                }
            }
        }

        /**
         * @brief Health and mana values detected from the game screen reach the potion
         * system
         * 
         * When the bot analyzes a game screenshot and determines that health or mana
         * has fallen below configured thresholds, those numeric values are injected
         * into the consumption thread. Health values go to the health potion handler
         * and mana values go to the mana potion handler, ensuring the right type of
         * potion is triggered for the right situation.
         */
        public void _testThreadInjectsThreshold()
        {
            var thread = _fixture();
            _resourceThreshold.ThresholdReturn.Add(123);
            _resourceThreshold.ThresholdReturn.Add(234);
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
            thread.Inject(0, _image);
            thread.Start();
            thread.Join(10000);
            Debug.Assert(_consumptionThread.InjectCalls == 2);
            Debug.Assert(
                (PotionThreadType)_consumptionThread.InjectCallArg_dataType[0] ==
                PotionThreadType.HealthThreshold
            );
            Debug.Assert(
                (PotionThreadType)_consumptionThread.InjectCallArg_dataType[1] ==
                PotionThreadType.ManaThreshold
            );
            Debug.Assert((int)_consumptionThread.InjectCallArg_data[0]! == 123);
            Debug.Assert((int)_consumptionThread.InjectCallArg_data[1]! == 234);
        }

        /**
         * @brief The bot identifies this component as the health and mana detection
         * system
         * 
         * When monitoring active bot components, the resource detection thread reports
         * its type as PotionThreadType.Resource. This allows the bot's management
         * system to distinguish this health/mana monitoring thread from other threads
         * like keyboard handlers or macro executors.
         */
        private void _testThreadState()
        {
            var thread = _fixture();
            Debug.Assert(thread.State() is PotionThreadType.Resource);
        }

        public void Run()
        {
            _testInjectingImageSetsResetEvent();
            _testInjectingActionInjectsThreadDependency();
            _testThreadInjectLoop();
            _testThreadInjectsThreshold();
            _testThreadState();
        }
    }


    public class ConsumptionThreadRefresherTests
    {
        private ConsumptionThreadContext _context = new ConsumptionThreadContext();

        private MockMacroRandom _macroRandom = new MockMacroRandom();

        private MockTimestampFactory _consumableStopwatchFactory = new MockTimestampFactory();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private List<MockTimestamp> _timestamps = [];

        private List<Consumable> _consumables = [];

        public AbstractConsumptionThreadRefresher _fixture()
        {
            _context = new ConsumptionThreadContext();
            _macroRandom = new MockMacroRandom();
            _consumableStopwatchFactory = new MockTimestampFactory();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration();
            return new ConsumptionThreadRefresher(
                _context,
                _macroRandom,
                _consumableStopwatchFactory
            );
        }

        private void _setupTestCase(AbstractConsumptionThreadRefresher refresher)
        {
            _timestamps = [
                new MockTimestamp(),
                new MockTimestamp()
            ];
            _consumables = [
                new Consumable
                {
                    Active = 123,
                    MinDelay = 234,
                    MaxDelay = 345
                },
                new Consumable
                {
                    Active = 0,
                    MinDelay = 345,
                    MaxDelay = 456
                },
                new Consumable
                {
                    Active = 345,
                    MinDelay = 456,
                    MaxDelay = 567
                }
            ];
            _consumableStopwatchFactory.CreateReturn.Add(_timestamps[0]);
            _consumableStopwatchFactory.CreateReturn.Add(_timestamps[1]);
            _maplestoryBotConfiguration.Consumables.Add(_consumables[0]);
            _maplestoryBotConfiguration.Consumables.Add(_consumables[1]);
            _maplestoryBotConfiguration.Consumables.Add(_consumables[2]);
            _macroRandom.NextReturn.Add(2345);
            _macroRandom.NextReturn.Add(4567);
            refresher.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
        }

        /**
         * @brief Tests that the bot filters out inactive consumables and creates proper
         * cooldown timers
         * 
         * When the bot's configuration is updated with a list of consumable items (potions,
         * buffs, etc.), the refresher examines each consumable's Active flag.
         * Consumables with Active = 0 are ignored and never added to the active consumption
         * queue. Only consumables with a non-zero Active value are tracked.
         */
        private void _testRefreshingConsumablesBuildsStopwatches()
        {
            var refresher = _fixture();
            _setupTestCase(refresher);
            refresher.Refresh();
            Debug.Assert(_context.Stopwatches.Count == 2);
            Debug.Assert(_context.Stopwatches[0] == _timestamps[0]);
            Debug.Assert(_context.Stopwatches[1] == _timestamps[1]);
            Debug.Assert(_context.Timestamps.Count == 2);
            Debug.Assert(_context.Timestamps[0] == 2.345);
            Debug.Assert(_context.Timestamps[1] == 4.567);
            var expectedConsumables = new[] { _consumables[0], _consumables[2] };
            for (int i = 0; i < expectedConsumables.Length; i++)
            {
                Debug.Assert(_context.Consumables.Count == 2);
                Debug.Assert(_context.Consumables[i].Active == expectedConsumables[i].Active);
                Debug.Assert(_context.Consumables[i].MinDelay == expectedConsumables[i].MinDelay);
                Debug.Assert(_context.Consumables[i].MaxDelay == expectedConsumables[i].MaxDelay);
            }
        }

        /**
         * @brief Tests that each active consumable receives a randomized cooldown period
         * within its configured MinDelay to MaxDelay range
         * 
         * For every active consumable, the bot generates a random cooldown duration between
         * MinDelay and MaxDelay (inclusive range). These values are specified in seconds
         * in the configuration, but the bot converts them to milliseconds by multiplying by
         * 1000 before calling the random number generator.
         */
        private void _testRefreshingConsumablesUsesRandomRange()
        {
            var refresher = _fixture();
            _setupTestCase(refresher);
            refresher.Refresh();
            Debug.Assert(_macroRandom.NextCalls == 2);
            Debug.Assert(_macroRandom.NextCallArg_minValue[0] == 234000);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[0] == 345000);
            Debug.Assert(_macroRandom.NextCallArg_minValue[1] == 456000);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[1] == 567000);
        }

        public void Run()
        {
            _testRefreshingConsumablesBuildsStopwatches();
            _testRefreshingConsumablesUsesRandomRange();
        }
    }


    public class ConsumptionQueueUpdaterTests
    {
        private ConsumptionThreadContext _context = new ConsumptionThreadContext();

        private AbstractConsumptionQueueUpdater _fixture()
        {
            _context = new ConsumptionThreadContext();
            return new ConsumptionQueueUpdater(_context);
        }

        /**
         * @brief Tests that the bot queues a consumable when its cooldown timer has expired
         * 
         * The bot tracks three consumables with different elapsed times and cooldown durations.
         * Consumable 0 has elapsed 123s against a 124s cooldown (not ready yet).
         * Consumable 1 has elapsed 234s against a 233s cooldown (ready - should queue).
         * Consumable 2 has elapsed 345s against a 346s cooldown (not ready yet).
         */
        private void _testQueueUpdaterAddsReadyConsumable()
        {
            var updater = _fixture();
            _context.Stopwatches = [
                new MockTimestamp { GetTimestampReturn = [123] },
                new MockTimestamp { GetTimestampReturn = [234] },
                new MockTimestamp { GetTimestampReturn = [345] }
            ];
            _context.Timestamps = [124, 233, 346];
            updater.Update();
            Debug.Assert(_context.Next.Count == 1);
            Debug.Assert(_context.Next.Dequeue() == 1);
        }

        /**
         * @brief Tests that the bot does not queue the same consumable multiple times
         * 
         * When a consumable becomes ready for use, the bot adds it to the consumption queue.
         * If the consumable remains ready on subsequent update cycles (because it hasn't
         * been consumed yet), the bot checks the queue first and avoids adding duplicate
         * entries. This prevents the same consumable from being used multiple times.
         */
        private void _testQueueUpdaterDoesNotAddDuplicateEntries()
        {
            var updater = _fixture();
            _context.Stopwatches = [
                new MockTimestamp { GetTimestampReturn = [10, 12] }
            ];
            _context.Timestamps = [5];
            updater.Update();
            Debug.Assert(_context.Next.Count == 1);
            Debug.Assert(_context.Next.Peek() == 0);
            updater.Update();
            Debug.Assert(_context.Next.Count == 1);
            Debug.Assert(_context.Next.Peek() == 0);
        }

        /**
         * @brief Tests that the bot handles having no active consumables without errors
         * 
         * When no consumables are configured or all consumables are marked inactive, the
         * Stopwatches and Timestamps collections remain empty. The updater must gracefully
         * skip the scanning loop without throwing exceptions. Any existing items in the
         * consumption queue remain unchanged, as there are no new consumables to process.
         */
        private void _testQueueUpdaterHandlesEmptyConsumableList()
        {
            var updater = _fixture();
            _context.Next.Enqueue(5);
            updater.Update();
            Debug.Assert(_context.Next.Count == 1);
            Debug.Assert(_context.Next.Dequeue() == 5);
        }

        public void Run()
        {
            _testQueueUpdaterAddsReadyConsumable();
            _testQueueUpdaterDoesNotAddDuplicateEntries();
            _testQueueUpdaterHandlesEmptyConsumableList();
        }
    }


    public class ResourceExecutorTests
    {
        private ConsumptionThreadContext _context = (
            new ConsumptionThreadContext()
        );

        private MockMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder = (
            new MockMacroCommandsExecutorBuilder()
        );

        private MockMacroCommandsExecutor _macroCommandsExecutor = (
            new MockMacroCommandsExecutor()
        );

        private MockMacroRandom _mockRandom = new MockMacroRandom();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private AbstractConsumptionExecutor _fixture(
            PotionResourceType resourceType
        )
        {
            _context = new ConsumptionThreadContext();
            _macroCommandsExecutorBuilder = new MockMacroCommandsExecutorBuilder();
            _macroCommandsExecutor = new MockMacroCommandsExecutor();
            _macroCommandsExecutorBuilder.BuildReturn.Add(_macroCommandsExecutor);
            _mockRandom = new MockMacroRandom();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                Hp = new Resource { Key = "hp" },
                Mp = new Resource { Key = "mp" }
            };
            var executor = new ResourceExecutor(
                resourceType,
                _context,
                _macroCommandsExecutorBuilder,
                _mockRandom
            );
            executor.Inject(
                SystemInjectType.KeystrokeTransmitter,
                new MockKeystrokeTransmitter()
            );
            return executor;
        }

        /**
         * @brief Tests that the bot presses the correct resource key (HP or MP potion)
         * when the resource threshold is met and the resource is active
         * 
         * This test verifies that the ResourceExecutor correctly handles both health and
         * mana potion triggers under various conditions. The bot should only press a
         * resource key when two conditions are both true: the threshold has been crossed
         * (meaning the resource is low enough to need a potion) AND the resource is marked
         * as active in the configuration.
         */
        private void _testInjectingResourcePressesResourceKeyOnExecute()
        {
            foreach (var resourceType in new[] { PotionResourceType.Health, PotionResourceType.Mana })
            foreach (var threshold in new[] { 1, 0 })
            foreach (var active in new[] { 1, 0 })
            {
                var executor = _fixture(resourceType);
                var key = (resourceType is PotionResourceType.Health ? "hp" : "mp");
                var resource = (
                    (resourceType is PotionResourceType.Health) ? _maplestoryBotConfiguration.Hp :
                    (resourceType is PotionResourceType.Mana) ? _maplestoryBotConfiguration.Mp :
                    new Resource()
                );
                var macroCommands = _macroCommandsExecutor.ExecuteCallArg_macroCommands;
                resource.Active = active;
                executor.Inject(SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration);
                executor.Inject(resourceType, threshold);
                var result = executor.Execute();
                if (threshold != 0 && active != 0)
                {
                    Debug.Assert(result);
                    Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 1);
                    Debug.Assert(macroCommands[0].Count == 1);
                    Debug.Assert(macroCommands[0][0] == "key press {" + key + "} {50} {150}");
                }
                else
                {
                    Debug.Assert(!result);
                    Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 0);
                }
            }
        }

        public void Run()
        {
            _testInjectingResourcePressesResourceKeyOnExecute();
        }
    }


    public class ConsumptionExecutorTests
    {
        private ConsumptionThreadContext _context = new ConsumptionThreadContext();

        private MockMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder = (
            new MockMacroCommandsExecutorBuilder()
        );

        private MockMacroCommandsExecutor _macroCommandsExecutor = (
            new MockMacroCommandsExecutor()
        );

        private MockMacroRandom _macroRandom = new MockMacroRandom();

        private List<int> _nextReturns = [];

        private List<Consumable> _consumables() {
            return [
                new Consumable {
                    MinDelay = 12,
                    MaxDelay = 23,
                    Key = "meow0"
                },
                new Consumable {
                    MinDelay = 23,
                    MaxDelay = 34,
                    Key = "meow1"
                },
                new Consumable {
                    MinDelay = 34,
                    MaxDelay = 45,
                    Key = "meow2"
                },
                new Consumable {
                    MinDelay = 45,
                    MaxDelay = 56,
                    Key = "meow3"
                }
            ];
        }

        public AbstractConsumptionExecutor _fixture()
        {
            _context = new ConsumptionThreadContext();
            _macroCommandsExecutorBuilder = new MockMacroCommandsExecutorBuilder();
            _macroCommandsExecutor = new MockMacroCommandsExecutor();
            _macroCommandsExecutorBuilder.BuildReturn.Add(_macroCommandsExecutor);
            _nextReturns = [67, 78, 89, 90];
            _macroRandom = new MockMacroRandom();
            var executor = new ConsumptionExecutor(
                _context,
                _macroCommandsExecutorBuilder,
                _macroRandom
            );
            executor.Inject(
                SystemInjectType.KeystrokeTransmitter,
                new MockKeystrokeTransmitter()
            );
            return executor;
        }

        private void _setupFixture()
        {
            _context.Next.Enqueue(3);
            _context.Next.Enqueue(1);
            _context.Next.Enqueue(2);
            _context.Next.Enqueue(0);
            _context.Consumables = [
                _consumables()[0],
                _consumables()[1],
                _consumables()[2],
                _consumables()[3]
            ];
            _context.Stopwatches = [
                new MockTimestamp(),
                new MockTimestamp(),
                new MockTimestamp(),
                new MockTimestamp(),
            ];
            _macroRandom.NextReturn.Add(_nextReturns[0]);
            _macroRandom.NextReturn.Add(_nextReturns[1]);
            _macroRandom.NextReturn.Add(_nextReturns[2]);
            _macroRandom.NextReturn.Add(_nextReturns[3]);
            _context.Timestamps = [0, 0, 0, 0];
        }

        /**
         * @brief Tests that the bot presses the correct consumable keys in queue order
         * 
         * When multiple consumables are ready for use, they are queued in a specific order.
         * This test verifies that the ConsumptionExecutor processes the queue in FIFO
         * (first-in, first-out) order and presses the correct key for each consumable.
         */
        private void _testExecutePressesConsumableKey()
        {
            var executor = _fixture();
            _setupFixture();
            var count = _context.Next.Count;
            var commands = _macroCommandsExecutor.ExecuteCallArg_macroCommands;
            for (int i = 0; i < count; i++)
            {
                Debug.Assert(executor.Execute());
                Debug.Assert(_macroCommandsExecutor.ExecuteCalls == i + 1);
            }
            Debug.Assert(commands[0].Count == 1);
            Debug.Assert(commands[0][0] == "key press {meow3} {50} {150}");
            Debug.Assert(commands[1].Count == 1);
            Debug.Assert(commands[1][0] == "key press {meow1} {50} {150}");
            Debug.Assert(commands[2].Count == 1);
            Debug.Assert(commands[2][0] == "key press {meow2} {50} {150}");
            Debug.Assert(commands[3].Count == 1);
            Debug.Assert(commands[3][0] == "key press {meow0} {50} {150}");
        }

        /**
         * @brief Tests that the bot resets the cooldown timer after using a consumable
         * 
         * Each time the bot uses a consumable (potion, buff, etc.), it must reset that
         * consumable's stopwatch to track when it was last used. This allows the bot to
         * enforce cooldown periods and prevent using the same consumable again until
         * enough time has passed.
         */
        private void _testExecuteSetsTimestamp()
        {
            var executor = _fixture();
            _setupFixture();
            var count = _context.Next.Count;
            var commands = _macroCommandsExecutor.ExecuteCallArg_macroCommands;
            for (int i = 0; i < count; i++)
            {
                var peek = _context.Next.Peek();
                var timestamp = (MockTimestamp)_context.Stopwatches[peek];
                Debug.Assert(timestamp.SetTimestampCalls == 0);
                Debug.Assert(executor.Execute());
                Debug.Assert(timestamp.SetTimestampCalls == 1);
            }
        }

        /**
         * @brief Tests that the bot generates random cooldown intervals within
         * configured ranges
         * 
         * To appear more human-like and avoid detection, the bot does not use consumables
         * on fixed intervals. Instead, it generates a random cooldown period between
         * MinDelay and MaxDelay (in seconds) for each consumable after using it.
         */
        private void _testExecuteRandomIntervals()
        {
            var executor = _fixture();
            _setupFixture();
            var count = _context.Next.Count;
            for (int i = 0; i < count; i++)
            {
                var peek = _context.Next.Peek();
                var consumable = _consumables()[peek];
                Debug.Assert(executor.Execute());
                Debug.Assert(_macroRandom.NextCalls == i + 1);
                Debug.Assert(_macroRandom.NextCallArg_minValue[i] == consumable.MinDelay * 1000);
                Debug.Assert(_macroRandom.NextCallArg_maxValue[i] == consumable.MaxDelay * 1000);

            }
        }

        /**
         * @brief Tests that the bot stores the randomized cooldown duration for each
         * consumable
         * 
         * After generating a random cooldown period, the bot stores this value as a timestamp
         * in the consumable's tracking data. The consumption thread later compares elapsed
         * time against this timestamp to determine when the consumable becomes ready again.
         */
        private void _testExecuteRandomAssignment()
        {
            var executor = _fixture();
            _setupFixture();
            var count = _context.Next.Count;
            for (int i = 0; i < count; i++)
            {
                var peek = _context.Next.Peek();
                Debug.Assert(_context.Timestamps[peek] == 0);
                Debug.Assert(executor.Execute());
                Debug.Assert(_context.Timestamps[peek] == _nextReturns[i] / 1000.0);
            }
        }

        /**
         * @brief Tests that the bot does nothing when there are no consumables waiting to be used
         * 
         * When the consumption queue is empty (meaning no health potions, mana potions, or
         * buffs are currently ready to be used), the bot's execution step should simply
         * return false and take no action. This prevents the bot from attempting to press
         * keys when there's nothing to consume.
         */
        private void _testExecuteEmpty()
        {
            var executor = _fixture();
            Debug.Assert(!executor.Execute());
        }

        public void Run()
        {
            _testExecutePressesConsumableKey();
            _testExecuteSetsTimestamp();
            _testExecuteRandomIntervals();
            _testExecuteRandomAssignment();
            _testExecuteEmpty();
        }
    }


    public class ConsumptionThreadHelperTests
    {
        private MockConsumptionThreadRefresher _contextRefresher = (
            new MockConsumptionThreadRefresher()
        );

        private MockConsumptionQueueUpdater _consumptionQueueUpdater = (
            new MockConsumptionQueueUpdater()
        );

        private List<AbstractConsumptionExecutor> _consumptionChain = [];

        private List<string> _callOrder = [];

        private AbstractConsumptionThreadHelper _fixture()
        {
            _contextRefresher = new MockConsumptionThreadRefresher();
            _consumptionQueueUpdater = new MockConsumptionQueueUpdater();
            _consumptionChain = [
                new MockConsumptionExecutor(),
                new MockConsumptionExecutor(),
                new MockConsumptionExecutor()
            ];
            _callOrder = [];
            return new ConsumptionThreadHelper(
                _contextRefresher,
                _consumptionQueueUpdater,
                _consumptionChain
            );
        }
        /**
         * @brief Tests that the bot's consumption coordinator forwards all updates to its
         * internal systems
         *
         * When the bot receives a configuration update (like a new potion key binding or
         * cooldown setting), that update must reach every part of the consumption system.
         * This test verifies that when the coordinator receives any injection, it forwards
         * that injection to all internal components. This ensures that when you change
         * your potion settings in the bot's configuration, every part of the consumption
         * system learns about the change immediately.
         */
        private void _testHelperInject()
        {
            var helper = _fixture();
            helper.Inject(123, 234);
            Debug.Assert(_contextRefresher.InjectCalls == 1);
            Debug.Assert((int)_contextRefresher.InjectCallArg_dataType[0] == 123);
            Debug.Assert((int)_contextRefresher.InjectCallArg_data[0]! == 234);
            Debug.Assert(_consumptionQueueUpdater.InjectCalls == 1);
            Debug.Assert((int)_consumptionQueueUpdater.InjectCallArg_dataType[0] == 123);
            Debug.Assert((int)_consumptionQueueUpdater.InjectCallArg_data[0]! == 234);
            foreach (MockConsumptionExecutor executor in _consumptionChain)
            {
                Debug.Assert(executor.InjectCalls == 1);
                Debug.Assert((int)executor.InjectCallArg_dataType[0] == 123);
                Debug.Assert((int)executor.InjectCallArg_data[0]! == 234);
            }
        }

        /**
         * @brief Tests that the bot processes consumables in the correct sequence each cycle
         * 
         * This test verifies that the sequence always happens in this exact order. The
         * bot always refreshes configurations first, then updates the ready queue, then
         * executes consumables. Executors are tried in priority order (health potions before
         * mana potions before buffs, for example), and the bot stops after the first
         * successful consumption to avoid using multiple items in a single cycle.
         */
        private void _testHelperExecute()
        {
            for (int i = 0; i < _consumptionChain.Count; i++)
            {
                var helper = _fixture();
                _contextRefresher.CallOrder = _callOrder;
                _consumptionQueueUpdater.CallOrder = _callOrder;
                foreach (MockConsumptionExecutor executor in _consumptionChain)
                {
                    executor.CallOrder = _callOrder;
                }
                var refresherRef = new TestUtilities().Reference(_contextRefresher);
                var consumptionRef = new TestUtilities().Reference(_consumptionQueueUpdater);
                var executeRefs = new List<string>();
                foreach (MockConsumptionExecutor executor in _consumptionChain)
                {
                    executeRefs.Add(new TestUtilities().Reference(executor));
                }
                for (int j = 0; j < i; j++)
                {
                    var executor = (MockConsumptionExecutor)_consumptionChain[j];
                    executor.ExecuteReturn.Add(false);
                }
                ((MockConsumptionExecutor)_consumptionChain[i]).ExecuteReturn.Add(true);
                helper.Run();
                Debug.Assert(_callOrder.Count == 3 + i);
                Debug.Assert(_callOrder[0] == refresherRef + "Refresh");
                Debug.Assert(_callOrder[1] == consumptionRef + "Update");
                for (int j = 0; j < i + 1; j++)
                {
                    Debug.Assert(_callOrder[2 + j] == executeRefs[j] + "Execute");
                }
            }
            
        }

        public void Run()
        {
            _testHelperInject();
            _testHelperExecute();
        }
    }


    public class ConsumptionThreadTests
    {
        private MockTimestamp _consumableTimestamp = new MockTimestamp();

        private MockMacroSleeper _macroSleeper = new MockMacroSleeper();

        private MockConsumptionThreadHelper _consumptionThreadHelper = (
            new MockConsumptionThreadHelper()
        );

        private MockRunningState _runningState = new MockRunningState();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private List<string> _callOrder = [];

        private AbstractThread _fixture()
        {
            _consumableTimestamp = new MockTimestamp();
            _macroSleeper = new MockMacroSleeper();
            _consumptionThreadHelper = new MockConsumptionThreadHelper();
            _runningState = new MockRunningState();
            _callOrder = [];
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                MacroSettings = new MacroSettings
                {
                    PotionFrequency = 3.456
                }
            };
            return new ConsumptionThread(
                _consumableTimestamp,
                _macroSleeper,
                _consumptionThreadHelper,
                _runningState        
            );
        }

        /**
         * @brief Tests that the bot forwards configuration updates and other data to the consumption helper
         * 
         * When the bot receives any injection (such as a configuration update, keystroke transmitter,
         * or threshold value), it must pass that data along to the consumption helper which manages
         * the actual consumption logic. This test verifies that when the consumption thread receives
         * an injection with dataType=123 and data=234, it forwards exactly that data to the helper.
         */
        private void _testInjectingToHelper()
        {
            var thread = _fixture();
            thread.Inject(123, 234);
            Debug.Assert(_consumptionThreadHelper.InjectCalls == 1);
            Debug.Assert((int)_consumptionThreadHelper.InjectCallArg_dataType[0] == 123);
            Debug.Assert((int)_consumptionThreadHelper.InjectCallArg_data[0]! == 234);
        }

        /**
         * @brief Tests that the bot identifies this thread as the consumable handler
         * 
         * The bot runs multiple threads simultaneously This test verifies that the
         * consumption thread reports its type as "Consumable" when asked for its state.
         * This allows the bot's thread manager to distinguish between different thread
         * types.
         */
        private void _testThreadState()
        {
            var thread = _fixture();
            Debug.Assert(thread.State() is PotionThreadType.Consumable);
        }

        /**
         * @brief Tests that the consumption thread can be discovered by other bot components
         * 
         * This test verifies that when an InjectAction is provided to the consumption thread,
         * the thread calls that action with its own reference and identifies itself as a thread
         * dependency. This allows other bot components to discover the consumption thread and
         * establish communication with it automatically.
         */
        private void _testInjectAction()
        {
            var dataType = new List<object>();
            var data = new List<object?>();
            var thread = _fixture();
            thread.Inject(
                SystemInjectType.InjectAction,
                new InjectAction(
                    (_, __) => { dataType.Add(_); data.Add(__); }
                )
            );
            Debug.Assert(dataType.Count == 1);
            Debug.Assert(dataType[0] is SystemInjectType.ThreadDependency);
            Debug.Assert(data[0] == thread);
        }

        /**
         * @brief Tests that the bot continuously monitors and consumes potions during gameplay
         * 
         * When the bot is in botting mode (actively playing the game), it follows a continuous
         * loop each cycle: record the current time, check if any consumables are ready and
         * use them, calculate how long to wait until the next check, and sleep for that duration.
         * This test verifies that when the bot is in botting states, it executes the consumption
         * helper each cycle.
         */
        private void _testThreadLoop()
        {
            for (int i = 0; i < (int)MacroExecutorStateTypes.MaxNum; i++)
            for (int j = 1; j < 5; j++)
            {
                var thread = _fixture();
                thread.Inject(
                    SystemInjectType.ConfigurationUpdate,
                    _maplestoryBotConfiguration
                );
                _runningState.IsRunningReturn.Add(false);
                for (int k = 0; k < j; k++)
                {
                    _runningState.IsRunningReturn.Add(true);
                    _consumableTimestamp.GetTimestampReturn.Add(0.00123);
                }
                _runningState.IsRunningReturn.Add(false);
                thread.Inject((MacroExecutorStateTypes)i, 0);
                _consumableTimestamp.CallOrder = _callOrder;
                _consumptionThreadHelper.CallOrder = _callOrder;
                _macroSleeper.CallOrder = _callOrder;
                var consumableRef = new TestUtilities().Reference(_consumableTimestamp);
                var consumptionRef = new TestUtilities().Reference(_consumptionThreadHelper);
                var sleeperRef = new TestUtilities().Reference(_macroSleeper);
                thread.Start();
                thread.Join(10000);
                if (
                    i != (int)MacroExecutorStateTypes.Login &&
                    i != (int)MacroExecutorStateTypes.Idle &&
                    i != (int)MacroExecutorStateTypes.Reset
                )
                {
                    for (int k = 0; k < j; k++)
                    {
                        Debug.Assert(_callOrder.Count == 4 * j);
                        Debug.Assert(_callOrder[4 * k + 0] == consumableRef + "SetTimestamp");
                        Debug.Assert(_callOrder[4 * k + 1] == consumptionRef + "Run");
                        Debug.Assert(_callOrder[4 * k + 2] == consumableRef + "GetTimestamp");
                        Debug.Assert(_callOrder[4 * k + 3] == sleeperRef + "Sleep");
                    }
                }
                else
                {
                    for (int k = 0; k < j; k++)
                    {
                        Debug.Assert(_callOrder.Count == 3 * j);
                        Debug.Assert(_callOrder[3 * k + 0] == consumableRef + "SetTimestamp");
                        Debug.Assert(_callOrder[3 * k + 1] == consumableRef + "GetTimestamp");
                        Debug.Assert(_callOrder[3 * k + 2] == sleeperRef + "Sleep");
                    }
                }
            }
        }

        /**
         * @brief Tests that the bot sleeps for the correct duration between consumption checks
         * 
         * The bot does not constantly check for consumable readiness every millisecond, as that
         * would waste CPU resources. Instead, it calculates how long to wait before the next
         * check based on the configured potion frequency and how much time has already passed
         * in the current cycle.
         */
        private void _testThreadSleep()
        {
            var thread = _fixture();
            thread.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
            _consumableTimestamp.GetTimestampReturn.Add(0.00123);
            thread.Start();
            thread.Join(10000);
            Debug.Assert(_macroSleeper.SleepCalls == 1);
            Debug.Assert(_macroSleeper.SleepCallArg_milliseconds[0] == 288);
        }

        public void Run()
        {
            _testInjectingToHelper();
            _testThreadState();
            _testInjectAction();
            _testThreadLoop();
            _testThreadSleep();
        }
    }


    public class ConsumptionSystemTests : AbstractSystem
    {
        private List<AbstractThreadFactory> _consumptionThreadFactories = [];

        private List<AbstractThread> _consumptionThreads = [];

        private AbstractSystem _fixture()
        {
            _consumptionThreads = [
                new MockThread(new ThreadRunningState()),
                new MockThread(new ThreadRunningState()),
                new MockThread(new ThreadRunningState()),
                new MockThread(new ThreadRunningState()),
            ];
            _consumptionThreadFactories = [
                new MockThreadFactory { CreateThreadReturn = [_consumptionThreads[0]] },
                new MockThreadFactory { CreateThreadReturn = [_consumptionThreads[1]] },
                new MockThreadFactory { CreateThreadReturn = [_consumptionThreads[2]] },
                new MockThreadFactory { CreateThreadReturn = [_consumptionThreads[3]] },
            ];
            return new ConsumptionSystem(_consumptionThreadFactories);
        }

        /**
         * @brief Tests that the consumption system properly creates, starts, and manages all
         * consumption threads
         * 
         * The bot's consumption system is responsible for managing multiple threads that handle
         * different aspects of consumable usage. This test verifies the complete setup of
         * the consumption system.
         */
        private void _testConsumptionSystem()
        {
            var system = _fixture();
            system.Initialize();
            foreach (var factory in _consumptionThreadFactories)
            {
                Debug.Assert(((MockThreadFactory)factory).CreateThreadCalls == 1);
            }
            system.Start();
            foreach (var thread in _consumptionThreads)
            {
                Debug.Assert(((MockThread)thread).ThreadStartCalls == 1);
            }
            system.Inject(123, 234);
            foreach (var thread in _consumptionThreads)
            {
                Debug.Assert(((MockThread)thread).InjectCalls == 1);
                Debug.Assert(((int)((MockThread)thread).InjectCallArg_dataType[0]) == 123);
                Debug.Assert(((int)((MockThread)thread).InjectCallArg_data[0]!) == 234);
            }
        }

        public void Run()
        {
            _testConsumptionSystem();
        }
    }


    public class ConsumptionSystemTestSuite
    {
        public void Run()
        {
            new ResourceStatusScreenCaptureSubscriberTests().Run();
            new ResourceDetectionThresholdTests().Run();
            new ResourceDetectionThreadTests().Run();
            new ConsumptionThreadRefresherTests().Run();
            new ConsumptionQueueUpdaterTests().Run();
            new ResourceExecutorTests().Run();
            new ConsumptionExecutorTests().Run();
            new ConsumptionThreadHelperTests().Run();
            new ConsumptionThreadTests().Run();
            new ConsumptionSystemTests().Run();
        }
    }
}
