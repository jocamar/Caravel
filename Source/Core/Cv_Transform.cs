using System;
using Microsoft.Xna.Framework;

namespace Caravel.Core
{
    public struct Cv_Transform
    {
        public readonly Vector3 Position;
        public readonly Vector2 Scale;
        public readonly Vector2 Origin;
        public readonly float Rotation;

        public Matrix TransformMatrix
        {
            get
            {
                return  Matrix.CreateScale(Scale.X, Scale.Y, 1) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);
            }
        }

		public static Cv_Transform Multiply(Cv_Transform t1, Cv_Transform t2)
		{
            var newMatrix = t2.TransformMatrix * t1.TransformMatrix;
            var newTransform = Cv_Transform.FromMatrix(newMatrix, t2.Origin);
			return newTransform;
		}

        public static Cv_Transform Inverse(Cv_Transform tf)
        {
            var inverse = Matrix.Invert(tf.TransformMatrix);
            var newTransform = Cv_Transform.FromMatrix(inverse, tf.Origin);
            return newTransform;
        }

        public static Cv_Transform Identity = new Cv_Transform(Vector3.Zero, Vector2.One, 0);

        public Cv_Transform(Vector3 position, Vector2 scale, float rotation, Vector2? origin = null)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            Origin = new Vector2(0.5f, 0.5f);

            if (origin != null)
            {
                Origin = origin.Value;
            }
        }

        public static Cv_Transform FromMatrix(Matrix value, Vector2 origin)
        {
            Vector3 scale;
            Vector3 pos;
            Quaternion rot;
            value.Decompose(out scale, out rot, out pos);
            var position = pos;
            var scale2D = new Vector2(scale.X, scale.Y);

            // See: https://stackoverflow.com/questions/5782658/extracting-yaw-from-a-quaternion
            float mag = (float) Math.Sqrt(rot.W*rot.W + rot.Z*rot.Z);
            rot.W /= mag;
            float ang = 2f * (float) Math.Acos(rot.W);

            if (rot.Z < 0)
            {
                ang = (float)(2*Math.PI) - ang;
            }

            var rotation = ang;

            return new Cv_Transform(position, scale2D, rotation, origin);
        }
    }
}