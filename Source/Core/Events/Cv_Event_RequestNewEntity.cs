using System.Runtime.Serialization;
using static Caravel.Core.Cv_GameView;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_RequestNewEntity : Cv_Event
    {
        public string EntityResource
        {
            get; private set;
        }

		public string EntityName
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

        public Cv_GameViewID GameViewID
        {
            get; private set;
        }

        public Cv_Event_RequestNewEntity(string entityResource, string entityName, Cv_Transform initialTransform,
                                            Cv_EntityID serverEntityID = Cv_EntityID.INVALID_ENTITY,
                                            Cv_GameViewID gameViewId = Cv_GameViewID.INVALID_GAMEVIEW)
        {
			EntityName = entityName;
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