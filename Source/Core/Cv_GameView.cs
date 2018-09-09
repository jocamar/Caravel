using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core
{
    public abstract class Cv_GameView
    {
        public enum Cv_GameViewType
        {
            Player,
            Remote,
            AI,
            Recorder,
            Other
        }

        public enum Cv_GameViewID
        {
            INVALID_GAMEVIEW = 0
        }

        public abstract Cv_GameViewType Type
        {
            get;
        }

        public abstract Cv_GameViewID ID
        {
            get;
        }

        public CaravelApp Caravel
        {
            get; private set;
        }

        public void Initialize(CaravelApp app)
        {
            Caravel = app;
        }

        protected internal abstract void VOnRender(float time, float elapsedTime);
        protected internal abstract void VOnPostRender();
        protected internal abstract void VOnUpdate(float time, float elapsedTime);
        protected internal abstract void VOnAttach(Cv_GameViewID id, Cv_EntityID entityId);
    }
}
