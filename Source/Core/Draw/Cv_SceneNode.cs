using System;
using System.Collections.Generic;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Draw
{
    public abstract class Cv_SceneNode
    {
        public class Cv_NodeProperties
        {
            public Cv_EntityID EntityID;
            public string Name;
            public Cv_Transform ToWorld;
            public Cv_Transform FromWorld;
            public float Radius;
        }

        public virtual Cv_NodeProperties Properties
        {
            get; private set;
        }

        public Cv_SceneNode Parent
        {
            get; private set;
        }

        public bool Paused
        {
            get; internal set;
        }

        //Updated on the OnMoveEntity callback in the scene element
        internal virtual Cv_Transform Transform
        {
            get
            {
                return Properties.ToWorld;
            }

            set
            {
                if (value.Position != Properties.ToWorld.Position
                    || value.Rotation != Properties.ToWorld.Rotation
                    || value.Scale != Properties.ToWorld.Scale
                    || value.Origin != Properties.ToWorld.Origin)
                {
                    Properties.ToWorld = value;
                    Properties.FromWorld = Cv_Transform.Inverse(value);
                    TransformChanged = true;
                }
            }
        }

        internal virtual Cv_Transform WorldTransform
        {
            get
            {
                var trans = Transform;

                if (Parent != null)
                {
                    trans = Cv_Transform.Multiply(Parent.WorldTransform, trans);
                }

                return trans;
            }
        }

        internal virtual Vector3 Position
        {
            get
            {
                return Properties.ToWorld.Position;
            }

            set
            {
                if (Properties.ToWorld.Position != value)
                {
                    Properties.ToWorld = new Cv_Transform(value, Properties.ToWorld.Scale, Properties.ToWorld.Rotation, Properties.ToWorld.Origin);
                    TransformChanged = true;
                }
            }
        }

        internal virtual Vector3 WorldPosition
        {
            get
            {
                var pos = WorldTransform.Position;

                return pos;
            }
        }

        internal virtual Vector2 Scale
        {
            get
            {
                return Properties.ToWorld.Scale;
            }

            set
            {
                if (Properties.ToWorld.Scale != value)
                {
                    Properties.ToWorld = new Cv_Transform(Properties.ToWorld.Position, value, Properties.ToWorld.Rotation, Properties.ToWorld.Origin);
                    TransformChanged = true;
                }
            }
        }

		internal virtual Vector2 Origin
        {
            get
            {
                return Properties.ToWorld.Origin;
            }

            set
            {
                if (Properties.ToWorld.Origin != value)
                {
                    Properties.ToWorld = new Cv_Transform(Properties.ToWorld.Position, Properties.ToWorld.Scale, Properties.ToWorld.Rotation, value);
                    TransformChanged = true;
                }
            }
        }

        internal virtual float Rotation
        {
            get
            {
                return Properties.ToWorld.Rotation;
            }

            set
            {
                if (Math.Abs(Properties.ToWorld.Rotation - value) > 0.00001)
                {
                    Properties.ToWorld = new Cv_Transform(Properties.ToWorld.Position, Properties.ToWorld.Scale, value, Properties.ToWorld.Origin);
                    TransformChanged = true;
                }
            }
        }

        internal bool TransformChanged
        {
            get; set;
        }

        internal bool WorldTransformChanged
        {
            get
            {
                var changed = TransformChanged;

                if (Parent != null)
                {
                    changed = changed || Parent.WorldTransformChanged;
                }

                return changed;
            }
        }

        protected List<Cv_SceneNode> Children;
        protected Cv_EntityComponent Component;

        private string Name
        {
            get
            {
                var entity = CaravelApp.Instance.Logic.GetEntity(Properties.EntityID);
                var name = (entity != null ? entity.EntityName : "root") + "_" + this.GetType().Name;
                return name;
            }
        }

        
        public void PrintTree(int level)
        {
            Console.WriteLine(Name);
            foreach(var n in Children)
            {
                for (var i = 0; i <= level; i++)
                {
                    Console.Write("\t");
                }
                n.PrintTree(level + 1);
            }
        }

        internal Cv_SceneNode(Cv_EntityID entityID, Cv_EntityComponent renderComponent, Cv_Transform to, Cv_Transform? from = null)
        {
            Properties = new Cv_NodeProperties();
            Properties.EntityID = entityID;
            Properties.ToWorld = to;
            Properties.FromWorld = (from != null ? from.Value : Cv_Transform.Identity);
            Properties.Name = renderComponent != null ? renderComponent.GetType().Name : "SceneNode";
            Properties.Radius = -1;
            TransformChanged = true;
            Component = renderComponent;
            Children = new List<Cv_SceneNode>();
        }

        internal virtual float GetRadius(Cv_Renderer renderer)
        {
            return Properties.Radius;
        }

        internal virtual void SetRadius(float value)
        {
            Properties.Radius = value;
        }

        internal virtual void VOnUpdate(float time, float elapsedTime)
        {
            if (Paused)
            {
                return;
            }
            
            foreach (var child in Children)
            {
                child.VOnUpdate(time, elapsedTime);
            }
        }

        internal virtual bool VOnChanged()
        {
            foreach (var child in Children)
            {
                child.VOnChanged();
            }

            return true;
        }

        internal abstract void VPreRender(Cv_Renderer renderer);

        internal abstract bool VIsVisible(Cv_Renderer renderer);

        internal abstract void VRender(Cv_Renderer renderer);

        internal abstract void VPostRender(Cv_Renderer renderer);

        internal virtual void VFinishedRender(Cv_Renderer renderer)
        {
            TransformChanged = false;

            foreach (var child in Children)
            {
                child.VFinishedRender(renderer);
            }
        }

        internal virtual void VRenderChildren(Cv_Renderer renderer)
        {
            foreach (var child in Children)
            {
                child.VPreRender(renderer);

                if (child.VIsVisible(renderer))
                {
                    child.VRender(renderer);
                    child.VRenderChildren(renderer);
                }

                child.VPostRender(renderer);
            }
        }

        internal virtual bool AddChild(Cv_SceneNode child)
        {
            if (child != null)
            {
                Children.Add(child);
                child.Parent = this;

                return true;
            }

            return false;
        }

        internal virtual bool RemoveChild(Cv_EntityID entityId)
        {
            Cv_SceneNode toErase = null;
            foreach (var c in Children)
            {
                if (c.Properties.EntityID != Cv_EntityID.INVALID_ENTITY && c.Properties.EntityID == entityId)
                {
                    toErase = c;
                    break;
                }
            }

            if (toErase != null)
            {
                Children.Remove(toErase);
                return true;
            }

            var removed = false;

            foreach (var c in Children)
            {
                if (c.RemoveChild(entityId))
                {
                    removed = true;
                    break;
                }
            }

            return removed;
        }

        internal virtual bool RemoveChild(Cv_SceneNode node)
        {
            Cv_SceneNode toErase = null;
            foreach (var c in Children)
            {
                if (c == node)
                {
                    toErase = c;
                    break;
                }
            }

            if (toErase != null)
            {
                Children.Remove(toErase);
                return true;
            }

            var removed = false;

            foreach (var c in Children)
            {
                if (c.RemoveChild(node))
                {
                    removed = true;
                    break;
                }
            }

            return removed;
        }

        internal virtual bool VPick(Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities)
        {
            var success = false;
            foreach (var child in Children)
            {
                if (child.VPick(renderer, screenPosition, entities))
                {
                    success = true;
                }
            }

            return success;
        }

        internal bool Pick<NodeType>(Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities) where NodeType : Cv_SceneNode
        {
            if (this is Cv_HolderNode)
            {
                CaravelApp.Instance.Scene.PushAndSetTransform(Transform);
            }

            var success = false;
            foreach (var child in Children)
            {
                if (child is Cv_HolderNode && child.Pick<NodeType>(renderer, screenPosition, entities))
                {
                    success = true;
                }
                else if (child is NodeType && child.VPick(renderer, screenPosition, entities))
                {
                    success = true;
                }
            }

            if (this is Cv_HolderNode)
            {
                CaravelApp.Instance.Scene.PopTransform();
            }

            return success;
        }
    }
}