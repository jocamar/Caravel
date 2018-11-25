using System.Runtime.Serialization;
using Caravel.Core.Physics;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_LeaveTrigger : Cv_Event
    {
        public Cv_Contact CollisionContact
        {
            get; private set;
        }

        public Cv_Event_LeaveTrigger(Cv_Contact contact, object sender, float timeStamp = 0) : base(contact.CollidedShape.Owner.ID, sender, timeStamp)
        {
            CollisionContact = contact;
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