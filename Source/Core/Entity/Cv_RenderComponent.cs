using System.Xml;
using Caravel.Core.Draw;
using Caravel.Core.Events;
using Microsoft.Xna.Framework;
using System.Globalization;
using System.Collections.Generic;
using System;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Entity
{
    public abstract class Cv_RenderComponent : Cv_EntityComponent
    {
        public Color Color
        {
            get; set;
        }

        public bool Visible
        {
            get; set;
        }

		public bool IsFading
		{
			get; private set;
		}

        public Cv_SceneNode SceneNode
        {
            get
            {
                if (m_SceneNode == null && Owner != null)
                {
                    m_SceneNode = VCreateSceneNode();
                }

                return m_SceneNode;
            }
            
            protected set
            {
                m_SceneNode = value;
            }
        }

        public int Width
        {
            get
            {
                return m_iWidth;
            }

            set
            {
                m_iWidth = value;

                if (SceneNode != null)
                {
                    SceneNode.SetRadius(-1);
                }
            }
        }

        public int Height
        {
            get
            {
                return m_iHeight;
            }

            set
            {
                m_iHeight = value;
                
                if (SceneNode != null)
                {
                    SceneNode.SetRadius(-1);
                }
            }
        }
        
        public float Parallax
        {
            get; set;
        }

        private Cv_SceneNode m_SceneNode;

        private int m_iFinalFadeAlpha;
        private float m_fRemainingFadeTime;
        private int m_iWidth, m_iHeight;

        public Cv_RenderComponent()
        {
            Parallax = 1f;
        }

        public override XmlElement VToXML()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement baseElement = VCreateBaseElement(doc);

            // color
            XmlElement color = doc.CreateElement("Color");
            color.SetAttribute("r", Color.R.ToString(CultureInfo.InvariantCulture));
            color.SetAttribute("g", Color.G.ToString(CultureInfo.InvariantCulture));
            color.SetAttribute("b", Color.B.ToString(CultureInfo.InvariantCulture));
            color.SetAttribute("a", Color.A.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(color);

            // color
            XmlElement visible = doc.CreateElement("Visible");
            visible.SetAttribute("status", Visible.ToString());
            baseElement.AppendChild(visible);

            var sizeElement = baseElement.OwnerDocument.CreateElement("Size");
            sizeElement.SetAttribute("width", Width.ToString(CultureInfo.InvariantCulture));
            sizeElement.SetAttribute("height", Height.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(sizeElement);
            
            var paralaxElement = baseElement.OwnerDocument.CreateElement("Parallax");
            paralaxElement.SetAttribute("value", Parallax.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(paralaxElement);

            // create XML for inherited classes
            VCreateInheritedElement(baseElement);

            return baseElement;
        }

        public void FadeTo(int alpha, float interval)
        {
            IsFading = true;
            m_fRemainingFadeTime = interval;
            m_iFinalFadeAlpha = alpha;
        }

        public void DrawSelectionHighlight(Cv_Renderer renderer)
        {
            var scene = CaravelApp.Instance.Scene;

            var pos = scene.Transform.Position;
            var rot = scene.Transform.Rotation;
            var scale = scene.Transform.Scale;

            //Draws the yellow contour when an entity is selected in the editor
            if (scene.Caravel.EditorRunning && scene.EditorSelectedEntity == Owner.ID)
            {
                var camTransf = scene.Camera.GetViewTransform(renderer.VirtualWidth, renderer.VirtualHeight, Cv_Transform.Identity);
                
                if (Parallax != 1 && Parallax > 0)
                {
                    var zoomFactor = ((1 + ((scene.Camera.Zoom - 1) * Parallax)) / scene.Camera.Zoom);
                    scale = scale * zoomFactor; //Magic formula
                    pos += ((Parallax - 1) * new Vector3(camTransf.Position.X, camTransf.Position.Y, 0));
                    pos += ((new Vector3(scene.Transform.Position.X, scene.Transform.Position.Y, 0)) * (1 - zoomFactor) * (Parallax - 1));
                }

                var noCamera = Parallax == 0;
                var rotMatrixZ = Matrix.CreateRotationZ(rot);

                Vector2 point1;
                Vector2 point2;
                List<Vector2> points = new List<Vector2>();
                var width = Width * scale.X;
                var height = Height * scale.Y;
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
                                            Color.Yellow, noCamera);
                }
            }
        }

        public bool Pick(Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities)
        {
            var camMatrix = renderer.CamMatrix;
            var worldTransform = CaravelApp.Instance.Scene.Transform;
            var pos = new Vector2(worldTransform.Position.X, worldTransform.Position.Y);
            var rot = worldTransform.Rotation;
            var scale = worldTransform.Scale;

            Vector3 tmpScale;
            Quaternion tmpQuat; //We don't care about these but we need them for compatibility with older .NET versions
            Vector3 camPos;
            camMatrix.Decompose(out tmpScale, out tmpQuat, out camPos);

            if (Parallax != 1)
            {
                var zoomFactor = ((1 + (((tmpScale.X / renderer.Transform.Scale.X) - 1) * Parallax)) / (tmpScale.X / renderer.Transform.Scale.X));
                scale = scale * zoomFactor; //Magic formula
                pos += ((Parallax - 1) * new Vector2(camPos.X, camPos.Y));
                pos += ((new Vector2(worldTransform.Position.X, worldTransform.Position.Y)) * (1 - zoomFactor) * (Parallax - 1));
            }
            
            var transformedVertices = new List<Vector2>();
            var point1 = new Vector2(-(worldTransform.Origin.X * Width * scale.X),
                                     -(worldTransform.Origin.Y * Height * scale.Y));

            var point2 = new Vector2(point1.X + (Width * scale.X),
                                     point1.Y);

            var point3 = new Vector2(point2.X,
                                     point1.Y + (Height * scale.Y));

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
                entities.Add(Owner.ID);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool VInitialize(XmlElement componentData)
        {
            Visible = true;

            XmlElement colorNode = (XmlElement) componentData.SelectSingleNode("Color");
            if (colorNode != null)
            {
                int r, g, b, a;

                r = int.Parse(colorNode.Attributes["r"].Value);
                g = int.Parse(colorNode.Attributes["g"].Value);
                b = int.Parse(colorNode.Attributes["b"].Value);

                a = 255;

                if (colorNode.Attributes["a"] != null)
                {
                    a = int.Parse(colorNode.Attributes["a"].Value);
                }

                Color = new Color(r,g,b,a);
            }

            XmlElement visibleNode = (XmlElement) componentData.SelectSingleNode("Visible");
            if (visibleNode != null)
            {
                bool visible;

                visible = bool.Parse(visibleNode.Attributes["status"].Value);
                Visible = visible;
            }

            var sizeNode = componentData.SelectNodes("Size").Item(0);
            if (sizeNode != null)
            {
                Width = int.Parse(sizeNode.Attributes["width"].Value);
                Height = int.Parse(sizeNode.Attributes["height"].Value);
            }

            var paralaxNode = componentData.SelectNodes("Parallax").Item(0);

            if (paralaxNode != null)
            {
                Parallax = float.Parse(paralaxNode.Attributes["value"].Value, CultureInfo.InvariantCulture);
            }

            return VInheritedInit(componentData);
        }

        public override bool VPostInitialize()
        {
            Cv_SceneNode sceneNode = SceneNode;
            Cv_Event newEvent = new Cv_Event_NewRenderComponent(Owner.ID, Owner.Parent, sceneNode, this);
            Cv_EventManager.Instance.QueueEvent(newEvent, true);
            return true;
        }

        public override void VOnChanged()
        {
            if (Owner.Initialized)
            {
                var newEvent = new Cv_Event_ModifiedRenderComponent(Owner.ID, this);
                Cv_EventManager.Instance.QueueEvent(newEvent, true);
            }
        }

        public override void VOnDestroy()
        {
            Cv_SceneNode renderNode = this.SceneNode;
            Cv_Event newEvent = new Cv_Event_DestroyRenderComponent(Owner.ID, renderNode, this);
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
            if (IsFading)
            {
                 var alphaDiff = m_iFinalFadeAlpha - this.Color.A;

                if (alphaDiff == 0 || m_fRemainingFadeTime <= 0)
                {
                    if (m_iFinalFadeAlpha <= 0)
                    {
                        this.Color = new Color(this.Color, 0);
                    }

                    IsFading = false;
                }
                else
                {
                    var alphaIncrement = alphaDiff / m_fRemainingFadeTime;
                    alphaIncrement *= elapsedTime;
                    var newAlpha = this.Color.A + alphaIncrement;

                    if (alphaIncrement <= 0 && newAlpha < m_iFinalFadeAlpha)
                    {
                        newAlpha = m_iFinalFadeAlpha;
                    }

                    if (alphaIncrement > 0 && newAlpha > m_iFinalFadeAlpha)
                    {
                        newAlpha = m_iFinalFadeAlpha;
                    }
                    
                    this.Color = new Color(this.Color, (int) newAlpha);
                    m_fRemainingFadeTime -= elapsedTime;
                }
            }
        }

        protected abstract Cv_SceneNode VCreateSceneNode();

        protected abstract bool VInheritedInit(XmlElement componentData);

        protected abstract XmlElement VCreateInheritedElement(XmlElement baseElement);

        protected virtual XmlElement VCreateBaseElement(XmlDocument doc)
        {
            return doc.CreateElement(GetComponentName(this));
        }
    }
}