using System;
using System.Globalization;
using System.Xml;
using Caravel.Core.Events;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Entity
{
    public class Cv_TransformComponent : Cv_EntityComponent
    {
        public Cv_Transform Transform
        {
            get
            {
                return new Cv_Transform(Position, Scale, Rotation, Origin);
            }
            
            set
            {
                Position = value.Position;
                Scale = value.Scale;
                Rotation = value.Rotation;
                Origin = value.Origin;
                
                m_OldTransform = m_Transform;
                m_Transform = value;

                if (Owner != null)
                {
                    var newEvent = new Cv_Event_TransformEntity(Owner.ID, m_OldTransform, m_Transform.Position, m_Transform.Scale, m_Transform.Origin, m_Transform.Rotation, this);
                    Cv_EventManager.Instance.TriggerEvent(newEvent);
                }
            }
        }

        // Even though the scene nodes already keep track of their own transforms and world positions
        // this is needed for other components that might need to do stuff according to world position
        // such as the rigid body components
        public virtual Cv_Transform WorldTransform
        {
            get
            {
                var trans = Transform;

                if (Owner.Parent != Cv_EntityID.INVALID_ENTITY)
                {
                    var parent = CaravelApp.Instance.Logic.GetEntity(Owner.Parent);

                    if (parent.GetComponent<Cv_TransformComponent>() != null)
                    {
                        trans = Cv_Transform.Multiply(parent.GetComponent<Cv_TransformComponent>().WorldTransform, trans);
                    }
                }

                return trans;
            }
        }

        public Vector3 Position
        {
            get; private set;
        }

        // Even though the scene nodes already keep track of their own transforms and world positions
        // this is needed for other components that might need to do stuff according to world position
        // such as the rigid body components
        public virtual Vector3 WorldPosition
        {
            get
            {
                var pos = WorldTransform.Position;

                return pos;
            }
        }

        public Vector2 Scale
        {
            get; private set;
        }

        public float Rotation
        {
            get; private set;
        }

        public Vector2 Origin
        {
            get; private set;
        }

        private Cv_Transform m_Transform;
        private Cv_Transform m_OldTransform;

        public Cv_TransformComponent()
        {
            m_OldTransform = Cv_Transform.Identity;
            Transform = Cv_Transform.Identity;
            Scale = Vector2.One;
            Origin = new Vector2(0.5f, 0.5f);
        }

        public void SetPosition(Vector3 value, object caller = null)
        {
            m_OldTransform = Transform;
            Position = value;
            var newEvent = new Cv_Event_TransformEntity(Owner.ID, m_OldTransform, value, Transform.Scale, Transform.Origin, Transform.Rotation, (caller != null ? caller : this));
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        public void ApplyVelocity(Vector2 velocity, object caller = null)
        {
            m_OldTransform = Transform;
            Position = Position + new Vector3(velocity, 0);
            var newEvent = new Cv_Event_TransformEntity(Owner.ID, m_OldTransform, Position, Transform.Scale, Transform.Origin, Transform.Rotation, (caller != null ? caller : this));
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        public void SetScale(Vector2 value, object caller = null)
        {
            m_OldTransform = Transform;
            Scale = value;
            var newEvent = new Cv_Event_TransformEntity(Owner.ID, m_OldTransform, Transform.Position, value, Transform.Origin, Transform.Rotation, (caller != null ? caller : this));
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        public void ApplyScale(Vector2 scale, object caller = null)
        {
            m_OldTransform = Transform;
            Scale = Scale * scale;
            var newEvent = new Cv_Event_TransformEntity(Owner.ID, m_OldTransform, Transform.Position, Scale, Transform.Origin, Transform.Rotation, (caller != null ? caller : this));
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        public void SetRotation(float value, object caller = null)
        {
            m_OldTransform = Transform;
            Rotation = value;
            var newEvent = new Cv_Event_TransformEntity(Owner.ID, m_OldTransform, Transform.Position, Transform.Scale, Transform.Origin, value, (caller != null ? caller : this));
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        public void ApplyRotation(float rotation, object caller = null)
        {
            m_OldTransform = Transform;
            Rotation += rotation;
            var newEvent = new Cv_Event_TransformEntity(Owner.ID, m_OldTransform, Transform.Position, Transform.Scale, Transform.Origin, Rotation, (caller != null ? caller : this));
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        public void SetOrigin(Vector2 value, object caller = null)
        {
            m_OldTransform = Transform;
            Origin = value;
            var newEvent = new Cv_Event_TransformEntity(Owner.ID, m_OldTransform, Transform.Position, Transform.Scale, value, Transform.Rotation, (caller != null ? caller : this));
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        public override XmlElement VToXML()
        {
            var componentDoc = new XmlDocument();
            var componentData = componentDoc.CreateElement(GetComponentName<Cv_TransformComponent>());
            var position = componentDoc.CreateElement("Position");
            var rotation = componentDoc.CreateElement("Rotation");
            var scale = componentDoc.CreateElement("Scale");
            var origin = componentDoc.CreateElement("Origin");

            position.SetAttribute("x", ((int) Position.X).ToString(CultureInfo.InvariantCulture));
            position.SetAttribute("y", ((int) Position.Y).ToString(CultureInfo.InvariantCulture));
            position.SetAttribute("z", ((int) Position.Z).ToString(CultureInfo.InvariantCulture));

            rotation.SetAttribute("radians", Transform.Rotation.ToString(CultureInfo.InvariantCulture));

            scale.SetAttribute("x", (Transform.Scale.X).ToString(CultureInfo.InvariantCulture));
            scale.SetAttribute("y", (Transform.Scale.Y).ToString(CultureInfo.InvariantCulture));

            origin.SetAttribute("x", Transform.Origin.X.ToString(CultureInfo.InvariantCulture));
            origin.SetAttribute("y", Transform.Origin.Y.ToString(CultureInfo.InvariantCulture));

            componentData.AppendChild(position);
            componentData.AppendChild(rotation);
            componentData.AppendChild(scale);
            componentData.AppendChild(origin);

            return componentData;
        }

        public override bool VInitialize(XmlElement componentData)
        {
            Cv_Debug.Assert(componentData != null, "Must have valid component data.");

            m_OldTransform = Transform;

            var positionNode = componentData.SelectNodes("Position").Item(0);
            if (positionNode != null)
            {
                float x, y, z;

                x = int.Parse(positionNode.Attributes["x"].Value);
                y = int.Parse(positionNode.Attributes["y"].Value);
                z = int.Parse(positionNode.Attributes["z"].Value);

                var position = new Vector3(x,y,z);
                Position = position;
            }

            var rotationNode = componentData.SelectNodes("Rotation").Item(0);
            if (rotationNode != null)
            {
                float rad;

                rad = float.Parse(rotationNode.Attributes["radians"].Value, CultureInfo.InvariantCulture);

                var rotation = rad;
                Rotation = rotation;
            }

            var scaleNode = componentData.SelectNodes("Scale").Item(0);
            if (scaleNode != null)
            {
                float x, y;

                x = float.Parse(scaleNode.Attributes["x"].Value, CultureInfo.InvariantCulture);
                y = float.Parse(scaleNode.Attributes["y"].Value, CultureInfo.InvariantCulture);

                var scale = new Vector2(x,y);
                Scale = scale;
            }

            var originNode = componentData.SelectNodes("Origin").Item(0);
            if (originNode != null)
            {
                float x, y;

                x = (float) double.Parse(originNode.Attributes["x"].Value, CultureInfo.InvariantCulture);
                y = (float) double.Parse(originNode.Attributes["y"].Value, CultureInfo.InvariantCulture);

                x = Math.Max(0, Math.Min(1, x));
                y = Math.Max(0, Math.Min(1, y));
                var origin = new Vector2(x,y);
                Origin = origin;
            }

            return true;
        }

        public override bool VPostInitialize()
        {
            return true;
        }

		public override void VPostLoad()
		{
		}

        public override void VOnChanged()
        {
            var newEvent = new Cv_Event_TransformEntity(Owner.ID, m_OldTransform, Transform.Position, Transform.Scale, Transform.Origin, Transform.Rotation, this);
			Cv_EventManager.Instance.QueueEvent(newEvent, true);
        }

        public override void VOnDestroy()
        {
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
        }
    }
}