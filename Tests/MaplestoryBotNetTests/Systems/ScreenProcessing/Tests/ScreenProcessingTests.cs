using ArrayFireNCC;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.ScreenProcessing.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.ScreenCapture.Tests.Mocks;
using MaplestoryBotNetTests.Systems.ScreenProcessing.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Controls;


namespace MaplestoryBotNetTests.Systems.ScreenProcessing.Tests
{
    public class GameMinimapProcessorTests
    {
        private MockBitmapTemplateMatcher _templateMatcher = new MockBitmapTemplateMatcher();
        private AbstractScreenPositionProcessor _fixture()
        {
            _templateMatcher = new MockBitmapTemplateMatcher();
            return new ScreenPositionProcessor();
        }

        /**
         * @brief Verifies that the processor uses the first match from the template matcher
         * results.
         * 
         * The template matcher returns matches already sorted by confidence (highest first).
         * This test confirms the processor takes the first match from this sorted list without
         * additional filtering or reordering. With matches at (200,200), (100,100), and (300,300),
         * the processor correctly returns the center coordinates (205,205) of the first match.
         */
        private void _testCalculationTakesFirstMatchedResult()
        {
            var processor = _fixture();
            _templateMatcher.calculateBitmapReturn.Add(
                [
                    new Tuple<int, int, int, int, float>(200, 200, 10, 10, 0.9f),
                    new Tuple<int, int, int, int, float>(100, 100, 10, 10, 0.5f),
                    new Tuple<int, int, int, int, float>(300, 300, 10, 10, 0.1f)
                ]
            );
            var returned = processor.Process(
                _templateMatcher, new RectangleMerger(), 0.123f, 0.234f, new Bitmap(2, 2)
            );
            Debug.Assert(returned != null);
            Debug.Assert(returned.Item1 == 205);
            Debug.Assert(returned.Item2 == 205);
        }

        /**
         * @brief Verifies that nearby overlapping matches are merged into a single detection.
         * 
         * Template matching may produce multiple overlapping rectangles around the same
         * character position. The rectangle merger combines overlapping detections based on
         * the overlap threshold (0.234f). This test confirms that two overlapping matches
         * at (200,200) and (202,202) are merged into a single rectangle, and the processor
         * correctly returns the center coordinates (206,206) of the merged result.
         */
        private void _testCalculationOverlapsCloseMatches()
        {
            var processor = _fixture();
            _templateMatcher.calculateBitmapReturn.Add(
                [
                    new Tuple<int, int, int, int, float>(200, 200, 10, 10, 0.9f),
                    new Tuple<int, int, int, int, float>(300, 300, 10, 10, 0.2f),
                    new Tuple<int, int, int, int, float>(202, 202, 10, 10, 0.1f)
                ]
            );
            var returned = processor.Process(
                _templateMatcher, new RectangleMerger(), 0.123f, 0.234f, new Bitmap(2, 2)
            );
            Debug.Assert(returned != null);
            Debug.Assert(returned.Item1 == 206);
            Debug.Assert(returned.Item2 == 206);
        }

        /**
         * @brief Verifies that the processor returns null when no matches are found.
         * 
         * When template matching fails to find any character icon in the minimap screenshot,
         * the processor must return null to indicate that character position could not be
         * determined. This test ensures that with an empty match list from the template matcher,
         * the Process method correctly returns null rather than attempting to calculate
         * coordinates from non-existent data.
         */
        private void _testCalculationReturnsNullForNoMatches()
        {
            var processor = _fixture();
            _templateMatcher.calculateBitmapReturn.Add([]);
            var returned = processor.Process(
                _templateMatcher, new RectangleMerger(), 0.123f, 0.234f, new Bitmap(2, 2)
            );
            Debug.Assert(returned == null);
        }

        /**
         * @brief Verifies that the correct minimap image and threshold are passed to template matching.
         * 
         * The processor must forward the exact minimap screenshot and detection threshold to the
         * template matcher. This test confirms that the bitmap passed into Process() is the same
         * one received by the template matcher's calculate method, and that the threshold value
         * (0.123f) is correctly propagated.
         */
        private void _testCalculationUsesInputSourceAndThreshold()
        {
            var processor = _fixture();
            _templateMatcher.calculateBitmapReturn.Add([]);
            var bitmap = new Bitmap(2, 2);
            processor.Process(
                _templateMatcher, new RectangleMerger(), 0.123f, 0.234f, bitmap
            );
            Debug.Assert(_templateMatcher.calculateBitmapCalls == 1);
            Debug.Assert(_templateMatcher.calculateBitmapCallArg_bitmap[0] == bitmap);
            Debug.Assert(_templateMatcher.calculateBitmapCallArg_threshold[0] == 0.123f);
        }

