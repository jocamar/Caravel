using System;
using Microsoft.Xna.Framework;

namespace Caravel.Core
{
    public struct Cv_Transform : IEquatable<Cv_Transform>
    {
        public readonly Vector3 Position;
        public readonly Vector2 Scale;
        public readonly Vector2 Origin;
        public readonly float Rotation;

        public Matrix TransformMatrix
        {
            get
            {
                if (!m_bMatrixCalculated) {
                    m_bMatrixCalculated = true;
                    m_TransformMatrix = Matrix.CreateScale(Scale.X, Scale.Y, 1) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);
                }
                
                return m_TransformMatrix;
            }
        }

        private Matrix m_TransformMatrix;
        private bool m_bMatrixCalculated;

		public static Cv_Transform Multiply(Cv_Transform t1, Cv_Transform t2)
		{
            Vector3 transformedPosition = t1.Transform(t2.Position);
            float transformedRotation = t1.Rotation + t2.Rotation;
            Vector2 transformedScale = t1.Scale * t2.Scale;
			return new Cv_Transform(transformedPosition, transformedScale, transformedRotation, t2.Origin);
		}

        public static Cv_Transform Inverse(Cv_Transform tf)
        {
            var inverse = Matrix.Invert(tf.TransformMatrix);
            var newTransform = Cv_Transform.FromMatrix(inverse, tf.Origin);
            return newTransform;
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

        public static Cv_Transform Lerp(Cv_Transform t1, Cv_Transform t2, float amount)
        {
            Vector3 transformedPos = Vector3.Lerp(t1.Position, t2.Position, amount);
            Vector2 transformedScale = Vector2.Lerp(t1.Scale, t1.Scale, amount);
            float transformedRotation = MathHelper.Lerp(t1.Rotation, t2.Rotation, amount);
            Vector2 transformedOrigin = Vector2.Lerp(t1.Origin, t1.Origin, amount);

            return new Cv_Transform(transformedPos, transformedScale, transformedRotation, transformedOrigin);
        }

        public static Cv_Transform Identity = new Cv_Transform(Vector3.Zero, Vector2.One, 0);

        public Cv_Transform(Vector3 position, Vector2 scale, float rotation, Vector2? origin = null)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            Origin = new Vector2(0.5f, 0.5f);
            m_bMatrixCalculated = false;
            m_TransformMatrix = Matrix.Identity;

            if (origin != null)
            {
                Origin = origin.Value;
            }
        }

        public Vector3 Transform(Vector3 vector)
        {
            Vector3 result = Vector3.Transform(vector, Matrix.CreateRotationZ(Rotation));
            result = new Vector3(result.X * Scale.X, result.Y * Scale.Y, result.Z);
            result = new Vector3(result.X + Position.X, result.Y + Position.Y, result.Z);
            return result;
        }

        public bool Equals(Cv_Transform other)
        {
            if (Position == other.Position
                && Scale == other.Scale
                && Origin == other.Origin
                && Rotation == other.Rotation)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Cv_Transform) obj);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Position.GetHashCode();
                hash = hash * 23 + Scale.GetHashCode();
                hash = hash * 23 + Origin.GetHashCode();
                hash = hash * 23 + Rotation.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Cv_Transform first, Cv_Transform second) 
        {
            return first.Equals(second);
        }

        public static bool operator !=(Cv_Transform first, Cv_Transform second) 
        {
            return !(first == second);
        }
    }
}