using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MonitorAttribute:Attribute
    {
        public string NamespaceName { get; set; }
        public string ClassName { get; set; }
        public string AssemblyName { get; set; }
        public Type Type { get; set; }

        public MonitorAttribute(string NamespaceName,string ClassName, string AssemblyName=null)
        {
            this.NamespaceName = NamespaceName;
            this.ClassName = ClassName;
            this.AssemblyName = AssemblyName;
        }

        public MonitorAttribute(Type type)
        {
            this.Type = type;
        }
    }
}
