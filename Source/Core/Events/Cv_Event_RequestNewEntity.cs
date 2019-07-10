using System.Runtime.Serialization;
using static Caravel.Core.Cv_GameView;
using static Caravel.Core.Cv_SceneManager;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_RequestNewEntity : Cv_Event
    {
        public string EntityResource
        {
            get; private set;
        }

		public string EntityResourceBundle
		{
			get; private set;
		}

        public Cv_SceneID SceneID
        {
            get; private set;
        }

        public string SceneName
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

        public Cv_EntityID Parent
        {
            get; private set;
        }

        public Cv_GameViewID GameViewID
        {
            get; private set;
        }

        public bool Visible
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

        public Cv_Event_RequestNewEntity(string entityResource, Cv_SceneID sceneID, string sceneName, string entityName, string resourceBundle, bool visible,
                                            Cv_EntityID parentId, Cv_Transform? initialTransform, object sender,
                                            Cv_EntityID serverEntityID = Cv_EntityID.INVALID_ENTITY,
                                            Cv_GameViewID gameViewId = Cv_GameViewID.INVALID_GAMEVIEW) : base(Cv_EntityID.INVALID_ENTITY, sender)
        {
            SceneID = sceneID;
            SceneName = sceneName;
			EntityName = entityName;
            EntityResource = entityResource;
            InitialTransform = (initialTransform != null ? initialTransform.Value : Cv_Transform.Identity);
            ServerEntityID = serverEntityID;
            GameViewID = gameViewId;
            Parent = parentId;
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