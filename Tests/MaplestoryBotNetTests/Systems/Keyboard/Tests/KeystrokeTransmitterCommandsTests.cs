using System.Diagnostics;
using Interception;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNetTests.LibraryWrappers.Tests;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests
{
    /**
     * @class KeystrokeTransmitterTests
     * 
     * @brief Unit tests for verifying keyboard input transmission functionality
     * 
     * This test class validates that the bot correctly transmits keyboard input commands
     * to the game, ensuring reliable simulation of key presses and releases during
     * gameplay automation across different key types and configurations.
     */
    public class KeystrokeTransmitterTests
    {
        private MockInterceptionLibrary _interceptionLibrary = new MockInterceptionLibrary();

        /**
         * @brief Creates a test keyboard mapping configuration
         * 
         * @return Configured KeyboardMapping instance
         * 
         * Prepares a comprehensive keyboard mapping with various key types including
         * characters and function keys, ensuring thorough testing of the transmission
         * system across different input scenarios.
         */
        private KeyboardMapping _keyboardMapping()
        {
            var keyboardMapping = new KeyboardMapping();
            keyboardMapping.Characters.Add("c1", "0xE0 0xAB");
            keyboardMapping.Characters.Add("c2", "0xE1 0xBC");
            keyboardMapping.Characters.Add("c3", "0xCD");
            keyboardMapping.Functions.Add("f1", "0xE0 0xDE");
            keyboardMapping.Functions.Add("f2", "0xE1 0xEF");
            keyboardMapping.Functions.Add("f3", "0xF0");
            for (int i = 0; i < 6; i++)
                _interceptionLibrary.SendReturn.Add(0);
            return keyboardMapping;
        }


        /**
         * @brief Generates expected keystroke outputs for verification
         * 
         * @param keyState The key state (down or up) to test
         * 
         * @return List of expected keystroke structures
         * 
         * Creates expected keystroke patterns for test verification, ensuring
         * that the transmission system correctly interprets and converts
         * keyboard mappings into proper input commands.
         */
        private List<InterceptionInterop.KeyStroke> _expected(InterceptionInterop.KeyState keyState)
        {
            return [
                new InterceptionInterop.KeyStroke{Code=0xAB, State=InterceptionInterop.KeyState.E0 | keyState},
                new InterceptionInterop.KeyStroke{Code=0xBC, State=InterceptionInterop.KeyState.E1 | keyState},
                new InterceptionInterop.KeyStroke{Code=0xCD, State=keyState},
                new InterceptionInterop.KeyStroke{Code=0xDE, State=InterceptionInterop.KeyState.E0 | keyState},
                new InterceptionInterop.KeyStroke{Code=0xEF, State=InterceptionInterop.KeyState.E1 | keyState},
                new InterceptionInterop.KeyStroke{Code=0xF0, State=keyState}
            ];
        }

        /**
         * @brief Creates a test environment for keystroke transmission testing
         * @return Configured KeystrokeTransmitter instance
         * 
         * Prepares a test environment with mock interception library and keyboard
         * mapping to verify keystroke transmission without requiring actual
         * hardware interaction.
         */
        private KeystrokeTransmitter _fixture()
        {
            _interceptionLibrary = new MockInterceptionLibrary();
            return new KeystrokeTransmitter(
                _interceptionLibrary,
                new KeystrokeConverter(),
                _keyboardMapping()
            );
        }

        /**
         * @brief Tests proper handling of missing keyboard device
         * 
         * Validates that the transmission system correctly ignores key press
         * commands when no keyboard device is available, preventing errors
         * during system initialization or device detection phases.
         */
        private void _testKeydownIsntSentIfKeyboardDeviceIsntInjected()
        {
            var keystrokeTransmitter = _fixture();
            keystrokeTransmitter.Keydown("c1");
            keystrokeTransmitter.Keydown("c2");
            keystrokeTransmitter.Keydown("c3");
            keystrokeTransmitter.Keydown("f1");
            keystrokeTransmitter.Keydown("f2");
            keystrokeTransmitter.Keydown("f3");
            Debug.Assert(_interceptionLibrary.SendCalls == 0);
        }

        /**
         * @brief Tests successful key press transmission
         * 
         * Validates that the transmission system correctly sends key press
         * commands when a keyboard device is available, ensuring accurate
         * simulation of keyboard input during gameplay automation.
         */
        private void _testKeydownIsSentIfKeyboardDeviceIsInjected()
        {
            var keystrokeTransmitter = _fixture();
            keystrokeTransmitter.InjectKeyboardDevice(new KeyboardDeviceContext(0x1234, 0x2345));
            keystrokeTransmitter.Keydown("c1");
            keystrokeTransmitter.Keydown("c2");
            keystrokeTransmitter.Keydown("c3");
            keystrokeTransmitter.Keydown("f1");
            keystrokeTransmitter.Keydown("f2");
            keystrokeTransmitter.Keydown("f3");
            unsafe {
                var expected = _expected(InterceptionInterop.KeyState.Down);
                Debug.Assert(_interceptionLibrary.SendCalls == 6);
                for (int i = 0; i < _interceptionLibrary.SendCalls; i++) {
                    Debug.Assert(_interceptionLibrary.SendCallArg_stroke[i].Key.Code == expected[i].Code);
                    Debug.Assert(_interceptionLibrary.SendCallArg_stroke[i].Key.State == expected[i].State);
                    Debug.Assert(_interceptionLibrary.SendCallArg_context[i] == 0x1234);
                    Debug.Assert(_interceptionLibrary.SendCallArg_device[i] == 0x2345);
                    Debug.Assert(_interceptionLibrary.SendCallArg_nstroke[i] == 1);
                }
            }
        }

        /**
         * @brief Tests proper handling of missing keyboard device for key releases
         * 
         * Validates that the transmission system correctly ignores key release
         * commands when no keyboard device is available, ensuring system
         * stability during initialization or device detection issues.
         */
        private void _testKeyupIsntSentIfKeyboardDeviceIsntInjected()
        {
            var keystrokeTransmitter = _fixture();
            keystrokeTransmitter.Keyup("c1");
            keystrokeTransmitter.Keyup("c2");
            keystrokeTransmitter.Keyup("c3");
            keystrokeTransmitter.Keyup("f1");
            keystrokeTransmitter.Keyup("f2");
            keystrokeTransmitter.Keyup("f3");
            Debug.Assert(_interceptionLibrary.SendCalls == 0);
        }

        /**
         * @brief Tests successful key release transmission
         * 
         * Validates that the transmission system correctly sends key release
         * commands when a keyboard device is available, ensuring complete
         * and accurate simulation of keyboard input sequences during
         * gameplay automation.
         */
        private void _testKeyupIsSentIfKeyboardDeviceIsInjected()
        {
            var keystrokeTransmitter = _fixture();
            keystrokeTransmitter.InjectKeyboardDevice(new KeyboardDeviceContext(0x1234, 0x2345));
            keystrokeTransmitter.Keyup("c1");
            keystrokeTransmitter.Keyup("c2");
            keystrokeTransmitter.Keyup("c3");
            keystrokeTransmitter.Keyup("f1");
            keystrokeTransmitter.Keyup("f2");
            keystrokeTransmitter.Keyup("f3");
            unsafe
            {
                var expected = _expected(InterceptionInterop.KeyState.Up);
                Debug.Assert(_interceptionLibrary.SendCalls == 6);
                for (int i = 0; i < _interceptionLibrary.SendCalls; i++)
                {
                    Debug.Assert(_interceptionLibrary.SendCallArg_stroke[i].Key.Code == expected[i].Code);
                    Debug.Assert(_interceptionLibrary.SendCallArg_stroke[i].Key.State == expected[i].State);
                    Debug.Assert(_interceptionLibrary.SendCallArg_context[i] == 0x1234);
                    Debug.Assert(_interceptionLibrary.SendCallArg_device[i] == 0x2345);
                    Debug.Assert(_interceptionLibrary.SendCallArg_nstroke[i] == 1);
                }
            }
        }

        /**
         * @brief Executes all keystroke transmission tests
         * 
         * Runs the complete test suite to ensure the transmission system
         * correctly handles both key press and release commands with proper
         * device dependency handling, providing confidence in the reliability
         * of keyboard input simulation during automation.
         */
        public void Run()
        {
            _testKeydownIsntSentIfKeyboardDeviceIsntInjected();
            _testKeydownIsSentIfKeyboardDeviceIsInjected();
            _testKeyupIsntSentIfKeyboardDeviceIsntInjected();
            _testKeyupIsSentIfKeyboardDeviceIsInjected();
        }
    }


    public class BracketContentsParserTests
    {
        private List<string> _testInput()
        {
            return [
                "something {a} {b} {c}",
                "{{c23} {123}} {d}",
                "{{{}}{a}}",
                "}}}",
                "{{{",
                "{{}{c}}}",
                "{{{ab}{}{c}}",
                "something 1 2 3"
            ];
        }

        private List<List<string>> _expectedOuput()
        {
            return [
                ["a", "b", "c"],
                ["c23", "123", "d"],
                ["", "a"],
                [],
                [],
                ["", "c"],
                ["ab", "", "c"],
                []
            ];
        }

        /**
         * @brief Confirms the bracket parser correctly extracts user content
         * 
         * Validates that when users write text containing curly braces, the
         * parser reliably extracts the intended content. This ensures that
         * macro commands using braces for parameters work as expected.
         */
        private void _testBracketParser()
        {
            var testInput = _testInput();
            var expectedOutput = _expectedOuput();
            for (int i = 0; i < testInput.Count; i++)
            {
                var parser = new BracketContentsParser();
                var output = parser.Parse(testInput[i]);
                Debug.Assert(expectedOutput[i].Count == output.Count);
                for (int j = 0; j < output.Count(); j++)
                {
                    Debug.Assert(output[j] == expectedOutput[i][j]);
                }
            }
        }

        public void Run()
        {
            _testBracketParser();
        }
    }


    public class WaitMacroCommandTests
    {
        private MockMacroSleeper _macroSleeper = new MockMacroSleeper();

        private AbstractParsedMacroCommand _fixture()
        {
            _macroSleeper = new MockMacroSleeper();
            return new WaitMacroCommand(123, _macroSleeper);
        }

        /**
         * @brief Tests the execution behavior of the wait macro command
         * 
         * Validates that when the WaitMacroCommand is executed, it correctly
         * invokes the sleep operation on the provided macro sleeper with the
         * configured duration.
         */
        private void _testWaitMacroCommand()
        {
            var waitCommand = _fixture();
            waitCommand.Run();
            Debug.Assert(_macroSleeper.SleepCalls == 1);
            Debug.Assert(_macroSleeper.SleepCallArg_milliseconds[0] == 123);
        }

        public void Run()
        {
            _testWaitMacroCommand();
        }
    }


    public class KeyPressMacroCommandTests
    {
        private MockMacroSleeper _macroSleeper = new MockMacroSleeper();

        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private List<string> _callOrder = [];

        private AbstractParsedMacroCommand _fixture()
        {
            _macroSleeper = new MockMacroSleeper();
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _callOrder = [];
            _macroSleeper.CallOrder = _callOrder;
            _keystrokeTransmitter.CallOrder = _callOrder;
            return new KeyPressMacroCommand(
                "some key", 123, _macroSleeper, _keystrokeTransmitter
            );
        }

        /**
         * @brief Verifies that key presses execute in the correct sequence
         * 
         * This test confirms the proper execution order, ensuring keys are
         * held for the intended duration rather than just being tapped
         * or having the release happen before the key is fully pressed.
         */
        private void _testKeyPressMacroCommandCalls()
        {
            var keyPressCommand = _fixture();
            keyPressCommand.Run();
            var macroSleeperRef = new TestUtilities().Reference(_macroSleeper);
            var keystrokeTransmitterRef = new TestUtilities().Reference(_keystrokeTransmitter);
            Debug.Assert(_callOrder.Count == 3);
            Debug.Assert(_callOrder[0] == keystrokeTransmitterRef + "Keydown");
            Debug.Assert(_callOrder[1] == macroSleeperRef + "Sleep");
            Debug.Assert(_callOrder[2] == keystrokeTransmitterRef + "Keyup");
        }

        /**
         * @brief Confirms key presses last for the user-specified duration
         * 
         * When users specify a hold time for a key press, the macro should
         * wait exactly that amount of time between pressing the key down
         * and releasing it. This test ensures the timing is accurate,
         * allowing users to control how long a key appears to be held.
         */
        private void _testKeyPressMacroCommandSleepsForApproproateMilliseconds()
        {
            var keyPressCommand = _fixture();
            keyPressCommand.Run();
            Debug.Assert(_macroSleeper.SleepCallArg_milliseconds[0] == 123);
        }

        /**
         * @brief Ensures the correct key is pressed down during execution
         * 
         * When users specify a key to press in their macro, the command
         * should send the press event for that exact key. This test
         * verifies that the intended key receives the down press event,
         * not a different key.
         */
        private void _testKeyPressMacroCommandPressesDownCorrectKey()
        {
            var keyPressCommand = _fixture();
            keyPressCommand.Run();
            Debug.Assert(_keystrokeTransmitter.KeydownCallArg_keystroke[0] == "some key");
        }

        /**
         * @brief Ensures the correct key is released after the hold duration
         * 
         * After holding a key for the specified time, the macro should
         * release exactly that key. This test confirms the release event
         * targets the intended key, preventing keys from getting stuck
         * in a pressed state.
         */
        private void _testKeyPressMacroCommandPressesUpCorrectKey()
        {
            var keyPressCommand = _fixture();
            keyPressCommand.Run();
            Debug.Assert(_keystrokeTransmitter.KeyupCallArg_keystroke[0] == "some key");
        }

        public void Run()
        {
            _testKeyPressMacroCommandCalls();
            _testKeyPressMacroCommandSleepsForApproproateMilliseconds();
            _testKeyPressMacroCommandPressesDownCorrectKey();
            _testKeyPressMacroCommandPressesUpCorrectKey();
        }
    }


    public class KeyDownMacroCommandTests
    {
        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private AbstractParsedMacroCommand _fixture()
        {
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            return new KeyDownMacroCommand(
                "some key", _keystrokeTransmitter
            );
        }

        /**
         * @brief Verifies that key down commands properly press the specified key
         * 
         * When users need to press a key and keep it held down (like holding
         * Shift to capitalize letters or holding Ctrl for multi-selection),
         * the key down command should send exactly one press event for the
         * intended key.
         */
        private void _testKeyDownMacroCommand()
        {
            var keyDownCommand = _fixture();
            keyDownCommand.Run();
            Debug.Assert(_keystrokeTransmitter.KeydownCalls == 1);
            Debug.Assert(_keystrokeTransmitter.KeydownCallArg_keystroke[0] == "some key");
        }

        public void Run()
        {
            _testKeyDownMacroCommand();
        }
    }


    public class KeyUpMacroCommandTests
    {
        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private AbstractParsedMacroCommand _fixture()
        {
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            return new KeyUpMacroCommand(
                "some key", _keystrokeTransmitter
            );
        }

        /**
         * @brief Verifies that key up commands properly release the specified key
         * 
         * After pressing and holding a key, users need to release it to complete
         * the action. This test ensures the key up command sends exactly one
         * release event for the intended key, allowing users to control the
         * exact moment a key is let go.
         */
        private void _testKeyUpMacroCommand()
        {
            var keyDownCommand = _fixture();
            keyDownCommand.Run();
            Debug.Assert(_keystrokeTransmitter.KeyupCalls == 1);
            Debug.Assert(_keystrokeTransmitter.KeyupCallArg_keystroke[0] == "some key");
        }

        public void Run()
        {
            _testKeyUpMacroCommand();
        }
    }



    public class WaitMacroCommandParserTests
    {
        private MockMacroRandom _macroRandom = new MockMacroRandom();

        private AbstractBracketContentsParser _bracketContentsParser = new BracketContentsParser();

        private MockParsedMacroCommandBuilder _parsedMacroCommandBuilder = new MockParsedMacroCommandBuilder();

        private MockParsedMacroCommand _parsedMacroCommand = new MockParsedMacroCommand();

        private AbstractMacroCommandParser _fixture()
        {
            _macroRandom = new MockMacroRandom();
            _macroRandom.NextReturn.Add(150);
            _bracketContentsParser = new BracketContentsParser();
            _parsedMacroCommandBuilder = new MockParsedMacroCommandBuilder();
            _parsedMacroCommand = new MockParsedMacroCommand();
            _parsedMacroCommandBuilder.BuildReturn.Add(_parsedMacroCommand);
            return new WaitMacroCommandParser(
                _macroRandom,
                _bracketContentsParser,
                _parsedMacroCommandBuilder
            );
        }

        /**
         * @brief Verifies that properly formatted wait commands execute successfully
         * 
         * When a user writes a wait command with two numbers in curly braces,
         * the parser should recognize it and prepare it for execution. This
         * test confirms that commands like "wait {123} {234}" are correctly
         * processed and ready to run.
         */
        private void _testParseValidWait()
        {
            var waitMacroCommandParser = _fixture();
            var result = waitMacroCommandParser.Parse("wait {123} {234}");
            Debug.Assert(_parsedMacroCommandBuilder.BuildCalls == 1);
            Debug.Assert(_parsedMacroCommandBuilder.WithArgCalls == 1);
            Debug.Assert((int)_parsedMacroCommandBuilder.WithArgCallArg_args[0] == 150);
            Debug.Assert(result == _parsedMacroCommand);
        }

        /**
         * @brief Confirms that wait durations vary within user-specified ranges
         * 
         * When users provide a range like {100} {300}, the parser should
         * generate random wait times between those values. This test ensures
         * the parser respects the user's minimum and maximum boundaries,
         * creating natural variation instead of fixed delays.
         */
        private void _testParseRandomizedMilliseconds()
        {
            var waitMacroCommandParser = _fixture();
            waitMacroCommandParser.Parse("wait {123} {234}");
            Debug.Assert(_macroRandom.NextCalls == 1);
            Debug.Assert(_macroRandom.NextCallArg_minValue[0] == 123);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[0] == 234);
        }

        /**
         * @brief Ensures the parser ignores incorrectly written wait commands
         * 
         * Users might accidentally format wait commands incorrectly. This test
         * verifies that the parser safely ignores these mistakes rather than
         * crashing or creating broken commands. The parser should silently
         * reject invalid formats and continue processing other commands.
         */
        private void _testParseInvalidBracketContent()
        {
            var invalidInputs = new[]
            {
                "wait {abc} {123}",
                "wait {123} {xyz}",
                "wait {123}",
                "wait {123} {234} {345}",
                "wait",
                "key press {123} {456}",
                "wait 123 456",
                "wait {123} {456",
                "wait {123} 456}",
                "wait {123} 456}"
            };
            for (int i = 0; i < invalidInputs.Length; i++)
            {
                var waitMacroCommandParser = _fixture();
                var result = waitMacroCommandParser.Parse(invalidInputs[i]);
                Debug.Assert(_parsedMacroCommandBuilder.BuildCalls == 0);
                Debug.Assert(result == null);
            }
        }

        public void Run()
        {
            _testParseValidWait();
            _testParseRandomizedMilliseconds();
            _testParseInvalidBracketContent();
        }
    }


    public class KeyPressMacroCommandParserTests
    {
        private MockMacroRandom _macroRandom = new MockMacroRandom();

        private BracketContentsParser _bracketContentsParser = new BracketContentsParser();

        private MockParsedMacroCommandBuilder _macroCommandBuilder = new MockParsedMacroCommandBuilder();

        private MockParsedMacroCommand _parsedMacroCommand = new MockParsedMacroCommand();

        private AbstractMacroCommandParser _fixture()
        {
            _macroRandom = new MockMacroRandom();
            _bracketContentsParser = new BracketContentsParser();
            _macroCommandBuilder = new MockParsedMacroCommandBuilder();
            _parsedMacroCommand = new MockParsedMacroCommand();
            _macroCommandBuilder.BuildReturn.Add(_parsedMacroCommand);
            return new KeyPressMacroCommandParser(
                _macroRandom, _bracketContentsParser, _macroCommandBuilder
            );
        }

        private List<string> _validInputs()
        {
            return [
                "key press {a} {123} {234}",
                "key press {a} {b} {123} {234}",
                "key press {a} {b} {c} {123} {234}",
            ];
        }

        private List<List<string>> _randomKeys()
        {
            return [["a"], ["a", "b"], ["a", "b", "c"]];
        }

        /**
         * @brief Verifies that valid key press commands produce correctly configured commands
         * 
         * When users write a key press command with multiple key options and a timing range,
         * the parser should randomly select one key and generate a random hold time within
         * the specified range. This test confirms the resulting command contains the
         * randomly selected key and the generated duration.
         */
        private void _testParseValidKeyPressResult()
        {
            var validInputs = _validInputs();
            var randomKey = _randomKeys();
            for (int i = 0; i < validInputs.Count(); i++)
            for (int j = 0; j < randomKey[i].Count(); j++)
            {
                var keyPressMacroCommandParser = _fixture();
                _macroRandom.NextReturn.Add(j);
                _macroRandom.NextReturn.Add(150);
                var result = keyPressMacroCommandParser.Parse(validInputs[i]);
                Debug.Assert(_macroCommandBuilder.WithArgCalls == 2);
                Debug.Assert((string) _macroCommandBuilder.WithArgCallArg_args[0] == randomKey[i][j]);
                Debug.Assert((int) _macroCommandBuilder.WithArgCallArg_args[1] == 150);
                Debug.Assert(_macroCommandBuilder.BuildCalls == 1);
                Debug.Assert(result == _parsedMacroCommand);
            }
        }

        /**
         * @brief Confirms random selection boundaries are correctly applied
         * 
         * When users specify multiple key options and a timing range, the parser must
         * generate random values within the correct ranges. This test ensures hold times
         * are randomly generated within the user-specified min/max range
         */
        private void _testParseValidKeyPressRandomDomain()
        {
            var validInputs = _validInputs();
            var randomKey = _randomKeys();
            for (int i = 0; i < validInputs.Count(); i++)
            for (int j = 0; j < randomKey[i].Count(); j++)
            {
                var keyPressMacroCommandParser = _fixture();
                _macroRandom.NextReturn.Add(j);
                _macroRandom.NextReturn.Add(150);
                keyPressMacroCommandParser.Parse(validInputs[i]);
                Debug.Assert(_macroRandom.NextCalls == 2);
                Debug.Assert(_macroRandom.NextCallArg_minValue[0] == 0);
                Debug.Assert(_macroRandom.NextCallArg_maxValue[0] == i);
                Debug.Assert(_macroRandom.NextCallArg_minValue[1] == 123);
                Debug.Assert(_macroRandom.NextCallArg_maxValue[1] == 234);
            }
        }

        /**
         * @brief Ensures the parser rejects incorrectly written key press commands
         * 
         * Users might accidentally format key press commands incorrectly. This test
         * verifies that the parser safely ignores these mistakes rather than crashing
         * or creating broken commands.
         */
        private void _testParseInvalidBracketContent()
        {
            var invalidInputs = new[]
            {
                "press key {a} {123} {234}",
                "key press {a}",
                "key press {a} {b}",
                "key press {a} {abc} {234}",
                "key press {a} {123} {xyz}",
                "key press {a} {123}",
                "key press {a} {123} {234",
                "key press {} {123} {234}",
                "key press {a} {} {234}",
                "key press {a} {123} {}",
                "key press a 123 234"
            };
            for (int i = 0; i < invalidInputs.Count(); i++)
            {
                var keyPressMacroCommandParser = _fixture();
                var result = keyPressMacroCommandParser.Parse(invalidInputs[i]);
                Debug.Assert(_macroCommandBuilder.BuildCalls == 0);
                Debug.Assert(result == null);
            }
        }

        public void Run()
        {
            _testParseValidKeyPressResult();
            _testParseValidKeyPressRandomDomain();
            _testParseInvalidBracketContent();
        }
    }


    public class KeyDownMacroCommandParserTests
    {
        private BracketContentsParser _bracketContentsParser = new BracketContentsParser();

        private MockParsedMacroCommandBuilder _macroCommandBuilder = new MockParsedMacroCommandBuilder();

        private MockParsedMacroCommand _parsedMacroCommand = new MockParsedMacroCommand();

        private AbstractMacroCommandParser _fixture()
        {
            _bracketContentsParser = new BracketContentsParser();
            _macroCommandBuilder = new MockParsedMacroCommandBuilder();
            _parsedMacroCommand = new MockParsedMacroCommand();
            _macroCommandBuilder.BuildReturn.Add(_parsedMacroCommand);
            return new KeyDownMacroCommandParser(
                _bracketContentsParser, _macroCommandBuilder
            );
        }

        /**
         * @brief Verifies that valid key down commands produce correctly configured commands
         * 
         * When users write a key down command with a valid key in curly braces,
         * the parser should extract the key and create a command that presses
         * and holds that key.
         */
        private void _testParseValidKeyDownResult()
        {
            var keyDownMacroCommandParser = _fixture();
            var result = keyDownMacroCommandParser.Parse("key down {a}");
            Debug.Assert(_macroCommandBuilder.BuildCalls == 1);
            Debug.Assert(_macroCommandBuilder.WithArgCalls == 1);
            Debug.Assert((string) _macroCommandBuilder.WithArgCallArg_args[0] == "a");
            Debug.Assert(result == _parsedMacroCommand);
        }

        /**
         * @brief Ensures the parser rejects incorrectly written key down commands
         * 
         * Users might accidentally format key down commands incorrectly. This test
         * verifies that the parser safely ignores these mistakes.
         */
        private void _testParseInvalidBracketContent()
        {
            var invalidInputs = new[]
            {
                "down key {a}",
                "key down a",
                "key down {a} {b}",
            };
            for (int i = 0; i < invalidInputs.Count(); i++)
            {
                var keyDownMacroCommandParser = _fixture();
                var result = keyDownMacroCommandParser.Parse(invalidInputs[i]);
                Debug.Assert(_macroCommandBuilder.BuildCalls == 0);
                Debug.Assert(result == null);
            }
        }

        public void Run()
        {
            _testParseValidKeyDownResult();
            _testParseInvalidBracketContent();
        }
    }


    public class KeyUpMacroCommandParserTests
    {
        private BracketContentsParser _bracketContentsParser = new BracketContentsParser();

        private MockParsedMacroCommandBuilder _macroCommandBuilder = new MockParsedMacroCommandBuilder();

        private MockParsedMacroCommand _parsedMacroCommand = new MockParsedMacroCommand();

        private AbstractMacroCommandParser _fixture()
        {
            _bracketContentsParser = new BracketContentsParser();
            _macroCommandBuilder = new MockParsedMacroCommandBuilder();
            _parsedMacroCommand = new MockParsedMacroCommand();
            _macroCommandBuilder.BuildReturn.Add(_parsedMacroCommand);
            return new KeyUpMacroCommandParser(
                _bracketContentsParser, _macroCommandBuilder
            );
        }

        /**
         * @brief Validates key up command parsing functionality for macro scripts
         * 
         * Tests ensure that key up commands in macro scripts are correctly interpreted,
         * allowing users to release previously pressed keys. This is essential for
         * completing key press sequences and preventing keys from getting stuck.
         */
        private void _testParseValidKeyUpResult()
        {
            var keyDownMacroCommandParser = _fixture();
            var result = keyDownMacroCommandParser.Parse("key up {a}");
            Debug.Assert(_macroCommandBuilder.BuildCalls == 1);
            Debug.Assert(_macroCommandBuilder.WithArgCalls == 1);
            Debug.Assert((string)_macroCommandBuilder.WithArgCallArg_args[0] == "a");
            Debug.Assert(result == _parsedMacroCommand);
        }

        /**
         * @brief Verifies that valid key up commands produce correctly configured commands
         * 
         * When users write a key up command with a valid key in curly braces,
         * the parser should extract the key and create a command that releases
         * that key.
         */
        private void _testParseInvalidBracketContent()
        {
            var invalidInputs = new[]
            {
                "up key {a}",
                "key up a",
                "key up {a} {b}",
            };
            for (int i = 0; i < invalidInputs.Count(); i++)
            {
                var keyDownMacroCommandParser = _fixture();
                var result = keyDownMacroCommandParser.Parse(invalidInputs[i]);
                Debug.Assert(_macroCommandBuilder.BuildCalls == 0);
                Debug.Assert(result == null);
            }
        }

        public void Run()
        {
            _testParseValidKeyUpResult();
            _testParseInvalidBracketContent();
        }
    }


    public class MacroCommandsExecutorTests
    {

        List<AbstractMacroCommandParser> _macroCommandsParsers = [];

        List<AbstractParsedMacroCommand> _parsedMacroCommands = [];

        List<string> _callOrder = [];

        private AbstractMacroCommandsExecutor _fixture(int maxParsers)
        {
            _macroCommandsParsers = [];
            _callOrder = [];
            for (int i = 0; i < maxParsers; i++)
            {
                var parser = new MockMacroCommandsParser();
                _macroCommandsParsers.Add(parser);
                for (int j = i + 1; j < maxParsers; j++)
                {
                    parser.ParseReturn.Add(null);
                }
                var parsedMacroCommand = new MockParsedMacroCommand { CallOrder = _callOrder };
                parser.ParseReturn.Add(parsedMacroCommand);
                _parsedMacroCommands.Add(parsedMacroCommand);
            }
            return new MacroCommandsExecutor(_macroCommandsParsers);
        }

        /**
         * @brief Verifies that user macro scripts execute completely from start to finish
         * 
         * When users write a macro with multiple commands, they expect every command
         * to run in the exact order they wrote them. This test ensures the macro
         * engine processes each line of the user's script, finds the right handler
         * for each command type, and executes them all.
         */
        private void _testExecuteTriesParseAndRunsCommandIfFound()
        {
            var macroCommands = new List<string> { "1", "2", "3", "4", "5"};
            var macroCommandsExecutor = _fixture(macroCommands.Count());
            macroCommandsExecutor.Execute(macroCommands);
            for (int i = 0; i < macroCommands.Count(); i++)
            {
                var parsedMacroCommand = (MockParsedMacroCommand)_parsedMacroCommands[i];
                var parsedMacroCommandCall = _callOrder[macroCommands.Count() - i - 1];
                var parsedMacroCommandRefrence = new TestUtilities().Reference(parsedMacroCommand);
                Debug.Assert(parsedMacroCommand.RunCalls == 1);
                Debug.Assert(parsedMacroCommandCall == parsedMacroCommandRefrence + "Run");
            }
        }

        public void Run()
        {
            _testExecuteTriesParseAndRunsCommandIfFound();
        }
    }


    public class KeystrokeTransmitterCommandsTestSuite
    {
    
        public void Run()
        {
            new KeystrokeTransmitterTests().Run();
            new BracketContentsParserTests().Run();
            new WaitMacroCommandTests().Run();
            new KeyPressMacroCommandTests().Run();
            new KeyDownMacroCommandTests().Run();
            new KeyUpMacroCommandTests().Run();
            new WaitMacroCommandParserTests().Run();
            new KeyPressMacroCommandParserTests().Run();
            new KeyDownMacroCommandParserTests().Run();
            new KeyUpMacroCommandParserTests().Run();
            new MacroCommandsExecutorTests().Run();
        }
    }
}
