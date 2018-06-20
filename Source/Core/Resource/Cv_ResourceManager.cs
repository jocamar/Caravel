using System.Collections.Generic;
using Caravel.Debugging;
using static Caravel.Core.Resource.Cv_Resource;

namespace Caravel.Core.Resource
{
	public class Cv_ResourceManager
    {
        public static Cv_ResourceManager Instance
        {
            get; private set;
        }

        public delegate void LoadProgressDelegate(int progress, out bool cancel);

        private Cv_ResourceBundle m_ResourceBundle;

        public Resource GetResource<Resource>(string resourceFile) where Resource : Cv_Resource, new()
        {
			var resource = m_ResourceBundle.VGetResource<Resource>(resourceFile);
			return resource;
        }

        public string[] GetResourceList<Resource> (string pattern) where Resource : Cv_Resource, new()
        {
            return m_ResourceBundle.Resources;
        }

        public int Preload(string pattern, LoadProgressDelegate progressCallback)
        {
			return m_ResourceBundle.VPreload(pattern, progressCallback);
		}

		internal Cv_ResourceManager(string assetsLocation)
        {
			m_ResourceBundle = new Cv_ZipResourceBundle(CaravelApp.Instance.Services, assetsLocation);
			CaravelApp.Instance.Content = m_ResourceBundle;
            Instance = this;
        }

        internal bool Init()
        {
			return true;
        }
    }
}
