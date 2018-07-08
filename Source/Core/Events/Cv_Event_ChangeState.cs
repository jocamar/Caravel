using System.Runtime.Serialization;
using Caravel.Core.Entity;
using static Caravel.Core.Cv_GameLogic;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_ChangeState : Cv_Event
    {
        public Cv_GameState PreviousState
        {
            get; private set;
        }

        public Cv_GameState NewState
        {
            get; private set;
        }

        public Cv_Event_ChangeState(Cv_GameState prev, Cv_GameState next, float timeStamp = 0) : base(Cv_EntityID.INVALID_ENTITY, timeStamp)
        {
            PreviousState = prev;
            NewState = next;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "ChangeState";
        }
    }
}