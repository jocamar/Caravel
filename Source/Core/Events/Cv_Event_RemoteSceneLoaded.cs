using System.Runtime.Serialization;
using Caravel.Core.Entity;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_RemoteSceneLoaded : Cv_Event
    {
        public string SceneResource
        {
            get; private set;
        }

        public string SceneID
        {
            get; private set;
        }

        public string ResourceBundle
        {
            get; private set;
        }

        public Cv_Event_RemoteSceneLoaded(string sceneResource, string sceneID, string resourceBundle, object sender, float timeStamp = 0) : base(Cv_EntityID.INVALID_ENTITY, sender, timeStamp)
        {
            SceneID = sceneID;
            SceneResource = sceneResource;
            ResourceBundle = resourceBundle;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "RemoteSceneLoaded";
        }
    }
}