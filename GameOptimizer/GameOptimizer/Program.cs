using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using Zintom.GameOptimizer.Helpers;
using Zintom.StorageFacility;
using Zintom.InteractiveShell;

namespace Zintom.GameOptimizer
{

    partial class Program
    {

        private static string AppName = "Zintom's Game Optimizer";
        const string WhitelistFile = "process_whitelist.txt";
        const string FileComment = "##";

        const int _optimizeWaitTimeMillis = 5000;

        private static ConsoleColor _defaultBackColor;
        private static ConsoleColor _defaultForeColor;

        private static Optimizer optimizer = default!;
        private static Storage _settings = default!;

        private static InteractiveShell.InteractiveShell _interactiveShell = default!;
        private static ShellDisplayOptions _shellDisplayOptions = default!;
        private static ShellTitleDisplayOptions _shellTitleDisplayOptions = default!;

        private static void SetupInteractiveShell()
        {
            _interactiveShell = new InteractiveShell.InteractiveShell();

            _shellDisplayOptions = new ShellDisplayOptions()
            {
                LeftOffset = 4
            };

            _shellTitleDisplayOptions = new ShellTitleDisplayOptions()
            {
                LeftOffset = 2
            };
        }

        static void Main(string[] args)
        {
            AppName = "Zintom's Game Optimizer - " + GetVersionInformation();

            if(args.Length > 0 && args[0] == "-melody!")
            {
                AppName = "Zintom's Melody Fluffer!";
            }

            Console.Title = AppName;
            _defaultBackColor = Console.BackgroundColor;
            _defaultForeColor = Console.ForegroundColor;

            LoadSettings();
            SetupInteractiveShell();

            optimizer = new Optimizer(GetWhitelistedProcessNames(), new CLIOutputDisplayer());

            while (true)
            {
                string command = "";
                _interactiveShell.DrawTitle(AppName, optimizer.IsOptimized ? "  Currently optimized, use 'Restore' or the command 'res' to de-optimize.\n  Some menu options are unavailable because of this." : "  Main Menu", _shellTitleDisplayOptions, true);
                int menuResult = _interactiveShell.DisplayMenu(new string[] { optimizer.IsOptimized ? "Unavailable" : "Quick Options", 
                                                                       optimizer.IsOptimized ? "Unavailable" : "Command Input", 
                                                                       "Restore",
                                                                       "Help", 
                                                                       "Exit" }, _shellDisplayOptions);
                switch (menuResult)
                {
                    case 0:
                        if (optimizer.IsOptimized) continue;

                        _interactiveShell.DrawTitle(AppName, "  Select a quick option to execute", _shellTitleDisplayOptions, true);
                        string[] quickCommands = new string[] { "1: Default optimization",
                            "2: Default optimization plus Affinity optimization",
                            "3: Boost whitelisted processes and de-prioritise everything else.",
                            "4: Just boost whitelisted processes without touching other processes",
                            "Back"};
                        int quickResult = _interactiveShell.DisplayMenu(quickCommands, _shellDisplayOptions);
                        if (quickResult == -1 || quickResult == 4) // If escape or back pressed.
                            continue;

                        command = quickResult.ToString();

                        Console.Clear();
                        break;
                    case 1:
                        if (optimizer.IsOptimized) continue;

                        ShellTitleDisplayOptions titleDispOptions = new ShellTitleDisplayOptions()
                        {
                            LeftOffset = 2,
                            SubtitleVerticalPadding = 0
                        };

                        _interactiveShell.DrawTitle(AppName, "Enter command to execute:", titleDispOptions, true);
                        _interactiveShell.Reset();

                        Console.Write("  >");
                        command = Console.ReadLine() ?? "";
                        if (string.IsNullOrEmpty(command))
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

                if (command == "opt")
                {
                    Command_OptimizeNoFlags();
                }
                else if (command.StartsWith("opt "))
                {
                    Command_OptimizeWithFlags(ParseFlags(command[4..]));
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
                    Process.Start("sndvol.exe", "-f " + NativeMethods.GetVirtualDisplaySize().Width);
                }
                else if (command == "audio mixer")
                {
                    Process.Start("sndvol.exe", "-m " + NativeMethods.GetVirtualDisplaySize().Width);
                }
                else if (command == "edit")
                {
                    Process.Start("notepad.exe", WhitelistFile);
                }
                else if (command == "help")
                {
                    _interactiveShell.DrawTitle(AppName, 
                        "  Help", "  opt         | Optimizes games by isolating cores and adjusting low priorities." + 
                                "\n  res         | Restores all processes back to normal." + 
                                "\n  edit        | Allows you to edit the priorty process list." + 
                                "\n  audio       | Launches SndVol.exe -f allowing you to change the computers master volume." + 
                                "\n  audio mixer | Launches SndVol.exe -m opening the volume mixer." +
                                "\n", _shellTitleDisplayOptions, true);
                    int result = _interactiveShell.DisplayMenu(new string[] { "Open README.txt", "Open LICENSE.txt", "Back" }, _shellDisplayOptions);

                    if (result == 0) Process.Start("notepad.exe" , "README.txt");
                    if (result == 1) Process.Start("notepad.exe", "LICENSE.txt");
                    continue;
                }
                else if (command == "exit")
                {
                    Console.Clear();
                    Environment.Exit(0);
                }
                else if (command == "0")
                {
                    Command_OptimizeNoFlags();
                }
                else if (command == "1")
                {
                    Command_OptimizeWithFlags(OptimizeConditions.OptimizeAffinity);
                }
                else if (command == "2")
                {
                    Command_OptimizeWithFlags(OptimizeConditions.BoostPriorities);
                }
                else if (command == "3")
                {
                    Command_OptimizeWithFlags(OptimizeConditions.BoostPriorities | OptimizeConditions.IgnoreOrdinaryProcesses);
                }
            }
        }

        static void LoadSettings()
        {
            Console.WriteLine("Loading settings information..");

            _settings = Storage.GetStorage("settings.dat");
        }

        /// <summary>
        /// Gets the <see cref="Version"/> information for the currently executing <see cref="System.Reflection.Assembly"/>.
        /// </summary>
        /// <returns>A string of the <see cref="Version"/> in the format "<c>Major.Minor.Build</c>"</returns>
        static string GetVersionInformation()
        {
            Version? version = System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Version;
            if (version == null) return "Version information not available.";

            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        /// <summary>
        /// Reset the console colours back to their defaults.
        /// </summary>
        public static void ResetColours()
        {
            Console.BackgroundColor = _defaultBackColor;
            Console.ForegroundColor = _defaultForeColor;
        }

        /// <summary>
        /// Creates the default priority proccesses file if it does not exist.
        /// </summary>
        static void CreateDefaultProcessWhitelistFile()
        {
            if (!File.Exists(WhitelistFile))
            {
                using (var stream = File.Create(WhitelistFile))
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    sw.WriteLine(string.Format(
                        "{0} Put processes you don't want affected by the optimizer here.{1}" +
                        "{0} Apps{1}Steam{1}SteamService{1}steamwebhelper{1}GameOverlayUI{1}" +
                        "NVDisplay.Container{1}nvsphelper64.exe{1}ffmpeg-mux64 ## OBS's encoder{1}obs64 ## Open Broadcaster{1}discord{1}{1}" +
                        "{0} Games{1}javaw {1}Minecraft", FileComment, Environment.NewLine));
                    sw.Flush();
                    sw.Close();
                }

                Console.WriteLine("Generated " + WhitelistFile + " as it did not exist.");
            }
        }

        /// <summary>
        /// Reads the <see cref="WhitelistFile"/> and returns the priority process names from within.
        /// </summary>
        /// <returns>All the priority process names in a <see cref="string"/> array.</returns>
        static List<string> GetWhitelistedProcessNames()
        {
            CreateDefaultProcessWhitelistFile();

            List<string> whitelistedProcessNames = new List<string>();

            string[] lines = File.ReadAllLines(WhitelistFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(FileComment, StringComparison.InvariantCulture))
                    whitelistedProcessNames.Add(lines[i].Substring(0, lines[i].IndexOf(FileComment)).Trim());
                else
                    whitelistedProcessNames.Add(lines[i].Trim());
            }

            Console.WriteLine(WhitelistFile + " loaded.");

            return whitelistedProcessNames;
        }

