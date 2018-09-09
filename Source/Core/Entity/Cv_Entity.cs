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

        public string EntityTypeResource
        {
            get; private set;
        }

        public Cv_EntityID Parent
        {
            get; private set;
        }

		public string ResourceBundle
		{
			get; private set;
		}

        public bool DestroyRequested
        {
            get; internal set;
        }

        public bool Visible
        {
            get; set;
        }

        public Cv_Entity[] Children
        {
            get
            {
                return m_Children.ToArray();
            }
        }

        private Dictionary<Cv_ComponentID, Cv_EntityComponent> m_ComponentMap;
        private List<Cv_EntityComponent> m_ComponentList;
        private List<Cv_EntityComponent> m_ComponentsToAdd;
        private List<Cv_EntityComponent> m_ComponentsToRemove;
        private List<Cv_Entity> m_Children;

        public XmlElement ToXML()
        {
            XmlDocument doc = new XmlDocument();
            var entityElement = doc.CreateElement("Entity");
            entityElement.SetAttribute("name", EntityName);
            entityElement.SetAttribute("type", EntityTypeResource);
            entityElement.SetAttribute("visible", Visible.ToString());

            // components
            foreach (var component in m_ComponentMap.Values)
            {
                var componentElement = component.VToXML();

                var importedComponentNode = doc.ImportNode(componentElement, true);
                entityElement.AppendChild(importedComponentNode);
            }

            doc.AppendChild(entityElement);

            return entityElement;
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

        public Cv_EntityComponent GetComponent(string componentName)
        {
            Cv_EntityComponent component;
            Cv_ComponentID componentID = Cv_EntityComponent.GetID(componentName);

            if (m_ComponentMap.TryGetValue(componentID, out component))
            {
                return component;
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

        internal Cv_Entity(string resourceBundle)
        {
            ID = Cv_EntityID.INVALID_ENTITY;
            EntityType = "Unknown";
            EntityTypeResource = "Unknown";
			EntityName = "Unknown_" + ID;
            m_ComponentMap = new Dictionary<Cv_ComponentID, Cv_EntityComponent>();
            m_ComponentList = new List<Cv_EntityComponent>();
            m_ComponentsToAdd = new List<Cv_EntityComponent>();
            m_ComponentsToRemove = new List<Cv_EntityComponent>();
			ResourceBundle = resourceBundle;
            DestroyRequested = false;
            Visible = true;
        }

        internal Cv_Entity(Cv_EntityID entityId, string resourceBundle)
        {
            ID = entityId;
            EntityType = "Unknown";
            EntityTypeResource = "Unknown";
			EntityName = "Unknown_" + ID;
            m_ComponentMap = new Dictionary<Cv_ComponentID, Cv_EntityComponent>();
            m_ComponentList = new List<Cv_EntityComponent>();
            m_ComponentsToAdd = new List<Cv_EntityComponent>();
            m_ComponentsToRemove = new List<Cv_EntityComponent>();
			ResourceBundle = resourceBundle;
            DestroyRequested = false;
            Visible = true;
            m_Children = new List<Cv_Entity>();
        }

        ~Cv_Entity()
        {
            Cv_Debug.Log("Entity", "Destroying entity " + (int) ID);
        }

        internal bool Initialize(string typeResource, XmlElement typeData, Cv_EntityID parent = Cv_EntityID.INVALID_ENTITY)
        {
            if (typeResource != null)
            {
                EntityTypeResource = typeResource;
            }

            if (typeData != null)
            {
                EntityType = typeData.Attributes["type"].Value;
            }
            else
            {
                EntityType = "Unknown";
            }

            Parent = parent;
            Cv_Debug.Log("Entity", "Initializing entity " + (int) ID + " of type " + EntityType);
            return true;
        }

        internal void PostInitialize()
        {
            var parent = CaravelApp.Instance.Logic.GetEntity(Parent);

            if (parent != null)
            {
                parent.AddChild(this);
            }

            foreach(var component in m_ComponentMap)
            {
                component.Value.VPostInitialize();
            }
        }

		internal void PostLoad()
        {
            foreach(var component in m_ComponentMap)
            {
                component.Value.VPostLoad();
            }
        }

        internal void OnUpdate(float elapsedTime)
        {
            if (DestroyRequested)
            {
                return;
            }

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
                component.VOnUpdate(elapsedTime);
            }
        }

        internal void OnDestroy()
        {
            foreach (var component in m_ComponentList)
            {
                component.VOnDestroy();
                component.Owner = null;
            }
        }

        internal void OnRemove()
        {
            m_ComponentList.Clear();
            m_ComponentMap.Clear();
            m_ComponentsToAdd.Clear();
            m_ComponentsToRemove.Clear();
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
                component.VOnDestroy();
                component.Owner = null;
            }
        }

        internal void RemoveComponent(string componentType)
        {
            var component = GetComponent(componentType);

            if (component != null)
            {
                m_ComponentsToRemove.Add(component);
                m_ComponentMap.Remove(Cv_EntityComponent.GetID(componentType));
                component.VOnDestroy();
                component.Owner = null;
            }
        }

        internal void AddChild(Cv_Entity entity)
        {
            m_Children.Add(entity);
            entity.Parent = ID;
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
                component.VOnDestroy();
                component.Owner = null;
            }
        }
    }
}
