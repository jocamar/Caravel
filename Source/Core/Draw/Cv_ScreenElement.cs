namespace Caravel.Core.Draw
{
    public abstract class Cv_ScreenElement
    {
        public bool IsVisible
        {
            get; set;
        }

        public int ZOrder
        {
            get; set;
        }

        public abstract void VOnRender(float time, float elapsedTime, Cv_Renderer renderer);
        public abstract void VOnPostRender(Cv_Renderer renderer);
        public abstract void VOnUpdate(float time, float elapsedTime);
    }
}