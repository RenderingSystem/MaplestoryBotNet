using MaplestoryRuneSolver.RuneSolving;
using MaplestoryRuneSolverTests.RuneSolving.Mocks;
using MaplestoryRuneSolverTests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;


namespace MaplestoryRuneSolverTests.RuneSolving.Tests
{
    public class RuneControllerTests
    {
        private MockRuneSolvingAgent _runeSolvingAgent = new MockRuneSolvingAgent();

        private MockTimestamp _pingStopwatch = new MockTimestamp();

        private RuneController _fixture()
        {
            _runeSolvingAgent = new MockRuneSolvingAgent();
            _pingStopwatch = new MockTimestamp();
            var runeController = new RuneController();
            RuneController.RuneSolvingAgent = _runeSolvingAgent;
            RuneController.PingStopwatch = _pingStopwatch;
            return runeController;
        }

        /**
         * @brief Verifies that the RuneController responds to ping requests with a
         * "Running" status indicator for server health checks
         * 
         * When the main bot application starts up or needs to verify that the rune
         * solving server is operational, it sends a GET request to the ping endpoint
         * (e.g., "GET /api/rune/ping"). This test ensures that the controller responds
         * with an HTTP 200 OK status and returns the string "Running". This health check
         * allows the main bot to confirm that the rune solving service is available.
         */
        private void _testRuneControllerReturnsPingRequests()
        {
            var controller = _fixture();
            var result = controller.Ping();
            Debug.Assert(result is OkObjectResult);
            var okResult = (OkObjectResult)result;
            Debug.Assert(okResult.StatusCode == 200);
            Debug.Assert(okResult.Value?.ToString() == "Running");
        }

        /**
         * @brief Verifies that the ping endpoint resets the watchdog's timeout timer
         * 
         * When the main bot application sends periodic ping requests to the rune solver
         * server, each successful ping "pets" the watchdog, resetting its internal timer.
         * This tells the watchdog that the parent process is still alive and functioning
         * correctly. If the watchdog stops receiving these pings (i.e., no one pets it
         * within the timeout period), it assumes the monitored process has stopped or
         * become unresponsive and will shut down the rune solver service.
         */
        private void _testRuneControllerPetsWatchdog()
        {
            var controller = _fixture();
            var result = controller.Ping();
            Debug.Assert(_pingStopwatch.SetTimestampCalls == 1);
        }

        /**
         * @brief Verifies that the RuneController successfully processes POST requests containing
         * base64-encoded rune images and returns the solving agent's prediction results
         * 
         * When the main bot application captures a rune puzzle image and sends it to the
         * solve endpoint (POST /api/rune/solve), the controller should forward the image
         * to the rune solving agent and return the agent's response. This ensures the end-to-end
         * flow from HTTP request to model inference completes successfully.
         */
        private void _testRuneControllerSolvesPostRequests()
        {
            var controller = _fixture();
            _runeSolvingAgent.SolveReturn.Add("meow");
            var result = controller.Solve("pop tarts");
            Debug.Assert(result is OkObjectResult);
            var okResult = (OkObjectResult)result;
            Debug.Assert(okResult.StatusCode == 200);
            Debug.Assert(okResult.Value?.ToString() == "meow");
        }

        /**
         * @brief Verifies that the RuneController correctly extracts the base64 image string
         * from the POST request body and passes it to the rune solving agent
         * 
         * When users send a rune image to the solve endpoint, the controller must extract
         * the base64-encoded image data from the request body and forward it unmodified to
         * the solving agent. This ensures the agent receives the exact image data needed
         * for inference without any corruption or transformation.
         */
        private void _testRuneControllerSolvesPostRequestImages()
        {
            var controller = _fixture();
            _runeSolvingAgent.SolveReturn.Add("meow");
            var result = controller.Solve("pop tarts");
            Debug.Assert(_runeSolvingAgent.SolveCallArg_base64Image[0] == "pop tarts");
        }

