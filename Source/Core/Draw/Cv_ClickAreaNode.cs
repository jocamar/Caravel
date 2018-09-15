using System;
using System.Collections.Generic;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Draw
{
    public class Cv_ClickAreaNode : Cv_SceneNode
    {
        public Cv_ClickAreaNode(Cv_EntityID entityId, Cv_ClickableComponent component) : base(entityId, component, Cv_Transform.Identity)
        {
        }

        internal override bool VIsVisible(Cv_Renderer renderer)
        {
            return true;
        }

        internal override void VPostRender(Cv_Renderer renderer)
        {
        }

        internal override void VPreRender(Cv_Renderer renderer)
        {
        }

        internal override void VRender(Cv_Renderer renderer)
        {
            if (renderer.DebugDrawClickAreas)
            {
                var clickableComponent = (Cv_ClickableComponent) Component;
                var scene = CaravelApp.Instance.Scene;

                var pos = scene.Transform.Position;
                var rot = scene.Transform.Rotation;
                var scale = scene.Transform.Scale;

                var offsetX = clickableComponent.AnchorPoint.X;
                var offsetY = clickableComponent.AnchorPoint.Y;

                var rotMatrixZ = Matrix.CreateRotationZ(rot);

                Vector2 point1;
                Vector2 point2;
                List<Vector2> points = new List<Vector2>();
                var width = clickableComponent.Width * scale.X;
                var height = clickableComponent.Height * scale.Y;
                points.Add(new Vector2(0, 0));
                points.Add(new Vector2(width, 0));
                points.Add(new Vector2(width, height));
                points.Add(new Vector2(0, height));
                for (int i = 0, j = 1; i < points.Count; i++, j++)
                {
                    if (j >= points.Count)
                    {
                        j = 0;
                    }

                    point1 = new Vector2(points[i].X, points[i].Y);
                    point2 = new Vector2(points[j].X, points[j].Y);

                    point1 -= new Vector2(scene.Transform.Origin.X * width, scene.Transform.Origin.Y * height);
                    point2 -= new Vector2(scene.Transform.Origin.X * width, scene.Transform.Origin.Y * height);
                    point1 -= new Vector2(offsetX * scale.X, offsetY * scale.Y);
                    point2 -= new Vector2(offsetX * scale.X, offsetY * scale.Y);
                    point1 = Vector2.Transform(point1, rotMatrixZ);
                    point2 = Vector2.Transform(point2, rotMatrixZ);
                    point1 += new Vector2(pos.X, pos.Y);
                    point2 += new Vector2(pos.X, pos.Y);

                    var thickness = (int) Math.Round(3 / scene.Camera.Zoom);
                    if (thickness <= 0)
                    {
                        thickness = 1;
                    }

                    Cv_DrawUtils.DrawLine(renderer,
                                            point1,
                                            point2,
                                            thickness,
                                            Cv_Renderer.MaxLayers-1,
                                            Color.White);
                }
            }
        }

        internal override bool VPick(Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities)
        {
            if (Component != null)
            {
                var clickableComp = (Cv_ClickableComponent) Component;
                var camMatrix = renderer.CamMatrix;
                var worldTransform = CaravelApp.Instance.Scene.Transform;
                var pos = new Vector2(worldTransform.Position.X, worldTransform.Position.Y);
                var rot = worldTransform.Rotation;
                var scale = worldTransform.Scale;
                var offsetX = clickableComp.AnchorPoint.X;
                var offsetY = clickableComp.AnchorPoint.Y;
                
                var transformedVertices = new List<Vector2>();
                var point1 = new Vector2(-(((worldTransform.Origin.X * clickableComp.Width) + offsetX) * scale.X),
                                        -(((worldTransform.Origin.Y * clickableComp.Height) + offsetY) * scale.Y));

                var point2 = new Vector2(point1.X + (clickableComp.Width * scale.X),
                                        point1.Y);

                var point3 = new Vector2(point2.X,
                                        point1.Y + (clickableComp.Height * scale.Y));

                var point4 = new Vector2(point1.X,
                                        point3.Y);

                Matrix rotMat = Matrix.CreateRotationZ(rot);
                point1 = Vector2.Transform(point1, rotMat);
                point2 = Vector2.Transform(point2, rotMat);
                point3 = Vector2.Transform(point3, rotMat);
                point4 = Vector2.Transform(point4, rotMat);

                point1 += pos;
                point2 += pos;
                point3 += pos;
                point4 += pos;

                transformedVertices.Add(point1);
                transformedVertices.Add(point2);
                transformedVertices.Add(point3);
                transformedVertices.Add(point4);

                var invertedTransform = Matrix.Invert(camMatrix);
                var worldPoint = Vector2.Transform(screenPosition, invertedTransform);
                if (Cv_DrawUtils.PointInPolygon(worldPoint, transformedVertices))
                {
                    entities.Add(Properties.EntityID);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}