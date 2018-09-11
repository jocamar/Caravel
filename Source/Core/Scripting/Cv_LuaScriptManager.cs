using System.IO;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Core.Process;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;
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
                return m_ScriptState.DebuggerEnabled;
            }

            set
            {
                m_ScriptState.DebuggerEnabled = value;
            }
        }

        private Script m_ScriptState;
        private string m_sLastError;

        internal Cv_LuaScriptManager()
        {
            Instance = this;
            Script.GlobalOptions.Platform = new LimitedPlatformAccessor();
            m_ScriptState = new Script(CoreModules.Preset_SoftSandbox);
        }

        internal override bool VInitialize()
        {
            m_ScriptState.Options.DebugPrint = LuaPrint;

            //Register basic types
            UserData.RegisterType<Vector2>();
            UserData.RegisterType<Vector3>();
            UserData.RegisterType<Matrix>();
            UserData.RegisterType<Color>();
            UserData.RegisterType<Cv_Transform>();
            UserData.RegisterType<Cv_BodyType>();
            UserData.RegisterType<Cv_EntityID>();
            UserData.RegisterType<Cv_GameViewID>();
            UserData.RegisterType<Cv_ComponentID>();
            UserData.RegisterType<Cv_GameState>();
            UserData.RegisterType<object>();

            //Register main Caravel types
            UserData.RegisterType<CaravelApp>();
            UserData.RegisterType<Cv_GameLogic>();
            UserData.RegisterType<Cv_ProcessManager>();
            UserData.RegisterType<Cv_PlayerView>();
            UserData.RegisterType<Cv_GameView>();
            UserData.RegisterType<Cv_Entity>();

            //Register processes
            UserData.RegisterType<Cv_Process>();
            UserData.RegisterType<Cv_TimerProcess>();

            //Register components
            UserData.RegisterType<Cv_EntityComponent>();
            UserData.RegisterType<Cv_SpriteComponent>();
            UserData.RegisterType<Cv_TransformComponent>();
            UserData.RegisterType<Cv_RigidBodyComponent>();
            UserData.RegisterType<Cv_TransformComponent>();
            UserData.RegisterType<Cv_SoundListenerComponent>();
            UserData.RegisterType<Cv_SoundEmitterComponent>();
            UserData.RegisterType<Cv_CameraComponent>();

            DynValue caravel = UserData.Create(CaravelApp.Instance);

            m_ScriptState.Globals.Set("caravel", caravel);
            
            return true;
        }

        internal override void VExecuteFile(string resource)
        {
            try
            {
                m_ScriptState.DoFile(resource);
            }
            catch (InterpreterException e)
            {
                Cv_Debug.Error("Error executing Lua script:\n" + e.DecoratedMessage);
            }
        }

        internal override void VExecuteString(string str)
        {
            try
            {
                m_ScriptState.DoString(str);
            }
            catch (InterpreterException e)
            {
                Cv_Debug.Error("Error executing Lua script:\n" + e.DecoratedMessage);
            }
        }

        internal override void VExecuteStream(Stream stream)
        {
            try
            {
                m_ScriptState.DoStream(stream);
            }
            catch (InterpreterException e)
            {
                Cv_Debug.Error("Error executing Lua script:\n" + e.DecoratedMessage);
            }
        }

        private void LuaPrint(string value)
        {
            Cv_Debug.Log("LuaScript", value);
        }
    }
}