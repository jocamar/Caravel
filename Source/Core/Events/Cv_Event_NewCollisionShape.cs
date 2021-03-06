using System.Runtime.Serialization;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Physics.Cv_GamePhysics;

namespace Caravel.Core.Events
{
    public class Cv_Event_NewCollisionShape : Cv_Event
    {
        public Cv_ShapeData ShapeData
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

        public Cv_Event_NewCollisionShape(Cv_EntityID entityId, Cv_ShapeData shape, object sender, float timeStamp = 0) : base(entityId, sender, timeStamp)
        {
            ShapeData = shape;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "NewCollisionShape";
        }
    }
}