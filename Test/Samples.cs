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
		public string InitMsg;
		public Computer() {
			InitMsg = "Init";
		}

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
        [RelocatedMethodAttribute("Test.Computer")]
		public static string GetCpu()
        {
			return "Not " + GetCpu_Original();
        }

        [ShadowMethod]
		public static string GetCpu_Original()
        {
            return null;
        }
        #endregion

		#region 构造方法
		[RelocatedMethodAttribute("Test.Computer")]
		public void Computer() {
			Computer_Original();

			var This = (Computer)(object)this;
			This.InitMsg = "Hook " + This.InitMsg;
		}

		[ShadowMethod]
		public void Computer_Original() {

		}
		#endregion

		#region 实例方法
		[RelocatedMethodAttribute(typeof(Computer), "GetRAMSize")]
        public string Hook_GetRAMSize()
        {
			return "Not " + GetRAMSize_Original();
        }

        [ShadowMethod]
		public string GetRAMSize_Original()
        {
            return null;
        }
        #endregion


        #region 实例属性
        public string Os
        {
			[RelocatedMethodAttribute(typeof(Computer), "", "getOs")]
            get
            {
				return "Not " + getOs();
            }
        }
		[ShadowMethod]
		public string getOs()
        {
			return null;
        }
        #endregion

		#region 泛型方法
		[RelocatedMethodAttribute(typeof(ComputerOf<string>))]
		public string ComputerIo(string name) {
			var human = "" + ComputerIo_Original(name);
			human = "Not " + human;
			return human;
		}
		[ShadowMethod]
		public string ComputerIo_Original(string owner) {
			return null;
		}
		#endregion

		#region 泛型方法
		[RelocatedMethodAttribute(typeof(ComputerOf<Computer>))]
		public Computer ComputerIo(Computer val) {
			var human = ComputerIo_Original(val);
			val.InitMsg = "ComputerIo Computer";
			return human;
		}
		[ShadowMethod]
		public Computer ComputerIo_Original(Computer owner) {
			return null;
		}
		#endregion
    }

    public class NetFrameworkDetour : IMethodHook
    {
		[RelocatedMethod("System.IO.File")]
        public static string ReadAllText(string file)
        {
            try
            {
				return ReadAllText_Original(file) + "NetFrameworkDetour";
            }
            catch
            {
                Debugger.Break();
                throw;
            }
        }

        [ShadowMethod]
		public static string ReadAllText_Original(string file)
        {
            return null;
        }
    }
}
