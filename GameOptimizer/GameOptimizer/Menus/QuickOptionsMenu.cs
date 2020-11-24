using System;

namespace Zintom.GameOptimizer.Menus
{
    public class QuickOptionsMenu : IConsoleMenu
    {

        public void Run(InteractiveShell.InteractiveShell gui)
        {
            gui.DrawTitle(Program.AppName, "Select a quick option to execute", null, true);
            string[] quickCommands = new string[] { "1: Default optimization",
                            "2: Default optimization plus Affinity optimization",
                            "3: Boost whitelisted processes and de-prioritise everything else.",
                            "4: Just boost whitelisted processes without touching other processes",
                            "Back"};

            int result = gui.DisplayMenu(quickCommands);

            switch (result)
            {
                case 0:
                    Program.Command_OptimizeWithFlags(OptimizeConditions.None);
                    break;
                case 1:
                    Program.Command_OptimizeWithFlags(OptimizeConditions.OptimizeAffinity);
                    break;
                case 2:
                    Program.Command_OptimizeWithFlags(OptimizeConditions.BoostPriorities);
                    break;
                case 3:
                    Program.Command_OptimizeWithFlags(OptimizeConditions.BoostPriorities | OptimizeConditions.IgnoreOrdinaryProcesses);
                    break;
                default:
                    return;
            }
        }
    }
}
