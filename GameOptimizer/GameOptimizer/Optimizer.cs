using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameOptimizer
{

    /// <summary>
    /// Flags that can modify the behaviour of the Optimizer.
    /// </summary>
    [Flags]
    public enum OptimizeFlags
    {
        None = 0,
        /// <summary>
        /// The windows explorer should be killed.
        /// </summary>
        KillExplorerExe = 1,
        /// <summary>
        /// The window should not hide after optimization.
        /// </summary>
        NoHide = 2,
        /// <summary>
        /// The optimizer should boost each priority process to <see cref="ProcessPriorityClass.AboveNormal"/> rather than leaving it at <see cref="ProcessPriorityClass.Normal"/>.
        /// </summary>
        BoostPriorities = 4,
        /// <summary>
        /// The optimizer should not de-prioritize 'non-priority' proccesses.
        /// </summary>
        IgnoreOrdinaryProcesses = 8,

        OptimizeAffinity = 16
    }

    /// <summary>
    /// Maps <see cref="Process.ProcessorAffinity"/> to enum values.
    /// </summary>
    [Flags]
    public enum ProcessAffinity
    {
        Null = 0,
        Core0 = 1,
        Core1 = 2,
        Core2 = 4,
        Core3 = 8,
        Core4 = 16,
        Core5 = 32,
        Core6 = 64,
        Core7 = 128,
        Core8 = 256,
        Core9 = 512,
        Core10 = 1024,
        Core11 = 2048,
        Core12 = 4096,
        Core13 = 8192,
        Core14 = 16384,
        Core15 = 32768,
    }

    /// <summary>
    /// A basic interface for outputting messages to the client.
    /// </summary>
    public interface IOutputProvider
    {
        /// <summary>
        /// The given <paramref name="errorMessage"/> is an error.
        /// </summary>
        void OutputError(string errorMessage);

        /// <summary>
        /// The given <paramref name="message"/> should be highlighted.
        /// </summary>
        void OutputHighlight(string message);

        /// <summary>
        /// The given <paramref name="message"/> should be treated as 'normal' or 'default'.
        /// </summary>
        void Output(string message);
    }

    public class Optimizer
    {

        /// <summary>
        /// Helper class for storing changes made to <see cref="Process"/>.
        /// </summary>
        private class ProcessStateChange
        {
            /// <summary>
            /// The <see cref="Process"/> that has been modified.
            /// </summary>
            internal Process ChangedProcess { get; }

            /// <summary>
            /// The <see cref="ProcessPriorityClass"/> prior to the change.
            /// </summary>
            internal ProcessPriorityClass? PreChangePriority { get; }

            /// <summary>
            /// The <see cref="ProcessAffinity"/> prior to the change.
            /// </summary>
            internal IntPtr? PreChangeAffinity { get; }

            internal ProcessStateChange(Process process, ProcessPriorityClass? preChangePriority, IntPtr? preChangeAffinity)
            {
                ChangedProcess = process;
                PreChangePriority = preChangePriority;
                PreChangeAffinity = preChangeAffinity;
            }
        }

        private readonly IReadOnlyList<string> _priorityProcessNames;

        private readonly IOutputProvider _outputProvider;

        private readonly List<ProcessStateChange> _changedProcesses;

        private OptimizeFlags _flagsUsedForOptimize = OptimizeFlags.None;

        private readonly int _optimizeAffinityMinimumCores = 4;
        private readonly int _affinityAllCores = 0.SetBitRange(0, Environment.ProcessorCount);

        /// <summary>
        /// <b>true</b> if optimization has been run. Returns to <b>false</b> when <see cref="Restore"/> is ran.
        /// </summary>
        public bool IsOptimized { get; private set; } = false;
        /// <summary>
        /// Whether the optimizer should display errors when it encounters them.
        /// </summary>
        public bool ShowErrorCodes { get; set; } = false;

        /// <param name="priorityProcessNames"></param>
        /// <param name="outputProvider">If not <b>null</b>, the optimizer will use this to output messages/problems or errors.</param>
        public Optimizer(IReadOnlyList<string> priorityProcessNames, IOutputProvider outputProvider = null)
        {
            _priorityProcessNames = priorityProcessNames;
            _outputProvider = outputProvider;
            _changedProcesses = new List<ProcessStateChange>();
        }

        public void Optimize(OptimizeFlags flags = OptimizeFlags.None)
        {
            if (IsOptimized) throw new InvalidOperationException("Cannot optimize whilst already optimized, please Restore first.");
            IsOptimized = true;

            _flagsUsedForOptimize = flags;

            if (!flags.HasFlag(OptimizeFlags.BoostPriorities) && flags.HasFlag(OptimizeFlags.IgnoreOrdinaryProcesses))
                throw new ArgumentException($"The given flags ({flags}) stop the Optimize method from actually doing any optimization, " +
                    "in its current state, flags is saying to not boost priorities and to ignore non-priorities.");

            if (flags.HasFlag(OptimizeFlags.OptimizeAffinity) && Environment.ProcessorCount < _optimizeAffinityMinimumCores)
                _outputProvider?.OutputHighlight($"{OptimizeFlags.OptimizeAffinity} flag is not applied on machines with less than {_optimizeAffinityMinimumCores}.");

            Process[] currentProcesses = Process.GetProcesses();

            foreach (Process process in currentProcesses)
            {
                if (process.ProcessName == "explorer" && flags.HasFlag(OptimizeFlags.KillExplorerExe))
                {
                    process.Kill();
                    continue;
                }

                foreach (string priority in _priorityProcessNames)
                {
                    if (process.ProcessName.ToLower() == priority.ToLower())
                    {
                        if (flags.HasFlag(OptimizeFlags.BoostPriorities))
                        {
                            ChangePriority(process, ProcessPriorityClass.AboveNormal);
                            _outputProvider?.OutputHighlight("Prioritized '" + process.ProcessName + "' because it is listed as a priority process.");
                        }
                        else
                        {
                            _outputProvider?.OutputHighlight("Ignored: '" + process.ProcessName + "' because it is listed as a priority process.");
                        }

                        goto continueLoop;
                    }
                }

                if (process.ProcessName != "svchost" && !flags.HasFlag(OptimizeFlags.IgnoreOrdinaryProcesses))
                {
                    // Set process to idle priority.
                    ChangePriority(process, ProcessPriorityClass.Idle);

                    if (flags.HasFlag(OptimizeFlags.OptimizeAffinity) && Environment.ProcessorCount >= _optimizeAffinityMinimumCores)
                    {
                        // Set the affinity to the last 2 cores.
                        int newAffinity = 0.SetBitRange(Environment.ProcessorCount - 2, Environment.ProcessorCount);

                        ChangeAffinity(process, (ProcessAffinity)newAffinity);
                    }
                }

            continueLoop:;
            }
        }

        public void Restore()
        {
            if (_flagsUsedForOptimize.HasFlag(OptimizeFlags.KillExplorerExe))
                Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe");

            if (_changedProcesses.Count == 0)
            {
                _outputProvider?.OutputError("No changes to restore.");
                return;
            }

            foreach (ProcessStateChange change in _changedProcesses)
            {
                try
                {
                    if (change.PreChangePriority != null)
                    {
                        change.ChangedProcess.PriorityClass = (ProcessPriorityClass)change.PreChangePriority;

                        _outputProvider?.Output($"Restored '{change.ChangedProcess.ProcessName}' priority to '{change.PreChangePriority}'.");
                    }
                }
                catch (System.ComponentModel.Win32Exception) { }

                try
                {
                    if (change.PreChangeAffinity != null)
                    {
                        change.ChangedProcess.ProcessorAffinity = (IntPtr)change.PreChangeAffinity;

                        _outputProvider?.Output($"Restored '{change.ChangedProcess.ProcessName}' affinity '{GetReadableAffinity(change.PreChangeAffinity)}'.");
                    }
                }
                catch (System.ComponentModel.Win32Exception) { }
            }

            _changedProcesses.Clear();

            IsOptimized = false;
        }

        public void ForceRestoreToNormal()
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                if (process.ProcessName != "svchost")
                {
                    ChangePriority(process, ProcessPriorityClass.Normal);
                    ChangeAffinity(process, (ProcessAffinity)_affinityAllCores);
                }
            }

            _changedProcesses.Clear();
        }

        string GetReadableAffinity(IntPtr? affinity)
        {
            if (affinity == null) return "null";

            if ((int)affinity == _affinityAllCores)
                return "All cores";
            else
                return ((ProcessAffinity)(int)affinity).ToString();
        }

        void ChangeAffinity(Process process, ProcessAffinity affinity)
        {
            try
            {
                // Store the pre-change here so that if we crash on the next line it will not be added to _changedProcesses.
                IntPtr preChangeAffinity = process.ProcessorAffinity;

                process.ProcessorAffinity = (IntPtr)affinity;

                // New affinity assignment was sucessful so log the change.
                _changedProcesses.Add(new ProcessStateChange(process, null, preChangeAffinity));

                _outputProvider?.Output(process.ProcessName + " : Affinity -> " + GetReadableAffinity((IntPtr)affinity));
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                if (ShowErrorCodes)
                    _outputProvider?.OutputError($"Failed to limit affinity on '{process.ProcessName}' because of Error Code {e.NativeErrorCode} ({e.Message})");
            }
            catch (InvalidOperationException) { return; }
        }

        void ChangePriority(Process p, ProcessPriorityClass newPriority, bool highlight = false)
        {
            // Original value
            string original;
            try
            {
                original = p.PriorityClass.ToString();
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                if (!ShowErrorCodes) return;

                _outputProvider?.OutputError($"'{p.ProcessName}' could not be accessed due to Error Code {e.NativeErrorCode} ({e.Message}).");
                return;
            }
            catch (InvalidOperationException) { return; }

            // Set new value
            try
            {
                // Store the pre-change here so that if we crash on the next line it will not be added to _changedProcesses.
                var preChangePriority = p.PriorityClass;

                p.PriorityClass = newPriority;

                // Priority change was successful so log the change.
                _changedProcesses.Add(new ProcessStateChange(p, preChangePriority, null));
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                if (!ShowErrorCodes) return;

                _outputProvider?.OutputError($"'{p.ProcessName}' remains Priority.{p.PriorityClass} due to Error Code {e.NativeErrorCode} ({e.Message}).");
                return;
            }
            catch (InvalidOperationException) { return; }

            // Print changes
            if (highlight)
                _outputProvider?.OutputHighlight(p.ProcessName + " : " + original + " -> " + newPriority);
            else
                _outputProvider?.Output(p.ProcessName + " : " + original + " -> " + newPriority);
        }

    }
}