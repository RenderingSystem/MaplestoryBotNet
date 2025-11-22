using System.Diagnostics;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNetTests.Systems.Configuration.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{
    public class ConfigurationSystemTest
    {
        List<ConfigurationEntry> _configurationEntries = [];

        MockFileReader _reader = new MockFileReader();

        List<MockConfiguration> _configurations = [];

        List<MockConfiguration> _configurationCopies = [];

        MockInjector _injector = new MockInjector();

        /**
         * @brief Creates a complete test environment for configuration system testing
         * 
         * @return Configured ConfigurationSystem instance
         * 
         * Prepares a comprehensive test environment with multiple configuration types
         * to verify the system's ability to handle different configuration formats
         * and sources for flexible bot customization.
         */
        private ConfigurationSystem _fixture()
        {
            _configurationEntries = [
                new ConfigurationEntry(
                    ConfigurationType.MainConfiguration,
                    "image_path_1",
                    new MockDeserializer()
                ),
                new ConfigurationEntry(
                    ConfigurationType.ImageConfiguration,
                    "image_path_2",
                    new MockDeserializer()
                ),
                new ConfigurationEntry(
                    ConfigurationType.KeyboardConfiguration,
                    "image_path_3",
                    new MockDeserializer()
                )
            ];
            _reader = new MockFileReader();
            _reader.ReadFileReturn = ["content_1", "content_2", "content_3"];
            _injector = new MockInjector();
            _setupConfigurations();
            return new ConfigurationSystem(_configurationEntries, _reader, _injector);
        }


        /**
         * @brief Configures mock configurations for testing
         * 
         * Sets up test configurations with proper copying behavior to verify
         * that the system correctly handles configuration isolation and
         * prevents unintended sharing between components.
         */
        private void _setupConfigurations()
        {
            _configurations.Clear();
            _configurationCopies.Clear();
            for (int i = 0; i < _configurationEntries.Count; i++)
            {
                var deserializer = (MockDeserializer)_configurationEntries[i].Deserializer;
                var configuration = new MockConfiguration();
                var configurationCopy = new MockConfiguration();
                configuration.CopyReturn.Add(configurationCopy);
                _configurations.Add(configuration);
                _configurationCopies.Add(configurationCopy);
                deserializer.DeserializeReturn.Add(configuration);
            }
        }

        /**
         * @brief Tests comprehensive configuration file reading during initialization
         * 
         * Validates that the bot correctly reads all configuration files from their
         * specified paths during initialization, ensuring that all customization
         * options are properly loaded before automation begins.
         */
        private void _testSystemInitializationReadsFromAllImagePaths()
        {
            var configurationSystem = _fixture();
            configurationSystem.Initialize();
            Debug.Assert(_reader.ReadFileCalls == 3);
            Debug.Assert(_reader.ReadFileCallArg_filePath.IndexOf("image_path_1") != -1);
            Debug.Assert(_reader.ReadFileCallArg_filePath.IndexOf("image_path_2") != -1);
            Debug.Assert(_reader.ReadFileCallArg_filePath.IndexOf("image_path_3") != -1);
        }

        /**
         * @brief Tests proper configuration content interpretation during initialization
         * 
         * Validates that the bot correctly interprets and processes all configuration
         * file contents during initialization, ensuring that customization options are
         * properly understood and applied to bot behavior.
         */
        private void _testSystemInitializationDeserializesAllImageContents()
        {
            var configurationSystem = _fixture();
            configurationSystem.Initialize();
            for (int i = 0; i < _configurationEntries.Count; i++)
            {
                var deserializer = (MockDeserializer)_configurationEntries[i].Deserializer;
                Debug.Assert(deserializer.DeserializeCalls == 1);
                Debug.Assert(deserializer.DeserializeCallArg_data[0] == "content_" + (i+1));
            }
        }

        /**
         * @brief Tests correct configuration retrieval by type
         * 
         * Validates that the bot correctly retrieves specific configuration types
         * when requested by automation components, ensuring that each component
         * receives the appropriate customization options for its operation.
         */
        private void _testSystemGetsCorrectConfiguration()
        {
            var configurationSystem = _fixture();
            configurationSystem.Initialize();
            for (int i = 0; i < _configurationEntries.Count; i++)
            {
                var entry = _configurationEntries[i];
                var configuration = _configurations[i];
                var configurationCopy = _configurationCopies[i];
                var result = (MockConfiguration?)configurationSystem.GetConfiguration(entry.ConfigType);
                Debug.Assert(result != null);
                Debug.Assert(result == configurationCopy);
            }
        }

        /**
         * @brief Tests correct configuration updating functionality
         * 
         * Validates that the bot correctly updates configuration values at runtime,
         * ensuring that automation behavior can be dynamically adjusted based on
         * changing game conditions or user preferences.
         */
        private void _testSystemSetsCorrectConfiguration()
        {
            var configurationSystem = _fixture();
            configurationSystem.Initialize();
            var configuration = new MockConfiguration();
            var configurationCopy = new MockConfiguration();
            configuration.CopyReturn.Add(configurationCopy);
            configurationSystem.SetConfiguration(ConfigurationType.MainConfiguration, configuration);
            var result = configurationSystem.GetConfiguration(ConfigurationType.MainConfiguration);
            Debug.Assert(result != null);
            Debug.Assert(result == configurationCopy);
        }

        /**
         * @brief Tests configuration update propagation to all dependent systems
         * 
         * Validates that when configuration changes occur, the update notifications
         * are properly broadcast to all components that depend on configuration data.
         * Ensures that all automation modules receive timely updates when settings
         * are modified, maintaining consistent behavior across the entire system.
         */
        private void _testSystemInjectsConfigurationUpdates()
        {
            var configurationSystem = _fixture();
            configurationSystem.Initialize();
            configurationSystem.Inject(SystemInjectType.ConfigurationUpdate, 0);
            Debug.Assert(_injector.InjectCalls == 3);
            for (int i = 0; i < 3; i++)
            {
                var configurationSystemConfiguration = _configurations[i];
                Debug.Assert(_injector.InjectCallArg_dataType[i] == SystemInjectType.ConfigurationUpdate);
                Debug.Assert(_injector.InjectCallArg_data[i] == configurationSystemConfiguration);
            }
        }

        /**
         * @brief Executes all configuration management tests
         * 
         * Runs the complete test suite to ensure the bot will correctly handle
         * all configuration operations, providing confidence that customization
         * options will be properly managed throughout automation sessions.
         */
        public void Run()
        {
            _testSystemInitializationReadsFromAllImagePaths();
            _testSystemInitializationDeserializesAllImageContents();
            _testSystemGetsCorrectConfiguration();
            _testSystemSetsCorrectConfiguration();
            _testSystemInjectsConfigurationUpdates();
        }
    }


    public class ConfigurationSystemTestSuite
    {
        public void Run()
        {
            new ConfigurationSystemTest().Run();
        }
    }
}
