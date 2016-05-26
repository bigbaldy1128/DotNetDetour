using DotNetDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class InstanceMethodMonitor : IMethodMonitor
    {
        [Monitor("Test", "A")]
        public string InstanceMethod()
        {
            return "B" + Ori();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Original]
        public string Ori()
        {
            return null;
        }
    }
}
