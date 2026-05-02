using MaplestoryBotNet.Systems.Configuration.SubSystems;
using System.Windows;
using System.Windows.Controls;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public class WindowRuneSolverRoboflowAPILoadModifier : AbstractWindowStateModifier
    {
        private TextBox _ipAddressTextBox;

        private TextBox _portTextBox;

        private TextBox _routeTextBox;

        private TextBox _classTagTextBox;

        private TextBox _xTextBox;

        private TextBox _yTextBox;

        private TextBox _leftArrowTextBox;

        private TextBox _upArrowTextBox;

        private TextBox _rightArrowTextBox;

        private TextBox _downArrowTextBox;

        public WindowRuneSolverRoboflowAPILoadModifier(
            TextBox ipAddressTextBox,
            TextBox portTextBox,
            TextBox routeTextBox,
            TextBox classTagTextBox,
            TextBox xTextBox,
            TextBox yTextBox,
            TextBox leftArrowTextBox,
            TextBox upArrowTextBox,
            TextBox rightArrowTextBox,
            TextBox downArrowTextBox
        )
        {
            _ipAddressTextBox = ipAddressTextBox;
            _portTextBox = portTextBox;
            _routeTextBox = routeTextBox;
            _classTagTextBox = classTagTextBox;
            _xTextBox = xTextBox;
            _yTextBox = yTextBox;
            _leftArrowTextBox = leftArrowTextBox;
            _upArrowTextBox = upArrowTextBox;
            _rightArrowTextBox = rightArrowTextBox;
            _downArrowTextBox = downArrowTextBox;
        }

        public override void Modify(object? value)
        {
            if (
                value is MaplestoryBotConfiguration configuration
                && configuration.RuneDetection is RuneDetection runeDetection
            )
            {
                _ipAddressTextBox.Text = runeDetection.RuneSolverIPAddress;
                _portTextBox.Text = runeDetection.RuneSolverPort;
                _routeTextBox.Text = runeDetection.RuneSolverRoute;
                _classTagTextBox.Text = runeDetection.ClassTag;
                _xTextBox.Text = runeDetection.X;
                _yTextBox.Text = runeDetection.Y;
                _leftArrowTextBox.Text = runeDetection.Left;
                _upArrowTextBox.Text = runeDetection.Up;
                _rightArrowTextBox.Text = runeDetection.Right;
                _downArrowTextBox.Text = runeDetection.Down;
            }
        }
    }


    public class WindowRuneSolverRoboflowAPILoadActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _systemWindow;

        private AbstractWindowStateModifier _roboflowAPILoadModifier;

        private MaplestoryBotConfiguration? _maplestoryBotConfiguration;

        public WindowRuneSolverRoboflowAPILoadActionHandler(
            AbstractSystemWindow systemWindow,
            AbstractWindowStateModifier roboflowAPILoadModifier
        )
        {
            _systemWindow = systemWindow;
            _roboflowAPILoadModifier = roboflowAPILoadModifier;
            ((Window)_systemWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _roboflowAPILoadModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate
                && data is MaplestoryBotConfiguration configuration
            )
            {
                _maplestoryBotConfiguration = configuration;
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (_systemWindow.Visible())
            {
                _roboflowAPILoadModifier.Modify(_maplestoryBotConfiguration);
            }
        }
    }

    
    public class WindowRuneSolverRoboflowAPILoadActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _roboflowAPILoadActionHandler;

        public WindowRuneSolverRoboflowAPILoadActionHandlerFacade(
            AbstractSystemWindow systemWindow,
            TextBox workspaceNameTextBox,
            TextBox workspaceIDTextBox,
            TextBox apiKeyTextBox,
            TextBox arrayTextBox,
            TextBox xTextBox,
            TextBox yTextBox,
            TextBox leftArrowTextBox,
            TextBox upArrowTextBox,
            TextBox rightArrowTextBox,
            TextBox downArrowTextBox
        )
        {
            _roboflowAPILoadActionHandler = (
                new WindowRuneSolverRoboflowAPILoadActionHandler(
                    systemWindow,
                    new WindowRuneSolverRoboflowAPILoadModifier(
                        workspaceNameTextBox,
                        workspaceIDTextBox,
                        apiKeyTextBox,
                        arrayTextBox,
                        xTextBox,
                        yTextBox,
                        leftArrowTextBox,
                        upArrowTextBox,
                        rightArrowTextBox,
                        downArrowTextBox
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _roboflowAPILoadActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _roboflowAPILoadActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _roboflowAPILoadActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowRuneSolverRoboflowAPISaveModifierParameters
    {
        public AbstractInjectAction? InjectAction = null;

        public AbstractConfiguration? MaplestoryBotConfiguration = null;
    }


    public class WindowRuneSolverRoboflowAPISaveModifier : AbstractWindowStateModifier
    {
        private TextBox _ipAddressTextBox;

        private TextBox _portTextBox;

        private TextBox _routeTextBox;

        private TextBox _classTagTextBox;

        private TextBox _xTextBox;

        private TextBox _yTextBox;

        private TextBox _leftArrowTextBox;

        private TextBox _upArrowTextBox;

        private TextBox _rightArrowTextBox;

        private TextBox _downArrowTextBox;

        public WindowRuneSolverRoboflowAPISaveModifier(
            TextBox ipAddressTextBox,
            TextBox portTextBox,
            TextBox routeTextBox,
            TextBox classTagTextBox,
            TextBox xTextBox,
            TextBox yTextBox,
            TextBox leftArrowTextBox,
            TextBox upArrowTextBox,
            TextBox rightArrowTextBox,
            TextBox downArrowTextBox
        )
        {
            _ipAddressTextBox = ipAddressTextBox;
            _portTextBox = portTextBox;
            _routeTextBox = routeTextBox;
            _classTagTextBox = classTagTextBox;
            _xTextBox = xTextBox;
            _yTextBox = yTextBox;
            _leftArrowTextBox = leftArrowTextBox;
            _upArrowTextBox = upArrowTextBox;
            _rightArrowTextBox = rightArrowTextBox;
            _downArrowTextBox = downArrowTextBox;
        }

        public override void Modify(object? value)
        {
            if (value is MaplestoryBotConfiguration maplestoryBotConfiguration)
            {
                maplestoryBotConfiguration.RuneDetection = (
                    new RuneDetection
                    {
                        RuneSolverIPAddress = _ipAddressTextBox.Text,
                        RuneSolverPort = _portTextBox.Text,
                        RuneSolverRoute = _routeTextBox.Text,
                        ClassTag = _classTagTextBox.Text,
                        X = _xTextBox.Text,
                        Y = _yTextBox.Text,
                        Left = _leftArrowTextBox.Text,
                        Up = _upArrowTextBox.Text,
                        Right = _rightArrowTextBox.Text,
                        Down = _downArrowTextBox.Text,
                    }
                );
            }
        }
    }


    public class WindowRuneSolverRoboflowAPISaveActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _systemWindow;

        private AbstractWindowStateModifier _roboflowAPISaveModifier;

        private MaplestoryBotConfiguration? _maplestoryBotConfiguration;

        public WindowRuneSolverRoboflowAPISaveActionHandler(
            AbstractSystemWindow systemWindow,
            AbstractWindowStateModifier roboflowAPISaveModifier
        )
        {
            _systemWindow = systemWindow;
            _roboflowAPISaveModifier = roboflowAPISaveModifier;
            ((Window)_systemWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _roboflowAPISaveModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.ConfigurationUpdate
                && data is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                _maplestoryBotConfiguration = maplestoryBotConfiguration;
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (!_systemWindow.Visible())
            {
                _roboflowAPISaveModifier.Modify(_maplestoryBotConfiguration);
            }
        }
    }


    public class WindowRuneSolverRoboflowAPISaveActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _roboflowAPISaveActionHandler;

        public WindowRuneSolverRoboflowAPISaveActionHandlerFacade(
            AbstractSystemWindow systemWindow,
            TextBox ipAddressTextBox,
            TextBox portTextBox,
            TextBox routeTextBox,
            TextBox classTagTextBox,
            TextBox xTextBox,
            TextBox yTextBox,
            TextBox leftArrowTextBox,
            TextBox upArrowTextBox,
            TextBox rightArrowTextBox,
            TextBox downArrowTextBox
        )
        {
            _roboflowAPISaveActionHandler = (
                new WindowRuneSolverRoboflowAPISaveActionHandler(
                    systemWindow,
                    new WindowRuneSolverRoboflowAPISaveModifier(
                        ipAddressTextBox,
                        portTextBox,
                        routeTextBox,
                        classTagTextBox,
                        xTextBox,
                        yTextBox,
                        leftArrowTextBox,
                        upArrowTextBox,
                        rightArrowTextBox,
                        downArrowTextBox
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _roboflowAPISaveActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _roboflowAPISaveActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _roboflowAPISaveActionHandler.OnDependencyEvent(sender, e);
        }
    }



    public class WindowRuneSolverRoboflowAPIInjectModifierParameters
    {
        public AbstractInjectAction? InjectAction = null;

        public AbstractConfiguration? MaplestoryBotConfiguration = null;
    }


    public class WindowRuneSolverRoboflowAPIInjectModifier : AbstractWindowStateModifier
    {
        public override void Modify(object? value)
        {
            if (
                value is WindowRuneSolverRoboflowAPIInjectModifierParameters parameters
                && parameters.InjectAction is AbstractInjectAction injectAction
                && parameters.MaplestoryBotConfiguration is MaplestoryBotConfiguration configuration
            )
            {
                var action = injectAction.GetAction();
                action(SystemInjectType.ConfigurationUpdate, configuration);
                action(SystemInjectType.ConfigurationSave, configuration);
            }
        }
    }


    public class WindowRuneSolverRoboflowAPIInjectActionHandler : AbstractWindowActionHandler
    {
        private AbstractSystemWindow _systemWindow;

        private AbstractWindowStateModifier _roboflowAPIInjectModifier;

        private AbstractInjectAction? _injectAction;

        private AbstractConfiguration? _maplestoryBotConfiguration;

        public WindowRuneSolverRoboflowAPIInjectActionHandler(
            AbstractSystemWindow systemWindow,
            AbstractWindowStateModifier roboflowAPIInjectModifier
        )
        {
            _systemWindow = systemWindow;
            _roboflowAPIInjectModifier = roboflowAPIInjectModifier;
            _injectAction = null;
            _maplestoryBotConfiguration = null;
            ((Window)_systemWindow.GetWindow()!).IsVisibleChanged += OnDependencyEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _roboflowAPIInjectModifier;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.InjectAction
                && data is AbstractInjectAction injectAction
            )
            {
                _injectAction = injectAction;
            }
            if (
                dataType is SystemInjectType.ConfigurationUpdate
                && data is MaplestoryBotConfiguration maplestoryBotConfiguration
            )
            {
                _maplestoryBotConfiguration = maplestoryBotConfiguration;
            }
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            if (!_systemWindow.Visible())
            {
                _roboflowAPIInjectModifier.Modify(
                    new WindowRuneSolverRoboflowAPIInjectModifierParameters
                    {
                        InjectAction = _injectAction,
                        MaplestoryBotConfiguration = _maplestoryBotConfiguration
                    }
                );
            }
        }
    }


    public class WindowRuneSolverRoboflowAPIInjectActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _apiInjectActionHandler;

        public WindowRuneSolverRoboflowAPIInjectActionHandlerFacade(
            AbstractSystemWindow systemWindow
        )
        {
            _apiInjectActionHandler = new WindowRuneSolverRoboflowAPIInjectActionHandler(
                systemWindow, new WindowRuneSolverRoboflowAPIInjectModifier()
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _apiInjectActionHandler.Modifier();
        }

        public override void Inject(object dataType, object? data)
        {
            _apiInjectActionHandler.Inject(dataType, data);
        }

        public override void OnDependencyEvent(
            object sender, DependencyPropertyChangedEventArgs e
        )
        {
            _apiInjectActionHandler.OnDependencyEvent(sender, e);
        }
    }


    public class WindowRuneSolverRoboflowAPIOutputModifier : AbstractWindowStateModifier
    {
        private TextBox _classTagTextBox;

        private TextBox _xTextBox;

        private TextBox _yTextBox;

        private TextBox _leftArrowTextBox;

        private TextBox _upArrowTextBox;

        private TextBox _rightArrowTextBox;

        private TextBox _downArrowTextBox;

        private TextBlock _outputFormatTextBlock;

        public WindowRuneSolverRoboflowAPIOutputModifier(
            TextBox classTagTextBox,
            TextBox xTextBox,
            TextBox yTextBox,
            TextBox leftArrowTextBox,
            TextBox upArrowTextBox,
            TextBox rightArrowTextBox,
            TextBox downArrowTextBox,
            TextBlock outputFormatTextBlock
        )
        {
            _classTagTextBox = classTagTextBox;
            _xTextBox = xTextBox;
            _yTextBox = yTextBox;
            _leftArrowTextBox = leftArrowTextBox;
            _upArrowTextBox = upArrowTextBox;
            _rightArrowTextBox = rightArrowTextBox;
            _downArrowTextBox = downArrowTextBox;
            _outputFormatTextBlock = outputFormatTextBlock;
        }

        public override void Modify(object? value)
        {
            _outputFormatTextBlock.Text = (
                "[\n" +
                "  {\n" +
                "    \"" + _xTextBox.Text + "\": <integer>\n" +
                "    \"" + _yTextBox.Text + "\": <integer>\n" +
                "    \"" + _classTagTextBox.Text + "\": <" +
                "\"" + _leftArrowTextBox.Text + "\"|" +
                "\"" + _upArrowTextBox.Text + "\"|" + 
                "\"" + _rightArrowTextBox.Text + "\"|" +
                "\"" + _downArrowTextBox.Text + "\">\n" +
                "  },\n" +
                "  ...\n" +
                "]\n"
            );
        }
    }


    public class WindowRuneSolverRoboflowAPIFormatActionHandler : AbstractWindowActionHandler
    {
        private TextBox _classTagTextBox;

        private TextBox _xTextBox;

        private TextBox _yTextBox;

        private TextBox _leftArrowTextBox;

        private TextBox _upArrowTextBox;

        private TextBox _rightArrowTextBox;

        private TextBox _downArrowTextBox;

        private AbstractWindowStateModifier _roboflowAPIOutputFormatModifier;

        public WindowRuneSolverRoboflowAPIFormatActionHandler(
            TextBox classTagTextBox,
            TextBox xTextBox,
            TextBox yTextBox,
            TextBox leftArrowTextBox,
            TextBox upArrowTextBox,
            TextBox rightArrowTextBox,
            TextBox downArrowTextBox,
            AbstractWindowStateModifier roboflowAPIOutputFormatModifier
        )
        {
            _classTagTextBox = classTagTextBox;
            _xTextBox = xTextBox;
            _yTextBox = yTextBox;
            _leftArrowTextBox = leftArrowTextBox;
            _upArrowTextBox = upArrowTextBox;
            _rightArrowTextBox = rightArrowTextBox;
            _downArrowTextBox = downArrowTextBox;
            _roboflowAPIOutputFormatModifier = roboflowAPIOutputFormatModifier;

            _classTagTextBox.TextChanged += OnEvent;
            _xTextBox.TextChanged += OnEvent;
            _yTextBox.TextChanged += OnEvent;
            _leftArrowTextBox.TextChanged += OnEvent;
            _upArrowTextBox.TextChanged += OnEvent;
            _rightArrowTextBox.TextChanged += OnEvent;
            _downArrowTextBox.TextChanged += OnEvent;
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _roboflowAPIOutputFormatModifier;
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _roboflowAPIOutputFormatModifier.Modify(null);
        }
    }


    public class WindowRuneSolverRoboflowAPIOutputActionHandlerFacade : AbstractWindowActionHandler
    {
        private AbstractWindowActionHandler _roboflowAPIOutputFormatActionHandler;

        public WindowRuneSolverRoboflowAPIOutputActionHandlerFacade(
            TextBox classTagTextBox,
            TextBox xTextBox,
            TextBox yTextBox,
            TextBox leftArrowTextBox,
            TextBox upArrowTextBox,
            TextBox rightArrowTextBox,
            TextBox downArrowTextBox,
            TextBlock outputFormatTextBlock
        )
        {
            _roboflowAPIOutputFormatActionHandler = (
                new WindowRuneSolverRoboflowAPIFormatActionHandler(
                    classTagTextBox,
                    xTextBox,
                    yTextBox,
                    leftArrowTextBox,
                    upArrowTextBox,
                    rightArrowTextBox,
                    downArrowTextBox,
                    new WindowRuneSolverRoboflowAPIOutputModifier(
                        classTagTextBox,
                        xTextBox,
                        yTextBox,
                        leftArrowTextBox,
                        upArrowTextBox,
                        rightArrowTextBox,
                        downArrowTextBox,
                        outputFormatTextBlock
                    )
                )
            );
        }

        public override AbstractWindowStateModifier Modifier()
        {
            return _roboflowAPIOutputFormatActionHandler.Modifier();
        }

        public override void OnEvent(object? sender, EventArgs e)
        {
            _roboflowAPIOutputFormatActionHandler.OnEvent(sender, e);
        }
    }
}