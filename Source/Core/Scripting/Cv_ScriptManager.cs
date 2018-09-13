using System.IO;
using Caravel.Core.Entity;

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

        public long MaxProcessTimeMillis
        {
            get; set;
        }

        public abstract void AddGameBindings(string scriptObject, object exposedClass);

        internal abstract void VExecuteFile(string resource, Cv_Entity runningEntity = null);
        internal abstract void VExecuteString(string resource, string str, Cv_Entity runningEntity = null);
        internal abstract void VExecuteStream(string resource, Stream stream, Cv_Entity runningEntity = null);
        internal abstract bool VInitialize();
        internal abstract void OnUpdate(float time, float elapsedTime);
    }
}
