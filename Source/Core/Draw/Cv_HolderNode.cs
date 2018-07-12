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
        public override float Radius
        {
            get
            {
                if (TransformChanged)
                {
                    Properties.Radius = 0;
                    foreach (var child in m_Children)
                    {
                        var childPos = child.Position;
                        var radius = childPos.Length() + child.Radius;

                        if (radius > Properties.Radius)
                        {
                            Properties.Radius = radius;
                        }
                    }

                    if (Properties.Radius > 0 && m_DebugCircleTex == null)
                    {
                        m_DebugCircleTex = Cv_DrawUtils.CreateCircle((int) Properties.Radius / 2);
                    }
                }

                return Properties.Radius;
            }
        }

        private Texture2D m_DebugCircleTex;

        public Cv_HolderNode(Cv_Entity.Cv_EntityID entityID) : base(entityID, null, new Cv_Transform())
        {
        }

        public override void VPreRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            scene.PushAndSetTransform(Transform);
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

            var scale = camTransform.Scale.X / Math.Max(renderer.Transform.Scale.X,renderer.Transform.Scale.Y);
            return (deltaX * deltaX + deltaY * deltaY) < (Radius*scale*Radius*scale);
        }

        public override void VPostRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            scene.PopTransform();
        }

        public override void VRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            if (renderer.DebugDraw && Radius > 0)
            {
                var pos = scene.Transform.Position;

                Rectangle r2 = new Rectangle((int)(pos.X - Radius), 
                                                (int)(pos.Y - Radius), 
                                                (int)(Radius * 2), 
                                                (int)(Radius * 2));
                renderer.Draw(m_DebugCircleTex, r2, Color.Blue);
            }
        }
    }
}