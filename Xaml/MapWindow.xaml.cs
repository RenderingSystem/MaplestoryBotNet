using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace MaplestoryBotNet.Xaml
{
    public partial class MapWindow : Window, INotifyPropertyChanged
    {
        private AbstractSystemWindow? _systemWindow = null;

        public MapWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            NumericPropertyBottom = 0;
            NumericPropertyRight = 0;
            NumericPropertyTop = 0;
            NumericPropertyLeft = 0;
        }

        private int SetValidatedProperty(
            ref int field,
            int value,
            [CallerMemberName] string propertyName = ""
        )
        {
            if (value >= 0 && value <= 9999)
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
            return field;
        }

        private int _numericPropertyBottom;

        public int NumericPropertyBottom
        {
            get => _numericPropertyBottom;
            set => SetValidatedProperty(ref _numericPropertyBottom, value);
        }

        private int _numericPropertyRight;

        public int NumericPropertyRight
        {
            get => _numericPropertyRight;
            set => SetValidatedProperty(ref _numericPropertyRight, value);
        }

        private int _numericPropertyTop;

        public int NumericPropertyTop
        {
            get => _numericPropertyTop;
            set => SetValidatedProperty(ref _numericPropertyTop, value);
        }

        private int _numericPropertyLeft;

        public int NumericPropertyLeft
        {
            get => _numericPropertyLeft;
            set => SetValidatedProperty(ref _numericPropertyLeft, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null
        )
        {
            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(propertyName)
            );
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var proposedText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = proposedText.Length > 4 || !e.Text.All(char.IsDigit);
        }

        private void NumberValidationPasting(object sender, DataObjectPastingEventArgs e)
        {
            var pastedText = (string)e.DataObject.GetData(typeof(string));
            var textBox = (TextBox)sender;
            var proposedText = textBox.Text
                .Remove(textBox.SelectionStart, textBox.SelectionLength)
                .Insert(textBox.SelectionStart, pastedText);
            if (proposedText.Length > 4 || !pastedText.All(char.IsDigit))
            {
                e.CancelCommand();
            }
        }

        private AbstractWindowActionHandler _instantiateWindowMenuItemHideActionHandler()
        {
            return new WindowMenuItemHideHandlerBuilder()
                .WithArgs(GetSystemWindow())
                .Build();
        }

        private AbstractWindowActionHandler _instantiateEditMenuActionHandler(
            AbstractSystemWindow editWindow
        )
        {
            return new WindowMapEditMenuActionHandlerFacade(EditButton, editWindow);
        }

        public List<AbstractWindowActionHandler> InstantiateActionHandlers(
            AbstractSystemWindow editWindow
        )
        {
            return [
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateEditMenuActionHandler(editWindow)
            ];
        }

        public AbstractSystemWindow GetSystemWindow()
        {
            if (_systemWindow == null)
            {
                _systemWindow = new SystemWindow(this);
            }
            return _systemWindow;
        }
    }
}
