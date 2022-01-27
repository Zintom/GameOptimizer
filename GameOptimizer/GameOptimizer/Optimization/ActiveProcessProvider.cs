using System;

namespace Zintom.GameOptimizer.Optimization
{
    internal interface IActiveProcessProvider
    {
        /// <inheritdoc cref="System.Diagnostics.Process.GetProcesses"/>
        IProcess[] GetProcesses();
    }

    /// <summary>
    /// Provides an array of all active processes on this system.
    /// </summary>
    internal class ActiveProcessProvider : IActiveProcessProvider
    {
        /// <summary>
        /// Abstracts <see cref="System.Diagnostics.Process.GetProcesses"/>
        /// </summary>
        public IProcess[] GetProcesses() => Array.ConvertAll(System.Diagnostics.Process.GetProcesses(), p => new ProcessWrapper(p));
    }
}
