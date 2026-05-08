using ArrayFireNCC;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using System.Drawing;
using System.Windows;


namespace MaplestoryBotNet.Systems.ScreenProcessing.SubSystems
{
    public abstract class AbstractGameMinimapProcessorThreadStateUpdater<T> where T : class
    {
        public abstract void AtomicUpdate(ref T? atomicField, T? updateObject);
    }


    public class GameMinimapProcessorThreadState
    {

        public static readonly GameMinimapProcessorThreadState Empty = (
            new GameMinimapProcessorThreadState()
        );

        public string? TemplateKey;

        public Bitmap? CurrentBitmap;

        public AbstractBitmapTemplateMatcher? TemplateMatcher;

        public AbstractRectangleMerger? RectangleMerger;

        public MapIcon? MapIcon;

        public AbstractBottingModel? BottingModel;

        public float? Threshold;

        public AbstractWindowStateModifier? PositionUpdater;

        public GameMinimapProcessorThreadState(
            string? templateKey = null,
            Bitmap? bitmap = null,
            AbstractBitmapTemplateMatcher? templateMatcher = null,
            AbstractRectangleMerger? rectangleMerger = null,
            MapIcon? mapIcon = null,
            AbstractBottingModel? bottingModel = null,
            float? threshold = null,
            AbstractWindowStateModifier? positionUpdater = null
        )
        {
            TemplateKey = templateKey;
            CurrentBitmap = bitmap;
            TemplateMatcher = templateMatcher;
            RectangleMerger = rectangleMerger;
            MapIcon = mapIcon;
            BottingModel = bottingModel;
            Threshold = threshold;
            PositionUpdater = positionUpdater;
        }

        public GameMinimapProcessorThreadState(
            GameMinimapProcessorThreadState copy
        )
        {
            TemplateKey = copy.TemplateKey;
            CurrentBitmap = copy.CurrentBitmap;
            TemplateMatcher = copy.TemplateMatcher;
            RectangleMerger = copy.RectangleMerger;
            MapIcon = copy.MapIcon;
            BottingModel = copy.BottingModel;
            Threshold = copy.Threshold;
            PositionUpdater = copy.PositionUpdater;
        }

        public static void AtomicUpdate(
            ref GameMinimapProcessorThreadState stateField,
            string? templateKey = null,
            Bitmap? currentBitmap = null,
            AbstractBitmapTemplateMatcher? templateMatcher = null,
            AbstractRectangleMerger? rectangleMerger = null,
            MapIcon? mapIcon = null,
            AbstractBottingModel? bottingModel = null,
            float? threshold = null,
            AbstractWindowStateModifier? positionUpdater = null
        )
        {
            while (true)
            {
                var current = stateField;
                var updated = new GameMinimapProcessorThreadState(
                    templateKey ?? current.TemplateKey,
                    currentBitmap ?? current.CurrentBitmap,
                    templateMatcher ?? current.TemplateMatcher,
                    rectangleMerger ?? current.RectangleMerger,
                    mapIcon ?? current.MapIcon,
                    bottingModel ?? current.BottingModel,
                    threshold ?? current.Threshold,
                    positionUpdater ?? current.PositionUpdater
                );
                if (Interlocked.CompareExchange(ref stateField, updated, current) == current)
                {
                    return;
                }
            }
        }
    }


    public class GameMinimapProcessorThreadStateUpdater : 
        AbstractGameMinimapProcessorThreadStateUpdater<
            GameMinimapProcessorThreadState
        >
    {
        public override void AtomicUpdate(
            ref GameMinimapProcessorThreadState? atomicField,
            GameMinimapProcessorThreadState? updateObject
        )
        {
            if (atomicField != null && updateObject != null)
            {
                GameMinimapProcessorThreadState.AtomicUpdate(
                    ref atomicField,
                    updateObject.TemplateKey,
                    updateObject.CurrentBitmap,
                    updateObject.TemplateMatcher,
                    updateObject.RectangleMerger,
                    updateObject.MapIcon,
                    updateObject.BottingModel,
                    updateObject.Threshold,
                    updateObject.PositionUpdater
                );
            }
        }
    }


    public abstract class AbstractScreenPositionProcessor
    {
        public abstract Tuple<int, int>? Process(
            AbstractBitmapTemplateMatcher templateMatcher,
            AbstractRectangleMerger merger,
            float threshold,
            float overlap,
            Bitmap inputSource
        );
    }


    public class ScreenPositionProcessor : AbstractScreenPositionProcessor
    {
        public override Tuple<int, int>? Process(
            AbstractBitmapTemplateMatcher templateMatcher,
            AbstractRectangleMerger merger,
            float threshold,
            float overlap,
            Bitmap inputSource
        )
        {
            var matches = templateMatcher.calculate(
                inputSource, threshold
            );
            if (matches.Count > 0)
            {
                var match = merger.merge(matches, overlap)[0];
                var x = match.Item1 + (match.Item3 / 2);
                var y = match.Item2 + (match.Item4 / 2);
                return new Tuple<int, int>(x, y);
            }
            return null;
        }
    }


    public abstract class AbstractGameMinimapProcessHandler
    {
        public abstract void Handle(GameMinimapProcessorThreadState threadState);
    }


    public class GameMinimapProcessHandler : AbstractGameMinimapProcessHandler
    {
        private AbstractTimestamp _timestamp;

        private AbstractScreenPositionProcessor _positionProcessor;

        public GameMinimapProcessHandler(
            AbstractTimestamp timestamp,
            AbstractScreenPositionProcessor positionProcessor
        )
        {
            _timestamp = timestamp;
            _positionProcessor = positionProcessor;
        }

