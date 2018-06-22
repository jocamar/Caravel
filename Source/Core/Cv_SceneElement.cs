using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core
{
    public class Cv_SceneElement : Cv_ScreenElement
    {
        private SpriteBatch m_SpriteBatch;

        public Cv_SceneElement(SpriteBatch sb)
        {
            m_SpriteBatch = sb;
        }

        public override void VOnRender(float time, float timeElapsed)
        {
            var res = Cv_ResourceManager.Instance.GetResource<Cv_RawTextureResource>("profile.png");
            var tex = res.GetTexture();
            
            m_SpriteBatch.Draw(tex.Texture, Vector2.Zero, Color.White);
        }

        public override void VOnUpdate(float time, float timeElapsed)
        {

        }
    }
}