using System;
using System.Collections.Generic;
using Caravel.Core.Entity;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Draw
{
    public class Cv_ParticleEmitterNode : Cv_SceneNode
    {
        private class Cv_Particle
        {
            public float TTL;
            public Cv_Transform Transform;
            public Vector2 Velocity;
            public bool IsAlive;
            public Vector2 InitialScale;
            public float InitialRotation;

            public Cv_Particle()
            {
                IsAlive = false;
            }
        }

        private LinkedList<Cv_Particle> m_Particles;
        private readonly int MAX_PARTICLES = 1024;
        private Random m_Random;
        private int m_iNumLiveParticles;

        public Cv_ParticleEmitterNode(Cv_Entity.Cv_EntityID entityID, Cv_EntityComponent renderComponent, Cv_Transform to, Cv_Transform? from = null) : base(entityID, renderComponent, to, from)
        {
            m_Particles = new LinkedList<Cv_Particle>();

            var particleComponent = (Cv_ParticleEmitterComponent) Component;
            for (var i = 0; i < particleComponent.MaxParticles && i < MAX_PARTICLES; i++)
            {
                m_Particles.AddFirst(new Cv_Particle());
            }

            m_iNumLiveParticles = 0;

            m_Random = new Random();
        }

        internal override float GetRadius(Cv_Renderer renderer)
        {
            if (Properties.Radius < 0)
            {
                var transf = Parent.Transform;
                var originFactorX = Math.Abs(transf.Origin.X - 0.5) + 0.5;
                var originFactorY = Math.Abs(transf.Origin.Y - 0.5) + 0.5;
                var originFactor = (float) Math.Max(originFactorX, originFactorY);

                var particleComponent = (Cv_ParticleEmitterComponent) Component;
                var particleMaxTravel = ((particleComponent.EmitterVelocity + particleComponent.Gravity*particleComponent.ParticleLifeTime) * particleComponent.ParticleLifeTime);

                var comp = ((Cv_ParticleEmitterComponent) Component);

                Properties.Radius = (float) Math.Sqrt((comp.Width+particleMaxTravel.Length())*(comp.Width+particleMaxTravel.Length()) + comp.Height*comp.Height) * originFactor;
                Properties.Radius *= Math.Max(transf.Scale.X, transf.Scale.Y);
            }

            return Properties.Radius;
        }

        internal override bool VIsVisible(Cv_Renderer renderer)
        {
            return true;
        }

        internal override void VPostRender(Cv_Renderer renderer)
        {
        }

        internal override void VPreRender(Cv_Renderer renderer)
        {
        }

        internal override void VRender(Cv_Renderer renderer)
        {
            var particleComponent = (Cv_ParticleEmitterComponent) Component;
            var scene = CaravelApp.Instance.Scene;

            particleComponent.DrawSelectionHighlight(renderer);

            if (!particleComponent.Visible || particleComponent.Texture == null || particleComponent.Texture == "")
            {
                return;
            }

            Cv_RawTextureResource resource = Cv_ResourceManager.Instance.GetResource<Cv_RawTextureResource>(particleComponent.Texture, particleComponent.Owner.ResourceBundle);
            var tex = resource.GetTexture().Texture;

            foreach (var particle in m_Particles)
            {
                if (!particle.IsAlive)
                {
                    break;
                }

                var layerDepth = (int) particle.Transform.Position.Z;
                layerDepth = (layerDepth % Cv_Renderer.MaxLayers);
                var layer = layerDepth / (float) Cv_Renderer.MaxLayers;

                var newColorVec = EaseValue(new Vector4(particleComponent.Color.R, particleComponent.Color.G, particleComponent.Color.B, particleComponent.Color.A),
                                            new Vector4(particleComponent.FinalColor.R, particleComponent.FinalColor.G, particleComponent.FinalColor.B, particleComponent.FinalColor.A),
                                                particleComponent.ParticleLifeTime,
                                                particleComponent.ParticleLifeTime - particle.TTL,
                                                particleComponent.ColorChangePoint);
                var newColor = new Color((int) newColorVec.X, (int) newColorVec.Y, (int) newColorVec.Z, (int) newColorVec.W);

                renderer.Draw(tex, new Rectangle((int) (particle.Transform.Position.X),
                                                    (int) (particle.Transform.Position.Y),
                                                    (int) (tex.Width * particle.Transform.Scale.X),
                                                    (int) (tex.Height * particle.Transform.Scale.Y)),
                                        null,
                                        newColor,
                                        particle.Transform.Rotation,
                                        new Vector2(tex.Width * 0.5f, tex.Height * 0.5f),
                                        SpriteEffects.None,
                                        layer);
            }
        }

        internal override bool VPick(Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities)
        {
            var particleComponent = (Cv_ParticleEmitterComponent) Component;
            return particleComponent.Pick(renderer, screenPosition, entities);
        }

        internal override void VOnUpdate(float time, float elapsedTime)
        {
            var worldTransform = WorldTransform;
            GenerateParticles(elapsedTime, worldTransform);
            
            var particleComponent = (Cv_ParticleEmitterComponent) Component;

            foreach (var particle in m_Particles)
            {
                if (!particle.IsAlive)
                {
                    break;
                }

                particle.TTL -= elapsedTime;

                if (particle.TTL <= 0)
                {
                    particle.IsAlive = false;
                    m_iNumLiveParticles--;
                }
                else
                {
                    particle.Velocity += elapsedTime * particleComponent.Gravity * particle.InitialScale / 1000;

                    var newPos = particle.Transform.Position + elapsedTime*new Vector3(particle.Velocity, 0)/1000;

                    var newScale = EaseValue(particleComponent.InitialScale * particle.InitialScale,
                                                particleComponent.FinalScale * particle.InitialScale,
                                                particleComponent.ParticleLifeTime,
                                                particleComponent.ParticleLifeTime - particle.TTL,
                                                particleComponent.ScaleChangePoint);

                    var newRotation = EaseValue(particleComponent.InitialRotation + particle.InitialRotation,
                                                particleComponent.FinalRotation + particle.InitialRotation,
                                                particleComponent.ParticleLifeTime,
                                                particleComponent.ParticleLifeTime - particle.TTL,
                                                particleComponent.RotationChangePoint);

                    particle.Transform = new Cv_Transform(newPos, newScale, newRotation);
                }
            }
        }

        private void GenerateParticles(float elapsedTime, Cv_Transform worldTransform)
        {
            var particleComponent = (Cv_ParticleEmitterComponent) Component;
            var numParticles = GetNumParticlesToGenerate(elapsedTime);

            while (numParticles > 0 && m_iNumLiveParticles < MAX_PARTICLES && m_iNumLiveParticles < particleComponent.MaxParticles)
            {
                m_iNumLiveParticles++;
                numParticles--;

                var particle = m_Particles.Last.Value;
                m_Particles.RemoveLast();

                particle.TTL = particleComponent.ParticleLifeTime;

                var variation_x = m_Random.Next((int) -particleComponent.EmitterVariation.X, (int) particleComponent.EmitterVariation.X);
                var variation_y = m_Random.Next((int) -particleComponent.EmitterVariation.Y, (int) particleComponent.EmitterVariation.Y);
                particle.Velocity = (particleComponent.EmitterVelocity + new Vector2(variation_x, variation_y)) * worldTransform.Scale;

                particle.IsAlive = true;

                var pos_x = worldTransform.Position.X + m_Random.Next(0, (int)(particleComponent.Width*worldTransform.Scale.X));
                var pos_y = worldTransform.Position.Y + m_Random.Next(0, (int)(particleComponent.Height*worldTransform.Scale.Y));
                pos_x -= (int)(particleComponent.Width*worldTransform.Scale.X*worldTransform.Origin.X);
                pos_y -= (int)(particleComponent.Height*worldTransform.Scale.Y*worldTransform.Origin.Y);
                
                var rotation = particleComponent.InitialRotation + worldTransform.Rotation;
                var scale = particleComponent.InitialScale * worldTransform.Scale;

                particle.Transform = new Cv_Transform(new Vector3(pos_x, pos_y, worldTransform.Position.Z), scale, rotation);
                particle.InitialScale = worldTransform.Scale;
                particle.InitialRotation = worldTransform.Rotation;
                m_Particles.AddFirst(particle);
            }
        }

        private double RandomBiasedPow(double min, double max, int tightness, double peak)
        {
            // Calculate skewed normal distribution, skewed by Math.Pow(...), specifiying where in the range the peak is
            // NOTE: This peak will yield unreliable results in the top 20% and bottom 20% of the range.
            //       To peak at extreme ends of the range, consider using a different bias function

            double total = 0.0;
            double scaledPeak = peak / (max - min) + min;

            double exp = GetExp(scaledPeak);

            for (int i = 1; i <= tightness; i++)
            {
                // Bias the random number to one side or another, but keep in the range of 0 - 1
                // The exp parameter controls how far to bias the peak from normal distribution
                total += BiasPow(m_Random.NextDouble(), exp);
            }

            return ((total / tightness) * (max - min)) + min;
        }

        private double GetExp(double peak)
        {
            // Get the exponent necessary for BiasPow(...) to result in the desired peak 
            // Based on empirical trials, and curve fit to a cubic equation, using WolframAlpha
            return -11.7588 * Math.Pow(peak, 3) + 27.3205 * Math.Pow(peak, 2) - 21.2365 * peak + 6.31735;
        }

        private double BiasPow(double input, double exp)
        {
            return Math.Pow(input, exp);
        }

        private int GetNumParticlesToGenerate(float elapsedTime)
        {
            var component = (Cv_ParticleEmitterComponent) Component;
            var totalParticles = (int) Math.Round((Math.Max(RandomBiasedPow(0, 3, 100, (elapsedTime*component.ParticlesPerSecond/900f)), 0) - 0.4f)*1.4f);

            return totalParticles;
        }

        private float EaseValue(float initialValue, float finalValue, float totalTime, float elapsedTime, float middlePoint)
        {
            var power = Math.Log(1/middlePoint) / Math.Log(2);
            float factor = (float) Math.Pow(elapsedTime / totalTime, power);
            return initialValue + (finalValue - initialValue) * factor;
        }

        private Vector2 EaseValue(Vector2 initialValue, Vector2 finalValue, float totalTime, float elapsedTime, float middlePoint)
        {
            return new Vector2(EaseValue(initialValue.X, finalValue.X, totalTime, elapsedTime, middlePoint),
                                EaseValue(initialValue.Y, finalValue.Y, totalTime, elapsedTime, middlePoint));
        }

        private Vector4 EaseValue(Vector4 initialValue, Vector4 finalValue, float totalTime, float elapsedTime, float middlePoint)
        {
            return new Vector4(EaseValue(initialValue.X, finalValue.X, totalTime, elapsedTime, middlePoint),
                                EaseValue(initialValue.Y, finalValue.Y, totalTime, elapsedTime, middlePoint),
                                EaseValue(initialValue.Z, finalValue.Z, totalTime, elapsedTime, middlePoint),
                                EaseValue(initialValue.W, finalValue.W, totalTime, elapsedTime, middlePoint));
        }
    }
}