using System.Xml;

namespace Caravel.Core
{
    public class Cv_ScriptResource : Cv_Resource
    {
        public override bool VLoad(out int size)
        {
            size = 0;

            return true;
        }
    }
}