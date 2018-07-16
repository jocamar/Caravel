using System.Globalization;
using System.Xml;
using Caravel.Core.Draw;
using Caravel.Core.Events;

namespace Caravel.Core.Entity
{
    public class Cv_CameraComponent : Cv_EntityComponent
    {
        public float Zoom
        {
            get
            {
                return m_Zoom;
            }

            set
            {
                m_Zoom = value;

                if (m_Zoom < 0.1)
                {
                    m_Zoom = 0.1f;
                }
                ZoomChanged = true;
            }
        }

        public Cv_CameraNode CameraNode
        {
            get
            {
                if (m_CameraNode == null)
                {
                    m_CameraNode = new Cv_CameraNode(Owner.ID, this);
                }

                return m_CameraNode;
            }
            
            protected set
            {
                m_CameraNode = value;
            }
        }

        public string CameraName
        {
            get; protected set;
        }

        public bool IsDefaultCamera
        {
            get; protected set;
        }

        public bool ZoomChanged
        {
            get; internal set;
        }

        private Cv_CameraNode m_CameraNode;
        private float m_Zoom;

        protected internal override bool VInit(XmlElement componentData)
        {
            IsDefaultCamera = false;
            Zoom = 1f;

            var propertiesNode = componentData.SelectSingleNode("//Properties");

            if (propertiesNode != null && propertiesNode.Attributes != null)
            {
                if (propertiesNode.Attributes["defaultCamera"] != null)
                {
                    IsDefaultCamera = bool.Parse(propertiesNode.Attributes["defaultCamera"].Value);
                }

                if (propertiesNode.Attributes["zoom"] != null)
                {
                    Zoom = (float) double.Parse(propertiesNode.Attributes["zoom"].Value, CultureInfo.InvariantCulture);
                }
            }

            //TODO(JM): Maybe add camera offset later

            return true;
        }

        protected internal override bool VPostInit()
        {
            Cv_CameraNode cameraNode = this.CameraNode;
            Cv_Event newEvent = new Cv_Event_NewCameraComponent(Owner.ID, Owner.Parent, cameraNode, IsDefaultCamera);
            Cv_EventManager.Instance.TriggerEvent(newEvent);
            return true;
        }

        protected internal override void VOnUpdate(float deltaTime)
        {
        }

        protected internal override void VOnChanged()
        {
        }

        protected internal override XmlElement VToXML()
        {
            var doc = new XmlDocument();
            var cameraElement = doc.CreateElement(GetComponentName<Cv_CameraComponent>());

            var propertiesElement = doc.CreateElement("Properties");
            propertiesElement.SetAttribute("defaultCamera", IsDefaultCamera.ToString());
            propertiesElement.SetAttribute("zoom", Zoom.ToString());
            cameraElement.AppendChild(propertiesElement);

            return cameraElement;
        }

		protected internal override void VPostLoad()
		{
		}
	}
}