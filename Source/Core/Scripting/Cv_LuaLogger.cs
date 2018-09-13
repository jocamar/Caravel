using Caravel.Debugging;

namespace Caravel.Core.Scripting
{
    public class Cv_LuaLogger
    {
        public void Info(string val)
        {
            Cv_Debug.Log("LuaScript", val);
        }

        public void Error(string val)
        {
            Cv_Debug.Error(val);
        }

        public void Warning(string val)
        {
            Cv_Debug.Warning(val);
        }
    }
}