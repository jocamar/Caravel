using System;
using System.Collections.Generic;
using System.Xml;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Core.Resource;
using Caravel.Debugging;
using static Caravel.Core.Cv_GameView;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Events.Cv_EventManager;

namespace Caravel.Core
{
    public class Cv_GameLogic
    {
        public enum Cv_GameState
        {
            Invalid,
            Initializing,
            MainMenu,
            WaitingForPlayers,
            LoadingGameEnvironment,
            WaitingForPlayersToLoadEnvironment,
            SpawningPlayerEntities,
            Running
        };

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
                    Cv_EventManager.Instance.AddListener<Cv_Event_RequestNewEntity>(OnRequestNewEntity);
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

        protected Dictionary<Cv_EntityID, Cv_Entity> Entities
        {
            get; private set;
        }

		protected Dictionary<string, Cv_Entity> EntitiesByName
		{
			get; private set;
		}

        protected Cv_GamePhysics GamePhysics
        {
            get; private set;
        }

        protected bool RenderDiagnostics
        {
            get; set;
        }

        protected Cv_EntityID LastEntityID
        {
            get; private set;
        }
#endregion

        protected internal Cv_GameView[] GameViews
        {
            get { return m_GameViews.ToArray(); }
        }

        protected NewEventDelegate OnTransformEntity
        {
            get; private set;
        }

        protected NewEventDelegate OnRequestNewEntity
        {
            get; private set;
        }

        protected NewEventDelegate OnDestroyEntity
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
            LastEntityID = 0;
            State = Cv_GameState.Initializing;
            Entities = new Dictionary<Cv_EntityID, Cv_Entity>();
			EntitiesByName = new Dictionary<string, Cv_Entity>();
            OnDestroyEntity = RequestDestroyEntityCallback;
            OnRequestNewEntity = RequestNewEntityCallback;
            OnTransformEntity = TransformEntityCallback;
        }

        ~Cv_GameLogic()
        {
            Cv_EventManager.Instance.RemoveListener<Cv_Event_RequestDestroyEntity>(OnDestroyEntity);
        }

        internal void Init()
        {
            m_EntityFactory = VCreateEntityFactory();
            m_SceneController.Init(Cv_ResourceManager.Instance.GetResourceList("scenes/*.xml"));
            Cv_EventManager.Instance.AddListener<Cv_Event_RequestDestroyEntity>(OnDestroyEntity);
        }

#region Entity methods
        public Cv_Entity GetEntity(Cv_EntityID entityId)
        {
            Cv_Entity ent = null;
            Entities.TryGetValue(entityId, out ent);

            return ent;
        }

		public Cv_Entity GetEntity(string entityName)
        {
            Cv_Entity ent = null;
            EntitiesByName.TryGetValue(entityName, out ent);

            return ent;
        }

        public Cv_Entity CreateEntity(string entityTypeResource, string name = null, Cv_EntityID parentId = Cv_EntityID.INVALID_ENTITY, XmlElement overrides = null, Cv_Transform transform = null, Cv_EntityID serverEntityId = Cv_EntityID.INVALID_ENTITY)
        {
            Cv_Debug.Assert(m_EntityFactory != null, "Entity factory should not be null.");
            if (m_EntityFactory == null)
            {
                return null;
            }

            if (!IsProxy && serverEntityId != Cv_EntityID.INVALID_ENTITY)
            {
                return null;
            }
            else if (IsProxy && serverEntityId == Cv_EntityID.INVALID_ENTITY)
            {
                return null;
            }

            var entity = m_EntityFactory.CreateEntity(entityTypeResource, parentId, overrides, transform, serverEntityId);

            if (entity != null)
            {
				if (name != null)
				{
					entity.EntityName = name;
				}

                Entities.Add(entity.ID, entity);
				EntitiesByName.Add(entity.EntityName, entity);

                if (!IsProxy && State == Cv_GameState.SpawningPlayerEntities || State == Cv_GameState.Running)
                {
                    var requestNewEntityEvent = new Cv_Event_RequestNewEntity(entityTypeResource, entity.EntityName, parentId, transform, entity.ID);
                    Cv_EventManager.Instance.TriggerEvent(requestNewEntityEvent);
                }

                LastEntityID = entity.ID;
                return entity;
            }

            Cv_Debug.Error("Could not create entity with resource: " + entityTypeResource);
            return null;
        }

        public void DestroyEntity(Cv_EntityID entityId)
        {
            var destroyEntityEvent = new Cv_Event_DestroyEntity(entityId);
            Cv_EventManager.Instance.TriggerEvent(destroyEntityEvent);

			Cv_Entity entity;
			if (Entities.TryGetValue(entityId, out entity))
			{
				Entities.Remove(entityId);
				EntitiesByName.Remove(entity.EntityName);
			}
            
        }

