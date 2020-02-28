using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Caravel.Core.Events;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using static Caravel.Core.Physics.Cv_CollisionShape;
using static Caravel.Core.Physics.Cv_GamePhysics;

namespace Caravel.Core.Entity
{
    public class Cv_RigidBodyComponent : Cv_EntityComponent
    {
        public enum Cv_BodyType { Static, Dynamic, Kinematic };

        public Cv_BodyType RigidBodyType
        {
            get
            {
                return m_RigidBodyType;
            }
            set
            {
                m_RigidBodyType = value;
                IsDirty = true;
            }
        }

        public bool FixedRotation
        {
            get
            {
                return m_bFixedRotation;
            }
            set
            {
                m_bFixedRotation = value;
                IsDirty = true;
            }
        }

        public float LinearDamping
        {
            get
            {
                return m_fLinearDamping;
            }
            set
            {
                m_fLinearDamping = value;
                IsDirty = true;
            }
        }

        public float AngularDamping
        {
            get
            {
                return m_fAngularDamping;
            }
            set
            {
                m_fAngularDamping = value;
                IsDirty = true;
            }
        }

        public float GravityScale
        {
            get
            {
                return m_fGravityScale;
            }
            set
            {
                m_fGravityScale = value;
                IsDirty = true;
            }
        }

        public bool UseEntityRotation { get; set; }

        public string Material
        {
            get
            {
                return m_sMaterial;
            }

            set
            {
                m_sMaterial = value;
                IsDirty = true;
            }
        }

        public string[] Shapes
        {
            get
            {
                return m_Shapes.Select(s => s.ShapeID).ToArray();
            }
        }

