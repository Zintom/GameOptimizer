using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace Zintom.GameOptimizer
{
    internal interface IGameIdentifierSource
    {
        /// <summary>
        /// Determines whether the given <paramref name="process"/> is a game or not.
        /// </summary>
        /// <param name="process"></param>
        /// <returns><see langword="true"/> if the given <paramref name="process"/> is believed to be a game, or <see langword="false"/> otherwise.</returns>
        bool IsGame(Process process);
    }

    public class UsageBasedGameIdentifierSource : IGameIdentifierSource
    {

        /// <summary>
        /// This Regex extracts the Process ID (PID) in an '<c>InstanceName</c>' from the '<c>GPU Engine</c>' <see cref="PerformanceCounter"/>.
        /// </summary>
        private readonly Regex _pidFromGpuEnginePerformanceCounter = new Regex("^pid_(?<pid>\\d+)_", RegexOptions.ExplicitCapture);

        private long _lastTimeGpuStatsCalculated = 0;

        /// <summary>
        /// A dictionary which holds sets of Process ID's and their associated GPU usage values.
        /// </summary>
        private Dictionary<int, float> _gpuUsageStatsCache = new();

        public bool IsGame(Process process)
        {
            if (DateTimeOffset.Now.ToUnixTimeSeconds() - _lastTimeGpuStatsCalculated > 30)
            {
                Debug.WriteLine("GPU Usage cache out of date, re-acquiring.");

                _gpuUsageStatsCache = CaptureGpuUsageStats();
                _lastTimeGpuStatsCalculated = DateTimeOffset.Now.ToUnixTimeSeconds();
            }

            if(_gpuUsageStatsCache.TryGetValue(process.Id, out float gpuUsage))
            {
                if (gpuUsage > 1)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Captures the GPU usage over a 1 second window.
        /// </summary>
        /// <remarks>This is a blocking function.</remarks>
        private Dictionary<int, float> CaptureGpuUsageStats()
        {
            // Holds an InstanceName and a CounterSample for that instance.
            Dictionary<string, CounterSample> _instanceSampleStore = new();

            // Holds an InstanceName and its calculated GPU usage percentage.
            Dictionary<string, float> instanceCalculatedPercentageStore = new();

            var gpuEngineCategory = GetGpuCounterCategory();
            if (gpuEngineCategory == null) return new();

            long captureBeginTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            do
            {
                foreach (var instanceName in gpuEngineCategory.GetInstanceNames())
                {
                    if (!instanceName.EndsWith("eng_0_engtype_3D"))
                    {
                        continue;
                    }

                    foreach (var counter in gpuEngineCategory.GetCounters(instanceName))
                    {
                        if (counter.CounterName != "Utilization Percentage")
                        {
                            continue;
                        }

                        if (_instanceSampleStore.TryGetValue(instanceName, out var oldSample))
                        {
                            var newSample = counter.NextSample();
                            _instanceSampleStore[instanceName] = newSample;

                            if (!instanceCalculatedPercentageStore.ContainsKey(instanceName))
                            {
                                instanceCalculatedPercentageStore.Add(instanceName, 0);
                            }

                            instanceCalculatedPercentageStore[instanceName] = CounterSample.Calculate(oldSample, newSample);

                            //Debug.WriteLine($"{instanceName}: {instanceCalculatedPercentageStore[instanceName]}");
                        }
                        else
                        {
                            _instanceSampleStore.Add(instanceName, counter.NextSample());
                        }
                    }
                }

                Thread.Sleep(250);
            }
            while (DateTimeOffset.Now.ToUnixTimeMilliseconds() - captureBeginTime < 1000); // Setting this too low yields zero (0) for the counters.

            _instanceSampleStore.Clear();

            // Holds a Process ID and its associated GPU usage value.
            Dictionary<int, float> processIdGpuUsageStore = new();

            foreach (var pair in instanceCalculatedPercentageStore)
            {
                // Try to extract the PID from the instance name.
                Match pidMatch = _pidFromGpuEnginePerformanceCounter.Match(pair.Key);
                if (!pidMatch.Success)
                {
                    // The pid could not be determined, move to the next instance.
                    continue;
                }

                int pid = int.Parse(pidMatch.Groups["pid"].Value);

                processIdGpuUsageStore.TryAdd(pid, pair.Value);
            }

            return processIdGpuUsageStore;

            //// Compile the list of InstanceNames-to-Percentages into their respective PID-to-Percentage pairs.
            //List<ProcessGpuUsage> result = new();

            //foreach (var pair in instanceCalculatedPercentageStore)
            //{
            //    // Try to extract the PID from the instance name.
            //    Match pidMatch = _pidFromGpuEnginePerformanceCounter.Match(pair.Key);
            //    if (!pidMatch.Success)
            //    {
            //        // The pid could not be determined, move to the next instance.
            //        continue;
            //    }

            //    int pid = int.Parse(pidMatch.Groups["pid"].Value);

            //    result.Add(new ProcessGpuUsage(pid, pair.Value));
            //}

            //return result;
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounterCategory"/> for "<c>GPU Engine</c>".
        /// </summary>
        /// <returns></returns>
        private static PerformanceCounterCategory? GetGpuCounterCategory()
        {
            var categories = PerformanceCounterCategory.GetCategories();
            foreach (var category in categories)
            {
                if (category.CategoryName == "GPU Engine")
                {
                    return category;
                }
            }

            return null;
        }

        [Obsolete("This is an ineffective way of detecting whether something is a game as common windows applications also have these libraries loaded.")]
        private static bool ContainsCommon3DLibrary(Process process)
        {
            ProcessModuleCollection? modules;
            try
            {
                modules = process.Modules;
            }
            catch
            {
                return false;
            }

            ProcessModule module;
            for (int i = 0; i < modules.Count; i++)
            {
                module = modules[i];
                string? moduleName = module.ModuleName?.ToLower();

                if (moduleName == "opengl32.dll" ||
                    moduleName == "d3d9.dll" ||
                    moduleName == "d3d10.dll" ||
                    moduleName == "d3d11.dll")
                {
                    return true;
                }
            }

            return false;
        }

        [DebuggerDisplay("PID = {ProcessID}, GPU Usage = {GPU_Usage}")]
        /// <summary>
        /// Represents a Process and its associated GPU usage at a given moment in time.
        /// </summary>
        private struct ProcessGpuUsage
        {
            internal readonly int ProcessID;
            internal readonly float GPU_Usage;

            internal ProcessGpuUsage(int processId, float gpuUsage)
            {
                ProcessID = processId;
                GPU_Usage = gpuUsage;
            }
        }
    }
}
