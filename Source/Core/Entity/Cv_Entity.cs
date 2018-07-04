using System;
using System.Collections.Generic;
using System.Xml;
using Caravel.Debugging;
using static Caravel.Core.Entity.Cv_EntityComponent;

namespace Caravel.Core.Entity
{
    public class Cv_Entity
    {
        public enum Cv_EntityID
        {
            INVALID_ENTITY = 0
        }

		public string EntityName
		{
			get; internal set;
		}

        public Cv_EntityID ID
        {
            get; private set;
        }

        public string EntityType
        {
            get; private set;
        }

        public Cv_EntityID Parent
        {
            get; private set;
        }

        private Dictionary<Cv_ComponentID, Cv_EntityComponent> m_ComponentMap;
        private List<Cv_EntityComponent> m_ComponentList;
        private List<Cv_EntityComponent> m_ComponentsToAdd;
        private List<Cv_EntityComponent> m_ComponentsToRemove;

        public XmlElement ToXML()
        {
            return null;
        }

        public Component GetComponent<Component>() where Component : Cv_EntityComponent
        {
            Cv_EntityComponent component;
            Cv_ComponentID componentID = Cv_EntityComponent.GetID<Component>();

            if (m_ComponentMap.TryGetValue(componentID, out component))
            {
                return (Component) component;
            }

            return null;
        }

        public Cv_EntityComponent GetComponent(Cv_ComponentID componentID)
        {
            Cv_EntityComponent component;

            if (m_ComponentMap.TryGetValue(componentID, out component))
            {
                return component;
            }

            return null;
        }

        internal Cv_Entity()
        {
            ID = Cv_EntityID.INVALID_ENTITY;
            EntityType = "Unknown";
			EntityName = "Unknown_" + ID;
            m_ComponentMap = new Dictionary<Cv_ComponentID, Cv_EntityComponent>();
            m_ComponentList = new List<Cv_EntityComponent>();
            m_ComponentsToAdd = new List<Cv_EntityComponent>();
            m_ComponentsToRemove = new List<Cv_EntityComponent>();
        }

        internal Cv_Entity(Cv_EntityID entityId)
        {
            ID = entityId;
            EntityType = "Unknown";
			EntityName = "Unknown_" + ID;
            m_ComponentMap = new Dictionary<Cv_ComponentID, Cv_EntityComponent>();
            m_ComponentList = new List<Cv_EntityComponent>();
            m_ComponentsToAdd = new List<Cv_EntityComponent>();
            m_ComponentsToRemove = new List<Cv_EntityComponent>();
        }

        ~Cv_Entity()
        {
            Cv_Debug.Log("Entity", "Destroying entity " + (int) ID);
        }

        internal bool Init(XmlElement typeData, Cv_EntityID parent = Cv_EntityID.INVALID_ENTITY)
        {
            EntityType = typeData.Attributes["type"].Value;
            Parent = parent;
            Cv_Debug.Log("Entity", "Initializing entity " + (int) ID + " of type " + EntityType);
            return true;
        }

        internal void PostInit()
        {
            foreach(var component in m_ComponentMap)
            {
                component.Value.VPostInit();
            }
        }

		internal void PostLoad()
        {
            foreach(var component in m_ComponentMap)
            {
                component.Value.VPostLoad();
            }
        }

        internal void OnUpdate(float timeElapsed)
        {
            foreach (var component in m_ComponentsToAdd)
            {
                m_ComponentList.Add(component);
            }
            m_ComponentsToAdd.Clear();

            foreach (var component in m_ComponentsToRemove)
            {
                m_ComponentList.Remove(component);
            }
            m_ComponentsToRemove.Clear();

            foreach (var component in m_ComponentList)
            {
                component.VOnUpdate(timeElapsed);
            }
        }

        internal void AddComponent(Cv_EntityComponent component)
        {
            RemoveComponent(component.GetType());

            m_ComponentMap.Add(component.ID, component);
            m_ComponentsToAdd.Add(component);
            component.Owner = this;
        }

        internal void RemoveComponent<Component>() where Component : Cv_EntityComponent
        {
            var component = GetComponent<Component>();

            if (component != null)
            {
                m_ComponentsToRemove.Add(component);
                m_ComponentMap.Remove(Cv_EntityComponent.GetID<Component>());
                component.Owner = null;
            }
        }

        private Cv_EntityComponent GetComponent(Type componentType)
        {
            Cv_EntityComponent component;
            if (m_ComponentMap.TryGetValue(Cv_EntityComponent.GetID(componentType), out component))
            {
                return component;
            }

            return null;
        }

        private void RemoveComponent(Type componentType)
        {
            var component = GetComponent(componentType);

            if (component != null)
            {
                m_ComponentsToRemove.Add(component);
                m_ComponentMap.Remove(Cv_EntityComponent.GetID(componentType));
            }
        }
    }
}
