using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Zintom.GameOptimizer.Menus;
using Zintom.InteractiveShell;

namespace Zintom.GameOptimizer
{

    partial class Program
    {

        public static string AppName = "Zintom's Game Optimizer";
        public const string WhitelistFile = "process_whitelist.txt";
        const string FileComment = "#";

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
            Assembly assembly = Assembly.GetExecutingAssembly();
            string defaultListResource = "Zintom.GameOptimizer.Assets.default_process_whitelist.txt";

            // Try get the text resource.
            using (var resourceStream = assembly.GetManifestResourceStream(defaultListResource))
            {
                if (resourceStream == null)
                {
                    Console.WriteLine(string.Format("Error occurred when loading default whitelist file, '{0}' not found in resources or could not be loaded.", WhitelistFile));
                    return;
                }

                using (var resourceReader = new StreamReader(resourceStream))
                using (var fileStream = File.Create(WhitelistFile))
                using (var fileWriter = new StreamWriter(fileStream))
                {
                    // Write the resource to file, flush and close.
                    fileWriter.Write(resourceReader.ReadToEnd());
                    fileWriter.Flush();
                    fileWriter.Close();

                    Console.WriteLine($"Generated '{WhitelistFile}'");
                }
            }
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