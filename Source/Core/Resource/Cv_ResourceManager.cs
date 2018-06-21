using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        private Dictionary<string, Cv_ResourceBundle> m_ResourceBundles;
        private Dictionary<string, Cv_ResourceData> m_ResourceData;
        private const string m_sDefaultBundleID = "Cv_DefaultEngineBundle";

        public Resource GetResource<Resource>(string resourceFile, string bundle = m_sDefaultBundleID) where Resource : Cv_Resource, new()
        {
            var resource = new Resource();
            resource.File = resourceFile;

            var isOwnedByResManager = resource.VIsManuallyManaged();
            Cv_ResourceData resData;
            if (isOwnedByResManager && m_ResourceData.TryGetValue(resourceFile, out resData))
            {
                resource.ResourceData = resData;
            }
            else
            {
                var size = 0;
                Cv_ResourceBundle resBundle;

                if (!m_ResourceBundles.TryGetValue(bundle, out resBundle))
                {
                    Cv_Debug.Error("Could not find requested resource bundle: " + bundle);
                    return default(Resource);
                }

                if (!resource.VLoad(resBundle.GetResourceStream(resourceFile), out size, resBundle))
                {
                    Cv_Debug.Error("Unable to load resource: " + resourceFile);
                    return default(Resource);
                }

                if (isOwnedByResManager)
                {
                    m_ResourceData.Add(resourceFile, resource.ResourceData);
                }
            }
            
			return resource;
        }

        public string[] GetResourceList(string pattern, string bundle = m_sDefaultBundleID)
        {
            List<string> resources = new List<string>();
            Regex mask = new Regex(pattern.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));

            Cv_ResourceBundle resBundle;

            if (!m_ResourceBundles.TryGetValue(bundle, out resBundle))
            {
                Cv_Debug.Error("Could not find requested resource bundle.");
                return null;
            }

            foreach (var r in resBundle.Resources)
            {
                if (mask.IsMatch(r))
                {
                    resources.Add(r);
                }
            }

            return resources.ToArray();
        }

        public int Preload<Resource>(string pattern, LoadProgressDelegate progressCallback, string bundle = m_sDefaultBundleID) where Resource : Cv_Resource, new()
        {
            Cv_ResourceBundle resBundle;

            if (!m_ResourceBundles.TryGetValue(bundle, out resBundle))
            {
                Cv_Debug.Error("Could not find requested resource bundle.");
                return 0;
            }

			var resourceList = GetResourceList(pattern, bundle);
            int loaded = 0;
            foreach (var r in resourceList)
            {
                GetResource<Resource>(r,bundle);
                loaded++;

                bool cancel = false;
                progressCallback(loaded * 100 / resourceList.Length, out cancel);

                if (cancel)
                {
                    break;
                }
            }

            return loaded;
		}

        public void Unload(string bundle)
        {
            Cv_ResourceBundle resBundle;

            if (!m_ResourceBundles.TryGetValue(bundle, out resBundle))
            {
                Cv_Debug.Error("Could not find requested resource bundle.");
                return;
            }

			resBundle.Unload();
        }

        public void UnloadManuallyManaged(string pattern)
        {
            Regex mask = new Regex(pattern.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));

            List<string> toRemove = new List<string>();
            foreach (var r in m_ResourceData)
            {
                if (mask.IsMatch(r.Key))
                {
                    toRemove.Add(r.Key);
                }
            }

            foreach (var r in toRemove)
            {
                m_ResourceData.Remove(r);
            }
        }

        public void AddResourceBundle(string bundleID, Cv_ResourceBundle bundle)
        {
            m_ResourceBundles.Add(bundleID, bundle);
        }

        public void RemoveResourceBundle(string bundleID)
        {
            m_ResourceBundles.Remove(bundleID);
        }

		internal Cv_ResourceManager()
        {
            m_ResourceData = new Dictionary<string, Cv_ResourceData>();
            m_ResourceBundles = new Dictionary<string, Cv_ResourceBundle>();
            Instance = this;
        }

        internal bool Init(string engineAssetsFile)
        {
            AddResourceBundle(m_sDefaultBundleID, new Cv_ZipResourceBundle(engineAssetsFile));
			return true;
        }
    }
}
