using System;
using System.IO;
using Zintom.GameOptimizer.Assistants;
using Zintom.StorageFacility;

namespace Zintom.GameOptimizer.Menus
{
    public class OptionsMenu : IConsoleMenu, ISettingsProvider
    {
        private const string SettingsFile = "opt_settings";
        public readonly Storage _settings;

        public bool DisplayErrors
        {
            get => _settings.Booleans.GetValue(nameof(DisplayErrors), false);
            private set => _settings.Edit().PutValue(nameof(DisplayErrors), value).Commit();
        }

        public bool TOTDEnabled
        {
            get => _settings.Booleans.GetValue(nameof(TOTDEnabled), true);
            private set => _settings.Edit().PutValue(nameof(TOTDEnabled), value).Commit();
        }

        public int TipNumber
        {
            get => _settings.Integers.GetValue(nameof(TipNumber), 0);
            set => _settings.Edit().PutValue(nameof(TipNumber), value).Commit();
        }

        public OptionsMenu()
        {
            Console.WriteLine("Loading settings information..");
            _settings = Storage.GetStorage(SettingsFile);

            // Try to hide the settings file from the user as to not clutter
            // the program folder.
            try
            {
                var fileAttributes = File.GetAttributes(SettingsFile);

                if (!fileAttributes.HasFlag(FileAttributes.Hidden))
                    File.SetAttributes(SettingsFile, fileAttributes | FileAttributes.Hidden);
            }
            catch { }
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
                    "Edit whitelist file",
                    "Back"
                };
                string[] footers = new string[] {
                    "If enabled, any errors during optimization will be displayed.\n                      ",
                    "Enable/disable the tip of the day screen                     \nwhich shows on launch.",
                    "Reset the 'Tip of the day' back to #1.                       \n                      ",
                    "Opens the whitelist file in the default editor program.      \n                      ",
                    "Goes back to the previous menu.                              \n                      "
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
                    case 3:
                        Program.OpenWithDefaultProgram(ExplicitWhitelistedProcessIdentifier.WhitelistFile);
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
