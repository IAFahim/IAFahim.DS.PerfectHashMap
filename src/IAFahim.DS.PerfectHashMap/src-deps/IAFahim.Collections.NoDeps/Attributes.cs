namespace Unity.Collections.LowLevel.Unsafe
{
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NativeDisableUnsafePtrRestrictionAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class NativeContainerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NativeSetThreadIndexAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class NativeContainerIsAtomicWriteOnlyAttribute : Attribute { }
}

namespace Unity.Collections
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class ReadOnlyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NativeDisableParallelForRestrictionAttribute : Attribute { }
}

namespace Unity.Burst
{
    using System;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class BurstCompileAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Struct)]
    public sealed class NoAliasAttribute : Attribute { }
}

namespace Unity.Jobs
{
    public interface IJob
    {
        void Execute();
    }

    public interface IJobFor
    {
        void Execute(int index);
    }

    public static class JobExtensions
    {
        public static JobHandle Schedule<T>(this T job, JobHandle dependency = default) where T : struct, IJob
        {
            job.Execute();
            return dependency;
        }

        public static JobHandle ScheduleParallel<T>(this T job, int arrayLength, int innerloopBatchCount, JobHandle dependency = default) where T : struct, IJobFor
        {
            for (int i = 0; i < arrayLength; i++)
            {
                job.Execute(i);
            }
            return dependency;
        }
    }
}