        public void Run()
        {
            _testCalculationTakesFirstMatchedResult();
            _testCalculationOverlapsCloseMatches();
            _testCalculationReturnsNullForNoMatches();
            _testCalculationUsesInputSourceAndThreshold();
        }
    }
    

    public class GameMinimapProcessorThreadTests
    {
        private MockRunningState _runningState = new MockRunningState();

        private MockResetEvent _resetEvent = new MockResetEvent();

        private MockGameMinimapProcessHandler _processHandler = new MockGameMinimapProcessHandler();

        private MockGameMinimapProcessorThreadStateUpdater _threadStateUpdater = (
            new MockGameMinimapProcessorThreadStateUpdater()
        );

        private List<string> _callOrder = [];

        private GameMinimapProcessorThread _fixture(bool mockUpdater = false)
        {
            _callOrder = [];
            _runningState = new MockRunningState();
            _resetEvent = new MockResetEvent();
            _processHandler = new MockGameMinimapProcessHandler();
            _threadStateUpdater = new MockGameMinimapProcessorThreadStateUpdater();
            _runningState.CallOrder = _callOrder;
            _resetEvent.CallOrder = _callOrder;
            _processHandler.CallOrder = _callOrder;
            _threadStateUpdater.CallOrder = _callOrder;
            return new GameMinimapProcessorThread(
                _runningState,
                new BitmapTemplateMatcherBuilder(),
                new RectangleMerger(),
                new ImageCropper(new ImageSharpConverter()),
                _processHandler,
                _resetEvent,
                mockUpdater ? _threadStateUpdater : new GameMinimapProcessorThreadStateUpdater(),
                MapIconInfo.Character
            );
        }

        private List<Tuple<Bitmap, bool>> _bitmapCases()
        {
            return [
                new Tuple<Bitmap, bool>(new Bitmap(1, 1), true),
                new Tuple<Bitmap, bool>(new Bitmap(2, 2), true),
                new Tuple<Bitmap, bool>(new Bitmap(2, 2), false),
            ];
        }

        /**
         * @brief Verifies that injecting a MapModel transfers the reference to the thread's state.
         * 
         * The MapModel serves as a shared data structure where the minimap processor writes
         * detected character positions. Other systems like the botting agent read from this
         * model to make navigation decisions. This test ensures the processor thread receives
         * the correct MapModel instance to write position data to, establishing the communication
         * channel between minimap detection other systems.
         */
        private void _testInjectingMapModelUpdatesThreadState()
        {
            var injectTypes = new List<SystemInjectType> {
                SystemInjectType.Configuration, (SystemInjectType) 123
            };
            for (int i = 0; i < injectTypes.Count; i++)
            {
                var fixture = _fixture();
                var mapModel = new BottingModel();
                fixture.Inject(injectTypes[i], mapModel);
                Debug.Assert(
                    injectTypes[i] == SystemInjectType.BottingModel ?
                    fixture.ThreadState.BottingModel == mapModel :
                    fixture.ThreadState.BottingModel == null
                );
            }
        }

