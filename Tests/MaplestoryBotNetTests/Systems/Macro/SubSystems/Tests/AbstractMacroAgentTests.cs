using System.Diagnostics;
using MaplestoryBotNet.Systems.Macro.SubSystems;
using MaplestoryBotNetTests.Systems.Macro.SubSystems.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;


namespace MaplestoryBotNetTests.Systems.Macro.SubSystems.Tests
{
    /**
     * @brief Test suite for validating macro agent thread execution control
     * 
     * @details This test class verifies that the macro agent thread correctly
     * responds to execution control signals, properly managing when to execute
     * macro sequences based on the running state. It ensures the thread respects
     * execution gates and maintains proper execution cycles during operation.
     */
    public class MacroAgentThreadTest
    {
        private MockMacroAgent _macroAgent = new MockMacroAgent();

        private MockRunningState _runningState = new MockRunningState();

        private MockExecutionFlag _executionFlag = new MockExecutionFlag();

        private List<string> _callOrder = [];

        /**
         * @brief Creates a standardized test environment for thread execution testing
         * 
         * @returns Configured MacroAgentThread instance ready for testing
         * 
         * Prepares a consistent testing environment with mock components to
         * verify thread execution control without actual threading operations.
         */
        private MacroAgentThread _fixture()
        {
            _callOrder = [];
            _macroAgent = new MockMacroAgent();
            _runningState = new MockRunningState();
            _executionFlag = new MockExecutionFlag();
            _macroAgent.CallOrder = _callOrder;
            _executionFlag.CallOrder = _callOrder;
            return new MacroAgentThread(_macroAgent, _executionFlag, _runningState);
        }

        /**
         * @brief Configures the execution cycle count for testing
         * 
         * @param count Number of execution cycles to simulate
         * 
         * Sets the running state to allow a specific number of execution
         * cycles before stopping, enabling controlled testing of execution
         * behavior under various operational durations.
         */
        private void _setLoopCount(int count)
        {
            _runningState.IsRunningReturn.Add(false);
            for (int i = 0; i < count; i++)
                _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
        }

        /**
         * @brief Verifies proper execution cycling through gate control
         * 
         * Tests that the macro agent thread correctly executes macro sequences
         * when the execution gate allows it, and properly respects the gate
         * control mechanism.
         */
        private void _testMacroAgentThreadCallsExecuteWhileRunning()
        {
            for (int i = 0; i < 10; i++)
            {
                var macroAgentThread = _fixture();
                var waitRef = new TestUtilities().Reference(_executionFlag) + "Wait";
                var executeRef = new TestUtilities().Reference(_macroAgent) + "Execute";
                _setLoopCount(i);
                macroAgentThread.Start();
                macroAgentThread.Join(10000);
                Debug.Assert(_callOrder.Count == i * 2);
                for (int j = 0; j < _callOrder.Count; j+=2)
                {
                    Debug.Assert(_callOrder[j] == waitRef);
                    Debug.Assert(_callOrder[j + 1] == executeRef);
                }
            }
        }

        /**
         * @brief Executes all macro agent thread control tests
         * 
         * Runs the complete test suite to ensure the macro agent thread
         * correctly responds to execution gates and state changes, providing
         * reliable control over when macro sequences are executed.
         */
        public void Run()
        {
            _testMacroAgentThreadCallsExecuteWhileRunning();
        }
    }


    public class AbstractMacroAgentTestSuite
    {
        public void Run()
        {
            new MacroAgentThreadTest().Run();
        }
    }

}
