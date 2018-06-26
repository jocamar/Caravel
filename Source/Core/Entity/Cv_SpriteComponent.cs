using System.Xml;
using Caravel.Debugging;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_SpriteComponent : Cv_RenderComponent
    {
        public string Texture
        {
            get; private set;
        }

        public int Width
        {
            get; private set;
        }

        public int Height
        {
            get; private set;
        }

        public Cv_SpriteComponent()
        {
        }

        public Cv_SpriteComponent(string resource, int width, int height, Color color)
        {
            Texture = resource;
            Width = width;
            Height = height;
            Color = color;
        }

        protected internal override bool VInheritedInit(XmlElement componentData)
        {
            Cv_Debug.Assert(componentData != null, "Must have valid component data.");

            var textureNode = componentData.SelectNodes("//Texture").Item(0);
            if (textureNode != null)
            {
                Texture = textureNode.Attributes["resource"].Value;
            }

            var sizeNode = componentData.SelectNodes("//Size").Item(0);
            if (sizeNode != null)
            {
                Width = int.Parse(sizeNode.Attributes["width"].Value);
                Height = int.Parse(sizeNode.Attributes["height"].Value);
            }

            return true;
        }

        protected internal override void VOnUpdate(float deltaTime)
        {
        }

        protected internal override XmlElement VToXML()
        {
            throw new System.NotImplementedException();
        }

        protected override Cv_SceneNode VCreateSceneNode()
        {
            var transformComponent = Owner.GetComponent<Cv_TransformComponent>();

            var transform = new Cv_Transform();
            if(transformComponent != null)
            {
                transform = transformComponent.Transform;
            }

            return new Cv_SpriteNode(Owner.ID, this, transform);
        }

        protected override XmlElement VCreateInheritedElement(XmlElement baseElement)
        {
            throw new System.NotImplementedException();
        }
    }
}