        /**
         * @brief Verifies that injecting ConfigurationImages creates a template matcher
         * from the icon image.
         * 
         * The character icon image must be converted into a template matcher that can
         * scan minimap screenshots for matching patterns. This test confirms the icon
         * pixels are correctly preserved during conversion, ensuring accurate detection
         * regardless of icon appearance.
         */
        private void _testInjectingTemplateImageUpdatesThreadState()
        {
            var injectTypes = new List<SystemInjectType> {
                SystemInjectType.Configuration, (SystemInjectType) 123
            };
            var image = new SixLabors.ImageSharp.Image<Bgra32>(2, 2);
            image[0, 0] = new Bgra32(255, 0, 0, 0);
            image[0, 1] = new Bgra32(0, 255, 0, 0);
            image[1, 0] = new Bgra32(0, 0, 255, 0);
            image[1, 1] = new Bgra32(0, 0, 0, 255);
            for (int i = 0; i < injectTypes.Count; i++)
            {
                var mapIconImages = new Dictionary<string, SixLabors.ImageSharp.Image<Bgra32>>();
                var configurationImages = new ConfigurationImages { MapIconImages = mapIconImages };
                var fixture = _fixture();
                mapIconImages[MapIconInfo.Character] = image;
                fixture.Inject(injectTypes[i], configurationImages);
                if (injectTypes[i] == SystemInjectType.Configuration)
                {
                    Debug.Assert(fixture.ThreadState.TemplateMatcher != null);
                    var templates = fixture.ThreadState.TemplateMatcher.get_templates();
                    Debug.Assert(templates.Count == 1);
                    Debug.Assert(templates[0].GetPixel(0, 0) == Color.FromArgb(0, 255, 0, 0));
                    Debug.Assert(templates[0].GetPixel(0, 1) == Color.FromArgb(0, 0, 255, 0));
                    Debug.Assert(templates[0].GetPixel(1, 0) == Color.FromArgb(0, 0, 0, 255));
                    Debug.Assert(templates[0].GetPixel(1, 1) == Color.FromArgb(255, 0, 0, 0));
                }
                else
                {
                    Debug.Assert(fixture.ThreadState.TemplateMatcher == null);
                }
            }
        }

        /**
         * @brief Verifies that injecting MaplestoryBotConfiguration updates the thread's
         * MapIcon reference with the character's detection parameters.
         * 
         * Each character icon requires specific detection parameters like frequency and
         * overlap thresholds. This test ensures those parameters are correctly loaded
         * into the processing thread, enabling character-specific detection behavior
         * without hardcoding values.
         */
        private void _testInjectingIconConfigurationUpdatesThreadState()
        {
            var injectTypes = new List<SystemInjectType> {
                SystemInjectType.Configuration, (SystemInjectType) 123
            };
            for (int i = 0; i < injectTypes.Count; i++)
            {
                var mapIconDict = new Dictionary<string, MapIcon>();
                var mapIconData = new MapIcon();
                var configuration = new MaplestoryBotConfiguration { MapIcons = mapIconDict };
                var fixture = _fixture();
                mapIconDict[MapIconInfo.Character] = mapIconData;
                fixture.Inject(injectTypes[i], configuration);
                Debug.Assert(
                    injectTypes[i] == SystemInjectType.Configuration ?
                    fixture.ThreadState.MapIcon == mapIconData :
                    fixture.ThreadState.MapIcon == null
                );
            }
        }

        /**
         * @brief Verifies that injecting a valid bitmap triggers threshold calculation
         * using the current map model's template threshold.
         * 
         * Template matching sensitivity must adapt to different minimap backgrounds.
         * When a minimap screenshot arrives, the thread should obtain the appropriate
         * detection threshold based on the current map's configuration, ensuring reliable
         * character detection across different map environments.
         */
        private void _testInjectingValidBitmapUpdatesThresholdValue()
        {
            var bitmaps = _bitmapCases();
            var expected = new List<float?>{null, new float?(0.123f), null};
            for (int j = 0; j < bitmaps.Count; j++)
            {
                var fixture = _fixture();
                var bottingModel = new BottingModel();
                bottingModel.GetMapModel().SetTemplateThreshold(MapIconInfo.Character, 0.123f);
                if (bitmaps[j].Item2)
                {
                    fixture.Inject(SystemInjectType.BottingModel, bottingModel);
                }
                Debug.Assert(fixture.ThreadState.Threshold == null);
                fixture.Inject(0, bitmaps[j].Item1);
                Debug.Assert(
                    (expected[j] == null) ?
                    fixture.ThreadState.Threshold == null :
                    fixture.ThreadState.Threshold!.Value == expected[j]!.Value
                );
            }
        }

