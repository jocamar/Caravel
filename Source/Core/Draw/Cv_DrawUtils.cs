using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core.Draw
{
    public class Cv_DrawUtils
    {
        private static Texture2D m_DrawPixel;

		public static void Initialize()
        {
            m_DrawPixel = new Texture2D(CaravelApp.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            m_DrawPixel.SetData(new[] { Color.White });
        }

		public static void DrawLine(Cv_Renderer r, Vector2 start, Vector2 end, int thickness, Color color)
		{
			Vector2 edge = end - start;
			// calculate angle to rotate line
			float angle = (float) Math.Atan2(edge.Y , edge.X);

			r.Draw(m_DrawPixel,
				new Rectangle(// rectangle defines shape of line and position of start of line
					(int)start.X,
					(int)start.Y,
					(int)edge.Length(), //sb will strech the texture to fill this rectangle
					thickness), //width of line, change this to make thicker line
				null,
				color, //colour of line
				angle,     //angle of line (calulated above)
				new Vector2(0, 0), // point in line about which to rotate
				SpriteEffects.None,
				0);
		}

		public static void DrawRectangle(Cv_Renderer r, Rectangle rectangleToDraw, int thickness, Color color)
		{
            // Draw top line
            r.Draw(m_DrawPixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thickness), color);

			// Draw left line
			r.Draw(m_DrawPixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thickness, rectangleToDraw.Height), color);

			// Draw right line
			r.Draw(m_DrawPixel, new Rectangle((rectangleToDraw.X + rectangleToDraw.Width - thickness),
				rectangleToDraw.Y, thickness, rectangleToDraw.Height), color);
			// Draw bottom line
			r.Draw(m_DrawPixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y + rectangleToDraw.Height - thickness,
				rectangleToDraw.Width, thickness), color);
		}

        public static Texture2D CreateCircle(int radius)
        {
            int outerRadius = radius * 2 + 2; // So circle doesn't go out of bounds
            Texture2D texture = new Texture2D(CaravelApp.Instance.GraphicsDevice, outerRadius, outerRadius);

            Color[] data = new Color[outerRadius * outerRadius];

            // Colour the entire texture transparent first.
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;

            // Work out the minimum step necessary using trigonometry + sine approximation.
            double angleStep = 1f / radius;

            for (double angle = 0; angle < Math.PI * 2; angle += angleStep)
            {
                // Use the parametric definition of a circle: http://en.wikipedia.org/wiki/Circle#Cartesian_coordinates
                int x = (int)Math.Round(radius + radius * Math.Cos(angle));
                int y = (int)Math.Round(radius + radius * Math.Sin(angle));

                data[y * outerRadius + x + 1] = Color.White;
            }

            texture.SetData(data);
            return texture;
        }
    }
}