using System;
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

            set
            {
                Vector3 scale;
                Vector3 pos;
                Quaternion rot;
                value.Decompose(out scale, out rot, out pos);
                Position = pos;
                Scale = new Vector2(scale.X, scale.Y);

                // See: https://stackoverflow.com/questions/5782658/extracting-yaw-from-a-quaternion
                rot.X = 0;
                rot.Y = 0;
                float mag = (float) Math.Sqrt(rot.W*rot.W + rot.Z*rot.Z);
                rot.W /= mag;
                rot.Z /= mag;
                float ang = 2f * (float) Math.Acos(rot.W);

                Rotation = ang;
            }
        }

		public static Cv_Transform Multiply(Cv_Transform t1, Cv_Transform t2)
		{
            var newMatrix = t1.TransformMatrix * t2.TransformMatrix;
            var newTransform = new Cv_Transform();
            newTransform.TransformMatrix = newMatrix;
			return newTransform;
		}

        public static Cv_Transform Inverse(Cv_Transform tf)
        {
            var inverse = Matrix.Invert(tf.TransformMatrix);
            var newTransform = new Cv_Transform();
            newTransform.TransformMatrix = inverse;
            return newTransform;
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