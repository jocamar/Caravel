using System.Runtime.Serialization;
using Caravel.Core.Entity;
using static Caravel.Core.Cv_SceneManager;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_RemoteSceneUnloaded : Cv_Event
    {
        public string SceneResource
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

        public string ResourceBundle
        {
            get; private set;
        }

        public Cv_Event_RemoteSceneUnloaded(string sceneResource, Cv_SceneID sceneID, string sceneName, string resourceBundle, object sender, float timeStamp = 0) : base(Cv_EntityID.INVALID_ENTITY, sender, timeStamp)
        {
            SceneID = sceneID;
            SceneName = sceneName;
            SceneResource = sceneResource;
            ResourceBundle = resourceBundle;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "RemoteSceneUnloaded";
        }
    }
}