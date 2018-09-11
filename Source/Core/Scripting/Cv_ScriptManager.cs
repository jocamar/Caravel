using System.IO;

namespace Caravel.Core.Scripting
{
    public abstract class Cv_ScriptManager
    {
        public static Cv_ScriptManager Instance
        {
            get; protected set;
        }

        public abstract bool DebuggerEnabled
        {
            get; set;
        }

        internal abstract void VExecuteFile(string resource);
        internal abstract void VExecuteString(string str);
        internal abstract void VExecuteStream(Stream stream);
        internal abstract bool VInitialize();
    }
}
