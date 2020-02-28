using System.Runtime.Serialization;

namespace Caravel.Core.Events
{
    public class Cv_Event_ChangeEntityPauseState : Cv_Event
    {
        public bool PauseState
        {
            get; private set;
        }

        public Cv_Event_ChangeEntityPauseState(Entity.Cv_Entity.Cv_EntityID entityId, bool state, object sender, float timeStamp = 0) : base(entityId, sender, timeStamp)
        {
            PauseState = state;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "ChangeEntityPauseState";
        }
    }
}