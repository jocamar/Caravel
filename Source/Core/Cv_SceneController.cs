using System.Collections.Generic;

namespace Caravel.Core
{
    class Cv_SceneController
    {
        string[] Scenes
        {
            get { return m_Scenes.ToArray(); }
        }

        int CurrentScene
        {
            get { return m_iCurrentScene; }
        }

        private List<string> m_Scenes = new List<string>();
        private int m_iCurrentScene;

        public bool Init(List<string> scenes)
        {
            m_Scenes = scenes;
            return true;
        }
    }
}