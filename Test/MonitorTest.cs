using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetDetour;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace Test
{
    [TestClass]
    public class ThunkMethodTest
    {
		[TestMethod]
		public void A_DotNetSystemMethod() {
			ClrMethodHook.Install();
			Assert.AreEqual("Hook My_name_is_：NetFrameworkDetour", File.ReadAllText("test"));
		}

        [TestMethod]
        public void StaticMethod()
        {
            ClrMethodHook.Install();
            Assert.AreEqual("Not Intel Core I7", Computer.GetCpu());
        }

		[TestMethod]
		public void ConstructorMethod(){
			ClrMethodHook.Install();

			CallContext.LogicalSetData("OpenComputerConstructorHook", "1");
			var o = new Computer();
			CallContext.LogicalSetData("OpenComputerConstructorHook", null);

			Assert.AreEqual("ConstructorMethod X1", o.Name);
		}

		[TestMethod]
		public void InternalTypeMethod() {
			ClrMethodHook.Install();
			Assert.AreEqual("InternalTypeMethod X1:off", new Computer().PowerOff());
		}

        [TestMethod]
        public void InstanceMethod()
        {
            ClrMethodHook.Install();
            Assert.AreEqual("Hook", new Computer().GetRAMSize());
        }

        [TestMethod]
        public void PropertyMethod()
        {
            ClrMethodHook.Install();
            Assert.AreEqual("Not Windows 10", new Computer().Os);
        }

		[TestMethod]
		public void GenericMethodMethod() {
			ClrMethodHook.Install();

			Assert.AreEqual("Hook<int> 123", Computer.Any<int>(123));
			//Assert.AreEqual("Hook<string> str", Computer.Any<string>("str"));
		}

		[TestMethod]
		public void GenericTypeMethod() {
			ClrMethodHook.Install();

			Assert.AreEqual("Hook<string> Jack", new ComputerOf<string>().ComputerIo("Jack"));

			Assert.AreEqual("Hook<object> X1", new ComputerOf<Computer>().ComputerIo(new Computer()).Name);

			Assert.AreEqual(5, new ComputerOf<int>().ComputerIo(4));
		}






		/// <summary>
		/// 验证：泛型类＜引用类型＞的函数是相同的
		/// </summary>
		[TestMethod]
		public void GenericPointRefCheck() {
			var t1 = typeof(ComputerOf<string>);
			var t2 = typeof(ComputerOf<Computer>);
			var h1 = t1.GetMethod("ComputerIo").MethodHandle.GetFunctionPointer();
			var h2 = t2.GetMethod("ComputerIo").MethodHandle.GetFunctionPointer();
			Assert.AreEqual(h1, h2);
		}
		/// <summary>
		/// 验证：泛型类＜值类型＞的函数是不相同的，泛型方法＜值类型、引用类型＞是不同的
		/// </summary>
		[TestMethod]
		public void GenericPointValCheck() {
			var t1 = typeof(ComputerOf<int>);
			var t2 = typeof(ComputerOf<bool>);
			var h1 = t1.GetMethod("ComputerIo").MethodHandle.GetFunctionPointer();
			var h2 = t2.GetMethod("ComputerIo").MethodHandle.GetFunctionPointer();
			Assert.AreNotEqual(h1, h2);

			var th1 = typeof(Computer).GetMethod("Any").MakeGenericMethod(typeof(object));
			var th2 = typeof(Computer).GetMethod("Any").MakeGenericMethod(typeof(Computer));
			var th3 = typeof(Computer).GetMethod("Any").MakeGenericMethod(typeof(Computer));
			h1 = th1.MethodHandle.GetFunctionPointer();
			h2 = th2.MethodHandle.GetFunctionPointer();
			var h3 = th3.MethodHandle.GetFunctionPointer();
			Assert.AreNotEqual(h1, h2);
			Assert.AreEqual(h3, h2);

			th1 = typeof(Computer).GetMethod("Any").MakeGenericMethod(typeof(int));
			th2 = typeof(Computer).GetMethod("Any").MakeGenericMethod(typeof(bool));
			th3 = typeof(Computer).GetMethod("Any").MakeGenericMethod(typeof(bool));
			h1 = th1.MethodHandle.GetFunctionPointer();
			h2 = th2.MethodHandle.GetFunctionPointer();
			h3 = th3.MethodHandle.GetFunctionPointer();
			Assert.AreNotEqual(h1, h2);
			Assert.AreEqual(h3, h2);
		}
    }
}
