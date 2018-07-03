using System.Collections.Generic;
using Caravel.Core.Entity;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core
{
    public class Cv_FarseerPhysics : Cv_GamePhysics
    {
        private struct Cv_PhysicsEntity
        {
            public Cv_Entity Entity;
            public Body Body;
            public Dictionary<Cv_CollisionShape, Fixture> Shapes;
        }

        private readonly World world;

        public override void VAddBox(Vector2 dimensions, Cv_Entity gameEntity, string densityStr, string physicsMaterial)
        {
            throw new System.NotImplementedException();
        }

        public override void VAddPointShape(Vector2 verts, int numPoints, Cv_Entity gameEntity, string densityStr, string physicsMaterial)
        {
            throw new System.NotImplementedException();
        }

        public override void VAddSphere(float radius, Cv_Entity entity, string densityStr, string physicsMaterial)
        {
            throw new System.NotImplementedException();
        }

        public override void VApplyForce(Vector2 dir, float newtons, Cv_EntityID entityId)
        {
            throw new System.NotImplementedException();
        }

        public override void VApplyTorque(Vector2 dir, float newtons, Cv_EntityID entityId)
        {
            throw new System.NotImplementedException();
        }

        public override void VCreateTrigger(Cv_Entity gameEntity, Vector2 pos, float dim)
        {
            throw new System.NotImplementedException();
        }

        public override Vector2 VGetAngularVelocity(Cv_EntityID entityId)
        {
            throw new System.NotImplementedException();
        }

        public override float VGetOrientationY(Cv_EntityID entityId)
        {
            throw new System.NotImplementedException();
        }

        public override Cv_Transform VGetTransform(Cv_EntityID entityId)
        {
            throw new System.NotImplementedException();
        }

        public override Vector2 VGetVelocity(Cv_EntityID entityId)
        {
            throw new System.NotImplementedException();
        }

        public override bool VInitialize()
        {
            throw new System.NotImplementedException();
        }

        public override bool VKinematicMove(Cv_Transform transf, Cv_EntityID entityId)
        {
            throw new System.NotImplementedException();
        }

        public override void VOnUpdate(float deltaSeconds)
        {
            throw new System.NotImplementedException();
        }

        public override void VRemoveEntity(Cv_EntityID id)
        {
            throw new System.NotImplementedException();
        }

        public override void VRenderDiagnostics()
        {
            throw new System.NotImplementedException();
        }

        public override void VRotateY(Cv_Entity entityId, float angleRadians, float time)
        {
            throw new System.NotImplementedException();
        }

        public override void VSetAngularVelocity(Cv_EntityID entityId, Vector2 vel)
        {
            throw new System.NotImplementedException();
        }

        public override void VSetTransform(Cv_EntityID entityId, Cv_Transform transf)
        {
            throw new System.NotImplementedException();
        }

        public override void VSetVelocity(Cv_EntityID entityId, Vector2 vel)
        {
            throw new System.NotImplementedException();
        }

        public override void VStopentity(Cv_EntityID entityId)
        {
            throw new System.NotImplementedException();
        }

        public override void VSyncVisibleScene()
        {
            throw new System.NotImplementedException();
        }

        public override void VTranslate(Cv_EntityID entityId, Vector2 vec)
        {
            throw new System.NotImplementedException();
        }
    }
}