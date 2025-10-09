﻿using System.Diagnostics;
using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNet.UserInterface;


namespace MaplestoryBotNet.Systems
{
    public class MainSubSystem : AbstractSystem
    {
        private List<SystemInformation> _initializationList;

        private List<SystemInformation> _startList;

        private List<SystemInformation> _updateList;

        private void _buildSubSystem(List<SystemInformation> subSystemInfo)
        {
            if (subSystemInfo.Count == 0)
            {
                return;
            }
            for (int i = 0; i < subSystemInfo.Count; i++)
            {
                if (subSystemInfo[i].System != null)
                {
                    continue;
                }
                var info = subSystemInfo[i];
                _buildSubSystem(info.BuildDependencies);
                foreach (var dependency in info.BuildDependencies)
                {
                    if (dependency.System != null)
                    {
                        info.SystemBuilder.WithArg(dependency.System);
                    }
                }
                foreach (var obj in info.BuildObjects)
                {
                    if (obj != null)
                    {
                        info.SystemBuilder.WithArg(obj);
                    }
                }
                info.System = info.SystemBuilder.Build();
            }
        }

        public MainSubSystem(List<SystemInformation> subSystemInfo)
        {
            _initializationList = subSystemInfo.OrderBy(info => info.InitializationPriority).ToList();
            _startList = subSystemInfo.OrderBy(info => info.StartPriority).ToList();
            _updateList = subSystemInfo.OrderBy(info => info.UpdatePriority).ToList();
            _buildSubSystem(subSystemInfo);
        }

        public override void Initialize()
        {
            for (int i = 0; i < _initializationList.Count; i++)
            {
                var system = _initializationList[i].System;
                if (system != null)
                {
                    system.Initialize();
                }
            }
        }

        public override void Start()
        {
            for (int i = 0; i < _startList.Count; i++)
            {
                var system = _startList[i].System;
                if (system != null)
                {
                    system.Start();
                }
            }
        }

        public override void Update()
        {
            for (int i = 0; i < _updateList.Count; i++)
            {
                var system = _updateList[i].System;
                if (system != null)
                {
                    system.Update();
                }
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            for (int i = 0; i < _updateList.Count; i++)
            {
                var system = _updateList[i].System;
                if (system != null)
                {
                    system.Inject(dataType, data);
                }
            }
        }
    }


    public class MainSubSystemThread : AbstractThread
    {
        private AbstractSystem _mainSubSystem;

        private bool _mainSubSystemStarted;

        private ReaderWriterLockSlim _mainSubSystemStartedLock;

        public MainSubSystemThread(
            AbstractSystem mainSubSystem,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _mainSubSystem = mainSubSystem;
            _mainSubSystemStarted = false;
            _mainSubSystemStartedLock = new ReaderWriterLockSlim();
        }

        public override void ThreadLoop()
        {
            _mainSubSystem.Initialize();
            _mainSubSystem.Start();
            try
            {
                _mainSubSystemStartedLock.EnterWriteLock();
                _mainSubSystemStarted = true;
            }
            finally
            {
                _mainSubSystemStartedLock.ExitWriteLock();
            }
            while (_runningState.IsRunning())
            {
                _mainSubSystem.Update();
            }
        }

        public override void Inject(SystemInjectType dataType, object? value)
        {
            _mainSubSystem.Inject(dataType, value);
        }

        public override object? State()
        {
            try
            {
                _mainSubSystemStartedLock.EnterReadLock();
                return _mainSubSystemStarted;
            }
            finally
            {
                _mainSubSystemStartedLock.ExitReadLock();
            }
        }
    }


    public class MainSubSystemInfoList : AbstractSubSystemInfoList
    {
        private SystemInformation _keyboardInfo()
        {
            return new SystemInformation(
                new KeyboardSystemBuilder(), [], [], 2, 2, 2
            );
        }

        private SystemInformation _screenCaptureInfo()
        {
            var semaphore = new SemaphoreSlim(0, 2);
            return new SystemInformation(
                new GameScreenCaptureSystemBuilder(),
                [],
                [
                    new NullScreenCaptureSubscriber(semaphore),
                    new GameScreenCaptureSubscriber(semaphore)
                ],
                1,
                1,
                1
            );
        }

        private SystemInformation _configInfo(List<SystemInformation> dependencies)
        {
            return new SystemInformation(
                new ConfigurationSystemBuilder(), dependencies, [], 0, 0, 0
            );
        }

        public override List<SystemInformation> GetSubSystemInfo()
        {
            var keyboardInfo = _keyboardInfo();
            var screenCaptureInfo = _screenCaptureInfo();
            var configInfo = _configInfo([keyboardInfo, screenCaptureInfo]);
            return [configInfo, keyboardInfo, screenCaptureInfo];
        }
    }


    public class MainSubSystemsThreadFactory : AbstractThreadFactory
    {
        AbstractSubSystemInfoList _subSystemsInfoList;

        public MainSubSystemsThreadFactory(AbstractSubSystemInfoList subSystemInfoList)
        {
            _subSystemsInfoList = subSystemInfoList;
        }

