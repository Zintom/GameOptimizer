using System;
using System.Collections.Generic;
using System.IO;
using Zintom.GameOptimizer.Menus;
using Zintom.InteractiveShell;

namespace Zintom.GameOptimizer
{

    partial class Program
    {

        public static string AppName = "Zintom's Game Optimizer";
        public const string WhitelistFile = "process_whitelist.txt";
        const string FileComment = "##";

        const int _optimizeWaitTimeMillis = 5000;

        private static ConsoleColor _defaultBackColor;
        private static ConsoleColor _defaultForeColor;

        private static readonly IOutputProvider outputDisplayer = new CLIOutputDisplayer();
        public static Optimizer Optimizer { get; private set; } = default!;

        private static InteractiveShell.InteractiveShell _gui = default!;

        static void Main(string[] args)
        {
            AppName = "Zintom's Game Optimizer - " + GetVersionInformation();

            if (args.Length > 0 && args[0] == "-melody!")
            {
                AppName = "Zintom's Melody Fluffer!";
            }

            Console.Title = AppName;
            _defaultBackColor = Console.BackgroundColor;
            _defaultForeColor = Console.ForegroundColor;

            SetupInteractiveShell();

            Optimizer = new Optimizer(GetWhitelistedProcessNames(outputDisplayer), outputDisplayer);

            // Begin Main Menu application loop
            IConsoleMenu mainMenu = new MainMenu();
            mainMenu.Run(_gui);
        }

        //public static void PlayMusic()
        //{
        //    try
        //    {
        //        // Only works on windows
        //        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        //        {
        //            var assembly = Assembly.GetExecutingAssembly();
        //            var resName = "Zintom.GameOptimizer.optimizer_background_compressed.wav";
        //            Stream? s = assembly.GetManifestResourceStream(resName);
        //            if (s != null)
        //            {
        //                SoundPlayer player = new SoundPlayer(s);
        //                player.PlayLooping();
        //            }
        //        }
        //    }
        //    catch { }
        //}

        static void SetupInteractiveShell()
        {
            _gui = new InteractiveShell.InteractiveShell();

            var _shellDisplayOptions = new ShellDisplayOptions()
            {
                LeftOffset = 2
            };

            var _shellTitleDisplayOptions = new ShellTitleDisplayOptions()
            {
                LeftOffset = 2
            };

            _gui.FallbackDisplayOptions = _shellDisplayOptions;
            _gui.FallbackTitleDisplayOptions = _shellTitleDisplayOptions;
        }

        /// <summary>
        /// Gets the <see cref="Version"/> information for the currently executing <see cref="System.Reflection.Assembly"/>.
        /// </summary>
        /// <returns>A string of the <see cref="Version"/> in the format "<c>Major.Minor.Build</c>"</returns>
        static string GetVersionInformation()
        {
            Version? version = System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Version;
            if (version == null) return "Version information not available.";

            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        /// <summary>
        /// Reset the console colours back to their defaults.
        /// </summary>
        public static void ResetColours()
        {
            Console.BackgroundColor = _defaultBackColor;
            Console.ForegroundColor = _defaultForeColor;
        }

        /// <summary>
        /// Creates the default priority proccesses file if it does not exist.
        /// </summary>
        static void CreateDefaultProcessWhitelistFile()
        {
            using (var stream = File.Create(WhitelistFile))
            using (StreamWriter sw = new StreamWriter(stream))
            {
                sw.WriteLine(string.Format(
                    "{0} Put processes you don't want affected by the optimizer here.{1}" +
                    "{0} Apps{1}Steam{1}SteamService{1}steamwebhelper{1}GameOverlayUI{1}" +
                    "NVDisplay.Container{1}nvsphelper64{1}ffmpeg-mux64 ## OBS's encoder{1}obs64 ## Open Broadcaster{1}discord{1}{1}" +
                    "{0} Games{1}javaw {1}Minecraft", FileComment, Environment.NewLine));
                sw.Flush();
                sw.Close();
            }

            Console.WriteLine("Generated " + WhitelistFile);
        }

        /// <summary>
        /// Reads the <see cref="WhitelistFile"/> and returns the priority process names from within.
        /// </summary>
        /// <returns>All the priority process names in a <see cref="string"/> array.</returns>
        static List<string> GetWhitelistedProcessNames(IOutputProvider outputDisplayer)
        {
            if (!File.Exists(WhitelistFile))
            {
                CreateDefaultProcessWhitelistFile();
            }

            outputDisplayer.Output(string.Format("Reading whitelist file '{0}'..", WhitelistFile));

            List<string> whitelistedProcessNames = new List<string>();

            string[] lines = File.ReadAllLines(WhitelistFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(FileComment, StringComparison.InvariantCulture))
                    whitelistedProcessNames.Add(lines[i].Substring(0, lines[i].IndexOf(FileComment)).Trim());
                else
                    whitelistedProcessNames.Add(lines[i].Trim());
            }

            outputDisplayer.Output(string.Format("'{0}' loaded.", WhitelistFile));

            return whitelistedProcessNames;
        }

        private class CLIOutputDisplayer : IOutputProvider
        {
            void IOutputProvider.Output(string message)
            {
                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);

                ResetColours();
            }

            void IOutputProvider.OutputError(string errorMessage)
            {
                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(errorMessage);

                ResetColours();
            }

            void IOutputProvider.OutputHighlight(string message)
            {
                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine(message);

                ResetColours();
            }
        }
    }
}