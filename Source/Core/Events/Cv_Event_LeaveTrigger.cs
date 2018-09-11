using System.Runtime.Serialization;
using Caravel.Core.Entity;
using Caravel.Core.Physics;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_LeaveTrigger : Cv_Event
    {
        public Cv_CollisionShape Trigger
        {
            get; private set;
        }

        public Cv_Event_LeaveTrigger(Cv_EntityID entityId, Cv_CollisionShape trigger, object sender, float timeStamp = 0) : base(entityId, sender, timeStamp)
        {
            Trigger = trigger;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "LeaveTrigger";
        }
    }
}