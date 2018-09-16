using Caravel.Core;
using Caravel.Core.Draw;
using Caravel.Core.Events;
using Caravel.Core.Input;
using Caravel.Core.Physics;
using Caravel.Core.Process;
using Caravel.Core.Resource;
using Caravel.Core.Scripting;
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

        public string InitScriptLocation
        {
            get; private set;
        }

        public string InitScriptBundle
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

        public readonly PlayerIndex PlayerOne = PlayerIndex.One;
        public readonly PlayerIndex PlayerTwo = PlayerIndex.Two;
        public readonly PlayerIndex PlayerThree = PlayerIndex.Three;
        public readonly PlayerIndex PlayerFour = PlayerIndex.Four;

        private Dictionary<string, string> m_TextResource;
        private Dictionary<string, string> m_TextResourceLocations;
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

            m_TextResource = new Dictionary<string, string>();
            m_TextResourceLocations = new Dictionary<string, string>();
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

            if (!EditorRunning && !LoadStrings("English"))
            {
                Cv_Debug.Error("Unable to load strings.");
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

            ScriptManager = new Cv_LuaScriptManager();
            if (!ScriptManager.VInitialize())
            {
                Cv_Debug.Error("Unable to initialize script manager.");
                Exit();
                return;
            }

            //This loads the preInit Lua script if there's one
            if (InitScriptLocation != null && InitScriptLocation != "" && InitScriptBundle != null && InitScriptBundle != "")
            {    
                Cv_ScriptResource initScript = ResourceManager.GetResource<Cv_ScriptResource>(InitScriptLocation, InitScriptBundle);
                initScript.RunScript();
            }

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

            Window.Title = VGetGameTitle();
            SaveGameDirectory = GetSaveGameDirectory(VGetGameAppDirectoryName());

            Logic = VCreateGameLogic();
            if (Logic == null) {
                Cv_Debug.Error("Unable to create game logic.");
                Exit();
                return;
            }

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
        }
        
        protected sealed override void UnloadContent()
        {
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
            ScriptManager.OnUpdate(gameTime.TotalGameTime.Milliseconds, gameTime.ElapsedGameTime.Milliseconds);
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
		Cv_ResourceManager.Instance.AddResourceBundle(bundleId, new Cv_DevelopmentResourceBundle(bundleFile));
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

    public void EditorReadStrings(string editorWorkingLocation)
    {
        EditorWorkingDirectory = editorWorkingLocation;
        ReadProjectFile();
        LoadStrings("English");
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
            string languageFile;
            if (m_TextResourceLocations.TryGetValue(language, out languageFile))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(GetGameWorkingDirectory(), languageFile));
                var root = doc.DocumentElement;

                if (root == null)
                {
                    Cv_Debug.Error("Error - Strings are missing.");
                    return false;
                }

                foreach (XmlElement elem in root.ChildNodes)
                {
                    string key = elem.Attributes["id"].Value;
                    string text = elem.Attributes["value"].Value;

                    if (key != null && text != null) 
                    {
                        m_TextResource.Add(key, text);
                    }
                }
                return true;
            }
            else
            {
                Cv_Debug.Error("Error - Could not find strings file for language: " + language);
                return false;
            }
        }

        public string GetString(string stringID)
        {
            string localizedString;
            
            if (m_TextResource.TryGetValue(stringID, out localizedString))
            {
                return localizedString;
            }
            else
            {
                Cv_Debug.Error("Error - String not found!");
                return "";
            }
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

            MaterialsLocation = "";
            if (materialsNode != null)
            {
                MaterialsLocation = materialsNode.Attributes["materialsFile"].Value;
            }

            var controlsNode = root.SelectSingleNode("Controls");

            ControlBindingsLocation = "";
            if (controlsNode != null)
            {
                ControlBindingsLocation = controlsNode.Attributes["bindingsFile"].Value;
            }

            var scriptsNode = root.SelectSingleNode("Scripts");

            InitScriptLocation = "";
            InitScriptBundle = "";
            if (scriptsNode != null)
            {
                InitScriptLocation = scriptsNode.Attributes["preInitScript"].Value;
                InitScriptBundle = scriptsNode.Attributes["preInitScriptBundle"].Value;
            }

            var stringsNode = root.SelectSingleNode("Strings");

            m_TextResourceLocations.Clear();
            if (stringsNode != null)
            {
                var languageNodes = stringsNode.SelectNodes("Language");

                foreach (XmlElement languageNode in languageNodes)
                {
                    var language = languageNode.Attributes["id"].Value;
                    var file = languageNode.Attributes["stringsFile"].Value;

                    m_TextResourceLocations.Add(language, file);
                }
            }
        }
#endregion
    }
}
