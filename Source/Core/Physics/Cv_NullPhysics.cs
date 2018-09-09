namespace Caravel.Core.Physics
{
    public class Cv_NullPhysics : Cv_FarseerPhysics
    {
        public Cv_NullPhysics(CaravelApp app) : base(app)
        {

        }

        public override void VOnUpdate(float elapsedTime)
        {
            SyncBodiesToEntities();
        }
    }
}