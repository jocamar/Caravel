using System.IO;
using System.Xml;
using Caravel.Core.Scripting;
using MoonSharp.Interpreter;

namespace Caravel.Core.Resource
{
    public class Cv_ScriptResource : Cv_Resource
    {
        public string File{ get; set; }

        public Cv_ResourceData ResourceData { get; set; }

        public bool VLoad(string resourceFile, Stream resourceStream, out int size, Cv_ResourceBundle bundle)
        {
            size = 0;

            Cv_ScriptManager.Instance.VExecuteStream(resourceStream);

            return true;
        }

        public bool VIsManuallyManaged()
        {
            return true;
        }
    }
}