using System;
using System.Collections;
using System.Collections.Generic;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core.Physics
{
    public class Cv_CollisionShape
    {
        public class Cv_CollisionCategories
        {
            private BitArray m_Categories = new BitArray(32);

            public void AddCategory(int category)
            {
                if (category >= 32)
                {
                    Cv_Debug.Error("Invalid category. There are only 32 collision categories.");
                    return;
                }

                m_Categories.Set(category, true);
            }

            public void RemoveCategory(int category)
            {
                if (category >= 32)
                {
                    Cv_Debug.Error("Invalid category. There are only 32 collision categories.");
                    return;
                }

                m_Categories.Set(category, false);
            }

            public int GetCategories()
            {
                int[] array = new int[1];
                m_Categories.CopyTo(array, 0);
                return array[0];
            }

			public int[] GetCategoriesArray()
            {
				List<int> categories = new List<int>();
                for(var i = 0; i < m_Categories.Length; i++)
				{
					if (m_Categories.Get(i))
					{
						categories.Add(i);
					}
				}

				return categories.ToArray();
            }

			public bool HasCategory(int category)
			{
				return m_Categories.Get(category);
			}
        }

        public List<Vector2> Points
        {
            get; set;
        }

        public struct ShapeBoundingBox
        {
			public Vector2 Start { get; internal set; }
            public float Width { get; internal set; }
            public float Height { get; internal set; }
        }
        
        public ShapeBoundingBox AABoundingBox {
            get {
                return CalculateAABoundingBox();
            }
        }

		public Vector2 AnchorPoint
        {
            get; set;
        }

        public bool IsSensor
        {
            get; set;
        }

        public bool IsBullet
        {
            get; set;
        }

        public float Density
        {
            get; set;
        }

        public float Friction
        {
            get; set;
        }

        public float Restitution
        {
            get; set;
        }

        public Cv_CollisionCategories CollisionCategories
        {
            get; set;
        }

        public Cv_CollisionCategories CollidesWith
        {
            get; set;
        }

        public bool IsCircle { get; private set; }
        public float Radius { get; set; } //TODO(JM): add subclasses (RectShape, CircleShape, PolygonShape) to simplify
        public Texture2D CircleOutlineTex { get; private set; }

        internal Cv_Entity Owner { get; set; }

		private Dictionary<int, string> m_CollisionDirections;

        public Cv_CollisionShape(List<Vector2> points, Vector2? anchorPoint, float density, bool isSensor,
									bool isBullet, Cv_CollisionCategories categories, Cv_CollisionCategories collidesWith,
									Dictionary<int, string> directions)
        {
            Points = points;

            if (anchorPoint == null)
                AnchorPoint = Vector2.Zero;
            else AnchorPoint = (Vector2) anchorPoint;

            IsSensor = isSensor;
            IsBullet = isBullet;
            Density = density;
            Friction = 1f;
            CollisionCategories = categories;
            CollidesWith = collidesWith;
			m_CollisionDirections = directions;

            Owner = null;
        }

        public Cv_CollisionShape(Vector2 point, float radius, Vector2? anchorPoint, float density, bool isSensor,
									bool isBullet, Cv_CollisionCategories categories, Cv_CollisionCategories collidesWith,
									Dictionary<int, string> directions)
        {
            Points = new List<Vector2>();
            Points.Add(point);

            if (anchorPoint == null)
                AnchorPoint = Vector2.Zero;
            else AnchorPoint = (Vector2)anchorPoint;

            Radius = radius;
            IsCircle = true;
            IsSensor = isSensor;
            IsBullet = isBullet;
            Density = density;
            Friction = 1f;
            CollisionCategories = categories;
            CollidesWith = collidesWith;
			m_CollisionDirections = directions;
            CircleOutlineTex = Cv_DrawUtils.CreateCircle((int) radius);

            Owner = null;
        }

        private ShapeBoundingBox CalculateAABoundingBox()
        {
            if(Owner == null)
            {
                Cv_Debug.Error("Shape not yet associated with an Entity.");
            }

            var rotation = Owner.GetComponent<Cv_TransformComponent>().Rotation;

            var offsetX = AnchorPoint.X;
            var offsetY = AnchorPoint.Y;

            var rotMatrixZ = Matrix.CreateRotationZ(rotation);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var point in Points)
            {
                var transformedPoint = new Vector2(point.X - offsetX, point.Y - offsetY);
                transformedPoint = Vector2.Transform(transformedPoint, rotMatrixZ);

                if (transformedPoint.X < minX)
                {
                    minX = transformedPoint.X;
                }
                if (transformedPoint.Y < minY)
                {
                    minY = transformedPoint.Y;
                }
                if (transformedPoint.X > maxX)
                {
                    maxX = transformedPoint.X;
                }
                if (transformedPoint.Y > maxY)
                {
                    maxY = transformedPoint.Y;
                }
            }

            var BoundingBox = new ShapeBoundingBox();
            BoundingBox.Start = new Vector2(minX, minY);
            BoundingBox.Width = maxX - minX;
            BoundingBox.Height = maxY - minY;

            return BoundingBox;
        }

		public bool CollidesWithFromDirection(Cv_CollisionCategories categories, string direction)
		{
			var catArray = categories.GetCategoriesArray();
			var collides = false;

			foreach (var c in catArray)
			{
				if (m_CollisionDirections.ContainsKey(c)
						&& (m_CollisionDirections[c].Contains(direction) || m_CollisionDirections[c] == "All"))
				{
					collides = true;
                    break;
				}
			}

			return collides;
		}
    }
}