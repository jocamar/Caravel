using Caravel.Core;
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
		}
	}
}