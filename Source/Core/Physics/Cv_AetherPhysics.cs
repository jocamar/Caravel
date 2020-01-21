using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Debugging;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using tainicom.Aether.Physics2D.Primitives;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Physics
{
    public class Cv_AetherPhysics : Cv_GamePhysics
    {
        private class Cv_PhysicsEntity
        {
            public Cv_Entity Entity;
            public Body Body;
            public Dictionary<Cv_CollisionShape, Fixture> Shapes;
            public Cv_Transform PrevWorldTransform;
        }

        public Microsoft.Xna.Framework.Vector2 Gravity
        {
            get
            {
                return new Microsoft.Xna.Framework.Vector2(m_World.Gravity.X, m_World.Gravity.Y);
            }

            set
            {
                m_World.Gravity = new Vector2(value.X, value.Y);
            }
        }

        public int Screen2WorldRatio
        {
            get; set;
        }

        public CaravelApp Caravel
        {
            get; private set;
        }

        private Dictionary<Cv_EntityID, Cv_PhysicsEntity> m_PhysicsEntities;
        private List<Cv_PhysicsEntity> m_PhysicsEntitiesList;
        private List<Cv_PhysicsEntity> m_PhysicsEntitiesToUpdate;
        private Dictionary<string, Cv_PhysicsMaterial> m_MaterialsTable;
		private List<Cv_RayCastIntersection> m_RaycastEntities;
		private Cv_RayCastType m_RaycastType;
        private List<Tuple<Cv_CollisionShape, Cv_CollisionShape>> m_CollisionPairs;
        private List<Tuple<Cv_CollisionShape, Cv_CollisionShape>> m_SeparationPairs;

        private readonly World m_World;
        private Cv_ListenerList m_Listeners = new Cv_ListenerList();

        public Cv_AetherPhysics(CaravelApp app)
        {
            Caravel = app;
            m_World = new World(Vector2.Zero);
            Screen2WorldRatio = 30;

            m_PhysicsEntities = new Dictionary<Cv_EntityID, Cv_PhysicsEntity>();
            m_PhysicsEntitiesList = new List<Cv_PhysicsEntity>();
            m_PhysicsEntitiesToUpdate = new List<Cv_PhysicsEntity>();
            m_MaterialsTable = new Dictionary<string, Cv_PhysicsMaterial>();
			m_RaycastEntities = new List<Cv_RayCastIntersection>();
            m_CollisionPairs = new List<Tuple<Cv_CollisionShape, Cv_CollisionShape>>();
            m_SeparationPairs = new List<Tuple<Cv_CollisionShape, Cv_CollisionShape>>();

            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_NewCollisionShape>(OnNewCollisionShape);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_ClearCollisionShapes>(OnClearCollisionShapes);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_DestroyEntity>(OnDestroyEntity);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_DestroyRigidBodyComponent>(OnDestroyEntity);
            m_Listeners += Cv_EventManager.Instance.AddListener<Cv_Event_TransformEntity>(OnMoveEntity);
        }

        ~Cv_AetherPhysics()
        {
            m_Listeners.Dispose();
        }

        public override Cv_CollisionShape VAddBox(Cv_Entity gameEntity, Cv_ShapeData data)
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
                body = CreateNewBody(gameEntity);
            }
            
            body = m_PhysicsEntities[gameEntity.ID].Body;
            body.IsBullet = false;

            var verts = new Vertices();
            verts.Add(new Vector2(-data.Dimensions.X/2, -data.Dimensions.Y/2));
            verts.Add(new Vector2(data.Dimensions.X/2, -data.Dimensions.Y/2));
            verts.Add(new Vector2(data.Dimensions.X/2, data.Dimensions.Y/2));
            verts.Add(new Vector2(-data.Dimensions.X/2, data.Dimensions.Y/2));

            Cv_PhysicsMaterial material;
            
            if (!m_MaterialsTable.TryGetValue(data.Material, out material))
            {
                Cv_Debug.Error("Material does not exist on the physics system.");
                return null;
            }
            else if (!CheckPolygonValidity(verts))
            {
                return null;
            }

            var shape = new Cv_CollisionShape(data.ShapeID, Verts2Points(verts), data.Anchor, material.Density, false, data.IsBullet,
													data.Categories, data.CollidesWith, data.CollisionDirections);
            shape.Owner = gameEntity;
            shape.Friction = material.Friction;
            shape.Restitution = material.Restitution;
            
            return AddShape(gameEntity, shape, false);
        }

        public override Cv_CollisionShape VAddPointShape(Cv_Entity gameEntity, Cv_ShapeData data)
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
                body = CreateNewBody(gameEntity);
            }
            
            body = m_PhysicsEntities[gameEntity.ID].Body;
            body.IsBullet = false;

            var vertices = Points2Verts(data.Points);

            Cv_PhysicsMaterial material;
            
            if (!m_MaterialsTable.TryGetValue(data.Material, out material))
            {
                Cv_Debug.Error("Material does not exist on the physics system.");
                return null;
            }
            else if (!CheckPolygonValidity(vertices))
            {
                return null;
            }

            var shape = new Cv_CollisionShape(data.ShapeID, Verts2Points(vertices), data.Anchor, material.Density, false, data.IsBullet,
													data.Categories, data.CollidesWith, data.CollisionDirections);
            shape.Owner = gameEntity;
            shape.Friction = material.Friction;
            shape.Restitution = material.Restitution;
            
            return AddShape(gameEntity, shape, false);
        }

        public override Cv_CollisionShape VAddCircle(Cv_Entity gameEntity, Cv_ShapeData data)
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
                body = CreateNewBody(gameEntity);
            }
            
            body = m_PhysicsEntities[gameEntity.ID].Body;
            body.IsBullet = false;

            Cv_PhysicsMaterial material;
            
            if (!m_MaterialsTable.TryGetValue(data.Material, out material))
            {
                Cv_Debug.Error("Material does not exist on the physics system.");
                return null;
            }

            var shape = new Cv_CollisionShape(data.ShapeID, Microsoft.Xna.Framework.Vector2.Zero, data.Radius, data.Anchor, material.Density, false,
												data.IsBullet, data.Categories, data.CollidesWith, data.CollisionDirections);
            shape.Owner = gameEntity;
            shape.Friction = material.Friction;
            shape.Restitution = material.Restitution;
            
            return AddShape(gameEntity, shape, false);
        }

        public override Cv_CollisionShape VAddTrigger(Cv_Entity gameEntity, Cv_ShapeData data)
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
                body = CreateNewBody(gameEntity);
            }
            
            body = m_PhysicsEntities[gameEntity.ID].Body;
            body.IsBullet = false;

            var verts = new Vertices();
            verts.Add(new Vector2(-data.Dimensions.X/2, -data.Dimensions.X/2));
            verts.Add(new Vector2(data.Dimensions.X/2, -data.Dimensions.X/2));
            verts.Add(new Vector2(data.Dimensions.X/2, data.Dimensions.X/2));
            verts.Add(new Vector2(-data.Dimensions.X/2, data.Dimensions.X/2));

            if (!CheckPolygonValidity(verts))
            {
                return null;
            }

            var shape = new Cv_CollisionShape(data.ShapeID, Verts2Points(verts), data.Anchor, 0, true, data.IsBullet,
												data.Categories, data.CollidesWith, data.CollisionDirections);
            shape.Owner = gameEntity;
            
            return AddShape(gameEntity, shape, true);
        }

        public override void RemoveCollisionObject(Cv_CollisionShape toRemove)
        {
            Cv_PhysicsEntity pe;
            m_PhysicsEntities.TryGetValue(toRemove.Owner.ID, out pe);

            pe.Body.Remove(pe.Shapes[toRemove]);
            pe.Shapes.Remove(toRemove);
        }

        public override void VApplyForce(Microsoft.Xna.Framework.Vector2 dir, float newtons, Cv_EntityID entityId)
        {
            var entity = Caravel.Logic.GetEntity(entityId);

            if (entity != null)
            {
                var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                if (rigidBodyComponent != null)
                {
                    Microsoft.Xna.Framework.Vector3 force = new Microsoft.Xna.Framework.Vector3(dir, 0);
                    force.Normalize();
                    rigidBodyComponent.Impulse = force * newtons;
                }
            }
        }

        public override void VApplyTorque(float newtons, Cv_EntityID entityId)
        {
            var entity = Caravel.Logic.GetEntity(entityId);

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
            var entity = Caravel.Logic.GetEntity(entityId);

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

        public override Microsoft.Xna.Framework.Vector2 VGetVelocity(Cv_EntityID entityId)
        {
            var entity = Caravel.Logic.GetEntity(entityId);

            if (entity != null)
            {
                var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                if (rigidBodyComponent != null)
                {
                    return new Microsoft.Xna.Framework.Vector2(rigidBodyComponent.Velocity.X, rigidBodyComponent.Velocity.Y);
                }
            }

            Cv_Debug.Error("Entity does not exist or does not have rigid body.");
            return Microsoft.Xna.Framework.Vector2.Zero;
        }

        public override void VSetAngularVelocity(Cv_EntityID entityId, float vel)
        {
            var entity = Caravel.Logic.GetEntity(entityId);

            if (entity != null)
            {
                var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                if (rigidBodyComponent != null)
                {
                    rigidBodyComponent.AngularVelocity = vel;
                }
            }
        }

        public override void VSetVelocity(Cv_EntityID entityId, Microsoft.Xna.Framework.Vector2 vel)
        {
            var entity = Caravel.Logic.GetEntity(entityId);

            if (entity != null)
            {
                var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                if (rigidBodyComponent != null)
                {
                    rigidBodyComponent.Velocity = new Microsoft.Xna.Framework.Vector3(vel, 0);
                }
            }
        }

        public override bool VInitialize()
        {
            LoadXML();
            return true;
        }

        public override void VOnUpdate(float elapsedTime)
        {
            SyncBodiesToEntities();
            m_World.Step(Math.Min(elapsedTime * 0.001f, (1f / 30f)));
            m_CollisionPairs.Clear();
            m_SeparationPairs.Clear();
        }

        public override void VRemoveEntity(Cv_EntityID id)
        {
            Cv_PhysicsEntity entity;
            if (!m_PhysicsEntities.TryGetValue(id, out entity))
            {
                return;
            }

            m_World.Remove(m_PhysicsEntities[id].Body);
            m_PhysicsEntities.Remove(id);
            m_PhysicsEntitiesList.Remove(entity);
        }

        public override string[] GetMaterials()
        {
            return m_MaterialsTable.Keys.ToArray();
        }

        public override void VRenderDiagnostics(Cv_CameraNode camera, Cv_Renderer renderer)
        {
            if (!renderer.DebugDrawPhysicsShapes && !renderer.DebugDrawPhysicsBoundingBoxes)
            {
                return;
            }

            var pv = Caravel.GetPlayerView();

            if (pv == null)
            {
                return;
            }
            
            foreach (var e in m_PhysicsEntitiesList)
            {
                var pos = e.Body.Position;

                Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle((int)(ToScreenCoord(pos.X) - 1), (int)(ToScreenCoord(pos.Y) - 1), 2, 2);
                Cv_DrawUtils.DrawRectangle(renderer, r, 4, Microsoft.Xna.Framework.Color.Blue);
                Vertices verts;

                Microsoft.Xna.Framework.Color color;
                foreach (var fixture in e.Body.FixtureList)
                {
                    if (renderer.DebugDrawPhysicsShapes)
                    {
                        color = Microsoft.Xna.Framework.Color.Red;
                        
                        if (e.Entity.ID == pv.EditorSelectedEntity)
                        {
                            color = Microsoft.Xna.Framework.Color.Yellow;
                        }

                        if (fixture.IsSensor)
                            color = Microsoft.Xna.Framework.Color.Green;

                        if (fixture.Shape.GetType() != typeof(CircleShape))
                        {
                            verts = ((PolygonShape)fixture.Shape).Vertices;
                            DrawCollisionShape(verts, pos, e.Body.Rotation, renderer, camera.Zoom, color);
                        }
                        else
                        {
                            var circle = ((CircleShape)fixture.Shape);

                            var rotMatrixZ = Matrix.CreateRotationZ(e.Body.Rotation);
                            var point = circle.Position;
                            point = Vector2.Transform(point, rotMatrixZ);

                            Microsoft.Xna.Framework.Rectangle r2 = new Microsoft.Xna.Framework.Rectangle((int)(ToScreenCoord(pos.X + point.X - circle.Radius)), 
                                                        (int)(ToScreenCoord(pos.Y + point.Y - circle.Radius)), 
                                                        (int)ToScreenCoord(circle.Radius)*2, 
                                                        (int)ToScreenCoord(circle.Radius)*2);
                            renderer.Draw(((Cv_CollisionShape) fixture.Tag).CircleOutlineTex, r2, color);
                        }
                    }
                    
                    if (renderer.DebugDrawPhysicsBoundingBoxes)
                    {
                        DrawBoundingBox((Cv_CollisionShape) fixture.Tag, ToScreenCoord(pos), renderer, camera.Zoom);
                    }
                }
            }
        }

        public override void VStopEntity(Cv_EntityID entityId)
        {
            VSetVelocity(entityId, Microsoft.Xna.Framework.Vector2.Zero);
        }

        public override void VSyncVisibleScene()
        {
            foreach (var e in m_PhysicsEntitiesList)
            {
                var entity = e.Entity;

                if (entity != null)
                {
                    var transformComponent = entity.GetComponent<Cv_TransformComponent>();

                    if (transformComponent != null)
                    {
                        var parent = CaravelApp.Instance.Logic.GetEntity(entity.Parent);

                        var parenttrans = Cv_Transform.Identity;
                        if (parent != null && parent.GetComponent<Cv_TransformComponent>() != null)
                        {
                            parenttrans = parent.GetComponent<Cv_TransformComponent>().WorldTransform;
                        }

                        var worldTransform = Cv_Transform.Multiply(parenttrans, transformComponent.Transform);
                        var newWorldPosition = ToOutsideVector(e.Body.Position, false);
                        var oldWorldPosition = new Microsoft.Xna.Framework.Vector2(worldTransform.Position.X, worldTransform.Position.Y);
                        var posDifference = newWorldPosition - oldWorldPosition;

                        if (posDifference.Length() > 0.00001f)
                        {
                            var newPosition = new Microsoft.Xna.Framework.Vector3(transformComponent.Position.X + (posDifference.X / parenttrans.Scale.X), transformComponent.Position.Y + (posDifference.Y / parenttrans.Scale.Y), transformComponent.Position.Z);
                            transformComponent.SetPosition(newPosition, this);
                        }

                        var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

                        if (rigidBodyComponent != null)
                        {
                            var newWorldRotation = e.Body.Rotation;
                            var oldWorldRotation = worldTransform.Rotation;
                            var rotDifference = newWorldRotation - oldWorldRotation;
                            if(rigidBodyComponent.UseEntityRotation && e.Body.Rotation != worldTransform.Rotation)
                            {
                                var newRotation = transformComponent.Rotation + rotDifference;
                                transformComponent.SetRotation(newRotation, this);
                            }

                            SyncMovementDataFromBody(e.Body, rigidBodyComponent);
                        }
                    }
                }
            }
        }

        public override Cv_PhysicsMaterial GetMaterial(string material)
        {
            return m_MaterialsTable[material];
        }

		public override Cv_RayCastIntersection[] RayCast(Microsoft.Xna.Framework.Vector2 startingPoint, Microsoft.Xna.Framework.Vector2 endingPoint, Cv_RayCastType type)
		{
			m_RaycastEntities.Clear();
			m_RaycastType = type;

			m_World.RayCast(OnRayCastIntersection,  ToWorldCoord(startingPoint), ToWorldCoord(endingPoint));

            m_RaycastEntities.Sort((i1, i2) => { return (int)((i1.Point - startingPoint).Length() - (i2.Point - startingPoint).Length()); } );

			return m_RaycastEntities.ToArray();
		}

        protected void SyncBodiesToEntities()
        {
            foreach (var e in m_PhysicsEntities)
            {
                var entity = Caravel.Logic.GetEntity(e.Key);

                if (entity != null)
                {
                    var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();
                    var body = e.Value.Body;

                    if (rigidBodyComponent != null)
                    {
                        if (rigidBodyComponent.IsDirty)
                        {
                            SyncPhysicsSettings(body, rigidBodyComponent);
                            SyncShapeSettings(e.Value, rigidBodyComponent);

                            rigidBodyComponent.IsDirty = false;
                        }

                        SyncMovementData(body, rigidBodyComponent);

                        body.ApplyForce(ToPhysicsVector(rigidBodyComponent.Acceleration));
                        body.ApplyLinearImpulse(ToPhysicsVector(rigidBodyComponent.Impulse));
                        rigidBodyComponent.Impulse = Microsoft.Xna.Framework.Vector3.Zero;

                        body.ApplyTorque(rigidBodyComponent.AngularAcceleration);
                        body.ApplyAngularImpulse(rigidBodyComponent.AngularImpulse);
                        rigidBodyComponent.AngularImpulse = 0f;
                    }
                }
            }
        }

        private void DrawBoundingBox(Cv_CollisionShape shape, Microsoft.Xna.Framework.Vector2 pos, Cv_Renderer renderer, float zoom)
		{
            var thickness = (int) Math.Round(2 / zoom);
            if (thickness <= 0)
            {
                thickness = 1;
            }

            var boundingBox = shape.AABoundingBox;
            Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle((int) (boundingBox.Start.X + pos.X),
										(int) (boundingBox.Start.Y + pos.Y),
										(int) boundingBox.Width,
										(int) boundingBox.Height);
			Cv_DrawUtils.DrawRectangle(renderer, r, thickness, Microsoft.Xna.Framework.Color.Green);
		}

        private Cv_CollisionShape AddShape(Cv_Entity entity, Cv_CollisionShape shape, bool isTrigger)
        {
            var body = m_PhysicsEntities[entity.ID].Body;
            var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();
            var collisionShape = new Vertices();

            if (entity.GetComponent<Cv_TransformComponent>() != null)
            {
                var scale = entity.GetComponent<Cv_TransformComponent>().WorldTransform.Scale;

                if (shape.IsCircle)
                {
                    shape.Radius *= Math.Max(scale.X, scale.Y);
                }
                else
                {
                    var newPoints = new List<Microsoft.Xna.Framework.Vector2>();
                    foreach (var p in shape.Points)
                    {
                        var newPoint = p * scale;
                        newPoints.Add(newPoint);
                    }

                    shape.Points = newPoints;
                }

                shape.AnchorPoint *= scale;
            }

            var offsetX = shape.AnchorPoint.X;
            var offsetY = shape.AnchorPoint.Y;

            foreach (var vertice in shape.Points)
            {
                collisionShape.Add(ToWorldCoord(new Microsoft.Xna.Framework.Vector2(vertice.X - offsetX, vertice.Y - offsetY)));
            }

            Fixture fixture = null;

            if (!isTrigger)
            {
                var material = m_MaterialsTable[rigidBodyComponent.Material];
                body.SetFriction(material.Friction);
                body.SetRestitution(material.Restitution);
            }

            if (shape.IsCircle)
            {
                fixture = body.CreateCircle(ToWorldCoord(shape.Radius), shape.Density, ToWorldCoord(new Microsoft.Xna.Framework.Vector2(offsetX, offsetY)));
                fixture.Tag = shape;
            }
            else
            {
                fixture = body.CreateFixture(new PolygonShape(collisionShape, shape.Density));
                fixture.Tag = shape;
            }

            if (!isTrigger)
            {
                fixture.Restitution = shape.Restitution;
            }

            fixture.IsSensor = shape.IsSensor;
            fixture.Friction = shape.Friction;
            fixture.CollisionCategories = (Category) shape.CollisionCategories.GetCategories();
            fixture.CollidesWith = (Category) shape.CollidesWith.GetCategories();

            fixture.OnCollision = OnNewCollisionPair;
            fixture.BeforeCollision = OnBeforeCollision;
            fixture.OnSeparation = OnSeparation;
            fixture.AfterCollision = OnAfterCollision;

            m_PhysicsEntities[rigidBodyComponent.Owner.ID].Shapes[shape] = fixture;
                
            if (shape.IsBullet)
                body.IsBullet = true;
            
            return shape;
        }

        private Body CreateNewBody(Cv_Entity entity)
        {
            var body = m_World.CreateBody();
            body.Tag = entity.EntityName;
            var shapeMap = new Dictionary<Cv_CollisionShape, Fixture>();
            var physicsEntity = new Cv_PhysicsEntity();
            physicsEntity.Body = body;
            physicsEntity.Entity = entity;
            physicsEntity.Shapes = shapeMap;

            m_PhysicsEntities.Add(entity.ID, physicsEntity);
            m_PhysicsEntitiesList.Add(physicsEntity);

            var rigidBodyComponent = entity.GetComponent<Cv_RigidBodyComponent>();

            if (rigidBodyComponent.RigidBodyType == Cv_RigidBodyComponent.Cv_BodyType.Kinematic)
                body.BodyType = BodyType.Kinematic;
            else if (rigidBodyComponent.RigidBodyType == Cv_RigidBodyComponent.Cv_BodyType.Static)
                body.BodyType = BodyType.Static;
            else
                body.BodyType = BodyType.Dynamic;

            var transformComponent = entity.GetComponent<Cv_TransformComponent>();

            if (transformComponent != null)
            {
                var worldTransform = transformComponent.WorldTransform;
                body.Position = ToPhysicsVector(worldTransform.Position);
                body.Rotation = worldTransform.Rotation;
                physicsEntity.PrevWorldTransform = worldTransform;
            }
            else
            {
                physicsEntity.PrevWorldTransform = Cv_Transform.Identity;
            }

            return body;
        }

        private void DrawCollisionShape(Vertices collisionShape, Vector2 position, float rotation, Cv_Renderer renderer, float zoom, Microsoft.Xna.Framework.Color c)
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

                    var thickness = (int) Math.Ceiling(3 / zoom);
                    if (thickness <= 0)
                    {
                        thickness = 1;
                    }

                    Cv_DrawUtils.DrawLine(renderer,
						                                ToScreenCoord(point1),
                                                        ToScreenCoord(point2),
						                                thickness,
                                                        Cv_Renderer.MaxLayers-1,
						                                c);
				}
			}
		}

        private void LoadXML()
        {
            if (Caravel.MaterialsLocation == null || Caravel.MaterialsLocation == "")
            {
                Cv_Debug.Log("Physics", "No materials to load.");
                return;
            }

            m_MaterialsTable.Clear();

            XmlDocument doc = new XmlDocument();
            doc.Load(Caravel.MaterialsLocation);

            var root = doc.FirstChild;

            var materials = root.SelectNodes("Materials").Item(0);

            foreach(XmlElement material in materials.ChildNodes)
            {
                float restitution = 0;
                float friction = 0;
                float density = 1;
                if (material.Attributes["restitution"] != null)
                {
                    restitution = float.Parse(material.Attributes["restitution"].Value,  CultureInfo.InvariantCulture);
                }

                if (material.Attributes["friction"] != null)
                {
                    friction = float.Parse(material.Attributes["friction"].Value,  CultureInfo.InvariantCulture);
                }

                if (material.Attributes["density"] != null)
                {
                    density = float.Parse(material.Attributes["density"].Value,  CultureInfo.InvariantCulture);
                }
                
                m_MaterialsTable.Add(material.Name, new Cv_PhysicsMaterial(friction, restitution, density));
            }
        }

        private bool OnBeforeCollision(Fixture fixtureA, Fixture fixtureB)
        {
            return true;
        }

        private bool OnNewCollisionPair(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            var collisionShapeA = (Cv_CollisionShape) fixtureA.Tag;
            var collisionShapeB = (Cv_CollisionShape) fixtureB.Tag;

            var collidedShape = collisionShapeA;
            var collidedFixture = fixtureA;
            var collidingShape = collisionShapeB;
            var collidingFixture = fixtureB;
            if (fixtureA.Body.LinearVelocity.Length() > fixtureB.Body.LinearVelocity.Length())
            {
                collidedShape = collisionShapeB;
                collidedFixture = fixtureB;
                collidingShape = collisionShapeA;
                collidingFixture = fixtureA;
            }

            var collisionDirection = GetCollisionDirection(contact);

            if (collisionDirection == Cv_CollisionDirection.Left || collisionDirection == Cv_CollisionDirection.Right)
            {
                if (collidingFixture.Body.LinearVelocity.X > 0)
                {
                    collisionDirection = Cv_CollisionDirection.Right;
                }
                else
                {
                    collisionDirection = Cv_CollisionDirection.Left;
                }
            }
            else
            {
                if (collidingFixture.Body.LinearVelocity.Y > 0)
                {
                    collisionDirection = Cv_CollisionDirection.Bottom;
                }
                else
                {
                    collisionDirection = Cv_CollisionDirection.Top;
                }
            }

            if (!collidingShape.CollidesWithFromDirection(collidedShape.CollisionCategories, DirectionToString(collisionDirection)))
            {
                return false;
            }

            var aetherContact = GenerateContact(contact, collidingShape, collidedShape);

            if (!m_CollisionPairs.Exists(cp => cp.Item1 == collidingShape && cp.Item2 == collidedShape))
            {
                if (collidedShape.IsSensor || collidingShape.IsSensor)
                {
                    var newEvent = new Cv_Event_EnterTrigger(aetherContact, this);
                    Cv_EventManager.Instance.QueueEvent(newEvent);
                    m_CollisionPairs.Add(new Tuple<Cv_CollisionShape, Cv_CollisionShape>(collidingShape, collidedShape));
                }
                else
                {
                    var newEvent = new Cv_Event_NewCollision(aetherContact);
                    Cv_EventManager.Instance.QueueEvent(newEvent);
                    m_CollisionPairs.Add(new Tuple<Cv_CollisionShape, Cv_CollisionShape>(collidingShape, collidedShape));
                }
            }

            return true;
        }

        private void OnAfterCollision(Fixture fixtureA, Fixture fixtureB, Contact contact, ContactVelocityConstraint impulse)
        {
        }

        private void OnSeparation(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            var collisionShapeA = (Cv_CollisionShape)fixtureA.Tag;
            var collisionShapeB = (Cv_CollisionShape)fixtureB.Tag;

            var aetherContact = GenerateContact(contact, collisionShapeA, collisionShapeB);

            if (!m_SeparationPairs.Exists(sp => sp.Item1 == collisionShapeB && sp.Item2 == collisionShapeA))
            {
                if (collisionShapeA.IsSensor || collisionShapeB.IsSensor)
                {
                    var newEvent = new Cv_Event_LeaveTrigger(aetherContact, this);
                    Cv_EventManager.Instance.QueueEvent(newEvent);
                    m_SeparationPairs.Add(new Tuple<Cv_CollisionShape, Cv_CollisionShape>(collisionShapeA, collisionShapeB));
                }
                else
                {
                    var newEvent = new Cv_Event_NewSeparation(aetherContact);
                    Cv_EventManager.Instance.QueueEvent(newEvent);
                    m_SeparationPairs.Add(new Tuple<Cv_CollisionShape, Cv_CollisionShape>(collisionShapeA, collisionShapeB));
                }
            }
        }

		private float OnRayCastIntersection(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
		{
			if (fixture.IsSensor && (m_RaycastType == Cv_RayCastType.AllSolid || m_RaycastType == Cv_RayCastType.ClosestSolid))
			{
				return -1;
			}

            var intersection = new Cv_RayCastIntersection();
            intersection.Entity = ((Cv_CollisionShape) fixture.Tag).Owner;
            intersection.Point = ToScreenCoord(point);
            intersection.Normal = new Microsoft.Xna.Framework.Vector2(normal.X, normal.Y);

			m_RaycastEntities.Add(intersection);

			if (m_RaycastType == Cv_RayCastType.Closest || m_RaycastType == Cv_RayCastType.ClosestSolid)
			{
				return 0;
			}
			else
			{
				return 1;
			}
		}

		private void OnNewCollisionShape(Cv_Event eventData)
        {
            var newCollisionEvt = (Cv_Event_NewCollisionShape) eventData;
            var shapeData = newCollisionEvt.ShapeData;
            var entity = Caravel.Logic.GetEntity(newCollisionEvt.EntityID);

            if (entity == null)
            {
                return;
            }

            switch (shapeData.Type)
            {
                case ShapeType.Circle:
                    VAddCircle(entity, shapeData);
                    break;
                case ShapeType.Box:
                    VAddBox(entity, shapeData);
                    break;
                case ShapeType.Trigger:
                    VAddTrigger(entity, shapeData);
                    break;
                default:
                    VAddPointShape(entity, shapeData);
                    break;
            }
        }

        private void OnClearCollisionShapes(Cv_Event eventData)
        {
            if (m_PhysicsEntities.ContainsKey(eventData.EntityID))
            {
                var collisionShapes = m_PhysicsEntities[eventData.EntityID].Shapes.Keys.ToArray();

                foreach (var shape in collisionShapes)
                {
                    RemoveCollisionObject(shape);
                }
            }
        }

        private void OnDestroyEntity(Cv_Event eventData)
        {
            VRemoveEntity(eventData.EntityID);
        }

        private void OnMoveEntity(Cv_Event eventData)
        {
            GetChildEntitiesToUpdate(eventData.EntityID);

            Cv_PhysicsEntity movedEntity = null;
            if (eventData.Sender == this && m_PhysicsEntitiesToUpdate.Count == 0)
            {
                if (m_PhysicsEntities.TryGetValue(eventData.EntityID, out movedEntity))
                {
                    var transformComp = movedEntity.Entity.GetComponent<Cv_TransformComponent>();
                    
                    if (transformComp != null)
                    {
                        movedEntity.PrevWorldTransform = transformComp.WorldTransform;
                    }
                }

                return;
            }

            if (eventData.Sender != this && m_PhysicsEntities.TryGetValue(eventData.EntityID, out movedEntity))
            {
                m_PhysicsEntitiesToUpdate.Add(movedEntity);
            }

            //TODO(JM) Fix concurrent access here 
            foreach (var pe in m_PhysicsEntitiesToUpdate)
            {
                var transformComp = pe.Entity.GetComponent<Cv_TransformComponent>();
                if (transformComp == null)
                {
                    continue;
                }

                var worldTransform = transformComp.WorldTransform;
                if (pe.PrevWorldTransform.Scale != worldTransform.Scale)
                {
                    var oldScale = pe.PrevWorldTransform.Scale;
                    foreach (var f in pe.Shapes)
                    {
                        if (f.Value.Shape.ShapeType == tainicom.Aether.Physics2D.Collision.Shapes.ShapeType.Polygon)
                        {
                            PolygonShape polygon = (PolygonShape) f.Value.Shape;

                            var oldVerts = polygon.Vertices;
                            var newVerts = new Vertices();

                            foreach (var v in oldVerts)
                            {
                                var newVert = ToWorldCoord( (ToScreenCoord(v) / oldScale) * worldTransform.Scale );
                                newVerts.Add(newVert);
                            }

                            polygon.Vertices = newVerts;
                        }
                        else if (f.Value.Shape.ShapeType == tainicom.Aether.Physics2D.Collision.Shapes.ShapeType.Circle)
                        {
                            CircleShape circle = (CircleShape) f.Value.Shape;

                            circle.Radius = ToWorldCoord((ToScreenCoord(circle.Radius) / Math.Max(oldScale.X, oldScale.Y))
                                                            * Math.Max(worldTransform.Scale.X, worldTransform.Scale.Y));
                        }

                        var colShape = f.Key;
                        var newPoints = new List<Microsoft.Xna.Framework.Vector2>();
                        foreach (var p in colShape.Points)
                        {
                            var newPoint = (p / oldScale) * worldTransform.Scale;
                            newPoints.Add(newPoint);
                        }

                        colShape.Points = newPoints;
                    }
                }

                pe.Body.Position = ToPhysicsVector(worldTransform.Position);

                if(pe.Entity.GetComponent<Cv_RigidBodyComponent>().UseEntityRotation)
                {
                    pe.Body.Rotation = worldTransform.Rotation;
                }

                pe.PrevWorldTransform = worldTransform;
            }
        }

        private void SyncPhysicsSettings(Body body, Cv_RigidBodyComponent physics)
        {
            Cv_PhysicsMaterial material;
            
            if(m_MaterialsTable.TryGetValue(physics.Material, out material))
            {
                body.SetRestitution(material.Restitution);
                body.SetFriction(material.Friction);
            }

            body.LinearDamping = physics.LinearDamping;
            body.AngularDamping = physics.AngularDamping;
            body.FixedRotation = physics.FixedRotation;
            body.GravityScale = physics.GravityScale;
        }

        private void SyncShapeSettings(Cv_PhysicsEntity e, Cv_RigidBodyComponent physics)
        {
            foreach (var ps in e.Shapes.Keys)
            {
                var materialID = physics.GetShapeMaterial(ps.ShapeID);

                Cv_PhysicsMaterial material;
                if (m_MaterialsTable.TryGetValue(materialID, out material))
                {
                    e.Shapes[ps].Restitution = material.Restitution;
                    e.Shapes[ps].Friction = material.Friction;
                    ps.Friction = material.Friction;
                    ps.Restitution = material.Restitution;
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

        private void SyncMovementDataFromBody(Body body, Cv_RigidBodyComponent rigidBody)
        {
            if (body.LinearVelocity.Length() > ToWorldCoord(rigidBody.MaxVelocity))
            {
                Vector2 linVelocity = body.LinearVelocity;
                linVelocity.Normalize();
                body.LinearVelocity = linVelocity * ToWorldCoord(rigidBody.MaxVelocity);
            }

            rigidBody.Velocity = new Microsoft.Xna.Framework.Vector3(ToOutsideVector(body.LinearVelocity), rigidBody.Velocity.Z);

            if (body.AngularVelocity > rigidBody.MaxAngularVelocity)
            {
                body.AngularVelocity = rigidBody.MaxAngularVelocity;
            }
            rigidBody.AngularVelocity = body.AngularVelocity;
        }

        private Vector2 ToPhysicsVector(Microsoft.Xna.Framework.Vector3 vector3D)
        {
            Vector2 vector2D;
            vector2D = new Vector2(ToWorldCoord(vector3D.X), ToWorldCoord(vector3D.Y));

            return vector2D;
        }

        private Microsoft.Xna.Framework.Vector2 ToOutsideVector(Vector2 vector2D, bool round = false)
        {
            Microsoft.Xna.Framework.Vector2 outsideV;
            if (round)
            {
                outsideV = new Microsoft.Xna.Framework.Vector2((float)Math.Round(ToScreenCoord(vector2D.X)),
                                        (float)Math.Round(ToScreenCoord(vector2D.Y)));
            }
            else
            {
                outsideV = new Microsoft.Xna.Framework.Vector2(ToScreenCoord(vector2D.X),
                                        ToScreenCoord(vector2D.Y));
            }
            

            return outsideV;
        }

        private Vector2 _ToOutsideVector(Vector2 vector2D, bool round = false)
        {
            Vector2 outsideV;
            if (round)
            {
                outsideV = new Vector2((float)Math.Round(ToScreenCoord(vector2D.X)),
                                        (float)Math.Round(ToScreenCoord(vector2D.Y)));
            }
            else
            {
                outsideV = new Vector2(ToScreenCoord(vector2D.X),
                                        ToScreenCoord(vector2D.Y));
            }


            return outsideV;
        }

        private float ToWorldCoord(float coord)
        {
            return coord / Screen2WorldRatio;
        }

        private Vector2 ToWorldCoord(Microsoft.Xna.Framework.Vector2 coord)
        {
            return new Vector2(coord.X, coord.Y) / Screen2WorldRatio;
        }

        private float ToScreenCoord(float coord)
        {
            return coord * Screen2WorldRatio;
        }

        private Microsoft.Xna.Framework.Vector2 ToScreenCoord(Vector2 coord)
        {
            return new Microsoft.Xna.Framework.Vector2(coord.X, coord.Y) * Screen2WorldRatio;
        }

        private List<Microsoft.Xna.Framework.Vector2> Verts2Points(Vertices verts)
        {
            List<Microsoft.Xna.Framework.Vector2> rtnVal = new List<Microsoft.Xna.Framework.Vector2>();

            foreach(var vert in verts)
            {
                rtnVal.Add(new Microsoft.Xna.Framework.Vector2(vert.X, vert.Y));
            }

            return rtnVal;
        }

        private Vertices Points2Verts(Microsoft.Xna.Framework.Vector2[] points)
        {
            Vertices rtnVal = new Vertices();

            foreach (var point in points)
            {
                rtnVal.Add(new Vector2(point.X, point.Y));
            }

            return rtnVal;
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

        private Cv_CollisionDirection GetCollisionDirection(Contact c)
        {
            Cv_CollisionDirection direction;

            // Work out collision direction
            Vector2 colNorm;
            FixedArray2<Vector2> points;
            c.GetWorldManifold(out colNorm, out points);
            if (Math.Abs(colNorm.X) > Math.Abs(colNorm.Y))
            {
                // X direction is dominant
                if (colNorm.X > 0)
                    direction = Cv_CollisionDirection.Right;
                else
                    direction = Cv_CollisionDirection.Left;
            }
            else
            {
                // Y direction is dominant
                if (colNorm.Y > 0)
                    direction = Cv_CollisionDirection.Bottom;
                else
                    direction = Cv_CollisionDirection.Top;

            }

            return direction;
        }

        private string DirectionToString(Cv_CollisionDirection direction)
        {
            switch (direction)
            {
                case Cv_CollisionDirection.Bottom:
                    return "Bottom";
                case Cv_CollisionDirection.Left:
                    return "Left";
                case Cv_CollisionDirection.Right:
                    return "Right";
                default:
                    return "Top";
            }
        }

        private Cv_CollisionDirection GetReverseDirection(Cv_CollisionDirection direction)
        {
            switch(direction)
            {
                case Cv_CollisionDirection.Bottom:
                    return Cv_CollisionDirection.Top;
                case Cv_CollisionDirection.Left:
                    return  Cv_CollisionDirection.Right;
                case Cv_CollisionDirection.Right:
                    return Cv_CollisionDirection.Left;
                default:
                    return Cv_CollisionDirection.Bottom;
            }
        }

        private void GetChildEntitiesToUpdate(Cv_EntityID entityId)
        {
            m_PhysicsEntitiesToUpdate.Clear();
            if (entityId == Cv_EntityID.INVALID_ENTITY)
            {
                return;
            }
            
            //TODO(JM) Fix concurrent access here
            foreach (var pe in m_PhysicsEntitiesList)
            {
                if (EntityIsDescendantOf(pe.Entity, entityId))
                {
                    m_PhysicsEntitiesToUpdate.Add(pe);
                }
            }
        }

        private bool EntityIsDescendantOf(Cv_Entity entity1, Cv_EntityID entity2)
        {
            var currEntity = entity1;
            while (currEntity.Parent != Cv_EntityID.INVALID_ENTITY)
            {
                if (currEntity.Parent == entity2)
                {
                    return true;
                }

                currEntity = Caravel.Logic.GetEntity(currEntity.Parent);
            }

            return false;
        }

        private Cv_AetherContact GenerateContact(Contact contact, Cv_CollisionShape collidingShape, Cv_CollisionShape collidedShape)
        {
            List<Microsoft.Xna.Framework.Vector2> collisionPoints = new List<Microsoft.Xna.Framework.Vector2>();
            Vector2 normalForce = Vector2.Zero;

            FixedArray2<Vector2> manifoldPoints;
            contact.GetWorldManifold(out normalForce, out manifoldPoints);

            for (var i = 0; i < contact.Manifold.PointCount; i++)
            {
                var point = manifoldPoints[i];

                collisionPoints.Add(ToOutsideVector(point));
            }

            normalForce += _ToOutsideVector(normalForce);

            var aetherContact = new Cv_AetherContact(contact);
            aetherContact.CollidedShape = collidedShape;
            aetherContact.CollidingShape = collidingShape;
            aetherContact.NormalForce = new Microsoft.Xna.Framework.Vector2(normalForce.X, normalForce.Y);
            aetherContact.CollisionPoints = collisionPoints.ToArray();

            if (m_PhysicsEntities.ContainsKey(collidedShape.Owner.ID))
            {
                aetherContact.CollidedShapeVelocity = ToOutsideVector(m_PhysicsEntities[collidedShape.Owner.ID].Body.LinearVelocity);
            }

            if (m_PhysicsEntities.ContainsKey(collidingShape.Owner.ID))
            {
                aetherContact.CollidingShapeVelocity = ToOutsideVector(m_PhysicsEntities[collidingShape.Owner.ID].Body.LinearVelocity);
            }

            return aetherContact;
        }
    }
}