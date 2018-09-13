using System.Runtime.Serialization;
using Caravel.Core.Physics;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Events
{
    public class Cv_Event_NewCollision : Cv_Event
    {
        public Cv_CollisionShape ShapeA
        {
            get; private set;
        }

        public Cv_CollisionShape ShapeB
        {
            get; private set;
        }

        public Vector2 NormalForce
        {
            get; private set;
        }

        public float FrictionForce
        {
            get; private set;
        }

        public Vector2[] CollisionPoints
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

        public Cv_Event_NewCollision(Cv_CollisionShape shapeA, Cv_CollisionShape shapeB, Vector2 normalForce,
                                        float frictionForce, Vector2[] collisionPoints, float timeStamp = 0) : base(shapeA.Owner.ID, timeStamp)
        {
            ShapeA = shapeA;
            ShapeB = shapeB;
            NormalForce = normalForce;
            FrictionForce = frictionForce;
            CollisionPoints = collisionPoints;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "NewCollision";
        }
    }
}