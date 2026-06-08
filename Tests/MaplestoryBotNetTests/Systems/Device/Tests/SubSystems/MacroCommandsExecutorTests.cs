using Interception;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Device.SubSystems;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.LibraryWrappers.Tests;
using MaplestoryBotNetTests.Systems.Device.Tests.SubSystems.Mocks;
using MaplestoryBotNetTests.TestHelpers;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.Device.Tests.SubSystems
{
    public class ButtonMappingFixture
    {
        public static List<Tuple<string, MouseButton>> Mapping()
        {
            return [
                new Tuple<string, MouseButton>("left", MouseButton.Left),
                new Tuple<string, MouseButton>("middle", MouseButton.Middle),
                new Tuple<string, MouseButton>("right", MouseButton.Right),
            ];
        }
    }


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
                _keyboardMapping(),
                new LockObject()
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
            keystrokeTransmitter.InjectKeyboardDevice(new DeviceContext(0x1234, 0x2345));
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
            keystrokeTransmitter.InjectKeyboardDevice(new DeviceContext(0x1234, 0x2345));
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


    public class MouseTransmitterTests
    {
        private MockInterceptionLibrary _interceptionLibrary = new MockInterceptionLibrary();

        private DeviceContext _mouseDevice = new DeviceContext(0, 0);

        private DeviceContext _context()
        {
            return new DeviceContext(0x1234, 0x2345);
        }

        private AbstractMouseTransmitter _fixture()
        {
            _mouseDevice = _context();
            _interceptionLibrary = new MockInterceptionLibrary();
            var transmitter = new MouseTransmitter(
                _interceptionLibrary,
                new LockObject()
            );
            transmitter.InjectMouseDevice(_mouseDevice);
            return transmitter;
        }

        /**
         * @brief Tests that the mouse transmitter sends absolute movement commands to the correct
         * hardware device
         * 
         * When the bot needs to move the mouse to a specific screen position (e.g., clicking on a
         * skill icon or targeting an enemy), the mouse transmitter must send a movement command
         * through the Interception library. This command includes the X and Y screen coordinates
         * as absolute pixel positions.
         */
        private void _testMouseTransmitterSendsMouseMovementCommands()
        {
            var transmitter = _fixture();
            var expected = _context();
            _interceptionLibrary.SendReturn.Add(0);
            transmitter.MouseMove(123, 234);
            Debug.Assert(_interceptionLibrary.SendCalls == 1);
            Debug.Assert(_interceptionLibrary.SendCallArg_context[0] == expected.Context);
            Debug.Assert(_interceptionLibrary.SendCallArg_device[0] == expected.Device);
            Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.State == 0);
            Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Flags == InterceptionInterop.MouseFlag.MoveAbsolute);
            Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Rolling == 0);
            Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.X == 123);
            Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Y == 234);
            Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Information == 0);
            Debug.Assert(_interceptionLibrary.SendCallArg_nstroke[0] == 1);
        }

        /**
         * @brief Tests that the mouse transmitter sends separate button-down commands for left,
         * right, and middle buttons
         * 
         * When the bot needs to press a mouse button (e.g., clicking on a UI element or holding
         * down a key for drag operations), the mouse transmitter must send button-specific down
         * commands through the Interception library. Each button type has a distinct state flag
         * that the operating system recognizes as a button press.
         */
        private void _testMouseTransmitterSendsMouseDownCommands()
        {
            foreach (
                var button in new[]
                {
                    new Tuple<MouseButton, InterceptionInterop.MouseState>(
                        MouseButton.Left, InterceptionInterop.MouseState.LeftButtonDown
                    ),
                    new Tuple<MouseButton, InterceptionInterop.MouseState>(
                        MouseButton.Middle, InterceptionInterop.MouseState.MiddleButtonDown
                    ),
                    new Tuple<MouseButton, InterceptionInterop.MouseState>(
                        MouseButton.Right, InterceptionInterop.MouseState.RightButtonDown
                    ),
                }
            )
            {
                var transmitter = _fixture();
                var expected = _context();
                _interceptionLibrary.SendReturn.Add(0);
                transmitter.MouseDown(button.Item1);
                Debug.Assert(_interceptionLibrary.SendCalls == 1);
                Debug.Assert(_interceptionLibrary.SendCallArg_context[0] == expected.Context);
                Debug.Assert(_interceptionLibrary.SendCallArg_device[0] == expected.Device);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.State == button.Item2);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Flags == 0);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Rolling == 0);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.X == 0);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Y == 0);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Information == 0);
                Debug.Assert(_interceptionLibrary.SendCallArg_nstroke[0] == 1);
            }
        }



        /**
         * @brief Tests that the mouse transmitter sends separate button-up commands for left,
         * right, and middle buttons
         * 
         * After pressing a mouse button, the bot must release it to complete the click action
         * or end a drag operation. The mouse transmitter sends button-specific up commands
         * through the Interception library, signaling the operating system to release the
         * previously pressed button.
         */
        private void _testMouseTransmitterSendsMouseUpCommands()
        {
            foreach (
                var button in new[]
                {
                    new Tuple<MouseButton, InterceptionInterop.MouseState>(
                        MouseButton.Left, InterceptionInterop.MouseState.LeftButtonUp
                    ),
                    new Tuple<MouseButton, InterceptionInterop.MouseState>(
                        MouseButton.Middle, InterceptionInterop.MouseState.MiddleButtonUp
                    ),
                    new Tuple<MouseButton, InterceptionInterop.MouseState>(
                        MouseButton.Right, InterceptionInterop.MouseState.RightButtonUp
                    ),
                }
            )
            {
                var transmitter = _fixture();
                var expected = _context();
                _interceptionLibrary.SendReturn.Add(0);
                transmitter.MouseUp(button.Item1);
                Debug.Assert(_interceptionLibrary.SendCalls == 1);
                Debug.Assert(_interceptionLibrary.SendCallArg_context[0] == expected.Context);
                Debug.Assert(_interceptionLibrary.SendCallArg_device[0] == expected.Device);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.State == button.Item2);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Flags == 0);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Rolling == 0);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.X == 0);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Y == 0);
                Debug.Assert(_interceptionLibrary.SendCallArg_stroke[0].Mouse.Information == 0);
                Debug.Assert(_interceptionLibrary.SendCallArg_nstroke[0] == 1);
            }
        }

        public void Run()
        {
            _testMouseTransmitterSendsMouseMovementCommands();
            _testMouseTransmitterSendsMouseDownCommands();
            _testMouseTransmitterSendsMouseUpCommands();
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


    public class MousePressMacroCommandTests
    {
        private MockMouseTransmitter _mouseTransmitter = new MockMouseTransmitter();

        private MockMacroSleeper _macroSleeper = new MockMacroSleeper();

        private List<string> _callOrder = [];

        private AbstractParsedMacroCommand _fixture(MouseButton button)
        {
            _mouseTransmitter = new MockMouseTransmitter();
            _macroSleeper = new MockMacroSleeper();
            _callOrder = [];
            _mouseTransmitter.CallOrder = _callOrder;
            _macroSleeper.CallOrder = _callOrder;
            return new MousePressMacroCommand(
                button,
                1234,
                _mouseTransmitter,
                _macroSleeper
            );
        }

        /**
         * @brief Tests that the mouse press macro command executes the complete press-release
         * sequence
         * 
         * When the bot executes a mouse press macro command, it must perform three actions
         * in the correct order: press the mouse button down, wait for the specified duration,
         * then release the button. This sequence creates a "click and hold" or a standard
         * click depending on the delay between press and release.
         */
        private void _testMousePressMacroCommandCalls()
        {
            for (int i = 0; i < (int)MouseButton.MaxNum; i++)
            {
                var button = (MouseButton)i;
                var mousePressCommand = _fixture(button);
                var transmitterRef = new TestUtilities().Reference(_mouseTransmitter);
                var sleeperRef = new TestUtilities().Reference(_macroSleeper);
                mousePressCommand.Run();
                Debug.Assert(_callOrder.Count == 3);
                Debug.Assert(_callOrder[0] == transmitterRef + "MouseDown");
                Debug.Assert(_callOrder[1] == sleeperRef + "Sleep");
                Debug.Assert(_callOrder[2] == transmitterRef + "MouseUp");
            }
        }

        /**
         * @brief Tests that the mouse press macro command waits the specified duration
         * between press and release
         * 
         * When clicking on menus or NPCs, the bot needs a small delay between pressing and
         * releasing the mouse button to ensure the game registers the click properly. This
         * delay helps prevent the game from interpreting a click as a mouse hover.
         */
        private void _testMousePressSleepsForAppropriateMilliseconds()
        {
            var mousePressCommand = _fixture(MouseButton.Left);
            mousePressCommand.Run();
            Debug.Assert(_macroSleeper.SleepCalls == 1);
            Debug.Assert(_macroSleeper.SleepCallArg_milliseconds[0] == 1234);
        }

        /**
         * @brief Tests that the mouse press macro command presses the specified mouse button
         * 
         * While MapleStory primarily uses keyboard for gameplay, mouse clicks are needed for
         * menu navigation and NPC interaction. The macro command must press the button that
         * was configured in the macro command.
         */
        private void _testMousePressMacroCommandPressesDownCorrectMouse()
        {
            for (int i = 0; i < (int)MouseButton.MaxNum; i++)
            {
                var button = (MouseButton)i;
                var mousePressCommand = _fixture(button);
                mousePressCommand.Run();
                Debug.Assert(_mouseTransmitter.MouseDownCalls == 1);
                Debug.Assert(_mouseTransmitter.MouseDownCallArg_button[0] == button);
            }
        }

        /**
         * @brief Tests that the mouse press macro command releases the specified mouse button
         * 
         * After the delay period, the macro command must release the same mouse button that
         * was pressed to complete the click action. This ensures the bot doesn't leave the
         * mouse button in a pressed state, which could interfere with subsequent operations.
         */
        private void _testMousePressMacroCommandPressesUpCorrectMouse()
        {
            for (int i = 0; i < (int)MouseButton.MaxNum; i++)
            {
                var button = (MouseButton)i;
                var mousePressCommand = _fixture(button);
                mousePressCommand.Run();
                Debug.Assert(_mouseTransmitter.MouseUpCalls == 1);
                Debug.Assert(_mouseTransmitter.MouseUpCallArg_button[0] == button);
            }
        }

        public void Run()
        {
            _testMousePressMacroCommandCalls();
            _testMousePressSleepsForAppropriateMilliseconds();
            _testMousePressMacroCommandPressesDownCorrectMouse();
            _testMousePressMacroCommandPressesUpCorrectMouse();
        }
    }


    public class MouseMoveMacroCommandTests
    {
        private MockMouseTransmitter _mouseTransmitter = new MockMouseTransmitter();

        private AbstractParsedMacroCommand _fixture()
        {
            _mouseTransmitter = new MockMouseTransmitter();
            return new MouseMoveMacroCommand(123, 234, _mouseTransmitter);
        }

        /**
         * @brief Tests that the mouse move macro command moves the cursor to the specified
         * screen coordinates
         * 
         * When interacting with NPCs or navigating menus in MapleStory, the bot must move the
         * mouse cursor to specific positions on the screen before clicking. The mouse move
         * macro command handles this by sending absolute movement commands through the mouse
         * transmitter.
         */
        private void _testMouseMoveMacroCommand()
        {
            var mouseMoveCommand = _fixture();
            mouseMoveCommand.Run();
            Debug.Assert(_mouseTransmitter.MouseMoveCalls == 1);
            Debug.Assert(_mouseTransmitter.MouseMoveCallArg_x[0] == 123);
            Debug.Assert(_mouseTransmitter.MouseMoveCallArg_y[0] == 234);
        }

        public void Run()
        {
            _testMouseMoveMacroCommand();
        }
    }


    public class MouseDownMacroCommandTests
    {
        private MockMouseTransmitter _mouseTransmitter = new MockMouseTransmitter();

        private AbstractParsedMacroCommand _fixture(MouseButton button)
        {
            _mouseTransmitter = new MockMouseTransmitter();
            return new MouseDownMacroCommand(button, _mouseTransmitter);
        }

        /**
         * @brief Tests that the mouse down macro command presses the specified mouse button
         * 
         * When navigating menus or interacting with NPCs in MapleStory, the bot may need to
         * press and hold a mouse button without releasing it immediately (e.g., for drag
         * operations or holding clicks). The mouse down macro command handles the press
         * portion of this action.
         */
        private void _testMouseDownMacroCommand()
        {
            foreach (var button in new[] { MouseButton.Left, MouseButton.Middle, MouseButton.Right })
            {
                var mouseDownCommand = _fixture(button);
                mouseDownCommand.Run();
                Debug.Assert(_mouseTransmitter.MouseDownCalls == 1);
                Debug.Assert(_mouseTransmitter.MouseDownCallArg_button[0] == button);
            }
        }

        public void Run()
        {
            _testMouseDownMacroCommand();
        }
    }


    public class MouseUpMacroCommandTests
    {
        private MockMouseTransmitter _mouseTransmitter = new MockMouseTransmitter();

        private AbstractParsedMacroCommand _fixture(MouseButton button)
        {
            _mouseTransmitter = new MockMouseTransmitter();
            return new MouseUpMacroCommand(button, _mouseTransmitter);
        }

        /**
         * @brief Tests that the mouse up macro command releases the specified mouse button
         * 
         * After a mouse down command has been executed, the bot must eventually release the
         * button to complete the click action or end a drag operation. The mouse up macro
         * command handles the release portion of this action.
         */
        private void _testMouseDownMacroCommand()
        {
            foreach (var button in new[] { MouseButton.Left, MouseButton.Middle, MouseButton.Right })
            {
                var mouseDownCommand = _fixture(button);
                mouseDownCommand.Run();
                Debug.Assert(_mouseTransmitter.MouseUpCalls == 1);
                Debug.Assert(_mouseTransmitter.MouseUpCallArg_button[0] == button);
            }
        }

        public void Run()
        {
            _testMouseDownMacroCommand();
        }
    }


    public class MousePressMacroCommandParserTests
    {
        private MockMacroRandom _macroRandom = new MockMacroRandom();

        private AbstractBracketContentsParser _bracketContentsParser = new BracketContentsParser();

        private MockParsedMacroCommandBuilder _parsedMacroCommandBuilder = new MockParsedMacroCommandBuilder();

        private MockParsedMacroCommand _parsedMacroCommand = new MockParsedMacroCommand();

        private AbstractMacroCommandParser _fixture()
        {
            _macroRandom = new MockMacroRandom();
            _bracketContentsParser = new BracketContentsParser();
            _parsedMacroCommandBuilder = new MockParsedMacroCommandBuilder();
            _parsedMacroCommand = new MockParsedMacroCommand();
            _parsedMacroCommandBuilder.BuildReturn.Add(_parsedMacroCommand);
            return new MousePressMacroCommandParser(
                _macroRandom,
                _bracketContentsParser,
                _parsedMacroCommandBuilder
            );
        }

        /**
         * @brief Tests that parsing a valid mouse press command correctly configures the command builder
         * 
         * When the user defines a mouse press macro command in the format "mouse press {button} {min} {max}",
         * the parser must extract the button type and the randomized delay range, then pass these values
         * to the command builder to construct the executable macro command.
         */
        private void _testParseValidMousePressBuilder()
        {
            foreach (var button in ButtonMappingFixture.Mapping())
            {
                var mousePressMacroCommandParser = _fixture();
                var builderRef = new TestUtilities().Reference(_parsedMacroCommandBuilder);
                _macroRandom.NextReturn.Add(3456);
                mousePressMacroCommandParser.Parse("mouse press {" + button.Item1 + "} {1234} {2345}");
                Debug.Assert(_parsedMacroCommandBuilder.CallOrder.Count == 3);
                Debug.Assert(_parsedMacroCommandBuilder.CallOrder[0] == builderRef + "WithArg");
                Debug.Assert(_parsedMacroCommandBuilder.CallOrder[1] == builderRef + "WithArg");
                Debug.Assert(_parsedMacroCommandBuilder.CallOrder[2] == builderRef + "Build");
                Debug.Assert((MouseButton)_parsedMacroCommandBuilder.WithArgCallArg_args[0] == button.Item2);
                Debug.Assert((int)_parsedMacroCommandBuilder.WithArgCallArg_args[1] == 3456);
            }
        }

        /**
         * @brief Tests that parsing a valid mouse press command returns a non-null result
         * 
         * When the user writes a correctly formatted mouse press command in the macro file
         * (e.g., "mouse press {left} {1234} {2345}"), the parser must recognize the command
         * and return an executable action that presses and holds the specified mouse button
         * for a randomized duration before releasing.
         */
        private void _testParseValidMousePressMacroCommand()
        {
            var mousePressMacroCommandParser = _fixture();
            _macroRandom.NextReturn.Add(3456);
            var result = mousePressMacroCommandParser.Parse("mouse press {left} {1234} {2345}");
            Debug.Assert(result == _parsedMacroCommand);
        }

        /**
         * @brief Tests that the mouse press command generates a randomized delay within the specified range
         * 
         * When the user defines a delay range (e.g., {1234} {2345}), the parser must generate a random
         * number between the minimum and maximum values. This randomized timing makes mouse clicks
         * appear more human-like and less detectable as automation when interacting with menus or NPCs.
         */
        private void _testParseValidMousePressRandomizedMilliseconds()
        {
            var mousePressMacroCommandParser = _fixture();
            _macroRandom.NextReturn.Add(3456);
            var result = mousePressMacroCommandParser.Parse("mouse press {left} {1234} {2345}");
            Debug.Assert(_macroRandom.NextCalls == 1);
            Debug.Assert(_macroRandom.NextCallArg_minValue[0] == 1234);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[0] == 2345);
        }

        /**
         * @brief Tests that invalid mouse press command formats are rejected
         * 
         * When the user enters a malformed mouse press command (incorrect spelling, missing brackets,
         * wrong number of parameters, empty brackets, etc.), the parser must return null and not build
         * a command. This prevents the bot from attempting to execute invalid or incomplete commands
         * that could cause unexpected behavior or crashes.
         */
        private void _testParseInvalidBracketContent()
        {
            var invalidInputs = new[]
            {
                "press mouse {left} {123} {234}",
                "mouse press {left}",
                "mosue press {left} {right}",
                "mouse press {left} {right} {234}",
                "mouse press {left} {123} {right}",
                "mouse press {left} {123}",
                "mouse press {left} {123} {234",
                "mouse press {} {123} {234}",
                "mouse press {left} {} {234}",
                "mouse press {left} {123} {}",
                "mouse press left 123 234"
            };
            for (int i = 0; i < invalidInputs.Length; i++)
            {
                var mousePressMacroCommandParser = _fixture();
                var result = mousePressMacroCommandParser.Parse(invalidInputs[i]);
                Debug.Assert(_parsedMacroCommandBuilder.BuildCalls == 0);
                Debug.Assert(result == null);
            }
        }

        public void Run()
        {
            _testParseValidMousePressBuilder();
            _testParseValidMousePressMacroCommand();
            _testParseValidMousePressRandomizedMilliseconds();
            _testParseInvalidBracketContent();
        }
    }


    public class MouseMoveMacroCommandParserTests
    {
        private AbstractBracketContentsParser _bracketContentsParser = new BracketContentsParser();

        private MockParsedMacroCommandBuilder _macroCommandBuilder = new MockParsedMacroCommandBuilder();

        private MockParsedMacroCommand _parsedMacroCommand = new MockParsedMacroCommand();

        private AbstractMacroCommandParser _fixture()
        {
            _bracketContentsParser = new BracketContentsParser();
            _macroCommandBuilder = new MockParsedMacroCommandBuilder();
            _parsedMacroCommand = new MockParsedMacroCommand();
            _macroCommandBuilder.BuildReturn.Add(_parsedMacroCommand);
            return new MouseMoveMacroCommandParser(
                _bracketContentsParser,
                _macroCommandBuilder
            );
        }

        /**
         * @brief Tests that parsing a valid mouse move command extracts the screen coordinates
         * 
         * When the user writes a mouse move command in the macro file (e.g., "mouse move {123} {234}"),
         * the parser must read the X and Y screen coordinates from the brackets and pass them to the
         * macro builder. The builder creates an executable action that moves the cursor to those
         * coordinates.
         */
        private void _testParseValidMouseMoveBuilder()
        {
            var mouseMoveMacroCommandParser = _fixture();
            var builderRef = new TestUtilities().Reference(_macroCommandBuilder);
            mouseMoveMacroCommandParser.Parse("mouse move {123} {234}");
            Debug.Assert(_macroCommandBuilder.CallOrder.Count == 2);
            Debug.Assert(_macroCommandBuilder.CallOrder[0] == builderRef + "WithArg");
            Debug.Assert(_macroCommandBuilder.CallOrder[1] == builderRef + "Build");
            Debug.Assert(((Tuple<int, int>)_macroCommandBuilder.WithArgCallArg_args[0]).Item1 == 123);
            Debug.Assert(((Tuple<int, int>)_macroCommandBuilder.WithArgCallArg_args[0]).Item2 == 234);
        }

        /**
         * @brief Tests that a properly formatted mouse move command is successfully recognized
         * 
         * When the macro file contains a correctly formatted mouse move command like "mouse move {123} {234}",
         * the parser must accept the command and prepare it for execution. The bot uses this command
         * to move the cursor to the specified screen coordinates for menu navigation or NPC interaction
         * in MapleStory.
         */
        private void _testParseValidMouseMoveMacroCommand()
        {
            var mousePressMacroCommandParser = _fixture();
            var result = mousePressMacroCommandParser.Parse("mouse move {123} {234}");
            Debug.Assert(result == _parsedMacroCommand);
        }

        /**
         * @brief Tests that invalid mouse move command formats are rejected
         * 
         * When the user enters a malformed mouse move command (incorrect spelling, missing brackets,
         * non-numeric coordinates, etc.), the parser must return null and not build a command. This
         * prevents the bot from attempting to execute invalid movement commands that could cause
         * the cursor to move to unintended locations or crash the bot.
         */
        private void _testParseInvalidBracketContent()
        {
            var invalidInputs = new[]
            {
                "move mouse {123} {234}",
                "mouse move 123 234",
                "mouse move {a} {b}",
                "mouse move {a} {123}",
                "mouse move {123} {b}",
            };
            foreach (var invalidInput in invalidInputs)
            {
                var mousePressMacroCommandParser = _fixture();
                var result = mousePressMacroCommandParser.Parse(invalidInput);
                Debug.Assert(_macroCommandBuilder.BuildCalls == 0);
                Debug.Assert(result == null);
            }
        }

        public void Run()
        {
            _testParseValidMouseMoveBuilder();
            _testParseValidMouseMoveMacroCommand();
            _testParseInvalidBracketContent();
        }
    }


    public class MouseDownMacroCommandParserTests
    {
        private AbstractBracketContentsParser _bracketContentsParser = new BracketContentsParser();

        private MockParsedMacroCommandBuilder _macroCommandBuilder = new MockParsedMacroCommandBuilder();

        private MockParsedMacroCommand _parsedMacroCommand = new MockParsedMacroCommand();

        private AbstractMacroCommandParser _fixture()
        {
            _bracketContentsParser = new BracketContentsParser();
            _macroCommandBuilder = new MockParsedMacroCommandBuilder();
            _parsedMacroCommand = new MockParsedMacroCommand();
            _macroCommandBuilder.BuildReturn.Add(_parsedMacroCommand);
            return new MouseDownMacroCommandParser(
                _bracketContentsParser,
                _macroCommandBuilder
            );
        }

        /**
         * @brief Tests that parsing a valid mouse down command extracts the button type from the macro text
         * 
         * When the user writes a mouse down command in the macro file (e.g., "mouse down {left}"),
         * the parser must read the button name from the brackets and pass it to the macro builder.
         * The builder creates an executable action that presses and holds that mouse button.
         */
        private void _testParseValidMouseDownBuilder()
        {
            foreach (var mapping in ButtonMappingFixture.Mapping())
            {
                var mouseDownMacroCommandParser = _fixture();
                var builderRef = new TestUtilities().Reference(_macroCommandBuilder);
                mouseDownMacroCommandParser.Parse("mouse down {" + mapping.Item1 + "}");
                Debug.Assert(_macroCommandBuilder.CallOrder.Count == 2);
                Debug.Assert(_macroCommandBuilder.CallOrder[0] == builderRef + "WithArg");
                Debug.Assert(_macroCommandBuilder.CallOrder[1] == builderRef + "Build");
                Debug.Assert(((MouseButton)_macroCommandBuilder.WithArgCallArg_args[0]) == mapping.Item2);
            }
        }

        /**
         * @brief Tests that a properly formatted mouse down command is successfully recognized
         * 
         * When the macro file contains a correctly formatted mouse down command like "mouse down {left}",
         * the parser must accept the command and prepare it for execution. The bot uses this command
         * to press and hold the specified mouse button for menu navigation or NPC interaction in
         * MapleStory, typically as part of a drag operation or context menu action.
         */
        private void _testParseValidMouseDownMacroCommand()
        {
            foreach (var mapping in ButtonMappingFixture.Mapping())
            {
                var mouseDownMacroCommandParser = _fixture();
                var result = mouseDownMacroCommandParser.Parse("mouse down {" + mapping.Item1 + "}");
                Debug.Assert(result == _parsedMacroCommand);
            }
        }

        /**
         * @brief Tests that incorrectly formatted mouse down commands are ignored by the parser
         * 
         * When the user makes a mistake typing a mouse down command (wrong spelling, missing brackets,
         * extra parameters, etc.), the parser must reject the command and return nothing. This prevents
         * the bot from trying to execute invalid commands that could press the wrong button or cause
         * the macro to stop working.
         */
        private void _testParseInvalidBracketContent()
        {
            var invalidInputs = new[]
            {
                "down mouse {123}",
                "mouse down left",
                "mouse down {left} {right}",
                "mouse down {left} {123}",
                "mouse down {123} {left}",
            };
            foreach (var invalidInput in invalidInputs)
            {
                var mouseDownMacroCommandParser = _fixture();
                var result = mouseDownMacroCommandParser.Parse(invalidInput);
                Debug.Assert(_macroCommandBuilder.BuildCalls == 0);
                Debug.Assert(result == null);
            }
        }

        public void Run()
        {
            _testParseValidMouseDownBuilder();
            _testParseValidMouseDownMacroCommand();
            _testParseInvalidBracketContent();
        }
    }


    public class MouseUpMacroCommandParserTests
    {
        private AbstractBracketContentsParser _bracketContentsParser = new BracketContentsParser();

        private MockParsedMacroCommandBuilder _macroCommandBuilder = new MockParsedMacroCommandBuilder();

        private MockParsedMacroCommand _parsedMacroCommand = new MockParsedMacroCommand();

        private AbstractMacroCommandParser _fixture()
        {
            _bracketContentsParser = new BracketContentsParser();
            _macroCommandBuilder = new MockParsedMacroCommandBuilder();
            _parsedMacroCommand = new MockParsedMacroCommand();
            _macroCommandBuilder.BuildReturn.Add(_parsedMacroCommand);
            return new MouseUpMacroCommandParser(
                _bracketContentsParser,
                _macroCommandBuilder
            );
        }

        /**
         * @brief Tests that parsing a valid mouse up command extracts the button type from the macro text
         * 
         * When the user writes a mouse up command in the macro file (e.g., "mouse up {left}"),
         * the parser must read the button name from the brackets and pass it to the macro builder.
         * The builder creates an executable action that presses and holds that mouse button.
         */
        private void _testParseValidMouseUpBuilder()
        {
            foreach (var mapping in ButtonMappingFixture.Mapping())
            {
                var mouseUpMacroCommandParser = _fixture();
                var builderRef = new TestUtilities().Reference(_macroCommandBuilder);
                mouseUpMacroCommandParser.Parse("mouse up {" + mapping.Item1 + "}");
                Debug.Assert(_macroCommandBuilder.CallOrder.Count == 2);
                Debug.Assert(_macroCommandBuilder.CallOrder[0] == builderRef + "WithArg");
                Debug.Assert(_macroCommandBuilder.CallOrder[1] == builderRef + "Build");
                Debug.Assert(((MouseButton)_macroCommandBuilder.WithArgCallArg_args[0]) == mapping.Item2);
            }
        }

        /**
         * @brief Tests that a properly formatted mouse up command is successfully recognized
         * 
         * When the macro file contains a correctly formatted mouse up command like "mouse up {left}",
         * the parser must accept the command and prepare it for execution. The bot uses this command
         * to press and hold the specified mouse button for menu navigation or NPC interaction in
         * MapleStory, typically as part of a drag operation or context menu action.
         */
        private void _testParseValidMouseUpMacroCommand()
        {
            foreach (var mapping in ButtonMappingFixture.Mapping())
            {
                var mouseUpMacroCommandParser = _fixture();
                var result = mouseUpMacroCommandParser.Parse("mouse up {" + mapping.Item1 + "}");
                Debug.Assert(result == _parsedMacroCommand);
            }
        }

        /**
         * @brief Tests that incorrectly formatted mouse up commands are ignored by the parser
         * 
         * When the user makes a mistake typing a mouse up command (wrong spelling, missing brackets,
         * extra parameters, etc.), the parser must reject the command and return nothing. This prevents
         * the bot from trying to execute invalid commands that could press the wrong button or cause
         * the macro to stop working.
         */
        private void _testParseInvalidBracketContent()
        {
            var invalidInputs = new[]
            {
                "up mouse {123}",
                "mouse up left",
                "mouse up {left} {right}",
                "mouse up {left} {123}",
                "mouse up {123} {left}",
            };
            foreach (var invalidInput in invalidInputs)
            {
                var mouseUpMacroCommandParser = _fixture();
                var result = mouseUpMacroCommandParser.Parse(invalidInput);
                Debug.Assert(_macroCommandBuilder.BuildCalls == 0);
                Debug.Assert(result == null);
            }
        }

        public void Run()
        {
            _testParseValidMouseUpBuilder();
            _testParseValidMouseUpMacroCommand();
            _testParseInvalidBracketContent();
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


    public class MacroCommandsExecutorTestSuite
    {
        public void Run()
        {
            new KeystrokeTransmitterTests().Run();
            new MouseTransmitterTests().Run();
            new BracketContentsParserTests().Run();
            new WaitMacroCommandTests().Run();
            new KeyPressMacroCommandTests().Run();
            new KeyDownMacroCommandTests().Run();
            new KeyUpMacroCommandTests().Run();
            new MousePressMacroCommandTests().Run();
            new MouseMoveMacroCommandTests().Run();
            new MouseDownMacroCommandTests().Run();
            new MouseUpMacroCommandTests().Run();
            new MousePressMacroCommandParserTests().Run();
            new MouseMoveMacroCommandParserTests().Run();
            new MouseDownMacroCommandParserTests().Run();
            new MouseUpMacroCommandParserTests().Run();
            new WaitMacroCommandParserTests().Run();
            new KeyPressMacroCommandParserTests().Run();
            new KeyDownMacroCommandParserTests().Run();
            new KeyUpMacroCommandParserTests().Run();
            new MacroCommandsExecutorTests().Run();
        }
    }
}
