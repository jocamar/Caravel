using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Caravel.Core.Entity;
using Caravel.Core.Resource;
using Caravel.Debugging;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
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

        private Dictionary<Cv_EntityID, Cv_PhysicsEntity> m_PhysicsEntities;
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

            m_PhysicsEntities = new Dictionary<Cv_EntityID, Cv_PhysicsEntity>();
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

        public override void VCreateTrigger(Cv_Entity gameEntity, Vector2 pos, float dim)
        {
            throw new System.NotImplementedException();
        }

        public override void VApplyForce(Vector2 dir, float newtons, Cv_EntityID entityId)
        {
            var entity = CaravelApp.Instance.GameLogic.GetEntity(entityId);

            if (entity != null)
            {
                var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                if (rigidBodyComponent != null)
                {
                    Vector3 force = new Vector3(dir, 0);
                    force.Normalize();
                    rigidBodyComponent.Impulse = force * newtons;
                }
            }
        }

        public override void VApplyTorque(float newtons, Cv_EntityID entityId)
        {
            var entity = CaravelApp.Instance.GameLogic.GetEntity(entityId);

            if (entity != null)
            {
                var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                if (rigidBodyComponent != null)
                {
                    rigidBodyComponent.AngularImpulse = newtons;
                }
            }
        }

        public override float VGetAngularVelocity(Cv_EntityID entityId)
        {
            var entity = CaravelApp.Instance.GameLogic.GetEntity(entityId);

            if (entity != null)
            {
                var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                if (rigidBodyComponent != null)
                {
                    return rigidBodyComponent.AngularVelocity;
                }
            }

            Cv_Debug.Error("Entity does not exist or does not have rigid body.");
            return 0;
        }

        public override float VGetOrientation(Cv_EntityID entityId)
        {
            var entity = CaravelApp.Instance.GameLogic.GetEntity(entityId);

            if (entity != null)
            {
                var transformComponent = entity.GetComponent<Cv_TransformComponent>();

                if (transformComponent != null)
                {
                    return transformComponent.Rotation;
                }
            }

            Cv_Debug.Error("Entity does not exist or does not have transform.");
            return 0;
        }

        public override Cv_Transform VGetTransform(Cv_EntityID entityId)
        {
            var entity = CaravelApp.Instance.GameLogic.GetEntity(entityId);

            if (entity != null)
            {
                var transformComponent = entity.GetComponent<Cv_TransformComponent>();

                if (transformComponent != null)
                {
                    return transformComponent.Transform;
                }
            }

            Cv_Debug.Error("Entity does not exist or does not have transform.");
            return null;
        }

        public override Vector2 VGetVelocity(Cv_EntityID entityId)
        {
            var entity = CaravelApp.Instance.GameLogic.GetEntity(entityId);

            if (entity != null)
            {
                var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                if (rigidBodyComponent != null)
                {
                    return new Vector2(rigidBodyComponent.Velocity.X, rigidBodyComponent.Velocity.Y);
                }
            }

            Cv_Debug.Error("Entity does not exist or does not have rigid body.");
            return Vector2.Zero;
        }

         public override void VRotate(Cv_EntityID entityId, float angleRadians, float time)
        {
            var entity = CaravelApp.Instance.GameLogic.GetEntity(entityId);

            if (entity != null)
            {
                var transformComponent = entity.GetComponent<Cv_TransformComponent>();

                if (transformComponent != null)
                {
                    transformComponent.Rotation += angleRadians;
                }
            }
        }

        public override void VSetAngularVelocity(Cv_EntityID entityId, float vel)
        {
            var entity = CaravelApp.Instance.GameLogic.GetEntity(entityId);

            if (entity != null)
            {
                var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                if (rigidBodyComponent != null)
                {
                    rigidBodyComponent.AngularVelocity = vel;
                }
            }
        }

        public override void VSetTransform(Cv_EntityID entityId, Cv_Transform transf)
        {
            throw new System.NotImplementedException();
        }

        public override void VSetVelocity(Cv_EntityID entityId, Vector2 vel)
        {
            var entity = CaravelApp.Instance.GameLogic.GetEntity(entityId);

            if (entity != null)
            {
                var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                if (rigidBodyComponent != null)
                {
                    rigidBodyComponent.Velocity = new Vector3(vel, 0);
                }
            }
        }

        public override bool VInitialize()
        {
            LoadXML();
            return true;
        }

        public override void VOnUpdate(float timeElapsed)
        {
            SyncBodiesToEntities();
            m_World.Step(Math.Min(timeElapsed * 0.001f, (1f / 30f)));
        }

        public override void VRemoveEntity(Cv_EntityID id)
        {
            throw new System.NotImplementedException();
        }

        public override void VRenderDiagnostics()
        {
            throw new System.NotImplementedException();
        }

        public override void VStopEntity(Cv_EntityID entityId)
        {
            throw new System.NotImplementedException();
        }

        public override void VSyncVisibleScene()
        {
            foreach (var e in m_PhysicsEntities)
            {
                var eID = e.Key;

                var entity = CaravelApp.Instance.GameLogic.GetEntity(eID);

                if (entity != null)
                {
                    var transformComponent = entity.GetComponent<Cv_TransformComponent>();

                    if (transformComponent != null)
                    {
                        if (ToWorldCoord(transformComponent.Position.X) != e.Value.Body.Position.X
                            || ToWorldCoord(transformComponent.Position.Y) != e.Value.Body.Position.Y)
                        {
                            transformComponent.Position = new Vector3(ToOutsideVector(e.Value.Body.Position), transformComponent.Position.Z);
                        }

                        var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                        if (rigidBodyComponent != null)
                        {
                            if(!rigidBodyComponent.UseEntityRotation)
                            {
                                transformComponent.Rotation = e.Value.Body.Rotation;
                            }

                            SyncMovementDataFromBody(e.Value.Body, rigidBodyComponent);
                        }
                    }
                }
            }
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

        private void RemoveCollisionObject(Cv_CollisionShape toRemove)
        {
            Cv_PhysicsEntity pe;
            m_PhysicsEntities.TryGetValue(toRemove.Owner.ID, out pe);

            pe.Body.DestroyFixture(pe.Shapes[toRemove]);
            pe.Shapes.Remove(toRemove);
        }

        private Cv_PhysicsMaterial LookupMaterial(string material)
        {
            return m_MaterialsTable[material];
        }

        private bool OnBeforeCollision(Fixture fixtureA, Fixture fixtureB)
        {
            var collisionShapeA = (Cv_CollisionShape) fixtureA.UserData;
            var collisionShapeB = (Cv_CollisionShape) fixtureB.UserData;
            return true;
        }

        private bool OnNewCollisionPair(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            var collisionShapeA = (Cv_CollisionShape) fixtureA.UserData;
            var collisionShapeB = (Cv_CollisionShape) fixtureB.UserData;
            return true;
        }

        private void OnAfterCollision(Fixture fixtureA, Fixture fixtureB, Contact contact, ContactVelocityConstraint impulse)
        {
            var collisionShapeA = (Cv_CollisionShape) fixtureA.UserData;
            var collisionShapeB = (Cv_CollisionShape) fixtureB.UserData;
        }

        private void OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            var collisionShapeA = (Cv_CollisionShape) fixtureA.UserData;
            var collisionShapeB = (Cv_CollisionShape) fixtureB.UserData;
        }

        private void SyncBodiesToEntities()
        {
            foreach (var e in m_PhysicsEntities)
            {
                var entity = CaravelApp.Instance.GameLogic.GetEntity(e.Key);

                if (entity != null)
                {
                    var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();
                    var body = e.Value.Body;

                    if (rigidBodyComponent != null)
                    {
                        if (rigidBodyComponent.IsDirty)
                        {
                            SyncPhysicsSettings(body, rigidBodyComponent);
                            rigidBodyComponent.IsDirty = false;
                        }

                        if(rigidBodyComponent.CollisionShapes.Any(s => s.IsDirty))
                        {
                            SyncCollisionShapes(body, rigidBodyComponent);
                        }

                        SyncPositionData(body, entity, rigidBodyComponent.UseEntityRotation);
                        SyncMovementData(body, rigidBodyComponent);

                        body.ApplyForce(ToPhysicsVector(rigidBodyComponent.Acceleration));
                        body.ApplyLinearImpulse(ToPhysicsVector(rigidBodyComponent.Impulse));
                        rigidBodyComponent.Impulse = Vector3.Zero;

                        body.ApplyTorque(rigidBodyComponent.AngularAcceleration);
                        body.ApplyAngularImpulse(rigidBodyComponent.AngularImpulse);
                        rigidBodyComponent.AngularImpulse = 0f;
                    }
                }
            }
        }

        private void SyncPhysicsSettings(Body body, Cv_RigidBodyComponent physics)
        {
            Cv_PhysicsMaterial material;
            
            if(m_MaterialsTable.TryGetValue(physics.Material, out material))
            {
                body.Restitution = material.Restitution;
                body.Friction = material.Friction;
            }

            body.LinearDamping = physics.LinearDamping;
            body.AngularDamping = physics.AngularDamping;
            body.FixedRotation = physics.FixedRotation;
            body.GravityScale = physics.GravityScale;
        }

        private void SyncPositionData(Body body, Cv_Entity entity, bool useEntityRotation)
        {
            var transf = entity.GetComponent<Cv_TransformComponent>();

            if (transf != null)
            {
                body.Position = ToPhysicsVector(transf.Position);

                if(useEntityRotation)
                {
                    body.Rotation = transf.Rotation;
                }
            }
        }

        private void SyncMovementData(Body body, Cv_RigidBodyComponent rigidBody)
        {
            body.LinearVelocity = ToPhysicsVector(rigidBody.Velocity);

            if (body.LinearVelocity.Length() > ToWorldCoord(rigidBody.MaxVelocity))
            {
                Vector2 linVelocity = body.LinearVelocity;
                linVelocity.Normalize();
                body.LinearVelocity = linVelocity * ToWorldCoord(rigidBody.MaxVelocity);
            }

            body.AngularVelocity = rigidBody.AngularVelocity;
            if (body.AngularVelocity > rigidBody.MaxAngularVelocity)
            {
                body.AngularVelocity = rigidBody.MaxAngularVelocity;
            }
        }

        private void SyncCollisionShapes(Body body, Cv_RigidBodyComponent physics)
        {
            body.IsBullet = false;

            foreach (var shape in physics.CollisionShapes)
            {
                if (shape.IsDirty)
                {
                    var collisionShape = new Vertices();
                    var offsetX = shape.AnchorPoint.X;
                    var offsetY = shape.AnchorPoint.Y;

                    foreach (var vertice in shape.Points)
                    {
                        collisionShape.Add(ToWorldCoord(new Vector2(vertice.X - offsetX, vertice.Y - offsetY)));
                    }

                    Fixture fixture = null;
                    if (m_PhysicsEntities[physics.Owner.ID].Shapes.ContainsKey(shape))
                    {
                        fixture = m_PhysicsEntities[physics.Owner.ID].Shapes[shape];
                        body.DestroyFixture(fixture);
                    }

                    if(!shape.IsCircle)
                    {
                        fixture = body.CreateFixture(new PolygonShape(collisionShape, shape.Density), shape);
                    }
                    else
                    {
                        fixture = FixtureFactory.AttachCircle(ToWorldCoord(shape.Radius), shape.Density, body, ToWorldCoord(new Vector2(offsetX, offsetY)), shape);
                    }

                    fixture.IsSensor = shape.IsSensor;
                    fixture.Friction = shape.Friction;
                    fixture.CollisionCategories = shape.CollisionCategories;
                    fixture.CollidesWith = shape.CollidesWith;

                    fixture.OnCollision = null;
                    fixture.BeforeCollision = null;
                    fixture.AfterCollision = null;
                    fixture.OnSeparation = null;

                    fixture.OnCollision = OnNewCollisionPair;
                    fixture.BeforeCollision = OnBeforeCollision;
                    fixture.OnSeparation = OnSeparation;
                    fixture.AfterCollision = OnAfterCollision;

                    m_PhysicsEntities[physics.Owner.ID].Shapes[shape] = fixture;
                    shape.IsDirty = false;
                }

                if (shape.IsBullet)
                    body.IsBullet = true;
            }

            var toRemove = m_PhysicsEntities[physics.Owner.ID].Shapes.Where(kvp => !physics.CollisionShapes.Contains(kvp.Key)).ToList();

            foreach (var physEntity in toRemove)
            {
                RemoveCollisionObject(physEntity.Key);
            }
        }

        private void SyncMovementDataFromBody(Body body, Cv_RigidBodyComponent rigidBody)
        {
            if (body.LinearVelocity.Length() > ToWorldCoord(rigidBody.MaxVelocity))
            {
                Vector2 linVelocity = body.LinearVelocity;
                linVelocity.Normalize();
                body.LinearVelocity = linVelocity * ToWorldCoord(rigidBody.MaxVelocity);
            }

            rigidBody.Velocity = new Vector3(ToOutsideVector(body.LinearVelocity), rigidBody.Velocity.Z);

            if (body.AngularVelocity > rigidBody.MaxAngularVelocity)
            {
                body.AngularVelocity = rigidBody.MaxAngularVelocity;
            }
            rigidBody.AngularVelocity = body.AngularVelocity;
        }

        private Vector2 ToPhysicsVector(Vector3 vector3D)
        {
            Vector2 vector2D;
            vector2D = new Vector2(ToWorldCoord(vector3D.X), ToWorldCoord(vector3D.Y));

            return vector2D;
        }

        private Vector2 ToOutsideVector(Vector2 vector2D)
        {
            Vector2 outsideV;
            outsideV = new Vector2(ToScreenCoord(vector2D.X),
                                    ToScreenCoord(vector2D.Y));
            

            return outsideV;
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