


using MaplestoryBotNet.Systems.Configuration;
using System.Diagnostics;
using System.Text.Json;

namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{
    public class ConfigurationSkillsFixture
    {
        public static string Serialized()
        {
            return """
            {
                "skills": [
                    {
                        "name": "",
                        "min_delay": 123,
                        "max_delay": 234,
                        "macros": [
                            "12",
                            "23",
                            "34"
                        ],
                        "active": 345
                    },
                    {
                        "name": "",
                        "min_delay": 234,
                        "max_delay": 345,
                        "macros": [
                            "23",
                            "34",
                            "45"
                        ],
                        "active": 456
                    },
                    {
                        "name": "",
                        "min_delay": 345,
                        "max_delay": 456,
                        "macros": [
                            "34",
                            "45",
                            "56"
                        ],
                        "active": 567
                    }
                ]
            }
            """;
        }

        public static ConfigurationSkills Deserialized()
        {
            return new ConfigurationSkills
            {
                Skills = [
                    new ConfigurationSkill
                    {
                        MinDelay = 123,
                        MaxDelay = 234,
                        Macros = ["12", "23", "34"],
                        Active = 345
                    },
                    new ConfigurationSkill
                    {
                        MinDelay = 234,
                        MaxDelay = 345,
                        Macros = ["23", "34", "45"],
                        Active = 456
                    },
                    new ConfigurationSkill
                    {
                        MinDelay = 345,
                        MaxDelay = 456,
                        Macros = ["34", "45", "56"],
                        Active = 567
                    }
                ]
            };
        }
    }


    public class SkillsSerializerTests
    {
        /**
         * @brief Tests that the bot correctly saves skill configurations to JSON format
         * 
         * Validates that when the bot saves skill configuration data (including skill macros,
         * cooldown delays, and active states) to a file, the output JSON matches the expected
         * format. This ensures that any skill configurations created or modified through
         * the bot's interface can be properly persisted and later reloaded without data loss
         * or corruption.
         */
        private void _testSerializeSkills()
        {
            var serializer = new SkillsSerializer();
            var deserialized = ConfigurationSkillsFixture.Deserialized();
            var expected = ConfigurationSkillsFixture.Serialized();
            var result = serializer.Serialize(deserialized);
            var normalizer = new JsonNormalizer();
            Debug.Assert(normalizer.Normalize(result) == normalizer.Normalize(expected));
        }

        public void Run()
        {
            _testSerializeSkills();
        }
    }


    public class SkillsDeserializerTests
    {
        /**
         * @brief Tests that the bot correctly loads skill configurations from JSON files
         * 
         * Validates that when the bot reads skill configuration data from a saved JSON file,
         * it correctly reconstructs all skill properties including macro sequences, delay
         * ranges, and active states. This ensures that skill configurations created by the
         * user are faithfully restored when configurations are reloaded.
         */
        private void _testDeserializeSkills()
        {
            var deserializer = new SkillsDeserializer();
            var deserialized = ConfigurationSkillsFixture.Deserialized();
            var serialized = ConfigurationSkillsFixture.Serialized();
            var result = JsonSerializer.Serialize(deserializer.Deserialize(serialized));
            var expected = JsonSerializer.Serialize(deserialized);
            var normalizer = new JsonNormalizer();
            Debug.Assert(result != "");
            Debug.Assert(normalizer.Normalize(result) == normalizer.Normalize(expected));
        }

        public void Run()
        {
            _testDeserializeSkills();
        }
    }


    public class ConfigurationSkillsTestSuite
    {
        public void Run()
        {
            new SkillsSerializerTests().Run();
            new SkillsDeserializerTests().Run();
        }
    }
}
