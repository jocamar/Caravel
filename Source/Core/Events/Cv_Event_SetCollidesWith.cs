using System.Runtime.Serialization;

namespace Caravel.Core.Events
{
    public class Cv_Event_SetCollidesWith : Cv_Event
    {
        public string ShapeID
        {
            get; private set;
        }

        public int Category
        {
            get; private set;
        }

        public string Direction
        {
            get; private set;
        }

        public bool State
        {
            get; private set;
        }

        public Cv_Event_SetCollidesWith(string shapeID, int category, string direction, bool state,
                                            Entity.Cv_Entity.Cv_EntityID entityId, 
                                            object sender, float timeStamp = 0) : base(entityId, sender, timeStamp)
        {
            ShapeID = shapeID;
            Category = category;
            State = state;
            Direction = direction;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "SetCollidesWith";
        }
    }
}