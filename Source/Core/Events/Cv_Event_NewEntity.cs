using System.Runtime.Serialization;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_NewEntity : Cv_Event
    {
        public Cv_EntityID EntityID
        {
            get; private set;
        }

        public int GameViewID
        {
            get; private set;
        }

        public Cv_Event_NewEntity(Cv_EntityID entityID, int gameViewId = 0)
        {
            EntityID = entityID;
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