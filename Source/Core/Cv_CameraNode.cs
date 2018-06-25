using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core
{
    public class Cv_CameraNode : Cv_SceneNode
    {
        public override Vector3 Position
        {
            get { return base.Position; }
            set
            {
                base.Position = value;
                m_bIsViewTransformDirty = true;
            }
        }

        public float Zoom
        {
            get { return Scale.X; }
            set
            {
                if (value < 0.1f)
                {
                    Scale = new Vector2(0.1f, 0.1f);
                }
                else
                {
                    Scale = new Vector2(value, value);
                }

                m_bIsViewTransformDirty = true;
            }
        }

        public override float Rotation
        {
            get
            {
                return base.Rotation;
            }
            set
            {
                base.Rotation = value;
                m_bIsViewTransformDirty = true;
            }
        }

        public bool IsDebugCamera
        {
            get; set;
        }

        private bool m_bIsViewTransformDirty = true;
        private Cv_Transform m_Transform = new Cv_Transform();
        private Cv_Transform m_ResTranslationTransform = new Cv_Transform();

        private int m_iPreviousVirtualWidth = -1;
        private int m_iPreviousVirtualHeight = -1;

        public Cv_CameraNode(string id, int camX = 0, int camY = 0, float camZoom = 1f) : base(Cv_EntityID.INVALID_ENTITY, null, new Cv_Transform())
        {
            Zoom = camZoom;
            Position = new Vector3(camX, camY, 0);
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
                m_bIsViewTransformDirty = true;
            }

            if (m_bIsViewTransformDirty)
            {
                m_ResTranslationTransform.Position = new Vector3(virtualWidth * 0.5f / Scale.X, virtualHeight * 0.5f / Scale.X, 0);
                var camScale = Matrix.CreateScale(Scale.X, Scale.Y, 1);
                var camRot = Matrix.CreateRotationZ(Rotation);
                var camTrans = Matrix.CreateTranslation(-Position / new Vector3(Scale, 1));

                m_Transform.TransformMatrix = camTrans *
                                                camRot *
                                                camScale *
                                                rendererTransform.TransformMatrix *
                                                m_ResTranslationTransform.TransformMatrix;

                m_bIsViewTransformDirty = false;
            }

            m_iPreviousVirtualWidth = virtualWidth;
            m_iPreviousVirtualHeight = virtualHeight;

            return m_Transform;
        }

        public void RecalculateTransformationMatrices()
        {
            m_bIsViewTransformDirty = true;
        }

        public override void VRender(Cv_SceneElement scene)
        {

        }
    }
}