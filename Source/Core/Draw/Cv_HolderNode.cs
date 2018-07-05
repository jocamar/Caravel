using System;
using Caravel.Core;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;

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
                    foreach (var child in m_Children)
                    {
                        var childPos = child.Position;
                        var radius = childPos.Length() + child.Radius;

                        if (radius > Properties.Radius)
                        {
                            Properties.Radius = radius;
                        }
                    }
                }

                return Properties.Radius;
            }
        }

        public Cv_HolderNode(Cv_Entity.Cv_EntityID entityID) : base(entityID, null, new Cv_Transform())
        {
        }

        public override void VPreRender(Cv_SceneElement scene)
        {
            scene.PushAndSetTransform(Transform);
        }

        public override bool VIsVisible(Cv_SceneElement scene)
        {
            Cv_Transform camTransform = new Cv_Transform();
            camTransform.TransformMatrix = scene.Renderer.CamMatrix;
            var worldPos = WorldPosition;

            var fromWorldPos = Vector3.Transform(worldPos, scene.Renderer.CamMatrix);

            //See: https://yal.cc/rectangle-circle-intersection-test/
            var nearestX = Math.Max(0, Math.Min(fromWorldPos.X / scene.Renderer.Transform.Scale.X, scene.Renderer.VirtualWidth));
            var nearestY = Math.Max(0, Math.Min(fromWorldPos.Y / scene.Renderer.Transform.Scale.Y, scene.Renderer.VirtualHeight));
            
            var deltaX = (fromWorldPos.X / scene.Renderer.Transform.Scale.X) - nearestX;
            var deltaY = (fromWorldPos.Y / scene.Renderer.Transform.Scale.Y) - nearestY;
            return (deltaX * deltaX + deltaY * deltaY) < (Radius*camTransform.Scale.X * Radius*camTransform.Scale.X);
        }

        public override void VPostRender(Cv_SceneElement scene)
        {
            base.VPostRender(scene);
            scene.PopTransform();
        }

        public override void VRender(Cv_SceneElement scene)
        {
        }
    }
}