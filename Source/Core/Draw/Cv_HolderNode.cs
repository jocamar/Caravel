using System;
using Caravel.Core;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Draw
{
    public class Cv_HolderNode : Cv_SceneNode
    {
        public Cv_HolderNode(Cv_Entity.Cv_EntityID entityID) : base(entityID, null, new Cv_Transform())
        {
        }

        public override void VPreRender(Cv_SceneElement scene)
        {
            Cv_Entity entity = CaravelApp.Instance.GameLogic.GetEntity(Properties.EntityID);

            if (entity != null)
            {
                Cv_TransformComponent tranformComponent = entity.GetComponent<Cv_TransformComponent>();

                if (tranformComponent != null)
                {
                    Transform = tranformComponent.Transform;
                }
            }

            scene.PushAndSetTransform(Transform);
        }

        public override bool VIsVisible(Cv_SceneElement scene)
        {
            Cv_Transform camTransform = scene.Camera.GetViewTransform(scene.Renderer.VirtualWidth, scene.Renderer.VirtualHeight, scene.Renderer.Transform);

            var worldPos = WorldPosition;

            var fromWorldPos = Vector3.Transform(worldPos, camTransform.TransformMatrix);

            //See: https://yal.cc/rectangle-circle-intersection-test/
            var nearestX = Math.Max(0, Math.Min(fromWorldPos.X / camTransform.Scale.X, scene.Renderer.VirtualWidth));
            var nearestY = Math.Max(0, Math.Min(fromWorldPos.Y / camTransform.Scale.Y, scene.Renderer.VirtualHeight));
            
            var deltaX = (fromWorldPos.X / camTransform.Scale.X) - nearestX;
            var deltaY = (fromWorldPos.Y / camTransform.Scale.Y) - nearestY;
            return (deltaX * deltaX + deltaY * deltaY) < (Radius*camTransform.Scale.X*Transform.Scale.X * Radius*camTransform.Scale.X*Transform.Scale.Y);
        }

        public override void VPostRender(Cv_SceneElement scene)
        {
            scene.PopTransform();
        }

        public override void VRender(Cv_SceneElement scene)
        {
        }
    }
}