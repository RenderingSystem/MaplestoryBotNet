using MaplestoryBotNet.Systems.Configuration.SubSystems;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{

    /**
     * @brief Configuration Map Model Deserializer Test Suite
     * 
     * Validates the complete configuration loading system for game automation setups.
     * 
     * @details
     * This test suite thoroughly validates the deserialization of complex JSON configuration
     * files that define game automation parameters. These configurations include map boundaries,
     * interaction points, automation macros with probabilistic execution, and nested command
     * sequences.
     */
    public class ConfigurationMapModelDeserializerTests
    {
        /**
         * @brief Provides a sample configuration JSON for testing
         * 
         * Creates a comprehensive JSON configuration structure that simulates
         * a real game map configuration setup.
         * 
         * This fixture represents a complete map configuration with a defined
         * map area  and two map points. Each point includes coordinates,
         * interaction ranges, and nested command structures with
         * multiple levels of automation macros.
         */
        private string _fixture()
        {
            return """
            {
                "map_area_left": 123,
                "map_area_top": 234,
                "map_area_right": 345,
                "map_area_bottom": 456,
                "map_points": [
                    {
                        "x": 12,
                        "y": 23,
                        "x_range": 34,
                        "y_range": 45,
                        "point_data": {
                            "element_name": "E1",
                            "point_name": "P1",
                            "commands": [
                                {
                                    "macro_name": "M1",
                                    "macro_chance": 12,
                                    "macro_commands": [
                                        "C10", "C11", "C12"
                                    ]
                                },
                                {
                                    "macro_name": "M2",
                                    "macro_chance": 23,
                                    "macro_commands": [
                                        "C20", "C21", "C22"
                                    ]
                                }
                            ]
                        }
                        },
                    {
                        "x": 23,
                        "y": 34,
                        "x_range": 45,
                        "y_range": 56,
                        "point_data": {
                            "element_name": "E2",
                            "point_name": "P2",
                            "commands": [
                                {
                                    "macro_name": "M3",
                                    "macro_chance": 23,
                                    "macro_commands": [
                                        "C30", "C31", "C32"
                                    ]
                                },
                                {
                                    "macro_name": "M4",
                                    "macro_chance": 34,
                                    "macro_commands": [
                                        "C40", "C41", "C42"
                                    ]
                                }
                            ]
                        }
                    }
                ]
            }
            """;
        }


        /**
         * @brief Tests boundary coordinate deserialization
         * 
         * @test
         * Validates that the deserializer correctly extracts the rectangular
         * map boundaries from configuration data.
         * 
         * @details
         * This test ensures that when users define their game map area with
         * specific pixel coordinates (left, top, right, bottom), the system
         * accurately loads these boundaries. Correct boundary parsing is
         * essential for the automation system to know exactly which screen
         * region represents the minimap area.
         */
        private void _testDeserializerDeserializesMapArea()
        {
            var deserializer = new ConfigurationMapModelDeserializer();
            var deserialized = deserializer.DeserializeMapModel(_fixture());
            Debug.Assert(deserialized.MapAreaLeft == 123);
            Debug.Assert(deserialized.MapAreaTop == 234);
            Debug.Assert(deserialized.MapAreaRight == 345);
            Debug.Assert(deserialized.MapAreaBottom == 456);
        }

        /**
         * @brief Tests coordinate point deserialization
         * 
         * @test
         * Verifies that individual interaction points with their coordinate
         * ranges are correctly loaded from configuration.
         * 
         * This test validates that specific points of interest within the
         * minim map are accurately parsed with their X/Y coordinates
         * and interaction tolerance ranges. Proper loading ensures the
         * automation system can reliably target these points during gameplay,
         */
        private void _testDeserializerDeserializesMapPoints()
        {
            var deserializer = new ConfigurationMapModelDeserializer();
            var deserialized = deserializer.DeserializeMapModel(_fixture());
            Debug.Assert(deserialized.MapPoints.Count == 2);
            Debug.Assert(deserialized.MapPoints[0].X == 12);
            Debug.Assert(deserialized.MapPoints[0].Y == 23);
            Debug.Assert(deserialized.MapPoints[0].XRange == 34);
            Debug.Assert(deserialized.MapPoints[0].YRange == 45);
            Debug.Assert(deserialized.MapPoints[1].X == 23);
            Debug.Assert(deserialized.MapPoints[1].Y == 34);
            Debug.Assert(deserialized.MapPoints[1].XRange == 45);
            Debug.Assert(deserialized.MapPoints[1].YRange == 56);
        }

        /**
         * @brief Tests element identification deserialization
         * 
         * @test
         * Validates that point names and element identifiers are correctly
         * extracted from configuration data.
         * 
         * This test ensures that descriptive names assigned to map points
         * are properly loaded, enabling clear identification during automation.
         * These human-readable identifiers appear in user interfaces
         * helping users understand which game elements the system is
         * interacting with at any given moment during automated sessions.
         */
        private void _testDeserializerDeserializesPointData()
        {
            var deserializer = new ConfigurationMapModelDeserializer();
            var deserialized = deserializer.DeserializeMapModel(_fixture());
            Debug.Assert(deserialized.MapPoints[0].PointData.ElementName == "E1");
            Debug.Assert(deserialized.MapPoints[0].PointData.PointName == "P1");
            Debug.Assert(deserialized.MapPoints[1].PointData.ElementName == "E2");
            Debug.Assert(deserialized.MapPoints[1].PointData.PointName == "P2");
        }

        /**
         * @brief Tests macro configuration deserialization
         * 
         * @test
         * Verifies that macro definitions with their execution probabilities
         * are correctly loaded from configuration.
         * 
         * @details
         * This test validates the loading of named automation macros and their
         * associated execution chance percentages. These settings allow users
         * to create complex, probabilistic behavior patterns - for example,
         * making an automation sequence that only executes 12% of the time
         * to simulate human-like randomness.
         */
        private void _testDeserializerDeserializesCommandData()
        {
            var deserializer = new ConfigurationMapModelDeserializer();
            var deserialized = deserializer.DeserializeMapModel(_fixture());
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands.Count == 2);
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[0].MacroName == "M1");
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[0].MacroChance == 12);
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[1].MacroName == "M2");
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[1].MacroChance == 23);
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[0].MacroName == "M3");
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[0].MacroChance == 23);
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[1].MacroName == "M4");
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[1].MacroChance == 34);
        }


        /**
         * @brief Tests command sequence deserialization
         * 
         * @test
         * Validates that nested command sequences within macros are correctly
         * loaded from configuration.
         * 
         * @details
         * This test ensures that the actual automation commands (like
         * keyboard presses, mouse clicks, or delays) are properly extracted
         * in their correct execution order. Each macro can contain multiple
         * sequential commands that execute in a specific order when triggered.
         * Accurate deserialization guarantees that complex automation routines
         * - such as "press A, wait 100ms, press B, move mouse to X,Y" - are
         * loaded exactly as configured.
         */
        private void _testDeserializerDeserializesCommands()
        {
            var deserializer = new ConfigurationMapModelDeserializer();
            var deserialized = deserializer.DeserializeMapModel(_fixture());
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[0].MacroCommands.Count == 3);
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[0].MacroCommands[0] == "C10");
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[0].MacroCommands[1] == "C11");
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[0].MacroCommands[2] == "C12");
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[1].MacroCommands.Count == 3);
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[1].MacroCommands[0] == "C20");
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[1].MacroCommands[1] == "C21");
            Debug.Assert(deserialized.MapPoints[0].PointData.Commands[1].MacroCommands[2] == "C22");
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[0].MacroCommands.Count == 3);
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[0].MacroCommands[0] == "C30");
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[0].MacroCommands[1] == "C31");
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[0].MacroCommands[2] == "C32");
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[1].MacroCommands.Count == 3);
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[1].MacroCommands[0] == "C40");
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[1].MacroCommands[1] == "C41");
            Debug.Assert(deserialized.MapPoints[1].PointData.Commands[1].MacroCommands[2] == "C42");
        }

        /**
         * @brief Executes the complete configuration deserialization test suite
         * 
         * @test
         * Runs all deserialization tests to validate the entire configuration
         * loading pipeline.
         * 
         * @details
         * This comprehensive test sequence validates that game automation
         * configurations load correctly from JSON format, ensuring users'
         * carefully crafted automation setups work exactly as designed.
         */
        public void Run()
        {
            _testDeserializerDeserializesMapArea();
            _testDeserializerDeserializesMapPoints();
            _testDeserializerDeserializesPointData();
            _testDeserializerDeserializesCommandData();
            _testDeserializerDeserializesCommands();
        }
    }


    /**
     * @brief Configuration Map Model Serializer Test Suite
     * 
     * @class ConfigurationMapModelSerializerTests
     * @test
     * Validates the complete configuration saving system for game automation setups.
     * 
     * @details
     * Successful serialization ensures that users' automation investments are preserved
     * across sessions, allowing for continuous improvement and refinement of automation
     * strategies without losing previously configured work.
     */
    public class ConfigurationMapModelSerializerTests
    {

        /**
         * @brief Creates a comprehensive configuration model for testing
         * 
         * @test
         * Builds a complete ConfigurationMapModel instance representing a typical
         * game automation setup with all configuration layers populated.
         * 
         * @details
         * The test data represents realistic automation scenarios where users have
         * configured the system to interact with specific game elements ("E1", "E2")
         * using probabilistic command sequences that create natural, varied gameplay.
         */
        private ConfigurationMapModel _fixture()
        {
            return new ConfigurationMapModel
            {
                MapAreaLeft = 123,
                MapAreaTop = 234,
                MapAreaRight = 345,
                MapAreaBottom = 456,
                MapPoints = [
                    new ConfigurationMinimapPoint{
                        X = 12,
                        Y = 23,
                        XRange = 34,
                        YRange = 45,
                        PointData = new ConfigurationPointData{
                            ElementName = "E1",
                            PointName = "P1",
                            Commands = [
                                new ConfigurationPointMacros
                                {
                                    MacroChance = 12,
                                    MacroName = "M1",
                                    MacroCommands = ["C10", "C11", "C12"]
                                },
                                new ConfigurationPointMacros
                                {
                                    MacroChance = 23,
                                    MacroName = "M2",
                                    MacroCommands = ["C20", "C21", "C22"]
                                }
                            ]
                        }
                    },
                    new ConfigurationMinimapPoint{
                        X = 23,
                        Y = 34,
                        XRange = 45,
                        YRange = 56,
                        PointData = new ConfigurationPointData{
                            ElementName = "E2",
                            PointName = "P2",
                            Commands = [
                                new ConfigurationPointMacros
                                {
                                    MacroChance = 23,
                                    MacroName = "M3",
                                    MacroCommands = ["C30", "C31", "C32"]
                                },
                                new ConfigurationPointMacros
                                {
                                    MacroChance = 34,
                                    MacroName = "M4",
                                    MacroCommands = ["C40", "C41", "C42"]
                                }
                            ]
                        }
                    }
                ]
            };
        }

        /**
         * @brief Provides the expected JSON output for serialization validation
         * 
         * @test
         * Defines the exact JSON structure that should result from serializing
         * the test configuration model.
         * 
         * @details
         * This expected output ensures that serialization produces user-friendly
         * JSON files that are both machine-readable and human-editable, allowing
         * users to manually modify configurations in text editors if desired.
         */
        private string _expected()
        {
            return """
            {
                "map_area_left": 123,
                "map_area_top": 234,
                "map_area_right": 345,
                "map_area_bottom": 456,
                "map_points": [
                    {
                        "x": 12,
                        "y": 23,
                        "x_range": 34,
                        "y_range": 45,
                        "point_data": {
                            "element_name": "E1",
                            "point_name": "P1",
                            "commands": [
                                {
                                    "macro_name": "M1",
                                    "macro_chance": 12,
                                    "macro_commands": [
                                        "C10", "C11", "C12"
                                    ]
                                },
                                {
                                    "macro_name": "M2",
                                    "macro_chance": 23,
                                    "macro_commands": [
                                        "C20", "C21", "C22"
                                    ]
                                }
                            ]
                        }
                        },
                    {
                        "x": 23,
                        "y": 34,
                        "x_range": 45,
                        "y_range": 56,
                        "point_data": {
                            "element_name": "E2",
                            "point_name": "P2",
                            "commands": [
                                {
                                    "macro_name": "M3",
                                    "macro_chance": 23,
                                    "macro_commands": [
                                        "C30", "C31", "C32"
                                    ]
                                },
                                {
                                    "macro_name": "M4",
                                    "macro_chance": 34,
                                    "macro_commands": [
                                        "C40", "C41", "C42"
                                    ]
                                }
                            ]
                        }
                    }
                ]
            }
            """;
        }

        /**
         * @brief Tests complete configuration model serialization
         * 
         * @test
         * Validates that the serializer correctly converts a ConfigurationMapModel
         * instance to its JSON representation.
         * 
         * @details
         * Successful serialization guarantees that users can save their
         * automation configurations with confidence, knowing they can be
         * reloaded exactly as saved for future gameplay sessions.
         */
        private void _testSerializeMapModel()
        {
            var serializer = new ConfigurationMapModelSerializer();
            var serialized = serializer.Serialize(_fixture());
            var normalizer = new JsonNormalizer();
            Debug.Assert(normalizer.Normalize(serialized) == normalizer.Normalize(_expected()));
        }

        /**
         * @brief Executes the complete configuration serialization test suite
         * 
         * @test
         * Runs the serialization test to validate the configuration saving pipeline.
         * 
         * @details
         * This test execution ensures that the entire serialization process functions
         * correctly, providing users with reliable configuration persistence. When this
         * test passes, users can trust that their automation setups will be saved
         * accurately, preserving all their custom settings, optimizations, and
         * gameplay strategies for consistent performance across multiple gaming sessions.
         */
        public void Run()
        {
            _testSerializeMapModel();
        }
    }


    public class ConfigurationMapModelTestSutie
    {
        public void Run()
        {
            new ConfigurationMapModelDeserializerTests().Run();
            new ConfigurationMapModelSerializerTests().Run();
        }
    }
}
