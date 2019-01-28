using System;
using System.Collections.Generic;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Caravel.Core.Entity.Cv_Entity;

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

        internal override void VPreRender(Cv_Renderer renderer)
        {
            CaravelApp.Instance.Scene.PushAndSetTransform(Transform);

            if (Properties.Radius > 0 && m_DebugCircleTex == null && renderer.DebugDrawRadius)
            {
                m_DebugCircleTex = Cv_DrawUtils.CreateCircle((int) Properties.Radius / 2);
            }
        }

        internal override float GetRadius(Cv_Renderer renderer)
        {
            Properties.Radius = 0;
            foreach (var child in Children)
            {
                var childPos = child.Position * new Vector3(WorldTransform.Scale, 1);
                var radius = childPos.Length() + child.GetRadius(renderer);

                if (radius > Properties.Radius)
                {
                    Properties.Radius = radius;
                }
            }

            return Properties.Radius;
        }

        internal override bool VIsVisible(Cv_Renderer renderer)
        {
            return m_Entity.Visible;
        }

        internal override void VPostRender(Cv_Renderer renderer)
        {
            CaravelApp.Instance.Scene.PopTransform();
        }

        internal override void VRender(Cv_Renderer renderer)
        {
            if (renderer.DebugDrawRadius && GetRadius(renderer) > 0 && m_DebugCircleTex != null)
            {
                var pos = CaravelApp.Instance.Scene.Transform.Position;
                var radius = GetRadius(renderer);

                Rectangle r2 = new Rectangle((int)(pos.X - radius), 
                                                (int)(pos.Y - radius), 
                                                (int)(radius * 2), 
                                                (int)(radius * 2));
                renderer.Draw(m_DebugCircleTex, r2, null, Color.Blue, 0, Vector2.Zero, SpriteEffects.None, pos.Z);
            }
        }

        internal override bool VPick(Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities)
        {
            CaravelApp.Instance.Scene.PushAndSetTransform(Transform);
            var rtrnVal = base.VPick(renderer, screenPosition, entities);
            CaravelApp.Instance.Scene.PopTransform();
            return rtrnVal;
        }
    }
}