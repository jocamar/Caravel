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
using static Caravel.Core.Draw.Cv_Renderer;
using Caravel.Debugging;

namespace Caravel.Core
{
    public class Cv_PlayerView : Cv_GameView
    {
        public Cv_EntityID EditorSelectedEntity
        {
            get
            {
                return Scene.EditorSelectedEntity;
            }
            
            set
            {
                Scene.EditorSelectedEntity = value;
            }
        }

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

        public bool DebugDrawPhysicsShapes
        {
            get
            {
                return Renderer.DebugDrawPhysicsShapes;
            }
            
            set
            {
                Renderer.DebugDrawPhysicsShapes = value;
            }
        }

        public bool DebugDrawPhysicsBoundingBoxes
        {
            get
            {
                return Renderer.DebugDrawPhysicsBoundingBoxes;
            }
            
            set
            {
                Renderer.DebugDrawPhysicsBoundingBoxes = value;
            }
        }

        public bool DebugDrawRadius
        {
            get
            {
                return Renderer.DebugDrawRadius;
            }
            
            set
            {
                Renderer.DebugDrawRadius = value;
            }
        }

        public bool DebugDrawCameras
        {
            get
            {
                return Renderer.DebugDrawCameras;
            }
            
            set
            {
                Renderer.DebugDrawCameras = value;
            }
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
                    var pauseEvt = new Cv_Event_PauseAllSounds(Cv_EntityID.INVALID_ENTITY, this);
                    Caravel.EventManager.QueueEvent(pauseEvt);
                }
                else
                {
                    var resumeEvt = new Cv_Event_ResumeAllSounds(Cv_EntityID.INVALID_ENTITY, this);
                    Caravel.EventManager.QueueEvent(resumeEvt);
                }
            }
        }

		public Cv_BlendState Blend
		{
			get
			{
				return Renderer.Blend;
			}

			set
			{
				Renderer.Blend = value;
			}
		}

		public Cv_SamplerState Sampling
		{
			get
			{
				return Renderer.Sampling;
			}

			set
			{
				Renderer.Sampling = value;
			}
		}

        protected Cv_SceneElement Scene
        {
            get; private set;
        }

        protected Cv_EntityID EntityID
        {
            get; private set;
        }
        
        protected Cv_GameState GameState
        {
            get; private set;
        }

        protected bool RunFullSpeed
        {
            get; private set;
        }

        //protected Cv_Console Console;

        protected Cv_Renderer Renderer
        {
            get; private set;
        }

        protected List<Cv_ScreenElement> ScreenElements
        {
            get; private set;
        }

        private Cv_GameViewType m_Type = Cv_GameViewType.Player;
        private Cv_GameViewID m_ID;
        private bool m_bAreSoundsPaused;

        public Cv_PlayerView(PlayerIndex player, Vector2? size, Vector2 startPos, SpriteBatch spriteBatch = null)
        {
            m_ID = Cv_GameViewID.INVALID_GAMEVIEW;
            GameState = Cv_GameState.Initializing;
            RegisterEventListeners();

            PlayerIdx = player;
            ScreenElements = new List<Cv_ScreenElement>();
            m_bAreSoundsPaused = false;

            Renderer = new Cv_Renderer(spriteBatch);

            if (size == null)
            {
                Renderer.ScreenSizePercent = new Vector2(1,1);
                Renderer.ScreenWidth = CaravelApp.Instance.Graphics.PreferredBackBufferWidth;
                Renderer.ScreenHeight = CaravelApp.Instance.Graphics.PreferredBackBufferHeight;
            }
            else
            {
                Renderer.ScreenSizePercent = size.Value;
                Renderer.ScreenWidth = (int) (size.Value.X * CaravelApp.Instance.Graphics.PreferredBackBufferWidth);
                Renderer.ScreenHeight = (int) (size.Value.Y * CaravelApp.Instance.Graphics.PreferredBackBufferHeight);
            }

            Renderer.ScreenOriginPercent = startPos;
            Renderer.VirtualWidth = Renderer.ScreenWidth;
            Renderer.VirtualHeight = Renderer.ScreenHeight;
            Renderer.StartX = (int) (startPos.X * CaravelApp.Instance.Graphics.PreferredBackBufferWidth);
            Renderer.StartY = (int) (startPos.Y * CaravelApp.Instance.Graphics.PreferredBackBufferHeight);
            Renderer.Initialize();

            Cv_DrawUtils.Initialize();

            Scene = CaravelApp.Instance.Scene;
        }

        ~Cv_PlayerView()
        {
            RemoveEventListeners();
        }

        public void PushScreenElement(Cv_ScreenElement element)
        {
            ScreenElements.Add(element);
        }

        public void RemoveScreenElement(Cv_ScreenElement element)
        {
            ScreenElements.Remove(element);
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
            Scene.PrintTree();
        }

        public bool Pick(Vector2 mousePos, out Cv_EntityID[] entities)
        {
            return Scene.Pick(mousePos, out entities, Renderer);
        }


        protected internal virtual void VRenderText()
        {
        }

        protected internal virtual void VOnLoadGame(XmlElement sceneData)
        {
        }

        protected internal override void VOnAttach(Cv_GameViewID id, Cv_EntityID entityId)
        {
            m_ID = id;
            EntityID = entityId;
        }

        protected internal override void VOnRender(float time, float timeElapsed)
        {
            if (RunFullSpeed && Caravel.IsFixedTimeStep)
            {
                Caravel.IsFixedTimeStep = false;
                Caravel.Graphics.SynchronizeWithVerticalRetrace = false;
                Caravel.Graphics.ApplyChanges();
            }
            else if (!Caravel.IsFixedTimeStep)
            {
                Caravel.IsFixedTimeStep = true;
                Caravel.Graphics.SynchronizeWithVerticalRetrace = true;
                Caravel.Graphics.ApplyChanges();
            }

            var sortedElements = ScreenElements.OrderBy(e => e).ToList();

            Renderer.SetupViewport();
            foreach (var se in sortedElements)
            {
                if (se.IsVisible)
                {
                    if (se == Scene)
                    {
                        Scene.Camera = Camera;
                    }

                    se.VOnRender(time, timeElapsed, Renderer);
                }
            }

            VRenderText();

            Renderer.BeginDraw(Camera);
            Caravel.Logic.VRenderDiagnostics(Camera, Renderer);
            Renderer.EndDraw();

            //m_Console.OnRender();

            Renderer.ResetViewport();
        }

        protected internal override void VOnPostRender()
        {
            Camera.IsViewTransformDirty = false;

            Scene.VOnPostRender(Renderer);
        }

        protected internal override void VOnUpdate(float time, float timeElapsed)
        {
            //m_Console.OnUpdate(time, timeElapsed);

            foreach (var se in ScreenElements)
            {
                se.VOnUpdate(time, timeElapsed);
            }
        }

        internal void LoadGame(XmlElement sceneData)
        {
            if (!ScreenElements.Contains(Scene)) {
                PushScreenElement(Scene);
            }

            Scene.IsVisible = true;
			
			if (sceneData.Attributes["vWidth"] != null)
			{
				Renderer.VirtualWidth = int.Parse(sceneData.Attributes["vWidth"].Value);
			}

			if (sceneData.Attributes["vHeight"] != null)
			{
				Renderer.VirtualHeight = int.Parse(sceneData.Attributes["vHeight"].Value);
			}

            VOnLoadGame(sceneData);
        }

        internal void SetPlayerEntity(Cv_EntityID entityId)
        {
            EntityID = entityId;
        }

        internal void OnGameState(Cv_Event eventData)
        {
            var newStateEvt = (Cv_Event_ChangeState) eventData;

            GameState = newStateEvt.NewState;
        }

        internal void OnWindowResize(int newWidth, int newHeight)
		{
			Renderer.ScreenWidth = (int) (newWidth * Renderer.ScreenSizePercent.X);
			Renderer.ScreenHeight = (int) (newHeight * Renderer.ScreenSizePercent.Y);
            Renderer.StartX = (int) (newWidth * Renderer.ScreenOriginPercent.X);
            Renderer.StartY = (int) (newHeight * Renderer.ScreenOriginPercent.Y);
			Camera.RecalculateTransformationMatrices();
		}

        private void RegisterEventListeners()
        {
            Cv_EventManager.Instance.AddListener<Cv_Event_ChangeState>(OnGameState);
            Cv_EventManager.Instance.AddListener<Cv_Event_NewCameraComponent>(OnNewCameraComponent);
            Cv_EventManager.Instance.AddListener<Cv_Event_PlaySound>(OnPlaySound);
            Cv_EventManager.Instance.AddListener<Cv_Event_StopSound>(OnStopSound);
            Cv_EventManager.Instance.AddListener<Cv_Event_PauseSound>(OnPauseSound);
            Cv_EventManager.Instance.AddListener<Cv_Event_ResumeSound>(OnResumeSound);
            Cv_EventManager.Instance.AddListener<Cv_Event_StopAllSounds>(OnStopAllSounds);
            Cv_EventManager.Instance.AddListener<Cv_Event_PauseAllSounds>(OnPauseAllSounds);
            Cv_EventManager.Instance.AddListener<Cv_Event_ResumeAllSounds>(OnResumeAllSounds);
        }

        private void RemoveEventListeners()
        {
            Cv_EventManager.Instance.RemoveListener<Cv_Event_ChangeState>(OnGameState);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_NewCameraComponent>(OnNewCameraComponent);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_PlaySound>(OnPlaySound);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_StopSound>(OnStopSound);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_PauseSound>(OnPauseSound);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_ResumeSound>(OnResumeSound);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_StopAllSounds>(OnStopAllSounds);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_PauseAllSounds>(OnPauseAllSounds);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_ResumeAllSounds>(OnResumeAllSounds);
        }

        private void OnResumeAllSounds(Cv_Event eventData)
        {
            Caravel.SoundManager.ResumeAllSounds();
        }

        private void OnPauseAllSounds(Cv_Event eventData)
        {
            Caravel.SoundManager.PauseAllSounds();
        }

        private void OnStopAllSounds(Cv_Event eventData)
        {
            Caravel.SoundManager.StopAllSounds();
        }

        private void OnResumeSound(Cv_Event eventData)
        {
            var resumeEvt = (Cv_Event_ResumeSound) eventData;
            Caravel.SoundManager.ResumeSound(resumeEvt.SoundResource);
        }

        private void OnPauseSound(Cv_Event eventData)
        {
            var pauseEvt = (Cv_Event_PauseSound) eventData;
            Caravel.SoundManager.ResumeSound(pauseEvt.SoundResource);
        }

        private void OnStopSound(Cv_Event eventData)
        {
            var stopEvt = (Cv_Event_StopSound) eventData;
            Caravel.SoundManager.StopSound(stopEvt.SoundResource);
        }

        private void OnPlaySound(Cv_Event eventData)
        {
            var playEvt = (Cv_Event_PlaySound) eventData;

            var entity = CaravelApp.Instance.Logic.GetEntity(playEvt.EntityID);
            
            if (playEvt.Fade && playEvt.Interval > 0)
            {
                if (playEvt.Volume <= 0)
                {
                    Caravel.SoundManager.FadeOutSound(playEvt.SoundResource, playEvt.Interval);
                    return;
                }

                if (entity == null)
                {
                    Cv_Debug.Error("Attempting to play sound without an entity associated.");
                    return;
                }

                if (playEvt.Localized)
                {
                    Caravel.SoundManager.FadeInSound2D(playEvt.SoundResource, entity.ResourceBundle, playEvt.Listener, playEvt.Emitter,
															playEvt.Interval, playEvt.Looping, playEvt.Volume, playEvt.Pan, playEvt.Pitch);
                }
                else
                {
                    Caravel.SoundManager.FadeInSound(playEvt.SoundResource, entity.ResourceBundle, playEvt.Interval, playEvt.Looping,
																								playEvt.Volume, playEvt.Pan, playEvt.Pitch);
                }
            }
            else
            {
                if (entity == null)
                {
                    Cv_Debug.Error("Attempting to play sound without an entity associated.");
                    return;
                }

                if (playEvt.Localized)
                {
                    Caravel.SoundManager.PlaySound2D(playEvt.SoundResource, entity.ResourceBundle, playEvt.Listener, playEvt.Emitter,
																			playEvt.Looping, playEvt.Volume, playEvt.Pan, playEvt.Pitch);
                }
                else
                {
                    Caravel.SoundManager.PlaySound(playEvt.SoundResource, entity.ResourceBundle, playEvt.Looping,
																			playEvt.Volume, playEvt.Pan, playEvt.Pitch);
                }
            }
        }
    }
}