using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Caravel.Core.Resource;
using Caravel.Core.Scripting;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_TransformAnimationComponent : Cv_EntityComponent
    {
        private struct Cv_TransformAnimationKeyFrame
        {
            public int X;
            public int Y;
            public float Alpha;
            public float ScaleX;
            public float ScaleY;
            public float Rotation;
            public float Time;
        }
        private class Cv_TransformAnimation
        {
            public List<Cv_TransformAnimationKeyFrame> AnimationKeyFrames;
            public int CurrPointIndex;
            public float TotalTimeElapsed;
            public float CurrPointTimeElapsed;

            public Cv_TransformAnimation()
            {
                AnimationKeyFrames = new List<Cv_TransformAnimationKeyFrame>();
                CurrPointIndex = 0;
                TotalTimeElapsed = 0;
                CurrPointTimeElapsed = 0;
            }
        }

        public Action OnEnd
        {
            get; set;
        }

        public string OnEndScript
        {
            get; set;
        }

        public bool Paused
        {
            get; set;
        }

        public bool Looping
        {
            get
            {
                return m_bLooping;
            }
            
            set
            {
                m_bLooping = value;

                if (m_bLooping)
                {
                    Finished = false;

                    if (m_Animation != null)
                    {
                        GetInitialTransformAndAlpha();
                    }
                }
            }
        }

        public bool ResetOnEnd
        {
            get; set;
        }

        public bool Finished
        {
            get; private set;
        }

        public string TransformAnimation
        {
            get
            {
                return m_sTransformAnimationResource;
            }

            set
            {
                m_sTransformAnimationResource = value;
                m_Animation = null;
            }
        }

        private string m_sTransformAnimationResource;
        private Cv_TransformAnimation m_Animation = null;
        private Cv_Transform m_InitialTransform;
        private int m_iInitialSpriteAlpha;
        private int m_iInitialTextAlpha;
        private int m_iInitialParticleAlpha;
        private Cv_TransformComponent m_TransformComponent;
        private Cv_SpriteComponent m_SpriteComponent;
        private Cv_TextComponent m_TextComponent;
        private Cv_ParticleEmitterComponent m_ParticleEmitterComponent;
        private bool m_bLooping;

        private static Cv_TransformAnimationKeyFrame m_initialKeyFrame = new Cv_TransformAnimationKeyFrame()
        {
            X = 0,
            Y = 0,
            ScaleX = 1,
            ScaleY = 1,
            Alpha = 1,
            Rotation = 0
        };

        public Cv_TransformAnimationComponent()
        {
            Looping = false;
            Paused = false;
            Finished = false;
            ResetOnEnd = false;
        }

        public void Restart()
        {
            Finished = false;
            Paused = false;

            if (m_Animation != null)
            {
                m_Animation.CurrPointIndex = 0;
                GetInitialTransformAndAlpha();
            }
        }

        public override XmlElement VToXML()
        {
            var componentDoc = new XmlDocument();
            var componentData = componentDoc.CreateElement(GetComponentName<Cv_TransformAnimationComponent>());
            var transformAnimation = componentDoc.CreateElement("TransformAnimation");
            var onEndScript = componentDoc.CreateElement("OnEndScript");
            var paused = componentDoc.CreateElement("Paused");
            var looping = componentDoc.CreateElement("Looping");
            var resetOnEnd = componentDoc.CreateElement("ResetOnEnd");

            transformAnimation.SetAttribute("resource", TransformAnimation);
            onEndScript.SetAttribute("resource", OnEndScript);
            paused.SetAttribute("status", Paused.ToString(CultureInfo.InvariantCulture));
            looping.SetAttribute("status", Looping.ToString(CultureInfo.InvariantCulture));
            resetOnEnd.SetAttribute("status", ResetOnEnd.ToString(CultureInfo.InvariantCulture));

            componentData.AppendChild(transformAnimation);
            componentData.AppendChild(onEndScript);
            componentData.AppendChild(paused);
            componentData.AppendChild(looping);
            componentData.AppendChild(resetOnEnd);
            
            return componentData;
        }

        public override bool VInitialize(XmlElement componentData)
        {
            var transformAnimationNode = componentData.SelectNodes("TransformAnimation").Item(0);
            if (transformAnimationNode != null)
            {
                TransformAnimation = transformAnimationNode.Attributes["resource"].Value;
            }

            var onEndScriptNode = componentData.SelectNodes("OnEndScript").Item(0);
            if (onEndScriptNode != null)
            {
                OnEndScript = onEndScriptNode.Attributes["resource"].Value;
            }

            var pauseNode = componentData.SelectNodes("Paused").Item(0);
            if (pauseNode != null)
            {
                Paused = bool.Parse(pauseNode.Attributes["status"].Value);
            }

            var loopingNode = componentData.SelectNodes("Looping").Item(0);
            if (loopingNode != null)
            {
                Looping = bool.Parse(loopingNode.Attributes["status"].Value);
            }

            var resetOnEndNode = componentData.SelectNodes("ResetOnEnd").Item(0);
            if (resetOnEndNode != null)
            {
                ResetOnEnd = bool.Parse(resetOnEndNode.Attributes["status"].Value);
            }

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
            return true;
        }

        public override void VPostLoad()
        {
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
            m_TransformComponent = Owner.GetComponent<Cv_TransformComponent>();
            m_SpriteComponent = Owner.GetComponent<Cv_SpriteComponent>();
            m_TextComponent = Owner.GetComponent<Cv_TextComponent>();
            m_ParticleEmitterComponent = Owner.GetComponent<Cv_ParticleEmitterComponent>();

            if (Finished || Paused || m_TransformComponent == null || CaravelApp.Instance.EditorRunning)
            {
                return;
            }

            if (m_Animation == null)
            {
                ReadAnimationInfo();
                GetInitialTransformAndAlpha();
            }
            
            if (m_Animation.AnimationKeyFrames.Count <= 0)
            {
                return;
            }
            else
            {
                m_Animation.CurrPointTimeElapsed += elapsedTime;
                m_Animation.TotalTimeElapsed += elapsedTime;

                while (m_Animation.CurrPointIndex < m_Animation.AnimationKeyFrames.Count
                        && m_Animation.CurrPointTimeElapsed > m_Animation.AnimationKeyFrames[m_Animation.CurrPointIndex].Time)
                {
                    m_Animation.CurrPointTimeElapsed -= m_Animation.AnimationKeyFrames[m_Animation.CurrPointIndex].Time;
                    m_Animation.CurrPointIndex++;

                    if (m_Animation.CurrPointIndex >= m_Animation.AnimationKeyFrames.Count)
                    {
                        if (CheckAnimationFinished())
                        {
                            break;
                        }
                    }
                }

                if (!Finished)
                {
                    var keyFramePercent = m_Animation.CurrPointTimeElapsed / m_Animation.AnimationKeyFrames[m_Animation.CurrPointIndex].Time;
                    
                    var prevKeyFrame = m_initialKeyFrame;
                    if (m_Animation.CurrPointIndex > 0)
                    {
                        prevKeyFrame = m_Animation.AnimationKeyFrames[m_Animation.CurrPointIndex-1];
                    }

                    var currFrame = m_Animation.AnimationKeyFrames[m_Animation.CurrPointIndex];

                    var newPos = new Vector3(m_InitialTransform.Position.X + prevKeyFrame.X + (currFrame.X - prevKeyFrame.X)*keyFramePercent,
                                                m_InitialTransform.Position.Y + prevKeyFrame.Y + (currFrame.Y - prevKeyFrame.Y)*keyFramePercent,
                                                m_InitialTransform.Position.Z);
                    var newScale = new Vector2((prevKeyFrame.ScaleX + (currFrame.ScaleX - prevKeyFrame.ScaleX)*keyFramePercent) * m_InitialTransform.Scale.X,
                                                (prevKeyFrame.ScaleY + (currFrame.ScaleY - prevKeyFrame.ScaleY)*keyFramePercent) * m_InitialTransform.Scale.Y);

                    m_TransformComponent.SetPosition(newPos);
                    m_TransformComponent.SetScale(newScale);
                    m_TransformComponent.SetRotation(m_InitialTransform.Rotation + prevKeyFrame.Rotation + (currFrame.Rotation - prevKeyFrame.Rotation)*keyFramePercent);

                    if (m_SpriteComponent != null)
                    {
                        m_SpriteComponent.Color = new Color(m_SpriteComponent.Color, (int)((prevKeyFrame.Alpha + (currFrame.Alpha - prevKeyFrame.Alpha)*keyFramePercent) * m_iInitialSpriteAlpha));
                    }

                    if (m_TextComponent != null)
                    {
                        m_TextComponent.Color = new Color(m_TextComponent.Color, (int)((prevKeyFrame.Alpha + (currFrame.Alpha - prevKeyFrame.Alpha)*keyFramePercent) * m_iInitialTextAlpha));
                    }

                    if (m_ParticleEmitterComponent != null)
                    {
                        m_ParticleEmitterComponent.Color = new Color(m_ParticleEmitterComponent.Color, (int)((prevKeyFrame.Alpha + (currFrame.Alpha - prevKeyFrame.Alpha)*keyFramePercent) * m_iInitialParticleAlpha));
                    }
                }
            }
        }

        private void ReadAnimationInfo()
        {
            var resource = Cv_ResourceManager.Instance.GetResource<Cv_XmlResource>(TransformAnimation, Owner.ResourceBundle);

            m_Animation = new Cv_TransformAnimation();

            var xmlData = resource.GetXmlData();
            var keyFrames = xmlData.RootNode.SelectNodes("KeyFrame");

            foreach (XmlElement frame in keyFrames)
            {
                Cv_TransformAnimationKeyFrame keyFrame;

                keyFrame.X = int.Parse(frame.Attributes["x"].Value, CultureInfo.InvariantCulture);
                keyFrame.Y = int.Parse(frame.Attributes["y"].Value, CultureInfo.InvariantCulture);
                keyFrame.Rotation = float.Parse(frame.Attributes["rotation"].Value, CultureInfo.InvariantCulture);
                keyFrame.ScaleX = float.Parse(frame.Attributes["scaleX"].Value, CultureInfo.InvariantCulture);
                keyFrame.ScaleY = float.Parse(frame.Attributes["scaleY"].Value, CultureInfo.InvariantCulture);
                keyFrame.Alpha = Math.Max(0, Math.Min(float.Parse(frame.Attributes["alpha"].Value, CultureInfo.InvariantCulture), 1f));
                keyFrame.Time = float.Parse(frame.Attributes["time"].Value, CultureInfo.InvariantCulture);

                m_Animation.AnimationKeyFrames.Add(keyFrame);
            }
        }

        private bool CheckAnimationFinished()
        {
            if (OnEnd != null)
            {
                OnEnd();
            }

            if (OnEndScript != null && OnEndScript != "")
            {
                Cv_ScriptResource scriptRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(OnEndScript, Owner.ResourceBundle);
                scriptRes.RunScript(Owner);
            }

            if (Looping)
            {
                Finished = false;
                m_Animation.CurrPointIndex = 0;
            }
            else
            {
                Finished = true;
            }

            if (ResetOnEnd)
            {
                m_TransformComponent.Transform = m_InitialTransform;

                if (m_SpriteComponent != null)
                {
                    m_SpriteComponent.Color = new Color(m_SpriteComponent.Color, m_iInitialSpriteAlpha);
                }

                if (m_TextComponent != null)
                {
                    m_TextComponent.Color = new Color(m_TextComponent.Color, m_iInitialTextAlpha);
                }

                if (m_ParticleEmitterComponent != null)
                {
                    m_ParticleEmitterComponent.Color = new Color(m_ParticleEmitterComponent.Color, m_iInitialParticleAlpha);
                }
            }
            else if (Finished)
            {
                var prevKeyFrame =  m_Animation.AnimationKeyFrames[m_Animation.CurrPointIndex-1];

                var newPos = new Vector3(m_InitialTransform.Position.X + prevKeyFrame.X, m_InitialTransform.Position.Y + prevKeyFrame.Y, m_InitialTransform.Position.Z);
                var newScale = new Vector2(prevKeyFrame.ScaleX * m_InitialTransform.Scale.X, prevKeyFrame.ScaleY * m_InitialTransform.Scale.Y);

                m_TransformComponent.SetPosition(newPos);
                m_TransformComponent.SetScale(newScale);
                m_TransformComponent.SetRotation(m_InitialTransform.Rotation + prevKeyFrame.Rotation);

                if (m_SpriteComponent != null)
                {
                    m_SpriteComponent.Color = new Color(m_SpriteComponent.Color, (int)(prevKeyFrame.Alpha * m_iInitialSpriteAlpha));
                }

                if (m_TextComponent != null)
                {
                    m_TextComponent.Color = new Color(m_TextComponent.Color, (int)(prevKeyFrame.Alpha * m_iInitialTextAlpha));
                }

                if (m_ParticleEmitterComponent != null)
                {
                    m_ParticleEmitterComponent.Color = new Color(m_ParticleEmitterComponent.Color, (int)(prevKeyFrame.Alpha * m_iInitialParticleAlpha));
                }
            }

            GetInitialTransformAndAlpha();

            return Finished;
        }

        private void GetInitialTransformAndAlpha()
        {
            m_InitialTransform = m_TransformComponent.Transform;
            m_iInitialSpriteAlpha = 255;
            m_iInitialTextAlpha = 255;
            m_iInitialParticleAlpha = 255;

            if (m_SpriteComponent != null)
            {
                m_iInitialSpriteAlpha = m_SpriteComponent.Color.A;
            }

            if (m_TextComponent != null)
            {
                m_iInitialTextAlpha = m_TextComponent.Color.A;
            }

            if (m_ParticleEmitterComponent != null)
            {
                m_iInitialParticleAlpha = m_ParticleEmitterComponent.Color.A;
            }
        }
    }
}