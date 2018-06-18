using System.Collections.Generic;

namespace Caravel.Core
{
    class Cv_SceneController
    {
        internal string[] Scenes
        {
            get { return m_Scenes.ToArray(); }
        }

        internal int CurrentScene
        {
            get { return m_iCurrentScene; }
        }

        private List<string> m_Scenes = new List<string>();
        private int m_iCurrentScene;

        internal bool Init(List<string> scenes)
        {
            m_Scenes = scenes;
            return true;
        }
    }
}