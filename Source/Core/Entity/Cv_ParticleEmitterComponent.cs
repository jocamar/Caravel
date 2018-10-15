using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Caravel.Core.Draw;
using Caravel.Debugging;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_ParticleEmitterComponent : Cv_RenderComponent
    {
         public string Texture
        {
            get; set;
        }

        public Vector2 EmitterVelocity
        {
            get; set;
        }

        public Vector2 EmitterVariation
        {
            get; set;
        }

        public Vector2 Gravity
        {
            get; set;
        }

        public int ParticlesPerSecond
        {
            get; set;
        }

        public float ParticleLifeTime
        {
            get; set;
        }

        public int MaxParticles
        {
            get; set;
        }

        public Color FinalColor
        {
            get; set;
        }

        public float ColorChangePoint
        {
            get; set;
        }

        public Vector2 InitialScale
        {
            get; set;
        }

        public Vector2 FinalScale
        {
            get; set;
        }

        public float ScaleChangePoint
        {
            get; set;
        }

        public float InitialRotation
        {
            get; set;
        }

        public float FinalRotation
        {
            get; set;
        }

        public float RotationChangePoint
        {
            get; set;
        }

        public Cv_ParticleEmitterComponent()
        {
            FinalColor = Color.White;
            ColorChangePoint = 0.5f;

            InitialScale = Vector2.One;
            FinalScale = Vector2.One;
            ScaleChangePoint = 0.5F;

            InitialRotation = 0;
            FinalRotation = 0;
            RotationChangePoint = 0.5f;

            Texture = "";

            EmitterVelocity = Vector2.Zero;
            EmitterVariation = Vector2.Zero;
            Gravity = Vector2.Zero;
            ParticlesPerSecond = 0;
            ParticleLifeTime = 0;
        }

        public override void VPostLoad()
        {
        }

        protected override Cv_SceneNode VCreateSceneNode()
        {
            return new Cv_ParticleEmitterNode(Owner.ID, this, Cv_Transform.Identity);
        }

        protected override bool VInheritedInit(XmlElement componentData)
        {
            Cv_Debug.Assert(componentData != null, "Must have valid component data.");

            var textureNode = componentData.SelectNodes("Texture").Item(0);
            if (textureNode != null)
            {
                Texture = textureNode.Attributes["resource"].Value;
            }

            var velocityNode = componentData.SelectNodes("EmitterVelocity").Item(0);
            if (velocityNode != null)
            {
                var x = int.Parse(velocityNode.Attributes["x"].Value);
                var y = int.Parse(velocityNode.Attributes["y"].Value);
                EmitterVelocity = new Vector2(x,y);
            }

            var variationNode = componentData.SelectNodes("EmitterVariation").Item(0);
            if (variationNode != null)
            {
                var x = int.Parse(variationNode.Attributes["x"].Value);
                var y = int.Parse(variationNode.Attributes["y"].Value);
                EmitterVariation = new Vector2(x,y);
            }

            var gravityNode = componentData.SelectNodes("Gravity").Item(0);
            if (gravityNode != null)
            {
                var x = int.Parse(gravityNode.Attributes["x"].Value);
                var y = int.Parse(gravityNode.Attributes["y"].Value);
                Gravity = new Vector2(x,y);
            }

            var particlesPerSecondNode = componentData.SelectNodes("ParticlesPerSecond").Item(0);
            if (particlesPerSecondNode != null)
            {
                ParticlesPerSecond = int.Parse(particlesPerSecondNode.Attributes["value"].Value);
            }

            var particleLifetimeNode = componentData.SelectNodes("ParticleLifetime").Item(0);
            if (particleLifetimeNode != null)
            {
                ParticleLifeTime = float.Parse(particleLifetimeNode.Attributes["value"].Value, CultureInfo.InvariantCulture);
            }

            var maxParticlesNode = componentData.SelectNodes("MaxParticles").Item(0);
            if (maxParticlesNode != null)
            {
                MaxParticles = int.Parse(maxParticlesNode.Attributes["value"].Value);
            }

            XmlElement finalColorNode = (XmlElement) componentData.SelectSingleNode("FinalColor");
            if (finalColorNode != null)
            {
                int r, g, b, a;

                r = int.Parse(finalColorNode.Attributes["r"].Value);
                g = int.Parse(finalColorNode.Attributes["g"].Value);
                b = int.Parse(finalColorNode.Attributes["b"].Value);

                a = 255;

                if (finalColorNode.Attributes["a"] != null)
                {
                    a = int.Parse(finalColorNode.Attributes["a"].Value);
                }

                FinalColor = new Color(r,g,b,a);
            }

            var colorMiddlePoint = componentData.SelectNodes("ColorEaseBias").Item(0);
            if (colorMiddlePoint != null)
            {
                var value = float.Parse(colorMiddlePoint.Attributes["value"].Value, CultureInfo.InvariantCulture);
                ColorChangePoint = (float) Math.Max(Math.Min(value, 1f), 0f);
            }

            var initialScaleNode = componentData.SelectNodes("InitialParticleScale").Item(0);
            if (initialScaleNode != null)
            {
                var x = float.Parse(initialScaleNode.Attributes["x"].Value, CultureInfo.InvariantCulture);
                var y = float.Parse(initialScaleNode.Attributes["y"].Value, CultureInfo.InvariantCulture);
                InitialScale = new Vector2(x,y);
            }

            var finalScaleNode = componentData.SelectNodes("FinalParticleScale").Item(0);
            if (finalScaleNode != null)
            {
                var x = float.Parse(finalScaleNode.Attributes["x"].Value, CultureInfo.InvariantCulture);
                var y = float.Parse(finalScaleNode.Attributes["y"].Value, CultureInfo.InvariantCulture);
                FinalScale = new Vector2(x,y);
            }

            var scaleMiddlePoint = componentData.SelectNodes("ScaleEaseBias").Item(0);
            if (scaleMiddlePoint != null)
            {
                var value = float.Parse(scaleMiddlePoint.Attributes["value"].Value, CultureInfo.InvariantCulture);
                ScaleChangePoint = (float) Math.Max(Math.Min(value, 1f), 0f);
            }

            var initialRotationNode = componentData.SelectNodes("InitialParticleRotation").Item(0);
            if (initialRotationNode != null)
            {
                InitialRotation = float.Parse(initialRotationNode.Attributes["value"].Value, CultureInfo.InvariantCulture);
            }

            var finalRotationNode = componentData.SelectNodes("FinalParticleRotation").Item(0);
            if (finalRotationNode != null)
            {
                FinalRotation = float.Parse(finalRotationNode.Attributes["value"].Value, CultureInfo.InvariantCulture);
            }

            var rotationMiddlePoint = componentData.SelectNodes("RotationEaseBias").Item(0);
            if (rotationMiddlePoint != null)
            {
                var value = float.Parse(rotationMiddlePoint.Attributes["value"].Value, CultureInfo.InvariantCulture);
                RotationChangePoint = (float) Math.Max(Math.Min(value, 1f), 0f);
            }

            return true;
        }

        protected override XmlElement VCreateInheritedElement(XmlElement baseElement)
        {
            var textureElement = baseElement.OwnerDocument.CreateElement("Texture");
            textureElement.SetAttribute("resource", Texture);
            baseElement.AppendChild(textureElement);

            var velocityElement = baseElement.OwnerDocument.CreateElement("EmitterVelocity");
            velocityElement.SetAttribute("x", EmitterVelocity.X.ToString(CultureInfo.InvariantCulture));
            velocityElement.SetAttribute("y", EmitterVelocity.Y.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(velocityElement);

            var variationElement = baseElement.OwnerDocument.CreateElement("EmitterVariation");
            variationElement.SetAttribute("x", EmitterVariation.X.ToString(CultureInfo.InvariantCulture));
            variationElement.SetAttribute("y", EmitterVariation.Y.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(variationElement);

            var gravityElement = baseElement.OwnerDocument.CreateElement("Gravity");
            gravityElement.SetAttribute("x", Gravity.X.ToString(CultureInfo.InvariantCulture));
            gravityElement.SetAttribute("y", Gravity.Y.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(gravityElement);

            var particlesPerSecondElement = baseElement.OwnerDocument.CreateElement("ParticlesPerSecond");
            particlesPerSecondElement.SetAttribute("value", ParticlesPerSecond.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(particlesPerSecondElement);

            var particlesLifetimeElement = baseElement.OwnerDocument.CreateElement("ParticleLifetime");
            particlesLifetimeElement.SetAttribute("value", ParticleLifeTime.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(particlesLifetimeElement);

            var maxParticlesElement = baseElement.OwnerDocument.CreateElement("MaxParticles");
            maxParticlesElement.SetAttribute("value", MaxParticles.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(maxParticlesElement);

            XmlElement finalColorElement = baseElement.OwnerDocument.CreateElement("FinalColor");
            finalColorElement.SetAttribute("r", FinalColor.R.ToString(CultureInfo.InvariantCulture));
            finalColorElement.SetAttribute("g", FinalColor.G.ToString(CultureInfo.InvariantCulture));
            finalColorElement.SetAttribute("b", FinalColor.B.ToString(CultureInfo.InvariantCulture));
            finalColorElement.SetAttribute("a", FinalColor.A.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(finalColorElement);

            var colorEaseBiasElement = baseElement.OwnerDocument.CreateElement("ColorEaseBias");
            colorEaseBiasElement.SetAttribute("value", ColorChangePoint.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(colorEaseBiasElement);

            var initialScaleElement = baseElement.OwnerDocument.CreateElement("InitialParticleScale");
            initialScaleElement.SetAttribute("x", InitialScale.X.ToString(CultureInfo.InvariantCulture));
            initialScaleElement.SetAttribute("y", InitialScale.Y.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(initialScaleElement);

            var finalScaleElement = baseElement.OwnerDocument.CreateElement("FinalParticleScale");
            finalScaleElement.SetAttribute("x", FinalScale.X.ToString(CultureInfo.InvariantCulture));
            finalScaleElement.SetAttribute("y", FinalScale.Y.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(finalScaleElement);

            var scaleEaseBiasElement = baseElement.OwnerDocument.CreateElement("ScaleEaseBias");
            scaleEaseBiasElement.SetAttribute("value", ScaleChangePoint.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(scaleEaseBiasElement);

            var initialRotationElement = baseElement.OwnerDocument.CreateElement("InitialParticleRotation");
            initialRotationElement.SetAttribute("value", InitialRotation.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(initialRotationElement);

            var finalRotationElement = baseElement.OwnerDocument.CreateElement("FinalParticleRotation");
            finalRotationElement.SetAttribute("value", FinalRotation.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(finalRotationElement);

            var rotationEaseBiasElement = baseElement.OwnerDocument.CreateElement("RotationEaseBias");
            rotationEaseBiasElement.SetAttribute("value", RotationChangePoint.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(rotationEaseBiasElement);

            return baseElement;
        }
    }
}