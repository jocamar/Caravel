using System;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Physics
{
    public abstract class Cv_Contact : IEquatable<Cv_Contact>
    {
        public Cv_CollisionShape CollidingShape {
            get; internal set;
        }

        public Cv_CollisionShape CollidedShape {
            get; internal set;
        }

        public abstract float Friction
        {
            get; set;
        }

        public abstract float Restitution
        {
            get; set;
        }

        public abstract bool Enabled
        {
            get; set;
        }

        public Vector2 NormalForce
        {
            get; internal set;
        }

        public Vector2 CollidedShapeVelocity
        {
            get; internal set;
        }

        public Vector2 CollidingShapeVelocity
        {
            get; internal set;
        }

        public Vector2[] CollisionPoints
        {
            get; internal set;
        }

        public abstract bool Equals(Cv_Contact other);
        public abstract void ResetFriction();
        public abstract void ResetRestitution();
    }
}