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

        private List<string> m_Scenes;
        private int m_iCurrentScene;

        internal bool Initialize(string[] scenes)
        {
            m_Scenes = new List<string>(scenes);
            return true;
        }
    }
}