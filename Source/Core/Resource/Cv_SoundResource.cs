using System;
using System.IO;
using Caravel.Debugging;
using Microsoft.Xna.Framework.Audio;

namespace Caravel.Core.Resource
{
    public class Cv_SoundResource : Cv_Resource
    {
        public class Cv_SoundData : Cv_ResourceData
        {
            public SoundEffect Sound;

            public long Size
            {
                get
                {
                    return 0;
                }
            }

            ~Cv_SoundData()
            {
                if (Sound != null)
                {
                    Sound.Dispose();
                }
            }
        }

        public string File { get; set; }

        public Cv_ResourceData ResourceData { get; set; }

        public bool VIsManuallyManaged()
        {
            return true;
        }

        public bool VLoad(Stream resourceStream, out int size, Cv_ResourceBundle bundle)
        {
            if ( resourceStream == null )
            {
                Cv_Debug.Error("Invalid resource stream.");
                size = 0;
                return false;
            }

            resourceStream.Position = 0;
			try {
				var sound = SoundEffect.FromStream(resourceStream);

				var resData = new Cv_SoundData();
				resData.Sound = sound;
				ResourceData = resData;

				size = 0;

				resourceStream.Dispose();
				
				return true;
			}
			catch (Exception e)
			{
				Cv_Debug.Error("Error loading sound stream.");
				size = 0;
				return false;
			}
        }

        public Cv_SoundData GetSoundData()
        {
            return (Cv_SoundData) ResourceData;
        }
    }
}