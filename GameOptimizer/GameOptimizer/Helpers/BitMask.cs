using System;

namespace Zintom.GameOptimizer
{
    /// <summary>
    /// Helper class for using Flags/Bitmasks on <see cref="Enum"/> types.
    /// </summary>
    /// <remarks><b>Note: </b>All bit related operations start at the least significant bit.</remarks>
    internal static class BitMask
    {

        /// <summary>
        /// Adds <paramref name="flag"/> to <paramref name="flags"/>.
        /// <para/>
        /// Equivalent to '<c>flags OR flag</c>'.
        /// </summary>
        internal static void SetFlag<TEnum>(ref this TEnum flags, TEnum flag) where TEnum : struct, Enum
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
        internal static void RemoveFlag<TEnum>(ref this TEnum flags, TEnum flag) where TEnum : struct, Enum
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (TEnum)(object)(flagsValue & ~flagValue);
        }

        /// <summary>
        /// Sets the bit at the given index.
        /// </summary>
        /// <param name="bitIndex">The index of the bit you wish to set.</param>
        internal static nint SetBit(nint val, int bitIndex)
        {
            nint mask = (nint)1 << bitIndex;
            return val |= mask;
        }

        /// <summary>
        /// Unsets the bit at the given index.
        /// </summary>
        /// <param name="bitIndex">The index of the bit you wish to unset.</param>
        internal static nint UnsetBit(nint val, int bitIndex)
        {
            nint mask = (nint)1 << bitIndex;
            return val &= ~mask;
        }

        /// <summary>
        /// Sets the range of bits from the <paramref name="startIndex"/> for <paramref name="count"/> binary digits.
        /// </summary>
        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit.</param>
        internal static nint SetBitRange(nint val, int startIndex, int count)
        {
            return ModifyBitRange(val, startIndex, count, true);
        }

        /// <summary>
        /// Unsets the range of bits from the <paramref name="startIndex"/> for <paramref name="count"/> binary digits.
        /// </summary>
        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit.</param>
        internal static nint UnsetBitRange(nint val, int startIndex, int count)
        {
            return ModifyBitRange(val, startIndex, count, false);
        }

        /// <summary>
        /// Sets or Unsets the range of bits from the <paramref name="startIndex"/> for <paramref name="count"/> binary digits.
        /// </summary>
        /// <param name="val">The value to modify.</param>
        /// <param name="startIndex">The index of the start bit.</param>
        /// <param name="setBits">Use <see langword="true"/> to set the range of bits, or <see langword="false"/> to unset them.</param>
        internal static nint ModifyBitRange(nint val, int startIndex, int count, bool setBits)
        {
            if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex), $"The {nameof(startIndex)} must be positive.");

            // The max number of bits in an uIntPtr (nuint)
            int nativeUintBitCount = Convert.ToString((nint)nuint.MaxValue, 2).Length;

            if (startIndex + count > nativeUintBitCount)
                throw new ArgumentOutOfRangeException($"({nameof(startIndex)} + {nameof(count)}) must be less than or equal to {nativeUintBitCount} (the number of bits in a native integer).");

            for (int i = startIndex; i < startIndex + count; i++)
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