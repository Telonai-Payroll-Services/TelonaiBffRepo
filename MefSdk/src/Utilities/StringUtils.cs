namespace MeF.Client.Util
{
    /// <summary>
    /// Represents utility operations for strings.
    /// </summary>
    internal static class StringUtils
    {
        public static bool IsBlank(string str)
        {
            return (str == null || str.Trim().Length == 0);
        }

        public static bool IsNotBlank(string str)
        {
            return !IsBlank(str);
        }

        public static bool IsEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string NullToEmpty(string str)
        {
            return (str == null ? "" : str);
        }

        public static string EmptyToNull(string str)
        {
            return (str == null ? null : (str.Length == 0 ? null : str));
        }

        public static bool Equals(string str1, string str2)
        {
            bool sNull1 = string.IsNullOrEmpty(str1);
            bool sNull2 = string.IsNullOrEmpty(str2);
            if (sNull1 && sNull2)
            {
                return true;
            }
            if (!sNull1 && !sNull2)
            {
                return str1.Equals(str2);
            }
            return false;
        }

        public static bool EqualsIgnoreCase(string str1, string str2)
        {
            bool sNull1 = string.IsNullOrEmpty(str1);
            bool sNull2 = string.IsNullOrEmpty(str2);
            if (sNull1 && sNull2)
            {
                return true;
            }
            if (!sNull1 && !sNull2)
            {
                string sUpper1 = str1.ToUpper();
                string sUpper2 = str2.ToUpper();
                return sUpper1.Equals(sUpper2);
            }
            return false;
        }
    }
}