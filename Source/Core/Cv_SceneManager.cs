using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Core.Resource;
using Caravel.Debugging;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Resource.Cv_XmlResource;

namespace Caravel.Core
{
    public class Cv_SceneManager
    {
        public enum Cv_SceneID
        {
            INVALID_SCENE = -1
        }

        protected CaravelApp Caravel;

        internal class Cv_SceneInfo {
            public Cv_SceneID ID;
            public string SceneName;
            public string ScenePath;
            public string SceneResource;
            public string ResourceBundle;
            public Cv_EntityID parentID;
            public Cv_EntityID SceneRoot;
            public Cv_Transform? InitTransform;
        };

        internal Cv_SceneInfo[] Scenes
        {
            get {
                lock (m_Scenes)
                {
                    return m_Scenes.Values.ToArray();
                }
            }
        }

        //NOTE(JM): Switching this while unloading scenes in a separate thread may cause unintended behavior
        internal Cv_SceneID MainScene
        {
            get { return m_sCurrentScene; }
            set {
                m_sCurrentScene = value;
            }
        }

        private Dictionary<Cv_SceneID, Cv_SceneInfo> m_Scenes;
        private Dictionary<string, Cv_SceneInfo> m_ScenePaths;
        private Cv_SceneID m_sCurrentScene = Cv_SceneID.INVALID_SCENE;
        private Cv_GameLogic m_Logic;
        private static int m_iLastSceneID = 0;

        internal Cv_SceneManager(CaravelApp caravel)
        {
            m_Scenes = new Dictionary<Cv_SceneID, Cv_SceneInfo>();
            m_ScenePaths = new Dictionary<string, Cv_SceneInfo>();
            Caravel = caravel;
        }

        internal bool Initialize()
        {
            return true;
        }

        internal string GetSceneName(Cv_SceneID sceneID)
        {
            lock(m_Scenes)
            {
                if (m_Scenes.ContainsKey(sceneID)) {
                    return m_Scenes[sceneID].SceneName;
                }
            }

            return null;
        }

        internal string GetScenePath(Cv_SceneID sceneID)
        {
            lock(m_Scenes)
            {
                if (m_Scenes.ContainsKey(sceneID)) {
                    return m_Scenes[sceneID].ScenePath;
                }
            }

            return null;
        }

        internal Cv_Entity GetSceneRoot(Cv_SceneID sceneID)
        {
            lock(m_Scenes)
            {
                if (m_Scenes.ContainsKey(sceneID)) {
                    return CaravelApp.Instance.Logic.GetEntity(m_Scenes[sceneID].SceneRoot);
                }
            }

            return null;
        }

        internal string GetSceneResource(Cv_SceneID sceneID)
        {
            lock(m_Scenes)
            {
                if (m_Scenes.ContainsKey(sceneID)) {
                    return m_Scenes[sceneID].SceneResource;
                }
            }

            return null;
        }

        internal bool IsSceneLoaded(string scenePath)
        {
            lock (m_Scenes)
            {
                return m_ScenePaths.ContainsKey(scenePath);
            }
        }

