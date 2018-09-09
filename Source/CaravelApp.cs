using Caravel.Core;
using Caravel.Core.Draw;
using Caravel.Core.Events;
using Caravel.Core.Input;
using Caravel.Core.Physics;
using Caravel.Core.Process;
using Caravel.Core.Resource;
using Caravel.Core.Sound;
using Caravel.Debugging;
using Caravel.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
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

		public Color BackgroundColor
		{
			get; set;
		}

		public bool AllowWindowResize
		{
			get
			{
				return Window.AllowUserResizing;
			}

			set
			{
				Window.AllowUserResizing = value;
			}
		}

		public GraphicsDevice CurrentGraphicsDevice
        {
            get; set;
        }
        
        public bool Quitting
        {
            get; set;
        }

		public bool UseDevelopmentDirectories
		{
			get; set;
		}

        public bool Running
        {
            get; protected set;
        }

        public Cv_GameLogic Logic
        {
            get; protected set;
        }

        public Cv_GameOptions GameOptions
        {
            get; protected set;
        }

        public bool EditorRunning
        {
            get; protected set;
        }

		public string EditorWorkingDirectory
		{
			get; set;
		}

        public Vector2 ScreenSize
        {
            get { return new Vector2(CurrentGraphicsDevice.PresentationParameters.BackBufferWidth,
                                        CurrentGraphicsDevice.PresentationParameters.BackBufferHeight); }
        }

        public string SaveGameDirectory
        {
            get; protected set;
        }

        public string MaterialsLocation
        {
            get; private set;
        }

        public string ControlBindingsLocation
        {
            get; private set;
        }
        
        private XmlElement m_BundleInfo;

#region Managers
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

        public Cv_SoundManager SoundManager
        {
            get; private set;
        }

        public Cv_InputManager InputManager
        {
            get; private set;
        }

        public GraphicsDeviceManager Graphics
        {
            get; private set;
        }

        public Cv_SceneElement Scene
        {
            get; private set;
        }

        private Dictionary<string, string>  m_TextResource;
#endregion
        
#endregion

        public CaravelApp(int screenWidth, int screenHeight, bool allowWindowResize = false)
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = screenWidth;
            Graphics.PreferredBackBufferHeight = screenHeight;
            Window.Title = "Loading";
            Instance = this;
			AllowWindowResize = allowWindowResize;
			BackgroundColor = Color.Black;
        	Window.ClientSizeChanged += OnResize;
			UseDevelopmentDirectories = false;
			EditorWorkingDirectory = Directory.GetCurrentDirectory();
        }

#region MonoGame Functions
        protected sealed override void Initialize()
        {
            if (!EditorRunning)
            {
                CurrentGraphicsDevice = GraphicsDevice;
            }
            
            Cv_Debug debug = new Cv_Debug();
            debug.Initialize("Logs/logTags.xml");

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

            ReadProjectFile();

            ResourceManager = new Cv_ResourceManager();
            if (!ResourceManager.Initialize(m_BundleInfo, UseDevelopmentDirectories))
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
            //if (!ScriptManager.Initialize())
            //{
            //    Cv_Debug.Error("Unable to initialize script manager.");
            //    Exit();
            //    return;
            //}

            EventManager = new Cv_EventManager(true);
            if (!EventManager.Initialize())
            {
                Cv_Debug.Error("Unable to initialize event manager.");
                Exit();
                return;
            }

            SoundManager = new Cv_SoundManager();
            if (!SoundManager.Initialize())
            {
                Cv_Debug.Error("Unable to initialize sound manager.");
                Exit();
                return;
            }

            InputManager = new Cv_InputManager();
            if (!InputManager.Initialize())
            {
                Cv_Debug.Error("Unable to initialize input manager.");
                Exit();
                return;
            }

            ProcessManager = new Cv_ProcessManager();
            if (!ProcessManager.Initialize())
            {
                Cv_Debug.Error("Unable to initialize process manager.");
                Exit();
                return;
            }

            Window.Title = VGetGameTitle();
            SaveGameDirectory = GetSaveGameDirectory(VGetGameAppDirectoryName());

            Logic = VCreateGameLogic();
            if (Logic == null) {
                Cv_Debug.Error("Unable to create game logic.");
                Exit();
                return;
            }

            RegisterEngineScriptEvents();

            Scene = new Cv_SceneElement(this);
            var gvs = VCreateGameViews();
            foreach (var gv in gvs)
            {
                Logic.AddView(gv);
            }
            Logic.AddGamePhysics(VCreateGamePhysics());
            Logic.Initialize();

            Running = true;

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
                    || Quitting)
            {
                Cv_Debug.Info("Exiting Game.");
                Exit();
            }

			EventManager.OnUpdate(gameTime.TotalGameTime.Milliseconds, gameTime.ElapsedGameTime.Milliseconds);
            SoundManager.OnUpdate(gameTime.TotalGameTime.Milliseconds, gameTime.ElapsedGameTime.Milliseconds);
            InputManager.OnUpdate(gameTime.TotalGameTime.Milliseconds, gameTime.ElapsedGameTime.Milliseconds);
            ProcessManager.OnUpdate(gameTime.TotalGameTime.Milliseconds, gameTime.ElapsedGameTime.Milliseconds);

            Logic.OnUpdate(gameTime.TotalGameTime.Milliseconds, gameTime.ElapsedGameTime.Milliseconds);

            base.Update(gameTime);
        }
        
        protected sealed override void Draw(GameTime gameTime)
        {
            if (!EditorRunning)
            {
                CurrentGraphicsDevice.Clear(BackgroundColor);
            }

            foreach (var gv in Logic.GameViews)
            {
                gv.VOnRender(gameTime.TotalGameTime.Milliseconds, gameTime.ElapsedGameTime.Milliseconds);
            }

            foreach (var gv in Logic.GameViews)
            {
                gv.VOnPostRender();
            }

            base.Draw(gameTime);
        }
