using System;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caravel.Core.Draw
{
    public class Cv_LineBatch : IDisposable
    {
        private const int m_iDefaultBufferSize = 500;

        // a basic effect, which contains the shaders that we will use to draw our
        // primitives.
        private BasicEffect m_BasicEffect;

        // the device that we will issue draw calls to.
        private GraphicsDevice m_Device;

        // hasBegun is flipped to true once Begin is called, and is used to make
        // sure users don't call End before Begin is called.
        private bool m_bHasBegun;

        private bool m_bIsDisposed;
        private VertexPositionColor[] m_LineVertices;
        private int m_iLineVertsCount;

        public Cv_LineBatch(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, m_iDefaultBufferSize)
        {
        }

        public Cv_LineBatch(GraphicsDevice graphicsDevice, int bufferSize)
        {
            if (graphicsDevice == null)
            {
                Cv_Debug.Error("Graphics device must not be null.");
            }

            m_Device = graphicsDevice;

            m_LineVertices = new VertexPositionColor[bufferSize - bufferSize % 2];

            // set up a new basic effect, and enable vertex colors.
            m_BasicEffect = new BasicEffect(graphicsDevice);
            m_BasicEffect.VertexColorEnabled = true;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !m_bIsDisposed)
            {
                if (m_BasicEffect != null)
                    m_BasicEffect.Dispose();

                m_bIsDisposed = true;
            }
        }

        public void Begin(Matrix projection, Matrix view)
        {
            if (m_bHasBegun)
            {
                throw new InvalidOperationException("End must be called before Begin can be called again.");
            }

            m_Device.SamplerStates[0] = SamplerState.AnisotropicClamp;
            //tell our basic effect to begin.
            m_BasicEffect.Projection = projection;
            m_BasicEffect.View = view;
            m_BasicEffect.CurrentTechnique.Passes[0].Apply();

            // flip the error checking boolean. It's now ok to call DrawLineShape, Flush,
            // and End.
            m_bHasBegun = true;
        }

        /*public void DrawLineShape(Shape shape)
        {
            DrawLineShape(shape, Color.Black);
        }

        public void DrawLineShape(Shape shape, Color color)
        {
            if (!hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before DrawLineShape can be called.");
            }
            if (shape.ShapeType != ShapeType.Edge &&
                shape.ShapeType != ShapeType.Chain)
            {
                throw new NotSupportedException("The specified shapeType is not supported by LineBatch.");
            }
            if (shape.ShapeType == ShapeType.Edge)
            {
                if (lineVertsCount >= lineVertices.Length)
                {
                    Flush();
                }
                EdgeShape edge = (EdgeShape)shape;
                lineVertices[lineVertsCount].Position = new Vector3(edge.Vertex1, 0f);
                lineVertices[lineVertsCount + 1].Position = new Vector3(edge.Vertex2, 0f);
                lineVertices[lineVertsCount].Color = lineVertices[lineVertsCount + 1].Color = color;
                lineVertsCount += 2;
            }
            else if (shape.ShapeType == ShapeType.Chain)
            {
                ChainShape loop = (ChainShape)shape;
                for (int i = 0; i < loop.Vertices.Count; ++i)
                {
                    if (lineVertsCount >= lineVertices.Length)
                    {
                        Flush();
                    }
                    lineVertices[lineVertsCount].Position = new Vector3(loop.Vertices[i], 0f);
                    lineVertices[lineVertsCount + 1].Position = new Vector3(loop.Vertices.NextVertex(i), 0f);
                    lineVertices[lineVertsCount].Color = lineVertices[lineVertsCount + 1].Color = color;
                    lineVertsCount += 2;
                }
            }
        }*/

        public void DrawPoints(Vector2[] verts)
        {
            DrawPoints(verts, Color.Black);
        }

        public void DrawPoints(Vector2[] verts, Color color)
        {
            if (!m_bHasBegun)
            {
                throw new InvalidOperationException("Begin must be called before DrawVertices can be called.");
            }
            for (int i = 0; i < verts.Length; ++i)
            {
                if (m_iLineVertsCount >= m_LineVertices.Length)
                {
                    Flush();
                }
                m_LineVertices[m_iLineVertsCount].Position = new Vector3(verts[i], 0f);
                m_LineVertices[m_iLineVertsCount + 1].Position = new Vector3(verts[(i+1) % m_LineVertices.Length], 0f);
                m_LineVertices[m_iLineVertsCount].Color = m_LineVertices[m_iLineVertsCount + 1].Color = color;
                m_iLineVertsCount += 2;
            }
        }

        public void DrawLine(Vector2 v1, Vector2 v2)
        {
            DrawLine(v1, v2, Color.Black);
        }

        public void DrawLine(Vector2 v1, Vector2 v2, Color color)
        {
            if (!m_bHasBegun)
            {
                throw new InvalidOperationException("Begin must be called before DrawLineShape can be called.");
            }
            if (m_iLineVertsCount >= m_LineVertices.Length)
            {
                Flush();
            }
            m_LineVertices[m_iLineVertsCount].Position = new Vector3(v1, 0f);
            m_LineVertices[m_iLineVertsCount + 1].Position = new Vector3(v2, 0f);
            m_LineVertices[m_iLineVertsCount].Color = m_LineVertices[m_iLineVertsCount + 1].Color = color;
            m_iLineVertsCount += 2;
        }

        // End is called once all the primitives have been drawn using AddVertex.
        // it will call Flush to actually submit the draw call to the graphics card, and
        // then tell the basic effect to end.
        public void End()
        {
            if (!m_bHasBegun)
            {
                throw new InvalidOperationException("Begin must be called before End can be called.");
            }

            // Draw whatever the user wanted us to draw
            Flush();

            m_bHasBegun = false;
        }

        private void Flush()
        {
            if (!m_bHasBegun)
            {
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
            }
            if (m_iLineVertsCount >= 2)
            {
                int primitiveCount = m_iLineVertsCount / 2;
                // submit the draw call to the graphics card
                m_Device.DrawUserPrimitives(PrimitiveType.LineList, m_LineVertices, 0, primitiveCount);
                m_iLineVertsCount -= primitiveCount * 2;
            }
        }
    }
}