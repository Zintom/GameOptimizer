using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Zintom.GameOptimizer.Optimization;
using Zintom.GameOptimizer.ProcessIdentifiers;
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
        /// The optimizer should boost each whitelisted process to <see cref="ProcessPriorityClass.AboveNormal"/> rather than leaving it at <see cref="ProcessPriorityClass.Normal"/>.
        /// </summary>
        BoostPriorities = 4,
        /// <summary>
        /// The optimizer should not de-prioritize 'non-whitelisted' proccesses.
        /// </summary>
        IgnoreOrdinaryProcesses = 8,
        /// <summary>
        /// Try to optimize the <see cref="IProcess.ProcessorAffinity"/> of non-game specific processes.
        /// </summary>
        /// <remarks>Whitelisted processes are not immune to this flag, only games are.</remarks>
        OptimizeAffinity = 16,
        /// <summary>
        /// The optimizer will try to put all '<c><see cref="Config.StreamerSpecificExecutables"/></c>' on the cores specified by '<c><see cref="Config.LimitStreamerSpecificExecutablesAffinity"/></c>',
        /// all other non 'streamer specific executables' will be put on the remaining available cores.
        /// </summary>
        StreamerMode = 32
    }

    /// <summary>
    /// Indicates to the optimizer whether the user wishes to optimize for SPEED or LATENCY.
    /// </summary>
    internal enum PerformancePreference
    {
        Speed,
        /// <summary>
        /// Ryzen Processors Only
        /// </summary>
        /// <remarks>
        /// Processors based on the Zen architecture incur a performance hit when work is distributed across CCX boundaries, optimizing for Latency in this case suggests the optimizer
        /// keep game processes on one CCX.
        /// </remarks>
        Latency
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
            public IProcess? ChangedProcess { get; }

            /// <summary>
            /// The <see cref="ProcessPriorityClass"/> prior to the change.
            /// </summary>
            public ProcessPriorityClass? PreChangePriority { get; }

            /// <summary>
            /// The <see cref="ProcessAffinities"/> prior to the change.
            /// </summary>
            public IntPtr? PreChangeAffinity { get; }

            public ProcessStateChange(IProcess? process, ProcessPriorityClass? preChangePriority, IntPtr? preChangeAffinity)
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

                IProcess? pProcess = null;
                try
                {
                    pProcess = IProcess.GetProcessById(pId);
                }
                catch (ArgumentException e)
                {
                    outputProvider?.OutputError(e.Message);
                }
                return new ProcessStateChange(pProcess, pPriority, pAffinity);
            }

        }

        /// <summary>
        /// Compares two processes by their <see cref="IProcess.ProcessName"/>.
        /// </summary>
        private class ProcessSorter : IComparer<IProcess>
        {
            public int Compare(IProcess? x, IProcess? y)
            {
                return x?.ProcessName?.CompareTo(y?.ProcessName) ?? 0;
            }
        }

        #region Private Members

        private readonly IOutputProvider? _outputProvider;

        private const string RestoreStateFile = "opt_restore_state";
        private readonly Storage? _restoreStateStorage;

        private OptimizeConditions _flagsUsedForOptimize = OptimizeConditions.None;

        private readonly nint _affinityAllCores = BitMask.SetBitRange(0, 0, Environment.ProcessorCount);

        private readonly Config? _config;

        private readonly object _optimizeLockObject = new();

        private readonly IActiveProcessProvider _activeProcessProvider;
        private readonly IWhitelistedProcessIdentifierSource _whitelistedProcessIdentifier;
        private readonly IGameProcessIdentifierSource _gameProcessIdentifier;

        #endregion

        #region Public Members

        /// <summary>
        /// <see langword="true"/> if optimization has been run. Returns to <see langword="false"/> when <see cref="Restore"/> is ran.
        /// </summary>
        internal bool IsOptimized { get; private set; }

        /// <summary>
        /// Whether the optimizer should display errors when it encounters them.
        /// </summary>
        internal bool ShowErrorCodes { get; set; }

        internal PerformancePreference PerformancePreference { get; set; } = PerformancePreference.Latency;

        #endregion

        /// <param name="outputProvider">If not <b>null</b>, the optimizer will use this to output messages/problems or errors.</param>
        /// <param name="holdState">Should the optimizer store its state on disk so that it may be restored at a later date?</param>
        internal Optimizer(IWhitelistedProcessIdentifierSource whitelistedProcessIdentifier, IGameProcessIdentifierSource gameProcessIdentifier, IActiveProcessProvider activeProcessProvider, Config? config = null, IOutputProvider? outputProvider = null, bool holdState = true)
        {
            this._whitelistedProcessIdentifier = whitelistedProcessIdentifier;
            this._gameProcessIdentifier = gameProcessIdentifier;
            this._activeProcessProvider = activeProcessProvider;
            this._outputProvider = outputProvider;
            this._config = config;

            if (holdState)
            {
                _restoreStateStorage = Storage.GetStorage(RestoreStateFile);
            }

            // Try to hide the restore state file from the user
            // as to not clutter the program directory.
            try
            {
                var fileAttributes = File.GetAttributes(RestoreStateFile);

                if (!fileAttributes.HasFlag(FileAttributes.Hidden))
                    File.SetAttributes(RestoreStateFile, fileAttributes | FileAttributes.Hidden);
            }
            catch { }

            if (_restoreStateStorage != null)
            {
                IsOptimized = _restoreStateStorage?.Strings.Count != 0;
            }
        }

        /// <summary>
        /// Runs the optimizer with the given <paramref name="flags"/>.
        /// </summary>
        /// <returns>The number of optimizations ran.</returns>
        internal int Optimize(OptimizeConditions flags = OptimizeConditions.None)
        {
            if (IsOptimized) throw new InvalidOperationException("Cannot optimize whilst already optimized, please Restore first.");
            IsOptimized = true;

            // Lock so that the optimizer cannot be ran twice at the same time.
            Monitor.Enter(_optimizeLockObject);

            _flagsUsedForOptimize = flags;

            #region Flag Checks
            if (!flags.HasFlag(OptimizeConditions.BoostPriorities) && flags.HasFlag(OptimizeConditions.IgnoreOrdinaryProcesses))
                _outputProvider?.OutputError($"The given flags ({flags}) stop the Optimize method from actually doing any optimization, " +
                    "in its current state, flags is saying to not boost priorities and to ignore non-priorities.");

            if (flags.HasFlag(OptimizeConditions.IgnoreOrdinaryProcesses) && flags.HasFlag(OptimizeConditions.OptimizeAffinity))
                _outputProvider?.OutputError($"Flag conflict! {OptimizeConditions.OptimizeAffinity} is overridden by {OptimizeConditions.IgnoreOrdinaryProcesses}.");

            if (flags.HasFlag(OptimizeConditions.StreamerMode) && flags.HasFlag(OptimizeConditions.OptimizeAffinity))
                _outputProvider?.OutputError($"Flag conflict! {OptimizeConditions.StreamerMode} overrides {OptimizeConditions.OptimizeAffinity}.");
            #endregion

            // Clear the restore_state file
            _restoreStateStorage?.Edit().Clear(true);

            RefreshProcessIdentifiers();

            IProcess[] currentProcesses = _activeProcessProvider.GetProcesses();

            // Sort the array alphabetically.
            Array.Sort(currentProcesses, new ProcessSorter());

            int optimizationsRan = 0;
            foreach (IProcess process in currentProcesses)
            {
                if (process.ProcessName == "explorer" && flags.HasFlag(OptimizeConditions.KillExplorerExe))
                {
                    process.Kill();
                    optimizationsRan++;
                    continue;
                }

                bool isGame = _gameProcessIdentifier.IsGame(process);

                // Games are considers whitelisted by default.
                bool isWhitelisted = isGame || _whitelistedProcessIdentifier.IsWhitelisted(process);

                if (isWhitelisted && flags.HasFlag(OptimizeConditions.BoostPriorities))
                {
                    // Set the priority to AboveNormal, if successful increment the
                    // optimizationsRan by one.
                    if (ChangePriority(process, ProcessPriorityClass.AboveNormal))
                    {
                        optimizationsRan++;
                        _outputProvider?.OutputHighlight("Prioritized '" + process.ProcessName + "' because it is a whitelisted process.");
                    }
                }

                if (flags.HasFlag(OptimizeConditions.StreamerMode) &&
                    process.ProcessName != "svchost" &&
                    _config != null &&
                    _config.LimitStreamerSpecificExecutablesAffinity != null &&
                    _config.LimitStreamerSpecificExecutablesAffinity.Length != 0 &&
                    _config.StreamerSpecificExecutables != null &&
                    _config.StreamerSpecificExecutables.Length != 0)
                {
                    nint newAffinity;

                    // If the process is a "stream specific executable" or isn't a whitelisted process, then force it onto specific cores.
                    if (_config.StreamerSpecificExecutables.AsSpan().Contains(process.ProcessName) ||
                        isWhitelisted == false ||
                        isGame == false)
                    {
                        newAffinity = GetAsAffinityMask(_config.LimitStreamerSpecificExecutablesAffinity, Environment.ProcessorCount);
                    }
                    // If the process is not a "stream specific executable" or is whitelisted, force it onto the cores the streaming software isn't using.
                    else
                    {
                        newAffinity = GetAsAffinityMask(_config.LimitStreamerSpecificExecutablesAffinity, Environment.ProcessorCount, invertMask: true);
                    }

                    if (ChangeAffinity(process, (ProcessAffinities)newAffinity))
                    {
                        optimizationsRan++;
                    }
                }

                if (isWhitelisted)
                {
                    _outputProvider?.OutputHighlight("Did not de-prioritize: '" + process.ProcessName + "' because it is a whitelisted process.");
                }
                else
                {
                    if (process.ProcessName != "svchost" && !flags.HasFlag(OptimizeConditions.IgnoreOrdinaryProcesses))
                    {
                        // Set process to idle priority, if successful increment the
                        // optimizationsRan by one.
                        if (ChangePriority(process, ProcessPriorityClass.Idle))
                            optimizationsRan++;
                    }
                }

                if (process.ProcessName != "svchost" &&
                    flags.HasFlag(OptimizeConditions.OptimizeAffinity))
                {
                    nint newAffinity;
                    if (isGame)
                    {
                        // If this is a game process then put it on the priority cores
                        newAffinity = GetOptimimumAffinityMask(returnPriorityCores: true);
                    }
                    else
                    {
                        // Because this is not a game, put the process on the non-priority cores.
                        newAffinity = GetOptimimumAffinityMask(returnPriorityCores: false);
                    }

                    // Change the affinity, if successful increment the optimizationsRan by one.
                    if (ChangeAffinity(process, (ProcessAffinities)newAffinity))
                        optimizationsRan++;
                }
            }

            _restoreStateStorage?.Edit().Commit();

            // Release the lock to allow this method to run again.
            Monitor.Exit(_optimizeLockObject);

            return optimizationsRan;
        }

        /// <summary>
        /// Calls refresh on the <see cref="_whitelistedProcessIdentifier"/> and <see cref="_gameProcessIdentifier"/>.
        /// </summary>
        /// <remarks>If both the identifiers are the same object, <c>Refresh</c> is only called once.</remarks>
        private void RefreshProcessIdentifiers()
        {
            if (_whitelistedProcessIdentifier.Equals(_gameProcessIdentifier))
            {
                _whitelistedProcessIdentifier.Refresh();
            }
            else
            {
                // Refresh the identifiers seperately as they are not the same object.
                _whitelistedProcessIdentifier.Refresh();
                _gameProcessIdentifier.Refresh();
            }
        }

        /// <summary>
        /// Restores all changes made to active processes by the `Optimize` method, this includes their Priority and Affinity.
        /// </summary>
        /// <returns>The number of restore operations completed.</returns>
        internal int Restore()
        {
            if (_flagsUsedForOptimize.HasFlag(OptimizeConditions.KillExplorerExe))
                Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe");

            if (_restoreStateStorage == null || _restoreStateStorage.Strings.Count == 0)
            {
                _outputProvider?.OutputError("No changes to restore.\n");
                return 0;
            }

            int restoreOperationsCompleted = 0;

            foreach (string changeString in _restoreStateStorage.Strings.Values)
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

            _restoreStateStorage.Edit().Clear(true).Commit();

            IsOptimized = false;

            return restoreOperationsCompleted;
        }

        /// <summary>
        /// Forces all active processes to <see cref="ProcessPriorityClass.Normal"/> and All Core affinity.
        /// </summary>
        /// <returns>The number of processes affected by the force restore.</returns>
        internal int ForceRestoreToNormal()
        {
            throw new NotImplementedException();
            //IProcess[] processes = Process.GetProcesses();

            //int affectedProcesses = 0;

            //foreach (Process process in processes)
            //{
            //    if (process.ProcessName != "svchost")
            //    {
            //        if (ChangePriority(process, ProcessPriorityClass.Normal)
            //            || ChangeAffinity(process, (ProcessAffinities)_affinityAllCores))
            //            affectedProcesses++;
            //    }
            //}

            //_restoreStateStorage.Edit().Clear(true).Commit();

            //IsOptimized = false;

            //return affectedProcesses;
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
        /// Converts a Span of ints (representing enabled cores) and converts it to a valid affinity mask
        /// which can be applied to a process.
        /// </summary>
        /// <param name="arrayOfCoresEnabled"></param>
        /// <param name="availableCores">The number of cores available to the runtime.</param>
        /// <param name="invertMask">Whether <c><paramref name="arrayOfCoresEnabled"/></c> should be treated as '<c>arrayOfCoresDisabled</c>'.</param>
        /// <remarks>Throws <see cref="ArgumentException"/> if any of the numbers in the <paramref name="arrayOfCoresEnabled"/> are
        /// wider than the width of a native integer on the executing machine.</remarks>
        /// <exception cref="ArgumentException"></exception>
        nint GetAsAffinityMask(Span<int> arrayOfCoresEnabled, int availableCores, bool invertMask = false)
        {
            nint value = invertMask ? (nint)Convert.ToInt64("".PadRight(availableCores, '1'), 2)
                                    : 0;

            for (int i = 0; i < arrayOfCoresEnabled.Length; i++)
            {
                if (arrayOfCoresEnabled[i] >= availableCores) { throw new ArgumentException($"{nameof(GetAsAffinityMask)}: The mask is {availableCores} wide, the number '{arrayOfCoresEnabled[i]}' is out of range."); }

                if (invertMask)
                {
                    value = BitMask.UnsetBit(value, arrayOfCoresEnabled[i]);
                }
                else
                {
                    value = BitMask.SetBit(value, arrayOfCoresEnabled[i]);
                }
            }

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnPriorityCores">Determines whether we should be giving you a mask for the priority cores, or for the non-priority cores.</param>
        /// <returns></returns>
        private nint GetOptimimumAffinityMask(bool returnPriorityCores = true)
        {
            var procLayout = ProcessorLayoutInformation.GetCurrentProcessorLayout();

            nint affinity = returnPriorityCores ? 0
                                                : (nint)Convert.ToInt64("".PadRight(procLayout.PhysicalCores, '1'), 2);

            // If this processor doesn't have core complexes OR has only one core complex OR the performance preference is set to "Speed".
            // We use a best guess on what cores a game process should be put on,
            // e.g: For 4 cpus, put games on the first 3 cores. For 6 cpus, put games on the first 4 cores.
            if (!procLayout.HasCoreComplexes ||
                 procLayout.NumberOfCoreComplexes == 1 ||
                 PerformancePreference == PerformancePreference.Speed)
            {
                // This branch optimizes by giving the most processing power to the game,
                // at the potential sacrifice of latency on Zen based processors (due to CCX interconnect latency)

                return procLayout.PhysicalCores switch
                {
                    2 => BitMask.ModifyBitRange(affinity, 0, 1, returnPriorityCores),
                    4 => BitMask.ModifyBitRange(affinity, 0, 3, returnPriorityCores),
                    6 => BitMask.ModifyBitRange(affinity, 0, 4, returnPriorityCores),
                    8 => BitMask.ModifyBitRange(affinity, 0, 6, returnPriorityCores),
                    12 => BitMask.ModifyBitRange(affinity, 0, 10, returnPriorityCores),
                    16 => BitMask.ModifyBitRange(affinity, 0, 12, returnPriorityCores),
                    24 => BitMask.ModifyBitRange(affinity, 0, 20, returnPriorityCores),
                    28 => BitMask.ModifyBitRange(affinity, 0, 20, returnPriorityCores),
                    32 => BitMask.ModifyBitRange(affinity, 0, 24, returnPriorityCores),
                    _ => BitMask.ModifyBitRange(affinity, 0, procLayout.PhysicalCores, returnPriorityCores) // If we don't have a preset then just show all cores as priority/non-priority.
                };
            }
            else
            {
                // This branch optimizes for latency, we isolate the game onto one CCX, this means
                // there is no interconnect latency penalty on Zen processors.
                // This may impact overall performance as depending on the CPU, there may be cores left idling because they sit outside of the first CCX
                // and the game can't touch them.

                return BitMask.ModifyBitRange(affinity, 0, procLayout.CoresPerCoreComplex, returnPriorityCores);
            }
        }

        /// <summary>
        /// Changes the given <paramref name="process"/> affinity to <paramref name="newAffinity"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the affinity was sucessfully changed, if it failed, returns <see langword="false"/>.</returns>
        bool ChangeAffinity(IProcess process, ProcessAffinities newAffinity)
        {
            try
            {
                // Store the pre-change here so that if we crash on the next line it will not be added to _changedProcesses.
                IntPtr preChangeAffinity = process.ProcessorAffinity;

                process.ProcessorAffinity = (IntPtr)newAffinity;

                // New affinity assignment was sucessful so log the change.
                var processStateChange = new ProcessStateChange(process, null, preChangeAffinity);
                _restoreStateStorage?.Edit().PutValue(DateTime.Now.Ticks + processStateChange.GetHashCode().ToString(), processStateChange.ToString());

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
        bool ChangePriority(IProcess p, ProcessPriorityClass newPriority, bool highlight = false)
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
                _restoreStateStorage?.Edit().PutValue(DateTime.Now.Ticks + processStateChange.GetHashCode().ToString(), processStateChange.ToString());
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