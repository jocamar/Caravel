using Caravel.Core;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Caravel.TestSamples
{
	public class SimpleGameLogic : Cv_GameLogic
	{
		private SimpleGame simpleGame;

		public SimpleGameLogic(SimpleGame app) : base(app)
		{
			simpleGame = app;
		}

		protected override void VGameOnUpdate(float time, float timeElapsed)
		{
            simpleGame.guntler.GetComponent<Cv_TransformComponent>().Transform.Rotation += timeElapsed / 1000;
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                simpleGame.pv.Camera.Move(new Vector2(-5,0));
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                simpleGame.pv.Camera.Move(new Vector2(5, 0));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                simpleGame.pv.Camera.Move(new Vector2(0,-5));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                simpleGame.pv.Camera.Move(new Vector2(0, 5));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                simpleGame.pv.Camera.Zoom += 0.01f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                simpleGame.pv.Camera.Zoom -= 0.01f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                var guybrushSprite = simpleGame.guybrush.GetComponent<Cv_SpriteComponent>();
                var anim = guybrushSprite.CurrentAnimation == "running" ? "walking" : "running";
                guybrushSprite.SetAnimation(anim);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                var guybrushSprite = simpleGame.guybrush.GetComponent<Cv_SpriteComponent>();
                guybrushSprite.Paused = !guybrushSprite.Paused;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.O))
            {
                var guybrushSprite = simpleGame.guybrush.GetComponent<Cv_SpriteComponent>();
                if (guybrushSprite.Speed != null)
                {
                    guybrushSprite.Speed = null;
                }
                else
                {
                    guybrushSprite.Speed = 0;
                }
            }
		}
	}
}