using System.IO;
using System.Text.Json;

namespace Zintom.GameOptimizer
{
    /// <summary>
    /// Holds configuration information for the program.
    /// </summary>
    /// <remarks>
    /// Additionally provides means to write and read <see cref="Config"/> object's to and from a file.
    /// </remarks>
    internal class Config
    {
        #region Properties for JSON to encode

        public string[]? StreamerSpecificExecutables { get; set; }

        public int[]? LimitStreamerSpecificExecutablesAffinity { get; set; }

        public int OptimizeDelayTimeMillis { get; set; }

        #endregion

        private Config()
        {
            StreamerSpecificExecutables = new string[] { "obs64", "ffmpeg-mux64", "obs-ffmpeg-mux" };
            LimitStreamerSpecificExecutablesAffinity = null;
            OptimizeDelayTimeMillis = 1000;
        }

        internal static Config Default { get => new Config(); }

        internal static void WriteDefaultConfigIfNotExists(string path)
        {
            if (File.Exists(path)) { return; }

            Write(path, Default);
        }

        /// <summary>
        /// Attempts to read the given config file at the <paramref name="path"/>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A <see cref="Config"/> object, or <see langword="null"/> if the config file doesn't exist.</returns>
        internal static Config? Read(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream))
            {
                return JsonSerializer.Deserialize<Config>(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">The path to save the configuration to.</param>
        /// <param name="config">The <see cref="Config"/> object to write.</param>
        internal static void Write(string path, Config config)
        {
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true }));

                writer.Flush();
                writer.Close();
            }
        }
    }
}
