using System.Collections.Generic;
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
    class Cv_SceneManager
    {
        protected CaravelApp Caravel;

        internal struct Cv_SceneInfo {
            public string SceneResource;
            public string ResourceBundle;
            public Cv_Transform? InitTransform;
        };

        internal string[] Scenes
        {
            get {
                lock (m_Scenes)
                {
                    return m_Scenes.Keys.ToArray();
                }
            }
        }

        //NOTE(JM): Switching this while unloading scenes in a separate thread may cause unintended behavior
        internal string MainScene
        {
            get { return m_sCurrentScene; }
            set {
                m_sCurrentScene = value;
            }
        }

        private Dictionary<string, Cv_SceneInfo> m_Scenes;
        private string m_sCurrentScene;
        private Cv_GameLogic m_Logic;

        internal Cv_SceneManager(CaravelApp caravel)
        {
            m_Scenes = new Dictionary<string, Cv_SceneInfo>();
            Caravel = caravel;
        }

        internal bool Initialize()
        {
            return true;
        }

        internal bool LoadScene(string sceneResource, string resourceBundle, string sceneID, Cv_Transform? sceneTransform, Cv_EntityID parent = Cv_EntityID.INVALID_ENTITY)
        {
            Cv_Debug.Assert(!m_Scenes.ContainsKey(sceneID), "Trying to load a scene with an already existing ID.");

            Cv_XmlResource resource;
			resource = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>(sceneResource, resourceBundle, Caravel.EditorRunning);
			
            var root = ((Cv_XmlData) resource.ResourceData).RootNode;

            if (root == null)
            {
                Cv_Debug.Error("Failed to load scene resource file: " + sceneResource);
                return false;
            }

            if (!Caravel.Logic.OnPreLoadScene(root, sceneID))
            {
                return false;
            }

            if (MainScene == null) {
                MainScene = sceneID;
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

            CreateNestedEntities(entitiesNodes, parent, resourceBundle, sceneID, sceneTransform);

            if (!Caravel.Logic.OnLoadScene(root, sceneID))
            {
                if (MainScene == sceneID) {
                    MainScene = null;
                }

                return false;
            }

            if (postLoadScript != null && postLoadScript != "")
            {
				Cv_ScriptResource postLoadRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(postLoadScript, resourceBundle);
				postLoadRes.RunScript();
            }

            Cv_SceneInfo info;
            info.SceneResource = sceneResource;
            info.ResourceBundle = resourceBundle;
            info.InitTransform = sceneTransform;

            lock (m_Scenes)
            {
                m_Scenes.Add(sceneID, info);
            }

            return true;
        }

        internal bool UnloadScene(string sceneID)
        {
            Cv_SceneInfo sceneInfo;
            
            if (!m_Scenes.TryGetValue(sceneID, out sceneInfo))
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

            if (!Caravel.Logic.OnPreUnloadScene(root, sceneID))
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

            var entitiesToRemove = Caravel.Logic.GetSceneEntities(sceneID);
            foreach (var e in entitiesToRemove)
            {
                if (e != null)
                {
                    Caravel.Logic.DestroyEntity(e.ID);
                }
            }

            Caravel.Logic.OnUnloadScene(root, sceneID, sceneInfo.SceneResource, sceneInfo.ResourceBundle);

            lock (m_Scenes)
            {
                m_Scenes.Remove(sceneID);
            }

            return true;
        }

        internal bool UnloadAllScenes()
        {
            string[] keyList;
            lock (m_Scenes)
            {
                keyList = m_Scenes.Keys.ToArray();
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

        internal bool UnloadScenes(string[] sceneIDs)
        {
            var success = true;
            foreach (var scene in sceneIDs)
            {
                if (!UnloadScene(scene))
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
                keyList = m_Scenes.Keys.ToArray();
            }

            Regex mask = new Regex(sceneExpression.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));

            var success = true;
            foreach (var scene in keyList)
            {
                if (mask.IsMatch(scene))
                {
                    if (!UnloadScene(scene))
                    {
                        success = false;
                    }
                }              
            }

            return success;
        }

        private void CreateNestedEntities(XmlNodeList entities, Cv_EntityID parentId, string resourceBundle = null, string sceneID = null, Cv_Transform? sceneTransform = null)
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
                        entity = Caravel.Logic.CreateEntity(entityTypeResource, name, resourceBundle, visible, parentId, (XmlElement) e, sceneTransform, sceneID);
                    }
                    else
                    {
                        entity = Caravel.Logic.CreateEmptyEntity(name, resourceBundle, visible, parentId, (XmlElement) e, sceneTransform, sceneID);
                    }

                    if (entity != null)
                    {
                        var newEntityEvent = new Cv_Event_NewEntity(entity.ID, this);
                        Cv_EventManager.Instance.QueueEvent(newEntityEvent);
                    }

                    var childEntities = e.SelectNodes("./Entity");

                    if (childEntities.Count > 0)
                    {
                        CreateNestedEntities(childEntities, entity.ID, resourceBundle, sceneID);
                    }
                }
            }
        }
    }
}