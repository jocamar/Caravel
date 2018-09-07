using System;
using Caravel.Core;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.TestSamples
{
	public class SimpleGameLogic : Cv_GameLogic
	{
		private SimpleGame simpleGame;
        int entities = 0;

		public SimpleGameLogic(SimpleGame app) : base(app)
		{
			simpleGame = app;
			Caravel.EventManager.AddListener<Cv_Event_NewCollision>(OnCollision);
		}

		private void OnCollision(Cv_Event eventData)
		{
			Caravel.SoundManager.PlaySound("hit2.wav", "Default");
		}

		protected override void VGameOnUpdate(float time, float timeElapsed)
		{
            var guntlerTransf = simpleGame.guntler.GetComponent<Cv_TransformComponent>();
            guntlerTransf.SetRotation(guntlerTransf.Rotation + (timeElapsed / 1000));

            var camTransf = simpleGame.CameraEntity.GetComponent<Cv_TransformComponent>();
            var camSettings = simpleGame.CameraEntity.GetComponent<Cv_CameraComponent>();

            var profileTransf = simpleGame.profile.GetComponent<Cv_TransformComponent>();
            var profileRigidBody = simpleGame.profile.GetComponent<Cv_RigidBodyComponent>();
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                camTransf.SetPosition(camTransf.Position + new Vector3(-5,0,0));
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                camTransf.SetPosition(camTransf.Position + new Vector3(5, 0, 0));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                camTransf.SetPosition(camTransf.Position + new Vector3(0,-5, 0));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                camTransf.SetPosition(camTransf.Position + new Vector3(0, 5, 0));
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
                profileRigidBody.Impulse = new Vector3(-500,0,0);
                //profileTransf.Position += new Vector3(-5,0,0);
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                profileRigidBody.Impulse += new Vector3(500, 0, 0);
                //profileTransf.Position += new Vector3(5,0,0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                profileTransf.SetRotation(profileTransf.Rotation + (timeElapsed / 1000));
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                profileTransf.SetRotation(profileTransf.Rotation - (timeElapsed / 1000));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                CreateEntity("entity_types/zombie.cve", "entity_" + entities, "Default");
                entities++;
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Vector2 mousePos = new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y);

                Cv_EntityID[] entities;
                if (this.simpleGame.pv.Pick(mousePos, out entities))
                {
                    Console.WriteLine(entities[0]);
                }
            }
		}
	}
}