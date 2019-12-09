using System;
using Microsoft.Xna.Framework;

namespace Caravel.Core {
    public class Cv_Math
    {
        public static float PI = (float) Math.PI;

        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public static Vector2 Lerp(Vector2 firstVector, Vector2 secondVector, float by)
        {
            float retX = Lerp(firstVector.X, secondVector.X, by);
            float retY = Lerp(firstVector.Y, secondVector.Y, by);
            return new Vector2(retX, retY);
        }

        public static Vector3 Lerp(Vector3 firstVector, Vector3 secondVector, float by)
        {
            float retX = Lerp(firstVector.X, secondVector.X, by);
            float retY = Lerp(firstVector.Y, secondVector.Y, by);
            float retZ = Lerp(firstVector.Z, secondVector.Z, by);
            return new Vector3(retX, retY, retZ);
        }

        public static float Deg2Rad(float deg)
        {
            return deg * PI / 180;
        }

        public static float Rad2Deg(float rad)
        {
            return rad * 180 / PI;
        }

        public static Vector2 RotateVector(Vector2 vec, float angle)
        {
            var sin = (float) Math.Sin(angle);
            var cos = (float) Math.Cos(angle);
            
            float tx = vec.X;
            float ty = vec.Y;

            var rotated = new Vector2((cos * tx) - (sin * ty), (sin * tx) + (cos * ty));
            return rotated;
        }
    }
}