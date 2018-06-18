using System.Runtime.Serialization;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_RequestNewEntity : Cv_Event
    {
        public string EntityResource
        {
            get; private set;
        }

        public Cv_Transform InitialTransform
        {
            get; private set;
        }

        public Cv_EntityID ServerEntityID
        {
            get; private set;
        }

        public int GameViewID
        {
            get; private set;
        }

        public Cv_Event_RequestNewEntity(string entityResource, Cv_Transform initialTransform, Cv_EntityID serverEntityID = Cv_EntityID.INVALID_ENTITY, int gameViewId = 0)
        {
            EntityResource = entityResource;
            InitialTransform = initialTransform;
            ServerEntityID = serverEntityID;
            GameViewID = gameViewId;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "RequestNewEntity";
        }
    }
}