        /**
         * @brief Verifies that the RuneController returns an HTTP 500 Internal Server Error
         * when the rune solving agent fails to process the image
         * 
         * When the rune solving agent encounters an error (e.g., model not loaded, invalid
         * image format, inference failure), the controller should report this to the caller
         * as a server error rather than returning a success status. This allows the main
         * bot application to distinguish between successful predictions and processing
         * failures, and potentially retry or log the error appropriately.
         */
        private void _testRuneControllerServerErrorOnPostRequest()
        {
            var controller = _fixture();
            _runeSolvingAgent.SolveReturn.Add("");
            var result = controller.Solve("pop tarts");
            Debug.Assert(result is StatusCodeResult);
            var statusCodeResult = (StatusCodeResult)result;
            Debug.Assert(statusCodeResult.StatusCode == 500);
        }

        private void _testRuneControllerHasCorrectRoute()
        {
            var controller = _fixture();
            var type = controller.GetType();
            var routeAttribute = (
                (RouteAttribute)
                Attribute.GetCustomAttribute(type, typeof(RouteAttribute))!
            );
            Debug.Assert(routeAttribute != null);
            Debug.Assert(routeAttribute.Template == "api/rune");
        }

        /**
         * @brief Verifies that the Ping action has the correct HTTP GET attribute
         * with route template
         * 
         * The Ping method provides a health check endpoint that the main bot uses to verify
         * the rune solver server is operational before sending solving requests. This test
         * ensures the method is correctly decorated with [HttpGet("ping")], making it respond
         * to GET requests at the URL "api/rune/ping".
         */
        public void _testRuneControllerPingHasCorrectHttpGetAttribute()
        {
            var controller = _fixture();
            var method = typeof(RuneController).GetMethod("Ping");
            var attribute = method?.GetCustomAttribute<HttpGetAttribute>();
            Debug.Assert(attribute != null);
            Debug.Assert(attribute.Template == "ping");
        }

        /**
         * @brief Verifies that the Solve action has the correct HTTP POST attribute
         * with route template
         * 
         * The Solve method processes rune puzzle images sent by the main bot. This test
         * ensures the method is correctly decorated with [HttpPost("solve")], making it
         * respond to POST requests at the URL "api/rune/solve". This is the primary endpoint
         * for rune detection and solving functionality.
         */
        public void _testRuneControllerSolveHasCorrectHttpPostAttribute()
        {
            var controller = _fixture();
            var method = typeof(RuneController).GetMethod("Solve");
            var attribute = method?.GetCustomAttribute<HttpPostAttribute>();
            Debug.Assert(attribute != null);
            Debug.Assert(attribute.Template == "solve");
        }

        public void Run()
        {
            try
            {
                _testRuneControllerReturnsPingRequests();
                _testRuneControllerPetsWatchdog();
                _testRuneControllerSolvesPostRequests();
                _testRuneControllerSolvesPostRequestImages();
                _testRuneControllerServerErrorOnPostRequest();
                _testRuneControllerHasCorrectRoute();
                _testRuneControllerPingHasCorrectHttpGetAttribute();
                _testRuneControllerSolveHasCorrectHttpPostAttribute();
            }
            finally
            {
                RuneController.RuneSolvingAgent = null;
                RuneController.PingStopwatch = null;
            }
        }
    }


    public class RuneSolvingServerTests
    {
        private MockProcessName _processName = new MockProcessName();

        private MockProcessMonitor _processMonitor = new MockProcessMonitor();

        private MockWebAppHost _webAppHost = new MockWebAppHost();

        private MockWatchdog _watchdog = new MockWatchdog();

        private MockRuneSolvingAgent _runeSolvingAgent = new MockRuneSolvingAgent();

        private List<string> _callOrder = new List<string>();

