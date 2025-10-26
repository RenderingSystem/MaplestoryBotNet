using System;
using System.Diagnostics;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Macro.SubSystems;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Macro.SubSystems.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using static System.Collections.Specialized.BitVector32;


namespace MaplestoryBotNetTests.Systems.Macro.SubSystems.Tests
{

    /**
     * @class DelayMacroTranslatorTests
     * 
     * @brief Unit tests for verifying random delay macro command processing functionality
     * 
     * This test class validates that random delay macro commands are correctly interpreted and executed,
     * ensuring proper randomized timing control during automated gameplay sequences and reliable
     * command processing across various input formats with varying delay ranges.
     */
    public class DelayMacroTranslatorTests
    {
        MockMacroSleeper _macroSleeper = new MockMacroSleeper();

        MockMacroRandom _macroRandom = new MockMacroRandom();

        /**
         * @brief Creates a test environment for random delay macro testing
         * 
         * @return Configured DelayMacroTranslator instance
         * 
         * Prepares a test environment with mock timing and randomization components to verify
         * random delay macro processing without requiring actual time delays or true random
         * number generation during testing.
         */
        private DelayMacroTranslator _fixture()
        {
            _macroSleeper = new MockMacroSleeper();
            _macroRandom = new MockMacroRandom();
            _macroRandom.NextReturn.Add(2345);
            return new DelayMacroTranslator(_macroSleeper, _macroRandom);
        }

        /**
         * @brief Tests proper handling of invalid random delay commands
         * 
         * Validates that the random delay macro system correctly ignores malformed or
         * unrecognized commands, preventing execution errors during automation
         * and ensuring only valid random delay instructions are processed.
         */
        private void _testTranslateReturnsEmptyOnInvalidInputs()
        {
            var delayMacroTranslator = _fixture();
            List<string> invalidInput = ["", "meow", "delay*", "12 34"];
            for (int i = 0; i < invalidInput.Count; i++)
            {
                var input = invalidInput[i];
                Debug.Assert(delayMacroTranslator.Translate(input).Count == 0);
            }
        }

        /**
         * @brief Tests proper randomization of delay timing
         * 
         * Validates that the random delay macro system correctly generates random timing
         * values within the specified range, ensuring varied timing patterns during
         * automated gameplay sequences for more natural behavior.
         */
        private void _testTranslateRandomizesDelayTime()
        {
            var delayMacroTranslator = _fixture();
            var result = delayMacroTranslator.Translate("delay*1234 5678");
            Debug.Assert(_macroRandom.NextCalls == 1);
            Debug.Assert(_macroRandom.NextCallArg_minValue[0] == 1234);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[0] == 5678);
            Debug.Assert(result.Count == 1);
            Debug.Assert(((DelayMacroAction)result[0]).Delay == 2345);
        }

        /**
         * @brief Tests accurate interpretation of valid random delay commands
         * 
         * Validates that the random delay macro system correctly interprets properly
         * formatted random delay commands with various formatting styles, ensuring
         * consistent timing randomization regardless of command casing or spacing.
         */
        private void _testTranslateReturnsCorrectDelayOnValidInput()
        {
            List<string> validInput = [
                "delay*1234 5678",
                "delay*  1234   2345 3456  ",
                "DeLaY*1234 5678"
            ];
            for (int i = 0; i < validInput.Count; i++)
            {
                var delayMacroTranslator = _fixture();
                var input = validInput[i];
                var result = delayMacroTranslator.Translate(input);
                var action = (DelayMacroAction)result[0];
                Debug.Assert(result.Count == 1);
                Debug.Assert(result[0] is DelayMacroAction);
                Debug.Assert(action.Delay == 2345);
            }
        }

        /**
         * @brief Tests precise timing execution of delay commands
         * 
         * Validates that delay commands are executed with exact timing precision,
         * ensuring automated sequences maintain proper pacing and timing consistency
         * during gameplay automation.
         */
        private void _testExecuteSleepsForCorrectAmountOfMilliseconds()
        {
            var delayMacroTranslator = _fixture();
            var result = delayMacroTranslator.Translate("delay*1500");
            var action = (DelayMacroAction)result[0];
            action.Execute();
            Debug.Assert(_macroSleeper.SleepCalls == 1);
            Debug.Assert(_macroSleeper.SleepCallArg_milliseconds[0] == 1500);
        }

