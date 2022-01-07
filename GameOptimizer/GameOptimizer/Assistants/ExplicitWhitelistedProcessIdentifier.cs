using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace Zintom.GameOptimizer.Assistants
{
    /// <summary>
    /// Explicitly identifies whitlisted processes via the process whitelist configuration file.
    /// </summary>
    internal class ExplicitWhitelistedProcessIdentifier : IWhitelistedProcessIdentifierSource
    {
        internal const string WhitelistFile = "process_whitelist.txt";
        private const string FileComment = "#";

        readonly IOutputProvider? _outputDisplayer;

        List<string> _whiteListedProcessNames;

        internal ExplicitWhitelistedProcessIdentifier(IOutputProvider? outputDisplayer = null)
        {
            _outputDisplayer = outputDisplayer;

            // Check if the whitelist file exists on load, if not then create it.
            if (!File.Exists(WhitelistFile))
            {
                CreateDefaultProcessWhitelistFile();
            }

            Refresh();
        }

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
                    Debug.WriteLine(string.Format("Error occurred when loading default whitelist file, '{0}' not found in resources or could not be loaded.", WhitelistFile));
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

                    Debug.WriteLine($"Generated '{WhitelistFile}'");
                }
            }
        }

        /// <summary>
        /// Reads the <see cref="WhitelistFile"/> and returns the priority process names from within.
        /// </summary>
        /// <returns>All the priority process names in a <see cref="string"/> array.</returns>
        private static List<string> GetWhitelistedProcessNames(IOutputProvider? outputDisplayer)
        {
            if (!File.Exists(WhitelistFile))
            {
                CreateDefaultProcessWhitelistFile();
            }

            outputDisplayer?.Output(string.Format("Reading whitelist file '{0}'..", WhitelistFile));

            List<string> whitelistedProcessNames = new List<string>();

            string[] lines = File.ReadAllLines(WhitelistFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(FileComment, StringComparison.InvariantCulture))
                    whitelistedProcessNames.Add(lines[i].Substring(0, lines[i].IndexOf(FileComment)).Trim());
                else
                    whitelistedProcessNames.Add(lines[i].Trim());
            }

            outputDisplayer?.Output(string.Format("'{0}' loaded.", WhitelistFile));

            return whitelistedProcessNames;
        }

        public bool IsWhitelisted(Process process)
        {
            for (int i = 0; i < _whiteListedProcessNames.Count; i++)
            {
                if (process.ProcessName == _whiteListedProcessNames[i])
                {
                    return true;
                }
            }

            return false;
        }

        [MemberNotNull(nameof(_whiteListedProcessNames))]
        public void Refresh()
        {
            // Reload the whitelist file.
            _whiteListedProcessNames = GetWhitelistedProcessNames(_outputDisplayer);
        }
    }
}
