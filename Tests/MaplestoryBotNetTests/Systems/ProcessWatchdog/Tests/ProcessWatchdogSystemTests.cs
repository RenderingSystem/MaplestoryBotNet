using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.ProcessWatchdog;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.Systems.ProcessWatchdog.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;


namespace MaplestoryBotNetTests.Systems.ProcessWatchdog.Tests
{
    public class RuneSolverWatchdogManagerTests
    {
        private MockProcessName _processName = new MockProcessName();

        private MockProcessLauncher _processLauncher = new MockProcessLauncher();

        private MockRuneSolverWatchdogClient _client = new MockRuneSolverWatchdogClient();

        public AbstractRuneSolverWatchdogManager _fixture()
        {
            _processName = new MockProcessName();
            _processLauncher = new MockProcessLauncher();
            _client = new MockRuneSolverWatchdogClient();
            return new RuneSolverWatchdogManager(
                _processName, _processLauncher, _client
            );
        }

        private List<object> _launchParameters()
        {
            return [
                new
                {
                    serverRuneModel = "123",
                    serverExecutable = "234\\876",
                    serverWatchdogTimeout = 345,
                    showConsole = 456,
                    runeSolverIPAddress = "567",
                    runeSolverPort = "678",
                    processName = "789"
                },
                new
                {
                    serverRuneModel = "123",
                    serverExecutable = "234\\876.exe",
                    serverWatchdogTimeout = 345,
                    showConsole = 456,
                    runeSolverIPAddress = "567",
                    runeSolverPort = "678",
                    processName = "789"
                },
                new
                {
                    serverRuneModel = "123",
                    serverExecutable = "876",
                    serverWatchdogTimeout = 345,
                    showConsole = 456,
                    runeSolverIPAddress = "567",
                    runeSolverPort = "678",
                    processName = "789"
                },
                new
                {
                    serverRuneModel = "123",
                    serverExecutable = "234\\876",
                    serverWatchdogTimeout = 345,
                    showConsole = 0,
                    runeSolverIPAddress = "567",
                    runeSolverPort = "678",
                    processName = "789"
                },
            ];
        }

        private List<object> _expectedLaunchInfo()
        {
            return [
                new
                {
                    fileName = "\"876.exe\"",
                    argumentList = new[] { "123", "789", "345", "567", "678" },
                    workingDirectory = "234",
                    useShellExecute = true,
                    windowStyle = ProcessWindowStyle.Normal
                },
                new
                {
                    fileName = "\"876.exe\"",
                    argumentList = new[] { "123", "789", "345", "567", "678" },
                    workingDirectory = "234",
                    useShellExecute = true,
                    windowStyle = ProcessWindowStyle.Normal
                },
                new
                {
                    fileName = "\"876.exe\"",
                    argumentList = new[] { "123", "789", "345", "567", "678" },
                    workingDirectory = ".",
                    useShellExecute = true,
                    windowStyle = ProcessWindowStyle.Normal
                },
                new
                {
                    fileName = "\"876.exe\"",
                    argumentList = new[] { "123", "789", "345", "567", "678" },
                    workingDirectory = "234",
                    useShellExecute = true,
                    windowStyle = ProcessWindowStyle.Hidden
                }
            ];
        }

        /**
         * @brief Verifies that the watchdog thread helper correctly constructs the ping URL
         * and delegates the HTTP GET request to the HTTP client
         * 
         * When the watchdog needs to check whether the rune solver server is alive and
         * responsive, it calls the Ping method with the rune detection configuration
         * (IP address, port, route) and a timeout value. This test ensures that the helper
         * properly formats the URL by combining the address, port, route, and "/ping" endpoint,
         * then forwards the request to the underlying HTTP client with the specified timeout.
         */
        public void _testPing()
        {
            var watchdogThreadHelper = _fixture();
            var responseMessage = new HttpResponseMessage();
            _client.GetReturn.Add(responseMessage);
            var result = watchdogThreadHelper.Ping(
                new RuneDetection
                {
                    RuneSolverIPAddress = "123",
                    RuneSolverPort = "456",
                    RuneSolverRoute = "789"
                },
                234
            );
            Debug.Assert(_client.GetCalls == 1);
            Debug.Assert(_client.GetCallArg_url[0] == "123:456/789/ping");
            Debug.Assert(_client.GetCallArg_timeoutMilliseconds[0] == 234);
            Debug.Assert(responseMessage == result);
        }

