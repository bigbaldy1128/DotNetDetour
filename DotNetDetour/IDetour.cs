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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawMethod">要hook的目标函数</param>
        /// <param name="customImplMethod">用户定义的函数，可以调用占位函数来实现对原函数的调用</param>
        /// <param name="placeholder">占位函数</param>
        bool Patch(MethodBase rawMethod/*要hook的目标函数*/,
            MethodInfo customImplMethod/*用户定义的函数，可以调用占位函数来实现对原函数的调用*/,
            MethodInfo placeholder/*占位函数*/);
        bool IsDetourInstalled();
    }
}
