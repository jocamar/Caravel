namespace Caravel.Core.Physics
{
    public class Cv_NullPhysics : Cv_FarseerPhysics
    {
        public override void VOnUpdate(float timeElapsed)
        {
            SyncBodiesToEntities();
        }
    }
}