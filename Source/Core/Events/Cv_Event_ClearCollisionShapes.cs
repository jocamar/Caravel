using System.Runtime.Serialization;
using Caravel.Core.Entity;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Physics.Cv_GamePhysics;

namespace Caravel.Core.Events
{
    public class Cv_Event_ClearCollisionShapes : Cv_Event
    {
        public Cv_Event_ClearCollisionShapes(Cv_EntityID entityId, float timeStamp = 0) : base(entityId, timeStamp)
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