﻿using System;
using System.Collections.Generic;
using System.Xml;

namespace Caravel.Core.Entity
{
    public abstract class Cv_EntityComponent
    {
        public enum Cv_ComponentID
        {
            INVALID_COMPONENT = 0
        }

        public Cv_ComponentID ID
        {
            get
            {
                if (m_ID == Cv_ComponentID.INVALID_COMPONENT)
                {
                    m_ID = GetID(GetType());
                }

                return m_ID;
            }
        }

        protected internal Cv_Entity Owner
        {
            get; internal set;
        }

        private static Dictionary<Type, Cv_ComponentID> m_ComponentIds = new Dictionary<Type, Cv_ComponentID>();
        private Cv_ComponentID m_ID = Cv_ComponentID.INVALID_COMPONENT;

        public static Cv_ComponentID GetID<ComponentType>() where ComponentType : Cv_EntityComponent
        {
            lock(m_ComponentIds)
            {
                Cv_ComponentID componentID;
                if (!m_ComponentIds.TryGetValue(typeof(ComponentType), out componentID))
                {
                    componentID = (Cv_ComponentID) typeof(ComponentType).Name.GetHashCode();
                    m_ComponentIds.Add(typeof(ComponentType), componentID);
                }

                return componentID;
            }
        }

        public static string GetComponentName<ComponentType>() where ComponentType : Cv_EntityComponent
        {
            return typeof(ComponentType).Name;
        }

        public static string GetComponentName(Cv_EntityComponent component)
        {
            return component.GetType().Name;
        }

        public abstract XmlElement VToXML();

        protected internal abstract bool VInitialize(XmlElement componentData);

        protected internal abstract bool VPostInitialize();

		protected internal abstract void VPostLoad();

        protected internal abstract void VOnUpdate(float deltaTime);

        protected internal abstract void VOnChanged();

        protected internal abstract void VOnDestroy();

        internal static Cv_ComponentID GetID(string componentName)
        {
            return (Cv_ComponentID) componentName.GetHashCode();
        }

        internal static Cv_ComponentID GetID(Type componentType)
        {
            lock(m_ComponentIds)
            {
                Cv_ComponentID componentID;
                if (!m_ComponentIds.TryGetValue(componentType, out componentID))
                {
                    componentID = (Cv_ComponentID) componentType.Name.GetHashCode();
                    m_ComponentIds.Add(componentType, componentID);
                }

                return componentID;
            }
        }
    }
}
