using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    /// <summary>
    /// 标记一个方法，使其作为目标方法的替代，这将使.NET框架执行到原始方法时转而执行被此特性标记的方法。
    /// 
    /// 如果提供OriginalMethod，OriginalMethod名称应当为“原始目标方法名_Original”，如果使用其他名称，应当设置相应的OriginalMethodName。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HookMethodAttribute : Attribute
    {
        public string TargetTypeFullName { get; private set; }
        public Type TargetType { get; private set; }
        private string TargetMethodName;
        private string OriginalMethodName;

        public string GetTargetMethodName(MethodBase method) {
            return String.IsNullOrEmpty(TargetMethodName) ? method.Name : TargetMethodName;
        }
        public string GetOriginalMethodName(MethodBase method) {
            return String.IsNullOrEmpty(OriginalMethodName) ? GetTargetMethodName(method) + "_Original" : OriginalMethodName;
        }

        /// <summary>
        /// 标记要hook的目标类型中的指定方法。
        /// 类型名称为完全限定名，如果是泛型可以提供type`1[[System.Int32]]这种完整形式。
        /// 方法名称targetMethodName可以不提供，默认取当前方法名称。
        /// 如果提供OriginalMethod时，OriginalMethod名称应当为“原始目标方法名_Original”，如果使用其他名称，应当设置OriginalMethodName。
        /// </summary>
        public HookMethodAttribute(string targetTypeFullName, string targetMethodName = null, string originalMethodName = null)
        {
            this.TargetTypeFullName = targetTypeFullName;
            this.TargetMethodName = targetMethodName;
            this.OriginalMethodName = originalMethodName;
        }

        /// <summary>
        /// 标记要hook的目标类型中的指定方法。
        /// 方法名称targetMethodName可以不提供，默认取当前方法名称。
        /// 如果提供OriginalMethod时，OriginalMethod名称应当为“原始目标方法名_Original”，如果使用其他名称，应当设置OriginalMethod。
        /// </summary>
        public HookMethodAttribute(Type targetType, string targetMethodName = null, string originalMethodName = null)
        {
            this.TargetType = targetType;
            this.TargetMethodName = targetMethodName;
            this.OriginalMethodName = originalMethodName;
        }
    }




    [Obsolete("此类已变更为HookMethodAttribute")]
    [AttributeUsage(AttributeTargets.Method)]
    public class MonitorAttribute : HookMethodAttribute {
        public MonitorAttribute(string NamespaceName, string ClassName)
            : base(NamespaceName + "." + ClassName) { }
        public MonitorAttribute(Type type)
            : base(type) { }
    }
    [Obsolete("此类已变更为HookMethodAttribute")]
    [AttributeUsage(AttributeTargets.Method)]
    public class RelocatedMethodAttribute : HookMethodAttribute {
        public RelocatedMethodAttribute(string targetTypeFullName, string targetMethodName)
            : base(targetTypeFullName, targetMethodName) { }
        public RelocatedMethodAttribute(Type type, string targetMethodName)
            : base(type, targetMethodName) { }
    }
}