        public override AbstractThread CreateThread()
        {
            return new MainSubSystemThread(
                new MainSubSystem(_subSystemsInfoList.GetSubSystemInfo()),
                new ThreadRunningState()
            );
        }
    }


    public class MainSystem : AbstractSystem
    {
        private AbstractThreadFactory _mainSubSystemThreadFactory;

        private AbstractThread? _mainSubSystemThread;

        public MainSystem(AbstractThreadFactory mainSubSystemThreadFactory)
        {
            _mainSubSystemThreadFactory = mainSubSystemThreadFactory;
        }

        public override void Initialize()
        {
            _mainSubSystemThread = _mainSubSystemThreadFactory.CreateThread();
        }

        public override void Start()
        {
            if (_mainSubSystemThread != null)
            {
                _mainSubSystemThread.Start();
            }
        }

        public override void Inject(SystemInjectType dataType, object? data)
        {
            if (_mainSubSystemThread != null)
            {
                _mainSubSystemThread.Inject(dataType, data);
            }
        }

        public override object? State()
        {
            if (_mainSubSystemThread != null)
            {
                return _mainSubSystemThread.State();
            }
            return null;
        }
    }


    public class MainApplication : AbstractApplication
    {
        private AbstractSystem _mainSystem;

        public MainApplication(AbstractSystem mainSystem)
        {
            _mainSystem = mainSystem;
        }

        public override void Launch(List<string> args)
        {
            _mainSystem.Initialize();
            _mainSystem.Start();
        }

        public override void ShutDown()
        {
            _mainSystem.Inject(SystemInjectType.ShutDown, true);
        }

        public override AbstractSystem System()
        {
            return _mainSystem;
        }
    }


    public class MainApplicationFacade : AbstractApplication
    {
        private AbstractApplication _mainApplication;

        public MainApplicationFacade()
        {
            _mainApplication = new MainApplication(
                new MainSystem(
                    new MainSubSystemsThreadFactory(
                        new MainSubSystemInfoList()
                    )
                )
            );
        }

        public override void Launch(List<string> args)
        {
            _mainApplication.Launch(args);
        }

        public override void ShutDown()
        {
            _mainApplication.ShutDown();
        }

        public override AbstractSystem System()
        {
            return _mainApplication.System();
        }
    }


    public class MainApplicationInitializer : AbstractApplicationInitializer
    {
        AbstractApplication _mainApplication;

        AbstractWindowActionHandler _windowViewUpdaterActionHandler;

        AbstractWindowActionHandler _windowViewCheckboxActionHandler;

        public MainApplicationInitializer(
            AbstractApplication mainApplication,
            AbstractWindowActionHandler windowViewUpdaterActionHandler,
            AbstractWindowActionHandler windowViewCheckboxActionHandler
        )
        {
            _mainApplication = mainApplication;
            _windowViewUpdaterActionHandler = windowViewUpdaterActionHandler;
            _windowViewCheckboxActionHandler = windowViewCheckboxActionHandler;
        }

        public override void Synchronize()
        {
            var mainSystem = _mainApplication.System();
            object? state = null;
            do
            {
                state = mainSystem.State();
            } while (state == null || (bool)state == false);
        }

        public override void Initialize()
        {
            var mainSystem = _mainApplication.System();
            var viewUpdateModifier = _windowViewUpdaterActionHandler.Modifier();
            var viewCheckboxModifier = _windowViewCheckboxActionHandler.Modifier();
            mainSystem.Inject(SystemInjectType.ViewModifier, viewUpdateModifier);
            mainSystem.Inject(SystemInjectType.ViewCheckbox, viewCheckboxModifier);
        }
    }


    public class MainApplicationInitializerBuilder : AbstractApplicationInitializerBuilder
    {
        AbstractApplication? _mainApplication;

        AbstractWindowActionHandler? _windowViewUpdaterActionHandler;

        AbstractWindowActionHandler? _windowViewCheckboxActionHandler;

        public MainApplicationInitializerBuilder()
        {
            _mainApplication = null;
            _windowViewUpdaterActionHandler = null;
            _windowViewCheckboxActionHandler = null;
        }

        public override AbstractApplicationInitializer Build()
        {
            Debug.Assert(_mainApplication != null);
            Debug.Assert(_windowViewUpdaterActionHandler != null);
            Debug.Assert(_windowViewCheckboxActionHandler != null);
            return new MainApplicationInitializer(
                _mainApplication,
                _windowViewUpdaterActionHandler,
                _windowViewCheckboxActionHandler
            );
        }

        public override AbstractApplicationInitializerBuilder WithApplication(AbstractApplication application)
        {
            _mainApplication = application;
            return this;
        }

        public override AbstractApplicationInitializerBuilder WithViewCheckboxActionHandler(AbstractWindowActionHandler handler)
        {
            _windowViewCheckboxActionHandler = handler;
            return this;
        }

        public override AbstractApplicationInitializerBuilder WithViewUpdaterActionHandler(AbstractWindowActionHandler handler)
        {
            _windowViewUpdaterActionHandler = handler;
            return this;
        }
    }

}
