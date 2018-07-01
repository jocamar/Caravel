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

        public Vector2 Origin
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
                return  Matrix.CreateScale(Scale.X, Scale.Y, 1) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateTranslation(Position.X, Position.Y, 0);
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
                float mag = (float) Math.Sqrt(rot.W*rot.W + rot.Z*rot.Z);
                rot.W /= mag;
                float ang = 2f * (float) Math.Acos(rot.W);

				if (rot.Z < 0)
				{
					ang = (float)(2*Math.PI) - ang;
				}

                Rotation = ang;
            }
        }

		public static Cv_Transform Multiply(Cv_Transform t1, Cv_Transform t2)
		{
            var newMatrix = t2.TransformMatrix * t1.TransformMatrix;
            var newTransform = new Cv_Transform();
            newTransform.TransformMatrix = newMatrix;
            newTransform.Origin = t2.Origin;
			return newTransform;
		}

        public static Cv_Transform Inverse(Cv_Transform tf)
        {
            var inverse = Matrix.Invert(tf.TransformMatrix);
            var newTransform = new Cv_Transform();
            newTransform.TransformMatrix = inverse;
            newTransform.Origin = tf.Origin;
            return newTransform;
        }

        public Cv_Transform()
        {
            Position = Vector3.Zero;
            Scale = Vector2.One;
            Rotation = 0;
            Origin = new Vector2(0.5f, 0.5f);
        }

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
    }
}