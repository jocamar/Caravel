using System;
using Caravel.Core;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core.Draw
{
    public class Cv_HolderNode : Cv_SceneNode
    {
        private Texture2D m_DebugCircleTex;

        public Cv_HolderNode(Cv_Entity.Cv_EntityID entityID) : base(entityID, null, new Cv_Transform())
        {
        }

        public override void VPreRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            scene.PushAndSetTransform(Transform);

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

        public override bool VIsVisible(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            Cv_Transform camTransform = new Cv_Transform();
            camTransform.TransformMatrix = renderer.CamMatrix;
            var worldPos = WorldPosition;

            var fromWorldPos = Vector3.Transform(worldPos, renderer.CamMatrix);

            //See: https://yal.cc/rectangle-circle-intersection-test/
            var nearestX = Math.Max(0, Math.Min(fromWorldPos.X / renderer.Transform.Scale.X, renderer.VirtualWidth));
            var nearestY = Math.Max(0, Math.Min(fromWorldPos.Y / renderer.Transform.Scale.Y, renderer.VirtualHeight));
            
            var deltaX = (fromWorldPos.X / renderer.Transform.Scale.X) - nearestX;
            var deltaY = (fromWorldPos.Y / renderer.Transform.Scale.Y) - nearestY;

            var radius = GetRadius(renderer);
            var scale = camTransform.Scale.X / Math.Max(renderer.Transform.Scale.X,renderer.Transform.Scale.Y);
            return (deltaX * deltaX + deltaY * deltaY) < (radius*scale*radius*scale);
        }

        public override void VPostRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            scene.PopTransform();
        }

        public override void VRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            if (renderer.DebugDrawRadius && GetRadius(renderer) > 0 && m_DebugCircleTex != null)
            {
                var pos = scene.Transform.Position;
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