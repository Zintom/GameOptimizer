﻿using System;
using System.Threading;
using ZintomShellHelper;

namespace Zintom.GameOptimizer
{
    partial class Program
    {

        static void Command_OptimizeNoFlags()
        {
            MenuManager.DrawTitle(AppName, $"  Optimizing in {_optimizeWaitTimeMillis / 1000} seconds...", true);
            Thread.Sleep(_optimizeWaitTimeMillis);
            optimizer.Optimize();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Optimized.");

            Thread.Sleep(1500);
            ConsoleWindowControl.MinimizeConsoleWindow();

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

        static void Command_OptimizeWithFlags(OptimizeFlags flags)
        {
            MenuManager.DrawTitle(AppName, $"  Optimizing in {_optimizeWaitTimeMillis / 1000} seconds...", true);
            Thread.Sleep(_optimizeWaitTimeMillis);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  Optimizing (Flags: {flags})...");

            optimizer.Optimize(flags);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Optimized.");

            if (!flags.HasFlag(OptimizeFlags.NoHide))
            {
                Thread.Sleep(1500);
                ConsoleWindowControl.MinimizeConsoleWindow();
            }

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

        static void Command_Restore()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            MenuManager.DrawTitle(AppName, "  Restoring", true);

            optimizer.Restore();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Restored.");

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

        static void Command_ForceRestore()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            MenuManager.DrawTitle(AppName, "  Force restoring process priorities to Normal.", true);

            optimizer.ForceRestoreToNormal();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Restored.");

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

    }
}
