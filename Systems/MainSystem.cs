using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.Keyboard;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.ThreadingUtils;


namespace MaplestoryBotNet.Systems
{

    public class MainSubSystem : AbstractSystem
    {
        private List<SubSystemInformation> _initializationList;

        private List<SubSystemInformation> _startList;

        private List<SubSystemInformation> _updateList;

        private void _buildSubSystem(List<SubSystemInformation> subSystemInfo)
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
                var dependencies = info.BuildDependencies;
                _buildSubSystem(dependencies);
                foreach (var dependency in dependencies)
                {
                    if (dependency.System != null)
                    {
                        info.SystemBuilder.WithArg(dependency.System);
                    }
                }
                info.System = info.SystemBuilder.Build();
            }
        }

        public MainSubSystem(List<SubSystemInformation> subSystemInfo)
        {
            _initializationList = subSystemInfo.OrderBy(info => info.InitializationPriority).ToList();
            _startList = subSystemInfo.OrderBy(info => info.StartPriority).ToList();
            _updateList = subSystemInfo.OrderBy(info => info.UpdatePriority).ToList();
            _buildSubSystem(subSystemInfo);
        }

        public override void InitializeSystem()
        {
            for (int i = 0; i < _initializationList.Count; i++)
            {
                var system = _initializationList[i].System;
                if (system != null)
                {
                    system.InitializeSystem();
                }
            }
        }

        public override void StartSystem()
        {
            for (int i = 0; i < _startList.Count; i++)
            {
                var system = _startList[i].System;
                if (system != null)
                {
                    system.StartSystem();
                }
            }
        }

        public override void UpdateSystem()
        {
            for (int i = 0; i < _updateList.Count; i++)
            {
                var system = _updateList[i].System;
                if (system != null)
                {
                    system.UpdateSystem();
                }
            }
        }
    }


    public class MainSubSystemThread : AbstractThread
    {
        private AbstractSystem _mainSystem;

        public MainSubSystemThread(
            AbstractSystem mainSystem,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _mainSystem = mainSystem;
        }

        public override void ThreadLoop()
        {
            _mainSystem.InitializeSystem();
            _mainSystem.StartSystem();
            while (_runningState.IsRunning())
            {
                _mainSystem.UpdateSystem();
            }
        }
    }


    public class MainSubSystemInfoList : AbstractSubSystemInfoList
    {
        public override List<SubSystemInformation> GetSubSystemInfo()
        {
            var keyboardInfo = new SubSystemInformation(
                new KeyboardSystemBuilder(), [], 2, 2, 2
            );
            var screenCaptureInfo = new SubSystemInformation(
                new GameScreenCaptureSystemBuilder(), [], 1, 1, 1
            );
            var configInfo = new SubSystemInformation(
                new ConfigurationSystemBuilder(), [keyboardInfo, screenCaptureInfo], 0, 0, 0
            );
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

        public override void InitializeSystem()
        {
            _mainSubSystemThread = _mainSubSystemThreadFactory.CreateThread();
        }

        public override void StartSystem()
        {
            if (_mainSubSystemThread != null)
            {
                _mainSubSystemThread.ThreadStart();
            }
        }
    }
}
