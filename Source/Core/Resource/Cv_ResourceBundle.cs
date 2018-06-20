using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using static Caravel.Core.Resource.Cv_ResourceManager;

namespace Caravel.Core.Resource
{
    public abstract class Cv_ResourceBundle : ContentManager
    {
		public abstract int NumResources
		{
			get;
		}

		public abstract string[] Resources
		{
			get;
		}

		public abstract bool VIsUsingDevDirectories
		{
			get;
		}

		protected string m_sBundleLocation;

		public Cv_ResourceBundle(IServiceProvider serviceProvider, string bundleLocation) : base(serviceProvider)
		{
			m_sBundleLocation = bundleLocation;
		}

		public abstract Resource VGetResource<Resource>(string resourceFile) where Resource : Cv_Resource, new();
		public abstract long VGetResourceSize(string resourceFile);
		public abstract int VPreload(string pattern, LoadProgressDelegate progressCallback);
		protected abstract override Stream OpenStream(string assetName);
    }
}