        /**
         * @brief Tests precise timing execution of randomized delay commands
         * 
         * Validates that randomized delay commands are executed with the correct randomly
         * generated timing values, ensuring automated sequences maintain proper varied
         * pacing during gameplay automation.
         */
        private void _testExecuteSleepsForCorrectAmountOfRandomMilliseconds()
        {
            var delayMacroTranslator = _fixture();
            var result = delayMacroTranslator.Translate("delay*1234 5678");
            result[0].Execute();
            Debug.Assert(_macroSleeper.SleepCalls == 1);
            Debug.Assert(_macroSleeper.SleepCallArg_milliseconds[0] == 2345);
        }

        /**
         * @brief Executes all random delay macro functionality tests
         * 
         * Runs the complete test suite to ensure random delay macro commands are correctly
         * processed and executed, providing confidence in the reliability of randomized
         * timing control during automated gameplay sequences.
         */
        public void Run()
        {
            _testTranslateReturnsEmptyOnInvalidInputs();
            _testTranslateRandomizesDelayTime();
            _testTranslateReturnsCorrectDelayOnValidInput();
            _testExecuteSleepsForCorrectAmountOfMilliseconds();
            _testExecuteSleepsForCorrectAmountOfRandomMilliseconds();
        }
    }

    /**
     * @class KeydownMacroTranslatorTests
     * 
     * @brief Unit tests for verifying keydown macro command processing functionality
     * 
     * This test class validates that keydown macro commands are correctly interpreted and executed,
     * ensuring proper key press simulation during automated gameplay sequences..
     */
    public class KeydownMacroTranslatorTests
    {
        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private KeyboardMapping _keyboardMapping = new KeyboardMapping();

        /**
         * @brief Creates a test environment for keydown macro testing
         * 
         * @return Configured KeydownMacroTranslator instance
         * 
         * Prepares a test environment with mock keyboard components to verify
         * keydown macro processing without requiring actual keyboard input
         * or hardware interaction during testing.
         */
        private KeydownMacroTranslator _fixture()
        {
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _keyboardMapping = new KeyboardMapping();
            _keyboardMapping.Characters["a"] = "0x12 0x34";
            return new KeydownMacroTranslator();
        }

        /**
         * @brief Tests proper handling of missing dependency injections
         * 
         * Validates that the keydown macro system correctly prevents command execution
         * when required components are not properly configured, ensuring system stability
         * during automation setup and configuration phases.
         */
        private void _testTranslateReturnsEmptyWhenNothingIsInjected()
        {
            var keydownMacroTranslator = _fixture();
            var result = keydownMacroTranslator.Translate("keydown*a");
            Debug.Assert(result.Count == 0);
        }

        /**
         * @brief Tests dependency requirement for keyboard mapping
         * 
         * Validates that the keydown macro system requires keyboard mapping configuration
         * to properly interpret key commands, preventing execution errors when keyboard
         * layout information is unavailable.
         */
        private void _testTranslateReturnsEmptyWhenKeyboardMappingIsntInjected()
        {
            var keydownMacroTranslator = _fixture();
            keydownMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keystrokeTransmitter);
            var result = keydownMacroTranslator.Translate("keydown*a");
            Debug.Assert(result.Count == 0);
        }


        /**
         * @brief Tests dependency requirement for keystroke transmission
         * 
         * Validates that the keydown macro system requires keystroke transmission capability
         * to execute key commands, preventing execution errors when input simulation
         * components are unavailable.
         */
        private void _testTranslateReturnsEmptyWhenKeystrokeTransmitterIsntInjected()
        {
            var keydownMacroTranslator = _fixture();
            keydownMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            var result = keydownMacroTranslator.Translate("keydown*a");
            Debug.Assert(result.Count == 0);
        }

        /**
         * @brief Tests successful command processing with all dependencies
         * 
         * Validates that the keydown macro system correctly processes commands when all
         * required components are properly configured, ensuring reliable key press
         * simulation during automated gameplay sequences.
         */
        private void _testTranslateReturnsCorrectActionWhenBothAreInjected()
        {
            var keydownMacroTranslator = _fixture();
            keydownMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keydownMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = keydownMacroTranslator.Translate("keydown*a");
            var action = (KeyDownMacroAction)result[0];
            Debug.Assert(result.Count == 1);
            Debug.Assert(result[0] is KeyDownMacroAction);
            Debug.Assert(action.Keystroke == "a");
        }


        /**
         * @brief Tests accurate execution of keydown commands
         * 
         * Validates that keydown commands are correctly translated into keystroke
         * transmission actions, ensuring precise key press simulation during
         * automated gameplay sequences.
         */
        private void _testTranslateActionExecutesCorrectly()
        {
            var keydownMacroTranslator = _fixture();
            keydownMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keydownMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = keydownMacroTranslator.Translate("keydown*a");
            var action = (KeyDownMacroAction)result[0];
            action.Execute();
            Debug.Assert(_keystrokeTransmitter.KeydownCalls == 1);
            Debug.Assert(_keystrokeTransmitter.KeydownCallArg_keystroke[0] == "a");
        }

        /**
         * @brief Tests proper handling of invalid keydown commands
         * 
         * Validates that the keydown macro system correctly ignores malformed or
         * unrecognized commands, preventing execution errors during automation
         * and ensuring only valid key press instructions are processed.
         */
        private void _testTranslateReturnsEmptyOnInvalidInputs()
        {
            var keydownMacroTranslator = _fixture();
            keydownMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keydownMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            List<string> invalidInput = ["", "meow", "keydown*", "12 34", "keydown*unknownkey"];
            for (int i = 0; i < invalidInput.Count; i++)
            {
                var input = invalidInput[i];
                var result = keydownMacroTranslator.Translate(input);
                Debug.Assert(result.Count == 0);
            }
        }

        /**
         * @brief Tests accurate interpretation of valid keydown commands
         * 
         * Validates that the keydown macro system correctly interprets properly
         * formatted keydown commands with various formatting styles, ensuring
         * consistent key press simulation regardless of command casing or spacing.
         */
        private void _testTranslateReturnsCorrectActionOnValidInput()
        {
            List<string> validInput = [
                "keydown*a",
                "keydown*  a   b c d",
                "KeYdOwN*a"
            ];
            for (int i = 0; i < validInput.Count; i++)
            {
                var keydownMacroTranslator = _fixture();
                keydownMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
                keydownMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
                var input = validInput[i];
                var result = keydownMacroTranslator.Translate(input);
                var action = (KeyDownMacroAction)result[0];
                Debug.Assert(result.Count == 1);
                Debug.Assert(result[0] is KeyDownMacroAction);
                Debug.Assert(action.Keystroke == "a");
            }
        }

        /**
         * @brief Executes all keydown macro functionality tests
         * 
         * Runs the complete test suite to ensure keydown macro commands are correctly
         * processed and executed, providing confidence in the reliability of key press
         * simulation during automated gameplay sequences.
         */
        public void Run()
        {
            _testTranslateReturnsEmptyWhenNothingIsInjected();
            _testTranslateReturnsEmptyWhenKeyboardMappingIsntInjected();
            _testTranslateReturnsEmptyWhenKeystrokeTransmitterIsntInjected();
            _testTranslateReturnsCorrectActionWhenBothAreInjected();
            _testTranslateActionExecutesCorrectly();
            _testTranslateReturnsEmptyOnInvalidInputs();
            _testTranslateReturnsCorrectActionOnValidInput();
        }
    }


    /**
     * @class KeyupMacroTranslatorTests
     * 
     * @brief Unit tests for verifying Keyup macro command processing functionality
     * 
     * This test class validates that Keyup macro commands are correctly interpreted and executed,
     * ensuring proper key press simulation during automated gameplay sequences..
     */
    public class KeyupMacroTranslatorTests
    {
        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private KeyboardMapping _keyboardMapping = new KeyboardMapping();

        /**
         * @brief Creates a test environment for Keyup macro testing
         * 
         * @return Configured KeyupMacroTranslator instance
         * 
         * Prepares a test environment with mock keyboard components to verify
         * Keyup macro processing without requiring actual keyboard input
         * or hardware interaction during testing.
         */
        private KeyupMacroTranslator _fixture()
        {
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _keyboardMapping = new KeyboardMapping();
            _keyboardMapping.Characters["a"] = "0x12 0x34";
            return new KeyupMacroTranslator();
        }

        /**
         * @brief Tests proper handling of missing dependency injections
         * 
         * Validates that the Keyup macro system correctly prevents command execution
         * when required components are not properly configured, ensuring system stability
         * during automation setup and configuration phases.
         */
        private void _testTranslateReturnsEmptyWhenNothingIsInjected()
        {
            var KeyupMacroTranslator = _fixture();
            var result = KeyupMacroTranslator.Translate("Keyup*a");
            Debug.Assert(result.Count == 0);
        }

        /**
         * @brief Tests dependency requirement for keyboard mapping
         * 
         * Validates that the Keyup macro system requires keyboard mapping configuration
         * to properly interpret key commands, preventing execution errors when keyboard
         * layout information is unavailable.
         */
        private void _testTranslateReturnsEmptyWhenKeyboardMappingIsntInjected()
        {
            var KeyupMacroTranslator = _fixture();
            KeyupMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = KeyupMacroTranslator.Translate("Keyup*a");
            Debug.Assert(result.Count == 0);
        }


        /**
         * @brief Tests dependency requirement for keystroke transmission
         * 
         * Validates that the Keyup macro system requires keystroke transmission capability
         * to execute key commands, preventing execution errors when input simulation
         * components are unavailable.
         */
        private void _testTranslateReturnsEmptyWhenKeystrokeTransmitterIsntInjected()
        {
            var KeyupMacroTranslator = _fixture();
            KeyupMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            var result = KeyupMacroTranslator.Translate("Keyup*a");
            Debug.Assert(result.Count == 0);
        }

        /**
         * @brief Tests successful command processing with all dependencies
         * 
         * Validates that the Keyup macro system correctly processes commands when all
         * required components are properly configured, ensuring reliable key press
         * simulation during automated gameplay sequences.
         */
        private void _testTranslateReturnsCorrectActionWhenBothAreInjected()
        {
            var KeyupMacroTranslator = _fixture();
            KeyupMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            KeyupMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = KeyupMacroTranslator.Translate("Keyup*a");
            var action = (KeyUpMacroAction)result[0];
            Debug.Assert(result.Count == 1);
            Debug.Assert(result[0] is KeyUpMacroAction);
            Debug.Assert(action.Keystroke == "a");
        }


        /**
         * @brief Tests accurate execution of Keyup commands
         * 
         * Validates that Keyup commands are correctly translated into keystroke
         * transmission actions, ensuring precise key press simulation during
         * automated gameplay sequences.
         */
        private void _testTranslateActionExecutesCorrectly()
        {
            var KeyupMacroTranslator = _fixture();
            KeyupMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            KeyupMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = KeyupMacroTranslator.Translate("Keyup*a");
            var action = (KeyUpMacroAction)result[0];
            action.Execute();
            Debug.Assert(_keystrokeTransmitter.KeyupCalls == 1);
            Debug.Assert(_keystrokeTransmitter.KeyupCallArg_keystroke[0] == "a");
        }

        /**
         * @brief Tests proper handling of invalid Keyup commands
         * 
         * Validates that the Keyup macro system correctly ignores malformed or
         * unrecognized commands, preventing execution errors during automation
         * and ensuring only valid key press instructions are processed.
         */
        private void _testTranslateReturnsEmptyOnInvalidInputs()
        {
            var KeyupMacroTranslator = _fixture();
            KeyupMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            KeyupMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            List<string> invalidInput = ["", "meow", "Keyup*", "12 34", "Keyup*unknownkey"];
            for (int i = 0; i < invalidInput.Count; i++)
            {
                var input = invalidInput[i];
                var result = KeyupMacroTranslator.Translate(input);
                Debug.Assert(result.Count == 0);
            }
        }

        /**
         * @brief Tests accurate interpretation of valid Keyup commands
         * 
         * Validates that the Keyup macro system correctly interprets properly
         * formatted Keyup commands with various formatting styles, ensuring
         * consistent key press simulation regardless of command casing or spacing.
         */
        private void _testTranslateReturnsCorrectActionOnValidInput()
        {
            List<string> validInput = [
                "Keyup*a",
                "Keyup*  a   b c d",
                "Keyup*a"
            ];
            for (int i = 0; i < validInput.Count; i++)
            {
                var KeyupMacroTranslator = _fixture();
                KeyupMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
                KeyupMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
                var input = validInput[i];
                var result = KeyupMacroTranslator.Translate(input);
                var action = (KeyUpMacroAction)result[0];
                Debug.Assert(result.Count == 1);
                Debug.Assert(result[0] is KeyUpMacroAction);
                Debug.Assert(action.Keystroke == "a");
            }
        }

        /**
         * @brief Executes all Keyup macro functionality tests
         * 
         * Runs the complete test suite to ensure Keyup macro commands are correctly
         * processed and executed, providing confidence in the reliability of key press
         * simulation during automated gameplay sequences.
         */
        public void Run()
        {
            _testTranslateReturnsEmptyWhenNothingIsInjected();
            _testTranslateReturnsEmptyWhenKeyboardMappingIsntInjected();
            _testTranslateReturnsEmptyWhenKeystrokeTransmitterIsntInjected();
            _testTranslateReturnsCorrectActionWhenBothAreInjected();
            _testTranslateActionExecutesCorrectly();
            _testTranslateReturnsEmptyOnInvalidInputs();
            _testTranslateReturnsCorrectActionOnValidInput();
        }
    }


    /**
     * @brief Test suite for validating keypress macro functionality
     * 
     * @details This test class verifies that keypress macros correctly translate
     * text commands into executable actions with proper timing and keystroke handling.
     * The tests ensure that the system behaves correctly when components are missing,
     * when using randomized timing, and when selecting from multiple keystroke options.
     */
    public class KeypressMacroTranslatorTests
    {
        private MockMacroSleeper _macroSleeper = new MockMacroSleeper();

        private MockMacroRandom _macroRandom = new MockMacroRandom();

        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private KeyboardMapping _keyboardMapping = new KeyboardMapping();

        private KeypressMacroTranslator _fixture()
        {
            _macroSleeper = new MockMacroSleeper();
            _macroRandom = new MockMacroRandom();
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _keyboardMapping = new KeyboardMapping();
            _keyboardMapping.Characters["a"] = "0x12 0x34";
            _keyboardMapping.Characters["b"] = "0x12 0x34";
            _keyboardMapping.Characters["c"] = "0x12 0x34";
            return new KeypressMacroTranslator(_macroSleeper, _macroRandom);
        }

        /**
         * @brief Verifies macro translation requires all necessary components
         * 
         * This test confirms that keypress macros cannot be translated when essential
         * system components like keyboard mapping or keystroke transmission are not
         * properly configured, ensuring system reliability.
         */
        private void _testTranslateReturnsEmptyWhenNothingIsInjected()
        {
            var keypressMacroTranslator = _fixture();
            var result = keypressMacroTranslator.Translate("keypress*1234 a");
            Debug.Assert(result.Count == 0);
        }

        /**
         * @brief Validates keyboard mapping dependency requirement
         * 
         * Ensures that keypress macros cannot execute without a valid keyboard mapping
         * configuration, preventing errors from unrecognized key codes.
         */
        private void _testTranslateReturnsEmptyWhenKeyboardMappingIsntInjected()
        {
            var keypressMacroTranslator = _fixture();
            keypressMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = keypressMacroTranslator.Translate("keypress*1234 a");
            Debug.Assert(result.Count == 0);
        }

        /**
         * @brief Validates keystroke transmitter dependency requirement
         * 
         * Confirms that keypress macros require a keystroke transmitter component
         * to properly execute, ensuring input commands can be properly simulated.
         */
        private void _testTranslateReturnsEmptyWhenKeystrokeTransmitterIsntInjected()
        {
            var keypressMacroTranslator = _fixture();
            keypressMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            var result = keypressMacroTranslator.Translate("keypress*1234 a");
            Debug.Assert(result.Count == 0);
        }

        /**
         * @brief Verifies correct translation of basic keypress commands
         * 
         * Tests that properly configured keypress macros with fixed delays are
         * correctly translated into executable actions with the right parameters.
         */
        private void _testTranslateReturnsCorrectActionWhenBothAreInjected()
        {
            var keypressMacroTranslator = _fixture();
            keypressMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keypressMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = keypressMacroTranslator.Translate("keypress*1234 a");
            var action = (KeyPressMacroAction)result[0];
            Debug.Assert(result.Count == 1);
            Debug.Assert(result[0] is KeyPressMacroAction);
            Debug.Assert(action.Keystroke == "a");
            Debug.Assert(action.Delay == 1234);
        }

        /**
         * @brief Validates randomized delay parameter handling
         * 
         * Ensures that keypress macros with randomized delay ranges correctly
         * use the specified minimum and maximum values for timing calculations.
         */
        private void _testTranslateUsesCorrectRandomizedDelayParameters()
        {
            var keypressMacroTranslator = _fixture();
            keypressMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keypressMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            _macroRandom.NextReturn.Add(2345);
            var result = keypressMacroTranslator.Translate("keypress*1234 3456 a");
            Debug.Assert(_macroRandom.NextCalls == 1);
            Debug.Assert(_macroRandom.NextCallArg_minValue[0] == 1234);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[0] == 3456);
        }

        /**
         * @brief Verifies execution of randomized delay actions
         * 
         * Tests that keypress macros with randomized delays correctly generate
         * actions with properly calculated delay values within the specified range.
         */
        private void _testTranslateReturnsCorrectRandomizedDelayAction()
        {
            var keypressMacroTranslator = _fixture();
            keypressMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keypressMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            _macroRandom.NextReturn.Add(2345);
            var result = keypressMacroTranslator.Translate("keypress*1234 3456 a");
            var action = (KeyPressMacroAction)result[0];
            Debug.Assert(result.Count == 1);
            Debug.Assert(result[0] is KeyPressMacroAction);
            Debug.Assert(action.Keystroke == "a");
            Debug.Assert(action.Delay == 2345);
        }


        /**
         * @brief Validates randomized keystroke parameter handling
         * 
         * Ensures that keypress macros with multiple keystroke options correctly
         * handle the randomization parameters for selecting between options.
         */
        private void _testTranslateUsesCorrectRandomizedKeystrokeParameters()
        {
            var keypressMacroTranslator = _fixture();
            keypressMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keypressMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            _macroRandom.NextReturn.Add(2);
            var result = keypressMacroTranslator.Translate("keypress*1234 a b c");
            Debug.Assert(_macroRandom.NextCalls == 1);
            Debug.Assert(_macroRandom.NextCallArg_minValue[0] == 1);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[0] == 4);
        }


        /**
         * @brief Verifies execution of randomized keystroke actions
         * 
         * Tests that keypress macros with multiple keystroke options correctly
         * select and execute a random option from the provided choices.
         */
        private void _testTranslateReturnsCorrectRandomizedKeystrokeAction()
        {
            var keypressMacroTranslator = _fixture();
            keypressMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keypressMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            _macroRandom.NextReturn.Add(2);
            var result = keypressMacroTranslator.Translate("keypress*1234 a b c");
            var action = (KeyPressMacroAction)result[0];
            Debug.Assert(result.Count == 1);
            Debug.Assert(result[0] is KeyPressMacroAction);
            Debug.Assert(action.Keystroke == "b");
            Debug.Assert(action.Delay == 1234);
        }


        /**
         * @brief Validates error handling for invalid keyboard mappings
         * 
         * Ensures that keypress macros gracefully handle situations where
         * requested keystrokes don't have valid mappings in the current configuration.
         */
        private void _testTranslateReturnsEmptyIfInvalidKeyboardMappingIsFound()
        {
            var keypressMacroTranslator = _fixture();
            keypressMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keypressMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            _macroRandom.NextReturn.Add(2);
            var result = keypressMacroTranslator.Translate("keypress*1234 a b c d");
            Debug.Assert(result.Count == 0);
        }

        /**
         * @brief Verifies combined randomized delay and keystroke handling
         * 
         * Tests that keypress macros with both randomized delays and randomized
         * keystroke selection correctly handle both randomization parameters.
         */
        private void _testTranslateWithRandomDelayUsesCorrectRandomizedKeystrokeParameters()
        {
            var keypressMacroTranslator = _fixture();
            keypressMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keypressMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            _macroRandom.NextReturn.Add(3);
            _macroRandom.NextReturn.Add(2345);
            var result = keypressMacroTranslator.Translate("keypress*1234 3456 a b c");
            Debug.Assert(_macroRandom.NextCalls == 2);
            Debug.Assert(_macroRandom.NextCallArg_minValue[0] == 2);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[0] == 5);
        }

        /**
         * @brief Validates execution of combined randomized actions
         * 
         * Ensures that keypress macros with both randomized delays and randomized
         * keystroke selection correctly generate actions with proper timing and
         * keystroke values.
         */
        private void _testTranslateWithRandomDelayReturnsCorrectRandomizedAction()
        {
            var keypressMacroTranslator = _fixture();
            keypressMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            keypressMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            _macroRandom.NextReturn.Add(3);
            _macroRandom.NextReturn.Add(2345);
            var result = keypressMacroTranslator.Translate("keypress*1234 3456 a b c");
            var action = (KeyPressMacroAction)result[0];
            Debug.Assert(result.Count == 1);
            Debug.Assert(result[0] is KeyPressMacroAction);
            Debug.Assert(action.Keystroke == "b");
            Debug.Assert(action.Delay == 2345);
        }

        /**
         * @brief Validates graceful handling of malformed macro commands
         * 
         * This test ensures that the keypress macro system properly handles various
         * types of invalid input formats without crashing or producing unexpected
         * actions.
         */
        private void _testTranslateReturnsEmptyForInvalidMacroTexts()
        {
            List<string> invalidMacroTexts = [
                "meow", "keypress*a 1234", "keypress1234 a", "1234 2345 a b c"
            ];
            for (int i = 0; i < invalidMacroTexts.Count; i++)
            {
                var keypressMacroTranslator = _fixture();
                keypressMacroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
                keypressMacroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
                var result = keypressMacroTranslator.Translate(invalidMacroTexts[i]);
                Debug.Assert(result.Count == 0);
            }
        }

        /**
         * @brief Executes all keypress macro functionality tests
         * 
         * Runs the complete test suite to ensure keypress macros correctly handle
         * various configurations, randomization options, and error conditions.
         */
        public void Run()
        {
            _testTranslateReturnsEmptyWhenNothingIsInjected();
            _testTranslateReturnsEmptyWhenKeyboardMappingIsntInjected();
            _testTranslateReturnsEmptyWhenKeystrokeTransmitterIsntInjected();
            _testTranslateReturnsCorrectActionWhenBothAreInjected();
            _testTranslateUsesCorrectRandomizedDelayParameters();
            _testTranslateReturnsCorrectRandomizedDelayAction();
            _testTranslateUsesCorrectRandomizedKeystrokeParameters();
            _testTranslateReturnsCorrectRandomizedKeystrokeAction();
            _testTranslateReturnsEmptyIfInvalidKeyboardMappingIsFound();
            _testTranslateWithRandomDelayUsesCorrectRandomizedKeystrokeParameters();
            _testTranslateWithRandomDelayReturnsCorrectRandomizedAction();
            _testTranslateReturnsEmptyForInvalidMacroTexts();
        }
    }


    /**
     * @class MacroTranslatorTests
     * 
     * @brief Unit tests for verifying complete macro command processing functionality
     * 
     * This test class validates that complex macro sequences containing multiple command types
     * are correctly interpreted and executed in the proper order, ensuring coordinated
     * automation of keyboard inputs and timing controls during gameplay sequences.
     */
    public class MacroTranslatorTests
    {
        MockMacroSleeper _macroSleeper = new MockMacroSleeper();

        MockMacroRandom _macroRandom = new MockMacroRandom();

        MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        KeyboardMapping _keyboardMapping = new KeyboardMapping();

        List<string> _callOrder = [];

        /**
         * @brief Creates a comprehensive test environment for macro processing
         * 
         * @return Configured MacroTranslator instance
         * 
         * Prepares a complete test environment with all macro processing components
         * to verify complex command sequences without requiring actual hardware
         * interaction or timing delays during testing.
         */
        private MacroTranslator _fixture()
        {
            _callOrder = [];
            _macroSleeper = new MockMacroSleeper();
            _macroRandom = new MockMacroRandom();
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _keyboardMapping = new KeyboardMapping();
            _macroSleeper.CallOrder = _callOrder;
            _keystrokeTransmitter.CallOrder = _callOrder;
            _keyboardMapping.Characters["a"] = "0x12 0x34";
            _keyboardMapping.Characters["b"] = "0x23 0x45";
            _macroRandom.NextReturn.Add(2345);
            var macroTranslator = new MacroTranslator(
                [
                    new DelayMacroTranslator(_macroSleeper, _macroRandom),
                    new KeydownMacroTranslator(),
                    new KeyupMacroTranslator(),
                ]
            );
            return macroTranslator;
        }

        /**
         * @brief Tests proper sequencing of mixed macro commands
         * 
         * Validates that the macro system correctly interprets and orders complex
         * command sequences containing both keyboard actions and timing controls,
         * ensuring proper execution flow during automated gameplay sequences.
         */
        private void _testTranslateMultipleCommandsResultsInOrderedResult()
        {
            var macroTranslator = _fixture();
            macroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            macroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = macroTranslator.Translate(
                "keydown*a\n" +
                "delay*1234\n" +
                "delay*1234 3456\n" +
                "keyup*a"
            );
            Debug.Assert(result.Count == 4);
            Debug.Assert(result[0] is KeyDownMacroAction);
            Debug.Assert(((KeyDownMacroAction)result[0]).Keystroke == "a");
            Debug.Assert(result[1] is DelayMacroAction);
            Debug.Assert(((DelayMacroAction)result[1]).Delay == 1234);
            Debug.Assert(result[2] is DelayMacroAction);
            Debug.Assert(((DelayMacroAction)result[2]).Delay == 2345);
            Debug.Assert(result[3] is KeyUpMacroAction);
            Debug.Assert(((KeyUpMacroAction)result[3]).Keystroke == "a");
        }


        /**
         * @brief Tests precise execution ordering of complex macro sequences
         * 
         * Validates that complex macro sequences are executed in the correct
         * chronological order, ensuring proper coordination between keyboard
         * inputs and timing controls during automated gameplay.
         */
        private void _testTranslateMultipleCommandsExecutesInCorrectOrder()
        {
            var macroTranslator = _fixture();
            macroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            macroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = macroTranslator.Translate(
                "keydown*a\n" +
                "delay*1234\n" +
                "delay*1234 3456\n" +
                "keyup*a"
            );
            foreach (var action in result)
            {
                action.Execute();
            }
            List<string> expectedCallOrder = [
                new TestUtilities().Reference(_keystrokeTransmitter) + "Keydown",
                new TestUtilities().Reference(_macroSleeper) + "Sleep",
                new TestUtilities().Reference(_macroSleeper) + "Sleep",
                new TestUtilities().Reference(_keystrokeTransmitter) + "Keyup",
            ];
            Debug.Assert(_callOrder.Count == expectedCallOrder.Count);
            for (int i = 0; i < _callOrder.Count; i++)
            {
                Debug.Assert(_callOrder[i] == expectedCallOrder[i]);
            }
        }

        /**
         * @brief Verifies macro behavior when keyboard mapping is not configured
         * 
         * Validates that key-related macro commands are automatically filtered out
         * when no keyboard mapping is provided, ensuring only non-input commands
         * (such as timing delays) are processed. This prevents partial or invalid
         * executions when system keyboard layout information is unavailable.
         */
        private void _testNotInjectingKeyboardMappingResultsInNoActions()
        {
            var macroTranslator = _fixture();
            macroTranslator.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = macroTranslator.Translate(
                "keydown*a\n" +
                "delay*1234\n" +
                "delay*1234 3456\n" +
                "keyup*a"
            );
            Debug.Assert(result.Count == 0);
        }

        /**
         * @brief Verifies macro behavior when output device is not configured
         * 
         * Confirms that keystroke commands are automatically suppressed when
         * no output transmitter is available, ensuring only non-output commands
         * are processed. This safety feature prevents execution errors when
         * output devices are disconnected or improperly configured.
         */
        private void _testNotInjectingKeystrokeTransmitterResultsInNoActions()
        {
            var macroTranslator = _fixture();
            macroTranslator.Inject(SystemInjectType.KeyboardMapping, _keyboardMapping);
            var result = macroTranslator.Translate(
                "keydown*a\n" +
                "delay*1234\n" +
                "delay*1234 3456\n" +
                "keyup*a"
            );
            Debug.Assert(result.Count == 0);
        }

        /**
         * @brief Executes all macro processing functionality tests
         * 
         * Runs the complete test suite to ensure complex macro sequences are correctly
         * processed and executed, providing confidence in the reliability of coordinated
         * automation during gameplay sequences.
         */
        public void Run()
        {
            _testTranslateMultipleCommandsResultsInOrderedResult();
            _testTranslateMultipleCommandsExecutesInCorrectOrder();
            _testNotInjectingKeyboardMappingResultsInNoActions();
            _testNotInjectingKeystrokeTransmitterResultsInNoActions();
        }
    }

    /**
     * @brief Executes all macro processing functionality tests
     * 
     * Runs the complete test suite to ensure complex macro sequences are correctly
     * processed and executed, providing confidence in the reliability of coordinated
     * automation during gameplay sequences.
     */
    public class MacroTranslatorTestSuite
    {
        public void Run()
        {
            new DelayMacroTranslatorTests().Run();
            new KeydownMacroTranslatorTests().Run();
            new KeyupMacroTranslatorTests().Run();
            new KeypressMacroTranslatorTests().Run();
            new MacroTranslatorTests().Run();
        }
    }
}
