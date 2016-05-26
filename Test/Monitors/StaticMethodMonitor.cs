using DotNetDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class StaticMethodMonitor : IMethodMonitor
    {
        [Monitor("Test","A", "Test.dll")]
        public static string StaticMethod()
        {
            return "B" + OriginalMethod();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Original]
        public static string OriginalMethod()
        {
            return null;
        }
    }
}
