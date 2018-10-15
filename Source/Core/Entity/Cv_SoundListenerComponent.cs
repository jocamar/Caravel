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

        public override bool VInitialize(XmlElement componentData)
        {
            return true;
        }

        public override void VOnChanged()
        {
        }

        public override void VOnDestroy()
        {
        }

        public override bool VPostInitialize()
        {
            var playerView = CaravelApp.Instance.GetPlayerView(PlayerIndex.One);

            playerView.ListenerEntity = Owner;
            return true;
        }

        public override void VPostLoad()
        {
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
        }
    }
}