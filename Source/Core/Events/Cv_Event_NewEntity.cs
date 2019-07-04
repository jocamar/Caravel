using System.Runtime.Serialization;
using static Caravel.Core.Cv_GameView;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_NewEntity : Cv_Event
    {
        public Cv_GameViewID GameViewID
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

        public Cv_Event_NewEntity(Cv_EntityID entityID, object sender, Cv_GameViewID gameViewId = 0) : base(entityID, sender)
        {
            GameViewID = gameViewId;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "NewEntity";
        }
    }
}