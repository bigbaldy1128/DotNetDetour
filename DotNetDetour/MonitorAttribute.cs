using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    /// <summary>
    /// 标记一个方法，使其作为目标方法的替代，这将使.NET框架执行到原始方法时转而执行被此特性标记的方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RelocatedMethodAttribute : Attribute
    {
        public string TargetTypeName { get; set; }
        public string TargetMethodName { get; private set; }
        public Type TargetType { get; set; }
        public string AssemblyQualifiedName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetClassName">target class full name</param>
        /// <param name="methodName"></param>
        public RelocatedMethodAttribute(string targetClassName, string methodName)
        {
            this.TargetTypeName = targetClassName;
            this.TargetMethodName = methodName;
        }

        public RelocatedMethodAttribute(Type targetType, string methodName)
        {
            this.TargetType = targetType;
            this.TargetTypeName = targetType.FullName;
            this.TargetMethodName = methodName;
            this.AssemblyQualifiedName = targetType.AssemblyQualifiedName;
        }
    }
}