        /**
         * @brief Verifies that injecting a valid bitmap updates the thread's CurrentBitmap
         * reference with the new minimap image.
         * 
         * The minimap processing thread must always work with the most recent screenshot
         * to provide accurate character position tracking. This test ensures that when
         * a new bitmap arrives, it becomes available to the processing loop.
         */
        private void _testInjectingValidBitmapUpdatesThreadState()
        {
            var bitmaps = _bitmapCases();
            var expected = new List<Bitmap?>{null, bitmaps[1].Item1, null};
            for (int i = 0; i < bitmaps.Count; i++)
            {
                var fixture = _fixture();
                var mapModel = new BottingModel();
                if (bitmaps[i].Item2)
                {
                    fixture.Inject(SystemInjectType.BottingModel, mapModel);
                }
                Debug.Assert(fixture.ThreadState.CurrentBitmap == null);
                fixture.Inject(0, bitmaps[i].Item1);
                Debug.Assert(fixture.ThreadState.CurrentBitmap == expected[i]);
            }
        }

        /**
         * @brief Verifies that the minimap capture thread only awakens when both a valid
         * screenshot and the map model are present
         * 
         * When the bot is running, the minimap capture thread should only consume CPU cycles
         * when it has both the necessary map configuration (to know where to look) and an
         * actual screenshot to process. Without either component, the thread should remain
         * dormant. This test injects bitmaps with and without a map model to confirm the
         * countdown mechanism only signals the thread when both prerequisites are satisfied.
         */
        private void _testInjectingValidBitmapAwakensThread()
        {
            var bitmaps = _bitmapCases();
            var expected = new List<bool> { false, true, false };
            for (int i = 0; i < bitmaps.Count; i++)
            {
                var fixture = _fixture();
                var mapModel = new BottingModel();
                if (bitmaps[i].Item2)
                {
                    fixture.Inject(SystemInjectType.BottingModel, mapModel);
                }
                Debug.Assert(_resetEvent.SetCalls == 0);
                fixture.Inject(0, bitmaps[i].Item1);
                Debug.Assert(
                    (expected[i] == true) ?
                    _resetEvent.SetCalls == 1 :
                    _resetEvent.SetCalls == 0
                );
            }
        }

        /**
         * @brief Verifies the correct sequence of operations in each thread loop iteration:
         * wait for bitmap, process the image, then update the thread state
         * 
         * When the minimap capture thread receives a new screenshot, it must follow a strict
         * order to ensure accurate character position tracking. First, it must wait at the
         * countdown for a bitmap to be injected. Then it processes the image to detect the
         * character's position on the minimap. Finally, it atomically updates the thread
         * state with the new position. Skipping or reordering these steps could cause
         * racing conditions where stale data is processed or position updates are missed.
         */
        private void _testThreadLoopWaitsBeforeHandlingAndUpdate()
        {
            for (int i = 1; i < 5; i++)
            {
                var fixture = _fixture(true);
                for (int j = 0; j < i + 2; j++)
                {
                    _runningState.IsRunningReturn.Add(j != 0 && j != i + 1);
                }
                var utilities = new TestUtilities();
                var runningStateRef = utilities.Reference(_runningState);
                var resetEventRef = utilities.Reference(_resetEvent);
                var processHandlerRef = utilities.Reference(_processHandler);
                var threadStateUpdaterRef = utilities.Reference(_threadStateUpdater);
                fixture.Start();
                fixture.Join(10000);
                Debug.Assert(_callOrder.Count == (4 * i) + 3);
                Debug.Assert(_resetEvent.WaitOneCalls == i);
                Debug.Assert(_processHandler.HandleCalls == i);
                Debug.Assert(_threadStateUpdater.AtomicUpdateCalls == i);
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(_callOrder[4 * j + 3] == resetEventRef + "WaitOne");
                    Debug.Assert(_callOrder[4 * j + 4] == processHandlerRef + "Handle");
                    Debug.Assert(_callOrder[4 * j + 5] == threadStateUpdaterRef + "AtomicUpdate");
                }
            }
        }

        /**
         * @brief Verifies that the minimap capture thread correctly waits for a signal
         * before processing each screenshot
         * 
         * The minimap capture thread uses an AutoResetEvent to synchronize with the screen
         * capture system. On each iteration of the thread loop, it must wait for the event
         * to be signaled before attempting to process a new minimap screenshot. This
         * ensures the thread only consumes CPU cycles when new image data is available,
         * rather than spinning continuously or processing stale data.
         */
        private void _testThreadLoopWaitsOne()
        {
            for (int i = 1; i < 5; i++)
            {
                var fixture = _fixture(true);
                for (int j = 0; j < i + 2; j++)
                {
                    _runningState.IsRunningReturn.Add(j != 0 && j != i + 1);
                }
                fixture.Start();
                fixture.Join(10000);
                Debug.Assert(_resetEvent.WaitOneCalls == i);
            }
        }

