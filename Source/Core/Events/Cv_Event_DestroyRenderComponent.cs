using System.Runtime.Serialization;
using Caravel.Core.Draw;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_DestroyRenderComponent : Cv_Event
    {
        public Cv_SceneNode SceneNode
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

        public Cv_Event_DestroyRenderComponent(Cv_EntityID entityID, Cv_SceneNode renderNode, object sender) : base(entityID, sender)
        {
            SceneNode = renderNode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "DestroyRenderComponent";
        }
    }
}