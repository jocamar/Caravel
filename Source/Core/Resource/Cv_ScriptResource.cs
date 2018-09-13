using System.IO;
using System.Xml;
using Caravel.Core.Scripting;

namespace Caravel.Core.Resource
{
    public class Cv_ScriptResource : Cv_Resource
    {
        public class Cv_ScriptData : Cv_ResourceData
        {
            public string Code;

            public long Size
            {
                get
                {
                    return 0;
                }
            }
        }

        public string File{ get; set; }

        public Cv_ResourceData ResourceData { get; set; }

        public bool VLoad(string resourceFile, Stream resourceStream, out int size, Cv_ResourceBundle bundle)
        {
            size = 0;

            using (StreamReader reader = new StreamReader(resourceStream))
            {
                resourceStream.Position = 0;
                var code = reader.ReadToEnd();
                var resData = new Cv_ScriptData();
				resData.Code = code;
				ResourceData = resData;
            }

            return true;
        }

        public bool VIsManuallyManaged()
        {
            return true;
        }

        public void RunScript()
        {
            Cv_ScriptManager.Instance.VExecuteString(File, ((Cv_ScriptData)ResourceData).Code);
        }
    }
}