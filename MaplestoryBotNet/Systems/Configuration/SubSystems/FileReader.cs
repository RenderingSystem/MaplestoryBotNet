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


    public abstract class AbstractDirectoryFiles
    {
        public abstract List<string> Files(string directory, string searchPattern);
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


    public class DirectoryFiles : AbstractDirectoryFiles
    {
        public override List<string> Files(
            string directory, string searchPattern = ""
        )
        {
            var files = Directory.GetFiles(directory, searchPattern).ToList();
            files = files.Select(Path.GetFileName).ToList()!;
            files = files.OrderBy(f => f).ToList();
            return files;
        }
    }
}
