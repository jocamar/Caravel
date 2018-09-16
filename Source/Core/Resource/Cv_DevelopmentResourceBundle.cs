using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caravel.Debugging;

namespace Caravel.Core.Resource
{
    public class Cv_DevelopmentResourceBundle : Cv_ResourceBundle
    {
        public override bool VIsUsingDevDirectories
        {
            get
            {
                return true;
            }
        }

        public override int NumResources
        {
            get
            {
                return m_FileInfo.Count;
            }
        }

        public override string[] Resources
        {
            get
            {
                return m_DirContents;
            }
        }

        private string m_AssetsDir;
        private Dictionary<string, FileInfo> m_FileInfo;
        private string[] m_DirContents;

        public Cv_DevelopmentResourceBundle(string fileName) : base(CaravelApp.Instance.Services, fileName)
        {
            var currDir = CaravelApp.Instance.GetGameWorkingDirectory();

            currDir += Path.DirectorySeparatorChar;
            currDir += Path.GetFileNameWithoutExtension(fileName);
            m_AssetsDir = currDir;
            RootDirectory = m_AssetsDir;

            m_FileInfo = new Dictionary<string, FileInfo>();

            ReadAssetsDirectory(m_AssetsDir);
        }

        public override void Refresh()
        {
            ReadAssetsDirectory(m_AssetsDir);
        }

        protected void ReadAssetsDirectory(string fileDir)
        {
            m_FileInfo.Clear();
            var skipDirectory = fileDir.Length;
            // because we don't want it to be prefixed by a slash
            // if dirPath like "C:\MyFolder", rather than "C:\MyFolder\"
            if(!fileDir.EndsWith("" + Path.DirectorySeparatorChar))
            {
                skipDirectory++;
            }

            var filenames = Directory.EnumerateFiles(fileDir, "*", SearchOption.AllDirectories)
                                        .Select(f => f.Substring(skipDirectory));

            m_DirContents = filenames.ToArray();

            foreach (var f in filenames)
            {
                var fullPath = fileDir;
                if (skipDirectory != fileDir.Length)
                {
                    fullPath += Path.DirectorySeparatorChar;
                }
                fullPath += f;
                var fileInfo = new FileInfo(fullPath);
                m_FileInfo.Add(f, fileInfo);
            }
        }

        protected override Stream GetStream(string assetName)
        {
            FileInfo fi;
            Stream fileStream = null;
            if (m_FileInfo.TryGetValue(assetName, out fi))
            {
                fileStream = fi.OpenRead();
            }
            else if (m_FileInfo.TryGetValue(assetName + ".xnb", out fi))
            {
                fileStream = fi.OpenRead();
            }
            else
            {
                var convertedAsset = assetName.Replace("/", "\\");

                if (m_FileInfo.TryGetValue(convertedAsset, out fi))
                {
                    fileStream = fi.OpenRead();
                }
                else if (m_FileInfo.TryGetValue(convertedAsset + ".xnb", out fi))
                {
                    fileStream = fi.OpenRead();
                }
            }

            if (fileStream != null)
            {
                var memoryStream = new MemoryStream();

                fileStream.CopyTo(memoryStream);
                fileStream.Dispose();
                memoryStream.Position = 0;
                return memoryStream;
            }

            Cv_Debug.Error("Unable to open stream.");
			return null;
        }

        public override long VGetResourceSize(string resourceFile)
        {
            return 0;
        }
    }
}