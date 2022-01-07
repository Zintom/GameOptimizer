using System;
using System.Diagnostics;
using Zintom.GameOptimizer.Assistants;
using Zintom.GameOptimizer.Menus;
using Zintom.InteractiveShell;

namespace Zintom.GameOptimizer
{

    partial class Program
    {

        public static string AppName = "Zintom's Game Optimizer";
        internal const string ConfigFile = "config.json";

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
            ConfigManager.WriteDefaultConfigIfNotExists(ConfigFile);

            Optimizer = new Optimizer(WhitelistReader.GetWhitelistedProcessNames(outputDisplayer), ConfigManager.Read(ConfigFile), outputDisplayer);

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
        /// Opens the given filePath with the default windows program.
        /// </summary>
        /// <remarks>Only supported on <see cref="System.Runtime.InteropServices.OSPlatform.Windows"/></remarks>
        /// <param name="filePath">The path to the file to start.</param>
        public static void OpenWithDefaultProgram(string filePath)
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return;

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "explorer",
                    Arguments = $"\"{filePath}\""
                };
                process.Start();
            }
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