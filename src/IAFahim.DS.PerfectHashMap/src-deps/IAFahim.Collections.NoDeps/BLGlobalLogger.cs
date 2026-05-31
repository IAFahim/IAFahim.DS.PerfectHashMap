namespace BovineLabs.Core
{
    using System;
    using System.Diagnostics;

    public static class BLGlobalLogger
    {
        [Conditional("UNITY_DOTS_DEBUG")]
        public static void LogError512(string msg) {}

        [Conditional("UNITY_DOTS_DEBUG")]
        public static void LogWarningString(string msg) {}
    }
}
