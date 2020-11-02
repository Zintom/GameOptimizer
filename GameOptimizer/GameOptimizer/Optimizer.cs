using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Zintom.GameOptimizer
{

    /// <summary>
    /// A bitmask of conditions that can modify the behaviour of the Optimizer.
    /// </summary>
    [Flags]
    public enum OptimizeConditions
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
        /// <summary>
        /// The optimizer should try to adjust the <see cref="Process.ProcessorAffinity"/> of 'non-priority' processes.
        /// </summary>
        OptimizeAffinity = 16
    }

    /// <summary>
    /// Maps <see cref="Process.ProcessorAffinity"/> to enum values.
    /// </summary>
    [Flags]
    public enum ProcessAffinities
    {
        None = 0,
        CPU_0 = 1,
        CPU_1 = 2,
        CPU_2 = 4,
        CPU_3 = 8,
        CPU_4 = 16,
        CPU_5 = 32,
        CPU_6 = 64,
        CPU_7 = 128,
        CPU_8 = 256,
        CPU_9 = 512,
        CPU_10 = 1_024,
        CPU_11 = 2_048,
        CPU_12 = 4_096,
        CPU_13 = 8_192,
        CPU_14 = 16_384,
        CPU_15 = 32_768,
        CPU_16 = 65_536,
        CPU_17 = 131_072,
        CPU_18 = 262_144,
        CPU_19 = 524_288,
        CPU_20 = 1_048_576,
        CPU_21 = 2_097_152,
        CPU_22 = 4_194_304,
        CPU_23 = 8_388_608,
        CPU_24 = 16_777_216,
        CPU_25 = 33_554_432,
        CPU_26 = 67_108_864,
        CPU_27 = 134_217_728,
        CPU_28 = 268_435_456,
        CPU_29 = 536_870_912,
        CPU_30 = 1_073_741_824,
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
            /// The <see cref="ProcessAffinities"/> prior to the change.
            /// </summary>
            internal IntPtr? PreChangeAffinity { get; }

            internal ProcessStateChange(Process process, ProcessPriorityClass? preChangePriority, IntPtr? preChangeAffinity)
            {
                ChangedProcess = process;
                PreChangePriority = preChangePriority;
                PreChangeAffinity = preChangeAffinity;
            }
        }

        /// <summary>
        /// Compares two processes by their <see cref="Process.ProcessName"/>.
        /// </summary>
        private class ProcessSorter : IComparer<Process>
        {
            public int Compare(Process? x, Process? y)
            {
                return x?.ProcessName?.CompareTo(y?.ProcessName) ?? 0;
            }
        }

        private readonly IOutputProvider? _outputProvider;

        private readonly IReadOnlyList<string> _priorityProcessNames;

        private readonly List<ProcessStateChange> _changedProcesses;

        private OptimizeConditions _flagsUsedForOptimize = OptimizeConditions.None;

        private readonly int _optimizeAffinityMinimumCores = 4;
        private readonly int _affinityAllCores = 0.SetBitRange(0, Environment.ProcessorCount);

        /// <summary>
        /// <b>true</b> if optimization has been run. Returns to <b>false</b> when <see cref="Restore"/> is ran.
        /// </summary>
        public bool IsOptimized { get; private set; }
        /// <summary>
        /// Whether the optimizer should display errors when it encounters them.
        /// </summary>
        public bool ShowErrorCodes { get; set; }

        /// <param name="outputProvider">If not <b>null</b>, the optimizer will use this to output messages/problems or errors.</param>
        public Optimizer(IReadOnlyList<string> priorityProcessNames, IOutputProvider? outputProvider = null)
        {
            _priorityProcessNames = priorityProcessNames;
            _outputProvider = outputProvider;
            _changedProcesses = new List<ProcessStateChange>();
        }

        public void Optimize(OptimizeConditions flags = OptimizeConditions.None)
        {
            if (IsOptimized) throw new InvalidOperationException("Cannot optimize whilst already optimized, please Restore first.");
            IsOptimized = true;

            _flagsUsedForOptimize = flags;

            #region Flag Checks
            if (!flags.HasFlag(OptimizeConditions.BoostPriorities) && flags.HasFlag(OptimizeConditions.IgnoreOrdinaryProcesses))
                _outputProvider?.OutputError($"The given flags ({flags}) stop the Optimize method from actually doing any optimization, " +
                    "in its current state, flags is saying to not boost priorities and to ignore non-priorities.");

            if (flags.HasFlag(OptimizeConditions.IgnoreOrdinaryProcesses) && flags.HasFlag(OptimizeConditions.OptimizeAffinity))
                _outputProvider?.OutputError($"Flag conflict! {OptimizeConditions.OptimizeAffinity} is overridden by {OptimizeConditions.IgnoreOrdinaryProcesses}.");

            if (flags.HasFlag(OptimizeConditions.OptimizeAffinity) && Environment.ProcessorCount < _optimizeAffinityMinimumCores)
                _outputProvider?.OutputHighlight($"{OptimizeConditions.OptimizeAffinity} flag is not applied on machines with less than {_optimizeAffinityMinimumCores}.");
            #endregion

            Process[] currentProcesses = Process.GetProcesses();

            // Sort the array alphabetically.
            Array.Sort(currentProcesses, new ProcessSorter());

            foreach (Process process in currentProcesses)
            {
                if (process.ProcessName == "explorer" && flags.HasFlag(OptimizeConditions.KillExplorerExe))
                {
                    process.Kill();
                    continue;
                }

                foreach (string priority in _priorityProcessNames)
                {
                    if (process.ProcessName.ToLower() == priority.ToLower())
                    {
                        if (flags.HasFlag(OptimizeConditions.BoostPriorities))
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

                if (process.ProcessName != "svchost" && !flags.HasFlag(OptimizeConditions.IgnoreOrdinaryProcesses))
                {
                    // Set process to idle priority.
                    ChangePriority(process, ProcessPriorityClass.Idle);

                    if (flags.HasFlag(OptimizeConditions.OptimizeAffinity) && Environment.ProcessorCount >= _optimizeAffinityMinimumCores)
                    {
                        // Set the affinity to the last 2 cores.
                        int newAffinity = 0.SetBitRange(Environment.ProcessorCount - 2, Environment.ProcessorCount);

                        ChangeAffinity(process, (ProcessAffinities)newAffinity);
                    }
                }

            continueLoop:;
            }
        }

        public void Restore()
        {
            if (_flagsUsedForOptimize.HasFlag(OptimizeConditions.KillExplorerExe))
                Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe");

            if (_changedProcesses.Count == 0)
            {
                _outputProvider?.OutputError("No changes to restore.");
                return;
            }   

            foreach (ProcessStateChange change in _changedProcesses)
            {
                // If the process has exited there is no reason to continue as it will just throw an exception.
                if (change.ChangedProcess.HasExited)
                    continue;

                // Restore Priority
                try
                {
                    if (change.PreChangePriority != null)
                    {
                        change.ChangedProcess.PriorityClass = (ProcessPriorityClass)change.PreChangePriority;

                        _outputProvider?.Output($"Restored '{change.ChangedProcess.ProcessName}' priority to '{change.PreChangePriority}'.");
                    }
                }
                catch (System.ComponentModel.Win32Exception) { }

                // Restore affinity
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
                    ChangeAffinity(process, (ProcessAffinities)_affinityAllCores);
                }
            }

            _changedProcesses.Clear();

            IsOptimized = false;
        }

        string GetReadableAffinity(IntPtr? affinity)
        {
            if (affinity == null) return "null";

            if ((int)affinity == _affinityAllCores)
                return "All cores";
            else
                return ((ProcessAffinities)(int)affinity).ToString();
        }

        void ChangeAffinity(Process process, ProcessAffinities affinity)
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