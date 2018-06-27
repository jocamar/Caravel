using System;
using System.Xml;
using Caravel.Core.Resource;
using Caravel.Debugging;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Entity.Cv_EntityComponent;

namespace Caravel.Core.Entity
{
    public class Cv_EntityFactory
    {
        protected GenericObjectFactory<Cv_EntityComponent, Cv_ComponentID> m_ComponentFactory;

        private Cv_EntityID m_lastEntityID = Cv_EntityID.INVALID_ENTITY;

        protected internal Cv_EntityFactory()
        {
            m_ComponentFactory = new GenericObjectFactory<Cv_EntityComponent, Cv_ComponentID>();

            m_ComponentFactory.Register<Cv_TransformComponent>(Cv_EntityComponent.GetID<Cv_TransformComponent>());
            m_ComponentFactory.Register<Cv_SpriteComponent>(Cv_EntityComponent.GetID<Cv_SpriteComponent>());
        }

        protected internal Cv_Entity CreateEntity(string entityTypeResource, XmlElement overrides, Cv_Transform initialTransform, Cv_EntityID serverEntityID)
        {
            var resource = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>(entityTypeResource);
            XmlElement root = ((Cv_XmlResource.Cv_XmlData) resource.ResourceData).RootNode;

            if (root == null)
            {
                Cv_Debug.Error("Failed to load entity resource file: " + entityTypeResource);
                return null;
            }

            Cv_EntityID entityId = serverEntityID;
            if (entityId == Cv_EntityID.INVALID_ENTITY)
            {
                entityId = GetNextEntityID();
            }

            var entity = new Cv_Entity(entityId);

            if (!entity.Init(root))
            {
                Cv_Debug.Error("Failed to initialize entity: " + entityTypeResource);
                return null;
            }

            foreach(var componentNode in root.ChildNodes)
            {
                var component = CreateComponent((XmlElement) componentNode);

                if (component != null)
                {
                    entity.AddComponent(component);
                }
                else {
                    return null;
                }
            }

            if (overrides != null)
            {
                ModifyEntity(entity, overrides);
            }

            var tranformComponent = entity.GetComponent<Cv_TransformComponent>();
            if (tranformComponent != null && initialTransform != null)
            {
                tranformComponent.Transform = initialTransform;
            }

            entity.PostInit();
            
            return entity;
        }

        protected internal void ModifyEntity(Cv_Entity entity, XmlElement overrides)
        {
            foreach (XmlElement componentNode in overrides.ChildNodes)
            {
                var componentID = Cv_EntityComponent.GetID(componentNode.Name);
                var component = entity.GetComponent(componentID);

                if (component != null)
                {
                    component.VInit(componentNode);
                    component.VOnChanged();
                }
                else
                {
                    component = CreateComponent(componentNode);
                    if (component != null)
                    {
                        entity.AddComponent(component);
                    }
                }
            }
        }

        protected Cv_EntityComponent CreateComponent(XmlElement componentData)
        {
            var component = m_ComponentFactory.Create(Cv_EntityComponent.GetID(componentData.Name));

            if (component != null)
            {
                if (!component.VInit(componentData))
                {
                    Cv_Debug.Error("Failed to initialize component: " + componentData.Name);
                    return null;
                }
            }
            else {
                Cv_Debug.Error("Couldn't find component " + componentData.Name + ". All components must be registered before use.");
            }

            return component;
        }

        private Cv_EntityID GetNextEntityID()
        {
            m_lastEntityID++;

            return m_lastEntityID;
        }
    }
}