        /// <summary>
        /// Parses <see cref="OptimizeConditions"/> out of the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Format: "-switch1 -switch2"</param>
        /// <returns>An <see cref="OptimizeConditions"/> enum with the various bitfields enabled if any of the given flags match a valid <see cref="OptimizeConditions"/> value.</returns>
        static OptimizeConditions ParseFlags(string input)
        {
            if (string.IsNullOrEmpty(input)) return OptimizeConditions.None;

            OptimizeConditions output = OptimizeConditions.None;

            string[] rawFlags = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var flag in rawFlags)
            {
                if (flag.ToLower() == "-" + OptimizeConditions.KillExplorerExe.ToString().ToLower())
                    output.SetFlag(OptimizeConditions.KillExplorerExe);

                else if (flag.ToLower() == "-" + OptimizeConditions.NoHide.ToString().ToLower())
                    output.SetFlag(OptimizeConditions.NoHide);

                else if (flag.ToLower() == "-" + OptimizeConditions.BoostPriorities.ToString().ToLower())
                    output.SetFlag(OptimizeConditions.BoostPriorities);

                else if (flag.ToLower() == "-" + OptimizeConditions.IgnoreOrdinaryProcesses.ToString().ToLower())
                    output.SetFlag(OptimizeConditions.IgnoreOrdinaryProcesses);
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