using System;
using System.Collections.Generic;
using System.Text;
using Caravel.Core.Entity;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Draw
{
    public class Cv_TextNode : Cv_SceneNode
    {
        public Cv_TextNode(Cv_EntityID entityID, Cv_RenderComponent renderComponent, Cv_Transform to, Cv_Transform? from = null) : base(entityID, renderComponent, to, from)
        {
        }

        internal override bool VIsVisible(Cv_Renderer renderer)
        {
            return true;
        }

        internal override void VPostRender(Cv_Renderer renderer)
        {
        }

        internal override void VPreRender(Cv_Renderer renderer)
        {
        }

        //TODO(JM): Maybe change rendering to draw text into a texture and then reuse texture
        internal override void VRender(Cv_Renderer renderer)
        {
            var textComponent = (Cv_TextComponent) Component;
            var scene = CaravelApp.Instance.Scene;

            var pos = scene.Transform.Position;
            var rot = scene.Transform.Rotation;
            var scale = scene.Transform.Scale;

            textComponent.DrawSelectionHighlight(renderer);
            
            if (!textComponent.Visible || textComponent.Text == null || textComponent.Text == "" || textComponent.FontResource == null || textComponent.FontResource == "")
            {
                return;
            }

            Cv_SpriteFontResource resource = Cv_ResourceManager.Instance.GetResource<Cv_SpriteFontResource>(textComponent.FontResource, textComponent.Owner.ResourceBundle);
			
            var font = resource.GetFontData().Font;

            var text = WrapText(CaravelApp.Instance.GetString(textComponent.Text), font, textComponent.Width, textComponent.Height);

            var layerDepth = (int) pos.Z;
            layerDepth = layerDepth % Cv_Renderer.MaxLayers;

            var bounds = new Rectangle((int) (pos.X - (textComponent.Width * scene.Transform.Origin.X)),
                                        (int) (pos.Y - (textComponent.Width * scene.Transform.Origin.Y)),
                                        (int) textComponent.Width,
                                        (int) textComponent.Width);

            renderer.DrawText(font, text, bounds, textComponent.HorizontalAlignment, textComponent.VerticalAlignment, textComponent.Color, 
                                    rot,
                                    Math.Min(scale.X, scale.Y),
                                    SpriteEffects.None,
                                    layerDepth / (float) Cv_Renderer.MaxLayers);
        }

        internal override float GetRadius(Cv_Renderer renderer)
        {
            if (Properties.Radius < 0)
            {
                var transf = Parent.Transform;
                var originFactorX = Math.Abs(transf.Origin.X - 0.5) + 0.5;
                var originFactorY = Math.Abs(transf.Origin.Y - 0.5) + 0.5;
                var originFactor = (float) Math.Max(originFactorX, originFactorY);

                var comp = ((Cv_TextComponent) Component);
                Properties.Radius = (float) Math.Sqrt(comp.Width*comp.Width + comp.Height*comp.Height) * originFactor;
                Properties.Radius *= Math.Max(transf.Scale.X, transf.Scale.Y);
            }

            return Properties.Radius;
        }

        internal override bool VPick(Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities)
        {
            var textComponent = (Cv_TextComponent) Component;
            return textComponent.Pick(renderer, screenPosition, entities);
        }

        private string[] WrapText(string text, SpriteFont font, int width, int height)
        {
            if(font.MeasureString(text).X < width) {
                return new string[] { text };
            }

            string[] words = text.Split(' ');
            StringBuilder wrappedText = new StringBuilder();
            float linewidth = 0f;
            float textHeight = 0f;
            float spaceWidth = font.MeasureString(" ").X;

            List<string> lines = new List<string>();
            for(int i = 0; i < words.Length; ++i) {

                Vector2 size = font.MeasureString(words[i]);
                if(linewidth + size.X < width) {
                    linewidth += size.X + spaceWidth;
                } else {
                    lines.Add(wrappedText.ToString());
                    wrappedText.Clear();
                    linewidth = size.X + spaceWidth;
                }

                wrappedText.Append(words[i]);
                wrappedText.Append(" ");
            }

            lines.Add(wrappedText.ToString());

            return lines.ToArray();
        }
    }
}