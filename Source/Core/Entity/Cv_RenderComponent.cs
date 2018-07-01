using System.Xml;
using Caravel.Core.Draw;
using Caravel.Core.Events;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Entity
{
    public abstract class Cv_RenderComponent : Cv_EntityComponent
    {
        public Color Color
        {
            get; protected set;
        }

        public Cv_SceneNode SceneNode
        {
            get
            {
                if (m_SceneNode == null)
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

        protected internal override bool VInit(XmlElement componentData)
        {
            XmlElement colorNode = (XmlElement) componentData.SelectSingleNode("//Color");
            if (colorNode != null)
            {
                if (colorNode != null)
                {
                    int r, g, b;

                    r = int.Parse(colorNode.Attributes["r"].Value);
                    g = int.Parse(colorNode.Attributes["g"].Value);
                    b = int.Parse(colorNode.Attributes["b"].Value);

                    Color = new Color(r,g,b);
                }
            }

            return VInheritedInit(componentData);
        }

        protected internal virtual bool VInheritedInit(XmlElement componentData)
        {
            return true;
        }

        protected internal override bool VPostInit()
        {
            Cv_SceneNode sceneNode = this.SceneNode;
            Cv_Event newEvent = new Cv_Event_NewRenderComponent(Owner.ID, Owner.Parent, sceneNode);
            Cv_EventManager.Instance.TriggerEvent(newEvent);
            return true;
        }

        protected internal override void VOnChanged()
        {
            var newEvent = new Cv_Event_ModifiedRenderComponent(Owner.ID);
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        protected internal override XmlElement VToXML()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement baseElement = VCreateBaseElement(doc);

            // color
            XmlElement color = doc.CreateElement("Color");
            color.SetAttribute("r", Color.R.ToString());
            color.SetAttribute("g", Color.G.ToString());
            color.SetAttribute("b", Color.B.ToString());
            baseElement.AppendChild(color);

            // create XML for inherited classes
            VCreateInheritedElement(baseElement);

            return baseElement;
        }

        protected abstract Cv_SceneNode VCreateSceneNode();

        protected abstract XmlElement VCreateInheritedElement(XmlElement baseElement);

        protected virtual XmlElement VCreateBaseElement(XmlDocument doc)
        {
            return doc.CreateElement(GetType().Name);
        }
    }
}