using System.Xml;

namespace Caravel.Core.Entity
{
    public class Cv_RenderComponent : Cv_EntityComponent
    {
        protected internal override bool VInit(XmlElement componentData)
        {
            throw new System.NotImplementedException();
        }

        protected internal override void VOnChanged()
        {
        }

        protected internal override void VOnUpdate(float deltaTime)
        {
        }

        protected internal override bool VPostInit()
        {
            return true;
        }

        protected internal override XmlElement VToXML()
        {
            throw new System.NotImplementedException();
        }
    }
}