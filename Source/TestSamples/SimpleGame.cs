using Caravel.Core;

namespace Caravel.TestSamples
{
    public class SimpleGame : CaravelApp
    {
        public SimpleGame(int screenWidth, int screenHeight) : base(screenWidth, screenHeight)
        {
        }

        protected override bool VCheckGameSystemResources()
        {
            return true;
        }

        protected override Cv_GameLogic VCreateGameLogic()
        {
            return new Cv_GameLogic(this);
        }

        protected override Cv_GamePhysics VCreateGamePhysics()
        {
            return new Cv_GamePhysics();
        }

        protected override Cv_GameView[] VCreateGameViews()
        {
            return new Cv_GameView[]{};
        }

        protected override string VGetGameAppDirectory()
        {
            return "SimpleGame";
        }

        protected override string VGetGameTitle()
        {
            return "Simple Example Game";
        }

        protected override bool VInitialize()
        {
            return true;
        }

        protected override bool VLoadGame()
        {
            return true;
        }

        protected override void VRegisterGameEvents()
        {
        }
    }
}