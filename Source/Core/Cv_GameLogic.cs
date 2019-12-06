using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Core.Physics;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using static Caravel.Core.Cv_GameView;
using static Caravel.Core.Cv_SceneManager;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Entity.Cv_EntityComponent;
using static Caravel.Core.Physics.Cv_GamePhysics;

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

		protected Dictionary<string, Cv_Entity> EntitiesByPath
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
        private Cv_SceneManager m_SceneManager;
        private ConcurrentQueue<Cv_Entity> m_EntitiesToDestroy;
        private ConcurrentQueue<Cv_Entity> m_EntitiesToAdd;
        private List<Cv_Entity> m_EntityList;
        private Cv_ListenerList m_Listeners = new Cv_ListenerList();

        public Cv_GameLogic(CaravelApp app)
        {
            Caravel = app;
            m_random = new Random();
            m_GameViews = new List<Cv_GameView>();
            m_SceneManager = new Cv_SceneManager(app);
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
			EntitiesByPath = new Dictionary<string, Cv_Entity>();
            m_EntitiesToDestroy = new ConcurrentQueue<Cv_Entity>();
            m_EntitiesToAdd = new ConcurrentQueue<Cv_Entity>();
            m_EntityList = new List<Cv_Entity>();
        }

        ~Cv_GameLogic()
        {
            m_Listeners.Dispose();
        }

