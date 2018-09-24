using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Core.Physics;
using Caravel.Core.Resource;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using static Caravel.Core.Cv_GameView;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Events.Cv_EventManager;
using static Caravel.Core.Physics.Cv_FarseerPhysics;
using static Caravel.Core.Physics.Cv_GamePhysics;
using static Caravel.Core.Resource.Cv_XmlResource;

namespace Caravel.Core
{
    public class Cv_GameLogic
    {
        public enum Cv_GameState
        {
            Invalid,
            Initializing,
            WaitingForPlayers,
            LoadingScene,
            WaitingForPlayersToLoadScene,
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
                    Cv_EventManager.Instance.AddListener<Cv_Event_RequestNewEntity>(OnNewEntityRequest);
                    GamePhysics = Cv_GamePhysics.CreateNullPhysics(Caravel);
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

        public CaravelApp Caravel
        {
            get; private set;
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

        protected Dictionary<Cv_EntityID, Cv_Entity> Entities
        {
            get; private set;
        }

		protected Dictionary<string, Cv_Entity> EntitiesByName
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

        protected internal Cv_GamePhysics GamePhysics
        {
            get; private set;
        }

		protected internal Cv_GameView[] GameViews
        {
            get { return m_GameViews.ToArray(); }
        }
#endregion

        private Random m_random;
        private bool m_bIsProxy;
        private List<Cv_GameView> m_GameViews;
        private Cv_EntityFactory m_EntityFactory;
        private Cv_SceneController m_SceneController;
        private ConcurrentQueue<Cv_Entity> m_EntitiesToDestroy;
        private ConcurrentQueue<Cv_Entity> m_EntitiesToAdd;
        private List<Cv_Entity> m_EntityList;

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
            m_EntitiesToDestroy = new ConcurrentQueue<Cv_Entity>();
            m_EntitiesToAdd = new ConcurrentQueue<Cv_Entity>();
            m_EntityList = new List<Cv_Entity>();
        }

        ~Cv_GameLogic()
        {
            Cv_EventManager.Instance.RemoveListener<Cv_Event_RequestDestroyEntity>(OnDestroyEntityRequest);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_SceneLoaded>(OnSceneLoaded);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_RemoteSceneLoaded>(OnSceneLoaded);
        }

#region Entity methods
        public Cv_Entity GetEntity(Cv_EntityID entityId)
        {
            Cv_Entity ent = null;
            lock(Entities)
            {
                Entities.TryGetValue(entityId, out ent);
            }

            return ent;
        }

		public Cv_Entity GetEntity(string entityName)
        {
            Cv_Entity ent = null;
            lock(Entities)
            {
                EntitiesByName.TryGetValue(entityName, out ent);
            }

            return ent;
        }

        public Cv_Entity CreateEntity(string entityTypeResource, string name, string resourceBundle, bool visible = true,
                                        Cv_EntityID parentId = Cv_EntityID.INVALID_ENTITY, XmlElement overrides = null,
                                        Cv_Transform? transform = null,Cv_EntityID serverEntityId = Cv_EntityID.INVALID_ENTITY)
        {
            Cv_Debug.Assert(m_EntityFactory != null, "Entity factory should not be null.");
            Cv_Debug.Assert(name != null, "Entity must have a name.");
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

            var entity = m_EntityFactory.CreateEntity(entityTypeResource, parentId, overrides, transform, serverEntityId, resourceBundle);

            if (entity != null)
            {
				entity.EntityName = name;
                entity.Visible = visible;
                m_EntitiesToAdd.Enqueue(entity);

                lock(Entities)
                {
                    Entities.Add(entity.ID, entity);
                    EntitiesByName.Add(entity.EntityName, entity);
                }

                entity.PostInitialize();
                
                if (overrides != null)
                {
                    m_EntityFactory.ModifyEntity(entity, overrides.SelectNodes("./*[not(self::Entity)]"));
                }

                var tranformComponent = entity.GetComponent<Cv_TransformComponent>();
                if (tranformComponent != null && transform != null)
                {
                    tranformComponent.Transform = transform.Value;
                }

                if (!IsProxy && State == Cv_GameState.Running)
                {
                    var requestNewEntityEvent = new Cv_Event_RequestNewEntity(entityTypeResource, entity.EntityName, resourceBundle, visible, parentId, transform, entity.ID);
                    Cv_EventManager.Instance.TriggerEvent(requestNewEntityEvent);
                }

                LastEntityID = entity.ID;
                return entity;
            }

            Cv_Debug.Error("Could not create entity with resource: " + entityTypeResource);
            return null;
        }

