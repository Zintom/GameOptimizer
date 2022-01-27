using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Zintom.GameOptimizer;
using Zintom.GameOptimizer.Optimization;
using Zintom.GameOptimizer.ProcessIdentifiers;

namespace Tests;

[TestClass]
public class OptimizerTests
{
    private static readonly IntPtr _allCoreProcessorAffinity = BitMask.SetBitRange(0, 0, Environment.ProcessorCount);

    [TestMethod]
    public void GameAffinityIsDifferentToWhitelistedAndNonWhitelisted()
    {
        MockProcess notSpecialFirst = new MockProcess() { ProcessName = "RandomProgram69", PriorityClass = ProcessPriorityClass.Normal };
        MockProcess notSpecialSecond = new MockProcess() { ProcessName = "RandomProgram420", PriorityClass = ProcessPriorityClass.Normal };
        MockProcess gameProcess1 = new MockProcess() { ProcessName = "Game1", PriorityClass = ProcessPriorityClass.Normal };
        MockProcess whitelistedProcess1 = new MockProcess() { ProcessName = "SpecialBoy", PriorityClass = ProcessPriorityClass.Normal };

        // This represents the array of active processes on the system.
        List<IProcess> processes = new List<IProcess> { notSpecialFirst, notSpecialSecond, whitelistedProcess1, gameProcess1 };

        // This essentially mocks our whitelist file.
        List<string> whitelistedProcessNames = new List<string> { whitelistedProcess1.ProcessName };
        List<string> gameProcessNames = new List<string> { gameProcess1.ProcessName };

        MockSystem mockSystem = new MockSystem(processes, whitelistedProcessNames, gameProcessNames);

        Optimizer optimizer = new Optimizer(mockSystem, mockSystem, mockSystem, null, null, false);
        optimizer.Optimize(OptimizeConditions.OptimizeAffinity);

        Assert.IsTrue(notSpecialFirst.ProcessorAffinity == notSpecialSecond.ProcessorAffinity);
        Assert.IsTrue(notSpecialFirst.ProcessorAffinity == whitelistedProcess1.ProcessorAffinity);

        string notSpecialFirstAff = Convert.ToString((nint)notSpecialFirst.ProcessorAffinity, 2).PadLeft(64, '0');
        string gameAff = Convert.ToString((nint)gameProcess1.ProcessorAffinity, 2).PadLeft(64, '0');

        Assert.IsTrue(BitMask.LogicalBinaryComplement(notSpecialFirstAff, gameAff));
    }

    [TestMethod]
    public void WhitelistedProcessPriorityNotChanged_NonWhitelistedProcessPriorityChanged()
    {
        MockProcess notSpecialFirst = new MockProcess() { ProcessName = "RandomProgram69", PriorityClass = ProcessPriorityClass.Normal };
        MockProcess notSpecialSecond = new MockProcess() { ProcessName = "RandomProgram420", PriorityClass = ProcessPriorityClass.Normal };
        MockProcess notSpecialThird = new MockProcess() { ProcessName = "RandomProgram69420", PriorityClass = ProcessPriorityClass.Normal };
        MockProcess whitelistedProcess1 = new MockProcess() { ProcessName = "SpecialBoy", PriorityClass = ProcessPriorityClass.Normal };

        // This represents the array of active processes on the system.
        List<IProcess> processes = new List<IProcess> { notSpecialFirst, notSpecialSecond, whitelistedProcess1, notSpecialThird };

        // This essentially mocks our whitelist file.
        List<string> whitelistedProcessNames = new List<string> { whitelistedProcess1.ProcessName };
        List<string> gameProcessNames = new List<string> { "" };

        MockSystem mockSystem = new MockSystem(processes, whitelistedProcessNames, gameProcessNames);

        Optimizer optimizer = new Optimizer(mockSystem, mockSystem, mockSystem, null, null, false);
        optimizer.Optimize();

        Assert.IsTrue(whitelistedProcess1.PriorityClass == ProcessPriorityClass.Normal);
        Assert.IsTrue(notSpecialFirst.PriorityClass == ProcessPriorityClass.Idle);
        Assert.IsTrue(notSpecialSecond.PriorityClass == ProcessPriorityClass.Idle);
        Assert.IsTrue(notSpecialThird.PriorityClass == ProcessPriorityClass.Idle);
    }

    private class MockSystem : IActiveProcessProvider, IWhitelistedProcessIdentifierSource, IGameProcessIdentifierSource
    {
        private readonly List<IProcess> _processes;
        private readonly List<string> _whitelistedProcesses;
        private readonly List<string> _gameProcesses;

        internal MockSystem(List<IProcess> processes, List<string> whitelistedProcesses, List<string> gameProcesses)
        {
            _processes = processes;
            _whitelistedProcesses = whitelistedProcesses;
            _gameProcesses = gameProcesses;
        }

        public bool IsWhitelisted(IProcess process)
        {
            return _whitelistedProcesses.Contains(process.ProcessName);
        }

        public bool IsGame(IProcess process)
        {
            return _gameProcesses.Contains(process.ProcessName);
        }

        public void Refresh()
        {
        }

        IProcess[] IActiveProcessProvider.GetProcesses()
        {
            return _processes.ToArray();
        }
    }

    private class MockProcess : IProcess
    {
        private static int _mockProcessIdCounter = 10;

        public int Id { get; }

        public string ProcessName { get; set; }

        public IntPtr ProcessorAffinity { get; set; } = OptimizerTests._allCoreProcessorAffinity;

        public bool HasExited { get; set; } = false;

        public ProcessPriorityClass PriorityClass { get; set; }

        public void Kill()
        {
            throw new NotImplementedException();
        }

        public MockProcess()
        {
            Id = _mockProcessIdCounter++;
        }
    }
}