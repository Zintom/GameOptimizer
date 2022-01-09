using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Zintom.GameOptimizer
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

        private static Dictionary<string, ProcessorLayout> _processorNameToLayout = new();

        //
        // The processor name should be the one given by the WMI under 'Name', this is also the name that Task Manager uses.
        //
        static ProcessorLayoutInformation()
        {
            _processorNameToLayout.Add("", new ProcessorLayout(Environment.ProcessorCount, Environment.ProcessorCount));

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

            // Return the appropriate ProcessorLayout for the given processor name.
            return _processorNameToLayout[processorName];
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

            internal ProcessorLayout(int physicalCores, int coresPerCCX = 0)
            {
                PhysicalCores = physicalCores;
                CoresPerCoreComplex = coresPerCCX;
            }
        }
    }
}
