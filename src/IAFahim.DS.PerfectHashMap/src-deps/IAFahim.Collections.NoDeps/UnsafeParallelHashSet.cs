namespace Unity.Collections.LowLevel.Unsafe
{
    using System;
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public unsafe struct UnsafeParallelHashSet<T> : IDisposable
        where T : unmanaged, IEquatable<T>
    {
        private NativeHashSet<T> set;

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.set.IsCreated;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeParallelHashSet(int capacity, Allocator allocator)
        {
            this.set = new NativeHashSet<T>(capacity, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item)
        {
            return this.set.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            return this.set.Remove(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(T item)
        {
            return this.set.Contains(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            this.set.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Count()
        {
            return this.set.Count();
        }

        public void Dispose()
        {
            this.set.Dispose();
        }

        public readonly NativeHashSet<T>.Enumerator GetEnumerator()
        {
            return this.set.GetEnumerator();
        }
    }
}
