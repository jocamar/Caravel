using System;
using System.Collections.Generic;
using System.Xml;
using Caravel.Debugging;

namespace Caravel.Core
{
    public class Cv_GameLogic
    {
        #region Type definitions
        public enum Cv_GameState
        {
            Invalid,
            Initializing,
            MainMenu,
            WaitingForPlayers,
            LoadingGameEnvironment,
            WaitingForPlayersToLoadEnvironment,
            SpawningPlayersActors,
            Running
        };

        public delegate void TransformEntityDelegate(Cv_EventData eventData);
        public delegate void RequestNewEntityDelegate(Cv_EventData eventData);
        public delegate void RequestDestroyEntityDelegate(Cv_EventData eventData);
        #endregion

        #region Public properties 
        public float Lifetime
        {
            get; private set;
        }

        public Cv_GameState State
        {
            get; private set;
        }

        public bool IsProxy
        {
            get
            {
                return m_bIsProxy;
            }

            set
            {
                m_bIsProxy = value;

                if (m_bIsProxy)
                {
                    //Caravel.EventManager.AddListener(VOnRequestNewEntity, Cv_EventType.RequestNewEntity);
                    //GamePhysics = new Cv_NullPhysics();
                }
            }
        }

        public bool CanRunScripts
        {
            get
            {
                return !IsProxy || State != Cv_GameState.Running;
            }
        }
        #endregion

        #region Protected properties
        protected int ExpectedPlayers
        {
            get; set;
        }

        protected int ExpectedRemotePlayers
        {
            get; set;
        }

        protected int ExpectedAI
        {
            get; set;
        }

        protected int HumanPlayersAttached
        {
            get; private set;
        }

        protected int AIPlayersAttached
        {
            get; private set;
        }

        protected int HumanPlayersLoaded
        {
            get; private set;
        }

        protected int RemotePlayerId
        {
            get; private set;
        }

        protected CaravelApp Caravel
        {
            get; private set;
        }

        protected Dictionary<int, Cv_Entity> Entities
        {
            get; private set;
        }

        protected Cv_GameView[] GameViews
        {
            get { return m_GameViews.ToArray(); }
        }

        protected Cv_GamePhysics GamePhysics
        {
            get; private set;
        }

        protected bool RenderDiagnostics
        {
            get; set;
        }

        protected int LastActorId
        {
            get; private set;
        }
        #endregion

        protected TransformEntityDelegate OnTransformEntity
        {
            get; private set;
        }

        protected RequestNewEntityDelegate OnRequestNewEntity
        {
            get; private set;
        }

        protected RequestDestroyEntityDelegate OnDestroyEntity
        {
            get; private set;
        }

        private Random m_random;
        private bool m_bIsProxy;
        private List<Cv_GameView> m_GameViews;
        private Cv_EntityFactory m_EntityFactory;
        private Cv_SceneController m_SceneController;

        public Cv_GameLogic(CaravelApp app)
        {
            Caravel = app;
            m_random = new Random();
            m_GameViews = new List<Cv_GameView>();
            m_SceneController = new Cv_SceneController();
            IsProxy = false;
            Lifetime = 0;
            ExpectedPlayers = 0;
            ExpectedAI = 0;
            ExpectedRemotePlayers = 0;
            HumanPlayersAttached = 0;
            HumanPlayersLoaded = 0;
            RemotePlayerId = 0;
            RenderDiagnostics = false;
            LastActorId = 0;
            State = Cv_GameState.Initializing;
            Entities = new Dictionary<int, Cv_Entity>();
            OnDestroyEntity = RequestDestroyEntityCallback;
            OnRequestNewEntity = RequestNewEntityCallback;
            OnTransformEntity = TransformEntityCallback;
        }

        public void Init()
        {
            m_EntityFactory = VCreateEntityFactory();
            //m_SceneController.Init();
            //Caravel.EventManager.AddListener(VOnDestroyEntity, Cv_EventType.RequestDestroyEntity);
        }

        #region Entity methods
        public Cv_Entity GetEntity(int entityId)
        {
            return null;
        }

        public Cv_Entity CreateEntity(string entityResource, XmlElement overrides, Cv_Transform transform, int serverEntityId = 0)
        {
            return null;
        }

        public void DestroyEntity(int entityId)
        {

        }

        public void ModifyEntity(int entityId, XmlElement overrides)
        {

        }

        public XmlElement GetEntityXML(int entityId)
        {
            return null;
        }

        public void TranformEntity(int entityId, Cv_Transform transform)
        {

        }
        #endregion

        #region GameView methods
        public void AddView(Cv_GameView view, int entityId = 0)
        {
            VGameOnAddView(view, entityId);
        }

        public void RemoveView(Cv_GameView view)
        {
            VGameOnRemoveView(view);
        }
        #endregion

        public void AddGamePhysics(Cv_GamePhysics physics)
        {
            GamePhysics = physics;
        }

        public bool LoadScene(string sceneResource)
        {
            VGameOnLoadScene(null);
            return true;
        }

        public void ChangeState(Cv_GameState newState)
        {
            VGameOnChangeState(newState);
        }

        public void VRenderDiagnostics()
        {

        }

        public void AttachProcess(Cv_Process process)
        {
            
        }

        #region Virtual methods that can be overriden by game logic class
        protected virtual void VGameOnUpdate(float time, float timeElapsed)
        {
        }

        protected virtual void VGameOnLoadScene(XmlElement sceneData)
        {
        }

        protected virtual void VGameOnChangeState(Cv_GameState newState)
        {
        }

        protected virtual void VGameOnAddView(Cv_GameView view, int entityId)
        {

        }

        protected virtual void VGameOnRemoveView(Cv_GameView view)
        {
            
        }

        protected virtual Cv_EntityFactory VCreateEntityFactory()
        {
            return null;
        }
        #endregion

        void OnUpdate(float time, float timeElapsed)
        {
            VGameOnUpdate(time, timeElapsed);
        }

        private void TransformEntityCallback(Cv_EventData eventData)
        {
            //Cv_EventData_TransformEntity data = (Cv_EventData_TransformEntity) eventData;
            //TransformEntity(data.EntityId, data.Transform);
        }

        private void RequestNewEntityCallback(Cv_EventData eventData)
        {
            Cv_Debug.Assert(IsProxy, "Should only enter RequestNewEntityCallback when game logic is a proxy.");
            if (!IsProxy)
            {
                return;
            }

            //Cv_EventData_RequestNewEntity data = (Cv_EventData_NewEntity) eventData;
            //var entity = CreateEntity(data.EntityResource, null, DataMisalignedException.InitialTransform, data.ServerEntityId);
            //if (entity)
            //{
            //    var newEvent = new Cv_EventData_NewEntity(entity.ID, data.ViewID);
            //    Caravel.EventManager.QueueEvent(newEvent);
            //}
        }

        private void RequestDestroyEntityCallback(Cv_EventData eventData)
        {
            //Cv_EventData_DestroyEntity data = (Cv_EventData_DestroyEntity) eventData;
            //DestroyEntity(data.EntityId);
        }
    }
}
