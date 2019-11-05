using System.IO;
using System;
using Caravel.Core.Entity;
using Caravel.Debugging;
using NLua;
using System.Collections.Generic;
using System.Diagnostics;
using Caravel.Core.Events;
using NLua.Exceptions;

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

        private struct Cv_ScriptExecutionRequest
        {
            public string Code;
            public Cv_Entity Entity;
            public Cv_Event Event;
            public string Resource;
            public bool RunInEditor;
        }

        private readonly int NUM_QUEUES = 2;

		private Lua m_ScriptState;
		private Cv_LuaScriptBindings m_ScriptBindings;
        private Cv_LuaLogger m_ScriptLogger;
        private string m_sLastError;
        private LinkedList<Cv_ScriptExecutionRequest>[] m_QueuedScriptLists;
        private int m_iActiveQueue = 0;

        public override void AddGameBindings(string scriptObject, object exposedClass)
        {
            m_ScriptState["env." + scriptObject] = exposedClass;
        }

        internal Cv_LuaScriptManager()
        {
            Instance = this;
			m_ScriptState = new Lua();
			m_ScriptBindings = new Cv_LuaScriptBindings();
            m_ScriptLogger = new Cv_LuaLogger();
            m_QueuedScriptLists = new LinkedList<Cv_ScriptExecutionRequest>[NUM_QUEUES];

            for (int i = 0; i < NUM_QUEUES; i++)
            {
                m_QueuedScriptLists[i] = new LinkedList<Cv_ScriptExecutionRequest>();
            }

            MaxProcessTimeMillis = long.MaxValue;
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
                                        if untrusted_code:byte(1) == 27 then error(""binary bytecode prohibited""); return nil, ""binary bytecode prohibited"" end
                                        local untrusted_function, message = loadstring(untrusted_code)
                                        if not untrusted_function then error(message); return nil, message end
                                        setfenv(untrusted_function, env)
                                        return pcall(untrusted_function)
                                    end");
			m_ScriptState["env.caravel"] = CaravelApp.Instance;
			m_ScriptState["env.new"] = m_ScriptBindings;
            m_ScriptState["env.log"] = m_ScriptLogger;
            
            return true;
        }

        internal override void OnUpdate(float time, float elapsedTime)
        {
            var queueToProcess = m_iActiveQueue;
            m_iActiveQueue = (m_iActiveQueue + 1) % NUM_QUEUES;
            m_QueuedScriptLists[m_iActiveQueue].Clear();

            long currentMs = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            lock (m_QueuedScriptLists[queueToProcess])
            {
                while (m_QueuedScriptLists[queueToProcess].Count > 0)
                {
                    var script = m_QueuedScriptLists[queueToProcess].First.Value;
                    m_QueuedScriptLists[queueToProcess].RemoveFirst();

                    if (!CaravelApp.Instance.EditorRunning || script.RunInEditor)
                    {
                        try
                        {
                            m_ScriptState["env.currentEntity"] = script.Entity;
                            m_ScriptState["env.currentEvent"] = script.Event;
                            m_ScriptState.DoString("run [[" + script.Code + "]]", script.Resource);
                        }
                        catch (LuaException e)
                        {
                            Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
                        }
                        catch (Exception e)
                        {
                            Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
                        }
                    }

                    currentMs = stopwatch.ElapsedMilliseconds;

                    if (MaxProcessTimeMillis != long.MaxValue)
                    {
                        if (currentMs >= MaxProcessTimeMillis)
                        {
                            Cv_Debug.Error("ScriptManager processing time exceeded. Aborting.");
                            stopwatch.Stop();
                            break;
                        }
                    }
                }

                while (m_QueuedScriptLists[queueToProcess].Count > 0)
                {
                    m_QueuedScriptLists[m_iActiveQueue].AddBefore(m_QueuedScriptLists[m_iActiveQueue].First, m_QueuedScriptLists[queueToProcess].Last);
                    m_QueuedScriptLists[queueToProcess].RemoveLast();
                }
            }
        }

        internal override void VExecuteFile(string resource, Cv_Entity runningEntity = null)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (LuaException e)
            {
                Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
            }
            catch (Exception e)
            {
                Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
            }
        }

        internal override void VExecuteFile(string resource, Cv_Event runningEvent)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (LuaException e)
            {
                Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
            }
            catch (Exception e)
            {
                Cv_Debug.Error("Error executing Lua script:\n" + e.ToString());
            }
        }

        internal override void VExecuteString(string resource, string str, bool runInEditor, Cv_Entity runningEntity = null)
        {
            Cv_Debug.Assert( (m_iActiveQueue >= 0 && m_iActiveQueue < NUM_QUEUES), "ScriptManager must have an active script queue.");

            if (str == null)
            {
                Cv_Debug.Error("Invalid script in VExecuteString.");
                return;
            }

            Cv_ScriptExecutionRequest request = new Cv_ScriptExecutionRequest();
            request.Code = str;
            request.Resource = resource;
            request.Entity = runningEntity;
            request.RunInEditor = runInEditor;
            request.Event = null;

            lock (m_QueuedScriptLists[m_iActiveQueue])
            {
                m_QueuedScriptLists[m_iActiveQueue].AddLast(request);
            }

            Cv_Debug.Log("LuaScript", "Queued script " + resource + " for entity " + (runningEntity != null ? runningEntity.EntityName : "[null]"));
        }

        internal override void VExecuteString(string resource, string str, bool runInEditor, Cv_Event runningEvent, Cv_Entity runningEntity)
        {
            Cv_Debug.Assert( (m_iActiveQueue >= 0 && m_iActiveQueue < NUM_QUEUES), "ScriptManager must have an active script queue.");

            if (str == null)
            {
                Cv_Debug.Error("Invalid script in VExecuteString.");
                return;
            }

            Cv_ScriptExecutionRequest request = new Cv_ScriptExecutionRequest();
            request.Code = str;
            request.Resource = resource;
            request.Entity = runningEntity;
            request.Event = runningEvent;
            request.RunInEditor = runInEditor;

            lock (m_QueuedScriptLists[m_iActiveQueue])
            {
                m_QueuedScriptLists[m_iActiveQueue].AddLast(request);
            }

            Cv_Debug.Log("LuaScript", "Queued script " + resource + " for entity " + (runningEntity != null ? runningEntity.EntityName : "[null]"));
        }

        internal override void VExecuteStream(string resource, Stream stream, bool runInEditor, Cv_Entity runningEntity = null)
        {
            Cv_Debug.Assert( (m_iActiveQueue >= 0 && m_iActiveQueue < NUM_QUEUES), "ScriptManager must have an active script queue.");

            if (stream == null)
            {
                Cv_Debug.Error("Invalid stream in VExecuteStream.");
                return;
            }

            string code;

            using (StreamReader reader = new StreamReader(stream))
            {
                stream.Position = 0;
                code = reader.ReadToEnd();
            }

            Cv_ScriptExecutionRequest request = new Cv_ScriptExecutionRequest();
            request.Code = code;
            request.Resource = resource;
            request.Entity = runningEntity;
            request.RunInEditor = runInEditor;
            request.Event = null;

            lock (m_QueuedScriptLists[m_iActiveQueue])
            {
                m_QueuedScriptLists[m_iActiveQueue].AddLast(request);
            }

            Cv_Debug.Log("LuaScript", "Queued script " + resource + " for entity " + (runningEntity != null ? runningEntity.EntityName : "[null]"));
        }

        internal override void VExecuteStream(string resource, Stream stream, bool runInEditor, Cv_Event runningEvent)
        {
            Cv_Debug.Assert( (m_iActiveQueue >= 0 && m_iActiveQueue < NUM_QUEUES), "ScriptManager must have an active script queue.");

            if (stream == null)
            {
                Cv_Debug.Error("Invalid stream in VExecuteStream.");
                return;
            }

            string code;

            using (StreamReader reader = new StreamReader(stream))
            {
                stream.Position = 0;
                code = reader.ReadToEnd();
            }

            var entity = CaravelApp.Instance.Logic.GetEntity(runningEvent.EntityID);

            Cv_ScriptExecutionRequest request = new Cv_ScriptExecutionRequest();
            request.Code = code;
            request.Resource = resource;
            request.Entity = entity;
            request.Event = runningEvent;
            request.RunInEditor = runInEditor;

            lock (m_QueuedScriptLists[m_iActiveQueue])
            {
                m_QueuedScriptLists[m_iActiveQueue].AddLast(request);
            }

            Cv_Debug.Log("LuaScript", "Queued script " + resource + " for entity " + (entity != null ? entity.EntityName : "[null]"));
        }
    }
}