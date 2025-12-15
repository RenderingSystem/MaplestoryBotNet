using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
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

        private WindowMapEditMenuState _editMenuState = new WindowMapEditMenuState();

        public MapWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            NumericPropertyBottom = 0;
            NumericPropertyRight = 0;
            NumericPropertyTop = 0;
            NumericPropertyLeft = 0;
            NumericPropertyX = 0;
            NumericPropertyY = 0;
        }

        private int SetValidatedProperty(
            ref int field,
            int value,
            int minValue,
            int maxValue,
            [CallerMemberName] string propertyName = ""
        )
        {
            if (value >= minValue && value <= maxValue)
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
            set => SetValidatedProperty(ref _numericPropertyBottom, value, 0, 9999);
        }

        private int _numericPropertyRight;

        public int NumericPropertyRight
        {
            get => _numericPropertyRight;
            set => SetValidatedProperty(ref _numericPropertyRight, value, 0, 9999);
        }

        private int _numericPropertyTop;

        public int NumericPropertyTop
        {
            get => _numericPropertyTop;
            set => SetValidatedProperty(ref _numericPropertyTop, value, 0, 9999);
        }

        private int _numericPropertyLeft;

        public int NumericPropertyLeft
        {
            get => _numericPropertyLeft;
            set => SetValidatedProperty(ref _numericPropertyLeft, value, 0, 9999);
        }

        private int _numericPropertyX;

        public int NumericPropertyX
        {
            get => _numericPropertyX;
            set => SetValidatedProperty(ref _numericPropertyX, value, 0, Convert.ToInt32(MapCanvas.ActualWidth));
        }

        private int _numericPropertyY;

        public int NumericPropertyY
        {
            get => _numericPropertyY;
            set => SetValidatedProperty(ref _numericPropertyY, value, 0, Convert.ToInt32(MapCanvas.ActualHeight));
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
            return new WindowMapEditMenuActionHandlerFacade(
                EditButton, GetSystemWindow(), editWindow
            );
        }

        private AbstractWindowActionHandler _instantiateAddPointButtonActionHandler()
        {
            return new WindowMapAddPointButtonActionHandlerFacade(
                AddButton, [AddButton, RemoveButton], _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiatePointDrawingActionHandler()
        {
            return new WindowMapCanvasPointDrawingActionHandlerFacade(
                MapCanvas, _editMenuState, new MouseEventPositionExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiatePointErasingActionHandler()
        {
            return new WindowMapCanvasPointErasingActionHandlerFacade(
                MapCanvas, _editMenuState, new MouseEventPositionExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiateRemovePointButtonActionHandler()
        {
            return new WindowMapRemovePointButtonActionHandlerFacade(
                RemoveButton, [AddButton, RemoveButton], _editMenuState
            );
        }

        private AbstractWindowActionHandler _instantiateSelectPointActionHandler()
        {
            return new WindowMapCanvasSelectActionHandlerFacade(
                MapCanvas,
                LocationTextBoxX,
                LocationTextBoxY,
                LabelTextBox,
                _editMenuState,
                new MouseEventPositionExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiateDragPointActionHandler()
        {
            return new WindowMapCanvasDragActionHandlerFacade(
                MapCanvas,
                LocationTextBoxX,
                LocationTextBoxY,
                _editMenuState,
                new MouseEventPositionExtractor()
            );
        }

        private AbstractWindowActionHandler _instantiateEditButtonAccessibilityActionHandler()
        {
            return new WindowMapCanvasEditButtonAccessibilityActionHandlerFacade(
                MapCanvas, EditButton, _editMenuState

            );
        }
        public List<AbstractWindowActionHandler> InstantiateActionHandlers(
            AbstractSystemWindow editWindow
        )
        {
            return [
                _instantiateWindowMenuItemHideActionHandler(),
                _instantiateEditMenuActionHandler(editWindow),
                _instantiateAddPointButtonActionHandler(),
                _instantiatePointDrawingActionHandler(),
                _instantiateRemovePointButtonActionHandler(),
                _instantiatePointErasingActionHandler(),
                _instantiateSelectPointActionHandler(),
                _instantiateDragPointActionHandler(),
                _instantiateEditButtonAccessibilityActionHandler()
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
