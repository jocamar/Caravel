using System.IO;
using Caravel.Core;
using Caravel.Core.Draw;
using Caravel.Core.Entity;
using Caravel.Core.Physics;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Caravel.Core.Cv_GameLogic;

namespace Caravel.TestSamples
{
	//TODO(JM):
	//Remove unused usings
	//Add collision categories to shapes
	//InputManager
	//ProcessManager
	//more... (see commented code)
    public class SimpleGame : CaravelApp
    {
        public Cv_Entity CameraEntity;
        public Cv_Entity guntler, guybrush, profile;
        public Cv_PlayerView pv, pv2;

        public SimpleGame(int screenWidth, int screenHeight) : base(screenWidth, screenHeight, true)
        {
			UseDevelopmentDirectories = true;
        }

        protected override bool VCheckGameSystemResources()
        {
            return true;
        }

        protected override Cv_GameLogic VCreateGameLogic()
        {
            return new SimpleGameLogic(this);
        }

        protected override Cv_GamePhysics VCreateGamePhysics()
        {
            var phys = new Cv_FarseerPhysics();
            phys.Gravity = new Vector2(0,1);
            return phys;
        }

        protected override Cv_GameView[] VCreateGameViews()
        {
            var gvs = new Cv_GameView[2];

            gvs[0] = new Cv_PlayerView(PlayerIndex.One, new Vector2(0.5f, 1), Vector2.Zero);
            gvs[1] = new Cv_PlayerView(PlayerIndex.Two, new Vector2(0.5f, 1), new Vector2(0.5f, 0));
            pv = (Cv_PlayerView) gvs[0];
            pv.DebugDrawRadius = true;

            pv2 = (Cv_PlayerView) gvs[1];
            pv2.DebugDrawPhysics = true;

            return gvs;
        }

        protected override string VGetGameAppDirectoryName()
        {
            return "SimpleGame";
        }

        protected override string VGetGameTitle()
        {
            return "Simple Example Game";
        }

        protected internal override bool VInitialize()
        {
            IsMouseVisible = true;
            GameLogic.ChangeState(Cv_GameState.LoadingGameEnvironment);
            return true;
        }

        protected internal override bool VLoadGame()
        {
            GameLogic.LoadScene("scenes/testScene.cvs", "Default");
            CameraEntity = GameLogic.GetEntity("camera");

            guntler = GameLogic.GetEntity("guntler");
            guybrush = GameLogic.GetEntity("guybrush");
            profile = GameLogic.GetEntity("profile");

            pv2.Camera = GameLogic.GetEntity("camera2").GetComponent<Cv_CameraComponent>().CameraNode;

            pv.PrintScene();
            pv2.PrintScene();
            return true;
        }
    }
}