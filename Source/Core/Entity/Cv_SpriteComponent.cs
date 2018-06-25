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

        public Cv_SpriteComponent(string resource, int width, int height, Color color)
        {
            Texture = resource;
            Width = width;
            Height = height;
            Color = color;
        }
    }
}