        public Cv_Entity CreateEmptyEntity(string name, string resourceBundle, bool visible = true,
                                            Cv_EntityID parentId = Cv_EntityID.INVALID_ENTITY, XmlElement overrides = null,
                                            Cv_Transform? transform = null, Cv_EntityID serverEntityId = Cv_EntityID.INVALID_ENTITY)
        {
            Cv_Debug.Assert(m_EntityFactory != null, "Entity factory should not be null.");
            Cv_Debug.Assert(name != null, "Entity must have a name.");
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

            var entity = m_EntityFactory.CreateEmptyEntity(resourceBundle, parentId, overrides, transform, serverEntityId);

            if (entity != null)
            {
				entity.EntityName = name;
                entity.Visible = visible;
                m_EntitiesToAdd.Enqueue(entity);
                
                lock(Entities)
                {
                    Entities.Add(entity.ID, entity);
                    EntitiesByName.Add(entity.EntityName, entity);
                }

                entity.PostInitialize();

                if (overrides != null)
                {
                    m_EntityFactory.ModifyEntity(entity, overrides.SelectNodes("./*[not(self::Entity)]"));
                }

                var tranformComponent = entity.GetComponent<Cv_TransformComponent>();
                if (tranformComponent != null && transform != null)
                {
                    tranformComponent.Transform = transform.Value;
                }

                if (!IsProxy && State == Cv_GameState.Running)
                {
                    var requestNewEntityEvent = new Cv_Event_RequestNewEntity(null, entity.EntityName, resourceBundle, visible, parentId, transform, entity.ID);
                    Cv_EventManager.Instance.TriggerEvent(requestNewEntityEvent);
                }

                LastEntityID = entity.ID;
                return entity;
            }

            Cv_Debug.Error("Could not create empty entity.");
            return null;
        }

