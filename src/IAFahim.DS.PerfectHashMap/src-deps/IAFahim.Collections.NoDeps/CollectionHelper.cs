namespace Unity.Collections
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public static class CollectionHelper
    {
        public const int CacheLineSize = 64;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Align(int size, int alignment)
        {
            return (size + alignment - 1) & ~(alignment - 1);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void CheckAllocator(AllocatorManager.AllocatorHandle allocator)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AssumePositive(int value)
        {
            return value;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckIndexInRange(int index, int length)
        {
            if (index < 0 || index >= length)
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}
