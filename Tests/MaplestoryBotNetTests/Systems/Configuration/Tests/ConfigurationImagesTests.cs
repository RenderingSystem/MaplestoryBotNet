

using System.Diagnostics;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{
    /**
     * @class ConfigurationImagesTests
     * 
     * @brief Validates the correct loading and processing of template images for automated bot operations
     * 
     * This test suite ensures that all template images used for screen recognition and template
     * matching are properly loaded, processed, and made available for the bot's automation functions.
     */
    public class ConfigurationImagesTests
    {
        /**
         * @brief Provides test configuration data for template image processing validation
         * 
         * Contains sample configuration data that defines various automation scenarios
         * including status effect detection (ailments) and on-screen element recognition (map icons).
         */
        private string _fixture()
        {
            return """
            {
                "ailments": {
                    "seal": {
                        "active": true,
                        "active_delay": 123,
                        "threshold": 234,
                        "macro_commands": ["123", "234", "345"],
                        "image": "Systems/Configuration/Tests/TestData/ConfigurationImagesTestData1.gif",
                        "static_rect": [ 345, 456, 567, 678 ]
                    }
                },
                "map_icons": {
                    "character": {
                        "image": "Systems/Configuration/Tests/TestData/ConfigurationImagesTestData2.gif"
                    },
                    "rune": {
                        "image": "Systems/Configuration/Tests/TestData/ConfigurationImagesTestData3.gif"
                    }
                }
            }
            """;
        }

        /**
         * @brief Defines expected pixel data for template matching validation
         * 
         * Specifies the precise color values that template images should contain
         * ensuring the bot's image recognition system will correctly identify these patterns
         * during automated operations.
         */
        private Dictionary<string, Bgra32[,]> _expected()
        {
            var buffer = new Dictionary<string, Bgra32[,]>
            {
                { "seal", new Bgra32[3, 3] },
                { "character", new Bgra32[3, 3] },
                { "rune", new Bgra32[3, 3] }
            };
            // Fixture 1
            buffer["seal"][0, 0] = new Bgra32(255, 201, 14, 255);
            buffer["seal"][0, 1] = new Bgra32(255, 174, 201, 255);
            buffer["seal"][0, 2] = new Bgra32(112, 146, 190, 255);
            buffer["seal"][1, 0] = new Bgra32(255, 127, 39, 255);
            buffer["seal"][1, 1] = new Bgra32(181, 230, 29, 255);
            buffer["seal"][1, 2] = new Bgra32(56, 67, 78, 255);
            buffer["seal"][2, 0] = new Bgra32(237, 28, 36, 255);
            buffer["seal"][2, 1] = new Bgra32(34, 177, 76, 255);
            buffer["seal"][2, 2] = new Bgra32(200, 191, 231, 255);
            // Fixture 2
            buffer["character"][0, 0] = new Bgra32(34, 177, 76, 255);
            buffer["character"][0, 1] = new Bgra32(0, 162, 232, 255);
            buffer["character"][0, 2] = new Bgra32(63, 72, 204, 255);
            buffer["character"][1, 0] = new Bgra32(255, 242, 0, 255);
            buffer["character"][1, 1] = new Bgra32(255, 127, 39, 255);
            buffer["character"][1, 2] = new Bgra32(237, 28, 36, 255);
            buffer["character"][2, 0] = new Bgra32(56, 67, 78, 255);
            buffer["character"][2, 1] = new Bgra32(127, 127, 127, 255);
            buffer["character"][2, 2] = new Bgra32(136, 0, 21, 255);
            // Fixture 3
            buffer["rune"][0, 0] = new Bgra32(0, 0, 0, 0);
            buffer["rune"][0, 1] = new Bgra32(255, 242, 0, 255);
            buffer["rune"][0, 2] = new Bgra32(34, 177, 76, 255);
            buffer["rune"][1, 0] = new Bgra32(127, 127, 127, 255);
            buffer["rune"][1, 1] = new Bgra32(255, 127, 39, 255);
            buffer["rune"][1, 2] = new Bgra32(0, 162, 232, 255);
            buffer["rune"][2, 0] = new Bgra32(136, 0, 21, 255);
            buffer["rune"][2, 1] = new Bgra32(237, 28, 36, 255);
            buffer["rune"][2, 2] = new Bgra32(63, 72, 204, 255);
            return buffer;
        }

        /**
         * @brief Validates that processed images match expected template specifications
         * 
         * Compares the actual pixel data of processed template images against expected values
         * to ensure accurate image recognition capabilities.
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
         * @brief Tests that configuration images are correctly deserialized and processed for template matching
         * 
         * Validates that template images referenced in configuration files are properly loaded with their
         * correct visual properties, ensuring the bot can accurately identify target elements on screen during
         * automated operations.
         */
        private void _testDeserializeDeserializesConfigurationImages()
        {
            var deserializer = new ConfigurationImagesDeserializer(
                new MaplestoryBotImageLoader(),
                new MaplestoryBotConfigurationDeserializer()
            );
            var result = (ConfigurationImages)deserializer.Deserialize(_fixture());
            var expected = _expected();
            _assertPixels(result.AilmentImages["seal"].Frames[0].PixelBuffer, expected["seal"]);
            _assertPixels(result.MapIconImages["character"].Frames[0].PixelBuffer, expected["character"]);
            _assertPixels(result.MapIconImages["rune"].Frames[0].PixelBuffer, expected["rune"]);
        }

        /**
         * @brief Executes all template image validation tests
         * 
         * Runs the complete test suite to verify that all template images specified in bot configurations
         * will be correctly processed and available for screen recognition.
         */
        public void Run()
        {
            _testDeserializeDeserializesConfigurationImages();
        }
    }


    /**
     * @class ConfigurationImagesTestSuite
     * 
     * @brief Comprehensive test suite for bot template image processing
     * 
     * This test suite validates the complete template image configuration system, ensuring that all visual
     * templates used for screen recognition and automated macro execution are correctly loaded and processed from
     * configuration files.
     */
    public class ConfigurationImagesTestSuite
    {
        /**
         * @brief Executes the complete template image configuration test suite
         * 
         * Runs all tests related to template image processing to ensure the bot will correctly identify
         * target elements on screen.
         */
        public void Run()
        {
            new ConfigurationImagesTests().Run();
        }
    }
}
