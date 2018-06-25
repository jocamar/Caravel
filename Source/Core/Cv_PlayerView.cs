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
            get;
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

        private Cv_GameViewType m_Type = Cv_GameViewType.Player;
        private Cv_GameViewID m_ID;
        private Cv_EntityID m_EntityID;
        private Cv_GameState m_GameState;
        private Cv_SceneElement m_Scene;
        private bool m_bRunFullSpeed;
        private bool m_bAreSoundsPaused;
        //private Cv_Console m_Console;
        private Cv_Renderer m_Renderer;
        private List<Cv_ScreenElement> m_ScreenElements;

        public Cv_PlayerView(PlayerIndex player, int vWidth, int vHeight)
        {
            m_ID = Cv_GameViewID.INVALID_GAMEVIEW;
            m_GameState = Cv_GameState.Initializing;
            RegisterEventListeners();

            PlayerIdx = player;
            m_ScreenElements = new List<Cv_ScreenElement>();
            m_bAreSoundsPaused = false;

            m_Renderer = new Cv_Renderer();
            m_Renderer.ScreenWidth = CaravelApp.Instance.Graphics.PreferredBackBufferWidth;
            m_Renderer.ScreenHeight = CaravelApp.Instance.Graphics.PreferredBackBufferHeight;
            m_Renderer.VirtualWidth = vWidth;
            m_Renderer.VirtualHeight = vHeight;
            m_Renderer.Init();

            m_Scene = new Cv_SceneElement(m_Renderer);
            Camera = new Cv_CameraNode("camera_" + player);

            m_Scene.AddNode(Cv_EntityID.INVALID_ENTITY, Camera);
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

            //TODO(JM): Order elements here
            var sortedElements = m_ScreenElements.OrderBy(e => e).ToList();

            m_Renderer.SetupViewport();
            CaravelApp.Instance.GraphicsDevice.Clear(m_Renderer.BackgroundColor);
            foreach (var se in sortedElements)
            {
                if (se.IsVisible)
                {
                    if (se == m_Scene)
                    {
                        m_Renderer.BeginDraw(Camera);
                    }
                    else
                    {
                        m_Renderer.BeginDraw();
                    }

                    se.VOnRender(time, timeElapsed);

                    m_Renderer.EndDraw();
                }
            }
            m_Renderer.ResetViewport();

            VRenderText();

            //m_Console.OnRender();
        }

        protected internal override void VOnUpdate(float time, float timeElapsed)
        {
            //m_Console.OnUpdate(time, timeElapsed);

            foreach (var se in m_ScreenElements)
            {
                se.VOnUpdate(time, timeElapsed);
            }

            //TEST, REMOVE LATER
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Camera.Move(new Vector2(-5,0));
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Camera.Move(new Vector2(5, 0));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Camera.Move(new Vector2(0,-5));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Camera.Move(new Vector2(0, 5));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                Camera.Zoom += 0.01f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                Camera.Zoom -= 0.01f;
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
            //set m_GameState
        }

        private void RegisterEventListeners()
        {
            //Cv_EventManager.Instance.AddListener<Cv_Event_PlaySound>(OnPlaySound);
            //Cv_EventManager.Instance.AddListener<Cv_Event_NewState>(OnGameState);
        }

        private void RemoveEventListeners()
        {
            //Cv_EventManager.Instance.RemoveListener<Cv_Event_PlaySound>(OnPlaySound);
            //Cv_EventManager.Instance.RemoveListener<Cv_Event_NewState>(OnGameState);
        }
    }
}