namespace Zintom.GameOptimizer.Menus
{
    public class TOTDMenu : IConsoleMenu
    {
        private readonly string[] tips = new string[] {
            "Make sure to add your games/apps to the whitelist file\nor they will be negatively affected by the optimizer!", 
            "Use the 'edit' command to quickly\nopen the process whitelist file.",
            "Run the application as administrator for more boosting power!\n\nThe optimizer will be able to isolate and\ndeprioritise more applications.",
            "Use Affinity optimization for even more kick!\n\nSee option 2 under the 'Optimize' sub-menu.",
            "Cannot be run in DOS mode!",
            "If for some reason you delete the 'restore_state' file\nand are unable to restore optimizations;\nEither restart your PC(recommended) or use the\n'res -force' command from the 'Command Input' screen."
            };

        readonly ISettingsProvider _settings;

        public TOTDMenu(ISettingsProvider settingsProvider)
        {
            _settings = settingsProvider;
        }

        public void Run(InteractiveShell.InteractiveShell gui)
        {
            if (!_settings.TOTDEnabled) return;

            int selectedOption = 0;
            while (true)
            {
                gui.DrawTitle("Tip of the day! ", string.Format("Tip #{0}", _settings.TipNumber + 1), null, true);
                gui.DrawSubtitleText(tips[_settings.TipNumber]);

                selectedOption = gui.DisplayMenu(new string[] { "Proceed to Main Menu", "Next Tip" }, null, selectedOption);

                // Increment tip by one now that the current tip has been viewed.
                _settings.TipNumber++;
                if (_settings.TipNumber >= tips.Length)
                    _settings.TipNumber = 0;

                if (selectedOption == 0)
                    break;
            }
        }
    }
}