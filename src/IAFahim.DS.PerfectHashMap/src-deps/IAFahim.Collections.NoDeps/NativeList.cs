namespace Unity.Collections
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Unity.Collections.LowLevel.Unsafe;

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeList<T> : IDisposable where T : unmanaged
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct ListState
        {
            public IntPtr Buffer;
            public int Length;
            public int Capacity;
            public Allocator Allocator;
        }

        private ListState* state;

        private static readonly int ElementSize = UnsafeUtility.SizeOf<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeList(int initialCapacity, Allocator allocator)
        {
            this.state = (ListState*)Marshal.AllocHGlobal((IntPtr)sizeof(ListState));
            this.state->Capacity = initialCapacity;
            this.state->Length = 0;
            this.state->Allocator = allocator;
            long byteCount = (long)initialCapacity * ElementSize;
            this.state->Buffer = initialCapacity > 0 ? Marshal.AllocHGlobal((IntPtr)byteCount) : IntPtr.Zero;
            if (initialCapacity > 0)
            {
                UnsafeUtility.MemClear((void*)this.state->Buffer, byteCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeList(Allocator allocator) : this(16, allocator)
        {
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.state != null ? this.state->Length : 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (this.state == null)
                {
                    return;
                }
                int val = value < 0 ? 0 : value;
                if (val > this.state->Capacity)
                {
                    this.ResizeCapacity(val);
                }
                this.state->Length = val;
            }
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
                this.ReallocateBuffer(val);
                this.state->Capacity = val;
                if (this.state->Length > this.state->Capacity)
                {
                    this.state->Length = this.state->Capacity;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void ReallocateBuffer(int newCapacity)
        {
            IntPtr newBuffer = newCapacity == 0
                ? IntPtr.Zero
                : Marshal.AllocHGlobal((IntPtr)((long)newCapacity * ElementSize));

            if (this.state->Buffer != IntPtr.Zero)
            {
                if (newCapacity > 0)
                {
                    int copySize = this.state->Length < newCapacity ? this.state->Length : newCapacity;
                    UnsafeUtility.MemCpy((void*)newBuffer, (void*)this.state->Buffer, (long)copySize * ElementSize);
                }
                Marshal.FreeHGlobal(this.state->Buffer);
            }
            this.state->Buffer = newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeCapacity(int newCapacity)
        {
            if (newCapacity <= this.state->Capacity)
            {
                return;
            }

            int cap = this.state->Capacity < 4 ? 4 : this.state->Capacity;
            while (cap < newCapacity && cap > 0)
            {
                int nextCap = cap << 1;
                if (nextCap < 0)
                {
                    cap = newCapacity;
                    break;
                }
                cap = nextCap;
            }
            if (cap < newCapacity)
            {
                cap = newCapacity;
            }
            this.Capacity = cap;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.state != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T* GetUnsafePtr()
        {
            return this.state != null ? (T*)this.state->Buffer : null;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                IntPtr byteOffset = this.state->Buffer + index * ElementSize;
                return System.Runtime.CompilerServices.Unsafe.Read<T>((void*)byteOffset);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                IntPtr byteOffset = this.state->Buffer + index * ElementSize;
                System.Runtime.CompilerServices.Unsafe.Write<T>((void*)byteOffset, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (this.state != null)
            {
                if (this.state->Buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(this.state->Buffer);
                }
                Marshal.FreeHGlobal((IntPtr)this.state);
                this.state = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
        {
            if (this.state != null)
            {
                this.state->Length = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            if (this.state->Length >= this.state->Capacity)
            {
                this.ResizeCapacity(this.state->Capacity < 4 ? 8 : this.state->Capacity << 1);
            }

            IntPtr byteOffset = this.state->Buffer + this.state->Length * ElementSize;
            System.Runtime.CompilerServices.Unsafe.Write<T>((void*)byteOffset, item);
            this.state->Length++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void RemoveAt(int index)
        {
            if (index < 0 || index >= this.state->Length)
            {
                return;
            }

            this.state->Length--;
            if (index == this.state->Length)
            {
                return;
            }

            long byteCount = (long)(this.state->Length - index) * ElementSize;
            byte* src = (byte*)this.state->Buffer + (index + 1) * ElementSize;
            byte* dst = (byte*)this.state->Buffer + index * ElementSize;
            UnsafeUtility.MemCpy(dst, src, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void RemoveRange(int index, int count)
        {
            if (index < 0 || count < 0 || index >= this.state->Length)
            {
                return;
            }

            int cnt = count > this.state->Length - index ? this.state->Length - index : count;

            this.state->Length -= cnt;
            if (index == this.state->Length || cnt == 0)
            {
                return;
            }

            long byteCount = (long)(this.state->Length - index) * ElementSize;
            byte* src = (byte*)this.state->Buffer + (index + cnt) * ElementSize;
            byte* dst = (byte*)this.state->Buffer + index * ElementSize;
            UnsafeUtility.MemCpy(dst, src, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int length, NativeArrayOptions options)
        {
            if (length < 0)
            {
                return;
            }

            if (length > this.state->Capacity)
            {
                this.Capacity = length;
            }

            int oldLength = this.state->Length;
            this.state->Length = length;

            if (options == NativeArrayOptions.ClearMemory && length > oldLength)
            {
                IntPtr startByte = this.state->Buffer + oldLength * ElementSize;
                long clearByteCount = (long)(length - oldLength) * ElementSize;
                UnsafeUtility.MemClear((byte*)startByte, clearByteCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResizeUninitialized(int length)
        {
            if (length < 0)
            {
                return;
            }

            if (length > this.state->Capacity)
            {
                this.Capacity = length;
            }

            this.state->Length = length;
        }

        public struct Enumerator : System.Collections.Generic.IEnumerator<T>
        {
            private readonly NativeList<T> list;
            private int index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeList<T> list)
            {
                this.list = list;
                this.index = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                this.index++;
                return this.index < this.list.Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                this.index = -1;
            }

            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.list[this.index];
            }

            readonly object System.Collections.IEnumerator.Current => this.Current;

            public readonly void Dispose()
            {
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeArray<T> AsArray()
        {
            return this.state != null
                ? new NativeArray<T>((void*)this.state->Buffer, this.state->Length, Allocator.None)
                : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeArray<T> AsDeferredJobArray()
        {
            return this.AsArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}