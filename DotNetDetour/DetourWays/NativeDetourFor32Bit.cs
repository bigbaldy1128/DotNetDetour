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
    public unsafe class NativeDetourFor32Bit : IDetour
    {
        //protected byte[] originalInstrs = new byte[5];
        protected byte[] newInstrs = { 0xE9, 0x90, 0x90, 0x90, 0x90 }; //jmp target
        protected byte* rawMethodPtr;

        public NativeDetourFor32Bit()
        {
        }

        public virtual bool Patch(MethodBase rawMethod/*要hook的目标函数*/, 
            MethodInfo customImplMethod/*用户定义的函数，可以调用占位函数来实现对原函数的调用*/, 
            MethodInfo placeholder/*占位函数*/)
        {
            //确保jit过了
            var typeHandles = rawMethod.DeclaringType.GetGenericArguments().Select(t => t.TypeHandle).ToArray();
            RuntimeHelpers.PrepareMethod(rawMethod.MethodHandle, typeHandles);

            rawMethodPtr = (byte*)rawMethod.MethodHandle.GetFunctionPointer().ToPointer();

            var customImplMethodPtr = (byte*)customImplMethod.MethodHandle.GetFunctionPointer().ToPointer();
            //生成跳转指令，使用相对地址，用于跳转到用户定义函数
            fixed (byte* newInstrPtr = newInstrs)
            {
                *(uint*)(newInstrPtr + 1) = (uint)customImplMethodPtr - (uint)rawMethodPtr - 5;
            }

            //因测试项目的特殊性，确保测试项目代码不会重入
            if (IsDetourInstalled())
            {
                return false;
            }

            //将对占位函数的调用指向原函数，实现调用占位函数即调用原始函数的功能
            if (placeholder != null)
            {
                MakePlacholderMethodCallPointsToRawMethod(placeholder);
            }


            //并且将对原函数的调用指向跳转指令，以此实现将对原始目标函数的调用跳转到用户定义函数执行的目的
            Patch();
            return true;
        }

        protected virtual void Patch()
        {
            uint oldProtect;
            //系统方法没有写权限，需要修改页属性
            NativeAPI.VirtualProtect((IntPtr)rawMethodPtr, 5, Protection.PAGE_EXECUTE_READWRITE, out oldProtect);
            for (int i = 0; i < newInstrs.Length; i++)
            {
                *(rawMethodPtr + i) = newInstrs[i];
            }
        }

        /// <summary>
        /// 将对placeholder的调用指向原函数
        /// </summary>
        /// <param name="placeholder"></param>
        protected virtual void MakePlacholderMethodCallPointsToRawMethod(MethodInfo placeholder)
        {
            uint oldProtect;
            var needSize = LDasm.SizeofMin5Byte(rawMethodPtr);
            var total_length = (int)needSize + 5;
            byte[] code = new byte[total_length];
            IntPtr ptr = Marshal.AllocHGlobal(total_length);
            //code[0] = 0xcc;//调试用
            for (int i = 0; i < needSize; i++)
            {
                code[i] = rawMethodPtr[i];
            }
            code[needSize] = 0xE9;
            fixed (byte* p = &code[needSize + 1])
            {
                *((uint*)p) = (uint)rawMethodPtr - (uint)ptr - 5;
            }
            Marshal.Copy(code, 0, ptr, total_length);
            NativeAPI.VirtualProtect(ptr, (uint)total_length, Protection.PAGE_EXECUTE_READWRITE, out oldProtect);
            RuntimeHelpers.PrepareMethod(placeholder.MethodHandle);
            *((uint*)placeholder.MethodHandle.Value.ToPointer() + 2) = (uint)ptr;
        }

        public virtual bool IsDetourInstalled()
        {
            byte[] v = new byte[newInstrs.Length];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = *(rawMethodPtr + i);
            }
            return v.SequenceEqual(newInstrs);
        }
    }
}
