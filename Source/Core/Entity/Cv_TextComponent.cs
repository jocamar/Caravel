using System;
using System.Globalization;
using System.Xml;
using Caravel.Core.Draw;
using Caravel.Debugging;
using static Caravel.Core.Draw.Cv_Renderer;

namespace Caravel.Core.Entity
{
    public class Cv_TextComponent : Cv_RenderComponent
    {
        public string Text
        {
            get; set;
        }

        public string FontResource
        {
            get; set;
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

        public Cv_TextAlign HorizontalAlignment
        {
            get; set;
        }

        public Cv_TextAlign VerticalAlignment
        {
            get; set;
        }

        private int m_iWidth, m_iHeight;

        protected override XmlElement VCreateInheritedElement(XmlElement baseElement)
        {
            var fontElement = baseElement.OwnerDocument.CreateElement("Font");
            fontElement.SetAttribute("resource", FontResource);
            baseElement.AppendChild(fontElement);

            var sizeElement = baseElement.OwnerDocument.CreateElement("Size");
            sizeElement.SetAttribute("width", Width.ToString(CultureInfo.InvariantCulture));
            sizeElement.SetAttribute("height", Height.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(sizeElement);

            var textElement = baseElement.OwnerDocument.CreateElement("Text");
            textElement.SetAttribute("text", Text);
            baseElement.AppendChild(textElement);

            var hAlignmentElement = baseElement.OwnerDocument.CreateElement("HorizontalAlignment");
            hAlignmentElement.SetAttribute("value", HorizontalAlignment.ToString());
            baseElement.AppendChild(hAlignmentElement);

            var vAlignmentElement = baseElement.OwnerDocument.CreateElement("VerticalAlignment");
            vAlignmentElement.SetAttribute("value", VerticalAlignment.ToString());
            baseElement.AppendChild(vAlignmentElement);

            return baseElement;
        }

        
        protected override bool VInheritedInit(XmlElement componentData)
        {
            Cv_Debug.Assert(componentData != null, "Must have valid component data.");

            var fontNode = componentData.SelectNodes("Font").Item(0);
            if (fontNode != null)
            {
                FontResource = fontNode.Attributes["resource"].Value;
            }

            var sizeNode = componentData.SelectNodes("Size").Item(0);
            if (sizeNode != null)
            {
                Width = int.Parse(sizeNode.Attributes["width"].Value);
                Height = int.Parse(sizeNode.Attributes["height"].Value);
            }

            var textNode = componentData.SelectNodes("Text").Item(0);
            if (textNode != null)
            {
                Text = textNode.Attributes["text"].Value;
            }

            var hAlignNode = componentData.SelectNodes("HorizontalAlignment").Item(0);
            if (hAlignNode != null)
            {
                HorizontalAlignment = (Cv_TextAlign) Enum.Parse(typeof(Cv_TextAlign), hAlignNode.Attributes["value"].Value);
            }

            var vAlignNode = componentData.SelectNodes("VerticalAlignment").Item(0);
            if (vAlignNode != null)
            {
                VerticalAlignment = (Cv_TextAlign) Enum.Parse(typeof(Cv_TextAlign), vAlignNode.Attributes["value"].Value);
            }

            return true;
        }

        protected override Cv_SceneNode VCreateSceneNode()
        {
            return new Cv_TextNode(Owner.ID, this, Cv_Transform.Identity);
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
            SceneNode.SetRadius(-1);

            base.VOnUpdate(elapsedTime);
        }

        protected internal override void VPostLoad()
        {
        }
    }
}