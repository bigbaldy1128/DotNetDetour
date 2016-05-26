using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class A
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string StaticMethod()
        {
            return "A";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string InstanceMethod()
        {
            return "A";
        }

        public string Property
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                return "A";
            }
        }
    }

    public class A1<T>
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public T GenericMethod(T t)
        {
            return t;
        }
    }
}
