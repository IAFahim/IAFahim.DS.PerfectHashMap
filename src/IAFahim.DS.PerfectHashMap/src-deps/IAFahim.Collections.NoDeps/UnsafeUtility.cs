namespace Unity.Collections.LowLevel.Unsafe
{
    using System;
    using System.Runtime.CompilerServices;

    public static unsafe class UnsafeUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>() where T : unmanaged
        {
            return sizeof(T);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AlignOf<T>() where T : unmanaged
        {
            return sizeof(AlignOfHelper<T>) - sizeof(T);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemCpy(void* destination, void* source, long size)
        {
            Buffer.MemoryCopy(source, destination, size, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemClear(void* destination, long size)
        {
            byte* p = (byte*)destination;
            for (long i = 0; i < size; i++)
                p[i] = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemSet(void* destination, byte value, long size)
        {
            byte* p = (byte*)destination;
            for (long i = 0; i < size; i++)
                p[i] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void* AddressOf<T>(ref T output) where T : unmanaged
        {
            return System.Runtime.CompilerServices.Unsafe.AsPointer(ref output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref U As<T, U>(ref T source)
            where T : unmanaged
            where U : unmanaged
        {
            return ref System.Runtime.CompilerServices.Unsafe.As<T, U>(ref source);
        }

        private struct AlignOfHelper<T> where T : unmanaged
        {
            private byte Dummy;
#pragma warning disable CS0649
            public T Data;
#pragma warning restore CS0649
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteArrayElement<T>(void* destination, int index, T value) where T : unmanaged
        {
            ((T*)destination)[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadArrayElement<T>(void* source, int index) where T : unmanaged
        {
            return ((T*)source)[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemCpyReplicate(void* destination, void* source, int size, int count)
        {
            byte* dest = (byte*)destination;
            byte* src = (byte*)source;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    dest[i * size + j] = src[j];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNativeContainerType<T>() where T : unmanaged
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T ArrayElementAsRef<T>(void* ptr, int index) where T : unmanaged
        {
            return ref ((T*)ptr)[index];
        }
    }
}