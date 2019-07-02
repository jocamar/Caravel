using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core.Draw
{
    public class Cv_Renderer
    {
        public static readonly int MaxLayers = 255;

		public enum Cv_BlendState
		{
			Additive,
			AlphaBlend,
			NonPremultiplied,
			Opaque
		}

        public enum Cv_TextAlign
        {
            Left,
            Center,
            Right,
            Bottom,
            Top,
        }

		public enum Cv_SamplerState
		{
			AnisotropicClamp,
			AnisotropicWrap,
			LinearClamp,
			LinearWrap,
			PointClamp,
			PointWrap
		}

		public Cv_BlendState Blend
		{
			get
			{
				if (m_BlendState == BlendState.Additive)
				{
					return Cv_BlendState.Additive;
				}
				else if (m_BlendState == BlendState.AlphaBlend)
				{
					return Cv_BlendState.AlphaBlend;
				}
				else if (m_BlendState == BlendState.NonPremultiplied)
				{
					return Cv_BlendState.NonPremultiplied;
				}
				else
				{
					return Cv_BlendState.Opaque;
				}
			}

			set
			{
				if (value == Cv_BlendState.Additive)
				{
					m_BlendState = BlendState.Additive;
				}
				else if (value == Cv_BlendState.AlphaBlend)
				{
					m_BlendState = BlendState.AlphaBlend;
				}
				else if (value == Cv_BlendState.NonPremultiplied)
				{
					m_BlendState = BlendState.NonPremultiplied;
				}
				else
				{
					m_BlendState = BlendState.Opaque;
				}
			}
		}

		public Cv_SamplerState Sampling
		{
			get
			{
				if (m_SamplerState == SamplerState.AnisotropicClamp)
				{
					return Cv_SamplerState.AnisotropicClamp;
				}
				else if (m_SamplerState == SamplerState.AnisotropicWrap)
				{
					return Cv_SamplerState.AnisotropicWrap;
				}
				else if (m_SamplerState == SamplerState.LinearClamp)
				{
					return Cv_SamplerState.LinearClamp;
				}
				else if (m_SamplerState == SamplerState.LinearWrap)
				{
					return Cv_SamplerState.LinearWrap;
				}
				else if (m_SamplerState == SamplerState.PointClamp)
				{
					return Cv_SamplerState.PointClamp;
				}
				else
				{
					return Cv_SamplerState.PointWrap;
				}
			}

			set
			{
				if (value == Cv_SamplerState.AnisotropicClamp)
				{
					m_SamplerState = SamplerState.AnisotropicClamp;
				}
				else if (value == Cv_SamplerState.AnisotropicWrap)
				{
					m_SamplerState = SamplerState.AnisotropicWrap;
				}
				else if (value == Cv_SamplerState.LinearClamp)
				{
					m_SamplerState = SamplerState.LinearClamp;
				}
				else if (value == Cv_SamplerState.LinearWrap)
				{
					m_SamplerState = SamplerState.LinearWrap;
				}
				else if (value == Cv_SamplerState.PointClamp)
				{
					m_SamplerState = SamplerState.PointClamp;
				}
				else
				{
					m_SamplerState = SamplerState.PointWrap;
				}
			}
		}

        private enum Cv_DrawType
        {
            Sprite,
            Text
        }

        private struct Cv_DrawCommand
        {
            public Cv_DrawType Type;
            public Texture2D Texture;
            public SpriteFont Font;
            public string Text;
            public Rectangle Dest;
            public Rectangle? Source;
            public Color Color;
            public float Rotation;
            public float TextScale;
            public float Layer;
            public Vector2 Origin;
            public SpriteEffects Effects;
            public bool NoCamera;
        }

        public Viewport Viewport;

        public bool RenderingToScreenIsFinished;

        public int StartX;
        public int StartY;

        public int VirtualHeight = 768;
        public int VirtualWidth = 1366;
        public int ScreenWidth = 1280;
        public int ScreenHeight = 720;

        public Vector2 ScreenSizePercent;
        public Vector2 ScreenOriginPercent;

        public bool DebugDrawRadius
        {
            get; set;
        }

        public bool DebugDrawPhysicsShapes
        {
            get; set;
        }

        public bool DebugDrawPhysicsBoundingBoxes
        {
            get; set;
        }

        public bool DebugDrawCameras
        {
            get; set;
        }

        public bool DebugDrawClickAreas
        {
            get; set;
        }

        public double Scale
        {
            get; private set;
        }

        public Cv_Transform Transform
        {
            get
            {
                if (m_bDirtyTransform)
                {
                    RecreateScaleMatrix();
                }

                return m_ScaleTransform;
            }
        }

        internal Matrix CamMatrix;

        private float m_fRatioX;
        private float m_fRatioY;

        private readonly int NUM_SUBLAYERS = 10000;
        private float m_iCurrSubLayer = 0;

        private SpriteBatch m_SpriteBatch;
        private Vector2 m_VirtualMousePosition = new Vector2();
        private static Cv_Transform m_ScaleTransform;
        private bool m_bDirtyTransform;
		private BlendState m_BlendState;
		private SamplerState m_SamplerState;
        private List<Cv_DrawCommand> m_DrawList;

        public Cv_Renderer(SpriteBatch spriteBatch)
        {
            if (spriteBatch == null)
            {
                m_SpriteBatch = new SpriteBatch(CaravelApp.Instance.CurrentGraphicsDevice);
            }
            else
            {
                m_SpriteBatch = spriteBatch;
            }

			m_BlendState = BlendState.NonPremultiplied;
			m_SamplerState = SamplerState.PointClamp;
            m_DrawList = new List<Cv_DrawCommand>();
        }
        
        public void Initialize()
        {
            SetupVirtualScreenViewport();

            m_bDirtyTransform = true;
        }

        public void DrawText(SpriteFont font, string text, Vector2 position, Color color, bool noCamera = false)
        {
            var newCommand = new Cv_DrawCommand();
            newCommand.Type = Cv_DrawType.Text;
            newCommand.Text = text;
            newCommand.Font = font;
            newCommand.Dest = new Rectangle((int)position.X, (int)position.Y, 1, 1);
            newCommand.Color = color;
            newCommand.Rotation = 0;
            newCommand.Origin = Vector2.Zero;
            newCommand.Effects = SpriteEffects.None;
            newCommand.Layer = 255;
            newCommand.TextScale = 1;
            newCommand.NoCamera = noCamera;

            m_DrawList.Add(newCommand);
        }

        public void DrawText(SpriteFont font, string[] text, Rectangle bounds, Cv_TextAlign horizontalAlign,
                                Cv_TextAlign verticalAlign, Color color, float rotation, float scale, SpriteEffects effects, float layerDepth, bool noCamera = false)
        {
            float totalY = 0f;

            foreach (var line in text)
            {
                totalY +=  font.MeasureString(line).Y;
            }

            var currSubLayer = m_iCurrSubLayer / (MaxLayers * NUM_SUBLAYERS);

            var currY = -totalY/2;
            foreach (var line in text)
            {
                Vector2 size = font.MeasureString(line);
                Vector2 pos = new Vector2(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
                Vector2 origin = new Vector2(size.X * 0.5f, 0);

                if (horizontalAlign == Cv_TextAlign.Left)
                {
                    origin.X += bounds.Width/2 - size.X/2;
                }

                if (horizontalAlign == Cv_TextAlign.Right)
                {
                    origin.X -= bounds.Width/2 - size.X/2;
                }

                if (verticalAlign == Cv_TextAlign.Top)
                {
                    origin.Y += bounds.Height/2 - totalY/2;
                }

                if (verticalAlign == Cv_TextAlign.Bottom)
                {
                    origin.Y -= bounds.Height/2 - totalY/2;
                }

                origin.Y -= currY;
                currY += size.Y;

                var newCommand = new Cv_DrawCommand();
                newCommand.Type = Cv_DrawType.Text;
                newCommand.Text = line;
                newCommand.Font = font;
                newCommand.Dest = new Rectangle((int) pos.X, (int) pos.Y, 1, 1);
                newCommand.Color = color;
                newCommand.Rotation = rotation;
                newCommand.Origin = origin;
                newCommand.Effects = effects;
                newCommand.Layer = layerDepth;
                newCommand.TextScale = scale;
                newCommand.NoCamera = noCamera;

                m_DrawList.Add(newCommand);
            }
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            var newCommand = new Cv_DrawCommand();
            newCommand.Type = Cv_DrawType.Sprite;
            newCommand.Texture = texture;
            newCommand.Dest = destinationRectangle;
            newCommand.Source = new Rectangle(0, 0, texture.Width, texture.Height);
            newCommand.Color = color;
            newCommand.Rotation = 0;
            newCommand.Origin = Vector2.Zero;
            newCommand.Effects = SpriteEffects.None;
            newCommand.Layer = 255;
            newCommand.NoCamera = false;

            m_DrawList.Add(newCommand);
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle sourceRectangle, Color color)
        {
            var newCommand = new Cv_DrawCommand();
            newCommand.Type = Cv_DrawType.Sprite;
            newCommand.Texture = texture;
            newCommand.Dest = destinationRectangle;
            newCommand.Source = sourceRectangle;
            newCommand.Color = color;
            newCommand.Rotation = 0;
            newCommand.Origin = Vector2.Zero;
            newCommand.Effects = SpriteEffects.None;
            newCommand.Layer = 255;
            newCommand.NoCamera = false;

            m_DrawList.Add(newCommand);
        }

        public void Draw(RenderTarget2D renderTarget2D, Vector2 position, Color color)
        {
            var newCommand = new Cv_DrawCommand();
            newCommand.Type = Cv_DrawType.Sprite;
            newCommand.Texture = renderTarget2D;
            newCommand.Dest = new Rectangle((int) position.X, (int) position.Y, renderTarget2D.Width, renderTarget2D.Height);
            newCommand.Source = new Rectangle(0, 0, renderTarget2D.Width, renderTarget2D.Height);
            newCommand.Color = color;
            newCommand.Rotation = 0;
            newCommand.Origin = Vector2.Zero;
            newCommand.Effects = SpriteEffects.None;
            newCommand.Layer = 255;
            newCommand.NoCamera = false;

            m_DrawList.Add(newCommand);
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle,
                            Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth, bool noCamera = false)
        {
            var newCommand = new Cv_DrawCommand();
            newCommand.Type = Cv_DrawType.Sprite;
            newCommand.Texture = texture;
            newCommand.Dest = destinationRectangle;
            newCommand.Source = sourceRectangle;
            newCommand.Color = color;
            newCommand.Rotation = rotation;
            newCommand.Origin = origin;
            newCommand.Effects = effects;
            newCommand.Layer = layerDepth;
            newCommand.NoCamera = noCamera;

            m_DrawList.Add(newCommand);
        }

        public void Draw(Texture2D texture, Vector3 position, Rectangle? sourceRectangle, Color color,
                                            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, bool noCamera = false)
        {
            float width = sourceRectangle != null ? sourceRectangle.Value.Width : texture.Width;
            float height = sourceRectangle != null ? sourceRectangle.Value.Height : texture.Height;

            width *= scale.X;
            height *= scale.Y;

            var newCommand = new Cv_DrawCommand();
            newCommand.Type = Cv_DrawType.Sprite;
            newCommand.Texture = texture;
            newCommand.Dest = new Rectangle((int) position.X, (int) position.Y, (int) width, (int) height);
            newCommand.Source = sourceRectangle;
            newCommand.Color = color;
            newCommand.Rotation = rotation;
            newCommand.Origin = origin;
            newCommand.Effects = effects;
            newCommand.Layer = 255;
            newCommand.NoCamera = noCamera;

            m_DrawList.Add(newCommand);
        }

        public Vector2 ScaleMouseToScreenCoordinates(Vector2 screenPosition)
        {
            var realX = screenPosition.X - Viewport.X;
            var realY = screenPosition.Y - Viewport.Y;

            m_VirtualMousePosition.X = realX;
            m_VirtualMousePosition.Y = realY;

            return m_VirtualMousePosition;
        }

        internal void BeginDraw(Cv_CameraNode camera = null)
        {
            m_DrawList.Clear();

            var transform = Transform.TransformMatrix;

            if (camera != null)
            {
                var cameraTransform = camera.GetViewTransform(VirtualWidth, VirtualHeight, Transform);
                if (camera.IsViewTransformDirty)
                {
                    CamMatrix = Matrix.CreateRotationZ(cameraTransform.Rotation) * Matrix.CreateTranslation(cameraTransform.Position.X, cameraTransform.Position.Y, 0)
                                                * Matrix.CreateScale(cameraTransform.Scale.X, cameraTransform.Scale.Y, 1);
                }

                transform = CamMatrix;
            }

            m_SpriteBatch.Begin(SpriteSortMode.Deferred, m_BlendState, m_SamplerState,
                                        DepthStencilState.None, RasterizerState.CullNone, null, transform);

            m_iCurrSubLayer = 0;
        }

        internal void EndDraw()
        {
            var sortedDrawList = m_DrawList.OrderBy(command => command.Layer).ThenBy(command => command.Dest.Y).ToArray();

            var drawWithCameraMatrix = true;
            foreach (var command in sortedDrawList)
            {
                if (command.NoCamera == drawWithCameraMatrix) {
                    m_SpriteBatch.End();

                    if (command.NoCamera)
                    {
                        m_SpriteBatch.Begin(SpriteSortMode.Deferred, m_BlendState, m_SamplerState,
                                        DepthStencilState.None, RasterizerState.CullNone, null, Transform.TransformMatrix);
                    }
                    else {
                        m_SpriteBatch.Begin(SpriteSortMode.Deferred, m_BlendState, m_SamplerState,
                                        DepthStencilState.None, RasterizerState.CullNone, null, CamMatrix);
                    }

                    drawWithCameraMatrix = !command.NoCamera;
                }

                if (command.Type == Cv_DrawType.Sprite)
                {
                    m_SpriteBatch.Draw(command.Texture, command.Dest,
                                            command.Source,
                                            command.Color,
                                            command.Rotation,
                                            command.Origin,
                                            command.Effects,
                                            0);
                }
                else
                {
                    m_SpriteBatch.DrawString(command.Font,
                                                command.Text,
                                                new Vector2(command.Dest.X, command.Dest.Y),
                                                command.Color,
                                                command.Rotation,
                                                command.Origin,
                                                command.TextScale,
                                                command.Effects,
                                                0);
                }
            }

            m_SpriteBatch.End();
        }

        internal void SetupViewport()
        {
            // Start by reseting viewport to (0,0,1,1)
            ResetViewport();
            // Calculate Proper Viewport according to Aspect Ratio
            SetupVirtualScreenViewport();
        }

        internal void ResetViewport()
        {
            SetupFullViewport();
        }

        private void SetupVirtualScreenViewport()
        {
            int width, height;
            double widthScale = 0;
            double heightScale = 0;
            widthScale = (double)ScreenWidth / VirtualWidth;
            heightScale = (double)ScreenHeight / VirtualHeight;

            Scale = Math.Min(widthScale, heightScale);

            width = (int)(VirtualWidth * Scale);
            height = (int)(VirtualHeight * Scale);

            // set up the new viewport centered in the backbuffer
            Viewport = new Viewport
                            {
                                X = StartX + (ScreenWidth / 2) - (width / 2),
                                Y = StartY + (ScreenHeight / 2) - (height / 2),
                                Width = width,
                                Height = height
                            };

            CaravelApp.Instance.CurrentGraphicsDevice.Viewport = Viewport;

            m_fRatioX = (float)Viewport.Width / VirtualWidth;
            m_fRatioY = (float)Viewport.Height / VirtualHeight;
        }

        private void SetupFullViewport()
        {
            var vp = new Viewport();
            vp.X = vp.Y = 0;
            vp.Width = ScreenWidth;
            vp.Height = ScreenHeight;
            CaravelApp.Instance.CurrentGraphicsDevice.Viewport = vp;
            m_bDirtyTransform = true;
        }

        private void RecreateScaleMatrix()
        {
            Cv_Transform newScale = new Cv_Transform(Vector3.Zero, new Vector2((float) Scale, (float) Scale), 0);
            m_ScaleTransform = newScale;
            m_bDirtyTransform = false;
        }
    }
}