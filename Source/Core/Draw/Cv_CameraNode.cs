using System;
using System.Collections.Generic;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Draw
{
    public class Cv_CameraNode : Cv_SceneNode
    {
        public float Zoom
        {
            get { return ((Cv_CameraComponent) m_Component).Zoom; }
        }

        public bool IsDebugCamera
        {
            get; set;
        }

        public bool IsViewTransformDirty
        {
            get; internal set;
        }

        private Cv_Transform m_Transform = new Cv_Transform();
        private Cv_Transform m_ResTranslationTransform = new Cv_Transform();

        private int m_iPreviousVirtualWidth = -1;
        private int m_iPreviousVirtualHeight = -1;

        public Cv_CameraNode(Cv_EntityID entityId, Cv_CameraComponent component) : base(entityId, component, new Cv_Transform())
        {
            IsDebugCamera = false;
        }

        public void Move(Vector2 amount)
        {
            Position += new Vector3(amount, 0);
        }

        public override float GetRadius(Cv_Renderer renderer)
        {
            if (Properties.Radius < 0 || ((Cv_CameraComponent) m_Component).ZoomChanged)
            {
                var transf = Parent.Transform;
                var originFactorX = Math.Abs(transf.Origin.X - 0.5) + 0.5;
                var originFactorY = Math.Abs(transf.Origin.Y - 0.5) + 0.5;
                var originFactor = (float) Math.Max(originFactorX, originFactorY);

                var zoom = Zoom;
                var width = renderer.VirtualWidth / zoom;
                var height = renderer.VirtualHeight / zoom;
                Properties.Radius = (float) Math.Sqrt(width*width + height*height) * originFactor;
                Properties.Radius *= Math.Max(transf.Scale.X, transf.Scale.Y);
            }

            return Properties.Radius;
        }

        public Cv_Transform GetViewTransform(int virtualWidth, int virtualHeight, Cv_Transform rendererTransform)
        {
            if ((virtualWidth > 0 && virtualWidth != m_iPreviousVirtualWidth)
                || (virtualHeight > 0 && virtualHeight != m_iPreviousVirtualHeight))
            {
                IsViewTransformDirty = true;
            }

            if (WorldTransformChanged || ((Cv_CameraComponent) m_Component).ZoomChanged)
            {
                IsViewTransformDirty = true;
            }

            if (IsViewTransformDirty)
            {
                var zoom = ((Cv_CameraComponent) m_Component).Zoom;
                var worldTransform = WorldTransform;
                var transform = Parent.Transform;
                var parentOrigin = m_Component.Owner.GetComponent<Cv_TransformComponent>().Origin;
                var parentWorldTranform = new Cv_Transform();

                if (Parent.Parent != null)
                {
                    parentWorldTranform = Parent.Parent.WorldTransform;
                }

                m_ResTranslationTransform.Position = new Vector3(virtualWidth * parentOrigin.X / zoom, virtualHeight * parentOrigin.Y / zoom, 0);
                var camScale = Matrix.CreateScale(zoom, zoom, 1);
                var camRot = Matrix.CreateRotationZ(-worldTransform.Rotation);
                var camTrans = Matrix.CreateTranslation( -transform.Position);
                var parentTrans = Matrix.CreateTranslation(-parentWorldTranform.Position / new Vector3(rendererTransform.Scale, 1));

                m_Transform.TransformMatrix = camScale *
                                                parentTrans *
                                                rendererTransform.TransformMatrix *
                                                camRot *
                                                camTrans *
                                                m_ResTranslationTransform.TransformMatrix;
            }

            m_iPreviousVirtualWidth = virtualWidth;
            m_iPreviousVirtualHeight = virtualHeight;
            

            return m_Transform;
        }

        public void RecalculateTransformationMatrices()
        {
            IsViewTransformDirty = true;
        }

        public override void VRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            if (CaravelApp.Instance.EditorRunning && renderer.DebugDrawCameras)
            {
                var zoom = ((Cv_CameraComponent) m_Component).Zoom;
                var rot = scene.Transform.Rotation;
                var pos = scene.Transform.Position;
                var rotMatrixZ = Matrix.CreateRotationZ(rot);

                Vector2 point1;
                Vector2 point2;
                List<Vector2> points = new List<Vector2>();
                points.Add(new Vector2(0, 0));
                points.Add(new Vector2((renderer.VirtualWidth / zoom), 0));
                points.Add(new Vector2((renderer.VirtualWidth / zoom), (renderer.VirtualHeight / zoom)));
                points.Add(new Vector2(0, (renderer.VirtualHeight / zoom)));
                for (int i = 0, j = 1; i < points.Count; i++, j++)
				{
					if (j >= points.Count)
                    {
						j = 0;
                    }

                    point1 = new Vector2(points[i].X, points[i].Y);
                    point2 = new Vector2(points[j].X, points[j].Y);
                    point1 -= new Vector2((renderer.VirtualWidth / zoom) * 0.5f, (renderer.VirtualHeight / zoom) * 0.5f);
                    point2 -= new Vector2((renderer.VirtualWidth / zoom) * 0.5f, (renderer.VirtualHeight / zoom) * 0.5f);
                    point1 = Vector2.Transform(point1, rotMatrixZ);
                    point2 = Vector2.Transform(point2, rotMatrixZ);
                    point1 += new Vector2(pos.X, pos.Y);
                    point2 += new Vector2(pos.X, pos.Y);

                    Cv_DrawUtils.DrawLine(renderer,
						                                point1,
                                                        point2,
						                                2,
                                                        254,
						                                Color.Purple);
                }

                if (scene.EditorSelectedEntity == Properties.EntityID)
                {
                    for (int i = 0, j = 1; i < points.Count; i++, j++)
                    {
                        if (j >= points.Count)
                        {
                            j = 0;
                        }

                        point1 = new Vector2(points[i].X, points[i].Y);
                        point2 = new Vector2(points[j].X, points[j].Y);
                        point1 = Vector2.Transform(point1, rotMatrixZ);
                        point2 = Vector2.Transform(point2, rotMatrixZ);
                        point1 += new Vector2(pos.X, pos.Y);
                        point2 += new Vector2(pos.X, pos.Y);
                        point1 -= new Vector2((renderer.VirtualWidth / zoom) * 0.5f, (renderer.VirtualHeight / zoom) * 0.5f);
                        point2 -= new Vector2((renderer.VirtualWidth / zoom) * 0.5f, (renderer.VirtualHeight / zoom) * 0.5f);

                        Cv_DrawUtils.DrawLine(renderer,
                                                            point1,
                                                            point2,
                                                            2,
                                                            255,
                                                            Color.Yellow);
                    }
                }
            }
        }

        public override void VPreRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
        }

        public override void VPostRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
        }

        public override void VFinishedRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            base.VFinishedRender(scene, renderer);
            ((Cv_CameraComponent) m_Component).ZoomChanged = false;
        }

        public override bool VIsVisible(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            return true;
        }
    }
}