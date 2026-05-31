namespace BovineLabs.Core.Memory
{
    using System;
    using System.Runtime.InteropServices;
    using Unity.Collections;

    public static unsafe class Unmanaged
    {
        public static void* Allocate(long size, int alignment, AllocatorManager.AllocatorHandle allocator)
        {
            return (void*)Marshal.AllocHGlobal((IntPtr)size);
        }

        public static void Free(void* ptr, AllocatorManager.AllocatorHandle allocator)
        {
            if (ptr != null)
            {
                Marshal.FreeHGlobal((IntPtr)ptr);
            }
        }
    }
}
