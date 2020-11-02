using System;
using System.Threading;
using Zintom.GameOptimizer.Helpers;
using ZintomShellHelper;

namespace Zintom.GameOptimizer
{
    partial class Program
    {
        private const string Text_PressAnyKeyToGoBack = "  Press any key to go back.";
        private const string Text_Optimized = "\n  Optimized.";

        static void Command_OptimizeNoFlags()
        {
            MenuManager.DrawTitle(AppName, $"  Optimizing in {_optimizeWaitTimeMillis / 1000} seconds...", true);
            Thread.Sleep(_optimizeWaitTimeMillis);
            optimizer.Optimize();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Text_Optimized);

            Thread.Sleep(1500);
            NativeMethods.MinimizeConsoleWindow();

            Console.Write(Text_PressAnyKeyToGoBack); Console.ReadKey();
        }

        static void Command_OptimizeWithFlags(OptimizeConditions flags)
        {
            MenuManager.DrawTitle(AppName, $"  Optimizing in {_optimizeWaitTimeMillis / 1000} seconds...", true);
            Thread.Sleep(_optimizeWaitTimeMillis);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  Optimizing (Flags: {flags})...");

            optimizer.Optimize(flags);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Text_Optimized);

            if (!flags.HasFlag(OptimizeConditions.NoHide))
            {
                Thread.Sleep(1500);
                NativeMethods.MinimizeConsoleWindow();
            }

            Console.Write(Text_PressAnyKeyToGoBack); Console.ReadKey();
        }

        static void Command_Restore()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            MenuManager.DrawTitle(AppName, "  Restoring", true);

            optimizer.Restore();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Restored.");

            Console.Write(Text_PressAnyKeyToGoBack); Console.ReadKey();
        }

        static void Command_ForceRestore()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            MenuManager.DrawTitle(AppName, "  Force restoring process priorities to Normal.", true);

            optimizer.ForceRestoreToNormal();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Restored.");

            Console.Write(Text_PressAnyKeyToGoBack); Console.ReadKey();
        }

    }
}
