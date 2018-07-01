using System.Runtime.Serialization;
using Caravel.Core.Draw;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_NewRenderComponent : Cv_Event
    {
        public Cv_EntityID EntityID
        {
            get; private set;
        }

        public Cv_EntityID ParentID
        {
            get; private set;
        }

        public Cv_SceneNode SceneNode
        {
            get; private set;
        }

        public Cv_Event_NewRenderComponent(Cv_EntityID entityID, Cv_EntityID parentId, Cv_SceneNode sceneNode)
        {
            EntityID = entityID;
            ParentID = parentId;
            SceneNode = sceneNode;
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