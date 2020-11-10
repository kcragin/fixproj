using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace fixproj
{
    internal static class Extensions
    {
        /// <summary>
        /// Checks if string is a member of input array.
        /// </summary>
        /// <param name="subject">String.</param>
        /// <param name="suffixes">Array of string.</param>
        /// <returns>Boolean.</returns>
        internal static bool EndsWithAnyOf(this string subject, params string[] suffixes) => suffixes.Any(subject.EndsWith);

        /// <summary>
        /// Finds element by local name.
        /// </summary>
        /// <param name="element">XElement.</param>
        /// <param name="localName">Local name.</param>
        /// <returns>A collection of XElement.</returns>
        internal static IEnumerable<XElement> ElementsByLocalName(this XElement element, string localName) => element.Elements().Where(x => x.Name.LocalName == localName);

        /// <summary>
        /// Checks if node contains value.
        /// </summary>
        /// <param name="element">XElement.</param>
        /// <returns>Boolean.</returns>
        internal static bool HasNoContent(this XElement element) => string.IsNullOrWhiteSpace(element.Value) && !element.HasElements;

        /// <summary>
        /// Makes element as empty node.
        /// </summary>
        /// <param name="element">XElement.</param>
        internal static void MakeEmpty(this XElement element) => element.ReplaceNodes(null);

        /// <summary>
        /// Checks if attribute value contains provided extension.
        /// </summary>
        /// <param name="element">XElement.</param>
        /// <param name="extension">Extension.</param>
        /// <returns>An attribute value.</returns>
        internal static string IfAttributesContainExtension(this XElement element, string extension) => element.Attributes().FirstOrDefault(x => x.Value.EndsWith(extension))?.Value;

        /// <summary>
        /// Finds attribute value by name.
        /// </summary>
        /// <param name="element">XElement.</param>
        /// <param name="attributeName">Attribute name.</param>
        /// <returns>An attribute value.</returns>
        internal static string AttributeValueByName(this XElement element, string attributeName)
        {
            if (element == null || !element.HasAttributes || string.IsNullOrWhiteSpace(element.Attribute(attributeName)?.Value))
            {
                return null;
            }

            return element.Attribute(attributeName)?.Value;
        }
    }
}