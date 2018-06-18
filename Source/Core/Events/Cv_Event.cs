using System.Runtime.Serialization;

namespace Caravel.Core.Events
{
    public abstract class Cv_Event : ISerializable
    {
        private Cv_EventType m_iEventID = (int) Cv_EventType.INVALID_EVENT;

        public enum Cv_EventType
        {
            INVALID_EVENT = 0
        }

        public Cv_EventType Type
        {
            get {
                if (m_iEventID == Cv_EventType.INVALID_EVENT)
                {
                    m_iEventID = (Cv_EventType) this.GetType().Name.GetHashCode();
                }

                return m_iEventID;
            }
        }

        public float TimeStamp
        {
            get; private set;
        }

        public Cv_Event(float timeStamp = 0.0f)
        {
            TimeStamp = timeStamp;
        }

        public static Cv_EventType GetType<Event>() where Event : Cv_Event
        {
            return (Cv_EventType) typeof(Event).Name.GetHashCode();
        }

        public abstract string VGetName();
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}