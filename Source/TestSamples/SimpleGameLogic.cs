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
            simpleGame.guntler.GetComponent<Cv_TransformComponent>().Rotation += timeElapsed / 1000;

            var camTransf = simpleGame.CameraEntity.GetComponent<Cv_TransformComponent>();
            var camSettings = simpleGame.CameraEntity.GetComponent<Cv_CameraComponent>();

            var profileTransf = simpleGame.profile.GetComponent<Cv_TransformComponent>();
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                camTransf.Position += new Vector3(-5,0,0);
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                camTransf.Position += new Vector3(5, 0, 0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                camTransf.Position += new Vector3(0,-5, 0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                camTransf.Position += new Vector3(0, 5, 0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                camSettings.Zoom += 0.01f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                camSettings.Zoom -= 0.01f;
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

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                profileTransf.Position += new Vector3(-5,0,0);
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                profileTransf.Position += new Vector3(5, 0, 0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                profileTransf.Rotation += timeElapsed / 1000;
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                profileTransf.Rotation -= timeElapsed / 1000;
            }
		}
	}
}