using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Utility
{
    /// <summary>
    /// Detects encodings.
    /// </summary>
    public class EncodedStringConverter
    {
        /// <summary>
        /// Creates new EncodedStringConverter instance.
        /// </summary>
        public EncodedStringConverter()
        {

        }

        /// <summary>
        /// Returns converter string from specified byte array.
        /// </summary>
        /// <param name="chars">Character array.</param>
        /// <param name="fallback">Fallback encoding when detecting of encoding fails.</param>
        /// <returns>Converted string.</returns>
        public string GetConvertedString(byte[] chars, Encoding fallback)
        {
            if (chars == null)
                throw new ArgumentNullException(nameof(chars));

            // check for UTF-32
            if (chars.Length >= 4)
            {
                // little endian: FF FE 00 00
                if (chars[0] == 0xFF
                 && chars[1] == 0xFE
                 && chars[2] == 0x00
                 && chars[3] == 0x00)
                {
                    return Encoding.UTF32.GetString(chars);
                }
            }
            // check for UTF-16
            if (chars.Length >= 2)
            {
                // little endian: FF FE
                if (chars[0] == 0xFF
                 && chars[1] == 0xFE)
                {
                    return Encoding.Unicode.GetString(chars);
                }
                // big endian: FE FF
                else if (chars[0] == 0xFE
                      && chars[1] == 0xFF)
                {
                    return Encoding.BigEndianUnicode.GetString(chars);
                }
            }

            // TODO:
            // Gamebryo engine uses Windows-1252 a lot, probably
            // need to add this encoding too.

            return fallback.GetString(chars);
        }
    }
}
