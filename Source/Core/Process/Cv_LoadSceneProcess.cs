using Caravel.Debugging;
using static Caravel.Core.Cv_GameLogic;

namespace Caravel.Core.Process
{
    public class Cv_LoadSceneProcess : Cv_ParallelProcess
    {
        private string m_sScene;
        private string m_sBundle;

        public Cv_LoadSceneProcess(string scene, string bundle)
        {
            m_sBundle = bundle;
            m_sScene = scene;
        }

        protected internal override void VThreadFunction()
        {
            if (CaravelApp.Instance.Logic.LoadScene(m_sScene, m_sBundle))
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