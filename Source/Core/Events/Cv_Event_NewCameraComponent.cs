using System.Runtime.Serialization;
using Caravel.Core.Draw;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_NewCameraComponent : Cv_Event
    {
        public string OwnersParent
        {
            get; set;
        }

        public Cv_EntityID EntityID
        {
            get; private set;
        }

        public Cv_CameraNode CameraNode
        {
            get; private set;
        }

        public bool IsDefault
        {
            get; private set;
        }

        public Cv_Event_NewCameraComponent(Cv_EntityID entityID, Cv_CameraNode cameraNode, bool isDefault, string ownersParent)
        {
            EntityID = entityID;
            CameraNode = cameraNode;
            IsDefault = isDefault;
            OwnersParent = ownersParent;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "NewCameraComponent";
        }
        
    }
}