#region Entity methods
        public Cv_Entity[] GetSceneEntities(Cv_SceneID sceneID)
        {
            var entityList = new List<Cv_Entity>();

            lock(Entities)
            {
                foreach (var e in Entities)
                {
                    if (e.Value.SceneID == sceneID)
                    {
                        entityList.Add(e.Value);
                    }
                }
            }

            return entityList.ToArray();
        }

        public Cv_Entity[] GetEntities(string pattern)
        {
            var entityList = new List<Cv_Entity>();
            Regex mask = new Regex(pattern.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));

            lock(Entities)
            {
                foreach (var e in Entities)
                {
                    if (mask.IsMatch(e.Value.EntityPath))
                    {
                        entityList.Add(e.Value);
                    }
                }
            }

            return entityList.ToArray();
        }

        public Cv_Entity GetEntity(Cv_EntityID entityID)
        {
            Cv_Entity ent = null;
            lock(Entities)
            {
                Entities.TryGetValue(entityID, out ent);
            }

            return ent;
        }

		public Cv_Entity GetEntity(string entityPath)
        {
            Cv_Entity ent = null;

            lock(Entities)
            {
                var path = entityPath;

                if (entityPath.Length > 0 && entityPath[0] != '/') {
                    path = "/" + entityPath;
                }
                
                EntitiesByPath.TryGetValue(path, out ent);
            }

            return ent;
        }

        public Cv_Entity CreateEntity(string entityTypeResource,
                                        string name,
                                        string resourceBundle,
                                        bool visible = true,
                                        Cv_EntityID parentID = Cv_EntityID.INVALID_ENTITY,
                                        XmlElement overrides = null,
                                        Cv_Transform? transform = null,
                                        Cv_SceneID sceneID = Cv_SceneID.INVALID_SCENE,
                                        Cv_EntityID serverEntityID = Cv_EntityID.INVALID_ENTITY)
        {
            var entity = InstantiateNewEntity(false, entityTypeResource, name, resourceBundle, visible, parentID,
                                            overrides, transform, sceneID, serverEntityID);
            entity.PostLoad();
            return entity;
        }

        public Cv_Entity CreateEmptyEntity(string name,
                                            string resourceBundle,
                                            bool visible = true,
                                            Cv_EntityID parentID = Cv_EntityID.INVALID_ENTITY,
                                            XmlElement overrides = null,
                                            Cv_Transform? transform = null,
                                            Cv_SceneID sceneID = Cv_SceneID.INVALID_SCENE,
                                            Cv_EntityID serverEntityID = Cv_EntityID.INVALID_ENTITY)
        {
            var entity = InstantiateNewEntity(false, null, name, resourceBundle, visible, parentID,
                                            overrides, transform, sceneID, serverEntityID);
            entity.PostLoad();
            return entity;
        }

        public void DestroyEntity(Cv_EntityID entityID)
        {
            lock(m_EntityList)
            lock(Entities)
            {
                Cv_Entity entity = null;
                var entityExists = false;
                
                entityExists = Entities.TryGetValue(entityID, out entity);

                if (entityExists && !entity.DestroyRequested)
                {
                    if (entity.SceneRoot)
                    {
                        UnloadScene(entity.SceneID);
                        return;
                    }

                    DestroyEntity(entity);
                }
            }
        }

        public void ModifyEntity(Cv_EntityID entityID, XmlNodeList overrides)
        {
            Cv_Debug.Assert(m_EntityFactory != null, "Entity factory should not be null.");

            if (m_EntityFactory == null)
            {
                return;
            }

            Cv_Entity ent = null;
            
            lock(Entities)
            {
                Entities.TryGetValue(entityID, out ent);
            }

            if (ent != null)
            {
                var components = m_EntityFactory.ModifyEntity(ent, overrides);

                foreach (var component in components)
                {
                    component.VPostInitialize();
                    component.VPostLoad();
                }
            }
        }

        public XmlElement GetEntityXML(Cv_EntityID entityID)
        {
            var entity = GetEntity(entityID);

            if (entity != null)
            {
                return entity.ToXML();
            }
            
            Cv_Debug.Error("Could not find entity with ID: " + (int) entityID);
            return null;
        }

        public void RemoveComponent<Component>(Cv_EntityID entityID) where Component : Cv_EntityComponent
        {
            var entity = GetEntity(entityID);

            if (entity != null)
            {
                entity.RemoveComponent<Component>();
            }
        }

        public void RemoveComponent<Component>(string entityPath) where Component : Cv_EntityComponent
        {
            var entity = GetEntity(entityPath);

            if (entity != null)
            {
                entity.RemoveComponent<Component>();
            }
        }

        public void RemoveComponent(Cv_EntityID entityID, string componentName)
        {
            var entity = GetEntity(entityID);

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

        public void AddComponent(Cv_EntityID entityID, Cv_EntityComponent component)
        {
            if (component.Owner != null)
            {
                Cv_Debug.Error("Trying to add a component that already has an owner.");
            }

            var entity = GetEntity(entityID);

            if (entity != null)
            {
                entity.AddComponent(component);
                component.VPostInitialize();
                component.VPostLoad();
            }
        }

        public void AddComponent(string entityPath, Cv_EntityComponent component)
        {
            if (component.Owner != null)
            {
                Cv_Debug.Error("Trying to add a component that already has an owner.");
            }
            
            var entity = GetEntity(entityPath);

            if (entity != null)
            {
                entity.AddComponent(component);
                component.VPostInitialize();
                component.VPostLoad();
            }
        }

        //Note(JM): Used for editor
        #if EDITOR
        public void AddComponent(string entityPath, string componentTypeName, Cv_EntityComponent component)
        {
            if (component.Owner != null)
            {
                Cv_Debug.Error("Trying to add a component that already has an owner.");
            }
            
            var entity = GetEntity(entityPath);

            if (entity != null)
            {
                entity.AddComponent(componentTypeName, component);
                component.VPostInitialize();
                component.VPostLoad();
            }
        }
        #endif

        public void ChangeType(Cv_EntityID entityID, string type, string typeResource)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement typeNode = doc.CreateElement("EntityType");

            typeNode.SetAttribute("type", type);
            
            var entity = GetEntity(entityID);

            if (entity != null)
            {
                entity.Initialize(typeResource, typeNode, entity.Parent);
            }
        }

        public void ChangeName(Cv_EntityID entityID, string newName)
        {
            var entity = GetEntity(entityID);

            lock(Entities)
            {
                var parentPath = "";

                if (entity != null)
                {
                    var parent = GetEntity(entity.Parent);
                    if (parent != null)
                    {
                        parentPath = parent.EntityPath;
                    }

                    var path = "/" + newName;

                    if (entity.SceneRoot)
                    {
                        path = "/" + entity.SceneName + path;
                    }

                    var newPath = parentPath + path;
                    if (!EntitiesByPath.ContainsKey(newPath))
                    {
                        EntitiesByPath.Remove(entity.EntityPath);
                        entity.EntityName = newName;
                        entity.EntityPath = newPath;
                        EntitiesByPath.Add(newPath, entity);

                        //Make sure to propagate path change to all descendants
                        List<Cv_Entity> childrenToProcess = new List<Cv_Entity>();
                        childrenToProcess.AddRange(entity.Children);
                        while(childrenToProcess.Count > 0)
                        {
                            var child = childrenToProcess[childrenToProcess.Count-1];

                            var childParent = GetEntity(child.Parent);

                            var childPath = "/" + child.EntityName;

                            if (child.SceneRoot)
                            {
                                path = "/" + child.SceneName + path;
                            }

                            var newChildPath = childParent.EntityPath + path;

                            EntitiesByPath.Remove(child.EntityPath);
                            child.EntityPath = newChildPath;
                            EntitiesByPath.Add(newChildPath, entity);

                            childrenToProcess.RemoveAt(childrenToProcess.Count-1);
                            childrenToProcess.AddRange(child.Children);
                        }
                    }
                }
            }
        }

        internal XmlElement GetComponentInfo(Cv_ComponentID componentID)
        {
            return m_EntityFactory.GetComponentInfo(componentID);
        }
        
