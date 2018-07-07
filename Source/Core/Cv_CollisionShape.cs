using System;
using System.Collections;
using System.Collections.Generic;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core
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
        }

        public List<Vector2> Points
        {
            get
            {
                return m_Points;
            }

            set
            {
                IsDirty = true;
                m_Points = value;
            }
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
            get
            {
                return m_AnchorPoint;
            }

            set
            {
                IsDirty = true;
                m_AnchorPoint = value;
            }
        }

        public bool IsSensor
        {
            get
            {
                return m_bIsSensor;
            }

            set
            {
                IsDirty = true;
                m_bIsSensor = value;
            }
        }

        public bool IsBullet
        {
            get
            {
                return m_bIsBullet;
            }

            set
            {
                IsDirty = true;
                m_bIsBullet = value;
            }
        }

        public float Density
        {
            get
            {
                return m_fDensity;
            }

            set
            {
                IsDirty = true;
                m_fDensity = value;
            }
        }

        public float Friction
        {
            get
            {
                return m_fFriction;
            }

            set
            {
                IsDirty = true;
                m_fFriction = value;
            }
        }

        public Cv_CollisionCategories CollisionCategories
        {
            get
            {
                return m_Categories;
            }

            set
            {
                IsDirty = true;
                m_Categories = value;
            }
        }

        public Cv_CollisionCategories CollidesWith
        {
            get
            {
                return m_CollidesWith;
            }

            set
            {
                IsDirty = true;
                m_CollidesWith = value;
            }
        }

        public bool IsCircle { get; private set; }
        public float Radius { get; set; } //TODO make this set the dirty flag and improve code
                                            //TODO add subclasses (RectShape, CircleShape, PolygonShape) to simplify
        public Texture2D CircleOutlineTex { get; private set; }

        internal Cv_Entity Owner { get; set; }
        internal bool IsDirty { get; set; }

        private List<Vector2> m_Points;
        private Vector2 m_AnchorPoint;
        private bool m_bIsSensor;
        private bool m_bIsBullet;
        private float m_fDensity;
        private float m_fFriction;
        private Cv_CollisionCategories m_Categories;
        private Cv_CollisionCategories m_CollidesWith;

        public Cv_CollisionShape(List<Vector2> points, Vector2? anchorPoint = null, float density = 1f, bool isSensor = false, bool isBullet = false)
        {
            Points = points;

            if (anchorPoint == null)
                AnchorPoint = Vector2.Zero;
            else AnchorPoint = (Vector2) anchorPoint;

            IsSensor = isSensor;
            IsBullet = isBullet;
            Density = density;
            Friction = 1f;
            CollisionCategories = new Cv_CollisionCategories();
            CollidesWith = new Cv_CollisionCategories();
            CollisionCategories.AddCategory(1);
            CollidesWith.AddCategory(1);

            Owner = null;
            IsDirty = true;
        }

        public Cv_CollisionShape(Vector2 point, float radius, Vector2? anchorPoint = null, float density = 1f, bool isSensor = false, bool isBullet = false)
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
            CollisionCategories = new Cv_CollisionCategories();
            CollidesWith = new Cv_CollisionCategories();
            CollisionCategories.AddCategory(1);
            CollidesWith.AddCategory(1);
            CircleOutlineTex = Cv_DrawUtils.CreateCircle((int) radius);

            Owner = null;
            IsDirty = true;
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

            foreach (var point in m_Points)
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
    }
}