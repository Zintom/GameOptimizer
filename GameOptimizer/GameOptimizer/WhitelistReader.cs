using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Zintom.GameOptimizer
{
    internal class WhitelistReader
    {

        internal const string WhitelistFile = "process_whitelist.txt";
        private const string FileComment = "#";

        /// <summary>
        /// Creates the default priority proccesses file if it does not exist.
        /// </summary>
        private static void CreateDefaultProcessWhitelistFile()
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
        internal static List<string> GetWhitelistedProcessNames(IOutputProvider outputDisplayer)
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

    }
}
