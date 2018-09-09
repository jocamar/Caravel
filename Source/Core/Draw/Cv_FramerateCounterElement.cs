using System.Globalization;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Draw
{
    public class Cv_FramerateCounterElement : Cv_ScreenElement
    {
        private float m_fElapsedTime = 0f;
        private NumberFormatInfo m_Format;
        private int m_iFrameCounter;
        private int m_iFrameRate;
        private Vector2 m_Position;

        public Cv_FramerateCounterElement()
        {
            m_Format = new NumberFormatInfo();
            m_Format.NumberDecimalSeparator = ".";
            m_Position = new Vector2(30, 25);
        }

        public override void VOnPostRender(Cv_Renderer renderer)
        {
        }

        public override void VOnRender(float time, float elapsedTime, Cv_Renderer renderer)
        {
            m_iFrameCounter++;

            string fps = string.Format(m_Format, "{0} fps", m_iFrameRate);
            var fontResource = Cv_ResourceManager.Instance.GetResource<Cv_SpriteFontResource>("FramerateCounterFont", "Default");

            if (fontResource != null)
            {
                renderer.BeginDraw();
                renderer.DrawText(fontResource.GetFontData().Font, fps, m_Position + Vector2.One, Color.Black);
                renderer.DrawText(fontResource.GetFontData().Font, fps, m_Position, Color.White);
                renderer.EndDraw();
            }
        }

        public override void VOnUpdate(float time, float elapsedTime)
        {
            m_fElapsedTime += elapsedTime;

            if (m_fElapsedTime <= 1000)
            {
                return;
            }

            m_fElapsedTime -= 1000;
            m_iFrameRate = m_iFrameCounter;
            m_iFrameCounter = 0;
        }
    }
}