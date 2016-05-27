using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDetour.DetourWays
{
    public unsafe class NativeDetourFor64Bit : NativeDetourFor32Bit
    {
        byte[] jmp_inst =
        {
            0x50,                                              //push rax
            0x48,0xB8,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90, //mov rax,target_addr
            0x50,                                              //push rax
            0x48,0x8B,0x44,0x24,0x08,                          //mov rax,qword ptr ss:[rsp+8]
            0xC2,0x08,0x00                                     //ret 8
        };

        protected override void CreateOriginalMethod(MethodInfo method)
        {
            uint oldProtect;
            var needSize = LDasm.SizeofMin5Byte(srcPtr);
            byte[] src_instr = new byte[needSize];
            for (int i = 0; i < needSize; i++)
            {
                src_instr[i] = srcPtr[i];
            }
            fixed (byte* p = &jmp_inst[3])
            {
                *((ulong*)p) = (ulong)(srcPtr + needSize);
            }
            var totalLength = src_instr.Length + jmp_inst.Length;
            IntPtr ptr = Marshal.AllocHGlobal(totalLength);
            Marshal.Copy(src_instr, 0, ptr, src_instr.Length);
            Marshal.Copy(jmp_inst, 0, ptr + src_instr.Length, jmp_inst.Length);
            NativeAPI.VirtualProtect(ptr, (uint)totalLength, Protection.PAGE_EXECUTE_READWRITE, out oldProtect);
            RuntimeHelpers.PrepareMethod(method.MethodHandle);
            *((ulong*)((uint*)method.MethodHandle.Value.ToPointer() + 2)) = (ulong)ptr;
        }
    }
}
