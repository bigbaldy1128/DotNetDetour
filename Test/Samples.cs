using DotNetDetour;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Computer
    {
		public string Name { get; set; }
		public Computer() {
			Name = "X1";
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
		public static string Any<T>(T val) {
			return val.ToString();
		}



		private class Item {
			public Computer Value;
			public string Status = "-";
		}
		public string PowerOff() {
			var item = new Item { Value = this };
			return PowerOffList(new List<Item>(new Item[] { item })) + ":" + item.Status;
		}
		private static string PowerOffList(List<Item> pcs) {
			var rtv = "";
			foreach (var pc in pcs) {
				rtv += "," + pc.Value.Name;
				pc.Status = "off";
			}
			return rtv.Substring(1);
		}
		private static string PowerOffList(List<string> pcs) {
			return "干扰测试";
		}
		private static string PowerOffList(object pcs) {
			return "干扰测试";
		}
    }


    public class ComputerOf<T>
    {
        public T ComputerIo(T owner)
        {
            return owner;
        }
    }

	public class ComputerDetour : IMethodHook {
		#region 静态方法HOOK
		[RelocatedMethodAttribute("Test.Computer")]
		public static string GetCpu() {
			return "Not " + GetCpu_Original();
		}

		[ShadowMethod]
		public static string GetCpu_Original() {
			return null;
		}
		#endregion

		#region 构造方法
		[RelocatedMethodAttribute("Test.Computer")]
		public void Computer() {
			Computer_Original();

			//根据上下文来决定是否要hook
			if (CallContext.LogicalGetData("OpenComputerConstructorHook") == (object)"1") {
				var This = (Computer)(object)this;
				This.Name = "ConstructorMethod " + This.Name;
			}
		}

		[ShadowMethod]
		public void Computer_Original() {

		}
		#endregion

		#region 实例方法（不实现ShadowMethod、方法名称和原方法名称不同）
		[RelocatedMethodAttribute(typeof(Computer), "GetRAMSize")]
		public string Hook_GetRAMSize() {
			return "Hook";
		}
		#endregion


		#region 实例属性
		//public string get_Os(){...} 不封装成属性也可以
		public string Os {
			[RelocatedMethodAttribute(typeof(Computer))]
			get {
				return "Not " + Os_Original;
			}
		}
		//public string get_Os_Original(){...} 不封装成属性也可以
		public string Os_Original {
			[ShadowMethod]
			get {
				return null;
			}
		}
		#endregion






		#region 带私有+内部类型的方法
		[RelocatedMethodAttribute("Test.Computer")]
		private static string PowerOffList([RememberType("System.Collections.Generic.List`1[[Test.Computer+Item]]")] object pcs) {
			var msg = PowerOffList_Original(pcs);
			return "InternalTypeMethod " + msg;
		}

		[ShadowMethod]
		private static string PowerOffList_Original(object pcs) {
			return null;
		}
		#endregion







		#region 泛型方法，每种使用到的类型都单独实现
		[RelocatedMethodAttribute("Test.Computer")]
		public static string Any([RememberType(isGeneric: true)]string val) {
			return "Hook<string> " + Any_Original(val);
		}
		[ShadowMethod]
		public static string Any_Original(string val) {
			return null;
		}
		#endregion

		#region 泛型方法，每种使用到的类型都单独实现
		[RelocatedMethodAttribute("Test.Computer")]
		public static string Any([RememberType(isGeneric: true)]int val) {
			return "Hook<int> " + Any_Original(val);
		}
		[ShadowMethod]
		public static string Any_Original(int val) {
			return null;
		}
		#endregion





		#region 泛型类型<引用类型>的方法，只能用一个方法进行hook，如：string，object
		[RelocatedMethodAttribute(typeof(ComputerOf<Object>))]
		public object ComputerIo(object name) {
			if (name is string) {
				var human = ComputerIo_Original(name);
				human = "Hook<string> " + human;
				return human;
			}
			if (name is Computer) {
				var o = (Computer)ComputerIo_Original(name);
				o.Name = "Hook<object> " + o.Name;
				return o;
			}
			return null;
		}
		[ShadowMethod]
		public object ComputerIo_Original(object owner) {
			return null;
		}
		#endregion

		#region 泛型类型<值类型>的方法，每种使用到的类型都单独实现，如：int、bool
		[RelocatedMethodAttribute("Test.ComputerOf`1[[System.Int32]]")]
		public int ComputerIo(int name) {
			return ComputerIo_Original(name) + 1;
		}
		[ShadowMethod]
		public int ComputerIo_Original(int owner) {
			return 0;
		}
		#endregion







		private class Framework : IMethodHook {
			[RelocatedMethod("System.IO.File")]
			public static string ReadAllText(string file) {
				return "Hook " + ori(file) + "：NetFrameworkDetour";
			}

			//一个hook的ShadowMethod名字可以随便写
			[ShadowMethod]
			public static string ori(string file) {
				return null;
			}
		}
	}
}
