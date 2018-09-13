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
        private Cv_LuaLogger m_ScriptLogger;
        private string m_sLastError;

        internal Cv_LuaScriptManager()
        {
            Instance = this;
			m_ScriptState = new Lua();
			m_ScriptBindings = new Cv_LuaScriptBindings();
            m_ScriptLogger = new Cv_LuaLogger();
        }

		~Cv_LuaScriptManager()
		{
			m_ScriptState.Dispose();
		}

        internal override bool VInitialize()
        {
			m_ScriptState.DoString(@"-- make environment
									env = {
                                            ipairs = ipairs,
                                            next = next,
                                            pairs = pairs,
                                            pcall = pcall,
                                            tonumber = tonumber,
                                            tostring = tostring,
                                            type = type,
                                            unpack = unpack,
                                            coroutine = { create = coroutine.create, resume = coroutine.resume, 
                                                running = coroutine.running, status = coroutine.status, 
                                                wrap = coroutine.wrap },
                                            string = { byte = string.byte, char = string.char, find = string.find, 
                                                format = string.format, gmatch = string.gmatch, gsub = string.gsub, 
                                                len = string.len, lower = string.lower, match = string.match, 
                                                rep = string.rep, reverse = string.reverse, sub = string.sub, 
                                                upper = string.upper },
                                            table = { insert = table.insert, maxn = table.maxn, remove = table.remove, 
                                                sort = table.sort },
                                            math = { abs = math.abs, acos = math.acos, asin = math.asin, 
                                                atan = math.atan, atan2 = math.atan2, ceil = math.ceil, cos = math.cos, 
                                                cosh = math.cosh, deg = math.deg, exp = math.exp, floor = math.floor, 
                                                fmod = math.fmod, frexp = math.frexp, huge = math.huge, 
                                                ldexp = math.ldexp, log = math.log, log10 = math.log10, max = math.max, 
                                                min = math.min, modf = math.modf, pi = math.pi, pow = math.pow, 
                                                rad = math.rad, random = math.random, sin = math.sin, sinh = math.sinh, 
                                                sqrt = math.sqrt, tan = math.tan, tanh = math.tanh },
                                            os = { clock = os.clock, difftime = os.difftime, time = os.time }
                                    } -- add functions you know are safe here

									-- run code under environment [Lua 5.1]
                                    function run(untrusted_code)
                                        if untrusted_code:byte(1) == 27 then return nil, ""binary bytecode prohibited"" end
                                        local untrusted_function, message = loadstring(untrusted_code)
                                        if not untrusted_function then return nil, message end
                                        setfenv(untrusted_function, env)
                                        return pcall(untrusted_function)
                                    end");
			m_ScriptState["env.caravel"] = CaravelApp.Instance;
			m_ScriptState["env.new"] = m_ScriptBindings;
            m_ScriptState["env.log"] = m_ScriptLogger;
            
            return true;
        }

        internal override void VExecuteFile(string resource, Cv_Entity runningEntity = null)
        {
            try
            {
                throw new NotImplementedException();
            }
			catch (Exception e)
            {
				Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
            }
        }

        internal override void VExecuteString(string resource, string str, Cv_Entity runningEntity = null)
        {
            try
            {
                m_ScriptState["env.currentEntity"] = runningEntity;
                m_ScriptState.DoString("run [[" + str + "]]", resource);
            }
            catch (Exception e)
            {
				Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
            }
        }

        internal override void VExecuteStream(string resource, Stream stream, Cv_Entity runningEntity = null)
        {
            try
            {
				using (StreamReader reader = new StreamReader(stream))
				{
					stream.Position = 0;
					var code = "run([[" + reader.ReadToEnd() + "]])";

                    m_ScriptState["env.currentEntity"] = runningEntity;
					m_ScriptState.DoString(code, resource);
				}
            }
            catch (Exception e)
            {
				Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
            }
        }
    }
}