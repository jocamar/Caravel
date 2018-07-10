using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using static Caravel.Core.Cv_GameLogic;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Events.Cv_EventManager;
using Caravel.Core.Draw;

namespace Caravel.Core
{
    public class Cv_PlayerView : Cv_GameView
    {
        public override Cv_GameViewType Type
        {
            get
            {
                return m_Type;
            }
        }

        public override Cv_GameViewID ID
        {
            get
            {
                return m_ID;
            }
        }

        public Cv_CameraNode Camera
		{
			get; set;
		}

        public PlayerIndex PlayerIdx
        {
            get; private set;
        }

        public bool SoundsPaused
        {
            get
            {
                return m_bAreSoundsPaused;
            }
            
            set
            {
                m_bAreSoundsPaused = value;
                if (value)
                {
                    //CaravelApp.Instance.SoundManager.StopAllSounds();
                }
                else
                {
                    //CaravelApp.Instance.SoundManager.ResumeAllSounds();
                }
            }
        }

        private Cv_SceneElement m_Scene;
        private Cv_GameViewType m_Type = Cv_GameViewType.Player;
        private Cv_GameViewID m_ID;
        private Cv_EntityID m_EntityID;
        private Cv_GameState m_GameState;
        private bool m_bRunFullSpeed;
        private bool m_bAreSoundsPaused;
        //private Cv_Console m_Console;
        private Cv_Renderer m_Renderer;
        private List<Cv_ScreenElement> m_ScreenElements;

        public Cv_PlayerView(PlayerIndex player, int vWidth, int vHeight, SpriteBatch spriteBatch = null)
        {
            m_ID = Cv_GameViewID.INVALID_GAMEVIEW;
            m_GameState = Cv_GameState.Initializing;
            RegisterEventListeners();

            PlayerIdx = player;
            m_ScreenElements = new List<Cv_ScreenElement>();
            m_bAreSoundsPaused = false;

            m_Renderer = new Cv_Renderer(spriteBatch);
            m_Renderer.ScreenWidth = CaravelApp.Instance.Graphics.PreferredBackBufferWidth;
            m_Renderer.ScreenHeight = CaravelApp.Instance.Graphics.PreferredBackBufferHeight;
            m_Renderer.VirtualWidth = vWidth;
            m_Renderer.VirtualHeight = vHeight;
            m_Renderer.Init();

            Cv_DrawUtils.Initialize();

            m_Scene = new Cv_SceneElement(m_Renderer);
        }

        ~Cv_PlayerView()
        {
            RemoveEventListeners();
        }

        public void PushScreenElement(Cv_ScreenElement element)
        {
            m_ScreenElements.Add(element);
        }

        public void RemoveScreenElement(Cv_ScreenElement element)
        {
            m_ScreenElements.Remove(element);
        }

        protected internal virtual void VRenderText()
        {

        }

        protected internal virtual bool VOnLoadGame(XmlElement sceneData)
        {
            PushScreenElement(m_Scene);
            m_Scene.IsVisible = true;
			
			if (sceneData.Attributes["vWidth"] != null)
			{
				m_Renderer.VirtualWidth = int.Parse(sceneData.Attributes["vWidth"].Value);
			}

			if (sceneData.Attributes["vHeight"] != null)
			{
				m_Renderer.VirtualHeight = int.Parse(sceneData.Attributes["vHeight"].Value);
			}
			
            return true;
        }

        protected internal override void VOnAttach(Cv_GameViewID id, Cv_EntityID entityId)
        {
            m_ID = id;
            m_EntityID = entityId;
        }

        protected internal override void VOnRender(float time, float timeElapsed)
        {
            if (m_bRunFullSpeed && CaravelApp.Instance.IsFixedTimeStep)
            {
                CaravelApp.Instance.IsFixedTimeStep = false;
                CaravelApp.Instance.Graphics.SynchronizeWithVerticalRetrace = false;
                CaravelApp.Instance.Graphics.ApplyChanges();
            }
            else if (!CaravelApp.Instance.IsFixedTimeStep)
            {
                CaravelApp.Instance.IsFixedTimeStep = true;
                CaravelApp.Instance.Graphics.SynchronizeWithVerticalRetrace = true;
                CaravelApp.Instance.Graphics.ApplyChanges();
            }

            var sortedElements = m_ScreenElements.OrderBy(e => e).ToList();

            m_Renderer.SetupViewport();
            foreach (var se in sortedElements)
            {
                if (se.IsVisible)
                {
                    if (se == m_Scene)
                    {
                        m_Scene.Camera = Camera;
                    }

                    se.VOnRender(time, timeElapsed);
                }
            }

            VRenderText();

            m_Renderer.BeginDraw(Camera);
            CaravelApp.Instance.GameLogic.VRenderDiagnostics(m_Renderer);
            m_Renderer.EndDraw();

            //m_Console.OnRender();

            m_Renderer.ResetViewport();
        }

        protected internal override void VOnUpdate(float time, float timeElapsed)
        {
            //m_Console.OnUpdate(time, timeElapsed);

            foreach (var se in m_ScreenElements)
            {
                se.VOnUpdate(time, timeElapsed);
            }
        }

        internal void LoadGame(XmlElement sceneData)
        {
            VOnLoadGame(sceneData);
        }

        internal void SetPlayerEntity(Cv_EntityID entityId)
        {
            m_EntityID = entityId;
        }

        internal void OnPlaySound(Cv_Event eventData)
        {
            //play sound
        }

        internal void OnGameState(Cv_Event eventData)
        {
            var newStateEvt = (Cv_Event_ChangeState) eventData;

            m_GameState = newStateEvt.NewState;
        }

        private void RegisterEventListeners()
        {
            //Cv_EventManager.Instance.AddListener<Cv_Event_PlaySound>(OnPlaySound);
            Cv_EventManager.Instance.AddListener<Cv_Event_ChangeState>(OnGameState);
            Cv_EventManager.Instance.AddListener<Cv_Event_NewCameraComponent>(OnNewCameraComponent);
        }

        private void RemoveEventListeners()
        {
            //Cv_EventManager.Instance.RemoveListener<Cv_Event_PlaySound>(OnPlaySound);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_ChangeState>(OnGameState);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_NewCameraComponent>(OnNewCameraComponent);
        }

        public void OnNewCameraComponent(Cv_Event eventData)
		{
			var castEventData = (Cv_Event_NewCameraComponent) eventData;
			var cameraNode = castEventData.CameraNode;
            var isDefault = castEventData.IsDefault;

            if (isDefault)
            {
                Camera = cameraNode;
            }
		}

        public void PrintScene()
        {
            m_Scene.PrintTree();
        }

        public bool Pick(Vector2 mousePos, out Cv_EntityID[] entities)
        {
            return m_Scene.Pick(mousePos, out entities);
        }

		internal void OnWindowResize(int newWidth, int newHeight)
		{
			m_Renderer.ScreenWidth = newWidth;
			m_Renderer.ScreenHeight = newHeight;
			Camera.RecalculateTransformationMatrices();
		}
    }
}