        private AbstractRuneSolvingServer _fixture(
            string runeModelPath = "12",
            string watchProcessName = "23",
            string watchdogTimeout = "34",
            string serverIpAddress = "45",
            string serverPort = "56"
        )
        {
            RuneController.RuneSolvingAgent = null;
            _processName = new MockProcessName();
            _processMonitor = new MockProcessMonitor();
            _webAppHost = new MockWebAppHost();
            _watchdog = new MockWatchdog();
            _runeSolvingAgent = new MockRuneSolvingAgent();
            _callOrder = new List<string>();
            _webAppHost.CallOrder = _callOrder;
            _watchdog.CallOrder = _callOrder;
            return new RuneSolvingServer(
                _processName,
                _processMonitor,
                _webAppHost,
                _watchdog,
                _runeSolvingAgent,
                runeModelPath,
                watchProcessName,
                watchdogTimeout,
                serverIpAddress,
                serverPort
            );
        }

        /**
         * @brief Verifies that the rune solving server starts all components in the correct order
         * 
         * When users launch the rune solver service, the system must start the web server first
         * so it can listen for rune image requests, then start the watchdog to monitor the parent
         * process. When the parent process stops, the web server shuts down.
         * This ensures the service only runs while needed and cleans up properly.
         */
        private void _testLaunchingServer()
        {
            var runeSolvingServer = _fixture();
            var processMonitorRef = new TestUtilities().Reference(_processMonitor);
            var runeSolvingAgentRef = new TestUtilities().Reference(_runeSolvingAgent);
            var webAppHostRef = new TestUtilities().Reference(_webAppHost);
            var watchdogRef = new TestUtilities().Reference(_watchdog);
            _processName.GetProcessNameReturn.Add("meow");
            _processMonitor.RunningReturn.Add(1);
            _runeSolvingAgent.LoadModelReturn.Add(true);
            _webAppHost.StartReturn.Add(true);
            _processMonitor.CallOrder = _callOrder;
            _runeSolvingAgent.CallOrder = _callOrder;
            runeSolvingServer.Launch();
            Debug.Assert(_callOrder.Count == 5);
            Debug.Assert(_callOrder[0] == processMonitorRef + "Running");
            Debug.Assert(_callOrder[1] == runeSolvingAgentRef + "LoadModel");
            Debug.Assert(_callOrder[2] == webAppHostRef + "Start");
            Debug.Assert(_callOrder[3] == watchdogRef + "Start");
            Debug.Assert(_callOrder[4] == webAppHostRef + "Stop");
        }

        /**
         * @brief Verifies that the web server binds to the correct network address and port
         * 
         * When users configure the rune solver with a specific IP address and port (e.g.,
         * "localhost:8080"), the system must start the web server on that exact endpoint.
         * This allows the main bot application to know where to send rune image requests.
         */
        private void _testLaunchingServerUrl()
        {
            var runeSolvingServer = _fixture();
            _processName.GetProcessNameReturn.Add("meow");
            _processMonitor.RunningReturn.Add(1);
            _runeSolvingAgent.LoadModelReturn.Add(true);
            _webAppHost.StartReturn.Add(true);
            runeSolvingServer.Launch();
            Debug.Assert(_webAppHost.StartCallArg_url[0] == "45:56/");
        }

        /**
         * @brief Verifies that the watchdog monitors the correct parent process with the
         * configured timeout value
         * 
         * When users specify which parent process to watch (e.g., the main bot executable)
         * and a timeout duration, the watchdog must monitor that specific process. If the
         * parent process stops running, the watchdog completes and triggers the rune solver
         * to shut down.
         */
        private void _testLaunchingServerWatchdog()
        {
            var runeSolvingServer = _fixture();
            _processName.GetProcessNameReturn.Add("meow");
            _processMonitor.RunningReturn.Add(1);
            _runeSolvingAgent.LoadModelReturn.Add(true);
            _webAppHost.StartReturn.Add(true);
            runeSolvingServer.Launch();
            Debug.Assert(_watchdog.StartCallArg_watchProcessName[0] == "23");
            Debug.Assert(_watchdog.StartCallArg_watchdogTimeout[0] == 34);
        }

