using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using Microsoft.Win32;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Documents;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public abstract class AbstractSaveFileDialog
    {
        public abstract void Prompt(string initialDirectory, string saveContent);

        public event EventHandler<FileSavedEventArgs>? FileSaved;

        public void InvokeFileSaved(string filePath, string saveContent)
        {
            FileSaved?.Invoke(this, new FileSavedEventArgs(filePath, saveContent));
        }
    }


    public class FileSavedEventArgs : EventArgs
    {
        public string FilePath { get; }

        public string Content { get; }

        public DateTime SaveTime { get; }

        public bool Success { get; }

        public FileSavedEventArgs(string filePath, string content)
        {
            FilePath = filePath;
            Content = content;
            SaveTime = DateTime.Now;
            Success = true;
        }
    }


    public class WindowSaveFileDialog : AbstractSaveFileDialog
    {
        private string _title;

        private string _filter;

        private string _defaultExt;

        public WindowSaveFileDialog(
            string title,
            string filter,
            string defaultExt
        )
        {
            _title = title;
            _filter = filter;
            _defaultExt = defaultExt;;
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
                var filePath = saveFileDialog.FileName;
                File.WriteAllText(saveFileDialog.FileName, saveContent);
                InvokeFileSaved(filePath, saveContent);
            }
        }
    }


    public abstract class AbstractLoadFileDialog
    {
        public abstract void Prompt(string initialDirectory);

        public event EventHandler<FileLoadedEventArgs>? FileLoaded;

        public virtual void InvokeFileLoaded(string filePath, string loadContent)
        {
            FileLoaded?.Invoke(this, new FileLoadedEventArgs(filePath, loadContent));
        }
    }


    public class FileLoadedEventArgs : EventArgs
    {
        public string FilePath { get; }

        public string Content { get; }

        public DateTime LoadTime { get; }

        public bool Success { get; }

        public FileLoadedEventArgs(string filePath, string content)
        {
            FilePath = filePath;
            Content = content;
            LoadTime = DateTime.Now;
            Success = true;
        }
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

        public override void Prompt(string initialDirectory)
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
                var filePath = openFileDialog.FileName;
                var loadContent = File.ReadAllText(openFileDialog.FileName);
                InvokeFileLoaded(filePath, loadContent);
            }
        }
    }

    public class WindowSaveMenuModifierParameters
    {
        public string InitialDirectory = "";

        public string SaveContent = "";
    }


    public class WindowSaveMenuModifier : AbstractWindowStateModifier
    {
        private AbstractSaveFileDialog _saveFileDialog;

        public WindowSaveMenuModifier(AbstractSaveFileDialog saveFileDialog)
        {
            _saveFileDialog = saveFileDialog;
        }

        public override void Modify(object? value)
        {
            if (value is WindowSaveMenuModifierParameters parameters)
            {
                _saveFileDialog.Prompt(
                    parameters.InitialDirectory,
                    parameters.SaveContent
                );
            }
        }
    }


    public class WindowLoadMenuModifierParameters
    {
        public string InitialDirectory = "";
    }


    public class WindowLoadMenuModifier : AbstractWindowStateModifier
    {
        private AbstractLoadFileDialog _loadFileDialog;

        public WindowLoadMenuModifier(
            AbstractLoadFileDialog loadFileDialog
        )
        {
            _loadFileDialog = loadFileDialog;
        }

        public override void Modify(object? value)
        {
            if (value is WindowLoadMenuModifierParameters parameters)
            {
                _loadFileDialog.Prompt(parameters.InitialDirectory);
            }
        }
    }
}
