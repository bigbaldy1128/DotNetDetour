using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetDetour;
using System.IO;

namespace Test
{
    [TestClass]
    public class ThunkMethodTest
    {
        [TestMethod]
        public void StaticMethod()
        {
            ClrMethodHook.Install();
            Assert.AreEqual("Not Intel Core I7", Computer.GetCpu());
        }

        [TestMethod]
        public void InstanceMethod()
        {
            ClrMethodHook.Install(AppDomain.CurrentDomain.BaseDirectory);
            Assert.AreEqual("Not 512M", new Computer().GetRAMSize());
        }

        [TestMethod]
        public void PropertyMethod()
        {
            ClrMethodHook.Install(AppDomain.CurrentDomain.BaseDirectory);
            Assert.AreEqual("Not Windows 10", new Computer().Os);
        }

        [TestMethod]
        public void SystemMethod()
        {
            ClrMethodHook.Install();
            Assert.AreEqual("My_name_is_NetFrameworkDetour", File.ReadAllText("../../test"));
        }

        [TestMethod]
        public void GenericMethod()
        {
            ClrMethodHook.Install(AppDomain.CurrentDomain.BaseDirectory);
            Assert.AreEqual("Not Jack", new ComputerOf<string>().ComputerIo("Jack"));
        }
    }
}
