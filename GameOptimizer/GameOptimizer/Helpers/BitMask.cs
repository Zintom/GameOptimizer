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
        public static ulong SetBit(this ulong val, int bitIndex)
        {
            ulong mask = 1ul << bitIndex;
            return val |= mask;
        }

        /// <summary>
        /// Unsets the bit at the given index.
        /// </summary>
        /// <param name="bitIndex">The index of the bit you wish to unset <b>(evaluated right to left)</b>.</param>
        public static ulong UnsetBit(this ulong val, int bitIndex)
        {
            ulong mask = 1ul << bitIndex;
            return val &= ~mask;
        }

        /// <summary>
        /// Sets the range of bits between the <paramref name="startIndex"/> and <paramref name="endIndex"/>.
        /// </summary>
        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit <b>(evaluated right to left)</b>.</param>
        /// <param name="endIndex">The index of the end bit <b>(evaluated right to left)</b>.</param>
        public static ulong SetBitRange(this ulong val, int startIndex, int endIndex)
        {
            return ModifyBitRange(val, startIndex, endIndex, true);
        }

        /// <summary>
        /// Unsets the range of bits between the <paramref name="startIndex"/> and <paramref name="endIndex"/>.
        /// </summary>
        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit <b>(evaluated right to left)</b>.</param>
        /// <param name="endIndex">The index of the end bit <b>(evaluated right to left)</b>.</param>
        public static ulong UnsetBitRange(this ulong val, int startIndex, int endIndex)
        {
            return ModifyBitRange(val, startIndex, endIndex, false);
        }

        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit <b>(evaluated right to left)</b>.</param>
        /// <param name="endIndex">The index of the end bit <b>(evaluated right to left)</b>.</param>
        /// <param name="setBits">Default behaviour is to unset the bits, set to <see langword="true"/> to set bits.</param>
        private static ulong ModifyBitRange(this ulong val, int startIndex, int endIndex, bool setBits)
        {
            if (startIndex < 0) throw new ArgumentOutOfRangeException($"The {nameof(startIndex)} must be positive.");
            if (endIndex > 64) throw new ArgumentOutOfRangeException($"The {nameof(endIndex)} must be less than or equal to 64(the number of bits in a UInt64).");

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