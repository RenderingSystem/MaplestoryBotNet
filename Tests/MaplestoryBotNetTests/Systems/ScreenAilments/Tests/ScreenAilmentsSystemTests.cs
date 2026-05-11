using ArrayFireNCC;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.ScreenAilmentsProcessing;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.ScreenAilments.Tests.Mocks;
using MaplestoryBotNetTests.Systems.ScreenProcessing.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.ScreenAilments.Tests
{
    public class ScreenAilmentDetectionThreadHelperTests
    {
        private MockTimestamp _detectionStopwatch = new MockTimestamp();

        private MockBitmapTemplateMatcher _templateMatcher = new MockBitmapTemplateMatcher();

        private AbstractScreenAilmentDetectionHelper _fixture()
        {
            _templateMatcher = new MockBitmapTemplateMatcher();
            _detectionStopwatch = new MockTimestamp();
            return new ScreenAilmentDetectionThreadHelper(
                _templateMatcher,
                _detectionStopwatch
            );
        }

        /**
         * @brief Verifies that the ailment detection helper only processes detection
         * when the configured cooldown delay has elapsed since the last check
         * 
         * When users have configured an active delay for a status ailment (e.g., wait
         * 10 seconds before checking again), the helper must check the elapsed time
         * since the last detection attempt. If not enough time has passed, the check
         * should be skipped to prevent spamming detection and excessive GPU usage.
         */
        private void _testAilmentDetectionChecksTimeDelay()
        {
            var helper = _fixture();
            foreach (var timestamp in new[] { 9.99, 10.0, 1.01 } )
            {
                _detectionStopwatch.GetTimestampReturn.Add(timestamp);
                Debug.Assert(helper.ShouldCheck(10.0f) == (timestamp > 10.0f));
            }
        }


        /**
         * @brief Verifies that the ailment detection helper updates the timestamp
         * whenever a detection scan is performed.
         * 
         * When the helper performs an ailment detection scan, it must record the current
         * timestamp to track when the last check occurred. This timestamp is used by the
         * ShouldCheck method to enforce the active delay cooldown period, preventing
         * excessive detection scans that would waste GPU resources.
         */
        private void _testAilmentDetectionSetsTimestamp()
        {
            var helper = _fixture();
            var image = new Image<Bgra32>(4, 3);
            var expected = new List<Tuple<int, int, int, int, float>>();
            _templateMatcher.calculatePointerReturn.Add(expected);
            helper.AilmentDetected(image, 0.123f);
            Debug.Assert(_detectionStopwatch.SetTimestampCalls == 1);
        }


        /**
         * @brief Verifies that the ailment detection helper correctly converts the
         * Image<Bgra32> to a raw pixel pointer and passes it to the template matcher
         * with the correct dimensions, stride, and threshold
         * 
         * When performing ailment detection on a captured screen image, the helper must
         * convert the SixLabors.ImageSharp image to a raw BGRA32 pixel buffer that the
         * template matcher can consume. The test uses a 4x3 test image with sequential
         * pixel values to verify that the pixel data is correctly converted and passed
         * to the matcher. The pixel order must be BGRA with stride equal to width
         * (4 bytes per pixel).
         */
        private void _testAilmentDetectionCalculatesMatches()
        {
            var helper = _fixture();
            var image = new Image<Bgra32>(4, 3);
            byte pixelData = 0;
            for (var y = 0; y < 3; y++)
            for (var x = 0; x < 4; x++)
            {
                image[x, y] = new Bgra32(
                    pixelData++, // Red
                    pixelData++, // Blue
                    pixelData++, // Green
                    pixelData++  // Alpha
                );
            }
            var expected = new List<Tuple<int, int, int, int, float>>();
            _templateMatcher.calculatePointerReturn.Add(expected);
            var result = helper.AilmentDetected(image, 0.123f);
            Debug.Assert(_templateMatcher.calculatePointerCalls == 1);
            unsafe
            {
                pixelData = 0;
                var callImage = (uint*)_templateMatcher.calculatePointerCallArg_image[0];
                for (int y = 0; y < 3; y++)
                for (int x = 0; x < 4; x++)
                {
                    var pixel = callImage[(y * 4) + x];
                    var b = (pixel & 0x00FF0000) >> 0x10;
                    var g = (pixel & 0x0000FF00) >> 0x08;
                    var r = (pixel & 0x000000FF) >> 0x00;
                    var a = (pixel & 0xFF000000) >> 0x18;
                    Debug.Assert(b == pixelData++);
                    Debug.Assert(g == pixelData++);
                    Debug.Assert(r == pixelData++);
                    Debug.Assert(a == pixelData++);
                }
            }
            Debug.Assert(_templateMatcher.calculatePointerCallArg_image_width[0] == 4);
            Debug.Assert(_templateMatcher.calculatePointerCallArg_image_height[0] == 3);
            Debug.Assert(_templateMatcher.calculatePointerCallArg_image_stride[0] == 4);
            Debug.Assert(_templateMatcher.calculatePointerCallArg_threshold[0] == 0.123f);
        }

        /**
         * @brief Verifies that the ailment detection helper correctly returns the match
         * results from the template matcher without modification
         * 
         * When the template matcher successfully detects ailment patterns in the screen
         * capture, it returns a list of matches containing the location, dimensions,
         * and confidence score for each detected instance. The helper must pass these
         * results through to the caller unchanged, allowing the detection thread to
         * determine whether the ailment is present.
         */
        private void _testAilmentDetectionReturnsMatches()
        {
            var helper = _fixture();
            var image = new Image<Bgra32>(4, 3);
            var expected = new List<Tuple<int, int, int, int, float>>();
            _templateMatcher.calculatePointerReturn.Add(expected);
            var result = helper.AilmentDetected(image, 0.123f);
            Debug.Assert(expected == result);
        }


        public void Run()
        {
            _testAilmentDetectionChecksTimeDelay();
            _testAilmentDetectionSetsTimestamp();
            _testAilmentDetectionCalculatesMatches();
            _testAilmentDetectionReturnsMatches();
        }
    }


    public class ScreenAilmentDetectionThreadTests
    {
        private string _ailmentKey = "";

        private Ailment _ailment = new Ailment();

        private AbstractBottingModel _bottingModel = new BottingModel();

        private MockResetEvent _mockResetEvent = new MockResetEvent();

        private MockScreenAilmentDetectionHelper _helper = new MockScreenAilmentDetectionHelper();

        private MockRunningState _runningState = new MockRunningState();

        private Image<Bgra32> _image = new Image<Bgra32>(1, 1);

        private AbstractThread _fixture()
        {
            _ailmentKey = "meow";
            _ailment = new Ailment
            {
                CheckDelay = 234,
                Threshold = 345
            };
            _bottingModel = new BottingModel();
            _mockResetEvent = new MockResetEvent();
            _helper = new MockScreenAilmentDetectionHelper();
            _image = new Image<Bgra32>(1, 1);
            _runningState = new MockRunningState();
            _runningState.IsRunningReturn.Add(false);
            _mockResetEvent.CallOrder = _helper.CallOrder;
            var thread = new ScreenAilmentDetectionThread(
                _ailmentKey,
                _ailment,
                _bottingModel,
                _mockResetEvent,
                _helper,
                _runningState
            );
            return thread;
        }

        /**
         * @brief Verifies that injecting a new screen image triggers the reset event,
         * waking up the detection thread from its wait state
         * 
         * When the screen capture system captures a new frame, it injects the image into
         * the detection thread. The thread waits on a reset event until a new image
         * arrives. This test ensures that the event is signaled exactly once when an
         * image is injected, allowing the thread to process the new frame.
         */
        private void _testInjectingImageSetsAutoEvent()
        {
            var thread = _fixture();
            thread.Inject(0, _image);
            Debug.Assert(_mockResetEvent.SetCalls == 1);
        }

        /**
         * @brief Verifies the complete detection flow for active ailments, confirming
         * the correct sequence and count of operations when matches are found
         * 
         * When an ailment is active and enough time has passed since the last check,
         * the thread should: wait for an image, check if enough time has elapsed,
         * then perform detection on the image. This test runs multiple iterations
         * (1-9) to verify the pattern repeats correctly and the call order matches
         * the expected sequence.
         */
        private void _testThreadDetectsAilmentStatus()
        {
            for (int i = 1; i < 10; i++)
            {
                var thread = _fixture();
                var resetRef = new TestUtilities().Reference(_mockResetEvent);
                var helperRef = new TestUtilities().Reference(_helper);
                var ailmentsModel = _bottingModel.GetAilmentsModel();
                var detectedAilments = new List<Tuple<int, int, int, int, float>>();
                _ailment.Active = 1;
                for (int j = 0; j < i; j++)
                {
                    _runningState.IsRunningReturn.Add(true);
                    _helper.ShouldCheckReturn.Add(true);
                    _helper.AilmentDetectedReturn.Add(detectedAilments);
                }
                _runningState.IsRunningReturn.Add(false);
                thread.Inject(0, _image);
                _mockResetEvent.CallOrder.Clear();
                thread.Start();
                thread.Join(10000);
                var expectedOrder = new List<string>();
                for (int j = 0; j < i; j++)
                {
                    expectedOrder.Add(resetRef + "WaitOne");
                    expectedOrder.Add(helperRef + "ShouldCheck");
                    expectedOrder.Add(helperRef + "AilmentDetected");
                }
                for (int j = 0; j < expectedOrder.Count; j++)
                {
                    Debug.Assert(_helper.CallOrder[j] == expectedOrder[j]);
                }
            }
        }

        /**
         * @brief Verifies that the detection thread completely skips processing for
         * ailments marked as inactive, regardless of other conditions
         * 
         * When an ailment is configured as inactive (Active = 0), the bot should not
         * attempt to detect or respond to that ailment, even if a new screen image
         * arrives. This allows users to disable specific ailments without removing
         * them from configuration.
         */
        private void _testThreadDoesntCheckInactiveAilments()
        {
            for (int i = 1; i < 10; i++)
            {
                var thread = _fixture();
                var resetRef = new TestUtilities().Reference(_mockResetEvent);
                var helperRef = new TestUtilities().Reference(_helper);
                var ailmentsModel = _bottingModel.GetAilmentsModel();
                var detectedAilments = new List<Tuple<int, int, int, int, float>>();
                _ailment.Active = 0;
                for (int j = 0; j < i; j++)
                {
                    _runningState.IsRunningReturn.Add(true);
                    _helper.ShouldCheckReturn.Add(true);
                    _helper.AilmentDetectedReturn.Add(detectedAilments);
                }
                _runningState.IsRunningReturn.Add(false);
                thread.Inject(0, _image);
                thread.Start();
                thread.Join(10000);
                Debug.Assert(_helper.CallOrder.Count == i + 1);
                Debug.Assert(_mockResetEvent.WaitOneCalls == i);
            }
        }

        /**
         * @brief Verifies that the detection thread respects the check delay cooldown,
         * skipping detection when not enough time has elapsed since the last check
         * 
         * When the configured check delay period has not yet elapsed since the last
         * detection attempt, the thread should skip the detection step to prevent
         * excessive CPU/GPU usage. The thread still waits for the reset event and
         * checks the elapsed time, but only proceeds to detection when the cooldown
         * has expired.
         */
        private void _testThreadDoesntCheckWhenLessThanCheckDelay()
        {
            for (int i = 1; i < 10; i++)
            {
                var thread = _fixture();
                var resetRef = new TestUtilities().Reference(_mockResetEvent);
                var helperRef = new TestUtilities().Reference(_helper);
                var ailmentsModel = _bottingModel.GetAilmentsModel();
                var detectedAilments = new List<Tuple<int, int, int, int, float>>();
                _ailment.Active = 1;
                for (int j = 0; j < i; j++)
                {
                    _runningState.IsRunningReturn.Add(true);
                    _helper.ShouldCheckReturn.Add(false);
                    _helper.AilmentDetectedReturn.Add(detectedAilments);
                }
                _runningState.IsRunningReturn.Add(false);
                thread.Inject(0, _image);
                thread.Start();
                thread.Join(10000);
                Debug.Assert(_helper.CallOrder.Count == 2 * i + 1);
                Debug.Assert(_mockResetEvent.WaitOneCalls == i);
                Debug.Assert(_helper.ShouldCheckCalls == i);
            }
        }

        /**
         * @brief Verifies that detected ailments are correctly recorded in the
         * botting model with the number of matches found
         * 
         * When the template matcher finds one or more matches for an ailment, the
         * detection thread must update the botting model's ailment status with the
         * count of detected matches. This allows the rest of the bot to respond
         * appropriately (e.g., triggering macro commands, pressing cure keys).
         */
        private void _testThreadSetsAilmentStatus()
        {
            for (int i = 1; i < 10; i++)
            {
                var thread = _fixture();
                var ailmentsModel = _bottingModel.GetAilmentsModel();
                var detectedAilments = new List<Tuple<int, int, int, int, float>>();
                _ailment.Active = 1;
                _runningState.IsRunningReturn.Add(true);
                _runningState.IsRunningReturn.Add(false);
                _helper.ShouldCheckReturn.Add(true);
                for (int j = 0; j < i; j++)
                {
                    detectedAilments.Add(
                        new Tuple<int, int, int, int, float>(0, 1, 2, 3, 4.0f)
                    );
                }
                _helper.AilmentDetectedReturn.Add(detectedAilments);
                thread.Inject(0, _image);
                thread.Start();
                thread.Join(10000);
                Debug.Assert(_helper.ShouldCheckCallArg_checkDelay[0] == 234 / 1000.0f);
                Debug.Assert(_helper.AilmentDetectedCallArg_image[0] == _image);
                Debug.Assert(_helper.AilmentDetectedCallArg_threshold[0] == 345 / 1000.0f);
                Debug.Assert(ailmentsModel.GetAilment(_ailmentKey) == i);
            }
        }

        /**
         * @brief Verifies that dynamic configuration updates correctly update the
         * ailment's detection parameters (check delay and threshold) at runtime
         * 
         * When users modify the ailment configuration while the bot is running,
         * the detection thread must apply the new settings without requiring a
         * restart. This test injects a new configuration with updated CheckDelay
         * (123ms) and Threshold (234), then verifies subsequent detection calls
         * use the new values.
         */
        private void _testInjectingConfigurationUpdatesAilment()
        {
            var thread = _fixture();
            var ailmentsModel = _bottingModel.GetAilmentsModel();
            var detectedAilments = new List<Tuple<int, int, int, int, float>>();
            var dict = new Dictionary<string, Ailment>();
            dict["meow"] = new Ailment { Active = 1, CheckDelay = 123, Threshold = 234 };
            _ailment.Active = 1;
            _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
            _helper.ShouldCheckReturn.Add(true);
            _helper.AilmentDetectedReturn.Add(detectedAilments);
            thread.Inject(0, _image);
            thread.Inject(
                SystemInjectType.ConfigurationUpdate,
                new MaplestoryBotConfiguration { Ailments = dict }
            );
            thread.Start();
            thread.Join(10000);
            Debug.Assert(_helper.ShouldCheckCallArg_checkDelay[0] == 123 / 1000.0f);
            Debug.Assert(_helper.AilmentDetectedCallArg_threshold[0] == 234 / 1000.0f);
        }

        /**
         * @brief Verifies that dynamic configuration updates correctly deactivate
         * an ailment when Active flag is set to false
         * 
         * When users disable an ailment in the configuration while the bot is running,
         * the detection thread must immediately stop processing that ailment. This
         * test injects a configuration with Active = 0 and verifies that no detection
         * calls occur afterward, even though the thread continues to receive images.
         */
        private void _testInjectingConfigurationUpdatesAilmentActive()
        {
            var thread = _fixture();
            var ailmentsModel = _bottingModel.GetAilmentsModel();
            var detectedAilments = new List<Tuple<int, int, int, int, float>>();
            var dict = new Dictionary<string, Ailment>();
            dict["meow"] = new Ailment { Active = 0, CheckDelay = 123, Threshold = 234 };
            _ailment.Active = 1;
            _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
            _helper.ShouldCheckReturn.Add(true);
            _helper.AilmentDetectedReturn.Add(detectedAilments);
            thread.Inject(0, _image);
            thread.Inject(
                SystemInjectType.ConfigurationUpdate,
                new MaplestoryBotConfiguration { Ailments = dict }
            );
            thread.Start();
            thread.Join(10000);
            Debug.Assert(_helper.ShouldCheckCalls == 0);
            Debug.Assert(_helper.AilmentDetectedCalls == 0);
        }

        public void Run()
        {
            _testInjectingImageSetsAutoEvent();
            _testThreadDetectsAilmentStatus();
            _testThreadDoesntCheckInactiveAilments();
            _testThreadDoesntCheckWhenLessThanCheckDelay();
            _testThreadSetsAilmentStatus();
            _testInjectingConfigurationUpdatesAilment();
            _testInjectingConfigurationUpdatesAilmentActive();
        }
    }


    public class ImagesharpTemplateMatcherBuilderTests
    {
        private MockBitmapTemplateMatcherBuilder _bitmapTemplateMatcherBuilder = (
            new MockBitmapTemplateMatcherBuilder()
        );

        private AbstractImageCropper _imageCropper = (
            new ImageCropper(new ImageSharpConverter())
        );

        public AbstractTemplateMatcherBuilder _fixture()
        {
            _bitmapTemplateMatcherBuilder = new MockBitmapTemplateMatcherBuilder();
            _imageCropper = new ImageCropper(new ImageSharpConverter());
            return new ImagesharpTemplateMatcherBuilder(
                _imageCropper,
                _bitmapTemplateMatcherBuilder
            );
        }

        private List<Image<Bgra32>> _imagesFixture(byte pixelByte)
        {
            var images = new List<Image<Bgra32>>();
            for (int i = 0; i < 3; i++)
            {
                images.Add(new Image<Bgra32>(2, 2));
                for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                {
                    images[i][x, y] = new Bgra32(
                        pixelByte++,
                        pixelByte++,
                        pixelByte++,
                        pixelByte++
                    );
                }
            }
            return images;
        }

        private ConfigurationImages _configurationFixture()
        {
            return new ConfigurationImages
            {
                AilmentImages =
                {
                    ["meow0"] = _imagesFixture(0),
                    ["meow1"] = _imagesFixture(1)
                }
            };
        }

        /**
         * @brief Verifies that the Imagesharp template matcher builder correctly constructs
         * a bitmap template matcher by processing the image frames through the pipeline
         * 
         * When building a template matcher for ailment detection, the builder must take
         * the configuration images, convert them to the appropriate format using the
         * image cropper, and delegate to the underlying bitmap template matcher builder.
         * This test ensures that the builder correctly calls the with_templates and build
         * methods in sequence and returns the constructed matcher.
         */
        private void _testBuildingImagesharpTemplateMatcher()
        {
            var builder = _fixture();
            var expected = new MockBitmapTemplateMatcher();
            var callOrder = _bitmapTemplateMatcherBuilder.CallOrder;
            var builderRef = new TestUtilities().Reference(_bitmapTemplateMatcherBuilder);
            _bitmapTemplateMatcherBuilder.BuildReturn.Add(expected);
            var result = builder
                .WithArg(_configurationFixture())
                .WithArg("meow0")
                .Build();
            Debug.Assert(callOrder.Count == 2);
            Debug.Assert(callOrder[0] == builderRef + "with_templates");
            Debug.Assert(callOrder[1] == builderRef + "build");
            Debug.Assert(result == expected);
        }

        /**
         * @brief Verifies that the image frames passed to the template matcher builder
         * contain the correct pixel data after conversion from ImageSharp format
         * 
         * When building a template matcher, the builder must convert each Image<Bgra32>
         * frame to a System.Drawing.Bitmap that the underlying template matcher can
         * consume. This test ensures the pixel data survives the conversion process intact,
         * preserving the original RGBA values from the test images. The test uses a 2x2
         * pixel pattern per frame, with sequential pixel values to verify correct conversion.
         */
        private void _testBuildingImagesharpTemplateMatcherFrames()
        {
            var builder = _fixture();
            var expected = new MockBitmapTemplateMatcher();
            _bitmapTemplateMatcherBuilder.BuildReturn.Add(expected);
            builder
                .WithArg(_configurationFixture())
                .WithArg("meow0")
                .Build();
            var templates = _bitmapTemplateMatcherBuilder.WithTemplatesCallArg_templates[0];
            Debug.Assert(templates.Count == 3);
            byte pixelByte = 0;
            for (int i = 0; i < 3; i++)
            for (int y = 0; y < 2; y++)
            for (int x = 0; x < 2; x++)
            {
                var color = templates[i].GetPixel(x, y);
                Debug.Assert(color.R == pixelByte++);
                Debug.Assert(color.G == pixelByte++);
                Debug.Assert(color.B == pixelByte++);
                Debug.Assert(color.A == pixelByte++);
            }
        }

        public void Run()
        {
            _testBuildingImagesharpTemplateMatcher();
            _testBuildingImagesharpTemplateMatcherFrames();
        }
    }


    public class ScreenAilmentDetectionThreadsBuilderTests
    {
        private MockTemplateMatcherBuilder _bitmapTemplateMatcherBuilder = (
            new MockTemplateMatcherBuilder()
        );

        private MockThreadsBuilder _singleAilmentThreadBuilder = (
            new MockThreadsBuilder()
        );

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = (
            new MaplestoryBotConfiguration()
        );

        private ConfigurationImages _configurationImages = (
            new ConfigurationImages()
        );

        private AbstractBottingModel _bottingModel = (
            new BottingModel()
        );

        private List<MockBitmapTemplateMatcher> _templateMatchers = [];

        private List<ConcurrentDictionary<string, AbstractThread>> _abstractThreads = [];

        private AbstractThreadsBuilder _fixture()
        {
            _templateMatchers = [];

            _bitmapTemplateMatcherBuilder = new MockTemplateMatcherBuilder();
            _singleAilmentThreadBuilder = new MockThreadsBuilder();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                Ailments =
                {
                    ["meow1"] = new Ailment { Active = 12, Threshold = 23, CheckDelay = 34 },
                    ["meow2"] = new Ailment { Active = 23, Threshold = 34, CheckDelay = 45 },
                    ["meow3"] = new Ailment { Active = 34, Threshold = 45, CheckDelay = 56 }
                }
            };
            _configurationImages = new ConfigurationImages();
            _bottingModel = new BottingModel();
            return new ScreenAilmentDetectionThreadsBuilder(
                _bitmapTemplateMatcherBuilder,
                _singleAilmentThreadBuilder
            );
        }

        private ConcurrentDictionary<string, AbstractThread> _buildResult(string ailmentKey)
        {
            var dict = new ConcurrentDictionary<string, AbstractThread>();
            dict[ailmentKey] = new MockThread(new ThreadRunningState());
            return dict;
        }

        private void _setupFixture(
            AbstractThreadsBuilder builder,
            bool configuration,
            bool images,
            bool bottingModel
        )
        {
            if (configuration)
            {
                builder.WithArg(
                    new ScreenAilmentDetectionThreadsBuilderArgs
                    {
                        DataType = SystemInjectType.ConfigurationUpdate,
                        Data = _maplestoryBotConfiguration
                    }
                );
            }
            if (images)
            {
                builder.WithArg(
                    new ScreenAilmentDetectionThreadsBuilderArgs
                    {
                        DataType = SystemInjectType.ConfigurationUpdate,
                        Data = _configurationImages
                    }
                );
            }
            if (bottingModel)
            {
                builder.WithArg(
                    new ScreenAilmentDetectionThreadsBuilderArgs
                    {
                        DataType = SystemInjectType.BottingModel,
                        Data = _bottingModel
                    }
                );
            }
        }

        private void _setupBuildReturns()
        {
            var ailmentsDict = _maplestoryBotConfiguration.Ailments;
            var ailmentsKeys = ailmentsDict.Keys.OrderBy(k => k);
            foreach (var ailmentKey in ailmentsKeys)
            {
                _templateMatchers.Add(new MockBitmapTemplateMatcher());
                _abstractThreads.Add(_buildResult(ailmentKey));
                _bitmapTemplateMatcherBuilder.BuildReturn.Add(_templateMatchers.Last());
                _singleAilmentThreadBuilder.BuildReturn.Add(_abstractThreads.Last());
            }
        }

        /**
         * @brief Verifies that the detection threads builder only constructs threads when
         * all required dependencies (configuration, images, botting model) are provided
         * 
         * When building detection threads for status ailments, the builder requires three
         * components to be present: the bot configuration (containing ailment settings),
         * the configuration images (reference templates for detection), and the botting
         * model (for storing detection results). This test ensures that threads are only
         * built when all three dependencies are available, and returns an empty dictionary
         * when any dependency is missing.
         */
        private void _testBuildingDetectionThreads()
        {
            foreach (var configuration in new[] {true, false})
            foreach (var images in new[] { true, false })
            foreach (var bottingModel in new[] { true, false })
            {
                var builder = _fixture();
                _setupFixture(builder, configuration, images, bottingModel);
                _setupBuildReturns();
                var result = builder.Build();
                if (configuration && images && bottingModel)
                {
                    Debug.Assert(result != null);
                    Debug.Assert(result.Count == 3);
                    Debug.Assert(result["meow1"] == _abstractThreads[0].First().Value);
                    Debug.Assert(result["meow2"] == _abstractThreads[1].First().Value);
                    Debug.Assert(result["meow3"] == _abstractThreads[2].First().Value);
                }
                else
                {
                    Debug.Assert(result != null);
                    Debug.Assert(result.Count == 0);
                }
            }
        }

        /**
         * @brief Verifies that the builder calls the underlying component builders in the
         * correct sequence when constructing detection threads
         * 
         * When building detection threads, the builder must first configure the template
         * matcher builder (passing the ailment key and configuration images), then build
         * the template matcher, then configure the single ailment thread builder (passing
         * the ailment key, ailment data, botting model, and template matcher), and finally
         * build the thread. This test ensures all WithArg and Build calls occur in the
         * correct order.
         */
        private void _testBuildingDetectionThreadCalls()
        {
            var builder = _fixture();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                Ailments =
                {
                    ["meow1"] = new Ailment
                    {
                        Active = 12,
                        Threshold = 23,
                        CheckDelay = 34
                    }
                }
            };
            _setupFixture(builder, true, true, true);
            _setupBuildReturns();
            var matcherRef = new TestUtilities().Reference(_bitmapTemplateMatcherBuilder);
            var ailmentRef = new TestUtilities().Reference(_singleAilmentThreadBuilder);
            var matcherCallOrder = _bitmapTemplateMatcherBuilder.CallOrder;
            var ailmentCallOrder = _singleAilmentThreadBuilder.CallOrder;
            var result = builder.Build();
            Debug.Assert(result != null);
            Debug.Assert(result.Count == 1);
            Debug.Assert(matcherCallOrder.Count == 3);
            Debug.Assert(matcherCallOrder[0] == matcherRef + "WithArg");
            Debug.Assert(matcherCallOrder[1] == matcherRef + "WithArg");
            Debug.Assert(matcherCallOrder[2] == matcherRef + "Build");
            Debug.Assert(ailmentCallOrder.Count == 5);
            Debug.Assert(ailmentCallOrder[0] == ailmentRef + "WithArg");
            Debug.Assert(ailmentCallOrder[1] == ailmentRef + "WithArg");
            Debug.Assert(ailmentCallOrder[2] == ailmentRef + "WithArg");
            Debug.Assert(ailmentCallOrder[3] == ailmentRef + "WithArg");
            Debug.Assert(ailmentCallOrder[4] == ailmentRef + "Build");
        }

        /**
         * @brief Verifies that the correct arguments are passed to the underlying
         * component builders when constructing detection threads
         * 
         * When building detection threads for an ailment, the builder must pass the
         * correct values to each component: the ailment key and configuration images
         * to the template matcher builder; and the ailment key, ailment data, botting
         * model, and constructed template matcher to the single ailment thread builder.
         * This test ensures that each argument matches the expected values from the
         * configuration fixture.
         */
        private void _testBuildingDetectionThreadArgs()
        {
            var builder = _fixture();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                Ailments = {
                    ["meow1"] = new Ailment
                    {
                        Active = 12,
                        Threshold = 23,
                        CheckDelay = 34
                    }
                }
            };
            _setupFixture(builder, true, true, true);
            _setupBuildReturns();
            var result = builder.Build();
            Debug.Assert(result != null);
            Debug.Assert(result.Count == 1);
            var matcherCallArg = _bitmapTemplateMatcherBuilder.WithArgCallArg_arg;
            var ailmentCallArg = _singleAilmentThreadBuilder.WithArgCallArg_arg;
            Debug.Assert((string)matcherCallArg[0] == "meow1");
            Debug.Assert(matcherCallArg[1] == _configurationImages);
            Debug.Assert((string)ailmentCallArg[0]! == "meow1");
            Debug.Assert(ailmentCallArg[1] is Ailment);
            Debug.Assert(((Ailment)ailmentCallArg[1]!).Active == 12);
            Debug.Assert(((Ailment)ailmentCallArg[1]!).Threshold == 23);
            Debug.Assert(((Ailment)ailmentCallArg[1]!).CheckDelay == 34);
            Debug.Assert(ailmentCallArg[2]! == _bottingModel);
            Debug.Assert(ailmentCallArg[3] == _templateMatchers[0]);
        }   

        public void Run()
        {
            _testBuildingDetectionThreads();
            _testBuildingDetectionThreadCalls();
            _testBuildingDetectionThreadArgs();
        }
    }


    public class LazyScreenAilmentsDetectionThreadBuilderTests
    {
        private MockThreadsBuilder _ailmentsDetectionThreadBuilder = new MockThreadsBuilder();

        private AbstractThreadsBuilder _fixture()
        {
            _ailmentsDetectionThreadBuilder = new MockThreadsBuilder();
            return new LazyScreenAilmentsDetectionThreadBuilder(
                _ailmentsDetectionThreadBuilder
            );
        }

        /**
         * @brief Ensures that the lazy thread builder only builds ailment detection threads
         * once, even if the system requests them multiple times
         * 
         * When the bot starts up and loads ailment configurations, the system may request
         * detection threads multiple times (e.g., during initialization, after config
         * updates, or from different components). The lazy builder should build the threads
         * only once and reuse the same threads for all subsequent requests, preventing
         * duplicate threads that would waste memory and CPU resources.
         */
        private void _testLazyBuildingDetectionThreads()
        {
            for (int i = 1; i < 10; i++)
            {
                var builder = _fixture();
                for (int j = 0; j < i; j++)
                {
                    _ailmentsDetectionThreadBuilder.BuildReturn.Add([]);
                }
                var mockThread = new MockThread(new ThreadRunningState());
                var dictionary = new ConcurrentDictionary<string, AbstractThread>
                {
                    ["meow"] = mockThread
                };
                _ailmentsDetectionThreadBuilder.BuildReturn.Add(dictionary);
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(builder.Build() == null);
                }
                for (int j = 0; j < 2; j++)
                {
                    var build = builder.Build()!;
                    Debug.Assert(build.Count == 1);
                    Debug.Assert(build["meow"] == mockThread);
                }

            }
        }

        /**
         * @brief Ensures that the lazy builder forwards configuration data to the
         * underlying thread builder correctly
         * 
         * When the bot loads configuration settings (like which ailments to monitor,
         * detection thresholds, and reference images), these settings must be passed
         * through the lazy builder to the actual thread builder. This test verifies that
         * the lazy builder doesn't interfere with or lose any configuration data.
         */
        private void _testDelegationWithArgs()
        {
            var builder = _fixture();
            var someArg = new object();
            builder.WithArg(someArg);
            Debug.Assert(_ailmentsDetectionThreadBuilder.WithArgCalls == 1);
            Debug.Assert(_ailmentsDetectionThreadBuilder.WithArgCallArg_arg[0] == someArg);
        }

        public void Run()
        {
            _testLazyBuildingDetectionThreads();
            _testDelegationWithArgs();
        }
    }


    public class ScreenAilmentDetectionThreadStarterTests
    {
        public AbstractScreenAilmentDetectionThreadStarter _fixture()
        {
            return new ScreenAilmentDetectionThreadStarter();
        }

        /**
         * @brief Verifies that when the bot starts detection threads, each thread receives
         * its dependency injection and is launched exactly once
         * 
         * When the user enables ailment detection, the bot must properly initialize each
         * detection thread by injecting a thread dependency and then starting the thread.
         * This should only happen once, even if the start is attempted multiple times,
         * to prevent duplicate threads.
         */
        private void _testStartAttemptInjectsAndStartsEachThread()
        {
            var dataTypeList = new List<object>();
            var dataList = new List<object?>();
            var ailmentThreads = new ConcurrentDictionary<string, AbstractThread>
            {
                ["meow1"] = new MockThread(new ThreadRunningState()),
                ["meow2"] = new MockThread(new ThreadRunningState()),
                ["meow3"] = new MockThread(new ThreadRunningState())
            };
            var injectAction = new InjectAction(
                (_, __) => { dataTypeList.Add(_); dataList.Add(__); }
            );
            var starter = _fixture();
            for (int i = 0; i < 2; i++)
            {
                starter.StartAttempt(ailmentThreads, injectAction);
                Debug.Assert(dataTypeList.Count == 3);
                Debug.Assert(dataList.Count == 3);
                var ailmentThreadKeys = ailmentThreads.Keys.OrderBy(k => k).ToList();
                for (var j = 0; j < ailmentThreadKeys.Count(); j++)
                {
                    var ailmentThreadKey = ailmentThreadKeys[j];
                    Debug.Assert(
                        (SystemInjectType)dataTypeList[i] ==
                        SystemInjectType.ThreadDependency
                    );
                    Debug.Assert(
                        dataList[j] ==
                        ailmentThreads["meow" + (j + 1).ToString()]
                    );
                }
                for (var j = 0; j < ailmentThreadKeys.Count(); j++)
                {
                    var thread = (MockThread)ailmentThreads["meow" + (j + 1).ToString()];
                    Debug.Assert(thread.ThreadStartCalls == 1);
                }
            }
        }

        /**
         * @brief Verifies that the thread starter does nothing when no inject action is
         * provided, preventing thread startup without proper dependency injection
         * 
         * When the bot system is not fully initialized (missing the inject action callback),
         * attempting to start detection threads should be safely ignored. This prevents
         * threads from running without proper dependency injection, which could cause
         * null reference exceptions or threads that cannot communicate with the rest of
         * the system.
         */
        private void _testStartAttemptWithNoInjectAction()
        {
            var ailmentThreads = new ConcurrentDictionary<string, AbstractThread>
            {
                ["meow1"] = new MockThread(new ThreadRunningState()),
                ["meow2"] = new MockThread(new ThreadRunningState()),
                ["meow3"] = new MockThread(new ThreadRunningState())
            };
            var starter = _fixture();
            for (int i = 0; i < 2; i++)
            {
                starter.StartAttempt(ailmentThreads, null);
                var ailmentThreadKeys = ailmentThreads.Keys.OrderBy(k => k).ToList();
                for (var j = 0; j < ailmentThreadKeys.Count(); j++)
                {
                    var thread = (MockThread)ailmentThreads["meow" + (j + 1).ToString()];
                    Debug.Assert(thread.ThreadStartCalls == 0);
                }
            }
        }

        public void Run()
        {
            _testStartAttemptInjectsAndStartsEachThread();
            _testStartAttemptWithNoInjectAction();
        }
    }


    public class ScreenAilmentsSystemTests
    {
        private ConcurrentDictionary<string, AbstractThread>? _ailmentThreads = [];

        private AbstractInjectAction? _injectAction;

        private MockThreadsBuilder _ailmentDetectionThreadsBuilder = new MockThreadsBuilder();

        private MockScreenAilmentDetectionThreadStarter _ailmentDetectionThreadsStarter = (
            new MockScreenAilmentDetectionThreadStarter()
        );

        private AbstractSystem _fixture()
        {
            _ailmentDetectionThreadsBuilder = new MockThreadsBuilder();
            _ailmentDetectionThreadsStarter = new MockScreenAilmentDetectionThreadStarter();
            _injectAction = new MockInjectAction();
            _ailmentThreads = [];
            return new ScreenAilmentsSystem(
                _ailmentDetectionThreadsBuilder,
                _ailmentDetectionThreadsStarter
            );
        }

        /**
         * @brief Verifies that when the system receives an inject action, it attempts to
         * start all built detection threads with the correct dependencies
         * 
         * When the user configures ailment detection and the system is ready, the inject
         * action (callback) is provided to the system. The system must then attempt to
         * start all detection threads, passing them the inject action so they can broadcast
         * themselves. This ensures threads are properly initialized and ready to receive
         * screen captures.
         */
        private void _testScreenAilmentsSystemStartAttempt()
        {
            var system = _fixture();
            _ailmentDetectionThreadsBuilder.BuildReturn.Add(_ailmentThreads!);
            system.Inject(SystemInjectType.InjectAction, _injectAction);
            Debug.Assert(_ailmentDetectionThreadsStarter.StartAttemptCalls == 1);
            Debug.Assert(
                _ailmentDetectionThreadsStarter.StartAttemptCallArg_injectAction[0] ==
                _injectAction
            );
            Debug.Assert(
                _ailmentDetectionThreadsStarter.StartAttemptCallArg_ailmentThreads[0] ==
                _ailmentThreads
            );
        }

        /**
         * @brief Verifies that when the system receives an injection (such as configuration
         * updates or dependencies), it builds the detection threads using the provided
         * arguments
         * 
         * When the bot loads configuration settings or receives dependencies (botting model,
         * configuration images), the system must forward these to the thread builder so
         * detection threads can be constructed with the correct parameters. The builder
         * should receive the injection arguments exactly as they were passed.
         */
        private void _testBuildAilmentThreads()
        {
            var system = _fixture();
            var callOrder = _ailmentDetectionThreadsBuilder.CallOrder;
            var builderRef = new TestUtilities().Reference(_ailmentDetectionThreadsBuilder);
            _ailmentDetectionThreadsBuilder.BuildReturn.Add(_ailmentThreads!);
            system.Inject(12, 23);
            var arg = (
                (ScreenAilmentDetectionThreadsBuilderArgs)
                _ailmentDetectionThreadsBuilder.WithArgCallArg_arg[0]!
            );
            Debug.Assert(callOrder.Count == 2);
            Debug.Assert(callOrder[0] == builderRef + "WithArg");
            Debug.Assert(callOrder[1] == builderRef + "Build");
            Debug.Assert((int)arg.DataType == 12);
            Debug.Assert((int)arg.Data! == 23);
        }

        /**
         * @brief Verifies that all built detection threads receive injections when the
         * system is updated with new data (e.g., screen captures, configuration changes)
         * 
         * When the system receives a configuration update, it must forward that data to
         * every detection thread. This ensures all ailment detectors work with the latest
         * settings.
         */
        private void _testInjectToAilmentThreads()
        {
            var system = _fixture();
            _ailmentThreads = new ConcurrentDictionary<string, AbstractThread>()
            {
                ["meow1"] = new MockThread(new ThreadRunningState()),
                ["meow2"] = new MockThread(new ThreadRunningState()),
                ["meow3"] = new MockThread(new ThreadRunningState())
            };
            _ailmentDetectionThreadsBuilder.BuildReturn.Add(_ailmentThreads);
            system.Inject(12, 23);
            foreach (var threadPair in _ailmentThreads)
            {
                var mockThread = (MockThread)threadPair.Value;
                Debug.Assert(mockThread.InjectCalls == 1);
                Debug.Assert((int)mockThread.InjectCallArg_dataType[0] == 12);
                Debug.Assert((int)mockThread.InjectCallArg_data[0]! == 23);
            }
        }

        public void Run()
        {
            _testScreenAilmentsSystemStartAttempt();
            _testBuildAilmentThreads();
            _testInjectToAilmentThreads();
        }
    }


    public class ScreenAilmentsSystemTestSuite
    {
        public void Run()
        {
            new ScreenAilmentDetectionThreadHelperTests().Run();
            new ScreenAilmentDetectionThreadTests().Run();
            new ImagesharpTemplateMatcherBuilderTests().Run();
            new ScreenAilmentDetectionThreadsBuilderTests().Run();
            new LazyScreenAilmentsDetectionThreadBuilderTests().Run();
            new ScreenAilmentDetectionThreadStarterTests().Run();
            new ScreenAilmentsSystemTests().Run();
        }
    }
}
