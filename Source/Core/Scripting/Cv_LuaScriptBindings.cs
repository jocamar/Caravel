using Caravel.Core.Process;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Scripting
{
    public class Cv_LuaScriptBindings
    {
        public Vector2 lua_Vector2(float x, float y)
		{
			return new Vector2(x, y);
		}

		public Vector3 lua_Vector3(float x, float y, float z)
		{
			return new Vector3(x, y, z);
		}

		public Color lua_Color(float r, float g, float b, float a)
		{
			return new Color((int)r, (int)g, (int)b, (int)a);
		}

		public Cv_Transform lua_Transform(Vector3 pos, Vector2 scale, Vector2 origin, float rot)
		{
			return new Cv_Transform(pos, scale, rot, origin);
		}

		public Cv_TimerProcess lua_Timer(float interval, string luaCode)
		{
			return new Cv_TimerProcess(interval, luaCode);
		}
    }
}