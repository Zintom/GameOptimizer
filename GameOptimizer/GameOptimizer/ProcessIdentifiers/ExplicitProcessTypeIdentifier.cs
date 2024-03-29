﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Zintom.GameOptimizer.Optimization;

namespace Zintom.GameOptimizer.ProcessIdentifiers
{
    /// <summary>
    /// Explicitly identifies whitlisted and game processes via the process whitelist configuration file.
    /// </summary>
    /// <remarks>Implements both <see cref="IWhitelistedProcessIdentifierSource"/> and <see cref="IGameProcessIdentifierSource"/>.</remarks>
    internal class ExplicitProcessTypeIdentifier : IWhitelistedProcessIdentifierSource, IGameProcessIdentifierSource
    {
        internal const string WhitelistFile = "process_whitelist.txt";

        private const string FileComment = "#";

        private readonly IOutputProvider? _outputDisplayer;

        /// <summary>
        /// This is a fallback source for identifying whether a process is a game if we can't determine it ourselves.
        /// </summary>
        private readonly IGameProcessIdentifierSource? _gameProcessIdentifierFallback;

        /// <summary>
        /// Holds the list of whitelisted process names as of the last <see cref="Refresh"/>.
        /// </summary>
        private readonly List<string> _whiteListedProcessNames = new();

        /// <summary>
        /// Holds the list of whitelisted process names as of the last <see cref="Refresh"/>.
        /// </summary>
        private readonly List<string> _gameProcessNames = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameProcessIdentifierFallback">A fallback source for identifying whether a process is a game if we can't determine it ourselves.</param>
        internal ExplicitProcessTypeIdentifier(IGameProcessIdentifierSource? gameProcessIdentifierFallback = null, IOutputProvider? outputDisplayer = null)
        {
            _outputDisplayer = outputDisplayer;
            _gameProcessIdentifierFallback = gameProcessIdentifierFallback;

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

        private void GetAllProcessesAndTypesFromWhitelistFile()
        {
            _whiteListedProcessNames.Clear();
            _gameProcessNames.Clear();

            string[] lines = File.ReadAllLines(WhitelistFile);

            // This is what we use to keep track of what
            // type of processes we are reading from the file,
            // when we encounter a relevant Process Type Tag (<Whitelisted>, <Games> etc)
            // this context variable will change to the relevant value, 'w' for whitelisted and 'g' for games.
            // The default unspecified type context is 'w'.
            char typeContext = 'w';

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // If the line is empty then skip it.
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Remove comments if they are present.
                if (line.Contains(FileComment))
                {
                    line = line.Substring(0, line.IndexOf(FileComment));
                }
                line = line.Trim();

                // Re-check if the line is now just whitespace after comment removal and skip if it is.
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Check for a type context tag (<Whitelisted>, <Games> etc).
                if (line.StartsWith("<Whitelisted>"))
                {
                    typeContext = 'w';

                    // Anything on this line after the type context tag is ignored.
                    continue;
                }
                else if (line.StartsWith("<Games>"))
                {
                    typeContext = 'g';

                    // Anything on this line after the type context tag is ignored.
                    continue;
                }

                // Using the typeContext we can determine which list this
                // process should go into.
                switch (typeContext)
                {
                    case 'w':
                        _whiteListedProcessNames.Add(line);
                        break;
                    case 'g':
                        _gameProcessNames.Add(line);
                        break;
                }
            }
        }

        public bool IsGame(IProcess process)
        {
            return _gameProcessNames.Contains(process.ProcessName) || (_gameProcessIdentifierFallback?.IsGame(process) ?? false);
        }

        public bool IsWhitelisted(IProcess process)
        {
            return _whiteListedProcessNames.Contains(process.ProcessName);
        }

        public void Refresh()
        {
            GetAllProcessesAndTypesFromWhitelistFile();

            // Refresh the fallback source as well.
            _gameProcessIdentifierFallback?.Refresh();
        }
    }
}
