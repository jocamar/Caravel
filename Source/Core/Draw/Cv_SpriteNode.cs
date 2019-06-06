using System;
using System.Collections.Generic;
using Caravel.Core.Entity;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Draw
{
    public class Cv_SpriteNode : Cv_SceneNode
    {
        public Cv_SpriteNode(Cv_EntityID entityID, Cv_RenderComponent renderComponent, Cv_Transform to, Cv_Transform? from = null) : base(entityID, renderComponent, to, from)
        {
        }

        internal override void VRender(Cv_Renderer renderer)
        {
            var spriteComponent = (Cv_SpriteComponent) Component;
            var scene = CaravelApp.Instance.Scene;

            var pos = scene.Transform.Position;
            var rot = scene.Transform.Rotation;
            var scale = scene.Transform.Scale;

            
            var camTransf = scene.Camera.GetViewTransform(renderer.VirtualWidth, renderer.VirtualHeight, Cv_Transform.Identity);

            if (spriteComponent.Parallax != 1)
            {
                var zoomFactor = ((1 + ((scene.Camera.Zoom - 1) * spriteComponent.Parallax)) / scene.Camera.Zoom);
                scale = scale * zoomFactor; //Magic formula
                pos += ((spriteComponent.Parallax - 1) * new Vector3(camTransf.Position.X, camTransf.Position.Y, 0));
                pos += ((new Vector3(scene.Transform.Position.X, scene.Transform.Position.Y, 0)) * (1 - zoomFactor) * (spriteComponent.Parallax - 1));
            }

            spriteComponent.DrawSelectionHighlight(renderer);

            if (!spriteComponent.Visible || spriteComponent.Texture == null || spriteComponent.Texture == "")
            {
                return;
            }

            Cv_RawTextureResource resource = Cv_ResourceManager.Instance.GetResource<Cv_RawTextureResource>(spriteComponent.Texture, spriteComponent.Owner.ResourceBundle);
			
            var tex = resource.GetTexture().Texture;

            var frameW = tex.Width / spriteComponent.FrameX;
			var frameH = tex.Height / spriteComponent.FrameY;
			var x = (spriteComponent.CurrentFrame % spriteComponent.FrameX) * frameW;
			var y = (spriteComponent.CurrentFrame / spriteComponent.FrameX) * frameH;

            var layerDepth = (int) Parent.Position.Z;
            layerDepth = layerDepth % Cv_Renderer.MaxLayers;

            var spriteEffect = SpriteEffects.None;

            if (spriteComponent.Mirrored)
            {
                spriteEffect = SpriteEffects.FlipHorizontally;
            }

            renderer.Draw(tex, new Rectangle((int) pos.X,
                                                (int)pos.Y,
                                                (int)(spriteComponent.Width * scale.X),
                                                (int)(spriteComponent.Height * scale.Y)),
                                    new Rectangle(x,y, frameW, frameH),
                                    spriteComponent.Color,
                                    rot,
                                    new Vector2(frameW * scene.Transform.Origin.X, frameH * scene.Transform.Origin.Y),
                                    spriteEffect,
                                    layerDepth / (float) Cv_Renderer.MaxLayers);
        }

        internal override float GetRadius(Cv_Renderer renderer)
        {
            Properties.Radius = -1; //Force radius recalculation each time
            if (Properties.Radius < 0)
            {
                var transf = Parent.WorldTransform;
                var originFactorX = Math.Abs(transf.Origin.X - 0.5) + 0.5;
                var originFactorY = Math.Abs(transf.Origin.Y - 0.5) + 0.5;
                var originFactor = (float) Math.Max(originFactorX, originFactorY);

                var comp = ((Cv_SpriteComponent) Component);
                Properties.Radius = (float) Math.Sqrt(comp.Width*comp.Width + comp.Height*comp.Height) * originFactor;
                Properties.Radius *= Math.Max(transf.Scale.X, transf.Scale.Y);
            }

            return Properties.Radius;
        }

        internal override void VPreRender(Cv_Renderer renderer)
        {
        }

        internal override bool VIsVisible(Cv_Renderer renderer)
        {
            return true;
        }

        internal override void VPostRender(Cv_Renderer renderer)
        {
        }

        internal override bool VPick(Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities)
        {
            var spriteComponent = (Cv_SpriteComponent) Component;
            return spriteComponent.Pick(renderer, screenPosition, entities);
        }
    }
}