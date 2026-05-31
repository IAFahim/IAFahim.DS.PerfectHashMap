namespace IAFahim.DS.PerfectHashMap
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Unity.Burst.CompilerServices;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Jobs.LowLevel.Unsafe;
    using BovineLabs.Core.Memory;

    public unsafe struct UnsafePerfectHashMap<TKey, TValue> : IDisposable
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged, IEquatable<TValue>
    {
        [NativeDisableUnsafePtrRestriction]
        internal TKey* Keys;

        [NativeDisableUnsafePtrRestriction]
        internal TValue* Values;

        internal int Size;
        internal TValue NullValue;

        private readonly AllocatorManager.AllocatorHandle allocator;

        public UnsafePerfectHashMap(NativeArray<TKey> keys, NativeArray<TValue> values, TValue nullValue, AllocatorManager.AllocatorHandle allocator)
        {
            NativeHashSet<int> uniqueSet = new NativeHashSet<int>(keys.Length, Allocator.Temp);
            AssertCollisionFree(keys, uniqueSet);

            int size = FindSize(keys, uniqueSet);
            int valueOffset;
            long totalSize = CalculateDataSize(size, out valueOffset);

            void* ptr = Unmanaged.Allocate(totalSize, JobsUtility.CacheLineSize, allocator);
            this.allocator = allocator;
            this.Size = size;
            this.NullValue = nullValue;
            this.Keys = (TKey*)ptr;
            this.Values = (TValue*)((byte*)ptr + valueOffset);

            UnsafeUtility.MemCpyReplicate(this.Values, &nullValue, sizeof(TValue), size);

            for (int i = 0; i < keys.Length; i++)
            {
                int index = IndexFor(keys[i], size);
                this.Keys[index] = keys[i];
                this.Values[index] = values[i];
            }
        }

        public static UnsafePerfectHashMap<TKey, TValue>* Alloc(
            NativeArray<TKey> keys, NativeArray<TValue> values, TValue nullValue, AllocatorManager.AllocatorHandle allocator)
        {
            UnsafePerfectHashMap<TKey, TValue>* data = (UnsafePerfectHashMap<TKey, TValue>*)Unmanaged.Allocate(
                (long)sizeof(UnsafePerfectHashMap<TKey, TValue>),
                UnsafeUtility.AlignOf<UnsafePerfectHashMap<TKey, TValue>>(),
                allocator);

            *data = new UnsafePerfectHashMap<TKey, TValue>(keys, values, nullValue, allocator);
            return data;
        }

        public static void Free(UnsafePerfectHashMap<TKey, TValue>* data)
        {
            if (data == null)
            {
                throw new InvalidOperationException("Hash based container has yet to be created or has been destroyed!");
            }

            AllocatorManager.AllocatorHandle allocator = data->allocator;
            data->Dispose();
            Unmanaged.Free(data, allocator);
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.Keys != null;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (Hint.Unlikely(!this.TryGetValue(key, out value)))
                {
                    this.ThrowKeyNotPresent(key);
                    return default;
                }

                return value;
            }

            set
            {
                int index;
                if (!this.TryGetIndex(key, out index))
                {
                    this.ThrowKeyNotPresent(key);
                }

                this.Values[index] = value;
            }
        }

        public void Dispose()
        {
            if (!this.IsCreated)
            {
                return;
            }

            Unmanaged.Free(this.Keys, this.allocator);
            this = default;
        }

        public bool TryGetValue(TKey key, out TValue item)
        {
            int index;
            if (!this.TryGetIndex(key, out index))
            {
                item = default;
                return false;
            }

            item = this.Values[index];
            return !item.Equals(this.NullValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IndexFor(TKey key, int size)
        {
            return key.GetHashCode() & (size - 1);
        }

        private static int FindSize(NativeArray<TKey> keys, NativeHashSet<int> unique)
        {
            int size = 1;

            while (HasCollisions(size, keys, unique))
            {
                size <<= 1;
            }

            return size;
        }

        private static bool HasCollisions(int size, NativeArray<TKey> keys, NativeHashSet<int> usedIndexes)
        {
            usedIndexes.Clear();

            for (int i = 0; i < keys.Length; i++)
            {
                TKey key = keys[i];
                int index = IndexFor(key, size);

                if (!usedIndexes.Add(index))
                {
                    return true;
                }
            }

            return false;
        }

        private static long CalculateDataSize(int count, out int outValueOffset)
        {
            long sizeOfTKey = sizeof(TKey);
            long sizeOfTValue = sizeof(TValue);

            long keysSize = sizeOfTKey * count;
            long valuesSize = sizeOfTValue * count;
            long totalSize = valuesSize + keysSize;

            outValueOffset = (int)keysSize;

            return totalSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetIndex(TKey key, out int index)
        {
            index = this.IndexFor(key);
            return index >= 0 && index < this.Size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int IndexFor(TKey key)
        {
            return IndexFor(key, this.Size);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [Conditional("UNITY_DOTS_DEBUG")]
        private static void AssertCollisionFree(NativeArray<TKey> keys, NativeHashSet<int> unique)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                TKey key = keys[i];
                if (!unique.Add(key.GetHashCode()))
                {
                    throw new ArgumentException("HashCode collision.");
                }
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [Conditional("UNITY_DOTS_DEBUG")]
        private void ThrowKeyNotPresent(TKey key)
        {
            throw new ArgumentException($"Key: {key} is not present.");
        }
    }
}
