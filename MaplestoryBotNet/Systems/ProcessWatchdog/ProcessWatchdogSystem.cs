using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using System.Diagnostics;
using System.IO;
using System.Net.Http;


namespace MaplestoryBotNet.Systems.ProcessWatchdog
{
    public abstract class AbstractRuneSolverWatchdogManager
    {
        public abstract HttpResponseMessage? Ping(
            RuneDetection runeDetection,
            int clientWatchdogTimeout
        );

        public abstract void StartProcess(
            RuneServerSettings runeServerSettings,
            RuneDetection runeDetection
        );
    }


    public abstract class AbstractRuneSolverWatchdogClient
    {
        public abstract HttpResponseMessage? Get(string url, int timeoutMilliseconds);
    }


    public abstract class AbstractRuneSolverWatchdogHelper
    {
        public abstract void SetStopwatch();

        public abstract bool NeedsLaunch(
            RuneDetection runeDetection,
            RuneServerSettings runeServerSettings
        );

        public abstract void Launch(
            RuneDetection runeDetection,
            RuneServerSettings runeServerSettings
        );

        public abstract void SleepRemaining(
            RuneServerSettings runeServerSettings
        );
    }


    public class RuneSolverWatchdogClient : AbstractRuneSolverWatchdogClient
    {
        public override HttpResponseMessage? Get(string url, int timeoutMilliseconds)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                    return httpClient.GetAsync(url).GetAwaiter().GetResult();
                }
            }
            catch
            {
                return null;
            }
        }
    }


    public class RuneSolverWatchdogManager : AbstractRuneSolverWatchdogManager
    {
        private AbstractProcessName _processName;

        private AbstractProcessLauncher _processLauncher;

        private AbstractRuneSolverWatchdogClient _watchdogClient;

        public RuneSolverWatchdogManager(
            AbstractProcessName processName,
            AbstractProcessLauncher processLauncher,
            AbstractRuneSolverWatchdogClient watchdogClient
        )
        {
            _processName = processName;
            _processLauncher = processLauncher;
            _watchdogClient = watchdogClient;
        }

        public override HttpResponseMessage? Ping(
            RuneDetection runeDetection,
            int clientWatchdogTimeout
        )
        {
            var pingUrl = "";
            pingUrl += runeDetection.RuneSolverIPAddress;
            pingUrl += ":";
            pingUrl += runeDetection.RuneSolverPort;
            pingUrl += "/";
            pingUrl += runeDetection.RuneSolverRoute;
            pingUrl += "/ping";
            return _watchdogClient.Get(pingUrl, clientWatchdogTimeout);
        }

        private string _parseDirectoryName(string executable)
        {
            var directory = Path.GetDirectoryName(executable);
            return string.IsNullOrEmpty(directory) ? "." : directory;
        }

        private string _parseExecutable(string executable)
        {
            executable = Path.GetFileName(executable).Trim('"');
            executable = executable.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ?
                executable : executable + ".exe";
            executable = "\"" + executable + "\"";
            return executable;
        }

        private ProcessWindowStyle _parseWindowStyle(int showConsole)
        {
            return (
                showConsole != 0 ?
                ProcessWindowStyle.Normal :
                ProcessWindowStyle.Hidden
            );
        }

        public override void StartProcess(
            RuneServerSettings runeServerSettings,
            RuneDetection runeDetection
        )
        {
            var directory = _parseDirectoryName(runeServerSettings.ServerExecutable);
            var executable = _parseExecutable(runeServerSettings.ServerExecutable);
            var processName = _processName.GetProcessName();
            var windowStyle = _parseWindowStyle(runeServerSettings.ServerRuneModelConsole);
            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                ArgumentList = {
                    runeServerSettings.ServerRuneModel,
                    processName,
                    runeServerSettings.ServerWatchdogTimeout.ToString(),
                    runeDetection.RuneSolverIPAddress,
                    runeDetection.RuneSolverPort
                },
                WorkingDirectory = directory,
                UseShellExecute = true,
                WindowStyle = windowStyle
            };
            _processLauncher.Launch(startInfo);
        }
    }


    public class RuneSolverWatchdogHelper : AbstractRuneSolverWatchdogHelper
    {
        private AbstractTimestamp _pingStopwatch;

        private AbstractProcessMonitor _processMonitor;

        private AbstractMacroSleeper _sleeper;

        private AbstractRuneSolverWatchdogManager _manager;

        public RuneSolverWatchdogHelper(
            AbstractTimestamp pingStopwatch,
            AbstractProcessMonitor processMonitor,
            AbstractMacroSleeper sleeper,
            AbstractRuneSolverWatchdogManager manager
        )
        {
            _pingStopwatch = pingStopwatch;
            _processMonitor = processMonitor;
            _sleeper = sleeper;
            _manager = manager;
        }

        public override void SetStopwatch()
        {
            _pingStopwatch.SetTimestamp();
        }
        private string _parseExecutable(string executable)
        {
            executable = Path.GetFileName(executable).Trim('"');
            executable = executable.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ?
                executable.Substring(0, executable.Length - ".exe".Length) : executable;
            return executable;
        }

        public override bool NeedsLaunch(
            RuneDetection runeDetection,
            RuneServerSettings runeServerSettings
        )
        {
            var executable = _parseExecutable(runeServerSettings.ServerExecutable);
            var timeout = runeServerSettings.ClientWatchdogTimeout;
            if (_processMonitor.Running(executable) == 0)
            {
                return true;
            }
            var response = _manager.Ping(runeDetection, timeout);
            return response?.IsSuccessStatusCode != true;
        }

        public override void Launch(
            RuneDetection runeDetection,
            RuneServerSettings runeServerSettings
        )
        {
            _manager.StartProcess(
                runeServerSettings,
                runeDetection
            );
        }

        public override void SleepRemaining(
            RuneServerSettings runeServerSettings
        )
        {
            var elapsed = (int)(_pingStopwatch.GetTimestamp() * 1000);
            var timeout = runeServerSettings.ClientWatchdogTimeout;
            _sleeper.Sleep(Math.Max(0, timeout - elapsed));
        }
    }


    public class RuneSolverWatchdogThread : AbstractThread
    {
        private AbstractRuneSolverWatchdogHelper _helper;

        private RuneDetection? _runeDetection;

        private RuneServerSettings? _runeServerSettings;

        public RuneSolverWatchdogThread(
            AbstractRuneSolverWatchdogHelper helper,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _helper = helper;
            _runeDetection = null;
            _runeServerSettings = null;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                var runeDetection = _runeDetection;
                var runeServerSettings = _runeServerSettings;
                if (runeDetection != null && runeServerSettings != null)
                {
                    _helper.SetStopwatch();
                    if (_helper.NeedsLaunch(runeDetection, runeServerSettings))
                    {
                        _helper.Launch(runeDetection, runeServerSettings);
                    }
                    _helper.SleepRemaining(runeServerSettings);
                }
                else
                {
                    Thread.Yield();
                }
            }
        }

        public override void Inject(object dataType, object? value)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate
                && value is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                _runeDetection = maplestoryBotConfiguration.RuneDetection.Copy();
                _runeServerSettings = maplestoryBotConfiguration.RuneServerSettings.Copy();
            }
        }
    }


    public class RuneSolverWatchdogThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            return new RuneSolverWatchdogThread(
                new RuneSolverWatchdogHelper(
                    new StopwatchTimestamp(),
                    new ProcessMonitor(),
                    new MacroSleeper(),
                    new RuneSolverWatchdogManager(
                        new ProcessName(),
                        new ProcessLauncher(),
                        new RuneSolverWatchdogClient()
                    )
                ),
                new ThreadRunningState()
            );
        }
    }


    public class ProcessWatchdogSystem : AbstractSystem
    {
        private List<AbstractThreadFactory> _watchdogThreadFactories;

        private List<AbstractThread> _watchdogThreads;

        public ProcessWatchdogSystem(List<AbstractThreadFactory> watchdogThreadFactories)
        {
            _watchdogThreadFactories = watchdogThreadFactories;
            _watchdogThreads = [];
        }

        public override void Initialize()
        {
            foreach (var watchdogThreadFactory in _watchdogThreadFactories)
            {
                _watchdogThreads.Add(watchdogThreadFactory.CreateThread());
            }
        }

        public override void Start()
        {
            foreach (var watchdogThread in _watchdogThreads)
            {
                watchdogThread.Start();
            }
        }

        public override void Inject(object dataType, object? data)
        {
            foreach (var watchdogThread in _watchdogThreads)
            {
                watchdogThread.Inject(dataType, data);
            }
        }
    }


    public class ProcessWatchdogSystemBuilder : AbstractSystemBuilder
    {
        public ProcessWatchdogSystemBuilder()
        {

        }

        public override AbstractSystem Build()
        {
            return new ProcessWatchdogSystem(
                [new RuneSolverWatchdogThreadFactory()]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
