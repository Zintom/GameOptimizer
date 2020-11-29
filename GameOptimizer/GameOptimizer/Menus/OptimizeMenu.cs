using System;

namespace Zintom.GameOptimizer.Menus
{
    public class OptimizeMenu : IConsoleMenu
    {

        public void Run(InteractiveShell.InteractiveShell gui)
        {
            gui.DrawTitle(Program.AppName, "Select an optimization method to execute:", null, true);
            string[] quickCommands = new string[] { "1: Default optimization",
                            "2: Default optimization plus Affinity optimization",
                            "Back"};

            string[] footerTexts = new string[] {
                "Sets all non-whitelisted processes to 'Low' priority,   \nwhilst leaving whitelisted processes at 'Normal' priority.              \n                       ",
                "Manages process priorties in the same way as option 1   \nwhilst also restricting all non-whitelisted processes to use only       \nthe last two CPU cores.",
                "Go back to the previous menu.                           \n                                                                        \n                       "
            };

            gui.FallbackDisplayOptions.FooterVerticalPadding = 2;
            gui.FallbackDisplayOptions.FooterForegroundColour = ConsoleColor.Cyan;
            int result = gui.DisplayMenu(quickCommands, footerTexts);

            switch (result)
            {
                case 0:
                    Program.Command_OptimizeWithFlags(OptimizeConditions.None);
                    break;
                case 1:
                    Program.Command_OptimizeWithFlags(OptimizeConditions.OptimizeAffinity);
                    break;
                //case 2:
                //    Program.Command_OptimizeWithFlags(OptimizeConditions.BoostPriorities);
                //    break;
                //case 3:
                //    Program.Command_OptimizeWithFlags(OptimizeConditions.BoostPriorities | OptimizeConditions.IgnoreOrdinaryProcesses);
                //    break;
                default:
                    return;
            }
        }
    }
}
