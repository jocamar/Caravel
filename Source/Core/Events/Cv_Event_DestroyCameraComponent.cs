using System.Runtime.Serialization;
using Caravel.Core.Draw;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_DestroyCameraComponent : Cv_Event
    {
        public Cv_CameraNode CameraNode
        {
            get; private set;
        }

        public override bool WriteToLog
        {
            get
            {
                return false;
            }
        }

        public Cv_Event_DestroyCameraComponent(Cv_EntityID entityID, Cv_CameraNode cameraNode, object sender) : base(entityID, sender)
        {
            CameraNode = cameraNode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "DestroyCameraComponent";
        }
    }
}