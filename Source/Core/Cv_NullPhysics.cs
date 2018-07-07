using System.Collections.Generic;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core
{
    public class Cv_NullPhysics : Cv_GamePhysics
    {
        public override void RemoveCollisionObject(Cv_CollisionShape toRemove)
        {
        }

        public override Cv_CollisionShape VAddBox(Vector2 dimensions, Vector2 anchor, Cv_Entity gameEntity,
                                                    string densityStr, string physicsMaterial, bool isBullet)
        {
            return null;
        }

        public override Cv_CollisionShape VAddPointShape(List<Vector2> verts, Vector2 anchor, Cv_Entity gameEntity,
                                                        string densityStr, string physicsMaterial, bool isBullet)
        {
            return null;
        }

        public override Cv_CollisionShape VAddCircle(float radius, Vector2 anchor, Cv_Entity gameEntity,
                                                        string densityStr, string physicsMaterial, bool isBullet)
        {
            return null;
        }

        public override void VApplyForce(Vector2 dir, float newtons, Cv_EntityID entityId)
        {
        }

        public override void VApplyTorque(float newtons, Cv_EntityID entityId)
        {
        }

        public override Cv_CollisionShape VCreateTrigger(Cv_Entity gameEntity, Vector2 pos, float dim, bool isBullet)
        {
            return null;
        }

        public override float VGetAngularVelocity(Cv_EntityID entityId)
        {
            return 0;
        }

        public override float VGetOrientation(Cv_EntityID entityId)
        {
            return 0;
        }

        public override Cv_Transform VGetTransform(Cv_EntityID entityId)
        {
            return null;
        }

        public override Vector2 VGetVelocity(Cv_EntityID entityId)
        {
            return Vector2.Zero;
        }

        public override bool VInitialize()
        {
            return true;
        }

        public override void VOnUpdate(float deltaSeconds)
        {
        }

        public override void VRemoveEntity(Cv_EntityID id)
        {
        }

        public override void VRenderDiagnostics(Cv_Renderer renderer)
        {
        }

        public override void VRotate(Cv_EntityID entityId, float angleRadians, float time)
        {
        }

        public override void VSetAngularVelocity(Cv_EntityID entityId, float vel)
        {
        }

        public override void VSetTransform(Cv_EntityID entityId, Cv_Transform transf)
        {
        }

        public override void VSetVelocity(Cv_EntityID entityId, Vector2 vel)
        {
        }

        public override void VStopEntity(Cv_EntityID entityId)
        {
        }

        public override void VSyncVisibleScene()
        {
        }

        public override void VTranslate(Cv_EntityID entityId, Vector2 vec)
        {
        }
    }
}