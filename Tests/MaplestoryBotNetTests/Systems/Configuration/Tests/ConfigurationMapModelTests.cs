using MaplestoryBotNet.Systems.Configuration.SubSystems;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{
    public class ConfigurationMapModelDeserializerTests
    {
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

        private void _testDeserializerDeserializesMapArea()
        {
            var deserializer = new ConfigurationMapModelDeserializer();
            var deserialized = deserializer.DeserializeMapModel(_fixture());
            Debug.Assert(deserialized.MapAreaLeft == 123);
            Debug.Assert(deserialized.MapAreaTop == 234);
            Debug.Assert(deserialized.MapAreaRight == 345);
            Debug.Assert(deserialized.MapAreaBottom == 456);
        }

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

        private void _testDeserializerDeserializesPointData()
        {
            var deserializer = new ConfigurationMapModelDeserializer();
            var deserialized = deserializer.DeserializeMapModel(_fixture());
            Debug.Assert(deserialized.MapPoints[0].PointData.ElementName == "E1");
            Debug.Assert(deserialized.MapPoints[0].PointData.PointName == "P1");
            Debug.Assert(deserialized.MapPoints[1].PointData.ElementName == "E2");
            Debug.Assert(deserialized.MapPoints[1].PointData.PointName == "P2");
        }

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

        public void Run()
        {
            _testDeserializerDeserializesMapArea();
            _testDeserializerDeserializesMapPoints();
            _testDeserializerDeserializesPointData();
            _testDeserializerDeserializesCommandData();
            _testDeserializerDeserializesCommands();
        }
    }


    public class ConfigurationMapModelSerializerTests
    {
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

        private void _testSerializeMapModel()
        {
            var serializer = new ConfigurationMapModelSerializer();
            var serialized = serializer.Serialize(_fixture());
            var normalizer = new JsonNormalizer();
            Debug.Assert(normalizer.Normalize(serialized) == normalizer.Normalize(_expected()));
        }

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
