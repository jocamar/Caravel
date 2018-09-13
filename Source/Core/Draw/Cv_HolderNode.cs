using System;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core.Draw
{
    public class Cv_HolderNode : Cv_SceneNode
    {
        private Texture2D m_DebugCircleTex;
        private Cv_Entity m_Entity;
        private bool m_bPreviousVisibility;
        private bool m_bCalculatingVisibilityFirstTime = true;

        public Cv_HolderNode(Cv_Entity.Cv_EntityID entityID) : base(entityID, null, Cv_Transform.Identity)
        {
            m_Entity = CaravelApp.Instance.Logic.GetEntity(entityID);
        }

        public override void VPreRender(Cv_Renderer renderer)
        {
            CaravelApp.Instance.Scene.PushAndSetTransform(Transform);

            if (Properties.Radius > 0 && m_DebugCircleTex == null && renderer.DebugDrawRadius)
            {
                m_DebugCircleTex = Cv_DrawUtils.CreateCircle((int) Properties.Radius / 2);
            }
        }

        public override float GetRadius(Cv_Renderer renderer)
        {
            Properties.Radius = 0;
            foreach (var child in m_Children)
            {
                var childPos = child.Position;
                var radius = childPos.Length() + child.GetRadius(renderer);

                if (radius > Properties.Radius)
                {
                    Properties.Radius = radius;
                }
            }

            return Properties.Radius;
        }

        public override bool VIsVisible(Cv_Renderer renderer)
        {
            if (!m_Entity.Visible)
            {
                if (!m_bCalculatingVisibilityFirstTime && m_bPreviousVisibility == true)
                {
                    var visibilityChangedEvt = new Cv_Event_EntityVisibilityChanged(Properties.EntityID, false, this);
                    Cv_EventManager.Instance.QueueEvent(visibilityChangedEvt);
                }

                m_bPreviousVisibility = false;
                return false;
            }

            Cv_Transform camTransform = Cv_Transform.FromMatrix(renderer.CamMatrix, new Vector2(0.5f, 0.5f));
            var worldPos = WorldPosition;

            var fromWorldPos = Vector3.Transform(worldPos, renderer.CamMatrix);

            //See: https://yal.cc/rectangle-circle-intersection-test/
            var nearestX = Math.Max(0, Math.Min(fromWorldPos.X / renderer.Transform.Scale.X, renderer.VirtualWidth));
            var nearestY = Math.Max(0, Math.Min(fromWorldPos.Y / renderer.Transform.Scale.Y, renderer.VirtualHeight));
            
            var deltaX = (fromWorldPos.X / renderer.Transform.Scale.X) - nearestX;
            var deltaY = (fromWorldPos.Y / renderer.Transform.Scale.Y) - nearestY;

            var radius = GetRadius(renderer);
            var scale = camTransform.Scale.X / Math.Max(renderer.Transform.Scale.X,renderer.Transform.Scale.Y);
            var visibility = (deltaX * deltaX + deltaY * deltaY) < (radius*scale*radius*scale);

            if (!m_bCalculatingVisibilityFirstTime && m_bPreviousVisibility != visibility)
            {
                var visibilityChangedEvt = new Cv_Event_EntityVisibilityChanged(Properties.EntityID, visibility, this);
                Cv_EventManager.Instance.QueueEvent(visibilityChangedEvt);
            }

            m_bPreviousVisibility = visibility;
            return visibility;
        }

        public override void VPostRender(Cv_Renderer renderer)
        {
            CaravelApp.Instance.Scene.PopTransform();
        }

        public override void VRender(Cv_Renderer renderer)
        {
            if (renderer.DebugDrawRadius && GetRadius(renderer) > 0 && m_DebugCircleTex != null)
            {
                var pos = CaravelApp.Instance.Scene.Transform.Position;
                var radius = GetRadius(renderer);

                Rectangle r2 = new Rectangle((int)(pos.X - radius), 
                                                (int)(pos.Y - radius), 
                                                (int)(radius * 2), 
                                                (int)(radius * 2));
                renderer.Draw(m_DebugCircleTex, r2, Color.Blue);
            }
        }
    }
}