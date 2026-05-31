namespace Unity.Collections
{
    using System;
    using Unity.Jobs;

    public interface INativeDisposable : IDisposable
    {
        JobHandle Dispose(JobHandle inputDeps);
    }
}
