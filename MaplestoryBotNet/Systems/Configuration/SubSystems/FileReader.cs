using System.IO;


namespace MaplestoryBotNet.Systems.Configuration.SubSystems
{
    public abstract class AbstractSerializer
    {
        public abstract string Serialize(object obj);
    }

    public abstract class AbstractDeserializer
    {
        public abstract object Deserialize(string data);
    }

    public abstract class AbstractFileReader
    {
        public abstract string ReadFile(string filePath);
    }


    public abstract class AbstractFileSaver
    {
        public abstract void SaveFile(string filePath, string content);
    }


    public class FileReader : AbstractFileReader
    {
        public override string ReadFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }
    }


    public class FileSaver : AbstractFileSaver
    {
        public override void SaveFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }
    }
}
