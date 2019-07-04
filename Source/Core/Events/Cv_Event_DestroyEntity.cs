using System.Runtime.Serialization;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_DestroyEntity : Cv_Event
    {
        public override bool WriteToLog
        {
            get
            {
                return false;
            }
        }
        
        public Cv_Event_DestroyEntity(Cv_EntityID entityID, object sender) : base(entityID, sender)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "DestroyEntity";
        }
    }
}