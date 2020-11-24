using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Zintom.StorageFacility;

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
    public enum ProcessAffinities : long
    {
        None = 0L,
        CPU_0 = 1L << 0,
        CPU_1 = 1L << 1,
        CPU_2 = 1L << 2,
        CPU_3 = 1L << 3,
        CPU_4 = 1L << 4,
        CPU_5 = 1L << 5,
        CPU_6 = 1L << 6,
        CPU_7 = 1L << 7,
        CPU_8 = 1L << 8,
        CPU_9 = 1L << 9,
        CPU_10 = 1L << 10,
        CPU_11 = 1L << 11,
        CPU_12 = 1L << 12,
        CPU_13 = 1L << 13,
        CPU_14 = 1L << 14,
        CPU_15 = 1L << 15,
        CPU_16 = 1L << 16,
        CPU_17 = 1L << 17,
        CPU_18 = 1L << 18,
        CPU_19 = 1L << 19,
        CPU_20 = 1L << 20,
        CPU_21 = 1L << 21,
        CPU_22 = 1L << 22,
        CPU_23 = 1L << 23,
        CPU_24 = 1L << 24,
        CPU_25 = 1L << 25,
        CPU_26 = 1L << 26,
        CPU_27 = 1L << 27,
        CPU_28 = 1L << 28,
        CPU_29 = 1L << 29,
        CPU_30 = 1L << 30,
        CPU_31 = 1L << 31,
        CPU_32 = 1L << 32,
        CPU_33 = 1L << 33,
        CPU_34 = 1L << 34,
        CPU_35 = 1L << 35,
        CPU_36 = 1L << 36,
        CPU_37 = 1L << 37,
        CPU_38 = 1L << 38,
        CPU_39 = 1L << 39,
        CPU_40 = 1L << 40,
        CPU_41 = 1L << 41,
        CPU_42 = 1L << 42,
        CPU_43 = 1L << 43,
        CPU_44 = 1L << 44,
        CPU_45 = 1L << 45,
        CPU_46 = 1L << 46,
        CPU_47 = 1L << 47,
        CPU_48 = 1L << 48,
        CPU_49 = 1L << 49,
        CPU_50 = 1L << 50,
        CPU_51 = 1L << 51,
        CPU_52 = 1L << 52,
        CPU_53 = 1L << 53,
        CPU_54 = 1L << 54,
        CPU_55 = 1L << 55,
        CPU_56 = 1L << 56,
        CPU_57 = 1L << 57,
        CPU_58 = 1L << 58,
        CPU_59 = 1L << 59,
        CPU_60 = 1L << 60,
        CPU_61 = 1L << 61,
        CPU_62 = 1L << 62,
        CPU_63 = 1L << 63
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
            public Process? ChangedProcess { get; }

            /// <summary>
            /// The <see cref="ProcessPriorityClass"/> prior to the change.
            /// </summary>
            public ProcessPriorityClass? PreChangePriority { get; }

            /// <summary>
            /// The <see cref="ProcessAffinities"/> prior to the change.
            /// </summary>
            public IntPtr? PreChangeAffinity { get; }

            public ProcessStateChange(Process? process, ProcessPriorityClass? preChangePriority, IntPtr? preChangeAffinity)
            {
                ChangedProcess = process;
                PreChangePriority = preChangePriority;
                PreChangeAffinity = preChangeAffinity;
            }

            public override string ToString()
            {
                return $"{ChangedProcess?.Id ?? 0},{(PreChangePriority == null ? "null" : (int)PreChangePriority)},{(PreChangeAffinity == null ? "null" : PreChangeAffinity)}";
            }

            /// <summary>
            /// Parses a given input, grammer format: <c>"int,null|int,null|int"</c>
            /// </summary>
            /// <returns>A <see cref="ProcessStateChange"/> with the values built from <paramref name="input"/>.</returns>
            public static ProcessStateChange Parse(string input, IOutputProvider? outputProvider)
            {
                string[] sections = input.Split(",");

                int pId = int.Parse(sections[0]);
                ProcessPriorityClass? pPriority = sections[1] != "null" ? (ProcessPriorityClass)int.Parse(sections[1]) : null;
                IntPtr? pAffinity = sections[2] != "null" ? IntPtr.Parse(sections[2]) : null;

                Process? pProcess = null;
                try
                {
                    pProcess = Process.GetProcessById(pId);
                }
                catch (ArgumentException e)
                {
                    outputProvider?.OutputError(e.Message);
                }
                return new ProcessStateChange(pProcess, pPriority, pAffinity);
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

        private readonly Storage _changedProcessesStorage;

        private OptimizeConditions _flagsUsedForOptimize = OptimizeConditions.None;

        private readonly int _optimizeAffinityMinimumCores = 4;
        private readonly nint _affinityAllCores = BitMask.SetBitRange(0, 0, Environment.ProcessorCount);

        private readonly object _whitelistLockObject = new object();
        private IReadOnlyList<string> _whitelistedProcessNames;

        /// <summary>
        /// Gets or sets the whitelisted process names list.
        /// </summary>
        public IReadOnlyList<string> WhitelistedProcessNames
        {
            get => _whitelistedProcessNames;
            set
            {
                lock (_whitelistLockObject)
                {
                    _whitelistedProcessNames = value;
                }
            }
        }

        /// <summary>
        /// <b>true</b> if optimization has been run. Returns to <b>false</b> when <see cref="Restore"/> is ran.
        /// </summary>
        public bool IsOptimized { get; private set; }
        /// <summary>
        /// Whether the optimizer should display errors when it encounters them.
        /// </summary>
        public bool ShowErrorCodes { get; set; }

        /// <param name="outputProvider">If not <b>null</b>, the optimizer will use this to output messages/problems or errors.</param>
        public Optimizer(IReadOnlyList<string> whitelistedProcessNames, IOutputProvider? outputProvider = null)
        {
            _whitelistedProcessNames = whitelistedProcessNames;
            _outputProvider = outputProvider;

            _changedProcessesStorage = Storage.GetStorage("restore_state");
            IsOptimized = _changedProcessesStorage.Strings.Count != 0;
        }

        /// <summary>
        /// Runs the optimizer with the given <paramref name="flags"/>.
        /// </summary>
        /// <returns>The number of optimizations ran.</returns>
        public int Optimize(OptimizeConditions flags = OptimizeConditions.None)
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

            // Lock on the process whitelist as we do not want it to be modified
            // whilst we are looping.
            Monitor.Enter(_whitelistLockObject);

            // Clear the restore_state file
            _changedProcessesStorage.Edit().Clear(true).Commit();

            Process[] currentProcesses = Process.GetProcesses();

            // Sort the array alphabetically.
            Array.Sort(currentProcesses, new ProcessSorter());

            int optimizationsRan = 0;
            foreach (Process process in currentProcesses)
            {
                if (process.ProcessName == "explorer" && flags.HasFlag(OptimizeConditions.KillExplorerExe))
                {
                    process.Kill();
                    optimizationsRan++;
                    continue;
                }

                foreach (string priority in _whitelistedProcessNames)
                {
                    if (process.ProcessName.ToLower() == priority.ToLower())
                    {
                        if (flags.HasFlag(OptimizeConditions.BoostPriorities))
                        {
                            // Set the priority to AboveNormal, if successful increment the
                            // optimizationsRan by one.
                            if (ChangePriority(process, ProcessPriorityClass.AboveNormal))
                                optimizationsRan++;

                            _outputProvider?.OutputHighlight("Prioritized '" + process.ProcessName + "' because it is a whitelisted process.");
                        }
                        else
                        {
                            _outputProvider?.OutputHighlight("Ignored: '" + process.ProcessName + "' because it is a whitelisted process.");
                        }

                        goto continueLoop;
                    }
                }

                if (process.ProcessName != "svchost" && !flags.HasFlag(OptimizeConditions.IgnoreOrdinaryProcesses))
                {
                    // Set process to idle priority, if successful increment the
                    // optimizationsRan by one.
                    if (ChangePriority(process, ProcessPriorityClass.Idle))
                        optimizationsRan++;

                    if (flags.HasFlag(OptimizeConditions.OptimizeAffinity) && Environment.ProcessorCount >= _optimizeAffinityMinimumCores)
                    {
                        // Set the affinity to the last 2 cores.
                        nint newAffinity = BitMask.SetBitRange(0, Environment.ProcessorCount - 2, Environment.ProcessorCount);

                        // Change the affinity, if successful increment the
                        // optimizationsRan by one.
                        if (ChangeAffinity(process, (ProcessAffinities)newAffinity))
                            optimizationsRan++;
                    }
                }

            continueLoop:;
            }

            // Release the whitelist so that it can be modified.
            Monitor.Exit(_whitelistLockObject);

            return optimizationsRan;
        }

        /// <summary>
        /// Restores all changes made to active processes by the `Optimize` method, this includes their Priority and Affinity.
        /// </summary>
        /// <returns>The number of restore operations completed.</returns>
        public int Restore()
        {
            if (_flagsUsedForOptimize.HasFlag(OptimizeConditions.KillExplorerExe))
                Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe");

            if (_changedProcessesStorage.Strings.Count == 0)
            {
                _outputProvider?.OutputError("No changes to restore.\n");
                return 0;
            }

            int restoreOperationsCompleted = 0;

            foreach (string changeString in _changedProcessesStorage.Strings.Values)
            {
                ProcessStateChange change = ProcessStateChange.Parse(changeString, ShowErrorCodes ? _outputProvider : null);

                // If the process has exited there is no reason to continue as it will just throw an exception.
                try
                {
                    if (change.ChangedProcess == null || change.ChangedProcess.HasExited)
                        continue;
                }
                catch (System.ComponentModel.Win32Exception) { continue; }

                // Priority and affinity are in their own
                // seperate catch blocks because you might be able to change
                // a processes priority but not its affinity.

                // Restore Priority
                try
                {
                    if (change.PreChangePriority != null)
                    {
                        change.ChangedProcess.PriorityClass = (ProcessPriorityClass)change.PreChangePriority;

                        _outputProvider?.Output($"Restored '{change.ChangedProcess.ProcessName}' priority to '{change.PreChangePriority}'.");

                        restoreOperationsCompleted++;
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

                        restoreOperationsCompleted++;
                    }
                }
                catch (System.ComponentModel.Win32Exception) { }
            }

            _changedProcessesStorage.Edit().Clear(true).Commit();

            IsOptimized = false;

            return restoreOperationsCompleted;
        }

        /// <summary>
        /// Forces all active processes to <see cref="ProcessPriorityClass.Normal"/> and All Core affinity.
        /// </summary>
        /// <returns>The number of processes affected by the force restore.</returns>
        public int ForceRestoreToNormal()
        {
            Process[] processes = Process.GetProcesses();

            int affectedProcesses = 0;

            foreach (Process process in processes)
            {
                if (process.ProcessName != "svchost")
                {
                    if (ChangePriority(process, ProcessPriorityClass.Normal)
                        || ChangeAffinity(process, (ProcessAffinities)_affinityAllCores))
                        affectedProcesses++;
                }
            }

            _changedProcessesStorage.Edit().Clear(true).Commit();

            IsOptimized = false;

            return affectedProcesses;
        }

        string GetReadableAffinity(IntPtr? affinity)
        {
            if (affinity == null) return "null";

            if (affinity == _affinityAllCores)
                return "All cores";
            else
                return ((ProcessAffinities)affinity).ToString();
        }

        /// <summary>
        /// Changes the given <paramref name="process"/> affinity to <paramref name="newAffinity"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the affinity was sucessfully changed, if it failed, returns <see langword="false"/>.</returns>
        bool ChangeAffinity(Process process, ProcessAffinities newAffinity)
        {
            try
            {
                // Store the pre-change here so that if we crash on the next line it will not be added to _changedProcesses.
                IntPtr preChangeAffinity = process.ProcessorAffinity;

                process.ProcessorAffinity = (IntPtr)newAffinity;

                // New affinity assignment was sucessful so log the change.
                var processStateChange = new ProcessStateChange(process, null, preChangeAffinity);
                _changedProcessesStorage.Edit().PutValue(DateTime.Now.Ticks + processStateChange.GetHashCode().ToString(), processStateChange.ToString()).Commit();

                _outputProvider?.Output(process.ProcessName + " : Affinity -> " + GetReadableAffinity((IntPtr)newAffinity));

                return true;
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                if (ShowErrorCodes)
                    _outputProvider?.OutputError($"Failed to limit affinity on '{process.ProcessName}' because of Error Code {e.NativeErrorCode} ({e.Message})");

                return false;
            }
            catch (InvalidOperationException) { return false; }
        }

        /// <summary>
        /// Changes the given <see cref="Process"/> <paramref name="p"/> priority to <paramref name="newPriority"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the priority was sucessfully changed, if it failed, returns <see langword="false"/>.</returns>
        bool ChangePriority(Process p, ProcessPriorityClass newPriority, bool highlight = false)
        {
            // Original value
            string original;
            try
            {
                original = p.PriorityClass.ToString();
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                if (!ShowErrorCodes) return false;

                _outputProvider?.OutputError($"'{p.ProcessName}' could not be accessed due to Error Code {e.NativeErrorCode} ({e.Message}).");
                return false;
            }
            catch (InvalidOperationException) { return false; }

            // Set new value
            try
            {
                // Store the pre-change here so that if we crash on the next line it will not be added to _changedProcesses.
                var preChangePriority = p.PriorityClass;

                p.PriorityClass = newPriority;

                // Priority change was successful so log the change.
                var processStateChange = new ProcessStateChange(p, preChangePriority, null);
                _changedProcessesStorage.Edit().PutValue(DateTime.Now.Ticks + processStateChange.GetHashCode().ToString(), processStateChange.ToString()).Commit();
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                if (!ShowErrorCodes) return false;

                _outputProvider?.OutputError($"'{p.ProcessName}' remains Priority.{p.PriorityClass} due to Error Code {e.NativeErrorCode} ({e.Message}).");
                return false;
            }
            catch (InvalidOperationException) { return false; }

            // Print changes
            if (highlight)
                _outputProvider?.OutputHighlight(p.ProcessName + " : " + original + " -> " + newPriority);
            else
                _outputProvider?.Output(p.ProcessName + " : " + original + " -> " + newPriority);

            return true;
        }

    }
}