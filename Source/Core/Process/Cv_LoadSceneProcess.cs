using Caravel.Debugging;

namespace Caravel.Core.Process
{
    public class Cv_LoadSceneProcess : Cv_ParallelProcess
    {
        private string m_sScene;
        private string m_sBundle;
        private string m_sSceneID;
        private Cv_Transform? m_SceneTransform;

        public Cv_LoadSceneProcess(string scene, string bundle, string sceneID, Cv_Transform? sceneTransform = null)
        {
            m_sBundle = bundle;
            m_sScene = scene;
            m_sSceneID = sceneID;
            m_SceneTransform = sceneTransform;
        }

        protected internal override void VThreadFunction()
        {
            if (CaravelApp.Instance.Logic.LoadScene(m_sScene, m_sBundle, m_sSceneID, m_SceneTransform))
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