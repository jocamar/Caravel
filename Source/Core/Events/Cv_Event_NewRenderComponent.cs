using System.Runtime.Serialization;
using Caravel.Core.Draw;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_NewRenderComponent : Cv_Event
    {
        public string OwnersParent
        {
            get; set;
        }

        public Cv_EntityID EntityID
        {
            get; private set;
        }

        public Cv_SceneNode SceneNode
        {
            get; private set;
        }

        public Cv_Event_NewRenderComponent(Cv_EntityID entityID, Cv_SceneNode sceneNode, string ownersParent)
        {
            EntityID = entityID;
            SceneNode = sceneNode;
            OwnersParent = ownersParent;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "NewRenderComponent";
        }
    }
}