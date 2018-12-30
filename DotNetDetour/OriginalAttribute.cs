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
	/// 标记函数的泛型参数或私有类型参数。
	/// 私有类型要指明其完全限定名，如：System.Int32、type`1[[System.Int32]]这种完整形式。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RememberTypeAttribute : Attribute
    {
		public RememberTypeAttribute(string fullName = null, bool isGeneric=false)
        {
            TypeFullNameOrNull = fullName;
			IsGeneric = isGeneric;
        }
		public string TypeFullNameOrNull { get; private set; }
		public bool IsGeneric { get; private set; }
    }
}