        /**
         * @brief Verifies that the server checks whether another instance is already running
         * before starting
         * 
         * When users launch the rune solver, the system first checks if the current process
         * name is already running. This prevents duplicate instances from running simultaneously
         * and competing for the same network port or system resources.
         */
        private void _testLaunchingServerChecksCurrentlyRunning()
        {
            var runeSolvingServer = _fixture();
            _processName.GetProcessNameReturn.Add("meow");
            _processMonitor.RunningReturn.Add(1);
            _runeSolvingAgent.LoadModelReturn.Add(true);
            _webAppHost.StartReturn.Add(true);
            runeSolvingServer.Launch();
            Debug.Assert(_processMonitor.RunningCalls == 1);
            Debug.Assert(_processMonitor.RunningCallArg_processName[0] == "meow");
        }

        /**
         * @brief Verifies that the server loads the machine learning model from the specified
         * file path at startup
         * 
         * When users provide a path to a trained rune solving model file,
         * the system must load this model into the rune solving agent before accepting any
         * image processing requests. This ensures the solver is ready to predict arrow
         * directions when rune images arrive.
         */
        private void _testLaunchingServerLoadsModelAtPath()
        {
            var runeSolvingServer = _fixture();
            _processName.GetProcessNameReturn.Add("meow");
            _processMonitor.RunningReturn.Add(1);
            _runeSolvingAgent.LoadModelReturn.Add(true);
            _webAppHost.StartReturn.Add(true);
            runeSolvingServer.Launch();
            Debug.Assert(_runeSolvingAgent.LoadModelCalls == 1);
            Debug.Assert(_runeSolvingAgent.LoadModelCallArg_runeModelPath[0] == "12");
        }

        /**
         * @brief Verifies that the rune controller has access to the loaded rune solving agent
         * for processing incoming HTTP requests
         * 
         * When users send rune images to the web server's solve endpoint, the controller needs
         * access to the rune solving agent to actually process the images and return predictions.
         * The system must inject the loaded agent into the controller so it can handle requests.
         */
        private void _testLaunchingServerLoadsRuneSolvingAgent()
        {
            var runeSolvingServer = _fixture();
            _processName.GetProcessNameReturn.Add("meow");
            _processMonitor.RunningReturn.Add(1);
            _runeSolvingAgent.LoadModelReturn.Add(true);
            _webAppHost.StartReturn.Add(true);
            runeSolvingServer.Launch();
            Debug.Assert(RuneController.RuneSolvingAgent == _runeSolvingAgent);
        }

        /**
         * @brief Verifies that the server refuses to start when an invalid watchdog timeout
         * is provided
         * 
         * When users enter a negative number or non-numeric text for the watchdog timeout,
         * the system should reject the configuration and fail to start.
         */
        private void _testLaunchingServerFailsOnInvalidWatchdog()
        {
            foreach (var timeout in new[] { "-1", "abc" })
            {
                var runeSolvingServer = _fixture(watchdogTimeout: timeout);
                _processName.GetProcessNameReturn.Add("meow");
                _processMonitor.RunningReturn.Add(1);
                _runeSolvingAgent.LoadModelReturn.Add(true);
                _webAppHost.StartReturn.Add(true);
                runeSolvingServer.Launch();
                Debug.Assert(RuneController.RuneSolvingAgent == null);
                Debug.Assert(_callOrder.Count == 0);
            }
        }

        /**
         * @brief Verifies that the server refuses to start when an invalid port number is
         * provided
         * 
         * When users enter a port number that is negative, non-numeric, or exceeds 65535
         * (the maximum valid port), the system should reject the configuration and fail
         * to start. Port numbers must be between 0 and 65535 to be valid for network binding.
         */
        private void _testLaunchingServerFailsOnInvalidPort()
        {
            foreach (var port in new[] { "-1", "abc", "99999" })
            {
                var runeSolvingServer = _fixture(serverPort: port);
                _processName.GetProcessNameReturn.Add("meow");
                _processMonitor.RunningReturn.Add(1);
                _runeSolvingAgent.LoadModelReturn.Add(true);
                _webAppHost.StartReturn.Add(true);
                runeSolvingServer.Launch();
                Debug.Assert(RuneController.RuneSolvingAgent == null);
                Debug.Assert(_callOrder.Count == 0);
            }
        }

