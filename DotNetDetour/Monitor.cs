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
    class DestAndOri
    {
        /// <summary>
        /// 代理方法
        /// </summary>
		public MethodBase RelocatedMethod { get; set; }

        /// <summary>
        /// 目标方法的影子方法
        /// </summary>
		public MethodBase ShadowMethod { get; set; }

		public IMethodHook Obj;
    }

	[Obsolete("此类已变更为ClrMethodHook")]
	public class Monitor : ClrMethodHook {}

    public class ClrMethodHook
    {
		static public BindingFlags AllFlag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
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

			foreach (var monitor in monitors) {
				var all = monitor.GetType().GetMethods(AllFlag);
				var relocatedMethods = all.Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(RelocatedMethodAttribute)));
				var shadowMethods = all.Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(ShadowMethodAttribute)));

				var destCount = relocatedMethods.Count();
				foreach (var relocated in relocatedMethods) {
					DestAndOri destAndOri = new DestAndOri();
					destAndOri.Obj = monitor;
					destAndOri.RelocatedMethod = relocated;
					if (destCount == 1) {
						destAndOri.ShadowMethod = shadowMethods.FirstOrDefault();
					} else {
						var shadowName = relocated.GetCustomAttribute<RelocatedMethodAttribute>().GetShadowMethodName(relocated);

						destAndOri.ShadowMethod = FindMethod(shadowMethods.ToArray(), shadowName, relocated);
					}

					destAndOris.Add(destAndOri);
				}
			}

            InstallInternal(true, assemblies);
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        }

        private static void InstallInternal(bool isInstall, Assembly[] assemblies)
        {
            foreach (var detour in destAndOris)
            {
                var relocatedMethod = detour.RelocatedMethod;
				var relocatedAttribute = relocatedMethod.GetCustomAttribute<RelocatedMethodAttribute>();
				var type = relocatedAttribute.TargetType;
				if (type == null) {
					foreach (var asm in assemblies) {
						type = asm.GetTypes().FirstOrDefault(t => TypeEq(relocatedAttribute.TargetTypeFullName, t.FullName));
						if (type != null) {
							break;
						}
					}
				}
				var methodName = relocatedAttribute.GetTargetMethodName(relocatedMethod);
				MethodBase rawMethod = null;
				if (type != null) {
					MethodBase[] methods;

					if (methodName == type.Name || methodName == ".ctor") {//构造方法
						methods = type.GetConstructors(AllFlag);
						methodName = ".ctor";
					} else {
						methods = type.GetMethods(AllFlag);
					}

					rawMethod = FindMethod(methods, methodName, relocatedMethod);
				}
				if (rawMethod == null)
                {
					if (isInstall) {
						Debug.WriteLine("没有找到与试图Hook的方法\"{0}\"匹配的目标方法.", new object[] { relocatedMethod.ReflectedType.FullName + "." + methodName });
					}
                    continue;
				}
				if (detour.Obj is IMethodHookWithSet) {
					((IMethodHookWithSet)detour.Obj).HookMethod(rawMethod);
				}

                var shadowMethod = detour.ShadowMethod;
                var engine = DetourFactory.CreateDetourEngine();
				engine.Patch(rawMethod, relocatedMethod, shadowMethod);

				Debug.WriteLine("已将目标方法 \"{0}\" 的调用指向 \"{1}\" Shadow: {2}.", rawMethod.DeclaringType + "." + rawMethod.Name, relocatedMethod.DeclaringType + "." + relocatedMethod.Name, shadowMethod == null ? " (无)" : shadowMethod.DeclaringType + "." + shadowMethod.Name);
            }
        }

		/// <summary>
		/// 判断一个手写的类型是否和运行中的类型一致。
		/// txtType为手写的类型，realType为运行获取的类型。
		/// 手写类型支持：
		///		完整类型名称 System.Int32
		///		完整泛型名称`泛型参数数量
		///		System.Collections.Generic.List`1
		/// </summary>
		private static bool TypeEq(string txtType, string realType) {
			if (txtType == realType) {
				return true;
			}
			if (txtType.IndexOf("`") != -1) {
				return realType.StartsWith(txtType);
			}
			return false;
		}
		//查找匹配函数
		private static MethodBase FindMethod(MethodBase[] methods, string name, MethodBase like) {
			var likeParams = like.GetParameters();
			foreach (var item in methods) {
				if (item.Name != name) {
					continue;
				}

				var paramArr = item.GetParameters();
				var len = paramArr.Count();
				if (len != likeParams.Count()) {
					continue;
				}

				for (var i = 0; i < len; i++) {
					var t1 = likeParams[i];
					var t2 = paramArr[i];
					if (t1.ParameterType == t2.ParameterType) {
						continue;
					}

					var type = t1.GetCustomAttribute<RememberTypeAttribute>();
					if (type != null && TypeEq(type.FullName, t2.ParameterType.FullName)) {
						continue;
					}
					goto next;
				}
				return item;
			next:
				continue;
			}
			return null;
		}

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            InstallInternal(false, new[] { args.LoadedAssembly });
        }
    }
}
