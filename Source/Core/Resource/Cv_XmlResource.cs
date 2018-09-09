using System.IO;
using System.Xml;
using Caravel.Debugging;

namespace Caravel.Core.Resource
{
    public struct Cv_XmlResource : Cv_Resource
    {
        public class Cv_XmlData : Cv_ResourceData
        {
            public XmlDocument Document {
                get; internal set;
            }

            public XmlElement RootNode {
                get; internal set;
            }

            public long Size
            {
                get
                {
                    return Document.OuterXml.Length;
                }
            }
        }
        
        public string File { get; set; }

        public Cv_ResourceData ResourceData { get; set; }

        public bool VLoad(string resourceFile, Stream resourceStream, out int size, Cv_ResourceBundle bundle)
        {
            if (resourceStream == null)
            {
                Cv_Debug.Error("Invalid resource stream.");
                size = 0;
                return false;
            }

            resourceStream.Position = 0;

            var doc = new XmlDocument();
            doc.Load(resourceStream);

            var newXmlData = new Cv_XmlData();
            newXmlData.Document = doc;
            newXmlData.RootNode = (XmlElement) doc.FirstChild;

            ResourceData = newXmlData;

            size = doc.OuterXml.Length;
            resourceStream.Dispose();
            return true;
        }

        public bool VIsManuallyManaged()
        {
            return true;
        }
    }
}