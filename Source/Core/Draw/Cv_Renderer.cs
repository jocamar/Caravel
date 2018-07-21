using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core.Draw
{
    public class Cv_Renderer
    {
		public enum Cv_BlendState
		{
			Additive,
			AlphaBlend,
			NonPremultiplied,
			Opaque
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

        private double m_dScale;
        private float m_fRatioX;
        private float m_fRatioY;

        private SpriteBatch m_SpriteBatch;
        private Vector2 m_VirtualMousePosition = new Vector2();
        private static Cv_Transform m_ScaleTransform;
        private bool m_bDirtyTransform;
		private BlendState m_BlendState;
		private SamplerState m_SamplerState;

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
        }
        
        public void Init()
        {
            SetupVirtualScreenViewport();

            m_fRatioX = (float)Viewport.Width / VirtualWidth;
            m_fRatioY = (float)Viewport.Height / VirtualHeight;

            m_bDirtyTransform = true;
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            m_SpriteBatch.Draw(texture, destinationRectangle, color);
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle sourceRectangle, Color color)
        {
            m_SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color);
        }

        public void Draw(RenderTarget2D renderTarget2D, Vector2 position, Color color)
        {
            m_SpriteBatch.Draw(renderTarget2D, position, color);
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle,
                            Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            m_SpriteBatch.Draw(texture, destinationRectangle,
                                        sourceRectangle,
                                        color,
                                        rotation,
                                        origin,
                                        effects,
                                        layerDepth);
        }

        public void Draw(Texture2D texture, Vector3 position, Rectangle? sourceRectangle, Color color,
                                            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects)
        {
            m_SpriteBatch.Draw(texture, new Vector2(position.X, position.Y),
                                        sourceRectangle,
                                        color,
                                        rotation,
                                        origin,
                                        scale,
                                        effects,
                                        position.Z);
        }

        public void Draw(Texture2D texture, Vector2? position, int z, Rectangle? destinationRectangle, Rectangle? sourceRectangle,
                                            Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects)
        {
            m_SpriteBatch.Draw(texture, position,
                                        destinationRectangle,
                                        sourceRectangle,
                                        origin,
                                        rotation,
                                        scale,
                                        color,
                                        effects,
                                        z);
        }

        public Vector2 ScaleMouseToScreenCoordinates(Vector2 screenPosition)
        {
            var realX = screenPosition.X - Viewport.X;
            var realY = screenPosition.Y - Viewport.Y;

            m_VirtualMousePosition.X = realX / m_fRatioX;
            m_VirtualMousePosition.Y = realY / m_fRatioY;

            return m_VirtualMousePosition;
        }

        internal void BeginDraw(Cv_CameraNode camera = null)
        {
            if (camera == null)
            {
                m_SpriteBatch.Begin(SpriteSortMode.FrontToBack, m_BlendState, m_SamplerState,
                                        DepthStencilState.None, RasterizerState.CullNone, null, Transform.TransformMatrix);
            }
            else
            {
                var cameraTransform = camera.GetViewTransform(VirtualWidth, VirtualHeight, Transform);
                if (camera.IsViewTransformDirty)
                {
                    CamMatrix = Matrix.CreateRotationZ(cameraTransform.Rotation) * Matrix.CreateTranslation(cameraTransform.Position.X, cameraTransform.Position.Y, 0)
                                                * Matrix.CreateScale(cameraTransform.Scale.X, cameraTransform.Scale.Y, 1);
                }

                m_SpriteBatch.Begin(SpriteSortMode.FrontToBack, m_BlendState, m_SamplerState,
                                        DepthStencilState.None, RasterizerState.CullNone, null, CamMatrix);
            }
        }

        internal void EndDraw()
        {
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

            m_dScale = Math.Min(widthScale, heightScale);

            width = (int)(VirtualWidth * m_dScale);
            height = (int)(VirtualHeight * m_dScale);

            // set up the new viewport centered in the backbuffer
            Viewport = new Viewport
                            {
                                X = StartX + (ScreenWidth / 2) - (width / 2),
                                Y = StartY + (ScreenHeight / 2) - (height / 2),
                                Width = width,
                                Height = height
                            };

            CaravelApp.Instance.CurrentGraphicsDevice.Viewport = Viewport;
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
            Cv_Transform newScale = new Cv_Transform(Vector3.Zero, new Vector2((float) m_dScale, (float) m_dScale), 0);
            m_ScaleTransform = newScale;
            m_bDirtyTransform = false;
        }
    }
}