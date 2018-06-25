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

        public Color Color
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

        protected internal override bool VInit(XmlElement componentData)
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

            var colorNode = componentData.SelectNodes("//Color").Item(0);
            if (colorNode != null)
            {
                int r, g, b;

                r = int.Parse(colorNode.Attributes["r"].Value);
                g = int.Parse(colorNode.Attributes["g"].Value);
                b = int.Parse(colorNode.Attributes["b"].Value);

                Color = new Color(r,g,b);
            }

            return true;
        }

        protected internal override bool VPostInit()
        {
            return true;
        }

        protected internal override void VOnUpdate(float deltaTime)
        {
        }

        protected internal override void VOnChanged()
        {
            throw new System.NotImplementedException();
        }

        protected internal override XmlElement VToXML()
        {
            throw new System.NotImplementedException();
        }
    }
}