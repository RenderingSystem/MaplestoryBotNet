using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaplestoryBotNet.Systems.Keyboard.SubSystems.Transmitters
{
    public enum CashShopOrchestratorThreadInjectType
    {
        None = 0,
        Stop,
        Start,
        MaxNum
    }

    public enum CashShopExecutorThreadedUpdate
    {
        Stopping = 0,
        Stopped,
        Starting,
        Started,
        TimedOut,
        MaxNum
    }

    internal class CashShopTransmitter
    {
    }
}
