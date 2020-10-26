using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameOptimizer.Helpers
{
    /// <summary>
    /// Helper class for controlling the console window.
    /// </summary>
    public static class ConsoleWindowControl
    {

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_MINIMIZE = 6;

        /// <summary>
        /// Minimizes the console window.
        /// </summary>
        public static void MinimizeConsoleWindow()
        {
            ShowWindow(GetConsoleWindow(), SW_MINIMIZE);
        }
    }
}