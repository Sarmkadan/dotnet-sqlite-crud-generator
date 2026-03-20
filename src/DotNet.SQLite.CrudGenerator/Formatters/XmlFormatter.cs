#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace DotNet.SQLite.CrudGenerator.Formatters;

/// <summary>
/// Formats data to XML with customizable serialization options.
/// Supports both single objects and collections with proper namespace handling.
/// </summary>
public sealed class XmlFormatter : IFormatter
{
    private readonly XmlWriterSettings _writerSettings;
    private readonly XmlSerializerNamespaces _namespaces;

    public XmlFormatter(bool pretty = true, bool omitDeclaration = false)
    {
        _writerSettings = new XmlWriterSettings
        {
            Indent = pretty,
            IndentChars = "  ",
            OmitXmlDeclaration = omitDeclaration,
            Encoding = System.Text.Encoding.UTF8,
            ConformanceLevel = ConformanceLevel.Document
        };

        _namespaces = new XmlSerializerNamespaces();
        _namespaces.Add(string.Empty, string.Empty);
    }

    public string Format<T>(T data) where T : class
    {
        try
        {
            if (data is null)
                return "<null />";

            var serializer = new XmlSerializer(typeof(T));

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings))
            {
                serializer.Serialize(xmlWriter, data, _namespaces);
                return stringWriter.ToString();
            }
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to format data as XML: {ex.Message}", ex);
        }
    }

    public string Format<T>(IEnumerable<T> items) where T : class
    {
        try
        {
            var itemList = items?.ToList() ?? new List<T>();

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings))
            {
                xmlWriter.WriteStartElement("root");

                foreach (var item in itemList)
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(xmlWriter, item, _namespaces);
                }

                xmlWriter.WriteEndElement();
                xmlWriter.Flush();

                return stringWriter.ToString();
            }
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to format collection as XML: {ex.Message}", ex);
        }
    }

    public async Task<string> FormatAsync<T>(T data) where T : class
    {
        return await Task.FromResult(Format(data));
    }

    public async Task<string> FormatAsync<T>(IEnumerable<T> items) where T : class
    {
        return await Task.FromResult(Format(items));
    }

    public T? Parse<T>(string xml) where T : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xml))
            {
                return (T?)serializer.Deserialize(reader);
            }
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to parse XML: {ex.Message}", ex);
        }
    }

    public IEnumerable<T>? ParseCollection<T>(string xml) where T : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var items = new List<T>();
            var serializer = new XmlSerializer(typeof(T));

            if (doc.DocumentElement?.ChildNodes is not null)
            {
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node is XmlElement element)
                    {
                        using (var reader = new StringReader(element.OuterXml))
                        {
                            var item = (T?)serializer.Deserialize(reader);
                            if (item is not null)
                                items.Add(item);
                        }
                    }
                }
            }

            return items;
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to parse XML collection: {ex.Message}", ex);
        }
    }

    public string? GetXmlValue<T>(T data, string xpath) where T : class
    {
        try
        {
            var xml = Format(data);
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var node = doc.SelectSingleNode(xpath);
            return node?.InnerText;
        }
        catch
        {
            return null;
        }
    }

    public void AddXmlAttribute<T>(T data, string attributeName, string attributeValue) where T : class
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var property = properties.FirstOrDefault(p => p.Name == attributeName);

        if (property is not null && property.CanWrite)
        {
            property.SetValue(data, attributeValue);
        }
    }
}
