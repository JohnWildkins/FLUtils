using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FLUtils
{
    public static class StringUtils
    {
        private static uint[] createIDTable;
        private static uint[] createFactionIDTable;

        public static string SplitPascalCase(this string convert)
        {
            return Regex.Replace(Regex.Replace(convert, "(\\P{Ll})(\\P{Ll}\\p{Ll})", "$1 $2"), "(\\p{Ll})(\\P{Ll})", "$1 $2");
        }

        public static int CountOccurrences(this string val, string match)
        {
            return Regex.Matches(val, match, RegexOptions.IgnoreCase).Count;
        }

        public static string Truncate(this string s, int length)
        {
            if (string.IsNullOrEmpty(s) || length <= 0)
                return string.Empty;
            return s.Length <= length ? s : s.Substring(0, length) + "...";
        }

        public static string TruncateAtWord(this string s, int length)
        {
            return s != null && s.Length >= length && s.IndexOf(" ", length, StringComparison.Ordinal) != -1 ? s.Substring(0, s.IndexOf(" ", length, StringComparison.Ordinal)) : s;
        }

        public static bool IsEmailAddress(this string email)
        {
            return Regex.Match(email, "^[a-zA-Z][\\w\\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\\w\\.-]*[a-zA-Z0-9]\\.[a-zA-Z][a-zA-Z\\.]*[a-zA-Z]$").Success;
        }

        public static bool IsDateTime(this string data, string format)
        {
            return DateTime.TryParseExact(data, format, (IFormatProvider)CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _);
        }

        public static uint CreateId(string nickname)
        {
            const int LOGICAL_BITS = 30;
            const int PHYSICAL_BITS = 32;
            if (StringUtils.createIDTable == null)
            {
                StringUtils.createIDTable = new uint[256];
                for (uint index1 = 0; index1 < 256U; ++index1)
                {
                    uint num = index1;
                    for (uint index2 = 0; index2 < 8U; ++index2)
                        num = ((int)num & 1) == 1 ? num >> 1 ^ 671105024U : num >> 1;
                    StringUtils.createIDTable[(int)index1] = num;
                }
            }
            byte[] bytes = Encoding.ASCII.GetBytes(nickname.ToLowerInvariant());
            // Calculate the hash.
            uint hash = 0;
            for (int i = 0; i < nickname.Length; i++)
                hash = (hash >> 8) ^ createIDTable[(byte)hash ^ nickname[i]];
            // b0rken because byte swapping is not the same as bit reversing, but 
            // that's just the way it is; two hash bits are shifted out and lost
            hash = (hash >> 24) | ((hash >> 8) & 0x0000FF00) | ((hash << 8) & 0x00FF0000) | (hash << 24);
            hash = (hash >> (PHYSICAL_BITS - LOGICAL_BITS)) | 0x80000000;
            return hash;
        }

        public static uint CreateFactionId(string nickname)
        {
            if (StringUtils.createFactionIDTable == null)
            {
                StringUtils.createFactionIDTable = new uint[256];
                for (uint index1 = 0; index1 < 256U; ++index1)
                {
                    uint num = index1 << 8;
                    for (uint index2 = 0; index2 < 8U; ++index2)
                        num = (((int)num & 32768) == 32768 ? (uint)((int)num << 1 ^ 4129) : num << 1) & (uint)ushort.MaxValue;
                    StringUtils.createFactionIDTable[(int)index1] = num;
                }
            }
            byte[] bytes = Encoding.ASCII.GetBytes(nickname.ToLowerInvariant());
            uint factionId = (uint)ushort.MaxValue;
            for (uint index = 0; (long)index < (long)bytes.Length; ++index)
                factionId = (factionId & 65280U) >> 8 ^ StringUtils.createFactionIDTable[(int)factionId & (int)byte.MaxValue ^ (int)bytes[(int)index]];
            return factionId;
        }
    }
}
