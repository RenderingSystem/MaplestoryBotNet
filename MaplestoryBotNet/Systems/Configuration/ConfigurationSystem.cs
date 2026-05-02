using MaplestoryBotNet.Systems.Configuration.SubSystems;


namespace MaplestoryBotNet.Systems.Configuration
{
    public enum ConfigurationType
    {
        MainConfiguration = 0,
        ImageConfiguration = 1,
        KeyboardConfiguration = 2
    }


    public class ConfigurationEntry
    {
        public readonly ConfigurationType ConfigType;

        public readonly string Path;

        public readonly AbstractDeserializer Deserializer;

        public readonly AbstractSerializer? Serializer;

        public AbstractConfiguration? Deserialized;

        public ConfigurationEntry(
            ConfigurationType configType,
            string path,
            AbstractDeserializer deserializer,
            AbstractSerializer? serializer
        )
        {
            ConfigType = configType;
            Path = path;
            Deserializer = deserializer;
            Serializer = serializer;
            Deserialized = null;
        }
    }


    public class ConfigurationSystem : AbstractSystem
    {
        private List<ConfigurationEntry> _configurationEntries;

        private AbstractFileReader _fileReader;

        private AbstractFileSaver _fileSaver;

        private IDataInjectable _configurationInjector;

        public ConfigurationSystem(
            List<ConfigurationEntry> configurationEntries,
            AbstractFileReader fileReader,
            AbstractFileSaver fileSaver,
            IDataInjectable configurationInjector
        ) {
            _configurationEntries = configurationEntries;
            _fileReader = fileReader;
            _fileSaver = fileSaver;
            _configurationInjector = configurationInjector;
        }

        public override void Initialize()
        {
            for (int i = 0; i < _configurationEntries.Count; i++)
            {
                var configurationEntry = _configurationEntries[i];
                var filePath = configurationEntry.Path;
                var deserializer = configurationEntry.Deserializer;
                var fileContent = _fileReader.ReadFile(filePath);
                var deserialized = (AbstractConfiguration)deserializer.Deserialize(fileContent);
                configurationEntry.Deserialized = deserialized;
            }
        }

        public override void Start()
        {
            for (int i = 0; i < _configurationEntries.Count; i++) {
                var configurationEntry = _configurationEntries[i];
                if (configurationEntry.Deserialized != null)
                {
                    _configurationInjector.Inject(
                        SystemInjectType.Configuration, configurationEntry.Deserialized
                    );
                }
            }
        }

        public override void Inject(object dataType, object? data)
        {
            if (dataType is SystemInjectType.ConfigurationUpdate)
            {
                if (data is not AbstractConfiguration configurationUpdate)
                {
                    foreach (var entry in _configurationEntries)
                    {
                        _configurationInjector.Inject(
                            SystemInjectType.ConfigurationUpdate, entry.Deserialized
                        );
                    }
                }
                else
                {
                    _configurationInjector.Inject(
                        SystemInjectType.ConfigurationUpdate, configurationUpdate
                    );
                }
            }
            if (
                dataType is SystemInjectType.ConfigurationSave
                && data is AbstractConfiguration configurationSave
            )
            {
                var configurationEntry = (
                    _configurationEntries.Find((e) => e.Deserialized == configurationSave)
                );
                if (configurationEntry != null && configurationEntry.Serializer != null)
                {
                    var serialized = configurationEntry.Serializer.Serialize(configurationSave);
                    _fileSaver.SaveFile(configurationEntry.Path, serialized);
                }
            }
        }
    }


    public class ConfigurationSystemBuilder : AbstractSystemBuilder
    {
        private List<AbstractSystem> _systems = [];

        public override AbstractSystem Build()
        {
            return new ConfigurationSystem(
                [
                    new ConfigurationEntry(
                        ConfigurationType.MainConfiguration,
                        "Configuration.json",
                        new MaplestoryBotConfigurationDeserializer(),
                        new MaplestoryBotConfigurationSerializer()
                    ),
                    new ConfigurationEntry(
                        ConfigurationType.ImageConfiguration,
                        "Configuration.json",
                        new ConfigurationImagesDeserializer(
                            new MaplestoryBotImageLoader(),
                            new MaplestoryBotConfigurationDeserializer()
                        ),
                        null
                    ),
                    new ConfigurationEntry(
                        ConfigurationType.KeyboardConfiguration,
                        "KeyboardEncoding.json",
                        new KeyboardMappingDeserializer(),
                        null
                    ),
                ],
                new FileReader(),
                new FileSaver(),
                new SystemInjector(_systems)
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            if (arg is AbstractSystem system)
            {
                _systems.Add(system);
            }
            return this;
        }
    }
}
