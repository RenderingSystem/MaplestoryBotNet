using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;


namespace MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters
{
    public enum SolvingOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }

    public enum SolvingExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        Solved,
        Failed,
        MaxNum
    }


    public abstract class AbstractRuneSolverManager
    {
        public abstract string Post(string url, string content);
    }


    public abstract class AbstractRuneSolverCaller
    {
        public abstract string Call(
            RuneDetection runeDetection,
            Image<Bgra32> image
        );
    }


    public abstract class AbstractRuneSolverWorkflow : IDataInjectable
    {
        public abstract RuneDetection? ValidatePrerequisites();

        public abstract bool ExecuteInteraction();

        public abstract JsonDocument? ExecuteArrowDetection(RuneDetection runeDetection);

        public abstract bool ExecuteArrowSequence(
            RuneDetection runeDetection, JsonDocument predictions
        );

        public abstract void Inject(object dataType, object? data);
    }


    public class RuneSolverManager : AbstractRuneSolverManager
    {
        public override string Post(string url, string base64Image)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var json = JsonSerializer.Serialize(base64Image);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return response.IsSuccessStatusCode ? result : "";
                }
            }
            catch
            {
                return "";
            }
        }
    }


    public class RuneSolverCaller : AbstractRuneSolverCaller
    {
        private AbstractRuneSolverManager _manager;

        public RuneSolverCaller(AbstractRuneSolverManager manager)
        {
            _manager = manager;
        }

        private string _buildUrl(RuneDetection runeDetection)
        {
            var url = "";
            url += runeDetection.RuneSolverIPAddress;
            url += ":";
            url += runeDetection.RuneSolverPort;
            url += "/";
            url += runeDetection.RuneSolverRoute;
            url += "/solve";
            return url;
        }

        private string _toBase64(Image<Bgra32> image)
        {
            using var memoryStream = new MemoryStream();
            image.Save(memoryStream, JpegFormat.Instance);
            byte[] imageBytes = memoryStream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }

        public override string Call(
            RuneDetection runeDetection,
            Image<Bgra32> image
        )
        {
            var url = _buildUrl(runeDetection);
            var base64Image = _toBase64(image);
            return _manager.Post(url, base64Image);
        }
    }


    public class SolvingScreenCaptureSubscriber : AbstractScreenCaptureSubscriber
    {
        private AbstractThread? _solvingOrchestratorThread;

        public SolvingScreenCaptureSubscriber(SemaphoreSlim semaphore) : base(semaphore)
        {
            _solvingOrchestratorThread = null;
        }

        public override void ProcessImage()
        {
            if (_solvingOrchestratorThread != null)
            {
                _solvingOrchestratorThread.Inject(0, _image);
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ThreadDependency
                && data is SolvingOrchestratorThread solvingOrchestratorThread
            )
            {
                _solvingOrchestratorThread = solvingOrchestratorThread;
            }
        }
    }


    public class RuneSolverWorkflow : AbstractRuneSolverWorkflow
    {
        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractRuneSolverCaller _runeSolverCaller;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        private RuneDetection? _runeDetection;

        private string _interactKey;

        private Image<Bgra32>? _solveImage;

        public RuneSolverWorkflow(
            AbstractMacroCommandsExecutorBuilder macroCommandsExecutorBuilder,
            AbstractRuneSolverCaller runeSolverCaller
        )
        {
            _macroCommandsExecutorBuilder = macroCommandsExecutorBuilder;
            _runeSolverCaller = runeSolverCaller;
            _macroCommandsExecutor = null;
            _runeDetection = null;
            _interactKey = "";
            _solveImage = null;
        }

        public string _mapClassToKey(RuneDetection runeDetection, string? arrowClass)
        {
            return (
                (arrowClass == runeDetection.Left) ? "ARROW_LEFT" :
                (arrowClass == runeDetection.Up) ? "ARROW_UP" :
                (arrowClass == runeDetection.Right) ? "ARROW_RIGHT" :
                (arrowClass == runeDetection.Down) ? "ARROW_DOWN" :
                ""
            );
        }

        public override RuneDetection? ValidatePrerequisites()
        {
            return (
                _runeDetection is RuneDetection runeDetection &&
                _macroCommandsExecutor is AbstractMacroCommandsExecutor &&
                _interactKey != ""
            ) ? runeDetection : null;
        }

        public override bool ExecuteInteraction()
        {
            if (_macroCommandsExecutor != null)
            {
                _macroCommandsExecutor.Execute(["key press {" + _interactKey + "} {50} {100}"]);
                _macroCommandsExecutor.Execute(["wait {800} {900}"]);
                return true;
            }
            return false;
        }

        public override JsonDocument? ExecuteArrowDetection(
            RuneDetection runeDetection
        )
        {
            try
            {
                return (
                    _solveImage is not Image<Bgra32> solveImage ||
                    _runeSolverCaller.Call(runeDetection, solveImage) is not string result ||
                    JsonDocument.Parse(result) is not JsonDocument predictions ||
                    predictions.RootElement.GetArrayLength() != 4
                ) ? null : predictions;
            }
            catch
            {
                return null;
            }
        }

        public override bool ExecuteArrowSequence(
            RuneDetection runeDetection, JsonDocument predictions
        )
        {
            try
            {
                if (_macroCommandsExecutor == null)
                {
                    return false;
                }
                var arrowKeys = new List<string>();
                foreach (var prediction in predictions.RootElement.EnumerateArray())
                {
                    var arrowClass = prediction.GetProperty(runeDetection.ClassTag).GetString();
                    var arrowKey = _mapClassToKey(runeDetection, arrowClass);
                    if (arrowKey == "")
                    {
                        return false;
                    }
                    arrowKeys.Add(arrowKey);
                }
                foreach (var arrowKey in arrowKeys)
                {
                    _macroCommandsExecutor.Execute(["key press {" + arrowKey + "} {100} {150}"]);
                    _macroCommandsExecutor.Execute(["wait {200} {250}"]);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate
                && data is MaplestoryBotConfiguration maplestoryBotConfiguration
                && maplestoryBotConfiguration.RuneDetection.Copy() is RuneDetection runeDetection
            )
            {
                _runeDetection = runeDetection;
                _interactKey = maplestoryBotConfiguration.MacroKeySettings.RuneInteractKey;
            }
            else if (data is Image<Bgra32> solveImage)
            {
                _solveImage = solveImage;
            }
            else if (
                dataType is SystemInjectType.KeystrokeTransmitter
                && data is AbstractKeystrokeTransmitter keystrokeTransmitter
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


    public class SolvingExecutorThreadHelper : AbstractKeystrokeTransmitterThreadHelper
    {
        private AbstractRuneSolverWorkflow _runeSolverWorkflow;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        public SolvingExecutorThreadHelper(
            AbstractRuneSolverWorkflow runeSolverWorkflow,
            AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _runeSolverWorkflow = runeSolverWorkflow;
            _threadState = threadState;
        }

        public override bool Transmit()
        {
            JsonDocument? predictions = null;

            try
            {
                if (
                    _runeSolverWorkflow.ValidatePrerequisites() is RuneDetection runeDetection &&
                    _runeSolverWorkflow.ExecuteInteraction() &&
                    (predictions = _runeSolverWorkflow.ExecuteArrowDetection(runeDetection)) != null &&
                    _runeSolverWorkflow.ExecuteArrowSequence(runeDetection, predictions)
                )
                {
                    _threadState.SetState((int)SolvingExecutorThreadedUpdate.Solved);
                }
                else
                {
                    _threadState.SetState((int)SolvingExecutorThreadedUpdate.Failed);
                }
            }
            finally
            {
                predictions?.Dispose();
            }
            return true;
        }

        public override void Reset()
        {
            return;
        }

        public override void Inject(object dataType, object? data)
        {
            _runeSolverWorkflow.Inject(dataType, data);
        }
    }


    public class SolvingExecutorThread : AbstractThread
    {
        private AbstractResetEvent _executionEvent;

        private AbstractKeystrokeTransmitterThreadHelper _solvingExecutorHelper;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractThreadRunningState _transmittingState;

        public SolvingExecutorThread(
            AbstractResetEvent executionEvent,
            AbstractKeystrokeTransmitterThreadHelper solvingExecutorHelper,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractThreadRunningState transmittingState,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _executionEvent = executionEvent;
            _solvingExecutorHelper = solvingExecutorHelper;
            _threadState = threadState;
            _transmittingState = transmittingState;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _executionEvent.WaitOne();
                _transmittingState.SetRunning(true);
                _solvingExecutorHelper.Reset();
                while (_threadState.GetState() == (int)RuneingExecutorThreadedUpdate.Started)
                {
                    if (!_solvingExecutorHelper.Transmit())
                    {
                        break;
                    }
                }
                _solvingExecutorHelper.Reset();
                _transmittingState.SetRunning(false);
            }
        }

        public override void Stop()
        {
            base.Stop();
            Inject(SolvingOrchestratorThreadInjectType.Stop, null);
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is SolvingOrchestratorThreadInjectType injectType)
            {
                if (injectType == SolvingOrchestratorThreadInjectType.Start)
                {
                    _threadState.SetState((int)SolvingExecutorThreadedUpdate.Starting);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)SolvingExecutorThreadedUpdate.Started);
                    _executionEvent.Set();
                }
                else if (injectType == SolvingOrchestratorThreadInjectType.Stop)
                {
                    _threadState.SetState((int)SolvingExecutorThreadedUpdate.Stopping);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)SolvingExecutorThreadedUpdate.Stopped);
                }
            }
            else
            {
                _solvingExecutorHelper.Inject(dataType, value);
            }
        }

        public override object? State()
        {
            return _threadState;
        }
    }


    public class SolvingOrchestratorThread : AbstractOrchestratorThread<SolvingOrchestratorThreadInjectType>
    {
        public SolvingOrchestratorThread(
            AbstractThread solvingExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(solvingExecutorThread, runningState, threadStates)
        { }
    }


    public class SolvingOrchestratorThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                (int)SolvingExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Solving
            );
            return new SolvingOrchestratorThread(
                new SolvingExecutorThread(
                    new ExecutionEvent(),
                    new SolvingExecutorThreadHelper(
                        new RuneSolverWorkflow(
                            new MacroCommandsExecutorBuilder(),
                            new RuneSolverCaller(new RuneSolverManager())
                        ),
                        threadState
                    ),
                    threadState,
                    new ThreadRunningState(),
                    new ThreadRunningState()
                ),
                new ThreadRunningState(),
                new BlockingCollection<int>()
            );
        }
    }


    public class SolvingOrchestratorSystem : AbstractOrchestratorSystem
    {
        public SolvingOrchestratorSystem(
            List<AbstractThreadFactory> threadFactories
        ) : base(threadFactories)
        { }
    }


    public class SolvingOrchestratorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new SolvingOrchestratorSystem(
                [new SolvingOrchestratorThreadFactory()]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
