﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZintomShellHelper;

namespace GameOptimizer
{
    partial class Program
    {

        static void Command_OptimizeNoFlags()
        {
            MenuManager.DrawTitle(AppName, $"  Optimizing in {OptimizeWaitTimeMillis / 1000} seconds...", true);
            Thread.Sleep(OptimizeWaitTimeMillis);
            optimizer.Optimize();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Optimized.");

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

        static void Command_OptimizeWithFlags(OptimizeFlags flags)
        {
            MenuManager.DrawTitle(AppName, $"  Optimizing in {OptimizeWaitTimeMillis / 1000} seconds...", true);
            Thread.Sleep(5000);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  Optimizing (Flags: {flags})...");

            optimizer.Optimize(flags);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Optimized.");

            Thread.Sleep(1500);

            if (!flags.HasFlag(OptimizeFlags.NoHide))
                ShowWindow(GetConsoleWindow(), SW_MINIMIZE);

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

        static void Command_Restore()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            MenuManager.DrawTitle(AppName, "  Restoring", true);

            optimizer.Restore();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Restored to normal priority.");

            Console.Write("  Press any key to go back."); Console.ReadKey();
        }

    }
}
