using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ZintomShellHelper;
using System.Windows.Forms;

namespace GameOptimizer
{
    [Flags] enum OptimizeFlags
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
        /// The optimizer should not touch 'non-priority' proccesses.
        /// </summary>
        IgnoreOrdinaryProcesses = 8
    }

    class Program
    {

        static string PriorityProcessesFile = "priority_processes.txt";

        static ConsoleColor defaultBackColor;
        static ConsoleColor defaultForeColor;

        static List<string> PriorityProcesses = new List<string>();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_MINIMIZE = 6;

        const string AppName = "Zintom's Game Optimizer";

        static void Main(string[] args)
        {
            Console.Title = AppName;
            defaultBackColor = Console.BackgroundColor;
            defaultForeColor = Console.ForegroundColor;

            LoadPriorityProcesses();

            while (true)
            {
                string command = "";
                MenuManager.DrawTitle(AppName, "  For commands, goto help.", true);
                int menuResult = MenuManager.CreateMenu(new string[] { "Quick Options", "Command", "Restore", "Help", "Exit" }, false, 2);
                switch (menuResult)
                {
                    case 0:
                        MenuManager.DrawTitle(AppName, "  Select a quick option to execute", true);
                        string[] quickCommands = new string[] { "1: Default Optimization",
                            "2: Boost Optimization",
                            "3: Just boost priority processes without touching other processes",
                            "Back"};
                        int quickResult = MenuManager.CreateMenu(quickCommands, false, 2);
                        if (quickResult == MenuManager.ESCAPE_KEY || quickResult == 3) // If escape or back pressed.
                            continue;

                        command = quickResult.ToString();

                        Console.Clear();
                        break;
                    case 1:
                        MenuManager.DrawTitle("Zintom's Game Optimizer", "  Enter command to execute:", true);
                        MenuManager.Reset();

                        Console.Write("  >");
                        command = Console.ReadLine();
                        if (command == "")
                            continue;

                        Console.Clear();
                        break;
                    case 2:
                        command = "res";
                        break;
                    case 3:
                        command = "help";
                        break;
                    case 4:
                        command = "exit";
                        break;
                }

                Console.ForegroundColor = ConsoleColor.White;

                if (command == "")
                    command = Console.ReadLine().ToLower();

                if (command == "opt")
                {
                    Console.WriteLine("Optimizing in 5 seconds...");
                    Thread.Sleep(5000);
                    Optimize();
                }
                else if (command.StartsWith("opt "))
                {
                    Console.WriteLine("Optimizing in 5 seconds...");
                    Thread.Sleep(5000);
                    Optimize(GetFlags(command.Substring(4)));
                }
                else if (command == "res")
                {
                    Restore();
                }
                else if (command == "audio")
                {
                    Process.Start("sndvol.exe", "-f " + SystemInformation.VirtualScreen.Width.ToString());
                }
                else if (command == "audio mixer")
                {
                    Process.Start("sndvol.exe", "-m " + SystemInformation.VirtualScreen.Width.ToString());
                }
                else if (command == "edit")
                {
                    Process.Start("notepad.exe", "priority_processes.txt");
                }
                else if (command == "help")
                {
                    MenuManager.DrawTitle(AppName, "  Help", "  opt         | Optimizes games by isolating cores and adjusting low priorities.\n  res         | Restores all processes back to normal.\n  edit        | Allows you to edit the priorty process list.\n  audio       | Launches SndVol.exe -f allowing you to change the computers master volume.\n  audio mixer | Launches SndVol.exe -m opening the volume mixer.", true);
                    MenuManager.CreateBackMenu(2);
                }
                else if (command == "exit")
                {
                    Environment.Exit(0);
                }
                else if (command == "0")
                {
                    MenuManager.DrawTitle(AppName, "  Optimizing in 5 seconds...", true);
                    Thread.Sleep(5000);
                    Optimize();
                }
                else if (command == "1")
                {
                    MenuManager.DrawTitle(AppName, "  Boost optimizing in 5 seconds...", true);
                    Thread.Sleep(5000);
                    Optimize(OptimizeFlags.BoostPriorities);
                }
                else if (command == "2")
                {
                    MenuManager.DrawTitle(AppName, "  Boosting apps to high priority in 5 seconds...", true);
                    Thread.Sleep(5000);
                    Optimize(OptimizeFlags.BoostPriorities | OptimizeFlags.IgnoreOrdinaryProcesses);
                }
            }
        }

        static void Optimize(OptimizeFlags flags = OptimizeFlags.None)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  Optimizing (Flags: {flags})...");

            Process[] currentProcesses = Process.GetProcesses();

            foreach (Process process in currentProcesses)
            {
                if (process.ProcessName == "explorer" && flags.HasFlag(OptimizeFlags.KillExplorerExe))
                {
                    process.Kill();
                    continue;
                }

                foreach (string priority in PriorityProcesses)
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

                        ResetColors();
                    }
                    else if (process.ProcessName != "svchost" && !flags.HasFlag(OptimizeFlags.IgnoreOrdinaryProcesses))
                    {
                        try
                        {
                            // Set process to idle priority.
                            ChangePriority(process, ProcessPriorityClass.Idle);

                            //LimitToOneCore(process);
                        }
                        catch { }
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Optimized.");

            Thread.Sleep(1500);

            if (!flags.HasFlag(OptimizeFlags.NoHide))
                ShowWindow(GetConsoleWindow(), SW_MINIMIZE);

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

        static void Restore()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            MenuManager.DrawTitle(AppName, "  Restoring", true);
            //Console.WriteLine("Restoring...");

            Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe");

            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                if (process.ProcessName != "svchost")
                {
                    try
                    {
                        ChangePriority(process, ProcessPriorityClass.Normal);// process.PriorityClass = ProcessPriorityClass.Normal;
                    }
                    catch { }
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Restored to normal priority.");

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

        static void ChangePriority(Process p, ProcessPriorityClass priority, bool highlight = false)
        {
            // Original value
            string original = p.PriorityClass.ToString();

            // Set new value
            p.PriorityClass = priority;

            // Print changes
            if (highlight)
            {
                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine(p.ProcessName + " : " + original + " -> " + priority);

                ResetColors();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("  " + p.ProcessName + " : ");
                //Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(original);
                ResetColors();
                Console.Write(" -> ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(priority);

                ResetColors();
            }
        }

        static void ResetColors()
        {
            Console.BackgroundColor = defaultBackColor;
            Console.ForegroundColor = defaultForeColor;
        }

        [Obsolete]
        static void LimitToOneCore(Process process)
        {
            return; // Doesn't really work on decent systems.
            //// Limit process to one core
            //long AffinityMask = (long)process.ProcessorAffinity;
            //AffinityMask = 0x0001; // Use only first core
            //process.ProcessorAffinity = (IntPtr)AffinityMask;
        }

        static void CreateDefaultPriorityProcessFile()
        {
            if (!File.Exists(PriorityProcessesFile))
            {
                using (var stream = File.Create(PriorityProcessesFile))
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    sw.WriteLine("##Put processes you don't want affected by the optimizer here.\n##Defaults\nSteam\nSteamService\nsteamwebhelper\nGameOverlayUI\n##Others");
                    sw.Flush();
                    sw.Close();
                }

                Console.WriteLine("Generated " + PriorityProcessesFile + " as it did not exist.");
            }
        }

        static void LoadPriorityProcesses()
        {
            PriorityProcesses.Clear();

            CreateDefaultPriorityProcessFile();

            string[] lines = File.ReadAllLines(PriorityProcessesFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("##")) continue;

                PriorityProcesses.Add(lines[i]);
            }

            Console.WriteLine(PriorityProcessesFile + " loaded.");
        }

        static OptimizeFlags GetFlags(string input)
        {
            if (string.IsNullOrEmpty(input)) return OptimizeFlags.None;

            OptimizeFlags output = OptimizeFlags.None;

            string[] rawFlags = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var flag in rawFlags)
            {
                if (flag.ToLower() == "-" + OptimizeFlags.KillExplorerExe.ToString().ToLower())
                    output.SetFlag(OptimizeFlags.KillExplorerExe);

                else if (flag.ToLower() == "-" + OptimizeFlags.NoHide.ToString().ToLower())
                    output.SetFlag(OptimizeFlags.NoHide);

                else if (flag.ToLower() == "-" + OptimizeFlags.BoostPriorities.ToString().ToLower())
                    output.SetFlag(OptimizeFlags.BoostPriorities);

                else if (flag.ToLower() == "-" + OptimizeFlags.IgnoreOrdinaryProcesses.ToString().ToLower())
                    output.SetFlag(OptimizeFlags.IgnoreOrdinaryProcesses);
            }

            return output;
        }

        //static List<CommandFlag> GetFlags(string input)
        //{
        //    if (!input.Contains(" "))
        //        return null;

        //    try
        //    {
        //        List<CommandFlag> flags = new List<CommandFlag>();
        //        List<string> arr = input.Split(' ').ToList<string>();
        //        arr.RemoveAt(0);
        //        for (int i = 0; i < arr.Count; i++)
        //        {
        //            flags.Add(new CommandFlag(arr[i], arr[i + 1]));
        //            i++;
        //        }

        //        return flags;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    }
}