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

            var width = textComponent.Width * scale.X;
            var height = textComponent.Height * scale.Y;

            //Draws the yellow contour when an entity is selected in the editor
            if (scene.Caravel.EditorRunning && scene.EditorSelectedEntity == Properties.EntityID)
            {
                var rotMatrixZ = Matrix.CreateRotationZ(rot);

                Vector2 point1;
                Vector2 point2;
                List<Vector2> points = new List<Vector2>();
                points.Add(new Vector2(0, 0));
                points.Add(new Vector2(width, 0));
                points.Add(new Vector2(width, height));
                points.Add(new Vector2(0, height));

                for (int i = 0, j = 1; i < points.Count; i++, j++)
                {
                    if (j >= points.Count)
                    {
                        j = 0;
                    }

                    point1 = new Vector2(points[i].X, points[i].Y);
                    point2 = new Vector2(points[j].X, points[j].Y);

                    point1 -= new Vector2(scene.Transform.Origin.X * width, scene.Transform.Origin.Y * height);
                    point2 -= new Vector2(scene.Transform.Origin.X * width, scene.Transform.Origin.Y * height);
                    point1 = Vector2.Transform(point1, rotMatrixZ);
                    point2 = Vector2.Transform(point2, rotMatrixZ);
                    point1 += new Vector2(pos.X, pos.Y);
                    point2 += new Vector2(pos.X, pos.Y);

                    var thickness = (int) Math.Round(3 / scene.Camera.Zoom);
                    if (thickness <= 0)
                    {
                        thickness = 1;
                    }

                    Cv_DrawUtils.DrawLine(renderer,
                                            point1,
                                            point2,
                                            thickness,
                                            Cv_Renderer.MaxLayers-1,
                                            Color.Yellow);
                }
            }

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
                                    Math.Min(scene.Transform.Scale.X, scene.Transform.Scale.Y),
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
            var camMatrix = renderer.CamMatrix;
            var worldTransform = CaravelApp.Instance.Scene.Transform;
            var pos = new Vector2(worldTransform.Position.X, worldTransform.Position.Y);
            var rot = worldTransform.Rotation;
            var scale = worldTransform.Scale;

            var textComponent = (Cv_TextComponent) Component;
            
            var transformedVertices = new List<Vector2>();
            var point1 = new Vector2(-(worldTransform.Origin.X * textComponent.Width * scale.X),
                                     -(worldTransform.Origin.Y * textComponent.Height * scale.Y));

            var point2 = new Vector2(point1.X + (textComponent.Width * scale.X),
                                     point1.Y);

            var point3 = new Vector2(point2.X,
                                     point1.Y + (textComponent.Height * scale.Y));

            var point4 = new Vector2(point1.X,
                                     point3.Y);

            Matrix rotMat = Matrix.CreateRotationZ(rot);
            point1 = Vector2.Transform(point1, rotMat);
            point2 = Vector2.Transform(point2, rotMat);
            point3 = Vector2.Transform(point3, rotMat);
            point4 = Vector2.Transform(point4, rotMat);

            point1 += pos;
            point2 += pos;
            point3 += pos;
            point4 += pos;

            transformedVertices.Add(point1);
            transformedVertices.Add(point2);
            transformedVertices.Add(point3);
            transformedVertices.Add(point4);

            var invertedTransform = Matrix.Invert(camMatrix);
            var worldPoint = Vector2.Transform(screenPosition, invertedTransform);
            if (Cv_DrawUtils.PointInPolygon(worldPoint, transformedVertices))
            {
                entities.Add(Properties.EntityID);
                return true;
            }
            else
            {
                return false;
            }
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