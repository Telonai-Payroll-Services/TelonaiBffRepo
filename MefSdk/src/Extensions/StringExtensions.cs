using System.Web;
using System;

namespace MeF.Client.Extensions
{
    public static partial class StringExtensions
    {
        /// <summary>
        /// Removes the line breaks.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string RemoveLineBreaks(this string value)
        {
            return value.Replace(Environment.NewLine, String.Empty);

        }

        /// <summary>
        /// Converts a string that has been HTML-encoded for HTTP transmission into a decoded string.
        /// </summary>
        /// <param name="s">The string to decode.</param>
        /// <returns>A decoded string</returns>
        public static string HtmlDecode(this string s)
        {
            return HttpUtility.HtmlDecode(s);
        }

        /// <summary>
        /// Converts a string to an HTML-encoded string.
        /// </summary>
        /// <param name="s">The string to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string HtmlEncode(this string s)
        {
            return HttpUtility.HtmlEncode(s);
        }

        /// <summary>
        /// Determines whether the specified the string contains any.
        /// </summary>
        /// <param name="theString">The string.</param>
        /// <param name="characters">The characters.</param>
        /// <returns>
        ///   <c>true</c> if the specified the string contains any; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsAny(this string theString, char[] characters)
        {
            foreach (char character in characters)
            {
                if (theString.Contains(character.ToString()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}