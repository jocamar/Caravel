using System.Collections.Generic;

namespace Caravel.Core
{
    public class GenericObjectFactory<BaseObjectType, IDType>
    {
        private delegate BaseObjectType ObjectConstructionDelegate();
        private Dictionary<IDType, ObjectConstructionDelegate> m_Constructors;

        public GenericObjectFactory()
        {
            m_Constructors = new Dictionary<IDType, ObjectConstructionDelegate>();
        }

        public bool Register<SubObjectType> (IDType id) where SubObjectType : BaseObjectType, new()
        {
            if (!m_Constructors.ContainsKey(id))
            {
                m_Constructors.Add(id, ObjectConstructor<SubObjectType>);
                return true;
            }

            return false;
        }

        public BaseObjectType Create(IDType id)
        {
            ObjectConstructionDelegate constructor = null;

            if (m_Constructors.TryGetValue(id, out constructor))
            {
                return constructor();
            }

            return default(BaseObjectType);
        }

        private BaseObjectType ObjectConstructor<SubObjectType>() where SubObjectType : BaseObjectType, new()
        {
            return new SubObjectType();
        }
    }
}