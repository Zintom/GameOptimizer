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

        /// <summary>
        /// Determines whether two binary strings are the logical bitwise complement of each other.
        /// </summary>
        /// <remarks>
        /// The binary strings must be the same length as this does a bit-for-bit comparison.
        /// Leading zeros that exist on both strings are trimmed.
        /// </remarks>
        internal static bool LogicalBinaryComplement(ReadOnlySpan<char> binaryString1, ReadOnlySpan<char> binaryString2)
        {
            // Both the binary strings must be of the same length to compare bit for bit.
            if (binaryString1.Length != binaryString2.Length) throw new Exception("Binary strings were not the same length.");

            // Work out the number of leading zeros for each string.
            int string1LeadingZeros = 0;
            for (int i = 0; i < binaryString1.Length; i++)
            {
                if (binaryString1[i] != '0') break;
                string1LeadingZeros++;
            }

            int string2LeadingZeros = 0;
            for (int i = 0; i < binaryString2.Length; i++)
            {
                if (binaryString2[i] != '0') break;
                string2LeadingZeros++;
            }

            // We want to trim any leading zeros that exist in BOTH strings.
            // So we compare the number of leading zeros in each string,
            // the string with the least leading zeros governs where each string will be trimmed.
            // i.e if string one is 000111 and string two is 001111, string two beats string one and the trim is performed at index 2.
            int leadingZerosSliceIndex;
            if (string1LeadingZeros <= string2LeadingZeros)
            {
                leadingZerosSliceIndex = string1LeadingZeros;
            }
            else
            {
                leadingZerosSliceIndex = string2LeadingZeros;
            }

            binaryString1 = binaryString1[leadingZerosSliceIndex..];
            binaryString2 = binaryString2[leadingZerosSliceIndex..];

            for (int i = 0; i < binaryString1.Length; i++)
            {
                // Ensure that each character we are checking is a '0' or '1'.
                bool is1Digit = int.TryParse(binaryString1.Slice(i, 1), System.Globalization.NumberStyles.None, null, out int digit1);
                bool is2Digit = int.TryParse(binaryString2.Slice(i, 1), System.Globalization.NumberStyles.None, null, out int digit2);

                if (!is1Digit || !is2Digit) throw new Exception("The strings given must consist of \"0\"'s or \"1\"'s only.");

                // We cannot use bitwise complement as it inverts the full width of the number, not just the first bit.
                // The solution below does the same thing as the bitwise complement check, however, it only checks the digit we are interested in.
                // If the digits are the same then they are not a complement, so fail.
                if (digit1 == digit2) return false;
            }

            return true;
        }
    }
}