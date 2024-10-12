using System.Xml.Linq;

namespace MeF.Client.Extensions
{
    public static class XAttributeExtensions
    {
        /// <summary>
        /// Gets the value or empty.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        public static string GetValueOrEmpty(this XAttribute attribute)
        {
            return attribute != null ? attribute.Value : string.Empty;
        }
    }
}