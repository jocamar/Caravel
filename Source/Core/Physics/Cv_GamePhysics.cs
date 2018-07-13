using System.Collections.Generic;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Physics.Cv_CollisionShape;

namespace Caravel.Core.Physics
{
    public abstract class Cv_GamePhysics
    {
		public enum ShapeType { Box, Circle, Polygon, Trigger };

        public struct Cv_ShapeData
        {
            public ShapeType Type;
            public Vector2 Anchor;
            public float Radius;
            public Vector2 Dimensions;
            public Vector2[] Points;
            public bool IsBullet;
            public string Material;
			public Cv_CollisionCategories Categories;
			public Cv_CollisionCategories CollidesWith;
			public Dictionary<int, string> CollisionDirections;
        }

        // Initialiazation and Maintenance of the Physics World
        public abstract bool VInitialize();
        public abstract void VSyncVisibleScene();
        public abstract void VOnUpdate( float timeElapsed ); 

        // Initialization of Physics Objects
        public abstract Cv_CollisionShape VAddCircle(Cv_Entity gameEntity, Cv_ShapeData data);
		public abstract Cv_CollisionShape VAddBox(Cv_Entity gameEntity, Cv_ShapeData data);
		public abstract Cv_CollisionShape VAddPointShape(Cv_Entity gameEntity, Cv_ShapeData data);
        public abstract Cv_CollisionShape VAddTrigger(Cv_Entity gameEntity, Cv_ShapeData data);
        public abstract void VRemoveEntity(Cv_EntityID id);
        public abstract void RemoveCollisionObject(Cv_CollisionShape toRemove);

        // Debugging
        public abstract void VRenderDiagnostics(Cv_Renderer renderer);
        
        // Physics entity states
        public abstract void VStopEntity(Cv_EntityID entityId);
        public abstract Vector2 VGetVelocity(Cv_EntityID entityId);
        public abstract void VSetVelocity(Cv_EntityID entityId, Vector2 vel);
        public abstract float VGetAngularVelocity(Cv_EntityID entityId);
        public abstract void VSetAngularVelocity(Cv_EntityID entityId, float vel);
        public abstract void VApplyForce(Vector2 dir, float newtons, Cv_EntityID entityId);
        public abstract void VApplyTorque(float newtons, Cv_EntityID entityId);

        public static Cv_GamePhysics CreateNullPhysics()
        {
            return new Cv_NullPhysics();
        }
    }
}