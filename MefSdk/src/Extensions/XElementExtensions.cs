using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MeF.Client.Extensions
{
    /// <summary>
    /// Extensions for <see cref="System.Xml.Linq.XElement"/>.
    /// </summary>
    public static class XElementExtensions
    {
        /// <summary>
        /// Returns the attribute value or null.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        public static string AttributeValueOrNull(this XElement element, string attributeName)
        {
            string s = null;
            var attr = element.Attribute(attributeName);
            if (attr != null) s = attr.Value;
            return s;
        }

        /// <summary>
        /// Returns the element value or null.
        /// </summary>
        /// <param name="elements">The elements.</param>
        public static string ElementValueOrNull(this IEnumerable<XElement> elements)
        {
            string s = null;
            var element = elements.FirstOrDefault();
            if (element != null) s = element.Value;
            return s;
        }

        /// <summary>
        /// Gets the <see cref="System.Xml.Linq.XElement"/>.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="name">The name.</param>
        public static XElement GetElement(this XNode node, XName name)
        {
            var element = node as XElement;
            if ((element != null) && (element.Name == name))
            {
                return element;
            }
            return null;
        }

        /// <summary>
        /// Determines whether the <see cref="System.Xml.Linq.XElement"/>
        /// has the specified <see cref="System.Xml.Linq.XName"/>.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// 	<c>true</c> when the element has the name; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasElementName(this XElement element, XName name)
        {
            return (element.Name == name);
        }

        /// <summary>
        /// Determines whether the <see cref="System.Xml.Linq.XNode"/>
        /// has the specified <see cref="System.Xml.Linq.XName"/>.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// 	<c>true</c> when the node has the name; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasElementName(this XNode node, XName name)
        {
            var element = node as XElement;
            return (element != null) && (element.Name == name);
        }

        /// <summary>
        /// Determines whether the specified node is <see cref="System.Xml.Linq.XElement"/>.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// 	<c>true</c> if the specified node is element; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsElement(this XNode node)
        {
            var element = node as XElement;
            return (element != null);
        }

        public static bool Exists(this IEnumerable<XElement> elements, string elementName)
        {
            return elements.Any(x => x.Name == elementName);
        }

        public static IEnumerable<XElement> GetChildren(this XElement element)
        {
            return element != null ? element.Elements() : null;
        }

        public static string GetValueOrEmpty(this XElement element)
        {
            return element != null ? element.Value : string.Empty;
        }

        public static string GetAttributeOrEmpty(this XElement element, string attribute)
        {
            return element != null ? element.Attribute(attribute).GetValueOrEmpty() : string.Empty;
        }

        /// <summary>
        /// Gets the element value or default.
        /// </summary>
        /// <typeparam name="T">The Data Type that you want returned.</typeparam>
        /// <param name="node">The node.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <returns>This function will return the element value for the specified node and name space or the Data Types default value.</returns>
        public static T GetElementValueOrDefault<T>(XNode node, String elementName, String nameSpace)
        {
            if (node == null) return default(T);

            XElement element = ((XElement)node).Element(XName.Get(elementName, nameSpace));
            if (element == null) return default(T);
            return (T)Convert.ChangeType(element.Value, typeof(T));
        }

        /// <summary>
        /// Gets the element value or default.
        /// </summary>
        /// <typeparam name="T">The Data Type that you want returned.</typeparam>
        /// <param name="node">The node.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <returns>This function will return the element value for the specified node and name space or the Data Types default value.</returns>
        public static T GetElementValueOrDefault<T>(XNode node, String elementName)
        {
            if (node == null) return default(T);

            XElement element = ((XElement)node).Element(elementName);
            if (element == null) return default(T);
            return (T)Convert.ChangeType(element.Value, typeof(T));
        }
    }
}