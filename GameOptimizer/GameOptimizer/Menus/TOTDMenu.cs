using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zintom.GameOptimizer.Menus
{
    public class TOTDMenu : IConsoleMenu
    {
        private string[] tips = new string[] { "Tip 1", "Tip 2" };

        ISettingsProvider _settings;

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
                gui.DrawTitle(string.Format("Tip of the day! #{0}", _settings.TipNumber + 1), tips[_settings.TipNumber], null, true);

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