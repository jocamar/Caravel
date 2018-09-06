using System.Runtime.Serialization;
using Caravel.Core.Entity;
using Caravel.Core.Physics;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_EntityVisibilityChanged : Cv_Event
    {
        public bool NewVisibility
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
    
        public Cv_Event_EntityVisibilityChanged(Cv_EntityID entityId, bool visibility, float timeStamp = 0) : base(entityId, timeStamp)
        {
            NewVisibility = visibility;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "EntityVisibilityChanged";
        }
    }
}