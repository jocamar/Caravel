using System.Collections.Generic;
using System.Xml;
using Caravel.Core.Draw;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_ParticleEmitterComponent : Cv_RenderComponent
    {
        public Color InitialColor
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

        public Cv_ParticleEmitterComponent()
        {
            InitialColor = Color.White;
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

        protected override Cv_SceneNode VCreateSceneNode()
        {
            return new Cv_ParticleEmitterNode(Owner.ID, this, Cv_Transform.Identity);
        }

        protected override bool VInheritedInit(XmlElement componentData)
        {
            throw new System.NotImplementedException();
        }

        protected override XmlElement VCreateInheritedElement(XmlElement baseElement)
        {
            throw new System.NotImplementedException();
        }

        protected internal override void VPostLoad()
        {
        }
    }
}