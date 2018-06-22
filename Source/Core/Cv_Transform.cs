using Microsoft.Xna.Framework;

namespace Caravel.Core
{
    public class Cv_Transform
    {
        public Vector3 Position
        {
            get; set;
        }

        public Vector2 Scale
        {
            get; set;
        }

        public float Rotation
        {
            get; set;
        }

        public Matrix TransformMatrix
        {
            get
            {
                return Matrix.CreateTranslation(Position.X, Position.Y, 0) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateScale(Scale.X, Scale.Y, 1);
            }
        }

		public static Cv_Transform Multiply(Cv_Transform t1, Cv_Transform t2)
		{
			return null;
		}

        public Cv_Transform()
        {
            Position = Vector3.Zero;
            Scale = Vector2.One;
            Rotation = 0;
        }

        public Cv_Transform(Vector3 position, Vector2 scale, float rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }
    }
}