        public void ModifyEntity(Cv_EntityID entityId, XmlNodeList overrides)
        {
            Cv_Debug.Assert(m_EntityFactory != null, "Entity factory should not be null.");

            if (m_EntityFactory == null)
            {
                return;
            }

            Cv_Entity ent = null;
            Entities.TryGetValue(entityId, out ent);

            if (ent != null)
            {
                m_EntityFactory.ModifyEntity(ent, overrides);
            }
        }

        public XmlElement GetEntityXML(Cv_EntityID entityId)
        {
            var entity = GetEntity(entityId);

            if (entity != null)
            {
                return entity.ToXML();
            }
            
            Cv_Debug.Error("Could not find entity with ID: " + (int) entityId);
            return null;
        }

        public void TransformEntity(Cv_EntityID entityId, Cv_Transform transform)
        {

        }
#endregion

#region GameView methods
        public void AddView(Cv_GameView view, Cv_EntityID entityId = 0)
        {
            Cv_GameViewID gvID = (Cv_GameViewID) m_GameViews.Count;

            m_GameViews.Add(view);
            view.VOnAttach(gvID, entityId);
            VGameOnAddView(view, entityId);
        }

        public void RemoveView(Cv_GameView view)
        {
            m_GameViews.Remove(view);
            VGameOnRemoveView(view);
        }
#endregion

        public void AddGamePhysics(Cv_GamePhysics physics)
        {
            GamePhysics = physics;
        }

        public bool LoadScene(string sceneResource)
        {
            var resource = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>(sceneResource);
            var root = ((Cv_XmlResource.Cv_XmlData) resource.ResourceData).RootNode;

            if (root == null)
            {
                Cv_Debug.Error("Failed to load scene resource file: " + sceneResource);
                return false;
            }

            string preLoadScript = null;
            string postLoadScript = null;

            var scriptElement = root.SelectNodes("//Script").Item(0);

            if (scriptElement != null)
            {
                preLoadScript = scriptElement.Attributes["preLoad"].Value;
                postLoadScript = scriptElement.Attributes["postLoad"].Value;
            }

            if (preLoadScript != null)
            {
                var preLoadRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(preLoadScript);
            }

            var entitiesNodes = root.SelectNodes("StaticEntities/Entity");

            CreateNestedEntities(entitiesNodes, Cv_EntityID.INVALID_ENTITY);

            foreach(var gv in m_GameViews)
            {
                if (gv.Type == Cv_GameViewType.Player)
                {
                    var playerView = (Cv_PlayerView) gv;
                    playerView.LoadGame(root);
                }
            }

            if (!VGameOnLoadScene(root))
            {
                return false;
            }

            if (postLoadScript != null)
            {
                var postLoadRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(postLoadScript);
            }

            if (IsProxy)
            {
                //var remoteSceneLoadedEvent = new Cv_Event_RemoteSceneLoaded();
                //Cv_EventManager.Instance.TriggerEvent(remoteSceneLoadedEvent);
            }
            else
            {
                //var sceneLoadedEvent = new Cv_Event_SceneLoaded();
                //Cv_EventManager.Instance.TriggerEvent(sceneLoadedEvent);
            }

            return true;
        }

