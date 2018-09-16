using System.Collections.Generic;
using System.IO;
using System.Text;
using Caravel.Debugging;
using ICSharpCode.SharpZipLib.Zip;

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
				int numFiles = 0;

				foreach (ZipEntry ze in m_ZipFile)
				{
					if (!ze.IsDirectory)
					{
						numFiles++;
					}
				}

				return numFiles;
			}
		}

		public override string[] Resources
		{
			get
			{
				List<string> files = new List<string>();
				foreach (ZipEntry ze in m_ZipFile)
				{
					if (!ze.IsDirectory)
					{
						files.Add(ze.Name);
					}
				}

				return files.ToArray();
			}
		}

		public Cv_ZipResourceBundle(string fileName) : base(CaravelApp.Instance.Services, fileName)
		{
			if (CaravelApp.Instance.UseDevelopmentDirectories)
			{
				return;
			}
			
			if (File.Exists(m_sBundleLocation))
			{
				if (Path.GetExtension(m_sBundleLocation) == ".zip")
				{
					#if !_MONO_
						Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
					#endif
					FileStream fs = File.OpenRead(m_sBundleLocation);
					
					var zipFile = new ZipFile(fs);

					if (zipFile != null)
					{
						m_ZipFile = zipFile;
						m_ZipFile.IsStreamOwner = true;
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

		~Cv_ZipResourceBundle()
		{
			if (m_ZipFile != null)
			{
				m_ZipFile.Close();
			}
		}

		public override long VGetResourceSize(string resourceFile)
		{
			return m_ZipFile.GetEntry(resourceFile).Size;
		}		

        public override void Refresh()
        {
        }

		protected override Stream OpenStream(string assetName)
		{
			return GetStream(assetName);
		}

		protected override Stream GetStream(string assetName)
		{
			int entry = m_ZipFile.FindEntry(assetName, true);
			Stream zipStream  = null;
			if (entry != -1)
			{
				zipStream = m_ZipFile.GetInputStream(entry);
			}
			else
			{
				entry = m_ZipFile.FindEntry(assetName + ".xnb", true);

				if (entry != -1)
				{
					zipStream = m_ZipFile.GetInputStream(entry);
				}
			}

			if (zipStream != null)
			{
				var memoryStream = new MemoryStream();

				zipStream.CopyTo(memoryStream);
				zipStream.Dispose();
				return memoryStream;
			}

			Cv_Debug.Error("Unable to open stream.");
			return null;
		}
    }
}