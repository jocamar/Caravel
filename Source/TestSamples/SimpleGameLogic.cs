using System;
using Caravel.Core;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
            Caravel.SoundManager.DistanceFallOff = 0.5f;
			simpleGame = app;
			Caravel.EventManager.AddListener<Cv_Event_NewCollision>(OnCollision);
		}

		private void OnCollision(Cv_Event eventData)
		{
            var newCollisionEvt = (Cv_Event_NewCollision) eventData;
            var camTransf = simpleGame.CameraEntity.GetComponent<Cv_TransformComponent>();
            var collisionEntity = newCollisionEvt.ShapeA.Owner;
            var entityTransf = collisionEntity.GetComponent<Cv_TransformComponent>();

            var emitter = Vector2.Zero;

            if (entityTransf != null)
            {
                emitter = new Vector2(entityTransf.Position.X, entityTransf.Position.Y);
            }
			Caravel.SoundManager.PlaySound2D("hit.wav", "Default", new Vector2(camTransf.Position.X, camTransf.Position.Y), emitter);
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
                var instance = Caravel.SoundManager.FadeInSound("hit.wav", "Default", 20000, true);
                var guybrushSprite = simpleGame.guybrush.GetComponent<Cv_SpriteComponent>();
                var anim = guybrushSprite.CurrentAnimation == "running" ? "walking" : "running";
                guybrushSprite.SetAnimation(anim);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                Caravel.SoundManager.FadeOutSound("hit.wav", 20000);
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