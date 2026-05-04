using MaplestoryBotNet.Systems.Configuration.SubSystems;
using System.Diagnostics;
using System.Text.Json.Serialization;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{

    public class ConfigurationFixture
    {
        public static string StringFixture()
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
                                        "C10",
                                        "C11",
                                        "C12"
                                    ]
                                },
                                {
                                    "macro_name": "M2",
                                    "macro_chance": 23,
                                    "macro_commands": [
                                        "C20",
                                        "C21",
                                        "C22"
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
                                        "C30",
                                        "C31",
                                        "C32"
                                    ]
                                },
                                {
                                    "macro_name": "M4",
                                    "macro_chance": 34,
                                    "macro_commands": [
                                        "C40",
                                        "C41",
                                        "C42"
                                    ]
                                }
                            ]
                        }
                    }
                ],
                "rune_frames": [
                    {
                        "x": 12,
                        "y": 23,
                        "width": 34,
                        "height": 45,
                        "rune_frame_data": {
                            "element_name": "E3",
                            "frame_name": "F3",
                            "frame_macros": [
                                {
                                    "macro_name": "M4",
                                    "element_name": "E4",
                                    "x": 23,
                                    "y": 34,
                                    "scale_x": 45,
                                    "scale_y": 56,
                                    "next_rune_frame": "NF4",
                                    "radius": 67,
                                    "commands": [
                                        "C50",
                                        "C51",
                                        "C52"
                                    ]
                                },
                                {
                                    "macro_name": "M5",
                                    "element_name": "E5",
                                    "x": 34,
                                    "y": 45,
                                    "scale_x": 56,
                                    "scale_y": 67,
                                    "next_rune_frame": "NF5",
                                    "radius": 78,
                                    "commands": [
                                        "C60",
                                        "C61",
                                        "C62"
                                    ]
                                }
                            ],
                            "frame_directions": [
                                {
                                    "direction_name": "D4",
                                    "direction": 0,
                                    "distance": 123,
                                    "commands": [
                                        "C70",
                                        "C71",
                                        "C72"
                                    ]
                                },
                                {
                                    "direction_name": "D5",
                                    "direction": 1,
                                    "distance": 234,
                                    "commands": [
                                        "C80",
                                        "C81",
                                        "C82"
                                    ]
                                }
                            ]
                        }
                    },
                    {
                        "x": 23,
                        "y": 34,
                        "width": 45,
                        "height": 56,
                        "rune_frame_data": {
                            "element_name": "E4",
                            "frame_name": "F4",
                            "frame_macros": [
                                {
                                    "macro_name": "M6",
                                    "element_name": "E6",
                                    "x": 34,
                                    "y": 45,
                                    "scale_x": 56,
                                    "scale_y": 67,
                                    "next_rune_frame": "NF6",
                                    "radius": 78,
                                    "commands": [
                                        "C90",
                                        "C91",
                                        "C92"
                                    ]
                                },
                                {
                                    "macro_name": "M7",
                                    "element_name": "E7",
                                    "x": 45,
                                    "y": 56,
                                    "scale_x": 67,
                                    "scale_y": 78,
                                    "next_rune_frame": "NF7",
                                    "radius": 89,
                                    "commands": [
                                        "C100",
                                        "C101",
                                        "C102"
                                    ]
                                }
                            ],
                            "frame_directions": [
                                {
                                    "direction_name": "D6",
                                    "direction": 1,
                                    "distance": 234,
                                    "commands": [
                                        "C110",
                                        "C111",
                                        "C112"
                                    ]
                                },
                                {
                                    "direction_name": "D7",
                                    "direction": 2,
                                    "distance": 345,
                                    "commands": [
                                        "C120",
                                        "C121",
                                        "C122"
                                    ]
                                }
                            ]
                        }
                    }
                ],
                "character_threshold": 0.234,
                "rune_threshold": 0.123,
                "rune_cooldown": 987,
                "rune_activation": 123,
                "rune_radius": 234,
                "uniform_movement": 345
            }
            """;
        }

        public static AbstractConfiguration ObjectFixture()
        {
            return new ConfigurationBottingModel
            {
                MapAreaLeft = 123,
                MapAreaTop = 234,
                MapAreaRight = 345,
                MapAreaBottom = 456,
                MapPoints = [
                    new ConfigurationMinimapPoint
                    {
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
                    new ConfigurationMinimapPoint
                    {
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
                ],
                RuneFrames = [
                    new ConfigurationRuneFrame
                    {
                        X = 12,
                        Y = 23,
                        Width = 34,
                        Height = 45,
                        RuneFrameData = new ConfigurationRuneFrameData
                        {
                            ElementName = "E3",
                            FrameName = "F3",
                            RuneFrameMacros = [
                                new ConfigurationRuneFrameMacro
                                {
                                    MacroName = "M4",
                                    ElementName = "E4",
                                    X = 23,
                                    Y = 34,
                                    ScaleX = 45,
                                    ScaleY = 56,
                                    Radius = 67,
                                    NextRuneFrame = "NF4",
                                    PointCommands = ["C50", "C51", "C52"]
                                },
                                new ConfigurationRuneFrameMacro
                                {
                                    MacroName = "M5",
                                    ElementName = "E5",
                                    X = 34,
                                    Y = 45,
                                    ScaleX = 56,
                                    ScaleY = 67,
                                    Radius = 78,
                                    NextRuneFrame = "NF5",
                                    PointCommands = ["C60", "C61", "C62"]
                                }
                            ],
                            RuneFrameDirections = [
                                new ConfigurationRuneFrameDirection
                                {
                                    DirectionName = "D4",
                                    Direction = 0,
                                    Distance = 123,
                                    DirectionCommands = ["C70", "C71", "C72"]
                                },
                                new ConfigurationRuneFrameDirection
                                {
                                    DirectionName = "D5",
                                    Direction = 1,
                                    Distance = 234,
                                    DirectionCommands = ["C80", "C81", "C82"]
                                }
                            ]
                        }
                    },
                    new ConfigurationRuneFrame
                    {
                        X = 23,
                        Y = 34,
                        Width = 45,
                        Height = 56,
                        RuneFrameData = new ConfigurationRuneFrameData
                        {
                            ElementName = "E4",
                            FrameName = "F4",
                            RuneFrameMacros = [
                                new ConfigurationRuneFrameMacro
                                {
                                    MacroName = "M6",
                                    ElementName = "E6",
                                    X = 34,
                                    Y = 45,
                                    ScaleX = 56,
                                    ScaleY = 67,
                                    Radius = 78,
                                    NextRuneFrame = "NF6",
                                    PointCommands = ["C90", "C91", "C92"]
                                },
                                new ConfigurationRuneFrameMacro
                                {
                                    MacroName = "M7",
                                    ElementName = "E7",
                                    X = 45,
                                    Y = 56,
                                    ScaleX = 67,
                                    ScaleY = 78,
                                    Radius = 89,
                                    NextRuneFrame = "NF7",
                                    PointCommands = ["C100", "C101", "C102"]
                                }
                            ],
                            RuneFrameDirections = [
                                new ConfigurationRuneFrameDirection
                                {
                                    DirectionName = "D6",
                                    Direction = 1,
                                    Distance = 234,
                                    DirectionCommands = ["C110", "C111", "C112"]
                                },
                                new ConfigurationRuneFrameDirection
                                {
                                    DirectionName = "D7",
                                    Direction = 2,
                                    Distance = 345,
                                    DirectionCommands = ["C120", "C121", "C122"]
                                }
                            ]
                        }
                    }
                ],
                CharacterThreshold = 0.234f,
                RuneThreshold = 0.123f,
                RuneCooldown = 987,
                RuneActivation = 123,
                RuneRadius = 234,
                UniformMovement = 345
            };
        }
    }

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
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());
            Debug.Assert(deserialized.MapAreaLeft == 123);
            Debug.Assert(deserialized.MapAreaTop == 234);
            Debug.Assert(deserialized.MapAreaRight == 345);
            Debug.Assert(deserialized.MapAreaBottom == 456);
        }

        /**
         * @brief Tests deserialization of image recognition threshold values
         * 
         * @test Validates that the deserializer correctly extracts confidence threshold
         * values for character and rune detection from configuration data.
         * 
         * These threshold values determine how strictly the bot matches template images
         * when searching for the player character and runes on the minimap. A higher
         * threshold (closer to 1.0) requires a more perfect match, reducing false
         * positives but potentially missing detections. A lower threshold (closer to 0.0)
         * is more tolerant, finding matches more easily but risking false detections.
         */
        private void _testDeserializerDeserializesThresholds()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());
            Debug.Assert(deserialized.CharacterThreshold == 0.234f);
            Debug.Assert(deserialized.RuneThreshold == 0.123f);
        }

        /**
         * @brief Tests coordinate point deserialization
         * 
         * @test
         * Verifies that individual interaction points with their coordinate
         * ranges are correctly loaded from configuration.
         * 
         * This test validates that specific points of interest within the
         * minimap are accurately parsed with their X/Y coordinates
         * and interaction tolerance ranges. Proper loading ensures the
         * automation system can reliably target these points during gameplay,
         */
        private void _testDeserializerDeserializesBottingMapPoints()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());
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
        private void _testDeserializerDeserializesBottingPointData()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());
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
        private void _testDeserializerDeserializesBottingCommandData()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());
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
        private void _testDeserializerDeserializesBottingCommands()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());
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
         * @brief Tests deserialization of rune frame definitions from configuration
         * 
         * @test Validates that frame objects (rectangular regions defining where runes
         * can appear on the game map) are correctly loaded with their position, dimensions,
         * and metadata.
         * 
         * In MapleStory, runes are anti-botting mechanisms that periodically appear on
         * the map. The bot must navigate from its current frame (region) to the frame
         * containing the rune. Rune frames define distinct screen regions or map areas
         * that the bot moves between during rune detection and approach sequences.
         */
        private void _testDeserializerDeserializesRuneingFrames()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());
            Debug.Assert(deserialized.RuneFrames.Count == 2);

            Debug.Assert(deserialized.RuneFrames[0].X == 12);
            Debug.Assert(deserialized.RuneFrames[0].Y == 23);
            Debug.Assert(deserialized.RuneFrames[0].Width == 34);
            Debug.Assert(deserialized.RuneFrames[0].Height == 45);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.FrameName == "F3");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.ElementName == "E3");

            Debug.Assert(deserialized.RuneFrames[1].X == 23);
            Debug.Assert(deserialized.RuneFrames[1].Y == 34);
            Debug.Assert(deserialized.RuneFrames[1].Width == 45);
            Debug.Assert(deserialized.RuneFrames[1].Height == 56);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.FrameName == "F4");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.ElementName == "E4");
        }

        /**
         * @brief Tests deserialization of macro definitions for frame-to-frame movement
         * 
         * @test Validates that macros (movement sequences) associated with rune frame
         * points are correctly loaded with their execution parameters.
         * 
         * When a rune is detected in a different frame (region), the bot must navigate
         * from its current frame to the target frame containing the rune. These macros
         * define the movement path between frames, including coordinates (where to move),
         * scale factors (for positioning accuracy), radius (movement tolerance), and
         * next frame references for chaining multiple frame transitions.
         */
        private void _testDeserializerDeserializesRuneingFrameMacros()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros.Count == 2);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros.Count == 2);

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].MacroName == "M4");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].ElementName == "E4");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].X == 23);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].Y == 34);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].ScaleX == 45);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].ScaleY == 56);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].Radius == 67);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].NextRuneFrame == "NF4");

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].MacroName == "M5");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].ElementName == "E5");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].X == 34);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].Y == 45);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].ScaleX == 56);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].ScaleY == 67);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].Radius == 78);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].NextRuneFrame == "NF5");

            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].MacroName == "M6");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].ElementName == "E6");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].X == 34);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].Y == 45);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].ScaleX == 56);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].ScaleY == 67);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].Radius == 78);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].NextRuneFrame == "NF6");

            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].MacroName == "M7");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].ElementName == "E7");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].X == 45);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].Y == 56);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].ScaleX == 67);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].ScaleY == 78);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].Radius == 89);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].NextRuneFrame == "NF7");
        }

        /**
         * @brief Tests deserialization of directional movement with platform safety limits
         * 
         * @test Validates that direction definitions are correctly loaded for navigating
         * between different frames while respecting platform boundaries.
         * 
         * Directions define how the bot should move to transition from one frame to
         * another. The Distance value specifies the maximum movement distance allowed
         * for a single command, ensuring the bot can move toward the target point
         * without falling off the platform.
         */
        private void _testDeserializerDeserializesRuneingFrameDirections()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections.Count == 2);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections.Count == 2);

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[0].DirectionName == "D4");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[0].Direction == 0);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[0].Distance == 123);

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[1].DirectionName == "D5");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[1].Direction == 1);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[1].Distance == 234);

            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[0].DirectionName == "D6");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[0].Direction == 1);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[0].Distance == 234);

            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[1].DirectionName == "D7");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[1].Direction == 2);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[1].Distance == 345);

        }

        /**
         * @brief Tests deserialization of movement command sequences for frame navigation
         * 
         * @test Validates that the actual automation commands (key presses, movement
         * actions, delays) for moving between frames are correctly loaded in order.
         * 
         * Each macro contains sequential commands that execute the frame-to-frame
         * movement. For example, moving from Frame A to Frame B might require:
         * ["Press Right Arrow", "Wait 500ms", "Press Up Arrow", "Wait 300ms", "Press Jump"].
         * These commands are executed in sequence until the bot reaches the target frame.
         */
        private void _testDeserializerDeserializesRuneingFrameMacroCommands()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].PointCommands.Count == 3);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].PointCommands.Count == 3);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].PointCommands.Count == 3);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].PointCommands.Count == 3);

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].PointCommands[0] == "C50");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].PointCommands[1] == "C51");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[0].PointCommands[2] == "C52");

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].PointCommands[0] == "C60");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].PointCommands[1] == "C61");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameMacros[1].PointCommands[2] == "C62");

            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].PointCommands[0] == "C90");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].PointCommands[1] == "C91");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[0].PointCommands[2] == "C92");

            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].PointCommands[0] == "C100");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].PointCommands[1] == "C101");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameMacros[1].PointCommands[2] == "C102");
        }

        /**
         * @brief Tests deserialization of directional movement commands for point-to-point
         * navigation within a frame
         * 
         * @test Validates that movement commands attached to directional navigation are
         * correctly loaded for moving between points inside the same frame without
         * falling off platforms.
         * 
         * When the bot needs to move from one point to another within the current frame,
         * these commands execute the actual movement sequence. The Distance constraint
         * specifies the maximum safe movement distance for a single macro, ensuring
         * the bot can move toward the target point without walking off platforms.
         */
        private void _testDeserializerDeserializesRuneingFrameDirectionCommands()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[0].DirectionCommands.Count == 3);
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[1].DirectionCommands.Count == 3);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[0].DirectionCommands.Count == 3);
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[1].DirectionCommands.Count == 3);

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[0].DirectionCommands[0] == "C70");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[0].DirectionCommands[1] == "C71");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[0].DirectionCommands[2] == "C72");

            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[1].DirectionCommands[0] == "C80");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[1].DirectionCommands[1] == "C81");
            Debug.Assert(deserialized.RuneFrames[0].RuneFrameData.RuneFrameDirections[1].DirectionCommands[2] == "C82");

            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[0].DirectionCommands[0] == "C110");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[0].DirectionCommands[1] == "C111");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[0].DirectionCommands[2] == "C112");

            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[1].DirectionCommands[0] == "C120");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[1].DirectionCommands[1] == "C121");
            Debug.Assert(deserialized.RuneFrames[1].RuneFrameData.RuneFrameDirections[1].DirectionCommands[2] == "C122");
        }

        /**
         * @brief Tests deserialization of rune activation cooldown and detection radius settings
         * 
         * @test Validates that the deserializer correctly extracts rune automation settings
         * from configuration data.
         * 
         * These settings control how the bot interacts with runes (anti-botting mechanisms)
         * in MapleStory. The RuneActivation value specifies the cooldown time in seconds
         * that the bot must wait after solving a rune before it can activate another rune.
         * The RuneRadius value defines the detection radius around the rune.
         */
        private void _testDeserializerDeserializesRuneingSettings()
        {
            var deserializer = new ConfigurationBottingModelDeserializer();
            var deserialized = deserializer.DeserializeBottingModel(ConfigurationFixture.StringFixture());
            Debug.Assert(deserialized.RuneActivation == 123);
            Debug.Assert(deserialized.RuneRadius == 234);
            Debug.Assert(deserialized.UniformMovement == 345);
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
            _testDeserializerDeserializesThresholds();
            _testDeserializerDeserializesBottingMapPoints();
            _testDeserializerDeserializesBottingPointData();
            _testDeserializerDeserializesBottingCommandData();
            _testDeserializerDeserializesBottingCommands();
            _testDeserializerDeserializesRuneingFrames();
            _testDeserializerDeserializesRuneingFrameMacros();
            _testDeserializerDeserializesRuneingFrameDirections();
            _testDeserializerDeserializesRuneingFrameMacroCommands();
            _testDeserializerDeserializesRuneingFrameDirectionCommands();
            _testDeserializerDeserializesRuneingSettings();
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
            var serialized = serializer.Serialize(ConfigurationFixture.ObjectFixture());
            var expeccted = ConfigurationFixture.StringFixture();
            var normalizer = new JsonNormalizer();
            Debug.Assert(normalizer.Normalize(serialized) == normalizer.Normalize(expeccted));
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


    public class ConfigurationMapModelTestSuite
    {
        public void Run()
        {
            new ConfigurationMapModelDeserializerTests().Run();
            new ConfigurationMapModelSerializerTests().Run();
        }
    }
}