        internal Cv_Entity[] LoadScene(string sceneResource, string resourceBundle, string sceneName, XmlElement overrides,
                                            Cv_Transform? sceneTransform, Cv_EntityID parentID = Cv_EntityID.INVALID_ENTITY)
        {
            Cv_XmlResource resource;
			resource = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>(sceneResource, resourceBundle, Caravel.EditorRunning);
			
            var root = ((Cv_XmlData) resource.ResourceData).RootNode;

            if (root == null)
            {
                Cv_Debug.Error("Failed to load scene resource file: " + sceneResource);
                return null;
            }

            if (!Caravel.Logic.OnPreLoadScene(root, sceneName))
            {
                return null;
            }

            Cv_SceneID newID = (Cv_SceneID) m_iLastSceneID;
            m_iLastSceneID++;

            if (MainScene == Cv_SceneID.INVALID_SCENE) {
                MainScene = newID;
            }

            var scenePath = "/" + sceneName;
            var parent = CaravelApp.Instance.Logic.GetEntity(parentID);
            if (parent != null)
            {
                scenePath = parent.EntityPath + scenePath;
            }

            Cv_SceneInfo info = new Cv_SceneInfo();
            info.SceneResource = sceneResource;
            info.ResourceBundle = resourceBundle;
            info.InitTransform = sceneTransform;
            info.SceneName = sceneName;
            info.ID = newID;
            info.ScenePath = scenePath;
            info.parentID = parentID;
            info.SceneRoot = Cv_EntityID.INVALID_ENTITY;

            lock (m_Scenes)
            {
                m_Scenes.Add(newID, info);
                m_ScenePaths.Add(scenePath, info);
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

            var sceneRootNodes = root.SelectNodes("StaticEntities/Entity|StaticEntities/Scene");

            if (sceneRootNodes.Count > 1 || sceneRootNodes.Count < 0 || sceneRootNodes.Item(0).Name == "Scene")
            {
                Cv_Debug.Error("Invalid scene root node for scene " + sceneName + "(" + sceneResource + ").\n Either the scene has multiple roots, no root or its root is not an entity.");
                return null;
            }

            XmlElement overridesNode = overrides;
            if (sceneTransform != null)
            {
                if (overridesNode == null)
                {
                    XmlDocument doc = new XmlDocument();
                    overridesNode = doc.CreateElement("Overrides");
                }

                var transformNode = GenerateTransformXml(sceneTransform.Value, overridesNode.OwnerDocument);
                overridesNode.AppendChild(transformNode);
            }
        
            var sceneRootNode = sceneRootNodes.Item(0);
            var sceneRoot = InstantiateSceneEntity(true, sceneRootNode, resourceBundle, parentID, newID, overridesNode);
            
            if (sceneRoot != null)
            {
                info.SceneRoot = sceneRoot.ID;

                var entitiesCreated = new List<Cv_Entity>();
                entitiesCreated.Add(sceneRoot);

                var entitiesNodes = sceneRootNode.SelectNodes("Entity|Scene");
                entitiesCreated.AddRange(CreateNestedEntities(entitiesNodes, sceneRoot.ID, resourceBundle, newID));

                if (!Caravel.Logic.OnLoadScene(root, newID, sceneName))
                {
                    if (MainScene == newID) {
                        MainScene = Cv_SceneID.INVALID_SCENE;
                    }

                    var entitiesToRemove = Caravel.Logic.GetSceneEntities(newID);
                    foreach (var e in entitiesToRemove)
                    {
                        if (e != null)
                        {
                            Caravel.Logic.DestroyEntity(e.ID);
                        }
                    }

                    lock(m_Scenes)
                    {
                        m_Scenes.Remove(newID);
                        m_ScenePaths.Remove(scenePath);
                    }

                    return null;
                }

                if (postLoadScript != null && postLoadScript != "")
                {
                    Cv_ScriptResource postLoadRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(postLoadScript, resourceBundle);
                    postLoadRes.RunScript();
                }

                return entitiesCreated.ToArray();
            }

            lock(m_Scenes)
            {
                m_Scenes.Remove(newID);
                m_ScenePaths.Remove(scenePath);
            }
            
            return null;
        }

        internal bool UnloadScene(string scenePath)
        {
            Cv_SceneInfo sceneInfo;
            
            if (!m_ScenePaths.TryGetValue(scenePath, out sceneInfo))
            {
                return true;
            }

            Cv_XmlResource resource;
			resource = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>(sceneInfo.SceneResource, sceneInfo.ResourceBundle, Caravel.EditorRunning);

            var root = ((Cv_XmlData) resource.ResourceData).RootNode;

            if (root == null)
            {
                Cv_Debug.Error("Failed to unload scene resource file: " + sceneInfo.SceneResource);
                return false;
            }

            if (!Caravel.Logic.OnPreUnloadScene(root, sceneInfo.ID, sceneInfo.SceneName))
            {
                Cv_Debug.Error("Failed to unload scene resource file: " + sceneInfo.SceneResource);
                return false;
            }

            string unloadScript = null;

            var scriptElement = root.SelectNodes("Script").Item(0);

            if (scriptElement != null)
            {
                unloadScript = scriptElement.Attributes["unLoad"].Value;
            }

            if (unloadScript != null && unloadScript != "")
            {
                Cv_ScriptResource unLoadRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(unloadScript, sceneInfo.ResourceBundle);
                unLoadRes.RunScript();
            }

            var entitiesToRemove = Caravel.Logic.GetSceneEntities(sceneInfo.ID);
            foreach (var e in entitiesToRemove)
            {
                if (e != null)
                {
                    Caravel.Logic.DestroyEntity(e.ID);
                }
            }

            Caravel.Logic.OnUnloadScene(root, sceneInfo.ID, sceneInfo.SceneName, sceneInfo.SceneResource, sceneInfo.ResourceBundle);

            lock (m_Scenes)
            {
                m_Scenes.Remove(sceneInfo.ID);
                m_ScenePaths.Remove(scenePath);
            }

            if (MainScene == sceneInfo.ID && m_Scenes.Count > 0) {
                MainScene = m_Scenes.Keys.First();
            }

            return true;
        }

        internal bool UnloadAllScenes()
        {
            string[] keyList;
            lock (m_Scenes)
            {
                keyList = m_ScenePaths.Keys.ToArray();
            }

            var success = true;
            foreach (var scene in keyList)
            {
                if (!UnloadScene(scene))
                {
                    success = false;
                }                
            }

            return success;
        }

        internal bool UnloadScenes(string[] scenePaths)
        {
            var success = true;
            foreach (var scenePath in scenePaths)
            {
                if (!UnloadScene(scenePath))
                {
                    success = false;
                }
            }

            return success;
        }

        internal bool UnloadScenes(string sceneExpression)
        {
            string[] keyList;
            
            lock (m_Scenes)
            {
                keyList = m_ScenePaths.Keys.ToArray();
            }

            Regex mask = new Regex(sceneExpression.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));

            var success = true;
            foreach (var scenePath in keyList)
            {
                if (mask.IsMatch(scenePath))
                {
                    if (!UnloadScene(scenePath))
                    {
                        success = false;
                    }
                }              
            }

            return success;
        }

