using System.Runtime.Serialization;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_TransformEntity : Cv_Event
    {
        public Cv_EntityID EntityID
        {
            get; private set;
        }

        public Cv_Transform Transform
        {
            get; private set;
        }

        public Cv_Event_TransformEntity(Cv_EntityID entityID, Cv_Transform transform)
        {
            EntityID = entityID;
            Transform = transform;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "TransformEntity";
        }
    }
}