using System.Diagnostics;
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

        public AbstractConfiguration? Deserialized;

        public ReaderWriterLockSlim ReaderWriterLock;

        public ConfigurationEntry(
            ConfigurationType configType,
            string path,
            AbstractDeserializer deserializer
        )
        {
            ConfigType = configType;
            Path = path;
            Deserializer = deserializer;
            Deserialized = null;
            ReaderWriterLock = new ReaderWriterLockSlim();

        }
    }


    public class ConfigurationSystem : AbstractSystem
    {
        private List<ConfigurationEntry> _configurationEntries;

        private AbstractFileReader _fileReader;

        private ISystemInjectable _configurationInjector;

        public ConfigurationSystem(
            List<ConfigurationEntry> configurationEntries,
            AbstractFileReader fileReader,
            ISystemInjectable configurationInjector
        ) {
            _configurationEntries = configurationEntries;
            _fileReader = fileReader;
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

        public virtual AbstractConfiguration? GetConfiguration(ConfigurationType key)
        {
            for (int i = 0; i < _configurationEntries.Count; i++) {
                var entry = _configurationEntries[i];
                try
                {
                    entry.ReaderWriterLock.EnterReadLock();
                    if (entry.ConfigType == key)
                    {
                        if (entry.Deserialized != null)
                        {
                            return entry.Deserialized.Copy();
                        }
                    }
                }
                finally
                {
                    entry.ReaderWriterLock.ExitReadLock();
                }
            }
            return null;
        }

        public virtual void SetConfiguration(ConfigurationType key, AbstractConfiguration configuration)
        {
            bool updated = false;
            for (int i = 0; i < _configurationEntries.Count; i++)
            {
                var entry = _configurationEntries[i];
                try
                {
                    entry.ReaderWriterLock.EnterWriteLock();
                    if (entry.ConfigType == key)
                    {
                        entry.Deserialized = configuration;
                        updated = true;
                        break;
                    }
                }
                finally
                {
                    entry.ReaderWriterLock.ExitWriteLock();
                }
            }
            if (updated)
            {
                _configurationInjector.Inject(
                    SystemInjectType.ConfigurationUpdate, configuration
                );
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
                        new MaplestoryBotConfigurationDeserializer()
                    ),
                    new ConfigurationEntry(
                        ConfigurationType.ImageConfiguration,
                        "Configuration.json",
                        new ConfigurationImagesDeserializer(
                            new MaplestoryBotImageLoader(),
                            new MaplestoryBotConfigurationDeserializer()
                        )
                    ),
                    new ConfigurationEntry(
                        ConfigurationType.KeyboardConfiguration,
                        "KeyboardEncoding.json",
                        new KeyboardMappingDeserializer()
                    ),
                ],
                new FileReader(),
                new SystemInjector(_systems)
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