        /**
         * @brief Verifies that the server refuses to start when the current process is already
         * running
         * 
         * When users try to launch the rune solver but an instance of the same process is already
         * running, the system should detect this and prevent a second instance from starting.
         * This avoids port conflicts and resource contention between duplicate instances.
         */
        private void _testLaunchingServerFailsIfAlreadyRunning()
        {
            foreach (var count in new[] { 2, 10, 999 })
            {
                var runeSolvingServer = _fixture();
                _processName.GetProcessNameReturn.Add("meow");
                _processMonitor.RunningReturn.Add(count);
                _runeSolvingAgent.LoadModelReturn.Add(true);
                _webAppHost.StartReturn.Add(true);
                runeSolvingServer.Launch();
                Debug.Assert(RuneController.RuneSolvingAgent == null);
                Debug.Assert(_callOrder.Count == 0);
            }
        }

        /**
         * @brief Verifies that the server refuses to start when the rune solving model fails
         * to load
         * 
         * When users provide a model file that is corrupt, missing, or in an unsupported format,
         * the agent will fail to load it. The system should detect this failure and abort the
         * server startup, as the rune solver cannot function without a valid model.
         */
        private void _testLaunchingServerFailsIfModelLoadFails()
        {
            var runeSolvingServer = _fixture();
            _processName.GetProcessNameReturn.Add("meow");
            _processMonitor.RunningReturn.Add(1);
            _runeSolvingAgent.LoadModelReturn.Add(false);
            _webAppHost.StartReturn.Add(true);
            runeSolvingServer.Launch();
            Debug.Assert(RuneController.RuneSolvingAgent == null);
            Debug.Assert(_callOrder.Count == 0);
        }

        /**
         * @brief Verifies that the server aborts startup when the web server fails to bind to
         * the configured address
         * 
         * When users request a port or IP address that is already in use, requires admin
         * privileges, or is otherwise unavailable, the web server will fail to start. The
         * system should detect this failure and abort the entire startup process without
         * starting the watchdog.
         */
        private void _testLaunchingServerFailsIfWebAppHostFails()
        {
            var runeSolvingServer = _fixture();
            var webAppHostRef = new TestUtilities().Reference(_webAppHost);
            _processName.GetProcessNameReturn.Add("meow");
            _processMonitor.RunningReturn.Add(1);
            _runeSolvingAgent.LoadModelReturn.Add(true);
            _webAppHost.StartReturn.Add(false);
            runeSolvingServer.Launch();
            Debug.Assert(RuneController.RuneSolvingAgent == null);
            Debug.Assert(_callOrder.Count == 1);
            Debug.Assert(_callOrder[0] == webAppHostRef + "Start");
        }

        public void Run()
        {
            try
            {
                _testLaunchingServer();
                _testLaunchingServerUrl();
                _testLaunchingServerWatchdog();
                _testLaunchingServerChecksCurrentlyRunning();
                _testLaunchingServerLoadsModelAtPath();
                _testLaunchingServerLoadsRuneSolvingAgent();
                _testLaunchingServerFailsOnInvalidWatchdog();
                _testLaunchingServerFailsOnInvalidPort();
                _testLaunchingServerFailsIfAlreadyRunning();
                _testLaunchingServerFailsIfModelLoadFails();
                _testLaunchingServerFailsIfWebAppHostFails();
            }
            finally
            {
                RuneController.RuneSolvingAgent = null;
            }
        }
    }


    public class RuneSolvingServerTestSuite
    {
        public void Run()
        {
            new RuneControllerTests().Run();
            new RuneSolvingServerTests().Run();
        }
    }
}
