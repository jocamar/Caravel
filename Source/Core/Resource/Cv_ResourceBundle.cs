using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

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

		public Stream GetResourceStream(string resourceFile)
		{
			return OpenStream(resourceFile);
		}

		public abstract long VGetResourceSize(string resourceFile);
		public abstract void Refresh();
		protected abstract override Stream OpenStream(string assetName);
    }
}