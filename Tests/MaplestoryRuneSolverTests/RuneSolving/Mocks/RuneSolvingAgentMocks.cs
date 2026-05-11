using MaplestoryRuneSolver.RuneSolving;
using MaplestoryRuneSolverTests.TestHelpers;


namespace MaplestoryRuneSolverTests.RuneSolving.Mocks
{
    public class MockRuneSolvingAgent : AbstractRuneSolvingAgent
    {
        public List<string> CallOrder = new List<string>();

        public int LoadModelCalls = 0;
        public int LoadModelIndex = 0;
        public List<string> LoadModelCallArg_runeModelPath = new List<string>();
        public List<bool> LoadModelReturn = new List<bool>();
        public override bool LoadModel(string runeModelPath)
        {
            var callReference = new TestUtilities().Reference(this) + "LoadModel";
            CallOrder.Add(callReference);
            LoadModelCalls++;
            LoadModelCallArg_runeModelPath.Add(runeModelPath);
            if (LoadModelIndex < LoadModelReturn.Count)
            {
                return LoadModelReturn[LoadModelIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int SolveCalls = 0;
        public int SolveIndex = 0;
        public List<string> SolveCallArg_base64Image = new List<string>();
        public List<string> SolveReturn = new List<string>();
        public override string Solve(string base64Image)
        {
            var callReference = new TestUtilities().Reference(this) + "Solve";
            CallOrder.Add(callReference);
            SolveCallArg_base64Image.Add(base64Image);
            SolveCalls++;
            if (SolveIndex < SolveReturn.Count)
            {
                return SolveReturn[SolveIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }
}
