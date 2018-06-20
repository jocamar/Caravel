using System;
using System.Collections.Generic;

namespace Caravel.Core.Resource
{
    public abstract class Cv_Resource
    {
        public enum Cv_ResourceType
        {
            INVALID_RESOURCE = 0
        }

        public string File
        {
            get; internal set;
        }

        public Cv_ResourceType Type
        {
            get
            {
                if (m_Type == Cv_ResourceType.INVALID_RESOURCE)
                {
                    m_Type = GetResType(GetType());
                }

                return m_Type;
            }
        }

        public int Size
        {
            get; internal set;
        }

        private static Dictionary<Type, Cv_ResourceType> m_ResourceTypes = new Dictionary<Type, Cv_ResourceType>();
        private Cv_ResourceType m_Type = Cv_ResourceType.INVALID_RESOURCE;

        public Cv_Resource()
        {
            Size = 0;
        }

        ~Cv_Resource()
        {

        }

        public abstract bool VLoad(out int size);

        public static Cv_ResourceType GetResType<Resource>() where Resource : Cv_Resource
        {
            Cv_ResourceType resType;
            var resourceType = typeof(Resource);
            if (!m_ResourceTypes.TryGetValue(resourceType, out resType))
            {
                resType = (Cv_ResourceType) resourceType.Name.GetHashCode();
                m_ResourceTypes.Add(resourceType, resType);
            }

            return resType;
        }

        internal static Cv_ResourceType GetResType(Type resourceType)
        {
            Cv_ResourceType resType;
            if (!m_ResourceTypes.TryGetValue(resourceType, out resType))
            {
                resType = (Cv_ResourceType) resourceType.Name.GetHashCode();
                m_ResourceTypes.Add(resourceType, resType);
            }

            return resType;
        }
    }
}