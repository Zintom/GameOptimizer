using System;
using Zintom.StorageFacility;

namespace Zintom.GameOptimizer.Menus
{
    public class OptionsMenu : IConsoleMenu, ISettingsProvider
    {
        public readonly Storage _settings;

        public bool DisplayErrors
        {
            get
            {
                _settings.Booleans.TryGetValue(nameof(DisplayErrors), out bool displayErrors);
                return displayErrors;
            }
            private set => _settings.Edit().PutValue(nameof(DisplayErrors), value).Commit();
        }

        public bool TOTDEnabled
        {
            get
            {
                bool exists = _settings.Booleans.TryGetValue(nameof(TOTDEnabled), out bool totdEnabled);
                if (!exists) return true;

                return totdEnabled;
            }
            private set => _settings.Edit().PutValue(nameof(TOTDEnabled), value).Commit();
        }

        public int TipNumber
        {
            get
            {
                _settings.Integers.TryGetValue(nameof(TipNumber), out int tipNumber);
                return tipNumber;
            }
            set => _settings.Edit().PutValue(nameof(TipNumber), value).Commit();
        }

        public OptionsMenu()
        {
            Console.WriteLine("Loading settings information..");
            _settings = Storage.GetStorage("settings.dat");
        }

        public void Run(InteractiveShell.InteractiveShell gui)
        {
            int selectedOption = 0;

            while (true)
            {
                gui.FallbackDisplayOptions.FooterForegroundColour = ConsoleColor.White;
                gui.FallbackDisplayOptions.FooterVerticalPadding = 2;

                gui.DrawTitle("Customize Optimizer Settings", "Press enter on any of the options to toggle them on or off.", null, true);

                string[] options = new string[] {
                    "Display Errors: " + (DisplayErrors ? "On" : "Off"),
                    "Tip of the day: " + (TOTDEnabled ? "On" : "Off"),
                    $"Reset TOTD ({TipNumber + 1})",
                    "Save and Exit"
                };
                string[] footers = new string[] {
                    "If on, any errors during optimization will be displayed",
                    "Shows at launch of the app                             ",
                    "Reset the 'Tip of the day' back to #1                  ",
                    "Saves all changes and goes back to the previous menu.  "
                };

                selectedOption = gui.DisplayMenu(options, footers, null, selectedOption);

                switch (selectedOption)
                {
                    case 0:
                        DisplayErrors = !DisplayErrors;
                        break;
                    case 1:
                        TOTDEnabled = !TOTDEnabled;
                        break;
                    case 2:
                        TipNumber = 0;
                        break;
                    default:
                        return;
                }
            }
        }
    }

    public interface ISettingsProvider
    {
        /// <summary>
        /// Program should print errors if they are encountered.
        /// </summary>
        public bool DisplayErrors { get; }

        public bool TOTDEnabled { get; }

        /// <summary>
        /// The current tip for 'Tip of the day'.
        /// </summary>
        public int TipNumber { get; set; }
    }
}
