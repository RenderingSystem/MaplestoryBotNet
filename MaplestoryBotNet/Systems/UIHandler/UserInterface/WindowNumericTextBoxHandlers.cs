using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class NumericTextBoxModifierParameters
    {
        public string Text = "";
    }


    public class NumericTextBoxValidationModifier : AbstractWindowStateModifier
    {
        private TextBox _numericTextBox;

        private int _maxValue;

        private bool _handled;

        public NumericTextBoxValidationModifier(TextBox numericTextBox, int maxValue)
        {
            _numericTextBox = numericTextBox;
            _maxValue = maxValue;
            _handled = false;
        }

        public override void Modify(object? value)
        {
            if (value is not NumericTextBoxModifierParameters parameters)
            {
                return;
            }
            var proposedText = _numericTextBox.Text;
            proposedText = proposedText.Remove(
                _numericTextBox.SelectionStart,
                _numericTextBox.SelectionLength
            );
            proposedText = proposedText.Insert(
                _numericTextBox.SelectionStart,
                parameters.Text
            );
            _handled = (
                !parameters.Text.All(char.IsDigit)
                || Convert.ToInt32(proposedText) > _maxValue
                || proposedText.Length > _maxValue.ToString().Length
            );
        }

        public override object? State(int stateType)
        {
            return _handled;
        }
    }


    public class NumericTextBoxValidationActionHandler : AbstractWindowActionHandler
    {
        private TextBox _numericTextBox;

        private AbstractWindowStateModifier _numericTextBoxValidation;

        public NumericTextBoxValidationActionHandler(
            TextBox numericTextBox,
            AbstractWindowStateModifier numericTextBoxValidation
        )
        {
            _numericTextBox = numericTextBox;
            _numericTextBoxValidation = numericTextBoxValidation;
            _numericTextBox.PreviewTextInput += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _numericTextBoxValidation;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (e is not TextCompositionEventArgs tce)
            {
                return;
            }
            _numericTextBoxValidation.Modify(
                new NumericTextBoxModifierParameters { Text = tce.Text }
            );
            tce.Handled = (bool)_numericTextBoxValidation.State(0)!;
        }
    }


    public class NumericTextBoxValidationActionHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private TextBox? _numericTextBox;

        private int _maxValue;

        public NumericTextBoxValidationActionHandlerBuilder()
        {
            _numericTextBox = null;
            _maxValue = 0;
        }

        public override AbstractWindowActionHandler Build()
        {
            return new NumericTextBoxValidationActionHandler(
                _numericTextBox!,
                new NumericTextBoxValidationModifier(_numericTextBox!, _maxValue)
            );
        }

        public override AbstractWindowActionHandlerBuilder WithArgs(object? args)
        {
            if (args is TextBox numericTextBox)
            {
                _numericTextBox = numericTextBox;
            }
            if (args is int maxValue)
            {
                _maxValue = maxValue;
            }
            return this;
        }
    }


    public class NumericTextBoxPasteValidationModifierParameters
    {
        public string PastedText = "";
    }


    public class NumericTextBoxPasteValidationModifier : AbstractWindowStateModifier
    {
        private TextBox _numericTextBox;

        private int _maxValue;

        private bool _cancelCommand;

        public NumericTextBoxPasteValidationModifier(TextBox numericTextBox, int maxValue)
        {
            _numericTextBox = numericTextBox;
            _maxValue = maxValue;
            _cancelCommand = false;
        }

        public override void Modify(object? value)
        {
            if (value is not NumericTextBoxPasteValidationModifierParameters parameters)
            {
                return;
            }
            var proposedText = _numericTextBox.Text;
            proposedText = proposedText.Remove(
                _numericTextBox.SelectionStart,
                _numericTextBox.SelectionLength
            );
            proposedText = proposedText.Insert(
                _numericTextBox.SelectionStart,
                parameters.PastedText
            );
            _cancelCommand = (
                string.IsNullOrEmpty(parameters.PastedText)
                || !parameters.PastedText.All(char.IsDigit)
                || Convert.ToInt32(proposedText) > _maxValue
                || proposedText.Length > _maxValue.ToString().Length
            );
        }

        public override object? State(int stateType)
        {
            return _cancelCommand;
        }
    }


    public class NumericTextBoxPasteValidationActionHandler : AbstractWindowActionHandler
    {
        private TextBox _numericTextBox;

        private AbstractWindowStateModifier _numericTextBoxPasteValidationModifier;

        public NumericTextBoxPasteValidationActionHandler(
            TextBox numericTextBox,
            AbstractWindowStateModifier numericTextBoxPasteValidationModifier
        )
        {
            _numericTextBox = numericTextBox;
            _numericTextBoxPasteValidationModifier = numericTextBoxPasteValidationModifier;
            DataObject.AddPastingHandler(_numericTextBox, OnEvent);
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _numericTextBoxPasteValidationModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            if (
                e is DataObjectPastingEventArgs dope
                && dope.DataObject.GetData(typeof(string)) is string pastedText
            )
            {
                _numericTextBoxPasteValidationModifier.Modify(
                    new NumericTextBoxPasteValidationModifierParameters
                    {
                        PastedText = pastedText
                    }
                );
                if ((bool)_numericTextBoxPasteValidationModifier.State(0)!)
                {
                    dope.CancelCommand();
                }
            }
        }
    }


    public class NumericTextBoxPasteValidationActionHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        private TextBox? _numericTextBox;

        private int _maxValue;

        public override AbstractWindowActionHandler Build()
        {
            return new NumericTextBoxPasteValidationActionHandler(
                _numericTextBox!,
                new NumericTextBoxPasteValidationModifier(
                    _numericTextBox!, _maxValue
                )
            );
        }

        public override AbstractWindowActionHandlerBuilder WithArgs(object? args)
        {
            if (args is TextBox numericTextBox)
            {
                _numericTextBox = numericTextBox;
            }
            if (args is int maxValue)
            {
                _maxValue = maxValue;
            }
            return this;
        }
    }
}
