using MaplestoryRuneSolver.RuneSolving;
using System.Diagnostics;
using System.Text;
using System.Text.Json;


namespace MaplestoryRuneSolverTests.RuneSolving.Tests
{

    public class RuneSolvingAgentTests
    {
        private string _expectedResult()
        {
            return """
            [
                {
                    "X": 544,
                    "Y": 244,
                    "Class":"up"
                },
                {
                    "X": 638,
                    "Y": 239,
                    "Class":"down"
                },
                {
                    "X": 729,
                    "Y": 242,
                    "Class": "right"
                },
                {
                    "X": 820,
                    "Y": 244,
                    "Class":"left"
                }
            ]
            """;
        }

        private string _normalize(string json)
        {
            using var doc = JsonDocument.Parse(json);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });
            doc.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        /**
         * @brief Verifies that the rune solving agent correctly processes a test image
         * and returns the expected predictions sorted by X coordinate
         * 
         * When users capture a rune puzzle image and send it to the solve endpoint,
         * the rune solving agent must detect all arrow markers in the image and return
         * their positions and directions. This test ensures the model loads successfully,
         * processes a standard test image, and returns predictions that match the expected
         * results. The predictions are sorted by X coordinate (left to right) to provide
         * consistent ordering for the bot to execute the arrow sequence in the correct
         * order when solving the rune puzzle.
         */
        private void _testSolve()
        {
            var runeSolvingAgent = new RuneSolvingAgentFacade();
            Debug.Assert(runeSolvingAgent.LoadModel("best.onnx"));
            byte[] imageBytes = File.ReadAllBytes("test_image.png");
            string base64Image = Convert.ToBase64String(imageBytes);
            string result = runeSolvingAgent.Solve(base64Image);
            Debug.Assert(_normalize(result) == _normalize(_expectedResult()));
        }

        /**
         * @brief Verifies that the rune solving agent gracefully handles missing model files
         * 
         * When users attempt to load a rune solving model from a file path that does not
         * exist, the system must fail gracefully without crashing. This prevents the bot
         * from starting up with a missing model and ensures the user receives appropriate
         * feedback about the missing dependency.
         */
        private void _testLoadingModelFailsIfFileNotFound()
        {
            var runeSolvingAgent = new RuneSolvingAgentFacade();
            Debug.Assert(!runeSolvingAgent.LoadModel("pinhead larry"));
        }

        /**
         * @brief Verifies that the rune solving agent rejects invalid model files
         * 
         * When users attempt to load a file that is not a valid ONNX model (e.g., an image file
         * or any other non-model file), the system must detect the invalid format and fail
         * gracefully. This prevents unexpected behavior that could crash the rune solving
         * service.
         */
        private void _testLoadingModelFailsIfInvalidFile()
        {
            var runeSolvingAgent = new RuneSolvingAgentFacade();
            Debug.Assert(!runeSolvingAgent.LoadModel("test_image.png"));
        }

        public void Run()
        {
            _testSolve();
            _testLoadingModelFailsIfFileNotFound();
            _testLoadingModelFailsIfInvalidFile();
        }
    }


    public class RuneSolvingAgentTestSuite
    {
        public void Run()
        {
            new RuneSolvingAgentTests().Run();
        }
    }
}
