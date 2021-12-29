using System;
using System.IO;
using System.Text.Json;

namespace Zintom.GameOptimizer
{
    internal class Config
    {
        public string[]? StreamerSpecificExecutables { get; set; }

        public int[]? LimitStreamerSpecificExecutablesAffinity { get; set; }
    }

    internal static class ConfigManager
    {

        public static void WriteDefaultConfigIfNotExists(string path)
        {
            if (File.Exists(path)) { return; }

            var config = new Config()
            {
                StreamerSpecificExecutables = new string[] { "obs64", "ffmpeg-mux64", "obs-ffmpeg-mux" },
                LimitStreamerSpecificExecutablesAffinity = new int[] { 5 }
            };

            Write(path, config);
        }

        /// <summary>
        /// Attempts to read the given config file at the <paramref name="path"/>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A <see cref="Config"/> object, or <see langword="null"/> if the config file doesn't exist.</returns>
        public static Config? Read(string path)
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
        public static void Write(string path, Config config)
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
