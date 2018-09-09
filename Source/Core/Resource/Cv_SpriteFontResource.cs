using System;
using System.IO;
using Caravel.Debugging;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core.Resource
{
    public class Cv_SpriteFontResource : Cv_Resource
    {
        public class Cv_SpriteFontData : Cv_ResourceData
        {
            public SpriteFont Font;

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

        public bool VIsManuallyManaged()
        {
            return false;
        }

        public bool VLoad(string resourceFile, Stream resourceStream, out int size, Cv_ResourceBundle bundle)
        {
			try {
                var font = bundle.Load<SpriteFont>(resourceFile);

				var resData = new Cv_SpriteFontData();
				resData.Font = font;
				ResourceData = resData;

				size = 0;
				
				return true;
			}
			catch (Exception e)
			{
				Cv_Debug.Error("Error loading font stream.\n" + e.ToString());
				size = 0;
				return false;
			}
        }

        public Cv_SpriteFontData GetFontData()
        {
            return (Cv_SpriteFontData) ResourceData;
        }
    }
}