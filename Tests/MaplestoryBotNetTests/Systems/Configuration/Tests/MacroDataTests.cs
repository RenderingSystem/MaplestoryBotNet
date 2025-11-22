using MaplestoryBotNet.Systems.Configuration.SubSystems;
using System.Diagnostics;
using System.Text.Json;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{
    /**
     * @class MacroDataDeserializerTests
     * 
     * @brief Unit tests for macro command data deserialization
     * 
     * This test class validates that macro command sequences are correctly parsed
     * from JSON format into executable command objects, ensuring that complex
     * automation sequences will be properly loaded and executed by the bot.
     */
    public class MacroDataDeserializerTests
    {
        /**
         * @brief Provides sample macro command data for testing
         * 
         * @return JSON string containing a sequence of macro commands
         * 
         * This fixture represents a typical macro configuration with multiple
         * sequential commands that the bot should execute in order during gameplay.
         */
        private string _fixture()
        {
            return """{ "macro": [ "a", "b", "c", "d", "e" ] }""";
        }

        /**
         * @brief Tests correct parsing of macro command sequences
         * 
         * Validates that macro commands are properly extracted from JSON format
         * and preserved in the correct order, ensuring that complex automation
         * sequences will execute exactly as configured by the user.
         */
        private void _testDeserializeMacroData()
        {
            var deserializer = new MacroDataDeserializer();
            var deserialized = deserializer.Deserialize(_fixture()) as MacroData;
            Debug.Assert(deserialized != null);
            Debug.Assert(deserialized.Macro.Count() == 5);
            Debug.Assert(deserialized.Macro[0] == "a");
            Debug.Assert(deserialized.Macro[1] == "b");
            Debug.Assert(deserialized.Macro[2] == "c");
            Debug.Assert(deserialized.Macro[3] == "d");
            Debug.Assert(deserialized.Macro[4] == "e");
        }

        /**
         * @brief Executes macro deserialization validation tests
         */
        public void Run()
        {
            _testDeserializeMacroData();
        }
    }


    /**
     * @class MacroDataSerializerTests
     * 
     * @brief Unit tests for macro command data serialization
     * 
     * This test class validates that macro command sequences are correctly
     * converted to JSON format for storage and configuration sharing,
     * ensuring that user-created automation sequences can be saved and reloaded.
     */
    public class MacroDataSerializerTests
    {
        /**
         * @brief Provides sample macro command object for testing
         * 
         * @return MacroData instance with predefined command sequence
         * 
         * This fixture creates a macro command object with a sequential
         * command pattern that the bot should serialize to JSON format.
         */
        private MacroData _fixture()
        {
            return new MacroData { Macro = ["a", "b", "c", "d", "e"] };
        }

        /**
         * @brief Tests correct conversion of macro commands to JSON format
         * 
         * Validates that macro command objects are properly converted to
         * standardized JSON format, ensuring that user configurations can be
         * reliably saved to disk and shared between different bot instances.
         */
        private void _testSerializeMacroData()
        {
            var serializer = new MacroDataSerializer();
            var serialized = serializer.Serialize(_fixture());
            var expected = """{ "macro": [ "a", "b", "c", "d", "e" ] }""";
            Debug.Assert(_normalize(serialized) == _normalize(expected));
        }

        /**
         * @brief Normalizes JSON strings for consistent comparison
         * 
         * @param json JSON string to normalize
         * 
         * @return Normalized JSON string with consistent formatting
         * 
         * This helper ensures that JSON comparisons focus on content rather than
         * formatting differences, providing reliable test results regardless of
         * whitespace or indentation variations.
         */
        private string _normalize(string json)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IndentCharacter = ' ',
                IndentSize = 0
            };
            var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, options);
        }

        /**
         * @brief Executes macro serialization validation tests
         */
        public void Run()
        {
            _testSerializeMacroData();
        }
    }


    public class MacroDataTestSuite
    {
        public void Run()
        {
            new MacroDataDeserializerTests().Run();
            new MacroDataSerializerTests().Run();
        }
    }
}
