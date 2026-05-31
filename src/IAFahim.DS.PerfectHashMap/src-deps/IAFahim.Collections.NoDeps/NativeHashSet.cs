namespace Unity.Collections
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Unity.Collections.LowLevel.Unsafe;

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeHashSet<T> : IDisposable
        where T : unmanaged, IEquatable<T>
    {
        private const uint FnvOffsetBasis = 2166136261;
        private const uint FnvPrime = 16777619;
        private const int MinBucketCount = 16;
        private const int EmptyBucket = -1;

        [StructLayout(LayoutKind.Sequential)]
        private struct HashSetState
        {
            public T* Keys;
            public int* Next;
            public int* Buckets;
            public int Capacity;
            public int Length;
            public int BucketCount;
            public Allocator Allocator;
        }

        private HashSetState* state;

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.state != null;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.state != null ? this.state->Capacity : 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (this.state == null)
                {
                    return;
                }
                int val = value < 0 ? 0 : value;
                if (val == this.state->Capacity)
                {
                    return;
                }
                this.Reallocate(val);
            }
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.state != null ? this.state->Length : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeHashSet(int capacity, Allocator allocator)
        {
            this.state = (HashSetState*)Marshal.AllocHGlobal((IntPtr)sizeof(HashSetState));
            this.state->Capacity = capacity;
            this.state->Length = 0;
            this.state->Allocator = allocator;

            int bucketCount = capacity < MinBucketCount ? MinBucketCount : capacity * 2;
            int powerOf2 = 1;
            while (powerOf2 < bucketCount)
            {
                powerOf2 <<= 1;
            }
            this.state->BucketCount = powerOf2;

            this.state->Keys = capacity > 0 ? (T*)Marshal.AllocHGlobal((IntPtr)((long)capacity * sizeof(T))) : null;
            this.state->Next = capacity > 0 ? (int*)Marshal.AllocHGlobal((IntPtr)((long)capacity * sizeof(int))) : null;
            this.state->Buckets = (int*)Marshal.AllocHGlobal((IntPtr)((long)this.state->BucketCount * sizeof(int)));

            for (int i = 0; i < capacity; i++)
            {
                this.state->Next[i] = EmptyBucket;
            }
            for (int i = 0; i < this.state->BucketCount; i++)
            {
                this.state->Buckets[i] = EmptyBucket;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item)
        {
            if (this.state == null)
            {
                return false;
            }

            if (this.Contains(item))
            {
                return false;
            }

            if (this.state->Length >= this.state->Capacity)
            {
                int newCap = this.state->Capacity < 4 ? 8 : this.state->Capacity * 2;
                this.Reallocate(newCap);
            }

            uint hash = Hash(item);
            int bucket = (int)(hash & (uint)(this.state->BucketCount - 1));

            int entryIdx = this.state->Length;
            this.state->Keys[entryIdx] = item;
            this.state->Next[entryIdx] = this.state->Buckets[bucket];
            this.state->Buckets[bucket] = entryIdx;
            this.state->Length++;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            if (this.state == null || this.state->Length == 0)
            {
                return false;
            }

            uint hash = Hash(item);
            int bucket = (int)(hash & (uint)(this.state->BucketCount - 1));

            int prev = -1;
            int entry = this.state->Buckets[bucket];
            while (entry != EmptyBucket)
            {
                if (this.state->Keys[entry].Equals(item))
                {
                    if (prev == -1)
                    {
                        this.state->Buckets[bucket] = this.state->Next[entry];
                    }
                    else
                    {
                        this.state->Next[prev] = this.state->Next[entry];
                    }

                    int lastIdx = this.state->Length - 1;
                    if (entry < lastIdx)
                    {
                        T lastKey = this.state->Keys[lastIdx];
                        this.state->Keys[entry] = lastKey;

                        uint lastHash = Hash(lastKey);
                        int lastBucket = (int)(lastHash & (uint)(this.state->BucketCount - 1));

                        int curr = this.state->Buckets[lastBucket];
                        int currPrev = -1;
                        while (curr != EmptyBucket)
                        {
                            if (curr == lastIdx)
                            {
                                if (currPrev == -1)
                                {
                                    this.state->Buckets[lastBucket] = entry;
                                }
                                else
                                {
                                    this.state->Next[currPrev] = entry;
                                }
                                this.state->Next[entry] = this.state->Next[lastIdx];
                                break;
                            }
                            currPrev = curr;
                            curr = this.state->Next[curr];
                        }
                    }

                    this.state->Length--;
                    return true;
                }
                prev = entry;
                entry = this.state->Next[entry];
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(T item)
        {
            if (this.state == null || this.state->Length == 0)
            {
                return false;
            }

            uint hash = Hash(item);
            int bucket = (int)(hash & (uint)(this.state->BucketCount - 1));

            int entry = this.state->Buckets[bucket];
            while (entry != EmptyBucket)
            {
                if (this.state->Keys[entry].Equals(item))
                {
                    return true;
                }
                entry = this.state->Next[entry];
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (this.state == null)
            {
                return;
            }
            this.state->Length = 0;
            for (int i = 0; i < this.state->BucketCount; i++)
            {
                this.state->Buckets[i] = EmptyBucket;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Count()
        {
            return this.state != null ? this.state->Length : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeArray<T> ToNativeArray(Allocator allocator)
        {
            if (this.state == null || this.state->Length == 0)
            {
                return new NativeArray<T>(0, allocator);
            }
            NativeArray<T> array = new NativeArray<T>(this.state->Length, allocator);
            int idx = 0;
            for (int i = 0; i < this.state->BucketCount; i++)
            {
                int entry = this.state->Buckets[i];
                while (entry != EmptyBucket)
                {
                    array[idx++] = this.state->Keys[entry];
                    entry = this.state->Next[entry];
                }
            }
            return array;
        }

        public void Dispose()
        {
            if (this.state != null)
            {
                if (this.state->Keys != null)
                {
                    Marshal.FreeHGlobal((IntPtr)this.state->Keys);
                }
                if (this.state->Next != null)
                {
                    Marshal.FreeHGlobal((IntPtr)this.state->Next);
                }
                if (this.state->Buckets != null)
                {
                    Marshal.FreeHGlobal((IntPtr)this.state->Buckets);
                }
                Marshal.FreeHGlobal((IntPtr)this.state);
                this.state = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reallocate(int newCapacity)
        {
            T* newKeys = newCapacity > 0 ? (T*)Marshal.AllocHGlobal((IntPtr)((long)newCapacity * sizeof(T))) : null;
            int* newNext = newCapacity > 0 ? (int*)Marshal.AllocHGlobal((IntPtr)((long)newCapacity * sizeof(int))) : null;

            if (this.state->Length > 0)
            {
                UnsafeUtility.MemCpy(newKeys, this.state->Keys, (long)this.state->Length * sizeof(T));
                UnsafeUtility.MemCpy(newNext, this.state->Next, (long)this.state->Length * sizeof(int));
            }

            for (int i = this.state->Length; i < newCapacity; i++)
            {
                newNext[i] = EmptyBucket;
            }

            if (this.state->Keys != null)
            {
                Marshal.FreeHGlobal((IntPtr)this.state->Keys);
            }
            if (this.state->Next != null)
            {
                Marshal.FreeHGlobal((IntPtr)this.state->Next);
            }

            this.state->Keys = newKeys;
            this.state->Next = newNext;
            this.state->Capacity = newCapacity;

            int newBucketCount = newCapacity < MinBucketCount ? MinBucketCount : newCapacity * 2;
            int powerOf2 = 1;
            while (powerOf2 < newBucketCount)
            {
                powerOf2 <<= 1;
            }

            if (powerOf2 != this.state->BucketCount)
            {
                Marshal.FreeHGlobal((IntPtr)this.state->Buckets);
                this.state->Buckets = (int*)Marshal.AllocHGlobal((IntPtr)((long)powerOf2 * sizeof(int)));
                this.state->BucketCount = powerOf2;
            }

            for (int i = 0; i < this.state->BucketCount; i++)
            {
                this.state->Buckets[i] = EmptyBucket;
            }

            for (int i = 0; i < this.state->Length; i++)
            {
                uint hash = Hash(this.state->Keys[i]);
                int bucket = (int)(hash & (uint)(this.state->BucketCount - 1));
                this.state->Next[i] = this.state->Buckets[bucket];
                this.state->Buckets[bucket] = i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Hash(T key)
        {
            int h = key.GetHashCode();
            uint hash = FnvOffsetBasis;
            byte* ptr = (byte*)&h;
            for (int i = 0; i < sizeof(int); i++)
            {
                hash = (hash ^ ptr[i]) * FnvPrime;
            }
            return hash;
        }

        public struct Enumerator : System.Collections.Generic.IEnumerator<T>
        {
            private readonly NativeHashSet<T> set;
            private int bucketIndex;
            private int entryIndex;
            private T current;

            internal Enumerator(NativeHashSet<T> set)
            {
                this.set = set;
                this.bucketIndex = -1;
                this.entryIndex = -1;
                this.current = default;
            }

            public bool MoveNext()
            {
                if (this.set.state == null)
                {
                    return false;
                }

                if (this.entryIndex != EmptyBucket)
                {
                    this.entryIndex = this.set.state->Next[this.entryIndex];
                    if (this.entryIndex != EmptyBucket)
                    {
                        this.current = this.set.state->Keys[this.entryIndex];
                        return true;
                    }
                }

                while (true)
                {
                    this.bucketIndex++;
                    if (this.bucketIndex >= this.set.state->BucketCount)
                    {
                        return false;
                    }

                    int firstEntry = this.set.state->Buckets[this.bucketIndex];
                    if (firstEntry != EmptyBucket)
                    {
                        this.entryIndex = firstEntry;
                        this.current = this.set.state->Keys[this.entryIndex];
                        return true;
                    }
                }
            }

            public void Reset()
            {
                this.bucketIndex = -1;
                this.entryIndex = -1;
                this.current = default;
            }

            public readonly T Current => this.current;
            readonly object System.Collections.IEnumerator.Current => this.Current;

            public readonly void Dispose()
            {
            }
        }

        public readonly Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
