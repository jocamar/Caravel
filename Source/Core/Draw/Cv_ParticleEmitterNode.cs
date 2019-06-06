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
            for (var i = 0; i < MAX_PARTICLES; i++)
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
                var particleMaxTravel = ((particleComponent.EmitterVelocity.Length() + particleComponent.Gravity.Length()*particleComponent.ParticleLifeTime/1000) * particleComponent.ParticleLifeTime/1000);
                particleMaxTravel *= 1.5f;

                var comp = ((Cv_ParticleEmitterComponent) Component);

                Properties.Radius = (float) Math.Sqrt((comp.Width+particleMaxTravel)*(comp.Width+particleMaxTravel) + comp.Height*comp.Height) * originFactor;
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

                var pos = particle.Transform.Position;
                var scale = particle.Transform.Scale;

                var camTransf = scene.Camera.GetViewTransform(renderer.VirtualWidth, renderer.VirtualHeight, Cv_Transform.Identity);

                if (particleComponent.Parallax != 1)
                {
                    var zoomFactor = ((1 + ((scene.Camera.Zoom - 1) * particleComponent.Parallax)) / scene.Camera.Zoom);
                    scale = scale * zoomFactor; //Magic formula
                    pos += ((particleComponent.Parallax - 1) * new Vector3(camTransf.Position.X, camTransf.Position.Y, 0));
                    pos += ((new Vector3(scene.Transform.Position.X, scene.Transform.Position.Y, 0)) * (1 - zoomFactor) * (particleComponent.Parallax - 1));
                }

                renderer.Draw(tex, new Rectangle((int) (pos.X),
                                                    (int) (pos.Y),
                                                    (int) (tex.Width * scale.X),
                                                    (int) (tex.Height * scale.Y)),
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

				Matrix rot = Matrix.CreateRotationZ(worldTransform.Rotation);
				particle.Velocity = Vector2.Transform(particle.Velocity, rot);

                particle.IsAlive = true;

                var pos_x = worldTransform.Position.X + m_Random.Next(0, (int)(particleComponent.Width*worldTransform.Scale.X));
                var pos_y = worldTransform.Position.Y + m_Random.Next(0, (int)(particleComponent.Height*worldTransform.Scale.Y));
                pos_x -= (int)(particleComponent.Width*worldTransform.Scale.X*worldTransform.Origin.X);
                pos_y -= (int)(particleComponent.Height*worldTransform.Scale.Y*worldTransform.Origin.Y);
                
                var rotation = particleComponent.InitialRotation + worldTransform.Rotation;
                var scale = particleComponent.InitialScale * worldTransform.Scale;

                particle.Transform = new Cv_Transform(new Vector3(pos_x, pos_y, Parent.Position.Z), scale, rotation);
                particle.InitialScale = worldTransform.Scale;
                particle.InitialRotation = worldTransform.Rotation;
                m_Particles.AddFirst(particle);
            }
        }

        private float RandomGaussian(float mean, float stdDev)
        {
            double u1 = 1.0-m_Random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0-m_Random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return (float) randNormal;
        }

        private int GetNumParticlesToGenerate(float elapsedTime)
        {
            var component = (Cv_ParticleEmitterComponent) Component;
            var totalParticles = (int) Math.Max(Math.Round(RandomGaussian(component.ParticlesPerSecond * elapsedTime / 1000f, 0.25f)), 0);

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