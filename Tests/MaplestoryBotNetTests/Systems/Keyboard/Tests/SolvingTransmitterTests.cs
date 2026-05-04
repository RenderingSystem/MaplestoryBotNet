using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.Json;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests
{
    public class RuneSolverCallerTests
    {
        private MockRuneSolverManager _runeSolverManager = (
            new MockRuneSolverManager()
        );

        private AbstractRuneSolverCaller _fixture()
        {
            _runeSolverManager = new MockRuneSolverManager();
            return new RuneSolverCaller(_runeSolverManager);
        }

        private Image<Bgra32> _image()
        {
            var image = new Image<Bgra32>(2, 2);
            image[0, 0] = new Bgra32(255, 0, 0, 0);
            image[0, 1] = new Bgra32(0, 255, 0, 0);
            image[1, 0] = new Bgra32(0, 0, 255, 0);
            image[1, 1] = new Bgra32(0, 0, 0, 255);
            return image;
        }

        private RuneDetection _runeDetection()
        {
            return new RuneDetection
            {
                RuneSolverIPAddress = "123",
                RuneSolverPort = "234",
                RuneSolverRoute = "345"
            };
        }

        private string _base64Image()
        {
            using var memoryStream = new MemoryStream();
            _image().Save(memoryStream, JpegFormat.Instance);
            byte[] imageBytes = memoryStream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }

        /**
         * @brief Verifies that the RuneSolverCaller correctly constructs the POST request
         * URL and sends the base64-encoded image to the solver endpoint
         * 
         * When the rune puzzle solving system captures a rune image that needs to be
         * processed, the caller must build the correct URL using the detection settings
         * (IP address, port, route) and send the image as a base64-encoded string in the
         * POST request body. This test ensures that the URL is properly formatted as
         * "{IP}:{Port}/{Route}/solve" and that the image data is correctly encoded and
         * passed to the underlying HTTP client with the appropriate content type.
         */
        private void _testCallerPostsImageToUrl()
        {
            var caller = _fixture();
            _runeSolverManager.PostReturn.Add("meow");
            var result = caller.Call(_runeDetection(), _image());
            Debug.Assert(_runeSolverManager.PostCalls == 1);
            Debug.Assert(_runeSolverManager.PostCallArg_url[0] == "123:234/345/solve");
            Debug.Assert(_runeSolverManager.PostCallArg_content[0] == _base64Image());
            Debug.Assert(result == "meow");
        }

        public void Run()
        {
            _testCallerPostsImageToUrl();
        }
    }


    public class SolvingScreenCaptureSubscriberTests
    {
        private MockThread _solvingExecutorThread = (
            new MockThread(new ThreadRunningState())
        );

        private SolvingOrchestratorThread _orchestratorThread = (
            new SolvingOrchestratorThread(
                new MockThread(new ThreadRunningState()),
                new ThreadRunningState(),
                new BlockingCollection<int>()
            )
        );

        private Image<Bgra32> _image = new Image<Bgra32>(2, 2);

        private AbstractScreenCaptureSubscriber _fixture()
        {
            _solvingExecutorThread = new MockThread(new ThreadRunningState());
            _orchestratorThread = (
                new SolvingOrchestratorThread(
                    _solvingExecutorThread,
                    new ThreadRunningState(),
                    new BlockingCollection<int>()
                )
            );
            var subscriber = new SolvingScreenCaptureSubscriber(new SemaphoreSlim(0, 1));
            subscriber.Inject(SystemInjectType.ThreadDependency, _orchestratorThread);
            subscriber.Notify(_image, false);
            return subscriber;
        }


        /**
         * @brief Verifies that when the rune puzzle image is captured from the screen,
         * it is successfully passed to the thread that solves the puzzle
         * 
         * When the main bot detects a rune puzzle on the game screen, it captures the
         * puzzle image and notifies the solving system. The solving system must then
         * deliver this image to the executor thread that processes the puzzle and sends
         * the corresponding arrow keystrokes. This test ensures that the image makes it
         * all the way to the solving thread, so users don't have to worry about the puzzle
         * being detected but not solved.
         */
        private void _testProcessingImageInjectsImageData()
        {
            var subscriber = _fixture();
            subscriber.ProcessImage();
            Debug.Assert(_solvingExecutorThread.InjectCalls == 1);
            Debug.Assert(_solvingExecutorThread.InjectCallArg_data[0] == _image);
        }

        public void Run()
        {
            _testProcessingImageInjectsImageData();
        }
    }


    public class RuneSolverWorkflowTests
    {
        private MockMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder = new MockMacroCommandsExecutorBuilder();

        private MockMacroCommandsExecutor _macroCommandsExecutor = new MockMacroCommandsExecutor();

        private MockKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private MockRuneSolverCaller _runeSolverCaller = new MockRuneSolverCaller();

        private Image<Bgra32> _solveImage = new Image<Bgra32>(2, 2);

        private MaplestoryBotConfiguration _maplestoryBotConfiguration = new MaplestoryBotConfiguration();
        private AbstractRuneSolverWorkflow _fixture()
        {
            _macroCommandsExecutorBuilder = new MockMacroCommandsExecutorBuilder();
            _macroCommandsExecutor = new MockMacroCommandsExecutor();
            _macroCommandsExecutorBuilder.BuildReturn.Add(_macroCommandsExecutor);
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _runeSolverCaller = new MockRuneSolverCaller();
            _solveImage = new Image<Bgra32>(2, 2);
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                RuneDetection = new RuneDetection(),
                RuneInteractKey = "meow"
            };
            var runeSolverWorkflow = new RuneSolverWorkflow(
                _macroCommandsExecutorBuilder, _runeSolverCaller
            );
            return runeSolverWorkflow;
        }

        private void _assertEqual(object _1, object _2)
        {
            var json1 = JsonSerializer.Serialize(_1);
            var json2 = JsonSerializer.Serialize(_2);
            Debug.Assert(json1 == json2);
        }

        private JsonDocument _predictions()
        {
            var predictions = new[]
            {
                new { X = 100, Y = 200, Class = "left" },
                new { X = 300, Y = 150, Class = "up" },
                new { X = 500, Y = 250, Class = "right" },
                new { X = 700, Y = 180, Class = "down" }
            };
            return JsonDocument.Parse(JsonSerializer.Serialize(predictions));
        }

        /**
         * @brief Verifies that the workflow correctly validates all prerequisites when
         * proper configuration and dependencies are provided
         * 
         * When the rune solver is ready to solve a puzzle, it must first check that all
         * required components are available: the rune detection settings, the interact key,
         * and the keystroke transmitter. This test ensures that when everything is properly
         * set up, the validation succeeds and returns the rune detection configuration.
         */
        private void _testValidatePrerequisites()
        {

            var workflow = _fixture();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                RuneDetection = new RuneDetection(),
                RuneInteractKey = "meow"
            };
            workflow.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            workflow.Inject(SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration);
            var result = workflow.ValidatePrerequisites();
            Debug.Assert(result != null);
            _assertEqual(result, _maplestoryBotConfiguration.RuneDetection);
        }

        /**
         * @brief Verifies that the workflow fails validation when the interact key is empty
         * 
         * The interact key is required to start the rune interaction sequence. If the user
         * has not configured an interact key (e.g., the key used to press the rune),
         * the solving workflow should not proceed to avoid sending invalid keystrokes.
         */
        private void _testValidatePrerequisitesFailsWhenInteractKeyEmpty()
        {

            var workflow = _fixture();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                RuneDetection = new RuneDetection(),
                RuneInteractKey = ""
            };
            workflow.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            workflow.Inject(SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration);
            var result = workflow.ValidatePrerequisites();
            Debug.Assert(result == null);
        }

        /**
         * @brief Verifies that the workflow fails validation when the keystroke transmitter
         * has not been injected
         * 
         * The solving workflow needs a keystroke transmitter to send arrow key presses
         * to the game. If the transmitter is missing (e.g., the solving thread wasn't
         * properly initialized), the workflow should not attempt to send any keystrokes.
         */
        private void _testValidatePrerequisitesFailsWhenTransmitterNotInjected()
        {
            var workflow = _fixture();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                RuneDetection = new RuneDetection(),
                RuneInteractKey = "meow"
            };
            workflow.Inject(SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration);
            var result = workflow.ValidatePrerequisites();
            Debug.Assert(result == null);
        }

        /**
         * @brief Verifies that the interaction sequence sends the correct keystrokes
         * to press the interact key and wait for the rune puzzle to appear
         * 
         * When the bot reaches a rune, it must first press the interact key to activate
         * the puzzle, then wait for the puzzle UI to fully appear. This test ensures that
         * exactly two commands are sent: one to press the configured interact key and
         * one to wait approximately 1 second for the puzzle to load.
         */
        private void _testExecuteInteraction()
        {
            var workflow = _fixture();
            _maplestoryBotConfiguration = new MaplestoryBotConfiguration
            {
                RuneDetection = new RuneDetection(),
                RuneInteractKey = "meow"
            };
            workflow.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            workflow.Inject(SystemInjectType.ConfigurationUpdate, _maplestoryBotConfiguration);
            var result = workflow.ExecuteInteraction();
            Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 2);
            Debug.Assert(_macroCommandsExecutor.ExecuteCallArg_macroCommands[0].Count == 1);
            Debug.Assert(_macroCommandsExecutor.ExecuteCallArg_macroCommands[0][0] == "key press {meow} {50} {100}");
            Debug.Assert(_macroCommandsExecutor.ExecuteCallArg_macroCommands[1].Count == 1);
            Debug.Assert(_macroCommandsExecutor.ExecuteCallArg_macroCommands[1][0] == "wait {800} {900}");
            Debug.Assert(result);
        }

        /**
         * @brief Verifies that arrow detection only succeeds when the solver returns
         * exactly four prediction elements
         * 
         * A rune puzzle always has exactly four arrows to solve. If the rune detection
         * service returns any number other than four predictions, the workflow should
         * treat it as a failure. This test ensures the detection logic validates the
         * count and only returns a JsonDocument when exactly four elements are present.
         */
        private void _testExecuteArrowDetectionPredictionsForFourElementJson()
        {
            var callResults = new[] {
                "[{},{},{},{},{}]", "[{},{},{},{}]", "[{},{},{}]", "[{},{}]", "[{}]", "[]", ""
            };
            var expecteds = new[] {
                false, true, false, false, false, false, false
            };
            for (int i = 0; i < callResults.Count(); i++)
            {
                var runeDetection = new RuneDetection();
                var callResult = callResults[i];
                var workflow = _fixture();
                workflow.Inject(0, _solveImage);
                _runeSolverCaller.CallReturn.Add(callResult);
                var result = workflow.ExecuteArrowDetection(runeDetection);
                if (expecteds[i])
                {
                    Debug.Assert(result != null);
                    Debug.Assert(result.RootElement.GetArrayLength() == 4);
                }
                else
                {
                    Debug.Assert(result == null);
                }
            }
        }

        /**
         * @brief Verifies that the arrow detection method correctly forwards the rune
         * detection settings and captured puzzle image to the solver caller
         * 
         * When the puzzle image is captured, the workflow must pass both the image and
         * the rune detection configuration (IP address, port, route) to the solver caller
         * so it can send the correct HTTP request. This test ensures the caller receives
         * the exact same rune detection object and image that were injected into the
         * workflow.
         */
        private void _testExecuteArrowDetectionWithSettingsAndImage()
        {
            var runeDetection = new RuneDetection();
            var workflow = _fixture();
            workflow.Inject(0, _solveImage);
            _runeSolverCaller.CallReturn.Add("");
            var result = workflow.ExecuteArrowDetection(runeDetection);
            Debug.Assert(_runeSolverCaller.CallReturn.Count == 1);
            Debug.Assert(_runeSolverCaller.CallCallArg_runeDetection[0] == runeDetection);
            Debug.Assert(_runeSolverCaller.CallCallArg_image[0] == _solveImage);
        }

        /**
         * @brief Verifies that the arrow sequence executor correctly sends arrow key presses
         * and wait commands for each prediction in the order they appear
         * 
         * When the rune puzzle image has been analyzed and predictions are returned,
         * the workflow must send the corresponding arrow keys to MapleStory in the exact
         * order of the predictions (left to right by X-coordinate). For each arrow detected,
         * the workflow sends a key press command with the appropriate virtual key code
         * (ARROW_LEFT, ARROW_UP, ARROW_RIGHT, ARROW_DOWN), followed by a short wait of
         * 100-150ms between presses to ensure the game registers each input correctly
         * before the next arrow is sent.
         */
        private void _testExecuteArrowSequence()
        {
            var runeDetection = new RuneDetection
            {
                ClassTag = "Class",
                Left = "left",
                Up = "up",
                Right = "right",
                Down = "down"
            };
            var predictions = _predictions();
            var workflow = _fixture();
            workflow.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = workflow.ExecuteArrowSequence(runeDetection, predictions);
            Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 8);
            var commands = _macroCommandsExecutor.ExecuteCallArg_macroCommands;
            foreach (var command in commands)
            {
                Debug.Assert(command.Count == 1);
            }
            Debug.Assert(commands[0][0] == "key press {ARROW_LEFT} {100} {150}");
            Debug.Assert(commands[1][0] == "wait {200} {250}");
            Debug.Assert(commands[2][0] == "key press {ARROW_UP} {100} {150}");
            Debug.Assert(commands[3][0] == "wait {200} {250}");
            Debug.Assert(commands[4][0] == "key press {ARROW_RIGHT} {100} {150}");
            Debug.Assert(commands[5][0] == "wait {200} {250}");
            Debug.Assert(commands[6][0] == "key press {ARROW_DOWN} {100} {150}");
            Debug.Assert(commands[7][0] == "wait {200} {250}");
            Debug.Assert(result);
        }

        /**
         * @brief Verifies that the arrow sequence execution fails gracefully when the
         * JSON property name for the arrow class does not match what the model returned
         * 
         * When the rune detection service returns predictions, it uses a configurable
         * property name to identify which JSON field contains the arrow direction
         * (e.g., "Class", "label", "direction"). If the model returns a different property
         * name than what is configured, the workflow will be unable to extract the arrow
         * class from the prediction.
         */
        private void _testExecuteArrowSequenceWithBadClassTag()
        {
            var runeDetection = new RuneDetection
            {
                ClassTag = "meow",
                Left = "left",
                Up = "up",
                Right = "right",
                Down = "down"
            };
            var predictions = _predictions();
            var workflow = _fixture();
            workflow.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = workflow.ExecuteArrowSequence(runeDetection, predictions);
            Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 0);
            Debug.Assert(!result);
        }

        /**
         * @brief Verifies that the arrow sequence execution fails completely when an
         * arrow class value does not match any configured mapping
         * 
         * When the rune detection service returns predictions, each prediction must
         * contain an arrow direction value that corresponds to one of the configured
         * mappings (left, up, right, down). If any prediction contains an unrecognized
         * value (e.g., "meow" instead of "up"), the entire arrow sequence should be
         * aborted without sending any keystrokes.
         */
        private void _testExecuteArrowSequenceWithBadArrowValue()
        {
            var runeDetection = new RuneDetection
            {
                ClassTag = "Class",
                Left = "left",
                Up = "meow",
                Right = "right",
                Down = "down"
            };
            var predictions = _predictions();
            var workflow = _fixture();
            workflow.Inject(SystemInjectType.KeystrokeTransmitter, _keystrokeTransmitter);
            var result = workflow.ExecuteArrowSequence(runeDetection, predictions);
            Debug.Assert(_macroCommandsExecutor.ExecuteCalls == 0);
            Debug.Assert(!result);
        }

        public void Run()
        {
            _testValidatePrerequisites();
            _testValidatePrerequisitesFailsWhenInteractKeyEmpty();
            _testValidatePrerequisitesFailsWhenTransmitterNotInjected();
            _testExecuteInteraction();
            _testExecuteArrowDetectionPredictionsForFourElementJson();
            _testExecuteArrowDetectionWithSettingsAndImage();
            _testExecuteArrowSequence();
            _testExecuteArrowSequenceWithBadClassTag();
            _testExecuteArrowSequenceWithBadArrowValue();
        }
    }


    public class SolvingExecutorThreadHelperTests
    {
        private MockRuneSolverWorkflow _runeSolverWorkflow = new MockRuneSolverWorkflow();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)SolvingExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Solving
            )
        );

        private AbstractKeystrokeTransmitterThreadHelper _fixture()
        {
            _runeSolverWorkflow = new MockRuneSolverWorkflow();
            _threadState = new KeystrokeTransmitterThreadState(
                (int)SolvingExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Solving
            );
            return new SolvingExecutorThreadHelper(
                _runeSolverWorkflow, _threadState
            );
        }

        /**
         * @brief Verifies that the transmit method returns true when all steps of the
         * rune solving process succeed (validation, interaction, detection, arrow sequence)
         * 
         * When the rune puzzle solver successfully completes all stages of solving a rune,
         * the transmit method should return true to indicate successful processing.
         */
        private void _testTransmitReturn()
        {
            var runeDetection = new RuneDetection();
            var predictions = JsonDocument.Parse("[{}, {}, {}, {}]");
            var helper = _fixture();
            var runeSolverWorkflowRef = new TestUtilities().Reference(_runeSolverWorkflow);
            var callOrder = _runeSolverWorkflow.CallOrder;
            _runeSolverWorkflow.ValidatePrerequisitesReturn.Add(runeDetection);
            _runeSolverWorkflow.ExecuteInteractionReturn.Add(true);
            _runeSolverWorkflow.ExecuteArrowDetectionReturn.Add(predictions);
            _runeSolverWorkflow.ExecuteArrowSequenceReturn.Add(true);
            Debug.Assert(helper.Transmit());
        }

        /**
         * @brief Verifies that the workflow methods are called in the correct order during
         * the rune solving process
         * 
         * When solving a rune puzzle, the steps must execute in a specific sequence:
         * first validate prerequisites, then execute the interaction key, then detect arrows,
         * and finally execute the arrow sequence. This test ensures the solving executor
         * calls the workflow in the proper order.
         */
        private void _testTransmitCallOrder()
        {
            var runeDetection = new RuneDetection();
            var predictions = JsonDocument.Parse("[{}, {}, {}, {}]");
            var helper = _fixture();
            var runeSolverWorkflowRef = new TestUtilities().Reference(_runeSolverWorkflow);
            var callOrder = _runeSolverWorkflow.CallOrder;
            _runeSolverWorkflow.ValidatePrerequisitesReturn.Add(runeDetection);
            _runeSolverWorkflow.ExecuteInteractionReturn.Add(true);
            _runeSolverWorkflow.ExecuteArrowDetectionReturn.Add(predictions);
            _runeSolverWorkflow.ExecuteArrowSequenceReturn.Add(true);
            helper.Transmit();
            Debug.Assert(callOrder.Count == 4);
            Debug.Assert(callOrder[0] == runeSolverWorkflowRef + "ValidatePrerequisites");
            Debug.Assert(callOrder[1] == runeSolverWorkflowRef + "ExecuteInteraction");
            Debug.Assert(callOrder[2] == runeSolverWorkflowRef + "ExecuteArrowDetection");
            Debug.Assert(callOrder[3] == runeSolverWorkflowRef + "ExecuteArrowSequence");
        }

        /**
         * @brief Verifies that the thread state is set to Solved when all rune solving
         * steps complete successfully
         * 
         * When the rune puzzle is successfully solved, the executor thread must update
         * its state to Solved. This informs the orchestrator that the puzzle is complete
         * and the system can transition to the next state.
         */
        private void _testTransmitStateSolved()
        {
            var runeDetection = new RuneDetection();
            var predictions = JsonDocument.Parse("[{}, {}, {}, {}]");
            var helper = _fixture();
            _runeSolverWorkflow.ValidatePrerequisitesReturn.Add(runeDetection);
            _runeSolverWorkflow.ExecuteInteractionReturn.Add(true);
            _runeSolverWorkflow.ExecuteArrowDetectionReturn.Add(predictions);
            _runeSolverWorkflow.ExecuteArrowSequenceReturn.Add(true);
            helper.Transmit();
            Debug.Assert(_threadState.GetState() == (int)SolvingExecutorThreadedUpdate.Solved);
        }

        /**
         * @brief Verifies that the rune detection settings are properly passed to the
         * arrow detection workflow method
         * 
         * When detecting arrow directions from the captured rune image, the workflow
         * needs the rune detection configuration (IP address, port, route, class tags)
         * to send the correct API request. This test ensures the configuration is
         * forwarded.
         */
        private void _testParametersUsedToExecuteArrowDetection()
        {
            var runeDetection = new RuneDetection();
            var predictions = JsonDocument.Parse("[{}, {}, {}, {}]");
            var helper = _fixture();
            _runeSolverWorkflow.ValidatePrerequisitesReturn.Add(runeDetection);
            _runeSolverWorkflow.ExecuteInteractionReturn.Add(true);
            _runeSolverWorkflow.ExecuteArrowDetectionReturn.Add(predictions);
            _runeSolverWorkflow.ExecuteArrowSequenceReturn.Add(true);
            helper.Transmit();
            Debug.Assert(_runeSolverWorkflow.ExecuteArrowDetectionCallArg_runeDetection[0] == runeDetection);
        }

        /**
         * @brief Verifies that the predicted arrow data is properly passed to the arrow
         * sequence execution method
         * 
         * After detecting the arrow directions from the puzzle image, the predictions
         * must be passed to the arrow sequence executor to send the corresponding keystrokes.
         * This test ensures the JSON document containing predictions is forwarded correctly.
         */
        private void _testParametersUsedToExecuteArrowSequence()
        {
            var runeDetection = new RuneDetection();
            var predictions = JsonDocument.Parse("[{}, {}, {}, {}]");
            var helper = _fixture();
            _runeSolverWorkflow.ValidatePrerequisitesReturn.Add(runeDetection);
            _runeSolverWorkflow.ExecuteInteractionReturn.Add(true);
            _runeSolverWorkflow.ExecuteArrowDetectionReturn.Add(predictions);
            _runeSolverWorkflow.ExecuteArrowSequenceReturn.Add(true);
            helper.Transmit();
            Debug.Assert(_runeSolverWorkflow.ExecuteArrowSequenceCallArg_predictions[0] == predictions);
            Debug.Assert(_runeSolverWorkflow.ExecuteArrowSequenceCallArg_runeDetection[0] == runeDetection);
        }

        /**
         * @brief Verifies that the solving process fails gracefully when prerequisites
         * validation fails (e.g., missing configuration or dependencies)
         * 
         * If the rune solver is missing required configuration (missing interact key,
         * no keystroke transmitter, etc.), the prerequisites validation will fail. The
         * system should mark the attempt as failed rather than crashing or proceeding.
         */
        private void _testTransmitFailureOnFailedPrerequisites()
        {
            var predictions = JsonDocument.Parse("[{}, {}, {}, {}]");
            var helper = _fixture();
            _runeSolverWorkflow.ValidatePrerequisitesReturn.Add(null);
            _runeSolverWorkflow.ExecuteInteractionReturn.Add(true);
            _runeSolverWorkflow.ExecuteArrowDetectionReturn.Add(predictions);
            _runeSolverWorkflow.ExecuteArrowSequenceReturn.Add(true);
            helper.Transmit();
            Debug.Assert(_threadState.GetState() == (int)SolvingExecutorThreadedUpdate.Failed);
        }

        /**
         * @brief Verifies that the solving process fails gracefully when the interaction
         * key execution fails
         * 
         * If pressing the interact key to activate the rune puzzle fails (e.g., invalid
         * key mapping or transmission error), the system should abort the solve attempt.
         * This prevents the solver from trying to detect arrows on a puzzle that wasn't
         * properly activated.
         */
        private void _testTransmitFailureOnFailedInteraction()
        {
            var runeDetection = new RuneDetection();
            var predictions = JsonDocument.Parse("[{}, {}, {}, {}]");
            var helper = _fixture();
            _runeSolverWorkflow.ValidatePrerequisitesReturn.Add(runeDetection);
            _runeSolverWorkflow.ExecuteInteractionReturn.Add(false);
            _runeSolverWorkflow.ExecuteArrowDetectionReturn.Add(predictions);
            _runeSolverWorkflow.ExecuteArrowSequenceReturn.Add(true);
            helper.Transmit();
            Debug.Assert(_threadState.GetState() == (int)SolvingExecutorThreadedUpdate.Failed);
        }

        /**
         * @brief Verifies that the solving process fails gracefully when arrow detection
         * fails or returns invalid data
         * 
         * If the rune detection API call fails, returns invalid JSON, or returns an
         * incorrect number of predictions (not exactly 4), the arrow detection step will
         * return null. The system should mark the attempt as failed without sending any
         * arrow keystrokes.
         */
        private void _testTransmitFailureOnFailedDetection()
        {
            var runeDetection = new RuneDetection();
            var helper = _fixture();
            _runeSolverWorkflow.ValidatePrerequisitesReturn.Add(runeDetection);
            _runeSolverWorkflow.ExecuteInteractionReturn.Add(true);
            _runeSolverWorkflow.ExecuteArrowDetectionReturn.Add(null);
            _runeSolverWorkflow.ExecuteArrowSequenceReturn.Add(true);
            helper.Transmit();
            Debug.Assert(_threadState.GetState() == (int)SolvingExecutorThreadedUpdate.Failed);
        }

        /**
         * @brief Verifies that the solving process fails gracefully when executing the
         * arrow keystroke sequence fails
         * 
         * If sending the arrow keystrokes to solve the puzzle fails (e.g., invalid arrow
         * mapping or transmission error), the system should mark the solve attempt as
         * failed. This ensures the orchestrator knows the puzzle was not successfully
         * solved.
         */
        private void _testTransmitFailureOnFailedArrowSequence()
        {
            var runeDetection = new RuneDetection();
            var predictions = JsonDocument.Parse("[{}, {}, {}, {}]");
            var helper = _fixture();
            _runeSolverWorkflow.ValidatePrerequisitesReturn.Add(runeDetection);
            _runeSolverWorkflow.ExecuteInteractionReturn.Add(true);
            _runeSolverWorkflow.ExecuteArrowDetectionReturn.Add(predictions);
            _runeSolverWorkflow.ExecuteArrowSequenceReturn.Add(false);
            helper.Transmit();
            Debug.Assert(_threadState.GetState() == (int)SolvingExecutorThreadedUpdate.Failed);
        }

        /**
         * @brief Verifies that configuration and dependency updates are forwarded to the
         * underlying workflow
         * 
         * When the solving executor receives a configuration update (e.g., new rune
         * detection settings or interact key) or a dependency injection (e.g., keystroke
         * transmitter), it must forward these updates to the workflow to keep it in sync.
         */
        private void _testInjectToRuneSolverWorkflow()
        {
            var helper = _fixture();
            helper.Inject(123, 234);
            Debug.Assert(_runeSolverWorkflow.InjectCalls == 1);
            Debug.Assert((int)_runeSolverWorkflow.InjectCallArg_dataType[0] == 123);
            Debug.Assert((int)_runeSolverWorkflow.InjectCallArg_data[0]! == 234);
        }

        public void Run()
        {
            _testTransmitReturn();
            _testTransmitCallOrder();
            _testTransmitStateSolved();
            _testParametersUsedToExecuteArrowDetection();
            _testParametersUsedToExecuteArrowSequence();
            _testTransmitFailureOnFailedPrerequisites();
            _testTransmitFailureOnFailedInteraction();
            _testTransmitFailureOnFailedDetection();
            _testTransmitFailureOnFailedArrowSequence();
            _testInjectToRuneSolverWorkflow();
        }
    }


    public class SolvingExecutorThreadTests
    {
        private MockKeystrokeTransmitterThreadHelper _executorThreadHelper = new();

        private MockResetEvent _executionEvent = new MockResetEvent();

        private MockRunningState _transmittingState = new MockRunningState();

        private MockRunningState _runningState = new MockRunningState();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)SolvingExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Solving
            )
        );

        private MockInjectAction _injectAction = new MockInjectAction();

        private List<string> _callOrder = [];

        private string _threadStateRef = "";

        private string _transmittingStateRef = "";

        private string _executionEventRef = "";

        private string _executorThreadHelperRef = "";

        private void _setupNewFixture(
            AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _executorThreadHelper = new MockKeystrokeTransmitterThreadHelper();
            _executionEvent = new MockResetEvent();
            _transmittingState = new MockRunningState();
            _runningState = new MockRunningState();
            _threadState = threadState;
            _injectAction = new MockInjectAction();
            _callOrder = [];
        }

        private void _setupCallOrder()
        {
            if (_threadState is MockKeystrokeTransmitterThreadState mockThreadState)
            {
                mockThreadState.CallOrder = _callOrder;
            }
            _executionEvent.CallOrder = _callOrder;
            _injectAction.CallOrder = _callOrder;
            _transmittingState.CallOrder = _callOrder;
        }

        private void _setupRunningState()
        {
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
        }

        private void _setupTransmit(int transmitCount)
        {
            for (int i = 0; i < transmitCount - 1; i++)
            {
                _executorThreadHelper.TransmitReturn.Add(true);
            }
            _executorThreadHelper.TransmitReturn.Add(false);
        }

        private List<Action> _stopLambdas(AbstractThread abstractThread)
        {
            return [
                () =>
                {
                    abstractThread.Inject(
                        SolvingOrchestratorThreadInjectType.Stop, null
                    );
                },
                () =>
                {
                    abstractThread.Stop();
                }
            ];
        }

        private void _setupReferences()
        {
            _threadStateRef = new TestUtilities().Reference(_threadState);
            _transmittingStateRef = new TestUtilities().Reference(_transmittingState); ;
            _executionEventRef = new TestUtilities().Reference(_executionEvent);
            _executorThreadHelperRef = new TestUtilities().Reference(_executorThreadHelper);
        }

        private AbstractThread _fixture(
            int transmitCount, AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _setupNewFixture(threadState);
            _setupCallOrder();
            _setupRunningState();
            _setupTransmit(transmitCount);
            _setupReferences();
            return new SolvingExecutorThread(
                _executionEvent,
                _executorThreadHelper,
                _threadState,
                _transmittingState,
                _runningState
            );
        }

        /**
         * @brief Verifies the handshake sequence when the rune puzzle solver starts its key-press
         * transmission routine
         * 
         * When the macro system detects that the character has reached a rune and needs to solve the
         * puzzle (e.g., typing arrow keys in a specific sequence), the solving orchestrator signals the
         * executor to start sending keystrokes. The executor performs a coordinated startup handshake
         * with the keystroke transmitter to ensure the transmitter is ready before any puzzle-solving
         * keystrokes are sent to MapleStory.
         */
        private void _testExecutorStartingHandshake()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                for (int j = 0; j < i; j++)
                {
                    _transmittingState.IsRunningReturn.Add(true);
                }
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(SolvingOrchestratorThreadInjectType.Start, 0);
                Debug.Assert(_callOrder.Count == (i + 4));
                Debug.Assert(_callOrder[0] == _threadStateRef + "SetState");
                for (int j = 1; j <= i + 1; j++)
                {
                    Debug.Assert(_callOrder[j] == _transmittingStateRef + "IsRunning");
                }
                Debug.Assert(_callOrder[i + 2] == _threadStateRef + "SetState");
                Debug.Assert(_callOrder[i + 3] == _executionEventRef + "Set");
            }
        }

        /**
         * @brief Verifies thread state changes correctly when the rune puzzle solver begins execution
         * 
         * When the macro system initiates rune puzzle solving, the executor thread transitions through
         * proper states: Starting -> Started. This ensures the rest of the system knows the solver is
         * actively processing the rune puzzle and can coordinate other activities accordingly.
         */
        private void _testExecutorStartingHandshakeSetsThreadStates()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            _transmittingState.IsRunningReturn.Add(false);
            keystrokeTransmitterExecutorThread.Inject(
                SolvingOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(threadState.SetStateCallArg_state[0] == (int)SolvingExecutorThreadedUpdate.Starting);
            Debug.Assert(threadState.SetStateCallArg_state[1] == (int)SolvingExecutorThreadedUpdate.Started);
        }

        /**
         * @brief Verifies the handshake sequence when the rune puzzle solver stops its keystroke
         * routine
         * 
         * When the rune puzzle is successfully solved (or if solving fails and needs to abort), the
         * orchestrator signals the executor to stop sending puzzle-solving keystrokes. The executor
         * performs a coordinated shutdown handshake to ensure keystrokes stop cleanly before the
         * routine exits, preventing stray inputs during the transition back to navigation or botting.
         */
        private void _testExecutorStoppingHandshake()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                for (int j = 0; j < i; j++)
                {
                    _transmittingState.IsRunningReturn.Add(true);
                }
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(SolvingOrchestratorThreadInjectType.Stop, 0);
                Debug.Assert(_callOrder.Count == (i + 3));
                Debug.Assert(_callOrder[0] == _threadStateRef + "SetState");
                for (int j = 1; j <= i + 1; j++)
                {
                    Debug.Assert(_callOrder[j] == _transmittingStateRef + "IsRunning");
                }
                Debug.Assert(_callOrder[i + 2] == _threadStateRef + "SetState");
            }
        }


        /**
         * @brief Verifies thread state changes correctly when the rune puzzle solver stops
         * 
         * When rune puzzle solving completes or is interrupted, the executor thread transitions through
         * proper shutdown states: Started → Stopping → Stopped. This ensures the system accurately
         * reflects that puzzle-solving is no longer active and other routines (like monster killing)
         * can safely resume.
         */
        private void _testExecutorStoppingHandshakeSetsThreadStates()
        {
            for (int i = 0; i < 2; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                _transmittingState.IsRunningReturn.Add(false);
                var stopLambdas = _stopLambdas(keystrokeTransmitterExecutorThread);
                stopLambdas[i]();
                Debug.Assert(threadState.SetStateCallArg_state[0] == (int)SolvingExecutorThreadedUpdate.Stopping);
                Debug.Assert(threadState.SetStateCallArg_state[1] == (int)SolvingExecutorThreadedUpdate.Stopped);
            }
        }

        /**
         * @brief Verifies the solver continuously processes and executes puzzle keystrokes while active
         * 
         * When the macro system is actively solving a rune puzzle, the executor thread must continuously
         * process the puzzle solution sequence and send the appropriate keystrokes to MapleStory
         * (e.g., left, up, right, down arrows) until the puzzle is solved or interrupted. This test
         * ensures that once started, the thread repeatedly calls the transmit method to send puzzle
         * inputs without stopping until completion.
         */
        private void _testExecutorThreadLoopTransmitsWhenStarted()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)SolvingExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Solving
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var start = SolvingOrchestratorThreadInjectType.Start;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(start, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == i);
            }
        }

        /**
         * @brief Verifies puzzle keystrokes stop immediately when solving is interrupted or completed
         * 
         * When the rune puzzle is solved or when the system needs to abort solving (e.g., due to an
         * error or user intervention), the executor thread must stop sending puzzle keystrokes.
         */
        private void _testExecutorThreadLoopDoesntTransmitWhenStopped()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)SolvingExecutorThreadedUpdate.Started,
                    KeystrokeTransmitterThreadType.Solving
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var stop = SolvingOrchestratorThreadInjectType.Stop;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(stop, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == 0);
            }
        }

        /**
         * @brief Verifies the solver thread helper is reset before and after each puzzle solve check
         * 
         * When the rune puzzle solver evaluates the current puzzle state to determine which arrow key
         * to press next, the thread helper must be reset to a clean state before calculating the next
         * keystroke. This prevents stale puzzle state data from previous steps from affecting the
         * current decision.
         */
        private void _testExecutorThreadLoopResetsBeforeAndAfterTransmit()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)SolvingExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Solving
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var callOrder = _executorThreadHelper.CallOrder;
                _transmittingState.IsRunningReturn.Add(false);
                var start = SolvingOrchestratorThreadInjectType.Start;
                keystrokeTransmitterExecutorThread.Inject(start, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(callOrder.Count == i + 2);
                Debug.Assert(callOrder[0] == _executorThreadHelperRef + "Reset");
                for (int j = 1; j <= i; j++)
                {
                    Debug.Assert(callOrder[j] == _executorThreadHelperRef + "Transmit");
                }
                Debug.Assert(callOrder[i + 1] == _executorThreadHelperRef + "Reset");
            }
        }

        public void Run()
        {
            _testExecutorStartingHandshake();
            _testExecutorStartingHandshakeSetsThreadStates();
            _testExecutorStoppingHandshake();
            _testExecutorStoppingHandshakeSetsThreadStates();
            _testExecutorThreadLoopTransmitsWhenStarted();
            _testExecutorThreadLoopDoesntTransmitWhenStopped();
            _testExecutorThreadLoopResetsBeforeAndAfterTransmit();
        }
    }


    public class SolvingOrchestratorThreadTests
    {
        private AbstractKeystrokeTransmitterThreadState _threadState = new KeystrokeTransmitterThreadState(
            0, KeystrokeTransmitterThreadType.Solving
        );

        private MockThread _thread = new MockThread(new ThreadRunningState());

        private MockRunningState _runningState = new MockRunningState();

        private BlockingCollection<int> _threadStates = new BlockingCollection<int>();

        private string _threadRef = "";

        private List<string> _callOrder = [];

        private AbstractThread _fixture(AbstractKeystrokeTransmitterThreadState threadState)
        {
            _threadState = threadState;
            _thread = new MockThread(new ThreadRunningState());
            _runningState = new MockRunningState();
            _callOrder = [];
            _thread.CallOrder = _callOrder;
            _threadStates = new BlockingCollection<int>();
            if (_threadState is MockKeystrokeTransmitterThreadState mockThreadState)
            {
                mockThreadState.CallOrder = _callOrder;
            }
            _threadRef = new TestUtilities().Reference(_thread);
            return new SolvingOrchestratorThread(
                _thread,
                _runningState,
                _threadStates
            );
        }

        private void _setTransmitFixture(int transmitCount)
        {
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(true);
            for (int j = 0; j < transmitCount; j++)
            {
                _runningState.IsRunningReturn.Add(true);
            }
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(false);
            for (int j = 0; j < transmitCount + 1; j++)
            {
                _threadStates.Add(j);
            }
        }

        /**
         * @brief Verifies that starting the orchestrator launches the executor thread
         * 
         * When users start their automation, the orchestrator should launch the
         * executor thread that actually runs the macros. This test ensures that
         * starting the orchestrator properly kicks off the executor.
         */
        private void _testStartingOrchestratorStartsExecutorThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.Solving
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(false);
            transmitterOrchestratorThread.Start();
            Debug.Assert(_thread.ThreadStartCalls == 1);
        }

        /**
         * @brief Verifies that stopping the orchestrator shuts down the executor thread
         * 
         * When users stop their automation, the orchestrator should cleanly shut
         * down the executor thread. This test ensures the shutdown sequence works
         * properly, including the handshake that confirms the thread has stopped.
         */
        private void _testStoppingOrchestratorStopsExecutorThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.Solving
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            _runningState.IsRunningReturn.Add(true);
            _thread.CallOrder = _callOrder;
            Debug.Assert(_threadStates.Count == 0);
            transmitterOrchestratorThread.Stop();
            Debug.Assert(_threadStates.Count == 1);
            Debug.Assert(_callOrder.Count == 1);
            Debug.Assert(_callOrder[0] == _threadRef + "ThreadStop");

        }

        /**
         * @brief Verifies that injected commands update the thread state
         * 
         * When the system sends commands to the orchestrator, the thread state
         * should update to reflect what it should be doing (starting, stopping,
         * running, etc.). This test ensures the orchestrator correctly tracks
         * its current operational state.
         */
        private void _testInjectingOrchestratorCommandAssignsThreadState()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                123, KeystrokeTransmitterThreadType.Solving
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            var max = SolvingOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                transmitterOrchestratorThread.Inject(
                    (SolvingOrchestratorThreadInjectType)i, 0
                );
                Debug.Assert(_threadStates.Count == 1);
                Debug.Assert(_threadStates.Take() == i);
            }
        }

        /**
         * @brief Confirms the orchestrator properly acknowledges commands
         * 
         * When commands are sent to the orchestrator, it should acknowledge them
         * by updating its state and signaling that the command was received.
         * This test ensures the orchestrator properly handles the command.
         */
        private void _testInjectOrchestratorCommand()
        {
            var max = SolvingOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var transmitterOrchestratorThread = _fixture(threadState);
                transmitterOrchestratorThread.Inject((SolvingOrchestratorThreadInjectType)i, 0);
                Debug.Assert(_threadStates.Count == 1);
                Debug.Assert(_threadStates.Take() == i);
            }
        }

        /**
         * @brief Verifies that data is forwarded to the executor thread
         * 
         * When the orchestrator receives data (like macro commands or configuration),
         * it should forward that data to the executor thread that will process it.
         * This test ensures the orchestrator correctly passes data along.
         */
        private void _testInjectToExecutorThread()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var transmitterOrchestratorThread = _fixture(threadState);
            transmitterOrchestratorThread.Inject(123, 456);
            Debug.Assert(_thread.InjectCalls == 1);
            Debug.Assert((int)_thread.InjectCallArg_dataType[0] == 123);
            Debug.Assert((int)_thread.InjectCallArg_data[0]! == 456);
        }

        /**
         * @brief Verifies that the orchestrator makes itself available as a thread dependency
         * 
         * When other systems in the application need to communicate with or control
         * the orchestrator thread, they need a reference to it. This test ensures
         * that when an InjectAction is received, the orchestrator properly registers
         * itself as a thread dependency that other components can discover and use.
         */
        private void _testInjectActionToExecutorThread()
        {
            var getActionDataType = new List<object>();
            var getActionData = new List<object>();
            var injectAction = new MockInjectAction();
            injectAction.GetActionReturn.Add(
                (object dataType, object data) =>
                {
                    getActionDataType.Add(dataType);
                    getActionData.Add(data);
                }
            );
            var threadState = new MockKeystrokeTransmitterThreadState();
            var transmitterOrchestratorThread = _fixture(threadState);
            transmitterOrchestratorThread.Inject(SystemInjectType.InjectAction, injectAction);
            Debug.Assert(_thread.InjectCalls == 1);
            Debug.Assert((int)_thread.InjectCallArg_dataType[0] == (int)SystemInjectType.InjectAction);
            Debug.Assert(_thread.InjectCallArg_data[0] == injectAction);
            Debug.Assert(injectAction.GetActionCalls == 1);
            Debug.Assert(getActionDataType.Count == 1);
            Debug.Assert((int)getActionDataType[0] == (int)SystemInjectType.ThreadDependency);
            Debug.Assert(getActionData[0] == transmitterOrchestratorThread);
        }

        /**
         * @brief Verifies the orchestrator's main processing loop
         * 
         * The orchestrator runs a main loop that coordinates all activities:
         * waiting for commands, updating state, and managing the executor.
         * This test ensures the loop properly sequences all these activities.
         */
        private void _testThreadLoopInjectsCommands()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    123, KeystrokeTransmitterThreadType.Solving
                );
                var transmitterOrchestratorThread = _fixture(threadState);
                _setTransmitFixture(i);
                transmitterOrchestratorThread.Start();
                transmitterOrchestratorThread.Join(10000);
                Debug.Assert(_callOrder.Count == i + 1);
                Debug.Assert(_callOrder[0] == _threadRef + "ThreadStart");
                for (int j = 1; j <= i; j++)
                {
                    Debug.Assert(_callOrder[j] == _threadRef + "ThreadInject");
                    Debug.Assert((int)_thread.InjectCallArg_dataType[j - 1]! == j - 1);
                }
            }
        }

        public void Run()
        {
            _testStartingOrchestratorStartsExecutorThread();
            _testStoppingOrchestratorStopsExecutorThread();
            _testInjectingOrchestratorCommandAssignsThreadState();
            _testInjectOrchestratorCommand();
            _testInjectToExecutorThread();
            _testInjectActionToExecutorThread();
            _testThreadLoopInjectsCommands();
        }
    }


    public class SolvingTransmitterTestSuite
    {
        public void Run()
        {
            new RuneSolverCallerTests().Run();
            new SolvingScreenCaptureSubscriberTests().Run();
            new RuneSolverWorkflowTests().Run();
            new SolvingExecutorThreadHelperTests().Run();
            new SolvingOrchestratorThreadTests().Run();
            new SolvingExecutorThreadTests().Run();
        }
    }
}
