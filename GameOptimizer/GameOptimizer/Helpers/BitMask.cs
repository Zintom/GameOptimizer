using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOptimizer
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
        public static int SetBit(this int val, int bitIndex)
        {
            int mask = 1 << bitIndex;
            return val |= mask;
        }

        /// <summary>
        /// Unsets the bit at the given index.
        /// </summary>
        /// <param name="bitIndex">The index of the bit you wish to unset <b>(evaluated right to left)</b>.</param>
        public static int UnsetBit(this int val, int bitIndex)
        {
            int mask = 1 << bitIndex;
            return val &= ~mask;
        }

        /// <summary>
        /// Sets the range of bits between the <paramref name="startIndex"/> and <paramref name="endIndex"/>.
        /// </summary>
        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit <b>(evaluated right to left)</b>.</param>
        /// <param name="endIndex">The index of the end bit <b>(evaluated right to left)</b>.</param>
        public static int SetBitRange(this int val, int startIndex, int endIndex)
        {
            if (startIndex < 0) throw new IndexOutOfRangeException("The start index must be positive.");
            if (endIndex > 31) throw new IndexOutOfRangeException("The end index must be less than 32(the number of bits in an Int32).");

            for (int i = startIndex; i < endIndex; i++)
            {
                val = val.SetBit(i);
            }

            return val;
        }

        /// <summary>
        /// Unsets the range of bits between the <paramref name="startIndex"/> and <paramref name="endIndex"/>.
        /// </summary>
        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit <b>(evaluated right to left)</b>.</param>
        /// <param name="endIndex">The index of the end bit <b>(evaluated right to left)</b>.</param>
        public static int UnsetBitRange(this int val, int startIndex, int endIndex)
        {
            if (startIndex < 0) throw new IndexOutOfRangeException("The start index must be positive.");
            if (endIndex > 31) throw new IndexOutOfRangeException("The end index must be less than 32(the number of bits in an Int32).");

            for (int i = startIndex; i < endIndex; i++)
            {
                val = val.UnsetBit(i);
            }

            return val;
        }

    }
}