        /**
         * @brief Verifies that the watchdog thread helper correctly launches the rune solver
         * process with properly formatted command-line arguments and process settings
         * 
         * When the main bot application detects that the rune solver process is not running
         * (or has stopped responding), the watchdog starts a new instance.
         * This test ensures that the executable path is correctly formatted (adding quotes
         * and .exe extension as needed), the working directory is properly set, command-line
         * arguments are passed in the correct order, and window visibility is assigned.
         */
        private void _testLaunch()
        {
            var parametersList = _launchParameters();
            var expectedList = _expectedLaunchInfo();
            for (int i = 0; i < _launchParameters().Count; i++)
            {
                dynamic parameters = parametersList[i];
                dynamic expected = expectedList[i];
                var watchdogThreadHelper = _fixture();
                _processName.GetProcessNameReturn.Add(parameters.processName);
                watchdogThreadHelper.StartProcess(
                    new RuneServerSettings
                    {
                        ServerRuneModel = parameters.serverRuneModel,
                        ServerExecutable = parameters.serverExecutable,
                        ServerWatchdogTimeout = parameters.serverWatchdogTimeout,
                        ServerRuneModelConsole = parameters.showConsole
                    },
                    new RuneDetection
                    {
                        RuneSolverIPAddress = parameters.runeSolverIPAddress,
                        RuneSolverPort = parameters.runeSolverPort,
                    }
                );
                Debug.Assert(_processLauncher.LaunchCalls == 1);
                var startInfo = _processLauncher.LaunchCallArg_startInfo[0];
                Debug.Assert(startInfo.FileName == (string)expected.fileName);
                Debug.Assert(startInfo.ArgumentList[0] == (string)expected.argumentList[0]);
                Debug.Assert(startInfo.ArgumentList[1] == (string)expected.argumentList[1]);
                Debug.Assert(startInfo.ArgumentList[2] == (string)expected.argumentList[2]);
                Debug.Assert(startInfo.ArgumentList[3] == (string)expected.argumentList[3]);
                Debug.Assert(startInfo.ArgumentList[4] == (string)expected.argumentList[4]);
                Debug.Assert(startInfo.WorkingDirectory == (string)expected.workingDirectory);
                Debug.Assert(startInfo.UseShellExecute == (bool)expected.useShellExecute);
                Debug.Assert(startInfo.WindowStyle == (ProcessWindowStyle)expected.windowStyle);
            }
        }

        public void Run()
        {
            _testPing();
            _testLaunch();
        }
    }


    public class RuneSolverWatchdogHelperTests
    {
        private MockTimestamp _pingStopwatch = new MockTimestamp();

        private MockProcessMonitor _processMonitor = new MockProcessMonitor();

        private MockMacroSleeper _sleeper = new MockMacroSleeper();

        private MockRuneSolverWatchdogManager _manager = new MockRuneSolverWatchdogManager();

        private AbstractRuneSolverWatchdogHelper _fixture()
        {
            _pingStopwatch = new MockTimestamp();
            _processMonitor = new MockProcessMonitor();
            _sleeper = new MockMacroSleeper();
            _manager = new MockRuneSolverWatchdogManager();
            return new RuneSolverWatchdogHelper(
                _pingStopwatch,
                _processMonitor,
                _sleeper,
                _manager
            );
        }

        /**
         * @brief Verifies that the watchdog timer is correctly reset on each cycle
         * 
         * When the watchdog runs its monitoring loop, it must reset the stopwatch at the
         * beginning of each iteration to accurately measure the time taken for the current
         * health check. This ensures that subsequent sleep calculations correctly subtract
         * the elapsed time from the configured timeout.
         */
        private void _testSettingStopwatch()
        {
            var helper = _fixture();
            helper.SetStopwatch();
            Debug.Assert(_pingStopwatch.SetTimestampCalls == 1);
        }

