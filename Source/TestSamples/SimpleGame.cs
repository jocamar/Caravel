using Caravel.Core;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Core.Physics;
using Caravel.Core.Process;
using Microsoft.Xna.Framework;
using static Caravel.Core.Cv_GameLogic;
using static Caravel.Core.Draw.Cv_Renderer;

namespace Caravel.TestSamples
{
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
            var phys = new Cv_FarseerPhysics(this);
            phys.Gravity = new Vector2(0,1);
            return phys;
        }

        protected override Cv_GameView[] VCreateGameViews()
        {
            var gvs = new Cv_GameView[1];

            gvs[0] = new Cv_PlayerView(PlayerIndex.One, new Vector2(1f, 1), Vector2.Zero);
            //gvs[1] = new Cv_PlayerView(PlayerIndex.Two, new Vector2(0.5f, 1), new Vector2(0.5f, 0));
            pv = (Cv_PlayerView) gvs[0];
            pv.DebugDrawRadius = false;
            pv.DebugDrawFPS = true;

            //pv2 = (Cv_PlayerView) gvs[1];
            //pv2.DebugDrawPhysicsShapes = false;

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
            //Graphics.ToggleFullScreen();
            //Graphics.ApplyChanges();
            IsMouseVisible = true;
            EventManager.AddListener<Cv_Event_SceneLoaded>(OnSceneLoaded);
            EventManager.AddListener<Cv_Event_RemoteSceneLoaded>(OnSceneLoaded);
            Logic.ChangeState(Cv_GameState.LoadingScene);
            return true;
        }

        protected internal override bool VLoadGame()
        {
            var loadProcess = new Cv_LoadSceneProcess("scenes/emptyscene.cvs", "Default");
            ProcessManager.AttachProcess(loadProcess);

           /* guntler = Logic.GetEntity("guntler");
            guybrush = Logic.GetEntity("guybrush");
            profile = Logic.GetEntity("profile");

            pv2.Camera = Logic.GetEntity("camera2").GetComponent<Cv_CameraComponent>().CameraNode;

            pv.PrintScene();
            pv2.PrintScene();*/
            return true;
        }

        private void OnSceneLoaded(Cv_Event evtData)
        {
            CameraEntity = Logic.GetEntity("camera");
        }
    }
}