        #region Movement properties
        public float MaxVelocity { get; set; }
        public float MaxAngularVelocity { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Acceleration { get; set; }
        public Vector3 Impulse { get; set; }
        public float AngularVelocity { get; set; }
        public float AngularAcceleration { get; set; }
        public float AngularImpulse { get; set; }
        #endregion

        internal bool IsDirty { get; set; }

        #region Private physics members
        private Cv_BodyType m_RigidBodyType;
        private float m_fLinearDamping;
        private float m_fAngularDamping;
        private float m_fGravityScale;
        private bool m_bFixedRotation;
        private string m_sMaterial;
        #endregion

        private List<Cv_ShapeData> m_Shapes = new List<Cv_ShapeData>();

        public string GetShapeMaterial(string shapeId)
        {
            foreach (var s in m_Shapes)
            {
                if (s.ShapeID == shapeId)
                {
                    return s.Material;
                }
            }

            return null;
        }

        public void SetShapeMaterial(string shapeId, string materialId)
        {
            for (var i = 0; i < m_Shapes.Count; i++)
            {
                if (m_Shapes[i].ShapeID == shapeId)
                {
                    var newShape = new Cv_ShapeData(m_Shapes[i]);
                    newShape.Material = materialId;

                    m_Shapes[i] = newShape;
                }
            }

            IsDirty = true;
        }

        public override XmlElement VToXML()
        {
            var doc = new XmlDocument();
            var rigidBodyElement = doc.CreateElement(GetComponentName(this));

            var materialElement = doc.CreateElement("Material");
            materialElement.SetAttribute("material", m_sMaterial);
            rigidBodyElement.AppendChild(materialElement);

            var physicsElement = doc.CreateElement("Physics");
            physicsElement.SetAttribute("fixedRotation", m_bFixedRotation.ToString(CultureInfo.InvariantCulture));
            physicsElement.SetAttribute("gravityScale", m_fGravityScale.ToString(CultureInfo.InvariantCulture));
            physicsElement.SetAttribute("maxVelocity", MaxVelocity.ToString(CultureInfo.InvariantCulture));
            physicsElement.SetAttribute("maxAngVelocity", MaxAngularVelocity.ToString(CultureInfo.InvariantCulture));
            rigidBodyElement.AppendChild(physicsElement);

            var bodyElement = doc.CreateElement("Body");
            bodyElement.SetAttribute("linearDamping", m_fLinearDamping.ToString(CultureInfo.InvariantCulture));
            bodyElement.SetAttribute("angularDamping", m_fAngularDamping.ToString(CultureInfo.InvariantCulture));
            bodyElement.SetAttribute("followEntityRotation", UseEntityRotation.ToString(CultureInfo.InvariantCulture));

            var bodyTypeStr = "";

            switch (m_RigidBodyType)
            {
                case Cv_BodyType.Static:
                    bodyTypeStr = "static";
                    break;
                case Cv_BodyType.Dynamic:
                    bodyTypeStr = "dynamic";
                    break;
                default:
                    bodyTypeStr = "kinematic";
                    break;
            }

            bodyElement.SetAttribute("type", bodyTypeStr);
            rigidBodyElement.AppendChild(bodyElement);

            var collisionShapesElement = doc.CreateElement("CollisionShapes");
            
            foreach (var shape in m_Shapes)
            {
                var shapeTypeStr = "";

                switch (shape.Type)
                {
                    case ShapeType.Box:
                        shapeTypeStr = "Box";
                        break;
                    case ShapeType.Circle:
                        shapeTypeStr = "Circle";
                        break;
                    case ShapeType.Polygon:
                        shapeTypeStr = "Polygon";
                        break;
                    default:
                        shapeTypeStr = "Trigger";
                        break;
                }

                var shapeElement = doc.CreateElement(shapeTypeStr);
                shapeElement.SetAttribute("id", shape.ShapeID);
                shapeElement.SetAttribute("material", shape.Material);
                shapeElement.SetAttribute("anchorX", ((int)shape.Anchor.X).ToString(CultureInfo.InvariantCulture));
                shapeElement.SetAttribute("anchorY", ((int)shape.Anchor.Y).ToString(CultureInfo.InvariantCulture));
                shapeElement.SetAttribute("isBullet", shape.IsBullet.ToString(CultureInfo.InvariantCulture));

                if (shape.Type == ShapeType.Circle)
                {
                    shapeElement.SetAttribute("radius", shape.Radius.ToString(CultureInfo.InvariantCulture));
                }
                else if (shape.Type == ShapeType.Box)
                {
                    shapeElement.SetAttribute("dimensionsX", ((int)shape.Dimensions.X).ToString(CultureInfo.InvariantCulture));
                    shapeElement.SetAttribute("dimensionsY", ((int)shape.Dimensions.Y).ToString(CultureInfo.InvariantCulture));
                }
                else if (shape.Type == ShapeType.Trigger)
                {
                    shapeElement.SetAttribute("dimensions", ((int)shape.Dimensions.X).ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    foreach (var point in shape.Points)
                    {
                        var pointElement = doc.CreateElement("Point");
                        pointElement.SetAttribute("x", ((int)point.X).ToString(CultureInfo.InvariantCulture));
                        pointElement.SetAttribute("y", ((int)point.Y).ToString(CultureInfo.InvariantCulture));
                        shapeElement.AppendChild(pointElement);
                    }
                }

                var collisionCategories = shape.Categories.GetCategoriesArray();
                foreach (var category in collisionCategories)
                {
                    var collisionCategoriesElement = doc.CreateElement("CollisionCategory");
                    collisionCategoriesElement.SetAttribute("id", category.ToString(CultureInfo.InvariantCulture));
                    shapeElement.AppendChild(collisionCategoriesElement);
                }

                var collidesWith = shape.CollidesWith.GetCategoriesArray();
                foreach (var category in collidesWith)
                {
                    var collidesWithElement = doc.CreateElement("CollidesWith");
                    collidesWithElement.SetAttribute("id", category.ToString(CultureInfo.InvariantCulture));
                    collidesWithElement.SetAttribute("directions", shape.CollisionDirections[category]);
                    shapeElement.AppendChild(collidesWithElement);
                }

                collisionShapesElement.AppendChild(shapeElement);
            }

            rigidBodyElement.AppendChild(collisionShapesElement);

            return rigidBodyElement;
        }

        public override bool VInitialize(XmlElement componentData)
        {
            XmlElement materialNode = (XmlElement) componentData.SelectSingleNode("Material");
            if (materialNode != null)
            {
                m_sMaterial = materialNode.Attributes["material"].Value;
            }

            XmlElement physicsNode = (XmlElement) componentData.SelectSingleNode("Physics");
            if (physicsNode != null)
            {
                m_bFixedRotation = bool.Parse(physicsNode.Attributes["fixedRotation"].Value);
                m_fGravityScale = float.Parse(physicsNode.Attributes["gravityScale"].Value, CultureInfo.InvariantCulture);
                MaxVelocity = float.Parse(physicsNode.Attributes["maxVelocity"].Value, CultureInfo.InvariantCulture);
                MaxAngularVelocity = float.Parse(physicsNode.Attributes["maxAngVelocity"].Value, CultureInfo.InvariantCulture);
            }

            XmlElement bodyNode = (XmlElement) componentData.SelectSingleNode("Body");
            if (bodyNode != null)
            {
                m_fLinearDamping = float.Parse(bodyNode.Attributes["linearDamping"].Value, CultureInfo.InvariantCulture);
                m_fAngularDamping = float.Parse(bodyNode.Attributes["angularDamping"].Value, CultureInfo.InvariantCulture);
                UseEntityRotation = bool.Parse(bodyNode.Attributes["followEntityRotation"].Value);
                var bodyType = bodyNode.Attributes["type"].Value.ToLowerInvariant();

                switch (bodyType)
                {
                    case "static":
                        m_RigidBodyType = Cv_BodyType.Static;
                        break;
                    case "kinematic":
                        m_RigidBodyType = Cv_BodyType.Kinematic;
                        break;
                    case "dynamic":
                        m_RigidBodyType = Cv_BodyType.Dynamic;
                        break;
                    default:
                        Cv_Debug.Error("Invalid body type. Unable to build component.");
                        return false;
                }
            }

            
            var collisionShapesNode = componentData.SelectSingleNode("CollisionShapes");
            if (collisionShapesNode != null)
            {
                m_Shapes.Clear();
                
                foreach(XmlElement shape in collisionShapesNode.ChildNodes)
                {
                    var shapeData = new Cv_ShapeData();

                    var shapeType = shape.Name.ToLowerInvariant();

                    switch (shapeType)
                    {
                        case "box":
                            shapeData.Type = ShapeType.Box;
                            break;
                        case "circle":
                            shapeData.Type = ShapeType.Circle;
                            break;
                        case "polygon":
                            shapeData.Type = ShapeType.Polygon;
                            break;
                        case "trigger":
                            shapeData.Type = ShapeType.Trigger;
                            break;
                        default:
                            Cv_Debug.Error("Invalid shape type. Unable to build component.");
                            return false;
                    }

                    shapeData.ShapeID = shape.Attributes?["id"].Value;
                    shapeData.Material = shape.Attributes?["material"].Value;

                    int x, y;

                    x = int.Parse(shape.Attributes?["anchorX"].Value, CultureInfo.InvariantCulture);
                    y = int.Parse(shape.Attributes?["anchorY"].Value, CultureInfo.InvariantCulture);
                    shapeData.Anchor = new Vector2((float) x, (float) y);

                    shapeData.IsBullet = bool.Parse(shape.Attributes?["isBullet"].Value);

                    if (shapeData.Type == ShapeType.Circle)
                    {
                        shapeData.Radius = float.Parse(shape.Attributes?["radius"].Value, CultureInfo.InvariantCulture);
                    }
                    else if (shapeData.Type == ShapeType.Box)
                    {
                        x = int.Parse(shape.Attributes?["dimensionsX"].Value, CultureInfo.InvariantCulture);
                        y = int.Parse(shape.Attributes?["dimensionsY"].Value, CultureInfo.InvariantCulture);
                        shapeData.Dimensions = new Vector2((float) x, (float) y);
                    }
                    else if (shapeData.Type == ShapeType.Trigger)
                    {
                        x = int.Parse(shape.Attributes?["dimensions"].Value, CultureInfo.InvariantCulture);
                        shapeData.Dimensions = new Vector2((float) x, (float) x);
                    }
                    else
                    {
                        var points = new List<Vector2>();
                        var shapePoints = shape.SelectNodes("Point");
                        foreach (XmlElement point in shapePoints)
                        {
                            x = int.Parse(point.Attributes?["x"].Value, CultureInfo.InvariantCulture);
                            y = int.Parse(point.Attributes?["y"].Value, CultureInfo.InvariantCulture);
                            points.Add(new Vector2((float) x, (float) y));
                        }

                        shapeData.Points = points.ToArray();
                    }

                    var shapeCollisionCategories = shape.SelectNodes("CollisionCategory");
                    shapeData.Categories = new Cv_CollisionCategories();
                    foreach (XmlElement category in shapeCollisionCategories)
                    {
                        var id = int.Parse(category.Attributes?["id"].Value, CultureInfo.InvariantCulture);

                        if (shapeData.Categories.HasCategory(id))
                        {
                            id = 0;

                            while (shapeData.Categories.HasCategory(id))
                            {
                                id++;

                                if (id >= 32)
                                {
                                    Cv_Debug.Error("Trying to add a collision category when all have been used already.");
                                }
                            }
                        }

                        shapeData.Categories.AddCategory(id);
                    }

                    var shapeCollidesWith = shape.SelectNodes("CollidesWith");
                    shapeData.CollidesWith = new Cv_CollisionCategories();
                    shapeData.CollisionDirections = new Dictionary<int, string>();
                    foreach (XmlElement category in shapeCollidesWith)
                    {
                        var id = int.Parse(category.Attributes?["id"].Value, CultureInfo.InvariantCulture);

                        if (shapeData.CollidesWith.HasCategory(id))
                        {
                            id = 0;

                            while (shapeData.CollidesWith.HasCategory(id))
                            {
                                id++;

                                if (id >= 32)
                                {
                                    Cv_Debug.Error("Trying to add a collision category when all have been used already.");
                                }
                            }
                        }

                        shapeData.CollidesWith.AddCategory(id);
                        shapeData.CollisionDirections.Add(id, category.Attributes["directions"].Value);
                    }

                    m_Shapes.Add(shapeData);
                }
            }

            IsDirty = true;
            return true;
        }

        public override void VOnChanged()
        {
            Cv_Event newEvt = new Cv_Event_ClearCollisionShapes(Owner.ID, this);
            Cv_EventManager.Instance.QueueEvent(newEvt, true);

            foreach (var s in m_Shapes)
            {
                newEvt = new Cv_Event_NewCollisionShape(Owner.ID, s, this);
                Cv_EventManager.Instance.QueueEvent(newEvt, true);
            }
        }

        public void SetCollisionCategory(string shapeId, int category, bool state)
        {
            if (!Owner)
            {
                return;
            }
            
            foreach (var s in m_Shapes)
            {
                if (s.ShapeID == shapeId)
                {
                    if (state)
                    {
                        s.Categories.AddCategory(category);
                    }
                    else
                    {
                        s.Categories.RemoveCategory(category);
                    }
                }
            }

            var evt = new Cv_Event_SetCollisionCategory(shapeId, category, state, Owner.ID, this);
            Cv_EventManager.Instance.TriggerEvent(evt);
        }

        public void SetCollidesWith(Cv_Entity.Cv_EntityID otherID, bool state, string shapeID = null, string otherShapeID = null)
        {
            if (Owner)
            {
                CaravelApp.Instance.Logic.SetCollidesWith(Owner.ID, otherID, state, shapeID, otherShapeID);
            }
        }

        public void SetCollidesWith(string shapeId, int category, bool state, string direction)
        {
            if (!Owner)
            {
                return;
            }

            foreach (var s in m_Shapes)
            {
                if (s.ShapeID == shapeId)
                {
                    if (state)
                    {
                        s.CollidesWith.AddCategory(category);
                        s.CollisionDirections[category] = direction;
                    }
                    else
                    {
                        s.Categories.RemoveCategory(category);
                        s.CollisionDirections.Remove(category);
                    }
                }
            }

            var evt = new Cv_Event_SetCollidesWith(shapeId, category, direction, state, Owner.ID, this);
            Cv_EventManager.Instance.TriggerEvent(evt);
        }

        public bool HasCategory(string shapeId, int category)
        {

            if (m_Shapes.Any(s => s.ShapeID == shapeId))
            {
                return m_Shapes.Find(s => s.ShapeID == shapeId).Categories.HasCategory(category);
            }

            return false;
        }

        public bool CollidesWith(string shapeId, int category, out string direction)
        {
            if (m_Shapes.Any(s => s.ShapeID == shapeId))
            {
                var shape = m_Shapes.Find(s => s.ShapeID == shapeId);
                if (shape.CollidesWith.HasCategory(category))
                {
                    direction = shape.CollisionDirections[category];
                    return true;
                }

                direction = "";
                return false;
            }

            direction = "";
            return false;
        }

        public void RemoveShape(string shapeId)
        {
            m_Shapes.RemoveAll(s => s.ShapeID == shapeId);

            VOnChanged();
        }

        public void AddBoxShape(string id, Vector2 dimensions, Vector2 anchor,
                                string material, bool isBullet,
                                Cv_CollisionCategories collisionCategories = null,
                                Cv_CollisionCategories collidesWith = null,
                                Dictionary<int, string> collisionDirections = null)
        {
            var shapeData = new Cv_ShapeData();
            shapeData.Dimensions = dimensions;
            shapeData.Type = ShapeType.Box;

            AddShape(shapeData, id, anchor, material, isBullet,
                        collisionCategories, collidesWith, collisionDirections);            
        }

        public void AddCircleShape(string id, float radius, Vector2 anchor,
                                string material, bool isBullet,
                                Cv_CollisionCategories collisionCategories = null,
                                Cv_CollisionCategories collidesWith = null,
                                Dictionary<int, string> collisionDirections = null)
        {
            var shapeData = new Cv_ShapeData();
            shapeData.Radius = radius;
            shapeData.Type = ShapeType.Circle;

            AddShape(shapeData, id, anchor, material, isBullet,
                        collisionCategories, collidesWith, collisionDirections);            
        }

        public void AddTriggerShape(string id, Vector2 dimensions, Vector2 anchor,
                                string material, bool isBullet,
                                Cv_CollisionCategories collisionCategories = null,
                                Cv_CollisionCategories collidesWith = null,
                                Dictionary<int, string> collisionDirections = null)
        {
            var shapeData = new Cv_ShapeData();
            shapeData.Dimensions = dimensions;
            shapeData.Type = ShapeType.Trigger;

            AddShape(shapeData, id, anchor, material, isBullet,
                        collisionCategories, collidesWith, collisionDirections);            
        }

        public void AddPolygonShape(string id, Vector2[] points, Vector2 anchor,
                                string material, bool isBullet,
                                Cv_CollisionCategories collisionCategories = null,
                                Cv_CollisionCategories collidesWith = null,
                                Dictionary<int, string> collisionDirections = null)
        {
            var shapeData = new Cv_ShapeData();
            shapeData.Points = points;
            shapeData.Type = ShapeType.Polygon;

            AddShape(shapeData, id, anchor, material, isBullet,
                        collisionCategories, collidesWith, collisionDirections);            
        }

        public override bool VPostInitialize()
        {
            VOnChanged();
            return true;
        }

        public override void VPostLoad()
        {
        }

        public override void VOnDestroy()
        {
            Cv_Event newEvt = new Cv_Event_DestroyRigidBodyComponent(Owner.ID, this);
            Cv_EventManager.Instance.TriggerEvent(newEvt);
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
        }

        private void AddShape(Cv_ShapeData newShape, string id, Vector2 anchor,
                                string material, bool isBullet,
                                Cv_CollisionCategories collisionCategories = null,
                                Cv_CollisionCategories collidesWith = null,
                                Dictionary<int, string> collisionDirections = null)
        {
            newShape.IsBullet = isBullet;
            newShape.Material = material;
            newShape.ShapeID = id;
            newShape.Anchor = anchor;

            if (collisionCategories == null)
            {
                var categories = new Cv_CollisionCategories();
                categories.AddAllCategories();
                newShape.Categories = categories;
            }
            else
            {
                newShape.Categories = collisionCategories;
            }

            if (collidesWith == null)
            {
                var categories = new Cv_CollisionCategories();
                categories.AddAllCategories();
                newShape.CollidesWith = categories;
            }
            else
            {
                newShape.CollidesWith = collidesWith;
            }

            if (collisionDirections == null)
            {
                var directions = new Dictionary<int, string>();
                
                var _collidesWith = newShape.CollidesWith.GetCategoriesArray();
                foreach (var c in _collidesWith)
                {
                    directions.Add(c, "All");
                }
                newShape.CollisionDirections = directions;
            }
            else
            {
                newShape.CollisionDirections = collisionDirections;
            }

            m_Shapes.Add(newShape);

            var newEvt = new Cv_Event_NewCollisionShape(Owner.ID, newShape, this);
            Cv_EventManager.Instance.QueueEvent(newEvt, true);
        }
    }
}