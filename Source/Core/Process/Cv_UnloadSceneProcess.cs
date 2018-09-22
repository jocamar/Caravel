namespace Caravel.Core.Process
{
    public class Cv_UnloadSceneProcess : Cv_ParallelProcess
    {
        private string m_sScene;
        private string m_sBundle;

        public Cv_UnloadSceneProcess(string scene, string bundle)
        {
            m_sBundle = bundle;
            m_sScene = scene;
        }

        protected internal override void VThreadFunction()
        {
            CaravelApp.Instance.Logic.UnloadScene(m_sScene, m_sBundle);
            
            Succeed();
        }
    }
}