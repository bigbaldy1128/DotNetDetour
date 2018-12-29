using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
	[Obsolete("此类已变更为ShadowMethodAttribute")]
	[AttributeUsage(AttributeTargets.Method)]
	public class OriginalAttribute : ShadowMethodAttribute { }
	[Obsolete("此类已变更为RememberTypeAttribute")]
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NonPublicParameterTypeAttribute : RememberTypeAttribute {
		public NonPublicParameterTypeAttribute(string fullName)
			: base(fullName) {
		}
	}

    /// <summary>
    /// 标记一个方法，使得该方法代表原始方法，从而可以被用户代码调用。
	/// 此方法命名为 原始目标方法名_Original；如果使用其他名称，在RelocatedMethodAttribute中应当指明。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ShadowMethodAttribute : Attribute
    {
		[Obsolete("应该使用不带参数的构造方法，此方法已无法兼容", true)]
		public ShadowMethodAttribute(string targetTypeName, string methodName) { }
		[Obsolete("应该使用不带参数的构造方法，此方法已无法兼容", true)]
		public ShadowMethodAttribute(Type classType, string methodName) { }

		public ShadowMethodAttribute() { }
    }
    /// <summary>
    /// 标记函数私有类型参数(此参数类型用object代替)，并指明其具体私有类型名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RememberTypeAttribute : Attribute
    {
        public RememberTypeAttribute(string fullName)
        {
            this.FullName = fullName;
        }
        public string FullName { get; private set; }
    }
}
