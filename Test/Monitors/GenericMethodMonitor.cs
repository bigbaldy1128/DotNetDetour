using DotNetDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class GenericMethodMonitor : IMethodMonitor
    {
        [Monitor(typeof(A1<string>))]
        public string GenericMethod(string t)
        {
            return "B" + Ori(t);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Original]
        public string Ori(string t)
        {
            return null;
        }
    }
}
