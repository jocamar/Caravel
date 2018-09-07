using System.Runtime.Serialization;
using Caravel.Core.Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_PauseAllSounds : Cv_Event
    {
        public Cv_Event_PauseAllSounds(Cv_Entity.Cv_EntityID entityId, object sender, float timeStamp = 0) : base(entityId, sender, timeStamp)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "PauseAllSounds";
        }
    }
}