        /**
         * @brief Verifies that NeedsLaunch returns true when the rune solver executable
         * is not running in the process list
         * 
         * When the rune solver process has crashed or been manually closed, the watchdog
         * must detect that the executable is not running and trigger a launch. This test
         * ensures that NeedsLaunch checks the process monitor and returns true regardless
         * of whether the executable path includes the .exe extension.
         */
        private void _testNeedsLaunchWhenExecutableNotRunning()
        {
            foreach (var executable in new[] { "123", "123.exe" })
            {
                var helper = _fixture();
                var runeDetection = new RuneDetection();
                var runeServerSettings = new RuneServerSettings { ServerExecutable = executable };
                _processMonitor.RunningReturn.Add(0);
                Debug.Assert(helper.NeedsLaunch(runeDetection, runeServerSettings));
                Debug.Assert(_processMonitor.RunningCalls == 1);
                Debug.Assert(_processMonitor.RunningCallArg_processName[0] == "123");
                Debug.Assert(_manager.PingCalls == 0);
            }
        }

        /**
         * @brief Verifies that NeedsLaunch returns true when the rune solver process is
         * running but completely unresponsive to ping requests
         * 
         * When the rune solver process is running but does not respond to HTTP health
         * checks, the watchdog must detect this unresponsive state and trigger a restart.
         * This test simulates a null response from the ping
         * (e.g., connection refused or timeout).
         */
        private void _testNeedsLaunchWhenRunningAndPingUnresponsive()
        {
            var helper = _fixture();
            var runeDetection = new RuneDetection();
            var runeServerSettings = new RuneServerSettings
            {
                ServerExecutable = "123",
                ClientWatchdogTimeout = 234
            };
            _processMonitor.RunningReturn.Add(1);
            _manager.PingReturn.Add(null);
            Debug.Assert(helper.NeedsLaunch(runeDetection, runeServerSettings));
            Debug.Assert(_processMonitor.RunningCalls == 1);
            Debug.Assert(_processMonitor.RunningCallArg_processName[0] == "123");
            Debug.Assert(_manager.PingCalls == 1);
            Debug.Assert(_manager.PingCallArg_runeDetection[0] == runeDetection);
            Debug.Assert(_manager.PingCallArg_clientWatchdogTimeout[0] == 234);
        }

        /**
         * @brief Verifies that NeedsLaunch returns true when the rune solver process is
         * running but returns an error HTTP status code from the ping endpoint
         * 
         * When the rune solver responds to ping requests but returns an error status
         * (e.g., 502 Bad Gateway, 500 Internal Server Error), the watchdog must treat
         * this as an unhealthy state and restart the process. A non-success status code
         * indicates the service is running but not functioning correctly.
         */
        private void _testNeedsLaunchWhenRunningAndPingError()
        {
            var helper = _fixture();
            var runeDetection = new RuneDetection();
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.BadGateway };
            var runeServerSettings = new RuneServerSettings
            {
                ServerExecutable = "123",
                ClientWatchdogTimeout = 234
            };
            _processMonitor.RunningReturn.Add(1);
            _manager.PingReturn.Add(response);
            Debug.Assert(helper.NeedsLaunch(runeDetection, runeServerSettings));
            Debug.Assert(_processMonitor.RunningCalls == 1);
            Debug.Assert(_processMonitor.RunningCallArg_processName[0] == "123");
            Debug.Assert(_manager.PingCalls == 1);
            Debug.Assert(_manager.PingCallArg_runeDetection[0] == runeDetection);
            Debug.Assert(_manager.PingCallArg_clientWatchdogTimeout[0] == 234);
        }

