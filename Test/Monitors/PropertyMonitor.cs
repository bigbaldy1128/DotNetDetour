using DotNetDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class PropertyMonitor : IMethodMonitor
    {
        public string Property
        {
            [Monitor("Test","A","Test.dll")]
            get
            {
                return "B" + OriginalProperty;
            }
        }

        public string OriginalProperty
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            [Original]
            get
            {
                return null;
            }
        }
    }
}
