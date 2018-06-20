using System.Collections.Generic;
using System.Text.RegularExpressions;
using Caravel.Debugging;
using static Caravel.Core.Cv_ResourceManager;

namespace Caravel.Core
{
    internal class Cv_ResourceCache<Resource> where Resource : Cv_Resource, new()
    {
        private List<Cv_Resource> m_ResourceList;
        private Dictionary<string, Cv_Resource> m_ResourceMap;

        private int m_iCacheSize;
        private int m_iAllocated;

        public Cv_ResourceCache(int cacheSize)
        {
            m_iAllocated = 0;
            m_iCacheSize = cacheSize;
            m_ResourceList = new List<Cv_Resource>();
            m_ResourceMap = new Dictionary<string, Cv_Resource>();
        }

        public Resource GetResource(string resourceFile)
        {
            var res = Find(resourceFile);

            if (res != null)
            {
                //Update the resource on the frequently used list
                m_ResourceList.Remove(res);
                m_ResourceList.Add(res);
            }
            else {
                res = Load(resourceFile);
                if (res == null)
                {
                    Cv_Debug.Error("Unable to load resource: " + resourceFile);
                }
            }

            if (res.Type == Cv_Resource.GetResType<Resource>())
            {
                return (Resource) res;
            }

            Cv_Debug.Error("Requested resource type does not match to existing resource.");
            return null;
        }

        public int Preload(string pattern, LoadProgressDelegate progressCallback)
        {
            return 0;
        }

        public string[] Match(string pattern)
        {
            var matchedResources = new List<string>();
            foreach(var r in m_ResourceList)
            {
                if (Regex.IsMatch(r.File, pattern))
                {
                    matchedResources.Add(r.File);
                }
            }

            return matchedResources.ToArray();
        }

        private Cv_Resource Load(string resource)
        {
            var newRes = new Resource();
            newRes.File = resource;

            int size;
            
            if (!newRes.VLoad(out size))
            {
                Cv_Debug.Error("Unable to load resource: " + resource);
                return null;
            }

            if (!MakeRoom(size))
            {
                Cv_Debug.Error("Unable to make size for resource: " + resource);
                return null;
            }

            newRes.Size = size;
            m_ResourceMap.Add(resource, newRes);
            m_ResourceList.Add(newRes);
            m_iAllocated += size;

            return newRes;
        }

        private Cv_Resource Find(string resource)
        {
            Cv_Resource res;

            if (m_ResourceMap.TryGetValue(resource, out res))
            {
                return res;
            }

            return null;
        }

        private bool MakeRoom(int size)
        {
            if (size > m_iCacheSize)
            {
                return false;
            }

            while (size > m_iCacheSize - m_iAllocated)
            {
                if (m_ResourceList.Count == 0)
                {
                    return false;
                }

                m_iAllocated -= m_ResourceList[0].Size;
                m_ResourceList.RemoveAt(0);
            }

            return true;
        }
    }
}