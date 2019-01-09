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
        /// <param name="hookMethod">用户定义的函数，可以调用原始占位函数来实现对原函数的调用</param>
        /// <param name="originalMethod">原始占位函数</param>
        void Patch(MethodBase rawMethod/*要hook的目标函数*/,
            MethodBase hookMethod/*用户定义的函数，可以调用原始占位函数来实现对原函数的调用*/,
            MethodBase originalMethod/*原始占位函数*/);
    }
}