#endregion

#region Functions to be defined by the game
        protected abstract string           VGetGameTitle();
        protected abstract string           VGetGameAppDirectoryName();
        protected abstract bool             VCheckGameSystemResources();
        protected abstract Cv_GameLogic     VCreateGameLogic();
        protected abstract Cv_GameView[]    VCreateGameViews();
        protected abstract Cv_GamePhysics   VCreateGamePhysics();
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

	public void EditorLoadResourceBundle(string bundleId, string editorWorkingLocation, string bundleFile)
	{
		EditorWorkingDirectory = editorWorkingLocation;
		Cv_ResourceManager.Instance.AddResourceBundle(bundleId, new Cv_DevelopmentZipResourceBundle(bundleFile));
	}

	public void EditorUnloadResourceBundle(string bundleId)
	{
		Cv_ResourceManager.Instance.RemoveResourceBundle(bundleId);
	}

    public void EditorReadMaterials(string editorWorkingLocation)
    {
        EditorWorkingDirectory = editorWorkingLocation;
        ReadProjectFile();
        MaterialsLocation = Path.Combine(editorWorkingLocation, MaterialsLocation);
        Logic.GamePhysics.VInitialize();
    }

    public void EditorReadControls(string editorWorkingLocation)
    {
        EditorWorkingDirectory = editorWorkingLocation;
        ReadProjectFile();
        ControlBindingsLocation = Path.Combine(editorWorkingLocation, ControlBindingsLocation);
        InputManager.Initialize(); 
    }
#endregion

#region CaravelApp functions
		public string GetGameWorkingDirectory()
		{
			if (EditorRunning)
			{
				return EditorWorkingDirectory;
			}
			else
			{
				return Directory.GetCurrentDirectory();
			}
		}

        public void AbortGame()
        {
            Quitting = true;
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
            foreach (var gv in Logic.GameViews)
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

		public void OnResize(Object sender, EventArgs e)
		{
			foreach (var gv in Logic.GameViews)
            {
                if (gv.Type == Cv_GameViewType.Player)
				{
					var pv = (Cv_PlayerView) gv;

                    if (EditorRunning)
                    {
                        var args = (Cv_ResizeWindowEvt) e;
                        pv.OnWindowResize(args.Width, args.Height);
                    }
                    else
                    {
                        pv.OnWindowResize(Window.ClientBounds.Width, Window.ClientBounds.Height);
                    }
				}
            }
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

        private void ReadProjectFile()
        {
            var files = Directory.GetFiles(GetGameWorkingDirectory(), "*.cvp");

            if (files.Length <= 0)
            {
                Cv_Debug.Error("Unable to find project file.");
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(files[0]);
            var root = doc.FirstChild;

            var resourceBundles = root.SelectSingleNode("//ResourceBundles");
            m_BundleInfo = (XmlElement) resourceBundles;

            var materialsNode = root.SelectSingleNode("Materials");

            if (materialsNode != null)
            {
                MaterialsLocation = materialsNode.Attributes["materialsFile"].Value;
            }

            var controlsNode = root.SelectSingleNode("Controls");

            if (controlsNode != null)
            {
                ControlBindingsLocation = controlsNode.Attributes["bindingsFile"].Value;
            }
        }
#endregion
    }
}
