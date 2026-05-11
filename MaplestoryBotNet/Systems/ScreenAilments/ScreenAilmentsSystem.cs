using ArrayFireNCC;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;
using System.Drawing;
using System.Windows;


namespace MaplestoryBotNet.Systems.ScreenAilmentsProcessing
{
    public abstract class AbstractScreenAilmentDetectionHelper
    {
        public abstract bool ShouldCheck(float checkDelay);

        public abstract List<Tuple<int, int, int, int, float>> AilmentDetected(
            Image<Bgra32> image, float threshold
        );
    }


    public abstract class AbstractTemplateMatcherBuilder
    {
        public abstract AbstractBitmapTemplateMatcher Build();

        public abstract AbstractTemplateMatcherBuilder WithArg(object arg);
    }


    public abstract class AbstractScreenAilmentDetectionThreadStarter
    {
        public abstract void StartAttempt(
            ConcurrentDictionary<string, AbstractThread>? ailmentThreads,
            AbstractInjectAction? injectAction
        );
    }


    public class ScreenCaptureAilmentsSubscriber : AbstractScreenCaptureSubscriber
    {
        List<ScreenAilmentDetectionThread> _ailmentDetectionThreads;

        public ScreenCaptureAilmentsSubscriber(
            SemaphoreSlim semaphore
        ) : base(semaphore)
        {
            _ailmentDetectionThreads = [];
        }

        public override void ProcessImage()
        {
            foreach (var ailmentDetectionThread in _ailmentDetectionThreads)
            {
                ailmentDetectionThread.Inject(0, _image);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ThreadDependency &&
                data is ScreenAilmentDetectionThread ailmentDetectionThread
            )
            {
                _ailmentDetectionThreads.Add(ailmentDetectionThread);
            }
        }
    }

    
    public class ScreenAilmentDetectionThreadHelper : AbstractScreenAilmentDetectionHelper
    {
        private AbstractTimestamp _detectionStopwatch;

        private AbstractBitmapTemplateMatcher _templateMatcher;

        public ScreenAilmentDetectionThreadHelper(
            AbstractBitmapTemplateMatcher templateMatcher,
            AbstractTimestamp detectionStopwatch
        )
        {
            _detectionStopwatch = detectionStopwatch;
            _templateMatcher = templateMatcher;
        }

        public override bool ShouldCheck(float checkDelay)
        {
            return _detectionStopwatch.GetTimestamp() > checkDelay;
        }

