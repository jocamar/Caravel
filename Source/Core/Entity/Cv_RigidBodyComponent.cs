using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Caravel.Core.Events;
using Caravel.Debugging;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_RigidBodyComponent : Cv_EntityComponent
    {
        public enum BodyType { Static, Dynamic, Kinematic };
        public enum ShapeType { Box, Circle, Polygon, Trigger };

        public struct Cv_ShapeData
        {
            public ShapeType Type;
            public Vector2 Anchor;
            public float Radius;
            public Vector2 Dimensions;
            public Vector2[] Points;
            public bool IsBullet;
            public string Density;
            public string Material;
        }

        public BodyType RigidBodyType
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
        private BodyType m_RigidBodyType;
        private float m_fLinearDamping;
        private float m_fAngularDamping;
        private float m_fGravityScale;
        private bool m_bFixedRotation;
        private string m_sDensity;
        private string m_sMaterial;
        #endregion

        private List<Cv_ShapeData> m_Shapes = new List<Cv_ShapeData>();

        protected internal override bool VInit(XmlElement componentData)
        {
            XmlElement materialNode = (XmlElement) componentData.SelectSingleNode("//Material");
            if (materialNode != null)
            {
                m_sMaterial = materialNode.Attributes["material"].Value;
                m_sDensity = materialNode.Attributes["density"].Value;
            }

            XmlElement physicsNode = (XmlElement) componentData.SelectSingleNode("//Physics");
            if (physicsNode != null)
            {
                m_bFixedRotation = bool.Parse(physicsNode.Attributes["fixedRotation"].Value);
                m_fGravityScale = float.Parse(physicsNode.Attributes["gravityScale"].Value, CultureInfo.InvariantCulture);
                MaxVelocity = float.Parse(physicsNode.Attributes["maxVelocity"].Value, CultureInfo.InvariantCulture);
                MaxAngularVelocity = float.Parse(physicsNode.Attributes["maxAngVelocity"].Value, CultureInfo.InvariantCulture);
            }

            XmlElement bodyNode = (XmlElement) componentData.SelectSingleNode("//Body");
            if (bodyNode != null)
            {
                m_fLinearDamping = float.Parse(bodyNode.Attributes["linearDamping"].Value, CultureInfo.InvariantCulture);
                m_fAngularDamping = float.Parse(bodyNode.Attributes["angularDamping"].Value, CultureInfo.InvariantCulture);
                UseEntityRotation = bool.Parse(bodyNode.Attributes["followEntityRotation"].Value);
                var bodyType = bodyNode.Attributes["type"].Value.ToLowerInvariant();

                switch (bodyType)
                {
                    case "static":
                        m_RigidBodyType = BodyType.Static;
                        break;
                    case "kinematic":
                        m_RigidBodyType = BodyType.Kinematic;
                        break;
                    case "dynamic":
                        m_RigidBodyType = BodyType.Dynamic;
                        break;
                    default:
                        Cv_Debug.Error("Invalid body type. Unable to build component.");
                        return false;
                }
            }

            
            var collisionShapesNode = componentData.SelectSingleNode("//CollisionShapes");
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

                shapeData.Material = shape.Attributes?["material"].Value;
                shapeData.Density = shape.Attributes?["density"].Value;

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
                    foreach (XmlElement point in shape.ChildNodes)
                    {
                        x = int.Parse(shape.Attributes?["x"].Value, CultureInfo.InvariantCulture);
                        y = int.Parse(shape.Attributes?["y"].Value, CultureInfo.InvariantCulture);
                        points.Add(new Vector2((float) x, (float) y));
                    }

                    shapeData.Points = points.ToArray();
                }

                m_Shapes.Add(shapeData);
            }

            IsDirty = true;
            return true;
        }

        protected internal override void VOnChanged()
        {
        }

        protected internal override void VOnUpdate(float deltaTime)
        {
        }

        protected internal override bool VPostInit()
        {
            foreach (var s in m_Shapes)
            {
                var newEvt = new Cv_Event_NewCollisionShape(Owner.ID, s);
                Cv_EventManager.Instance.TriggerEvent(newEvt);
            }

            return true;
        }

        protected internal override void VPostLoad()
        {
        }

        protected internal override XmlElement VToXML()
        {
            throw new System.NotImplementedException();
        }
    }
}