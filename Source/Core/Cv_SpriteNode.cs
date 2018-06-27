using System;
using Caravel.Core.Entity;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core
{
    public class Cv_SpriteNode : Cv_SceneNode
    {
        public Cv_SpriteNode(Cv_Entity.Cv_EntityID entityID, Cv_RenderComponent renderComponent, Cv_Transform to, Cv_Transform from = null) : base(entityID, renderComponent, to, from)
        {
            var comp = ((Cv_SpriteComponent) renderComponent);
            Radius = (float) Math.Sqrt(comp.Width*comp.Width + comp.Height*comp.Height)/2;
        }

        public override void VRender(Cv_SceneElement scene)
        {
            var spriteComponent = (Cv_SpriteComponent) m_RenderComponent;

            var resource = Cv_ResourceManager.Instance.GetResource<Cv_RawTextureResource>(spriteComponent.Texture);
            var tex = resource.GetTexture().Texture;
            var pos = Position;
            var rot = Rotation;
            var scale = Scale;

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
                                    new Vector2(frameW / 2, frameH / 2),
                                    SpriteEffects.None,
                                    pos.Z);
        }

        public override bool VOnChanged(Cv_SceneElement scene)
        {
            var comp = ((Cv_SpriteComponent) m_RenderComponent);
            Radius = (float) Math.Sqrt(comp.Width*comp.Width + comp.Height*comp.Height)/2;
            return true;
        }
    }
}