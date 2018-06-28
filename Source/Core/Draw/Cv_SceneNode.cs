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

        public virtual Cv_Transform Transform
        {
            get
            {
                return Properties.ToWorld;
            }

            set
            {
                Properties.ToWorld = value;
                Properties.FromWorld = Cv_Transform.Inverse(value);
            }
        }

        public virtual Vector3 Position
        {
            get
            {
                return Properties.ToWorld.Position;
            }

            set
            {
                Properties.ToWorld.Position = value;
            }
        }

        public virtual Vector3 WorldPosition
        {
            get
            {
                var pos = Position;

                if (Parent != null)
                {
                    pos += Parent.WorldPosition;
                }

                return pos;
            }
        }

        public virtual Vector2 Scale
        {
            get
            {
                return Properties.ToWorld.Scale;
            }

            set
            {
                Properties.ToWorld.Scale = value;
            }
        }

        public virtual float Rotation
        {
            get
            {
                return Properties.ToWorld.Rotation;
            }

            set
            {
                Properties.ToWorld.Rotation = value;
            }
        }

        public virtual float Radius
        {
            get
            {
                return Properties.Radius;
            }

            set
            {
                Properties.Radius = value;
            }
        }

        public Cv_SceneNode Parent
        {
            get; private set;
        }

        protected List<Cv_SceneNode> m_Children;
        protected Cv_RenderComponent m_RenderComponent;

        public Cv_SceneNode(Cv_EntityID entityID, Cv_RenderComponent renderComponent, Cv_Transform to, Cv_Transform from = null)
        {
            Properties = new Cv_NodeProperties();
            Properties.EntityID = entityID;
            Properties.ToWorld = to;
            Properties.FromWorld = from;
            Properties.Name = renderComponent != null ? renderComponent.GetType().Name : "SceneNode";
            Properties.Radius = 1;
            m_RenderComponent = renderComponent;
            m_Children = new List<Cv_SceneNode>();
        }

        public virtual void VOnUpdate(float time, float timeElapsed, Cv_SceneElement scene)
        {
            foreach (var child in m_Children)
            {
                child.VOnUpdate(time, timeElapsed, scene);
            }
        }

        public virtual bool VOnChanged(Cv_SceneElement scene)
        {
            foreach (var child in m_Children)
            {
                child.VOnChanged(scene);
            }

            return true;
        }

        public abstract void VPreRender(Cv_SceneElement scene);

        public abstract bool VIsVisible(Cv_SceneElement scene);

        public abstract void VPostRender(Cv_SceneElement scene);

        public abstract void VRender(Cv_SceneElement scene);

        public virtual void VRenderChildren(Cv_SceneElement scene)
        {
            foreach (var child in m_Children)
            {
                child.VPreRender(scene);

                if (child.VIsVisible(scene))
                {
                    child.VRender(scene);

                    child.VRenderChildren(scene);
                }

                child.VPostRender(scene);
            }
        }

        public virtual bool AddChild(Cv_SceneNode child)
        {
            if (child != null)
            {
                m_Children.Add(child);
                child.Parent = this;
                var childPos = child.Position;
                var radius = childPos.Length() + child.Radius;

                if (radius > Radius)
                {
                    Radius = radius;
                }

                return true;
            }

            return false;
        }

        public virtual bool RemoveChild(Cv_EntityID entityId)
        {
            Cv_SceneNode toErase = null;
            foreach (var c in m_Children)
            {
                if (c.Properties.EntityID != Cv_EntityID.INVALID_ENTITY && c.Properties.EntityID == entityId)
                {
                    toErase = c;
                    break;
                }
            }

            if (toErase != null)
            {
                m_Children.Remove(toErase);
                return true;
            }

            foreach (var c in m_Children)
            {
                if (c.RemoveChild(entityId))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool VPick(Cv_SceneElement scene,  Vector2 screenPosition)
        {
            foreach (var child in m_Children)
            {
                if (!child.VPick(scene, screenPosition))
                {
                    return false;
                }
            }

            return true;
        }
    }
}