using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OriginalAttribute:Attribute
    {
    }
}
