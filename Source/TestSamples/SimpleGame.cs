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
    //Check how to do splitscreen
	//Add renderer options for blendmode and effects
	//Decouple asset loading from current folder for editor
	//InputManager
	//ProcessManager
	//more... (see commented code)
    public class SimpleGame : CaravelApp
    {
        public Cv_Entity CameraEntity;
        public Cv_Entity guntler, guybrush, profile;
        public Cv_PlayerView pv;

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
            var gvs = new Cv_GameView[1];

            gvs[0] = new Cv_PlayerView(PlayerIndex.One, 1280, 720);
            pv = (Cv_PlayerView) gvs[0];

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
            IsMouseVisible = true;
            GameLogic.ChangeState(Cv_GameState.LoadingGameEnvironment);
            return true;
        }

        protected internal override bool VLoadGame()
        {
            GameLogic.LoadScene("scenes/testScene.xml");
            CameraEntity = GameLogic.GetEntity("camera");

            guntler = GameLogic.GetEntity("guntler");
            guybrush = GameLogic.GetEntity("guybrush");
            profile = GameLogic.GetEntity("profile");

            pv.PrintScene();
            return true;
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16*1024];
            int read;
            while((read = input.Read (buffer, 0, buffer.Length)) > 0)
            {
                output.Write (buffer, 0, read);
            }
        }
    }
}