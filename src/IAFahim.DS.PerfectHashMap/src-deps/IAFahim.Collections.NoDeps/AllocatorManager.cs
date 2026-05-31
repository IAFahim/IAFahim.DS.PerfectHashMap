namespace Unity.Collections
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static unsafe class AllocatorManager
    {
        public struct AllocatorHandle
        {
            public int Value;

            public readonly AllocatorHandle Handle => this;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Allocator(AllocatorHandle handle)
            {
                return (Allocator)handle.Value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator AllocatorHandle(Allocator allocator)
            {
                return new AllocatorHandle { Value = (int)allocator };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Allocate(AllocatorHandle allocator, long sizeInBytes, int alignInBytes)
        {
            return (void*)Marshal.AllocHGlobal((nint)sizeInBytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Allocate(AllocatorHandle allocator, int itemSizeInBytes, int alignmentInBytes, int items)
        {
            long byteCount = (long)itemSizeInBytes * items;
            return (void*)Marshal.AllocHGlobal((nint)byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Allocate(AllocatorHandle allocator, void* ptr, long sizeInBytes, int alignInBytes)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Allocate<T>(AllocatorHandle allocator, int items = 1) where T : unmanaged
        {
            long byteCount = (long)items * sizeof(T);
            return (T*)Marshal.AllocHGlobal((nint)byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(AllocatorHandle allocator, void* pointer)
        {
            if (pointer != null)
                Marshal.FreeHGlobal((nint)pointer);
        }
    }
}
