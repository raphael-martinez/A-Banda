using UnityEngine;
using System.Xml.Serialization;
using System.Collections;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;

namespace Playmove
{
    public class PYXML
    {
        public static XmlDocument GetXmlDocument(string xmlContent)
        {
            XmlDocument xml = new XmlDocument();

            byte[] buffer = System.Text.ASCIIEncoding.UTF8.GetBytes(xmlContent);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                xml.Load(stream);
            }

            return xml;
        }

        public static Stream GetXmlStream(string xmlContent)
        {
            byte[] buffer = System.Text.ASCIIEncoding.UTF8.GetBytes(xmlContent);
            MemoryStream stream = new MemoryStream(buffer);

            return stream;
        }

        public static void Serializer(string path, object obj)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (XmlWriter stream = XmlWriter.Create(new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite), settings))
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(stream, obj);
            }
        }

        public static T Deserializer<T>(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stream);
            }
        }
        public static T DeserializerFromContent<T>(string xmlContent)
        {
            byte[] buffer = System.Text.ASCIIEncoding.UTF8.GetBytes(xmlContent);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stream);
            }
        }
    }
}