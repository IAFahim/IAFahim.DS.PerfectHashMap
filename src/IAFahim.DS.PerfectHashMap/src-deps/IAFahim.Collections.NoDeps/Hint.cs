namespace Unity.Burst.CompilerServices
{
    using System.Runtime.CompilerServices;

    public static class Hint
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Likely(bool val) => val;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unlikely(bool val) => val;
    }
}
