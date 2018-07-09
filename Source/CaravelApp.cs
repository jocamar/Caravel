using Caravel.Core;
using Caravel.Core.Events;
using Caravel.Core.Physics;
using Caravel.Core.Resource;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using static Caravel.Core.Cv_GameView;

namespace Caravel
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public abstract class CaravelApp : Game
    {
#region Properties
        public static CaravelApp Instance;
        
        public bool Quitting
        {
            get { return m_bQuitting; }
            set { 
                m_bQuitting = value;
            }
        }

        public bool Running
        {
            get { return m_bIsRunning; }
        }

        public Cv_GameLogic GameLogic
        {
            get { return m_GameLogic; }
        }

        public Cv_GameOptions GameOptions
        {
            get; private set;
        }

        public bool EditorRunning
        {
            get { return m_bIsEditorRunning; }
        }

        public Vector2 ScreenSize
        {
            get { return new Vector2(CurrentGraphicsDevice.PresentationParameters.BackBufferWidth,
                                        CurrentGraphicsDevice.PresentationParameters.BackBufferHeight); }
        }

        public GraphicsDevice CurrentGraphicsDevice
        {
            get; set;
        }

        public string SaveGameDirectory
        {
            get { return m_sSaveGameDirectory; }
        }

        protected bool m_bIsRunning;
        protected bool m_bQuitRequested;
        protected bool m_bQuitting;
        protected bool m_bIsEditorRunning;
        protected string m_sSaveGameDirectory;

        protected Cv_GameLogic m_GameLogic;
        protected Cv_Debug m_Debug;

        // Managers
        public Cv_EventManager EventManager
        {
            get; private set;
        }

        public Cv_ResourceManager ResourceManager
        {
            get; private set;
        }

        public Cv_NetworkManager NetworkManager
        {
            get; private set;
        }

        public Cv_ProcessManager ProcessManager
        {
            get; private set;
        }

        public Cv_ScriptManager ScriptManager
        {
            get; private set;
        }

        public GraphicsDeviceManager Graphics
        {
            get; private set;
        }

        private Dictionary<string, string>  m_TextResource;
        
#endregion

        public CaravelApp(int screenWidth, int screenHeight)
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = screenWidth;
            Graphics.PreferredBackBufferHeight = screenHeight;
            Window.Title = "Loading";
            Instance = this;
        }

