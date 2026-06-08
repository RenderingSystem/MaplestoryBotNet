using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.Device;
using MaplestoryBotNet.Systems.Device.SubSystems;
using MaplestoryBotNet.Systems.Device.SubSystems.Transmitters;
using MaplestoryBotNet.Systems.Macro;
using MaplestoryBotNet.Systems.UIHandler.Utilities.Models;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Device.Tests.SubSystems.Mocks;
using MaplestoryBotNetTests.Systems.Device.Tests.SubSystems.Transmitters.Mocks;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.TestHelpers;
using MaplestoryBotNetTests.ThreadingUtils;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;


namespace MaplestoryBotNetTests.Systems.Device.Tests.SubSystems.Transmitters
{
    public class RandomBottingMacroCommandsSelectorTests
    {
        private List<MinimapPointMacros> _minimapPointMacros = [];

        private MockMacroRandom _macroRandom = new MockMacroRandom();

        private AbstractBottingMacroCommandsSelector _fixture(int macroCount)
        {
            _macroRandom = new MockMacroRandom();
            _minimapPointMacros = [];
            for (int i = 0; i < macroCount; i++)
            {
                _macroRandom.NextReturn.Add(10 + (20 * i));
                _minimapPointMacros.Add(
                    new MinimapPointMacros
                    {
                        MacroChance = 20,
                        MacroCommands = [i.ToString()]
                    }
                );
            }
            return new BottingRandomMacroCommandsSelector(_macroRandom);
        }

        /**
         * @brief Verifies that macro commands are selected based on random chance
         * 
         * When users configure multiple macro points with trigger probabilities,
         * the system should evaluate each point in order and randomly determine
         * whether to execute its commands. This test ensures the selection logic
         * correctly uses the random values to determine which macro run.
         */
        private void _testSelectMacroCommand()
        {
            var macroCount = 5;
            var macroCommandsSelector = _fixture(macroCount);
            for (int i = 0; i < macroCount; i++)
            {
                var commands = macroCommandsSelector.SelectMacroCommands(_minimapPointMacros);
                Debug.Assert(_minimapPointMacros[i].MacroCommands == commands);
                Debug.Assert(commands.Count == 1);
                Debug.Assert(commands[0] == i.ToString());
            }
        }

        public void Run()
        {
            _testSelectMacroCommand();
        }
    }


    public class BottingPointDataSelectorTests
    {
        private BottingModel _bottingModel = new BottingModel();

        private List<RectangleF> _minimapRects()
        {
            return [
                new RectangleF { X = -5.0f, Y= -5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  5.0f, Y= -5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X = -5.0f, Y=  5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  5.0f, Y=  5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  0.0f, Y=  0.0f, Width = 10.0f, Height = 10.0f}
            ];
        }

        private List<PointF> _closestPoints()
        {
            return [
                new PointF(1.0f, 1.0f),
                new PointF(9.0f, 1.0f),
                new PointF(1.0f, 9.0f),
                new PointF(9.0f, 9.0f),
                new PointF(4.0f, 5.0f)
            ];
        }

        private AbstractBottingPointDataSelector _fixture()
        {
            _bottingModel = new BottingModel();
            var minimapRects = _minimapRects();
            for (int i = 0; i < minimapRects.Count(); i++)
            {
                _bottingModel.GetMacroModel().AddMacroPoint(
                    new MinimapPoint
                    {
                        X = minimapRects[i].X,
                        Y = minimapRects[i].Y,
                        XRange = minimapRects[i].Width,
                        YRange = minimapRects[i].Height,
                        PointData = new MinimapPointData{ElementName = i.ToString()},
                    }
                );
            }
            return new BottingPointDataSelector("some key");
        }

        /**
         * @brief Verifies that the closest minimap point is correctly identified
         * 
         * When users move around the game world, the system should always select
         * the nearest predefined point based on the player's current position.
         * This test ensures the distance calculation works correctly across
         * different regions of the minimap.
         */
        private void _testSelectPointSelectsClosestPoint()
        {
            var closestPoints = _closestPoints();
            var pointDataSelector = _fixture();
            for (int i = 0; i < closestPoints.Count(); i++)
            {
                var (pX, pY) = ((int)closestPoints[i].X, (int)closestPoints[i].Y);
                _bottingModel.GetMapModel().SetTemplatePosition("some key", pX, pY);
                var result = pointDataSelector.SelectPoint(_bottingModel);
                Debug.Assert(result!.ElementName == i.ToString());
            }
        }

        /**
         * @brief Verifies that the botting point data selector returns null when the
         * character's position cannot be detected on the minimap
         * 
         * When the bot's image recognition fails to locate the character marker on the
         * minimap (e.g., due to map transition, UI overlay, or detection threshold issues),
         * the system should not attempt to select a nearest point. Returning null allows
         * the calling code to handle this gracefully, such as retrying the detection or
         * waiting for the character position to become available again.
         */
        private void _testSelectPointSelectsNoPointOnInvalidPosition()
        {
            var closestPoints = _closestPoints();
            var pointDataSelector = _fixture();
            _bottingModel.GetMapModel().SetTemplatePosition("some key", -1, -1);
            var result = pointDataSelector.SelectPoint(_bottingModel);
            Debug.Assert(result == null);
        }

        public void Run()
        {
            _testSelectPointSelectsClosestPoint();
            _testSelectPointSelectsNoPointOnInvalidPosition();
        }
    }


    public class SkillMacroCommandsSelectorTests
    {
        private OrderedDictionary<string, SkillTimeout> _skillTimeouts = [];

        private MockTimestampFactory _stopwatchFactory = new MockTimestampFactory();

        private MockMacroRandom _macroRandom = new MockMacroRandom();

        private AbstractSkillsModel _skillsModel = new SkillsModel();

        private AbstractSkillMacroCommandsSelector _fixture()
        {
            _skillTimeouts = _timeouts();
            _stopwatchFactory = new MockTimestampFactory();
            _macroRandom = new MockMacroRandom();
            _skillsModel = new SkillsModel();
            return new SkillMacroCommandsSelector(
                _skillTimeouts,
                _stopwatchFactory,
                _macroRandom
            );
        }
        
        private List<Skill> _skills()
        {
            return [
                new Skill
                {
                    Name = "skill1",
                    Active = 123,
                    MinDelay = 234,
                    MaxDelay = 345,
                    Macros = ["macro1", "macro2"]
                },
                new Skill
                {
                    Name = "skill2",
                    Active = 0,
                    MinDelay = 12,
                    MaxDelay = 23,
                    Macros = ["macro2", "macro3"]
                },
                new Skill
                {
                    Name = "skill3",
                    Active = 234,
                    MinDelay = 345,
                    MaxDelay = 456,
                    Macros = ["macro3", "macro4"]
                },
                new Skill
                {
                    Name = "skill4",
                    Active = 0,
                    MinDelay = 23,
                    MaxDelay = 34,
                    Macros = ["macro4", "macro5"]
                },
                new Skill
                {
                    Name = "skill5",
                    Active = 0,
                    MinDelay = 34,
                    MaxDelay = 45,
                    Macros = ["macro5", "macro6"]
                }
            ];
        }

