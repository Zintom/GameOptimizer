using System;

namespace Zintom.GameOptimizer.Menus
{
    internal class OptimizeMenu : IConsoleMenu
    {

        private readonly ISettingsProvider _settingsProvider;

        internal OptimizeMenu(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public void Run(InteractiveShell.InteractiveShell gui)
        {
            gui.DrawTitle(Program.AppName, "Select an optimization method to execute.\n(Run this app as administrator for more boosting power!)", null, true);
            string[] quickCommands = new string[] { "1: Default optimization",
                            "2: Priority and Affinity optimization.",
                            //"3: Streamer mode optimization",
                            "Back"};

            string[] footerTexts = new string[] {
                "Sets all non-whitelisted/game processes to 'Low' priority  \nwhilst leaving whitelisted and game processes at 'Normal' priority.   \n                             ",
                "Sets process priorities in the same way as option 1,       \nhowever it also optimizes the distribution of processes across all of \nyour system processing cores.",
                //"Forces all 'streamer specific' processes onto the cpu cores\nspecified in the 'config.json' file, and forces all other processes onto the      \nremaining cores.       ",
                "Go back to the previous menu.                              \n                                                                      \n                             "
            };

            gui.FallbackDisplayOptions.FooterVerticalPadding = 2;
            gui.FallbackDisplayOptions.FooterForegroundColour = ConsoleColor.Cyan;
            int result = gui.DisplayMenu(quickCommands, footerTexts);

            var baseOptimizeConditions = OptimizeConditions.None;

            // Add any flags set by the ISettingsProvider
            if (!_settingsProvider.AutoHideWindow) baseOptimizeConditions.SetFlag(OptimizeConditions.NoHide);

            switch (result)
            {
                case 0:
                    Program.Command_OptimizeWithFlags(baseOptimizeConditions);
                    break;
                case 1:
                    Program.Command_OptimizeWithFlags(baseOptimizeConditions | OptimizeConditions.OptimizeAffinity);
                    break;
                //case 2:
                //    Program.Command_OptimizeWithFlags(baseOptimizeConditions | OptimizeConditions.StreamerMode);
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