        public void ChangeState(Cv_GameState newState)
        {
            //var changedStateEvt = new Cv_Event_NewState(State, newState);

            if (newState == Cv_GameState.WaitingForPlayers)
            {
                ExpectedPlayers = 1; //NOTE(JM): this must change for splitscreen
                //ExpectedRemotePlayers = Caravel.GameOptions.ExpectedPlayers - ExpectedPlayers;
                //ExpectedAI = Caravel.GameOptions.NumAI;

                /*if (!string.IsNullOrEmpty(Caravel.GameOptions.GameHost))
                {
                    IsProxy = true;
                    ExpectedAI = 0;
                    ExpectedRemotePlayers = 0;

                    if (!Caravel.AttachAsClient())
                    {
                        ChangeState(Cv_GameState.MainMenu);
                        return;
                    }
                }
                else if (ExpectedRemotePlayers > 0)
                {
                    //Add socket on GameOptions.ListenPort
                }*/
            }
            else if (newState == Cv_GameState.LoadingGameEnvironment)
            {
                State = newState;
                VGameOnChangeState(newState);
                //Cv_EventManager.Instance.TriggerEvent(changedStateEvt);

                if (!Caravel.VLoadGame())
                {
                    Cv_Debug.Error("Error loading game.");
                    Caravel.AbortGame();
                    return;
                }
                else
                {
                    ChangeState(Cv_GameState.WaitingForPlayersToLoadEnvironment);
                    return;
                }
            }

            State = newState;
            VGameOnChangeState(newState);
            //Cv_EventManager.Instance.TriggerEvent(changedStateEvt);
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

        protected virtual bool VGameOnLoadScene(XmlElement sceneData)
        {
            return true;
        }

        protected virtual void VGameOnChangeState(Cv_GameState newState)
        {
        }

        protected virtual void VGameOnAddView(Cv_GameView view, Cv_EntityID entityId)
        {

        }

        protected virtual void VGameOnRemoveView(Cv_GameView view)
        {
            
        }

        protected virtual Cv_EntityFactory VCreateEntityFactory()
        {
            return new Cv_EntityFactory();
        }
#endregion

        internal void OnUpdate(float time, float timeElapsed)
        {
            switch (State)
            {
                case Cv_GameState.Initializing:
                    ChangeState(Cv_GameState.MainMenu);
                    break;
                case Cv_GameState.MainMenu:
                    break;
                case Cv_GameState.LoadingGameEnvironment:
                    break;
                case Cv_GameState.WaitingForPlayersToLoadEnvironment:
                    if (ExpectedPlayers + ExpectedRemotePlayers == HumanPlayersLoaded)
                    {
                        ChangeState(Cv_GameState.SpawningPlayerEntities);
                    }
                    break;
                case Cv_GameState.SpawningPlayerEntities:
                    ChangeState(Cv_GameState.Running);
                    break;
                case Cv_GameState.WaitingForPlayers:
                    if (ExpectedPlayers + ExpectedRemotePlayers == HumanPlayersAttached)
                    {
                        //if (!string.IsNullOrEmpty(Caravel.GameOptions.Level))
                        //{
                            ChangeState(Cv_GameState.LoadingGameEnvironment);
                        //}*/
                    }
                    break;
                case Cv_GameState.Running:
                    //m_pProcessManager->UpdateProcesses(deltaMilliseconds);
                    //TODO(JM): Update ProcessManager in Caravel Update

                    if (GamePhysics != null && !IsProxy)
                    {
                        //GamePhysics.VOnUpdate(timeElapsed);
                        //GamePhysics.SyncVisibleScene();
                    }
                    break;
                default:
                    Cv_Debug.Error("Unrecognized state.");
                    break;
            }

            foreach (var gv in GameViews)
            {
                gv.VOnUpdate(time, timeElapsed);
            }

            foreach (var e in Entities)
            {
                e.Value.OnUpdate(timeElapsed);
            }

            VGameOnUpdate(time, timeElapsed);
        }

#region Event callbacks
        private void TransformEntityCallback(Cv_Event eventData)
        {
            Cv_Event_TransformEntity data = (Cv_Event_TransformEntity) eventData;
            TransformEntity(data.EntityID, data.Transform);
        }

        private void RequestNewEntityCallback(Cv_Event eventData)
        {
            Cv_Debug.Assert(IsProxy, "Should only enter RequestNewEntityCallback when game logic is a proxy.");
            if (!IsProxy)
            {
                return;
            }

            Cv_Event_RequestNewEntity data = (Cv_Event_RequestNewEntity) eventData;
            var entity = CreateEntity(data.EntityResource, data.EntityName, data.Parent, null, data.InitialTransform, data.ServerEntityID);
            if (entity != null)
            {
                var newEvent = new Cv_Event_NewEntity(entity.ID, data.GameViewID);
                Cv_EventManager.Instance.QueueEvent(newEvent);
            }
        }

        private void RequestDestroyEntityCallback(Cv_Event eventData)
        {
            Cv_Event_RequestDestroyEntity data = (Cv_Event_RequestDestroyEntity) eventData;
            DestroyEntity(data.EntityID);
        }
#endregion

        private void CreateNestedEntities(XmlNodeList entities, Cv_EntityID parentId)
        {
             if (entities != null)
            {
                foreach(XmlNode e in entities)
                {
                    var entityResource = e.Attributes["resource"].Value;
					var name = e.Attributes?["name"].Value;
                    var entity = CreateEntity(entityResource, name, parentId, (XmlElement) e);

                    if (entity != null)
                    {
                        var newEntityEvent = new Cv_Event_NewEntity(entity.ID);
                        Cv_EventManager.Instance.QueueEvent(newEntityEvent);
                    }

                    var childEntities = e.SelectNodes("./Entity");

                    if (childEntities.Count > 0)
                    {
                        CreateNestedEntities(childEntities, entity.ID);
                    }
                }
            }
        }
    }
}
