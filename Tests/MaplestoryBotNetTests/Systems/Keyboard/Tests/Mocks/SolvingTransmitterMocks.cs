using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems;
using MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters;
using MaplestoryBotNetTests.TestHelpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.Json;


namespace MaplestoryBotNetTests.Systems.Keyboard.Tests.Mocks
{

    public class MockRuneSolverManager : AbstractRuneSolverManager
    {
        public List<string> CallOrder = [];

        public int PostCalls = 0;
        public int PostIndex = 0;
        public List<string> PostCallArg_url = [];
        public List<string> PostCallArg_content = [];
        public List<string> PostReturn = [];
        public override string Post(string url, string content)
        {
            var callReference = new TestUtilities().Reference(this) + "Post";
            CallOrder.Add(callReference);
            PostCalls++;
            PostCallArg_url.Add(url);
            PostCallArg_content.Add(content);
            if (PostIndex < PostReturn.Count)
            {
                return PostReturn[PostIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockRuneSolverCaller : AbstractRuneSolverCaller
    {
        public List<string> CallOrder = [];

        public int CallCalls = 0;
        public int CallIndex = 0;
        public List<RuneDetection> CallCallArg_runeDetection = [];
        public List<Image<Bgra32>> CallCallArg_image = [];
        public List<string> CallReturn = [];
        public override string Call(RuneDetection runeDetection, Image<Bgra32> image)
        {
            var callReference = new TestUtilities().Reference(this) + "Call";
            CallOrder.Add(callReference);
            CallCalls++;
            CallCallArg_runeDetection.Add(runeDetection);
            CallCallArg_image.Add(image);
            if (CallIndex < CallReturn.Count)
            {
                return CallReturn[CallIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockRuneSolverWorkflow : AbstractRuneSolverWorkflow
    {
        public List<string> CallOrder = [];

        public int ExecuteArrowDetectionCalls = 0;
        public int ExecuteArrowDetectionIndex = 0;
        public List<RuneDetection> ExecuteArrowDetectionCallArg_runeDetection = [];
        public List<JsonDocument?> ExecuteArrowDetectionReturn = [];
        public override JsonDocument? ExecuteArrowDetection(RuneDetection runeDetection)
        {
            var callReference = new TestUtilities().Reference(this) + "ExecuteArrowDetection";
            CallOrder.Add(callReference);
            ExecuteArrowDetectionCalls++;
            ExecuteArrowDetectionCallArg_runeDetection.Add(runeDetection);
            if (ExecuteArrowDetectionIndex < ExecuteArrowDetectionReturn.Count)
            {
                return ExecuteArrowDetectionReturn[ExecuteArrowDetectionIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int ExecuteArrowSequenceCalls = 0;
        public int ExecuteArrowSequenceIndex = 0;
        public List<RuneDetection> ExecuteArrowSequenceCallArg_runeDetection = [];
        public List<JsonDocument> ExecuteArrowSequenceCallArg_predictions = [];
        public List<bool> ExecuteArrowSequenceReturn = [];
        public override bool ExecuteArrowSequence(
            RuneDetection runeDetection, JsonDocument predictions
        )
        {
            var callReference = new TestUtilities().Reference(this) + "ExecuteArrowSequence";
            CallOrder.Add(callReference);
            ExecuteArrowSequenceCalls++;
            ExecuteArrowSequenceCallArg_runeDetection.Add(runeDetection);
            ExecuteArrowSequenceCallArg_predictions.Add(predictions);
            if (ExecuteArrowSequenceIndex < ExecuteArrowSequenceReturn.Count)
            {
                return ExecuteArrowSequenceReturn[ExecuteArrowSequenceIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int ExecuteInteractionCalls = 0;
        public int ExecuteInteractionIndex = 0;
        public List<bool> ExecuteInteractionReturn = [];
        public override bool ExecuteInteraction()
        {
            var callReference = new TestUtilities().Reference(this) + "ExecuteInteraction";
            CallOrder.Add(callReference);
            ExecuteInteractionCalls++;
            if (ExecuteInteractionIndex < ExecuteInteractionReturn.Count)
            {
                return ExecuteInteractionReturn[ExecuteInteractionIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int ValidatePrerequisitesCalls = 0;
        public int ValidatePrerequisitesIndex = 0;
        public List<RuneDetection?> ValidatePrerequisitesReturn = [];
        public override RuneDetection? ValidatePrerequisites()
        {
            var callReference = new TestUtilities().Reference(this) + "ValidatePrerequisites";
            CallOrder.Add(callReference);
            ValidatePrerequisitesCalls++;
            if (ValidatePrerequisitesIndex < ValidatePrerequisitesReturn.Count)
            {
                return ValidatePrerequisitesReturn[ValidatePrerequisitesIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int InjectCalls = 0;
        public int InjectIndex = 0;
        public List<object> InjectCallArg_dataType = [];
        public List<object?> InjectCallArg_data = [];
        public override void Inject(object dataType, object? data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }

    }
}
