using Caravel.Core;
using Caravel.Core.Entity;
using Microsoft.Xna.Framework;
using static Caravel.Core.Cv_GameLogic;

namespace Caravel.TestSamples
{
    public class SimpleGame : CaravelApp
    {
        public Cv_PlayerView pv;
        public Cv_Entity profile, guntler;

        public SimpleGame(int screenWidth, int screenHeight) : base(screenWidth, screenHeight)
        {
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
            return new Cv_GamePhysics();
        }

        protected override Cv_GameView[] VCreateGameViews()
        {
            var gvs = new Cv_GameView[1];

            pv = new Cv_PlayerView(PlayerIndex.One, 640, 640);
			var camera = new Cv_CameraNode("camera_" + PlayerIndex.One);
			pv.Camera = camera;
            pv.Camera.Position = new Vector3(0, 0, 0);
            gvs[0] = pv;

            return gvs;
        }

        protected override string VGetGameAppDirectory()
        {
            return "SimpleGame";
        }

        protected override string VGetGameTitle()
        {
            return "Simple Example Game";
        }

        protected override void VRegisterGameEvents()
        {
        }

        protected internal override bool VInitialize()
        {
            GameLogic.ChangeState(Cv_GameState.LoadingGameEnvironment);
            return true;
        }

        protected internal override bool VLoadGame()
        {
            GameLogic.LoadScene("scenes/testScene.xml");
            profile = GameLogic.CreateEntity("entities/profile.xml", null);
            guntler = GameLogic.CreateEntity("entities/guntler.xml", null);
            return true;
        }
    }
}