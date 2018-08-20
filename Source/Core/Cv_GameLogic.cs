﻿using System;
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
                    Cv_EventManager.Instance.AddListener<Cv_Event_RequestNewEntity>(OnRequestNewEntity);
                    GamePhysics = Cv_GamePhysics.CreateNullPhysics();
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
#endregion

        protected internal Cv_GameView[] GameViews
        {
            get { return m_GameViews.ToArray(); }
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
        private List<Cv_Entity> m_EntitiesToDestroy;
        private List<Cv_Entity> m_EntitiesToAdd;
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
            m_EntitiesToDestroy = new List<Cv_Entity>();
            m_EntitiesToAdd = new List<Cv_Entity>();
            m_EntityList = new List<Cv_Entity>();
            OnDestroyEntity = RequestDestroyEntityCallback;
            OnRequestNewEntity = RequestNewEntityCallback;
        }

        ~Cv_GameLogic()
        {
            Cv_EventManager.Instance.RemoveListener<Cv_Event_RequestDestroyEntity>(OnDestroyEntity);
        }

        internal void Init()
        {
            m_EntityFactory = VCreateEntityFactory();
           // m_SceneController.Init(Cv_ResourceManager.Instance.GetResourceList("scenes/*.xml"));
            Cv_EventManager.Instance.AddListener<Cv_Event_RequestDestroyEntity>(OnDestroyEntity);
            GamePhysics.VInitialize();
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

        public Cv_Entity CreateEntity(string entityTypeResource, string name, string resourceBundle, Cv_EntityID parentId = Cv_EntityID.INVALID_ENTITY, XmlElement overrides = null, Cv_Transform transform = null, Cv_EntityID serverEntityId = Cv_EntityID.INVALID_ENTITY)
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
                m_EntitiesToAdd.Add(entity);
                Entities.Add(entity.ID, entity);
				EntitiesByName.Add(entity.EntityName, entity);

                entity.PostInit();

                if (!IsProxy && State == Cv_GameState.Running)
                {
                    var requestNewEntityEvent = new Cv_Event_RequestNewEntity(entityTypeResource, entity.EntityName, resourceBundle, parentId, transform, entity.ID);
                    Cv_EventManager.Instance.TriggerEvent(requestNewEntityEvent);
                }

                LastEntityID = entity.ID;
                return entity;
            }

            Cv_Debug.Error("Could not create entity with resource: " + entityTypeResource);
            return null;
        }

        public Cv_Entity CreateEmptyEntity(string name, string resourceBundle, Cv_EntityID parentId = Cv_EntityID.INVALID_ENTITY, XmlElement overrides = null, Cv_Transform transform = null, Cv_EntityID serverEntityId = Cv_EntityID.INVALID_ENTITY)
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
                m_EntitiesToAdd.Add(entity);
                Entities.Add(entity.ID, entity);
				EntitiesByName.Add(entity.EntityName, entity);

                entity.PostInit();

                if (!IsProxy && State == Cv_GameState.Running)
                {
                    var requestNewEntityEvent = new Cv_Event_RequestNewEntity(null, entity.EntityName, resourceBundle, parentId, transform, entity.ID);
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
			Cv_Entity entity;
			if (Entities.TryGetValue(entityId, out entity))
			{
                foreach (var e in m_EntityList) //TODO(JM): this might get really slow with tons of entities. Optimize if it becomes a problem
                {
                    if (e.ID != entityId && e.Parent == entityId)
                    {
                        DestroyEntity(e.ID);
                    }
                }

                m_EntitiesToDestroy.Add(entity);

                var destroyEntityEvent = new Cv_Event_DestroyEntity(entityId);
                Cv_EventManager.Instance.TriggerEvent(destroyEntityEvent);
                Entities.Remove(entityId);
                EntitiesByName.Remove(entity.EntityName);
                entity.OnDestroy();
                entity.DestroyRequested = true;
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
                component.VPostInit();
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
                component.VPostInit();
            }
        }

        public void ChangeType(Cv_EntityID entityId, string type, string typeResource)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement typeNode = doc.CreateElement("Entity");

            typeNode.SetAttribute("type", type);
            
            var entity = GetEntity(entityId);

            if (entity != null)
            {
                entity.Init(typeResource, typeNode, entity.Parent);
            }
        }
        
#endregion

