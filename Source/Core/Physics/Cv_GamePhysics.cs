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
            public string ShapeID;
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

            public Cv_ShapeData(Cv_ShapeData toCopy)
            {
                ShapeID = toCopy.ShapeID;
                Type = toCopy.Type;
                Anchor = toCopy.Anchor;
                Radius = toCopy.Radius;
                Dimensions = toCopy.Dimensions;
                Points = toCopy.Points;
                IsBullet = toCopy.IsBullet;
                Material = toCopy.Material;
                Categories = toCopy.Categories;
                CollidesWith = toCopy.CollidesWith;
                CollisionDirections = toCopy.CollisionDirections;
            }
        }

		public struct Cv_PhysicsMaterial
        {
            public float Friction;
            public float Restitution;
            public float Density;

            public Cv_PhysicsMaterial(float friction, float restitution, float density)
            {
                Friction = friction;
                Restitution = restitution;
                Density = density;
            }
        }

        
        public struct Cv_RayCastIntersection
        {
            public Cv_Entity Entity;
            public Vector2 Point;
            public Vector2 Normal;
        }

        public enum Cv_CollisionDirection
        {
            Right,
            Left,
            Top,
            Bottom
        }

		public enum Cv_RayCastType
		{
			Closest,
			ClosestSolid,
			All,
			AllSolid,
		}

        // Initialiazation and Maintenance of the Physics World
        public abstract bool VInitialize();
        public abstract void VSyncVisibleScene();
        public abstract void VOnUpdate(float elapsedTime); 

        // Initialization of Physics Objects
        public abstract Cv_CollisionShape VAddCircle(Cv_Entity gameEntity, Cv_ShapeData data);
		public abstract Cv_CollisionShape VAddBox(Cv_Entity gameEntity, Cv_ShapeData data);
		public abstract Cv_CollisionShape VAddPointShape(Cv_Entity gameEntity, Cv_ShapeData data);
        public abstract Cv_CollisionShape VAddTrigger(Cv_Entity gameEntity, Cv_ShapeData data);
        public abstract void VRemoveEntity(Cv_EntityID id);
        public abstract void RemoveCollisionObject(Cv_CollisionShape toRemove);

        //Editor
        public abstract string[] GetMaterials();

        // Debugging
        public abstract void VRenderDiagnostics(Cv_CameraNode camera, Cv_Renderer renderer);
        
        // Physics entity states
        public abstract void VStopEntity(Cv_EntityID entityId);
        public abstract Vector2 VGetVelocity(Cv_EntityID entityId);
        public abstract void VSetVelocity(Cv_EntityID entityId, Vector2 vel);
        public abstract float VGetAngularVelocity(Cv_EntityID entityId);
        public abstract void VSetAngularVelocity(Cv_EntityID entityId, float vel);
        public abstract void VApplyForce(Vector2 dir, float newtons, Cv_EntityID entityId);
        public abstract void VApplyTorque(float newtons, Cv_EntityID entityId);
		public abstract Cv_RayCastIntersection[] RayCast(Vector2 startingPoint, Vector2 endingPoint, Cv_RayCastType type);
		public abstract Cv_PhysicsMaterial GetMaterial(string material);
        public abstract void SetCollidesWith(Cv_EntityID entityId1, Cv_EntityID entityId2, bool state, string shapeId1 = null, string shapeId2 = null);
        public abstract void SetEntityPaused(Cv_EntityID entityId, bool state);

        public static Cv_GamePhysics CreateNullPhysics(CaravelApp app)
        {
            return new Cv_NullPhysics(app);
        }
    }
}