        private List<Cv_Entity> CreateNestedEntities(XmlNodeList entities, Cv_EntityID parentId,
                                            string resourceBundle = null, Cv_SceneID sceneID = Cv_SceneID.INVALID_SCENE)
        {
            List<Cv_Entity> entitiesCreated = new List<Cv_Entity>();
            if (entities != null)
            {
                foreach(XmlNode e in entities)
                {
                    var visible = true;

                    if (e.Attributes["visible"] != null)
                    {
                        visible = bool.Parse(e.Attributes["visible"].Value);
                    }

                    if (e.Name == "Entity")
                    {
                        var entity = InstantiateSceneEntity(false, e, resourceBundle, parentId, sceneID);

                        entitiesCreated.Add(entity);

                        var childEntities = e.SelectNodes("./Entity|./Scene");

                        if (childEntities.Count > 0)
                        {
                            entitiesCreated.AddRange(CreateNestedEntities(childEntities, entity.ID, resourceBundle, sceneID));
                        }
                    }
                    else
                    {
                        var name = e.Attributes?["name"].Value;
                        var sceneResource = e.Attributes["resource"].Value;

                        if (sceneResource == null || sceneResource == "")
                        {
                            continue;
                        }

                        var sceneOverrides = GetSceneOverrides((XmlElement) e);
                        
                        var createdEntities = LoadScene(sceneResource, resourceBundle, name, sceneOverrides, null, parentId);

                        if (createdEntities == null || createdEntities.Length <= 0)
                        {
                            Cv_Debug.Warning("Unable to load a sub scene that as part of another scene [" + name + "]");
                            continue;
                        }

                        entitiesCreated.AddRange(createdEntities);
                    }
                }
            }

            return entitiesCreated;
        }

        private XmlElement GetSceneOverrides(XmlElement sceneElement)
        {
            Cv_Debug.Assert(sceneElement != null, "Must have valid scene element.");
            return (XmlElement) sceneElement.SelectSingleNode("Overrides");
        }

        private Cv_Entity InstantiateSceneEntity(bool sceneRoot, XmlNode e, string resourceBundle, Cv_EntityID parentId,
                                                    Cv_SceneID sceneID = Cv_SceneID.INVALID_SCENE, XmlElement sceneOverrides = null)
        {
            Cv_Entity entity = null;
            var entityTypeResource = e.Attributes["type"].Value;
            var name = e.Attributes?["name"].Value;
            var visible = true;

            if (e.Attributes["visible"] != null)
            {
                visible = bool.Parse(e.Attributes["visible"].Value);
            }

            var overrides = e.OwnerDocument.CreateElement("Overrides");
            foreach(XmlNode o in e.ChildNodes)
            {
                var newNode = o.CloneNode(true);
                overrides.AppendChild(newNode);
            }

            if (sceneOverrides != null)
            {
                foreach(XmlNode o in sceneOverrides.ChildNodes)
                {
                    var newNode = o.CloneNode(true);
                    overrides.AppendChild(overrides.OwnerDocument.ImportNode(newNode, true));
                }
            }

            if (entityTypeResource != "")
            {
                entity = Caravel.Logic.InstantiateNewEntity(sceneRoot, entityTypeResource, name, resourceBundle,
                                                            visible, parentId, overrides,
                                                            null, sceneID, Cv_EntityID.INVALID_ENTITY);
            }
            else
            {
                entity = Caravel.Logic.InstantiateNewEntity(sceneRoot, null, name, resourceBundle,
                                                                visible, parentId, overrides,
                                                                null, sceneID, Cv_EntityID.INVALID_ENTITY);
            }

            return entity;
        }

        private XmlElement GenerateTransformXml(Cv_Transform t, XmlDocument doc)
        {
            var transform = doc.CreateElement(Cv_EntityComponent.GetComponentName<Cv_TransformComponent>());

            var positionNode = doc.CreateElement("Position");
            positionNode.SetAttribute("x", t.Position.X.ToString(CultureInfo.InvariantCulture));
            positionNode.SetAttribute("y", t.Position.Y.ToString(CultureInfo.InvariantCulture));
            positionNode.SetAttribute("z", t.Position.Z.ToString(CultureInfo.InvariantCulture));

            var rotationNode = doc.CreateElement("Rotation");
            rotationNode.SetAttribute("radians", t.Rotation.ToString(CultureInfo.InvariantCulture));

            var scaleNode = doc.CreateElement("Scale");
            scaleNode.SetAttribute("x", t.Scale.X.ToString(CultureInfo.InvariantCulture));
            scaleNode.SetAttribute("y", t.Scale.Y.ToString(CultureInfo.InvariantCulture));

            var originNode = doc.CreateElement("Origin");
            originNode.SetAttribute("x", t.Origin.X.ToString(CultureInfo.InvariantCulture));
            originNode.SetAttribute("y", t.Origin.Y.ToString(CultureInfo.InvariantCulture));

            transform.AppendChild(positionNode);
            transform.AppendChild(rotationNode);
            transform.AppendChild(scaleNode);
            transform.AppendChild(originNode);

            return transform;
        }
    }
}