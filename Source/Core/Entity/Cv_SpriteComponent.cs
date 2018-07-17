using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Caravel.Core.Draw;
using Caravel.Debugging;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_SpriteComponent : Cv_RenderComponent
    {
        public struct Cv_SpriteSubAnimation
        {
            public string ID;

            public int StartFrame
            {
                get; set;
            }

            public int EndFrame
            {
                get; set;
            }

            public int Speed
            {
                get; set;
            }
        }

        public delegate void OnEndDelegate();

        public string Texture
        {
            get; set;
        }

        public int Width
        {
            get; set;
        }

        public int Height
        {
            get; set;
        }

		public int? Speed
        {
            get; set;
        }

		public int FrameX
        {
            get; set;
        }

		public int FrameY
        {
            get; set;
        }

		public bool Looping
        {
            get; set;
        }

        public int? StartFrame
        {
            get; set;
        }

		public int? EndFrame
        {
            get; set;
        }

		public int CurrentFrame
        {
            get; set;
        }

		public bool Paused
        {
            get; set;
        }

		public bool Ended
        {
            get; private set;
        }

        public OnEndDelegate OnEnd
        {
            get; set;
        }

        public string[] Animations
        {
            get
            {
                return m_SubAnimations.Keys.ToArray();
            }
        }

        public string CurrentAnimation
        {
            get
            {
                return m_CurrAnim != null ? m_CurrAnim.Value.ID : null;
            }
        }

        private string m_sDefaultAnim;
		private long timeSinceLastUpdate;
        private Dictionary<string, Cv_SpriteSubAnimation> m_SubAnimations;
        private Cv_SpriteSubAnimation? m_CurrAnim;
        private int m_ActualStartFrame, m_ActualEndFrame, m_ActualSpeed;

        public Cv_SpriteComponent()
        {
			Texture = null;
            Width = 1;
            Height = 1;
            Color = Color.White;
			FrameX = 1;
			FrameY = 1;
			Looping = false;
			CurrentFrame = 0;
			Paused = false;
			Ended = false;
			timeSinceLastUpdate = 0;
            m_CurrAnim = null;
            m_SubAnimations = new Dictionary<string, Cv_SpriteSubAnimation>();
            m_sDefaultAnim = "";
        }

        public Cv_SpriteComponent(string resource, int width, int height, Color color,
										int fx = 1, int fy = 1, bool loop = false, int? speed = null, int? startFrame = null, int? endFrame = null)
        {
            Texture = resource;
            Width = width;
            Height = height;
            Color = color;
			Speed = speed;
			FrameX = fx;
			FrameY = fy;
			Looping = loop;
			StartFrame = startFrame;
			EndFrame = endFrame;
			CurrentFrame = StartFrame != null ? StartFrame.Value : 0;
			Paused = false;
			Ended = false;
			timeSinceLastUpdate = 0;
            m_CurrAnim = null;
            m_SubAnimations = new Dictionary<string, Cv_SpriteSubAnimation>();
            m_sDefaultAnim = "";
        }

        public void SetAnimation(string animationId, OnEndDelegate onEnd = null)
        {
            Cv_SpriteSubAnimation anim;

            if (m_SubAnimations.TryGetValue(animationId, out anim))
            {
                m_CurrAnim = anim;
                OnEnd = onEnd;
            }
        }

        public void AddAnimation(Cv_SpriteSubAnimation animation)
        {
            m_SubAnimations.Add(animation.ID, animation);
        }

        public void RemoveAnimation(string id)
        {
            m_SubAnimations.Remove(id);
        }

        protected internal override bool VInheritedInit(XmlElement componentData)
        {
            Cv_Debug.Assert(componentData != null, "Must have valid component data.");

            var textureNode = componentData.SelectNodes("//Texture").Item(0);
            if (textureNode != null)
            {
                Texture = textureNode.Attributes["resource"].Value;
            }

            var sizeNode = componentData.SelectNodes("//Size").Item(0);
            if (sizeNode != null)
            {
                Width = int.Parse(sizeNode.Attributes["width"].Value);
                Height = int.Parse(sizeNode.Attributes["height"].Value);
            }

			var animationNode = componentData.SelectNodes("//Animation").Item(0);

            if (animationNode != null)
            {
                if (animationNode.Attributes["fx"] != null)
                {
                    FrameX = int.Parse(animationNode.Attributes["fx"].Value);
                }

                if (animationNode.Attributes["fy"] != null)
                {
                    FrameY = int.Parse(animationNode.Attributes["fy"].Value);
                }

                if (animationNode.Attributes["loop"] != null)
                {
                    Looping = bool.Parse(animationNode.Attributes["loop"].Value);
                }
                
                if (animationNode.Attributes["speed"] != null)
                {
                    Speed = int.Parse(animationNode.Attributes["speed"].Value);
                }
                    
                if (animationNode.Attributes["startFrame"] != null)
                {
                    StartFrame = int.Parse(animationNode.Attributes["startFrame"].Value);
                }
                
                if (animationNode.Attributes["endFrame"] != null)
                {
                    EndFrame = int.Parse(animationNode.Attributes["endFrame"].Value);
                }

                var subAnimations = animationNode.SelectNodes("//SubAnimation");
                m_SubAnimations.Clear();

                foreach (XmlElement subAnimation in subAnimations)
                {
                    var anim = new Cv_SpriteSubAnimation();

                    anim.ID = subAnimation.Attributes["id"].Value;
                    anim.Speed = int.Parse(subAnimation.Attributes["speed"].Value);
                    anim.StartFrame = int.Parse(subAnimation.Attributes["startFrame"].Value);
                    anim.EndFrame = int.Parse(subAnimation.Attributes["endFrame"].Value);
                    
                    AddAnimation(anim);
                }

                if (m_SubAnimations.Count > 0 && animationNode.Attributes["defaultAnim"] != null)
                {
                    var defaultAnim = animationNode.Attributes["defaultAnim"].Value;
                    m_sDefaultAnim = defaultAnim;
                    SetAnimation(defaultAnim);
                    CurrentFrame = m_CurrAnim != null ? m_CurrAnim.Value.StartFrame : 0;
                }

                if (StartFrame != null)
                {
                    CurrentFrame = StartFrame.Value;
                }
            }
            
            return true;
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
            m_ActualEndFrame = m_CurrAnim != null ? m_CurrAnim.Value.EndFrame : 0;
            if (EndFrame != null)
            {
                m_ActualEndFrame = EndFrame.Value;
            }

            m_ActualStartFrame = m_CurrAnim != null ? m_CurrAnim.Value.StartFrame : 0;
            if (StartFrame != null)
            {
                m_ActualStartFrame = StartFrame.Value;
            }

            m_ActualSpeed = m_CurrAnim != null ? m_CurrAnim.Value.Speed : 0;
            if (Speed != null)
            {
                m_ActualSpeed = Speed.Value;
            }

			if (m_ActualSpeed > 0 && !Paused)
			{
				var frameIntervalMillis = 1000 / m_ActualSpeed;

				if (timeSinceLastUpdate > frameIntervalMillis)
                {
                    var framesToSkip = timeSinceLastUpdate / frameIntervalMillis;
                    timeSinceLastUpdate = 0;

                    while (framesToSkip > 0)
                    {
                        if (CurrentFrame + 1 > m_ActualEndFrame)
                        {
                            if (Looping)
                            {
                                CurrentFrame = m_ActualStartFrame;
                            }
                            else 
                            {
                                Ended = true;
                                OnEnd?.Invoke();
                                break;
                            };
                        }
                        else
                            CurrentFrame++;

                        framesToSkip--;
                    }
                }
                else
                {
                    timeSinceLastUpdate += (long) elapsedTime;
                }
			}
        }

        protected override Cv_SceneNode VCreateSceneNode()
        {
            return new Cv_SpriteNode(Owner.ID, this, new Cv_Transform());
        }

        protected override XmlElement VCreateInheritedElement(XmlElement baseElement)
        {
            var textureElement = baseElement.OwnerDocument.CreateElement("Texture");
            textureElement.SetAttribute("resource", Texture);
            baseElement.AppendChild(textureElement);

            var sizeElement = baseElement.OwnerDocument.CreateElement("Size");
            sizeElement.SetAttribute("width", Width.ToString());
            sizeElement.SetAttribute("height", Height.ToString());
            baseElement.AppendChild(sizeElement);

            var animationElement = baseElement.OwnerDocument.CreateElement("Animation");
            animationElement.SetAttribute("fx", FrameX.ToString());
            animationElement.SetAttribute("fy", FrameY.ToString());
            animationElement.SetAttribute("loop", Looping.ToString());
            animationElement.SetAttribute("speed", Speed.ToString());
            animationElement.SetAttribute("startFrame", StartFrame.ToString());
            animationElement.SetAttribute("endFrame", EndFrame.ToString());

            if (m_SubAnimations.Count > 0)
            {
                animationElement.SetAttribute("defaultAnim", m_sDefaultAnim);
            }

            foreach (var subAnim in m_SubAnimations.Values)
            {
                var subAnimationNode = baseElement.OwnerDocument.CreateElement("SubAnimation");
                subAnimationNode.SetAttribute("id", subAnim.ID);
                subAnimationNode.SetAttribute("speed", subAnim.Speed.ToString());
                subAnimationNode.SetAttribute("startFrame", subAnim.StartFrame.ToString());
                subAnimationNode.SetAttribute("endFrame", subAnim.EndFrame.ToString());

                animationElement.AppendChild(subAnimationNode);
            }

            baseElement.AppendChild(animationElement);
            return baseElement;
        }

		protected internal override void VPostLoad()
		{
		}
	}
}