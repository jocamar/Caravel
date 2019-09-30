using System.Xml;
using Caravel.Debugging;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Process
{
    public class Cv_LoadSceneProcess : Cv_ParallelProcess
    {
        private string m_sScene;
        private string m_sBundle;
        private string m_sSceneName;
        private Cv_EntityID m_ParentID;
        private Cv_Transform? m_SceneTransform;
        private XmlElement m_Overrides;

        public Cv_LoadSceneProcess(string scene, string bundle, string sceneName, XmlElement overrides = null,
                                    Cv_Transform? sceneTransform = null, Cv_EntityID parent = Cv_EntityID.INVALID_ENTITY)
        {
            m_sBundle = bundle;
            m_sScene = scene;
            m_sSceneName = sceneName;
            m_ParentID = parent;
            m_SceneTransform = sceneTransform;
            m_Overrides = overrides;
        }

        protected internal override void VThreadFunction()
        {
            var sceneID = CaravelApp.Instance.Logic.LoadScene(m_sScene, m_sBundle, m_sSceneName, m_Overrides, m_SceneTransform, m_ParentID);
            
            if (sceneID != Cv_SceneManager.Cv_SceneID.INVALID_SCENE)
            {
                Succeed();
            }
            else
            {
                Fail();
            }
        }

        protected internal override void VOnFail()
        {
            Cv_Debug.Error("Error - Unable to load scene " + m_sScene + ".");
            CaravelApp.Instance.AbortGame();
        }
    }
}