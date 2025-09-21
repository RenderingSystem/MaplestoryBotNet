using MaplestoryBotNet.ThreadingUtils;


namespace MaplestoryBotNet.Systems.Macro.SubSystems
{
    public abstract class AbstractMacroAgent
    {
        public abstract void Execute();

        public abstract void Update(object data);
    }


    public class MacroAgentThread : AbstractThread
    {
        AbstractMacroAgent _macroAgent;

        AbstractExecutionFlag _executionFlag;

        public MacroAgentThread(
            AbstractMacroAgent macroAgent,
            AbstractExecutionFlag executionFlag,
            AbstractThreadRunningState runningState
        ) : base(runningState)
        {
            _macroAgent = macroAgent;
            _executionFlag = executionFlag;
        }

        public override void ThreadLoop()
        {
            while (_runningState.IsRunning())
            {
                _executionFlag.Wait();
                _macroAgent.Execute();
            }
        }
    }
}
