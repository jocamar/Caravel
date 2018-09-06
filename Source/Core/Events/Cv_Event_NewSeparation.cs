using System.Runtime.Serialization;
using Caravel.Core.Entity;
using Caravel.Core.Physics;

namespace Caravel.Core.Events
{
    public class Cv_Event_NewSeparation : Cv_Event
    {
        public Cv_CollisionShape ShapeA
        {
            get; private set;
        }

        public Cv_CollisionShape ShapeB
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

        public Cv_Event_NewSeparation(Cv_CollisionShape shapeA, Cv_CollisionShape shapeB, float timeStamp = 0) : base(shapeA.Owner.ID, timeStamp)
        {
            ShapeA = shapeA;
            ShapeB = shapeB;
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