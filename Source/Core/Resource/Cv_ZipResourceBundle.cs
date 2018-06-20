using System.IO;
using Caravel.Debugging;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Resource
{
	public class Cv_ZipResourceBundle : Cv_ResourceBundle
	{
		private ZipFile m_ZipFile;

		public override bool VIsUsingDevDirectories
		{
			get
			{
				return false;
			}
		}

		public override int NumResources
		{
			get
			{
				return 0;
			}
		}

		public override string[] Resources
		{
			get
			{
				return new string[0];
			}
		}

		public Cv_ZipResourceBundle(GameServiceContainer gsc, string fileName) : base(gsc, fileName)
		{
			if (File.Exists(m_sBundleLocation))
			{
				if (Path.GetExtension(m_sBundleLocation) == ".zip")
				{
					var zipFile = ZipFile.Create(m_sBundleLocation);

					if (zipFile != null)
					{
						m_ZipFile = zipFile;
					}
					else
					{
						Cv_Debug.Error("Unable to open assets file.");
					}
				}
				else
				{
					Cv_Debug.Error("Assets file is not the correct type.");
				}
			}
			else
			{
				Cv_Debug.Error("Could not find the provided assets file.");
			}
		}

		public override long VGetResourceSize(string resourceFile)
		{
			return m_ZipFile.GetEntry(resourceFile).Size;
		}

		public override Resource VGetResource<Resource>(string resourceFile)
		{
			throw new System.NotImplementedException();
		}

		public override int VPreload(string pattern, Cv_ResourceManager.LoadProgressDelegate progressCallback)
		{
			throw new System.NotImplementedException();
		}

		protected override Stream OpenStream(string assetName)
		{
			int entry = m_ZipFile.FindEntry(assetName, true);
			if (entry != -1)
			{
				return m_ZipFile.GetInputStream(entry);
			}
			else
			{
				entry = m_ZipFile.FindEntry(assetName + ".xnb", true);

				if (entry != -1)
				{
					return m_ZipFile.GetInputStream(entry);
				}
			}

			Cv_Debug.Error("Unable to open stream.");
			return null;
		}
	}
}