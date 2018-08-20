using System.Runtime.Serialization;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_DestroyRigidBodyComponent : Cv_Event
    {
        public Cv_Event_DestroyRigidBodyComponent(Cv_EntityID entityID) : base(entityID)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "DestroyRigidBodyComponent";
        }
    }
}