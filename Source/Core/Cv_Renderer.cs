using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core
{
    public class Cv_Renderer
    {
        public Color BackgroundColor = Color.Black;
        public Viewport Viewport;

        public bool RenderingToScreenIsFinished;

        public int StartX;
        public int StartY;

        public int VirtualHeight = 768;
        public int VirtualWidth = 1366;
        public int ScreenWidth = 1280;
        public int ScreenHeight = 720;

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

        private double m_dScale;
        private float m_fRatioX;
        private float m_fRatioY;

        private SpriteBatch m_SpriteBatch;
        private Vector2 m_VirtualMousePosition = new Vector2();
        private static Cv_Transform m_ScaleTransform;
        private bool m_bDirtyTransform = true;

        public Cv_Renderer()
        {
            m_SpriteBatch = new SpriteBatch(CaravelApp.Instance.GraphicsDevice);
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
            m_SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
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
                m_SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                                        DepthStencilState.None, RasterizerState.CullNone, null, Transform.TransformMatrix);
            }
            else
            {
                var cameraTransformMatrix = camera.GetViewTransform(VirtualWidth, VirtualHeight, Transform).TransformMatrix;
                m_SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                                        DepthStencilState.None, RasterizerState.CullNone, null, cameraTransformMatrix);
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

            CaravelApp.Instance.GraphicsDevice.Viewport = Viewport;
        }

        private void SetupFullViewport()
        {
            var vp = new Viewport();
            vp.X = vp.Y = 0;
            vp.Width = ScreenWidth;
            vp.Height = ScreenHeight;
            CaravelApp.Instance.GraphicsDevice.Viewport = vp;
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