        public void DestroyEntity(Cv_EntityID entityId)
        {
            lock(m_EntityList)
            lock(Entities)
            {
                Cv_Entity entity;
                var entityExists = false;
                
                entityExists = Entities.TryGetValue(entityId, out entity);

                if (entityExists)
                {
                    foreach (var e in m_EntityList) //TODO(JM): this might get really slow with tons of entities. Optimize if it becomes a problem
                    {
                        if (e.ID != entityId && e.Parent == entityId)
                        {
                            DestroyEntity(e.ID);
                        }
                    }

                    m_EntitiesToDestroy.Enqueue(entity);

                    var destroyEntityEvent = new Cv_Event_DestroyEntity(entityId, this);
                    Cv_EventManager.Instance.TriggerEvent(destroyEntityEvent);

                    Entities.Remove(entityId);
                    EntitiesByName.Remove(entity.EntityName);

                    entity.OnDestroy();
                    entity.DestroyRequested = true;
                }
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
            
            lock(Entities)
            {
                Entities.TryGetValue(entityId, out ent);
            }

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

        public void RemoveComponent<Component>(Cv_EntityID entityId) where Component : Cv_EntityComponent
        {
            var entity = GetEntity(entityId);

            if (entity != null)
            {
                entity.RemoveComponent<Component>();
            }
        }

        public void RemoveComponent<Component>(string entityName) where Component : Cv_EntityComponent
        {
            var entity = GetEntity(entityName);

            if (entity != null)
            {
                entity.RemoveComponent<Component>();
            }
        }

        public void RemoveComponent(Cv_EntityID entityId, string componentName)
        {
            var entity = GetEntity(entityId);

            if (entity != null)
            {
                entity.RemoveComponent(componentName);
            }
        }

        public Component CreateComponent<Component>() where Component : Cv_EntityComponent
        {
            return m_EntityFactory.CreateComponent<Component>();
        }

        public Cv_EntityComponent CreateComponent(string componentName)
        {
            return m_EntityFactory.CreateComponent(componentName);
        }

        public void AddComponent(Cv_EntityID entityId, Cv_EntityComponent component)
        {
            if (component.Owner != null)
            {
                Cv_Debug.Error("Trying to add a component that already has an owner.");
            }

            var entity = GetEntity(entityId);

            if (entity != null)
            {
                entity.AddComponent(component);
                component.VPostInitialize();
            }
        }

        public void AddComponent(string entityName, Cv_EntityComponent component)
        {
            if (component.Owner != null)
            {
                Cv_Debug.Error("Trying to add a component that already has an owner.");
            }
            
            var entity = GetEntity(entityName);

            if (entity != null)
            {
                entity.AddComponent(component);
                component.VPostInitialize();
            }
        }

        public void ChangeType(Cv_EntityID entityId, string type, string typeResource)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement typeNode = doc.CreateElement("EntityType");

            typeNode.SetAttribute("type", type);
            
            var entity = GetEntity(entityId);

            if (entity != null)
            {
                entity.Initialize(typeResource, typeNode, entity.Parent);
            }
        }

        public void ChangeName(Cv_EntityID entityId, string newName)
        {
            var entity = GetEntity(entityId);

            lock(Entities)
            {
                if (entity != null && !EntitiesByName.ContainsKey(newName))
                {
                    EntitiesByName.Remove(entity.EntityName);
                    entity.EntityName = newName;
                    EntitiesByName.Add(newName, entity);
                }
            }
        }
        
#endregion

#region GameView methods
        public void AddView(Cv_GameView view, Cv_EntityID entityId = Cv_EntityID.INVALID_ENTITY)
        {
            Cv_GameViewID gvID = (Cv_GameViewID) m_GameViews.Count+1;

            view.Initialize(Caravel);

            lock(m_GameViews)
            {
                m_GameViews.Add(view);
            }

            view.VOnAttach(gvID, entityId);
            VGameOnAddView(view, entityId);
        }

        public void RemoveView(Cv_GameView view)
        {
            lock(m_GameViews)
            {
                m_GameViews.Remove(view);
            }

            VGameOnRemoveView(view);
        }
#endregion

#region Scene methods
        public bool LoadScene(string sceneResource, string resourceBundle)
        {
            Cv_XmlResource resource;
			resource = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>(sceneResource, resourceBundle, Caravel.EditorRunning);
			
            var root = ((Cv_XmlData) resource.ResourceData).RootNode;

            if (root == null)
            {
                Cv_Debug.Error("Failed to load scene resource file: " + sceneResource);
                return false;
            }

            if (!VGameOnPreLoadScene(root))
            {
                return false;
            }

            string preLoadScript = null;
            string postLoadScript = null;

            var scriptElement = root.SelectNodes("Script").Item(0);

            if (scriptElement != null)
            {
                preLoadScript = scriptElement.Attributes["preLoad"].Value;
                postLoadScript = scriptElement.Attributes["postLoad"].Value;
            }

            if (preLoadScript != null && preLoadScript != "")
            {
                Cv_ScriptResource preLoadRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(preLoadScript, resourceBundle);
                preLoadRes.RunScript();
            }

            var entitiesNodes = root.SelectNodes("StaticEntities/Entity");

            CreateNestedEntities(entitiesNodes, Cv_EntityID.INVALID_ENTITY, resourceBundle);

            lock(m_GameViews)
            {
                foreach(var gv in m_GameViews)
                {
                    if (gv.Type == Cv_GameViewType.Player)
                    {
                        var playerView = (Cv_PlayerView) gv;
                        playerView.LoadGame(root);
                    }
                }
            }

            if (!VGameOnLoadScene(root))
            {
                return false;
            }

            if (postLoadScript != null && postLoadScript != "")
            {
				Cv_ScriptResource postLoadRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(postLoadScript, resourceBundle);
				postLoadRes.RunScript();
            }

			foreach (var e in Entities)
			{
				e.Value.PostLoad();
			}

            if (IsProxy)
            {
                var remoteSceneLoadedEvent = new Cv_Event_RemoteSceneLoaded(sceneResource, resourceBundle, this);
                Cv_EventManager.Instance.TriggerEvent(remoteSceneLoadedEvent);
            }
            else
            {
                var sceneLoadedEvent = new Cv_Event_SceneLoaded(sceneResource, resourceBundle, this);
                Cv_EventManager.Instance.TriggerEvent(sceneLoadedEvent);
            }

            return true;
        }

        public void UnloadScene(string sceneResource, string resourceBundle)
        {
            Cv_XmlResource resource;
			resource = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>(sceneResource, resourceBundle, Caravel.EditorRunning);

            var root = ((Cv_XmlResource.Cv_XmlData) resource.ResourceData).RootNode;

            if (root == null)
            {
                Cv_Debug.Error("Failed to unload scene resource file: " + sceneResource);
                return;
            }

            VGameOnPreUnloadScene(root);

            string unloadScript = null;

            var scriptElement = root.SelectNodes("Script").Item(0);

            if (scriptElement != null)
            {
                unloadScript = scriptElement.Attributes["unLoad"].Value;
            }

            if (unloadScript != null)
            {
                Cv_ScriptResource unLoadRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(unloadScript, resourceBundle);
                unLoadRes.RunScript();
            }

            var entitiesNodes = root.SelectNodes("StaticEntities/Entity");

            foreach (XmlElement entity in entitiesNodes)
            {
                var e = GetEntity(entity.Attributes["name"].Value);

                if (e != null)
                {
                    DestroyEntity(e.ID);
                }
            }

            VGameOnUnloadScene(root);

            if (IsProxy)
            {
                var remoteSceneUnloadedEvent = new Cv_Event_RemoteSceneUnloaded(sceneResource, resourceBundle, this);
                Cv_EventManager.Instance.TriggerEvent(remoteSceneUnloadedEvent);
            }
            else
            {
                var sceneUnloadedEvent = new Cv_Event_SceneUnloaded(sceneResource, resourceBundle, this);
                Cv_EventManager.Instance.TriggerEvent(sceneUnloadedEvent);
            }
        }
#endregion

#region Physics methods
        public void AddGamePhysics(Cv_GamePhysics physics)
        {
            GamePhysics = physics;
        }

		public Cv_Entity[] RayCast(Vector2 startingPoint, Vector2 endingPoint, Cv_RayCastType type)
		{
			if (GamePhysics != null)
			{
				return GamePhysics.RayCast(startingPoint, endingPoint, type);
			}

			return new Cv_Entity[0];
		}
#endregion

        public bool ChangeState(Cv_GameState newState)
        {
            if (newState == Cv_GameState.WaitingForPlayers)
            {
                ExpectedPlayers = 1; //NOTE(JM): this must change for splitscreen
                //ExpectedRemotePlayers = Caravel.GameOptions.ExpectedPlayers - ExpectedPlayers;
                //ExpectedAI = Caravel.GameOptions.NumAI;
                HumanPlayersAttached = 0;
                HumanPlayersLoaded = 0;

                /*if (!string.IsNullOrEmpty(Caravel.GameOptions.GameHost))
                {
                    IsProxy = true;
                    ExpectedAI = 0;
                    ExpectedRemotePlayers = 0;

                    if (!Caravel.AttachAsClient())
                    {
                        return false;
                    }
                }
                else if (ExpectedRemotePlayers > 0)
                {
                    //Add socket on GameOptions.ListenPort
                }*/
            }

            var changedStateEvt = new Cv_Event_ChangeState(State, newState, this);

            State = newState;
            VGameOnChangeState(newState);
            Cv_EventManager.Instance.TriggerEvent(changedStateEvt);

            if (newState == Cv_GameState.WaitingForPlayersToLoadScene)
            {
                HumanPlayersLoaded++; //TODO(JM): In future maybe change this to event handler (to handle remote players too)
            }
            
            if (newState == Cv_GameState.LoadingScene)
            {
                if (!Caravel.VLoadGame()) //TODO(JM): Maybe change this to automatically load the scene set in the options instead of being overriden by subclass
                {
                    Cv_Debug.Error("Error - Unable to load scene.");
                    Caravel.AbortGame();
                }
            }

            return true;
        }

#region Virtual methods that can be overriden by game logic class
        protected virtual void VGameOnUpdate(float time, float elapsedTime)
        {
        }

        protected virtual void VGameOnPreUnloadScene(XmlElement sceneData)
        {
        }

        protected virtual bool VGameOnPreLoadScene(XmlElement sceneData)
        {
            return true;
        }

        protected virtual void VGameOnUnloadScene(XmlElement sceneData)
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

		internal void VRenderDiagnostics(Cv_CameraNode camera, Cv_Renderer renderer)
        {
            if (camera == null)
            {
                return;
            }
            
            GamePhysics.VRenderDiagnostics(camera, renderer);
        }

        internal void Initialize()
        {
            m_EntityFactory = VCreateEntityFactory();
           // m_SceneController.Initialize(Cv_ResourceManager.Instance.GetResourceList("scenes/*.xml"));
            Cv_EventManager.Instance.AddListener<Cv_Event_RequestDestroyEntity>(OnDestroyEntityRequest);
            Cv_EventManager.Instance.AddListener<Cv_Event_SceneLoaded>(OnSceneLoaded);
            Cv_EventManager.Instance.AddListener<Cv_Event_RemoteSceneLoaded>(OnSceneLoaded);
            GamePhysics.VInitialize();
        }

		internal void OnUpdate(float time, float elapsedTime)
        {
            switch (State)
            {
                case Cv_GameState.Initializing:
                    break;
                case Cv_GameState.WaitingForPlayers:
                    if (ExpectedPlayers + ExpectedRemotePlayers <= HumanPlayersAttached)
                    {
                        //if (!string.IsNullOrEmpty(Caravel.GameOptions.Scene))
                        //{
                            ChangeState(Cv_GameState.LoadingScene);
                        //}*/
                    }
                    break;
                case Cv_GameState.LoadingScene:
                    break;
                case Cv_GameState.WaitingForPlayersToLoadScene:
                    if (ExpectedPlayers + ExpectedRemotePlayers <= HumanPlayersLoaded)
                    {
                        ChangeState(Cv_GameState.Running);
                    }
                    break;
                case Cv_GameState.Running:
                    if (GamePhysics != null && !IsProxy)
                    {
                        GamePhysics.VOnUpdate(elapsedTime);
                        GamePhysics.VSyncVisibleScene();
                    }
                    break;
                default:
                    Cv_Debug.Error("Unrecognized state.");
                    break;
            }
            
            foreach (var gv in GameViews)
            {
                gv.VOnUpdate(time, elapsedTime);
            }

            foreach (var e in m_EntityList)
            {
                e.OnUpdate(elapsedTime);
            }

            Cv_Entity toRemove = null;
            while (m_EntitiesToDestroy.TryDequeue(out toRemove))
            {
                lock(m_EntityList)
                {
                    m_EntityList.Remove(toRemove);
                }
                toRemove.OnRemove();
            }

            Cv_Entity toAdd = null;
            while (m_EntitiesToAdd.TryDequeue(out toAdd))
            {
                lock(m_EntityList)
                {
                    m_EntityList.Add(toAdd);
                }
            }

            VGameOnUpdate(time, elapsedTime);
        }

#region Event callbacks

        private void OnNewEntityRequest(Cv_Event eventData)
        {
            Cv_Debug.Assert(IsProxy, "Should only enter RequestNewEntityCallback when game logic is a proxy.");
            if (!IsProxy)
            {
                return;
            }

            Cv_Event_RequestNewEntity data = (Cv_Event_RequestNewEntity) eventData;
            Cv_Entity entity;
            var bundle = data.EntityResourceBundle;
            if (data.EntityResource != null)
            {
                entity = CreateEntity(data.EntityResource, data.EntityName, bundle, data.Visible, data.Parent, null, data.InitialTransform, data.ServerEntityID);
            }
            else
            {
                entity = CreateEmptyEntity(data.EntityName, bundle, data.Visible, data.Parent, null, data.InitialTransform, data.ServerEntityID);
            }

            if (entity != null)
            {
                var newEvent = new Cv_Event_NewEntity(entity.ID, data.GameViewID);
                Cv_EventManager.Instance.QueueEvent(newEvent);
            }
        }

        private void OnDestroyEntityRequest(Cv_Event eventData)
        {
            Cv_Event_RequestDestroyEntity data = (Cv_Event_RequestDestroyEntity) eventData;
            DestroyEntity(data.EntityID);
        }

        private void OnSceneLoaded(Cv_Event eventData)
        {
            if (State == Cv_GameState.LoadingScene)
            {
                ChangeState(Cv_GameState.WaitingForPlayersToLoadScene);
            }
        }
#endregion

        private void CreateNestedEntities(XmlNodeList entities, Cv_EntityID parentId, string resourceBundle = null)
        {
             if (entities != null)
            {
                foreach(XmlNode e in entities)
                {
                    var entityTypeResource = e.Attributes["type"].Value;
					var name = e.Attributes?["name"].Value;
                    var visible = true;

                    if (e.Attributes["visible"] != null)
                    {
                        visible = bool.Parse(e.Attributes["visible"].Value);
                    }

                    Cv_Entity entity;
                    if (entityTypeResource != "")
                    {
                        entity = CreateEntity(entityTypeResource, name, resourceBundle, visible, parentId, (XmlElement) e);
                    }
                    else
                    {
                        entity = CreateEmptyEntity(name, resourceBundle, visible, parentId, (XmlElement) e);
                    }

                    if (entity != null)
                    {
                        var newEntityEvent = new Cv_Event_NewEntity(entity.ID, this);
                        Cv_EventManager.Instance.QueueEvent(newEntityEvent);
                    }

                    var childEntities = e.SelectNodes("./Entity");

                    if (childEntities.Count > 0)
                    {
                        CreateNestedEntities(childEntities, entity.ID, resourceBundle);
                    }
                }
            }
        }
    }
}
