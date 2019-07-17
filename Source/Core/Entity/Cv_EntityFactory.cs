using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Caravel.Core.Resource;
using Caravel.Debugging;
using static Caravel.Core.Cv_SceneManager;
using static Caravel.Core.Entity.Cv_Entity;
using static Caravel.Core.Entity.Cv_EntityComponent;

namespace Caravel.Core.Entity
{
    public class Cv_EntityFactory
    {
        protected GenericObjectFactory<Cv_EntityComponent, Cv_ComponentID> ComponentFactory;

        private Cv_EntityID m_lastEntityID = Cv_EntityID.INVALID_ENTITY;
        private object m_Mutex;
        private Dictionary<Cv_ComponentID, XmlElement> m_GameComponentInfo;

        internal XmlElement GetComponentInfo(Cv_ComponentID componentID)
        {
            XmlElement info;

            if (m_GameComponentInfo.TryGetValue(componentID, out info))
            {
                return info;
            }

            return null;
        }

        protected internal Cv_EntityFactory()
        {
            m_Mutex = new object();
            m_GameComponentInfo = new Dictionary<Cv_ComponentID, XmlElement>();
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

            RegisterGameComponents();
        }

        virtual protected internal Cv_Entity CreateEntity(string entityTypeResource, Cv_EntityID parent, Cv_EntityID serverEntityID, string resourceBundle, Cv_SceneID sceneID, string sceneName)
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

            var entity = new Cv_Entity(entityId, resourceBundle, sceneName, sceneID);

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

        protected internal Cv_Entity CreateEmptyEntity(Cv_EntityID parent, Cv_EntityID serverEntityID, string resourceBundle, Cv_SceneID sceneID, string sceneName)
        {
            Cv_EntityID entityId = serverEntityID;
            if (entityId == Cv_EntityID.INVALID_ENTITY)
            {
                lock(m_Mutex)
                {
                    entityId = GetNextEntityID();
                }
            }

            var entity = new Cv_Entity(entityId, resourceBundle, sceneName, sceneID);

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

        private void RegisterGameComponents()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(CaravelApp.Instance.GetGameWorkingDirectory(), CaravelApp.Instance.ComponentDescriptionLocation));
            var root = doc.DocumentElement;

            if (root != null)
            {
                foreach (XmlElement elem in root.ChildNodes)
                {
                    string name = elem.Attributes["name"].Value;
                    string nameSpace = elem.Attributes["namespace"].Value;

                    if (name != null && name != "" && nameSpace != null) 
                    {
                        var assembly = Assembly.GetEntryAssembly();
                        var componentType = assembly.GetType(nameSpace + "." + name);
                        var method = ComponentFactory.GetType().GetMethods().Single(m => m.Name == "Register" && m.IsGenericMethodDefinition);
                        method = method.MakeGenericMethod(componentType);
                        object[] arguments = { Cv_EntityComponent.GetID(name) };
                        method.Invoke(ComponentFactory, arguments);

                        m_GameComponentInfo[Cv_EntityComponent.GetID(name)] = elem;
                    }
                }
            }
        }
    }
}