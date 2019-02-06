using System.Xml;
using Caravel.Core.Resource;
using Caravel.Debugging;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Entity.Cv_EntityComponent;

namespace Caravel.Core.Entity
{
    public class Cv_EntityFactory
    {
        protected GenericObjectFactory<Cv_EntityComponent, Cv_ComponentID> ComponentFactory;

        private Cv_EntityID m_lastEntityID = Cv_EntityID.INVALID_ENTITY;
        private object m_Mutex;

        protected internal Cv_EntityFactory()
        {
            m_Mutex = new object();
            ComponentFactory = new GenericObjectFactory<Cv_EntityComponent, Cv_ComponentID>();

            ComponentFactory.Register<Cv_TransformComponent>(Cv_EntityComponent.GetID<Cv_TransformComponent>());
            ComponentFactory.Register<Cv_SpriteComponent>(Cv_EntityComponent.GetID<Cv_SpriteComponent>());
            ComponentFactory.Register<Cv_CameraComponent>(Cv_EntityComponent.GetID<Cv_CameraComponent>());
            ComponentFactory.Register<Cv_RigidBodyComponent>(Cv_EntityComponent.GetID<Cv_RigidBodyComponent>());
            ComponentFactory.Register<Cv_SoundEmitterComponent>(Cv_EntityComponent.GetID<Cv_SoundEmitterComponent>());
            ComponentFactory.Register<Cv_SoundListenerComponent>(Cv_EntityComponent.GetID<Cv_SoundListenerComponent>());
            ComponentFactory.Register<Cv_ScriptComponent>(Cv_EntityComponent.GetID<Cv_ScriptComponent>());
            ComponentFactory.Register<Cv_ClickableComponent>(Cv_EntityComponent.GetID<Cv_ClickableComponent>());
            ComponentFactory.Register<Cv_TextComponent>(Cv_EntityComponent.GetID<Cv_TextComponent>());
            ComponentFactory.Register<Cv_TransformAnimationComponent>(Cv_EntityComponent.GetID<Cv_TransformAnimationComponent>());
            ComponentFactory.Register<Cv_ParticleEmitterComponent>(Cv_EntityComponent.GetID<Cv_ParticleEmitterComponent>());
        }

        virtual protected internal Cv_Entity CreateEntity(string entityTypeResource, Cv_EntityID parent,
                                                    XmlElement overrides, Cv_Transform? initialTransform,
                                                    Cv_EntityID serverEntityID, string resourceBundle, string sceneID)
        {
            Cv_XmlResource resource;
			resource = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>(entityTypeResource, resourceBundle, CaravelApp.Instance.EditorRunning);

            XmlElement root = ((Cv_XmlResource.Cv_XmlData) resource.ResourceData).RootNode;

            if (root == null)
            {
                Cv_Debug.Error("Failed to load entity resource file: " + entityTypeResource);
                return null;
            }

            Cv_EntityID entityId = serverEntityID;
            if (entityId == Cv_EntityID.INVALID_ENTITY)
            {
                lock(m_Mutex)
                {
                    entityId = GetNextEntityID();
                }
            }

            var entity = new Cv_Entity(entityId, resourceBundle, sceneID);

            if (!entity.Initialize(entityTypeResource, root, parent))
            {
                Cv_Debug.Error("Failed to initialize entity: " + entityTypeResource);
                return null;
            }

            foreach(var componentNode in root.ChildNodes)
            {
				if (componentNode.GetType() != typeof(XmlElement))
				{
					continue;
				}

                var component = CreateComponent((XmlElement) componentNode);

                if (component != null)
                {
                    entity.AddComponent(component);
                }
                else {
                    return null;
                }
            }
            
            return entity;
        }

        protected internal Cv_Entity CreateEmptyEntity(string resourceBundle, string sceneID, Cv_EntityID parent, XmlElement overrides,
                                                                Cv_Transform? initialTransform, Cv_EntityID serverEntityID)
        {
            Cv_EntityID entityId = serverEntityID;
            if (entityId == Cv_EntityID.INVALID_ENTITY)
            {
                lock(m_Mutex)
                {
                    entityId = GetNextEntityID();
                }
            }

            var entity = new Cv_Entity(entityId, resourceBundle, sceneID);

            if (!entity.Initialize(null, null, parent))
            {
                Cv_Debug.Error("Failed to initialize empty entity.");
                return null;
            }
            
            return entity;
        }

        virtual protected internal void ModifyEntity(Cv_Entity entity, XmlNodeList overrides)
        {
            foreach (XmlElement componentNode in overrides)
            {
                var componentID = Cv_EntityComponent.GetID(componentNode.Name);
                var component = entity.GetComponent(componentID);

                if (component != null)
                {
                    component.VInitialize(componentNode);
                    component.VOnChanged();
                }
                else
                {
                    component = CreateComponent(componentNode);
                    if (component != null)
                    {
                        entity.AddComponent(component);
                        component.VPostInitialize();
                    }
                }
            }
        }

        virtual protected internal Cv_EntityComponent CreateComponent(string componentName)
        {
            var component = ComponentFactory.Create(Cv_EntityComponent.GetID(componentName));

            if (component == null)
            {
                Cv_Debug.Error("Couldn't find component " + componentName + ". All components must be registered before use.");
            }

            return component;
        }

        virtual protected internal Component CreateComponent<Component>() where Component : Cv_EntityComponent
        {
            var component = (Component) ComponentFactory.Create(Cv_EntityComponent.GetID(typeof(Component)));

            if (component == null)
            {
                Cv_Debug.Error("Couldn't find component " + typeof(Component).Name + ". All components must be registered before use.");
            }

            return component;
        }

        virtual protected Cv_EntityComponent CreateComponent(XmlElement componentData)
        {
            var component = ComponentFactory.Create(Cv_EntityComponent.GetID(componentData.Name));

            if (component != null)
            {
                if (!component.VInitialize(componentData))
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