using System;
using System.Globalization;
using System.Xml;
using Caravel.Core.Events;
using Caravel.Debugging;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_TransformComponent : Cv_EntityComponent
    {
        public Cv_Transform Transform
        {
            get
            {
                return m_Transform;
            }
            
            set
            {
                if (value != m_Transform)
                {
                    m_Transform = value;
                }
            }
        }

        public Vector3 Position
        {
            get
            {
                return Transform.Position;
            }

            set
            {
				var newEvent = new Cv_Event_TransformEntity(Owner.ID, Transform, value, Transform.Scale, Transform.Origin, Transform.Rotation);
				Cv_EventManager.Instance.TriggerEvent(newEvent);
            }
        }

        public Vector2 Scale
        {
            get
            {
                return Transform.Scale;
            }

            set
            {
				var newEvent = new Cv_Event_TransformEntity(Owner.ID, Transform, Transform.Position, value, Transform.Origin, Transform.Rotation);
				Cv_EventManager.Instance.TriggerEvent(newEvent);
            }
        }

        public float Rotation
        {
            get
            {
                return Transform.Rotation;
            }

            set
            {
				var newEvent = new Cv_Event_TransformEntity(Owner.ID, Transform, Transform.Position, Transform.Scale, Transform.Origin, value);
				Cv_EventManager.Instance.TriggerEvent(newEvent);
            }
        }

        public Vector2 Origin
        {
            get
            {
                return Transform.Origin;
            }

            set
            {
				var newEvent = new Cv_Event_TransformEntity(Owner.ID, Transform, Transform.Position, Transform.Scale, value, Transform.Rotation);
				Cv_EventManager.Instance.TriggerEvent(newEvent);
            }
        }

        private Cv_Transform m_Transform;
        private Cv_Transform m_OldTransform;

        public Cv_TransformComponent()
        {
            Transform = new Cv_Transform();
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

        protected internal override bool VInit(XmlElement componentData)
        {
            Cv_Debug.Assert(componentData != null, "Must have valid component data.");

            m_OldTransform = new Cv_Transform(Transform);

            var positionNode = componentData.SelectNodes("//Position").Item(0);
            if (positionNode != null)
            {
                float x, y, z;

                x = int.Parse(positionNode.Attributes["x"].Value);
                y = int.Parse(positionNode.Attributes["y"].Value);
                z = int.Parse(positionNode.Attributes["z"].Value);

                var position = new Vector3(x,y,z);
                Transform.Position = position;
            }

            var rotationNode = componentData.SelectNodes("//Rotation").Item(0);
            if (rotationNode != null)
            {
                float rad;

                rad = float.Parse(rotationNode.Attributes["radians"].Value, CultureInfo.InvariantCulture);

                var rotation = rad;
                Transform.Rotation = rotation;
            }

            var scaleNode = componentData.SelectNodes("//Scale").Item(0);
            if (scaleNode != null)
            {
                float x, y;

                x = float.Parse(scaleNode.Attributes["x"].Value, CultureInfo.InvariantCulture);
                y = float.Parse(scaleNode.Attributes["y"].Value, CultureInfo.InvariantCulture);

                var scale = new Vector2(x,y);
                Transform.Scale = scale;
            }

            var originNode = componentData.SelectNodes("//Origin").Item(0);
            if (originNode != null)
            {
                float x, y;

                x = (float) double.Parse(originNode.Attributes["x"].Value, CultureInfo.InvariantCulture);
                y = (float) double.Parse(originNode.Attributes["y"].Value, CultureInfo.InvariantCulture);

                var origin = new Vector2(x,y);
                Transform.Origin = origin;
            }

            return true;
        }

        protected internal override bool VPostInit()
        {
			var newEvent = new Cv_Event_TransformEntity(Owner.ID, null, Transform.Position, Transform.Scale, Transform.Origin, Transform.Rotation);
			Cv_EventManager.Instance.QueueEvent(newEvent);
			Cv_EventManager.Instance.AddListener<Cv_Event_TransformEntity>(OnTransformEntity);
            return true;
        }

		protected internal override void VPostLoad()
		{
		}

        protected internal override void VOnChanged()
        {
            var newEvent = new Cv_Event_TransformEntity(Owner.ID, m_OldTransform, Transform.Position, Transform.Scale, Transform.Origin, Transform.Rotation);
			Cv_EventManager.Instance.QueueEvent(newEvent);
        }

        protected internal override void VOnDestroy()
        {
            Cv_EventManager.Instance.RemoveListener<Cv_Event_TransformEntity>(OnTransformEntity);
        }

        protected internal override void VOnUpdate(float deltaTime)
        {
        }

		private void OnTransformEntity(Cv_Event transformEvent)
		{
			if (transformEvent.EntityID == Owner.ID)
			{
				var tEvent = (Cv_Event_TransformEntity) transformEvent;

				Transform.Position = tEvent.NewPosition;
				Transform.Scale = tEvent.NewScale;
				Transform.Rotation = tEvent.NewRotation;
				Transform.Origin = tEvent.NewOrigin;
			}
		}
    }
}