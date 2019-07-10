namespace Caravel.Core.Process
{
    public class Cv_UnloadSceneProcess : Cv_ParallelProcess
    {
        private string m_sScenePath;
        private string m_sBundle;

        public Cv_UnloadSceneProcess(string scenePath, string bundle)
        {
            m_sBundle = bundle;
            m_sScenePath = scenePath;
        }

        protected internal override void VThreadFunction()
        {
            CaravelApp.Instance.Logic.UnloadScene(m_sScenePath);
            
            Succeed();
        }
    }
}