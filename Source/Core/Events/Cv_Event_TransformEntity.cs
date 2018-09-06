using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_TransformEntity : Cv_Event
    {
        /*public Cv_Transform Transform
        {
            get; private set;
		}*/

		public Vector3? OldPosition
		{
			get; private set;
		}

		public Vector3 NewPosition
		{
			get; private set;
		}

		public Vector2? OldScale
		{
			get; private set;
		}

		public Vector2 NewScale
		{
			get; private set;
		}

		public Vector2? OldOrigin
		{
			get; private set;
		}

		public Vector2 NewOrigin
		{
			get; private set;
		}

		public float? OldRotation
		{
			get; private set;
		}

		public float NewRotation
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

        public Cv_Event_TransformEntity(Cv_EntityID entityID, Cv_Transform oldTransform, Vector3 newPos,
														Vector2 newScale, Vector2 newOrigin, float newRotation, object sender) : base(entityID, sender)
        {
            NewPosition = newPos;
			NewScale = newScale;
			NewRotation = newRotation;
			NewOrigin = newOrigin;
			
			OldPosition = oldTransform.Position;
			OldScale = oldTransform.Scale;
			OldRotation = oldTransform.Rotation;
			OldOrigin = oldTransform.Origin;
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