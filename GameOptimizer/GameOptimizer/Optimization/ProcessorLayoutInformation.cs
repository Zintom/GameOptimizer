using System;
using System.Collections.Generic;
using System.Management;

namespace Zintom.GameOptimizer.Optimization
{

    //
    // Handy resources for core configurations.
    //
    // https://en.wikipedia.org/wiki/Template:AMD_Ryzen_1000_series
    // https://en.wikipedia.org/wiki/Template:AMD_Ryzen_2000_series
    // https://en.wikipedia.org/wiki/Template:AMD_Ryzen_3000_series
    // https://en.wikipedia.org/wiki/Template:AMD_Ryzen_5000_series
    // 
    // We're interested in the 'Core config', aka how many CCX's there are and how many cores per CCX.
    //
    internal class ProcessorLayoutInformation
    {

        private static readonly Dictionary<string, ProcessorLayout> _processorNameToLayout = new();

        private static string? _currentProcessorNameCache = null;

        //
        // The processor name should be the one given by the WMI under 'Name', this is also the name that Task Manager uses.
        //
        static ProcessorLayoutInformation()
        {
            _processorNameToLayout.Add("", new ProcessorLayout(Environment.ProcessorCount, 0));

            // Add more processors here
            _processorNameToLayout.Add("AMD Ryzen 5 1600 Six-Core Processor", new ProcessorLayout(6, 3));
            _processorNameToLayout.Add("AMD Ryzen 7 1700 Eight-Core Processor", new ProcessorLayout(8, 4));
        }

        /// <summary>
        /// Gets the <see cref="ProcessorLayout"/> for the active CPU.
        /// </summary>
        /// <returns></returns>
        internal static ProcessorLayout GetCurrentProcessorLayout()
        {
            if (_currentProcessorNameCache != null)
            {
                return _processorNameToLayout[_currentProcessorNameCache];
            }

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            string processorName = "";
            foreach (var i in searcher.Get())
            {
                try
                {
                    processorName = i.Properties["Name"]?.Value?.ToString() ?? "";
                    break;
                }
                catch { }
            }

            // Sometimes the WMI returns the name with whitespace chars, so trim it.
            _currentProcessorNameCache = processorName.Trim();

            // If we don't have a layout for this CPU then use the default.
            if (!_processorNameToLayout.ContainsKey(_currentProcessorNameCache))
            {
                _currentProcessorNameCache = "";
            }

            // Return the appropriate ProcessorLayout for the given processor name.
            return _processorNameToLayout[_currentProcessorNameCache];
        }

        internal struct ProcessorLayout
        {
            /// <summary>
            /// The number of physical cores on this processor.
            /// </summary>
            internal readonly int PhysicalCores;

            /// <summary>
            /// The number of physical cores per Core Complex (CCX) on this processor.
            /// </summary>
            /// <remarks>Only relevant to AMD CPU's.</remarks>
            internal readonly int CoresPerCoreComplex;

            /// <summary>
            /// The number of Core Complexes (CCX's) on this processor.
            /// </summary>
            internal readonly int NumberOfCoreComplexes
            {
                get => PhysicalCores / CoresPerCoreComplex;
            }

            /// <summary>
            /// Indicates whether this processor has "Core Complexes".
            /// </summary>
            /// <remarks>A <see langword="true"/> value usually indicates whether the processor is using the Zen architecture.</remarks>
            internal bool HasCoreComplexes
            {
                get => CoresPerCoreComplex != 0;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="physicalCores"></param>
            /// <param name="coresPerCCX">A value of '0' indicates this processor does not have Core Complexes.</param>
            internal ProcessorLayout(int physicalCores, int coresPerCCX = 0)
            {
                PhysicalCores = physicalCores;
                CoresPerCoreComplex = coresPerCCX;
            }
        }
    }
}