        /**
         * @brief Verifies that NeedsLaunch returns false when the rune solver is both
         * running and responding with a successful HTTP status code
         * 
         * When the rune solver process is running and the ping endpoint returns HTTP 200 OK,
         * the watchdog should consider the service healthy and not attempt to restart it.
         * This is the normal operating state of a healthy rune solver.
         */
        private void _testNeedsLaunchWhenRunningAndPingSuccessful()
        {
            var helper = _fixture();
            var runeDetection = new RuneDetection();
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var runeServerSettings = new RuneServerSettings
            {
                ServerExecutable = "123",
                ClientWatchdogTimeout = 234
            };
            _processMonitor.RunningReturn.Add(1);
            _manager.PingReturn.Add(response);
            Debug.Assert(!helper.NeedsLaunch(runeDetection, runeServerSettings));
            Debug.Assert(_processMonitor.RunningCalls == 1);
            Debug.Assert(_processMonitor.RunningCallArg_processName[0] == "123");
            Debug.Assert(_manager.PingCalls == 1);
            Debug.Assert(_manager.PingCallArg_runeDetection[0] == runeDetection);
            Debug.Assert(_manager.PingCallArg_clientWatchdogTimeout[0] == 234);
        }

        /**
         * @brief Verifies that Launch correctly delegates process startup to the manager
         * 
         * When the watchdog determines that the rune solver needs to be started or
         * restarted, the Launch method must forward the request to the underlying manager
         * with the correct configuration settings (server settings and rune detection
         * settings).
         */
        private void _testLaunchStartsProcess()
        {
            var helper = _fixture();
            var runeDetection = new RuneDetection();
            var runeServerSettings = new RuneServerSettings();
            helper.Launch(runeDetection, runeServerSettings);
            Debug.Assert(_manager.StartProcessCalls == 1);
            Debug.Assert(_manager.StartProcessCallArg_runeServerSettings[0] == runeServerSettings);
            Debug.Assert(_manager.StartProcessCallArg_runeDetection[0] == runeDetection);
        }

        /**
         * @brief Verifies that SleepRemaining correctly calculates and sleeps for the
         * remaining time after subtracting elapsed ping duration
         * 
         * When the watchdog completes a health check cycle, it must sleep for the
         * remaining portion of the configured timeout. If the health check took 1.5 seconds
         * and the timeout is 1.5 seconds, it should sleep for 0 milliseconds (immediate next check).
         * If the timeout is longer than elapsed time, it should sleep for the difference.
         * A negative remaining time is capped at 0 (no sleep).
         */
        private void _testSleepRemainingWaitTime()
        {
            var timeouts = new[] { 1503, 1502, 1501, 1500, 1499 };
            var expected = new[] { 3, 2, 1, 0, 0 };
            for (int i = 0; i < timeouts.Length; i++)
            {
                var helper = _fixture();
                _pingStopwatch.GetTimestampReturn.Add(1.5);
                helper.SleepRemaining(
                    new RuneServerSettings { ClientWatchdogTimeout = timeouts[i] }
                );
                Debug.Assert(_sleeper.SleepCalls == 1);
                Debug.Assert(_sleeper.SleepCallArg_milliseconds[0] == expected[i]);
            }
        }

