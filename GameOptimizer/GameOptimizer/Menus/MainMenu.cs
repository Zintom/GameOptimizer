using System;

namespace Zintom.GameOptimizer.Menus
{
    internal class MainMenu : IConsoleMenu
    {
        private readonly OptimizeMenu optimizeMenu;
        private readonly CommandMenu commandMenu;
        private readonly OptionsMenu optionsMenu;

        internal MainMenu()
        {
            optionsMenu = new OptionsMenu();
            optimizeMenu = new OptimizeMenu(settingsProvider: optionsMenu);
            commandMenu = new CommandMenu();
        }

        public void Run(InteractiveShell.InteractiveShell gui)
        {
            TOTDMenu tOTDMenu = new TOTDMenu(settingsProvider: optionsMenu);
            tOTDMenu.Run(gui);

            int selectedOption = 0;

            while (true)
            {
                gui.DrawTitle(Program.AppName, Program.Optimizer.IsOptimized ? "Currently optimized, use 'Restore' or the command 'res' to de-optimize.\nSome menu options are unavailable because of this." : "Main Menu", null, true);
                selectedOption = gui.DisplayMenu(new string[] { Program.Optimizer.IsOptimized ? "Unavailable" : "Optimize >",
                                                                       Program.Optimizer.IsOptimized ? "Unavailable" : "Command Input >",
                                                                       "Restore",
                                                                       "Options >",
                                                                       "Help >",
                                                                       "Exit" }, null, selectedOption);

                switch (selectedOption)
                {
                    case 0:
                        if (Program.Optimizer.IsOptimized) continue;

                        optimizeMenu.Run(gui);
                        break;
                    case 1:
                        if (Program.Optimizer.IsOptimized) continue;

                        commandMenu.Run(gui);
                        break;
                    case 2:
                        commandMenu.RunCommand("res", gui);
                        break;
                    case 3:
                        optionsMenu.Run(gui);
                        break;
                    case 4:
                        commandMenu.RunCommand("help", gui);
                        break;
                    case 5:
                        commandMenu.RunCommand("exit", gui);
                        break;
                }

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}