using System.Diagnostics;

namespace Zintom.GameOptimizer.Menus
{
    public class HelpMenu : IConsoleMenu
    {
        public void Run(InteractiveShell.InteractiveShell gui)
        {
        reset:
            gui.DrawTitle(Program.AppName,
            "Help", "opt         | Optimizes games by isolating cores and adjusting low priorities." +
            "\nres         | Restores all processes back to normal." +
            "\nres -force  | Forces all processes to Normal Priority and Affinity 'All Cores'" +
            "\nedit        | Allows you to edit the priorty process list." +
            "\naudio       | Launches SndVol.exe -f allowing you to change the computers master volume." +
            "\naudio mixer | Launches SndVol.exe -m opening the volume mixer."
            , null, true);
            int result = gui.DisplayMenu(new string[] { "Open README.txt", "Open LICENSE.txt", "Back" });

            if (result == 0) { Process.Start("notepad.exe", "README.txt"); goto reset; }
            if (result == 1) { Process.Start("notepad.exe", "LICENSE.txt"); goto reset; }
        }
    }
}