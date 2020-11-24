using System;
using System.Threading;
using Zintom.GameOptimizer.Helpers;

namespace Zintom.GameOptimizer
{
    partial class Program
    {
        private const string Text_PressAnyKeyToGoBack = "Press any key to go back.";
        private const string Text_Optimized = "\nOptimized.";

        static void Command_OptimizeWithFlags(OptimizeConditions flags)
        {
            _gui.DrawTitle(AppName, $"Optimizing in {_optimizeWaitTimeMillis / 1000} seconds...", null, true);
            Thread.Sleep(_optimizeWaitTimeMillis);

            _gui.DrawContentText($"Optimizing (Flags: {flags})...");

            _gui.DrawSubtitleText(string.Format("\nCompleted {0} optimizations.", optimizer.Optimize(flags)));

            if (!flags.HasFlag(OptimizeConditions.NoHide))
            {
                Thread.Sleep(1500);
                NativeMethods.MinimizeConsoleWindow();
            }

            _gui.DrawContentText(Text_PressAnyKeyToGoBack, false); Console.ReadKey();
        }

        static void Command_Restore()
        {
            _gui.DrawTitle(AppName, "Restoring", null, true);

            _gui.DrawSubtitleText(string.Format("\nCompleted {0} restore operations.", optimizer.Restore()));

            _gui.DrawContentText(Text_PressAnyKeyToGoBack, false); Console.ReadKey();
        }

        static void Command_ForceRestore()
        {
            _gui.DrawTitle(AppName, "Force restoring process priorities to Normal.", null, true);

            _gui.DrawSubtitleText(string.Format("\n{0} processes affected by the force-restore.", optimizer.ForceRestoreToNormal()));

            _gui.DrawContentText(Text_PressAnyKeyToGoBack, false); Console.ReadKey();
        }

    }
}
