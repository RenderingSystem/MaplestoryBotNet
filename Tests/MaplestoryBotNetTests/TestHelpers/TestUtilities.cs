using System.Runtime.CompilerServices;


namespace MaplestoryBotNetTests.TestHelpers
{
    public class TestUtilities
    {
        public string Reference(object obj)
        {
            return obj.GetType().FullName + " {" + RuntimeHelpers.GetHashCode(obj) + "} ";
        }
    }
}
