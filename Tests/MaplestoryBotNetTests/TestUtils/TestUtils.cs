using System.Runtime.CompilerServices;

namespace MaplestoryBotNetTests
{
    public class TestUtils
    {
        public string Reference(object obj)
        {
            return obj.GetType().FullName + " {" + RuntimeHelpers.GetHashCode(obj) + "} ";
        }
    }
}