        private OrderedDictionary<string, SkillTimeout> _timeouts()
        {
            var skillTimeouts = new OrderedDictionary<string, SkillTimeout>();
            skillTimeouts["s1"] = new SkillTimeout(new Skill { Macros = ["1", "2"] }, new MockTimestamp(), 10.0);
            skillTimeouts["s2"] = new SkillTimeout(new Skill { Macros = ["2", "3"] }, new MockTimestamp(), 10.0);
            skillTimeouts["s3"] = new SkillTimeout(new Skill { Macros = ["3", "4"] }, new MockTimestamp(), 10.0);
            return skillTimeouts;
        }

        /**
         * @brief Tests that clearing the skill timeout dictionary removes all tracked skills
         * 
         * When the bot clears its skill tracking state (e.g., during configuration reload
         * or when the user stops the bot), all previously tracked skill cooldowns should
         * be removed. This prevents old skill states from persisting across different
         * automation sessions or skill configurations.
         */
        private void _testClearingSkillTimeouts()
        {
            var selector = _fixture();
            for (int i = 0; i < 10; i++)
            {
                _skillTimeouts.Add(
                    "meow" + i.ToString(),
                    new SkillTimeout(new Skill(), new StopwatchTimestamp(), 123)
                );
            }
            selector.Clear();
            Debug.Assert(_skillTimeouts.Count == 0);
        }

        /**
         * @brief Tests that only active skills are added to the timeout tracking system
         * 
         * When updating skill timers from the skills model, the bot must only track skills
         * marked as active (Active != 0). Inactive skills should be ignored and not receive
         * stopwatches or cooldown timers. This ensures the bot doesn't waste resources
         * tracking disabled skills and doesn't accidentally execute skills the user has
         * turned off.
         */
        private void _testUpdateSkillTimerAddsActiveSkills()
        {
            var selector = _fixture();
            _skillsModel.SetSkills(_skills());
            var skills = _skills()
                .Where(s => s.Name == "skill1" || s.Name == "skill3")
                .ToList();
            for (int i = 0; i < skills.Count; i++)
            {
                _stopwatchFactory.CreateReturn.Add(new MockTimestamp());
                _macroRandom.NextReturn.Add(0);
            }
            selector.Update(_skillsModel);
            Debug.Assert(_skillTimeouts.Count == 2);
            Debug.Assert(_skillTimeouts.ContainsKey("skill1"));
            Debug.Assert(_skillTimeouts.ContainsKey("skill3"));
            var skillTimeouts = new[] { _skillTimeouts["skill1"], _skillTimeouts["skill3"] };
            for (int i = 0; i < skills.Count; i++)
            {
                Debug.Assert(skills[i].Active == skillTimeouts[i].Skill.Active);
                Debug.Assert(skills[i].Name == skillTimeouts[i].Skill.Name);
                Debug.Assert(skills[i].MinDelay == skillTimeouts[i].Skill.MinDelay);
                Debug.Assert(skills[i].MaxDelay == skillTimeouts[i].Skill.MaxDelay);
                Debug.Assert(skills[i].Macros.Count == skillTimeouts[i].Skill.Macros.Count);
                for (int j = 0; j < skills[i].Macros.Count; j++)
                {
                    Debug.Assert(skills[i].Macros[j] == skillTimeouts[i].Skill.Macros[j]);
                }
            }
        }

        /**
         * @brief Tests that each active skill receives a stopwatch for tracking cooldown elapsed time
         * 
         * For each active skill, the bot must create a stopwatch that records when the skill
         * was last used. This stopwatch is used to measure elapsed time against the skill's
         * randomized cooldown period to determine when the skill becomes available again.
         */
        private void _testUpdateSkillTimerSetsStopwatches()
        {
            var selector = _fixture();
            _skillsModel.SetSkills(_skills());
            var stopwatches = new[] { new MockTimestamp(), new MockTimestamp() };
            _stopwatchFactory.CreateReturn.Add(stopwatches[0]);
            _stopwatchFactory.CreateReturn.Add(stopwatches[1]);
            _macroRandom.NextReturn.Add(0);
            _macroRandom.NextReturn.Add(0);
            selector.Update(_skillsModel);
            var skillTimeouts = new[] { _skillTimeouts["skill1"], _skillTimeouts["skill3"] };
            Debug.Assert(stopwatches[0].SetTimestampCalls == 1);
            Debug.Assert(stopwatches[1].SetTimestampCalls == 1);
            Debug.Assert(skillTimeouts[0].Stopwatch == stopwatches[0]);
            Debug.Assert(skillTimeouts[1].Stopwatch == stopwatches[1]);
        }

