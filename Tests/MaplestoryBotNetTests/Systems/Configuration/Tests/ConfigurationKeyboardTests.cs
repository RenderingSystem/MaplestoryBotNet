using System.Diagnostics;
using MaplestoryBotNet.Systems.Configuration;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{
    internal class KeyboardMappingDesrializerTest {
        private KeyboardMapping _expected()
        {
            return new KeyboardMapping
            {
                Characters = new Dictionary<string, string>
                {
                    { "character_1", "0x12 0x23" },
                    { "character_2", "0x23 0x34" },
                    { "character_3", "0x34 0x45" }
                },
                Specials = new Dictionary<string, string>
                {
                    { "special_1", "0x45 0x56" },
                    { "special_2", "0x56 0x67" },
                    { "special_3", "0x67 0x78" }
                },
                Numpad = new Dictionary<string, string>
                {
                    { "numpad_1", "0x23 0x34" },
                    { "numpad_2", "0x34 0x45" },
                    { "numpad_3", "0x45 0x56" }
                },
                Functions = new Dictionary<string, string>
                {
                    { "function_1", "0x34 0x45" },
                    { "function_2", "0x45 0x56" },
                    { "function_3", "0x56 0x67" }
                }
            };
        }

        private string _fixture()
        {
            return """
            {
                "characters": {
                    "character_1": "0x12 0x23",
                    "character_2": "0x23 0x34",
                    "character_3": "0x34 0x45"
                },
                "specials": {
                    "special_1": "0x45 0x56",
                    "special_2": "0x56 0x67",
                    "special_3": "0x67 0x78"
                },
                "numpad": {
                    "numpad_1": "0x23 0x34",
                    "numpad_2": "0x34 0x45",
                    "numpad_3": "0x45 0x56"
                },
                "functions": {
                    "function_1": "0x34 0x45",
                    "function_2": "0x45 0x56",
                    "function_3": "0x56 0x67"
                }
            }
            """;
        }

        private void _testDeserializerDeserializesKeyboardMapping()
        {
            var deserializer = new KeyboardMappingDeserializer();
            var result = (KeyboardMapping) deserializer.Deserialize(_fixture());
            var expected = _expected();
            foreach (var kvp in expected.Characters)
                Debug.Assert(result.Characters[kvp.Key] == kvp.Value);
            foreach (var kvp in expected.Specials)
                Debug.Assert(result.Specials[kvp.Key] == kvp.Value);
            foreach (var kvp in expected.Numpad)
                Debug.Assert(result.Numpad[kvp.Key] == kvp.Value);
            foreach (var kvp in expected.Functions)
                Debug.Assert(result.Functions[kvp.Key] == kvp.Value);
        }

        public void Run()
        {
            _testDeserializerDeserializesKeyboardMapping();
        }
    }


    public class ConfigurationKeyboardTestSuite
    {
        public void Run()
        {
            new KeyboardMappingDesrializerTest().Run();
        }
    }
}
