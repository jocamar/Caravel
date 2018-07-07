using System.Collections.Generic;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core
{
    public abstract class Cv_GamePhysics
    {
        // Initialiazation and Maintenance of the Physics World
        public abstract bool VInitialize();
        public abstract void VSyncVisibleScene();
        public abstract void VOnUpdate( float timeElapsed ); 

        // Initialization of Physics Objects
        public abstract Cv_CollisionShape VAddCircle(float radius, Vector2 anchor, Cv_Entity gameEntity,
                                                        string densityStr, string physicsMaterial, bool isBullet);
		public abstract Cv_CollisionShape VAddBox(Vector2 dimensions, Vector2 anchor, Cv_Entity gameEntity,
                                                        string densityStr, string physicsMaterial, bool isBullet);
		public abstract Cv_CollisionShape VAddPointShape(List<Vector2> verts, Vector2 anchor, Cv_Entity gameEntity,
                                                        string densityStr, string physicsMaterial, bool isBullet);
        public abstract Cv_CollisionShape VCreateTrigger(Cv_Entity gameEntity, Vector2 pos, float dim, bool isBullet);
        public abstract void VRemoveEntity(Cv_EntityID id);
        public abstract void RemoveCollisionObject(Cv_CollisionShape toRemove);

        // Debugging
        public abstract void VRenderDiagnostics(Cv_Renderer renderer);

        // Physics world modifiers 
        public abstract void VApplyForce(Vector2 dir, float newtons, Cv_EntityID entityId);
        public abstract void VApplyTorque(float newtons, Cv_EntityID entityId);
        
        // Physics entity states
        public abstract void VRotate(Cv_EntityID entityId, float angleRadians, float time);
        public abstract float VGetOrientation(Cv_EntityID entityId);
        public abstract void VStopEntity(Cv_EntityID entityId);
        public abstract Vector2 VGetVelocity(Cv_EntityID entityId);
        public abstract void VSetVelocity(Cv_EntityID entityId, Vector2 vel);
        public abstract float VGetAngularVelocity(Cv_EntityID entityId);
        public abstract void VSetAngularVelocity(Cv_EntityID entityId, float vel);
        public abstract void VTranslate(Cv_EntityID entityId, Vector2 vec);

        public abstract void VSetTransform(Cv_EntityID entityId, Cv_Transform transf);
        public abstract Cv_Transform VGetTransform(Cv_EntityID entityId);

        public static Cv_GamePhysics CreateNullPhysics()
        {
            return new Cv_NullPhysics();
        }
    }
}