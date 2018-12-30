using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    [Obsolete("此接口已变更为IMethodHook")]
    public interface IMethodMonitor : IMethodHook {
    }

    public interface IMethodHook
    {
    }
    /// <summary>
    /// 此接口用于支持Hook初始化时进行回调
    /// </summary>
    public interface IMethodHookWithSet : IMethodHook {
        /// <summary>
        /// 当前类每成功进行一个Hook的初始化，就会传入被Hook的原始方法（判断方法名称来确定是初始化的哪个方法），这个方法可用于获取方法所在的类（如：非公开类）。
        /// 注意：此方法应当当做静态方法来进行编码。
        /// </summary>
        void HookMethod(MethodBase method);
    }
}
