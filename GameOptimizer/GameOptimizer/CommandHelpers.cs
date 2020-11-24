using System;
using System.Threading;
using Zintom.GameOptimizer.Helpers;

namespace Zintom.GameOptimizer
{
    partial class Program
    {
        private const string Text_PressAnyKeyToGoBack = "Press any key to go back.";
        private const string Text_Optimized = "\nOptimized.";

        static void Command_OptimizeNoFlags()
        {
            _interactiveShell.DrawTitle(AppName, $"Optimizing in {_optimizeWaitTimeMillis / 1000} seconds...", null, true);
            Thread.Sleep(_optimizeWaitTimeMillis);
            optimizer.Optimize();

            _interactiveShell.DrawSubtitleText(Text_Optimized);

            Thread.Sleep(1500);
            NativeMethods.MinimizeConsoleWindow();

            _interactiveShell.DrawContentText(Text_PressAnyKeyToGoBack, false); Console.ReadKey();
        }

        static void Command_OptimizeWithFlags(OptimizeConditions flags)
        {
            _interactiveShell.DrawTitle(AppName, $"Optimizing in {_optimizeWaitTimeMillis / 1000} seconds...", null, true);
            Thread.Sleep(_optimizeWaitTimeMillis);

            _interactiveShell.DrawContentText($"Optimizing (Flags: {flags})...");

            optimizer.Optimize(flags);

            _interactiveShell.DrawSubtitleText(Text_Optimized);

            if (!flags.HasFlag(OptimizeConditions.NoHide))
            {
                Thread.Sleep(1500);
                NativeMethods.MinimizeConsoleWindow();
            }

            _interactiveShell.DrawContentText(Text_PressAnyKeyToGoBack, false); Console.ReadKey();
        }

        static void Command_Restore()
        {
            _interactiveShell.DrawTitle(AppName, "Restoring", null, true);

            optimizer.Restore();

            _interactiveShell.DrawSubtitleText("\nRestored.");

            _interactiveShell.DrawContentText(Text_PressAnyKeyToGoBack, false); Console.ReadKey();
        }

        static void Command_ForceRestore()
        {
            _interactiveShell.DrawTitle(AppName, "Force restoring process priorities to Normal.", null, true);

            optimizer.ForceRestoreToNormal();

            _interactiveShell.DrawSubtitleText("\nRestored.");

            _interactiveShell.DrawContentText(Text_PressAnyKeyToGoBack, false); Console.ReadKey();
        }

    }
}
