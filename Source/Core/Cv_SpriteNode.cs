using System;
using Caravel.Core.Entity;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;

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
            var pos = Position;

            scene.Renderer.Draw(resource.GetTexture().Texture, new Rectangle((int) pos.X - spriteComponent.Width/2,
                                                                                (int)pos.Y - spriteComponent.Height/2,
                                                                                spriteComponent.Width,
                                                                                spriteComponent.Height),
                                                                spriteComponent.Color);
        }
    }
}