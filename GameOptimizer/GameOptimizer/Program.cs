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
    class Program
    {
        static ConsoleColor defaultBackColor;
        static ConsoleColor defaultForeColor;

        static List<string> PriorityProcesses = new List<string>();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static string title = "Zintom's Game Optimizer";

        static void Main(string[] args)
        {
            Console.Title = "Zintom's Game Optimizer";
            defaultBackColor = Console.BackgroundColor;
            defaultForeColor = Console.ForegroundColor;
            //Console.WriteLine("Zintom's Game Optimizer - Type 'help' for commands.\n1: opt -nokillexplorer t -ignoresteam t\n2: boost-opt -nokillexplorer t -ignoresteam t\n3: Just boost priority processes to High Priority.");

            while (true)
            {
                string command = "";
                MenuManager.DrawTitle(title, "  For commands, goto help.", true);
                int menuResult = MenuManager.CreateMenu(new string[] { "Quick Options", "Command", "Restore", "Help", "Exit" }, false, 2);
                switch (menuResult)
                {
                    case 0:
                        MenuManager.DrawTitle(title, "  Select a quick option to execute", true);
                        string[] quickCommands = new string[] { "1: opt -nokillexplorer t",
                            "2: boost-opt -nokillexplorer t",
                            "3: Just boost priority processes to High Priority.",
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

                var flags = GetFlags(command);

                if (command == "opt" || command.StartsWith("opt "))
                {
                    Console.WriteLine("Optimizing in 5 seconds...");
                    Thread.Sleep(5000);
                    Optimize(flags, false);
                }
                else if (command == "boost-opt" || command.StartsWith("boost-opt "))
                {
                    Console.WriteLine("Optimizing in 5 seconds...");
                    Thread.Sleep(5000);
                    Optimize(flags, true);
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
                    MenuManager.DrawTitle(title, "  Help", "  opt         | Optimizes games by isolating cores and adjusting low priorities.\n  boost-opt   | Optimizes games like before but boosts the game to 'Above Normal' priority.\n  res         | Restores all processes back to normal.\n  edit        | Allows you to edit the priorty process list.\n  audio       | Launches SndVol.exe -f allowing you to change the computers master volume.\n  audio mixer | Launches SndVol.exe -m opening the volume mixer.", true);
                    MenuManager.CreateBackMenu(2);
                    //Console.WriteLine("  opt         | Optimizes games by isolating cores and adjusting low priorities.\n  boost-opt   | Optimizes games like before but boosts the game to 'Above Normal' priority.\n  res         | Restores all processes back to normal.\n  edit        | Allows you to edit the priorty process list.\n  audio       | Launches SndVol.exe -f allowing you to change the computers master volume.\n  audio mixer | Launches SndVol.exe -m opening the volume mixer.");
                }
                else if (command == "exit")
                {
                    Environment.Exit(0);
                }
                else if (command == "0")
                {
                    flags = GetFlags("opt -nokillexplorer t -ignoresteam t");

                    MenuManager.DrawTitle(title, "  Optimizing in 5 seconds...", true);
                    //Console.WriteLine("Optimizing in 5 seconds...");
                    Thread.Sleep(5000);
                    Optimize(flags, false);
                }
                else if (command == "1")
                {
                    flags = GetFlags("boost-opt -nokillexplorer t -ignoresteam t");

                    MenuManager.DrawTitle(title, "  Boost optimizing in 5 seconds...", true);
                    //Console.WriteLine("Boost optimizing in 5 seconds...");
                    Thread.Sleep(5000);
                    Optimize(flags, true);
                }
                else if (command == "2")
                {
                    MenuManager.DrawTitle(title, "  Boosting apps to high priority in 5 seconds...", true);
                    //Console.WriteLine("Boosting to high priority in 5 seconds...");
                    Thread.Sleep(5000);
                    JustBoostGame();
                }
            }
        }

        static void JustBoostGame()
        {
            GetPriorityProcesses();

            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                foreach (string priority in PriorityProcesses)
                {
                    if (process.ProcessName == priority)
                    {
                        ChangePriority(process, ProcessPriorityClass.High, true);
                        //string original = process.PriorityClass.ToString();
                        //process.PriorityClass = ProcessPriorityClass.High;
                        //printChange(process.ProcessName, original, process.PriorityClass.ToString());

                        //Console.WriteLine("  " + process.ProcessName + " : " + original + " -> " + process.PriorityClass.ToString());
                        //Console.WriteLine("Boosted '" + process.ProcessName + "' because it is a priority.");
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Optimized.");

            Thread.Sleep(1500);

            ShowWindow(GetConsoleWindow(), 6); // SW_MINIMIZE = 6

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

        static void Optimize(List<CommandFlag> flags, bool boostPriority)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  Optimizing...");

            #region Flag Stuff
            bool flag_nokillexplorer = false;
            bool flag_nohide = false;
            //bool flag_ignoresteam = false;

            if (flags != null)
            {
                foreach (CommandFlag s in flags)
                {
                    if (s.Flag == "-nokillexplorer" && s.Value == "t")
                    {
                        flag_nokillexplorer = true;
                        Console.WriteLine("  Flag used: -nokillexplorer");
                    }
                    else if (s.Flag == "-nohide" && s.Value == "t")
                    {
                        flag_nohide = true;
                        Console.WriteLine("  Flag used: -nohide");
                    }
                    //else if (s.Flag == "-ignoresteam" && s.Value == "t")
                    //{
                    //    flag_ignoresteam = true;
                    //    Console.WriteLine("  Flag used: -ignoresteam");
                    //}
                }
            }
            #endregion

            GetPriorityProcesses();

            // Kill windows explorer.
            if (!flag_nokillexplorer)
            {
                Process[] p = Process.GetProcesses();

                foreach (Process process in p)
                {
                    if (process.ProcessName == "explorer")
                    {
                        process.Kill();
                        break;
                    }
                }
            }

            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                bool IsPriority = false;
                foreach (string priority in PriorityProcesses)
                {
                    if (process.ProcessName.ToLower() == priority.ToLower())
                    {
                        IsPriority = true;

                        if (boostPriority)
                            ChangePriority(process, ProcessPriorityClass.AboveNormal);// process.PriorityClass = ProcessPriorityClass.AboveNormal;
                    }
                }

                if (IsPriority == false) // If process isn't a priority process
                {
                    //if ((process.ProcessName.ToLower().Contains("steam") || process.ProcessName == "GameOverlayUI") && flag_ignoresteam)
                    //{
                    //    try
                    //    {
                    //        if (boostPriority)
                    //        {
                    //            ChangePriority(process, ProcessPriorityClass.AboveNormal, true);// process.PriorityClass = ProcessPriorityClass.AboveNormal;
                    //            //Console.WriteLine("Saw steam, boosted to AboveNormal to match Boost Priority.");
                    //        }
                    //        else
                    //        {
                    //            Console.WriteLine("Ignored STEAM.");
                    //        }
                    //    }
                    //    catch { }
                    //}
                    //else 
                    if (process.ProcessName.ToLower() == "discord")
                    {
                        try
                        {
                            ChangePriority(process, ProcessPriorityClass.BelowNormal);// process.PriorityClass = ProcessPriorityClass.BelowNormal; // Set discord to below normal
                            //LimitToOneCore(process);
                        }
                        catch { }
                    }
                    else if (process.ProcessName != "svchost")
                    {
                        try
                        {
                            // Set process to idle priority.
                            ChangePriority(process, ProcessPriorityClass.Idle);// process.PriorityClass = ProcessPriorityClass.Idle;
                            //LimitToOneCore(process);
                        }
                        catch { }
                    }
                }
                else
                {
                    Console.Write("  ");
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine("Ignored: '" + process.ProcessName + "' because it is a priority.");
                    ResetColors();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Optimized.");

            Thread.Sleep(1500);

            if (!flag_nohide)
                ShowWindow(GetConsoleWindow(), 6); // SW_MINIMIZE = 6

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

        static void Restore()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            MenuManager.DrawTitle(title, "  Restoring", true);
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

        static void LimitToOneCore(Process process)
        {
            return; // Doesn't really work on decent systems.
            //// Limit process to one core
            //long AffinityMask = (long)process.ProcessorAffinity;
            //AffinityMask = 0x0001; // Use only first core
            //process.ProcessorAffinity = (IntPtr)AffinityMask;
        }

        static void GetPriorityProcesses()
        {
            PriorityProcesses.Clear();

            string filename = "priority_processes.txt";
            if (!File.Exists(filename))
            {
                File.Create(filename).Close();
                using (StreamWriter sw = new StreamWriter(filename))
                {
                    sw.WriteLine("##Put processes you don't want affected by the optimizer here.\n##Defaults\nSteam\nSteamService\nsteamwebhelper\nGameOverlayUI\n##Others");
                    sw.Close();
                }
            }

            string[] lines_array = File.ReadAllLines(filename);

            List<string> lines = new List<string>(lines_array);

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (line.Length >= 1)
                {
                    if (line == "##") { lines.Remove(line); i--; }
                    if (line.Substring(0, 2) == "##") { lines.Remove(line); i--; }
                }
                else if (line.Length == 0)
                {
                    lines.Remove(line);
                    i--;
                }
            }

            foreach (string line in lines)
            {
                PriorityProcesses.Add(line);
            }
        }

        static List<CommandFlag> GetFlags(string input)
        {
            if (!input.Contains(" "))
                return null;

            try
            {
                List<CommandFlag> flags = new List<CommandFlag>();
                List<string> arr = input.Split(' ').ToList<string>();
                arr.RemoveAt(0);
                for (int i = 0; i < arr.Count; i++)
                {
                    flags.Add(new CommandFlag(arr[i], arr[i + 1]));
                    i++;
                }

                return flags;
            }
            catch
            {
                return null;
            }
        }
    }
}