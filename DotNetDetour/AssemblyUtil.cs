using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace DotNetDetour
{
    public static class AssemblyUtil
    {
        public static T CreateInstance<T>(string type)
        {
            return CreateInstance<T>(type, new object[0]);
        }

        /// <summary>
        /// 在当前的程序集中反射创建实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T CreateInstance<T>(string type, object[] parameters)
        {
            Type instanceType = null;
            var result = default(T);

            instanceType = Type.GetType(type, false,true);
            if (instanceType == null)
                return default(T);
            object instance = Activator.CreateInstance(instanceType, parameters);
            result = (T)instance;
            return result;
        }

        /// <summary>
        /// 在给定的程序集中反射创建实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembleName"></param>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>

        public static T CreateInstance<T>(string assembleName, string type)
        {
            Type instanceType = null;
            var result = default(T);
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assem in asms)
            {
                if (string.Equals(assem.FullName, assembleName, StringComparison.CurrentCultureIgnoreCase))
                {
                    var types = assem.GetTypes();
                    foreach (var t in types)
                    {
                        if (string.Equals(t.ToString(), type, StringComparison.CurrentCultureIgnoreCase))
                        {
                            instanceType = t;
                            break;
                        }
                    }
                    break;
                }
            }
            if (instanceType == null)
                return default(T);
            object instance = Activator.CreateInstance(instanceType, new object[0]);
            result = (T)instance;
            return result;
        }

        /// <summary>
        /// 在给定的程序集中反射创建实例,并且传入构造函数的参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembleName"></param>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T CreateInstance<T>(string assembleName, string type, object[] parameters)
        {
            Type instanceType = null;
            var result = default(T);
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assem in asms)
            {
                if (string.Equals(assem.FullName, assembleName, StringComparison.CurrentCultureIgnoreCase))
                {
                    var types = assem.GetTypes();
                    foreach (var t in types)
                    {
                        if (string.Equals(t.ToString(), type, StringComparison.CurrentCultureIgnoreCase))
                        {
                            instanceType = t;
                            break;
                        }
                    }
                    break;
                }
            }
            if (instanceType == null)
                return default(T);
            object instance = Activator.CreateInstance(instanceType, parameters);
            result = (T)instance;
            return result;
        }    

        public static IEnumerable<Type> GetImplementTypes<TBaseType>(this Assembly assembly)
        {
            return assembly.GetExportedTypes().Where(t =>
                t.IsSubclassOf(typeof(TBaseType)) && t.IsClass && !t.IsAbstract);
        }

        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly)
            where TBaseInterface : class
        {
            return GetImplementedObjectsByInterface<TBaseInterface>(assembly, typeof(TBaseInterface));
        }

        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Type targetType)
            where TBaseInterface : class
        {
            Type[] arrType = assembly.GetExportedTypes();

            var result = new List<TBaseInterface>();

            for (int i = 0; i < arrType.Length; i++)
            {
                var currentImplementType = arrType[i];

                if (currentImplementType.IsAbstract)
                    continue;

                if (!targetType.IsAssignableFrom(currentImplementType))
                    continue;

                result.Add((TBaseInterface)Activator.CreateInstance(currentImplementType));
            }

            return result;
        }

        public static T BinaryClone<T>(this T target)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, target);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        private static object[] m_EmptyObjectArray = new object[] { };
        public static T CopyPropertiesTo<T>(this T source, T target)
        {
            PropertyInfo[] properties = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            Dictionary<string, PropertyInfo> sourcePropertiesDict = properties.ToDictionary(p => p.Name);

            PropertyInfo[] targetProperties = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            for (int i = 0; i < targetProperties.Length; i++)
            {
                var p = targetProperties[i];
                PropertyInfo sourceProperty;

                if (sourcePropertiesDict.TryGetValue(p.Name, out sourceProperty))
                {
                    if (sourceProperty.PropertyType != p.PropertyType)
                        continue;

                    if (!sourceProperty.PropertyType.IsSerializable)
                        continue;

                    p.SetValue(target, sourceProperty.GetValue(source, m_EmptyObjectArray), m_EmptyObjectArray);
                }
            }

            return target;
        }

        public static IEnumerable<Assembly> GetAssembliesFromString(string assemblyDef)
        {
            return GetAssembliesFromStrings(assemblyDef.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public static IEnumerable<Assembly> GetAssembliesFromStrings(string[] assemblies)
        {
            List<Assembly> result = new List<Assembly>(assemblies.Length);

            foreach (var a in assemblies)
            {
                result.Add(Assembly.Load(a));
            }

            return result;
        }

        public static string GetAssembleVer(string filePath)
        {        
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(filePath);
            string versionStr = string.Format(" {0}.{1}.{2}.{3}", fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart, fvi.ProductPrivatePart);
            return versionStr;
        }
    }
}
