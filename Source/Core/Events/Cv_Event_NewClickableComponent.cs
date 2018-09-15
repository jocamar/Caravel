using System.Runtime.Serialization;
using Caravel.Core.Draw;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Events
{
    public class Cv_Event_NewClickableComponent : Cv_Event
    {
        public Cv_EntityID ParentID
        {
            get; private set;
        }

        public Cv_ClickAreaNode ClickAreaNode
        {
            get; private set;
        }

        public int Width
        {
            get; private set;
        }

        public int Height
        {
            get; private set;
        }

        public Vector2 AnchorPoint
        {
            get; private set;
        }

        public bool Active
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

        public Cv_Event_NewClickableComponent(Cv_EntityID entityID, Cv_EntityID parentId, Cv_ClickAreaNode clickAreaNode,
                                                    int width, int height, Vector2 anchorPoint, bool active, object sender) : base(entityID, sender)
        {
            ParentID = parentId;
            ClickAreaNode = clickAreaNode;
            Width = width;
            Height = height;
            AnchorPoint = anchorPoint;
            Active = active;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string VGetName()
        {
            return "NewClickableComponent";
        }
    }
}