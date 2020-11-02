using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Zintom.GameOptimizer.Helpers;
using ZintomShellHelper;

namespace Zintom.GameOptimizer
{

    partial class Program
    {

        const string AppName = "Zintom's Game Optimizer";
        const string PriorityProcessesFile = "priority_processes.txt";
        const string FileComment = "##";

        static ConsoleColor defaultBackColor;
        static ConsoleColor defaultForeColor;

        static int _optimizeWaitTimeMillis = 5000;

        static Optimizer optimizer;

        static void Main(string[] args)
        {
            Console.Title = AppName;
            defaultBackColor = Console.BackgroundColor;
            defaultForeColor = Console.ForegroundColor;

            optimizer = new Optimizer(GetPriorityProcessesNames(), new CLIOutputDisplayer());

            while (true)
            {
                string command = "";
                MenuManager.DrawTitle(AppName, optimizer.IsOptimized ? "  Currently optimized, use 'Restore' or the command 'res' to de-optimize." : "  For commands, goto help.", true);
                int menuResult = MenuManager.CreateMenu(new string[] { "Quick Options", "Command", "Restore", "Help", "Exit" }, false, 2);
                switch (menuResult)
                {
                    case 0:
                        MenuManager.DrawTitle(AppName, "  Select a quick option to execute", true);
                        string[] quickCommands = new string[] { "1: Default optimization",
                            "2: Default optimization plus Affinity optimization",
                            "3: Boost priorities and de-prioritise everything else.",
                            "4: Just boost priority processes without touching other processes",
                            "Back"};
                        int quickResult = MenuManager.CreateMenu(quickCommands, false, 2);
                        if (quickResult == MenuManager.ESCAPE_KEY || quickResult == 4) // If escape or back pressed.
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
                    Command_OptimizeNoFlags();
                }
                else if (command.StartsWith("opt "))
                {
                    Command_OptimizeWithFlags(ParseFlags(command.Substring(4)));
                }
                else if (command == "res")
                {
                    Command_Restore();
                }
                else if (command == "force restore")
                {
                    Command_ForceRestore();
                }
                else if (command == "toggle_errors")
                {
                    optimizer.ShowErrorCodes = !optimizer.ShowErrorCodes;
                }
                else if (command == "audio")
                {
                    Process.Start("sndvol.exe", "-f " + SystemMetrics.GetVirtualDisplaySize().Width);
                }
                else if (command == "audio mixer")
                {
                    Process.Start("sndvol.exe", "-m " + SystemMetrics.GetVirtualDisplaySize().Width);
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
                    Command_OptimizeNoFlags();
                }
                else if (command == "1")
                {
                    Command_OptimizeWithFlags(OptimizeFlags.OptimizeAffinity);
                }
                else if (command == "2")
                {
                    Command_OptimizeWithFlags(OptimizeFlags.BoostPriorities);
                }
                else if (command == "3")
                {
                    Command_OptimizeWithFlags(OptimizeFlags.BoostPriorities | OptimizeFlags.IgnoreOrdinaryProcesses);
                }
            }
        }

        /// <summary>
        /// Reset the console colours back to their defaults.
        /// </summary>
        public static void ResetColours()
        {
            Console.BackgroundColor = defaultBackColor;
            Console.ForegroundColor = defaultForeColor;
        }

        /// <summary>
        /// Creates the default priority proccesses file if it does not exist.
        /// </summary>
        static void CreateDefaultPriorityProcessesFile()
        {
            if (!File.Exists(PriorityProcessesFile))
            {
                using (var stream = File.Create(PriorityProcessesFile))
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    sw.WriteLine(FileComment + "Put processes you don't want affected by the optimizer here.\n" + 
                        FileComment + "Defaults\nSteam\nSteamService\nsteamwebhelper\nGameOverlayUI\n" +
                        "NVDisplay.Container\nnvsphelper64.exe\nffmpeg-mux64\nobs64\n\n" + 
                        FileComment + "Others");
                    sw.Flush();
                    sw.Close();
                }

                Console.WriteLine("Generated " + PriorityProcessesFile + " as it did not exist.");
            }
        }

        /// <summary>
        /// Reads the <see cref="PriorityProcessesFile"/> and returns the priority process names from within.
        /// </summary>
        /// <returns>All the priority process names in a <see cref="string"/> array.</returns>
        static List<string> GetPriorityProcessesNames()
        {
            CreateDefaultPriorityProcessesFile();

            List<string> priorityProcessNames = new List<string>();

            string[] lines = File.ReadAllLines(PriorityProcessesFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(FileComment))
                    priorityProcessNames.Add(lines[i].Substring(0, lines[i].IndexOf(FileComment)).Trim());
                else
                    priorityProcessNames.Add(lines[i].Trim());
            }

            Console.WriteLine(PriorityProcessesFile + " loaded.");

            return priorityProcessNames;
        }

        /// <summary>
        /// Parses <see cref="OptimizeFlags"/> out of the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Format: "-switch1 -switch2"</param>
        /// <returns>An <see cref="OptimizeFlags"/> enum with the various bitfields enabled if any of the given flags match a valid <see cref="OptimizeFlags"/> value.</returns>
        static OptimizeFlags ParseFlags(string input)
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

        private class CLIOutputDisplayer : IOutputProvider
        {
            void IOutputProvider.Output(string message)
            {
                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);

                ResetColours();
            }

            void IOutputProvider.OutputError(string errorMessage)
            {
                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(errorMessage);

                ResetColours();
            }

            void IOutputProvider.OutputHighlight(string message)
            {
                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine(message);

                ResetColours();
            }
        }
    }
}