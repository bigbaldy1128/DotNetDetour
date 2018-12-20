using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour
{
    [DebuggerDisplay("{TargetTypeFullName}.{MethodName}")]
    class DestAndOri
    {
        /// <summary>
        /// 代理方法
        /// </summary>
        public MethodInfo ProxyMethod;
        public string MethodName;
        public Type TargetType;
        /// <summary>
        /// 目标方法的影子方法，用于确认签名
        /// </summary>
        public MethodInfo ShadowMethod;

        public string TargetTypeFullName { get; internal set; }
        public override string ToString()
        {
            return $"{TargetTypeFullName}.{MethodName}".ToString();
        }
    }

    public class ClrMethodHook
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
            IEnumerable<IMethodHook> monitors;
            if (string.IsNullOrEmpty(dir))
            {
                monitors = assemblies.SelectMany(t => t.GetImplementedObjectsByInterface<IMethodHook>());
            }
            else
            {
                assemblies = assemblies.Concat(Directory
                            .GetFiles(dir, "*.dll")
                            .Select(d => { try { return Assembly.LoadFrom(d); } catch { return null; } })
                            .Where(x => x != null))
                            .Distinct()
                            .ToArray();
                monitors = assemblies
                            .SelectMany(d => d.GetImplementedObjectsByInterface<IMethodHook>());
            }
            List<MethodInfo> proxyMethods = new List<MethodInfo>();
            List<MethodInfo> clrMethods = new List<MethodInfo>();
            foreach (var monitor in monitors)
            {
                var type = monitor.GetType();



                proxyMethods.AddRange(type.GetMethods().Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(RelocatedMethodAttribute))));
                proxyMethods.AddRange(type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(RelocatedMethodAttribute))));
                proxyMethods.AddRange(type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(RelocatedMethodAttribute))));

                clrMethods.AddRange(type.GetMethods().Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(ShadowMethodAttribute))));
                clrMethods.AddRange(type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(ShadowMethodAttribute))));
                clrMethods.AddRange(type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(ShadowMethodAttribute))));
            }
            foreach (var item in proxyMethods.Distinct())
            {
                DestAndOri detour = new DestAndOri();
                var proxyMethodAttr = item.GetCustomAttribute<RelocatedMethodAttribute>();
                var clrMethod = clrMethods.FirstOrDefault(x => ParametersSequenceEqual(x, item) && x.GetCustomAttribute<ShadowMethodAttribute>().TargetMethodName == proxyMethodAttr.TargetMethodName);
                detour.ProxyMethod = item;
                detour.ShadowMethod = clrMethod;
                detour.MethodName = proxyMethodAttr.TargetMethodName;
                detour.TargetType = proxyMethodAttr.TargetType;
                detour.TargetTypeFullName = proxyMethodAttr.TargetTypeName;
                destAndOris.Add(detour);
            }
            InstallInternal(assemblies);
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        }

        private static void InstallInternal(Assembly[] assemblies)
        {
            foreach (var detour in destAndOris)
            {
                MethodBase rawMethod = null;
                var customImplMethod = detour.ProxyMethod;
                var methodName = detour.MethodName;
                var paramTypes = customImplMethod.GetParameters().Select(t => t.ParameterType).ToArray();
                foreach (var asm in assemblies)
                {
                    Type type = null;
                    if (detour.TargetType.IsGenericType)
                    {
                        type = asm.GetTypes().FirstOrDefault(t => t.BaseType == detour.TargetType.DeclaringType && detour.TargetType.GenericTypeArguments.SequenceEqual(t.GenericTypeArguments));
                        if (type == null)
                        {
                            type = asm.GetTypes().FirstOrDefault(t => t.Name == detour.TargetType.Name && t.Namespace == detour.TargetType.Namespace && t.Module.FullyQualifiedName == detour.TargetType.Module.FullyQualifiedName);
                            if (type == null)
                            {
                                var types = asm.GetTypes().Where(t => t.Name.StartsWith("Computer"));
                                if (types.Any())
                                {
                                    var m = type.GenericTypeArguments.SequenceEqual(detour.TargetType.GenericTypeArguments);
                                }
                            }
                            else
                            {
                                type = type.MakeGenericType(detour.TargetType.GenericTypeArguments);
                            }
                        }
                    }
                    else
                    {
                        type = asm.GetTypes().FirstOrDefault(t => t.FullName == detour.TargetTypeFullName);
                    }
                    if (type != null)
                    {
                        if (methodName == ".ctor")
                        {
                            rawMethod = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .FirstOrDefault(item => item.Name == methodName
                                && ParametersSequenceEqual(item.GetParameters().ToList(), (detour.ShadowMethod ?? customImplMethod).GetParameters().ToList(), assemblies));
                        }
                        else
                        {
                            rawMethod = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                            //src = type.GetMethods((detour.ShadowMethod.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) | (detour.ShadowMethod.IsStatic ? BindingFlags.Static : BindingFlags.Instance))
                                .FirstOrDefault(item => item.Name == methodName
                                && ParametersSequenceEqual(item.GetParameters().ToList(), (detour.ShadowMethod ?? customImplMethod).GetParameters().ToList(), assemblies));

                        }
                        break;
                    }
                }
                if (rawMethod == null)
                {
                    Debug.WriteLine("没有找到与试图Hook的方法\"{0}\"匹配的目标方法.", new object[] { detour.TargetTypeFullName + "." + detour.MethodName });
                    continue;
                }

                var shadowMethod = detour.ShadowMethod;

                //IsStatic必须一致，否则报内存访问错误
                if (rawMethod.IsStatic != shadowMethod.IsStatic)
                {
                    var clrDesc = rawMethod.IsStatic ? "static" : "  non static";
                    var shadowDesc = shadowMethod.IsStatic ? "static" : "  non static";
                    throw new Exception(string.Format("the method \"{0}\" you implemented is {1}, but the target method \"{2}\" is {3}",
                        shadowMethod.DeclaringType + "." + shadowMethod.Name,
                        shadowDesc,
                        rawMethod.DeclaringType + "." + rawMethod.Name,
                        clrDesc
                        ));
                }
                var engine = DetourFactory.CreateDetourEngine();
                if (engine.Patch(rawMethod, customImplMethod, shadowMethod))
                {
                    Debug.WriteLine("已将目标方法 \"{0}\" 的调用指向 \"{1}\".", new object[] { rawMethod.DeclaringType + "." + rawMethod.Name, customImplMethod.DeclaringType + "." + customImplMethod.Name });
                }
                else
                {
                    Debug.WriteLine("可能在历史调用中已将目标方法的调用\"{0}\"指向\"{1}\".", new object[] { rawMethod.DeclaringType + "." + rawMethod.Name, customImplMethod.DeclaringType + "." + customImplMethod.Name });
                }
            }
        }
        public static bool ParametersSequenceEqual(MethodBase p1, MethodBase p2)
        {
            return p1.GetParameters().Select(x => x.ParameterType).SequenceEqual(p2.GetParameters().Select(x => x.ParameterType));
        }
        public static bool ParametersSequenceEqual(List<ParameterInfo> p1, List<ParameterInfo> p2, Assembly[] assemblies)
        {
            if (p1.SequenceEqual(p2))
            {
                return true;
            }
            else if (p1.Count() != p2.Count())
            {
                return false;
            }
            else
            {
                foreach (var type2 in p2)
                {
                    var index = p2.IndexOf(type2);
                    var d = type2.GetCustomAttributesData();
                    var opt = type2.GetCustomAttribute<NonPublicParameterTypeAttribute>();
                    if (opt != null)
                    {
                        var itemType = assemblies.SelectMany(x => x.GetTypes()).FirstOrDefault(x => x.FullName == opt.FullName);
                        if (itemType != null && itemType.Equals(p1[index].ParameterType))
                        {
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (!type2.ParameterType.Equals(p1[index].ParameterType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            InstallInternal(new[] { args.LoadedAssembly });
        }
    }
}
