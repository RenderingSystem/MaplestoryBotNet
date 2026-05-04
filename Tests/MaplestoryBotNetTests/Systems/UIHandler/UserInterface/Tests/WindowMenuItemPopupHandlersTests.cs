using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters;
using MaplestoryBotNet.Systems.Macro;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.ThreadingUtils;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{

    /**
     * @brief Tests for popup window behavior when menu items are clicked
     * 
     * Verifies that when users click menu items, the appropriate popup windows
     * appear on screen as expected.
     */
    public class WindowMenuItemPopupHandlerTests
    {
        MockSystemWindow _systemWindow = new MockSystemWindow();

        MenuItem _popupMenuItem = new MenuItem();

        /**
         * @brief Sets up the test environment with menu items and windows
         * 
         * @return Ready-to-test popup handler
         */
        public WindowMenuItemPopupHandler _fixture()
        {
            _systemWindow = new MockSystemWindow();
            _popupMenuItem = new MenuItem();
            return (WindowMenuItemPopupHandler) new WindowMenuItemPopupHandlerBuilder()
                .WithArgs(_systemWindow)
                .WithArgs(_popupMenuItem)
                .Build();
        }

        /**
         * @brief Verifies clicking menu items shows popup windows
         * 
         * @test Ensures users see popup windows when they click menu items
         * 
         * When users click on a menu item that should open a popup window,
         * this test confirms the window actually appears on screen.
         */
        public void _testModifyShowsPopupWindow()
        {
            var handler = _fixture();
            _popupMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Debug.Assert(_systemWindow.ShowCalls == 1);
        }

        /**
         * @brief Runs all popup window visibility tests
         */
        public void Run()
        {
            _testModifyShowsPopupWindow();
        }
    }


    /**
     * @brief Tests for window closing and hiding behavior
     * 
     * Ensures windows close properly when users click the close button
     * and behave correctly during application shutdown.
     */
    public class WindowMenuItemHideHandlerTests
    {

        Window _systemWindow = new Window();

        MockSystemWindow _mockSystemWindow = new MockSystemWindow();


        /**
         * @brief Sets up test environment with windows and close handlers
         * 
         * @return Ready-to-test hide handler
         */
        public WindowMenuItemHideHandler _fixture()
        {
            _systemWindow = new Window();
            _mockSystemWindow = new MockSystemWindow();
            _mockSystemWindow.GetWindowReturn.Add(_systemWindow);
            return (WindowMenuItemHideHandler) new WindowMenuItemHideHandlerBuilder()
                .WithArgs(_mockSystemWindow)
                .Build();
        }

        /**
         * @brief Verifies windows hide instead of close during normal use
         * 
         * @test Ensures windows minimize properly when users click close
         * 
         * When users click the close button on a window during normal application use,
         * this test confirms the window hides from view rather than closing completely,
         * allowing it to be restored later.
         */
        public void _testCloseEventCancelsAndHidesWindow()
        {
            var handler = _fixture();
            _mockSystemWindow.ShutdownFlag = false;
            _systemWindow.Close();
            Debug.Assert(_mockSystemWindow.HideCalls == 1);
        }

        /**
         * @brief Verifies windows close completely during application shutdown
         * 
         * @test Ensures all windows close properly when application exits
         * 
         * When the user exits the application, this test confirms that windows
         * close completely rather than just hiding, ensuring proper application shutdown.
         */
        public void _testCloseEventClosesWindow()
        {
            var handler = _fixture();
            _mockSystemWindow.ShutdownFlag = true;
            _systemWindow.Close();
            Debug.Assert(_mockSystemWindow.HideCalls == 0);
        }

        /**
         * @brief Runs all window closing behavior tests
         */
        public void Run()
        {
            _testCloseEventCancelsAndHidesWindow();
            _testCloseEventClosesWindow();
        }
    }


    public class WindowMenuItemStartTextActionHandlerTests
    {
        private MenuItem _menuItem = new MenuItem();

        private MockDispatcher _dispatcher = new MockDispatcher();

        private AbstractWindowActionHandler _fixture()
        {
            _menuItem = new MenuItem();
            _dispatcher = new MockDispatcher();
            return new WindowMenuItemStartTextActionHandlerFacade(_menuItem, _dispatcher);
        }

        /**
         * @brief Verifies the menu item text changes correctly for each automation state
         * 
         * When users interact with the automation system, the start/stop menu item
         * should update to clearly indicate what action will happen if clicked.
         * 
         * State transitions:
         * - When automation is Stopped -> Shows "Start!" (click to start)
         * - When starting up -> Shows "Starting..." (feedback that start is in progress)
         * - When running -> Shows "Stop!" (click to stop)
         * - When stopping -> Shows "Stopping..." (feedback that stop is in progress)
         */
        private void _testInjectingExecutorThreadedUpdateSetsMenuItemText()
        {
            var injectItems = new[]
            {
                 MacroExecutorThreadedUpdate.Starting,
                 MacroExecutorThreadedUpdate.Started,
                 MacroExecutorThreadedUpdate.Stopping,
                 MacroExecutorThreadedUpdate.Stopped
            };
            var expectedText = new[]
            {
                "Starting...",
                "Stop!",
                "Stopping...",
                "Start!"
            };
            for (int i = 0; i < injectItems.Length; i++)
            {
                var startTextActionHandler = _fixture();
                startTextActionHandler.Inject(injectItems[i], null);
                Debug.Assert(_dispatcher.DispatchCalls == 1);
                Debug.Assert(_menuItem.Header == null);
                _dispatcher.DispatchCallArg_action[0]();
                Debug.Assert((string) _menuItem.Header! == expectedText[i]);
            }
        }

        public void Run()
        {
            _testInjectingExecutorThreadedUpdateSetsMenuItemText();
        }
    }


    public class WindowMeuItemStartActionHandlerTests
    {
        private MenuItem _menuItem = new MenuItem();

        private MockThread _mockThread = new MockThread(new ThreadRunningState());

        private AbstractWindowActionHandler _fixture()
        {
            _menuItem = new MenuItem();
            _mockThread = new MockThread(new ThreadRunningState());
            return new WindowMenuItemStartActionHandlerFacade(_menuItem);
        }

        /**
         * @brief Verifies that clicking "Start!" sends a start command to the orchestrator
         * 
         * When users click the button while it shows "Start!", the system should
         * tell the orchestrator to begin botting. This test ensures the start
         * command is properly sent to the thread that controls automation.
         */
        private void _testClickingMenuItemInjectsStartThread()
        {
            var startActionHandler = _fixture();
            _mockThread.ThreadStateReturn.Add(
                new KeystrokeTransmitterThreadState(
                    0, KeystrokeTransmitterThreadType.Macro
                )
            );
            startActionHandler.Inject(SystemInjectType.ThreadDependency, _mockThread);
            _menuItem.Header = WindowMenuItemStartTextTypes.Start;
            _menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Debug.Assert(_mockThread.InjectCalls == 1);
            Debug.Assert(
                (int) _mockThread.InjectCallArg_dataType[0]
                == (int) MacroOrchestratorThreadInjectType.Start
            );
        }

        /**
         * @brief Ensures start command is only sent to the orchestrator thread, not other threads
         * 
         * The system may have multiple threads that can be injected as dependencies.
         * Only the orchestrator thread should respond to start/stop commands. This test
         * verifies that if a thread dependency is injected that is not a valid orchestrator
         * thread (returns null state), clicking the button does nothing rather than
         * accidentally sending commands to the wrong thread.
         */
        private void _testClickingMenuItemDoesntInjectStartWhenWrongThread()
        {
            var startActionHandler = _fixture();
            _mockThread.ThreadStateReturn.Add(null);
            startActionHandler.Inject(SystemInjectType.ThreadDependency, _mockThread);
            _menuItem.Header = WindowMenuItemStartTextTypes.Start;
            _menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Debug.Assert(_mockThread.InjectCalls == 0);
        }

        /**
         * @brief Verifies that clicking "Stop!" sends a stop command to the orchestrator
         * 
         * When users click the button while it shows "Stop!", the system should
         * tell the orchestrator to stop botting. This test ensures the stop
         * command is properly sent to the thread controlling automation.
         */
        private void _testClickingMenuItemInjectsStopThread()
        {
            var startActionHandler = _fixture();
            _mockThread.ThreadStateReturn.Add(
                new KeystrokeTransmitterThreadState(
                    0, KeystrokeTransmitterThreadType.Macro
                )
            );
            startActionHandler.Inject(SystemInjectType.ThreadDependency, _mockThread);
            _menuItem.Header = WindowMenuItemStartTextTypes.Stop;
            _menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Debug.Assert(_mockThread.InjectCalls == 1);
            Debug.Assert(
                (int)_mockThread.InjectCallArg_dataType[0]
                == (int)MacroOrchestratorThreadInjectType.Stop
            );
        }

        /**
         * @brief Ensures stop command is only sent to the orchestrator thread, not other threads
         * 
         * The system may have multiple threads that can be injected as dependencies.
         * Only the orchestrator thread should respond to start/stop commands. This test
         * verifies that if a thread dependency is injected that is not a valid orchestrator
         * thread (returns null state), clicking the button does nothing rather than
         * accidentally sending commands to the wrong thread.
         */
        private void _testClickingMenuItemDoesntInjectStopWhenWrongThread()
        {
            var startActionHandler = _fixture();
            _mockThread.ThreadStateReturn.Add(null);
            startActionHandler.Inject(SystemInjectType.ThreadDependency, _mockThread);
            _menuItem.Header = WindowMenuItemStartTextTypes.Stop;
            _menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Debug.Assert(_mockThread.InjectCalls == 0);
        }

        /**
         * @brief Ensures clicks only work when the button shows proper start/stop text
         * 
         * The UI should only respond to clicks when the button shows valid
         * start or stop text. This test ensures that if the button
         * displays unexpected text, clicking it won't send any commands.
         */
        private void _testClickingMenuItemDoesntInjectWhenWrongText()
        {
            var startActionHandler = _fixture();
            _mockThread.ThreadStateReturn.Add(
                new KeystrokeTransmitterThreadState(
                    0, KeystrokeTransmitterThreadType.Macro
                )
            );
            startActionHandler.Inject(SystemInjectType.ThreadDependency, _mockThread);
            _menuItem.Header = "meow";
            _menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Debug.Assert(_mockThread.InjectCalls == 0);
        }

        public void Run()
        {
            _testClickingMenuItemInjectsStartThread();
            _testClickingMenuItemDoesntInjectStartWhenWrongThread();
            _testClickingMenuItemInjectsStopThread();
            _testClickingMenuItemDoesntInjectStopWhenWrongThread();
            _testClickingMenuItemDoesntInjectWhenWrongText();
        }
    }


    public class WindowBottingTextStatusActionHandlerTests
    {
        private List<TextBlock> _allTexts = [];

        private MockDispatcher _dispatcher = new MockDispatcher();

        private AbstractWindowActionHandler _fixture()
        {
            _allTexts = [];
            for (int i = 0; i < (int)MacroExecutorStateTypes.MaxNum; i++)
            {
                _allTexts.Add(new TextBlock());
                _allTexts[i].Visibility = Visibility.Visible;
            }
            _dispatcher = new MockDispatcher();
            return new WindowBottingTextStatusActionHandlerFacade(
                _allTexts, _dispatcher
            );
        }

        /**
         * @brief Verifies that when a macro executor state update is injected, the UI
         * text blocks update their visibility to show only the active state
         * 
         * When the botting system changes operational states (e.g., from Botting to
         * Runeing or from Solving to Idle), the UI must reflect the current state so
         * users can see what the bot is doing at a glance. This test ensures that
         * injecting a new state (represented as an integer) triggers a UI update where
         * only the text block corresponding to the active state remains visible, while
         * all other state text blocks are hidden.
         */
        private void _testInjectingSetsVisibilityStatus()
        {
            for (int i = 0; i < (int)MacroExecutorStateTypes.MaxNum; i++)
            {
                var textStatusActionHandler = _fixture();
                textStatusActionHandler.Inject((MacroExecutorStateTypes)i, 0);
                for (int j = 0; j < (int)MacroExecutorStateTypes.MaxNum; j++)
                {
                    Debug.Assert(_allTexts[j].Visibility == Visibility.Visible);
                }
                Debug.Assert(_dispatcher.DispatchCalls == 1);
                _dispatcher.DispatchCallArg_action[0]();
                for (int j = 0; j < (int)MacroExecutorStateTypes.MaxNum; j++)
                {
                    Debug.Assert(
                        (j != i) ?
                        _allTexts[j].Visibility == Visibility.Hidden :
                        _allTexts[j].Visibility == Visibility.Visible
                    );
                }
            }
        }

        public void Run()
        {
            _testInjectingSetsVisibilityStatus();
        }
    }


    public class WindowMenuItemPopupHandlersTestSuite
    {
        public void Run()
        {
            new WindowMenuItemPopupHandlerTests().Run();
            new WindowMenuItemHideHandlerTests().Run();
            new WindowMenuItemStartTextActionHandlerTests().Run();
            new WindowMeuItemStartActionHandlerTests().Run();
            new WindowBottingTextStatusActionHandlerTests().Run();
        }
    }
}
