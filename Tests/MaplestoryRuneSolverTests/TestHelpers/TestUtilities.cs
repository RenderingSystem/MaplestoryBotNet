using System.Runtime.CompilerServices;


namespace MaplestoryRuneSolverTests.TestHelpers
{
    public class TestUtilities
    {
        public string Reference(object obj)
        {
            return obj.GetType().FullName + " {" + RuntimeHelpers.GetHashCode(obj) + "} ";
        }
    }
}
