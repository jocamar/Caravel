using System.IO;
using System;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Core.Process;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using NLua;
using static Caravel.Core.Cv_GameLogic;
using static Caravel.Core.Cv_GameView;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Entity.Cv_EntityComponent;
using static Caravel.Core.Entity.Cv_RigidBodyComponent;

namespace Caravel.Core.Scripting
{
    public class Cv_LuaScriptManager : Cv_ScriptManager
    {
        public override bool DebuggerEnabled
        {
            get
            {
				return false;
            }

            set
            {
            }
        }

		private Lua m_ScriptState;
		private Cv_LuaScriptBindings m_ScriptBindings;
        private string m_sLastError;

        internal Cv_LuaScriptManager()
        {
            Instance = this;
			m_ScriptState = new Lua();
			m_ScriptBindings = new Cv_LuaScriptBindings();
        }

		~Cv_LuaScriptManager()
		{
			m_ScriptState.Dispose();
		}

        internal override bool VInitialize()
        {
			m_ScriptState.DoString(@"-- make environment
									env = {} -- add functions you know are safe here

									-- run code under environment [Lua 5.2]
									function run(untrusted_code)
										local untrusted_function, message = load(untrusted_code, nil, 't', env)
										if not untrusted_function then return nil, message end
										return pcall(untrusted_function)
									end");
			m_ScriptState["caravel"] = CaravelApp.Instance;
			m_ScriptState["new"] = m_ScriptBindings;
            
            return true;
        }

        internal override void VExecuteFile(string resource)
        {
            try
            {
                //m_ScriptState.DoFile(resource);
            }
			catch (Exception e)
            {
				Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
            }
        }

        internal override void VExecuteString(string str)
        {
            try
            {
                m_ScriptState.DoString("run [[" + str + "]]");
            }
            catch (Exception e)
            {
				Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
            }
        }

        internal override void VExecuteStream(Stream stream)
        {
            try
            {
				using (StreamReader reader = new StreamReader(stream))
				{
					stream.Position = 0;
					var code = "run (" + reader.ReadToEnd() + ")";
					m_ScriptState.DoString(code);
				}
            }
            catch (Exception e)
            {
				Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
            }
        }

        private void LuaPrint(string value)
        {
            Cv_Debug.Log("LuaScript", value);
        }
    }
}