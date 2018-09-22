using System.Threading;

namespace Caravel.Core.Process
{
    public abstract class Cv_ParallelProcess : Cv_Process
    {
        protected Thread ProcessThread
        {
            get; set;
        }

        protected ThreadPriority Priority
        {
            get; set;
        }

        internal Cv_ParallelProcess(ThreadPriority priority = ThreadPriority.Normal)
        {
            Priority = priority;
        }

        internal void ThreadFunction()
        {
            VThreadFunction();
        }

        protected internal override void VOnInitialize()
        {
            base.VOnInitialize();

            ProcessThread = new Thread(new ThreadStart(ThreadFunction));
            ProcessThread.Priority = Priority;
            ProcessThread.Start();
        }

        protected internal override void VOnAbort()
        {
        }

        protected internal override void VOnFail()
        {
        }

        protected internal override void VOnSuccess()
        {
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
        }

        protected internal abstract void VThreadFunction(); 
    }
}