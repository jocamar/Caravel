using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Caravel.Core.Events;
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
        private Dictionary<string, float> m_DensityTable;
        private Dictionary<string, Cv_PhysicsMaterial> m_MaterialsTable;
        private int m_iScreenToWorldRatio = 30;

        private readonly World m_World;

        public Cv_FarseerPhysics()
        {
            //Register events
            /*REGISTER_EVENT(EvtData_PhysTrigger_Enter);
            REGISTER_EVENT(EvtData_PhysTrigger_Leave);
            REGISTER_EVENT(EvtData_PhysCollision);
            REGISTER_EVENT(EvtData_PhysSeparation);*/

            m_World = new World(Vector2.Zero);

            m_PhysicsEntities = new Dictionary<Cv_EntityID, Cv_PhysicsEntity>();
            m_DensityTable = new Dictionary<string, float>();
            m_MaterialsTable = new Dictionary<string, Cv_PhysicsMaterial>();

            Cv_EventManager.Instance.AddListener<Cv_Event_NewCollisionShape>(OnNewCollisionShape);
            Cv_EventManager.Instance.AddListener<Cv_Event_DestroyEntity>(OnDestroyEntity);
            Cv_EventManager.Instance.AddListener<Cv_Event_TransformEntity>(OnTransformEntity);
        }

        ~Cv_FarseerPhysics()
        {
            Cv_EventManager.Instance.RemoveListener<Cv_Event_NewCollisionShape>(OnNewCollisionShape);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_DestroyEntity>(OnDestroyEntity);
            Cv_EventManager.Instance.RemoveListener<Cv_Event_TransformEntity>(OnTransformEntity);
        }

        public override Cv_CollisionShape VAddBox(Vector2 dimensions, Vector2 anchor, Cv_Entity gameEntity, string densityStr, string physicsMaterial, bool isBullet)
        {
            var rigidBodyComponent = gameEntity.GetComponent<Cv_RigidBodyComponent>();

            if (rigidBodyComponent == null)
            {
                Cv_Debug.Error("Entity does not contain a valid rigid body component.");
                return null;
            }

            Body body;

            if (!m_PhysicsEntities.ContainsKey(gameEntity.ID))
            {
                body = BodyFactory.CreateBody(m_World);
                var shapeMap = new Dictionary<Cv_CollisionShape, Fixture>();
                var physicsEntity = new Cv_PhysicsEntity();
                physicsEntity.Body = body;
                physicsEntity.Entity = gameEntity;
                physicsEntity.Shapes = shapeMap;

                m_PhysicsEntities.Add(gameEntity.ID, physicsEntity);

                if (rigidBodyComponent.RigidBodyType == Cv_RigidBodyComponent.BodyType.Kinematic)
                    body.BodyType = BodyType.Kinematic;
                else if (rigidBodyComponent.RigidBodyType == Cv_RigidBodyComponent.BodyType.Static)
                    body.BodyType = BodyType.Static;
                else
                    body.BodyType = BodyType.Dynamic;
            }
            
            body = m_PhysicsEntities[gameEntity.ID].Body;
            body.IsBullet = false;

            var verts = new Vertices();
            verts.Add(new Vector2(-dimensions.X/2, -dimensions.Y/2));
            verts.Add(new Vector2(dimensions.X/2, -dimensions.Y/2));
            verts.Add(new Vector2(dimensions.X/2, dimensions.Y/2));
            verts.Add(new Vector2(-dimensions.X/2, dimensions.Y/2));

            Cv_PhysicsMaterial material;
            float density;
            
            if (!m_MaterialsTable.TryGetValue(physicsMaterial, out material))
            {
                Cv_Debug.Error("Material does not exist on the physics system.");
                return null;
            }

            if (!m_DensityTable.TryGetValue(densityStr, out density))
            {
                Cv_Debug.Error("Density does not exist on the physics system.");
                return null;
            }

            if (!CheckPolygonValidity(verts))
            {
                return null;
            }

            var shape = new Cv_CollisionShape(verts, anchor, density, false, isBullet);
            shape.Owner = gameEntity;
            shape.Friction = material.Friction;
            
            var collisionShape = new Vertices();
            var offsetX = shape.AnchorPoint.X;
            var offsetY = shape.AnchorPoint.Y;

            foreach (var vertice in shape.Points)
            {
                collisionShape.Add(ToWorldCoord(new Vector2(vertice.X - offsetX, vertice.Y - offsetY)));
            }

            Fixture fixture = null;

            body.Friction = shape.Friction;
            body.Restitution = material.Restitution;
            fixture = body.CreateFixture(new PolygonShape(collisionShape, shape.Density), shape);

            fixture.Restitution = material.Restitution;
            fixture.IsSensor = shape.IsSensor;
            fixture.Friction = shape.Friction;
            fixture.CollisionCategories = (Category) shape.CollisionCategories.GetCategories();
            fixture.CollidesWith = (Category) shape.CollidesWith.GetCategories();

            fixture.OnCollision = OnNewCollisionPair;
            fixture.BeforeCollision = OnBeforeCollision;
            fixture.OnSeparation = OnSeparation;
            fixture.AfterCollision = OnAfterCollision;

            m_PhysicsEntities[rigidBodyComponent.Owner.ID].Shapes[shape] = fixture;
            shape.IsDirty = false;
                
            if (shape.IsBullet)
                body.IsBullet = true;
            
            return shape;
        }

        public override Cv_CollisionShape VAddPointShape(List<Vector2> verts, Vector2 anchor, Cv_Entity gameEntity, string densityStr, string physicsMaterial, bool isBullet)
        {
            var rigidBodyComponent = gameEntity.GetComponent<Cv_RigidBodyComponent>();

            if (rigidBodyComponent == null)
            {
                Cv_Debug.Error("Entity does not contain a valid rigid body component.");
                return null;
            }

            Body body;

            if (!m_PhysicsEntities.ContainsKey(gameEntity.ID))
            {
                body = BodyFactory.CreateBody(m_World);
                var shapeMap = new Dictionary<Cv_CollisionShape, Fixture>();
                var physicsEntity = new Cv_PhysicsEntity();
                physicsEntity.Body = body;
                physicsEntity.Entity = gameEntity;
                physicsEntity.Shapes = shapeMap;

                m_PhysicsEntities.Add(gameEntity.ID, physicsEntity);

                if (rigidBodyComponent.RigidBodyType == Cv_RigidBodyComponent.BodyType.Kinematic)
                    body.BodyType = BodyType.Kinematic;
                else if (rigidBodyComponent.RigidBodyType == Cv_RigidBodyComponent.BodyType.Static)
                    body.BodyType = BodyType.Static;
                else
                    body.BodyType = BodyType.Dynamic;
            }
            
            body = m_PhysicsEntities[gameEntity.ID].Body;
            body.IsBullet = false;

            var vertices = new Vertices(verts);

            Cv_PhysicsMaterial material;
            float density;
            
            if (!m_MaterialsTable.TryGetValue(physicsMaterial, out material))
            {
                Cv_Debug.Error("Material does not exist on the physics system.");
                return null;
            }

            if (!m_DensityTable.TryGetValue(densityStr, out density))
            {
                Cv_Debug.Error("Density does not exist on the physics system.");
                return null;
            }


            if (!CheckPolygonValidity(vertices))
            {
                return null;
            }

            var shape = new Cv_CollisionShape(vertices, anchor, density, false, isBullet);
            shape.Owner = gameEntity;
            shape.Friction = material.Friction;
            
            var collisionShape = new Vertices();
            var offsetX = shape.AnchorPoint.X;
            var offsetY = shape.AnchorPoint.Y;

            foreach (var vertice in shape.Points)
            {
                collisionShape.Add(ToWorldCoord(new Vector2(vertice.X - offsetX, vertice.Y - offsetY)));
            }

            Fixture fixture = null;

            body.Friction = shape.Friction;
            body.Restitution = material.Restitution;
            fixture = body.CreateFixture(new PolygonShape(collisionShape, shape.Density), shape);

            fixture.Restitution = material.Restitution;
            fixture.IsSensor = shape.IsSensor;
            fixture.Friction = shape.Friction;
            fixture.CollisionCategories = (Category) shape.CollisionCategories.GetCategories();
            fixture.CollidesWith = (Category) shape.CollidesWith.GetCategories();

            fixture.OnCollision = OnNewCollisionPair;
            fixture.BeforeCollision = OnBeforeCollision;
            fixture.OnSeparation = OnSeparation;
            fixture.AfterCollision = OnAfterCollision;

            m_PhysicsEntities[rigidBodyComponent.Owner.ID].Shapes[shape] = fixture;
            shape.IsDirty = false;
                
            if (shape.IsBullet)
                body.IsBullet = true;
            
            return shape;
        }

        public override Cv_CollisionShape VAddCircle(float radius, Vector2 anchor, Cv_Entity gameEntity, string densityStr, string physicsMaterial, bool isBullet)
        {
            var rigidBodyComponent = gameEntity.GetComponent<Cv_RigidBodyComponent>();

            if (rigidBodyComponent == null)
            {
                Cv_Debug.Error("Entity does not contain a valid rigid body component.");
                return null;
            }

            Body body;

            if (!m_PhysicsEntities.ContainsKey(gameEntity.ID))
            {
                body = BodyFactory.CreateBody(m_World);
                var shapeMap = new Dictionary<Cv_CollisionShape, Fixture>();
                var physicsEntity = new Cv_PhysicsEntity();
                physicsEntity.Body = body;
                physicsEntity.Entity = gameEntity;
                physicsEntity.Shapes = shapeMap;

                m_PhysicsEntities.Add(gameEntity.ID, physicsEntity);

                if (rigidBodyComponent.RigidBodyType == Cv_RigidBodyComponent.BodyType.Kinematic)
                    body.BodyType = BodyType.Kinematic;
                else if (rigidBodyComponent.RigidBodyType == Cv_RigidBodyComponent.BodyType.Static)
                    body.BodyType = BodyType.Static;
                else
                    body.BodyType = BodyType.Dynamic;
            }
            
            body = m_PhysicsEntities[gameEntity.ID].Body;
            body.IsBullet = false;

            Cv_PhysicsMaterial material;
            float density;
            
            if (!m_MaterialsTable.TryGetValue(physicsMaterial, out material))
            {
                Cv_Debug.Error("Material does not exist on the physics system.");
                return null;
            }

            if (!m_DensityTable.TryGetValue(densityStr, out density))
            {
                Cv_Debug.Error("Density does not exist on the physics system.");
                return null;
            }

            var shape = new Cv_CollisionShape(Vector2.Zero, radius, anchor, density, false, isBullet);
            shape.Owner = gameEntity;
            shape.Friction = material.Friction;
            
            var collisionShape = new Vertices();
            var offsetX = shape.AnchorPoint.X;
            var offsetY = shape.AnchorPoint.Y;

            foreach (var vertice in shape.Points)
            {
                collisionShape.Add(ToWorldCoord(new Vector2(vertice.X - offsetX, vertice.Y - offsetY)));
            }

            Fixture fixture = null;

            body.Friction = shape.Friction;
            body.Restitution = material.Restitution;
            fixture = FixtureFactory.AttachCircle(ToWorldCoord(shape.Radius), shape.Density, body, ToWorldCoord(new Vector2(offsetX, offsetY)), shape);

            fixture.Restitution = material.Restitution;
            fixture.IsSensor = shape.IsSensor;
            fixture.Friction = shape.Friction;
            fixture.CollisionCategories = (Category) shape.CollisionCategories.GetCategories();
            fixture.CollidesWith = (Category) shape.CollidesWith.GetCategories();

            fixture.OnCollision = OnNewCollisionPair;
            fixture.BeforeCollision = OnBeforeCollision;
            fixture.OnSeparation = OnSeparation;
            fixture.AfterCollision = OnAfterCollision;

            m_PhysicsEntities[rigidBodyComponent.Owner.ID].Shapes[shape] = fixture;
            shape.IsDirty = false;
                
            if (shape.IsBullet)
                body.IsBullet = true;
            
            return shape;
        }

        public override Cv_CollisionShape VCreateTrigger(Cv_Entity gameEntity, Vector2 pos, float dim, bool isBullet)
        {
            var rigidBodyComponent = gameEntity.GetComponent<Cv_RigidBodyComponent>();

            if (rigidBodyComponent == null)
            {
                Cv_Debug.Error("Entity does not contain a valid rigid body component.");
                return null;
            }

            Body body;

            if (!m_PhysicsEntities.ContainsKey(gameEntity.ID))
            {
                body = BodyFactory.CreateBody(m_World);
                var shapeMap = new Dictionary<Cv_CollisionShape, Fixture>();
                var physicsEntity = new Cv_PhysicsEntity();
                physicsEntity.Body = body;
                physicsEntity.Entity = gameEntity;
                physicsEntity.Shapes = shapeMap;

                m_PhysicsEntities.Add(gameEntity.ID, physicsEntity);

                if (rigidBodyComponent.RigidBodyType == Cv_RigidBodyComponent.BodyType.Kinematic)
                    body.BodyType = BodyType.Kinematic;
                else if (rigidBodyComponent.RigidBodyType == Cv_RigidBodyComponent.BodyType.Static)
                    body.BodyType = BodyType.Static;
                else
                    body.BodyType = BodyType.Dynamic;
            }
            
            body = m_PhysicsEntities[gameEntity.ID].Body;
            body.IsBullet = false;

            var verts = new Vertices();
            verts.Add(new Vector2(-dim/2, -dim/2));
            verts.Add(new Vector2(dim/2, -dim/2));
            verts.Add(new Vector2(dim/2, dim/2));
            verts.Add(new Vector2(-dim/2, dim/2));

            if (!CheckPolygonValidity(verts))
            {
                return null;
            }

            var shape = new Cv_CollisionShape(verts, pos, 1, true, isBullet);
            shape.Owner = gameEntity;
            
            var collisionShape = new Vertices();
            var offsetX = shape.AnchorPoint.X;
            var offsetY = shape.AnchorPoint.Y;

            foreach (var vertice in shape.Points)
            {
                collisionShape.Add(ToWorldCoord(new Vector2(vertice.X - offsetX, vertice.Y - offsetY)));
            }

            Fixture fixture = null;

            fixture = body.CreateFixture(new PolygonShape(collisionShape, shape.Density), shape);

            fixture.IsSensor = shape.IsSensor;
            fixture.Friction = shape.Friction;
            fixture.CollisionCategories = (Category) shape.CollisionCategories.GetCategories();
            fixture.CollidesWith = (Category) shape.CollidesWith.GetCategories();

            fixture.OnCollision = OnNewCollisionPair;
            fixture.BeforeCollision = OnBeforeCollision;
            fixture.OnSeparation = OnSeparation;
            fixture.AfterCollision = OnAfterCollision;

            m_PhysicsEntities[rigidBodyComponent.Owner.ID].Shapes[shape] = fixture;
            shape.IsDirty = false;
                
            if (shape.IsBullet)
                body.IsBullet = true;
            
            return shape;
        }

        public override void RemoveCollisionObject(Cv_CollisionShape toRemove)
        {
            Cv_PhysicsEntity pe;
            m_PhysicsEntities.TryGetValue(toRemove.Owner.ID, out pe);

            pe.Body.DestroyFixture(pe.Shapes[toRemove]);
            pe.Shapes.Remove(toRemove);
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
            if (!m_PhysicsEntities.ContainsKey(id))
            {
                return;
            }

            m_World.RemoveBody(m_PhysicsEntities[id].Body);
            m_PhysicsEntities.Remove(id);
        }

        public override void VRenderDiagnostics(Cv_Renderer renderer)
        {
            if (!DebugDraw)
            {
                return; 
            }

            foreach (var e in m_PhysicsEntities.Values)
            {
                var pos = e.Body.Position;

                Rectangle r = new Rectangle((int)(ToScreenCoord(pos.X) - 1), (int)(ToScreenCoord(pos.Y) - 1), 2, 2);
                Cv_DrawUtils.DrawRectangle(renderer, r, 4, Color.Blue);
                Vertices verts;

                Color color;
                foreach (var fixture in e.Body.FixtureList)
                {
                    color = Color.Red;

                    if (fixture.IsSensor)
                        color = Color.Green;

                    if (fixture.Shape.GetType() != typeof(CircleShape))
                    {
                        verts = ((PolygonShape)fixture.Shape).Vertices;
                        DrawCollisionShape(verts, pos, e.Body.Rotation, renderer, color);
                    }
                    else
                    {
                        var circle = ((CircleShape)fixture.Shape);

                        var rotMatrixZ = Matrix.CreateRotationZ(e.Body.Rotation);
                        var point = circle.Position;
                        point = Vector2.Transform(point, rotMatrixZ);

                        Rectangle r2 = new Rectangle((int)(ToScreenCoord(pos.X + point.X - circle.Radius)), 
                                                    (int)(ToScreenCoord(pos.Y + point.Y - circle.Radius)), 
                                                    (int)ToScreenCoord(circle.Radius)*2, 
                                                    (int)ToScreenCoord(circle.Radius)*2);
                        renderer.Draw(((Cv_CollisionShape) fixture.UserData).CircleOutlineTex, r2, color);
                    }
                    
                    DrawBoundingBox((Cv_CollisionShape) fixture.UserData, ToScreenCoord(pos), renderer);
                }
            }
        }

        private void DrawBoundingBox(Cv_CollisionShape shape, Vector2 pos, Cv_Renderer renderer)
		{
            var boundingBox = shape.AABoundingBox;
			Rectangle r = new Rectangle((int) (boundingBox.Start.X + pos.X),
										(int) (boundingBox.Start.Y + pos.Y),
										(int) boundingBox.Width,
										(int) boundingBox.Height);
			Cv_DrawUtils.DrawRectangle(renderer, r, 2, Color.Green);
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
                        if (Math.Floor(transformComponent.Position.X) != Math.Floor(ToScreenCoord(e.Value.Body.Position.X))
                            || Math.Floor(transformComponent.Position.Y) != Math.Floor(ToScreenCoord(e.Value.Body.Position.Y)))
                        {
                            transformComponent.Position = new Vector3(ToOutsideVector(e.Value.Body.Position), transformComponent.Position.Z);
                        }

                        var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                        if (rigidBodyComponent != null)
                        {
                            if(rigidBodyComponent.UseEntityRotation && e.Value.Body.Rotation != transformComponent.Rotation)
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

        public void OnNewCollisionShape(Cv_Event eventData)
        {
            var newCollisionEvt = (Cv_Event_NewCollisionShape) eventData;
            var shapeData = newCollisionEvt.ShapeData;
            var entity = CaravelApp.Instance.GameLogic.GetEntity(newCollisionEvt.EntityID);

            switch (shapeData.Type)
            {
                case Cv_RigidBodyComponent.ShapeType.Circle:
                    VAddCircle(shapeData.Radius, shapeData.Anchor, entity, shapeData.Density, shapeData.Material, shapeData.IsBullet);
                    break;
                case Cv_RigidBodyComponent.ShapeType.Box:
                    VAddBox(shapeData.Dimensions, shapeData.Anchor, entity, shapeData.Density, shapeData.Material, shapeData.IsBullet);
                    break;
                case Cv_RigidBodyComponent.ShapeType.Trigger:
                    VCreateTrigger(entity, shapeData.Anchor, shapeData.Dimensions.X, shapeData.IsBullet);
                    break;
                default:
                    var points = new List<Vector2>(shapeData.Points);
                    VAddPointShape(points, shapeData.Anchor, entity, shapeData.Density, shapeData.Material, shapeData.IsBullet);
                    break;
            }
        }

        public void OnDestroyEntity(Cv_Event eventData)
        {
            VRemoveEntity(eventData.EntityID);
        }

        public void OnTransformEntity(Cv_Event eventData)
        {
            var transformEntityEvt = (Cv_Event_TransformEntity) eventData;

            if (m_PhysicsEntities.ContainsKey(eventData.EntityID))
            {
                var pe = m_PhysicsEntities[eventData.EntityID];

                if (transformEntityEvt.OldScale == null || transformEntityEvt.OldScale.Value != transformEntityEvt.NewScale)
                {
                    var oldScale = new Vector2(1, 1);

                    if (transformEntityEvt.OldScale != null)
                    {
                        oldScale = transformEntityEvt.OldScale.Value;
                    }

                    foreach (var f in pe.Shapes)
                    {
                        if (f.Value.Shape.ShapeType == ShapeType.Polygon)
                        {
                            PolygonShape polygon = (PolygonShape) f.Value.Shape;

                            var oldVerts = polygon.Vertices;
                            var newVerts = new Vertices();

                            foreach (var v in oldVerts)
                            {
                                var newVert = ToWorldCoord( (ToScreenCoord(v) / oldScale) * transformEntityEvt.NewScale );
                                newVerts.Add(newVert);
                            }

                            polygon.Vertices = newVerts;
                        }
                        else if (f.Value.Shape.ShapeType == ShapeType.Circle)
                        {
                            CircleShape circle = (CircleShape) f.Value.Shape;

                            circle.Radius = ToWorldCoord((ToScreenCoord(circle.Radius) / Math.Max(oldScale.X, oldScale.Y))
                                                            * Math.Max(transformEntityEvt.NewScale.X, transformEntityEvt.NewScale.Y));
                        }

                        var colShape = f.Key;
                        var newPoints = new List<Vector2>();
                        foreach (var p in colShape.Points)
                        {
                            var newPoint = (p / oldScale) * transformEntityEvt.NewScale;
                            newPoints.Add(newPoint);
                        }

                        colShape.Points = newPoints;
                    }
                }

                if (transformEntityEvt.OldPosition == null
                    || Math.Floor(transformEntityEvt.NewPosition.X) != Math.Floor(transformEntityEvt.OldPosition.Value.X)
                    || Math.Floor(transformEntityEvt.NewPosition.Y) != Math.Floor(transformEntityEvt.OldPosition.Value.Y))
                {
                    pe.Body.Position = ToPhysicsVector(transformEntityEvt.NewPosition);
                }

                if (transformEntityEvt.OldRotation == null || transformEntityEvt.NewRotation != transformEntityEvt.OldRotation.Value)
                {
                    if(pe.Entity.GetComponent<Cv_RigidBodyComponent>().UseEntityRotation)
                    {
                        pe.Body.Rotation = transformEntityEvt.NewRotation;
                    }
                }
            }
        }

        private void AddShape(Cv_Entity entity, Cv_CollisionShape shape, float mass, string physicsMaterial)
        {

        }

        private void DrawCollisionShape(Vertices collisionShape, Vector2 position, float rotation, Cv_Renderer renderer, Color c)
		{
			if(collisionShape.Count >= 2)
			{
                var rotMatrixZ = Matrix.CreateRotationZ(rotation);

                Vector2 point1;
                Vector2 point2;
                for (int i = 0, j = 1; i < collisionShape.Count; i++, j++)
				{
					if (j >= collisionShape.Count)
						j = 0;

                    point1 = new Vector2(collisionShape[i].X, collisionShape[i].Y);
                    point2 = new Vector2(collisionShape[j].X, collisionShape[j].Y);
                    point1 = Vector2.Transform(point1, rotMatrixZ);
                    point2 = Vector2.Transform(point2, rotMatrixZ);
                    point1 += position;
                    point2 += position;

                    Cv_DrawUtils.DrawLine(renderer,
						                                ToScreenCoord(point1),
                                                        ToScreenCoord(point2),
						                                1,
						                                c);
				}
			}
		}

        private void LoadXML()
        {
            var physicsConfig = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>("config/physics.xml");
            var root = ((Cv_XmlResource.Cv_XmlData) physicsConfig.ResourceData).RootNode;

            var materials = root.SelectNodes("//Materials").Item(0);

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
                

                m_MaterialsTable.Add(material.Name, new Cv_PhysicsMaterial(friction, restitution));
            }

            var densities = root.SelectNodes("//Densities").Item(0);

            foreach(XmlElement density in densities.ChildNodes)
            {
                m_DensityTable.Add(density.Name, float.Parse(density.FirstChild.Value,  CultureInfo.InvariantCulture));
            }
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

        private bool CheckPolygonValidity(Vertices points)
        {
            PolygonError error = points.CheckPolygon();
			if (error == PolygonError.AreaTooSmall || error == PolygonError.SideTooSmall)
            {
				Cv_Debug.Error("CollisionShape does not support shapes of that size.");
                return false;
            }
			if (error == PolygonError.InvalidAmountOfVertices)
            {
				Cv_Debug.Error("CollisionShape does not yet support shapes with over 8 or under 1 vertex.");
                return false;
            }
			if (error == PolygonError.NotConvex)
            {
				Cv_Debug.Error("CollisionShape does not support non convex shapes.");
                return false;
            }
			if (error == PolygonError.NotCounterClockWise)
            {
				Cv_Debug.Error("CollisionShape does not support non counter-clockwise shapes.");
                return false;
            }
			if (error == PolygonError.NotSimple)
            {
				Cv_Debug.Error("CollisionShape does not support non simple shapes.");
                return false;
            }

            return true;
        }
    }
}