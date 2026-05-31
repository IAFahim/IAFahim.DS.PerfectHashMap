namespace BovineLabs.Core.Assertions
{
    using System.Diagnostics;

    public static class Assert
    {
        [Conditional("UNITY_DOTS_DEBUG")]
        public static void IsTrue(bool condition)
        {
            Debug.Assert(condition);
        }

        [Conditional("UNITY_DOTS_DEBUG")]
        public static void IsFalse(bool condition)
        {
            Debug.Assert(!condition);
        }

        [Conditional("UNITY_DOTS_DEBUG")]
        public static void AreEqual<T>(T expected, T actual)
        {
            Debug.Assert(System.Collections.Generic.EqualityComparer<T>.Default.Equals(expected, actual));
        }
    }

    public static class Check
    {
        [Conditional("UNITY_DOTS_DEBUG")]
        public static void Assume(bool assumption)
        {
            Debug.Assert(assumption);
        }

        [Conditional("UNITY_DOTS_DEBUG")]
        public static void Assume(bool assumption, string message)
        {
            Debug.Assert(assumption, message);
        }
    }
}
