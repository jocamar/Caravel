using System.IO;
using Caravel.Debugging;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core.Resource
{
    public struct Cv_RawTextureResource : Cv_Resource
    {
        public class Cv_TextureData : Cv_ResourceData
        {
            public Texture2D Texture
            {
                get; internal set;
            }

            public long Size
            {
                get
                {
                    return 0;
                }
            }
        }
        
        public string File { get; set; }

        public Cv_ResourceData ResourceData { get; set; }

        public bool VLoad(Stream resourceStream, out int size, Cv_ResourceBundle bundle)
        {
            if ( resourceStream == null )
            {
                Cv_Debug.Error("Invalid resource stream.");
                size = 0;
                return false;
            }

            var texture = Texture2D.FromStream(CaravelApp.Instance.GraphicsDevice, resourceStream);

            var resData = new Cv_TextureData();
            resData.Texture = texture;
            ResourceData = resData;

            size = 0;

            resourceStream.Dispose();
            
            return true;
        }

        public bool VIsManuallyManaged()
        {
            return true;
        }

        public Cv_TextureData GetTexture()
        {
            return (Cv_TextureData) ResourceData;
        }
    }
}