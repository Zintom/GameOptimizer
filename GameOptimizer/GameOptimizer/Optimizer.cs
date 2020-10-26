using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameOptimizer
{

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
        IgnoreOrdinaryProcesses = 8
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
        readonly IReadOnlyList<string> _priorityProcessNames;

        readonly IOutputProvider _outputProvider;

        private OptimizeFlags _flagsUsedForOptimize = OptimizeFlags.None;

        /// <summary>
        /// <b>true</b> if optimization has been run. Returns to <b>false</b> when <see cref="Restore"/> is ran.
        /// </summary>
        public bool IsOptimized { get; private set; } = false;
        /// <summary>
        /// Whether the optimizer should display errors when it encounters them.
        /// </summary>
        public bool ShowErrorCodes { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="priorityProcessNames"></param>
        /// <param name="outputProvider">If not <b>null</b>, the optimizer will use this to output messages/problems or errors.</param>
        public Optimizer(IReadOnlyList<string> priorityProcessNames, IOutputProvider outputProvider = null)
        {
            _priorityProcessNames = priorityProcessNames;
            _outputProvider = outputProvider;
        }

        public void Optimize(OptimizeFlags flags = OptimizeFlags.None)
        {
            if (IsOptimized) throw new InvalidOperationException("Cannot optimize whilst already optimized, please Restore first.");
            IsOptimized = true;

            _flagsUsedForOptimize = flags;

            if (!flags.HasFlag(OptimizeFlags.BoostPriorities) && flags.HasFlag(OptimizeFlags.IgnoreOrdinaryProcesses))
                throw new ArgumentException($"The given flags ({flags}) stop the Optimize method from actually doing any optimization, " +
                    "in its current state, flags is saying to not boost priorities and to ignore non-priorities.");

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
                }

            continueLoop:;
            }
        }

        public void Restore()
        {
            if (_flagsUsedForOptimize.HasFlag(OptimizeFlags.KillExplorerExe))
                Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe");

            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                if (process.ProcessName != "svchost")
                {
                    ChangePriority(process, ProcessPriorityClass.Normal);
                }
            }

            IsOptimized = false;
        }

        void ChangePriority(Process p, ProcessPriorityClass priority, bool highlight = false)
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

            // Set new value
            try
            {
                p.PriorityClass = priority;
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                if (!ShowErrorCodes) return;

                _outputProvider?.OutputError($"'{p.ProcessName}' remains Priority.{p.PriorityClass} due to Error Code {e.NativeErrorCode} ({e.Message}).");
                return;
            }

            // Print changes
            if (highlight)
                _outputProvider?.OutputHighlight(p.ProcessName + " : " + original + " -> " + priority);
            else
                _outputProvider?.Output(p.ProcessName + " : " + original + " -> " + priority);
        }

    }
}