        public override List<Tuple<int, int, int, int, float>> AilmentDetected(
            Image<Bgra32> image, float threshold
        )
        {
            image.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> memory);
            _detectionStopwatch.SetTimestamp();
            var matches = new List<Tuple<int, int, int, int, float>>();
            unsafe
            {
                using (var handle = memory.Pin())
                {
                    var ptr = (uint*)handle.Pointer;
                    matches = _templateMatcher.calculate(
                        ptr,
                        image.Width,
                        image.Height,
                        image.Width,
                        threshold
                    );
                }
            }
            return matches;
        }
    }


    public class ScreenAilmentDetectionThread : AbstractThread
    {
        private string _ailmentKey;

        private Ailment? _ailment;

        private AbstractResetEvent _resetEvent;

        private AbstractBottingModel _bottingModel;

        private AbstractScreenAilmentDetectionHelper _helper;

        private Image<Bgra32>? _image;

        public ScreenAilmentDetectionThread(
            string ailmentKey,
            Ailment ailment,
            AbstractBottingModel bottingModel,
            AbstractResetEvent resetEvent,
            AbstractScreenAilmentDetectionHelper helper,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _ailmentKey = ailmentKey;
            _ailment = ailment;
            _bottingModel = bottingModel;
            _resetEvent = resetEvent;
            _helper = helper;
            _image = null;
        }

        public override void ThreadLoop()
        {
            while(_runningState.IsRunning())
            {
                _resetEvent.WaitOne();
                if (
                    _image is Image<Bgra32> image &&
                    _ailment is Ailment ailment &&
                    ailment.Active != 0 &&
                    _helper.ShouldCheck(ailment.CheckDelay / 1000.0f)
                )
                {
                    var detected = _helper.AilmentDetected(image, ailment.Threshold / 1000.0f);
                    var ailmentsModel = _bottingModel.GetAilmentsModel();
                    ailmentsModel.SetAilment(_ailmentKey, detected.Count);
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
                dataType is SystemInjectType.ConfigurationUpdate &&
                value is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                var configuration = (MaplestoryBotConfiguration)maplestoryBotConfiguration.Copy();
                if (configuration.Ailments.ContainsKey(_ailmentKey))
                {
                    _ailment = (Ailment)configuration.Ailments[_ailmentKey].Copy();
                }
            }
        }
    }


    public class ImagesharpTemplateMatcherBuilder : AbstractTemplateMatcherBuilder
    {
        private string _ailmentKey;

        private ConfigurationImages? _configurationImages;

        private AbstractImageCropper _imageCropper;

        private AbstractBitmapTemplateMatcherBuilder _templateMatcherBuilder;

        public ImagesharpTemplateMatcherBuilder(
            AbstractImageCropper imageCropper,
            AbstractBitmapTemplateMatcherBuilder templateMatcherBuilder
        )
        {
            _ailmentKey = "";
            _configurationImages = null;
            _imageCropper = imageCropper;
            _templateMatcherBuilder = templateMatcherBuilder;
        }

        private Bitmap _convertAilmentFrame(Image<Bgra32> frame)
        {
            return _imageCropper.Crop(
                frame, new Rect(0, 0, frame.Width, frame.Height)
            );
        }

        public override AbstractBitmapTemplateMatcher Build()
        {
            var imageFrames = _configurationImages!.AilmentImages[_ailmentKey];
            var bitmapFrames = imageFrames.Select(_convertAilmentFrame).ToList();
            return _templateMatcherBuilder.with_templates(bitmapFrames).build();
        }

        public override AbstractTemplateMatcherBuilder WithArg(object args)
        {
            if (args is ConfigurationImages configurationImages)
            {
                _configurationImages = configurationImages;
            }
            else if (args is string ailmentKey)
            {
                _ailmentKey = ailmentKey;
            }
            return this;
        }
    }


    public class SingleAilmentThreadBuilder : AbstractThreadsBuilder
    {
        private string _ailmentKey;

        private Ailment? _ailment;

        private AbstractBottingModel? _bottingModel;

        private AbstractBitmapTemplateMatcher? _bitmapTemplateMatcher;

        public SingleAilmentThreadBuilder()
        {
            _ailmentKey = "";
            _ailment = null;
            _bottingModel = null;
            _bitmapTemplateMatcher = null;
        }

        public override ConcurrentDictionary<string, AbstractThread>? Build()
        {
            if (
                _ailmentKey != "" &&
                _ailment != null &&
                _bottingModel != null &&
                _bitmapTemplateMatcher != null
            )
            {
                return new ConcurrentDictionary<string, AbstractThread>
                {
                    [_ailmentKey] = new ScreenAilmentDetectionThread(
                        _ailmentKey,
                        _ailment,
                        _bottingModel,
                        new ExecutionEvent(),
                        new ScreenAilmentDetectionThreadHelper(
                            _bitmapTemplateMatcher,
                            new StopwatchTimestamp()
                        ),
                        new ThreadRunningState()
                    )
                };
            }
            return null;
        }

        public override AbstractThreadsBuilder WithArg(object? arg)
        {
            if (arg == null)
            {
                return this;
            }
            if (arg is string ailmentKey)
            {
                _ailmentKey = ailmentKey;
            }
            else if (arg is Ailment ailment)
            {
                _ailment = (Ailment)ailment.Copy();
            }
            else if (arg is AbstractBottingModel bottingModel)
            {
                _bottingModel = bottingModel;
            }
            else if (arg is AbstractBitmapTemplateMatcher bitmapTemplateMatcher)
            {
                _bitmapTemplateMatcher = bitmapTemplateMatcher;
            }
            return this;
        }
    }


    public class ScreenAilmentDetectionThreadsBuilderArgs
    {
        public object DataType = new object();

        public object? Data = null;
    }


    public class ScreenAilmentDetectionThreadsBuilder : AbstractThreadsBuilder
    {
        private AbstractTemplateMatcherBuilder _templateMatcherBuilder;

        private AbstractThreadsBuilder _singleAilmentThreadBuilder;

        private MaplestoryBotConfiguration? _maplestoryBotConfiguration;

        private ConfigurationImages? _configurationImages;

        private AbstractBottingModel? _bottingModel;

        public ScreenAilmentDetectionThreadsBuilder(
            AbstractTemplateMatcherBuilder templateMatcherBuilder,
            AbstractThreadsBuilder singleAilmentThreadBuilder
        )
        {
            _templateMatcherBuilder = templateMatcherBuilder;
            _singleAilmentThreadBuilder = singleAilmentThreadBuilder;
            _maplestoryBotConfiguration = null;
            _configurationImages = null;
            _bottingModel = null;
        }

        private ConcurrentDictionary<string, AbstractThread> _buildDetectionThreads()
        {
            var detectionThreads = new ConcurrentDictionary<string, AbstractThread>();
            var ailmentsDict = _maplestoryBotConfiguration!.Ailments;
            var ailmentKeys = ailmentsDict.Keys.OrderBy(k => k);
            foreach (var ailmentKey in ailmentKeys)
            {
                var ailment = ailmentsDict[ailmentKey];
                var templateMatcher = _templateMatcherBuilder
                    .WithArg(ailmentKey)
                    .WithArg(_configurationImages!)
                    .Build();
                var detectionThreadDict = _singleAilmentThreadBuilder
                    .WithArg(ailmentKey)
                    .WithArg(ailment)
                    .WithArg(_bottingModel!)
                    .WithArg(templateMatcher)
                    .Build();
                if (detectionThreadDict != null)
                {
                    detectionThreads.TryAdd(
                        ailmentKey,
                        detectionThreadDict[ailmentKey]
                    );
                }
            }
            return detectionThreads;
        }

        public override ConcurrentDictionary<string, AbstractThread> Build()
        {
            if (
                _maplestoryBotConfiguration == null ||
                _configurationImages == null ||
                _bottingModel == null
            )
            {
                return [];
            }
            return _buildDetectionThreads();
        }

        public override AbstractThreadsBuilder WithArg(object? arg)
        {
            if (arg is not ScreenAilmentDetectionThreadsBuilderArgs parameters)
            {
                return this;
            }
            if (
                parameters.DataType is SystemInjectType.ConfigurationUpdate &&
                parameters.Data is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                _maplestoryBotConfiguration = maplestoryBotConfiguration;
            }
            else if (
                parameters.DataType is SystemInjectType.ConfigurationUpdate &&
                parameters.Data is ConfigurationImages configurationImages
            )
            {
                _configurationImages = configurationImages;
            }
            else if (
                parameters.DataType is SystemInjectType.BottingModel &&
                parameters.Data is AbstractBottingModel bottingModel
            )
            {
                _bottingModel = bottingModel;
            }
            return this;
        }
    }


    public class LazyScreenAilmentsDetectionThreadBuilder : AbstractThreadsBuilder
    {
        private ConcurrentDictionary<string, AbstractThread>? _ailmentThreads;

        private readonly object _buildLock;

        private AbstractThreadsBuilder _ailmentDetectionThreadsBuilder;

        public LazyScreenAilmentsDetectionThreadBuilder(
            AbstractThreadsBuilder ailmentsDetectionThreadBuilder
        )
        {
            _ailmentThreads = null;
            _buildLock = new object();
            _ailmentDetectionThreadsBuilder = ailmentsDetectionThreadBuilder;
        }

        public override ConcurrentDictionary<string, AbstractThread>? Build()
        {
            if (_ailmentThreads == null)
            {
                lock (_buildLock)
                {
                    if (_ailmentThreads == null)
                    {
                        var ailmentThreads = _ailmentDetectionThreadsBuilder.Build();
                        if (ailmentThreads != null && ailmentThreads.Count > 0)
                        {
                            _ailmentThreads = ailmentThreads;
                        }
                    }
                }
            }
            return _ailmentThreads;
        }

        public override AbstractThreadsBuilder WithArg(object? arg)
        {
            _ailmentDetectionThreadsBuilder.WithArg(arg);
            return this;
        }
    }


    public class ScreenAilmentDetectionThreadStarter : AbstractScreenAilmentDetectionThreadStarter
    {
        private bool _started;

        private readonly object _lock;

        public ScreenAilmentDetectionThreadStarter()
        {
            _started = false;
            _lock = new object();
        }

        public override void StartAttempt(
            ConcurrentDictionary<string, AbstractThread>? ailmentThreads,
            AbstractInjectAction? injectAction
        )
        {
            lock (_lock)
            {
                if (!_started && ailmentThreads != null && injectAction != null)
                {
                    _started = true;
                    var ailmentThreadKeys = ailmentThreads.Keys.OrderBy(k => k);
                    foreach (var ailmentThreadKey in ailmentThreadKeys)
                    {
                        var ailmentThread = ailmentThreads[ailmentThreadKey];
                        var action = injectAction.GetAction();
                        action(SystemInjectType.ThreadDependency, ailmentThread);
                    }
                    foreach (var ailmentThreadKey in ailmentThreadKeys)
                    {
                        var ailmentThread = ailmentThreads[ailmentThreadKey];
                        ailmentThread.Start();
                    }
                }
            }
        }
    }


    public class ScreenAilmentsSystem : AbstractSystem
    {
        private AbstractThreadsBuilder _ailmentDetectionThreadsBuilder;

        private AbstractScreenAilmentDetectionThreadStarter _ailmentDetectionThreadsStarter;

        private ConcurrentDictionary<string, AbstractThread>? _ailmentThreads;

        private AbstractInjectAction? _injectAction;

        public ScreenAilmentsSystem(
            AbstractThreadsBuilder ailmentDetectionThreadsBuilder,
            AbstractScreenAilmentDetectionThreadStarter ailmentDetectionThreadsStarter
        )
        {
            _ailmentDetectionThreadsBuilder = ailmentDetectionThreadsBuilder;
            _ailmentDetectionThreadsStarter = ailmentDetectionThreadsStarter;
            _ailmentThreads = null;
            _injectAction = null;
        }

        private void _injectThreadAction(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.InjectAction &&
                data is AbstractInjectAction injectAction
            )
            {
                _injectAction = injectAction;
            }
        }

        private void _buildAilmentThreads(object dataType, object? data)
        {
            _ailmentThreads = _ailmentDetectionThreadsBuilder
                .WithArg(
                    new ScreenAilmentDetectionThreadsBuilderArgs 
                    {
                        DataType = dataType,
                        Data = data
                    }
                )
                .Build();
        }

        private void _injectToAilmentThreads(object dataType, object? data)
        {
            var threads = _ailmentThreads;
            if (threads != null)
            {
                foreach (var ailmentThread in threads)
                {
                    ailmentThread.Value.Inject(dataType, data);
                }
            }
        }

        private void _startAilmentThreads()
        {
            _ailmentDetectionThreadsStarter.StartAttempt(
                _ailmentThreads, _injectAction
            );
        }

        public override void Inject(object dataType, object? data)
        {
            _injectThreadAction(dataType, data);
            _buildAilmentThreads(dataType, data);
            _injectToAilmentThreads(dataType, data);
            _startAilmentThreads();
        }
    }


    public class ScreenAilmentsSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new ScreenAilmentsSystem(
                new ScreenAilmentDetectionThreadsBuilder(
                    new ImagesharpTemplateMatcherBuilder(
                        new ImageCropper(new ImageSharpConverter()),
                        new BitmapTemplateMatcherBuilder()
                    ),
                    new SingleAilmentThreadBuilder()
                ),
                new ScreenAilmentDetectionThreadStarter()
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
