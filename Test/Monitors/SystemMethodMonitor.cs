using DotNetDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class SystemMethodMonitor : IMethodMonitor
    {
        [Monitor("System.IO","File")]
        public static string ReadAllText(string path)
        {
            return "B" + OriginalReadAllText(path);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Original]
        public static string OriginalReadAllText(string path)
        {
            return null;
        }
    }
}
