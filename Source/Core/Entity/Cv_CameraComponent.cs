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
            get; set;
        }

        public Cv_CameraNode CameraNode
        {
            get
            {
                if (m_CameraNode == null)
                {
                    var transformComponent = Owner.GetComponent<Cv_TransformComponent>();

                    var x = 0;
                    var y = 0;

                    if (transformComponent != null)
                    {
                        x = (int) transformComponent.Transform.Position.X;
                        x = (int) transformComponent.Transform.Position.Y;
                    }

                    m_CameraNode = new Cv_CameraNode(CameraName, x, y, Zoom);
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

        public string OwnersParent
        {
            get; private set;
        }

        private Cv_CameraNode m_CameraNode;

        protected internal override bool VInit(XmlElement componentData)
        {
            IsDefaultCamera = false;
            Zoom = 1f;

            if (componentData.Attributes != null)
            {
                if (componentData.Attributes["defaultCamera"] != null)
                {
                    IsDefaultCamera = bool.Parse(componentData.Attributes["defaultCamera"].Value);
                }

                if (componentData.Attributes["zoom"] != null)
                {
                    Zoom = (float) double.Parse(componentData.Attributes["zoom"].Value, CultureInfo.InvariantCulture);
                }

                if (componentData.Attributes["parent"] != null)
                {
                    OwnersParent = componentData.Attributes["parent"].Value;
                }
            }

            //TODO(JM): Maybe add camera offset later

            return true;
        }

        protected internal override bool VPostInit()
        {
            Cv_CameraNode cameraNode = this.CameraNode;
            Cv_Event newEvent = new Cv_Event_NewCameraComponent(Owner.ID, cameraNode, IsDefaultCamera, OwnersParent);
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
            throw new System.NotImplementedException();
        }
    }
}