using System.Xml;
using Caravel.Debugging;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_SpriteComponent : Cv_RenderComponent
    {
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

		public int Speed
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

		public int StartFrame
        {
            get; set;
        }

		public int EndFrame
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

		private long timeSinceLastUpdate;

        public Cv_SpriteComponent()
        {
			Texture = null;
            Width = 1;
            Height = 1;
            Color = Color.White;
			Speed = 0;
			FrameX = 1;
			FrameY = 1;
			Looping = false;
			StartFrame = 0;
			EndFrame = 0;
			CurrentFrame = StartFrame;
			Paused = false;
			Ended = false;
			timeSinceLastUpdate = 0;
        }

        public Cv_SpriteComponent(string resource, int width, int height, Color color, int speed = 0,
										int fx = 1, int fy = 1, bool loop = false, int startFrame = 0, int endFrame = 0)
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
			CurrentFrame = StartFrame;
			Paused = false;
			Ended = false;
			timeSinceLastUpdate = 0;
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
                Speed = int.Parse(animationNode.Attributes["speed"].Value);
                FrameX = int.Parse(animationNode.Attributes["fx"].Value);
				FrameY = int.Parse(animationNode.Attributes["fy"].Value);
				Looping = bool.Parse(animationNode.Attributes["loop"].Value);
				StartFrame = int.Parse(animationNode.Attributes["startFrame"].Value);
				EndFrame = int.Parse(animationNode.Attributes["endFrame"].Value);
				CurrentFrame = StartFrame;
            }

            return true;
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
			if (Speed > 0 && !Paused)
			{
				var frameIntervalMillis = 1000 / Speed;

				if (Speed > 0 && timeSinceLastUpdate > frameIntervalMillis)
                {
                    var framesToSkip = timeSinceLastUpdate * frameIntervalMillis;
                    timeSinceLastUpdate = 0;

                    while (framesToSkip > 0)
                    {
                        if (CurrentFrame + 1 >= EndFrame)
                        {
                            if (Looping)
                            {
                                CurrentFrame = 0;
                            }
                            else 
                            {
                                Ended = true;
                                //OnStop?.Invoke();
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

        protected internal override XmlElement VToXML()
        {
            throw new System.NotImplementedException();
        }

        protected override Cv_SceneNode VCreateSceneNode()
        {
            var transformComponent = Owner.GetComponent<Cv_TransformComponent>();

            var transform = new Cv_Transform();
            if(transformComponent != null)
            {
                transform = transformComponent.Transform;
            }

            return new Cv_SpriteNode(Owner.ID, this, transform);
        }

        protected override XmlElement VCreateInheritedElement(XmlElement baseElement)
        {
            throw new System.NotImplementedException();
        }
    }
}