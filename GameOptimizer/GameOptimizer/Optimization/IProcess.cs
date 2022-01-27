using System;
using System.Diagnostics;

namespace Zintom.GameOptimizer.Optimization
{
    /// <summary>
    /// Maps the interface members of <see cref="IProcess"/> onto a <see cref="Process"/>.
    /// </summary>
    /// <remarks>
    /// Because .NET doesn't support casting an object to an interface <b>with the same public methods</b> unless that object <b>explicitly implements the interface</b> <i><b>WHY?</b></i>
    /// <para/>
    /// Also, before you try, you can't make "IProcess" a child class of Process as it still will not cast.
    /// </remarks>
    internal class ProcessWrapper : IProcess
    {
        private readonly Process _wrappedProcess;

        public int Id => _wrappedProcess.Id;

        public string ProcessName => _wrappedProcess.ProcessName;

        public IntPtr ProcessorAffinity { get => _wrappedProcess.ProcessorAffinity; set => _wrappedProcess.ProcessorAffinity = value; }

        public bool HasExited => _wrappedProcess.HasExited;

        public ProcessPriorityClass PriorityClass { get => _wrappedProcess.PriorityClass; set => _wrappedProcess.PriorityClass = value; }

        public void Kill() => _wrappedProcess.Kill();

        /// <inheritdoc cref="ProcessWrapper"/>
        internal ProcessWrapper(Process process)
        {
            _wrappedProcess = process;
        }

    }

    internal interface IProcess
    {
        /// <inheritdoc cref="Process.Id"/>
        int Id { get; }

        /// <inheritdoc cref="Process.Kill"/>
        void Kill();

        /// <inheritdoc cref="Process.ProcessName"/>
        string ProcessName { get; }

        /// <inheritdoc cref="Process.ProcessorAffinity"/>
        IntPtr ProcessorAffinity { get; set; }

        /// <inheritdoc cref="Process.HasExited"/>
        bool HasExited { get; }

        /// <inheritdoc cref="Process.PriorityClass"/>
        ProcessPriorityClass PriorityClass { get; set; }

        /// <inheritdoc cref="Process.GetProcessById(int)"/>
        static IProcess GetProcessById(int processId) => new ProcessWrapper(Process.GetProcessById(processId));

    }
}
