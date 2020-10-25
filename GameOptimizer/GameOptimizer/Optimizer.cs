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

    public class Optimizer
    {
        readonly IReadOnlyList<string> PriorityProcessNames;

        private OptimizeFlags FlagsUsedForOptimize = OptimizeFlags.None;

        /// <summary>
        /// <b>true</b> if optimization has been run. Returns to <b>false</b> when <see cref="Restore"/> is ran.
        /// </summary>
        public bool IsOptimized { get; private set; } = false;
        /// <summary>
        /// Whether the optimizer should display errors when it encounters them.
        /// </summary>
        public bool ShowErrorCodes { get; set; } = false;

        public Optimizer(IReadOnlyList<string> priorityProcessNames)
        {
            PriorityProcessNames = priorityProcessNames;
        }

        public void Optimize(OptimizeFlags flags = OptimizeFlags.None)
        {
            if (IsOptimized) throw new InvalidOperationException("Cannot optimize whilst already optimized, please Restore first.");
            IsOptimized = true;

            FlagsUsedForOptimize = flags;

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

                foreach (string priority in PriorityProcessNames)
                {
                    if (process.ProcessName.ToLower() == priority.ToLower())
                    {
                        Console.Write("  ");
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;

                        if (flags.HasFlag(OptimizeFlags.BoostPriorities))
                        {
                            ChangePriority(process, ProcessPriorityClass.AboveNormal);
                            Console.WriteLine("Prioritized '" + process.ProcessName + "' because it is listed as a priority process.");
                        }
                        else
                        {
                            Console.WriteLine("Ignored: '" + process.ProcessName + "' because it is listed as a priority process.");
                        }

                        Program.ResetColours();

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
            if (FlagsUsedForOptimize.HasFlag(OptimizeFlags.KillExplorerExe))
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

                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"'{p.ProcessName}' could not be accessed due to Error Code {e.NativeErrorCode} ({e.Message}).");

                Program.ResetColours();
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

                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"'{p.ProcessName}' remains Priority.{p.PriorityClass} due to Error Code {e.NativeErrorCode} ({e.Message}).");

                Program.ResetColours();
                return;
            }

            // Print changes
            if (highlight)
            {
                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine(p.ProcessName + " : " + original + " -> " + priority);

                Program.ResetColours();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("  " + p.ProcessName + " : ");
                Console.Write(original);
                Program.ResetColours();
                Console.Write(" -> ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(priority);

                Program.ResetColours();
            }
        }

    }
}