        public override void Handle(
            GameMinimapProcessorThreadState currentThreadState
        )
        {
            if (
                currentThreadState.TemplateKey == null
                || currentThreadState.TemplateMatcher == null
                || currentThreadState.RectangleMerger == null
                || currentThreadState.Threshold == null
                || currentThreadState.MapIcon == null
                || currentThreadState.CurrentBitmap == null
                || currentThreadState.BottingModel == null
                || currentThreadState.PositionUpdater == null
            )
            {
                return;
            }
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


    public class GameMinimapProcessorThread : AbstractThread
    {
        private AbstractResetEvent _threadResetEvent;

        private AbstractBitmapTemplateMatcherBuilder _templateMatcherBuilder;

        private AbstractImageCropper _cropper;

        private AbstractGameMinimapProcessHandler _processHandler;

        private string _imageKey;

        protected AbstractGameMinimapProcessorThreadStateUpdater<
            GameMinimapProcessorThreadState
        > _threadStateUpdater;

        public GameMinimapProcessorThreadState ThreadState;

        public GameMinimapProcessorThread(
            AbstractThreadRunningState runningState,
            AbstractBitmapTemplateMatcherBuilder templateMatcherBuilder,
            AbstractRectangleMerger matchMerger,
            AbstractImageCropper cropper,
            AbstractGameMinimapProcessHandler processHandler,
            AbstractResetEvent threadResetEvent,
            AbstractGameMinimapProcessorThreadStateUpdater<
                GameMinimapProcessorThreadState
            > threadStateUpdater,
            string imageKey
        ) : base(runningState)
        {
            _threadResetEvent = threadResetEvent;
            _templateMatcherBuilder = templateMatcherBuilder;
            _cropper = cropper;
            _processHandler = processHandler;
            _imageKey = imageKey;
            _threadStateUpdater = threadStateUpdater;
            ThreadState = new GameMinimapProcessorThreadState(
                templateKey: imageKey, rectangleMerger: matchMerger
            );
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _threadResetEvent.WaitOne();
                _processHandler.Handle(ThreadState);
                _threadStateUpdater.AtomicUpdate(
                    ref ThreadState!,
                    new GameMinimapProcessorThreadState(ThreadState)
                    {
                        CurrentBitmap = null
                    }
                );
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (
                dataType is SystemInjectType.BottingModel
                && value is AbstractBottingModel bottingModel
            )
            {
                var threadState = ThreadState;
                _threadStateUpdater.AtomicUpdate(
                    ref ThreadState!,
                    new GameMinimapProcessorThreadState(ThreadState)
                    {
                        BottingModel = bottingModel
                    }
                );
            }
            else if (
                dataType is SystemInjectType.Configuration
                && value is ConfigurationImages configurationImages
            )
            {
                var template = configurationImages.MapIconImages[_imageKey];
                var templateRect = new Rect(0, 0, template.Width, template.Height);
                var templateBitmap = _cropper.Crop(template, templateRect);
                var templateMatcher = (
                    _templateMatcherBuilder
                        .with_templates([templateBitmap])
                        .build()
                );
                _threadStateUpdater.AtomicUpdate(
                    ref ThreadState!,
                    new GameMinimapProcessorThreadState(ThreadState)
                    {
                        TemplateMatcher = templateMatcher
                    }
                );
            }
            else if (
                dataType is SystemInjectType.Configuration
                && value is MaplestoryBotConfiguration configuration
            )
            {
                _threadStateUpdater.AtomicUpdate(
                    ref ThreadState!,
                    new GameMinimapProcessorThreadState(ThreadState)
                    {
                        MapIcon = configuration.MapIcons[_imageKey]
                    }
                );
            }
            else if (value is Bitmap bitmap)
            {
                var threadState = ThreadState;
                if (
                    threadState.BottingModel != null
                    && bitmap.Width > 1
                    && bitmap.Height > 1
                )
                {
                    _threadStateUpdater.AtomicUpdate(
                        ref ThreadState!,
                        new GameMinimapProcessorThreadState(ThreadState)
                        {
                            CurrentBitmap = bitmap,
                            Threshold = (
                                threadState.BottingModel
                                .GetMapModel()
                                .GetTemplateThreshold(_imageKey)
                            )
                        }
                    );
                    _threadResetEvent.Set();
                }
            }
            else if (
                dataType is SystemInjectType.ActionHandler
                && value is WindowMinimapPositionActionHandlerFacade handler
                && handler.Modifier().State(0) is string templateKey
                && templateKey == _imageKey
            )
            {
                _threadStateUpdater.AtomicUpdate(
                    ref ThreadState!,
                    new GameMinimapProcessorThreadState(ThreadState)
                    {
                        PositionUpdater = handler.Modifier()
                    }
                );
            }
        }

        public override object? State()
        {
            return ThreadState;
        }
    }

    
    public class GameMinimapCharacterProcessorThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            return new GameMinimapProcessorThread(
                new ThreadRunningState(),
                new BitmapTemplateMatcherBuilder(),
                new RectangleMerger(),
                new ImageCropper(new ImageSharpConverter()),
                new GameMinimapProcessHandler(new StopwatchTimestamp(), new ScreenPositionProcessor()),
                new ExecutionEvent(),
                new GameMinimapProcessorThreadStateUpdater(),
                MapIconInfo.Character
            );
        }
    }


    public class GameMinimapRuneProcessorThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread() {
            return new GameMinimapProcessorThread(
                new ThreadRunningState(),
                new BitmapTemplateMatcherBuilder(),
                new RectangleMerger(),
                new ImageCropper(new ImageSharpConverter()),
                new GameMinimapProcessHandler(new StopwatchTimestamp(), new ScreenPositionProcessor()),
                new ExecutionEvent(),
                new GameMinimapProcessorThreadStateUpdater(),
                MapIconInfo.Rune
            );
        }
    }
}

