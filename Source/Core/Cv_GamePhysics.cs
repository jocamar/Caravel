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
        public abstract void VAddSphere(float radius, Cv_Entity gameEntity, /*const Mat4x4& initialTransform, */string densityStr, string physicsMaterial);
		public abstract void VAddBox(Vector2 dimensions, Cv_Entity gameEntity, /*const Mat4x4& initialTransform, */ string densityStr, string physicsMaterial);
		public abstract void VAddPointShape(Vector2 verts, int numPoints, Cv_Entity gameEntity, /*const Mat4x4& initialTransform, */ string densityStr, string physicsMaterial);
        public abstract void VRemoveEntity(Cv_EntityID id);

        // Debugging
        public abstract void VRenderDiagnostics();

        // Physics world modifiers
        public abstract void VCreateTrigger(Cv_Entity gameEntity, Vector2 pos, float dim);
        public abstract void VApplyForce(Vector2 dir, float newtons, Cv_EntityID entityId);
        public abstract void VApplyTorque(Vector2 dir, float newtons, Cv_EntityID entityId);
        public abstract bool VKinematicMove(Cv_Transform transf, Cv_EntityID entityId);
        
        // Physics entity states
        public abstract void VRotateY(Cv_Entity entityId, float angleRadians, float time);
        public abstract float VGetOrientationY(Cv_EntityID entityId);
        public abstract void VStopentity(Cv_EntityID entityId);
        public abstract Vector2 VGetVelocity(Cv_EntityID entityId);
        public abstract void VSetVelocity(Cv_EntityID entityId, Vector2 vel);
        public abstract Vector2 VGetAngularVelocity(Cv_EntityID entityId);
        public abstract void VSetAngularVelocity(Cv_EntityID entityId, Vector2 vel);
        public abstract void VTranslate(Cv_EntityID entityId, Vector2 vec);

        public abstract void VSetTransform(Cv_EntityID entityId, Cv_Transform transf);
        public abstract Cv_Transform VGetTransform(Cv_EntityID entityId);

        public static Cv_GamePhysics CreateNullPhysics()
        {
            return new Cv_NullPhysics();
        }
    }
}