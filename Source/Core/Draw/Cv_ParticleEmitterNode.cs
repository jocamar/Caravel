using System;
using System.Collections.Generic;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
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
        }

        private Queue<Cv_Particle> Particles;
        private readonly int MAX_PARTICLES = 1024;
        private Random m_Random;

        public Cv_ParticleEmitterNode(Cv_Entity.Cv_EntityID entityID, Cv_EntityComponent renderComponent, Cv_Transform to, Cv_Transform? from = null) : base(entityID, renderComponent, to, from)
        {
            Particles = new Queue<Cv_Particle>();
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

                var comp = ((Cv_ParticleEmitterComponent) Component);
                Properties.Radius = (float) Math.Sqrt(comp.Width*comp.Width + comp.Height*comp.Height) * originFactor;
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
            throw new System.NotImplementedException();
        }

        internal override bool VPick(Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities)
        {
            var particleComponent = (Cv_ParticleEmitterComponent) Component;
            return particleComponent.Pick(renderer, screenPosition, entities);
        }

        internal override void VOnUpdate(float time, float elapsedTime)
        {

        }

        private static double RandomBiasedPow(double min, double max, int tightness, double peak)
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

        private static double GetExp(double peak)
        {
            // Get the exponent necessary for BiasPow(...) to result in the desired peak 
            // Based on empirical trials, and curve fit to a cubic equation, using WolframAlpha
            return -11.7588 * Math.Pow(peak, 3) + 27.3205 * Math.Pow(peak, 2) - 21.2365 * peak + 6.31735;
        }

        private static double BiasPow(double input, double exp)
        {
            return Math.Pow(input, exp);
        }
    }
}