        public void Run()
        {
            _testSettingStopwatch();
            _testNeedsLaunchWhenExecutableNotRunning();
            _testNeedsLaunchWhenRunningAndPingUnresponsive();
            _testNeedsLaunchWhenRunningAndPingError();
            _testNeedsLaunchWhenRunningAndPingSuccessful();
            _testLaunchStartsProcess();
            _testSleepRemainingWaitTime();
        }
    }


    public class RuneSolverWatchdogThreadTests
    {
        private MockRuneSolverWatchdogHelper _helper = new MockRuneSolverWatchdogHelper();

        private MockRunningState _runningState = new MockRunningState();

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = new MaplestoryBotConfiguration();

        private AbstractThread _fixture()
        {
            _helper = new MockRuneSolverWatchdogHelper();
            _runningState = new MockRunningState();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                RuneDetection = new RuneDetection
                {
                    RuneSolverIPAddress = "123",
                    RuneSolverPort = "234",
                    RuneSolverRoute = "345",
                    X = "a",
                    Y = "b",
                    ClassTag = "c",
                    Left = "d",
                    Up = "e",
                    Right = "f",
                    Down = "g"
                },
                RuneServerSettings = new RuneServerSettings
                {
                    ServerExecutable = "asd",
                    ClientWatchdogTimeout = 123,
                    ServerRuneModel = "234",
                    ServerRuneModelConsole = 345,
                    ServerWatchdogTimeout = 456
                }
            };
            _runningState.IsRunningReturn.Add(false);
            return new RuneSolverWatchdogThread(
                _helper, _runningState
            );
        }

        private void _assertEqual(object _1, object _2)
        {
            var json1 = JsonSerializer.Serialize(_1);
            var json2 = JsonSerializer.Serialize(_2);
            Debug.Assert(json1 == json2);
        }

        /**
         * @brief Verifies that the watchdog thread correctly executes the monitoring loop.
         * 
         * When the watchdog thread is running, it must continuously monitor the health of
         * the rune solver service. On each iteration of the loop, the thread should:
         * 1. Reset the stopwatch to measure the current check duration
         * 2. Determine if the service needs to be launched (based on process status and ping)
         * 3. Launch the service if needed (otherwise skip)
         * 4. Sleep for the remaining time until the next check
         */
        private void _testThreadLoopLaunchesWhenNeeded()
        {
            for (int i = 1; i < 10; i++)
            {
                var runeSolverWatchdog = _fixture();
                var helperRef = new TestUtilities().Reference(_helper);
                runeSolverWatchdog.Inject(
                    SystemInjectType.ConfigurationUpdate,
                    _maplestoryBotConfiguration
                );
                for (int j = 0; j < i; j++)
                {
                    _helper.NeedsLaunchReturn.Add((j % 2) == 0);
                    _runningState.IsRunningReturn.Add(true);
                }
                _runningState.IsRunningReturn.Add(false);
                runeSolverWatchdog.Start();
                runeSolverWatchdog.Join(10000);
                var oddCount = Math.Floor(i / 2.0) * 3;
                var evenCount = Math.Ceiling(i / 2.0) * 4;
                Debug.Assert(_helper.CallOrder.Count == oddCount + evenCount);
                for (int j = 0; j < _helper.CallOrder.Count; j += 7)
                {
                    Debug.Assert(_helper.CallOrder[j + 0] == helperRef + "SetStopwatch");
                    Debug.Assert(_helper.CallOrder[j + 1] == helperRef + "NeedsLaunch");
                    Debug.Assert(_helper.CallOrder[j + 2] == helperRef + "Launch");
                    Debug.Assert(_helper.CallOrder[j + 3] == helperRef + "SleepRemaining");
                }
                for (int j = 4; j < _helper.CallOrder.Count; j += 7)
                {
                    Debug.Assert(_helper.CallOrder[j + 0] == helperRef + "SetStopwatch");
                    Debug.Assert(_helper.CallOrder[j + 1] == helperRef + "NeedsLaunch");
                    Debug.Assert(_helper.CallOrder[j + 2] == helperRef + "SleepRemaining");
                }
            }
        }

        /**
         * @brief Verifies that the watchdog thread correctly uses the rune detection
         * and server settings injected via configuration update
         * 
         * When the main bot loads a configuration file containing rune solver settings
         * (IP address, port, route, arrow mappings, executable path, timeouts, etc.),
         * these settings must be injected into the watchdog thread. The watchdog then
         * uses these settings for all subsequent health checks and launch operations.
         */
        private void _testThreadLoopUsesInjectedRuneSettings()
        {
            var runeSolverWatchdog = _fixture();
            runeSolverWatchdog.Inject(
                SystemInjectType.ConfigurationUpdate,
                _maplestoryBotConfiguration
            );
            _helper.NeedsLaunchReturn.Add(true);
            _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
            runeSolverWatchdog.Start();
            runeSolverWatchdog.Join(10000);
            _assertEqual(
                _helper.NeedsLaunchCallArg_runeDetection[0],
                _maplestoryBotConfiguration.RuneDetection
            );
            _assertEqual(
                _helper.NeedsLaunchCallArg_runeServerSettings[0],
                _maplestoryBotConfiguration.RuneServerSettings
            );
            _assertEqual(
                _helper.LaunchCallArg_runeDetection[0],
                _maplestoryBotConfiguration.RuneDetection
            );
            _assertEqual(
                _helper.LaunchCallArg_runeServerSettings[0],
                _maplestoryBotConfiguration.RuneServerSettings
            );
            _assertEqual(
                _helper.SleepRemainingCallArg_runeServerSettings[0],
                _maplestoryBotConfiguration.RuneServerSettings
            );
        }

        public void Run()
        {
            _testThreadLoopLaunchesWhenNeeded();
            _testThreadLoopUsesInjectedRuneSettings();
        }
    }


    public class ProcessWatchdogSystemTests
    {
        private List<AbstractThreadFactory> _watchdogThreadFactories = [];

        private List<AbstractThread> _watchdogThreads = [];

        private AbstractSystem _fixture()
        {
            _watchdogThreadFactories = [
                new MockThreadFactory(),
                new MockThreadFactory(),
                new MockThreadFactory()
            ];
            _watchdogThreads = [];
            for (int i = 0; i < _watchdogThreadFactories.Count; i++)
            {
                _watchdogThreads.Add(new MockThread(new ThreadRunningState()));
                var mockThreadFactory = (MockThreadFactory)_watchdogThreadFactories[i];
                mockThreadFactory.CreateThreadReturn.Add(_watchdogThreads[i]);
            }
            return new ProcessWatchdogSystem(
                _watchdogThreadFactories
            );
        }
        /**
         * @brief Verifies that initializing the process watchdog system creates all
         * configured watchdog threads without starting them
         * 
         * When the main bot application starts up and initializes the process watchdog
         * system, it must create all watchdog threads that will monitor various processes
         * The threads should be created and ready to start, but should not begin execution
         * until explicitly started.
         */
        private void _testInitializingCreatesWatchdogThreads()
        {
            var processWatchdogSystem = _fixture();
            processWatchdogSystem.Initialize();
            foreach (MockThreadFactory watchdogThreadFactory in _watchdogThreadFactories)
            {
                Debug.Assert(watchdogThreadFactory.CreateThreadCalls == 1);
            }
        }

        /**
         * @brief Verifies that starting the process watchdog system launches all
         * previously created watchdog threads
         * 
         * After the watchdog system has been initialized, the main bot application must
         * be able to start all watchdog threads simultaneously. This ensures that all
         * monitored processes are actively being watched for health and availability.
         */
        private void _testStartingStartsWatchdogThreads()
        {
            var processWatchdogSystem = _fixture();
            processWatchdogSystem.Initialize();
            foreach (MockThread watchdogThread in _watchdogThreads)
            {
                Debug.Assert(watchdogThread.ThreadStartCalls == 0);
            }
            processWatchdogSystem.Start();
            foreach (MockThread watchdogThread in _watchdogThreads)
            {
                Debug.Assert(watchdogThread.ThreadStartCalls == 1);
            }
        }

        /**
         * @brief Verifies that configuration updates injected into the watchdog system
         * are propagated to all active watchdog threads
         * 
         * When the user loads a new configuration file or updates settings (such as
         * rune solver IP address, port, executable paths, or timeouts), the watchdog
         * system must broadcast these changes to every monitoring thread. This ensures
         * all watchdogs use the latest configuration for process monitoring and
         * health checks.
         */
        private void _testInjectingInjectsToWatchdogThreads()
        {
            var processWatchdogSystem = _fixture();
            processWatchdogSystem.Initialize();
            processWatchdogSystem.Start();
            foreach (MockThread watchdogThread in _watchdogThreads)
            {
                Debug.Assert(watchdogThread.InjectCalls == 0);
            }
            processWatchdogSystem.Inject(123, 234);
            foreach (MockThread watchdogThread in _watchdogThreads)
            {
                Debug.Assert(watchdogThread.InjectCalls == 1);
                Debug.Assert((int)watchdogThread.InjectCallArg_dataType[0] == 123);
                Debug.Assert((int)watchdogThread.InjectCallArg_data[0]! == 234);
            }

        }

        public void Run()
        {
            _testInitializingCreatesWatchdogThreads();
            _testStartingStartsWatchdogThreads();
            _testInjectingInjectsToWatchdogThreads();
        }
    }


    public class ProcessWatchdogSystemTestSuite
    {
        public void Run()
        {
            new RuneSolverWatchdogManagerTests().Run();
            new RuneSolverWatchdogHelperTests().Run();
            new RuneSolverWatchdogThreadTests().Run();
            new ProcessWatchdogSystemTests().Run();
        }
    }
}
