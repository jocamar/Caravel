using System.Xml;

namespace Caravel.Core
{
    public class Cv_XmlResource : Cv_Resource
    {
        public XmlDocument Document {
            get; internal set;
        }

        public XmlElement RootNode {
            get; internal set;
        }

        public override bool VLoad(out int size)
        {
            var doc = new XmlDocument();
            doc.Load(File);

            Document = doc;
            RootNode = (XmlElement) doc.FirstChild;
            size = doc.OuterXml.Length;

            return true;
        }
    }
}