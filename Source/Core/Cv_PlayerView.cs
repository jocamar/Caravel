using System.Collections.Generic;
using System.Xml;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Caravel.Core.Cv_GameLogic;
using static Caravel.Core.Entity.Cv_Entity;
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

        public Cv_Player PlayerIdx
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

        public bool DebugDrawClickableAreas
        {
            get
            {
                return Renderer.DebugDrawClickAreas;
            }
            
            set
            {
                Renderer.DebugDrawClickAreas = value;
            }
        }

        public bool DebugDrawFPS
        {
            get; set;
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

        public Cv_Entity ListenerEntity
        {
            get; set;
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
        private Cv_ListenerList m_Listeners = new Cv_ListenerList();

        public Cv_PlayerView(Cv_Player player, Vector2? size, Vector2 startPos, SpriteBatch spriteBatch = null)
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

            ListenerEntity = null;

            Scene = CaravelApp.Instance.Scene;
        }

        ~Cv_PlayerView()
        {
            RemoveEventListeners();
        }

        public void PushScreenElement(Cv_ScreenElement element)
        {
            lock (ScreenElements)
            {
                ScreenElements.Add(element);
            }
        }

        public void RemoveScreenElement(Cv_ScreenElement element)
        {
            lock (ScreenElements)
            {
                ScreenElements.Remove(element);
            }
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

        public bool Pick(Vector2 screenPoint, out Cv_EntityID[] entities)
        {
            return Scene.Pick(screenPoint, out entities, Renderer);
        }

        public bool Pick<NodeType>(Vector2 screenPoint, out Cv_EntityID[] entities) where NodeType : Cv_SceneNode
        {
            return Scene.Pick<NodeType>(screenPoint, out entities, Renderer);
        }

        public Vector2? GetWorldCoords(Vector2 screenPoint)
        {
            var scaledPosition = Renderer.ScaleScreenToViewCoordinates(screenPoint);
			
			if (scaledPosition.X >= 0 && scaledPosition.X <= Renderer.Viewport.Width
					&& scaledPosition.Y >= 0 && scaledPosition.Y <= Renderer.Viewport.Height)
			{
				var camMatrix = Renderer.CamMatrix;

                var invertedTransform = Matrix.Invert(camMatrix);
                var worldPoint = Vector2.Transform(scaledPosition, invertedTransform);

                return worldPoint;
			}

            return null;
        }

        public Vector2? GetScreenCoords(Vector2 worldPoint)
        {
            var camMatrix = Renderer.CamMatrix;
            var screenPoint = Vector2.Transform(worldPoint, camMatrix);

            var scaledPosition = Renderer.ScaleScreenToViewCoordinates(screenPoint);
			
			if (scaledPosition.X >= 0 && scaledPosition.X <= Renderer.Viewport.Width
					&& scaledPosition.Y >= 0 && scaledPosition.Y <= Renderer.Viewport.Height)
			{
                return scaledPosition;
			}

            return null;
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

        protected internal override void VOnRender(float time, float elapsedTime)
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

            Renderer.SetupViewport();

            lock (ScreenElements)
            {
                foreach (var se in ScreenElements)
                {
                    if (se.IsVisible)
                    {
                        if (se == Scene)
                        {
                            Scene.Camera = Camera;
                        }

                        se.VOnRender(time, elapsedTime, Renderer);
                    }
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
            if (Camera != null)
            {
                Camera.IsViewTransformDirty = false;
            }

            Scene.VOnPostRender(Renderer);
        }

        protected internal override void VOnUpdate(float time, float elapsedTime)
        {
            //m_Console.OnUpdate(time, elapsedTime);

            lock (ScreenElements)
            {
                foreach (var se in ScreenElements)
                {
                    se.VOnUpdate(time, elapsedTime);
                }
            }
        }

        internal void LoadGame(XmlElement sceneData)
        {
            lock(ScreenElements)
            {
                if (!ScreenElements.Contains(Scene)) {
                    PushScreenElement(Scene);
                    
                    if (DebugDrawFPS)
                    {
                        var framerateCounter = new Cv_FramerateCounterElement();
                        framerateCounter.IsVisible = true;
                        PushScreenElement(framerateCounter);
                    }
                }
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

            if (Camera != null)
            {
			    Camera.RecalculateTransformationMatrices();
            }
		}

        private void RegisterEventListeners()
        {
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_ChangeState>(OnGameState);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_NewCameraComponent>(OnNewCameraComponent);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_PlaySound>(OnPlaySound);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_StopSound>(OnStopSound);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_PauseSound>(OnPauseSound);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_ResumeSound>(OnResumeSound);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_StopAllSounds>(OnStopAllSounds);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_PauseAllSounds>(OnPauseAllSounds);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_ResumeAllSounds>(OnResumeAllSounds);
        }

        private void RemoveEventListeners()
        {
            m_Listeners.Dispose();
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

            var entity = CaravelApp.Instance.Logic.GetEntity(resumeEvt.EntityID);

            Caravel.SoundManager.ResumeSound(resumeEvt.SoundResource, entity);
        }

        private void OnPauseSound(Cv_Event eventData)
        {
            var pauseEvt = (Cv_Event_PauseSound) eventData;

            var entity = CaravelApp.Instance.Logic.GetEntity(pauseEvt.EntityID);

            Caravel.SoundManager.ResumeSound(pauseEvt.SoundResource, entity);
        }

        private void OnStopSound(Cv_Event eventData)
        {
            var stopEvt = (Cv_Event_StopSound) eventData;

            var entity = CaravelApp.Instance.Logic.GetEntity(stopEvt.EntityID); 

            Caravel.SoundManager.StopSound(stopEvt.SoundResource, entity);
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
                    Caravel.SoundManager.FadeInSound2D(playEvt.SoundResource, entity, playEvt.Listener, playEvt.Emitter,
															playEvt.Interval, playEvt.Looping, playEvt.Volume, playEvt.Pan, playEvt.Pitch);
                }
                else
                {
                    Caravel.SoundManager.FadeInSound(playEvt.SoundResource, entity, playEvt.Interval, playEvt.Looping,
																								playEvt.Volume, playEvt.Pan, playEvt.Pitch);
                }
            }
            else
            {
                if (entity == null)
                {
                    Cv_Debug.Warning("Attempting to play sound without an entity associated.");
                    return;
                }

                if (playEvt.Localized)
                {
                    Caravel.SoundManager.PlaySound2D(playEvt.SoundResource, entity, playEvt.Listener, playEvt.Emitter,
																			playEvt.Looping, playEvt.Volume, playEvt.Pan, playEvt.Pitch);
                }
                else
                {
                    Caravel.SoundManager.PlaySound(playEvt.SoundResource, entity, playEvt.Looping,
																			playEvt.Volume, playEvt.Pan, playEvt.Pitch);
                }
            }
        }
    }
}