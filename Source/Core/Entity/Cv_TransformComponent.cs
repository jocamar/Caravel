using System;
using System.Xml;
using Caravel.Debugging;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_TransformComponent : Cv_EntityComponent
    {
        public Cv_Transform Transform
        {
            get; set;
        }

        public Vector3 Position
        {
            get
            {
                return Transform.Position;
            }

            set
            {
                Transform.Position = value;
            }
        }

        public Cv_TransformComponent()
        {
            Transform = new Cv_Transform();
        }

        protected internal override bool VInit(XmlElement componentData)
        {
            Cv_Debug.Assert(componentData != null, "Must have valid component data.");

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

                rad = float.Parse(rotationNode.Attributes["radians"].Value);

                var rotation = rad;
                Transform.Rotation = rotation;
            }

            var scaleNode = componentData.SelectNodes("//Scale").Item(0);
            if (scaleNode != null)
            {
                float x, y;

                x = int.Parse(scaleNode.Attributes["x"].Value);
                y = int.Parse(scaleNode.Attributes["y"].Value);

                var scale = new Vector2(x,y);
                Transform.Scale = scale;
            }

            return true;
        }

        protected internal override bool VPostInit()
        {
            return true;
        }

        protected internal override void VOnChanged()
        {
        }

        protected internal override void VOnUpdate(float deltaTime)
        {
        }

        protected internal override XmlElement VToXML()
        {
            var componentDoc = new XmlDocument();
            var componentData = componentDoc.CreateElement(GetComponentName<Cv_TransformComponent>());
            var position = componentDoc.CreateElement("Position");
            var rotation = componentDoc.CreateElement("Rotation");
            var scale = componentDoc.CreateElement("Scale");

            position.SetAttribute("x", ((int) Position.X).ToString());
            position.SetAttribute("y", ((int) Position.Y).ToString());
            position.SetAttribute("z", ((int) Position.Z).ToString());

            rotation.SetAttribute("radians", Transform.Rotation.ToString());

            scale.SetAttribute("x", ((int) Transform.Scale.X).ToString());
            scale.SetAttribute("y", ((int) Transform.Scale.Y).ToString());

            componentData.AppendChild(position);
            componentData.AppendChild(rotation);
            componentData.AppendChild(scale);

            return componentData;
        }
    }
}