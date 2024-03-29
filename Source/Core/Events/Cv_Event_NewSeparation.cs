using System.Runtime.Serialization;
using Caravel.Core.Physics;

namespace Caravel.Core.Events
{
    public class Cv_Event_NewSeparation : Cv_Event
    {
        public Cv_Contact CollisionContact
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

        public Cv_Event_NewSeparation(Cv_Contact contact, float timeStamp = 0) : base(contact.CollidingShape.Owner.ID, timeStamp)
        {
            CollisionContact = contact;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "NewSeparation";
        }
    }
}