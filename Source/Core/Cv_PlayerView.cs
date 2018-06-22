using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
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
        private bool m_bRunFullSpeed;
        private Cv_GameState m_GameState;
        private SpriteBatch m_SpriteBatch;
        private List<Cv_ScreenElement> m_ScreenElements;
        private bool m_bAreSoundsPaused;
        private Cv_SceneElement m_Scene;
        //private Cv_CameraNode m_Camera;
        //private Cv_Console m_Console;

        public Cv_PlayerView(PlayerIndex player)
        {
            m_ID = Cv_GameViewID.INVALID_GAMEVIEW;
            m_GameState = Cv_GameState.Initializing;
            RegisterEventListeners();

            PlayerIdx = player;
            m_SpriteBatch = new SpriteBatch(CaravelApp.Instance.GraphicsDevice);
            m_ScreenElements = new List<Cv_ScreenElement>();
            m_bAreSoundsPaused = false;
            m_Scene = new Cv_SceneElement(m_SpriteBatch);
            //m_Camera = new Cv_Camera()

            //m_Scene.AddNode(Cv_EntityID.INVALID_ENTITY, m_Camera);
            //m_Sece.Camera = m_Camera;
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

            m_SpriteBatch.Begin();

            foreach (var se in sortedElements)
            {
                if (se.IsVisible)
                {
                    se.VOnRender(time, timeElapsed);
                }
            }

            VRenderText();

            //m_Console.OnRender();
            
            m_SpriteBatch.End();
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