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
                if (value != Properties.ToWorld)
                {
                    Properties.ToWorld = value;
                    Properties.FromWorld = Cv_Transform.Inverse(value);
                    TransformChanged = true;
                }
            }
        }

        public virtual Cv_Transform WorldTransform
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

        public virtual Vector3 Position
        {
            get
            {
                return Properties.ToWorld.Position;
            }

            set
            {
                if (Properties.ToWorld.Position != value)
                {
                    Properties.ToWorld.Position = value;
                    TransformChanged = true;
                }
            }
        }

        public virtual Vector3 WorldPosition
        {
            get
            {
                var pos = WorldTransform.Position;

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
                if (Properties.ToWorld.Scale != value)
                {
                    Properties.ToWorld.Scale = value;
                    TransformChanged = true;
                }
            }
        }

		public virtual Vector2 Origin
        {
            get
            {
                return Properties.ToWorld.Origin;
            }

            set
            {
                if (Properties.ToWorld.Origin != value)
                {
                    Properties.ToWorld.Origin = value;
                    TransformChanged = true;
                }
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
                if (Math.Abs(Properties.ToWorld.Rotation - value) > 0.00001)
                {
                    Properties.ToWorld.Rotation = value;
                    TransformChanged = true;
                }
            }
        }

        public bool TransformChanged
        {
            get; protected set;
        }

        public bool WorldTransformChanged
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

        public Cv_SceneNode Parent
        {
            get; private set;
        }

        protected List<Cv_SceneNode> m_Children;
        protected Cv_EntityComponent m_Component;

        private string Name
        {
            get
            {
                 var entity = CaravelApp.Instance.GameLogic.GetEntity(Properties.EntityID);
                 var name = (entity != null ? entity.EntityName : "root") + "_" + this.GetType().Name;
                 return name;
            }
        }

        public Cv_SceneNode(Cv_EntityID entityID, Cv_EntityComponent renderComponent, Cv_Transform to, Cv_Transform from = null)
        {
            Properties = new Cv_NodeProperties();
            Properties.EntityID = entityID;
            Properties.ToWorld = to;
            Properties.FromWorld = from;
            Properties.Name = renderComponent != null ? renderComponent.GetType().Name : "SceneNode";
            Properties.Radius = -1;
            TransformChanged = true;
            m_Component = renderComponent;
            m_Children = new List<Cv_SceneNode>();
        }

        public virtual float GetRadius(Cv_Renderer renderer)
        {
            return Properties.Radius;
        }

        public virtual void SetRadius(float value)
        {
            Properties.Radius = value;
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

        public abstract void VPreRender(Cv_SceneElement scene, Cv_Renderer renderer);

        public abstract bool VIsVisible(Cv_SceneElement scene, Cv_Renderer renderer);

        public abstract void VRender(Cv_SceneElement scene, Cv_Renderer renderer);

        public abstract void VPostRender(Cv_SceneElement scene, Cv_Renderer renderer);

        public virtual void VFinishedRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            TransformChanged = false;

            foreach (var child in m_Children)
            {
                child.VFinishedRender(scene, renderer);
            }
        }

        public virtual void VRenderChildren(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            foreach (var child in m_Children)
            {
                child.VPreRender(scene, renderer);

                if (child.VIsVisible(scene, renderer))
                {
                    child.VRender(scene, renderer);
                    child.VRenderChildren(scene, renderer);
                }

                child.VPostRender(scene, renderer);
            }
        }

        public virtual bool AddChild(Cv_SceneNode child)
        {
            if (child != null)
            {
                m_Children.Add(child);
                child.Parent = this;

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

        public virtual bool VPick(Cv_SceneElement scene, Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities)
        {
            var success = false;
            foreach (var child in m_Children)
            {
                if (child.VPick(scene, renderer, screenPosition, entities))
                {
                    success = true;
                }
            }

            return success;
        }

        public void PrintTree(int level)
        {
            Console.WriteLine(Name);
            foreach(var n in m_Children)
            {
                for (var i = 0; i <= level; i++)
                {
                    Console.Write("\t");
                }
                n.PrintTree(level + 1);
            }
        }
    }
}