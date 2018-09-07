using System.Runtime.Serialization;
using Caravel.Core.Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_StopSound : Cv_Event
    {
        public string SoundResource
        {
            get; private set;
        }

        public Cv_Event_StopSound(Cv_Entity.Cv_EntityID entityId, string soundResource, object sender, float timeStamp = 0) : base(entityId, sender, timeStamp)
        {
            SoundResource = soundResource;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "StopSound";
        }
    }
}