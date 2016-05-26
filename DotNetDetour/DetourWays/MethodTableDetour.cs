using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour.DetourWays
{
    /// <summary>
    /// 方法表hook,通过修改jitted code指针实现
    /// </summary>
    public class MethodTableDetour : IDetour
    {
        object oldPointer;
        IntPtr destAdr;
        IntPtr srcAdr;

        public object CallOriginalMethod(object o = null, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Patch(MethodInfo src, MethodInfo dest)
        {
            if (!MethodSignaturesEqual(src, dest))
            {
                throw new ArgumentException("The method signatures are not the same.", "source");
            }
            destAdr = GetMethodAddress(dest);
            srcAdr = GetMethodAddress(src);
            unsafe
            {
                if (IntPtr.Size == 8)
                {
                    ulong* d = (ulong*)destAdr.ToPointer();
                    oldPointer = *d;
                    *d = *((ulong*)srcAdr.ToPointer());
                }
                else
                {
                    uint* d = (uint*)destAdr.ToPointer();
                    oldPointer = *d;
                    *d = *((uint*)srcAdr.ToPointer());
                }
            }
        }

        private void Recover()
        {
            unsafe
            {
                if (IntPtr.Size == 8)
                {
                    ulong* d = (ulong*)destAdr.ToPointer();
                    *d = (ulong)oldPointer;
                }
                else
                {
                    uint* d = (uint*)destAdr.ToPointer();
                    *d = (uint)oldPointer;
                }
            }
        }

        private IntPtr GetMethodAddress(MethodBase method)
        {
            if ((method is DynamicMethod))
            {
                return GetDynamicMethodAddress(method);
            }

            // Prepare the method so it gets jited
            RuntimeHelpers.PrepareMethod(method.MethodHandle);

            // If 3.5 sp1 or greater than we have a different layout in memory.
            if (IsNet20Sp2OrGreater())
            {
                return GetMethodAddress20SP2(method);
            }

            unsafe
            {
                // Skip these
                const int skip = 10;

                // Read the method index.
                UInt64* location = (UInt64*)(method.MethodHandle.Value.ToPointer());
                int index = (int)(((*location) >> 32) & 0xFF);

                if (IntPtr.Size == 8)
                {
                    // Get the method table
                    ulong* classStart = (ulong*)method.DeclaringType.TypeHandle.Value.ToPointer();
                    ulong* address = classStart + index + skip;
                    return new IntPtr(address);
                }
                else
                {
                    // Get the method table
                    uint* classStart = (uint*)method.DeclaringType.TypeHandle.Value.ToPointer();
                    uint* address = classStart + index + skip;
                    return new IntPtr(address);
                }
            }
        }

        private IntPtr GetDynamicMethodAddress(MethodBase method)
        {
            unsafe
            {
                byte* ptr = (byte*)GetDynamicMethodRuntimeHandle(method).ToPointer();
                if (IsNet20Sp2OrGreater())
                {
                    if (IntPtr.Size == 8)
                    {
                        ulong* address = (ulong*)ptr;
                        address = (ulong*)*(address + 5);
                        return new IntPtr(address + 12);
                    }
                    else
                    {
                        uint* address = (uint*)ptr;
                        address = (uint*)*(address + 5);
                        return new IntPtr(address + 12);
                    }
                }
                else
                {
                    if (IntPtr.Size == 8)
                    {
                        ulong* address = (ulong*)ptr;
                        address += 6;
                        return new IntPtr(address);
                    }
                    else
                    {
                        uint* address = (uint*)ptr;
                        address += 6;
                        return new IntPtr(address);
                    }
                }
            }
        }

        private IntPtr GetDynamicMethodRuntimeHandle(MethodBase method)
        {
            if (method is DynamicMethod)
            {
                FieldInfo fieldInfo = typeof(DynamicMethod).GetField("m_method", BindingFlags.NonPublic | BindingFlags.Instance);
                return ((RuntimeMethodHandle)fieldInfo.GetValue(method)).Value;
            }
            return method.MethodHandle.Value;
        }

        private static IntPtr GetMethodAddress20SP2(MethodBase method)
        {
            unsafe
            {
                return new IntPtr(((int*)method.MethodHandle.Value.ToPointer() + 2));
            }
        }

        private bool MethodSignaturesEqual(MethodBase x, MethodBase y)
        {
            if (x.CallingConvention != y.CallingConvention)
            {
                return false;
            }
            Type returnX = GetMethodReturnType(x), returnY = GetMethodReturnType(y);
            if (returnX != returnY)
            {
                return false;
            }
            ParameterInfo[] xParams = x.GetParameters(), yParams = y.GetParameters();
            if (xParams.Length != yParams.Length)
            {
                return false;
            }
            for (int i = 0; i < xParams.Length; i++)
            {
                if (xParams[i].ParameterType != yParams[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }

        private Type GetMethodReturnType(MethodBase method)
        {
            MethodInfo methodInfo = method as MethodInfo;
            if (methodInfo == null)
            {
                // Constructor info.
                throw new ArgumentException("Unsupported MethodBase : " + method.GetType().Name, "method");
            }
            return methodInfo.ReturnType;
        }

        private bool IsNet20Sp2OrGreater()
        {
            return Environment.Version.Major > 2;
        }

        
    }
}
