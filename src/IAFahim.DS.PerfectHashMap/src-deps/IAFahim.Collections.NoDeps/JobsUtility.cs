namespace Unity.Jobs.LowLevel.Unsafe
{
    public static class JobsUtility
    {
        public const int CacheLineSize = 64;
        public static int ThreadIndex => 0;
        public static int ThreadIndexCount => 1;
        public static int JobWorkerCount => 1;
    }
}
