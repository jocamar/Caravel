using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Caravel.Core.Draw;
using Caravel.Core.Scripting;
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

        public string Texture
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

		public bool Finished
        {
            get; private set;
        }

        public bool Mirrored
        {
            get; set;
        }

        public Action OnEnd
        {
            get; set;
        }

		public string OnEndScript
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
			Finished = false;
            Visible = true;
            Mirrored = false;
			timeSinceLastUpdate = 0;
            m_CurrAnim = null;
            m_SubAnimations = new Dictionary<string, Cv_SpriteSubAnimation>();
            m_sDefaultAnim = "";
        }

        public void SetAnimation(string animationId, Action onEnd = null)
        {
            Cv_SpriteSubAnimation anim;

            if (m_SubAnimations.TryGetValue(animationId, out anim))
            {
                m_CurrAnim = anim;
                OnEnd = onEnd;
                CurrentFrame = anim.StartFrame;
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

        public override void VPostLoad()
		{
		}

        protected internal override void VOnUpdate(float elapsedTime)
        {
            SceneNode.SetRadius(-1);
            
            GetAnimationInfo();

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
                            if (CheckAnimationFinished())
                            {
                                break;
                            }
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

            base.VOnUpdate(elapsedTime);
        }

        protected override Cv_SceneNode VCreateSceneNode()
        {
            return new Cv_SpriteNode(Owner.ID, this, Cv_Transform.Identity);
        }

        protected override bool VInheritedInit(XmlElement componentData)
        {
            Cv_Debug.Assert(componentData != null, "Must have valid component data.");

            var textureNode = componentData.SelectNodes("Texture").Item(0);
            if (textureNode != null)
            {
                Texture = textureNode.Attributes["resource"].Value;
            }

			var animationNode = componentData.SelectNodes("Animation").Item(0);

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

                var subAnimations = animationNode.SelectNodes("SubAnimation");
                m_SubAnimations.Clear();
                m_CurrAnim = null;

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

            var mirroredNode = componentData.SelectNodes("Mirrored").Item(0);
            if (mirroredNode != null)
            {
                Mirrored = bool.Parse(mirroredNode.Attributes["status"].Value);
            }

            var scriptNode = componentData.SelectNodes("OnEndScript").Item(0);
            if (scriptNode != null)
            {
                OnEndScript = scriptNode.Attributes["resource"].Value;
            }
            
            return true;
        }

        protected override XmlElement VCreateInheritedElement(XmlElement baseElement)
        {
            var textureElement = baseElement.OwnerDocument.CreateElement("Texture");
            textureElement.SetAttribute("resource", Texture);
            baseElement.AppendChild(textureElement);

            var animationElement = baseElement.OwnerDocument.CreateElement("Animation");
            animationElement.SetAttribute("fx", FrameX.ToString(CultureInfo.InvariantCulture));
            animationElement.SetAttribute("fy", FrameY.ToString(CultureInfo.InvariantCulture));
            animationElement.SetAttribute("loop", Looping.ToString(CultureInfo.InvariantCulture));

            if (Speed != null)
            {
                animationElement.SetAttribute("speed", Speed.ToString());
            }

            if (StartFrame != null)
            {
                animationElement.SetAttribute("startFrame", StartFrame.ToString());
            }

            if (EndFrame != null)
            {
                animationElement.SetAttribute("endFrame", EndFrame.ToString());
            }

            if (m_SubAnimations.Count > 0)
            {
                animationElement.SetAttribute("defaultAnim", m_sDefaultAnim);
            }

            foreach (var subAnim in m_SubAnimations.Values)
            {
                var subAnimationNode = baseElement.OwnerDocument.CreateElement("SubAnimation");
                subAnimationNode.SetAttribute("id", subAnim.ID);
                subAnimationNode.SetAttribute("speed", subAnim.Speed.ToString(CultureInfo.InvariantCulture));
                subAnimationNode.SetAttribute("startFrame", subAnim.StartFrame.ToString(CultureInfo.InvariantCulture));
                subAnimationNode.SetAttribute("endFrame", subAnim.EndFrame.ToString(CultureInfo.InvariantCulture));

                animationElement.AppendChild(subAnimationNode);
            }

            baseElement.AppendChild(animationElement);

            var mirroredElement = baseElement.OwnerDocument.CreateElement("Mirrored");
            mirroredElement.SetAttribute("status", Mirrored.ToString(CultureInfo.InvariantCulture));
            baseElement.AppendChild(mirroredElement);

            var scriptElement = baseElement.OwnerDocument.CreateElement("OnEndScript");
            scriptElement.SetAttribute("resource", OnEndScript);
            baseElement.AppendChild(scriptElement);
            return baseElement;
        }

        private bool CheckAnimationFinished()
        {
            if (OnEnd != null)
            {
                OnEnd();
            }

            if (OnEndScript != null && OnEndScript != "")
            {
                Cv_ScriptManager.Instance.VExecuteFile(OnEndScript, Owner);
            }

            if (Looping)
            {
                Finished = false;
                CurrentFrame = m_ActualStartFrame;
                return false;
            }
            else 
            {
                Finished = true;
                return true;
            }
        }

        private void GetAnimationInfo()
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
        }
	}
}