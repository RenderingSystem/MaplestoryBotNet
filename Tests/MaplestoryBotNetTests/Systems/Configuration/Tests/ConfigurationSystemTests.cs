using System.Diagnostics;
using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNetTests.Systems.Configuration.Tests.Mocks;
using MaplestoryBotNetTests.Systems.Tests;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{
    public class ConfigurationSystemTest
    {
        List<ConfigurationEntry> _configurationEntries = [];

        MockFileReader _reader = new MockFileReader();

        MockFileSaver _saver = new MockFileSaver();

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
                    new MockDeserializer(),
                    new MockSerializer()
                ),
                new ConfigurationEntry(
                    ConfigurationType.ImageConfiguration,
                    "image_path_2",
                    new MockDeserializer(),
                    new MockSerializer()
                ),
                new ConfigurationEntry(
                    ConfigurationType.KeyboardConfiguration,
                    "image_path_3",
                    new MockDeserializer(),
                    new MockSerializer()
                )
            ];
            _reader = new MockFileReader();
            _reader.ReadFileReturn = ["content_1", "content_2", "content_3"];
            _saver = new MockFileSaver();
            _injector = new MockInjector();
            _setupConfigurations();
            return new ConfigurationSystem(
                _configurationEntries, _reader, _saver, _injector
            );
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
                Debug.Assert(_injector.InjectCallArg_dataType[i] is SystemInjectType.ConfigurationUpdate);
                Debug.Assert(_injector.InjectCallArg_data[i] == configurationSystemConfiguration);
            }
        }

        /**
         * @brief Verifies that when a configuration update is injected, the system
         * broadcasts the updated configuration to all dependent components
         * 
         * When users change bot settings through the UI, the updated configuration
         * needs to be sent to all components that rely on it. This test ensures that
         * the configuration system properly propagates a single configuration injection
         * to all listeners.
         */
        private void _testSystemInjectsConfigurationUpdate()
        {
            var configurationSystem = _fixture();
            var configuration = new MockConfiguration();
            configurationSystem.Initialize();
            configurationSystem.Inject(SystemInjectType.ConfigurationUpdate, configuration);
            Debug.Assert(_injector.InjectCalls == 1);
            Debug.Assert(_injector.InjectCallArg_dataType[0] is SystemInjectType.ConfigurationUpdate);
            Debug.Assert(_injector.InjectCallArg_data[0] == configuration);
        }


        /**
         * @brief Verifies that when a configuration is saved, the system writes the
         * serialized content to the correct file path
         * 
         * When users modify bot settings, they expect their changes to persist after
         * restarting the bot. The configuration system must serialize the current
         * configuration data and write it to the appropriate file. This test ensures
         * that the save operation targets the correct file path and writes the properly
         * serialized content.
         */
        private void _testSystemSavesConfigurationToFile()
        {
            var configurationSystem = _fixture();
            configurationSystem.Initialize();
            var configuration = _configurationEntries[2].Deserialized;
            var serializer = (MockSerializer)_configurationEntries[2].Serializer!;
            serializer.SerializeReturn.Add("serialized_content");
            configurationSystem.Inject(SystemInjectType.ConfigurationSave, configuration);
            Debug.Assert(_saver.SaveFileCalls == 1);
            Debug.Assert(_saver.SaveFileCallArg_filePath[0] == "image_path_3");
            Debug.Assert(_saver.SaveFileCallArg_content[0] == "serialized_content");
        }


        public void Run()
        {
            _testSystemInitializationReadsFromAllImagePaths();
            _testSystemInitializationDeserializesAllImageContents();
            _testSystemInjectsConfigurationUpdates();
            _testSystemInjectsConfigurationUpdate();
            _testSystemSavesConfigurationToFile();
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
