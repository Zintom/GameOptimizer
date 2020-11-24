using System;

namespace Zintom.GameOptimizer.Menus
{
    public class MainMenu : IConsoleMenu
    {
        readonly Optimizer _optimizer;

        public MainMenu(Optimizer optimizer)
        {
            _optimizer = optimizer;
        }

        public void Run(InteractiveShell.InteractiveShell gui)
        {
            CommandMenu commandMenu = new CommandMenu(_optimizer);

            while (true)
            {
                gui.DrawTitle(Program.AppName, _optimizer.IsOptimized ? "Currently optimized, use 'Restore' or the command 'res' to de-optimize.\nSome menu options are unavailable because of this." : "Main Menu", null, true);
                int menuResult = gui.DisplayMenu(new string[] { _optimizer.IsOptimized ? "Unavailable" : "Quick Options",
                                                                       _optimizer.IsOptimized ? "Unavailable" : "Command Input",
                                                                       "Restore",
                                                                       "Help",
                                                                       "Exit" });
                switch (menuResult)
                {
                    case 0:
                        if (_optimizer.IsOptimized) continue;

                        IConsoleMenu quickOptionsMenu = new QuickOptionsMenu();
                        quickOptionsMenu.Run(gui);
                        break;
                    case 1:
                        if (_optimizer.IsOptimized) continue;

                        commandMenu.Run(gui);
                        break;
                    case 2:
                        commandMenu.RunCommand("res", gui);
                        break;
                    case 3:
                        commandMenu.RunCommand("help", gui);
                        break;
                    case 4:
                        commandMenu.RunCommand("exit", gui);
                        break;
                }

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}