#region MonoGame Functions
        protected sealed override void Initialize()
        {
            CurrentGraphicsDevice = GraphicsDevice;
            
            Cv_Debug debug = new Cv_Debug();
            debug.Init("Logs/logTags.xml");

            if (!CheckEngineSystemResources())
            {
                Cv_Debug.Error("Not enough system resources to run the engine.");
                Exit();
                return;
            }

            if (!VCheckGameSystemResources())
            {
                Cv_Debug.Error("Not enough system resources to run the game.");
                Exit();
                return;
            }

            RegisterEngineEvents();
            VRegisterGameEvents();

            ResourceManager = new Cv_ResourceManager();
            if (!ResourceManager.Init("Assets.zip", EditorRunning))
            {
                Cv_Debug.Error("Unable to initialize resource manager.");
                Exit();
                return;
            }

            if (!LoadStrings("English"))
            {
                Cv_Debug.Error("Unable to load strings.");
                Exit();
                return;
            }

            //TODO(JM): init the script manager here
            //ScriptManager = new Cv_ScriptManager("Scripts/PreInit.lua");
            //if (!ScriptManager.Init())
            //{
            //    Cv_Debug.Error("Unable to initialize script manager.");
            //    Exit();
            //    return;
            //}

            EventManager = new Cv_EventManager(true);
            if (!EventManager.Init())
            {
                Cv_Debug.Error("Unable to initialize event manager.");
                Exit();
                return;
            }

            //TODO(JM): init the process manager here
            //ProcessManager = new Cv_ProcessManager();
            //if (!ProcessManager.Init())
            //{
            //    Cv_Debug.Error("Unable to initialize process manager.");
            //    Exit();
            //    return;
            //}

            Window.Title = VGetGameTitle();
            m_sSaveGameDirectory = GetSaveGameDirectory(VGetGameAppDirectory());

            m_GameLogic = VCreateGameLogic();
            if (m_GameLogic == null) {
                Cv_Debug.Error("Unable to create game logic.");
                Exit();
                return;
            }

            RegisterEngineScriptEvents();

            var gvs = VCreateGameViews();
            foreach (var gv in gvs)
            {
                m_GameLogic.AddView(gv);
            }
            m_GameLogic.AddGamePhysics(VCreateGamePhysics());
            m_GameLogic.Init();

            m_bIsRunning = true;

            VInitialize();

            base.Initialize();
        }
        
        protected sealed override void LoadContent()
        {
            //TODO(JM): use this.Content to load game content here
        }
        
        protected sealed override void UnloadContent()
        {
            //TODO(JM): Unload any non ContentManager content here
        }
        
        protected sealed override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                    || Keyboard.GetState().IsKeyDown(Keys.Escape)
                    || m_bQuitting)
            {
                Cv_Debug.Info("Exiting Game.");
                Exit();
            }

			EventManager.OnUpdate(gameTime.TotalGameTime.Milliseconds, gameTime.ElapsedGameTime.Milliseconds);
            GameLogic.OnUpdate(gameTime.TotalGameTime.Milliseconds, gameTime.ElapsedGameTime.Milliseconds);

            base.Update(gameTime);
        }
        
        protected sealed override void Draw(GameTime gameTime)
        {
            if (!EditorRunning)
            {
                CurrentGraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
            }

            foreach (var gv in m_GameLogic.GameViews)
            {
                gv.VOnRender(gameTime.TotalGameTime.Milliseconds, gameTime.ElapsedGameTime.Milliseconds);
            }

            base.Draw(gameTime);
        }
#endregion

#region Functions to be defined by the game
        protected abstract string           VGetGameTitle();
        protected abstract string           VGetGameAppDirectory();
        protected abstract bool             VCheckGameSystemResources();
        protected abstract Cv_GameLogic     VCreateGameLogic();
        protected abstract Cv_GameView[]    VCreateGameViews();
        protected abstract Cv_GamePhysics   VCreateGamePhysics();
        protected abstract void             VRegisterGameEvents();
        protected internal abstract bool    VInitialize();
        protected internal abstract bool    VLoadGame();
#endregion

#region Functions to be used by editor
    public void EditorInitialize()
    {
        Initialize();
    }

    public void EditorLoadContent()
    {
        LoadContent();
    }

    public void EditorUpdate(GameTime time)
    {
        Update(time);
    }

    public void EditorDraw(GameTime time)
    {
        Draw(time);
    }

    public void EditorUnloadContent()
    {
        UnloadContent();
    }
#endregion

#region CaravelApp functions
        public void AbortGame()
        {
            m_bQuitting = true;
        }

        public bool LoadStrings(string language)
        {
            return true;
        }

        public string GetString(string stringID)
        {
            return "";
        }

        public Cv_PlayerView GetPlayerView(PlayerIndex player)
        {
            foreach (var gv in m_GameLogic.GameViews)
            {
                if (gv.Type == Cv_GameViewType.Player)
                {
                    var pView = (Cv_PlayerView) gv;
                    if (pView.PlayerIdx == player)
                    {
                        return pView;
                    }
                }
            }

            return null;
        }

        public bool AttachAsClient()
        {
            return true;
        }

        private void RegisterEngineEvents()
        {

        }

        private void RegisterEngineScriptEvents()
        {

        }

        private bool CheckEngineSystemResources()
        {
            //TODO(JM): Add any checks necessary for bare minimum resources here
            return true;
        }

        private string GetSaveGameDirectory(string gameDirectory)
        {
            var userDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var finalPath = Path.Combine(userDataPath, gameDirectory);
            finalPath += Path.DirectorySeparatorChar;
            return finalPath;
        }
#endregion
    }
}
