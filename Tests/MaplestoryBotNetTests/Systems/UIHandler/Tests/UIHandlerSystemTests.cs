using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.UIHandler.Tests
{
    /**
     * @class UIHandlerSystemTests
     * 
     * @brief Unit tests for UI handler system dependency injection
     * 
     * This test class validates that the UI handler system properly receives and forwards
     * dependency injections to all registered window action handlers, ensuring that
     * configuration data and services are correctly distributed throughout the UI layer.
     */
    public class UIHandlerSystemTests
    {
        private List<MockWindowActionHandler> _mockHandlers = [];

        /**
         * @brief Creates test fixture with specified number of mock handlers
         * 
         * @param handlerCount Number of mock handlers to create and register
         * 
         * @return Configured UI handler system ready for testing
         * 
         * Sets up a clean UI handler system with the specified number of mock handlers
         * to simulate various UI component scenarios and test injection distribution.
         */
        private UIHandlerSystem _fixture(int handlerCount)
        {
            _mockHandlers.Clear();
            for (int i = 0; i < handlerCount; i++)
            {
                _mockHandlers.Add(new MockWindowActionHandler());
            }
            return (UIHandlerSystem)new UIHandlerSystemBuilder().Build();
        }

        /**
         * @brief Tests dependency injection propagation through UI handler system
         * 
         * @test Validates injected dependencies reach all registered UI handlers
         * 
         * Verifies that when dependencies are injected into the UI handler system,
         * they are properly forwarded to all registered window action handlers.
         * Tests various handler counts to ensure consistent behavior regardless
         * of the number of UI components in the system.
         */
        private void _testInjectingHandlers()
        {
            for (int i = 1; i < 10; i++)
            {
                var uHandlerSystem = _fixture(i);
                for (int j = 0; j < i; j++)
                {
                    uHandlerSystem.Inject(SystemInjectType.ActionHandler, _mockHandlers[j]);
                }
                uHandlerSystem.Inject((SystemInjectType)999, 1234);
                for (int j = 0; j < i; j++)
                {
                    Debug.Assert(_mockHandlers[j].InjectCalls == 1);
                    Debug.Assert((int)(_mockHandlers[j].InjectCallArg_dataType[0]!) == 999);
                    Debug.Assert((int)(_mockHandlers[j].InjectCallArg_data[0]!) ==1234);
                }
            }
        }

        public void Run()
        {
            _testInjectingHandlers();
        }
    }


    public class UIHandlerSystemTestSuite
    {

        public void Run()
        {
            new UIHandlerSystemTests().Run();
        }
    }
}
