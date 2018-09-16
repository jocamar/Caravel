using System.Xml;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_SoundListenerComponent : Cv_EntityComponent
    {
        public override XmlElement VToXML()
        {
            var componentDoc = new XmlDocument();
            var componentData = componentDoc.CreateElement(GetComponentName<Cv_SoundListenerComponent>());

            return componentData;
        }

        protected internal override bool VInitialize(XmlElement componentData)
        {
            return true;
        }

        protected internal override void VOnChanged()
        {
        }

        protected internal override void VOnDestroy()
        {
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
        }

        protected internal override bool VPostInitialize()
        {
            var playerView = CaravelApp.Instance.GetPlayerView(PlayerIndex.One);

            playerView.ListenerEntity = Owner;
            return true;
        }

        protected internal override void VPostLoad()
        {
        }
    }
}