#region GameView methods
        public void AddView(Cv_GameView view, Cv_EntityID entityId = Cv_EntityID.INVALID_ENTITY)
        {
            Cv_GameViewID gvID = (Cv_GameViewID) m_GameViews.Count+1;

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

        public bool LoadScene(string sceneResource, string resourceBundle)
        {
            Cv_XmlResource resource;
			resource = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>(sceneResource, resourceBundle, CaravelApp.Instance.EditorRunning);
			
            var root = ((Cv_XmlResource.Cv_XmlData) resource.ResourceData).RootNode;

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

            var scriptElement = root.SelectNodes("//Script").Item(0);

            if (scriptElement != null)
            {
                preLoadScript = scriptElement.Attributes["preLoad"].Value;
                postLoadScript = scriptElement.Attributes["postLoad"].Value;
            }

            if (preLoadScript != null)
            {
                Cv_ScriptResource preLoadRes;
				preLoadRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(preLoadScript, resourceBundle);
            }

            var entitiesNodes = root.SelectNodes("StaticEntities/Entity");

            CreateNestedEntities(entitiesNodes, Cv_EntityID.INVALID_ENTITY, resourceBundle);

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
				Cv_ScriptResource postLoadRes;
				postLoadRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(postLoadScript, resourceBundle);
				
            }

			foreach (var e in Entities)
			{
				e.Value.PostLoad();
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

            var changedStateEvt = new Cv_Event_ChangeState(State, newState);

            State = newState;
            VGameOnChangeState(newState);
            Cv_EventManager.Instance.TriggerEvent(changedStateEvt);

            return true;
        }

        public void VRenderDiagnostics(Cv_Renderer renderer)
        {
            GamePhysics.VRenderDiagnostics(renderer);
        }

        public void AttachProcess(Cv_Process process)
        {
            
        }

#region Virtual methods that can be overriden by game logic class
        protected virtual void VGameOnUpdate(float time, float timeElapsed)
        {
        }

        protected virtual bool VGameOnPreLoadScene(XmlElement sceneData)
        {
            return true;
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
                    break;
                case Cv_GameState.WaitingForPlayers:
                    if (ExpectedPlayers + ExpectedRemotePlayers == HumanPlayersAttached)
                    {
                        //if (!string.IsNullOrEmpty(Caravel.GameOptions.Scene))
                        //{
                            ChangeState(Cv_GameState.LoadingScene);
                        //}*/
                    }
                    break;
                case Cv_GameState.LoadingScene:
                    if (!Caravel.VLoadGame()) //TODO(JM): Maybe change this to automatically load the scene set in the options instead of being overriden by subclass
                    {
                        Cv_Debug.Error("Error loading scene.");
                        Caravel.AbortGame();
                    }
                    else
                    {
                        HumanPlayersLoaded++; //TODO(JM): In future maybe change this to event handler (to handle remote players too)
                        ChangeState(Cv_GameState.WaitingForPlayersToLoadScene);
                    }
                    break;
                case Cv_GameState.WaitingForPlayersToLoadScene:
                    if (ExpectedPlayers + ExpectedRemotePlayers == HumanPlayersLoaded)
                    {
                        ChangeState(Cv_GameState.Running);
                    }
                    break;
                case Cv_GameState.Running:
                    //m_pProcessManager->UpdateProcesses(deltaMilliseconds);

                    if (GamePhysics != null && !IsProxy)
                    {
                        GamePhysics.VOnUpdate(timeElapsed);
                        GamePhysics.VSyncVisibleScene();
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

            foreach (var e in m_EntityList)
            {
                e.OnUpdate(timeElapsed);
            }

            foreach (var e in m_EntitiesToDestroy)
            {
                m_EntityList.Remove(e);
                e.OnRemove();
            }
            m_EntitiesToDestroy.Clear();

            foreach (var e in m_EntitiesToAdd)
            {
                m_EntityList.Add(e);
            }
            m_EntitiesToAdd.Clear();

            VGameOnUpdate(time, timeElapsed);
        }

#region Event callbacks

        private void RequestNewEntityCallback(Cv_Event eventData)
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
                entity = CreateEntity(data.EntityResource, data.EntityName, bundle, data.Parent, null, data.InitialTransform, data.ServerEntityID);
            }
            else
            {
                entity = CreateEmptyEntity(data.EntityName, bundle, data.Parent, null, data.InitialTransform, data.ServerEntityID);
            }

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

        private void CreateNestedEntities(XmlNodeList entities, Cv_EntityID parentId, string resourceBundle = null)
        {
             if (entities != null)
            {
                foreach(XmlNode e in entities)
                {
                    var entityTypeResource = e.Attributes["type"].Value;
					var name = e.Attributes?["name"].Value;
                    var entity = CreateEntity(entityTypeResource, name, resourceBundle, parentId, (XmlElement) e);

                    if (entity != null)
                    {
                        var newEntityEvent = new Cv_Event_NewEntity(entity.ID);
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
