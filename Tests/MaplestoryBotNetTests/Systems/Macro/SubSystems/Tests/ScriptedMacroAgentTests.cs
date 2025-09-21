using System.Diagnostics;
using MaplestoryBotNet.Systems.Macro.SubSystems;
using MaplestoryBotNetTests.Systems.Macro.SubSystems.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Macro.SubSystems.Tests
{
    /**
     * @brief Test suite for validating scripted macro agent functionality
     * 
     * @details This test class verifies that the scripted macro agent correctly
     * manages macro execution, including action sequencing, pause state handling,
     * and command processing. These tests ensure reliable automation behavior
     * during gameplay sequences.
     */ 
    public class ScriptedMacroAgentTest
    {
        private MockMacroTranslator _macroTranslator = new MockMacroTranslator();

        private List<AbstractMacroAction> _macroActions = [];

        private List<string> _callOrder = [];

        /**
         * @brief Creates a standardized test environment for scripted macro agent testing
         * 
         * @returns Configured ScriptedMacroAgent instance ready for testing
         * 
         * Prepares a consistent testing environment with predefined macro actions
         * and a mock translator to ensure reliable and reproducible test conditions.
         */
        private ScriptedMacroAgent _fixture()
        {
            _callOrder = [];
            _macroActions = [
                new MockMacroAction(),
                new MockMacroAction(),
                new MockMacroAction()
            ];
            for (int i = 0; i < _macroActions.Count; i++)
                ((MockMacroAction)_macroActions[i]).CallOrder = _callOrder;
            _macroTranslator = new MockMacroTranslator();
            _macroTranslator.TranslateReturn.Add(_macroActions);
            return new ScriptedMacroAgent(_macroTranslator);
        }

        /**
         * @brief Verifies macro command processing functionality
         * 
         * Tests that the agent correctly processes text commands by translating
         * them into executable actions, ensuring proper command interpretation
         * and action generation for automated sequences.
         */
        public void _testUpdatingWithTextUpdatesMacroActions()
        {
            var scriptedMacroAgent = _fixture();
            scriptedMacroAgent.Update("some string");
            Debug.Assert(_macroTranslator.TranslateCalls == 1);
            Debug.Assert(_macroTranslator.TranslateCallArg_macroText[0] == "some string");
            Debug.Assert(scriptedMacroAgent.MacroActions.Count == _macroActions.Count);
            for (int i = 0; i < _macroActions.Count; i++)
                Debug.Assert(scriptedMacroAgent.MacroActions[i] == _macroActions[i]);
        }

        /**
         * @brief Validates pause/resume functionality
         * 
         * Ensures that the agent correctly responds to pause and resume commands,
         * maintaining proper state management for interrupting and continuing
         * automated action sequences during gameplay.
         */
        private void _testUpdatingWithBoolUpdatesPauseState()
        {
            var scriptedMacroAgent = _fixture();
            Debug.Assert(scriptedMacroAgent.PauseRunning == false);
            scriptedMacroAgent.Update(true);
            Debug.Assert(scriptedMacroAgent.PauseRunning == true);
            scriptedMacroAgent.Update(false);
            Debug.Assert(scriptedMacroAgent.PauseRunning == false);
        }

        /**
         * @brief Verifies ordered execution of action sequences
         * 
         * Tests that the agent executes macro actions in the correct sequential
         * order, ensuring reliable and predictable automation behavior for
         * complex gameplay sequences that require precise timing.
         */
        private void _testExecutingRunsEachActionInTranslatedMacroActionsInOrder()
        {
            var scriptedMacroAgent = _fixture();
            scriptedMacroAgent.Update("some string");
            scriptedMacroAgent.Execute();
            for (int i = 0; i < _callOrder.Count; i++)
            {
                Debug.Assert(
                    _callOrder[i] == new TestUtilities().Reference(_macroActions[i]) + "Execute"
                );
            }
        }

        /**
         * @brief Executes all scripted macro agent functionality tests
         * 
         * Runs the complete test suite to ensure the macro agent correctly handles
         * command processing, execution sequencing, and state management for
         * reliable automation during gameplay.
         */
        public void Run()
        {
            _testUpdatingWithTextUpdatesMacroActions();
            _testUpdatingWithBoolUpdatesPauseState();
            _testExecutingRunsEachActionInTranslatedMacroActionsInOrder();
        }
    }


    public class ScriptedMacroAgentTestSuite
    {
        public void Run()
        {
            new ScriptedMacroAgentTest().Run();
        }
    }
}
