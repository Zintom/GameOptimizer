using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zintom.GameOptimizer;

namespace Tests
{
    [TestClass]
    public class BitMaskTests
    {

        [TestMethod]
        public void SetBitRange_FirstHalfSet()
        {
            nint value = 0;

            // Set the first half of the binary digits to 1, and leave the second half as 0.
            value = BitMask.SetBitRange(value, 0, 32);

            Assert.IsTrue((ulong)value == 0b_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111_1111_1111_1111_1111);
        }

        [TestMethod]
        public void SetBitRange_SecondHalfSet()
        {
            nint value = 0;

            // Set the second half of the binary digits to 1, and leave the first half as 0.
            value = BitMask.SetBitRange(value, 32, 32);

            Assert.IsTrue((ulong)value == 0b_1111_1111_1111_1111_1111_1111_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000);
        }

        [TestMethod]
        public void SetBitRange_AllSet()
        {
            nint value = 0;

            // Set all the binary digits to 1.
            value = BitMask.SetBitRange(value, 0, 64);

            Assert.IsTrue((ulong)value == 0b_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111);
        }

        [TestMethod]
        public void SetBitRange_SecondShortSet()
        {
            nint value = 0;

            value = BitMask.SetBitRange(value, 16, 16);

            Assert.IsTrue((ulong)value == 0b_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111_0000_0000_0000_0000);
        }

        [TestMethod]
        public void UnsetBitRange_FirstHalfUnset()
        {
            nint value = (nint)nuint.MaxValue;

            // Set the first half of the binary digits to 0, and leave the second half as 1.
            value = BitMask.UnsetBitRange(value, 0, 32);

            Assert.IsTrue((ulong)value == 0b_1111_1111_1111_1111_1111_1111_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000);
        }

        [TestMethod]
        public void UnsetBitRange_SecondHalfUnset()
        {
            nint value = (nint)nuint.MaxValue;

            // Set the second half of the binary digits to 0, and leave the first half as 1.
            value = BitMask.UnsetBitRange(value, 32, 32);

            Assert.IsTrue((ulong)value == 0b_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111_1111_1111_1111_1111);
        }

        [TestMethod]
        public void UnsetBitRange_AllUnset()
        {
            nint value = (nint)nuint.MaxValue;

            // Set all the binary digits to 0.
            value = BitMask.UnsetBitRange(value, 0, 64);

            Assert.IsTrue((ulong)value == 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000);
        }

        [TestMethod]
        public void UnsetBitRange_SecondShortUnset()
        {
            nint value = (nint)nuint.MaxValue;

            value = BitMask.UnsetBitRange(value, 16, 16);

            Assert.IsTrue((ulong)value == 0b_1111_1111_1111_1111_1111_1111_1111_1111_0000_0000_0000_0000_1111_1111_1111_1111);
        }
    }
}
