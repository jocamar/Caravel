using System.IO;
using System.Xml;

namespace Caravel.Core.Resource
{
    public class Cv_ScriptResource : Cv_Resource
    {
        public string File{ get; set; }

         public Cv_ResourceData ResourceData { get; set; }

        public bool VLoad(Stream resourceStream, out int size, Cv_ResourceBundle bundle)
        {
            size = 0;

            return true;
        }

        public bool VIsManuallyManaged()
        {
            return true;
        }
    }
}