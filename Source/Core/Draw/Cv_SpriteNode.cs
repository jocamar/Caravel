using System;
using Caravel.Core.Entity;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core.Draw
{
    public class Cv_SpriteNode : Cv_SceneNode
    {
        public override float Radius
        {
            get
            {
                if (Properties.Radius < 0)
                {
                    var transf = Parent.Transform;
                    var originFactorX = Math.Abs(transf.Origin.X - 0.5) + 0.5;
                    var originFactorY = Math.Abs(transf.Origin.Y - 0.5) + 0.5;
                    var originFactor = (float) Math.Max(originFactorX, originFactorY);

                    var comp = ((Cv_SpriteComponent) m_Component);
                    Properties.Radius = (float) Math.Sqrt(comp.Width*comp.Width + comp.Height*comp.Height) * originFactor;
                    Properties.Radius *= Math.Max(transf.Scale.X, transf.Scale.Y);
                }

                return Properties.Radius;
            }
        }

        public Cv_SpriteNode(Cv_Entity.Cv_EntityID entityID, Cv_RenderComponent renderComponent, Cv_Transform to, Cv_Transform from = null) : base(entityID, renderComponent, to, from)
        {
            var comp = ((Cv_SpriteComponent) renderComponent);
        }

        public override void VRender(Cv_SceneElement scene)
        {
            var spriteComponent = (Cv_SpriteComponent) m_Component;

            var resource = Cv_ResourceManager.Instance.GetResource<Cv_RawTextureResource>(spriteComponent.Texture);
            var tex = resource.GetTexture().Texture;
            var pos = scene.Transform.Position;
            var rot = scene.Transform.Rotation;
            var scale = scene.Transform.Scale;

            var frameW = tex.Width / spriteComponent.FrameX;
			var frameH = tex.Height / spriteComponent.FrameY;
			var x = (spriteComponent.CurrentFrame % spriteComponent.FrameX) * frameW;
			var y = (spriteComponent.CurrentFrame / spriteComponent.FrameX) * frameH;

            scene.Renderer.Draw(tex, new Rectangle((int) pos.X,
                                                    (int)pos.Y,
                                                    (int)(spriteComponent.Width * scale.X),
                                                    (int)(spriteComponent.Height * scale.Y)),
                                    new Rectangle(x,y, frameW, frameH),
                                    spriteComponent.Color,
                                    rot,
                                    new Vector2(frameW * scene.Transform.Origin.X, frameH * scene.Transform.Origin.Y),
                                    SpriteEffects.None,
                                    pos.Z);
        }

        public override bool VOnChanged(Cv_SceneElement scene)
        {
            var comp = ((Cv_SpriteComponent) m_Component);
            Radius = (float) Math.Sqrt(comp.Width*comp.Width + comp.Height*comp.Height)/2;
            return true;
        }

        public override void VPreRender(Cv_SceneElement scene)
        {
        }

        public override bool VIsVisible(Cv_SceneElement scene)
        {
            return true;
        }
    }
}