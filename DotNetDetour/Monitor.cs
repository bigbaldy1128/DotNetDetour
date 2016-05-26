using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    public class Monitor
    {
        static bool installed = false;
        /// <summary>
        /// 安装监视器
        /// </summary>
        public static void Install(string dir = null)
        {
            if (installed)
                return;
            installed = true;
            IEnumerable<IMethodMonitor> monitors = null;
            if (string.IsNullOrEmpty(dir))
            {
                monitors = AppDomain
                            .CurrentDomain
                            .GetAssemblies()
                            .SelectMany(t => t.GetImplementedObjectsByInterface<IMethodMonitor>());
            }
            else
            {
                monitors = Directory
                            .GetFiles(dir)
                            .SelectMany(d => Assembly.LoadFrom(d).GetImplementedObjectsByInterface<IMethodMonitor>());
            }
            foreach (var monitor in monitors)
            {
                MethodInfo src = null;
                var dest = monitor
                            .GetType()
                            .GetMethods()
                            .FirstOrDefault(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(MonitorAttribute)));
                if (dest == null)
                    continue;
                var monitorAttribute=dest.GetCustomAttribute(typeof(MonitorAttribute)) as MonitorAttribute;
                var methodName = dest.Name;
                var paramTypes = dest.GetParameters().Select(t => t.ParameterType).ToArray();
                if (monitorAttribute.Type != null)
                {
                    src = monitorAttribute.Type.GetMethod(methodName, paramTypes);
                }
                else
                {
                    var srcNamespaceAndClass = monitorAttribute.NamespaceName + "." + monitorAttribute.ClassName;

                    if (string.IsNullOrEmpty(monitorAttribute.AssemblyName))
                    {
                        src = Type.GetType(srcNamespaceAndClass).GetMethod(methodName, paramTypes);
                    }
                    else
                    {
                        Assembly asm = Assembly.LoadFrom(monitorAttribute.AssemblyName);
                        src = asm.GetType(srcNamespaceAndClass).GetMethod(methodName, paramTypes);
                    }
                }
                if (src == null)
                    continue;
                var ori = monitor.GetType()
                            .GetMethods()
                            .FirstOrDefault(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(OriginalAttribute)));
                var engine = DetourFactory.CreateDetourEngine();
                engine.Patch(src, dest, ori);
            }
        }
    }
}
