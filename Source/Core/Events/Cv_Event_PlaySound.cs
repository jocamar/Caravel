using System.Runtime.Serialization;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Events
{
    public class Cv_Event_PlaySound : Cv_Event
    {
        public string SoundResource
        {
            get; private set;
        }

        public float Volume
        {
            get; private set;
        }

        public float Pan
        {
            get; private set;
        }

        public float Pitch
        {
            get; private set;
        }

        public bool Fade
        {
            get; private set;
        }

        public bool Localized
        {
            get; private set;
        }

        public float Interval
        {
            get; private set;
        }

        public Vector2 Emitter
        {
            get; private set;
        }

        public Vector2 Listener
        {
            get; private set;
        }

        public override bool WriteToLog
		{
			get
			{
				return false;
			}
		}

        public Cv_Event_PlaySound(Cv_Entity.Cv_EntityID entityId, object sender, string soundResource, float volume = 1f, float pan = 0f, float pitch = 0f, bool fade = false,
                                    float interval = 0f, bool localized = false, Vector2 emitter = default(Vector2), Vector2 listener = default(Vector2), float timeStamp = 0) : base(entityId, sender, timeStamp)
        {
            SoundResource = soundResource;
            Volume = volume;
            Pan = pan;
            Pitch = pitch;
            Fade = fade;
            Interval = interval;
            Localized = localized;
            Emitter = emitter;
            Listener = listener;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "PlaySound";
        }
    }
}