using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zintom.GameOptimizer
{
    /// <summary>
    /// Helper class for using Flags/Bitmasks on <see cref="Enum"/> types.
    /// </summary>
    public static class BitMask
    {

        /// <summary>
        /// Adds <paramref name="flag"/> to <paramref name="flags"/>.
        /// <para/>
        /// Equivalent to '<c>flags OR flag</c>'.
        /// </summary>
        public static void SetFlag<TEnum>(ref this TEnum flags, TEnum flag) where TEnum : struct, Enum
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (TEnum)(object)(flagsValue | flagValue);
        }

        /// <summary>
        /// Removes <paramref name="flag"/> from <paramref name="flags"/>.
        /// <para/>
        /// Equivalent to '<c>flags AND NOT flag</c>'.
        /// </summary>
        public static void RemoveFlag<TEnum>(ref this TEnum flags, TEnum flag) where TEnum : struct, Enum
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (TEnum)(object)(flagsValue & ~flagValue);
        }

        /// <summary>
        /// Sets the bit at the given index.
        /// </summary>
        /// <param name="bitIndex">The index of the bit you wish to set <b>(evaluated right to left)</b>.</param>
        public static nuint SetBit(this nuint val, int bitIndex)
        {
            nuint mask = (nuint)1 << bitIndex;
            return val |= mask;
        }

        /// <summary>
        /// Unsets the bit at the given index.
        /// </summary>
        /// <param name="bitIndex">The index of the bit you wish to unset <b>(evaluated right to left)</b>.</param>
        public static nuint UnsetBit(this nuint val, int bitIndex)
        {
            nuint mask = (nuint)1 << bitIndex;
            return val &= ~mask;
        }

        /// <summary>
        /// Sets the range of bits between the <paramref name="startIndex"/> and <paramref name="endIndex"/>.
        /// </summary>
        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit <b>(evaluated right to left)</b>.</param>
        /// <param name="endIndex">The index of the end bit <b>(evaluated right to left)</b>.</param>
        public static nuint SetBitRange(this nuint val, int startIndex, int endIndex)
        {
            return ModifyBitRange(val, startIndex, endIndex, true);
        }

        /// <summary>
        /// Unsets the range of bits between the <paramref name="startIndex"/> and <paramref name="endIndex"/>.
        /// </summary>
        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit <b>(evaluated right to left)</b>.</param>
        /// <param name="endIndex">The index of the end bit <b>(evaluated right to left)</b>.</param>
        public static nuint UnsetBitRange(this nuint val, int startIndex, int endIndex)
        {
            return ModifyBitRange(val, startIndex, endIndex, false);
        }

        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit <b>(evaluated right to left)</b>.</param>
        /// <param name="endIndex">The index of the end bit <b>(evaluated right to left)</b>.</param>
        /// <param name="setBits">Default behaviour is to unset the bits, set to <see langword="true"/> to set bits.</param>
        private static nuint ModifyBitRange(this nuint val, int startIndex, int endIndex, bool setBits)
        {
            if (startIndex < 0) throw new ArgumentOutOfRangeException($"The {nameof(startIndex)} must be positive.");

            // The max number of bits in an IntPtr (nint)
            int IntPtrBitCount = Convert.ToString(nint.MaxValue, 2).Length;

            if (endIndex > IntPtrBitCount)
                throw new ArgumentOutOfRangeException($"The {nameof(endIndex)} must be less than or equal to {IntPtrBitCount}(the number of bits in a native integer).");

            for (int i = startIndex; i < endIndex; i++)
            {
                if (setBits)
                    val = SetBit(val, i);
                else
                    val = UnsetBit(val, i);
            }

            return val;
        }

    }
}