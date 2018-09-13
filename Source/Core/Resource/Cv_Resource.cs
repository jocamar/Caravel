using System.IO;

namespace Caravel.Core.Resource
{
    public interface Cv_Resource
    {
        string File
        {
            get; set;
        }

        Cv_ResourceData ResourceData
        {
            get; set;
        }

        bool VLoad(string resourceFile, Stream resourceStream, out int size, Cv_ResourceBundle bundle);
        bool VIsManuallyManaged();
    }
}