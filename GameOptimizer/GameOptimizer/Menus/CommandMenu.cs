using System;
using System.Diagnostics;
using Zintom.GameOptimizer.Helpers;
using Zintom.InteractiveShell;

namespace Zintom.GameOptimizer.Menus
{
    /// <summary>
    /// The menu that allows users to enter commands manually.
    /// </summary>
    public class CommandMenu : IConsoleMenu
    {

        public void Run(InteractiveShell.InteractiveShell gui)
        {
            ShellTitleDisplayOptions titleDispOptions = new ShellTitleDisplayOptions()
            {
                LeftOffset = 2,
                SubtitleVerticalPadding = 0
            };

            gui.DrawTitle(Program.AppName, "Enter command to execute:", titleDispOptions, true);
            gui.Reset();

            Console.Write("  >");
            string? command = Console.ReadLine();

            if (string.IsNullOrEmpty(command))
                return;

            RunCommand(command, gui);
        }

        /// <summary>
        /// Runs the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to run.</param>
        public void RunCommand(string command, InteractiveShell.InteractiveShell gui)
        {
            if (command.StartsWith("opt "))
            {
                Program.Command_OptimizeWithFlags(ParseFlags(command[4..]));
                return;
            }

            switch (command)
            {
                case "opt":
                    Program.Command_OptimizeWithFlags(OptimizeConditions.None);
                    break;
                case "res":
                    Program.Command_Restore();
                    break;
                case "res -force":
                    Program.Command_ForceRestore();
                    break;
                case "toggle_errors":
                    Program.Optimizer.ShowErrorCodes = !Program.Optimizer.ShowErrorCodes;
                    break;
                case "audio":
                    Process.Start("sndvol.exe", "-f " + NativeMethods.GetVirtualDisplaySize().Width);
                    break;
                case "audio mixer":
                    Process.Start("sndvol.exe", "-m " + NativeMethods.GetVirtualDisplaySize().Width);
                    break;
                case "edit":
                    Process.Start("notepad.exe", WhitelistReader.WhitelistFile);
                    break;
                case "help":
                    IConsoleMenu helpMenu = new HelpMenu();
                    helpMenu.Run(gui);
                    break;
                case "exit":
                    Console.Clear();
                    Environment.Exit(0);
                    break;
            }
        }

        /// <summary>
        /// Parses <see cref="OptimizeConditions"/> from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Format: "-shortSwitch1 --longSwitch1"</param>
        /// <returns>An <see cref="OptimizeConditions"/> enum with the various bitfields enabled if any of the given flags match a valid <see cref="OptimizeConditions"/> value.</returns>
        public static OptimizeConditions ParseFlags(string input)
        {
            if (string.IsNullOrEmpty(input)) return OptimizeConditions.None;

            OptimizeConditions output = OptimizeConditions.None;

            string[] rawFlags = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var f in rawFlags)
            {
                string flag = f.ToLower();

                if (flag == "--" + OptimizeConditions.KillExplorerExe.ToString().ToLower() ||
                    flag == "-k")
                {
                    output.SetFlag(OptimizeConditions.KillExplorerExe);
                }
                else if (flag == "--" + OptimizeConditions.NoHide.ToString().ToLower() ||
                         flag == "-nh")
                {
                    output.SetFlag(OptimizeConditions.NoHide);
                }
                else if (flag == "--" + OptimizeConditions.BoostPriorities.ToString().ToLower() ||
                         flag == "-b")
                {
                    output.SetFlag(OptimizeConditions.BoostPriorities);
                }
                else if (flag == "--" + OptimizeConditions.IgnoreOrdinaryProcesses.ToString().ToLower() ||
                         flag == "-i")
                {
                    output.SetFlag(OptimizeConditions.IgnoreOrdinaryProcesses);
                }
            }

            return output;
        }
    }
}