using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour.Extensions
{
    public static class TypeExtensions
    {
        public static T GetCustomAttribute<T>(this MemberInfo @this)
        {
            var list = @this.GetCustomAttributes(typeof(T), true)?.ToList();
            return (T)list.FirstOrDefault();
        }

        public static T GetCustomAttribute<T>(this ParameterInfo @this)
        {
            var list = @this.GetCustomAttributes(typeof(T), true)?.ToList();
            return (T)list.FirstOrDefault();
        }
    }
}
