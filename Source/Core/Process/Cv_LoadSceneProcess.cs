using Caravel.Debugging;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Process
{
    public class Cv_LoadSceneProcess : Cv_ParallelProcess
    {
        private string m_sScene;
        private string m_sBundle;
        private string m_sSceneID;
        private Cv_EntityID m_ParentID;
        private Cv_Transform? m_SceneTransform;

        public Cv_LoadSceneProcess(string scene, string bundle, string sceneID, Cv_Transform? sceneTransform = null, Cv_EntityID parent = Cv_EntityID.INVALID_ENTITY)
        {
            m_sBundle = bundle;
            m_sScene = scene;
            m_sSceneID = sceneID;
            m_ParentID = parent;
            m_SceneTransform = sceneTransform;
        }

        protected internal override void VThreadFunction()
        {
            if (CaravelApp.Instance.Logic.LoadScene(m_sScene, m_sBundle, m_sSceneID, m_SceneTransform, m_ParentID))
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