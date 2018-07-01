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
                var camTrans = Matrix.CreateTranslation( -transform.Position/* * new Vector3(zoom, zoom, 1)*/);
                var parentTrans = Matrix.CreateTranslation(-parentWorldTranform.Position / new Vector3(rendererTransform.Scale, 1));

                m_Transform.TransformMatrix = camScale *
                                                parentTrans *
                                                camRot *
                                                camTrans *
                                                rendererTransform.TransformMatrix *
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

        public override void VRender(Cv_SceneElement scene)
        {
        }

        public override void VPreRender(Cv_SceneElement scene)
        {
        }

        public override void VPostRender(Cv_SceneElement scene)
        {
            base.VPostRender(scene);
            ((Cv_CameraComponent) m_Component).ZoomChanged = false;
        }

        public override bool VIsVisible(Cv_SceneElement scene)
        {
            return true;
        }
    }
}