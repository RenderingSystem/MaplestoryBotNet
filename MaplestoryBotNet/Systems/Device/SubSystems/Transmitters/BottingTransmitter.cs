using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Macro;
using MaplestoryBotNet.Systems.UIHandler.Utilities.Models;
using MaplestoryBotNet.ThreadingUtils;
using System.Collections.Concurrent;


namespace MaplestoryBotNet.Systems.Device.SubSystems.Transmitters
{
    public enum BottingOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }


    public enum BottingExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        MaxNum
    }


    public abstract class AbstractBottingMacroCommandsSelector
    {
        public abstract List<string> SelectMacroCommands(
            List<MinimapPointMacros> macros
        );
    }


    public abstract class AbstractBottingPointDataSelector
    {
        public abstract MinimapPointData? SelectPoint(AbstractBottingModel bottingModel);
    }


    public class BottingRandomMacroCommandsSelector : AbstractBottingMacroCommandsSelector
    {
        AbstractMacroRandom _macroRandom;
        public BottingRandomMacroCommandsSelector(AbstractMacroRandom macroRandom)
        {
            _macroRandom = macroRandom;
        }

        public override List<string> SelectMacroCommands(
            List<MinimapPointMacros> macros
        )
        {
            var totalChance = 0;
            for (int i = 0; i < macros.Count; i++)
            {
                totalChance += macros[i].MacroChance;
            }
            var randomNumber = _macroRandom.Next(0, totalChance - 1);
            var cumulativeChance = 0;
            for (int i = 0; i < macros.Count; i++)
            {
                cumulativeChance += macros[i].MacroChance;
                if (randomNumber < cumulativeChance)
                {
                    return macros[i].MacroCommands;
                }
            }
            return [];
        }
    }


    public class BottingPointDataSelector : AbstractBottingPointDataSelector
    {
        private string _templateKey;

        public BottingPointDataSelector(string templateKey)
        {
            _templateKey = templateKey;
        }

        public override MinimapPointData? SelectPoint(AbstractBottingModel bottingModel)
        {
            var minimapPoints = bottingModel.GetMacroModel().MacroPoints();
            var (charX, charY) = bottingModel.GetMapModel().GetTemplatePosition(_templateKey);
            MinimapPointData? selectedMinimapPoint = null;
            var minDistanceSquared = double.PositiveInfinity;
            if (charX > -1 && charY > -1)
            {
                for (int i = 0; i < minimapPoints.Count; i++)
                {
                    var currMinimapPoint = minimapPoints[i];
                    var currX = currMinimapPoint.X + (currMinimapPoint.XRange / 2);
                    var currY = currMinimapPoint.Y + (currMinimapPoint.YRange / 2);
                    var (vX, vY) = (charX - currX, charY - currY);
                    var distanceSquared = (vX * vX) + (vY * vY);
                    if (distanceSquared < minDistanceSquared)
                    {
                        minDistanceSquared = distanceSquared;
                        selectedMinimapPoint = currMinimapPoint.PointData.Copy();
                    }
                }
            }
            return selectedMinimapPoint;
        }
    }


    public abstract class AbstractSkillMacroCommandsSelector
    {
        public abstract void Clear();

        public abstract void Update(AbstractSkillsModel skillsModel);

        public abstract List<string> Select(AbstractSkillsModel skillsModel);
    }


    public class SkillTimeout
    {
        public Skill Skill;

        public AbstractTimestamp Stopwatch;

        public double Timeout;

        public SkillTimeout(
            Skill skill,
            AbstractTimestamp stopwatch,
            double timeout
        )
        {
            Skill = skill;
            Stopwatch = stopwatch;
            Timeout = timeout;
        }
    }


    public class SkillMacroCommandsSelector : AbstractSkillMacroCommandsSelector
    {
        private object _skillsLock;

        private OrderedDictionary<string, SkillTimeout> _skillTimeouts;

        private AbstractTimestampFactory _stopwatchFactory;

        private AbstractMacroRandom _macroRandom;

        public SkillMacroCommandsSelector(
            OrderedDictionary<string, SkillTimeout> skillTimeouts,
            AbstractTimestampFactory stopwatchFactory,
            AbstractMacroRandom macroRandom
        )
        {
            _skillsLock = new object();
            _skillTimeouts = skillTimeouts;
            _stopwatchFactory = stopwatchFactory;
            _macroRandom = macroRandom;
        }

        public override void Clear()
        {
            lock (_skillsLock)
            {
                _skillTimeouts.Clear();
            }
        }

        public override void Update(
            AbstractSkillsModel skillsModel
        )
        {
            lock (_skillsLock)
            {
                var skills = skillsModel
                    .GetSkills()
                    .Where(s => s.Active != 0)
                    .ToList();

                foreach (var skill in skills)
                {
                    if (
                        !_skillTimeouts.ContainsKey(skill.Name) ||
                        _skillTimeouts[skill.Name].Skill.MinDelay != skill.MinDelay ||
                        _skillTimeouts[skill.Name].Skill.MaxDelay != skill.MaxDelay
                    )
                    {
                        var stopwatch = _stopwatchFactory.Create();
                        var min = Math.Min(skill.MinDelay, skill.MaxDelay) * 1000;
                        var max = Math.Max(skill.MinDelay, skill.MaxDelay) * 1000;
                        var random = _macroRandom.Next(min, max) / 1000.0;
                        var skillTimeout = new SkillTimeout(skill, stopwatch, random);
                        stopwatch.SetTimestamp();
                        _skillTimeouts[skill.Name] = skillTimeout;
                    }
                }

                var keysToRemove = _skillTimeouts
                    .Where(kv => !skills.Any(s => s.Name == kv.Key))
                    .Select(kv => kv.Key)
                    .ToList();
                foreach (var key in keysToRemove)
                {
                    _skillTimeouts.Remove(key);
                }
            }
        }

        public override List<string> Select(
            AbstractSkillsModel skillsModel
        )
        {
            lock (_skillsLock)
            {
                foreach (var skillTimeout in _skillTimeouts.Values)
                {
                    if (skillTimeout.Stopwatch.GetTimestamp() > skillTimeout.Timeout)
                    {
                        skillTimeout.Stopwatch.SetTimestamp();
                        skillTimeout.Timeout = _macroRandom.Next(
                            skillTimeout.Skill.MinDelay * 1000,
                            skillTimeout.Skill.MaxDelay * 1000
                        ) / 1000.0;
                        return [.. skillTimeout.Skill.Macros];
                    }
                }
                return [];
            }
        }
    }


    public abstract class AbstractBottingCommandsExecutor : IDataInjectable
    {
        public abstract bool Execute();

        public abstract void Inject(object dataType, object? data);
    }


    public class SkillCommandsExecutor : AbstractBottingCommandsExecutor
    {
        private AbstractSkillMacroCommandsSelector _skillCommandsSelector;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        private AbstractSkillsModel? _skillsModel;

        public SkillCommandsExecutor(
            AbstractSkillMacroCommandsSelector skillCommandsSelector,
            AbstractMacroCommandsExecutorBuilder macroCommandsExecutorBuilder
        )
        {
            _skillCommandsSelector = skillCommandsSelector;
            _macroCommandsExecutorBuilder = macroCommandsExecutorBuilder;
            _macroCommandsExecutor = null;
            _skillsModel = null;
        }

        public override bool Execute()
        {
            _skillCommandsSelector.Update(_skillsModel!);
            if (
                _skillCommandsSelector.Select(_skillsModel!)
                is List<string> skillCommands
                && skillCommands.Count > 0
            )
            {
                _macroCommandsExecutor!.Execute(skillCommands);
                return true;
            }
            return false;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.Transmitters &&
                data is TransmitterInfo transmitterInfo
            )
            {
                _macroCommandsExecutor = _macroCommandsExecutorBuilder
                    .WithArg(transmitterInfo)
                    .Build();
            }
            else if (
                dataType is SystemInjectType.SkillsModel &&
                data is AbstractSkillsModel skillsModel
            )
            {
                _skillsModel = skillsModel;
            }
            else if (dataType is MacroExecutorThreadedUpdate.Stopped)
            {
                _skillCommandsSelector.Clear();
            }
        }
    }


    public class BottingCommandsExecutor : AbstractBottingCommandsExecutor
    {
        private AbstractBottingPointDataSelector _pointDataSelector;

        private AbstractBottingMacroCommandsSelector _macroCommandsSelector;

        private AbstractMacroCommandsExecutorBuilder _macroCommandsExecutorBuilder;

        private AbstractMacroCommandsExecutor? _macroCommandsExecutor;

        private AbstractBottingModel? _bottingModel;

        public BottingCommandsExecutor(
            AbstractBottingPointDataSelector pointDataSelector,
            AbstractBottingMacroCommandsSelector macroCommandsSelector,
            AbstractMacroCommandsExecutorBuilder macroCommandsExecutorBuilder
        )
        {
            _pointDataSelector = pointDataSelector;
            _macroCommandsSelector = macroCommandsSelector;
            _macroCommandsExecutorBuilder = macroCommandsExecutorBuilder;
            _macroCommandsExecutor = null;
            _bottingModel = null;
        }

        public override bool Execute()
        {
            if (
                _pointDataSelector.SelectPoint(_bottingModel!)
                is MinimapPointData transmitData
            )
            {
                var commands = transmitData.Commands;
                var macroCommands = _macroCommandsSelector.SelectMacroCommands(commands);
                _macroCommandsExecutor!.Execute(macroCommands);
                return true;
            }
            return false;
        }

        public override void Inject(object dataType, object? data)
        {
            if (
                dataType is SystemInjectType.Transmitters &&
                data is TransmitterInfo transmitterInfo
            )
            {
                _macroCommandsExecutor = _macroCommandsExecutorBuilder
                    .WithArg(transmitterInfo)
                    .Build();
            }
            else if (
                dataType is SystemInjectType.BottingModel &&
                data is AbstractBottingModel bottingModel
            )
            {
                _bottingModel = bottingModel;
            }
        }
    }


    public class BottingExecutorThreadHelper : AbstractKeystrokeTransmitterThreadHelper
    {
        private List<AbstractBottingCommandsExecutor> _commandExecutors;

        public BottingExecutorThreadHelper(
            List<AbstractBottingCommandsExecutor> commandExecutors
        )
        {
            _commandExecutors = commandExecutors;
        }

        public override bool Transmit()
        {
            foreach (var executor in _commandExecutors)
            {
                if (executor.Execute())
                {
                    return true;
                }
            }
            return true;
        }

        public override void Inject(object dataType, object? data)
        {
            foreach (var executor in _commandExecutors)
            {
                executor.Inject(dataType, data);
            }
        }

        public override void Reset()
        {

        }
    }


    public class BottingExecutorThread : AbstractThread
    {
        private AbstractResetEvent _executionEvent;

        private AbstractKeystrokeTransmitterThreadHelper _bottingExecutorThreadHelper;

        private AbstractKeystrokeTransmitterThreadState _threadState;

        private AbstractThreadRunningState _transmittingState;

        public BottingExecutorThread(
            AbstractResetEvent executionEvent,
            AbstractKeystrokeTransmitterThreadHelper bottingExecutorThreadHelpers,
            AbstractKeystrokeTransmitterThreadState threadState,
            AbstractThreadRunningState transmittingState,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _executionEvent = executionEvent;
            _bottingExecutorThreadHelper = bottingExecutorThreadHelpers;
            _threadState = threadState;
            _transmittingState = transmittingState;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _executionEvent.WaitOne();
                _transmittingState.SetRunning(true);
                _bottingExecutorThreadHelper.Reset();
                while (_threadState.GetState() == (int)BottingExecutorThreadedUpdate.Started)
                {
                    if (!_bottingExecutorThreadHelper.Transmit())
                    {
                        break;
                    }
                }
                _bottingExecutorThreadHelper.Reset();
                _transmittingState.SetRunning(false);
            }
        }

        public override void Stop()
        {
            base.Stop();
            Inject(BottingOrchestratorThreadInjectType.Stop, null);
        }

        public override void Inject(object dataType, object? value)
        {
            if (dataType is BottingOrchestratorThreadInjectType injectType)
            {
                if (injectType == BottingOrchestratorThreadInjectType.Start)
                {
                    _threadState.SetState((int)BottingExecutorThreadedUpdate.Starting);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)BottingExecutorThreadedUpdate.Started);
                    _executionEvent.Set();
                }
                else if (injectType == BottingOrchestratorThreadInjectType.Stop)
                {
                    _threadState.SetState((int)BottingExecutorThreadedUpdate.Stopping);
                    while (_transmittingState.IsRunning())
                    {
                        Thread.Yield();
                    }
                    _threadState.SetState((int)BottingExecutorThreadedUpdate.Stopped);
                }
            }
            else
            {
                _bottingExecutorThreadHelper.Inject(dataType, value);
            }
        }

        public override object? State()
        {
            return _threadState;
        }
    }


    public class BottingOrchestratorThread : 
        AbstractOrchestratorThread<BottingOrchestratorThreadInjectType>
    {
        public BottingOrchestratorThread(
            AbstractThread bottingExecutorThread,
            AbstractThreadRunningState runningState,
            BlockingCollection<int> threadStates
        ) : base(bottingExecutorThread, runningState, threadStates)
        { }
    }


    public class BottingOrchestratorThreadFactory : AbstractThreadFactory
    {
        private string _templateKey;

        public BottingOrchestratorThreadFactory(string templateKey)
        {
            _templateKey = templateKey;
        }

        public override AbstractThread CreateThread()
        {
            return new BottingOrchestratorThread(
                new BottingExecutorThread(
                    new ExecutionEvent(),
                    new BottingExecutorThreadHelper(
                        [
                            new SkillCommandsExecutor(
                                new SkillMacroCommandsSelector(
                                    [],
                                    new StopwatchTimestampFactory(),
                                    new MacroRandom()
                                ),
                                new MacroCommandsExecutorBuilder()
                            ),
                            new BottingCommandsExecutor(
                                new BottingPointDataSelector(_templateKey),
                                new BottingRandomMacroCommandsSelector(new MacroRandom()),
                                new MacroCommandsExecutorBuilder()
                            )
                        ]
                    ),
                    new KeystrokeTransmitterThreadState(
                        (int)BottingExecutorThreadedUpdate.Stopped,
                        KeystrokeTransmitterThreadType.Botting
                    ),
                    new ThreadRunningState(),
                    new ThreadRunningState()
                ),
                new ThreadRunningState(),
                new BlockingCollection<int>()
            );
        }
    }


    public class BottingOrchestratorSystem : AbstractOrchestratorSystem
    {
        public BottingOrchestratorSystem(
            List<AbstractThreadFactory> threadFactories
        ) : base(threadFactories)
        { }
    }


    public class BottingOrchestratorSystemBuilder : AbstractSystemBuilder
    {
        public override AbstractSystem Build()
        {
            return new BottingOrchestratorSystem(
                [
                    new BottingOrchestratorThreadFactory(MapIconInfo.Character)
                ]
            );
        }

        public override AbstractSystemBuilder WithArg(object arg)
        {
            return this;
        }
    }
}
