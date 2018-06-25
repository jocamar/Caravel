using System;
using System.Collections.Generic;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core
{
    public class Cv_SceneNode
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

                if (m_Parent != null)
                {
                    pos += m_Parent.WorldPosition;
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

        protected List<Cv_SceneNode> m_Children;
        protected Cv_SceneNode m_Parent;
        protected Cv_RenderComponent m_RenderComponent;

        public Cv_SceneNode(Cv_EntityID entityID, Cv_RenderComponent renderComponent, Cv_Transform to, Cv_Transform from = null)
        {
            Properties = new Cv_NodeProperties();
            Properties.EntityID = entityID;
            Properties.ToWorld = to;
            Properties.FromWorld = from;
            Properties.Name = renderComponent != null ? renderComponent.GetType().Name : "SceneNode";
            Properties.Radius = 0;
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

        public virtual void VPreRender(Cv_SceneElement scene)
        {
            Cv_Entity entity = CaravelApp.Instance.GameLogic.GetEntity(Properties.EntityID);

            if (entity != null)
            {
                Cv_TransformComponent tranformComponent = entity.GetComponent<Cv_TransformComponent>();

                if (tranformComponent != null)
                {
                    Transform = tranformComponent.Transform;
                }
            }

            scene.PushAndSetTransform(Transform);
        }

        public virtual bool VIsVisible(Cv_SceneElement scene)
        {
            Cv_Transform camTransform = scene.Camera.GetViewTransform(scene.Renderer.VirtualWidth, scene.Renderer.VirtualHeight, scene.Renderer.Transform);

            var worldPos = WorldPosition;

            var fromWorldPos = Vector3.Transform(worldPos, camTransform.TransformMatrix);

            //See: https://yal.cc/rectangle-circle-intersection-test/
            var nearestX = Math.Max(0, Math.Min(fromWorldPos.X, scene.Renderer.VirtualWidth));
            var nearestY = Math.Max(0, Math.Min(fromWorldPos.Y, scene.Renderer.VirtualHeight));
            
            var deltaX = fromWorldPos.X - nearestX;
            var deltaY = fromWorldPos.Y - nearestY;
            return (deltaX * deltaX + deltaY * deltaY) < (Radius * Radius);
        }

        public virtual void VRender(Cv_SceneElement scene)
        {

        }

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

        public virtual void VPostRender(Cv_SceneElement scene)
        {
            scene.PopTransform();
        }

        public virtual bool AddChild(Cv_SceneNode child)
        {
            if (child != null)
            {
                m_Children.Add(child);
                child.m_Parent = this;
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