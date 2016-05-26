using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    class DestAndOri
    {
        public MethodInfo Dest;
        public MethodInfo Ori;
    }

    public class Monitor
    {
        static bool installed = false;
        static List<DestAndOri> destAndOris = new List<DestAndOri>();
        /// <summary>
        /// 安装监视器
        /// </summary>
        public static void Install(string dir = null)
        {
            if (installed)
                return;
            installed = true;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<IMethodMonitor> monitors;
            if (string.IsNullOrEmpty(dir))
            {
                monitors = assemblies.SelectMany(t => t.GetImplementedObjectsByInterface<IMethodMonitor>());
            }
            else
            {
                monitors = Directory
                            .GetFiles(dir)
                            .SelectMany(d => Assembly.LoadFrom(d).GetImplementedObjectsByInterface<IMethodMonitor>());
            }
            foreach (var monitor in monitors)
            {
                DestAndOri destAndOri = new DestAndOri();
                destAndOri.Dest= monitor
                            .GetType()
                            .GetMethods()
                            .FirstOrDefault(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(MonitorAttribute)));
                destAndOri.Ori = monitor
                            .GetType()
                            .GetMethods()
                            .FirstOrDefault(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(OriginalAttribute)));
                if (destAndOri.Dest != null)
                {
                    destAndOris.Add(destAndOri);
                }
            }
            InstallInternal(assemblies);
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        }

        private static void InstallInternal(Assembly[] assemblies)
        {
            foreach (var destAndOri in destAndOris)
            {
                MethodInfo src = null;
                var dest = destAndOri.Dest;
                var monitorAttribute = dest.GetCustomAttribute(typeof(MonitorAttribute)) as MonitorAttribute;
                var methodName = dest.Name;
                var paramTypes = dest.GetParameters().Select(t => t.ParameterType).ToArray();
                if (monitorAttribute.Type != null)
                {
                    src = monitorAttribute.Type.GetMethod(methodName, paramTypes);
                }
                else
                {
                    var srcNamespaceAndClass = monitorAttribute.NamespaceName + "." + monitorAttribute.ClassName;
                    foreach (var asm in assemblies)
                    {
                        var type= asm.GetExportedTypes().FirstOrDefault(t => t.FullName == srcNamespaceAndClass);
                        if (type != null)
                        {
                            src = type.GetMethod(methodName, paramTypes);
                            break;
                        }
                    }
                }
                if (src == null)
                    continue;
                var ori = destAndOri.Ori;
                var engine = DetourFactory.CreateDetourEngine();
                engine.Patch(src, dest, ori);
            }
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            InstallInternal(new[] { args.LoadedAssembly });
        }
    }
}
