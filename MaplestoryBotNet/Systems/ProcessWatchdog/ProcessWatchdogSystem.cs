using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using System.Diagnostics;
using System.IO;
using System.Net.Http;


namespace MaplestoryBotNet.Systems.ProcessWatchdog
{
    public abstract class AbstractRuneSolverPinger
    {
        public abstract HttpResponseMessage? Ping(
            RuneDetection runeDetection,
            int clientWatchdogTimeout
        );
    }


    public abstract class AbstractRuneSolverController
    {
        public abstract void Start(
            RuneServerSettings runeServerSettings,
            RuneDetection runeDetection
        );

        public abstract void Purge(
            RuneServerSettings runeServerSettings
        );
    }


    public abstract class AbstractRuneSolverClient
    {
        public abstract HttpResponseMessage? Get(string url, int timeoutMilliseconds);
    }


    public abstract class AbstractRuneSolverWatchdogTimer
    {
        public abstract void SetStopwatch();

        public abstract void SleepRemaining(
            RuneServerSettings runeServerSettings
        );
    }


    public abstract class AbstractRuneSolverSupervisor
    {
        public abstract void EnsureRunning(
            RuneDetection runeDetection,
            RuneServerSettings runeServerSettings
        );
    }


    public abstract class AbstractRuneSolverHealthMonitor
    {
        public abstract bool NeedsLaunch(
            RuneDetection runeDetection,
            RuneServerSettings runeServerSettings
        );
    }


    public class RuneSolverClient : AbstractRuneSolverClient
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


    public class RuneSolverPinger : AbstractRuneSolverPinger
    {
        private AbstractRuneSolverClient _watchdogClient;
        public RuneSolverPinger(
            AbstractRuneSolverClient watchdogClient
        )
        {
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
    }


    public class RuneSolverHealthMonitor : AbstractRuneSolverHealthMonitor
    {
        private AbstractProcessMonitor _processMonitor;

        private AbstractRuneSolverPinger _watchdogPinger;

        public RuneSolverHealthMonitor(
            AbstractRuneSolverPinger watchdogPinger,
            AbstractProcessMonitor processMonitor
        )
        {
            _watchdogPinger = watchdogPinger;
            _processMonitor = processMonitor;
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
            if (_processMonitor.Running(executable).Count == 0)
            {
                return true;
            }
            var response = _watchdogPinger.Ping(runeDetection, timeout);
            return response?.IsSuccessStatusCode != true;
        }
    }


    public class RuneSolverController : AbstractRuneSolverController
    {
        private AbstractProcessName _processName;

        private AbstractProcessMonitor _processMonitor;

        private AbstractProcessStarter _processLauncher;

        public RuneSolverController(
            AbstractProcessName processName,
            AbstractProcessMonitor processMonitor,
            AbstractProcessStarter processLauncher
        )
        {
            _processName = processName;
            _processMonitor = processMonitor;
            _processLauncher = processLauncher;
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

        private string _parseProcessName(string executable)
        {
            executable = Path.GetFileName(executable).Trim('"');
            executable = executable.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ?
                executable.Substring(0, executable.Length - ".exe".Length) : executable;
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

        public override void Start(
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
            _processLauncher.Start(startInfo);
        }

        public override void Purge(
            RuneServerSettings runeServerSettings
        )
        {
            var processName = _parseProcessName(runeServerSettings.ServerExecutable);
            var processes = _processMonitor.Running(processName);
            foreach (var process in processes)
            {
                process.Kill(runeServerSettings.ClientWatchdogTimeout);
            }
        }
    }


    public class RuneSolverSupervisor : AbstractRuneSolverSupervisor
    {
        private AbstractRuneSolverController _controller;

        private AbstractRuneSolverHealthMonitor _healthMonitor;

        private int _needsLaunchCount;

        private int _maxLaunchTolerance;

        public RuneSolverSupervisor(
            AbstractRuneSolverController controller,
            AbstractRuneSolverHealthMonitor healthMonitor,
            int maxLaunchTolerance
        )
        {
            _controller = controller;
            _healthMonitor = healthMonitor;
            _needsLaunchCount = 0;
            _maxLaunchTolerance = maxLaunchTolerance;
        }

        public override void EnsureRunning(
            RuneDetection runeDetection,
            RuneServerSettings runeServerSettings
        )
        {
            if (_healthMonitor.NeedsLaunch(runeDetection, runeServerSettings))
            {
                if (++_needsLaunchCount >= _maxLaunchTolerance)
                {
                    _controller.Purge(runeServerSettings);
                    _needsLaunchCount = 0;
                }
                _controller.Start(runeServerSettings, runeDetection);
            }
            else
            {
                _needsLaunchCount = 0;
            }
        }
    }


    public class RuneSolverWatchdogTimer : AbstractRuneSolverWatchdogTimer
    {
        private AbstractTimestamp _pingStopwatch;

        private AbstractMacroSleeper _sleeper;

        public RuneSolverWatchdogTimer(
            AbstractTimestamp pingStopwatch,
            AbstractMacroSleeper sleeper
        )
        {
            _pingStopwatch = pingStopwatch;
            _sleeper = sleeper;
        }

        public override void SetStopwatch()
        {
            _pingStopwatch.SetTimestamp();
        }

        public override void SleepRemaining(
            RuneServerSettings runeServerSettings
        )
        {
            var elapsed = (int)(_pingStopwatch.GetTimestamp() * 1000);
            var timeout = runeServerSettings.ClientWatchdogTimeout;
            _sleeper.Sleep(timeout - elapsed);
        }
    }


    public class RuneSolverWatchdogThread : AbstractThread
    {
        private AbstractRuneSolverWatchdogTimer _timer;

        private AbstractRuneSolverSupervisor _supervisor;

        private RuneDetection? _runeDetection;

        private RuneServerSettings? _runeServerSettings;

        public RuneSolverWatchdogThread(
            AbstractRuneSolverWatchdogTimer timer,
            AbstractRuneSolverSupervisor supervisor,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _timer = timer;
            _supervisor = supervisor;
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
                    _timer.SetStopwatch();
                    _supervisor.EnsureRunning(runeDetection, runeServerSettings);
                    _timer.SleepRemaining(runeServerSettings);
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
                && maplestoryBotConfiguration.RuneDetection.Copy() is RuneDetection runeDetection
                && maplestoryBotConfiguration.RuneServerSettings.Copy() is RuneServerSettings runeServerSettings
            )
            {
                _runeDetection = runeDetection;
                _runeServerSettings = runeServerSettings;
            }
        }
    }


    public class RuneSolverWatchdogThreadFactory : AbstractThreadFactory
    {
        public override AbstractThread CreateThread()
        {
            return new RuneSolverWatchdogThread(
                new RuneSolverWatchdogTimer(
                    new StopwatchTimestamp(),
                    new MacroSleeper()
                ),
                new RuneSolverSupervisor(
                    new RuneSolverController(
                        new ProcessName(),
                        new ProcessMonitor(),
                        new ProcessStarter()
                    ),
                    new RuneSolverHealthMonitor(
                        new RuneSolverPinger(
                            new RuneSolverClient()
                        ),
                        new ProcessMonitor()
                    ),
                    3
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
