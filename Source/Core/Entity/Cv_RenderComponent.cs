using System.Xml;
using Caravel.Core.Draw;
using Caravel.Core.Events;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace Caravel.Core.Entity
{
    public abstract class Cv_RenderComponent : Cv_EntityComponent
    {
        public Color Color
        {
            get; protected set;
        }

        public bool Visible
        {
            get; set;
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

        private Cv_SceneNode m_SceneNode;

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

            // create XML for inherited classes
            VCreateInheritedElement(baseElement);

            return baseElement;
        }

        protected internal override bool VInitialize(XmlElement componentData)
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

            return VInheritedInit(componentData);
        }

        protected internal virtual bool VInheritedInit(XmlElement componentData)
        {
            return true;
        }

        protected internal override bool VPostInitialize()
        {
            Cv_SceneNode sceneNode = SceneNode;
            Cv_Event newEvent = new Cv_Event_NewRenderComponent(Owner.ID, Owner.Parent, sceneNode, this);
            Cv_EventManager.Instance.TriggerEvent(newEvent);
            return true;
        }

        protected internal override void VOnChanged()
        {
            var newEvent = new Cv_Event_ModifiedRenderComponent(Owner.ID, this);
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        protected internal override void VOnDestroy()
        {
            Cv_SceneNode renderNode = this.SceneNode;
            Cv_Event newEvent = new Cv_Event_DestroyRenderComponent(Owner.ID, renderNode, this);
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        protected abstract Cv_SceneNode VCreateSceneNode();

        protected abstract XmlElement VCreateInheritedElement(XmlElement baseElement);

        protected virtual XmlElement VCreateBaseElement(XmlDocument doc)
        {
            return doc.CreateElement(GetComponentName(this));
        }
    }
}