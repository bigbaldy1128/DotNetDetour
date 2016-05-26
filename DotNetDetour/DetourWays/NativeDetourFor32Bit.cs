using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using System.Threading;
using System.Linq.Expressions;
using System.IO;

namespace DotNetDetour.DetourWays
{
    /// <summary>
    /// inline hook,通过修改函数的前5字节指令为jmp target_addr实现
    /// </summary>
    public unsafe class NativeDetourFor32Bit: IDetour
    {
        //protected byte[] originalInstrs = new byte[5];
        protected byte[] newInstrs = { 0xE9, 0x90, 0x90, 0x90, 0x90 }; //jmp target
        protected byte* srcPtr;

        public NativeDetourFor32Bit()
        {
        }

        public virtual void Patch(MethodInfo src, MethodInfo dest,MethodInfo ori)
        {
            //确保jit过了
            var typeHandles = src.DeclaringType.GetGenericArguments().Select(t => t.TypeHandle).ToArray();
            RuntimeHelpers.PrepareMethod(src.MethodHandle, typeHandles);
            
            srcPtr = (byte*)src.MethodHandle.GetFunctionPointer().ToPointer();
            var destPtr = (byte*)dest.MethodHandle.GetFunctionPointer().ToPointer();
            if (ori != null)
            {
                CreateOriginalMethod(ori); //生成原函数
            }
            fixed (byte* newInstrPtr=newInstrs)
            {
                *(uint*)(newInstrPtr + 1) = (uint)destPtr - (uint)srcPtr - 5;
            }
            Patch();
        }

        protected virtual void Patch()
        {
            uint oldProtect;
            //系统方法没有写权限，需要修改页属性
            NativeAPI.VirtualProtect((IntPtr)srcPtr, 5, Protection.PAGE_EXECUTE_READWRITE, out oldProtect);
            for (int i = 0; i < newInstrs.Length; i++)
            {
                *(srcPtr + i) = newInstrs[i];
            }
        }

        protected virtual void CreateOriginalMethod(MethodInfo method)
        {
            uint oldProtect;
            var needSize = LDasm.SizeofMin5Byte(srcPtr);
            var total_length = (int)needSize + 5;
            byte[] code = new byte[total_length];
            IntPtr ptr = Marshal.AllocHGlobal(total_length);
            //code[0] = 0xcc;//调试用
            for (int i = 0; i < needSize; i++)
            {
                code[i] = srcPtr[i];
            }
            code[needSize] = 0xE9;
            fixed (byte* p = &code[needSize + 1])
            {
                *((uint*)p) = (uint)srcPtr - (uint)ptr - 5;
            }
            Marshal.Copy(code, 0, ptr, total_length);
            NativeAPI.VirtualProtect(ptr, (uint)total_length, Protection.PAGE_EXECUTE_READWRITE, out oldProtect);
            RuntimeHelpers.PrepareMethod(method.MethodHandle);
            *((uint*)method.MethodHandle.Value.ToPointer() + 2) = (uint)ptr;
        }

    }
}