        /**
         * @brief Tests that each active skill receives a randomized cooldown timeout within its
         * delay range
         * 
         * To appear more human-like and avoid detection, the bot randomizes the cooldown
         * period for each skill between MinDelay and MaxDelay (converted from seconds to
         * milliseconds). This prevents the bot from using skills on predictable, fixed
         * intervals that anti-cheat systems might detect as automation.
         */
        private void _testUpdateSkillTimerSetsTimeouts()
        {
            var selector = _fixture();
            _skillsModel.SetSkills(_skills());
            _stopwatchFactory.CreateReturn.Add(new MockTimestamp());
            _stopwatchFactory.CreateReturn.Add(new MockTimestamp());
            _macroRandom.NextReturn.Add(1234);
            _macroRandom.NextReturn.Add(2345);
            selector.Update(_skillsModel);
            var skillTimeouts = new[] { _skillTimeouts["skill1"], _skillTimeouts["skill3"] };
            Debug.Assert(skillTimeouts[0].Timeout == 1.234);
            Debug.Assert(skillTimeouts[1].Timeout == 2.345);
            Debug.Assert(_macroRandom.NextCalls == 2);
            Debug.Assert(_macroRandom.NextCallArg_minValue[0] == 234000);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[0] == 345000);
            Debug.Assert(_macroRandom.NextCallArg_minValue[1] == 345000);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[1] == 456000);
        }

        /**
         * @brief Tests that skill timers are correctly updated when skill configurations change
         * 
         * When the user modifies a skill's configuration (such as changing its name, delays,
         * or macros), the bot must detect these changes and create new stopwatches for the
         * updated skills. This ensures that changes to skill delay parameters take effect
         * immediately without requiring a bot restart.
         */
        private void _testUpdateSkillTimerUpdatesStopwatches()
        {
            var selector = _fixture();
            _skillsModel.SetSkills(_skills());
            var stopwatches = new[]
            {
                new MockTimestamp(),
                new MockTimestamp(),
                new MockTimestamp(),
                new MockTimestamp(),
            };
            _stopwatchFactory.CreateReturn.Add(stopwatches[0]);
            _stopwatchFactory.CreateReturn.Add(stopwatches[1]);
            _stopwatchFactory.CreateReturn.Add(stopwatches[2]);
            _stopwatchFactory.CreateReturn.Add(stopwatches[3]);
            _macroRandom.NextReturn.Add(0);
            _macroRandom.NextReturn.Add(0);
            _macroRandom.NextReturn.Add(0);
            _macroRandom.NextReturn.Add(0);
            selector.Update(_skillsModel);
            selector.Update(_skillsModel);
            Debug.Assert(_skillTimeouts["skill1"].Stopwatch == stopwatches[0]);
            Debug.Assert(_skillTimeouts["skill3"].Stopwatch == stopwatches[1]);
            Debug.Assert(stopwatches[0].SetTimestampCalls == 1);
            Debug.Assert(stopwatches[1].SetTimestampCalls == 1);
            _skillsModel.SetSkill(
                new Skill
                {
                    Name = "skill1",
                    Active = 123,
                    MinDelay = 12,
                    MaxDelay = 345,
                    Macros = ["macro1", "macro2"]
                }
            );
            _skillsModel.SetSkill(
                new Skill
                {
                    Name = "skill3",
                    Active = 234,
                    MinDelay = 345,
                    MaxDelay = 1234,
                    Macros = ["macro3", "macro4"]
                }
            );
            selector.Update(_skillsModel);
            Debug.Assert(_skillTimeouts["skill1"].Stopwatch == stopwatches[2]);
            Debug.Assert(_skillTimeouts["skill3"].Stopwatch == stopwatches[3]);
            Debug.Assert(stopwatches[2].SetTimestampCalls == 1);
            Debug.Assert(stopwatches[3].SetTimestampCalls == 1);
        }

        /**
         * @brief Tests that skill timeout values are recalculated when skill delays change
         * 
         * When the user modifies a skill's MinDelay or MaxDelay values, the bot must
         * regenerate the randomized cooldown timeout using the new delay range. This ensures
         * that updated delay settings are immediately applied to the skill's cooldown
         * behavior.
         */
        private void _testUpdateSkillTimerUpdatesTimeouts()
        {
            var selector = _fixture();
            _skillsModel.SetSkills(_skills());
            _stopwatchFactory.CreateReturn.Add(new MockTimestamp());
            _stopwatchFactory.CreateReturn.Add(new MockTimestamp());
            _stopwatchFactory.CreateReturn.Add(new MockTimestamp());
            _stopwatchFactory.CreateReturn.Add(new MockTimestamp());
            _macroRandom.NextReturn.Add(1234);
            _macroRandom.NextReturn.Add(2345);
            _macroRandom.NextReturn.Add(3456);
            _macroRandom.NextReturn.Add(4567);
            selector.Update(_skillsModel);
            selector.Update(_skillsModel);
            Debug.Assert(_macroRandom.NextCalls == 2);
            Debug.Assert(_skillTimeouts["skill1"].Timeout == 1.234);
            Debug.Assert(_skillTimeouts["skill3"].Timeout == 2.345);
            _skillsModel.SetSkill(
                new Skill
                {
                    Name = "skill1",
                    Active = 123,
                    MinDelay = 12,
                    MaxDelay = 345,
                    Macros = ["macro1", "macro2"]
                }
            );
            _skillsModel.SetSkill(
                new Skill
                {
                    Name = "skill3",
                    Active = 234,
                    MinDelay = 345,
                    MaxDelay = 1234,
                    Macros = ["macro3", "macro4"]
                }
            );
            selector.Update(_skillsModel);
            Debug.Assert(_skillTimeouts.Count == 2);
            Debug.Assert(_skillTimeouts["skill1"].Timeout == 3.456);
            Debug.Assert(_skillTimeouts["skill3"].Timeout == 4.567);
            Debug.Assert(_macroRandom.NextCalls == 4);
            Debug.Assert(_macroRandom.NextCallArg_minValue[2] == 12000);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[2] == 345000);
            Debug.Assert(_macroRandom.NextCallArg_minValue[3] == 345000);
            Debug.Assert(_macroRandom.NextCallArg_maxValue[3] == 1234000);
        }

        /**
         * @brief Tests that skills marked inactive are removed from the timeout tracking system
         * 
         * When the user deactivates a skill (Active = 0) that was previously active, the bot
         * must remove that skill from the timeout dictionary. This prevents the bot from
         * continuing to track cooldowns for skills that are no longer enabled and ensures
         * that disabled skills are never executed during combat automation.
         */
        private void _testUpdateSkillTimerRemovesInactiveSkills()
        {
            var selector = _fixture();
            _skillsModel.SetSkills(_skills());
            _stopwatchFactory.CreateReturn.Add(new MockTimestamp());
            _stopwatchFactory.CreateReturn.Add(new MockTimestamp());
            _macroRandom.NextReturn.Add(1234);
            _macroRandom.NextReturn.Add(2345);
            selector.Update(_skillsModel);
            _skillsModel.SetSkill(
                new Skill
                {
                    Name = "skill3",
                    Active = 0,
                    MinDelay = 345,
                    MaxDelay = 1234,
                    Macros = ["macro3", "macro4"]
                }
            );
            selector.Update(_skillsModel);
            Debug.Assert(_skillTimeouts.ContainsKey("skill1"));
            Debug.Assert(!_skillTimeouts.ContainsKey("skill3"));
        }

        /**
         * @brief Tests that when a skill's cooldown has expired, its macro commands are returned
         * 
         * This test creates three skills with different elapsed times:
         * - s1: 9.99s elapsed (not ready - still on cooldown)
         * - s2: 10.01s elapsed (ready - cooldown expired)
         * - s3: 10.00s elapsed (not ready - equal does not exceed threshold)
         */
        private void _testSelectSkillMacroCommandsReturnsReadySkillMacros()
        {
            var selector = _fixture();
            _macroRandom.NextReturn.Add(1234);
            ((MockTimestamp)_skillTimeouts["s1"].Stopwatch).GetTimestampReturn.Add(9.99);
            ((MockTimestamp)_skillTimeouts["s2"].Stopwatch).GetTimestampReturn.Add(10.01);
            ((MockTimestamp)_skillTimeouts["s3"].Stopwatch).GetTimestampReturn.Add(10.00);
            var result = selector.Select(_skillsModel);
            Debug.Assert(result.Count == 2);
            Debug.Assert(result[0] == "2");
            Debug.Assert(result[1] == "3");
        }

        /**
         * @brief Tests that when a skill's cooldown expires, its timer is reset for the next cycle
         * 
         * When a skill is selected for execution (s2 with elapsed 10.01 > timeout 10.0),
         * the bot must reset that skill's stopwatch and generate a new random timeout.
         * This prevents the skill from being used again immediately and creates randomized
         * cooldown periods for more human-like behavior.
         */
        private void _testSelectSkillMacroCommandsResetsSkillTimer()
        {
            var selector = _fixture();
            _macroRandom.NextReturn.Add(1234);
            ((MockTimestamp)_skillTimeouts["s1"].Stopwatch).GetTimestampReturn.Add(9.99);
            ((MockTimestamp)_skillTimeouts["s2"].Stopwatch).GetTimestampReturn.Add(10.01);
            ((MockTimestamp)_skillTimeouts["s3"].Stopwatch).GetTimestampReturn.Add(10.00);
            var result = selector.Select(_skillsModel);
            Debug.Assert(((MockTimestamp)_skillTimeouts["s2"].Stopwatch).SetTimestampCalls == 1);
            Debug.Assert(_skillTimeouts["s2"].Timeout == 1.234);
        }

        /**
         * @brief Tests that skills are checked in insertion order and each can be selected over time
         * 
         * Skills are stored in an ordered dictionary and evaluated in the order they were added.
         * This test performs two sequential Select calls:
         * 
         * First call: s1 has 0s elapsed (not ready), s2 has 10.01s elapsed (ready).
         * The selector returns s2's macros ["2", "3"].
         * 
         * Second call: After s2's timer was reset, both s1 and s2 show 0s elapsed (not ready),
         * but s3 now has 10.01s elapsed (ready). The selector returns s3's macros ["3", "4"].
         */
        private void _testSelectSkillMacroCommandsChecksSkillsInOrder()
        {
            var selector = _fixture();
            _macroRandom.NextReturn.Add(1234);
            _macroRandom.NextReturn.Add(1234);
            ((MockTimestamp)_skillTimeouts["s1"].Stopwatch).GetTimestampReturn.Add(0);
            ((MockTimestamp)_skillTimeouts["s2"].Stopwatch).GetTimestampReturn.Add(10.01);
            ((MockTimestamp)_skillTimeouts["s1"].Stopwatch).GetTimestampReturn.Add(0);
            ((MockTimestamp)_skillTimeouts["s2"].Stopwatch).GetTimestampReturn.Add(0);
            ((MockTimestamp)_skillTimeouts["s3"].Stopwatch).GetTimestampReturn.Add(10.01);
            var result1 = selector.Select(_skillsModel);
            Debug.Assert(result1.Count == 2);
            Debug.Assert(result1[0] == "2");
            Debug.Assert(result1[1] == "3");
            var result2 = selector.Select(_skillsModel);
            Debug.Assert(result2.Count == 2);
            Debug.Assert(result2[0] == "3");
            Debug.Assert(result2[1] == "4");
        }

        /**
         * @brief Tests that no macro commands are returned when all skills are on cooldown
         * 
         * All three skills have elapsed time (0s) less than their timeout values (10.0s),
         * meaning none are ready for execution. The selector should return an empty list.
         * This prevents the bot from attempting to execute skills when all are still
         * within their randomized cooldown periods.
         */
        private void _testSelectSkillMacroCommandsReturnsEmptyOnCooldown()
        {
            var selector = _fixture();
            ((MockTimestamp)_skillTimeouts["s1"].Stopwatch).GetTimestampReturn.Add(0);
            ((MockTimestamp)_skillTimeouts["s2"].Stopwatch).GetTimestampReturn.Add(0);
            ((MockTimestamp)_skillTimeouts["s3"].Stopwatch).GetTimestampReturn.Add(0);
            var result = selector.Select(_skillsModel);
            Debug.Assert(result.Count == 0);
        }

        public void Run()
        {
            _testClearingSkillTimeouts();
            _testUpdateSkillTimerAddsActiveSkills();
            _testUpdateSkillTimerSetsStopwatches();
            _testUpdateSkillTimerSetsTimeouts();
            _testUpdateSkillTimerUpdatesStopwatches();
            _testUpdateSkillTimerUpdatesTimeouts();
            _testUpdateSkillTimerRemovesInactiveSkills();
            _testSelectSkillMacroCommandsReturnsReadySkillMacros();
            _testSelectSkillMacroCommandsResetsSkillTimer();
            _testSelectSkillMacroCommandsChecksSkillsInOrder();
            _testSelectSkillMacroCommandsReturnsEmptyOnCooldown();
        }
    }


    public class SkillCommandsExecutorTests
    {
        private MockSkillMacroCommandsSelector _skillCommandsSelector = (
            new MockSkillMacroCommandsSelector()
        );

        private MockMacroCommandsExecutorBuilder _executorBuilder = (
            new MockMacroCommandsExecutorBuilder()
        );

        private MockMacroCommandsExecutor _executor = (
            new MockMacroCommandsExecutor()
        );

        private AbstractSkillsModel _skillsModel = new SkillsModel();

        private List<string> _callOrder = [];

        private AbstractBottingCommandsExecutor _fixture()
        {
            _skillCommandsSelector = new MockSkillMacroCommandsSelector();
            _executorBuilder = new MockMacroCommandsExecutorBuilder();
            _executor = new MockMacroCommandsExecutor();
            _executorBuilder.BuildReturn.Add(_executor);
            _skillsModel = new SkillsModel();
            _callOrder = [];
            _skillCommandsSelector.CallOrder = _callOrder;
            _executor.CallOrder = _callOrder;
            var executor = new SkillCommandsExecutor(
                _skillCommandsSelector,
                _executorBuilder
            );
            executor.Inject(SystemInjectType.SkillsModel, _skillsModel);
            executor.Inject(
                SystemInjectType.Transmitters,
                new TransmitterInfo { KeystrokeTransmitter = new MockKeystrokeTransmitter() }
            );
            return executor;
        }

        /**
         * @brief Tests that the skill commands executor follows the correct execution sequence
         * 
         * When the bot executes skill commands, it must first update the skill timers to
         * recalculate cooldowns, then select which skill's macro commands are ready, and
         * finally execute those commands through the macro executor. This order ensures
         * that skill cooldowns are current before selection and that selected commands
         * are properly executed.
         */
        private void _testExecuteCallOrder()
        {
            var executor = _fixture();
            var selectorRef = new TestUtilities().Reference(_skillCommandsSelector);
            var executeRef = new TestUtilities().Reference(_executor);
            _skillCommandsSelector.SelectReturn.Add(["1"]);
            executor.Execute();
            Debug.Assert(_callOrder.Count == 3);
            Debug.Assert(_callOrder[0] == selectorRef + "Update");
            Debug.Assert(_callOrder[1] == selectorRef + "Select");
            Debug.Assert(_callOrder[2] == executeRef + "Execute");
        }

        /**
         * @brief Tests that macro commands are only executed when skills are actually selected
         * 
         * When the skill selector returns macro commands (indicating a skill is ready),
         * the executor must pass those commands to the macro executor for execution.
         * When the selector returns an empty list (no skills ready), the executor should
         * do nothing and not invoke the macro executor.
         */
        private void _testExecuteCallsWhenCommandsSelected()
        {
            foreach (var commands in new[] { new List<string> { "1", "2" }, [] })
            {
                var executor = _fixture();
                _skillCommandsSelector.SelectReturn.Add(commands);
                Debug.Assert(executor.Execute() == commands.Count > 0);
                if (commands.Count > 0)
                {
                    Debug.Assert(_executor.ExecuteCalls == 1);
                    Debug.Assert(_executor.ExecuteCallArg_macroCommands[0].Count == commands.Count);
                    for (int i = 0; i < commands.Count; i++)
                    {
                        Debug.Assert(_executor.ExecuteCallArg_macroCommands[0][i] == commands[i]);
                    }
                }
                else
                {
                    Debug.Assert(_executor.ExecuteCalls == 0);
                }
            }
        }

        /**
         * @brief Tests that the correct skills model is passed to the selector methods
         * 
         * The skill commands executor holds a reference to the skills model containing
         * all skill configurations. When updating and selecting skills, it must pass this
         * same model instance to the selector's Update and Select methods. This ensures
         * the selector has access to the most current skill data.
         */
        private void _testSelectorParameters()
        {
            var executor = _fixture();
            _skillCommandsSelector.SelectReturn.Add([]);
            executor.Execute();
            Debug.Assert(_skillCommandsSelector.UpdateCallArg_skillsModel[0] == _skillsModel);
            Debug.Assert(_skillCommandsSelector.SelectCallArg_skillsModel[0] == _skillsModel);
        }

        /**
         * @brief Tests that the skill selector is cleared when the macro stops
         * 
         * When the bot's macro execution stops, the skill selector's internal state
         * should be cleared. This prevents stale skill timers from persisting across
         * different botting sessions and ensures a fresh state when automation resumes.
         */
        private void _testSelectorClearsWhenMacroStopped()
        {
            var executor = _fixture();
            executor.Inject(MacroExecutorThreadedUpdate.Stopped, 0);
            Debug.Assert(_skillCommandsSelector.ClearCalls == 1);
        }

        public void Run()
        {
            _testExecuteCallOrder();
            _testExecuteCallsWhenCommandsSelected();
            _testSelectorParameters();
            _testSelectorClearsWhenMacroStopped();
        }
    }


    public class BottingCommandsExecutorTests
    {
        private MockMacroCommandsExecutorBuilder _executorBuilder = new MockMacroCommandsExecutorBuilder();

        private MockMacroCommandsExecutor _executor = new MockMacroCommandsExecutor();

        private AbstractKeystrokeTransmitter _keystrokeTransmitter = new MockKeystrokeTransmitter();

        private BottingModel _bottingModel = new BottingModel();

        private List<RectangleF> _minimapRects()
        {
            return [
                new RectangleF { X = -5.0f, Y= -5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  5.0f, Y= -5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X = -5.0f, Y=  5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  5.0f, Y=  5.0f, Width = 10.0f, Height = 10.0f},
                new RectangleF { X =  0.0f, Y=  0.0f, Width = 10.0f, Height = 10.0f}
            ];
        }

        private List<PointF> _closestPoints()
        {
            return [
                new PointF(1.0f, 1.0f),
                new PointF(9.0f, 1.0f),
                new PointF(1.0f, 9.0f),
                new PointF(9.0f, 9.0f),
                new PointF(4.0f, 5.0f)
            ];
        }

        private AbstractBottingCommandsExecutor _fixture()
        {
            _executorBuilder = new MockMacroCommandsExecutorBuilder();
            _executor = new MockMacroCommandsExecutor();
            _keystrokeTransmitter = new MockKeystrokeTransmitter();
            _bottingModel = new BottingModel();
            var minimapRects = _minimapRects();
            for (int i = 0; i < minimapRects.Count(); i++)
            {
                _bottingModel.GetMacroModel().AddMacroPoint(
                    new MinimapPoint
                    {
                        X = minimapRects[i].X,
                        Y = minimapRects[i].Y,
                        XRange = minimapRects[i].Width,
                        YRange = minimapRects[i].Height,
                        PointData = new MinimapPointData
                        {
                            ElementName = i.ToString(),
                            Commands = [
                                new MinimapPointMacros
                                {
                                    MacroChance = 20,
                                    MacroCommands = [i.ToString()]
                                }
                            ]
                        }
                    }
                );
            }
            _executorBuilder.BuildReturn.Add(_executor);
            return new BottingCommandsExecutor(
                new BottingPointDataSelector("some key"),
                new BottingRandomMacroCommandsSelector(new MacroRandom()),
                _executorBuilder
            );
        }

        /**
         * @brief Verifies that the executor is built when a keystroke transmitter is injected
         * 
         * When users start using keyboard automation, the system needs to create
         * an executor that can run macro commands. This test ensures that the
         * executor is properly built once the keystroke transmitter is available.
         */
        private void _testInjectingBottingBuildsExecutor()
        {
            var bottingCommandsExecutor = _fixture();
            var transmitterInfo = new TransmitterInfo { KeystrokeTransmitter = _keystrokeTransmitter };
            bottingCommandsExecutor.Inject(
                SystemInjectType.Transmitters,
                transmitterInfo
            );
            Debug.Assert(_executorBuilder.WithArgCalls == 1);
            Debug.Assert(_executorBuilder.WithArgCallArg_arg[0] == transmitterInfo);
            Debug.Assert(_executorBuilder.BuildCalls == 1);
        }

        /**
         * @brief Verifies that the correct macro commands execute based on player position
         * 
         * When users move to different locations in the game, the system should
         * automatically select and execute the appropriate macro commands for
         * that area. This test ensures that the current position correctly
         * determines which macros run.
         */
        private void _testTransmitExecutesSelectedMacroCommands()
        {
            var closestPoints = _closestPoints();
            for (int i = 0; i < closestPoints.Count(); i++)
            {
                var bottingCommandsExecutor = _fixture();
                var (pX, pY) = ((int) closestPoints[i].X, (int) closestPoints[i].Y);
                _bottingModel.GetMapModel().SetTemplatePosition("some key", pX, pY);
                bottingCommandsExecutor.Inject(
                    SystemInjectType.Transmitters,
                    new TransmitterInfo { KeystrokeTransmitter = _keystrokeTransmitter }
                );
                bottingCommandsExecutor.Inject(SystemInjectType.BottingModel, _bottingModel);
                bottingCommandsExecutor.Execute();
                Debug.Assert(_executor.ExecuteCalls == 1);
                Debug.Assert(_executor.ExecuteCallArg_macroCommands.Count == 1);
                Debug.Assert(_executor.ExecuteCallArg_macroCommands[0].Count == 1);
                Debug.Assert(_executor.ExecuteCallArg_macroCommands[0][0] == i.ToString());
            }
        }

        public void Run()
        {
            _testInjectingBottingBuildsExecutor();
            _testTransmitExecutesSelectedMacroCommands();
        }
    }
    

    public class BottingExecutorThreadHelperTests
    {
        private List<AbstractBottingCommandsExecutor> _bottingCommandsExecutor = [];

        private AbstractKeystrokeTransmitterThreadHelper _fixture()
        {
            _bottingCommandsExecutor = [
                new MockBottingCommandsExecutor(),
                new MockBottingCommandsExecutor(),
                new MockBottingCommandsExecutor(),
                new MockBottingCommandsExecutor(),
            ];
            return new BottingExecutorThreadHelper(_bottingCommandsExecutor);
        }

        /**
         * @brief Tests that the botting executor thread helper executes executors in order until one succeeds
         * 
         * The botting system may have multiple executors (e.g., macro executor, skill executor,
         * consumable executor) that are tried in priority order. The thread helper must iterate
         * through each executor, call its Execute method, and stop at the first one that returns true
         * (indicating a successful execution). This prevents multiple actions from being taken
         * in a single cycle and establishes execution priority.
         */
        private void _testTransmitExecutesUntilSuccessful()
        {
            for (int i = 0; i < _bottingCommandsExecutor.Count; i++)
            {
                var helper = _fixture();
                for (int j = 0; j < i; j++)
                {
                    var failExecutor = (MockBottingCommandsExecutor)_bottingCommandsExecutor[j];
                    failExecutor.ExecuteReturn.Add(false);
                }
                var successfulExecutor = (MockBottingCommandsExecutor)_bottingCommandsExecutor[i];
                successfulExecutor.ExecuteReturn.Add(true);
                helper.Transmit();
                for (int j = 0; j <= i; j++)
                {
                    var calledExecutor = (MockBottingCommandsExecutor)_bottingCommandsExecutor[j];
                    Debug.Assert(calledExecutor.ExecuteCalls == 1);
                }
                for (int j = i + 1; j < _bottingCommandsExecutor.Count; j++)
                {
                    var calledExecutor = (MockBottingCommandsExecutor)_bottingCommandsExecutor[j];
                    Debug.Assert(calledExecutor.ExecuteCalls == 0);
                }
            }
        }

        /**
         * @brief Tests that injection data is forwarded to all botting executors
         * 
         * When external data (such as configuration updates, keystroke transmitters, or
         * threshold values) is injected into the thread helper, it must forward that data
         * to every botting executor in the collection. This ensures all executors stay
         * synchronized with the latest bot configuration and system state.
         */
        private void _testInjectionIntoAllExecutors()
        {
            var helper = _fixture();
            helper.Inject(123, 234);
            foreach (var executor in _bottingCommandsExecutor)
            {
                var calledExecutor = (MockBottingCommandsExecutor)executor;
                Debug.Assert(calledExecutor.InjectCalls == 1);
                Debug.Assert((int)calledExecutor.InjectCallArg_dataType[0] == 123);
                Debug.Assert((int)calledExecutor.InjectCallArg_data[0]! == 234);
            }

        }

        public void Run()
        {
            _testTransmitExecutesUntilSuccessful();
            _testInjectionIntoAllExecutors();
        }
    }


    public class BottingExecutorThreadTests
    {
        private MockKeystrokeTransmitterThreadHelper _executorThreadHelper = (
            new MockKeystrokeTransmitterThreadHelper()
        );

        private MockResetEvent _executionEvent = new MockResetEvent();

        private MockRunningState _transmittingState = new MockRunningState();

        private MockRunningState _runningState = new MockRunningState();

        private AbstractKeystrokeTransmitterThreadState _threadState = (
            new KeystrokeTransmitterThreadState(
                (int)BottingExecutorThreadedUpdate.Stopped,
                KeystrokeTransmitterThreadType.Botting
            )
        );

        private MockInjectAction _injectAction = new MockInjectAction();

        private List<string> _callOrder = [];

        private string _threadStateRef = "";

        private string _transmittingStateRef = "";

        private string _executionEventRef = "";

        private string _executorThreadHelperRef = "";

        private void _setupNewFixture(
            AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _executorThreadHelper = new MockKeystrokeTransmitterThreadHelper();
            _executionEvent = new MockResetEvent();
            _transmittingState = new MockRunningState();
            _runningState = new MockRunningState();
            _threadState = threadState;
            _injectAction = new MockInjectAction();
            _callOrder = [];
        }

        private void _setupCallOrder()
        {
            if (_threadState is MockKeystrokeTransmitterThreadState mockThreadState)
            {
                mockThreadState.CallOrder = _callOrder;
            }
            _executionEvent.CallOrder = _callOrder;
            _injectAction.CallOrder = _callOrder;
            _transmittingState.CallOrder = _callOrder;
        }

        private void _setupRunningState()
        {
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(true);
            _runningState.IsRunningReturn.Add(false);
        }

        private void _setupTransmit(int transmitCount)
        {
            for (int i = 0; i < transmitCount - 1; i++)
            {
                _executorThreadHelper.TransmitReturn.Add(true);
            }
            _executorThreadHelper.TransmitReturn.Add(false);
        }

        private List<Action> _stopLambdas(AbstractThread abstractThread)
        {
            return [
                () =>
                {
                    abstractThread.Inject(
                        BottingOrchestratorThreadInjectType.Stop, null
                    );
                },
                () =>
                {
                    abstractThread.Stop();
                }
            ];
        }

        private void _setupReferences()
        {
            _threadStateRef = new TestUtilities().Reference(_threadState);
            _transmittingStateRef = new TestUtilities().Reference(_transmittingState); ;
            _executionEventRef = new TestUtilities().Reference(_executionEvent);
            _executorThreadHelperRef = new TestUtilities().Reference(_executorThreadHelper);
        }

        private AbstractThread _fixture(
            int transmitCount, AbstractKeystrokeTransmitterThreadState threadState
        )
        {
            _setupNewFixture(threadState);
            _setupCallOrder();
            _setupRunningState();
            _setupTransmit(transmitCount);
            _setupReferences();
            return new BottingExecutorThread(
                _executionEvent,
                _executorThreadHelper,
                _threadState,
                _transmittingState,
                _runningState
            );
        }

        /**
         * @brief Verifies the handshake sequence when the botting executor starts its
         * monster-killing transmission routine
         * 
         * When the macro system determines that the character should begin killing
         * monsters on the map, the botting orchestrator signals the executor to start
         * its transmission routine. The executor performs a coordinated startup handshake
         * with the botting executor to ensure that transmission is ready.
         */
        private void _testExecutorStartingHandshake()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                for (int j = 0; j < i; j++)
                {
                    _transmittingState.IsRunningReturn.Add(true);
                }
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(BottingOrchestratorThreadInjectType.Start, 0);
                Debug.Assert(_callOrder.Count == (i + 4));
                Debug.Assert(_callOrder[0] == _threadStateRef + "SetState");
                for (int j = 1; j <= i + 1; j++)
                {
                    Debug.Assert(_callOrder[j] == _transmittingStateRef + "IsRunning");
                }
                Debug.Assert(_callOrder[i + 2] == _threadStateRef + "SetState");
                Debug.Assert(_callOrder[i + 3] == _executionEventRef + "Set");
            }
        }

        /**
         * @brief Verifies thread state changes correctly during startup
         * 
         * When users start automation, the thread transitions through proper states:
         * Starting -> Started. This test ensures the thread correctly updates its
         * state so the rest of the system knows what it's doing.
         */
        private void _testExecutorStartingHandshakeSetsThreadStates()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
            _transmittingState.IsRunningReturn.Add(false);
            keystrokeTransmitterExecutorThread.Inject(
                BottingOrchestratorThreadInjectType.Start, 0
            );
            Debug.Assert(threadState.SetStateCallArg_state[0] == (int)BottingExecutorThreadedUpdate.Starting);
            Debug.Assert(threadState.SetStateCallArg_state[1] == (int)BottingExecutorThreadedUpdate.Started);
        }

        /**
         * @brief Verifies the handshake sequence when the botting executor stops its
         * monster-killing transmission routine
         * 
         * When the macro system needs to switch to a different transmission routine
         * (such as navigating to a rune or solving the rune puzzle), the orchestrator
         * signals the executor to stop its current routine. The executor performs a
         * coordinated shutdown handshake to ensure keystrokes stop cleanly before
         * the routine exits.
         */
        private void _testExecutorStoppingHandshake()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                for (int j = 0; j < i; j++)
                {
                    _transmittingState.IsRunningReturn.Add(true);
                }
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(BottingOrchestratorThreadInjectType.Stop, 0);
                Debug.Assert(_callOrder.Count == (i + 3));
                Debug.Assert(_callOrder[0] == _threadStateRef + "SetState");
                for (int j = 1; j <= i + 1; j++)
                {
                    Debug.Assert(_callOrder[j] == _transmittingStateRef + "IsRunning");
                }
                Debug.Assert(_callOrder[i + 2] == _threadStateRef + "SetState");
            }
        }


        /**
         * @brief Verifies thread state changes correctly during shutdown
         * 
         * When users stop automation, the thread transitions from Started → Stopping
         * -> Stopped. This test ensures the thread correctly reports its state during
         * shutdown for proper system coordination.
         */
        private void _testExecutorStoppingHandshakeSetsThreadStates()
        {
            for (int i = 0; i < 2; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var keystrokeTransmitterExecutorThread = _fixture(1, threadState);
                _transmittingState.IsRunningReturn.Add(false);
                var stopLambdas = _stopLambdas(keystrokeTransmitterExecutorThread);
                stopLambdas[i]();
                Debug.Assert(threadState.SetStateCallArg_state[0] == (int)BottingExecutorThreadedUpdate.Stopping);
                Debug.Assert(threadState.SetStateCallArg_state[1] == (int)BottingExecutorThreadedUpdate.Stopped);
            }
        }

        /**
         * @brief Verifies macros execute continuously while automation runs
         * 
         * When users have automation running, the thread should continuously process
         * macro commands based on their location. This test ensures that once started,
         * the thread repeatedly checks the player's position and executes the
         * appropriate macros without stopping.
         */
        private void _testExecutorThreadLoopTransmitsWhenStarted()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)BottingExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Botting
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var start = BottingOrchestratorThreadInjectType.Start;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(start, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == i);
            }
        }

        /**
         * @brief Verifies macros stop executing when automation is stopped
         * 
         * When users stop automation, the thread should immediately stop executing
         * macros. This test ensures that after a stop command, no further macros
         * are executed even if the player continues moving.
         */
        private void _testExecutorThreadLoopDoesntTransmitWhenStopped()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)BottingExecutorThreadedUpdate.Started,
                    KeystrokeTransmitterThreadType.Botting
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var stop = BottingOrchestratorThreadInjectType.Stop;
                _transmittingState.IsRunningReturn.Add(false);
                keystrokeTransmitterExecutorThread.Inject(stop, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(_executorThreadHelper.TransmitCalls == 0);
            }
        }

        /**
         * @brief Verifies that the executor thread helper is reset before and after each
         * transmission cycle to ensure clean state for the next macro execution
         * 
         * When the botting executor processes macros for killing monsters, the thread
         * helper must be reset to a clean state before executing keystroke transmissions
         * for the current character position. This prevents stale data from previous
         * macro executions from affecting the current transmission.
         */
        private void _testExecutorThreadLoopResetsBeforeAndAfterTransmit()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    (int)BottingExecutorThreadedUpdate.Stopped,
                    KeystrokeTransmitterThreadType.Botting
                );
                var keystrokeTransmitterExecutorThread = _fixture(i, threadState);
                var callOrder = _executorThreadHelper.CallOrder;
                _transmittingState.IsRunningReturn.Add(false);
                var start = BottingOrchestratorThreadInjectType.Start;
                keystrokeTransmitterExecutorThread.Inject(start, 0);
                keystrokeTransmitterExecutorThread.Start();
                keystrokeTransmitterExecutorThread.Join(10000);
                Debug.Assert(callOrder.Count == i + 2);
                Debug.Assert(callOrder[0] == _executorThreadHelperRef + "Reset");
                for (int j = 1; j <= i; j++)
                {
                    Debug.Assert(callOrder[j] == _executorThreadHelperRef + "Transmit");
                }
                Debug.Assert(callOrder[i + 1] == _executorThreadHelperRef + "Reset");
            }
        }

        public void Run()
        {
            _testExecutorStartingHandshake();
            _testExecutorStartingHandshakeSetsThreadStates();
            _testExecutorStoppingHandshake();
            _testExecutorStoppingHandshakeSetsThreadStates();
            _testExecutorThreadLoopTransmitsWhenStarted();
            _testExecutorThreadLoopDoesntTransmitWhenStopped();
            _testExecutorThreadLoopResetsBeforeAndAfterTransmit();
        }
    }


    public class BottingOrchestratorThreadTests
    {
        private AbstractKeystrokeTransmitterThreadState _threadState = new KeystrokeTransmitterThreadState(
            0, KeystrokeTransmitterThreadType.Botting
        );

        private MockThread _thread = new MockThread(new ThreadRunningState());

        private MockRunningState _runningState = new MockRunningState();

        private BlockingCollection<int> _threadStates = new BlockingCollection<int>();

        private string _threadRef = "";

        private List<string> _callOrder = [];

        private AbstractThread _fixture(AbstractKeystrokeTransmitterThreadState threadState)
        {
            _threadState = threadState;
            _thread = new MockThread(new ThreadRunningState());
            _runningState = new MockRunningState();
            _callOrder = [];
            _thread.CallOrder = _callOrder;
            _threadStates = new BlockingCollection<int>();
            if (_threadState is MockKeystrokeTransmitterThreadState mockThreadState)
            {
                mockThreadState.CallOrder = _callOrder;
            }
            _threadRef = new TestUtilities().Reference(_thread);
            return new BottingOrchestratorThread(
                _thread,
                _runningState,
                _threadStates
            );
        }

        private void _setTransmitFixture(int transmitCount)
        {
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(true);
            for (int j = 0; j < transmitCount; j++)
            {
                _runningState.IsRunningReturn.Add(true);
            }
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(false);
            for (int j = 0; j < transmitCount + 1; j++)
            {
                _threadStates.Add(j);
            }
        }

        /**
         * @brief Verifies that starting the orchestrator launches the executor thread
         * 
         * When users start their automation, the orchestrator should launch the
         * executor thread that actually runs the macros. This test ensures that
         * starting the orchestrator properly kicks off the executor.
         */
        private void _testStartingOrchestratorStartsExecutorThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.Botting
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            _runningState.IsRunningReturn.Add(false);
            _runningState.IsRunningReturn.Add(false);
            transmitterOrchestratorThread.Start();
            Debug.Assert(_thread.ThreadStartCalls == 1);
        }

        /**
         * @brief Verifies that stopping the orchestrator shuts down the executor thread
         * 
         * When users stop their automation, the orchestrator should cleanly shut
         * down the executor thread. This test ensures the shutdown sequence works
         * properly, including the handshake that confirms the thread has stopped.
         */
        private void _testStoppingOrchestratorStopsExecutorThread()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                0, KeystrokeTransmitterThreadType.Botting
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            _runningState.IsRunningReturn.Add(true);
            _thread.CallOrder = _callOrder;
            Debug.Assert(_threadStates.Count == 0);
            transmitterOrchestratorThread.Stop();
            Debug.Assert(_threadStates.Count == 1);
            Debug.Assert(_callOrder.Count == 1);
            Debug.Assert(_callOrder[0] == _threadRef + "ThreadStop");

        }

        /**
         * @brief Verifies that injected commands update the thread state
         * 
         * When the system sends commands to the orchestrator, the thread state
         * should update to reflect what it should be doing (starting, stopping,
         * running, etc.). This test ensures the orchestrator correctly tracks
         * its current operational state.
         */
        private void _testInjectingOrchestratorCommandAssignsThreadState()
        {
            var threadState = new KeystrokeTransmitterThreadState(
                123, KeystrokeTransmitterThreadType.Botting
            );
            var transmitterOrchestratorThread = _fixture(threadState);
            var max = BottingOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                transmitterOrchestratorThread.Inject(
                    (BottingOrchestratorThreadInjectType)i, 0
                );
                Debug.Assert(_threadStates.Count == 1);
                Debug.Assert(_threadStates.Take() == i);
            }
        }

        /**
         * @brief Confirms the orchestrator properly acknowledges commands
         * 
         * When commands are sent to the orchestrator, it should acknowledge them
         * by updating its state and signaling that the command was received.
         * This test ensures the orchestrator properly handles the command.
         */
        private void _testInjectOrchestratorCommand()
        {
            var max = BottingOrchestratorThreadInjectType.MaxNum;
            for (int i = 0; i < (int)max; i++)
            {
                var threadState = new MockKeystrokeTransmitterThreadState();
                var transmitterOrchestratorThread = _fixture(threadState);
                transmitterOrchestratorThread.Inject((BottingOrchestratorThreadInjectType)i, 0);
                Debug.Assert(_threadStates.Count == 1);
                Debug.Assert(_threadStates.Take() == i);
            }
        }

        /**
         * @brief Verifies that data is forwarded to the executor thread
         * 
         * When the orchestrator receives data (like macro commands or configuration),
         * it should forward that data to the executor thread that will process it.
         * This test ensures the orchestrator correctly passes data along.
         */
        private void _testInjectToExecutorThread()
        {
            var threadState = new MockKeystrokeTransmitterThreadState();
            var transmitterOrchestratorThread = _fixture(threadState);
            transmitterOrchestratorThread.Inject(123, 456);
            Debug.Assert(_thread.InjectCalls == 1);
            Debug.Assert((int)_thread.InjectCallArg_dataType[0] == 123);
            Debug.Assert((int)_thread.InjectCallArg_data[0]! == 456);
        }

        /**
         * @brief Verifies that the orchestrator makes itself available as a thread dependency
         * 
         * When other systems in the application need to communicate with or control
         * the orchestrator thread, they need a reference to it. This test ensures
         * that when an InjectAction is received, the orchestrator properly registers
         * itself as a thread dependency that other components can discover and use.
         */
        private void _testInjectActionToExecutorThread()
        {
            var getActionDataType = new List<object>();
            var getActionData = new List<object>();
            var injectAction = new MockInjectAction();
            injectAction.GetActionReturn.Add(
                (object dataType, object data) =>
                {
                    getActionDataType.Add(dataType);
                    getActionData.Add(data);
                }
            );
            var threadState = new MockKeystrokeTransmitterThreadState();
            var transmitterOrchestratorThread = _fixture(threadState);
            transmitterOrchestratorThread.Inject(SystemInjectType.InjectAction, injectAction);
            Debug.Assert(_thread.InjectCalls == 1);
            Debug.Assert((int)_thread.InjectCallArg_dataType[0] == (int)SystemInjectType.InjectAction);
            Debug.Assert(_thread.InjectCallArg_data[0] == injectAction);
            Debug.Assert(injectAction.GetActionCalls == 1);
            Debug.Assert(getActionDataType.Count == 1);
            Debug.Assert((int)getActionDataType[0] == (int)SystemInjectType.ThreadDependency);
            Debug.Assert(getActionData[0] == transmitterOrchestratorThread);
        }

        /**
         * @brief Verifies the orchestrator's main processing loop
         * 
         * The orchestrator runs a main loop that coordinates all activities:
         * waiting for commands, updating state, and managing the executor.
         * This test ensures the loop properly sequences all these activities.
         */
        private void _testThreadLoopInjectsCommands()
        {
            for (int i = 1; i < 10; i++)
            {
                var threadState = new KeystrokeTransmitterThreadState(
                    123, KeystrokeTransmitterThreadType.Botting
                );
                var transmitterOrchestratorThread = _fixture(threadState);
                _setTransmitFixture(i);
                transmitterOrchestratorThread.Start();
                transmitterOrchestratorThread.Join(10000);
                Debug.Assert(_callOrder.Count == i + 1);
                Debug.Assert(_callOrder[0] == _threadRef + "ThreadStart");
                for (int j = 1; j <= i; j++)
                {
                    Debug.Assert(_callOrder[j] == _threadRef + "ThreadInject");
                    Debug.Assert((int)_thread.InjectCallArg_dataType[j - 1]! == j - 1);
                }
            }
        }

        public void Run()
        {
            _testStartingOrchestratorStartsExecutorThread();
            _testStoppingOrchestratorStopsExecutorThread();
            _testInjectingOrchestratorCommandAssignsThreadState();
            _testInjectOrchestratorCommand();
            _testInjectToExecutorThread();
            _testInjectActionToExecutorThread();
            _testThreadLoopInjectsCommands();
        }
    }



    public class BottingTransmitterTestSuite
    {
        public void Run()
        {
            new RandomBottingMacroCommandsSelectorTests().Run();
            new BottingPointDataSelectorTests().Run();
            new SkillMacroCommandsSelectorTests().Run();
            new SkillCommandsExecutorTests().Run();
            new BottingCommandsExecutorTests().Run();
            new BottingExecutorThreadHelperTests().Run();
            new BottingExecutorThreadTests().Run();
            new BottingOrchestratorThreadTests().Run();
        }
    }
}
