using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    /// <summary>
    /// 标记一个方法，使得该方法代表原始方法，从而可以被用户代码调用
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ShadowMethodAttribute : Attribute
    {
        public string TargetMethodName;
        public string TargetTypeName { get; set; }
        public Type TargetType { get; private set; }
        public ShadowMethodAttribute(string targetTypeName, string methodName)
        {
            this.TargetTypeName = targetTypeName;
            this.TargetMethodName = methodName;
        }

        public ShadowMethodAttribute(Type classType, string methodName)
        {
            this.TargetType = classType;
            this.TargetTypeName = classType.FullName;
            this.TargetMethodName = methodName;
        }
    }
    /// <summary>
    /// 标记函数参数，指明其类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class NonPublicParameterTypeAttribute : Attribute
    {
        public NonPublicParameterTypeAttribute(string fullName)
        {
            this.FullName = fullName;
        }
        public string FullName { get; set; }
    }
}
