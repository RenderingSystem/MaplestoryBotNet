using Microsoft.Win32;
using System.IO;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public abstract class AbstractSaveFileDialog
    {
        public abstract void Prompt(string initialDirectory, string saveContent);
    }


    public class WindowSaveFileDialog : AbstractSaveFileDialog
    {
        private string _title;

        private string _filter;

        private string _defaultExt;

        public WindowSaveFileDialog(string title, string filter, string defaultExt)
        {
            _title = title;
            _filter = filter;
            _defaultExt = defaultExt;
        }

        public override void Prompt(string initialDirectory, string saveContent)
        {
            string resolvedDirectory = Path.GetFullPath(initialDirectory);
            if (!Directory.Exists(resolvedDirectory))
            {
                Directory.CreateDirectory(resolvedDirectory);
            }
            var saveFileDialog = new SaveFileDialog
            {
                Title = _title,
                Filter = _filter,
                DefaultExt = _defaultExt,
                InitialDirectory = resolvedDirectory
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, saveContent);
            }
        }
    }


    public abstract class AbstractLoadFileDialog
    {
        public abstract string Prompt(string initialDirectory);
    }


    public class WindowLoadFileDialog : AbstractLoadFileDialog
    {
        private string _title;

        private string _filter;

        public WindowLoadFileDialog(string title, string filter)
        {
            _title = title;
            _filter = filter;
        }

        public override string Prompt(string initialDirectory)
        {
            string resolvedDirectory = Path.GetFullPath(initialDirectory);
            if (!Directory.Exists(resolvedDirectory))
            {
                Directory.CreateDirectory(resolvedDirectory);
            }
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = resolvedDirectory,
                Title = _title,
                Filter = _filter,
                CheckFileExists = true,
                CheckPathExists = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                return File.ReadAllText(openFileDialog.FileName);
            }
            return "";
        }
    }
}
