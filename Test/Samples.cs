using DotNetDetour;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Computer
    {

        public static string GetCpu()
        {
            return "Intel Core I7";
        }

        public string GetRAMSize()
        {
            return "512M";
        }

        internal string Os
        {
            get
            {
                return "Windows 10";
            }
        }
    }


    public class ComputerOf<T>
    {
        public T ComputerIo(T owner)
        {
            return owner;
        }
    }

    public class ComputerDetour : IMethodHook
    {
        #region 静态方法HOOK
        [RelocatedMethodAttribute(typeof(Computer), "GetCpu")]
        public static string _impl_GetCpu()
        {
            return "Not " + GetCpu();
        }

        [ShadowMethod(typeof(Computer), "GetCpu")]
        public static string GetCpu()
        {
            return null;
        }
        #endregion

        #region 实例方法
        [RelocatedMethodAttribute(typeof(Computer), "GetRAMSize")]
        public string _impl_GetRAMSize()
        {
            return "Not " + GetRAMSize();
        }

        [ShadowMethod(typeof(Computer), "GetRAMSize")]
        public string GetRAMSize()
        {
            return null;
        }
        #endregion


        #region 实例属性
        public string _impl_Os
        {
            [RelocatedMethodAttribute(typeof(Computer), "get_Os")]
            get
            {
                return "Not " + Os;
            }
        }

        public string Os
        {

            [ShadowMethod(typeof(Computer), "get_Os")]
            get
            {
                return null;
            }
        }
        #endregion

        #region 泛型方法
        [RelocatedMethodAttribute(typeof(ComputerOf<string>), "ComputerIo")]
        public string _impl_ComputerIo(string name)
        {
            var human = ComputerIo(name);
            human = "Not " + human;
            return human;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        [ShadowMethod(typeof(ComputerOf<string>), "ComputerIo")]
        public string ComputerIo(string owner)
        {
            return null;
        }
        #endregion
    }

    public class NetFrameworkDetour : IMethodHook
    {
        [RelocatedMethod(typeof(System.IO.File), "ReadAllText")]
        public static string _impl_ReadAllText(string file)
        {
            try
            {
                return ReadAllText(file) + "NetFrameworkDetour";
            }
            catch (Exception ex)
            {
                Debugger.Break();
                throw;
            }
        }

        [ShadowMethod(typeof(System.IO.File), "ReadAllText")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ReadAllText(string file)
        {
            return null;
        }
    }
}
