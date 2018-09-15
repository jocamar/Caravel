using System.Runtime.Serialization;
using Caravel.Core.Draw;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_DestroyClickableComponent : Cv_Event
    {
        public Cv_ClickAreaNode ClickAreaNode
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

        public Cv_Event_DestroyClickableComponent(Cv_EntityID entityID, Cv_ClickAreaNode clickAreaNode, object sender) : base(entityID, sender)
        {
            ClickAreaNode = clickAreaNode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "DestroyClickableComponent";
        }
    }
}