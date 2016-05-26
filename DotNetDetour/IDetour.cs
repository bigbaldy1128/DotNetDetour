using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    public interface IDetour
    {
        void Patch(MethodInfo src, MethodInfo dest, MethodInfo ori);
    }
}
