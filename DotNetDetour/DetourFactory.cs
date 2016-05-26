using DotNetDetour.DetourWays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    public class DetourFactory
    {
        public static IDetour CreateDetourEngine()
        {
            if (IntPtr.Size == 4)
            {
                return new NativeDetourFor32Bit();
            }
            else if (IntPtr.Size == 8)
            {
                return new NativeDetourFor64Bit();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}

