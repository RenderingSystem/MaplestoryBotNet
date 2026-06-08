using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.GPUSelector;
using MaplestoryBotNet.Systems.Device.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNetTests.Systems.Tests;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class WindowSplashScreenCompleterTests
    {
        private MockSystemWindow _splashScreen = new MockSystemWindow();

        private MockSystemWindow _mainWindow = new MockSystemWindow();

        private MockDispatcher _dispatcher = new MockDispatcher();

        private WindowSplashScreenCompleterParameters _parameters = new WindowSplashScreenCompleterParameters();

        public AbstractWindowActionHandler _fixture()
        {
            _splashScreen = new MockSystemWindow();
            _mainWindow = new MockSystemWindow();
            _dispatcher = new MockDispatcher();
            _parameters = new WindowSplashScreenCompleterParameters();
            return new WindowSplashScreenCompleteActionHandler(
                new WindowSplashScreenCompleter(_splashScreen, _mainWindow, _dispatcher),
                _parameters
            );
        }

        private IEnumerable<List<int>> _permutations(List<int> list, int length)
        {
            if (length == 1)
            {
                return list.Select(t => new List<int> { t });
            }
            return _permutations(list, length - 1)
                .SelectMany(
                    t => list.Where(
                        e => !t.Contains(e)
                    ),
                    (t1, t2) =>
                    {
                        return t1.Concat(new List<int> { t2 }).ToList();
                    }
                );
        }

        /**
         * @brief Tests that the completion event dispatches exactly once when all dependencies
         * are injected
         * 
         * The splash screen completer requires four dependencies to be fully initialized:
         * keyboard device, mouse device, GPU selection, and the final inject action trigger.
         * The completer should only dispatch the completion event after ALL dependencies have
         * been received, regardless of the order they are injected.
         */
        public void _testInjectionDispatchesModificationEvent()
        {
            var injections = new List<(object dataType, object data)>
            {
                (InputDeviceTypes.Keyboard, new DeviceContext(123, 234)),
                (InputDeviceTypes.Mouse, new DeviceContext(234, 345)),
                (0, new GPUSelection()),
                (SystemInjectType.InjectAction, new InjectAction((_, __) => { }))
            };
            var indices = Enumerable.Range(0, injections.Count).ToList();
            var permutations = _permutations(indices, indices.Count);
            foreach (var order in permutations)
            {
                var completer = _fixture();
                _dispatcher.DispatchCalls = 0;
                for (var i = 0; i < order.Count; i++)
                {
                    var (dataType, data) = injections[order[i]];
                    completer.Inject(dataType, data);
                    Debug.Assert(_dispatcher.DispatchCalls == (i == order.Count - 1 ? 1 : 0));
                }
            }
        }

        /**
         * @brief Tests that when completion is triggered, the splash screen closes and main window
         * opens
         * 
         * After all dependencies have been successfully injected, the bot must transition from
         * the initialization phase to the main application. This involves closing the splash
         * screen window and showing the main bot window where users can configure settings
         * and start automation.
         */
        public void _testInjectionHidesSplashScreenAndShowsMainWindow()
        {
            var completer = _fixture();
            completer.Inject(InputDeviceTypes.Keyboard, new DeviceContext(123, 234));
            completer.Inject(InputDeviceTypes.Mouse, new DeviceContext(234, 345));
            completer.Inject(0, new GPUSelection());
            completer.Inject(SystemInjectType.InjectAction, new InjectAction((_, __) => { }));
            Debug.Assert(_splashScreen.CloseCalls == 0);
            Debug.Assert(_mainWindow.ShowCalls == 0);
            _dispatcher.DispatchCallArg_action[0]();
            Debug.Assert(_splashScreen.CloseCalls == 1);
            Debug.Assert(_mainWindow.ShowCalls == 1);
        }

        /**
         * @brief Tests that upon completion, input devices are properly injected into the main system
         * 
         * When the splash screen completer finishes initialization, it must forward the
         * initialized device contexts (keyboard and mouse) to the main system via the dispatcher.
         * This ensures the automation system has access to the configured input devices.
         */
        private void _testInjectCompletionInjectsDevices()
        {
            var completer = _fixture();
            var dataTypes = new List<object>();
            var data = new List<object?>();
            var keyboardDeviceContext = new DeviceContext(123, 234);
            var mouseDeviceContext = new DeviceContext(234, 345);
            var gpuSelection = new GPUSelection();
            var injectAction = new InjectAction((_, __) => { dataTypes.Add(_); data.Add(__); });
            completer.Inject(InputDeviceTypes.Keyboard, keyboardDeviceContext);
            completer.Inject(InputDeviceTypes.Mouse, mouseDeviceContext);
            completer.Inject(0, gpuSelection);
            completer.Inject(SystemInjectType.InjectAction, injectAction);
            _dispatcher.DispatchCallArg_action[0]();
            Debug.Assert(dataTypes.Count == 2);
            Debug.Assert(dataTypes.IndexOf(SystemInjectType.KeyboardDevice) != -1);
            Debug.Assert(dataTypes.IndexOf(SystemInjectType.MouseDevice) != -1);
            Debug.Assert(data.IndexOf(keyboardDeviceContext) == dataTypes.IndexOf(SystemInjectType.KeyboardDevice));
            Debug.Assert(data.IndexOf(mouseDeviceContext) == dataTypes.IndexOf(SystemInjectType.MouseDevice));
        }

        public void Run()
        {
            _testInjectionDispatchesModificationEvent();
            _testInjectionHidesSplashScreenAndShowsMainWindow();
            _testInjectCompletionInjectsDevices();
        }
    }


    public class WindowSplashScreenStateHandlersTestSuite
    {

        public void Run()
        {
            new WindowSplashScreenCompleterTests().Run();
        }
    }
}
