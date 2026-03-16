using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.ScreenProcessing;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.ThreadingUtils;
using System.Diagnostics;


namespace MaplestoryBotNetTests.Systems.ScreenProcessing.Tests
{
    public class ScreenProcessingSystemTests
    {
        private List<AbstractThreadFactory> _threadFactories = [];

        private List<MockThread> _mockThreads = [];

        private AbstractSystem _fixture()
        {
            _threadFactories = [];
            _mockThreads = [];
            for (int i = 0; i < 10; i++)
                _threadFactories.Add(new MockThreadFactory());
            for (int i = 0; i < 10; i++)
                _mockThreads.Add(new MockThread(new ThreadRunningState()));
            for (int i = 0; i < 10; i++)
                ((MockThreadFactory)_threadFactories[i]).CreateThreadReturn.Add(_mockThreads[i]);
            return new ScreenProcessingSystem(_threadFactories);
        }

        /**
         * @brief Verifies that the screen processing system properly initializes and starts all threads.
         * 
         * The system manages multiple background threads that process different aspects of screen
         * capture (minimap, runes, character positions, etc.). During Initialization, the system must
         * create all thread instances using their factories. During Start, it must actually begin
         * execution of each created thread. This test confirms the two-phase lifecycle: creation
         * happens during initialization, and actual thread execution begins only when started.
         */
        private void _testInitializationAndStartCreatesAndStartsThreads()
        {
            var _screenProcessingSystem = _fixture();
            _screenProcessingSystem.Initialize();
            for (int i = 0; i < _threadFactories.Count; i++)
            {
                var threadFactory = ((MockThreadFactory)_threadFactories[i]);
                var thread = _mockThreads[i];
                Debug.Assert(threadFactory.CreateThreadCalls == 1);
                Debug.Assert(thread.ThreadStartCalls == 0);

            }
            _screenProcessingSystem.Start();
            for (int i = 0; i < _mockThreads.Count; i++)
            {
                Debug.Assert(_mockThreads[i].ThreadStartCalls == 1);
            }
        }

        /**
         * @brief Verifies that the system can inject an action that receives each created thread.
         * 
         * When a client injects an action, the system must iterate through all managed threads and
         * call the provided action for each one, passing the dependency and the thread instance.
         * This allows external components to or configure needing direct access to the
         * thread collection.
         */
        public void _testInjectActionWithCreatedThreads()
        {
            var _screenProcessingSystem = _fixture();
            var dataTypes = new List<SystemInjectType>();
            var dataList = new List<object>();
            _screenProcessingSystem.Initialize();
            _screenProcessingSystem.Inject(
                SystemInjectType.InjectAction,
                (SystemInjectType dataType, object data) => {
                    dataTypes.Add(dataType);
                    dataList.Add(data);
                }
            );
            Debug.Assert(dataTypes.Count == _mockThreads.Count);
            Debug.Assert(dataList.Count == _mockThreads.Count);
            for (int i = 0; i < _mockThreads.Count; i++)
            {
                Debug.Assert(dataTypes[i] == SystemInjectType.ThreadDependency);
                Debug.Assert(dataList[i] == _mockThreads[i]);
            }

        }

        /**
         * @brief Verifies that the system broadcasts injections to all managed threads.
         * 
         * When configuration data or runtime updates need to reach all screen processing threads,
         * the system must forward the injection to every thread it manages. This test confirms
         * that a single Inject call on the system results in each thread receiving the same
         * data type and value.
         */
        public void _testInjectWithCreatedThreads()
        {
            var _screenProcessingSystem = _fixture();
            _screenProcessingSystem.Initialize();
            _screenProcessingSystem.Inject((SystemInjectType)123, 234);
            for(int i = 0; i < _mockThreads.Count; i++)
            {
                Debug.Assert(_mockThreads[i].InjectCalls == 1);
                Debug.Assert((int)_mockThreads[i].InjectCallArg_dataType[0] == 123);
                Debug.Assert((int)_mockThreads[i].InjectCallArg_data[0]! == 234);
            }

        }

        public void Run()
        {
            _testInitializationAndStartCreatesAndStartsThreads();
            _testInjectActionWithCreatedThreads();
            _testInjectWithCreatedThreads();
        }
    }


    public class ScreenProcessingSystemTestSuite
    {
        public void Run()
        {
            new ScreenProcessingSystemTests().Run();
        }
    }
}
