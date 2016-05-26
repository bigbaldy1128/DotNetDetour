using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetDetour;
using System.IO;

namespace Test
{
    [TestClass]
    public class MonitorTest
    {
        [TestMethod]
        public void StaticMethod()
        {
            Monitor.Install();
            Assert.AreEqual("BA", A.StaticMethod());
        }

        [TestMethod]
        public void InstanceMethod()
        {
            Monitor.Install();
            Assert.AreEqual("BA", new A().InstanceMethod());
        }

        [TestMethod]
        public void PropertyMethod()
        {
            Monitor.Install();
            Assert.AreEqual("BA", new A().Property);
        }

        [TestMethod]
        public void SystemMethod()
        {
            Monitor.Install();
            Assert.AreEqual("BA", File.ReadAllText("../../test"));
        }

        [TestMethod]
        public void GenericMethod()
        {
            Monitor.Install();
            Assert.AreEqual("BA", new A1<string>().GenericMethod("A"));
        }
    }
}