#endregion

#region GameView methods
        public void AddView(Cv_GameView view, Cv_EntityID entityID = Cv_EntityID.INVALID_ENTITY)
        {
            Cv_GameViewID gvID = (Cv_GameViewID) m_GameViews.Count+1;

            view.Initialize(Caravel);

            lock(m_GameViews)
            {
                m_GameViews.Add(view);
            }

            view.VOnAttach(gvID, entityID);
            VGameOnAddView(view, entityID);
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
        public string[] GetLoadedScenes()
        {
            var sceneInfo = m_SceneManager.Scenes;

            var scenePaths = new List<string>();

            foreach (var info in sceneInfo)
            {
                scenePaths.Add(info.ScenePath);
            }

            return scenePaths.ToArray();
        }

        public Cv_SceneID GetSceneID(string scenePath)
        {
            return m_SceneManager.GetSceneID(scenePath);
        }

        public string GetScenePath(Cv_SceneID sceneID)
        {
            return m_SceneManager.GetScenePath(sceneID);
        }

        public Cv_Entity GetSceneRoot(Cv_SceneID sceneID)
        {
            return m_SceneManager.GetSceneRoot(sceneID);
        }

        public string GetSceneResource(Cv_SceneID sceneID)
        {
            return m_SceneManager.GetSceneResource(sceneID);
        }

        public bool IsSceneLoaded(string scenePath)
        {
            return m_SceneManager.IsSceneLoaded(scenePath);
        }

        public void SetMainScene(Cv_SceneID sceneID)
        {
            var path = GetScenePath(sceneID);
            if (IsSceneLoaded(path))
            {
                m_SceneManager.MainScene = sceneID;

                if (!IsSceneLoaded(path))
                {
                    Cv_Debug.Warning("Main scene unloaded after being set.");
                }
            }
            else
            {
                Cv_Debug.Error("Trying to set a scene that is not loaded as the main scene.");
            }
        }

        public Cv_SceneID LoadScene(string sceneResource, string resourceBundle, string sceneName, XmlElement overrides = null,
                                Cv_Transform? sceneTransform = null, Cv_EntityID parentID = Cv_EntityID.INVALID_ENTITY)
        {
            var sceneEntities = m_SceneManager.LoadScene(sceneResource, resourceBundle, sceneName, overrides, sceneTransform, parentID);

            if (sceneEntities == null || sceneEntities.Length <= 0)
            {  
                return Cv_SceneID.INVALID_SCENE;
            }

            var sceneID = sceneEntities[0].SceneID;

            lock(Entities)
            {
                foreach (var e in sceneEntities)
                {
                    e.PostLoad();
                }
            }

            if (IsProxy)
            {
                var remoteSceneLoadedEvent = new Cv_Event_RemoteSceneLoaded(sceneResource, sceneID, sceneName, resourceBundle, this);
                Cv_EventManager.Instance.TriggerEvent(remoteSceneLoadedEvent);
            }
            else
            {
                var sceneLoadedEvent = new Cv_Event_SceneLoaded(sceneResource, sceneID, sceneName, resourceBundle, this);
                Cv_EventManager.Instance.TriggerEvent(sceneLoadedEvent);
            }

            return sceneID;
        }

        public void UnloadScene(Cv_SceneID sceneID = Cv_SceneID.INVALID_SCENE)
        {
            var scene = m_SceneManager.MainScene;
            if (sceneID != Cv_SceneID.INVALID_SCENE)
            {
                scene = sceneID;
            }
            else if (scene == Cv_SceneID.INVALID_SCENE)
            {
                return;
            }

            m_SceneManager.UnloadScene(m_SceneManager.GetScenePath(scene));
        }

        public void UnloadScene(string scenePath = null)
        {
            var scene = m_SceneManager.GetScenePath(m_SceneManager.MainScene);
            if (scenePath != null)
            {
                scene = scenePath;

                if (scene.Length > 0 && scene[0] != '/') {
                    scene = "/" + scene;
                }
            }
            else if (scene == null)
            {
                return;
            }

            m_SceneManager.UnloadScene(scene);
        }

        internal bool UnloadAllScenes()
        {
            return m_SceneManager.UnloadAllScenes();
        }

        internal bool UnloadScenes(string[] sceneIDs)
        {
            return m_SceneManager.UnloadScenes(sceneIDs);
        }

        internal bool UnloadScenes(string sceneExpression)
        {
            return m_SceneManager.UnloadScenes(sceneExpression);
        }
#endregion

#region Physics methods
        public void AddGamePhysics(Cv_GamePhysics physics)
        {
            GamePhysics = physics;
        }

		public Cv_RayCastIntersection[] RayCast(Vector2 startingPoint, Vector2 endingPoint, Cv_RayCastType type)
		{
			if (GamePhysics != null)
			{
				return GamePhysics.RayCast(startingPoint, endingPoint, type);
			}

			return new Cv_RayCastIntersection[0];
		}

        public Cv_PhysicsMaterial GetMaterial(string materialId)
        {
            return GamePhysics.GetMaterial(materialId);
        }

        public string[] GetMaterials()
        {
            return GamePhysics.GetMaterials();
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

        protected virtual void VGameOnPreUnloadScene(XmlElement sceneData, Cv_SceneID sceneID, string sceneName)
        {
        }

        protected virtual bool VGameOnPreLoadScene(XmlElement sceneData, string sceneName)
        {
            return true;
        }

        protected virtual void VGameOnUnloadScene(XmlElement sceneData, Cv_SceneID sceneID, string sceneName)
        {
        }

        protected virtual bool VGameOnLoadScene(XmlElement sceneData, Cv_SceneID sceneID, string sceneName)
        {
            return true;
        }

        protected virtual void VGameOnChangeState(Cv_GameState newState)
        {
        }

        protected virtual void VGameOnAddView(Cv_GameView view, Cv_EntityID entityID)
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
            m_SceneManager.Initialize(/*Cv_ResourceManager.Instance.GetResourceList("scenes/*.xml")*/);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_RequestDestroyEntity>(OnDestroyEntityRequest);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_SceneLoaded>(OnSceneLoaded);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_RemoteSceneLoaded>(OnSceneLoaded);
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
                if (!e.DestroyRequested) {
                    e.OnUpdate(elapsedTime);
                }
            }

            Cv_Entity toAdd = null;
            while (m_EntitiesToAdd.TryDequeue(out toAdd))
            {
                lock (m_EntityList)
                {
                    m_EntityList.Add(toAdd);
                }
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

            VGameOnUpdate(time, elapsedTime);
        }

        internal bool OnPreLoadScene(XmlElement sceneData, string sceneName)
        {
            return VGameOnPreLoadScene(sceneData, sceneName);
        }

        internal bool OnLoadScene(XmlElement sceneData, Cv_SceneID sceneID, string sceneName)
        {
            lock(m_GameViews)
            {
                foreach(var gv in m_GameViews)
                {
                    if (gv.Type == Cv_GameViewType.Player)
                    {
                        var playerView = (Cv_PlayerView) gv;
                        playerView.LoadGame(sceneData);
                    }
                }
            }

            if (!VGameOnLoadScene(sceneData, sceneID, sceneName))
            {
                return false;
            }

            return true;
        }

        internal bool OnPreUnloadScene(XmlElement sceneData, Cv_SceneID sceneID, string sceneName)
        {
            VGameOnPreUnloadScene(sceneData, sceneID, sceneName);
            return true;
        }

        internal bool OnUnloadScene(XmlElement sceneData, Cv_SceneID sceneID, string sceneName, string sceneResource, string resourceBundle)
        {
            VGameOnUnloadScene(sceneData, sceneID, sceneName);

            if (IsProxy)
            {
                var remoteSceneUnloadedEvent = new Cv_Event_RemoteSceneUnloaded(sceneResource, sceneID, sceneName, resourceBundle, this);
                Cv_EventManager.Instance.TriggerEvent(remoteSceneUnloadedEvent);
            }
            else
            {
                var sceneUnloadedEvent = new Cv_Event_SceneUnloaded(sceneResource, sceneID, sceneName, resourceBundle, this);
                Cv_EventManager.Instance.TriggerEvent(sceneUnloadedEvent);
            }

            return true;
        }

        internal Cv_Entity InstantiateNewEntity(bool isSceneRoot, string entityTypeResource, string name, string resourceBundle,
                                                bool visible, Cv_EntityID parentID, XmlElement overrides,
                                                Cv_Transform? transform, Cv_SceneID sceneID, Cv_EntityID serverEntityID)
        {
            if (!CanCreateEntity(name, serverEntityID))
            {
                return null;
            }

            var scene = sceneID == Cv_SceneID.INVALID_SCENE ? m_SceneManager.MainScene : sceneID;
            var sceneName = m_SceneManager.GetSceneName(scene);
            Cv_Debug.Assert(sceneName != null, "Trying to add an entity to an invalid scene [" + scene + ", " + name + "]");

            var path = "/" + name;

            if (isSceneRoot) //Insert fake node in path
            {
                path = "/" + sceneName + path;
            }

            if (parentID == Cv_EntityID.INVALID_ENTITY)
            {
                if (!isSceneRoot)
                {
                    var sceneRoot = m_SceneManager.GetSceneRoot(scene);
                    path = sceneRoot.EntityPath + path;
                    Cv_Debug.Assert(sceneRoot != null, "Trying to add an entity to an invalid scene [" + scene + ", " + name + "]");
                    parentID = sceneRoot.ID;
                }
            }
            else
            {
                var parent = GetEntity(parentID);
                if (parent == null)
                {
                    Cv_Debug.Warning("Attempting to add an entity to a parent that doesn't exist.");
                    return null;
                }

                if (parent.SceneID != scene && !isSceneRoot)
                {
                    scene = parent.SceneID;
                    sceneName = parent.SceneName;

                    Cv_Debug.Warning("Attempting to add an entity of a scene to a parent that is not of the same scene [" + scene + ", " + name + "]. Adding to parent scene instead.");
                }

                path = parent.EntityPath + path;
            }

            Cv_Debug.Assert(!EntitiesByPath.ContainsKey(path), "All entities with the same parent must have a unique ID. Trying to add repeated entity [" + scene + ", " + name + "]");

            Cv_Entity entity = null;
            if (entityTypeResource != null)
            {
                entity = m_EntityFactory.CreateEntity(entityTypeResource, parentID, serverEntityID, resourceBundle, scene, sceneName);
            }
            else
            {
                entity = m_EntityFactory.CreateEmptyEntity(parentID, serverEntityID, resourceBundle, scene, sceneName);
            }

            if (entity != null)
            {
				entity.EntityName = name;
                entity.EntityPath = path;
                entity.Visible = visible;
                entity.SceneRoot = isSceneRoot;
                m_EntitiesToAdd.Enqueue(entity);
                
                lock(Entities)
                {
                    Entities.Add(entity.ID, entity);
                    EntitiesByPath.Add(entity.EntityPath, entity);
                }

                if (overrides != null)
                {
                    m_EntityFactory.ModifyEntity(entity, overrides.SelectNodes("./*[not(self::Entity|self::Scene)]"));
                }

                var tranformComponent = entity.GetComponent<Cv_TransformComponent>();
                if (tranformComponent != null && transform != null)
                {
                    tranformComponent.Transform = transform.Value;
                }

                LastEntityID = entity.ID;

                entity.PostInitialize();

                if (!IsProxy && State == Cv_GameState.Running)
                {
                    var requestNewEntityEvent = new Cv_Event_RequestNewEntity(null, scene, sceneName, entity.EntityName, resourceBundle, visible, parentID, transform, entity.ID);
                    Cv_EventManager.Instance.TriggerEvent(requestNewEntityEvent);
                }
                
                var newEntityEvent = new Cv_Event_NewEntity(entity.ID, this);
                Cv_EventManager.Instance.TriggerEvent(newEntityEvent);

                return entity;
            }

            Cv_Debug.Error("Could not create entity with resource [" + resourceBundle + "].");
            return null;
        }

        internal void DestroyEntity(Cv_Entity entity)
        {
            if (entity == null)
            {
                return;
            }

            foreach (var e in m_EntityList) //TODO(JM): this might get really slow with tons of entities. Optimize if it becomes a problem
            {
                if (e.ID != entity.ID && e.Parent == entity.ID && !e.DestroyRequested)
                {
                    if (e.SceneRoot)
                    {
                        UnloadScene(e.SceneID);
                    }
                    else
                    {
                        DestroyEntity(e);
                    }
                }
            }

            foreach (var e in m_EntitiesToAdd) //TODO(JM): this might get really slow with tons of entities. Optimize if it becomes a problem
            {
                if (e.ID != entity.ID && e.Parent == entity.ID && !e.DestroyRequested)
                {
                    if (e.SceneRoot)
                    {
                        UnloadScene(e.SceneID);
                    }
                    else
                    {
                        DestroyEntity(e);
                    }
                }
            }

            m_EntitiesToDestroy.Enqueue(entity);

            var destroyEntityEvent = new Cv_Event_DestroyEntity(entity.ID, this);
            Cv_EventManager.Instance.TriggerEvent(destroyEntityEvent);
            
            entity.OnDestroy();

            Entities.Remove(entity.ID);
            EntitiesByPath.Remove(entity.EntityPath);

            entity.DestroyRequested = true;
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
            var bundle = data.EntityResourceBundle;
            if (data.EntityResource != null)
            {
                CreateEntity(data.EntityResource, data.EntityName, bundle, data.Visible, data.Parent, null, data.InitialTransform, data.SceneID, data.ServerEntityID);
            }
            else
            {
                CreateEmptyEntity(data.EntityName, bundle, data.Visible, data.Parent, null, data.InitialTransform, data.SceneID, data.ServerEntityID);
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

        private bool CanCreateEntity(string entityName, Cv_EntityID serverEntityID)
        {
            Cv_Debug.Assert(m_EntityFactory != null, "Entity factory should not be null.");
            Cv_Debug.Assert(entityName != null, "Entity must have a name.");
            Cv_Debug.Assert(m_SceneManager.MainScene != Cv_SceneID.INVALID_SCENE, "Must have loaded a scene before creating entity.");

            if (!IsProxy && serverEntityID != Cv_EntityID.INVALID_ENTITY)
            {
                return false;
            }
            else if (IsProxy && serverEntityID == Cv_EntityID.INVALID_ENTITY)
            {
                return false;
            }

            return true;
        }
    }
}
