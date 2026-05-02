using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;



namespace MaplestoryRuneSolver.RuneSolving
{
    public abstract class AbstractRuneSolvingAgent
    {
        public abstract bool LoadModel(string runeModelPath);

        public abstract string Solve(string base64Image);
    }


    public abstract class AbstractRuneClassMap
    {
        public abstract string MapToString(int classId);
    }


    public class RuneClassMap : AbstractRuneClassMap
    {
        public override string MapToString(int classId)
        {
            return (
                classId == 0 ? "down" :
                classId == 1 ? "left" :
                classId == 2 ? "right" :
                classId == 4 ? "up" :
                "undefined"
            );
        }
    }


    public class RuneSolvingAgent : AbstractRuneSolvingAgent
    {
        private InferenceSession? _session;

        private AbstractRuneClassMap _runeClassMap;

        private float _confidenceThreshold;

        private int _inputWidth;

        private int _inputHeight;

        public RuneSolvingAgent(
            AbstractRuneClassMap runeClassMap,
            float confidenceThreshold
        )
        {
            _session = null;
            _runeClassMap = runeClassMap;
            _confidenceThreshold = confidenceThreshold;
            _inputWidth = 640;
            _inputHeight = 640;
        }

        public override bool LoadModel(string runeModelPath)
        {
            try
            {
                var sessionOptions = new SessionOptions();
                sessionOptions.AppendExecutionProvider_DML();
                sessionOptions.AppendExecutionProvider_CPU();
                sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                _session = new InferenceSession(runeModelPath, sessionOptions);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private DenseTensor<float> _preprocess(Image<Rgba32> image)
        {
            image.Mutate(x => x.Resize(_inputWidth, _inputHeight));
            var inputTensor = new DenseTensor<float>([1, 3, _inputHeight, _inputWidth]);
            for (int y = 0; y < _inputHeight; y++)
            for (int x = 0; x < _inputWidth; x++)
            {
                var pixel = image[x, y];
                inputTensor[0, 0, y, x] = pixel.R / 255.0f;
                inputTensor[0, 1, y, x] = pixel.G / 255.0f;
                inputTensor[0, 2, y, x] = pixel.B / 255.0f;
            }
            return inputTensor;
        }

        private List<NamedOnnxValue> _inputs(Image<Rgba32> image)
        {
            return new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", _preprocess(image))
            };
        }

        private object? _detection(Tensor<float> output, int index)
        {
            var detection = new
            {
                x1 = output[0, index, 0],
                y1 = output[0, index, 1],
                x2 = output[0, index, 2],
                y2 = output[0, index, 3],
                confidence = output[0, index, 4],
                classId = (int)output[0, index, 5]
            };
            return (detection.confidence >= _confidenceThreshold) ?
                detection : null;
        }

        private object _prediction(dynamic detection, Point originalDims)
        {
            var centerX = (detection.x1 + detection.x2) / 2;
            var centerY = (detection.y1 + detection.y2) / 2;
            var scaleX = ((float)originalDims.X) / _inputWidth;
            var scaleY = ((float)originalDims.Y) / _inputHeight;
            return new
            {
                X = (int)Math.Round(centerX * scaleX),
                Y = (int)Math.Round(centerY * scaleY),
                Class = _runeClassMap.MapToString(detection.classId)
            };
        }

        private string _serialize(List<object> predictions)
        {
            var sortedPredictions = predictions
                .OrderBy(p => ((dynamic)p).X)
                .ToList();
            return JsonSerializer.Serialize(sortedPredictions);
        }

        private string _runInference(Image<Rgba32> image, Point originalDims)
        {
            var results = _session!.Run(_inputs(image));
            var output = results.First().AsTensor<float>();
            var predictions = new List<object>();
            for (int i = 0; i < output.Dimensions[1]; i++)
            {
                if (_detection(output, i) is object detection)
                {
                    predictions.Add(_prediction(detection, originalDims));
                }
            }
            return _serialize(predictions);
        }

        public override string Solve(string base64Image)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                var image = Image.Load<Rgba32>(new MemoryStream(imageBytes));
                var originalDims = new Point(image.Width, image.Height);
                return _runInference(image, originalDims);
            }
            catch
            {
                return "[]";
            }
        }
    }


    public class RuneSolvingAgentFacade : AbstractRuneSolvingAgent
    {
        private AbstractRuneSolvingAgent _runeSolvingAgent;

        public RuneSolvingAgentFacade()
        {
            _runeSolvingAgent = new RuneSolvingAgent(new RuneClassMap(), 0.5f);
        }

        public override bool LoadModel(string runeModelPath)
        {
            return _runeSolvingAgent.LoadModel(runeModelPath);
        }

        public override string Solve(string base64Image)
        {
            return _runeSolvingAgent.Solve(base64Image);
        }
    }
}
