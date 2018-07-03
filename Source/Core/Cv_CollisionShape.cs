using System;
using Caravel.Core.Entity;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core
{
    public class Cv_CollisionShape
    {
        public Vertices Points
        {
            get
            {
                return points;
            }

            set
            {
                IsDirty = true;
                points = value;
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
                return anchorPoint;
            }

            set
            {
                IsDirty = true;
                anchorPoint = value;
            }
        }

        public bool IsSensor
        {
            get
            {
                return isSensor;
            }

            set
            {
                IsDirty = true;
                isSensor = value;
            }
        }

        public bool IsBullet
        {
            get
            {
                return isBullet;
            }

            set
            {
                IsDirty = true;
                isBullet = value;
            }
        }

        public float Density
        {
            get
            {
                return density;
            }

            set
            {
                IsDirty = true;
                density = value;
            }
        }

        public float Friction
        {
            get
            {
                return friction;
            }

            set
            {
                IsDirty = true;
                friction = value;
            }
        }

        public Category CollisionCategories
        {
            get
            {
                return categories;
            }

            set
            {
                IsDirty = true;
                categories = value;
            }
        }

        public Category CollidesWith
        {
            get
            {
                return collidesWith;
            }

            set
            {
                IsDirty = true;
                collidesWith = value;
            }
        }

        public bool IsCircle { get; private set; }
        public float Radius { get; set; } //TODO make this set the dirty flag and improve code
                                            //TODO add subclasses (RectShape, CircleShape, PolygonShape) to simplify
        public Texture2D CircleOutlineTex { get; private set; }

        internal Cv_Entity Owner { get; set; }
        internal bool IsDirty { get; set; }

        private Vertices points;
        private Vector2 anchorPoint;
        private bool isSensor;
        private bool isBullet;
        private float density;
        private float friction;
        private Category categories;
        private Category collidesWith;

        public Cv_CollisionShape(Vertices points, Vector2? anchorPoint = null, float density = 1f, bool isSensor = false, bool isBullet = false)
        {
            bool error = points.CheckPolygon();
            if (error)
            {
                //TODO(JM) add checks
            }

            Points = points;

            if (anchorPoint == null)
                AnchorPoint = Vector2.Zero;
            else AnchorPoint = (Vector2) anchorPoint;

            IsSensor = isSensor;
            IsBullet = isBullet;
            Density = density;
            Friction = 1f;
            CollisionCategories = Category.Cat1;
            CollidesWith = Category.Cat1;

            Owner = null;
            IsDirty = true;
        }

        public Cv_CollisionShape(Vector2 point, float radius, Vector2? anchorPoint = null, float density = 1f, bool isSensor = false, bool isBullet = false)
        {
            Points = new Vertices();
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
            CollisionCategories = Category.Cat1;
            CollidesWith = Category.Cat1;
            //CircleOutlineTex = DrawUtils.CreateCircle((int) radius);

            Owner = null;
            IsDirty = true;
        }

        private ShapeBoundingBox CalculateAABoundingBox()
        {
            if(Owner == null)
            {
                throw new Exception("Shape is not yet associated with an Entity.");
            }

            var rotation = Owner.GetComponent<Cv_TransformComponent>().Rotation;

            var offsetX = AnchorPoint.X;
            var offsetY = AnchorPoint.Y;

            var rotMatrixZ = Matrix.CreateRotationZ(rotation);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var point in points)
            {
                var transformedPoint = new Vector2(point.X - offsetX, point.Y - offsetY);
                transformedPoint = Vector2.Transform(transformedPoint, rotMatrixZ);

                if (transformedPoint.X < minX)
                    minX = transformedPoint.X;
                if (transformedPoint.Y < minY)
                    minY = transformedPoint.Y;
                if (transformedPoint.X > maxX)
                    maxX = transformedPoint.X;
                if (transformedPoint.Y > maxY)
                    maxY = transformedPoint.Y;
            }

            var BoundingBox = new ShapeBoundingBox();
            BoundingBox.Start = new Vector2(minX, minY);
            BoundingBox.Width = maxX - minX;
            BoundingBox.Height = maxY - minY;

            return BoundingBox;
        }
    }
}