using System.Runtime.Serialization;
using Caravel.Core.Entity;
using Caravel.Core.Physics;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_EnterTrigger : Cv_Event
    {
        public Cv_CollisionShape Trigger
        {
            get; private set;
        }
    
        public Cv_Event_EnterTrigger(Cv_EntityID entityId, Cv_CollisionShape trigger, float timeStamp = 0) : base(entityId, timeStamp)
        {
            Trigger = trigger;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "EnterTrigger";
        }
    }
}