using System.Collections.Generic;
using System.IO;
using System.Xml;
using Caravel.Debugging;
using static Caravel.Core.Cv_Resource;

namespace Caravel.Core
{
    public class Cv_ResourceManager
    {
        public static Cv_ResourceManager Instance
        {
            get; private set;
        }

        public delegate void LoadProgressDelegate(int progress, out bool cancel);

        private string m_resourceFolder;
        private Dictionary<Cv_ResourceType, object> m_ResourceCaches;

        public Cv_ResourceManager()
        {
            m_ResourceCaches = new Dictionary<Cv_ResourceType, object>();
            Instance = this;
        }

        public bool Init()
        {
            m_resourceFolder = CaravelApp.Instance.Content.RootDirectory;
            
            AddResourceCache<Cv_XmlResource>(10*1024*1024);
            return true;
        }

        public Resource GetResource<Resource>(string resourceFile) where Resource : Cv_Resource, new()
        {
            var resType = Cv_Resource.GetResType<Resource>();
            object genericCache;

            if (m_ResourceCaches.TryGetValue(resType, out genericCache))
            {
                var resCache = (Cv_ResourceCache<Resource>) genericCache;
                var resource = resCache.GetResource(resourceFile);

                return resource;
            }

            Cv_Debug.Error("No resource cache exists for the selected resource.");
            return null;
        }

        public string[] GetResourceList<Resource> (string pattern) where Resource : Cv_Resource, new()
        {
            var resType = Cv_Resource.GetResType<Resource>();
            object genericCache;

            if (m_ResourceCaches.TryGetValue(resType, out genericCache))
            {
                var resCache = (Cv_ResourceCache<Resource>) genericCache;
                return resCache.Match(pattern);
            }

            return new string[0];
        }

        public int Preload(string pattern, LoadProgressDelegate progressCallback)
        {
            return 0;
        }

        private void AddResourceCache<Resource> (int size) where Resource : Cv_Resource, new()
        {
            m_ResourceCaches.Add(Cv_Resource.GetResType<Resource>(), new Cv_ResourceCache<Resource>(size));
        }
    }
}
