using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace MaplestoryRuneSolver.RuneSolving
{
    public abstract class AbstractConfiguration
    {
        public abstract void Configuration(IApplicationBuilder appBuilder);
    }


    public abstract class AbstractRuneSolvingServer
    {
        public abstract void Launch();
    }


    public abstract class AbstractWebAppHost
    {
        public abstract bool Start(string url);

        public abstract void Stop();

        public abstract WebApplication? WebApp();
    }


    public class WebAppHost : AbstractWebAppHost
    {
        private WebApplication? _app;

        public override bool Start(string url)  
        {
            try
            {
                var builder = WebApplication.CreateBuilder();
                builder.WebHost.UseUrls(url);
                builder.Logging.AddFilter(
                    "Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Information
                );
                builder.Logging.SetMinimumLevel(LogLevel.None);
                builder.Services.AddControllers();
                _app = builder.Build();
                _app.MapControllers();
                _app.RunAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Stop()
        {
            _app?.StopAsync().GetAwaiter().GetResult();
            _app?.DisposeAsync().GetAwaiter().GetResult();
        }

        public override WebApplication? WebApp()
        {
            return _app;
        }
    }


    [ApiController]
    [Route("api/rune")]
    public class RuneController : ControllerBase
    {
        public static AbstractRuneSolvingAgent? RuneSolvingAgent { get; set; }

        public static AbstractTimestamp? PingStopwatch { set; get; }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            PingStopwatch?.SetTimestamp();
            return Ok("Running");
        }

        [HttpPost("solve")]
        public IActionResult Solve([FromBody] string imageBase64)
        {
            var solution = RuneSolvingAgent != null ? 
                RuneSolvingAgent.Solve(imageBase64) : "";
            return solution == "" ? StatusCode(500) : Ok(solution);
        }
    }


    public class RuneSolvingServer : AbstractRuneSolvingServer
    {
        private AbstractProcessName _currProcessName;

        private AbstractProcessMonitor _processMonitor;

        private AbstractWebAppHost _webAppHost;

        private AbstractWatchdog _watchdog;

        private AbstractRuneSolvingAgent _runeSolvingAgent;

        private string _runeModelPath;

        private string _watchProcessName;

        private string _watchdogTimeout;

        private string _serverIpAddress;

        private string _serverPort;

        public RuneSolvingServer(
            AbstractProcessName currProcessName,
            AbstractProcessMonitor processMonitor,
            AbstractWebAppHost webAppHost,
            AbstractWatchdog watchdog,
            AbstractRuneSolvingAgent runeSolvingAgent,
            string runeModelPath,
            string watchProcessName,
            string watchdogTimeout,
            string serverIpAddress,
            string serverPort
         )
        {
            _currProcessName = currProcessName;
            _processMonitor = processMonitor;
            _webAppHost = webAppHost;
            _watchdog = watchdog;
            _runeSolvingAgent = runeSolvingAgent;
            _runeModelPath = runeModelPath;
            _watchProcessName = watchProcessName;
            _watchdogTimeout = watchdogTimeout;
            _serverIpAddress = serverIpAddress;
            _serverPort = serverPort;
        }

        public override void Launch()
        {
            if (
                int.TryParse(_watchdogTimeout, out int watchdogTimeout)
                && int.TryParse(_serverPort, out int serverPort)
                && watchdogTimeout >= 0
                && serverPort >= 0
                && serverPort <= 65535
                && _currProcessName.GetProcessName() is string currProcessName
                && _processMonitor.Running(currProcessName) <= 1
                && _runeSolvingAgent.LoadModel(_runeModelPath)
            )
            {
                RuneController.RuneSolvingAgent = _runeSolvingAgent;
                if (_webAppHost.Start(_serverIpAddress + ":" + _serverPort + "/"))
                {
                    _watchdog.Start(_watchProcessName, watchdogTimeout);
                    _webAppHost.Stop();
                }
                else
                {
                    RuneController.RuneSolvingAgent = null;
                }
            }
        }
    }


    public class RuneSolvingServerFacade : AbstractRuneSolvingServer
    {
        public AbstractRuneSolvingServer _runeSolvingServer;

        public RuneSolvingServerFacade(
            string runeModelPath,
            string watchProcessName,
            string watchdogTimeout,
            string serverIpAddress,
            string serverPort
        )
        {
            RuneController.PingStopwatch = new StopwatchTimestamp();
            RuneController.PingStopwatch.SetTimestamp();
            _runeSolvingServer = new RuneSolvingServer(
                new ProcessName(),
                new ProcessMonitor(),
                new WebAppHost(),
                new RuneSolvingWatchdogFacade(RuneController.PingStopwatch),
                new RuneSolvingAgentFacade(),
                runeModelPath,
                watchProcessName,
                watchdogTimeout,
                serverIpAddress,
                serverPort
            );
        }

        public override void Launch()
        {
            _runeSolvingServer.Launch();
        }
    }
}
