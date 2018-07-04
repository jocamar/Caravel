using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Caravel.Core.Entity;
using Caravel.Core.Resource;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
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
            public Dictionary<Fixture, Cv_CollisionShape> Shapes;
        }

        private struct Cv_PhysicsMaterial
        {
            public float Friction;
            public float Restitution;

            public Cv_PhysicsMaterial(float friction, float restitution)
            {
                Friction = friction;
                Restitution = restitution;
            }
        }

        public Vector2 Gravity
        {
            get
            {
                return m_World.Gravity;
            }

            set
            {
                m_World.Gravity = value;
            }
        }

        public int Screen2WorldRatio
        {
            get
            {
                return m_iScreenToWorldRatio;
            }

            set
            {
                m_iScreenToWorldRatio = value;
            }
        }

        public bool DebugDraw = true;

        private Dictionary<Cv_Entity, Cv_PhysicsEntity> m_PhysicsEntities;
        private Dictionary<Fixture, Cv_PhysicsEntity> m_Fixtures;
        private Dictionary<string, float> m_DensityTable;
        private Dictionary<string, Cv_PhysicsMaterial> m_MaterialsTable;
        private HashSet<Tuple<Cv_CollisionShape, Cv_CollisionShape>> m_PreviousTickCollisionPairs;
        private int m_iScreenToWorldRatio = 30;

        private readonly World m_World;

        public Cv_FarseerPhysics()
        {
            //Register events
            /*REGISTER_EVENT(EvtData_PhysTrigger_Enter);
            REGISTER_EVENT(EvtData_PhysTrigger_Leave);
            REGISTER_EVENT(EvtData_PhysCollision);
            REGISTER_EVENT(EvtData_PhysSeparation);*/

            m_PhysicsEntities = new Dictionary<Cv_Entity, Cv_PhysicsEntity>();
            m_Fixtures = new Dictionary<Fixture, Cv_PhysicsEntity>();
            m_DensityTable = new Dictionary<string, float>();
            m_MaterialsTable = new Dictionary<string, Cv_PhysicsMaterial>();
            m_PreviousTickCollisionPairs = new HashSet<Tuple<Cv_CollisionShape, Cv_CollisionShape>>();
        }

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

        public override void VOnUpdate(float timeElapsed)
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

        private void AddShape(Cv_Entity entity, Cv_CollisionShape shape, float mass, string physicsMaterial)
        {

        }

        private void LoadXML()
        {
            var physicsConfig = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>("config/physics.xml");
            var root = ((Cv_XmlResource.Cv_XmlData) physicsConfig.ResourceData).RootNode;

            var materials = root.SelectNodes("//PhysicsMaterials").Item(0);

            foreach(XmlElement material in materials.ChildNodes)
            {
                float restitution = 0;
                float friction = 0;
                if (material.Attributes["restitution"] != null)
                {
                    restitution = float.Parse(material.Attributes["restitution"].Value,  CultureInfo.InvariantCulture);
                }

                if (material.Attributes["friction"] != null)
                {
                    friction = float.Parse(material.Attributes["friction"].Value,  CultureInfo.InvariantCulture);
                }
                

                m_MaterialsTable.Add(material.Value, new Cv_PhysicsMaterial(friction, restitution));
            }

            var densities = root.SelectNodes("//Densities").Item(0);

            foreach(XmlElement density in densities.ChildNodes)
            {
                m_DensityTable.Add(density.Value, float.Parse(density.FirstChild.Value,  CultureInfo.InvariantCulture));
            }
        }

        private void RemoveCollisionObject(Fixture toRemove)
        {

        }

        private Cv_PhysicsMaterial LookupMaterial(string material)
        {
            return m_MaterialsTable[material];
        }

        private bool OnNewCollisionPair(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            return true;
        }

        private void OnAfterCollision(Fixture fixtureA, Fixture fixtureB, Contact contact, ContactVelocityConstraint impulse)
        {
            var collisionShapeA = m_Fixtures[fixtureA].Shapes[fixtureA];
            var collisionShapeB = m_Fixtures[fixtureB].Shapes[fixtureB];

        }

        private void OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            var collisionShapeA = m_Fixtures[fixtureA].Shapes[fixtureA];
            var collisionShapeB = m_Fixtures[fixtureB].Shapes[fixtureB];
        }

        private Vector2 ToPhysicsVector(Vector3 vector3D)
        {
            Vector2 vector2D;
            vector2D = new Vector2(ToWorldCoord(vector3D.X), ToWorldCoord(vector3D.Y));

            return vector2D;
        }

        private Vector3 ToOutsideVector(Vector2 vector2D, Vector3 outsideVec)
        {
            Vector3 vector3D;
                vector3D = new Vector3(ToScreenCoord(vector2D.X),
                                        ToScreenCoord(vector2D.Y),
                                        outsideVec.Z);
            

            return vector3D;
        }

        private float ToWorldCoord(float coord)
        {
            return coord / m_iScreenToWorldRatio;
        }

        private Vector2 ToWorldCoord(Vector2 coord)
        {
            return coord / m_iScreenToWorldRatio;
        }

        private float ToScreenCoord(float coord)
        {
            return coord * m_iScreenToWorldRatio;
        }

        private Vector2 ToScreenCoord(Vector2 coord)
        {
            return coord * m_iScreenToWorldRatio;
        }
    }
}