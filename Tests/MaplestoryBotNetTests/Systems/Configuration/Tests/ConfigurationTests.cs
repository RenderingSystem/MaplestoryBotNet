using System.Diagnostics;
using System.Text.Json;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{
    public class JsonNormalizer
    {
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
        public string Normalize(string json)
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
    }


    /**
    * @class MaplestoryBotConfigurationDeserializerTest
    * 
    * @brief Unit tests for verifying proper parsing of MapleStory bot configuration files
    * 
    * This test class validates that the bot correctly interprets configuration files,
    * ensuring all game automation settings are properly loaded and will work as expected
    * during actual gameplay.
    */
    internal class MaplestoryBotConfigurationDeserializerTest
    {
        /**
         * @brief Provides a standard test configuration for all test methods
         * 
         * @return JSON string containing a complete bot configuration
         * 
         * This fixture represents a typical bot configuration that users would create
         * to automate their MapleStory gameplay, including health/mana monitoring,
         * status ailment responses, and map navigation settings.
         */
        private string _fixture()
        {
            return """
            {
                "process_name": "some_process",
                "hp": {
                    "rect": [ 123, 234, 345, 456 ],
                    "pixel": [234, 345],
                    "rgb": [345, 456, 567],
                    "key": "meow_1",
                    "active": true
                },
                "mp": {
                    "rect": [ 234, 345, 567, 678 ],
                    "pixel": [345, 456],
                    "rgb": [456, 567, 678],
                    "key": "meow_2",
                    "active": false
                },
                "ailments": {
                    "seal": {
                        "active": true,
                        "active_delay": 123,
                        "threshold": 234,
                        "macro_commands": ["123", "234", "345"],
                        "image": "some_image_1",
                        "static_rect": [ 345, 456, 567, 678 ]
                    },
                    "weakness": {
                        "active": false,
                        "active_delay": 234,
                        "threshold": 345,
                        "macro_commands": ["234", "345", "456"],
                        "image": "some_image_2",
                        "static_rect": [ 456, 567, 678, 789 ]
                    }
                },
                "ailments_allcure_key": "some_allcure_key",
                "map_icons": {
                    "character": {
                        "image": "some_image_3"
                    },
                    "rune": {
                        "image": "some_image_4"
                    }
                },
                "macro_directory": "cool_macros",
                "map_directory": "cool_maps"
            }
            """;
        }


        /**
         * @brief Tests correct parsing of the game process name
         * 
         * This test ensures the bot will attach to the correct game window,
         * preventing it from trying to interact with the wrong application
         * or failing to find the game entirely.
         */
        private void _testDeserializeName()
        {
            var deserializer = new MaplestoryBotConfigurationDeserializer();
            var output = (MaplestoryBotConfiguration)deserializer.Deserialize(_fixture());
            Debug.Assert(output.ProcessName == "some_process");
        }

        /**
         * @brief Tests correct parsing of health point monitoring settings
         * 
         * Validates that the bot will properly monitor the player's health status
         * and execute the configured response when health drops below certain thresholds,
         * ensuring the character doesn't unexpectedly die during automation.
         */
        private void _testDeserializeHp() {
            var deserializer = new MaplestoryBotConfigurationDeserializer();
            var output = (MaplestoryBotConfiguration)deserializer.Deserialize(_fixture());
            Debug.Assert(output.Hp.Rect.SequenceEqual([123, 234, 345, 456]));
            Debug.Assert(output.Hp.Pixel.SequenceEqual([234, 345]));
            Debug.Assert(output.Hp.Rgb.SequenceEqual([345, 456, 567]));
            Debug.Assert(output.Hp.Key == "meow_1");
            Debug.Assert(output.Hp.Active == true);
        }

        /**
         * @brief Tests correct parsing of mana point monitoring settings
         * 
         * Validates that the bot will properly monitor the player's mana status
         * and execute the configured response when mana is low, ensuring the
         * character can continue using skills without interruption.
         */
        private void _testDeserializeMp()
        {
            var deserializer = new MaplestoryBotConfigurationDeserializer();
            var output = (MaplestoryBotConfiguration)deserializer.Deserialize(_fixture());
            Debug.Assert(output.Mp.Rect.SequenceEqual([234, 345, 567, 678]));
            Debug.Assert(output.Mp.Pixel.SequenceEqual([345, 456]));
            Debug.Assert(output.Mp.Rgb.SequenceEqual([456, 567, 678]));
            Debug.Assert(output.Mp.Key == "meow_2");
            Debug.Assert(output.Mp.Active == false);
        }

        /**
         * @brief Tests correct parsing of status ailment response settings
         * 
         * Validates that the bot will properly detect and respond to different
         * status ailments (like seal or weakness) with the configured macros
         * and timings, preventing the character from being disabled by debuffs.
         */
        private void _testDeserializeAilments()
        {
            var deserializer = new MaplestoryBotConfigurationDeserializer();
            var output = (MaplestoryBotConfiguration)deserializer.Deserialize(_fixture());
            Debug.Assert(output.Ailments["seal"].Active == true);
            Debug.Assert(output.Ailments["seal"].ActiveDelay == 123);
            Debug.Assert(output.Ailments["seal"].Threshold == 234);
            Debug.Assert(output.Ailments["seal"].MacroCommands.SequenceEqual(["123", "234", "345"]));
            Debug.Assert(output.Ailments["seal"].Image == "some_image_1");
            Debug.Assert(output.Ailments["seal"].StaticRect.SequenceEqual([345, 456, 567, 678]));
            Debug.Assert(output.Ailments["weakness"].Active == false);
            Debug.Assert(output.Ailments["weakness"].ActiveDelay == 234);
            Debug.Assert(output.Ailments["weakness"].Threshold == 345);
            Debug.Assert(output.Ailments["weakness"].MacroCommands.SequenceEqual(["234", "345", "456"]));
            Debug.Assert(output.Ailments["weakness"].Image == "some_image_2");
            Debug.Assert(output.Ailments["weakness"].StaticRect.SequenceEqual([456, 567, 678, 789]));
            Debug.Assert(output.AilmentsAllcureKey == "some_allcure_key");
        }

        /**
         * @brief Tests correct parsing of map icon recognition settings
         * 
         * Validates that the bot will properly recognize important map elements
         * like the player character and runes, enabling accurate navigation
         * and interaction with the game environment.
         */
        private void _testDeserializeMapIcons()
        {
            var deserializer = new MaplestoryBotConfigurationDeserializer();
            var output = (MaplestoryBotConfiguration)deserializer.Deserialize(_fixture());
            Debug.Assert(output.MapIcons["character"].Image == "some_image_3");
            Debug.Assert(output.MapIcons["rune"].Image == "some_image_4");
        }

        /**
         * @brief Tests correct parsing of the macro directory location
         * 
         * Validates that the bot will look for macro files in the correct folder,
         * ensuring that all automated key sequences and complex commands can be
         * properly loaded and executed during gameplay.
         */
        private void _testDeserializeMacroDirectory()
        {
            var deserializer = new MaplestoryBotConfigurationDeserializer();
            var output = (MaplestoryBotConfiguration)deserializer.Deserialize(_fixture());
            Debug.Assert(output.MacroDirectory == "cool_macros");
        }

        /**
         * @brief Tests correct parsing of the map directory location
         * 
         * This test validates that the bot will look for map configuration files
         * in the correct folder, ensuring that all map definitions, waypoints, and
         * navigation data can be properly loaded for automated movement and positioning
         * during gameplay.
         */
        private void _testDeserializeMapDirectory()
        {
            var deserializer = new MaplestoryBotConfigurationDeserializer();
            var output = (MaplestoryBotConfiguration)deserializer.Deserialize(_fixture());
            Debug.Assert(output.MapDirectory == "cool_maps");
        }

        /**
         * @brief Executes all configuration parsing tests
         * 
         * Runs the complete test suite to ensure the bot will correctly
         * interpret all aspects of the configuration file, providing
         * confidence that the automation will work as intended during gameplay.
         */
        public void Run()
        {
            _testDeserializeName();
            _testDeserializeHp();
            _testDeserializeMp();
            _testDeserializeAilments();
            _testDeserializeMapIcons();
            _testDeserializeMacroDirectory();
            _testDeserializeMapDirectory();
        }
    }


    /**
     * @class MaplestoryBotConfigurationSerializerTest
     * 
     * @brief Unit tests for verifying proper serialization of MapleStory bot configuration objects
     * 
     * This test class validates that the bot correctly converts configuration objects into JSON format,
     * ensuring that saved configuration files will be properly formatted and contain all necessary settings
     * for reliable bot operation.
     */
    internal class MaplestoryBotConfigurationSerializerTest
    {
        /**
         * @brief Creates a standard test configuration object for serialization testing
         * 
         * @return MaplestoryBotConfiguration object with all settings populated
         * 
         * This fixture represents a typical bot configuration that users would create
         * to automate their MapleStory gameplay, including health/mana monitoring,
         * status ailment responses, and map navigation settings.
         */
        private MaplestoryBotConfiguration _fixture()
        {
            var configuration = new MaplestoryBotConfiguration
            {
                ProcessName="some_process",
                Hp={
                    Rect=[123, 234, 345, 456],
                    Pixel=[234, 345],
                    Rgb=[345, 456, 567],
                    Key="meow_1",
                    Active=true
                },
                Mp={
                    Rect=[234, 345, 456, 567],
                    Pixel=[345, 456],
                    Rgb=[456, 567, 678],
                    Key="meow_2",
                    Active=false
                },
                Ailments={
                    ["seal"] = new Ailment
                    {
                        Active=true,
                        ActiveDelay=123,
                        Threshold=234,
                        MacroCommands = ["123", "234", "345"],
                        Image="some_image_1",
                        StaticRect = [345, 456, 567, 678]
                    },
                    ["weakness"] = new Ailment
                    {
                        Active=false,
                        ActiveDelay=234,
                        Threshold=345,
                        MacroCommands = ["234", "345", "456"],
                        Image="some_image_2",
                        StaticRect = [456, 567, 678, 789]
                    }
                },
                AilmentsAllcureKey="some_allcure_key",
                MapIcons =
                {
                    ["character"] = new MapIcon
                    {
                        Image="some_image_3"
                    },
                    ["rune"] = new MapIcon
                    {
                        Image="some_image_4"
                    }
                },
                MacroDirectory="cool_macros",
                MapDirectory="cool_maps"

            };
            return configuration;
        }

        /**
         * @brief Provides the expected JSON output for the test configuration
         * 
         * @return String containing the properly formatted JSON configuration
         * 
         * This represents the exact JSON format that users' configuration files
         * should follow, ensuring consistency and reliability when saving and
         * loading bot settings.
         */
        private string _expected()
        {
            return """
            {
                "process_name": "some_process",
                "hp": {
                    "rect": [
                        123,
                        234,
                        345,
                        456
                    ],
                    "pixel": [
                        234,
                        345
                    ],
                    "rgb": [
                        345,
                        456,
                        567
                    ],
                    "key": "meow_1",
                    "active": true
                },
                "mp": {
                    "rect": [
                        234,
                        345,
                        456,
                        567
                    ],
                    "pixel": [
                        345,
                        456
                    ],
                    "rgb": [
                        456,
                        567,
                        678
                    ],
                    "key": "meow_2",
                    "active": false
                },
                "ailments": {
                    "seal": {
                        "active": true,
                        "active_delay": 123,
                        "threshold": 234,
                        "macro_commands": [
                            "123",
                            "234",
                            "345"
                        ],
                        "image": "some_image_1",
                        "static_rect": [
                            345,
                            456,
                            567,
                            678
                        ]
                    },
                    "weakness": {
                        "active": false,
                        "active_delay": 234,
                        "threshold": 345,
                        "macro_commands": [
                            "234",
                            "345",
                            "456"
                        ],
                        "image": "some_image_2",
                        "static_rect": [
                            456,
                            567,
                            678,
                            789
                        ]
                    }
                },
                "ailments_allcure_key": "some_allcure_key",
                "map_icons": {
                    "character": {
                        "image": "some_image_3"
                    },
                    "rune": {
                        "image": "some_image_4"
                    }
                },
                "macro_directory": "cool_macros",
                "map_directory": "cool_maps"
            }
            """;
        }

        /**
         * @brief Tests correct serialization of bot configuration to JSON format
         * 
         * Validates that the bot will properly save configuration settings to JSON files
         * with the correct structure and formatting, ensuring that saved configurations
         * can be reliably loaded back into the bot without errors or data loss.
         */
        private void _testSerialize()
        {
            var output = new MaplestoryBotConfigurationSerializer().Serialize(_fixture());
            var normalized_1 = new JsonNormalizer().Normalize(output);
            var normalized_2 = new JsonNormalizer().Normalize(_expected());
            Debug.Assert(normalized_1 == normalized_2);
        }

        /**
         * @brief Executes the configuration serialization test
         * 
         * Runs the test to ensure the bot will correctly serialize configuration objects
         * into properly formatted JSON, providing confidence that user settings will be
         * preserved correctly between bot sessions.
         */
        public void Run()
        {
            _testSerialize();
        }

    }


    /**
     * @class MaplestoryBotImageLoaderTest
     * 
     * @brief Unit tests for verifying proper loading and processing of game images
     * 
     * This test class validates that the bot correctly loads and interprets game image assets,
     * ensuring accurate color detection and image recognition during gameplay automation.
     */
    internal class MaplestoryBotImageLoaderTest
    {
        /**
         * @brief Creates test image data with specific pixel color values
         * 
         * @param index The index of the test fixture to return (0-2)
         * 
         * @return A 2D array of Bgra32 pixels representing test image data
         * 
         * Provides standardized test images with known color values to verify
         * that the image loader correctly interprets color information from
         * game assets, which is critical for accurate UI element detection.
         */
        private Bgra32[,] _bufferFixture(int index)
        {
            var buffer = new List<Bgra32[,]>
            {
                new Bgra32[3, 3],
                new Bgra32[3, 3],
                new Bgra32[3, 3]
            };
            buffer.Add(new Bgra32[3, 3]);
            // Fixture 1
            buffer[0][0, 0] = new Bgra32(255, 201,  14, 255);
            buffer[0][0, 1] = new Bgra32(255, 174, 201, 255);
            buffer[0][0, 2] = new Bgra32(112, 146, 190, 255);
            buffer[0][1, 0] = new Bgra32(255, 127,  39, 255);
            buffer[0][1, 1] = new Bgra32(181, 230,  29, 255);
            buffer[0][1, 2] = new Bgra32( 56,  67,  78, 255);
            buffer[0][2, 0] = new Bgra32(237,  28,  36, 255);
            buffer[0][2, 1] = new Bgra32( 34, 177,  76, 255);
            buffer[0][2, 2] = new Bgra32(200, 191, 231, 255);
            // Fixture 2
            buffer[1][0, 0] = new Bgra32( 34, 177,  76, 255);
            buffer[1][0, 1] = new Bgra32(  0, 162, 232, 255);
            buffer[1][0, 2] = new Bgra32( 63,  72, 204, 255);
            buffer[1][1, 0] = new Bgra32(255, 242,   0, 255);
            buffer[1][1, 1] = new Bgra32(255, 127,  39, 255);
            buffer[1][1, 2] = new Bgra32(237,  28,  36, 255);
            buffer[1][2, 0] = new Bgra32( 56,  67,  78, 255);
            buffer[1][2, 1] = new Bgra32(127, 127, 127, 255);
            buffer[1][2, 2] = new Bgra32(136,   0,  21, 255);
            // Fixture 3
            buffer[2][0, 0] = new Bgra32(0, 0, 0, 0);
            buffer[2][0, 1] = new Bgra32(0, 0, 0, 0);
            buffer[2][0, 2] = new Bgra32(0, 0, 0, 0);
            buffer[2][1, 0] = new Bgra32(0, 0, 0, 0);
            buffer[2][1, 1] = new Bgra32(0, 0, 0, 0);
            buffer[2][1, 2] = new Bgra32(0, 0, 0, 0);
            buffer[2][2, 0] = new Bgra32(0, 0, 0, 0);
            buffer[2][2, 1] = new Bgra32(0, 0, 0, 0);
            buffer[2][2, 2] = new Bgra32(0, 0, 0, 0);
            return buffer[index];
        }

        /**
         * @brief Compares actual image buffer content with expected pixel values
         * 
         * @param buffer The actual image buffer to test
         * @param expected The expected pixel values to compare against
         * 
         * Validates that each pixel in the loaded image matches the expected color values,
         * ensuring the bot will correctly recognize game UI elements based on their
         * visual characteristics during automation.
         */
        private void _assertPixels(Buffer2D<Bgra32> buffer, Bgra32[,] expected)
        {
            Debug.Assert(buffer.Width == expected.GetLength(0));
            Debug.Assert(buffer.Height == expected.GetLength(1));
            for (int x = 0; x < buffer.Width; x++)
            {
                for (int y = 0; y < buffer.Height; y++)
                {
                    var buffer_pixel = buffer[x, y];
                    var expected_pixel = expected[x, y];
                    Debug.Assert(buffer_pixel == expected_pixel);
                }
            }
        }

        /**
         * @brief Tests correct loading and parsing of animated game images
         * 
         * Validates that the bot properly loads multi-frame animated images (GIF format)
         * and correctly interprets each frame's dimensions and pixel data, which is
         * essential for detecting animated UI elements and game effects.
         */
        private void _testLoadImage()
        {
            var imagePath = "Systems/Configuration/Tests/TestData/LoadImageTestData.gif";
            var image = new MaplestoryBotImageLoader().LoadImage(imagePath);
            Debug.Assert(image.Frames.Count == 3);
            for (int i = 0; i < image.Frames.Count; i++)
            {
                Debug.Assert(image.Frames[i].Bounds().X == 0);
                Debug.Assert(image.Frames[i].Bounds().Y == 0);
                Debug.Assert(image.Frames[i].Bounds().Width == 3);
                Debug.Assert(image.Frames[i].Bounds().Height == 3);
                _assertPixels(image.Frames[i].PixelBuffer, _bufferFixture(i));
            }
        }

        /**
         * @brief Executes the image loading tests
         * 
         * Runs the complete test suite to ensure the bot will correctly load and
         * interpret game image assets, providing confidence that visual detection
         * features will work reliably during gameplay automation.
         */
        public void Run()
        {
            _testLoadImage();
        }
    }


    public class ConfigurationTestSuite
    {
        public void Run()
        {
            new MaplestoryBotConfigurationDeserializerTest().Run();
            new MaplestoryBotConfigurationSerializerTest().Run();
            new MaplestoryBotImageLoaderTest().Run();
        }
    }
}
