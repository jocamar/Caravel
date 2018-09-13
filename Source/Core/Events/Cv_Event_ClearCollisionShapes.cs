using System.Runtime.Serialization;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_ClearCollisionShapes : Cv_Event
    {
        public override bool WriteToLog
        {
            get
            {
                return false;
            }
        }
        
        public Cv_Event_ClearCollisionShapes(Cv_EntityID entityId, object sender,  float timeStamp = 0) : base(entityId, sender, timeStamp)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "ClearCollisionShapes";
        }
    }
}