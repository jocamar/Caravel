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

        public override XmlElement VToXML()
        {
            var doc = new XmlDocument();
            var cameraElement = doc.CreateElement(GetComponentName<Cv_CameraComponent>());

            var propertiesElement = doc.CreateElement("Properties");
            propertiesElement.SetAttribute("defaultCamera", IsDefaultCamera.ToString());
            propertiesElement.SetAttribute("zoom", Zoom.ToString(CultureInfo.InvariantCulture));
            cameraElement.AppendChild(propertiesElement);

            return cameraElement;
        }

        public override bool VInitialize(XmlElement componentData)
        {
            IsDefaultCamera = false;
            Zoom = 1f;

            var propertiesNode = componentData.SelectSingleNode("Properties");

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

        public override bool VPostInitialize()
        {
            Cv_CameraNode cameraNode = this.CameraNode;
            Cv_Event newEvent = new Cv_Event_NewCameraComponent(Owner.ID, Owner.Parent, cameraNode, IsDefaultCamera, this);
            Cv_EventManager.Instance.TriggerEvent(newEvent);
            return true;
        }

        public override void VOnChanged()
        {
        }

		public override void VPostLoad()
		{
		}

        public override void VOnDestroy()
        {
            Cv_CameraNode cameraNode = this.CameraNode;
            Cv_Event newEvent = new Cv_Event_DestroyCameraComponent(Owner.ID,cameraNode, this);
            Cv_EventManager.Instance.TriggerEvent(newEvent);
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
        }
    }
}