        /**
         * @brief Verifies that each processing iteration uses the current thread state.
         * 
         * The minimap processor must always operate on the most up-to-date configuration,
         * including the current map model, detection thresholds, and template matcher.
         * This test ensures the process handler receives the exact thread state instance
         * that contains all latest settings for each iteration, guaranteeing that
         * character detection always reflects the current bot configuration and map data.
         */
        private void _testThreadLoopProcessesWithCurrentThreadState()
        {
            for (int i = 1; i < 5; i++)
            {
                var fixture = _fixture(true);
                for (int j = 0; j < i + 2; j++)
                {
                    _runningState.IsRunningReturn.Add(j != 0 && j != i + 1);
                }
                fixture.Start();
                fixture.Join(10000);
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(
                        _processHandler.HandleCallArg_threadState[j] == fixture.ThreadState
                    );
                }
            }
        }

        /**
         * @brief Verifies that the thread clears the current bitmap after each processing iteration.
         * 
         * After processing a minimap screenshot, the bitmap must be cleared to prevent stale data
         * from being processed again in the next iteration. This ensures that when the countdown
         * mechanism triggers the next cycle, the thread will only process if a fresh bitmap has
         * been injected. Without this reset, the thread might repeatedly process the same old
         * image, causing incorrect position tracking or unnecessary CPU usage.
         */
        private void _testThreadLoopResetsCurrentBitmap()
        {
            for (int i = 1; i < 5; i++)
            {
                var fixture = _fixture(true);
                for (int j = 0; j < i + 2; j++)
                {
                    _runningState.IsRunningReturn.Add(j != 0 && j != i + 1);
                }
                fixture.Start();
                fixture.Join(10000);
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(
                        _threadStateUpdater.AtomicUpdateCallArg_updateObject[j]!.CurrentBitmap == null
                    );
                }
            }
        }

        public void Run()
        {
            _testInjectingMapModelUpdatesThreadState();
            _testInjectingTemplateImageUpdatesThreadState();
            _testInjectingIconConfigurationUpdatesThreadState();
            _testInjectingValidBitmapUpdatesThresholdValue();
            _testInjectingValidBitmapUpdatesThreadState();
            _testInjectingValidBitmapAwakensThread();
            _testThreadLoopWaitsBeforeHandlingAndUpdate();
            _testThreadLoopWaitsOne();
            _testThreadLoopProcessesWithCurrentThreadState();
            _testThreadLoopResetsCurrentBitmap();
        }
    }


    public class GameMinimapProcessHandlerTests
    {
        private MockGameMinimapPositionProcessor _positionProcessor = new MockGameMinimapPositionProcessor();

        private MockTimestamp _timestamp = new MockTimestamp();

        private GameMinimapProcessorThreadState _threadState = new GameMinimapProcessorThreadState();

        private MockWindowStateModifier _positionUpdater = new MockWindowStateModifier();

        private BottingModel _bottingModel = new BottingModel();

        private MapIcon _mapIcon = new MapIcon();

        private AbstractGameMinimapProcessHandler _fixture()
        {
            _positionProcessor = new MockGameMinimapPositionProcessor();
            _positionUpdater = new MockWindowStateModifier();
            _timestamp = new MockTimestamp();
            _bottingModel = new BottingModel();
            _mapIcon = new MapIcon();
            _threadState = new GameMinimapProcessorThreadState(
                "lol",
                new Bitmap(2, 2),
                new MockBitmapTemplateMatcher(),
                new RectangleMerger(),
                _mapIcon,
                _bottingModel,
                0.123f,
                _positionUpdater
            );
            return new GameMinimapProcessHandler(_timestamp, _positionProcessor);
        }

        /**
         * @brief Verifies that the handler updates character position in the map model when
         * a valid detection is made, or sets invalid coordinates (-1,-1) when detection fails.
         * 
         * When template matching successfully finds the character icon, the handler calculates
         * the center coordinates (123,234) and updates the position modifier with these coordinates
         * along with the map model reference. When detection fails (null result), the handler
         * still invokes the position modifier but with coordinates (-1,-1) to indicate the
         * character could not be located on the minimap.
         */
        private void _testHandlerExecutesPositionModification()
        {
            var processReturn = new List<Tuple<int, int>?>
            {
                new Tuple<int, int>(123, 234),
                null
            };
            for (int i = 0; i < processReturn.Count; i++)
            {
                var processHandler = _fixture();
                _timestamp.GetTimestampReturn.Add(999.0);
                _mapIcon.Frequency = 0.1f;
                _positionProcessor.ProcessReturn.Add(processReturn[i]!);
                processHandler.Handle(_threadState);
                Debug.Assert(_positionUpdater.ModifyCalls == 1);
                var parameters = (
                    (WindowMinimapPositionModifierParameters)
                    _positionUpdater.ModifyCallArg_value[0]!
                );
                Debug.Assert(parameters.Model == _bottingModel.GetMapModel());
                Debug.Assert(
                    processReturn[i] != null ?
                    parameters.Position.Item1 == 123 :
                    parameters.Position.Item1 == -1
                );
                Debug.Assert(
                    processReturn[i] != null ?
                    parameters.Position.Item2 == 234 :
                    parameters.Position.Item2 == -1
                );
            }
        }

        /**
         * @brief Verifies that the handler respects the frequency limit between position updates.
         * 
         * The map icon configuration specifies a frequency (0.1f) that determines the minimum time
         * between position updates (10 seconds). When insufficient time has elapsed since the last
         * update (GetTimestamp returns 5.0), the handler should skip position processing entirely
         * without invoking the position modifier.
         */
        private void _testHandlerDoesNotUpdateIfNotEnoughTimeHasElapsed()
        {
            var processHandler = _fixture();
            _timestamp.GetTimestampReturn.Add(5.0);
            _mapIcon.Frequency = 0.1f;
            processHandler.Handle(_threadState);
            Debug.Assert(_positionUpdater.ModifyCalls == 0);
        }

        /**
         * @brief Verifies that a frequency of zero disables throttling and always processes updates.
         * 
         * Some map icons or debugging scenarios may require position updates on every available
         * minimap frame without any time restriction. Setting frequency to 0.0f indicates no
         * minimum interval should be enforced. This test confirms that even with a small elapsed
         * time (0.00001), the handler still processes the position update when frequency is zero.
         */
        private void _testHandlerUpdatesWhenFrequencyIsZero()
        {
            var processHandler = _fixture();
            _timestamp.GetTimestampReturn.Add(0.00001);
            _mapIcon.Frequency = 0.0f;
            _positionProcessor.ProcessReturn.Add(new Tuple<int, int>(123, 234));
            processHandler.Handle(_threadState);
            Debug.Assert(_positionUpdater.ModifyCalls == 1);
        }

        /**
         * @brief Verifies the correct sequence of operations during position processing.
         * 
         * The handler must execute operations in a specific order to maintain correct timing
         * behavior: first check the current timestamp, then record the new timestamp (marking
         * the start of this processing cycle), perform template matching to find the position,
         * and finally update the position modifier with the results.
         */
        private void _testHandlerSetsTimestampBeforeProcessAndUpdate()
        {
            var processHandler = _fixture();
            var callOrder = new List<string>();
            _timestamp.CallOrder = callOrder;
            _positionProcessor.CallOrder = callOrder;
            _positionUpdater.CallOrder = callOrder;
            _timestamp.GetTimestampReturn.Add(999.0);
            _mapIcon.Frequency = 0.1f;
            _positionProcessor.ProcessReturn.Add(new Tuple<int, int>(123, 234));
            processHandler.Handle(_threadState);
            Debug.Assert(_positionUpdater.ModifyCalls == 1);
            Debug.Assert(callOrder.Count == 4);
            Debug.Assert(callOrder[0] == new TestUtilities().Reference(_timestamp) + "GetTimestamp");
            Debug.Assert(callOrder[1] == new TestUtilities().Reference(_timestamp) + "SetTimestamp");
            Debug.Assert(callOrder[2] == new TestUtilities().Reference(_positionProcessor) + "Process");
            Debug.Assert(callOrder[3] == new TestUtilities().Reference(_positionUpdater) + "Modify");
        }

        public void Run()
        {
            _testHandlerExecutesPositionModification();
            _testHandlerDoesNotUpdateIfNotEnoughTimeHasElapsed();
            _testHandlerSetsTimestampBeforeProcessAndUpdate();
            _testHandlerUpdatesWhenFrequencyIsZero();
        }
    }


    public class GameMinimapCharacterProcessorThreadTests
    {
        private static AbstractThread _fixture()
        {
            return new GameMinimapCharacterProcessorThreadFactory().CreateThread();
        }

        /**
         * @brief Verifies that injecting a character position action handler transfers the 
         * position modifier to the thread's state.
         * 
         * The character position action handler contains a modifier component that actually
         * writes detected character coordinates to the map model. When the action handler is
         * injected with SystemInjectType.ActionHandler, its modifier must be extracted and
         * stored in the thread state as the PositionUpdater. This establishes the connection
         * between minimap detection results and the UI/navigation systems that consume them.
         */
        public void _testInjectingCharacterPositionHandlerUpdatesThreadState()
        {
            var injectTypes = new List<SystemInjectType> {
                SystemInjectType.ActionHandler, (SystemInjectType) 123
            };
            for (int i = 0; i < injectTypes.Count; i++)
            {
                var actionHandler = new WindowMinimapPositionActionHandlerFacade(
                    new MockDispatcher(),
                    new TextBox(),
                    new TextBox(),
                    MapIconInfo.Character,
                    new System.Windows.Controls.Image()
                );
                var fixture = _fixture();
                fixture.Inject(injectTypes[i], actionHandler);
                var threadState = (GameMinimapProcessorThreadState)fixture.State()!;
                Debug.Assert(fixture.State() is GameMinimapProcessorThreadState);
                Debug.Assert(
                    injectTypes[i] == SystemInjectType.ActionHandler ?
                    threadState.PositionUpdater == actionHandler.Modifier() :
                    threadState.PositionUpdater == null
                );
            }
        }

        public void Run()
        {
            _testInjectingCharacterPositionHandlerUpdatesThreadState();
        }
    }


    public class GameMinimapRuneProcessorThreadTests
    {
        private static AbstractThread _fixture()
        {
            return new GameMinimapRuneProcessorThreadFactory().CreateThread();
        }

        /**
         * @brief Verifies that injecting a rune position action handler transfers the 
         * position modifier to the thread's state.
         * 
         * Similar to character position handling, the rune position action handler contains
         * a modifier that writes detected rune coordinates to the map model. When injected
         * with SystemInjectType.ActionHandler, its modifier must be extracted and stored
         * as the PositionUpdater in the thread state. This allows the rune detector to
         * output its findings to the shared map model.
         */
        public void _testInjectingRunePositionHandlerUpdatesThreadState()
        {
            var injectTypes = new List<SystemInjectType> {
                SystemInjectType.ActionHandler, (SystemInjectType) 123
            };
            for (int i = 0; i < injectTypes.Count; i++)
            {
                var actionHandler = new WindowMinimapPositionActionHandlerFacade(
                    new MockDispatcher(),
                    new TextBox(),
                    new TextBox(),
                    MapIconInfo.Rune,
                    new System.Windows.Controls.Image()
                );
                var fixture = _fixture();
                fixture.Inject(injectTypes[i], actionHandler);
                var threadState = (GameMinimapProcessorThreadState)fixture.State()!;
                Debug.Assert(fixture.State() is GameMinimapProcessorThreadState);
                Debug.Assert(
                    injectTypes[i] == SystemInjectType.ActionHandler ?
                    threadState.PositionUpdater == actionHandler.Modifier() :
                    threadState.PositionUpdater == null
                );
            }
        }

        public void Run()
        {
            _testInjectingRunePositionHandlerUpdatesThreadState();
        }
    }

    public class ScreenProcesstingTestSuite
    {
        public void Run()
        {
            new GameMinimapProcessorThreadTests().Run();
            new GameMinimapCharacterProcessorThreadTests().Run();
            new GameMinimapRuneProcessorThreadTests().Run();
            new GameMinimapProcessorTests().Run();
            new GameMinimapProcessHandlerTests().Run();
        }
    }
}
